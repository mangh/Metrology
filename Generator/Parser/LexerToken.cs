/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

using Microsoft.CodeAnalysis.Text;

namespace Mangh.Metrology
{
    public partial class Lexer
    {
        /// <summary>
        /// Token (lexeme) found in definitions text stream
        /// </summary>
        public class Token
        {
            #region Statics
            /// <summary>
            /// Constant for an uninitialized token.
            /// </summary>
            public static readonly Token Unknown = new(Symbol.Unknown, string.Empty, new(0, 0), new(LinePosition.Zero, LinePosition.Zero));
            #endregion

            #region Properties
            /// <summary>Token symbol (id)</summary>
            public Symbol Symbol { get; }

            /// <summary>Token text.</summary>
            public string Text { get; }

            /// <summary>
            /// Token position span (in the input stream).
            /// </summary>
            public TextSpan Extent { get; }

            /// <summary>
            /// Token line position span (in the input stream).
            /// </summary>
            public LinePositionSpan Span { get; }

            /// <summary>Error message for a token that could not be recognized as a valid one.</summary>
            public string? Error { get; }

            /// <summary>
            /// Token body i.e.:
            /// <list type="bullet">
            /// <item><description><see cref="Text"/> without the surrounding quotation marks: for a <c>StringLiteral</c> token,</description></item>
            /// <item><description><see cref="Text"/>: for any other token.</description></item>
            /// </list>
            /// </summary>
#pragma warning disable IDE0057 // Use range operator
            public string Body => (Symbol != Symbol.StringLiteral) ? Text : Text.Substring(1, Text.Length - 2);
#pragma warning restore IDE0057 // Use range operator
            #endregion

            #region Constructor(s)
            /// <summary>
            /// Token basic constructor.
            /// </summary>
            /// <param name="symbol">token <see cref="Lexer.Symbol"/> id.</param>
            /// <param name="text">token text.</param>
            /// <param name="extent">token start position.</param>
            /// <param name="span">token end position.</param>
            /// <param name="error">error message for a token that could not be recognized as a valid one.</param>
            public Token(Symbol symbol, string text, TextSpan extent, LinePositionSpan span, string? error = null)
            {
                Symbol = symbol;
                Text = text;
                Extent = extent;
                Span = span;
                Error = error;
            }
            /// <summary>
            /// Token copy constructor
            /// </summary>
            /// <param name="original"></param>
            public Token(Token original)
            {
                Symbol = original.Symbol;
                Text = original.Text;
                Extent = original.Extent;
                Span = original.Span;
                Error = original.Error;
            }
            #endregion

            #region Formatting
            /// <summary>
            /// Token as a string.
            /// </summary>
            /// <returns>Stringified token.</returns>
            public override string ToString() =>
                (Symbol
                    is Symbol.Identifier
                    or Symbol.IntNumber
                    or Symbol.RealNumber
                    or Symbol.StringLiteral) ? $"{Symbol} ({Span}): {Text}" : $"{Symbol} ({Span})";
            #endregion
        }
    }
}
