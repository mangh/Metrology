/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology.CS
{
    /// <summary>
    /// Constant numeral expressions for C#.
    /// </summary>
    public static class ConstantNumExpr
    {
        /// <summary>
        /// Double-precision floating-point number One (<c>1.0d</c>) as a C# expression.
        /// </summary>
        public static readonly NumeralExpression DoubleOne = ConstNumExpr(NumeralDouble.One);

        /// <summary>
        /// Double-precision floating-point number Zero (<c>0.0d</c>) as a C# expression.
        /// </summary>
        public static readonly NumeralExpression DoubleZero = ConstNumExpr(NumeralDouble.Zero);

        /// <summary>
        /// Single-precision floating-point number One (<c>1.0f</c>) as a C# expression.
        /// </summary>
        public static readonly NumeralExpression FloatOne = ConstNumExpr(NumeralFloat.One);

        /// <summary>
        /// Single-precision floating-point number Zero (<c>0.0f</c>) as a C# expression.
        /// </summary>
        public static readonly NumeralExpression FloatZero = ConstNumExpr(NumeralFloat.Zero);

        /// <summary>
        /// Decimal floating-point number One (<c>1.0m</c>) as a C# expression.
        /// </summary>
        public static readonly NumeralExpression DecimalOne = ConstNumExpr(NumeralDecimal.One);

        /// <summary>
        /// Decimal floating-point number Zero (<c>1.0m</c>) as a C# expression.
        /// </summary>
        public static readonly NumeralExpression DecimalZero = ConstNumExpr(NumeralDecimal.Zero);

        private static NumeralExpression ConstNumExpr(Numeral constant) => new(true, constant, NumExprEncoder.Stringify(constant));
    }
}
