/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Globalization;

namespace Man.Metrology
{
    /// <summary>
    /// Numeral abstract type.
    /// </summary>
    public abstract class Numeral : AbstractType, IEquatable<Numeral>
    {
        #region Properties
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Numeral constructor.
        /// </summary>
        /// <param name="name">name (keyword) to be used in a target language for that numeral type.</param>
        /// <param name="predefinedName">fully qualified (predefined) type name (if any).</param>
        protected Numeral(string name, string predefinedName) :
            base(name, predefinedName)
        {
        }
        #endregion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        #region Equality
        public abstract bool /*IEquatable<Numeral>*/ Equals(Numeral? other);
        public override bool /*IObject*/ Equals(object? obj) => (obj is Numeral n) && Equals(n);
        public override int /*IObject*/ GetHashCode() => 0;
        #endregion

        #region Arithmetic
        public abstract Numeral Add(Numeral other);
        public abstract Numeral Subtract(Numeral other);
        public abstract Numeral Multiply(Numeral other);
        public abstract Numeral Divide(Numeral other);
        public abstract Numeral Negate();
        #endregion

        #region Operators
        public static Numeral operator +(Numeral lhs, Numeral rhs) => lhs.Add(rhs);
        public static Numeral operator -(Numeral lhs, Numeral rhs) => lhs.Subtract(rhs);
        public static Numeral operator *(Numeral lhs, Numeral rhs) => lhs.Multiply(rhs);
        public static Numeral operator /(Numeral lhs, Numeral rhs) => lhs.Divide(rhs);
        public static bool operator ==(Numeral lhs, Numeral rhs) => ReferenceEquals(lhs, rhs) || (!Equals(lhs, null) && lhs.Equals(rhs));
        public static bool operator !=(Numeral lhs, Numeral rhs) => !(lhs == rhs);
        #endregion

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #region Formatting
        /// <summary>
        /// Numeral in a string form with max precision available.
        /// </summary>
        /// <returns>Stringified <see cref="Numeral"/></returns>
        public abstract string ToPreciseString();
        #endregion

        #region Static methods
        /// <summary>
        /// Create <see cref="Numeral"/> from a value given as an object.
        /// </summary>
        /// <param name="value">object supposed to be a: <c>double</c>, <c>float</c> or <c>decimal</c> value</param>
        /// <returns><see cref="Numeral"/> for the value given; <c>null</c> in case of inappropriate value type.</returns>
        public static Numeral? CreateFromObject(object value) => 
            (value is double dbl) ? new NumeralDouble(dbl) :
            (value is decimal dec) ? new NumeralDecimal(dec) :
            (value is float flt) ? new NumeralFloat(flt) :
            null;
        #endregion
    }

    /// <summary>
    /// Double-precision floating-point numeral type.
    /// </summary>
    public class NumeralDouble : Numeral
    {
        #region Constants
        /// <summary>
        /// Keyword used for a double-precision floating-point numerals in a target language (C# here).
        /// </summary>
        public const string Keyword = "double";

        /// <summary>
        /// Fully qualified type name for a double-precision floating-point numerals (in C# language).
        /// </summary>
        public const string BuiltinTypename = "System.Double";
        #endregion

        #region Properties
        /// <summary>
        /// Numeral value.
        /// </summary>
        public double Value { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="NumeralDouble"/> constructor.
        /// </summary>
        /// <param name="value">numeral value</param>
        public NumeralDouble(double value) :
            base(Keyword, BuiltinTypename) => Value = value;
        #endregion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        #region Equality
        public override bool Equals(Numeral? other) => Value == Cast(other).Value;
        public override int GetHashCode() => Value.GetHashCode();
        #endregion

        #region Arithmetic
        public override Numeral Add(Numeral other) => new NumeralDouble(Value + Cast(other).Value);
        public override Numeral Subtract(Numeral other) => new NumeralDouble(Value - Cast(other).Value);
        public override Numeral Multiply(Numeral other) => new NumeralDouble(Value * Cast(other).Value);
        public override Numeral Divide(Numeral other) => new NumeralDouble(Value / Cast(other).Value);
        public override Numeral Negate() => new NumeralDouble(-Value);
        private static NumeralDouble Cast(Numeral? other)
        {
            if (other is NumeralDouble n)
                return n;

            string otherArg = other is null ? "null" : $"{other.GetType().Name} {other}";
            throw new InvalidOperationException($"{nameof(NumeralDouble)}.{nameof(Cast)}({otherArg}): expected {nameof(NumeralDouble)} argument.");
        }
        #endregion

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #region Formatting
        /// <summary>
        /// <see cref="NumeralDouble"/> in a string form with max precision available.
        /// </summary>
        /// <returns>Stringified <see cref="NumeralDouble"/></returns>
        public override string ToPreciseString() => Value.ToString("G17", CultureInfo.InvariantCulture);
        /// <summary>
        /// <see cref="NumeralDouble"/> in a string form with a standard precision.
        /// </summary>
        /// <returns>Stringified <see cref="NumeralDouble"/></returns>
        public override string ToString() => Value.ToString("G", CultureInfo.InvariantCulture);
        #endregion

        #region Statics
        /// <summary>
        /// One (1.0) as a <see cref="NumeralDouble"/>.
        /// </summary>
        public static readonly NumeralDouble One = new(1.0d);
        /// <summary>
        /// Zero (0.0) as a <see cref="NumeralDouble"/>.
        /// </summary>
        public static readonly NumeralDouble Zero = new(0.0d);

        /// <summary>
        /// Try parse numeral string into a <see cref="NumeralDouble"/>.
        /// </summary>
        /// <param name="numeral">numeral input string</param>
        /// <returns><see cref="NumeralDouble"/> for a valid numeral string; <c>null</c> for an invalid string.</returns>
        public static NumeralDouble? TryParse(string numeral)
            => double.TryParse(numeral, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? new NumeralDouble(d) : null;

        #endregion
    }


    /// <summary>
    /// Decimal floating-point numeral type.
    /// </summary>
    public class NumeralDecimal : Numeral
    {
        #region Constants
        /// <summary>
        /// Keyword used for a decimal floating-point numerals in a target language (C# here).
        /// </summary>
        public const string Keyword = "decimal";
        /// <summary>
        /// Fully qualified type name for a decimal floating-point numerals (in C# language).
        /// </summary>
        public const string BuiltinTypename = "System.Decimal";
        #endregion

        #region Properties
        /// <summary>
        /// Numeral value.
        /// </summary>
        public decimal Value { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="NumeralDecimal"/> constructor.
        /// </summary>
        /// <param name="value">numeral value</param>
        public NumeralDecimal(decimal value) :
            base(Keyword, BuiltinTypename) => Value = value;
        #endregion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        #region Equality
        public override bool Equals(Numeral? other) => Value == Cast(other).Value;
        public override int GetHashCode() => Value.GetHashCode();
        #endregion

        #region Arithmetic
        public override Numeral Add(Numeral other) => new NumeralDecimal(Value + Cast(other).Value);
        public override Numeral Subtract(Numeral other) => new NumeralDecimal(Value - Cast(other).Value);
        public override Numeral Multiply(Numeral other) => new NumeralDecimal(Value * Cast(other).Value);
        public override Numeral Divide(Numeral other) => new NumeralDecimal(Value / Cast(other).Value);
        public override Numeral Negate() => new NumeralDecimal(-Value);

        private static NumeralDecimal Cast(Numeral? other)
        {
            if (other is NumeralDecimal n)
                return n;

            string otherArg = other is null ? "null" : $"{other.GetType().Name} {other}";
            throw new InvalidOperationException($"{nameof(NumeralDecimal)}.{nameof(Cast)}({otherArg}): expected {nameof(NumeralDecimal)} argument.");
        }
        #endregion

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #region Formatting
        /// <summary>
        /// <see cref="NumeralDecimal"/> in a string form with max precision available.
        /// </summary>
        /// <returns>Stringified <see cref="NumeralDecimal"/></returns>
        public override string ToPreciseString() => ToString();
        /// <summary>
        /// <see cref="NumeralDecimal"/> in a string form with standard precision.
        /// </summary>
        /// <returns>Stringified <see cref="NumeralDecimal"/></returns>
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        #endregion

        #region Statics
        /// <summary>
        /// One (1.0) as a <see cref="NumeralDecimal"/>.
        /// </summary>
        public static readonly NumeralDecimal One = new(decimal.One);
        /// <summary>
        /// Zero (0.0) as a <see cref="NumeralDecimal"/>.
        /// </summary>
        public static readonly NumeralDecimal Zero = new(decimal.Zero);

        /// <summary>
        /// Try parse numeral string into a <see cref="NumeralDecimal"/>.
        /// </summary>
        /// <param name="numeral">numeral input string</param>
        /// <returns><see cref="NumeralDecimal"/> for a valid numeral string; <c>null</c> for an invalid string.</returns>
        public static NumeralDecimal? TryParse(string numeral)
            => decimal.TryParse(numeral, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal m) ? new NumeralDecimal(m) : null;
        #endregion
    }

    /// <summary>
    /// Single-precision floating-point numeral type.
    /// </summary>
    public class NumeralFloat : Numeral
    {
        #region Constants
        /// <summary>
        /// Keyword used for a single-precision floating-point numerals in a target language (C# here).
        /// </summary>
        public const string Keyword = "float";
        /// <summary>
        /// Fully qualified type name for a single-precision floating-point numerals (in C# language).
        /// </summary>
        public const string BuiltinTypename = "System.Single";
        #endregion

        #region Properties
        /// <summary>
        /// Numeral value.
        /// </summary>
        public float Value { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="NumeralFloat"/> constructor.
        /// </summary>
        /// <param name="value">numeral value</param>
        public NumeralFloat(float value) :
            base(Keyword, BuiltinTypename) => Value = value;
        #endregion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        #region Equality
        public override bool Equals(Numeral? other) => Value == Cast(other).Value;
        public override int GetHashCode() => Value.GetHashCode();
        #endregion

        #region Arithmetic
        public override Numeral Add(Numeral other) => new NumeralFloat(Value + Cast(other).Value);
        public override Numeral Subtract(Numeral other) => new NumeralFloat(Value - Cast(other).Value);
        public override Numeral Multiply(Numeral other) => new NumeralFloat(Value * Cast(other).Value);
        public override Numeral Divide(Numeral other) => new NumeralFloat(Value / Cast(other).Value);
        public override Numeral Negate() => new NumeralFloat(-Value);

        private static NumeralFloat Cast(Numeral? other)
        {
            if (other is NumeralFloat n)
                return n;

            string otherArg = other is null ? "null" : $"{other.GetType().Name} {other}";
            throw new InvalidOperationException($"{nameof(NumeralFloat)}.{nameof(Cast)}({otherArg}): expected {nameof(NumeralFloat)} argument.");
        }
        #endregion

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        #region Formatting
        /// <summary>
        /// <see cref="NumeralFloat"/> in a string form with max precision available.
        /// </summary>
        /// <returns>Stringified <see cref="NumeralFloat"/></returns>
        public override string ToPreciseString() => Value.ToString("G9", CultureInfo.InvariantCulture);
        /// <summary>
        /// <see cref="NumeralFloat"/> in a string form with standard precision.
        /// </summary>
        /// <returns>Stringified <see cref="NumeralFloat"/></returns>
        public override string ToString() => Value.ToString("G", CultureInfo.InvariantCulture);
        #endregion

        #region Statics
        /// <summary>
        /// One (1.0) as a <see cref="NumeralFloat"/>.
        /// </summary>
        public static readonly NumeralFloat One = new(1.0f);
        /// <summary>
        /// Zero (0.0) as a <see cref="NumeralFloat"/>.
        /// </summary>
        public static readonly NumeralFloat Zero = new(0.0f);

        /// <summary>
        /// Try parse numeral string into a <see cref="NumeralFloat"/>.
        /// </summary>
        /// <param name="numeral">numeral input string</param>
        /// <returns><see cref="NumeralFloat"/> for a valid numeral string; <c>null</c> for an invalid string.</returns>
        public static NumeralFloat? TryParse(string numeral)
            => float.TryParse(numeral, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? new NumeralFloat(f) : null;
        #endregion
    }
}
