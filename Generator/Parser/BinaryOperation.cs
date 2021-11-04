/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology
{
    /// <summary>
    /// Binary operation components.
    /// </summary>
    public class BinaryOperation
    {
        /// <summary>
        /// Result of the operation.
        /// </summary>
        public AbstractType Result { get; private set; }

        /// <summary>
        /// Operator.
        /// </summary>
        public string Operation { get; private set; }

        /// <summary>
        /// Left-hand side argument for the operation.
        /// </summary>
        public AbstractType Lhs { get; private set; }

        /// <summary>
        /// Right-hand side argument for the operation.
        /// </summary>
        public AbstractType Rhs { get; private set; }

        /// <summary>
        /// BinaryOperation constructor.
        /// </summary>
        /// <param name="result">result of the operation.</param>
        /// <param name="operation">operator.</param>
        /// <param name="lhs">left-hand side argument.</param>
        /// <param name="rhs">right-hand side argument.</param>
        public BinaryOperation(AbstractType result, string operation, AbstractType lhs, AbstractType rhs)
        {
            Result = result;
            Operation = operation;
            Lhs = lhs;
            Rhs = rhs;
        }
    }
}
