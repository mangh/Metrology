/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.IO;

namespace Mangh.Metrology
{
    internal partial class Lexer
    {
        /// <summary>
        /// Character reader.
        /// </summary>
        internal class CharReader
        {
            #region Constants
            protected const int LINE_NUMBER_OFFSET = 1; // index of first line in the stream
            protected const int LINE_COLUMN_OFFSET = 1; // index of first character (column) in the line
            #endregion

            #region Fields
            private readonly TextReader _crd;

            // last character read (not copied into the token buffer yet)
            protected char _char;       // the character
            protected int _line;        // line number
            protected int _column;      // column (character) number
            protected int _position;    // character position relative to the beginning of the stream

            protected bool _eof;        // end-of-file flag
            #endregion

            #region Constructor(s)
            /// <summary>
            /// <see cref="CharReader"/> constructor.
            /// </summary>
            /// <param name="crd">Text stream character reader.</param>
            /// <exception cref="IOException">On <see cref="ReadNextChar"/> I/O error.</exception>
            /// <exception cref="ObjectDisposedException">On <see cref="ReadNextChar"/> operation performed on a disposed reader.</exception>
            protected CharReader(TextReader crd)
            {
                _crd = crd;
                _char = '\0';   // any character != LF

                ReadNextChar();

                _line = LINE_NUMBER_OFFSET;
                _column = LINE_COLUMN_OFFSET;
                _position = 0;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Read next character.
            /// </summary>
            /// <returns>
            /// "<c>true</c>" when the next character has been read; otherwise "<c>false</c>" (at the end-of-file).
            /// </returns>
            /// <exception cref="IOException">on <see cref="TextReader.Read()"/> I/O error.</exception>
            /// <exception cref="ObjectDisposedException">on <see cref="TextReader.Read()"/> performed on a disposed reader.</exception>
            protected bool ReadNextChar()
            {
                int c = _crd.Read();
                _eof = c == -1;
                if (!_eof)
                {
                    ++_position;
                    if (_char == LF)
                    {
                        ++_line;
                        _column = LINE_COLUMN_OFFSET;
                    }
                    else
                    {
                        ++_column;
                    }
                    _char = (char)c;
                }
                return !_eof;
            }
            #endregion
        }
    }
}
