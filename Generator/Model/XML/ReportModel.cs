/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Threading;
using System.Xml;

namespace Mangh.Metrology.XML
{
    /// <summary>
    /// Report XML Model.
    /// </summary>
    public class ReportModel : Model
    {
        #region Constructor(s)
        /// <summary>
        /// <see cref="ReportModel"/> constructor.
        /// </summary>
        /// <param name="templatePath">Path to the XSLT template file.</param>
        /// <param name="tc">XML model translation context.</param>
        public ReportModel(string templatePath, TranslationContext tc)
            : base(templatePath, tc, capacity: 8 * 1024/*, useAsync: false*/)
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Builds XML model of the report on <see cref="Unit"/>s and <see cref="Scale"/>s that have been generated.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> definitions.</param>
        /// <returns><see langword="true"/> on a successful build (XML model ready in the internal buffer), otherwise <see langword="false"/>.</returns>
        public bool Build(Definitions definitions)
        {
            try
            {
                _model.Position = 0;
                _model.SetLength(0);

                using XmlWriter xml = XmlWriter.Create(_model);

                xml.WriteStartElement("report");
                {
                    xml.WriteAttributeString("ns", _tc.TargetNamespace);

                    xml.WriteStartAttribute("uct");
                    xml.WriteValue(definitions.Units.Count);
                    xml.WriteEndAttribute();

                    xml.WriteStartAttribute("ufct");
                    xml.WriteValue(definitions.FamilyCount(definitions.Units));
                    xml.WriteEndAttribute();

                    xml.WriteStartAttribute("sct");
                    xml.WriteValue(definitions.Scales.Count);
                    xml.WriteEndAttribute();

                    xml.WriteStartAttribute("sfct");
                    xml.WriteValue(definitions.FamilyCount(definitions.Scales));
                    xml.WriteEndAttribute();

                    xml.WriteStartAttribute("tm");
                    xml.WriteValue(_tc.Timestamp);
                    xml.WriteEndAttribute();
                }

                foreach (Unit m in definitions.Units)
                {
                    if (_tc.CancellationToken.IsCancellationRequested) break;

                    xml.WriteStartElement("unit");
                    xml.WriteAttributeString("name", m.TargetKeyword);
                    {
                        xml.WriteStartElement("synopsis");
                        xml.WriteCData(m.ToString());
                        xml.WriteEndElement();

                        xml.WriteStartElement("family");
                        foreach (Measure r in m.Relatives())
                        {
                            xml.WriteStartElement("relative");
                            xml.WriteString(r.TargetKeyword);
                            xml.WriteEndElement();
                        }
                        xml.WriteEndElement();

                        xml.WriteStartElement("fellow");
                        foreach (BinaryOperation o in m.FellowOperations)
                        {
                            xml.WriteStartElement("operation");
                            xml.WriteAttributeString("op", o.Operation);
                            {
                                xml.WriteStartElement("lhs");
                                xml.WriteString(o.Lhs.TargetKeyword);
                                xml.WriteEndElement();

                                xml.WriteStartElement("rhs");
                                xml.WriteString(o.Rhs.TargetKeyword);
                                xml.WriteEndElement();

                                xml.WriteStartElement("ret");
                                xml.WriteString(o.Result.TargetKeyword);
                                xml.WriteEndElement();
                            }
                            xml.WriteEndElement();
                        }
                        xml.WriteEndElement();
                    }
                    xml.WriteEndElement();
                }

                foreach (Scale m in definitions.Scales)
                {
                    if (_tc.CancellationToken.IsCancellationRequested) break;

                    xml.WriteStartElement("scale");
                    xml.WriteAttributeString("name", m.TargetKeyword);
                    {
                        xml.WriteStartElement("synopsis");
                        xml.WriteCData(m.ToString());
                        xml.WriteEndElement();

                        xml.WriteStartElement("family");
                        foreach (Measure r in m.Relatives())
                        {
                            xml.WriteStartElement("relative");
                            xml.WriteString(r.TargetKeyword);
                            xml.WriteEndElement();
                        }
                        xml.WriteEndElement();
                    }
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
                return true;
            }
            catch (ArgumentException ex)
            {
                // ArgumentNullException
                // ArgumentOutOfRangeException
                // EncoderFallbackException
                ReportException(ex);
            }
            catch (InvalidOperationException ex)
            {
                // ObjectDisposedException 
                ReportException(ex);
            }
            catch (NotSupportedException ex)
            {
                ReportException(ex);
            }

            return false;

            void ReportException(Exception ex)
                => _tc.Report("Report", "unable to build the XML model.", ex);
        }

        /// <summary>
        /// Translate <see cref="Definitions"/> into the report file.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> collections.</param>
        /// <param name="filepath">Path to the report file.</param>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/> (error message sent to the logger).</returns>
        public bool ToFile(Definitions definitions, string filepath)
            => _template.Load() && Build(definitions) && ToFile(filepath);

        #endregion
    }
}
