using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mangh.Metrology
{
    public partial class Generator
    {
        /// <summary>
        /// Provider for unit and scale definitions.
        /// </summary>
        internal class DefinitionProvider : Provider
        {
            /// <summary>
            /// Units loaded from a definition stream.
            /// </summary>
            public List<UnitType> Units { get; private set; }

            /// <summary>
            /// Scales loaded from a definition stream.
            /// </summary>
            public List<ScaleType> Scales { get; private set; }

            public DefinitionProvider(AdditionalText definitions) :
                base(category: "DEFINITIONS", template: definitions)
            {
                Units = new();
                Scales = new();
            }

            /// <summary>
            /// Load unit/scale definitions from the <see cref="Provider.Template"/> into <see cref="Units"/> and <see cref="Scales"/> lists.
            /// </summary>
            /// <param name="context">source generator context (for reporting <see cref="Diagnostic"/> issues)</param>
            /// <returns>"true" on success, "false" in case of a problem detected when loading.</returns>
            internal bool Load(GeneratorExecutionContext context)
            {
                int errorCount = 0;

                MemoryStream? definitionStream = LoadTemplate(context);
                if (definitionStream is not null)
                {
                    definitionStream.Position = 0;

                    using (StreamReader reader = new(definitionStream))
                    {
                        try
                        {
                            Lexer lexer = new(reader);
                            Parser parser = new(lexer, ReportParseError, Units, Scales);
                            parser.Parse();
                            return errorCount == 0;
                        }
                        catch (IOException ex)
                        {
                            ReportException(context, FormatMessage(ex.Message));
                        }
                        catch (ObjectDisposedException ex)
                        {
                            ReportException(context, FormatMessage(ex.Message));
                        }
                        catch (InvalidOperationException ex)
                        {
                            ReportException(context, FormatMessage(ex.Message));
                        }
                    }
                }

                return false;

                string FormatMessage(string message)
                    => string.Format("{0} : definitions could not be read : {1}", Template.Path, message);

                void ReportParseError(TextSpan extent, LinePositionSpan span, string message)
                {
                    errorCount++;
                    ReportTemplateError(context, extent, span, message);
                }
            }
        }
    }
}