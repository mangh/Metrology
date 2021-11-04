/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Text;

namespace Man.Metrology
{
    public partial class Lexer
    {
        internal class Scanner
        {
            #region Fields
            private readonly TextReader _reader;

            // token data:
            private readonly StringBuilder _buf;    // token buffer
            private LinePosition _start;            // token start position
            private LinePosition _end;              // token end position
            private TextSpan _extent;               // token span

            // last character read (not copied into the token buffer yet)
            private char _char;     // the character
            private int _line;      // (zero-based) line number
            private int _column;    // (zero-based) column (character) number
            private int _offset;    // (zero-based) position relative to the beginning of the input stream
            #endregion

            #region Properties
            public char Char => _char;
            public bool EOF { get; private set; }
            #endregion

            #region Constructor(s)
            /// <summary>
            /// Creates lexer scanner.
            /// </summary>
            /// <param name="reader">definition text reader</param>
            /// <exception cref="IOException">on <see cref="ReadNextChar"/> I/O error.</exception>
            /// <exception cref="ObjectDisposedException">on <see cref="ReadNextChar"/> operation performed on a disposed reader.</exception>
            public Scanner(TextReader reader)
            {
                _reader = reader;
                _buf = new StringBuilder(1024);
                _char = '\0';   // any character != LF

                ReadNextChar();

                _line = 0;
                _column = 0;
                _offset = 0;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Read next input character.
            /// </summary>
            /// <exception cref="IOException">on <see cref="TextReader.Read()"/> I/O error.</exception>
            /// <exception cref="ObjectDisposedException">on <see cref="TextReader.Read()"/> performed on a disposed reader.</exception>
            /// <returns></returns>
            private bool ReadNextChar()
            {
                _end = new(_line, _column);

                int c = _reader.Read();
                if (EOF = c == -1)
                    return false;

                _offset += 1;

                if (_char == LF)
                {
                    _line += 1;
                    _column = 0;
                }
                else
                {
                    _column += 1;
                }

                _char = (char)c;

                return true;
            }

            /// <summary>
            /// Append last character read to the token buffer and read next character from the input stream.
            /// </summary>
            /// <returns>true on success, false on EOF.</returns>
            /// <exception cref="IOException">on <see cref="ReadNextChar"/> I/O error.</exception>
            /// <exception cref="ObjectDisposedException">on <see cref="ReadNextChar"/> operation performed on a disposed reader.</exception>
            public bool GetNextChar()
            {
                _buf.Append(_char);
                return ReadNextChar();
            }

            public void OpenToken()
            {
                _extent = new(_offset, 0);
                _start = new(_line, _column);
                _buf.Clear();
            }

            public Token CloseToken(Symbol symbol, string? error = null)
            {
                _extent = TextSpan.FromBounds(_extent.Start, _offset);
                return new(symbol, _buf.ToString(), _extent, new(_start, _end), error);
            }

            public Token CloseTokenAhead(Symbol symbol, string? error = null)
            {
                GetNextChar();
                return CloseToken(symbol, error);
            }
            #endregion
        }
    }
}
