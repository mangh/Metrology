/*******************************************************************************

    Units of Measurement for C# applications

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
            GetNextToken();

            // Identifier (scale name)
            string? scaleName = GetEntityName("scale");
            if (scaleName is null) return false;

            // Format
            string? format = GetFormat(scaleName, string.Empty);
            if (format is null) return false;

            // Identifier (refpoint)
            string? refpoint = GetReferencePoint(scaleName);
            if (refpoint is null) return false;

            // "="
            if (_token.Symbol == Lexer.Symbol.EQ)
                GetNextToken();
            else
            {
                Notify("{0}: found \"{1}\" while expected equal sign \"=\".", scaleName, _token.Text);
                return false;
            }

            // Identifier (unit)
            UnitType? unit = GetScaleUnit(refpoint, scaleName);
            if (unit is null) return false;

            SelectNumeralContext(unit.Factor.Value.Typename);

            // Scale offset <Num Expr>
            ASTNode? offsetAST = GetNumExpr(scaleName);
            if (offsetAST is null) return false;

            // Encode offset expression
            NumeralExpression? offsetExpr = EncodeFactor(scaleName, offsetAST);
            if (offsetExpr is null) return false;

            // create scale
            ScaleType scale = new(
                scaleName, 
                refpoint, 
                unit, 
                offsetExpr, 
                string.IsNullOrWhiteSpace(format) ? unit.Format : format
            );

            // join family
            ScaleType? relative = GetRelativeScale(scale);
            if (relative is not null)
                relative.AddRelative(scale);

            // add to the list
            _scales.Add(scale);

            // Semicolon ";"
            return GetSemicolon(scaleName);
        }

        private string? GetReferencePoint(string scaleName)
        {
            if (_token.Symbol == Lexer.Symbol.EQ)
                return string.Empty;

            if (_token.Symbol == Lexer.Symbol.Identifier)
            {
                string refpoint = _token.Text;
                GetNextToken();
                return refpoint;
            }
            Notify("{0}: found \"{1}\" while expected reference-point name or equal sign \"=\".", scaleName, _token.Text);
            return null;
        }

        private UnitType? GetScaleUnit(string refpoint, string scaleName)
        {
            UnitType? unit;
            ScaleType? twin;
            if (_token.Symbol != Lexer.Symbol.Identifier)
            {
                Notify("{0}: found \"{1}\" while expected unit name.", scaleName, _token.Text);
            }
            else if ((unit = FindUnit(_token.Text)) is null)
            {
                Notify("{0}: undefined unit \"{1}\".", scaleName, _token.Text);
            }
            else if ((twin = FindScale(refpoint, unit)) is not null)
            {
                Notify("{0}: same unit {1} as in scale {2} (ambiguous unit-to-scale relationship).", scaleName, _token.Text, twin.Typename);
            }
            else
            {
                GetNextToken();
                return unit;
            }
            return null;
            
        }

        //<Num Expr> ::= <Num Expr> '+' <Num Term>
        //            |  <Num Expr> '-' <Num Term>
        //            |  <Num Term>
        private ASTNode? GetNumExpr(string scaleName)
        {
            ASTNode? lhs = GetNumTerm(scaleName);
            if (lhs is null)
                return null;

            while ((_token.Symbol == Lexer.Symbol.Plus) || (_token.Symbol == Lexer.Symbol.Minus))
            {
                Lexer.Symbol operation = _token.Symbol;

                GetNextToken();

                ASTNode? rhs = GetNumTerm(scaleName);
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
        private ASTNode? GetNumTerm(string scaleName)
        {
            ASTNode? lhs = GetNumUnary(scaleName);

            if (lhs is null)
                return null;

            while ((_token.Symbol == Lexer.Symbol.Times) || (_token.Symbol == Lexer.Symbol.Div))
            {
                Lexer.Symbol operation = _token.Symbol;
                GetNextToken();

                ASTNode? rhs = GetNumUnary(scaleName);
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
        private ASTNode? GetNumUnary(string scaleName)
        {
            if (_token.Symbol == Lexer.Symbol.Plus)
            {
                GetNextToken();
                ASTNode? factor = GetNumFactor(scaleName);
                return (factor is null) ? null : new ASTUnary(true, factor);
            }
            else if (_token.Symbol == Lexer.Symbol.Minus)
            {
                GetNextToken();
                ASTNode? factor = GetNumFactor(scaleName);
                return (factor is null) ? null : new ASTUnary(false, factor);
            }
            else
            {
                return GetNumFactor(scaleName);
            }
        }

        // <Num Factor> ::= <Num Literal>
        //              | '(' <Num Expr> ')'
        //
        // <Num Literal> ::= IntLiteral | RealLiteral | StringLiteral  ! Member-access (e.g. "Math.PI")
        private ASTNode? GetNumFactor(string scaleName)
        {
            if (_token.Symbol == Lexer.Symbol.LParen)
            {
                GetNextToken();
                ASTNode? factor = GetNumExpr(scaleName);
                if (factor is not null)
                {
                    if (_token.Symbol != Lexer.Symbol.RParen)
                    {
                        Notify("{0}: found \"{1}\" while expected closing parenthesis \")\".", scaleName, _token.Text);
                    }
                    else
                    {
                        GetNextToken();
                        return new ASTParenthesized(factor);
                    }
                }
            }
            else if ((_token.Symbol == Lexer.Symbol.IntNumber) || (_token.Symbol == Lexer.Symbol.RealNumber))
            {
                Numeral? number = _tryParse!(_token.Text);
                if (number is null)
                {
                    Notify("{0}: invalid number \"{1}\".", scaleName, _token.Text);
                }
                else
                {
                    GetNextToken();
                    return new ASTNumber(number);
                }
            }
            else if (_token.Symbol == Lexer.Symbol.StringLiteral)
            {
                string literal = _token.Body;
                GetNextToken();
                return new ASTLiteral(literal, _eval!(literal));
            }
            else
            {
                Notify("{0}: found \"{1}\" while expected numeric factor: number | (numeric expression) | \"stringliteral\".", scaleName, _token.Text);
            }
            return null;
        }

        private ScaleType? GetRelativeScale(ScaleType scale)
        {
            MeasureType? prime = scale.Unit.Prime ?? scale.Unit;
            return _scales.Find(s =>
                ReferenceEquals(s.Unit.Prime ?? s.Unit, prime) &&
                string.Equals(s.RefPoint, scale.RefPoint, StringComparison.Ordinal)
            );
        }
        #endregion
    }
}
