/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Mangh.Metrology
{
    /// <summary>
    /// Aliases file generator.
    /// </summary>
    public class AliasTranslator : Translator
    {
        private const string TargetFileName = "Aliases.inc";

        /// <summary>
        /// Aliases translator constructor.
        /// </summary>
        /// <param name="targetNamespace">target namespace the generated units and scales are put into.</param>
        public AliasTranslator(string targetNamespace) :
            base(targetNamespace)
        {
        }

        /// <summary>
        /// Transforms <paramref name="units"/> and <paramref name="scales"/> into "aliasing" statements (structured according to an XSLT <paramref name="template"/>).
        /// </summary>
        /// <param name="template">XSLT template for the Aliases.</param>
        /// <param name="units">list of transformed units.</param>
        /// <param name="scales">list of transformed scales.</param>
        /// <param name="global">
        /// <list type="bullet">
        /// <item><term>true</term><description> global (project-wide) aliases,</description></item>
        /// <item><term>false</term><description> local (file scoped) aliases.</description></item>
        /// </list>
        /// </param>
        /// <returns>Pair of strings (name, contents) for the <see cref="TargetFileName"/> file.</returns>
        public (string, string) Translate(XslCompiledTransform template,
                                          List<UnitType> units,
                                          List<ScaleType> scales,
                                          bool global = true)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(XmlAliases())))
            {
                StringBuilder csb = new(8 * 1024);
                template.Transform(reader, null, new StringWriter(csb));
                return (TargetFileName, csb.ToString());
            }

            string XmlAliases()
            {
                StringBuilder xsb = new(2 * 1024);

                xsb.Append("<aliases")
                    .Append(" ns=\"").Append(TargetNamespace).Append('"')
                    .Append(global ? " global=\"yes\">" : ">");

                foreach (UnitType u in units)
                {
                    xsb.Append("<unit ")
                        .Append(" name=\"").Append(u.Typename).Append('"')
                        .Append(" alias=\"").Append(u.Factor.Value.PredefinedTypename).Append('"')
                        .Append("/>");
                }

                foreach (ScaleType s in scales)
                {
                    xsb.Append("<scale ")
                        .Append(" name=\"").Append(s.Typename).Append('"')
                        .Append(" alias=\"").Append(s.Unit.Factor.Value.PredefinedTypename).Append('"')
                        .Append("/>");
                }

                xsb.Append("</aliases>");

                return xsb.ToString();
            }
        }

    }
}
