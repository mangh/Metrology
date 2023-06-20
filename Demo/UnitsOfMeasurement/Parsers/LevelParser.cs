/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Demo.UnitsOfMeasurement
{
    public class LevelParser<T>
        where T : struct
    {
        #region Level tokenizer delegate
        public delegate Scale<T>? Tokenizer(string level, IEnumerable<Scale<T>> scales, out string? number, out string symbol);
        #endregion

        #region Properties
        public IEnumerable<Scale<T>> Scales { get; private set; }
        public NumberStyles NumberStyle { get; set; }
        public Tokenizer TryTokenize { get; set; }
        #endregion

        #region Constructor(s)
        public LevelParser(IEnumerable<Scale<T>> allowedScales)
        {
            Scales = allowedScales;
            NumberStyle = NumberStyles.Float;
            TryTokenize = TokenizePostfixed;
        }
        public LevelParser(int family) :
            this(Catalog.Scales<T>(family))
        {
        }
        #endregion

        #region Parsing
        public bool TryParse(string input, out ILevel<T>? result)
            => TryParse(input, CultureInfo.CurrentCulture, this.NumberStyle, out result);

        public bool TryParse(string input, IFormatProvider fp, out ILevel<T>? result)
            => TryParse(input, fp, this.NumberStyle, out result);

        public bool TryParse(string input, IFormatProvider fp, NumberStyles style, out ILevel<T>? result)
        {
            Scale<T>? scale = TryTokenize(input, Scales, out string? number, out string symbol);
            if ((scale is not null) && TryParseNumber(number!, style, fp, out T value))
            {
                result = scale.From(value);
                return true;
            }
            result = null;
            return false;
        }
        #endregion

        #region Tokenizers
        public static Scale<T>? TokenizePrefixed(string level, IEnumerable<Scale<T>> scales, out string? number, out string symbol)
            => LevelTokenizer.Tokenize(level, scales, u => level.StartsWith(u, StringComparison.Ordinal), out number, out symbol) as Scale<T>;

        public static Scale<T>? TokenizePostfixed(string level, IEnumerable<Scale<T>> scales, out string? number, out string symbol)
            => LevelTokenizer.Tokenize(level, scales, u => level.EndsWith(u, StringComparison.Ordinal), out number, out symbol) as Scale<T>;

        private static readonly NumericParserDelegate<T> TryParseNumber = (NumericParser.SelectDelegate(typeof(T)) as NumericParserDelegate<T>)!;
        #endregion
    }

    public static class LevelTokenizer
    {
        /// <summary>Tokenize a level string into a <paramref name="number"/> and a unit <paramref name="symbol"/>.</summary>
        /// <param name="level">Input level string to be analyzed.</param>
        /// <param name="scales">Scales allowed in the <paramref name="level"/> string (to validate unit symbol).</param>
        /// <param name="matchunitsymbol">Predicate to check whether a unit symbol is present in the <paramref name="level"/> string.</param>
        /// <param name="number">Number found in the <paramref name="level"/> string; <see langword="null"/> if it is not found.</param>
        /// <param name="symbol">Unit symbol found in the <paramref name="level"/> string.</param>
        /// <returns>
        /// <see cref="Scale"/> corresponding to the the unit <paramref name="symbol"/> in the <paramref name="level"/> string;
        /// <see langword="null"/> if none of the allowed scales has been matched.
        /// </returns>
        public static Scale? Tokenize(string level, IEnumerable<Scale> scales, Predicate<string> matchunitsymbol, out string? number, out string symbol)
        {
            Scale? scale = null;
            symbol = string.Empty;
            foreach (var sc in scales)
            {
                foreach (var s in sc.Unit.Symbol)
                {
                    if ((symbol.Length < s.Length) && matchunitsymbol(s))
                    {
                        symbol = s;
                        scale = sc;
                    }
                }
            }
            number = (scale is null) ? null : level[..^symbol.Length].Trim();
            return scale;
        }
    }
}
