/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mangh.Metrology.Language
{
    /// <summary>
    /// C++ language context.
    /// </summary>
    public class ContextCPP : Context
    {
        #region Target language properties
        /// <summary>
        /// C++ Language ID.
        /// </summary>
        public override ID Id => ID.CPP;

        /// <summary>
        /// C++ phrases.
        /// </summary>
        public override Phrasebook Phrase => _phrase;

        /// <summary>
        /// Builders of <see cref="DimExpr"/> and <see cref="NumExpr{T}"/> expressions
        /// for <see cref="Numeric{T}"/> types supported by the C++ language.
        /// </summary>
        public override IEnumerable<IExprBuilder> ExprBuilders => _builders;
        #endregion

        #region Implementation details
        private static readonly Phrasebook _phrase = new();

        private static readonly IExprBuilder[] _builders = new IExprBuilder[]
        {
            new ExprBuilder<double>(
                _phrase,
                new DoubleAgent(
                    new Term(sourceKeyword: "double", targetKeyword: "double", targetTypename: "double"),
                    d => string.Format(CultureInfo.InvariantCulture, (d == Math.Truncate(d)) ? "{0:F1}" : "{0}", d)
                )
            ),
            new ExprBuilder<double>(
                _phrase,
                new DoubleAgent(
                    new Term(sourceKeyword: "longdouble", targetKeyword: "long double", targetTypename: "long double"),
                    d => string.Format(CultureInfo.InvariantCulture, (d == Math.Truncate(d)) ? "{0:F1}L" : "{0}L", d)
                )
            ),
            //new ExprBuilder<decimal>(
            //    _wordbook,
            //    new DecimalAgent(
            //        new Term(sourceKeyword: "decimal", targetKeyword: "decimal", targetTypename: "decimal"),
            //        m => (m == decimal.Zero) ? "decimal::Zero" :
            //             (m == decimal.One) ? "decimal::One" :
            //             string.Format(CultureInfo.InvariantCulture, "{0}m", m)
            //    )
            //),
            new ExprBuilder<float>(
                _phrase,
                new FloatAgent(
                    new Term(sourceKeyword: "float", targetKeyword: "float", targetTypename: "float"),
                    f => string.Format(CultureInfo.InvariantCulture, (f == Math.Truncate(f)) ? "{0:F1}f" : "{0}f", f)
                )
            )
        };

        static ContextCPP()
        {
            _phrase[Language.Phrase.ALIASES] = "aliases";
            _phrase[Language.Phrase.CATALOG] = "catalog";
            _phrase[Language.Phrase.DEFINITIONS] = "definitions";
            _phrase[Language.Phrase.REPORT] = "report";
            _phrase[Language.Phrase.SCALE] = "scale";
            _phrase[Language.Phrase.UNIT] = "unit";

            _phrase[Language.Phrase.SOURCE_EXT] = "h";

            _phrase[Language.Phrase.SCOPE_SEPARATOR] = "::";
            _phrase[Language.Phrase.SENSE_PROPERTY] = "sense";
            _phrase[Language.Phrase.FACTOR_PROPERTY] = "factor";
            _phrase[Language.Phrase.QUANTITY_FORMAT] = "%f %s";
        }
        #endregion
    }
}
