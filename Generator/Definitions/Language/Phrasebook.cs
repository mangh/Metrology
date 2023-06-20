/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology.Language
{
    /// <summary>
    /// <see cref="Phrasebook"/> index.
    /// </summary>
    public enum Phrase
    {
        //////////////////////////////////////////////////////////////////
        //
        //  Base file names (w/o extension)
        //

        /// <summary>
        /// Aliases file name (w/o extension).
        /// </summary>
        ALIASES,

        /// <summary>
        /// Catalog file name (w/o extension).
        /// </summary>
        CATALOG,

        /// <summary>
        /// Definitions file name (w/o extension).
        /// </summary>
        DEFINITIONS,

        /// <summary>
        /// Report file name (w/o extension).
        /// </summary>
        REPORT,

        /// <summary>
        /// Scale template file name (w/o extension).
        /// </summary>
        SCALE,

        /// <summary>
        /// Unit template file name (w/o extension).
        /// </summary>
        UNIT,

        //////////////////////////////////////////////////////////////////
        //
        //  File extensions
        //

        /// <summary>
        /// Source code file extension.
        /// </summary>
        SOURCE_EXT,

        //////////////////////////////////////////////////////////////////
        //
        //  Phrases
        //

        /// <summary>
        /// Scope resolution operator string (e.g. "." for C# or "::" for C++).
        /// </summary>
        SCOPE_SEPARATOR,

        /// <summary>
        /// The name of the dimensional "<c>Sense</c>" property.
        /// </summary>
        SENSE_PROPERTY,

        /// <summary>
        /// The name of the conversion "<c>Factor</c>" property.
        /// </summary>
        FACTOR_PROPERTY,

        /// <summary>
        /// Default string for formatting quantities (pairs of value and unit).
        /// </summary>
        QUANTITY_FORMAT,

        //////////////////////////////////////////////////////////////////
        //
        //  Phrase count
        //

        /// <summary>
        /// Number of <see cref="Phrasebook"/> entries.
        /// </summary>
        COUNT
    }

    /// <summary>
    /// Phrases (specific to programming language).
    /// </summary>
    public class Phrasebook
    {
        #region Constants
        /// <summary>
        /// Number of <see cref="Phrasebook"/> entries.
        /// </summary>
        public const int SIZE = (int)Phrase.COUNT;
        #endregion

        #region Fields
        private readonly string[] _phrase = new string[SIZE];
        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets the <see cref="Phrasebook"/> entry.
        /// </summary>
        /// <param name="p"><see cref="Phrasebook"/> entry index.</param>
        /// <returns></returns>
        public string this[Phrase p]
        {
            get { return _phrase[(int)p];  }
            set { _phrase[(int)p] = value; }
        }

        /// <summary>
        /// Returns the textual form of the <see cref="Dimension"/> expression
        /// for the selected <see cref="Magnitude"/> value.
        /// </summary>
        /// <param name="magnitude">The selected <see cref="Magnitude"/>.</param>
        public virtual string DimensionExpr(Magnitude magnitude)
            => $"Dimension{this[Phrase.SCOPE_SEPARATOR]}{magnitude}";

        /// <summary>
        /// Returns the textual form of a <see cref="Metrology.Dimension"/> expression
        /// for a dimensionless value.
        /// </summary>
        public virtual string DimensionExpr()
            => $"Dimension{this[Phrase.SCOPE_SEPARATOR]}None";

        /// <summary>
        /// Returns the textual form of the "<c>Sense</c>" property for the given <see cref="Measure"/> object.
        /// </summary>
        /// <param name="m">The <see cref="Measure"/> object.</param>
        public virtual string SenseProperty(Measure m)
            => $"{m.TargetKeyword}{this[Phrase.SCOPE_SEPARATOR]}{this[Phrase.SENSE_PROPERTY]}";

        /// <summary>
        /// Returns the textual form of the "<c>Factor</c>" property for the given <see cref="Measure"/> object.
        /// </summary>
        /// <param name="m">The <see cref="Measure"/> object.</param>
        public virtual string FactorProperty(Measure m)
            => $"{m.TargetKeyword}{this[Phrase.SCOPE_SEPARATOR]}{this[Phrase.FACTOR_PROPERTY]}";
        #endregion
    }
}
