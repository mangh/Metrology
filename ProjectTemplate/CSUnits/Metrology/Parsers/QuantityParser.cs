/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace %NAMESPACE%
{
    public class QuantityParser<T>
        where T : struct
    {
        #region Quantity tokenizer delegate
        public delegate Unit<T>? Tokenizer(string quantity, IEnumerable<Unit<T>> units, out string? number, out string symbol);
        #endregion

        #region Properties
        public IEnumerable<Unit<T>> Units { get; private set; }
        public NumberStyles NumberStyle { get; set; }
        public Tokenizer TryTokenize { get; set; }
        #endregion

        #region Constructor(s)
        public QuantityParser(IEnumerable<Unit<T>> allowedUnits)
        {
            Units = allowedUnits;
            NumberStyle = NumberStyles.Float;
            TryTokenize = TokenizePostfixed;
        }
        public QuantityParser(IEnumerable<Scale<T>> allowedScales) :
            this(Catalog.Units<T>(allowedScales))
        {
        }
        public QuantityParser(int family) :
            this(Catalog.Units<T>(family))
        {
        }
        #endregion

        #region Parsing
        public bool TryParse(string input, out IQuantity<T>? result)
            => TryParse(input, CultureInfo.CurrentCulture, this.NumberStyle, out result);

        public bool TryParse(string input, IFormatProvider fp, out IQuantity<T>? result)
            => TryParse(input, fp, this.NumberStyle, out result);

        public bool TryParse(string input, IFormatProvider fp, NumberStyles style, out IQuantity<T>? result)
        {
            Unit<T>? unit = TryTokenize(input, Units, out string? number, out string symbol);
            if ((unit is not null) && TryParseNumber(number!, style, fp, out T value))
            {
                result = unit.From(value);
                return true;
            }
            result = null;
            return false;
        }
        #endregion

        #region Tokenizers
        public static Unit<T>? TokenizePrefixed(string quantity, IEnumerable<Unit<T>> units, out string? number, out string symbol)
            => QuantityTokenizer.Tokenize(quantity, units, u => quantity.StartsWith(u, StringComparison.Ordinal), out number, out symbol) as Unit<T>;

        public static Unit<T>? TokenizePostfixed(string quantity, IEnumerable<Unit<T>> units, out string? number, out string symbol)
            => QuantityTokenizer.Tokenize(quantity, units, u => quantity.EndsWith(u, StringComparison.Ordinal), out number, out symbol) as Unit<T>;

        private static readonly NumericParserDelegate<T> TryParseNumber = (NumericParser.SelectDelegate(typeof(T)) as NumericParserDelegate<T>)!;
        #endregion
    }

    public static class QuantityTokenizer
    {
        /// <summary>Tokenize a quantity string into a <paramref name="number"/> and a unit <paramref name="symbol"/>.</summary>
        /// <param name="quantity">Input quantity string to be analyzed.</param>
        /// <param name="units">Units allowed in the input string (providing allowed unit symbols).</param>
        /// <param name="matchunitsymbol">Predicate to check whether given unit symbol is part of the input string.</param>
        /// <param name="number">Number found in the input string; <see langword="null"/> if it is not found.</param>
        /// <param name="symbol">Unit symbol found in the input string.</param>
        /// <returns>
        /// <see cref="Unit"/> corresponding to the the unit <paramref name="symbol"/> in the <paramref name="quantity"/> string;
        /// <see langword="null"/> if none of the allowed units has been matched.
        /// </returns>
        public static Unit? Tokenize(string quantity, IEnumerable<Unit> units, Predicate<string> matchunitsymbol, out string? number, out string symbol)
        {
            Unit? unit = null;
            symbol = string.Empty;
            foreach (var u in units)
            {
                foreach (var s in u.Symbol)
                {
                    if ((symbol.Length < s.Length) && matchunitsymbol(s))
                    {
                        symbol = s;
                        unit = u;
                    }
                }
            }
            number = (unit is null) ? null : quantity[..^symbol.Length].Trim();
            return unit;
        }
    }
}
