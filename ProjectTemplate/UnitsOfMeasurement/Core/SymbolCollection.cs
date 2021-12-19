/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/unitsofmeasurement


********************************************************************************/

using System;
using System.Collections;   // IEnumerable.GetEnumerator()
using System.Collections.Generic;
using System.Linq;

namespace Metrological.Namespace
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SymbolCollection : IEnumerable<string>
    {
        #region Fields
        private readonly string[] m_collection;
        #endregion

        #region Properties
        public int Count => m_collection.Length;
        public string Default => m_collection[0];
        #endregion

        #region Constructor(s)
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
        public string this[int index]
        {
            get { return m_collection[index]; }
        }
        public int IndexOf(string symbol) => Array.FindIndex(m_collection, s => string.CompareOrdinal(s, symbol) == 0);
        public bool Intersects(IEnumerable<string> symbols) => symbols.Any(s => IndexOf(s) >= 0);
        #endregion

        #region Formatting
        public override string ToString() => $"{{\"{string.Join("\", \"", m_collection)}\"}}";
        #endregion
    }
}
