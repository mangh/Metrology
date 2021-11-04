/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Collections.Generic;

namespace Mangh.Metrology.CS
{
    /// <summary>
    /// Encoder for dimensional expressions.
    /// </summary>
    internal class DimExprEncoder : IDimExprEncoder
    {
        #region Fields
        private readonly Stack<DimensionalExpression> _stack;
        #endregion

        #region Constructor(s)
        public DimExprEncoder() => _stack = new Stack<DimensionalExpression>(16);
        #endregion

        /// <summary>
        /// Encode AST node dimensional expression.
        /// </summary>
        /// <param name="node">node to encode</param>
        /// <returns>encoded dimensional expression</returns>
        /// <exception cref="OverflowException">thrown when dimension calculation overflows.</exception>
        /// <exception cref="ArgumentException">thrown when dimension arithmetic finds error.</exception>
        /// <exception cref="InvalidOperationException">thrown when encoding (visiting) the expression goes wrong (which should never happen).</exception>
        public DimensionalExpression Accept(ASTNode node)
        {
            _stack.Clear();
            node.Accept(this);
            if (_stack.Count != 1)
            {
                throw new InvalidOperationException($"{nameof(DimExprEncoder)}.{nameof(Accept)}({node}): invalid stack processing");
            }
            return _stack.Pop();
        }

        public void Encode(ASTNumber node) => _stack.Push(ConstantDimExpr.Dimensionless);
        public void Encode(ASTLiteral node) => _stack.Push(ConstantDimExpr.Dimensionless);
        public void Encode(ASTMagnitude node)
        {
            if (node.Exponent == -1)
            {
                // fake -1 denoting dimensionless
                _stack.Push(ConstantDimExpr.Dimensionless);
            }
            else
            {
                Magnitude m = (Magnitude)node.Exponent;
                _stack.Push(new DimensionalExpression(new Dimension(m), $"Dimension.{m}"));
            }
        }
        public void Encode(ASTUnit node) => _stack.Push(new DimensionalExpression(node.Unit.Sense.Value, $"{node.Unit.Typename}.Sense"));

        /// <summary>
        /// Supposed to create dimensional expression of the NONSENSICAL form: <c>&#177;Dimension</c>.
        /// </summary>
        /// <remarks>Note: the method should NEVER be called:
        /// <list type="bullet">
        /// <item><description><c>ParseUnit</c> does not use <c>ASTUnary</c>,</description></item>
        /// <item><description><c>ParseScale</c> (which uses <c>ASTUnary</c>) does not encode dimension.</description></item>
        /// </list>
        /// </remarks>
        public void Encode(ASTUnary node)
            => throw new InvalidOperationException($"{nameof(DimExprEncoder)}.{nameof(Encode)}({nameof(ASTUnary)} {node})");

        public void Encode(ASTParenthesized node)
        {
            DimensionalExpression nested = _stack.Pop();
            _stack.Push(new DimensionalExpression(nested.Value, $"({nested.Code})"));
        }
        public void Encode(ASTProduct node)
        {
            DimensionalExpression rhs = _stack.Pop();
            DimensionalExpression lhs = _stack.Pop();
            if (lhs.Value == Dimension.None)
                _stack.Push(new DimensionalExpression(rhs.Value, rhs.Code));
            else if (rhs.Value == Dimension.None)
                _stack.Push(new DimensionalExpression(lhs.Value, lhs.Code));
            else
                _stack.Push(new DimensionalExpression(lhs.Value * rhs.Value, $"{lhs.Code} * {rhs.Code}"));
        }
        public void Encode(ASTQuotient node)
        {
            DimensionalExpression rhs = _stack.Pop();
            DimensionalExpression lhs = _stack.Pop();
            if (rhs.Value == Dimension.None)
                _stack.Push(new DimensionalExpression(lhs.Value, lhs.Code));
            else
                _stack.Push(new DimensionalExpression(lhs.Value / rhs.Value, $"{lhs.Code} / {rhs.Code}"));
        }

        /// <summary>
        /// Supposed to create dimensional expression of the NONSENSICAL form: <c>Dimension+Dimension</c>.
        /// </summary>
        /// <remarks>Note: the method should NEVER be called:
        /// <list type="bullet">
        /// <item><description><c>ParseUnit</c> does not use <c>ASTSum</c>,</description></item>
        /// <item><description><c>ParseScale</c> (which uses <c>ASTSum</c>) does not encode dimension.</description></item>
        /// </list>
        /// </remarks>
        public void Encode(ASTSum node)
            => throw new InvalidOperationException($"{nameof(DimExprEncoder)}.{nameof(Encode)}({nameof(ASTSum)} {node})");

        /// <summary>
        /// Supposed to create dimensional expression of the NONSENSICAL form: <c>Dimension-Dimension</c>.
        /// </summary>
        /// <remarks>Note: the method should NEVER be called:
        /// <list type="bullet">
        /// <item><description><c>ParseUnit</c> does not use <c>ASTDifference</c>,</description></item>
        /// <item><description><c>ParseScale</c> (which uses <c>ASTDifference</c>) does not encode dimension.</description></item>
        /// </list>
        /// </remarks>
        public void Encode(ASTDifference node)
            => throw new InvalidOperationException($"{nameof(DimExprEncoder)}.{nameof(Encode)}({nameof(ASTDifference)} {node})");
    }
}
