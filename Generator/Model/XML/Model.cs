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
using System.Text;

namespace Mangh.Metrology.XML
{
    /// <summary>
    /// Base XML model.
    /// </summary>
    public abstract class Model : IDisposable
    {
        #region Fields
        /// <summary>
        /// XML model translation context.
        /// </summary>
        protected readonly TranslationContext _tc;

        /// <summary>
        /// Model buffer.
        /// </summary>
        protected readonly MemoryStream _model;

        /// <summary>
        /// XSLT template wrapper.
        /// </summary>
        protected readonly XsltTemplate _template;

        /// <summary>
        /// Options for creating a <see cref="FileStream"/> object.
        /// </summary>
        protected readonly FileOptions _fileOptions;

        /// <summary>
        /// Character encoding to use (default <see cref="Encoding.UTF8"/>).
        /// </summary>
        protected readonly Encoding _encoding;

        /// <summary>
        /// Write buffer size (default <c>4096</c>).
        /// </summary>
        protected readonly int _bufferSize;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Model"/> constructor.
        /// </summary>
        /// <param name="template">XSLT template.</param>
        /// <param name="tc">XML model translation context.</param>
        /// <param name="capacity">Initial size for the XML model (in bytes).</param>
        /// <param name="useAsync">(<c>true</c>) asynchronous or (<c>false</c>) synchronous file writing mode.</param>
        /// <param name="bufferSize">Write buffer size.</param>
        /// <param name="encoding">Character encoding to use (UTF8 by default).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> value &lt; 0.</exception>
        protected Model(XsltTemplate template,
                        TranslationContext tc,
                        int capacity,
                        bool useAsync,
                        int bufferSize = 4096,
                        Encoding? encoding = null)
        {
            _template = template;
            _tc = tc;

            _model = new MemoryStream(capacity);

            _bufferSize = bufferSize;
            _encoding = encoding ?? Encoding.UTF8;

            _fileOptions = (useAsync) ? FileOptions.Asynchronous : FileOptions.None;
        }

        /// <summary>
        /// <see cref="Model"/> constructor.
        /// </summary>
        /// <param name="templatePath">Path to the XSLT template file.</param>
        /// <param name="tc">XML model translation context.</param>
        /// <param name="capacity">Initial size for the XML model (in bytes).</param>
        /// <param name="useAsync">(<see langword="true"/>) asynchronous or (<see langword="false"/>) synchronous file writing mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> value &lt; 0.</exception>
        protected Model(string templatePath, TranslationContext tc, int capacity = 2048, bool useAsync = false)
            : this(new XsltTemplate(templatePath, tc, useAsync), tc, capacity, useAsync)
        {
        }

        /// <summary>
        /// Release model <see cref="MemoryStream"/>.
        /// </summary>
        public void Dispose() => _model.Dispose();
        #endregion

        #region Methods
        /// <summary>
        /// Transforms XML model <see cref="Stream"/> into target language source code file.
        /// </summary>
        /// <param name="sourcePath">Path to the target source code file (transformation output).</param>
        /// <returns>
        /// <see langword="true"/> on successful transformation, otherwise <see langword="false"/>
        /// (error message sent to the logger).
        /// </returns>
        public bool ToFile(string sourcePath)
        {
            if (DumpModel(sourcePath))
            {
                try
                {
                    using FileStream sourceCode = new(sourcePath, FileMode.Create, FileAccess.Write, FileShare.Read, _bufferSize, _fileOptions);
                    return _template.ToStream(_model, sourceCode, sourcePath);
                }
                catch (ArgumentException ex) { ReportException(ex); }
                catch (IOException ex) { ReportException(ex); }
                catch (NotSupportedException ex) { ReportException(ex); }
                catch (SecurityException ex) { ReportException(ex); }
                catch (UnauthorizedAccessException ex) { ReportException(ex); }
            }

            return false;

            void ReportException(Exception ex)
                => _tc.Report(sourcePath, "unable to transform XML model into the source code file.", ex);
        }

        /// <summary>
        /// Transforms XML model <see cref="Stream"/> into <see cref="SourceText"/> object.
        /// </summary>
        /// <param name="hintPath">An identifier to be used to reference the generated <see cref="SourceText"/>.</param>
        /// <returns>
        /// <see cref="SourceText"/> object on success transformation, otherwise <see langword="null"/>
        /// (error message sent to the logger).
        /// </returns>
        public SourceText? ToSourceText(string hintPath)
        {
            if (DumpModel(hintPath))
            {
                try
                {
                    using MemoryStream sourceCode = new(8 * 1024);
                    if (_template.ToStream(_model, sourceCode, hintPath))
                    {
                        // go back to the beginning of the source code stream
                        sourceCode.Position = 0;
                        SourceText sourceText = SourceText.From(sourceCode, _encoding, SourceHashAlgorithm.Sha256, throwIfBinaryDetected: false, canBeEmbedded: true);

                        // Dump the source code (optionally, depending on DumpOptions):
                        return DumpSourceCode(sourceText, hintPath) ? sourceText : null;
                    }
                }
                catch (ArgumentException ex) { ReportException(ex); }
                catch (InvalidDataException ex) { ReportException(ex); }
                catch (IOException ex) { ReportException(ex); }
                catch (ObjectDisposedException ex) { ReportException(ex); }
            }

            return null;

            void ReportException(Exception ex)
                => _tc.Report(hintPath, "unable to transform XML model into the SourceText object.", ex);
        }

        /// <summary>
        /// Dump XML model to a file.
        /// </summary>
        /// <param name="sourcePath">Path to the associated source file.</param>
        /// <returns><see langword="true"/> on successful dump, otherwise <see langword="false"/> (error message sent to the logger).</returns>
        protected bool DumpModel(string sourcePath)
        {
            if ((_tc.DumpOptions & DumpOption.Model) != DumpOption.Model)
                return true;

            string xmlPath = _tc.ModelFilePath(sourcePath);
            try
            {
                using FileStream xml = new(xmlPath, FileMode.Create);
                _model.Position = 0;
                _model.CopyTo(xml);
                xml.Flush();
                return true;
            }
            catch (Exception ex)
            {
                _tc.Report(xmlPath, "unable to dump XMl model.", ex);
            }
            return false;
        }

        /// <summary>
        /// Dump source text to a file.
        /// </summary>
        /// <param name="sourceText">Source text to save.</param>
        /// <param name="sourcePath">Path to the source text file.</param>
        /// <returns><see langword="true"/> on successful dump, otherwise <see langword="false"/> (error message sent to the logger).</returns>
        protected bool DumpSourceCode(SourceText sourceText, string sourcePath)
        {
            if ((_tc.DumpOptions & DumpOption.SourceCode) != DumpOption.SourceCode)
                return true;

            try
            {
                using FileStream cs = new(sourcePath, FileMode.Create);
                byte[] bytes = _encoding.GetBytes(sourceText.ToString());
                cs.Write(bytes, 0, bytes.Length);
                return true;
            }
            catch (Exception ex)
            {
                _tc.Report(sourcePath, "unable to dump source text.", ex);
            }
            return false;
        }
        #endregion
    }
}
