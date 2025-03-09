/*******************************************************************************

    Units of Measurement for C# and C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Mangh.Metrology
{
    public partial class SourceGenerator
    {
        internal class TranslationContext : XML.TranslationContext
        {
            #region Constants
            private const string METROLOGY_ID = "Metrology";            // Diagnostic identifier
            private const string CATEGORY = "SourceGenerator";          // Diagnostic category
            private const string DEFAULT_NAMESPACE = "Metrology.Units"; // Default namespace
            #endregion

            #region Fields
            private string? _targetNamespace;
            private readonly TemplateNames _templateNames;
            private string? _templateDirectory = null;
            private SourceProductionContext? _spc = null;
            #endregion

            #region Properties

            /// <summary>
            /// Target namespace for the generated source code structures.
            /// </summary>
            public override string TargetNamespace => _targetNamespace ?? DEFAULT_NAMESPACE;

            ///////////////////////////////////////////////////////////////////////
            //
            //      File paths
            //

            /// <summary>
            /// Template directory.
            /// </summary>
            public string TemplateDirectory
            {
                set { _templateDirectory = value; }
                get => (_templateDirectory is not null) ? _templateDirectory :
                    throw new InvalidOperationException($"{nameof(TranslationContext)}.{nameof(TemplateDirectory)} property not set.");
            }

            /// <summary>
            /// Returns the name of the template file (without the path).
            /// </summary>
            /// <param name="template">Template index.</param>
            public string this[Template template] => _templateNames[template];

            /// <summary>
            /// Returns the path to the template file.
            /// </summary>
            /// <param name="template">Template index.</param>
            public string Path(Template template) => FilePath.Combine(TemplateDirectory, _templateNames[template]);

            /// <summary>
            /// Returns the path to the (output) Aliases file.
            /// </summary>
            public string AliasesFilePath => FilePath.Combine(TemplateDirectory, AliasesFileName);

            /// <summary>
            /// Returns the path to the (output) report file.
            /// </summary>
            public string ReportFilePath => FilePath.Combine(TemplateDirectory, ReportFileName);

            ///////////////////////////////////////////////////////////////////////
            //
            //      Source generator context settings
            //

            /// <summary>
            /// Source production context.
            /// </summary>
            public SourceProductionContext SourcePool
            {
                set => _spc = value;
                get => (_spc is SourceProductionContext spc) ? spc :
                    throw new InvalidOperationException($"{nameof(TranslationContext)}.{nameof(SourcePool)} property not set.");
            }

            /// <summary>
            /// Cancellation notification.
            /// </summary>
            public override CancellationToken CancellationToken => SourcePool.CancellationToken;

            #endregion

            #region Constructors
            /// <summary>
            /// <see cref="TranslationContext"/> constructor.
            /// </summary>
            /// <param name="context"></param>
            /// <exception cref="InvalidOperationException">Thrown when the C# language context is not available.</exception>
            public TranslationContext(IncrementalGeneratorInitializationContext context)
                : base(Definitions.Contexts.First(c => c.Id == Metrology.Language.ID.CS))
            {
                // Template file names:
                _templateNames = new TemplateNames(this);

                // Do not dump intermediate results (models, source texts) to files:
                DumpOptions = DumpOption.None;

                // Get target namespace
                IncrementalValueProvider<string?> rootNamespace = context.AnalyzerConfigOptionsProvider.Select(
                    (AnalyzerConfigOptionsProvider provider, CancellationToken _) =>
                        provider.GlobalOptions.TryGetValue("build_property.RootNamespace", out string? ns) ? ns : null);

                context.RegisterSourceOutput(rootNamespace, (SourceProductionContext spc, string? ns) =>
                {
                    _targetNamespace = ns;
                });
            }
            #endregion

            #region Methods

            ///////////////////////////////////////////////////////////////////////
            //
            //      Reporting errors
            //

            private static string ComposeErrorMessage(string intro, Exception? ex)
            {
                if (ex is null)
                    return intro;

                StringBuilder sb = new(intro);
                while (ex is not null)
                {
                    sb.Append(" ").Append(ex.GetType().Name).Append(": ").Append(ex.Message);
                    ex = ex.InnerException;
                }
                return sb.ToString();
            }

            public override void Report(string path, string message, Exception? ex)
            {
                SourcePool.ReportDiagnostic(
                    Diagnostic.Create(
                        id: METROLOGY_ID,
                        category: CATEGORY,
                        message: $"{path}: {ComposeErrorMessage(message, ex)}",
                        severity: DiagnosticSeverity.Error,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        warningLevel: 0
                    )
                );
            }

            public override void Report(string path, TextSpan extent, LinePositionSpan span, string message, Exception? ex)
            {
                SourcePool.ReportDiagnostic(
                    Diagnostic.Create(
                        id: METROLOGY_ID,
                        category: CATEGORY,
                        message: $"{path} ({LinePositionSpanString(span)}): {ComposeErrorMessage(message, ex)}",
                        severity: DiagnosticSeverity.Error,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        warningLevel: 0,
                        location: Location.Create(path, extent, ToInternalLinePositionSpan(span))
                    )
                );

                // In the generator, line and column numbers start at 1,
                // while for Microsoft they start at 0:
                static LinePosition ToInternalLinePosition(LinePosition p)
                    => (p.Line > 0) && (p.Character > 0) ? new(p.Line - 1, p.Character - 1) : p;

                static LinePositionSpan ToInternalLinePositionSpan(LinePositionSpan s)
                    => new(ToInternalLinePosition(s.Start), ToInternalLinePosition(s.End));

                // Formatting LinePositionSpan:
                static string LinePositionSpanString(LinePositionSpan s) =>
                    s.Start.Equals(s.End) ? $"{s.Start}" :
                        !s.Start.Line.Equals(s.End.Line) ? $"({s.Start})-({s.End})" :
                            $"{s.Start.Line},{s.Start.Character}-{s.End.Character}";
            }
            #endregion
        }
    }
}
