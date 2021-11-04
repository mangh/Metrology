/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mangh.Metrology.CS
{
    /// <summary>
    /// Encoder for numeral expressions.
    /// </summary>
    internal class NumExprEncoder : INumExprEncoder
    {
        #region Statics
        public static readonly NumExprEncoder Double = new(ConstantNumExpr.DoubleOne, ConstantNumExpr.DoubleZero);
        public static readonly NumExprEncoder Decimal = new(ConstantNumExpr.DecimalOne, ConstantNumExpr.DecimalZero);
        public static readonly NumExprEncoder Float = new(ConstantNumExpr.FloatOne, ConstantNumExpr.FloatZero);
        #endregion

        #region Fields
        private readonly Stack<NumeralExpression> _stack;
        private readonly NumeralExpression _one;
        private readonly NumeralExpression _zero;
        #endregion

        #region Properties
        public NumeralExpression One => _one;
        public NumeralExpression Zero => _zero;
        #endregion

        #region Constructor(s)
        public NumExprEncoder(NumeralExpression one, NumeralExpression zero)
        {
            _stack = new Stack<NumeralExpression>(8);
            _one = one;
            _zero = zero;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Encode AST node numeric expression.
        /// </summary>
        /// <param name="node">node to encode</param>
        /// <returns>encoded numeric expression</returns>
        /// <exception cref="InvalidOperationException">
        /// thrown when encoding (visiting) the expression goes wrong (which should never happen).
        /// </exception>
        public NumeralExpression Accept(ASTNode node)
        {
            _stack.Clear();
            node.Accept(this);
            if (_stack.Count != 1)
            {
                throw new InvalidOperationException($"{nameof(NumExprEncoder)}.{nameof(Accept)}({node}): invalid stack processing");
            }
            return _stack.Pop();
        }

        public void Encode(ASTNumber node) => _stack.Push(new(true, node.Number, Stringify(node.Number)));
        public void Encode(ASTLiteral node) => _stack.Push(new(false, _one.Value/* fake value (one) of the literal */, node.Literal));
        public void Encode(ASTMagnitude node) => _stack.Push(new(true, _one.Value, _one.Code));
        public void Encode(ASTUnit node)
        {
            UnitType unit = node.Unit;
            _stack.Push(new(unit.Factor.IsTrueValue, unit.Factor.Value, string.Format("{0}.Factor", node.Unit.Typename)));
        }
        public void Encode(ASTUnary node)
        {
            NumeralExpression expr = _stack.Pop();
            _stack.Push(node.Plus ?
                new(expr.IsTrueValue, expr.Value, string.Format("+{0}", expr.Code)) :
                new(expr.IsTrueValue, expr.Value.Negate(), string.Format("-{0}", expr.Code))
            );
        }
        public void Encode(ASTParenthesized node)
        {
            NumeralExpression expr = _stack.Pop();
            _stack.Push(new(expr.IsTrueValue, expr.Value, string.Format("({0})", expr.Code)));
        }
        public void Encode(ASTProduct node)
        {
            NumeralExpression rhs = _stack.Pop();
            NumeralExpression lhs = _stack.Pop();
            _stack.Push(new(lhs.IsTrueValue && rhs.IsTrueValue, lhs.Value * rhs.Value, string.Format("{0} * {1}", lhs.Code, rhs.Code)));
        }
        public void Encode(ASTQuotient node)
        {
            NumeralExpression rhs = _stack.Pop();
            NumeralExpression lhs = _stack.Pop();
            _stack.Push(new(lhs.IsTrueValue && rhs.IsTrueValue, lhs.Value / rhs.Value, string.Format("{0} / {1}", lhs.Code, rhs.Code)));
        }
        public void Encode(ASTSum node)
        {
            NumeralExpression rhs = _stack.Pop();
            NumeralExpression lhs = _stack.Pop();
            _stack.Push(new(lhs.IsTrueValue && rhs.IsTrueValue, lhs.Value + rhs.Value, string.Format("{0} + {1}", lhs.Code, rhs.Code)));
        }
        public void Encode(ASTDifference node)
        {
            NumeralExpression rhs = _stack.Pop();
            NumeralExpression lhs = _stack.Pop();
            _stack.Push(new(lhs.IsTrueValue && rhs.IsTrueValue, lhs.Value - rhs.Value, string.Format("{0} - {1}", lhs.Code, rhs.Code)));
        }

        public static string Stringify(Numeral? n)
        {
            if (n is NumeralDouble d)
                return string.Format(CultureInfo.InvariantCulture, "{0}d", d.Value);

            else if (n is NumeralFloat f)
                return string.Format(CultureInfo.InvariantCulture, "{0}f", f.Value);

            else if (n is NumeralDecimal m)
                return
                    (m.Value == decimal.Zero) ? "decimal.Zero" :
                    (m.Value == decimal.One) ? "decimal.One" :
                    string.Format(CultureInfo.InvariantCulture, "{0}m", m.Value);

            string arg = (n is null) ? "null" : $"{n.GetType().Name} {n}";

            throw new InvalidOperationException($"{nameof(Stringify)}({arg}): expected argument of type \"{nameof(NumeralDouble)}\", \"{nameof(NumeralFloat)}\" or \"{nameof(NumeralDecimal)}\".");
        }
        #endregion
    }
}
