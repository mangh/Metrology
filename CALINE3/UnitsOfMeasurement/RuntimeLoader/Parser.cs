/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;

namespace CALINE3.Metrology
{
    public partial class RuntimeLoader
    {
        private class Parser
        {
            #region Fields
            private readonly RuntimeLoader m_loader;
            private readonly IDefinitions m_definitions;
            #endregion

            #region Properties
            #endregion

            #region Constructor(s)
            public Parser(RuntimeLoader loader, IDefinitions definitions)
            {
                m_loader = loader;
                m_definitions = definitions;
            }
            #endregion

            #region Methods
            public bool Parse(string? definitionsPath, TextReader input)
            {
                bool errorsFound = false;
                try
                {
                    Mangh.Metrology.Lexer lexer = new(input);
                    Mangh.Metrology.Parser parser = new(lexer, ReportParseError, m_definitions.Units, m_definitions.Scales);
                    parser.Parse();
                    return !errorsFound;
                }
                catch (IOException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (ObjectDisposedException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (InvalidOperationException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                return false;

                string FormatErrorMessage(string message) =>
                    $"{(definitionsPath is null ? "D" : '"' + definitionsPath + '"' + ": d")}efinitions could not be read : {message}.";

                void ReportParseError(TextSpan extent, LinePositionSpan span, string message)
                {
                    errorsFound = true;

                    LinePositionSpan oneBasedSpan = new(
                            new(span.Start.Line + 1, span.Start.Character + 1), 
                            new(span.End.Line + 1, span.End.Character + 1));

                    m_loader.ReportError($"{definitionsPath ?? string.Empty}({oneBasedSpan}): {message}");
                }
            }
            #endregion
        }
    }
}
