/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Man.Metrology
{
    /// <summary>
    /// Numeral expression.
    /// </summary>
    public class NumeralExpression
    {
        /// <summary>Expression value.</summary>
        public Numeral Value { get; }

        /// <summary>Expression encoded as a string for a target language.</summary>
        public string Code { get; }

        /// <summary>
        /// Is the <see cref="Value"/> a true one or a fake value assigned e.g. to a string literal that cannot be evaluated.
        /// </summary>
        public bool IsTrueValue { get; }

        /// <summary>
        /// Numeral expression constructor.
        /// </summary>
        /// <param name="isTrueValue">Is the <paramref name="value"/> a true one or a fake one?</param>
        /// <param name="value">expression value</param>
        /// <param name="code">expression encoded for a target language.</param>
        public NumeralExpression(bool isTrueValue, Numeral value, string code)
        {
            IsTrueValue = isTrueValue;
            Value = value;
            Code = code;
        }

        /// <summary>
        /// Numeral expression as a string.
        /// </summary>
        /// <returns>Stringified <see cref="Value"/> (for true values) or <see cref="Code"/> (for a fake ones, of unknown value).</returns>
        public override string ToString() => IsTrueValue ? Value.ToString() : Code;
    }
}
