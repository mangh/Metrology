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
    /// The <see cref="double"/> type numeric agent.
    /// </summary>
    public class DoubleAgent : NumericAgent<double>
    {
        #region Fields
        private readonly Func<double, string> _toTargetString;
        #endregion

        #region Properties
        /// <summary>
        /// Neutral (identity) element for multiplying <see cref="double"/> numbers.
        /// </summary>
        public override double One => 1.0d;

        /// <summary>
        /// Neutral (identity) element for adding <see cref="double"/> numbers.
        /// </summary>
        public override double Zero => 0.0d;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="DoubleAgent"/> constructor.
        /// </summary>
        /// <param name="numericTerm">Related numeric term.</param>
        /// <param name="toTargetString">Function to convert <see cref="double"/> number to the target language string.</param>
        public DoubleAgent(Term numericTerm, Func<double, string> toTargetString)
            : base(numericTerm)
        {
            _toTargetString = toTargetString;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts a string to its <see cref="double"/> equivalent.
        /// </summary>
        /// <param name="numeric">A string to convert.</param>
        /// <param name="result"><see cref="double"/> equivalent of the <paramref name="numeric"/> string parameter.</param>
        /// <returns><see langword="true"/> if <paramref name="numeric"/> string has been converted successfully, otherwise <see langword="false"/>.</returns>
        public override bool TryParse(string numeric, out double result) =>
            double.TryParse(numeric, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

        /// <summary>
        /// Checks the literal to see if it is the name of a math constant in C#.
        /// </summary>
        /// <param name="literal">The literal to be checked.</param>
        /// <param name="constant">Value of the constant as a number of type <see cref="double"/>.</param>
        /// <returns>
        /// Standard, fully qualified name of the C# constant - or - 
        /// <see langword="null"/>, if the <paramref name="literal"/> is not a constant.
        /// </returns>
        public override string? TryMatchCSharpConstant(string literal, out double constant)
            => CheckCSharpMathConstant(literal, out constant);

        /// <summary>
        /// Converts <see cref="double"/> number to the target language string equivalent.
        /// </summary>
        /// <param name="number">A number to convert.</param>
        /// <returns><paramref name="number"/> in a text form (in the target language).</returns>
        public override string ToTargetString(double number) => _toTargetString(number);
        #endregion
    }
}
