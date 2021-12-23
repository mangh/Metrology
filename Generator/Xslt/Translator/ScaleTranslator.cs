/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Xsl;

namespace Mangh.Metrology
{
    /// <summary>
    /// Scale translator
    /// </summary>
    public class ScaleTranslator : Translator
    {
        /// <summary>
        /// Family number (id) last set in <see cref="Translate"/> method; equal to the number of families found so far.
        /// </summary>
        public int Family { get; private set; }

        /// <summary>
        /// Selects compile-time (Late=false) or run-time (Late=true) mode.
        /// </summary>
        public bool Late { get; }

        /// <summary>
        /// ScaleGenerator constructor
        /// </summary>
        /// <param name="targetNamespace">target namespace for the scales to be transformed</param>
        /// <param name="late">false for compile-time scales, true for run-time scales.</param>
        public ScaleTranslator(string targetNamespace, bool late = false) :
            base(targetNamespace)
        {
            Family = 0;
            Late = late;
        }

        /// <summary>
        /// Translates <paramref name="scales"/> into target language structs/classes, according to an XSLT scale <paramref name="template"/>.
        /// </summary>
        /// <param name="ct">propagates notification that operation should be canceled.</param>
        /// <param name="template">XSLT template for a scale</param>
        /// <param name="scales">collection of scales to be translatmed</param>
        /// <param name="initialFamily">family initial id</param>
        /// <param name="startIndex">index of the item the translation is to start from
        /// (or the number of items to be skipped at the beginning of the list e.g. all compile time items)
        /// </param>
        /// <returns>Collection of string pairs (class name, class code); one pair for each scale.</returns>
        public IEnumerable<(string, string)> Translate(CancellationToken ct,
                                                       XslCompiledTransform template,
                                                       List<ScaleType> scales,
                                                       int initialFamily,
                                                       int startIndex = 0)
        {
            Family = initialFamily;

            StringBuilder xsb = new(2 * 1024);
            StringBuilder csb = new(8 * 1024);

            string timestamp = DateTime.Now.ToString();

            for (int i = startIndex; i < scales.Count; i++)
            {
                if (ct.IsCancellationRequested) break;

                using (XmlReader reader = XmlReader.Create(new StringReader(XmlScale(scales[i]))))
                {
                    csb.Clear();
                    template.Transform(reader, null, new StringWriter(csb));
                    yield return (scales[i].Typename, csb.ToString());
                }
            }

            string XmlScale(ScaleType s)
            {
                UnitType u = s.Unit;
                xsb.Clear();

                xsb.Append("<scale")
                    .Append(" name=\"").Append(s.Typename).Append('"')
                    .Append(" ns=\"").Append(TargetNamespace).Append('"')
                    .Append(" unit=\"").Append(s.Unit.Typename).Append('"')
                    .Append(" late=\"").Append(Late ? "yes" : "no").Append('"')
                    .Append(" tm=\"").Append(timestamp).Append("\">");
                {
                    xsb.Append("<valuetype>");
                    {
                        xsb.Append("<name>").Append(u.Factor.Value.Typename).Append("</name>");
                        xsb.Append("<alias>").Append(u.Factor.Value.PredefinedTypename).Append("</alias>");
                        xsb.Append("<one>").Append(u.One.Code).Append("</one>");
                        xsb.Append("<zero>").Append(u.Zero.Code).Append("</zero>");
                    }
                    xsb.Append("</valuetype>");

                    xsb.Append("<level>").Append(Late ? "Level" : "m_level").Append("</level>");
                    xsb.Append("<offset>").Append(s.Offset.Code).Append("</offset>");
                    xsb.Append("<format>").Append(s.Format).Append("</format>");
                    xsb.Append("<refpoint normalized=\"").Append(s.RefPointNormalized).Append("\">")
                        .Append(string.IsNullOrWhiteSpace(s.RefPoint) ? string.Empty : s.RefPoint)
                        .Append("</refpoint>");

                    xsb.Append("<family id=\"").Append((s.Prime is null) ? Family++.ToString() : s.Prime.Typename + ".Family").Append("\">");
                    foreach (var r in s.Relatives())
                    {
                        if (r is ScaleType rs)
                        {
                            xsb.Append("<relative").Append(" unit=\"").Append(rs.Unit.Typename).Append("\">")
                                .Append(rs.Typename)
                                .Append("</relative>");
                        }
                    }
                    xsb.Append("</family>");
                }
                xsb.Append("</scale>");

                return xsb.ToString();
            }
        }
    }
}
