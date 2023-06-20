/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Text;

namespace Mangh.Metrology
{
    /// <summary>
    /// Sequential read-only text file.
    /// </summary>
    public abstract class TextFile
    {
        #region Fields
        /// <summary>
        /// Path to the file<br/>
        /// (or, in the case of <see cref="TextFile"/> loaded from a string, any string to identify possible errors).
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// Translation context.
        /// </summary>
        public readonly TranslationContext Context;

        /// <summary>
        /// Options for creating a <see cref="FileStream"/> object.
        /// </summary>
        protected readonly FileOptions _fileOptions;

        /// <summary>
        /// Character encoding to use (default <see cref="Encoding.UTF8"/>).
        /// </summary>
        protected readonly Encoding _encoding;

        /// <summary>
        /// Read buffer size (default <c>4096</c>).
        /// </summary>
        protected readonly int _bufferSize;
        #endregion

        #region Properties
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="TextFile"/> constructor.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="context">Translation context.</param>
        /// <param name="useAsync">(<c>true</c>) asynchronous or (<c>false</c>) synchronous file reading mode.</param>
        /// <param name="encoding">Character encoding to use (UTF8 by default).</param>
        /// <param name="bufferSize">Read buffer size.</param>
        protected TextFile(string path, TranslationContext context, bool useAsync = false, int bufferSize = 4096, Encoding? encoding = null)
        {
            Path = path;
            Context = context;

            _bufferSize = bufferSize;
            _encoding = encoding ?? Encoding.UTF8;

            _fileOptions =
                (useAsync ? FileOptions.Asynchronous : FileOptions.None) |
                FileOptions.SequentialScan;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads textual data from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">Text stream.</param>
        /// <returns><see langword="true"/> on successful load, otherwise <see langword="false"/>.</returns>
        public abstract bool Load(Stream stream);

        /// <summary>
        /// Loads textual data from a file.
        /// </summary>
        /// <returns><see langword="true"/> on successful load, otherwise <see langword="false"/>.</returns>
        public bool Load()
        {
            try
            {
                using FileStream stream = new(Path, FileMode.Open, FileAccess.Read, FileShare.Read, _bufferSize, _fileOptions);
                return Load(stream);
            }
            catch (ArgumentException ex) { ReportError(ex); }
            catch (IOException ex) { ReportError(ex); }
            catch (NotSupportedException ex) { ReportError(ex); }
            catch (UnauthorizedAccessException ex) { ReportError(ex); }
            catch (System.Security.SecurityException ex) { ReportError(ex); }

            return false;

            void ReportError(Exception ex) =>
                Context.Report(Path, "file could not be read.", ex);
        }

        /// <summary>
        /// Loads textual data from a source string.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <returns><see langword="true"/> on successful load, otherwise <see langword="false"/>.</returns>
        public bool Load(string source)
        {
            try
            {
                using MemoryStream stream = new(16 * 1024);
                using (StreamWriter writer = new(stream, _encoding, _bufferSize, leaveOpen: true))
                {
                    writer.Write(source);
                    writer.Flush();
                }
                stream.Position = 0;
                return Load(stream);
            }
            catch (ArgumentException ex) { ReportError(ex); }
            catch (IOException ex) { ReportError(ex); }
            catch (NotSupportedException ex) { ReportError(ex); }
            catch (ObjectDisposedException ex) { ReportError(ex); }

            return false;

            void ReportError(Exception ex) =>
                Context.Report(Path, "unable to read.", ex);

        }
        #endregion
    }
}
