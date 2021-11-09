/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;

namespace Demo.UnitsOfMeasurement
{
    public partial class RuntimeLoader
    {
        private class Parser
        {
            #region Fields
            private readonly RuntimeLoader m_loader;
            private readonly IDefinitions m_catalog;
            #endregion

            #region Properties
            #endregion

            #region Constructor(s)
            public Parser(RuntimeLoader loader, IDefinitions catalog)
            {
                m_loader = loader;
                m_catalog = catalog;
            }
            #endregion

            #region Methods
            public bool Parse(string definitionHint, TextReader input)
            {
                bool errorsFound = false;
                try
                {
                    Mangh.Metrology.Lexer lexer = new(input);
                    Mangh.Metrology.Parser parser = new(lexer, ReportParseError, m_catalog.Units, m_catalog.Scales);
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

                void ReportParseError(TextSpan extent, LinePositionSpan span, string message)
                {
                    errorsFound = true;
                    m_loader.ReportError(
                        string.Format("\"{0}({1})\": {2}", definitionHint, span, message)
                    );
                }

                string FormatErrorMessage(string message) =>
                    string.Format("{0}{1}{2}(\"{3}\", ...) : definitions could not be read : {4}", 
                        nameof(RuntimeLoader),
                        nameof(Parser),
                        nameof(Parse),
                        definitionHint, 
                        message
                    );
            }
            #endregion
        }
    }
}
