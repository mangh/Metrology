/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Threading;
using System.Xml;

namespace Mangh.Metrology.XML
{
    /// <summary>
    /// Aliasing Statements XML Model.
    /// </summary>
    public class AliasingModel : Model
    {
        #region Fields
        /// <summary>
        /// Option for the scope of "<c>using</c>" directives:<br/>
        /// - <see langword="true"/> : project-wide "<c>global using</c>" directives,<br/>
        /// - <see langword="false"/> : file-scoped "<c>using</c>" directives (only).
        /// </summary>
        private readonly bool _global;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="AliasingModel"/> constructor.
        /// </summary>
        /// <param name="templatePath">Path to the XSLT template file.</param>
        /// <param name="tc">XML model translation context.</param>
        /// <param name="global">Option for the scope of "<c>using</c>" directives:<br/>
        /// - <see langword="true"/> : project-wide "<c>global using</c>" directives,<br/>
        /// - <see langword="false"/> : file-scoped "<c>using</c>" directives (only).
        /// </param>
        public AliasingModel(string templatePath,  TranslationContext tc, bool global = false)
            : base(templatePath, tc/*, capacity: 2 * 1024, useAsync: false*/)
        {
            _global = global;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Builds XML model for Alias Statements.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> collections.</param>
        /// <returns><see langword="true"/> on a successful build (XML model ready in the internal buffer), otherwise <see langword="false"/>.</returns>
        public bool Build(Definitions definitions)
        {
            try
            {
                _model.Position = 0;
                _model.SetLength(0);

                using XmlWriter xml = XmlWriter.Create(_model);

                xml.WriteStartElement("aliases");
                {
                    xml.WriteAttributeString("ns", _tc.TargetNamespace);
                    xml.WriteAttributeString("global", _global ? "yes" : "no");
                    xml.WriteStartAttribute("tm");
                    xml.WriteValue(_tc.Timestamp);
                    xml.WriteEndAttribute();
                }

                foreach (Unit u in definitions.Units)
                {
                    if (_tc.CancellationToken.IsCancellationRequested) break;

                    xml.WriteStartElement("unit");
                    xml.WriteAttributeString("name", u.TargetKeyword);
                    xml.WriteAttributeString("alias", u.NumericType.TargetTypename);
                    xml.WriteEndElement();
                }

                foreach (Scale s in definitions.Scales)
                {
                    if (_tc.CancellationToken.IsCancellationRequested) break;

                    xml.WriteStartElement("scale");
                    xml.WriteAttributeString("name", s.TargetKeyword);
                    xml.WriteAttributeString("alias", s.Unit.NumericType.TargetTypename);
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
                => _tc.Report("Aliases", "unable to build the XML model.", ex);
        }

        /// <summary>
        /// Translate <see cref="Definitions"/> into the Alias Statements file.
        /// </summary>
        /// <param name="definitions"><see cref="Unit"/> and <see cref="Scale"/> collections.</param>
        /// <param name="filepath">Path to the aliasing statements source code file.</param>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/> (error message sent to the logger).</returns>
        public bool ToFile(Definitions definitions, string filepath)
            => _template.Load() && Build(definitions) && ToFile(filepath);

        #endregion
    }
}
