/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Mangh.Metrology.Language;
using System.Xml;
using System.Xml.Xsl;

namespace Mangh.Metrology.XML
{
    /// <summary>
    /// XML Model Translation Context.
    /// </summary>
    public abstract class TranslationContext : Metrology.TranslationContext
    {
        #region Translation settings (model-specific)
        /// <summary>XML Resolver.</summary>
        public XmlResolver XmlResolver { get; set; }

        /// <summary>XML reader settings</summary>
        public XmlReaderSettings XmlReaderSettings { get; set; }

        /// <summary>XSLT settings.</summary>
        public XsltSettings XsltSettings { get; set; }
        #endregion

        #region Input file names (model-specific)
        /// <summary>Template file extension.</summary>
        public override string TEMPLATE_EXT => "xslt";
        #endregion

        #region Output file names (model-specific)
        /// <summary>Model file extension.</summary>
        public override string MODEL_EXT => "xml";

        /// <summary>
        /// Model file path (made from the path to the source code file).
        /// </summary>
        /// <param name="sourcepath">Source code file path.</param>
        public virtual string ModelFilePath(string sourcepath)
            => FilePath.ChangeExtension(sourcepath, MODEL_EXT);
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="TranslationContext"/> constructor.
        /// </summary>
        /// <param name="lc">Target language context.</param>
        public TranslationContext(Context lc)
            : base(lc)
        {
            // The XmlResolver resolves <xsl:import> or <xsl:include> elements:
            XmlResolver = new XmlUrlResolver();

            // The XSLT document() function can be useful,
            // but script blocks can be dangerous:
            XsltSettings = new(enableDocumentFunction: true, enableScript: false);

            XmlReaderSettings = new()
            {
                Async = false,
                XmlResolver = XmlResolver
            };
        }
        #endregion
    }
}
