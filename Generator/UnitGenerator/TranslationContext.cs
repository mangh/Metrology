/*******************************************************************************

    Units of Measurement for C# and C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis.Text;
using System;
using System.Threading;

using static System.Console;

namespace Mangh.Metrology.UnitGenerator
{
    internal class TranslationContext : XML.TranslationContext
    {
        #region Properties

        /////////////////////////////////////////////////////////////////////
        //
        //      Translation settings
        //

        /// <summary>
        /// The path to the template directory.
        /// </summary>
        public string TemplateDirectory { get; }

        /// <summary>
        /// The path to the destination directory
        /// (for the generated source code files).
        /// </summary>
        public string TargetDirectory { get; }

        /// <summary>
        /// The target namespace
        /// (for the generated source code structures).
        /// </summary>
        public override string TargetNamespace { get; }

        /// <summary>
        /// Cancellation notification.
        /// </summary>
        public override CancellationToken CancellationToken => CancellationToken.None;

        /////////////////////////////////////////////////////////////////////
        //
        //      File paths
        //

        /// <summary><see cref="Definitions"/> (input) file path.</summary>
        public string DefinitionsFilePath => FilePath.Combine(TemplateDirectory, DefinitionsFileName);

        /// <summary>Aliases template (input) file path.</summary>
        public string AliasesTemplateFilePath => FilePath.Combine(TemplateDirectory, AliasesTemplateFileName);

        /// <summary>Aliases (output) file path.</summary>
        public string AliasesFilePath => FilePath.Combine(TargetDirectory, AliasesFileName);

        /// <summary>Catalog template (input) file path.</summary>
        public string CatalogTemplateFilePath => FilePath.Combine(TemplateDirectory, CatalogTemplateFileName);

        /// <summary>Catalog (output) file path.</summary>
        public string CatalogFilePath => FilePath.Combine(TargetDirectory, CatalogFileName);

        /// <summary>Report template (input) file path.</summary>
        public string ReportTemplateFilePath => FilePath.Combine(TemplateDirectory, ReportTemplateFileName);

        /// <summary>Report (output) file path.</summary>
        public string ReportFilePath => FilePath.Combine(TargetDirectory, ReportFileName);

        /// <summary>Scale template (input) file path.</summary>
        public string ScaleTemplateFilePath => FilePath.Combine(TemplateDirectory, ScaleTemplateFileName);

        /// <summary>Unit template (input) file path.</summary>
        public string UnitTemplateFilePath => FilePath.Combine(TemplateDirectory, UnitTemplateFileName);

        #endregion

        #region Constructors
        public TranslationContext(Language.Context targetContext,
                                           string targetNamespace,
                                           string templateDirectory,
                                           string targetDirectory)
            : base(targetContext)
        {
            TargetNamespace = targetNamespace;
            TemplateDirectory = templateDirectory;
            TargetDirectory = targetDirectory;
        }
        #endregion

        #region Methods

        ///////////////////////////////////////////////////////////////////////
        //
        //      Error logging
        //

        public override void Report(string path, string message, Exception? ex)
        {
            WriteLine($"{path}: {message}");
            if (ex is not null)
            {
                WriteLine();
                WriteLine(ex.ToString());
            }
        }

        public override void Report(string path, TextSpan extent, LinePositionSpan span, string message, Exception? ex)
        {
            WriteLine($"{path}{LinePositionString(span)}: {message}");
            if (ex is not null)
            {
                WriteLine();
                WriteLine(ex.ToString());
            }

            static string LinePositionString(LinePositionSpan lp) =>
                (!lp.Start.Equals(lp.End)) ? $", {lp}" :
                (!lp.Start.Equals(LinePosition.Zero)) ? $", ({lp.Start})" :
                // LinePosition.Zero actually means that
                // there is no information about the error position:
                string.Empty;
        }
        #endregion
    }
}
