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
    /// Catalog class generator.
    /// </summary>
    public class CatalogTranslator : Translator
    {
        private const string TargetClassName = "Catalog";

        /// <summary>
        /// Catalog translator
        /// </summary>
        /// <param name="targetNamespace">target namespace to put Catalog class in.</param>
        public CatalogTranslator(string targetNamespace) :
            base(targetNamespace)
        {
        }

        /// <summary>
        /// Builds a Catalog class (structured according to an XSLT <paramref name="template"/>) populated with <paramref name="units"/> and <paramref name="scales"/> generated.
        /// </summary>
        /// <param name="template">XSLT template for the Catalog</param>
        /// <param name="unitFamilyCount">number of unit families</param>
        /// <param name="units">list of transformed units</param>
        /// <param name="scaleFamilyCount">number of scale families</param>
        /// <param name="scales">list of transformed scales</param>
        /// <returns>Pair of strings (name, code) for the Catalog class.</returns>
        public (string, string) Translate(XslCompiledTransform template,
                                          int unitFamilyCount,
                                          List<UnitType> units,
                                          int scaleFamilyCount,
                                          List<ScaleType> scales)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(XmlCatalog())))
            {
                StringBuilder csb = new(16 * 1024);
                template.Transform(reader, null, new StringWriter(csb));
                return (TargetClassName, csb.ToString());
            }

            string XmlCatalog()
            {
                StringBuilder xsb = new(10 * 1024);

                xsb.Append("<catalog")
                    .Append(" ns=\"").Append(TargetNamespace).Append('"')
                    .Append(" uct=\"").Append(units.Count).Append('"')
                    .Append(" ufct=\"").Append(unitFamilyCount).Append('"')
                    .Append(" sct=\"").Append(scales.Count).Append('"')
                    .Append(" sfct=\"").Append(scaleFamilyCount).Append('"')
                    .Append(" tm=\"").Append(DateTime.Now).Append("\">");

                foreach (UnitType u in units)
                {
                    xsb.Append("<unit name=\"").Append(u.Typename).Append("\">")
                        .Append("<![CDATA[").Append(GetComment(u)).Append("]]>")
                        .Append("</unit>");
                }

                foreach (ScaleType s in scales)
                {
                    xsb.Append("<scale name=\"").Append(s.Typename).Append("\">")
                        .Append("<![CDATA[").Append(GetComment(s)).Append("]]>")
                        .Append("</scale>");
                }

                xsb.Append("</catalog>");

                return xsb.ToString();
            }

            static string GetComment(MeasureType m)
            {
                int offset = 36 - m.Typename.Length - 12;
                return $"{string.Empty.PadRight(offset > 0 ? offset : 1)}// {m}";
            }
        }
    }
}
