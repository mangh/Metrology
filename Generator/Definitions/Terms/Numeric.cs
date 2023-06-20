/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Globalization;

namespace Mangh.Metrology
{
    /// <summary>
    /// Numeric value wrapper for processing (multiply, divide etc.) numeric values in a generic way.
    /// </summary>
    /// <typeparam name="T">
    /// <see cref="double"/>, <see cref="decimal"/>, <see cref="float"/> etc. (more numeric types may be added here).
    /// </typeparam>
    public class Numeric<T> : IEquatable<Numeric<T>>
        where T : struct, IEquatable<T>
    {
        #region Properties
        /// <summary>Value of numeric type <typeparamref name="T"/>.</summary>
        public T Value { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Numeric{T}"/> constructor.
        /// </summary>
        /// <param name="value">Value of numeric type <typeparamref name="T"/>.</param>
        public Numeric(T value) => Value = value;
        #endregion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        #region Equality
        public bool /*IEquatable<Numeric<T>>*/ Equals(Numeric<T> other) => Value.Equals(other.Value);
        public override bool /*IObject*/ Equals(object? obj) => (obj is Numeric<T> n) && Equals(n);
        public override int /*IObject*/ GetHashCode() => Value.GetHashCode();
        #endregion

        #region Arithmetic
        public Numeric<T> Add(Numeric<T> other) => new(_base.Add(Value, other.Value));
        public Numeric<T> Subtract(Numeric<T> other) => new(_base.Subtract(Value, other.Value));
        public Numeric<T> Multiply(Numeric<T> other) => new(_base.Multiply(Value, other.Value));
        public Numeric<T> Divide(Numeric<T> other) => new(_base.Divide(Value, other.Value));
        public Numeric<T> Negate() => new(_base.Negate(Value));
        #endregion

        #region Operators
        public static Numeric<T> operator +(Numeric<T> lhs, Numeric<T> rhs) => lhs.Add(rhs);
        public static Numeric<T> operator -(Numeric<T> lhs, Numeric<T> rhs) => lhs.Subtract(rhs);
        public static Numeric<T> operator -(Numeric<T> lhs) => lhs.Negate();
        public static Numeric<T> operator *(Numeric<T> lhs, Numeric<T> rhs) => lhs.Multiply(rhs);
        public static Numeric<T> operator /(Numeric<T> lhs, Numeric<T> rhs) => lhs.Divide(rhs);
        public static bool operator ==(Numeric<T> lhs, Numeric<T> rhs) => ReferenceEquals(lhs, rhs) || ((lhs is not null) && lhs.Equals(rhs));
        public static bool operator !=(Numeric<T> lhs, Numeric<T> rhs) => !(lhs == rhs);
        #endregion

        #region Formatting
        public override string ToString() => _base.ToString(Value);
        public string ToPreciseString() => _base.ToPreciseString(Value);
        #endregion

        #region Statics
        private static readonly INumeric<T> _base =
            (typeof(T) == typeof(double)) ? (new Double() as INumeric<T>)! :
            (typeof(T) == typeof(decimal)) ? (new Decimal() as INumeric<T>)! :
            (typeof(T) == typeof(float)) ? (new Float() as INumeric<T>)! :
            throw new NotSupportedException($"{nameof(Numeric<T>)}: unsupported type {typeof(T).FullName}.");

        #endregion

        #region Base numeric types
        private interface INumeric<N>
            where N : struct
        {
            N Add(N lhs, N rhs);
            N Subtract(N lhs, N rhs);
            N Multiply(N lhs, N rhs);
            N Divide(N lhs, N rhs);
            N Negate(N lhs);

            string ToString(N lhs);
            string ToPreciseString(N lhs);
        }

        private class Double : INumeric<double>
        {
            public double Add(double lhs, double rhs) => lhs + rhs;
            public double Subtract(double lhs, double rhs) => lhs - rhs;
            public double Multiply(double lhs, double rhs) => lhs * rhs;
            public double Divide(double lhs, double rhs) => lhs / rhs;
            public double Negate(double lhs) => -lhs;
            public string ToString(double lhs) => lhs.ToString("G", CultureInfo.InvariantCulture);
            public string ToPreciseString(double lhs) => lhs.ToString("G17", CultureInfo.InvariantCulture);
        }

        private class Float : INumeric<float>
        {
            public float Add(float lhs, float rhs) => lhs + rhs;
            public float Subtract(float lhs, float rhs) => lhs - rhs;
            public float Multiply(float lhs, float rhs) => lhs * rhs;
            public float Divide(float lhs, float rhs) => lhs / rhs;
            public float Negate(float lhs) => -lhs;
            public string ToString(float lhs) => lhs.ToString("G", CultureInfo.InvariantCulture);
            public string ToPreciseString(float lhs) => lhs.ToString("G9", CultureInfo.InvariantCulture);
        }

        private class Decimal : INumeric<decimal>
        {
            public decimal Add(decimal lhs, decimal rhs) => lhs + rhs;
            public decimal Subtract(decimal lhs, decimal rhs) => lhs - rhs;
            public decimal Multiply(decimal lhs, decimal rhs) => lhs * rhs;
            public decimal Divide(decimal lhs, decimal rhs) => lhs / rhs;
            public decimal Negate(decimal lhs) => -lhs;
            public string ToString(decimal lhs) => lhs.ToString(CultureInfo.InvariantCulture);
            public string ToPreciseString(decimal lhs) => ToString(lhs);
        }
        #endregion
    }
}
