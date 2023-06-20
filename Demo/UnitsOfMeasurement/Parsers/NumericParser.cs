/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Globalization;

namespace Demo.UnitsOfMeasurement
{
    internal delegate bool NumericParserDelegate<T>(string input,
                                                    NumberStyles style,
                                                    IFormatProvider fp,
                                                    out T result) where T : struct;

    internal static class NumericParser
    {
        public static NumericParserDelegate<double> TryParseDouble = double.TryParse;
        public static NumericParserDelegate<decimal> TryParseDecimal = decimal.TryParse;
        public static NumericParserDelegate<float> TryParseFloat = float.TryParse;

        public static System.Delegate SelectDelegate(Type type)
        {
            if (type == typeof(double)) return TryParseDouble;
            if (type == typeof(decimal)) return TryParseDecimal;
            if (type == typeof(float)) return TryParseFloat;

            throw new NotImplementedException(String.Format("Not implemented numeric parser for type \"{0}\"", type.Name));
        }
    }
}
