/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

using Mangh.Metrology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Demo.UnitsOfMeasurement
{
    public partial class RuntimeLoader
    {
        public class Generator
        {
            #region Constants
            private const string TEMPLATE_FOLDER = "Templates";
            private const string UNIT_TEMPLATE = "unit.xslt";
            private const string SCALE_TEMPLATE = "scale.xslt";
            #endregion

            #region Fields
            private readonly RuntimeLoader m_loader;
            #endregion

            #region Properties
            #endregion

            #region Constructor(s)
            public Generator(RuntimeLoader loader)
            {
                m_loader = loader;
            }
            #endregion

            #region Methods
            public string? Transform(IDefinitions catalog, int unitStartIndex, int scaleStartIndex)
            {
                string targetNamespace = GetType().Namespace!;
                string? templateFolder = m_loader.m_io.GetSubfolder(TEMPLATE_FOLDER);
                if (templateFolder is null)
                    return null;

                UnitTranslator unitTranslator = new(targetNamespace, late: true);
                XslCompiledTransform? unitTemplate = m_loader.m_io.CompileXsltTemplate(templateFolder, UNIT_TEMPLATE);
                if (unitTemplate is null)
                    return null;

                ScaleTranslator scaleTranslator = new(targetNamespace, late: true);
                XslCompiledTransform? scaleTemplate = m_loader.m_io.CompileXsltTemplate(templateFolder, SCALE_TEMPLATE);
                if (scaleTemplate is null)
                    return null;

                int familyStartId = catalog.MaxFamilyFound + 1;    // start id for (possible) new families

                StringBuilder csb = new(16 * 1024);

                // Units ///////////////////////////////////////////////////////////////////
                foreach ((_, string contents) in unitTranslator.Translate(unitTemplate, catalog.Units, initialFamily: familyStartId, startIndex: unitStartIndex))
                {
                    csb.Append(contents);
                }
                // Scales ///////////////////////////////////////////////////////////////////
                foreach ((_, string contents) in scaleTranslator.Translate(scaleTemplate, catalog.Scales, initialFamily: unitTranslator.Family, scaleStartIndex))
                {
                    csb.Append(contents);
                }
                return csb.ToString();
            }
           #endregion
        }
    }
}