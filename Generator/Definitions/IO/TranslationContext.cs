/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Mangh.Metrology.Language;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Threading;

namespace Mangh.Metrology
{
    /// <summary>
    /// Translation context<br/>
    /// (target language, standard filenames, error logging).
    /// </summary>
    public abstract class TranslationContext
    {
        #region Properties 

        ///////////////////////////////////////////////////////////////////////
        //
        //      Translation Settings
        //

        /// <summary>
        /// Target language context.
        /// </summary>
        public Language.Context Language { get; }

        /// <summary>
        /// Target namespace (to be used in the target language) for units, scales and related structures.
        /// </summary>
        public abstract string TargetNamespace { get; }

        /// <summary>
        /// Options to dump intermediate translation objects to files.
        /// </summary>
        public DumpOption DumpOptions { get; set; }

        /// <summary>
        /// Cancellation notification.
        /// </summary>
        public abstract CancellationToken CancellationToken { get; }

        /// <summary>
        /// Translation timestamp.
        /// </summary>
        public DateTime Timestamp { get; }

        ///////////////////////////////////////////////////////////////////////
        //
        //      Standard file extensions
        //

        /// <summary>Aliases (import) file extension.</summary>
        public virtual string IMPORT_EXT { get; } = "inc";

        /// <summary>Model file extension.</summary>
        public abstract string MODEL_EXT { get; }

        /// <summary>Source file extension.</summary>
        public virtual string SOURCE_EXT => Language[Phrase.SOURCE_EXT];

        /// <summary>Template file extension.</summary>
        public abstract string TEMPLATE_EXT { get; }

        /// <summary>Text file extension.</summary>
        public virtual string TEXT_EXT { get; } = "txt";

        ///////////////////////////////////////////////////////////////////////
        //
        //      Standard file names
        //

        /// <summary>Definitions filename (with extension).</summary>
        public virtual string DefinitionsFileName
            => FilePath.ChangeExtension(Language[Phrase.DEFINITIONS], TEXT_EXT);

        /// <summary>Aliases filename (with extension).</summary>
        public virtual string AliasesFileName
            => FilePath.ChangeExtension(Language[Phrase.ALIASES], IMPORT_EXT);

        /// <summary>Aliases template filename (with extension).</summary>
        public virtual string AliasesTemplateFileName
            => FilePath.ChangeExtension(Language[Phrase.ALIASES], TEMPLATE_EXT);

        /// <summary>Catalog filename (with extension).</summary>
        public virtual string CatalogFileName
            => FilePath.ChangeExtension(Language[Phrase.CATALOG], SOURCE_EXT);

        /// <summary>Catalog template filename (with extension).</summary>
        public virtual string CatalogTemplateFileName
            => FilePath.ChangeExtension(Language[Phrase.CATALOG], TEMPLATE_EXT);

        /// <summary>Report filename (with extension).</summary>
        /// <remarks>
        /// NOTE:<br/>
        /// Report filename should not collide with unit and scale filenames<br/>
        /// (saved in the same folder). Therefore, the function adds a long<br/>
        /// prefix to the default name.
        /// </remarks>
        public virtual string ReportFileName
            => FilePath.ChangeExtension("generator_" + Language[Phrase.REPORT].ToLower(), TEXT_EXT);

        /// <summary>Report template filename (with extension).</summary>
        public virtual string ReportTemplateFileName
            => FilePath.ChangeExtension(Language[Phrase.REPORT], TEMPLATE_EXT);

        /// <summary>Scale template filename (with extension).</summary>
        public virtual string ScaleTemplateFileName
            => FilePath.ChangeExtension(Language[Phrase.SCALE], TEMPLATE_EXT);

        /// <summary>Unit template filename (with extension).</summary>
        public virtual string UnitTemplateFileName
            => FilePath.ChangeExtension(Language[Phrase.UNIT], TEMPLATE_EXT);
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="TranslationContext"/> constructor.
        /// </summary>
        /// <param name="lc">Target language context.</param>
        public TranslationContext(Language.Context lc)
        {
            Timestamp = DateTime.Now;
            Language = lc;
            DumpOptions = DumpOption.None;
        }
        #endregion

        #region Methods

        ///////////////////////////////////////////////////////////////////////
        //
        //      Standard file names (continued)
        //

        /// <summary>
        /// Source code filename (with extension) for the <see cref="Measure"/>
        /// (<see cref="Unit"/> or <see cref="Scale"/>) structure.
        /// </summary>
        /// <param name="m"><see cref="Unit"/> or <see cref="Scale"/> object.</param>
        public virtual string SourceFileName(Measure m)
            => FilePath.ChangeExtension(m.TargetKeyword, SOURCE_EXT);

        ///////////////////////////////////////////////////////////////////////
        //
        //      Error logging
        //

        /// <summary>
        /// Reports an error.
        /// </summary>
        /// <param name="path">Path to the file affected by the error.</param>
        /// <param name="message">Error message.</param>
        /// <param name="ex">
        /// The exception that caused the reported error<br/>
        /// (<see langword="null"/> for internal errors detected on its own).
        /// </param>
        public abstract void Report(string path, string message, Exception? ex);

        /// <summary>
        /// Reports an error and its position.
        /// </summary>
        /// <param name="path">Path to the file affected by the error.</param>
        /// <param name="extent">Text span (as a text boundary positions in the input stream).</param>
        /// <param name="span">Text span (as a line/character positions of the text boundaries).</param>
        /// <param name="message">Error message.</param>
        /// <param name="ex">
        /// The exception that caused the reported error<br/>
        /// (<see langword="null"/> for internal errors detected on its own).
        /// </param>
        public abstract void Report(string path, TextSpan extent, LinePositionSpan span, string message, Exception? ex);
        #endregion
    }
}
