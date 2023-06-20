/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Mangh.Metrology
{
    // 64-bit dimension = 8 x 8-bit exponents
    using DIMENSION = System.UInt64;

    /// <summary>
    /// Magnitude (dimension component index).
    /// </summary>
    public enum Magnitude : int
    {
        Length = 0,
        Time = 1,
        Mass = 2,
        Temperature = 3,
        ElectricCurrent = 4,
        AmountOfSubstance = 5,
        LuminousIntensity = 6,
        Other = 7,
        Money = Other,
    }

    /// <summary>
    /// Dimension struct.
    /// </summary>
    public struct Dimension : IEquatable<Dimension>
    {
        #region Constants
        // Constants for exponent fields within UInt64/32 dimension structure:
        private const int COMPLEMENT = 256; // 2's complement base
        private const int MAXEXP = 127;     // exponent max value
        private const int MINEXP = -128;    // exponent min value
        private const int LOGWIDTH = 3;     // exponent bit-width = 2^LOGWIDTH
        private const byte EXPMASK = 0xFF;  // exponent bit mask
#if !STANDARD_ARITHMETIC
        private const DIMENSION CARRY = 0x7F7F7F7F7F7F7F7F; // carry/borrow bits for binary addition/subtraction
#endif

        // Primary dimensions
        public static readonly Dimension None = new();    // new Dimension(0, 0, 0, 0, 0, 0, 0, 0);
        public static readonly Dimension Length = new(Magnitude.Length);
        public static readonly Dimension Time = new(Magnitude.Time);
        public static readonly Dimension Mass = new(Magnitude.Mass);
        public static readonly Dimension Temperature = new(Magnitude.Temperature);
        public static readonly Dimension ElectricCurrent = new(Magnitude.ElectricCurrent);
        public static readonly Dimension AmountOfSubstance = new(Magnitude.AmountOfSubstance);
        public static readonly Dimension LuminousIntensity = new(Magnitude.LuminousIntensity);
        public static readonly Dimension Other = new(Magnitude.Money);
        public static readonly Dimension Money = new(Magnitude.Other);
        #endregion

        #region Fields
        private DIMENSION m_exponents;
        #endregion

        #region Properties
        /// <summary>
        /// Dimension as uint32/uint64 struct with exponents encoded within.
        /// </summary>
        public readonly DIMENSION Exponents => m_exponents;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Creates primary dimension (for a single magnitude).
        /// </summary>
        /// <param name="magnitude">magnitude index</param>
        public Dimension(Magnitude magnitude)
        {
            m_exponents = 0;
            this[magnitude] = 1;
        }

        /// <summary>
        /// Creates composite dimension (for all magnitudes at once).
        /// </summary>
        /// <param name="length">value for the Length exponent</param>
        /// <param name="time">value for the Time exponent</param>
        /// <param name="mass">value for the Mass exponent</param>
        /// <param name="temperature">value for the Temperature exponent</param>
        /// <param name="current">value for the ElectricCurrent exponent</param>
        /// <param name="substance">value for the AmountOfSubstance exponent</param>
        /// <param name="intensity">value for the LuminousIntensity exponent</param>
        /// <param name="other">value for the Other (e.g. Money) exponent</param>
        /// <exception cref="OverflowException">thrown for any exponent value(s) out of permissible range.</exception>
        public Dimension(int length, int time, int mass, int temperature, int current, int substance, int intensity, int other)
        {
            m_exponents = 0;

            this[Magnitude.Length] = length;
            this[Magnitude.Time] = time;
            this[Magnitude.Mass] = mass;
            this[Magnitude.Temperature] = temperature;
            this[Magnitude.ElectricCurrent] = current;
            this[Magnitude.AmountOfSubstance] = substance;
            this[Magnitude.LuminousIntensity] = intensity;
            this[Magnitude.Other] = other;
        }

        /// <summary>
        /// Internal dimension constructor.
        /// </summary>
        /// <param name="exponents">internally calculated exponents</param>
        private Dimension(DIMENSION exponents) => m_exponents = exponents;

        #endregion Constructor(s)

        #region Indexer(s)
        /// <summary>
        /// Exponent indexer.
        /// </summary>
        /// <param name="magnitude">magnitude index</param>
        /// <returns>exponent for the selected magnitude (index)</returns>
        /// <exception cref="OverflowException">thrown when setting an exponent to an out-of-permissible-range value.</exception>
        public int this[Magnitude magnitude]
        {
            get
            {
                int slot = (int)magnitude;
                byte exponent = (byte)((m_exponents >> (slot << LOGWIDTH)) & EXPMASK);
                return (exponent > MAXEXP) ? (exponent - COMPLEMENT) : exponent;
            }
            private set
            {
                int slot = (int)magnitude;
                if ((value < MINEXP) || (MAXEXP < value))
                {
                    throw new OverflowException($"{nameof(Dimension)}[{magnitude}] = {value}: value out of range [{MINEXP},{MAXEXP}].");
                }
                m_exponents |= ((DIMENSION)(value & EXPMASK)) << (slot << LOGWIDTH);
            }
        }
        #endregion Indexer(s)

        #region IEquatable<Dimension>
        public readonly bool Equals(Dimension other) => m_exponents == other.Exponents;
        public override readonly bool Equals(object? obj) => (obj is Dimension dim) && Equals(dim);
        public override int GetHashCode() => m_exponents.GetHashCode();
        #endregion

        #region Operators
        public static bool operator ==(Dimension A, Dimension B) => A.Equals(B);
        public static bool operator !=(Dimension A, Dimension B) => !A.Equals(B);

        /// <summary>
        /// Multiply dimensions.
        /// </summary>
        /// <param name="lhs">left-hand-side dimension</param>
        /// <param name="rhs">right-hand-side dimension</param>
        /// <returns>dimension = lhs * rhs</returns>
        /// <exception cref="OverflowException">thrown when any of the result exponents overflows permissible range.</exception>
        public static Dimension operator *(Dimension lhs, Dimension rhs)
        {
#if STANDARD_ARITHMETIC
            return new Dimension(
                lhs[Magnitude.Length] + rhs[Magnitude.Length],
                lhs[Magnitude.Time] + rhs[Magnitude.Time],
                lhs[Magnitude.Mass] + rhs[Magnitude.Mass],
                lhs[Magnitude.Temperature] + rhs[Magnitude.Temperature],
                lhs[Magnitude.ElectricCurrent] + rhs[Magnitude.ElectricCurrent],
                lhs[Magnitude.AmountOfSubstance] + rhs[Magnitude.AmountOfSubstance],
                lhs[Magnitude.LuminousIntensity] + rhs[Magnitude.LuminousIntensity],
                lhs[Magnitude.Other] + rhs[Magnitude.Other]
            );
#else
            // The algoritm below is a modified binary addition of two's complement integers.
            // It performs parallel addition of 8 exponents numbers packed within dimension structure.
            // The addition is performed in UP TO 5 iterations (for 4-bit exponents) or 9 iteration (for 8-bit exponents).
            // This is significantly faster than old algorithm (above) that always requires 16 unpacks, 8 additions and 8 packs.
            // However it's very ugly (in that it's hard to see what is it doing) and that's why I left the old one as an option.
            DIMENSION sum = lhs.Exponents ^ rhs.Exponents;
            DIMENSION summed;
            DIMENSION carry = lhs.Exponents & rhs.Exponents;
            DIMENSION carried = 0;
            while (carry != 0)
            {
                carried |= carry;
                carry &= CARRY; // no bit can be carried over between adjacent exponent numbers
                carry <<= 1;
                summed = sum;
                sum = summed ^ carry;
                carry = summed & carry;
            }
            // Check overflow condition:
            if (((carried ^ (carried << 1)) & ~CARRY) != 0)
            {
                throw new OverflowException($"{lhs} * {rhs}: {nameof(Dimension)} product out of range [{MINEXP},{MAXEXP}]");
            }
            return new Dimension(sum);
#endif
        }

        /// <summary>
        /// Divide dimensions.
        /// </summary>
        /// <param name="lhs">left-hand-side dimension</param>
        /// <param name="rhs">right-hand-side dimension</param>
        /// <returns>dimension = lhs / rhs</returns>
        /// <exception cref="OverflowException">thrown when any of the result exponents overflows permissible range.</exception>
        public static Dimension operator /(Dimension lhs, Dimension rhs)
        {
#if STANDARD_ARITHMETIC
            return new Dimension(
                lhs[Magnitude.Length] - rhs[Magnitude.Length],
                lhs[Magnitude.Time] - rhs[Magnitude.Time],
                lhs[Magnitude.Mass] - rhs[Magnitude.Mass],
                lhs[Magnitude.Temperature] - rhs[Magnitude.Temperature],
                lhs[Magnitude.ElectricCurrent] - rhs[Magnitude.ElectricCurrent],
                lhs[Magnitude.AmountOfSubstance] - rhs[Magnitude.AmountOfSubstance],
                lhs[Magnitude.LuminousIntensity] - rhs[Magnitude.LuminousIntensity],
                lhs[Magnitude.Other] - rhs[Magnitude.Other]
            );
#else
            // The algoritm below is a modified binary subtraction of two's complement integers.
            // It performs parallel subtraction of 8 exponents numbers packed within dimension structure.
            // The subtraction is performed in UP TO 5 iterations (for 4-bit exponents) or 9 iteration (for 8-bit exponents).
            // This is significantly faster than old algorithm (above) that always requires 16 unpacks, 8 subtractions and 8 packs.
            // However it's very ugly (in that it's hard to see what is it doing) and that's why I left the old one as an option.
            DIMENSION difference = lhs.Exponents ^ rhs.Exponents;
            DIMENSION borrow = difference & rhs.Exponents;
            DIMENSION borrowed = 0;
            while (borrow != 0)
            {
                borrowed |= borrow;
                borrow &= CARRY;    // no bit can be carried over between adjacent exponent numbers
                borrow <<= 1;
                difference ^= borrow;
                borrow &= difference;
            }
            // Check overflow condition:                
            if (((borrowed ^ (borrowed << 1)) & ~CARRY) != 0)
            {
                throw new OverflowException($"{lhs} / {rhs}: {nameof(Dimension)} quotient out of range [{MINEXP},{MAXEXP}]");
            }

            return new Dimension(difference);
#endif
        }

        /// <summary>
        /// Raising dimension to a rational power.
        /// </summary>
        /// <param name="dim">dimension to be raised</param>
        /// <param name="num">numerator</param>
        /// <param name="den">denominator</param>
        /// <returns>raised dimension = dim^(num/den).</returns>
        /// <exception cref="ArgumentException">thrown when any of the result exponents would have to be fractional.</exception>
        /// <exception cref="OverflowException">thrown when any of the result exponents overflows permissible range.</exception>
        public static Dimension Pow(Dimension dim, int num, int den)
        {
            if (den == 0)
                throw new ArgumentException($"{nameof(Dimension)}.{nameof(Pow)}({dim}, {num}, {den}): illegal zero denominator.");

            if (num == 0)
                return Dimension.None;

            if (den == num)
                return dim;

            int length = dim[Magnitude.Length];
            int time = dim[Magnitude.Time];
            int mass = dim[Magnitude.Mass];
            int temperature = dim[Magnitude.Temperature];
            int current = dim[Magnitude.ElectricCurrent];
            int substance = dim[Magnitude.AmountOfSubstance];
            int intensity = dim[Magnitude.LuminousIntensity];
            int other = dim[Magnitude.Other];

            if (num % den == 0)
            {
                return new Dimension(
                    (num / den) * length,
                    (num / den) * time,
                    (num / den) * mass,
                    (num / den) * temperature,
                    (num / den) * current,
                    (num / den) * substance,
                    (num / den) * intensity,
                    (num / den) * other
                );
            }

            if ((length % den == 0) &&
                (time % den == 0) &&
                (mass % den == 0) &&
                (temperature % den == 0) &&
                (current % den == 0) &&
                (substance % den == 0) &&
                (intensity % den == 0) &&
                (other % den == 0))
            {
                return new Dimension(
                    (length / den) * num,
                    (time / den) * num,
                    (mass / den) * num,
                    (temperature / den) * num,
                    (current / den) * num,
                    (substance / den) * num,
                    (intensity / den) * num,
                    (other / den) * num
                );
            }

            throw new ArgumentException($"{nameof(Dimension)}.{nameof(Pow)}({dim}, {num}, {den}): cannot create dimension with fractional exponent(s)");
        }

        /// <summary>
        /// Raising dimension to an integer power.
        /// </summary>
        /// <param name="dim">dimension to be raised</param>
        /// <param name="num">power exponent (numerator)</param>
        /// <returns>dimension = dim^num.</returns>
        /// <exception cref="OverflowException">thrown when any of the result exponents overflows permissible range.</exception>
        public static Dimension Pow(Dimension dim, int num) => Pow(dim, num, 1);

        /// <summary>
        /// Square root of dimension.
        /// </summary>
        /// <param name="dim"></param>
        /// <returns>root dimension = dim^(1/2)</returns>
        /// <exception cref="ArgumentException">thrown when any the result exponents would have to be fractional.</exception>
        public static Dimension Sqrt(Dimension dim) => Pow(dim, 1, 2);

        /// <summary>
        /// Cubic root of dimension.
        /// </summary>
        /// <param name="dim"></param>
        /// <returns>root dimension = dim^(1/3)</returns>
        /// <exception cref="ArgumentException">thrown when any the result exponents would have to be fractional.</exception>
        public static Dimension Cubrt(Dimension dim) => Pow(dim, 1, 3);
        #endregion

        #region Formatting
        private static readonly string[] s_symbol =
        {
            "L",        /* Length */
            "T",        /* Time */
            "M",        /* Mass */
            "\u03F4"    /* Temperature (Θ - greek capital letter theta) */,
            "I",        /* Electric Current */
            "N",        /* Amount of Substance */
            "J",        /* Luminous Intensity */
            "\u00A4"    /* Money (¤ - generic currency sign) */
        };

        private string ExponentString(Magnitude magnitude)
        {
            int exponent = this[magnitude];
            return (exponent == 0) ? string.Empty :
                   (exponent == 1) ? s_symbol[(int)magnitude] :
                                     s_symbol[(int)magnitude] + exponent.ToString();
        }

        /// <summary>
        /// Returns dimension in a text form.
        /// </summary>
        /// <returns>dimension as a string</returns>
        public override string ToString()
        {
            string ret = ExponentString(Magnitude.Length) +
                        ExponentString(Magnitude.Time) +
                        ExponentString(Magnitude.Mass) +
                        ExponentString(Magnitude.Temperature) +
                        ExponentString(Magnitude.ElectricCurrent) +
                        ExponentString(Magnitude.AmountOfSubstance) +
                        ExponentString(Magnitude.LuminousIntensity) +
                        ExponentString(Magnitude.Other);

            return string.IsNullOrWhiteSpace(ret) ? "1" : ret;
        }
        #endregion
    }
}
