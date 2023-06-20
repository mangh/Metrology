/*******************************************************************************

    Units of Measurement for C#/C++ applications

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
        public Term Result { get; private set; }

        /// <summary>
        /// Operator.
        /// </summary>
        public string Operation { get; private set; }

        /// <summary>
        /// Left-hand side operand.
        /// </summary>
        public Term Lhs { get; private set; }

        /// <summary>
        /// Right-hand side operand.
        /// </summary>
        public Term Rhs { get; private set; }

        /// <summary>
        /// BinaryOperation constructor.
        /// </summary>
        /// <param name="result">Result of the operation.</param>
        /// <param name="operation">Operation/operator symbol.</param>
        /// <param name="lhs">Left-hand side operand.</param>
        /// <param name="rhs">Right-hand side operand.</param>
        public BinaryOperation(Term result, string operation, Term lhs, Term rhs)
        {
            Result = result;
            Operation = operation;
            Lhs = lhs;
            Rhs = rhs;
        }
    }
}
