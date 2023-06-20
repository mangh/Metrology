/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;

namespace Mangh.Metrology
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public partial class Parser
    {
        #region Methods
        // <Scale> ::= 'scale' Identifier <Format> Identifier '=' Identifier <Num Expr> ';'
        //      1st identifier = scale identifier, 
        //      2nd identifier = ref.point identifier, 
        //      3rd identifier = unit identifier, 
        private bool ParseScale()
        {
            GetToken();

            // Identifier (scale name)
            if (!GetEntityName("scale")) return false;

            // Format
            string? format = GetFormat(defaultFormat: string.Empty /* means: if the format is not specified take that of unit */);
            if (format is null) return false;

            // Identifier (refpoint)
            string? refpoint = GetReferencePoint();
            if (refpoint is null) return false;

            // "="
            if (_token.Symbol != Lexer.Symbol.EQ)
            {
                Notify("{0}: found \"{1}\" while expected equal sign \"=\".", _entityName, _token.Text);
                return false;
            }
            GetToken();

            // Identifier (unit)
            Unit? unit = GetScaleUnit(refpoint);
            if (unit is null) return false;

            // Offset encoder: the same as for the underlying unit
            IExprBuilder? exprBuilder = GetExprBuilder(unit.NumericType.SourceKeyword);
            if (exprBuilder is null) return false;

            // Scale offset <Num Expr>
            ASTNode? offsetExpr = GetNumExpr();
            if (offsetExpr is null) return false;

            // Encode offset expression
            (DimExpr? _, NumExpr? offset) = BuildExpression(offsetExpr, exprBuilder);
            if (offset is null) return false;

            // create scale
            Scale scale = new(
                _entityName,
                unit.NumericType,
                refpoint,
                unit,
                offset,
                string.IsNullOrWhiteSpace(format) ? unit.Format : format
            );

            // Terminating semicolon ";"
            bool done = GetSemicolon();
            if (done)
            {
                // Join the list of relatives
                GetRelativeScale(scale)?.AddRelative(scale);

                // Set family id
                _defs.AssignFamilyTo(scale);

                // Add to the results
                _defs.Scales.Add(scale);
            }
            return done;
        }

        private string? GetReferencePoint()
        {
            if (_token.Symbol == Lexer.Symbol.EQ)
                return string.Empty;

            if (_token.Symbol == Lexer.Symbol.Identifier)
            {
                string refpoint = _token.Text;
                GetToken();
                return refpoint;
            }
            Notify("{0}: found \"{1}\" while expected a name of reference-point or equal sign \"=\".", _entityName, _token.Text);
            return null;
        }

        private Unit? GetScaleUnit(string refpoint)
        {
            Unit? unit;
            Scale? twin;
            if (_token.Symbol != Lexer.Symbol.Identifier)
            {
                Notify("{0}: found \"{1}\" while expected unit name.", _entityName, _token.Text);
            }
            else if ((unit = FindUnit(_token.Text)) is null)
            {
                Notify("{0}: undefined unit \"{1}\".", _entityName, _token.Text);
            }
            else if ((twin = FindScale(refpoint, unit)) is not null)
            {
                Notify("{0}: same unit {1} as in scale {2} (ambiguous unit-to-scale mapping).",
                    _entityName, _token.Text, twin.TargetKeyword
                );
            }
            else
            {
                GetToken();
                return unit;
            }
            return null;

        }

        //<Num Expr> ::= <Num Expr> '+' <Num Term>
        //            |  <Num Expr> '-' <Num Term>
        //            |  <Num Term>
        private ASTNode? GetNumExpr()
        {
            ASTNode? lhs = GetNumTerm();
            if (lhs is null)
                return null;

            while ((_token.Symbol == Lexer.Symbol.Plus) || (_token.Symbol == Lexer.Symbol.Minus))
            {
                Lexer.Symbol operation = _token.Symbol;

                GetToken();

                ASTNode? rhs = GetNumTerm();
                if (rhs is null)
                    return null;
                else if (operation == Lexer.Symbol.Plus)
                    lhs = new ASTSum(lhs, rhs);
                else
                    lhs = new ASTDifference(lhs, rhs);
            }
            return lhs;
        }

        //<Num Term> ::= <Num Term> '*' <Num Unary>
        //            |  <Num Term> '/' <Num Unary>
        //            |  <Num Unary>
        private ASTNode? GetNumTerm()
        {
            ASTNode? lhs = GetNumUnary();

            if (lhs is null)
                return null;

            while ((_token.Symbol == Lexer.Symbol.Times) || (_token.Symbol == Lexer.Symbol.Div))
            {
                Lexer.Symbol operation = _token.Symbol;
                GetToken();

                ASTNode? rhs = GetNumUnary();
                if (rhs is null)
                    return null;
                else if (operation == Lexer.Symbol.Times)
                    lhs = new ASTProduct(lhs, rhs);
                else
                    lhs = new ASTQuotient(lhs, rhs);
            }
            return lhs;
        }

        //<Num Unary> ::= <Num Factor>
        //             | '+' <Num Unary>
        //             | '-' <Num Unary>
        private ASTNode? GetNumUnary()
        {
            if (_token.Symbol == Lexer.Symbol.Plus)
            {
                GetToken();
                ASTNode? factor = GetNumFactor();
                return (factor is null) ? null : new ASTUnary(true, factor);
            }
            else if (_token.Symbol == Lexer.Symbol.Minus)
            {
                GetToken();
                ASTNode? factor = GetNumFactor();
                return (factor is null) ? null : new ASTUnary(false, factor);
            }
            else
            {
                return GetNumFactor();
            }
        }

        // <Num Factor> ::= <Num Literal>
        //              | '(' <Num Expr> ')'
        //
        // <Num Literal> ::= IntLiteral | RealLiteral | StringLiteral  ! Member-access (e.g. "Math.PI")
        private ASTNode? GetNumFactor()
        {
            if (_token.Symbol == Lexer.Symbol.LParen)
            {
                GetToken();
                ASTNode? factor = GetNumExpr();
                if (factor is not null)
                {
                    if (_token.Symbol != Lexer.Symbol.RParen)
                    {
                        Notify("{0}: found \"{1}\" while expected expression in parentheses ().", _entityName, _token.Text);
                    }
                    else
                    {
                        GetToken();
                        return new ASTParenthesized(factor);
                    }
                }
            }
            else if ((_token.Symbol == Lexer.Symbol.IntNumber) || (_token.Symbol == Lexer.Symbol.RealNumber))
            {
                ASTNumber number = new(_token.Text);
                GetToken();
                return number;
            }
            else if (_token.Symbol == Lexer.Symbol.StringLiteral)
            {
                ASTLiteral literal = new(_token.Body);
                GetToken();
                return literal;
            }
            else
            {
                Notify("{0}: found \"{1}\" while expected numeric factor: number | (expression) | \"literal\".", _entityName, _token.Text);
            }
            return null;
        }

        private Scale? GetRelativeScale(Scale scale)
        {
            Measure prime = scale.Unit.Prime ?? scale.Unit;
            return _defs.Scales.Find(s =>
                ReferenceEquals(s.Unit.Prime ?? s.Unit, prime) &&
                string.Equals(s.RefPoint, scale.RefPoint, StringComparison.Ordinal)
            );
        }
        #endregion
    }
}
