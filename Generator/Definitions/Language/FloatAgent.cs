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
    /// The <see cref="float"/> type numeric agent.
    /// </summary>
    public class FloatAgent : NumericAgent<float>
    {
        #region Fields
        private readonly Func<float, string> _toTargetString;
        #endregion

        #region Properties
        /// <summary>
        /// Neutral (identity) element for multiplying <see cref="float"/> numbers.
        /// </summary>
        public override float One => 1.0f;

        /// <summary>
        /// Neutral (identity) element for adding <see cref="float"/> numbers.
        /// </summary>
        public override float Zero => 0.0f;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="FloatAgent"/> constructor.
        /// </summary>
        /// <param name="numericTerm">Related numeric term.</param>
        /// <param name="toTargetString">Function to convert <see cref="float"/> number to the target language string.</param>
        public FloatAgent(Term numericTerm, Func<float, string> toTargetString)
            : base(numericTerm)
        {
            _toTargetString = toTargetString;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts a string to its <see cref="float"/> equivalent.
        /// </summary>
        /// <param name="numeric">A string to convert.</param>
        /// <param name="result"><see cref="float"/> equivalent of the <paramref name="numeric"/> string parameter.</param>
        /// <returns><see langword="true"/> if <paramref name="numeric"/> string has been converted successfully, otherwise <see langword="false"/>.</returns>
        public override bool TryParse(string numeric, out float result) =>
            float.TryParse(numeric, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

        /// <summary>
        /// Checks the literal to see if it is the name of a math constant in C#.
        /// </summary>
        /// <param name="literal">The literal to be checked.</param>
        /// <param name="constant">Value of the constant as a number of type <see cref="float"/>.</param>
        /// <returns>
        /// Standard, fully qualified name of the C# constant - or - 
        /// <see langword="null"/>, if the <paramref name="literal"/> is not a constant.
        /// </returns>
        public override string? TryMatchCSharpConstant(string literal, out float constant)
        {
            string? name = CheckCSharpMathConstant(literal, out double value);
            constant = (name is null) ? default : (float)value;
            return name;
        }

        /// <summary>
        /// Converts <see cref="float"/> number to the target language string equivalent.
        /// </summary>
        /// <param name="number">A number to convert.</param>
        /// <returns><paramref name="number"/> in a text form (in the target language).</returns>
        public override string ToTargetString(float number) => _toTargetString(number);
        #endregion
    }
}
