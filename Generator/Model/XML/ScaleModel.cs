/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Xml;

namespace Mangh.Metrology.XML
{
    /// <summary>
    /// XML Model for a <see cref="Scale"/>.
    /// </summary>
    public class ScaleModel : Model
    {
        #region Fields
        private readonly bool _late;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="ScaleModel"/> constructor.
        /// </summary>
        /// <param name="templatePath">Path to the XSLT template file.</param>
        /// <param name="tc">XML model translation context.</param>
        /// <param name="late"><see langword="false"/> for compile-time scales, <see langword="true"/> for run-time (i.e. late) scale.</param>
        public ScaleModel(string templatePath, TranslationContext tc, bool late = false)
            : base(templatePath, tc, capacity: 8 * 1024/*, useAsync: false*/)
        {
            _late = late;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Builds XML model for a single <see cref="Scale"/>.
        /// </summary>
        /// <param name="s"><see cref="Scale"/> to be transformed.</param>
        /// <returns><see langword="true"/> on a successful build (in the internal buffer), otherwise <see langword="false"/>.</returns>
        public bool Build(Scale s)
        {
            try
            {
                _model.Position = 0;
                _model.SetLength(0);

                using XmlWriter xml = XmlWriter.Create(_model);

                xml.WriteStartElement("scale");
                xml.WriteAttributeString("name", s.TargetKeyword);
                xml.WriteAttributeString("ns", _tc.TargetNamespace);
                xml.WriteAttributeString("unit", s.Unit.TargetKeyword);
                xml.WriteAttributeString("late", _late ? "yes" : "no");
                xml.WriteStartAttribute("tm");
                xml.WriteValue(_tc.Timestamp);
                xml.WriteEndAttribute();

                // scale children
                {
                    xml.WriteStartElement("valuetype");
                    xml.WriteAttributeString("keywd", s.NumericType.TargetKeyword);
                    xml.WriteString(s.NumericType.TargetTypename);
                    xml.WriteEndElement();

                    xml.WriteStartElement("offset");
                    xml.WriteString(s.Offset.UnfoldedCode);
                    xml.WriteEndElement();

                    xml.WriteStartElement("format");
                    xml.WriteCData(s.Format);
                    xml.WriteEndElement();

                    xml.WriteStartElement("refpoint");
                    xml.WriteAttributeString("normalized", s.RefPointNormalized);
                    if (!string.IsNullOrWhiteSpace(s.RefPoint))
                        xml.WriteString(s.RefPoint);
                    xml.WriteEndElement();

                    xml.WriteStartElement("family");
                    xml.WriteStartAttribute("id");
                    xml.WriteValue(s.Family);
                    xml.WriteEndAttribute();
                    if (s.Prime is not null)
                        xml.WriteAttributeString("prime", s.Prime.TargetKeyword);

                    foreach (Scale r in s.Relatives().Cast<Scale>())
                    {
                        xml.WriteStartElement("relative");
                        xml.WriteAttributeString("unit", r.Unit.TargetKeyword);
                        xml.WriteString(r.TargetKeyword);
                        xml.WriteEndElement();
                    }
                    xml.WriteEndElement(/*family*/);
                }

                xml.WriteEndElement(/*scale*/);
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
                => _tc.Report(s.TargetKeyword, "unable to build the XML model for the scale.", ex);
        }

        /// <summary>
        /// Translate <see cref="Definitions.Scales"/> into target structures and save them to files.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> collections.</param>
        /// <param name="targetDirectory">Path to the folder where the generated <see cref="Scale"/> structures will be saved.</param>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/> (error message sent to the logger).</returns>
        public bool ToFile(Definitions definitions, string targetDirectory)
        {
            if (!_template.Load())
                return false;

            foreach (Scale s in definitions.Scales)
            {
                if (_tc.CancellationToken.IsCancellationRequested || !Build(s) || !ToFile(FilePath.Combine(targetDirectory, _tc.SourceFileName(s))))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Transforms <see cref="Definitions.Scales"/> into a <see cref="SourceText"/> collection.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> collections.</param>
        /// <param name="targetDirectory">Path to the folder where the generated <see cref="Scale"/> structures will be saved.</param>
        /// <param name="collect"><see cref="SourceText"/> collection strategy.</param>
        /// <param name="startIndex"><see cref="Scale"/> index to start with.</param>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/> (error message sent to the logger).</returns>
        public bool ToSourceText(Definitions definitions, string targetDirectory, Action<string, SourceText> collect, int startIndex = 0)
        {
            if (!_template.Load())
                return false;

            for (int i = startIndex; i < definitions.Scales.Count; ++i)
            {
                Scale s = definitions.Scales[i];

                if (_tc.CancellationToken.IsCancellationRequested || !Build(s))
                    return false;

                string hintPath = FilePath.Combine(targetDirectory, _tc.SourceFileName(s));
                SourceText? sourceCode = ToSourceText(hintPath);
                if (sourceCode is null)
                    return false;

                collect(hintPath, sourceCode);
            }

            return true;
        }
        #endregion
    }
}
