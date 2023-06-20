/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Globalization;

namespace Mangh.Metrology.Language
{
    /// <summary>
    /// The <see cref="decimal"/> type numeric agent.
    /// </summary>
    public class DecimalAgent : NumericAgent<decimal>
    {
        #region Fields
        private readonly Func<decimal, string> _toTargetString;
        #endregion

        #region Properties
        /// <summary>
        /// Neutral (identity) element for multiplying <see cref="decimal"/> numbers.
        /// </summary>
        public override decimal One => decimal.One;

        /// <summary>
        /// Neutral (identity) element for adding <see cref="decimal"/> numbers.
        /// </summary>
        public override decimal Zero => decimal.Zero;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="DecimalAgent"/> constructor.
        /// </summary>
        /// <param name="numericTerm">Related numeric term.</param>
        /// <param name="toTargetString">Function to convert <see cref="decimal"/> number to the target language string.</param>
        public DecimalAgent(Term numericTerm, Func<decimal, string> toTargetString)
            : base(numericTerm)
        {
            _toTargetString = toTargetString;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts a string to its <see cref="decimal"/> equivalent.
        /// </summary>
        /// <param name="numeric">A string to convert.</param>
        /// <param name="result"><see cref="decimal"/> equivalent of the <paramref name="numeric"/> string parameter.</param>
        /// <returns><see langword="true"/> if the <paramref name="numeric"/> string has been converted successfully, otherwise <see langword="false"/>.</returns>
        public override bool TryParse(string numeric, out decimal result) =>
            decimal.TryParse(numeric, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

        /// <summary>
        /// Checks the literal to see if it is the name of a math constant in C#.
        /// </summary>
        /// <param name="literal">The literal to be checked.</param>
        /// <param name="constant">Value of the constant as a number of type <see cref="decimal"/>.</param>
        /// <returns>
        /// Standard, fully qualified name of the C# constant - or - 
        /// <see langword="null"/>, if the <paramref name="literal"/> is not a constant.
        /// </returns>
        public override string? TryMatchCSharpConstant(string literal, out decimal constant)
        {
            string? name = CheckCSharpMathConstant(literal, out double value);
            constant = (name is null) ? default : (decimal)value;
            return name;
        }

        /// <summary>
        /// Converts <see cref="decimal"/> number to the target language string equivalent.
        /// </summary>
        /// <param name="number">A number to convert.</param>
        /// <returns>The <paramref name="number"/> in a text form (in the target language).</returns>
        public override string ToTargetString(decimal number) => _toTargetString(number);
        #endregion
    }
}
