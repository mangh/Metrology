using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Xml.Xsl;

namespace Mangh.Metrology
{
    public partial class Generator
    {
        /// <summary>
        /// Provider for a source code of units.
        /// </summary>
        internal class UnitProvider : Provider
        {
            public UnitTranslator Master { get; }

            public UnitProvider(string targetNamespace, bool late, AdditionalText template) :
               base("UNITS", template)
            {
                Master = new UnitTranslator(targetNamespace, late);
            }

            public int AddSource(GeneratorExecutionContext context, List<UnitType> units)
            {
                XslCompiledTransform? template = LoadXsltTemplate(context);
                if (template is not null)
                {
                    foreach ((string file, string contents) in
                        Master.Translate(context.CancellationToken, template, units, initialFamily: 0, startIndex: 0))
                    {
                        context.AddSource(file, contents);
                    }
                }
                return Master.Family;   // number of unit families generated
            }
        }
    }
}