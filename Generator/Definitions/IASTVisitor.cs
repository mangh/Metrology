/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Mangh.Metrology
{
    /// <summary>
    /// <see cref="ASTNode"/> visitor interface.
    /// </summary>
    public interface IASTVisitor
    {
        void Visit(ASTNumber number);
        void Visit(ASTLiteral literal);
        void Visit(ASTMagnitude magnitude);
        void Visit(ASTUnit unit);
        void Visit(ASTUnary term);
        void Visit(ASTParenthesized term);
        void Visit(ASTProduct product);
        void Visit(ASTQuotient quotient);
        void Visit(ASTSum sum);
        void Visit(ASTDifference difference);
    }
}
