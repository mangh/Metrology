using System;
using System.Collections.Generic;
using System.Text;

namespace Man.Metrology.CS
{
    /// <summary>
    /// Constant dimensional expressions for C#.
    /// </summary>
    public static class ConstantDimExpr
    {
        /// <summary>
        /// Dimensionless dimension as a C# expression.
        /// </summary>
        public static readonly DimensionalExpression Dimensionless = new(Dimension.None, "Dimension.None");
    }
}
