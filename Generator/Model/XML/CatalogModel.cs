/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Threading;
using System.Xml;

namespace Mangh.Metrology.XML
{
    /// <summary>
    /// Catalog XML Model.
    /// </summary>
    public class CatalogModel : Model
    {
        #region Constructor(s)
        /// <summary>
        /// <see cref="CatalogModel"/> constructor.
        /// </summary>
        /// <param name="templatePath">Path to the XSLT template file.</param>
        /// <param name="tc">XML model translation context.</param>
        public CatalogModel(string templatePath, TranslationContext tc)
            : base(templatePath, tc, capacity: 16 * 1024/*, useAsync: false*/)
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Builds XML model of the "<c>Catalog</c>" of <see cref="Unit"/> and <see cref="Scale"/> entities.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> definitions.</param>
        /// <returns><see langword="true"/> on a successful build (XML model ready in the internal buffer), otherwise <see langword="false"/>.</returns>
        public bool Build(Definitions definitions)
        {
            try
            {
                _model.Position = 0;
                _model.SetLength(0);

                using XmlWriter xml = XmlWriter.Create(_model);

                xml.WriteStartElement("catalog");
                {
                    xml.WriteAttributeString("ns", _tc.TargetNamespace);

                    xml.WriteStartAttribute("uct");
                    xml.WriteValue(definitions.Units.Count);
                    xml.WriteEndAttribute();

                    xml.WriteStartAttribute("ufct");
                    xml.WriteValue(definitions.FamilyCount(definitions.Units));
                    xml.WriteEndAttribute();

                    xml.WriteStartAttribute("sct");
                    xml.WriteValue(definitions.Scales.Count);
                    xml.WriteEndAttribute();

                    xml.WriteStartAttribute("sfct");
                    xml.WriteValue(definitions.FamilyCount(definitions.Scales));
                    xml.WriteEndAttribute();

                    xml.WriteStartAttribute("tm");
                    xml.WriteValue(_tc.Timestamp);
                    xml.WriteEndAttribute();
                }

                foreach (Unit u in definitions.Units)
                {
                    if (_tc.CancellationToken.IsCancellationRequested) break;

                    xml.WriteStartElement("unit");
                    xml.WriteAttributeString("name", u.TargetKeyword);
                    xml.WriteCData(GetComment(u));
                    xml.WriteEndElement();
                }

                foreach (Scale s in definitions.Scales)
                {
                    if (_tc.CancellationToken.IsCancellationRequested) break;

                    xml.WriteStartElement("scale");
                    xml.WriteAttributeString("name", s.TargetKeyword);
                    xml.WriteCData(GetComment(s));
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
                return true;
            }
            catch (ArgumentException ex)
            {
                // ArgumentNullException
                // ArgumentOutOfRangeException
                // EncoderFallbackException
                ReportException(ex);
            }
            catch (InvalidOperationException ex)
            {
                // ObjectDisposedException 
                ReportException(ex);
            }
            catch (NotSupportedException ex)
            {
                ReportException(ex);
            }

            return false;

            void ReportException(Exception ex)
                => _tc.Report("Catalog", "unable to build the XML model.", ex);

            static string GetComment(Measure m)
            {
                int offset = 36 - m.TargetKeyword.Length - 12;
                return $"{string.Empty.PadRight(offset > 0 ? offset : 1)}// {m}";
            }
        }

        /// <summary>
        /// Translate <see cref="Definitions"/> into the <c>Catalog</c> class file.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> collections.</param>
        /// <param name="filepath">Path to the Catalog source code file.</param>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/> (error message sent to the logger).</returns>
        public bool ToFile(Definitions definitions, string filepath)
        {
            return _template.Load() && Build(definitions) && ToFile(filepath);
        }

        /// <summary>
        /// Translate <see cref="Definitions"/> into the <c>Catalog</c> class <see cref="SourceText"/> item.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> collections.</param>
        /// <param name="hintPath">An identifier used to refer to the generated <see cref="SourceText"/> (e.g. the path to the <c>Catalog</c> source code file).</param>
        /// <param name="collect"><see cref="SourceText"/> collection strategy.</param>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/> (error message sent to the logger).</returns>
        public bool ToSourceText(Definitions definitions, string hintPath, Action<string, SourceText> collect)
        {
            if (_template.Load() && Build(definitions))
            {
                SourceText? sourceCode = ToSourceText(hintPath);
                if (sourceCode is not null)
                {
                    collect(hintPath, sourceCode);
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
