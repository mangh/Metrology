/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Mangh.Metrology.XML;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Threading;

//#if DEBUG
//using System.Diagnostics;
//#endif 

namespace Mangh.Metrology
{
    /// <summary>
    /// Metrology Source Generator (for the C# language).
    /// </summary>
    [Generator]
    public partial class SourceGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// Initializes the generator.
        /// </summary>
        /// <param name="context">Source generator context.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif
            TranslationContext tc = new(context);

            IncrementalValuesProvider<string> defsPath = context.AdditionalTextsProvider
                .Where((AdditionalText text) => text.Path.EndsWith(tc[Template.DEFINITIONS], StringComparison.OrdinalIgnoreCase))
                .Select((AdditionalText text, CancellationToken ct) =>
                {
                    ct.ThrowIfCancellationRequested();
                    return text.Path;
                });

            context.RegisterSourceOutput(defsPath, (SourceProductionContext spc, string path) =>
            {
                tc.SourcePool = spc;
                tc.TemplateDirectory = FilePath.GetDirectoryName(path);

                Definitions definitions = new(path, tc);
                bool done = definitions.Load();

                if (done)
                {
                    using UnitModel model = new(tc.Path(Template.UNIT), tc, late: false);
                    done = model.ToSourceText(definitions, targetDirectory: string.Empty, spc.AddSource);
                }

                if (done)
                {
                    using ScaleModel model = new(tc.Path(Template.SCALE), tc, late: false);
                    done = model.ToSourceText(definitions, targetDirectory: string.Empty, spc.AddSource);
                }

                if (done)
                {
                    using CatalogModel model = new(tc.Path(Template.CATALOG), tc);
                    done = model.ToSourceText(definitions, hintPath: tc.CatalogFileName, spc.AddSource);
                }

                if (done)
                {
                    using AliasingModel model = new(tc.Path(Template.ALIASES), tc, global: true);
                    done = model.ToFile(definitions, tc.AliasesFilePath);
                }

                if (done)
                {
                    using ReportModel model = new(tc.Path(Template.REPORT), tc);
                    done = model.ToFile(definitions, tc.ReportFilePath);
                }
            });
        }
    }
}
