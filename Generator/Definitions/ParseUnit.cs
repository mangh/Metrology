/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Mangh.Metrology.Language;
using System;
using System.Collections.Generic;
using System.Linq;

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
            GetToken();

            // optional <ValueType> (default "double")
            IExprBuilder? exprBuilder = GetUnitNumericType();
            if (exprBuilder is null) return false;

            // Identifier (unit name)
            if (!GetEntityName("unit")) return false;

            // Tags
            List<string>? tags = GetUnitTags();
            if (tags is null) return false;

            // Format
            string? format = GetFormat(defaultFormat: _defs.Context.Language[Phrase.QUANTITY_FORMAT]);
            if (format is null) return false;

            // Equal sign "="
            if (_token.Symbol != Lexer.Symbol.EQ)
            {
                Notify("{0}: found \"{1}\" while expected equal sign \"=\".", _entityName, _token.Text);
                return false;
            }
            GetToken();

            // Primary expression
            //
            // <Dim Expr> ::= <Dim Expr> '|' <Dim Term>
            //           |   <Dim Term>
            ASTNode? primary = GetDimTerm(exprBuilder);
            if (primary is null) return false;

            (DimExpr? sense, NumExpr? factor) = BuildExpression(primary, exprBuilder);
            if (sense is null || factor is null) return false;

            // create unit
            Unit unit = new(
                _entityName,
                exprBuilder.Agent.NumericTerm,
                sense,
                factor,
                format,
                tags
            );

            // bind the unit to units used in the initial expression
            primary.Normalized().Bind(unit);

            // Secondary expression(s) may follow the primary one
            //
            // <Dim Expr> ::= <Dim Expr> '|' <Dim Term>
            //           |   <Dim Term>
            if (!GetSecondaryDimExpr(unit, exprBuilder)) return false;

            // Terminating semicolon ";"
            bool done = GetSemicolon();
            if (done)
            {
                // Set family id
                _defs.AssignFamilyTo(unit);

                // Add to the results
                _defs.Units.Add(unit);
            }

            return done;
        }

        // <ValueType> ::= '<' <ValueTypeName> '>'
        //              | ! no value type -> default type "double"
        // 
        // <ValueTypeName> ::= double
        //                  | float
        //                  | decimal
        private IExprBuilder? GetUnitNumericType()
        {
            IExprBuilder? exprBuilder;

            if (_token.Symbol != Lexer.Symbol.LT)
            {
                return _defs.Context.Language.ExprBuilders.First();
                // NOTE: the default numeric type is the one used by the first expression builder:
            }
            else if ((GetToken() != Lexer.Symbol.Identifier) || ((exprBuilder = GetExprBuilder(_token.Text)) is null))
            {
                Notify("found \"{0}\" while expected name of the numeric type: \"{1}\".", _token.Text,
                    string.Join("\", \"", _defs.Context.Language.NumericAgents.Select(a => a.NumericTerm.SourceKeyword))
                );
            }
            else if (GetToken() == Lexer.Symbol.GT)
            {
                GetToken();
                return exprBuilder;
            }
            else
            {
                Notify("found \"{0}\", while expected a numeric type name in angle brackets <>.", _token.Text);
            }
            return null;
        }

        // <Tags> ::= <Tags> StringLiteral
        //         | StringLiteral
        private List<string>? GetUnitTags()
        {
            List<string> tags = new();
            while (_token.Symbol != Lexer.Symbol.EOF)
            {
                if (_token.Symbol != Lexer.Symbol.StringLiteral)
                    break;

                string tag = _token.Body;
                if (string.IsNullOrWhiteSpace(tag))
                {
                    Notify("{0}: an empty string cannot be a unit symbol.", _entityName);
                }
                else
                {
                    Unit? other = _defs.Units.Find(u => u.Tags.Contains(tag));
                    if (other is null)
                    {
                        tags.Add(tag);
                    }
                    else
                    {
                        Notify("{0}: unit symbol \"{1}\" rejected (it has already been used in the {2} unit).", _entityName, tag, other.SourceKeyword);
                    }
                }
                GetToken();
            }

            if (tags.Count > 0) return tags;

            Notify("{0}: missing unit symbol(s).", _entityName);

            return null;
        }

        //<Dim Term> ::= <Dim Term> '*' <Dim Factor>
        //            |  <Dim Term> '^' <Dim Factor>
        //            |  <Dim Term> '/' <Dim Factor>
        //            |  <Dim Factor>
        private ASTNode? GetDimTerm(IExprBuilder exprBuilder)
        {
            ASTNode? lhs = GetDimFactor(exprBuilder);
            if (lhs is null)
                return null;

            while ((_token.Symbol == Lexer.Symbol.Times) || (_token.Symbol == Lexer.Symbol.Div) || (_token.Symbol == Lexer.Symbol.Wedge))
            {
                Lexer.Token operation = new(_token);

                GetToken();

                ASTNode? rhs = GetDimFactor(exprBuilder);
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
                    Notify(
                        operation,
                        "{0}: invalid wedge-product \"{1} ^ {2}\" (^-operator can only be used to multiply units).",
                        _entityName,
                        lhs.ToString(),
                        rhs.ToString()
                    );
                    return null;
                }
            }
            return lhs;
        }

        //<Dim Expr> ::= <Dim Expr> '|' <Dim Term>
        //           |   <Dim Term>
        private bool GetSecondaryDimExpr(Unit unit, IExprBuilder exprBuilder)
        {
            while (_token.Symbol == Lexer.Symbol.Pipe)
            {
                GetToken();

                ASTNode? secondary = GetDimTerm(exprBuilder);
                if (secondary is null) return false;

                (DimExpr? sense, NumExpr? factor) = BuildExpression(secondary, exprBuilder);

                if ((sense is null) || (factor is null))
                {
                    return false;
                }
                else if (sense.Value != unit.Sense.Value)
                {
                    Notify("{0}: inconsistent dimensions: {1} == {2} != {3} == {4}.",
                        unit.SourceKeyword, unit.Sense.SimpleCode, unit.Sense.Value, sense.Value, sense.SimpleCode
                    );
                    return false;
                }
                else if (factor.IsReal && unit.Factor.IsReal && !factor.ValueEquals(unit.Factor))
                {
                    Notify("{0}: inconsistent conversion factors: {1} == {2} != {3} == {4}.", unit.SourceKeyword,
                        unit.Factor.SimpleCode, unit.Factor.ValuePreciseString(), factor.ValuePreciseString(), factor.SimpleCode
                    );
                    return false;
                }

                secondary.Normalized().Bind(unit);
            }
            return true;
        }

        //<Dim Factor> ::= '<' <Magnitude> '>'
        //             |    Identifier        ! Unit name (e.g. Meter)
        //             |    <Num Literal>
        //             |   '(' <Dim Term> ')'
        //
        // <Num Literal> ::= IntLiteral | RealLiteral | StringLiteral  ! Member-access (e.g. "Math.PI")
        private ASTNode? GetDimFactor(IExprBuilder exprBuilder)
        {
            // '<' Magnitude '>'?
            if (_token.Symbol == Lexer.Symbol.LT)
            {
                GetToken();
                return GetDimMagnitude();
            }

            // '(' <Dim Term> ')'?
            else if (_token.Symbol == Lexer.Symbol.LParen)
            {
                GetToken();
                ASTNode? factor = GetDimTerm(exprBuilder);
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

            // <Num Literal>?
            else if ((_token.Symbol == Lexer.Symbol.IntNumber) || (_token.Symbol == Lexer.Symbol.RealNumber))
            {
                ASTNumber number = new(_token.Text);
                GetToken();
                return number;
            }

            // <String Literal> e.g. "System.Math.PI"?
            else if (_token.Symbol == Lexer.Symbol.StringLiteral)
            {
                ASTLiteral literal = new(_token.Body);
                GetToken();
                return literal;
            }

            // Unit identifier?
            else if (_token.Symbol == Lexer.Symbol.Identifier)
            {
                Unit? u = FindUnit(_token.Text);
                if (u is null)
                {
                    Notify("{0}: undefined unit \"{1}\".", _entityName, _token.Text);
                }
                else if (u.NumericType.SourceKeyword != exprBuilder.Agent.NumericTerm.SourceKeyword)
                {
                    Notify("{0}: numeric type <{1}> is not compatible with the type <{2}> used in the \"{3}\" unit.",
                        _entityName,
                        exprBuilder.Agent.NumericTerm.SourceKeyword,
                        u.NumericType.SourceKeyword,
                        _token.Text
                    );
                }
                else
                {
                    GetToken();
                    return new ASTUnit(u);
                }
            }

            else
            {
                Notify("{0}: found \"{1}\" while expected: <dimension> | unit | number | (expression) | \"literal\".", _entityName, _token.Text);
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
        private ASTMagnitude? GetDimMagnitude()
        {
            string errFmt = "{0}: found \"{1}\" while expected a dimension keyword (in angle brackets): <{2}> or <> for dimensionless unit.";

            if (_token.Symbol == Lexer.Symbol.GT)
            {
                GetToken();
                return new ASTMagnitude();
            }

            if ((_token.Symbol != Lexer.Symbol.Identifier) || !Enum.TryParse(_token.Text, false, out Magnitude m))
            {
                Notify(errFmt, _entityName, _token.Text, string.Join(">, <", Enum.GetNames(typeof(Magnitude))));
            }
            else if (GetToken() == Lexer.Symbol.GT)
            {
                GetToken();
                return new ASTMagnitude(m);
            }
            else
            {
                Notify(errFmt, _entityName, _token.Text, string.Join(">, <", Enum.GetNames(typeof(Magnitude))));
            }
            return null;
        }
        #endregion
    }
}
