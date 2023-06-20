/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;

namespace Mangh.Metrology
{
    /// <summary>
    /// Numeric expression code.
    /// </summary>
    public abstract class NumExpr
    {
        /// <summary>
        /// Is the <c>Value</c> of the expression true or fake
        /// (e.g. fake value "<c>1</c>" assigned to a string literal that cannot be evaluated).
        /// </summary>
        public abstract bool IsReal { get; }

        /// <summary>
        /// Simple numeric expression in the target language.
        /// </summary>
        public string SimpleCode { get; }

        /// <summary>
        /// Unfolded numeric expression, equivalent to <see cref="SimpleCode"/>,
        /// but built of numeric constants in the target language.
        /// </summary>
        public string UnfoldedCode { get; }

        /// <summary>
        /// <see cref="NumExpr"/> constructor.
        /// </summary>
        /// <param name="simpleCode">Expression encoded for a target language.</param>
        /// <param name="unfoldedCode">The <paramref name="simpleCode"/> parameter equivalent, composed of numeric constants only.</param>
        protected NumExpr(string simpleCode, string unfoldedCode)
        {
            SimpleCode = simpleCode;
            UnfoldedCode = unfoldedCode;
        }

        /// <summary>
        /// Checks whether the current <see cref="NumExpr"/> expression has the same <c>Value</c> as another.
        /// </summary>
        /// <param name="other">The other <see cref="NumExpr"/>.</param>
        /// <returns><see langword="true"/> when both expression have the same <c>Value</c>, otherwise <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="NumExpr"/> expressions are of different type.</exception>
        public abstract bool ValueEquals(NumExpr other);

        /// <summary>
        /// Returns expression <see cref="NumExpr{T}.Value"/> as a string of max precision available.
        /// </summary>
        public abstract string ValuePreciseString();
    }

    /// <summary>
    /// Numeric expression code with its value.
    /// </summary>
    /// <typeparam name="T">Numeric type (e.g. <see cref="double"/>) to specialize the <see cref="NumExpr{T}.Value"/> property.</typeparam>
    public class NumExpr<T> : NumExpr
        where T : struct, IEquatable<T>
    {
        /// <summary>Expression value.</summary>
        public Numeric<T> Value { get; }

        /// <summary>
        /// Is the <see cref="Value"/> a true or a fake one
        /// e.g. fake value '<c>1</c>' assigned to a string literal that cannot be evaluated.
        /// </summary>
        public override bool IsReal { get; }

        /// <summary>
        /// <see cref="NumExpr{T}"/> expression constructor.
        /// </summary>
        /// <param name="isReal">Is <paramref name="value"/> parameter a real value or a substitute for an unknown value?</param>
        /// <param name="value">Expression value</param>
        /// <param name="simple">Expression encoded for the target language.</param>
        /// <param name="unfolded">The <paramref name="simple"/> parameter equivalent, but composed of numeric constants only.</param>
        public NumExpr(bool isReal, Numeric<T> value, string simple, string? unfolded = null)
            : base(simple, unfolded ?? simple)
        {
            IsReal = isReal;
            Value = value;
        }

        /// <summary>
        /// Checks whether the current <see cref="NumExpr"/> expression has the same <c>Value</c> as another.
        /// </summary>
        /// <param name="other">The other <see cref="NumExpr"/>.</param>
        /// <returns><see langword="true"/> when both expression have the same <see cref="Value"/>, otherwise <see langword="false"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the <paramref name="other"/> expressions is of different type.</exception>
        public override bool ValueEquals(NumExpr other) => (other is NumExpr<T> o) ?
            Value == o.Value :
            throw new InvalidOperationException($"{nameof(NumExpr<T>)}.{nameof(ValueEquals)}: incomparable numeric expressions.");

        /// <summary>
        /// Return expression <see cref="Value"/> as a string of max precision available.
        /// </summary>
        public override string ValuePreciseString() => Value.ToPreciseString();

        /// <summary>
        /// <see cref="NumExpr{T}"/> in a text form.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item><description>for <c>real</c> value - expression value,</description></item>
        /// <item><description>for <c>fake</c> value - expression code.</description></item>
        /// </list>
        /// </returns>
        public override string ToString() => IsReal ? Value.ToPreciseString() : SimpleCode;
    }
}