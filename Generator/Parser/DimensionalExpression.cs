/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology
{
    /// <summary>
    /// Dimensional expression.
    /// </summary>
    public class DimensionalExpression
    {
        /// <summary>
        /// Expression value.
        /// </summary>
        public Dimension Value { get; }

        /// <summary>
        /// Expression encoded as a string for a target language.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Dimensional expression constructor.
        /// </summary>
        /// <param name="value">expression value</param>
        /// <param name="code">expression encoded as a string for a target language.</param>
        public DimensionalExpression(Dimension value, string code)
        {
            Value = value;
            Code = code;
        }

        /// <summary>
        /// Expression value as a string.
        /// </summary>
        /// <returns>value of the expression as a C# string.</returns>
        public override string ToString() => Value.ToString();
    }
}
