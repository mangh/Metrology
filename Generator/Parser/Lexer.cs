/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.IO;

namespace Man.Metrology
{
    public partial class Lexer
    {
        #region Constants
        const char ATSIGN       = '@';
        const char BACKSLASH    = '\\';
        const char CARET        = '^';
        const char COLON        = ':';
        const char CR           = '\r';
        const char DOT          = '.';
        const char DBL_QUOT_MARK = '"';
        const char EQUAL        = '=';
        const char GREATER_THAN = '>';
        const char LESS_THAN    = '<';
        const char LF           = '\n';
        const char LPAREN       = '(';
        const char MINUS        = '-';
        const char PIPE         = '|';
        const char PLUS         = '+';
        const char RPAREN       = ')';
        const char SEMICOLON    = ';';
        const char SLASH        = '/';
        const char STAR         = '*';
        //const char TAB          = '\t';
        const char UNDERSCORE   = '_';
        #endregion

        #region Fields
        private readonly Scanner _scnr;
        #endregion

        #region Properties
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Creates lexer (lexical analyzer) for a definitions stream.
        /// </summary>
        /// <param name="input">definition reader</param>
        /// <exception cref="IOException">on an I/O error thrown in <see cref="Scanner"/> constructor.</exception>
        /// <exception cref="ObjectDisposedException">in <see cref="Scanner"/> constructor on a disposed <paramref name="input"/> reader.</exception>
        public Lexer(TextReader input)
        {
            _scnr = new Scanner(input);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets next token from input stream.
        /// </summary>
        /// <returns>Next <see cref="Token"/> read from the input stream.</returns>
        /// <exception cref="IOException">on <see cref="Scanner.GetNextChar"/> I/O error.</exception>
        /// <exception cref="ObjectDisposedException">on <see cref="Scanner.GetNextChar"/> performed on a disposed <see cref="_scnr"/> reader.</exception>
        public Token GetNextToken()
        {
            _scnr.OpenToken();

            if (_scnr.EOF)
                return _scnr.CloseToken(Symbol.EOF);

            Token? token;

            if ((token = TryGetWhitespace()) is not null)
                return token!;

            if ((token = TryGetCommentOrDivision()) is not null)
                return token!;

            if (char.IsLetter(_scnr.Char) || (_scnr.Char == UNDERSCORE))
                return GetIdentifier(nchars: 1);

            if (_scnr.Char == ATSIGN)
                return GetIdentifier(nchars: 0);

            // "0" .. "9", "."
            if (char.IsDigit(_scnr.Char) || (_scnr.Char == DOT))
                return GetNumeral();

            if (_scnr.Char == DBL_QUOT_MARK)
                return GetStringLiteral();

            if (_scnr.Char == LESS_THAN) return _scnr.CloseTokenAhead(Symbol.LT);
            if (_scnr.Char == GREATER_THAN)  return _scnr.CloseTokenAhead(Symbol.GT);
            if (_scnr.Char == EQUAL)     return _scnr.CloseTokenAhead(Symbol.EQ);
            if (_scnr.Char == LPAREN)    return _scnr.CloseTokenAhead(Symbol.LParen);
            if (_scnr.Char == RPAREN)    return _scnr.CloseTokenAhead(Symbol.RParen);
            if (_scnr.Char == PIPE)      return _scnr.CloseTokenAhead(Symbol.Pipe);
            if (_scnr.Char == PLUS)      return _scnr.CloseTokenAhead(Symbol.Plus);
            if (_scnr.Char == MINUS)     return _scnr.CloseTokenAhead(Symbol.Minus);
            if (_scnr.Char == STAR)      return _scnr.CloseTokenAhead(Symbol.Times);
            if (_scnr.Char == CARET)     return _scnr.CloseTokenAhead(Symbol.Wedge);
            if (_scnr.Char == COLON)     return _scnr.CloseTokenAhead(Symbol.Colon);
            if (_scnr.Char == SEMICOLON) return _scnr.CloseTokenAhead(Symbol.Semicolon);

            return _scnr.CloseTokenAhead(Symbol.Error, "unrecognized token");
        }

        private Token? TryGetWhitespace()
        {
            int nchars = 0;
            while (!_scnr.EOF && char.IsWhiteSpace(_scnr.Char))
            {
                nchars++;
                _scnr.GetNextChar();
            }
            return nchars > 0 ? _scnr.CloseToken(Symbol.Whitespace) : null;
        }

        // Comment Line = '//'
        // Comment Start = '/*'
        // Comment End = '*/'
        private Token? TryGetCommentOrDivision()
        {
            if (_scnr.Char == SLASH)
            {
                if (_scnr.GetNextChar())
                {
                    if (_scnr.Char == SLASH)
                    {
                        while (_scnr.GetNextChar() && (_scnr.Char != CR) && (_scnr.Char != LF))
                            ;
                        return _scnr.CloseToken(Symbol.LineComment);
                    }
                    else if (_scnr.Char == STAR)
                    {
                        bool foundStar = false;
                        while (_scnr.GetNextChar())
                        {
                            if (foundStar && (_scnr.Char == SLASH))
                            {
                                return _scnr.CloseTokenAhead(Symbol.BlockComment);
                            }
                            foundStar = (_scnr.Char == STAR);
                        }
                        return _scnr.CloseToken(Symbol.Error, "unexpected EOF while looking for characters \"*/\" closing block comment");
                    }
                }
                return _scnr.CloseToken(Symbol.Div);
            }
            return null;
        }

        // StringLiteral = '"'( {String Ch} | '\'{Printable} )* '"'
        private Token GetStringLiteral()
        {
            string unexpectedEOL = "unexpected EOL/EOF while looking for the quotation mark (\") closing string literal";
            while (_scnr.GetNextChar())
            {
                if (_scnr.Char == DBL_QUOT_MARK)
                {
                    return _scnr.CloseTokenAhead(Symbol.StringLiteral);
                }
                else if (_scnr.Char == BACKSLASH)
                {
                    _scnr.GetNextChar(); // read next char w/o checking what it is
                }
                else if (_scnr.Char is CR or LF)
                { 
                    return _scnr.CloseToken(Symbol.Error, unexpectedEOL);
                }
            }
            return _scnr.CloseToken(Symbol.Error, unexpectedEOL);
        }

        // Identifier = [@]? {ID Head} {ID Tail}*   !The @ is an override char
        // {ID Head}  = {Letter} + [_]
        // {ID Tail}  = {Alphanumeric} + [_]
        private Token GetIdentifier(int nchars)
        {
            while (_scnr.GetNextChar() && IsValidChar(_scnr.Char))
                nchars++;

            return (nchars > 0) ?
                _scnr.CloseToken(Symbol.Identifier) :
                _scnr.CloseToken(Symbol.Error, "invalid identifier");

            bool IsValidChar(char c)
                => ((nchars > 0) ? char.IsLetterOrDigit(c) : char.IsLetter(c)) || (c == UNDERSCORE);
        }

        // <Positive Number> ::= IntLiteral | RealLiteral
        // IntLiteral    = {Digit}+
        // RealLiteral   = {Digit}*'.'{Digit}+(('e'|'E')('+'|'-')*{Digit}+)*
        private Token GetNumeral()
        {
            string invalidNumeral = 
                "invalid numeral (not of the form: \"{{digit}}+ | {{digit}}*'.'{{digit}}+(('e'|'E')('+'|'-')*{{digit}}+)*\")";

            // {Digit}* ...
            int digits = GetDigits();

            // {Digit}*'.' ...
            if (_scnr.Char == DOT)
            {
                _scnr.GetNextChar(); // '.' --> token
                // {Digit}*'.'{Digit}+ ...
                if (GetDigits() <= 0)
                {
                    return _scnr.CloseToken(Symbol.Error, invalidNumeral);
                }

                // {Digit}*'.'{Digit}+(('e'|'E') ...)*
                if (_scnr.Char is 'e' or 'E')
                {
                    _scnr.GetNextChar(); // 'e|E' --> token
                    // {Digit}*'.'{Digit}+(('e'|'E')('+'|'-')* ...)*
                    if (_scnr.Char is PLUS or MINUS)
                    {
                        _scnr.GetNextChar();
                    }

                    // {Digit}*'.'{Digit}+(('e'|'E')('+'|'-')*{Digit}+)*...
                    if (GetDigits() <= 0)
                    {
                        return _scnr.CloseToken(Symbol.Error, invalidNumeral);
                    }
                }
                return _scnr.CloseToken(Symbol.RealNumber);
            }

            return (digits > 0) ?
                _scnr.CloseToken(Symbol.IntNumber) :
                _scnr.CloseToken(Symbol.Error, invalidNumeral);

            // digits --> token
            int GetDigits()
            {
                int digits = 0;
                while (!_scnr.EOF && char.IsDigit(_scnr.Char))
                {
                    digits++;
                    _scnr.GetNextChar();
                }
                return digits;
            }
        }
        #endregion
    }
}
