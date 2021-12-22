/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Collections.Generic;

namespace Mangh.Metrology
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public partial class Parser
    {
        #region Methods

        // <Unit> ::= 'unit'<ValueType> Identifier <Tags> <Format> '=' <Dim Expr> ';'
        private bool ParseUnit()
        {
            GetNextToken();

            // optional <ValueType> (default "double")
            if (!GetUnitNumericType()) return false;

            // Identifier (unit name)
            string? unitName = GetEntityName("unit");
            if (unitName is null) return false;

            // Tags
            List<string>? tags = GetUnitTags(unitName);
            if (tags is null) return false;

            // Format
            string? format = GetFormat(unitName, "{0} {1}");
            if (format is null) return false;

            // Equal sign "="
            if (_token.Symbol == Lexer.Symbol.EQ)
                GetNextToken();
            else
            {
                Notify("{0}: found \"{1}\" while expected equal sign \"=\".", unitName, _token.Text);
                return false;
            }

            // <Dim Expr>
            UnitType? unit = GetDimExpr(unitName, format, tags);
            if (unit is null) return false;

            _units.Add(unit);

            // Semicolon ";"
            return GetSemicolon(unitName);
        }

        // <ValueType> ::= '<' <ValueTypeName> '>'
        //              | ! no value type -> default type "double"
        // 
        // <ValueTypeName> ::= double
        //                  | float
        //                  | decimal
        private bool GetUnitNumericType()
        {
            if (_token.Symbol != Lexer.Symbol.LT)
            {
                return SelectNumeralContext(NumeralDouble.Keyword);
            }
            else if ((GetNextToken() != Lexer.Symbol.Identifier) || !SelectNumeralContext(_token.Text))
            {
                Notify("found\"{0}\" while expected numeric type name: \"double\" | \"float\" | \"decimal\".", _token.Text);
            }
            else if (GetNextToken() == Lexer.Symbol.GT)
            {
                GetNextToken(); // prepare for next token
                return true;
            }
            else
            {
                Notify("found\"{0}\" while expected closing bracket \">\".", _token.Text);
            }
            return false;
        }

        // <Tags> ::= <Tags> StringLiteral
        //         | StringLiteral
        private List<string>? GetUnitTags(string unitName)
        {
            List<string> tags = new();
            while (_token.Symbol != Lexer.Symbol.EOF)
            {
                if (_token.Symbol != Lexer.Symbol.StringLiteral)
                    break;

                string tag = _token.Body;

                UnitType? other = _units.Find(u => u.Tags.Contains(tag));
                if (other is null)
                    tags.Add(tag);
                else
                    Notify("{0}: symbol \"{1}\" rejected as already used for {2} unit.", unitName, tag, other.Typename);

                GetNextToken();
            }
            if (tags.Count > 0) return tags;

            Notify("{0}: missing unit symbol(s).", unitName);

            return null;
        }

        // <Format> ::= ':' StringLiteral
        //           |  ! No format -> default format: "{0} {1}" ("value symbol" e.g. "100 mph")
        private string? GetFormat(string unitName, string defaultFormat)
        {
            string? format = null;
            if (_token.Symbol != Lexer.Symbol.Colon)
            {
                format = defaultFormat;
            }
            else if (GetNextToken() != Lexer.Symbol.StringLiteral)
            {
                Notify("{0}: found \"{1}\" while expected format string e.g. \"{{0}} {{1}}\".", unitName, _token.Text);
            }
            else if (string.IsNullOrWhiteSpace(format = _token.Body))
            {
                Notify("{0}: empty format string.", unitName);
                format = null;
            }
            else
            {
                GetNextToken();
            }
            return format;
        }

        //<Dim Expr> ::= <Dim Expr> '|' <Dim Term>
        //           |   <Dim Term>
        private UnitType? GetDimExpr(string unitName, string format, List<string> tags)
        {
            ASTNode? premier = GetDimTerm(unitName);
            if (premier is null) return null;

            DimensionalExpression? sense = EncodeDimension(unitName, premier);
            if (sense is null) return null;

            NumeralExpression? factor = EncodeFactor(unitName, premier);
            if (factor is null) return null;

            UnitType unit = new(unitName, sense, factor, _numExprEncoder!.One, _numExprEncoder.Zero, format, tags);

            premier.Normalized().Bind(unit);

            while (_token.Symbol == Lexer.Symbol.Pipe)
            {
                GetNextToken();

                ASTNode? alternate = GetDimTerm(unit.Typename);
                if (alternate is null) return null;

                if ((sense = EncodeDimension(unitName, alternate)) is null) return null;
                if (sense.Value != unit.Sense.Value)
                {
                    Notify("{0}: inconsistent dimensions: {1} == {2} != {3} == {4}.", unit.Typename, 
                        unit.Sense.Code, unit.Sense.Value, sense.Value, sense.Code
                    );
                    return null;
                }

                if ((factor = EncodeFactor(unitName, alternate)) is null) return null;
                if (factor.IsTrueValue && unit.Factor.IsTrueValue && (factor.Value != unit.Factor.Value))
                {
                    Notify("{0}: inconsistent conversion factors: {1} == {2} != {3} == {4}.", unit.Typename, 
                        unit.Factor.Code, unit.Factor.Value.ToPreciseString(), factor.Value.ToPreciseString(), factor.Code
                    );
                    return null;
                }

                alternate.Normalized().Bind(unit);
            }
            return unit;
        }


        //<Dim Term> ::= <Dim Term> '*' <Dim Factor>
        //            |  <Dim Term> '^' <Dim Factor>
        //            |  <Dim Term> '/' <Dim Factor>
        //            |  <Dim Factor>
        private ASTNode? GetDimTerm(string unitName)
        {
            ASTNode? lhs = GetDimFactor(unitName);
            if (lhs is null)
                return null;

            while ((_token.Symbol == Lexer.Symbol.Times) || (_token.Symbol == Lexer.Symbol.Div) || (_token.Symbol == Lexer.Symbol.Wedge))
            {
                Lexer.Token operation = new(_token);
                
                GetNextToken();

                ASTNode? rhs = GetDimFactor(unitName);
                if (rhs is null)
                {
                    return null;
                }
                else if (operation.Symbol == Lexer.Symbol.Div)
                {
                    lhs = new ASTQuotient(lhs, rhs);
                }
                else if (operation.Symbol == Lexer.Symbol.Times)
                {
                    lhs = new ASTProduct(lhs, rhs, iswedgeproduct: false);
                }
                else if (lhs.IsWedgeCompatible && rhs.IsWedgeCompatible)
                {
                    lhs = new ASTProduct(lhs, rhs, iswedgeproduct: true);
                }
                else
                {
                    string denote = "\u02C7{0}\u02C7";   //  ˇexprˇ
                    Notify(operation, 
                        "{0}: wedge-product {1} ^ {2} incompatible factor(s).",
                        unitName,
                        lhs.IsWedgeCompatible ? lhs.ToString() : string.Format(denote, lhs),
                        rhs.IsWedgeCompatible ? rhs.ToString() : string.Format(denote, rhs)
                    );
                    return null;
                }
            }
            return lhs;
        }

        //<Dim Factor> ::= '<' <Magnitude> '>'
        //             |    Identifier        ! Unit name (e.g. Meter)
        //             |    <Num Literal>
        //             |   '(' <Dim Term> ')'
        //
        // <Num Literal> ::= IntLiteral | RealLiteral | StringLiteral  ! Member-access (e.g. "Math.PI")
        private ASTNode? GetDimFactor(string unitName)
        {
            // '<' Magnitude '>'?
            if (_token.Symbol == Lexer.Symbol.LT)
            {
                GetNextToken();
                return GetDimMagnitude(unitName);
            }

            // '(' <Dim Term> ')'?
            else if (_token.Symbol == Lexer.Symbol.LParen)
            {
                GetNextToken();
                ASTNode? factor = GetDimTerm(unitName);
                if (factor is not null)
                {
                    if (_token.Symbol != Lexer.Symbol.RParen)
                    {
                        Notify("{0}: found \"{1}\" while expected closing parenthesis \")\".", unitName, _token.Text);
                    }
                    else
                    {
                        GetNextToken();
                        return new ASTParenthesized(factor);
                    }
                }
            }

            // <Num Literal>?
            else if ((_token.Symbol == Lexer.Symbol.IntNumber) || (_token.Symbol == Lexer.Symbol.RealNumber))
            {
                Numeral? number = _tryParse!(_token.Text);
                if (number is null)
                {
                    Notify("{0}: invalid number \"{1}\".", unitName, _token.Text);
                }
                else
                {
                    GetNextToken();
                    return new ASTNumber(number);
                }
            }

            // <String Literal> e.g. "System.Math.PI"?
            else if (_token.Symbol == Lexer.Symbol.StringLiteral)
            {
                ASTNode factor = new ASTLiteral(_token.Body, _eval!(_token.Body));
                GetNextToken();
                return factor;
            }

            // Unit identifier?
            else if (_token.Symbol == Lexer.Symbol.Identifier)
            {
                UnitType? u = FindUnit(_token.Text);
                if (u is null)
                {
                    Notify("{0}: undefined unit \"{1}\".", unitName, _token.Text);
                }
                else if (u.Factor.Value.Typename != _numTypeKeyword)
                {
                    Notify("{0}: unit \"{1}\" is of type <{2}> != <{3}>.", unitName, _token.Text, u.Factor.Value.Typename, _numTypeKeyword!);
                }
                else
                {
                    GetNextToken();
                    return new ASTUnit(u);
                }
            }

            else
            {
                Notify("{0}: found \"{1}\" while expected: <dimension> | unit | number | \"literal\" | (expression).", unitName, _token.Text);
            }

            return null;
        }

        //<Magnitude> ::= Length
        //             |  Time
        //             |  Mass
        //             |  Temperature
        //             |  ElectricCurrent
        //             |  AmountOfSubstance
        //             |  LuminousIntensity
        //             |  Other | Money ! Other and Money represent the same magnitude
        //             |    ! Dimensionless
        private ASTMagnitude? GetDimMagnitude(string unitName)
        {
            if (_token.Symbol == Lexer.Symbol.GT)
            {
                GetNextToken();
                return new ASTMagnitude();
            }

            if ((_token.Symbol != Lexer.Symbol.Identifier) || !Enum.TryParse(_token.Text, false, out Magnitude m))
            {
                Notify("{0}: found \"{1}\" while expected dimension keyword: <{2}> or <> for dimensionless unit.",
                    unitName, _token.Text, string.Join(">, <", Enum.GetNames(typeof(Magnitude))));
            }
            else if (GetNextToken() == Lexer.Symbol.GT)
            {
                GetNextToken();
                return new ASTMagnitude(m);
            }
            else
            {
                Notify("{0}: found \"{1}\" while expected closing bracket \">\".", unitName, _token.Text);
            }
            return null;
        }
        #endregion
    }
}
