using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Xml.Xsl;

namespace Mangh.Metrology
{
    public partial class Generator
    {
        /// <summary>
        /// Provider for a Report file.
        /// </summary>
        internal class ReportProvider : Provider
        {
            public ReportTranslator Master { get; }

            public ReportProvider(string targetNamespace, AdditionalText template) :
                base("REPORT", template)
            {
                Master = new(targetNamespace);
            }

            public void MakeReport(GeneratorExecutionContext context,
                                   int unitFamilyCount,
                                   List<UnitType> units,
                                   int scaleFamilyCount,
                                   List<ScaleType> scales)
            {
                XslCompiledTransform? template = LoadXsltTemplate(context);
                if (template is not null)
                {
                    (string file, string contents) =
                        Master.Translate(context.CancellationToken, template, unitFamilyCount, units, scaleFamilyCount, scales);

                    // Save the report to the template folder
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