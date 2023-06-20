/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System.Collections.Generic;
using System.Linq;

namespace Mangh.Metrology.Language
{
    /// <summary>
    /// Supported languages identifiers.
    /// </summary>
    public enum ID
    {
        /// <summary>The C++ language identifier.</summary>
        CPP,

        /// <summary>The CSharp language identifier.</summary>
        CS
    }

    /// <summary>
    /// Target language context.
    /// </summary>
    public abstract class Context
    {
        #region Target language properties
        /// <summary>
        /// Target language <see cref="ID"/>.
        /// </summary>
        public abstract ID Id { get; }

        /// <summary>
        /// Target language phrases.
        /// </summary>
        public abstract Phrasebook Phrase { get; }

        /// <summary>
        /// Builders of <see cref="DimExpr"/> and <see cref="NumExpr"/> expressions
        /// for all <see cref="Numeric{T}"/> types supported in the target language.
        /// </summary>
        public abstract IEnumerable<IExprBuilder> ExprBuilders { get; }

        /// <summary>
        /// <see cref="NumericAgent"/> collection for all <see cref="Numeric{T}"/> types supported in the target language.
        /// </summary>
        public virtual IEnumerable<NumericAgent> NumericAgents => ExprBuilders.Select(b => b.Agent);

        /// <summary>
        /// Returns a language phrase (<see cref="Phrasebook"/> entry) for the specified index.
        /// </summary>
        /// <param name="p"><see cref="Phrasebook"/> index.</param>
        public string this[Phrase p] => Phrase[p];
        #endregion
    }
}
