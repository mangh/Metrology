using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Mangh.Metrology
{
    public partial class Generator
    {
        /// <summary>
        /// Base provider (for a source code).
        /// </summary>
        internal class Provider
        {
            #region Properties
            /// <summary>
            /// Provider category (badge)
            /// </summary>
            public string Category { get; }

            /// <summary>
            /// Unit definitions or an XSLT template (as appropriate for the specific provider).
            /// </summary>
            public AdditionalText Template { get; }
            #endregion

            #region Constructor(s)
            protected Provider(string category, AdditionalText template)
            {
                Category = category;
                Template = template;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Loads <see cref="Template"/> file into a <see cref="MemoryStream"/>.
            /// </summary>
            /// <param name="context">source generator context (with a <see cref="System.Threading.CancellationToken"/> for the generation task)</param>
            /// <returns><see cref="MemoryStream"/> loaded with <see cref="Template"/> data, or <c>null</c> if there were errors when reading the file.</returns>
            protected MemoryStream? LoadTemplate(GeneratorExecutionContext context)
            {
                SourceText? contents = Template.GetText(context.CancellationToken);
                if (contents is null)
                    return null;    // It can be null only in case of errors found (and reported earlier?)

                MemoryStream stream = new();

                // No exception is expected to be thrown while creating StreamWriter with the parameters used below:
                using (StreamWriter writer = new(stream, Encoding.UTF8, bufferSize: 2048, leaveOpen: true))
                {
                    contents.Write(writer);
                }

                stream.Position = 0;

                return stream;
            }

            /// <summary>
            /// Loads an XSL stylesheet from the <see cref="Template"/>.
            /// </summary>
            /// <param name="context">source generator context (for reporting <see cref="Diagnostic"/> issues)</param>
            /// <returns>XSLT processor as <see cref="XslCompiledTransform"/>, or <c>null</c> if there were errors when reading the stylesheet.</returns>
            protected XslCompiledTransform? LoadXsltTemplate(GeneratorExecutionContext context)
            {
                MemoryStream? template = LoadTemplate(context);
                if (template is not null)
                {
                    XslCompiledTransform transform = new();

                    // No exception is expected to be thrown (in the context of AdditionalText
                    // stream) while creating XmlReader with the parameters used below:
                    using (XmlReader rdr = XmlReader.Create(template))
                    {
                        try
                        {
                            // Load() can throw essential exceptions though:
                            transform.Load(rdr);
                            return transform;
                        }
                        catch (System.Security.SecurityException ex)
                        {
                            ReportException(context, string.Format("\"{0}\": template could not be read :: {1}", Template.Path, ex.Message));
                        }
                        catch (XsltException ex)
                        {
                            LinePosition spot = new(ToZeroBased(ex.LineNumber), ToZeroBased(ex.LinePosition));
                            ReportTemplateError(context, /*unknown*/ extent: new(), span: new(spot, spot), ex.Message);
                        }
                    }
                }
                return null;

                static int ToZeroBased(int n) => n > 0 ? n - 1 : 0;
            }

            /// <summary>
            /// Make path to another file (of <paramref name="filename"/>) in the same folder (as <paramref name="path"/>).
            /// </summary>
            /// <param name="context">source generator context (for reporting <see cref="Diagnostic"/> issues)</param>
            /// <param name="path">path to some other file</param>
            /// <param name="filename">name for the another file</param>
            /// <returns>Path to another file, or <c>null</c> if there were errors found in the <paramref name="path"/> or <paramref name="filename"/>.</returns>
            protected string? MakePath(GeneratorExecutionContext context, string path, string filename)
            {
                try
                {
                    return Path.Combine(Path.GetDirectoryName(path), filename);
                }
                catch (ArgumentNullException ex)
                {
                    ReportException(context, FormatMessage(ex.Message));
                }
                catch (ArgumentException ex)
                {
                    ReportException(context, FormatMessage(ex.Message));
                }
                catch (PathTooLongException ex)
                {
                    ReportException(context, FormatMessage(ex.Message));
                }
                return null;

                string FormatMessage(string message) => string.Format("{0}({1}, {2}): {3}", nameof(MakePath), path, filename, message);
            }

            /// <summary>
            /// Saves text to a file.
            /// </summary>
            /// <param name="context">source generator context (for reporting <see cref="Diagnostic"/> issues)</param>
            /// <param name="path">path to the file</param>
            /// <param name="contents">text to be saved</param>
            /// <returns><c>true</c> on success, <c>false</c> if there were errors when saving.</returns>
            protected bool SaveToFile(GeneratorExecutionContext context, string path, string contents)
            {
                try
                {
                    using (StreamWriter writer = new(path))
                    {
                        writer.Write(contents);
                    }
                    return true;
                }
                catch (System.ArgumentException ex)
                {
                    ReportException(context, FormatMessage(ex.Message));
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    ReportException(context, FormatMessage(ex.Message));
                }
                catch (System.NotSupportedException ex)
                {
                    ReportException(context, FormatMessage(ex.Message));
                }
                catch (System.IO.IOException ex)
                {
                    ReportException(context, FormatMessage(ex.Message));
                }
                return false;

                string FormatMessage(string message) => string.Format("\"{0}\": file could not be saved :: {1}", path, message);
            }

            protected void ReportException(GeneratorExecutionContext context, string message)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        id: DiagnosticId,
                        category: Category,
                        message: message,
                        severity: DiagnosticSeverity.Error,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        warningLevel: 0
                    )
                );
            }

            protected void ReportTemplateError(GeneratorExecutionContext context, TextSpan extent, LinePositionSpan span, string message)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        id: DiagnosticId,
                        category: Category,
                        message: string.Format("{0}({1}, {2}): {3}", Template.Path, span.Start.Line + 1, span.Start.Character + 1, message),
                        severity: DiagnosticSeverity.Error,
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true,
                        warningLevel: 0,
                        title: null,
                        description: null,
                        helpLink: null,
                        location: Location.Create(Template.Path, extent, span)
                    )
                );
            }

            #endregion
        }
    }
}