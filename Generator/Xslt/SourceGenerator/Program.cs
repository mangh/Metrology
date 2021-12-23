using Microsoft.CodeAnalysis;
using System;

//#if DEBUG
//using System.Diagnostics;
//#endif 

namespace Mangh.Metrology
{
    /// <summary>
    /// Metrology Source Generator
    /// </summary>
    [Generator]
    public partial class Generator : ISourceGenerator
    {
        #region Constants
        /// <summary>
        /// An identifier for a Metrology diagnostic.
        /// </summary>
        public const string DiagnosticId = "UoM";
        #endregion

        #region Properties
        private DefinitionProvider? definitions = null;
        private UnitProvider? unitProvider = null;
        private ScaleProvider? scaleProvider = null;
        private CatalogProvider? catalogProvider = null;
        private AliasProvider? aliasProvider = null;
        private ReportProvider? reportProvider = null;

        // root namespace from the project file
        private string? rootNamespace;
        private string TargetNamespace => rootNamespace ?? "Metrology.UnitsOfMeasurement";
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the generator.
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one

            //#if DEBUG
            //            if (!Debugger.IsAttached)
            //            {
            //                Debugger.Launch();
            //            }
            //#endif
        }

        /// <summary>
        /// Generates code for unit and scale structs as well as their aliases and a summary report.
        /// </summary>
        /// <param name="context">context for additional files (templates) and generated code.</param>
        public void Execute(GeneratorExecutionContext context)
        {
            // Get target namespace (for units, scales and Catalog):
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out rootNamespace);

            // Bind additional files to the appropriate provider (generator):
            BindAdditionalFiles(context);

            // Only perform source generation when all required providers are ready:
            if ((definitions is not null) &&
                (unitProvider is not null) &&
                (scaleProvider is not null) &&
                (catalogProvider is not null) &&
                (aliasProvider is not null) &&
                definitions.Load(context))
            {
                int unitFamilyCount = unitProvider.AddSource(context, definitions.Units);

                int scaleFamilyCount = scaleProvider.AddSource(context, definitions.Scales, unitProvider.Master.Family);

                catalogProvider.AddSource(context, unitFamilyCount, definitions.Units, scaleFamilyCount, definitions.Scales);

                aliasProvider.MakeAliases(context, definitions.Units, definitions.Scales);

                if (reportProvider is not null)
                {
                    reportProvider.MakeReport(context, unitFamilyCount, definitions.Units, scaleFamilyCount, definitions.Scales);
                }
            }
        }

        private void BindAdditionalFiles(GeneratorExecutionContext context)
        {
            foreach (AdditionalText template in context.AdditionalFiles)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    break;

                if (template.Path.EndsWith("definitions.txt", StringComparison.OrdinalIgnoreCase))
                {
                    definitions = new(template);
                }
                else if (template.Path.EndsWith("unit.xslt", StringComparison.OrdinalIgnoreCase))
                {
                    unitProvider = new(TargetNamespace, late: false, template);
                }
                else if (template.Path.EndsWith("scale.xslt", StringComparison.OrdinalIgnoreCase))
                {
                    scaleProvider = new(TargetNamespace, late: false, template);
                }
                else if (template.Path.EndsWith("catalog.xslt", StringComparison.OrdinalIgnoreCase))
                {
                    catalogProvider = new(TargetNamespace, template);
                }
                else if (template.Path.EndsWith("aliases.xslt", StringComparison.OrdinalIgnoreCase))
                {
                    aliasProvider = new(TargetNamespace, template);
                }
                else if (template.Path.EndsWith("report.xslt", StringComparison.OrdinalIgnoreCase))
                {
                    reportProvider = new(TargetNamespace, template);
                }
            }
        }
        #endregion
    }
}
