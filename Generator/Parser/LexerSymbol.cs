/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Man.Metrology
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public partial class Lexer
    {
        /// <summary>
        /// Token Id
        /// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public enum Symbol
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
            Whitespace = 30,    // Comment
            LineComment = 31,   // Comment
            BlockComment = 32,  // Comment
            Identifier = 33,    // Identifier
            IntNumber = 34,     // IntLiteral
            RealNumber = 35,    // RealLiteral
            StringLiteral = 36, // StringLiteral
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
