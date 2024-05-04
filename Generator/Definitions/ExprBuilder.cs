/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Mangh.Metrology.Language;
using System;
using System.Collections.Generic;

namespace Mangh.Metrology
{
    /// <summary>
    /// Builder of <see cref="DimExpr"/> and <see cref="NumExpr{T}"/> expressions.
    /// </summary>
    /// <typeparam name="T">Numeric type (e.g. <see cref="double"/>) to specialize the encoder for.</typeparam>
    public class ExprBuilder<T> : IExprBuilder
        where T : struct, IEquatable<T>
    {
        #region Fields
        private readonly Stack<(DimExpr, NumExpr<T>)> _stack;
        private readonly NumericAgent<T> _agent;
        private readonly Phrasebook _phrase;
        #endregion

        #region Properties

        /// <summary>
        /// <see cref="Numeric{T}"/> agent.
        /// </summary>
        public NumericAgent Agent => _agent;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="ExprBuilder{T}"/> constructor.
        /// </summary>
        /// <param name="phrase">The target language <see cref="Phrasebook"/>.</param>
        /// <param name="agent">Agent for handling the <see cref="Numeric{T}"/> type.</param>
        public ExprBuilder(Phrasebook phrase, NumericAgent<T> agent)
        {
            _stack = new Stack<(DimExpr, NumExpr<T>)>(8);
            _agent = agent;
            _phrase = phrase;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Encode <see cref="ASTNode"/> expression in the target language.
        /// </summary>
        /// <param name="node">Node to encode.</param>
        /// <returns>
        /// (<see cref="DimExpr"/>, <see cref="NumExpr"/>) expressions
        /// for the given <paramref name="node"/>, encoded in the target language.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when internal stack processing goes wrong (which should never happen).
        /// </exception>
        public (DimExpr, NumExpr) Encode(ASTNode node)
        {
            _stack.Clear();
            node.Accept(this);
            if (_stack.Count != 1)
            {
                throw new InvalidOperationException($"{nameof(ExprBuilder<T>)}.{nameof(Encode)}({node}): invalid stack processing.");
            }
            return _stack.Pop();
        }

        /////////////////////////////////////////////////////////////////////////////////
        //
        //      Visitor implementation
        //

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public void Visit(ASTNumber node)
        {
            bool isvalid = _agent.TryParse(node.Number, out T n);
            if (!isvalid)
            {
                // This should never happen (node.Number has been syntactically verified before it came here):
                throw new InvalidOperationException($"{nameof(ExprBuilder<T>)}.{nameof(Visit)}({nameof(ASTNumber)} {node}): invalid number.");
            }
            _stack.Push((
                new(Dimension.None, _phrase.DimensionExpr()),
                new(true, new(n), _agent.ToTargetString(n))
            ));
        }

        public void Visit(ASTLiteral node)
        {
            string? constant = _agent.TryMatchCSharpConstant(node.Literal, out T value);
            bool isvalid = (constant is not null) || _agent.TryParse(node.Literal, out value);
            _stack.Push((
                new(Dimension.None, _phrase.DimensionExpr()),
                new(isvalid, new(isvalid ? value : _agent.One), constant ?? node.Literal)
            ));
        }

        public void Visit(ASTMagnitude node)
        {
            DimExpr dimExpr;
            if (node.Exponent == -1)    // Fake -1 denoting dimensionless?    
            {
                dimExpr = new(Dimension.None, _phrase.DimensionExpr());
            }
            else
            {
                Magnitude m = (Magnitude)node.Exponent;
                dimExpr = new(new(m), _phrase.DimensionExpr(m));
            }

            _stack.Push((
                dimExpr,
                new(true, new(_agent.One), _agent.ToTargetString(_agent.One))
            ));
        }

        public void Visit(ASTUnit node)
        {
            Unit unit = node.Unit;
            if (unit.Factor is NumExpr<T> factor)
            {
                _stack.Push((
                    new(unit.Sense.Value, _phrase.SenseProperty(unit), unit.Sense.UnfoldedCode),
                    new(factor.IsReal, factor.Value, _phrase.FactorProperty(unit), factor.UnfoldedCode)
                ));
            }
            else
            {
                throw new InvalidOperationException(
                    $"{nameof(ExprBuilder<T>)}.{nameof(Visit)}({nameof(ASTUnit)} {node}): invalid unit factor."
                );
            }
        }

        public void Visit(ASTUnary node)
        {
            (DimExpr dimExpr, NumExpr<T> numExpr) = _stack.Pop();
            _stack.Push((
                dimExpr,
                new(numExpr.IsReal, numExpr.Value, (node.Plus ? "+" : "-") + numExpr.SimpleCode)
            ));
        }

        public void Visit(ASTParenthesized node)
        {
            (DimExpr dimExpr, NumExpr<T> numExpr) = _stack.Pop();
            _stack.Push((
                new(dimExpr.Value, Parenthesize(dimExpr.SimpleCode), Parenthesize(dimExpr.UnfoldedCode)),
                new(numExpr.IsReal, numExpr.Value, Parenthesize(numExpr.SimpleCode), Parenthesize(numExpr.UnfoldedCode))
            ));

            // Encloses expression string within parentheses (if neccessary).
            static string Parenthesize(string expr)
            {
                int pcount = 0;
                foreach (char c in expr)
                {
                    if (c == '(')
                        ++pcount;
                    else if (c == ')')
                        --pcount;
                    else if ((c != ' ') && (pcount == 0))
                        return '(' + expr + ')';
                }
                return expr;
            }
        }
        public void Visit(ASTProduct node)
        {
            (DimExpr rhsDim, NumExpr<T> rhsNum) = _stack.Pop();
            (DimExpr lhsDim, NumExpr<T> lhsNum) = _stack.Pop();

            string op = " * ";

            DimExpr resultDim =
                lhsDim.Value == Dimension.None ?
                    rhsDim :
                rhsDim.Value == Dimension.None ?
                    lhsDim :
                    new(
                        lhsDim.Value * rhsDim.Value,
                        lhsDim.SimpleCode + op + rhsDim.SimpleCode,
                        "(" + lhsDim.UnfoldedCode + op + rhsDim.UnfoldedCode + ")"
                    );

            NumExpr<T> resultNum = new(
                    lhsNum.IsReal && rhsNum.IsReal,
                    lhsNum.Value * rhsNum.Value,
                    lhsNum.SimpleCode + op + rhsNum.SimpleCode,
                    "(" + lhsNum.UnfoldedCode + op + rhsNum.UnfoldedCode + ")"
            );

            _stack.Push((resultDim, resultNum));
        }

        public void Visit(ASTQuotient node)
        {
            (DimExpr rhsDim, NumExpr<T> rhsNum) = _stack.Pop();
            (DimExpr lhsDim, NumExpr<T> lhsNum) = _stack.Pop();

            string op = " / ";

            DimExpr resultDim =
                rhsDim.Value == Dimension.None ?
                    lhsDim :
                    new(
                        lhsDim.Value / rhsDim.Value,
                        lhsDim.SimpleCode + op + rhsDim.SimpleCode,
                        "(" + lhsDim.UnfoldedCode + op + rhsDim.UnfoldedCode + ")"
                    );

            NumExpr<T> resultNum = new(
                lhsNum.IsReal && rhsNum.IsReal,
                lhsNum.Value / rhsNum.Value,
                lhsNum.SimpleCode + op + rhsNum.SimpleCode,
                "(" + lhsNum.UnfoldedCode + op + rhsNum.UnfoldedCode + ")"
            );

            _stack.Push((resultDim, resultNum));
        }

        public void Visit(ASTSum node)
        {
            (DimExpr rhsDim, NumExpr<T> rhsNum) = _stack.Pop();
            (DimExpr lhsDim, NumExpr<T> lhsNum) = _stack.Pop();

            string op = " + ";

            DimExpr resultDim = rhsDim.Value == lhsDim.Value ?
                rhsDim :
                throw new InvalidOperationException(
                    $"{nameof(ExprBuilder<T>)}.{nameof(Visit)}({nameof(ASTSum)} {node}): cannot add terms of different dimensions."
                );

            NumExpr<T> resultNum = new(
                lhsNum.IsReal && rhsNum.IsReal,
                lhsNum.Value + rhsNum.Value,
                lhsNum.SimpleCode + op + rhsNum.SimpleCode,
                "(" + lhsNum.UnfoldedCode + op + rhsNum.UnfoldedCode + ")"
            );

            _stack.Push((resultDim, resultNum));
        }

        public void Visit(ASTDifference node)
        {
            (DimExpr rhsDim, NumExpr<T> rhsNum) = _stack.Pop();
            (DimExpr lhsDim, NumExpr<T> lhsNum) = _stack.Pop();

            string op = " - ";

            DimExpr resultDim = rhsDim.Value == lhsDim.Value ?
                rhsDim :
                throw new InvalidOperationException(
                    $"{nameof(ExprBuilder<T>)}.{nameof(Visit)}({nameof(ASTDifference)} {node}): cannot subtract terms of different dimensions."
                );

            NumExpr<T> resultNum = new(
                lhsNum.IsReal && rhsNum.IsReal,
                lhsNum.Value - rhsNum.Value,
                lhsNum.SimpleCode + op + rhsNum.SimpleCode,
                "(" + lhsNum.UnfoldedCode + op + rhsNum.UnfoldedCode + ")"
            );

            _stack.Push((resultDim, resultNum));
        }
        #endregion
    }
}
