/*******************************************************************************

    Units of Measurement for C# and C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Threading;

namespace Mangh.Metrology
{
    public partial class SourceGenerator
    {
        internal class TranslationContext : XML.TranslationContext
        {
            #region Constants
            private const string METROLOGY_ID = "METROLOGY";            // Diagnostic identifier
            private const string CATEGORY = "SourceGenerator";          // Diagnostic category
            private const string DEFAULT_NAMESPACE = "Metrology.Units"; // Default namespace
            #endregion

            #region Fields
            private readonly GeneratorExecutionContext _gc;
            private readonly TemplateName _templateName;
            private readonly AdditionalText[] _templateText;
            private string? _targetDirectory = null;
            private readonly bool _reportMissingAdditionalFiles = false;
            #endregion

            #region Properties

            ///////////////////////////////////////////////////////////////////////
            //
            //      Translation Settings
            //

            public GeneratorExecutionContext GeneratorContext => _gc;

            /// <summary>
            /// Path to the template directory<br/>
            /// (only to save the alias file and the report file; the source code goes directly to the compiler).
            /// </summary>
            /// <exception cref="InvalidOperationException">
            /// The template directory is retrieved from the definition file path; 
            /// it will be left unset if the definition file has not been specified in the project file.
            /// </exception>
            public string TemplateDirectory => (_targetDirectory is not null) ? _targetDirectory :
                throw new InvalidOperationException($"{nameof(TranslationContext)}.{nameof(TemplateDirectory)} property not set.");

            /// <summary>
            /// Target namespace for the generated source code structures.
            /// </summary>
            public override string TargetNamespace { get; }

            /// <summary>
            /// Cancellation notification.
            /// </summary>
            public override CancellationToken CancellationToken => _gc.CancellationToken;

            /// <summary>
            /// Returns the <see cref="AdditionalText"/> object for the selected template.
            /// </summary>
            /// <param name="template">Template index.</param>
            public AdditionalText this[Template template]
            {
                get { return _templateText[(int)template]; }
                private set { _templateText[(int)template] = value; }
            }

            ///////////////////////////////////////////////////////////////////////
            //
            //      File paths
            //

            /// <summary>
            /// Path to the (output) aliases file.
            /// </summary>
            public string AliasesFilePath => FilePath.Combine(TemplateDirectory, AliasesFileName);

            /// <summary>
            /// Path to the (output) report file.
            /// </summary>
            public string ReportFilePath => FilePath.Combine(TemplateDirectory, ReportFileName);

            #endregion

            #region Constructors
            /// <summary>
            /// <see cref="TranslationContext"/> constructor.
            /// </summary>
            /// <param name="gc">Generator execution context.</param>
            /// <exception cref="InvalidOperationException">Thrown when the C# language context is not available.</exception>
            public TranslationContext(GeneratorExecutionContext gc)
                : base(Definitions.Contexts.First(c => c.Id == Metrology.Language.ID.CS))
            {
                _gc = gc;

                // The project file has to provide <RootNamespace> property
                // specyfying the target namespace for the generated source code:
                _gc.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                    "build_property.RootNamespace", out string? rootNamespace);

                TargetNamespace = rootNamespace ?? DEFAULT_NAMESPACE;

                // Is it required to report AdditionalText files missing in the project file?
                if (_gc.AnalyzerConfigOptions.GlobalOptions.TryGetValue(
                    "build_property.ReportMissingAdditionalFiles", out string? reportMissingAdditionalFilesSwitch))
                {
                    _reportMissingAdditionalFiles = reportMissingAdditionalFilesSwitch.Equals("true", StringComparison.OrdinalIgnoreCase);
                }

                // Do not dump intermediate objects (models, source text duplicates):
                DumpOptions = DumpOption.None;

                _templateName = new TemplateName(this);
                _templateText = new AdditionalText[TemplateName.SIZE];

                // Arrange the templates (in AdditionalText objects)
                foreach (AdditionalText text in _gc.AdditionalFiles)
                {
                    if (_gc.CancellationToken.IsCancellationRequested)
                        break;

                    if (AssignAdditionalText(text, Template.ALIASES)) continue;
                    if (AssignAdditionalText(text, Template.CATALOG)) continue;
                    if (AssignAdditionalText(text, Template.DEFINITIONS)) continue;
                    if (AssignAdditionalText(text, Template.REPORT)) continue;
                    if (AssignAdditionalText(text, Template.SCALE)) continue;
                    if (AssignAdditionalText(text, Template.UNIT)) continue;
                }

                bool AssignAdditionalText(AdditionalText text, Template template)
                {
                    bool match = text.Path.EndsWith(_templateName[template], StringComparison.OrdinalIgnoreCase);
                    if (match) this[template] = text;
                    return match;
                }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Checks whether the translation context is ready to run the generator<br/>
            /// (i.e. whether all required templates have been provided in the project file).
            /// </summary>
            /// <returns><see langword="true"/> when the context is ready, otherwise <see langword="false"/>.</returns>
            /// <remarks>
            /// NOTE:<br/>The report template is the only one that may not be specified in the project file<br/>
            /// (in this case, the final report will simply not be generated).
            /// </remarks>
            public bool IsReady()
            {
                // Check the template list for completeness (only the generator report may be missing):
                bool ready = true;
                for (int t = 0; t < _templateText.Length; ++t)
                {
                    if ((_templateText[t] is null) && (t != (int)Template.REPORT))
                    {
                        ReportMissingAdditionalFile((Template)t);
                        ready = false;
                    }
                }

                if (ready)
                {
                    _targetDirectory = FilePath.GetDirectoryName(this[Template.DEFINITIONS].Path);
                }

                return ready;
            }

            ///////////////////////////////////////////////////////////////////////
            //
            //      Error logging
            //

#pragma warning disable RS2008  // Enable analyzer release tracking for the analyzer project containing rule 'METROLOGY'
                                // mangh: should I need such a bookkeeping?

            private readonly DiagnosticDescriptor _missingAdditionalFile = new(
                id: METROLOGY_ID,
                title: "Missing additional file",
                messageFormat: "\"{0}\": required file has been not specified in the project file",
                category: CATEGORY,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true
            );

            private void ReportMissingAdditionalFile(Template t)
            {
                if (_reportMissingAdditionalFiles)
                {
                    _gc.ReportDiagnostic(
                        Diagnostic.Create(_missingAdditionalFile, Location.None, _templateName[t])
                    );
                }
            }

            public override void Report(string path, string message, Exception? ex)
            {
                _gc.ReportDiagnostic(
                    Diagnostic.Create(
                        id: METROLOGY_ID,
                        category: CATEGORY,
                        message: $"{path}: {message}",
                        severity: DiagnosticSeverity.Error,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        warningLevel: 0,
                        title: null,
                        description: ex?.ToString(),
                        helpLink: null,
                        location: Location.None
                    )
                );
            }

            public override void Report(string path, TextSpan extent, LinePositionSpan span, string message, Exception? ex)
            {
                _gc.ReportDiagnostic(
                    Diagnostic.Create(
                        id: METROLOGY_ID,
                        category: CATEGORY,
                        message: $"{path} {span}: {message}",
                        severity: DiagnosticSeverity.Error,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        warningLevel: 0,
                        title: null,
                        description: ex?.ToString(),
                        helpLink: null,
                        location: Location.Create(path, extent, span)
                    )
                );
            }
            #endregion
        }
    }
}
