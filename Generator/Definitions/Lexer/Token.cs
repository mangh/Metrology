/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis.Text;

namespace Mangh.Metrology
{
    internal partial class Lexer
    {
        /// <summary>
        /// Token found in the definitions text stream.
        /// </summary>
        internal class Token
        {
            #region Statics
            /// <summary>
            /// Uninitialized/invalid token.
            /// </summary>
            public static readonly Token Unknown = new(
                Symbol.Unknown, 
                string.Empty, 
                new TextSpan(0, 0), 
                new LinePositionSpan(LinePosition.Zero, LinePosition.Zero)
            );
            #endregion

            #region Properties
            /// <summary>
            /// Token symbol (id)
            /// </summary>
            public Symbol Symbol { get; }

            /// <summary>
            /// Token text.
            /// </summary>
            public string Text { get; }

            /// <summary>
            /// Token text position (as an offset in the input stream).
            /// </summary>
            public TextSpan Extent { get; }

            /// <summary>
            /// Token text position (as a line &amp; character position in the input stream).
            /// </summary>
            public LinePositionSpan Position { get; }

            /// <summary>
            /// Error message for a token that could not be recognized as a valid one.
            /// </summary>
            public string? Error { get; }

            /// <summary>
            /// <see cref="Text"/> without the surrounding quotation marks for a <c><see cref="Symbol.StringLiteral"/></c> token
            /// --or--
            /// the entire <see cref="Text"/> for any other token.
            /// </summary>
            public string Body => Symbol == Symbol.StringLiteral ? Text.Substring(1, Text.Length - 2) : Text;
            #endregion

            #region Constructor(s)
            /// <summary>
            /// <see cref="Token"/> constructor.
            /// </summary>
            /// <param name="symbol">Token <see cref="Lexer.Symbol"/> (id).</param>
            /// <param name="text">Token text.</param>
            /// <param name="extent">Token text span.</param>
            /// <param name="position">Token position span.</param>
            /// <param name="error">Error message for a token that could not be recognized as a valid one.</param>
            public Token(Symbol symbol, string text, TextSpan extent, LinePositionSpan position, string? error = null)
            {
                Symbol = symbol;
                Text = text;
                Extent = extent;
                Position = position;
                Error = error;
            }
            /// <summary>
            /// <see cref="Token"/> copy constructor
            /// </summary>
            /// <param name="source">Source <see cref="Token"/> to be copied.</param>
            public Token(Token source)
                : this(source.Symbol, source.Text, source.Extent, source.Position, source.Error)
            {
            }
            #endregion

            #region Formatting
            /// <summary>
            /// Returns <see cref="Token"/> info in string form.
            /// </summary>
            public override string ToString() =>
                (Symbol
                    is Symbol.Identifier
                    or Symbol.IntNumber
                    or Symbol.RealNumber
                    or Symbol.StringLiteral) ? $"{Symbol} ({Position}): {Text}" : $"{Symbol} ({Position})";
            #endregion
        }
    }
}
