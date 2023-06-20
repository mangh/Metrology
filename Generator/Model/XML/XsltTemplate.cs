/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Security;
using System.Xml;
using System.Xml.Xsl;

namespace Mangh.Metrology.XML
{
    /// <summary>
    /// XSLT template wrapper.
    /// </summary>
    public class XsltTemplate : TextFile
    {
        #region Fields
        /// <summary>
        /// Translation settings.
        /// </summary>
        private readonly TranslationContext _tc;

        /// <summary>
        /// XSLT Template Processor.
        /// </summary>
        public readonly XslCompiledTransform _xslt;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="XsltTemplate"/> constructor.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="tc">XML model translation context.</param>
        /// <param name="useAsync">(<see langword="true"/>) asynchronous or (<see langword="false"/>) synchronous file reading mode.</param>
        public XsltTemplate(string path, TranslationContext tc, bool useAsync = false)
            : base(path, tc, useAsync)
        {
            _tc = tc;
            _xslt = new XslCompiledTransform();
        }
        #endregion

        #region Methods

        ///////////////////////////////////////////////////////////////////////
        //
        //      Load 
        //
        //

        /// <summary>
        /// Loads XSLT template from a byte <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">XSLT template byte <see cref="Stream"/>.</param>
        /// <returns>
        /// <see langword="true"/> on successful load, otherwise <see langword="false"/>
        /// (error message sent to the logger).
        /// </returns>
        public override bool Load(Stream stream)
        {
            string errorMessage = "template stream load error.";

            try
            {
                using XmlReader reader = XmlReader.Create(stream, _tc.XmlReaderSettings, baseUri: Path);
                _xslt.Load(reader, _tc.XsltSettings, _tc.XmlResolver);
                return true;
            }
            catch (SecurityException ex) { _tc.Report(Path, errorMessage, ex); }
            catch (XmlException ex) { ReportXmlXsltException(ex.LineNumber, ex.LinePosition, ex, errorMessage); }
            catch (XsltException ex) { ReportXmlXsltException(ex.LineNumber, ex.LinePosition, ex, errorMessage); }

            return false;
        }

        ///////////////////////////////////////////////////////////////////////
        //
        //      Transform
        //
        //

        /// <summary>
        /// Transforms XML model <see cref="Stream"/> into target language source code <see cref="Stream"/>.
        /// </summary>
        /// <param name="model">XML model <see cref="Stream"/> (transformation input).</param>
        /// <param name="sourceCode">Target source code <see cref="Stream"/> (transformation output).</param>
        /// <param name="sourcePath">File path assigned to the <paramref name="sourceCode"/> stream (for reporting transformation errors).</param>
        /// <returns>
        /// <see langword="true"/> on successful transformation, otherwise <see langword="false"/>
        /// (error message sent to the logger).
        /// </returns>
        public bool ToStream(Stream model, Stream sourceCode, string sourcePath)
        {
            try
            {
                model.Position = 0;    // go back to the beginning of the model stream
                using XmlReader reader = XmlReader.Create(model);
                using StreamWriter writer = new(sourceCode, _encoding, _bufferSize, leaveOpen: true);
                _xslt.Transform(reader, null, writer);
                writer.Flush();
                return true;
            }
            catch (ArgumentException ex)
            {
                // reader, writer, stream or encoding is null.
                // stream is not writable.
                // bufferSize is negative (ArgumentOutOfRangeException)
                ReportException(ex, ErrorMessage());
            }
            catch (IOException ex)
            {
                // I/O error occurs (Position = 0).
                ReportException(ex, ErrorMessage());
            }
            catch (NotSupportedException ex)
            {
                // The stream does not support seeking (to Position = 0).
                ReportException(ex, ErrorMessage());
            }
            catch (ObjectDisposedException ex)
            {
                // Methods were called after the stream was closed (Position = 0).
                ReportException(ex, ErrorMessage());
            }
            catch (SecurityException ex)
            {
                // The XmlReader does not have sufficient permissions to access the location of the XML data.
                ReportException(ex, ErrorMessage());
            }
            catch (XmlException ex)
            {
                // XML model read error (i.e. model is not valid XML).
                ReportXmlXsltException(ex.LineNumber, ex.LinePosition, ex, ErrorMessage());
            }
            catch (XsltException ex)
            {
                // Error executing the XSLT transform.
                ReportXmlXsltException(ex.LineNumber, ex.LinePosition, ex, ErrorMessage());
            }

            return false;

            string ErrorMessage()
                => $"unable to transform XML model into source code stream \"{sourcePath}\".";
        }

        ///////////////////////////////////////////////////////////////////////
        //
        //      Error logging
        //
        //

        /// <summary>
        /// Reports XSLT load/transform exception.
        /// </summary>
        /// <param name="ex">Exception thrown.</param>
        /// <param name="message">Error message.</param>
        private void ReportException(Exception ex, string message)
            => _tc.Report(Path, message, ex);

        /// <summary>
        /// Reports XML/XSLT error.
        /// </summary>
        /// <param name="line">Error line position.</param>
        /// <param name="col">Error column (character) position.</param>
        /// <param name="ex">Exception thrown.</param>
        /// <param name="message">Error message.</param>
        private void ReportXmlXsltException(int line, int col, Exception ex, string message)
        {
            LinePosition position = new(line, col);
            _tc.Report(Path, new TextSpan(0, 0), new LinePositionSpan(position, position), message, ex);
        }
        #endregion
    }
}
