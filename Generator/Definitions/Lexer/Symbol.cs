/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology
{
    internal partial class Lexer
    {
        #region Constants
        const char ATSIGN = '@';
        const char BACKSLASH = '\\';
        const char CARET = '^';
        const char COLON = ':';
        const char CR = '\r';
        const char DOT = '.';
        const char DBL_QUOT_MARK = '"';
        const char EQUAL = '=';
        const char GREATER_THAN = '>';
        const char LESS_THAN = '<';
        const char LF = '\n';
        const char LPAREN = '(';
        const char MINUS = '-';
        const char PIPE = '|';
        const char PLUS = '+';
        const char RPAREN = ')';
        const char SEMICOLON = ';';
        const char SLASH = '/';
        const char STAR = '*';
        //const char TAB          = '\t';
        const char UNDERSCORE = '_';
        #endregion

        /// <summary>
        /// Token symbol (id).
        /// </summary>
        internal enum Symbol
        {
            // Special
            EOF = -2,
            Error = -1,
            Unknown = 0,

            // Operators
            Minus = 1,          // '-'
            Plus = 2,           // '+'
            Times = 3,          // '*'
            Div = 4,            // '/'
            EQ = 5,             // '='
            LT = 6,             // '<'
            GT = 7,             // '>'
            Pipe = 8,           // '|'
            Wedge = 9,          // '^'

            // Brackets
            LParen = 13,        // '('
            RParen = 14,        // ')'
            Colon = 17,         // ':'
            Semicolon = 18,     // ';'

            // Non-terminals
            Whitespace = 30,    // Whitespace
            LineComment = 31,   // Comment
            BlockComment = 32,  // Comment
            Identifier = 33,    // Identifier
            IntNumber = 34,     // IntLiteral
            RealNumber = 35,    // RealLiteral
            StringLiteral = 36, // StringLiteral
        }
    }
}
