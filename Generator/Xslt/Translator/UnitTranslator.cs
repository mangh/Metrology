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
using System.Xml;
using System.Xml.Xsl;

namespace Mangh.Metrology
{
    /// <summary>
    /// Unit generator.
    /// </summary>
    public class UnitTranslator : Translator
    {
        /// <summary>
        /// Family id/number last set in <see cref="Translate"/> method; equal to the number of families found so far.
        /// </summary>
        public int Family { get; private set; }

        /// <summary>
        /// Selects compile-time (Late=false) or run-time (Late=true) mode.
        /// </summary>
        public bool Late { get; private set; }

        /// <summary>
        /// Unit generator constructor
        /// </summary>
        /// <param name="targetNamespace">target namespace for the units to be transformed</param>
        /// <param name="late">false for compile time units, true for run time (i.e. late) units</param>
        public UnitTranslator(string targetNamespace, bool late = false) :
            base(targetNamespace)
        {
            Family = 0;
            Late = late;
        }

        /// <summary>
        /// Translates <paramref name="units"/> into target language structs/classes, according to an XSLT unit <paramref name="template"/>.
        /// </summary>
        /// <param name="template">XSLT template for a unit</param>
        /// <param name="units">collection of units to be translated</param>
        /// <param name="initialFamily">family initial id</param>
        /// <param name="startIndex">index of the item the translattion is to start from
        /// (or the number of items to be skipped at the beginning of the list e.g. all compile time items)
        /// </param>
        /// <returns>Collection of string pairs (class name, class code); one pair for each unit</returns>
        public IEnumerable<(string, string)> Translate(XslCompiledTransform template,
                                                       List<UnitType> units,
                                                       int initialFamily = 0,
                                                       int startIndex = 0)
        {
            Family = initialFamily;

            StringBuilder csb = new(8 * 1024);
            StringBuilder xsb = new(2 * 1024);

            string timestamp = DateTime.Now.ToString();

            for (int i = startIndex; i < units.Count; i++)
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(XmlUnit(units[i]))))
                {
                    csb.Clear();
                    template.Transform(reader, null, new StringWriter(csb));
                    yield return (/*class name*/ units[i].Typename, /*class code*/ csb.ToString());
                }
            }

            string XmlUnit(UnitType u)
            {
                xsb.Clear();

                xsb.Append("<unit")
                    .Append(" name=\"").Append(u.Typename).Append('"')
                    .Append(" ns=\"").Append(TargetNamespace).Append('"')
                    .Append(" late=\"").Append(Late ? "yes" : "no").Append('"')
                    .Append(" monetary=\"").Append((u.Sense.Value[Magnitude.Money] == 0) ? "yes" : "no").Append('"')
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

                    xsb.Append("<value>").Append(Late ? "Value" : "m_value").Append("</value>");
                    xsb.Append("<sense>").Append(u.Sense.Code).Append("</sense>");
                    xsb.Append("<factor>").Append(u.Factor.Code).Append("</factor>");
                    xsb.Append("<format>").Append(u.Format).Append("</format>");

                    xsb.Append("<tags>");
                    foreach (var tag in u.Tags)
                    {
                        xsb.Append("<tag>").Append(tag).Append("</tag>");
                    }
                    xsb.Append("</tags>");

                    xsb.Append("<family")
                        .Append(" id=\"").Append((u.Prime is null) ? Family++.ToString() : u.Prime.Typename + ".Family").Append('"')
                        .Append('>');
                    foreach (MeasureType relative in u.Relatives())
                    {
                        xsb.Append("<relative>").Append(relative.Typename).Append("</relative>");
                    }
                    xsb.Append("</family>");

                    xsb.Append("<outer>");
                    foreach (BinaryOperation o in u.OuterOperations)
                    {
                        xsb.Append("<operation op=\"").Append(o.Operation).Append('"').Append(o.Operation == "^" ? " inner=\"*\">" : ">");
                        {
                            xsb.Append("<lhs").Append(o.Lhs.IsPredefined ? " builtin=\"yes\">" : ">")
                                .Append(o.Lhs.Typename)
                                .Append("</lhs>");

                            xsb.Append("<rhs").Append(o.Rhs.IsPredefined ? " builtin=\"yes\">" : ">")
                                .Append(o.Rhs.Typename)
                                .Append("</rhs>");

                            xsb.Append("<ret").Append(o.Result.IsPredefined ? " builtin=\"yes\">" : ">")
                                .Append(o.Result.Typename)
                                .Append("</ret>");
                        }
                        xsb.Append("</operation>");
                    }
                    xsb.Append("</outer>");
                }
                xsb.Append("</unit>");

                return xsb.ToString();
            }
        }
    }
}
