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
    /// <summary>
    /// Lexical analyser.
    /// </summary>
    internal partial class Lexer
    {
        #region Fields
        private readonly TokenReader _rdr;
        #endregion

        #region Properties
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Lexer"/> constructor.
        /// </summary>
        /// <param name="crd">Text stream character reader.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown by <see cref="TokenReader"/> constructor on bad arguments.</exception>
        /// <exception cref="IOException">Thrown by <see cref="TokenReader"/> constructor on I/O error.</exception>
        /// <exception cref="ObjectDisposedException">Thrown by <see cref="TokenReader"/> constructor on a disposed <paramref name="crd"/> reader.</exception>
        public Lexer(TextReader crd)
        {
            _rdr = new TokenReader(crd);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets next token from input stream.
        /// </summary>
        /// <returns>Next <see cref="Token"/> read from the input stream.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown by <see cref="TokenReader.OpenToken"/> method.</exception>
        /// <exception cref="ArgumentException">Thrown by <see cref="TokenReader.CloseToken"/> method(s).</exception>
        /// <exception cref="IOException">Thrown by <see cref="TokenReader.GetNextChar"/> method.</exception>
        /// <exception cref="ObjectDisposedException">Thrown by <see cref="TokenReader.GetNextChar"/> performed on a disposed <see cref="_rdr"/> reader.</exception>
        internal Token GetToken()
        {
            _rdr.OpenToken();

            if (_rdr.EOF)
                return _rdr.CloseToken(Symbol.EOF);

            Token? token;

            if ((token = TryGetWhitespace()) is not null)
                return token;

            if ((token = TryGetCommentOrDivision()) is not null)
                return token;

            if (char.IsLetter(_rdr.Char) || (_rdr.Char == UNDERSCORE))
                return GetIdentifier(nchars: 1);

            if (_rdr.Char == ATSIGN)
                return GetIdentifier(nchars: 0);

            // "0" .. "9"
            if (char.IsDigit(_rdr.Char))
                return GetNumeral();

            if (_rdr.Char == DBL_QUOT_MARK)
                return GetStringLiteral();

            if (_rdr.Char == LESS_THAN) return _rdr.CloseTokenEarly(Symbol.LT);
            if (_rdr.Char == GREATER_THAN) return _rdr.CloseTokenEarly(Symbol.GT);
            if (_rdr.Char == EQUAL) return _rdr.CloseTokenEarly(Symbol.EQ);
            if (_rdr.Char == LPAREN) return _rdr.CloseTokenEarly(Symbol.LParen);
            if (_rdr.Char == RPAREN) return _rdr.CloseTokenEarly(Symbol.RParen);
            if (_rdr.Char == PIPE) return _rdr.CloseTokenEarly(Symbol.Pipe);
            if (_rdr.Char == PLUS) return _rdr.CloseTokenEarly(Symbol.Plus);
            if (_rdr.Char == MINUS) return _rdr.CloseTokenEarly(Symbol.Minus);
            if (_rdr.Char == STAR) return _rdr.CloseTokenEarly(Symbol.Times);
            if (_rdr.Char == CARET) return _rdr.CloseTokenEarly(Symbol.Wedge);
            if (_rdr.Char == COLON) return _rdr.CloseTokenEarly(Symbol.Colon);
            if (_rdr.Char == SEMICOLON) return _rdr.CloseTokenEarly(Symbol.Semicolon);

            return _rdr.CloseTokenEarly(Symbol.Error, "unrecognized token");
        }

        private Token? TryGetWhitespace()
        {
            int nchars = 0;
            while (!_rdr.EOF && char.IsWhiteSpace(_rdr.Char))
            {
                nchars++;
                _rdr.GetNextChar();
            }
            return nchars > 0 ? _rdr.CloseToken(Symbol.Whitespace) : null;
        }

        // Comment Line = '//'
        // Comment Start = '/*'
        // Comment End = '*/'
        private Token? TryGetCommentOrDivision()
        {
            if (_rdr.Char == SLASH)
            {
                if (_rdr.GetNextChar())
                {
                    if (_rdr.Char == SLASH)
                    {
                        while (_rdr.GetNextChar() && (_rdr.Char != CR) && (_rdr.Char != LF))
                            ;
                        return _rdr.CloseToken(Symbol.LineComment);
                    }
                    else if (_rdr.Char == STAR)
                    {
                        bool foundStar = false;
                        while (_rdr.GetNextChar())
                        {
                            if (foundStar && (_rdr.Char == SLASH))
                            {
                                return _rdr.CloseTokenEarly(Symbol.BlockComment);
                            }
                            foundStar = (_rdr.Char == STAR);
                        }
                        return _rdr.CloseToken(Symbol.Error, "unexpected EOF while looking for characters \"*/\" closing block comment");
                    }
                }
                return _rdr.CloseToken(Symbol.Div);
            }
            return null;
        }

        // StringLiteral = '"'( {String Ch} | '\'{Printable} )* '"'
        private Token GetStringLiteral()
        {
            string unexpectedEOL = "unexpected EOL/EOF while looking for the quotation mark (\") closing string literal";
            while (_rdr.GetNextChar())
            {
                if (_rdr.Char == DBL_QUOT_MARK)
                {
                    return _rdr.CloseTokenEarly(Symbol.StringLiteral);
                }
                else if (_rdr.Char == BACKSLASH)
                {
                    _rdr.GetNextChar(); // read next char w/o checking what it is
                }
                else if (_rdr.Char is CR or LF)
                {
                    return _rdr.CloseToken(Symbol.Error, unexpectedEOL);
                }
            }
            return _rdr.CloseToken(Symbol.Error, unexpectedEOL);
        }

        // Identifier = [@]? {ID Head} {ID Tail}*   !The @ is an override char
        // {ID Head}  = {Letter} + [_]
        // {ID Tail}  = {Alphanumeric} + [_]
        private Token GetIdentifier(int nchars)
        {
            while (_rdr.GetNextChar() && IsValidChar(_rdr.Char))
                nchars++;

            return (nchars > 0) ?
                _rdr.CloseToken(Symbol.Identifier) :
                _rdr.CloseToken(Symbol.Error, "invalid identifier");

            bool IsValidChar(char c)
                => ((nchars > 0) ? char.IsLetterOrDigit(c) : char.IsLetter(c)) || (c == UNDERSCORE);
        }

        // <Positive Number> ::= IntLiteral | RealLiteral
        // IntLiteral    = {Digit}+
        // RealLiteral   = {Digit}+'.'{Digit}+(('e'|'E')('+'|'-')*{Digit}+)*

        // NOTE: The parser expects numbers that conform (syntactically) to the
        // Microsoft C# language specification (see "Integer literals" and "Real literals"
        // at https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/lexical-structure#6454-real-literals).
        private Token GetNumeral()
        {
            string invalidNumeral =
                "invalid number (not of the form: \"{digit}+ | {digit}+'.'{digit}+(('e'|'E')('+'|'-')*{digit}+)*\")";

            // {Digit}+ ...
            GetDigits();

            // {Digit}+'.' ...
            bool foundReal = (_rdr.Char == DOT);
            if (foundReal)
            {
                _rdr.GetNextChar(); // '.' --> token
                // {Digit}*'.'{Digit}+ ...
                if (GetDigits() <= 0)
                {
                    return _rdr.CloseToken(Symbol.Error, invalidNumeral);
                }
            }

            // {Digit}+'.'{Digit}+(('e'|'E') ...)*
            if (_rdr.Char is 'e' or 'E')
            {
                _rdr.GetNextChar(); // 'e|E' --> token

                // {Digit}+'.'{Digit}+(('e'|'E')('+'|'-')* ...)*
                if (_rdr.Char is PLUS or MINUS)
                {
                    _rdr.GetNextChar();
                }

                // {Digit}+'.'{Digit}+(('e'|'E')('+'|'-')*{Digit}+)*...
                if (GetDigits() <= 0)
                {
                    return _rdr.CloseToken(Symbol.Error, invalidNumeral);
                }
            }

            return _rdr.CloseToken(foundReal ? Symbol.RealNumber : Symbol.IntNumber);

            // digits --> token
            int GetDigits()
            {
                int digits = 0;
                while (!_rdr.EOF && char.IsDigit(_rdr.Char))
                {
                    digits++;
                    _rdr.GetNextChar();
                }
                return digits;
            }
        }
        #endregion
    }
}
