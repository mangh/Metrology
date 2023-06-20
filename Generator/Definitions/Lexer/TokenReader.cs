/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Text;

namespace Mangh.Metrology
{
    internal partial class Lexer
    {
        /// <summary>
        /// Lexical analyser token reader.
        /// </summary>
        internal class TokenReader : CharReader
        {
            #region Fields
            private readonly StringBuilder _buf;    // token buffer
            private LinePosition _start;            // token start position
            private LinePosition _end;              // token end position
            private TextSpan _extent;               // token offset span
            #endregion

            #region Properties
            /// <summary>
            /// Last character read (not yet copied to the token buffer).
            /// </summary>
            public char Char => _char;

            /// <summary>
            /// End of file flag (end of input stream found).
            /// </summary>
            public bool EOF => _eof;
            #endregion

            #region Constructor(s)
            /// <summary>
            /// <see cref="TokenReader"/> constructor.
            /// </summary>
            /// <param name="crd">Text stream character reader.</param>
            /// <exception cref="ArgumentOutOfRangeException">When <see cref="LinePosition"/> or <see cref="StringBuilder"/> arguments are negative.</exception>
            /// <exception cref="IOException">On <see cref="CharReader.ReadNextChar"/> I/O error.</exception>
            /// <exception cref="ObjectDisposedException">On <see cref="CharReader.ReadNextChar"/> operation performed on a disposed reader.</exception>
            public TokenReader(TextReader crd)
                : base(crd)
            {
                _buf = new StringBuilder(512);
                _start = _end = new LinePosition(_line, _column);
            }
            #endregion

            #region Methods

            /// <summary>
            /// Appends last character read to the token buffer and read next character from the input stream.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> when the next character has been read; otherwise <see langword="false"/> (at the end-of-file).
            /// </returns>
            /// <exception cref="ArgumentOutOfRangeException">When <see cref="LinePosition"/> arguments are negative.</exception>
            /// <exception cref="IOException">On <see cref="CharReader.ReadNextChar"/> I/O error.</exception>
            /// <exception cref="ObjectDisposedException">On <see cref="CharReader.ReadNextChar"/> operation performed on a disposed reader.</exception>
            public bool GetNextChar()
            {
                _buf.Append(_char);
                _end = new LinePosition(_line, _column);
                return ReadNextChar();
            }

            /// <summary>
            /// Open a new <see cref="Token"/>.
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">When <see cref="LinePosition"/> arguments are negative.</exception>
            public void OpenToken()
            {
                _extent = new TextSpan(_position, 0);
                _start = new LinePosition(_line, _column);
                _buf.Clear();
            }

            /// <summary>
            /// Close the open <see cref="Token"/>.
            /// </summary>
            /// <param name="symbol"><see cref="Token"/>.<see cref="Symbol"/> (id).</param>
            /// <param name="error">Possible error message.</param>
            /// <returns><see cref="Token"/> extracted from the input stream.</returns>
            /// <exception cref="ArgumentException">When <c>end</c> precedes <c>start</c> in <see cref="LinePositionSpan"/> arguments.</exception>
            public Token CloseToken(Symbol symbol, string? error = null)
            {
                _extent = TextSpan.FromBounds(_extent.Start, _position);
                return new Token(symbol, _buf.ToString(), _extent, new LinePositionSpan(_start, _end), error);
            }

            /// <summary>
            /// Close the open <see cref="Token"/> early
            /// (appending the last character read that is still not in the buffer).
            /// </summary>
            /// <param name="symbol"><see cref="Token"/>.<see cref="Symbol"/> (id).</param>
            /// <param name="error">Possible error message.</param>
            /// <returns><see cref="Token"/> extracted from the input stream.</returns>
            /// <exception cref="ArgumentException">Thrown by <see cref="CloseToken"/> method.</exception>
            public Token CloseTokenEarly(Symbol symbol, string? error = null)
            {
                GetNextChar();
                return CloseToken(symbol, error);
            }
            #endregion
        }
    }
}
