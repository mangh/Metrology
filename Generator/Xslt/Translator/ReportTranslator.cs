/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Mangh.Metrology
{
    /// <summary>
    /// Report text generator.
    /// </summary>
    public class ReportTranslator : Translator
    {
        private const string TargetFileName = "generator_report.txt";

        /// <summary>
        /// Report generator constructor.
        /// </summary>
        /// <param name="targetNamespace">target namespace the generated units and scales are put into.</param>
        public ReportTranslator(string targetNamespace) :
            base(targetNamespace)
        {
        }

        /// <summary>
        /// Builds a Report (structured according to an XSLT <paramref name="template"/>) of <paramref name="units"/> and <paramref name="scales"/> generated.
        /// </summary>
        /// <param name="template">XSLT template for the Report.</param>
        /// <param name="unitFamilyCount">number of unit families found.</param>
        /// <param name="units">list of generated units</param>
        /// <param name="scaleFamilyCount">number of scale families found.</param>
        /// <param name="scales">list of generated scales</param>
        /// <returns>Pair of strings (name, contents) for the Report file.</returns>
        public (string, string) Translate(XslCompiledTransform template,
                                          int unitFamilyCount,
                                          List<UnitType> units,
                                          int scaleFamilyCount,
                                          List<ScaleType> scales)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(XmlReport())))
            {
                StringBuilder csb = new(8 * 1024);
                template.Transform(reader, null, new StringWriter(csb));
                return (TargetFileName, csb.ToString());
            }

            string XmlReport()
            {
                StringBuilder xsb = new(2 * 1024);

                xsb.Append("<report")
                    .Append(" ns=\"").Append(TargetNamespace).Append('"')
                    .Append(" uct=\"").Append(units.Count).Append('"')
                    .Append(" ufct=\"").Append(unitFamilyCount).Append('"')
                    .Append(" sct=\"").Append(scales.Count).Append('"')
                    .Append(" sfct=\"").Append(scaleFamilyCount).Append('"')
                    .Append(" tm=\"").Append(DateTime.Now).Append("\">");

                foreach (UnitType m in units)
                {
                    xsb.Append("<unit name=\"").Append(m.Typename).Append("\">");
                    {
                        xsb.Append("<synopsis>").Append("<![CDATA[").Append(m.ToString()).Append("]]></synopsis>");

                        xsb.Append("<family>");
                        foreach (MeasureType r in m.Relatives())
                        {
                            xsb.Append("<relative>").Append(r.Typename).Append("</relative>");
                        }
                        xsb.Append("</family>");

                        xsb.Append("<outer>");
                        foreach (BinaryOperation o in m.OuterOperations)
                        {
                            xsb.Append("<operation op=\"").Append(o.Operation).Append("\">");
                            {
                                xsb.Append("<lhs>").Append(o.Lhs.Typename).Append("</lhs>");
                                xsb.Append("<rhs>").Append(o.Rhs.Typename).Append("</rhs>");
                                xsb.Append("<ret>").Append(o.Result.Typename).Append("</ret>");
                            }
                            xsb.Append("</operation>");
                        }
                        xsb.Append("</outer>");

                    }
                    xsb.Append("</unit>");
                }

                foreach (ScaleType m in scales)
                {
                    xsb.Append("<scale name=\"").Append(m.Typename).Append("\">");
                    {
                        xsb.Append("<synopsis>").Append("<![CDATA[").Append(m.ToString()).Append("]]></synopsis>");
                        xsb.Append("<family>");
                        foreach (MeasureType r in m.Relatives())
                        {
                            xsb.Append("<relative>").Append(r.Typename).Append("</relative>");
                        }
                        xsb.Append("</family>");
                    }
                    xsb.Append("</scale>");
                }

                xsb.Append("</report>");

                return xsb.ToString();
            }
        }
    }
}
