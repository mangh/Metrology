/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Mangh.Metrology
{
    using ErrorLogger = Action<TextSpan, LinePositionSpan, string>;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public partial class Parser
    {
        #region Fields
        private readonly Lexer _lexer;
        private readonly ErrorLogger _logger;

        private readonly List<UnitType> _units;
        private readonly List<ScaleType> _scales;

        private Lexer.Token _token;

        private string? _numTypeKeyword;
        private readonly IDimExprEncoder _dimExprEncoder;
        private INumExprEncoder? _numExprEncoder;
        private Func<string, Numeral?>? _tryParse;
        private Func<string, Numeral?>? _eval;
        #endregion

        #region Properties
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Creates parser for a definitions stream.
        /// </summary>
        /// <param name="lexer">lexical analyzer</param>
        /// <param name="logger">error logger (receiver)</param>
        /// <param name="units">output list of units found in definition stream (not necessarily empty).</param>
        /// <param name="scales">output list of scales found in definition stream (not necessarily empty).</param>
        /// <exception cref="IOException">on <see cref="GetNextToken"/> I/O error.</exception>
        /// <exception cref="ObjectDisposedException">on <see cref="GetNextToken()"/> performed on a disposed reader.</exception>
        public Parser(Lexer lexer, ErrorLogger logger, List<UnitType> units, List<ScaleType> scales)
        {
            _lexer = lexer;
            _logger = logger;

            _units = units;
            _scales = scales;

            // Encoder and numeric type being set for each unit/scale value type (not known yet):
            _numTypeKeyword = null;
            _tryParse = null;
            _eval = null;
            _numExprEncoder = null;
            _dimExprEncoder = new CS.DimExprEncoder();

            _token = Lexer.Token.Unknown;   // the assignment made only to quiet the compiler about uninitialized, null variable

            GetNextToken();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets next token from input stream.
        /// </summary>
        /// <returns><see cref="Lexer.Symbol"/> of the token read (and stored in the _token field).</returns>
        /// <exception cref="IOException">on <see cref="Lexer.GetNextToken"/> I/O error.</exception>
        /// <exception cref="ObjectDisposedException">on <see cref="Lexer.GetNextToken()"/> performed on a disposed reader object.</exception>
        private Lexer.Symbol GetNextToken()
        {
            do
            {
                _token = _lexer.GetNextToken();
            }
            while (_token.Symbol is
                Lexer.Symbol.Whitespace or
                Lexer.Symbol.LineComment or
                Lexer.Symbol.BlockComment);

            return _token.Symbol;
        }

        // <UnitsOfMeasure> ::=  <UnitsOfMeasure> <Entity> | <Entity>
        // <Entity> ::= <Unit> | <Scale>
        // <Unit> ::= 'unit'<ValueType> Identifier <Tags> <Format> '=' <Dim Expr> ';'
        // <Scale> ::= 'scale' Identifier <Format> Identifier '=' Identifier <Num Expr> ';'

        /// <summary>
        /// Parse unit/scale definitions fed in by the lexer.
        /// </summary>
        /// <param name="ct">propagates notification that parsing should be canceled.</param>
        /// <exception cref="InvalidOperationException">on an (unrecoverable) fatal error.</exception>
        /// <exception cref="IOException">on <see cref="GetNextToken"/> I/O error.</exception>
        /// <exception cref="ObjectDisposedException">on <see cref="GetNextToken()"/> performed on a disposed reader.</exception>
        public void Parse(CancellationToken ct)
        {
            string badTokenMessageFormat = "found \"{0}\" while expected \"unit\" or \"scale\" keyword.";

            while (!ct.IsCancellationRequested && (_token.Symbol != Lexer.Symbol.EOF))
            {
                if (_token.Symbol == Lexer.Symbol.Error)
                {
                    Notify("{0}: {1}.", _token.Text, _token.Error!);
                }
                else if (_token.Symbol != Lexer.Symbol.Identifier)
                {
                    Notify(badTokenMessageFormat, _token.Text);
                }
                else if (_token.Text == "unit")
                {
                    if (ParseUnit()) continue;
                }
                else if (_token.Text == "scale")
                {
                    if (ParseScale()) continue;
                }
                else
                {
                    Notify(badTokenMessageFormat, _token.Text);
                }
                Synchronize();
            }
        }

        private string? GetEntityName(string entityType)
        {
            if (_token.Symbol != Lexer.Symbol.Identifier)
            {
                Notify("found \"{0}\" while expected {1} name.", _token.Text, entityType);
            }
            else if ((FindUnit(_token.Text) is null) && (FindScale(_token.Text) is null))
            {
                string entityName = _token.Text;
                GetNextToken();
                return entityName;
            }
            else
            {
                Notify("{0}: duplicate definition (unit and scale names must be unique).", _token.Text);
            }
            return null;
        }

        private bool GetSemicolon(string entityName)
        {
            if (_token.Symbol == Lexer.Symbol.Semicolon)
            {
                GetNextToken();
                return true;
            }
            else if (_token.Symbol == Lexer.Symbol.Error)
            {
                Notify("{0}: {1}.", _token.Text, _token.Error!);
            }
            else
            {
                Notify("missing semicolon \";\" closing the \"{0}\" definition.", entityName);
            }
            return false;
        }

        private void Synchronize()
        {
            while (_token.Symbol != Lexer.Symbol.EOF)
            {
                if (_token.Symbol == Lexer.Symbol.Error)
                {
                    Notify("{0}: {1}.", _token.Text, _token.Error!);
                }
                else if (_token.Symbol == Lexer.Symbol.Semicolon)
                {
                    GetNextToken();
                    break;
                }
                else if ((_token.Symbol == Lexer.Symbol.Identifier) &&
                    ((_token.Text == "unit") || (_token.Text == "scale")))
                {
                    break;
                }
                GetNextToken();
            }
        }

        private bool SelectNumeralContext(string numTypeKeyword)
        {
            _numTypeKeyword = numTypeKeyword;

            if (numTypeKeyword == NumeralDouble.Keyword)
            {
                _eval = CS.Eval.Double;
                _tryParse = NumeralDouble.TryParse;
                _numExprEncoder = CS.NumExprEncoder.Double;
                return true;
            }
            else if (numTypeKeyword == NumeralDecimal.Keyword)
            {
                _eval = CS.Eval.Decimal;
                _tryParse = NumeralDecimal.TryParse;
                _numExprEncoder = CS.NumExprEncoder.Decimal;
                return true;
            }
            else if (numTypeKeyword == NumeralFloat.Keyword)
            {
                _eval = CS.Eval.Float;
                _tryParse = NumeralFloat.TryParse;
                _numExprEncoder = CS.NumExprEncoder.Float;
                return true;
            }
            return false;
        }

        private DimensionalExpression? EncodeDimension(string entityName, ASTNode node)
        {
            string errorFormat = "{0}: dimension could not be encoded :: {1}";

            try
            {
                return _dimExprEncoder.Accept(node);
            }
            catch (ArgumentException e)
            {
                Notify(errorFormat, entityName, e.Message);
            }
            catch (OverflowException e)
            {
                Notify(errorFormat, entityName, e.Message);
            }
            //catch (InvalidOperationException e)
            //{
            // THIS MEANS THE PARSER ALGORITHM IS PLAIN WRONG AND NOTHING REASONABLE CAN BE DONE.
            //}
            return null;
        }

        private NumeralExpression? EncodeFactor(string /*entityName*/_, ASTNode node)
        {
            //string errorFormat = "{0}: factor could not be encoded :: {1}";
            //try
            //{
            //    return _numExprEncoder!.Accept(node);
            //}
            //catch (InvalidOperationException e)
            //{
            //    THIS MEANS THE PARSER ALGORITHM IS PLAIN WRONG AND NOTHING REASONABLE CAN BE DONE.
            //}
            //return null;
            return _numExprEncoder!.Accept(node);
        }


        private UnitType? FindUnit(string name) => _units.Find(u => u.Typename == name);
        private ScaleType? FindScale(string name) => _scales.Find(s => s.Typename == name);
        private ScaleType? FindScale(string refpoint, UnitType unit) => _scales.Find(s => (s.RefPoint == refpoint) && (s.Unit == unit));

        private void Notify(Lexer.Token token, string format, params object[] arguments)
            => _logger(token.Extent, token.Span, string.Format(format, arguments));

        private void Notify(string format, params object[] arguments)
            => Notify(_token, format, arguments);
        #endregion
    }
}
