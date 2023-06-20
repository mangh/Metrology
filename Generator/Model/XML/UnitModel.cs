/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis.Text;
using System;
using System.Xml;

namespace Mangh.Metrology.XML
{
    /// <summary>
    /// XML Model for a <see cref="Unit"/>.
    /// </summary>
    public class UnitModel : Model
    {
        #region Fields
        private readonly bool _late;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="UnitModel"/> constructor.
        /// </summary>
        /// <param name="templatePath">Path to the XSLT template file.</param>
        /// <param name="tc">XML model translation context.</param>
        /// <param name="late"><see langword="false"/> for compile-time units, <see langword="true"/> for run-time (i.e. late) units.</param>
        public UnitModel(string templatePath, TranslationContext tc, bool late = false)
            : base(templatePath, tc, capacity: 8 * 1024/*, useAsync: false*/)
        {
            _late = late;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Builds XML model for a single <see cref="Unit"/>.
        /// </summary>
        /// <param name="u"><see cref="Unit"/> to be transformed.</param>
        /// <returns><see langword="true"/> on a successful build (in the internal buffer), otherwise <see langword="false"/>.</returns>
        public bool Build(Unit u)
        {
            try
            {
                _model.Position = 0;
                _model.SetLength(0);

                using XmlWriter xml = XmlWriter.Create(_model);

                xml.WriteStartElement("unit");
                xml.WriteAttributeString("name", u.TargetKeyword);
                xml.WriteAttributeString("ns", _tc.TargetNamespace);
                xml.WriteAttributeString("late", _late ? "yes" : "no");
                xml.WriteAttributeString("monetary", u.Sense.Value[Magnitude.Money] != 0 ? "yes" : "no");
                xml.WriteStartAttribute("tm");
                xml.WriteValue(_tc.Timestamp);
                xml.WriteEndAttribute();

                // unit children
                {
                    xml.WriteStartElement("valuetype");
                    xml.WriteAttributeString("keywd", u.NumericType.TargetKeyword);
                    xml.WriteString(u.NumericType.TargetTypename);
                    xml.WriteEndElement();

                    xml.WriteStartElement("sense");
                    xml.WriteAttributeString("expr", u.Sense.SimpleCode);
                    xml.WriteString(u.Sense.UnfoldedCode);
                    xml.WriteEndElement();

                    xml.WriteStartElement("factor");
                    xml.WriteAttributeString("expr", u.Factor.SimpleCode);
                    xml.WriteString(u.Factor.UnfoldedCode);
                    xml.WriteEndElement();

                    xml.WriteStartElement("format");
                    xml.WriteCData(u.Format);
                    xml.WriteEndElement();

                    xml.WriteStartElement("tags");
                    foreach (var tag in u.Tags)
                    {
                        xml.WriteStartElement("tag");
                        xml.WriteString(tag);
                        xml.WriteEndElement();
                    }
                    xml.WriteEndElement();

                    xml.WriteStartElement("family");
                    xml.WriteStartAttribute("id");
                    xml.WriteValue(u.Family);
                    xml.WriteEndAttribute();
                    if (u.Prime is not null)
                        xml.WriteAttributeString("prime", u.Prime.TargetKeyword);
                    foreach (Measure relative in u.Relatives())
                    {
                        xml.WriteStartElement("relative");
                        xml.WriteString(relative.TargetKeyword);
                        xml.WriteEndElement();
                    }
                    xml.WriteEndElement(/*family*/);

                    xml.WriteStartElement("fellow");
                    foreach (BinaryOperation o in u.FellowOperations)
                    {
                        xml.WriteStartElement("operation");
                        xml.WriteAttributeString("op", o.Operation);
                        if (o.Operation == "^")
                            xml.WriteAttributeString("inner", "*");
                        {
                            xml.WriteStartElement("lhs");
                            if (o.Lhs.IsPredefined) xml.WriteAttributeString("builtin", "yes");
                            xml.WriteString(o.Lhs.TargetKeyword);
                            xml.WriteEndElement();

                            xml.WriteStartElement("rhs");
                            if (o.Rhs.IsPredefined) xml.WriteAttributeString("builtin", "yes");
                            xml.WriteString(o.Rhs.TargetKeyword);
                            xml.WriteEndElement();

                            xml.WriteStartElement("ret");
                            if (o.Result.IsPredefined) xml.WriteAttributeString("builtin", "yes");
                            xml.WriteString(o.Result.TargetKeyword);
                            xml.WriteEndElement();
                        }
                        xml.WriteEndElement(/*operation*/);
                    }
                    xml.WriteEndElement(/*fellow*/);

                }

                xml.WriteEndElement(/*unit*/);
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
                => _tc.Report(u.TargetKeyword, "unable to build the XML model for the unit.", ex);
        }

        /// <summary>
        /// Translate <see cref="Definitions.Units"/> into the target structures and save them to files.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> collections.</param>
        /// <param name="targetDirectory">Path to the folder where the generated <see cref="Scale"/> structures will be saved.</param>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/> (error message sent to the logger).</returns>
        public bool ToFile(Definitions definitions, string targetDirectory)
        {
            if (!_template.Load())
                return false;

            foreach (Unit u in definitions.Units)
            {
                if (_tc.CancellationToken.IsCancellationRequested || !Build(u) || !ToFile(FilePath.Combine(targetDirectory, _tc.SourceFileName(u))))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Transforms <see cref="Definitions.Units"/> into a <see cref="SourceText"/> collection.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> collections.</param>
        /// <param name="targetDirectory">Path to the folder where the generated <see cref="Scale"/> structures will be saved.</param>
        /// <param name="collect"><see cref="SourceText"/> collection strategy.</param>
        /// <param name="startIndex"><see cref="Unit"/> index to start with.</param>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/> (error message sent to the logger).</returns>
        public bool ToSourceText(Definitions definitions, string targetDirectory, Action<string, SourceText> collect, int startIndex = 0)
        {
            if (!_template.Load())
                return false;

            for (int i = startIndex; i < definitions.Units.Count; ++i)
            {
                Unit u = definitions.Units[i];

                if (_tc.CancellationToken.IsCancellationRequested || !Build(u))
                    return false;

                string hintPath = FilePath.Combine(targetDirectory, _tc.SourceFileName(u));
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
