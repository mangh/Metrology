/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology.CS
{
    /// <summary>
    /// String literal evaluation.
    /// </summary>
    internal static class Eval
    {
#pragma warning disable IDE0060 // Remove unused parameter

        /// <summary>
        /// Evaluates string into Float.
        /// </summary>
        public static NumeralFloat? Float(string literal) => null;

        /// <summary>
        /// Evaluates string into Decimal.
        /// </summary>
        public static NumeralDecimal? Decimal(string literal) => null;

#pragma warning restore IDE0060 // Remove unused parameter

        /// <summary>
        /// Evaluates string into Double.
        /// </summary>
        public static NumeralDouble? Double(string literal) => literal switch
        {
            "System.Math.PI" => new NumeralDouble(System.Math.PI),
            "System.Math.E" => new NumeralDouble(System.Math.E),
            _ => null,
        };
    }
}
