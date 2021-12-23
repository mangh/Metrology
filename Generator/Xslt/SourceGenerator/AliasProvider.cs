using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Xml.Xsl;

namespace Mangh.Metrology
{
    public partial class Generator
    {
        /// <summary>
        /// Provider for Aliases source code (in an include file).
        /// </summary>
        internal class AliasProvider : Provider
        {
            public AliasTranslator Master { get; }

            public AliasProvider(string targetNameSpace, AdditionalText template) :
                base("ALIASES", template)
            {
                Master = new(targetNameSpace);
            }

            public void MakeAliases(GeneratorExecutionContext context, List<UnitType> units, List<ScaleType> scales)
            {
                XslCompiledTransform? template = LoadXsltTemplate(context);
                if (template is not null)
                {
                    (string file, string contents) =
                        Master.Translate(context.CancellationToken, template, units, scales, global: true);

                    // Save aliases to the template folder
                    string? path = MakePath(context, Template.Path, file);
                    if (path is not null)
                    {
                        SaveToFile(context, path, contents);
                    }
                }
            }
        }
    }
}