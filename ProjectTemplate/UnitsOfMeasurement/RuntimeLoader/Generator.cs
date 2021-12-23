/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/
using Mangh.Metrology;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Xsl;

namespace Metrological.Namespace
{
    public partial class RuntimeLoader
    {
        public class Generator
        {
            #region Constants
            private const string TEMPLATE_FOLDER = "Templates";
            private const string UNIT_TEMPLATE = "unit.xslt";
            private const string SCALE_TEMPLATE = "scale.xslt";
            #endregion

            #region Fields
            private readonly RuntimeLoader m_loader;
            private readonly IDefinitions m_definitions;
            #endregion

            #region Properties
            #endregion

            #region Constructor(s)
            public Generator(RuntimeLoader loader, IDefinitions definitions)
            {
                m_loader = loader;
                m_definitions = definitions;
            }
            #endregion

            #region Methods
            public string? Transform(int unitStartIndex, int scaleStartIndex)
            {
                string targetNamespace = GetType().Namespace!;
                string? templateFolder = m_loader.GetSubfolder(TEMPLATE_FOLDER);
                if (templateFolder is not null)
                {
                    UnitTranslator utor = new(targetNamespace, late: true);
                    XslCompiledTransform? unitTemplate = CompileXsltTemplate(templateFolder, UNIT_TEMPLATE);
                    if (unitTemplate is not null)
                    {
                        ScaleTranslator stor = new(targetNamespace, late: true);
                        XslCompiledTransform? scaleTemplate = CompileXsltTemplate(templateFolder, SCALE_TEMPLATE);
                        if (scaleTemplate is not null)
                        {
                            int familyStartId = m_definitions.MaxFamilyFound + 1;    // start id for new families

                            StringBuilder csb = new(32 * 1024);
                            CancellationToken noCancellation = CancellationToken.None;

                            // units
                            foreach ((_, string contents) in utor.Translate(noCancellation, unitTemplate, m_definitions.Units, initialFamily: familyStartId, startIndex: unitStartIndex))
                            {
                                csb.Append(contents);
                            }
                            // scales
                            foreach ((_, string contents) in stor.Translate(noCancellation, scaleTemplate, m_definitions.Scales, initialFamily: utor.Family, scaleStartIndex))
                            {
                                csb.Append(contents);
                            }
                            return csb.ToString();
                        }
                    }
                }
                return null;
            }
            #endregion

            #region Xslt Methods
            public XslCompiledTransform? CompileXsltTemplate(string templateFolder, string templateFilename)
            {
                string? templatePath = m_loader.PathCombine(templateFolder, templateFilename);
                if (templatePath is not null)
                {
                    StreamReader? templateFile = m_loader.FileOpenText(templatePath);
                    if (templateFile is not null)
                    {
                        try
                        {
                            using (XmlReader rdr = XmlReader.Create(templateFile))
                            {
                                XslCompiledTransform xslt = new();
                                xslt.Load(rdr);
                                return xslt;
                            }
                        }
                        catch (ArgumentException ex)
                        {
                            m_loader.ReportError(FormatErrorMessage(ex.Message));
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            m_loader.ReportError(FormatErrorMessage(ex.Message));
                        }
                        catch (NotSupportedException ex)
                        {
                            m_loader.ReportError(FormatErrorMessage(ex.Message));
                        }
                        catch (IOException ex)
                        {
                            m_loader.ReportError(FormatErrorMessage(ex.Message));
                        }
                        catch (XsltException ex)
                        {
                            m_loader.ReportError(FormatXsltErrorMessage(ex.LineNumber, ex.LinePosition, ex.Message));
                        }
                    }
                }
                return null;

                string FormatErrorMessage(string message) =>
                    $"\"{templatePath}\": XSLT template could not be read :: {message}.";

                string FormatXsltErrorMessage(int lineNumber, int linePosition, string message) =>
                    $"\"{templatePath}({lineNumber}, {linePosition})\": XSLT template could not be read :: {message}.";

            }
            #endregion
        }
    }
}
