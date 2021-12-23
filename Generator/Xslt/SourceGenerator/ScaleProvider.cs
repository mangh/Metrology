using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Xml.Xsl;

namespace Mangh.Metrology
{
    public partial class Generator
    {
        /// <summary>
        /// Provider for a source code of scales.
        /// </summary>
        internal class ScaleProvider : Provider
        {
            public ScaleTranslator Master { get; }

            public ScaleProvider(string targetNamespace, bool late, AdditionalText template) :
               base("SCALES", template)
            {
                Master = new(targetNamespace, late);
            }
            public int AddSource(GeneratorExecutionContext context, List<ScaleType> scales, int initialFamily)
            {
                XslCompiledTransform? template = LoadXsltTemplate(context);
                if (template is not null)
                {
                    foreach ((string file, string contents) in
                        Master.Translate(context.CancellationToken, template, scales, initialFamily, startIndex: 0))
                    {
                        context.AddSource(file, contents);
                    }
                }
                return Master.Family - initialFamily;   // number of scale families generated
            }
        }
    }
}