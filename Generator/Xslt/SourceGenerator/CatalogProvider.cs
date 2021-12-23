using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Xsl;

namespace Mangh.Metrology
{
    public partial class Generator
    {
        /// <summary>
        /// Provider for a Catalog source code.
        /// </summary>
        internal class CatalogProvider : Provider
        {
            public CatalogTranslator Master { get; }

            public CatalogProvider(string targetNamespace, AdditionalText template) :
                base("CATALOG", template)
            {
                Master = new(targetNamespace);
            }

            public void AddSource(GeneratorExecutionContext context,
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

                    context.AddSource(file, contents);
                }
            }
        }
    }
}