/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System.Collections.Generic;
using System.Globalization;

namespace Mangh.Metrology.Language
{
    /// <summary>
    /// C# language context.
    /// </summary>
    public class ContextCS : Context
    {
        #region Target language properties
        /// <summary>
        /// C# Language ID.
        /// </summary>
        public override ID Id => ID.CS;

        /// <summary>
        /// C# phrases.
        /// </summary>
        public override Phrasebook Phrase => _phrase;

        /// <summary>
        /// Builders of <see cref="DimExpr"/> and <see cref="NumExpr{T}"/> expressions
        /// for <see cref="Numeric{T}"/> types supported in the C# language.
        /// </summary>
        public override IEnumerable<IExprBuilder> ExprBuilders => _builders;
        #endregion

        #region Implementation details
        private static readonly Phrasebook _phrase = new();

        private static readonly IExprBuilder[] _builders =
        {
            new ExprBuilder<double>(
                _phrase,
                new DoubleAgent(
                    new Term(sourceKeyword: "double", targetKeyword: "double", targetTypename: "System.Double"),
                    d => string.Format(CultureInfo.InvariantCulture, "{0}d", d)
                )
            ),
            new ExprBuilder<decimal>(
                _phrase,
                new DecimalAgent(
                    new Term(sourceKeyword: "decimal", targetKeyword: "decimal", targetTypename: "System.Decimal"),
                    m => (m == decimal.Zero) ? "decimal.Zero" :
                         (m == decimal.One) ? "decimal.One" :
                         string.Format(CultureInfo.InvariantCulture, "{0}m", m)
                )
            ),
            new ExprBuilder<float>(
                _phrase,
                new FloatAgent(
                    new Term(sourceKeyword: "float", targetKeyword: "float", targetTypename: "System.Single"),
                    f => string.Format(CultureInfo.InvariantCulture, "{0}f", f)
                )
            )
        };

        static ContextCS()
        {
            // File names
            _phrase[Language.Phrase.ALIASES] = "Aliases";
            _phrase[Language.Phrase.CATALOG] = "Catalog";
            _phrase[Language.Phrase.DEFINITIONS] = "Definitions";
            _phrase[Language.Phrase.REPORT] = "Report";
            _phrase[Language.Phrase.SCALE] = "Scale";
            _phrase[Language.Phrase.UNIT] = "Unit";

            // File extensions
            _phrase[Language.Phrase.SOURCE_EXT] = "cs";

            // Phrases
            _phrase[Language.Phrase.SCOPE_SEPARATOR] = ".";
            _phrase[Language.Phrase.SENSE_PROPERTY] = "Sense";
            _phrase[Language.Phrase.FACTOR_PROPERTY] = "Factor";
            _phrase[Language.Phrase.QUANTITY_FORMAT] = "{0} {1}";
        }
        #endregion
    }
}
