/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;

namespace Mangh.Metrology.Language
{
    /// <summary>
    /// Generic agent for numerics.
    /// </summary>
    public abstract class NumericAgent
    {
        #region C# Math Constants
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public const string SYSTEM_MATH_PI = "System.Math.PI";
        public const string MATH_PI = "Math.PI";
        public const string SYSTEM_MATH_E = "System.Math.E";
        public const string MATH_E = "Math.E";
#pragma warning restore
        #endregion

        #region Properties
        /// <summary>
        /// Supported numeric <see cref="Term"/>.
        /// </summary>
        public Term NumericTerm { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="NumericAgent"/> constructor.
        /// </summary>
        /// <param name="numericTerm">Related numeric type.</param>
        public NumericAgent(Term numericTerm) => NumericTerm = numericTerm;
        #endregion

        #region Methods
        /// <summary>
        /// Checks a string to see if it is the name of a math constant in C#.
        /// </summary>
        /// <param name="literal">A string to be checked.</param>
        /// <param name="constant">Value of the constant as a <see cref="double"/> number.</param>
        /// <returns>
        /// Standard, fully qualified name of the C# constant - or - 
        /// <see langword="null"/>, if the <paramref name="literal"/> is not a constant.
        /// </returns>
        public string? CheckCSharpMathConstant(string literal, out double constant)
        {
            if ((literal == MATH_PI) || (literal == SYSTEM_MATH_PI))
            {
                constant = Math.PI;
                return SYSTEM_MATH_PI;
            }
            if ((literal == MATH_E) || (literal == SYSTEM_MATH_E))
            {
                constant = Math.E;
                return SYSTEM_MATH_E;
            }

            constant = default;
            return null;
        }
        #endregion
    }

    /// <summary>
    /// <see cref="NumericAgent"/> for the specific <see cref="Numeric{T}"/> type.
    /// </summary>
    /// <typeparam name="T">
    /// Base numeric type (e.g. <see cref="double"/>, <see cref="float"/>, <see cref="decimal"/> etc.).
    /// </typeparam>
    public abstract class NumericAgent<T> : NumericAgent
        where T : struct, IEquatable<T>
    {
        #region Properties
        /// <summary>
        /// The neutral (identity) element of multiplication.
        /// </summary>
        public abstract T One { get; }

        /// <summary>
        /// The neutral (identity) element of addition.
        /// </summary>
        public abstract T Zero { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="NumericAgent{T}"/>constructor.
        /// </summary>
        /// <param name="numericTerm">Related numeric type.</param>
        public NumericAgent(Term numericTerm)
            : base(numericTerm)
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts a string to its numeric equivalent.
        /// </summary>
        /// <param name="numeric">A string numeric to convert.</param>
        /// <param name="result">Numeric equivalent of the <paramref name="numeric"/> parameter.</param>
        /// <returns><see langword="true"/> if <paramref name="numeric"/> has been converted successfully, otherwise, <see langword="false"/>.</returns>
        public abstract bool TryParse(string numeric, out T result);

        /// <summary>
        /// Checks the literal to see if it is the name of a math constant in C#.
        /// </summary>
        /// <param name="literal">The literal to be checked.</param>
        /// <param name="constant">Constant as a number of type <typeparamref name="T"/>.</param>
        /// <returns>
        /// Standard, fully qualified name of the C# constant - or - 
        /// <see langword="null"/>, if the <paramref name="literal"/> is not a constant.
        /// </returns>
        public abstract string? TryMatchCSharpConstant(string literal, out T constant);

        /// <summary>
        /// Converts a number to its string equivalent in the target language.
        /// </summary>
        /// <param name="number">A number to convert.</param>
        /// <returns>Number as a string in the target language.</returns>
        public abstract string ToTargetString(T number);
        #endregion
    }
}
