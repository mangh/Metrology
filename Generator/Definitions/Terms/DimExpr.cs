/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology
{
    /// <summary>
    /// Dimensional expression.
    /// </summary>
    public class DimExpr
    {
        /// <summary>
        /// Expression value.
        /// </summary>
        public Dimension Value { get; }

        /// <summary>
        /// Expression encoded as a string for the target language.
        /// </summary>
        public string SimpleCode { get; }

        /// <summary>
        /// Expression made only of dimensional constants in the target language.
        /// </summary>
        public string UnfoldedCode { get; }

        /// <summary>
        /// Dimensional expression constructor.
        /// </summary>
        /// <param name="value">Expression value.</param>
        /// <param name="simpleCode">Expression encoded as a string for the target language.</param>
        /// <param name="unfoldedCode"><paramref name="simpleCode"/> parameter equivalent made only of dimensional constants.</param>
        public DimExpr(Dimension value, string simpleCode, string? unfoldedCode = null)
        {
            Value = value;
            SimpleCode = simpleCode;
            UnfoldedCode = unfoldedCode ?? simpleCode;
        }

        /// <summary>
        /// Expression value as a string.
        /// </summary>
        /// <returns>Value of the expression as a string.</returns>
        public override string ToString() => Value.ToString();
    }
}
