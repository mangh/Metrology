/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Mangh.Metrology
{
    /// <summary>
    /// Definition parser.
    /// </summary>
    public partial class Parser
    {
        #region Constants
        private const string UNIT_KEYWORD = "unit";
        private const string SCALE_KEYWORD = "scale";
        #endregion

        #region Fields
        private readonly Definitions _defs;
        private readonly Lexer _lexer;

        /// <summary>
        /// Last token read.
        /// </summary>
        private Lexer.Token _token = Lexer.Token.Unknown;

        /// <summary>
        /// The name of the entity (<see cref="Unit"/> or <see cref="Scale"/>) being currently processed.
        /// </summary>
        private string _entityName = string.Empty;

        /// <summary>
        /// Number of errors found during parsing.
        /// </summary>
        private int _errorCount;
        #endregion

        #region Properties
        /// <summary>
        /// <see cref="Unit"/> and <see cref="Scale"/> collections (preloaded and those read from the definition stream).
        /// </summary>
        public Definitions Defs => _defs;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Parser"/> constructor.
        /// </summary>
        /// <param name="crd">Definition text stream character reader.</param>
        /// <param name="definitions">
        /// <see cref="Unit"/> and <see cref="Scale"/> collections<br/>(may be preloaded with compile-time entities built in the past).
        /// </param>
        /// <exception cref="ArgumentException">Thrown by <see cref="Lexer"/>.</exception>
        /// <exception cref="IOException">Thrown by <see cref="Lexer"/>.</exception>
        /// <exception cref="ObjectDisposedException">Thrown by <see cref="Lexer"/>.</exception>
        public Parser(TextReader crd, Definitions definitions)
        {
            _lexer = new Lexer(crd);
            _defs = definitions;

            GetToken();
        }

        /// <summary>
        /// <see cref="Parser"/> constructor.
        /// </summary>
        /// <param name="crd">Definition text stream character reader.</param>
        /// <param name="path">Path to the definitions file.</param>
        /// <param name="context">Translation context.</param>
        /// <exception cref="ArgumentException">Thrown by <see cref="Lexer"/>.</exception>
        /// <exception cref="IOException">Thrown by <see cref="Lexer"/>.</exception>
        /// <exception cref="ObjectDisposedException">Thrown by <see cref="Lexer"/>.</exception>
        public Parser(TextReader crd, string path, TranslationContext context)
            : this(crd, new Definitions(path, context))
        {
        }
        #endregion

        #region Methods

        // <UnitsOfMeasure> ::=  <UnitsOfMeasure> <Entity> | <Entity>
        // <Entity> ::= <Unit> | <Scale>
        // <Unit> ::= 'unit'<ValueType> Identifier <Tags> <Format> '=' <Dim Expr> ';'
        // <Scale> ::= 'scale' Identifier <Format> Identifier '=' Identifier <Num Expr> ';'

        /// <summary>
        /// Parse <see cref="Definitions"/> stream.
        /// </summary>
        /// <param name="ct">Notification/request to abort parsing.</param>
        /// <returns><see langword="true"/> on successful parsing (no errors found), otherwise <see langword="true"/>.</returns>
        /// <exception cref="ArgumentException">Thrown by <see cref="Lexer"/>.</exception>
        /// <exception cref="InvalidOperationException">Fatal (unrecoverable) <see cref="Parser"/> error.</exception>
        /// <exception cref="IOException">Thrown by <see cref="Lexer"/>.</exception>
        /// <exception cref="NotSupportedException">Attempt to use an unsupported <see cref="Numeric{T}"/> type.</exception>
        /// <exception cref="ObjectDisposedException">Thrown by <see cref="Lexer"/>.</exception>
        public bool Parse(CancellationToken ct)
        {
            _errorCount = 0;

            while (!ct.IsCancellationRequested && (_token.Symbol != Lexer.Symbol.EOF))
            {
                if (_token.Symbol != Lexer.Symbol.Identifier)
                {
                    ReportInvalidToken();
                }
                else if (_token.Text == UNIT_KEYWORD)
                {
                    if (ParseUnit()) continue;
                }
                else if (_token.Text == SCALE_KEYWORD)
                {
                    if (ParseScale()) continue;
                }
                else
                {
                    ReportInvalidToken();
                }
                Synchronize();
            }

            return _errorCount == 0;

            void ReportInvalidToken() =>
                Notify($"found \"{{0}}\" while expected \"{UNIT_KEYWORD}\" or \"{SCALE_KEYWORD}\" keyword.", _token.Text);
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //      Methods common to units and scales
        //
        //

        /// <summary>
        /// Retrieves the next (meaningful) token from the input stream.
        /// </summary>
        /// <returns><see cref="Lexer.Symbol"/> of the token read from the input stream and stored in the <see cref="_token"/> field.</returns>
        /// <exception cref="ArgumentException">Thrown by <see cref="Lexer"/>.</exception>
        /// <exception cref="IOException">Thrown by <see cref="Lexer"/>.</exception>
        /// <exception cref="ObjectDisposedException">Thrown by <see cref="Lexer"/>.</exception>
        private Lexer.Symbol GetToken()
        {
            do
            {
                _token = _lexer.GetToken();
            }
            while (_token.Symbol is Lexer.Symbol.Whitespace
                                 or Lexer.Symbol.LineComment
                                 or Lexer.Symbol.BlockComment);
            return _token.Symbol;
        }

        private bool GetEntityName(string entityType)
        {
            if (_token.Symbol != Lexer.Symbol.Identifier)
            {
                Notify("found \"{0}\" while expected {1} name.", _token.Text, entityType);
            }
            else if ((FindUnit(_token.Text) is null) && (FindScale(_token.Text) is null))
            {
                _entityName = _token.Text;
                GetToken();
                return true;
            }
            else
            {
                Notify("{0}: redefinition is not allowed (units/scales must have unique names).", _token.Text);
            }
            return false;
        }

        // <Format> ::= ':' StringLiteral
        //           |  ! No format -> default format: "{0} {1}" ("value symbol" e.g. "100 mph")
        private string? GetFormat(string defaultFormat)
        {
            string? format = null;
            if (_token.Symbol != Lexer.Symbol.Colon)
            {
                format = defaultFormat;
            }
            else if (GetToken() != Lexer.Symbol.StringLiteral)
            {
                Notify("{0}: found \"{1}\", while a format string (e.g. \"{{0}} {{1}}\" or \"%f %s\") was expected.", _entityName, _token.Text);
            }
            else if (string.IsNullOrWhiteSpace(format = _token.Body))
            {
                Notify("{0}: empty format string.", _entityName);
                format = null;
            }
            else
            {
                GetToken();
            }
            return format;
        }

        private bool GetSemicolon()
        {
            bool done = (_token.Symbol == Lexer.Symbol.Semicolon);
            if (done)
            {
                GetToken();
            }
            else
            {
                Notify("{0}: found \"{1}\", while a definition terminated with semicolon (\";\") was expected.", _entityName, _token.Text);
            }
            return done;
        }

        private void Synchronize()
        {
            // Report an error (if it has occurred):
            if (_token.Symbol == Lexer.Symbol.Error)
            {
                Notify("'{0}': {1}.", _token.Text, _token.Error!);
                GetToken();
            }

            // Skip everything until the next token, which allows the analysis to resume:
            while (_token.Symbol != Lexer.Symbol.EOF)
            {
                if (_token.Symbol == Lexer.Symbol.Semicolon)
                {
                    GetToken();
                    break;
                }
                else if ((_token.Symbol == Lexer.Symbol.Identifier) && ((_token.Text == UNIT_KEYWORD) || (_token.Text == SCALE_KEYWORD)))
                {
                    break;
                }
                GetToken();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //      Encoding AST nodes
        //
        //

        /// <summary>
        /// Returns <see cref="IExprBuilder"/> for the given numeric type source keyword.
        /// </summary>
        /// <param name="sourceKeyword">Source keyword for a <see cref="Numeric{T}"/> type.</param>
        private IExprBuilder? GetExprBuilder(string sourceKeyword) =>
            _defs.Context.Language.ExprBuilders.FirstOrDefault(b => b.Agent.NumericTerm.SourceKeyword == sourceKeyword);

        private (DimExpr?, NumExpr?) BuildExpression(ASTNode node, IExprBuilder exprBuilder)
        {
            // There is a small risk that multiplying/dividing dimensions (within
            // the expression builder) may overflow. It would be good to show the
            // exact location of the issue:
            try
            {
                return exprBuilder.Encode(node);
            }
            catch (OverflowException ex)
            {
                Notify(ex, "{0}: overflow exception while encoding {1}", _entityName, node);
            }
            return (null, null);
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //      Search for units and scales (among those already enlisted)
        //
        //

        private Unit? FindUnit(string name) => _defs.Units.Find(u => u.TargetKeyword == name);
        private Scale? FindScale(string name) => _defs.Scales.Find(s => s.TargetKeyword == name);
        private Scale? FindScale(string refpoint, Unit unit) => _defs.Scales.Find(s => (s.RefPoint == refpoint) && (s.Unit == unit));


        ///////////////////////////////////////////////////////////////////////////////////////
        //
        //
        //      Error logging
        //
        //

        private void Notify(Exception? ex, Lexer.Token token, string format, params object[] arguments)
        {
            ++_errorCount;
            _defs.Context.Report(_defs.Path, token.Extent, token.Position, string.Format(format, arguments), ex);
        }

        private void Notify(Lexer.Token token, string format, params object[] arguments)
            => Notify(null, token, format, arguments);

        private void Notify(Exception? ex, string format, params object[] arguments)
            => Notify(ex, _token, format, arguments);

        private void Notify(string format, params object[] arguments)
            => Notify(null, _token, format, arguments);

        #endregion
    }
}
