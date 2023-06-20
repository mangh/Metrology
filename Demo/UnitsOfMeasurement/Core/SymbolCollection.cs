/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Demo.UnitsOfMeasurement
{
    /// <summary>
    /// A collection of symbols (tags) of a unit.
    /// </summary>
    public class SymbolCollection : IEnumerable<string>
    {
        #region Fields
        private readonly string[] m_collection;
        #endregion

        #region Properties
        /// <summary>
        /// Number of symbols.
        /// </summary>
        public int Count => m_collection.Length;

        /// <summary>
        /// Default symbol.
        /// </summary>
        public string Default => m_collection[0];
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="SymbolCollection"/> constructor.
        /// </summary>
        /// <param name="symbols">Symbol array.</param>
        /// <exception cref="ArgumentException"></exception>
        public SymbolCollection(params string[] symbols)
        {
            if ((symbols is null) || (symbols.Length < 1) || (Array.FindIndex(symbols, s => string.IsNullOrWhiteSpace(s)) >= 0))
                throw new ArgumentException("Symbol collection can neither be empty/null nor contain empty/null items.");

            m_collection = symbols;
        }
        #endregion

        #region IEnumerable
        public IEnumerator<string> GetEnumerator() => (m_collection as IEnumerable<string>).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => m_collection.GetEnumerator();
        #endregion

        #region Indexer, Intersection
        /// <summary>
        /// Returns symbol of the specified index.
        /// </summary>
        /// <param name="index">Symbol index.</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public string this[int index] => m_collection[index];

        /// <summary>
        /// Finds the index of the first occurence of a symbol in the unit's symbol list.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>Index (zero-based) of the first occurence of the <paramref name="symbol"/>, if found; otherwise -1.</returns>
        public int IndexOf(string symbol) => Array.FindIndex(m_collection, s => string.CompareOrdinal(s, symbol) == 0);

        /// <summary>
        /// Checks whether any of the unit's symbols appear in another symbol collection.
        /// </summary>
        /// <param name="another">Another symbol collection.</param>
        /// <returns><see langword="true"/> when common elements are found, otherwise <see langword="false"/>.</returns>
        public bool Intersects(IEnumerable<string> another) => another.Any(s => IndexOf(s) >= 0);
        #endregion

        #region Formatting
        public override string ToString() => $"{{\"{string.Join("\", \"", m_collection)}\"}}";
        #endregion
    }
}
