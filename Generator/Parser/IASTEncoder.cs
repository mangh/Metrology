/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Man.Metrology/*.Parser*/
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    internal interface IASTEncoder
    {
        void Encode(ASTNumber number);
        void Encode(ASTLiteral literal);
        void Encode(ASTMagnitude magnitude);
        void Encode(ASTUnit unit);
        void Encode(ASTUnary term);
        void Encode(ASTParenthesized term);
        void Encode(ASTProduct product);
        void Encode(ASTQuotient quotient);
        void Encode(ASTSum sum);
        void Encode(ASTDifference difference);
    }
    internal interface IDimExprEncoder : IASTEncoder
    {
        DimensionalExpression Accept(ASTNode node);
    }
    internal interface INumExprEncoder : IASTEncoder
    {
        NumeralExpression Accept(ASTNode node);
        NumeralExpression One { get; }
        NumeralExpression Zero { get; }

    }
}
