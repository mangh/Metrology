/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Mangh.Metrology.Language;

namespace Mangh.Metrology
{
    /// <summary>
    /// Builder of <see cref="DimExpr"/> and <see cref="NumExpr"/> expressions.
    /// </summary>
    public interface IExprBuilder : IASTVisitor
    {
        /// <summary>
        /// Generic agent for numerics.
        /// </summary>
        NumericAgent Agent { get; }

        /// <summary>
        /// Encode <see cref="ASTNode "/> expression into the target language.
        /// </summary>
        /// <param name="node"><see cref="ASTNode"/> to encode.</param>
        /// <returns>
        /// (<see cref="DimExpr"/>, <see cref="NumExpr"/>) expressions in the target language.
        /// </returns>
        (DimExpr, NumExpr) Encode(ASTNode node);
    }
}
