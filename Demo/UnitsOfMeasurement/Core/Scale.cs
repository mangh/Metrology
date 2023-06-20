/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

using System;

namespace Demo.UnitsOfMeasurement
{
    /// <summary>
    /// Scale's base proxy
    /// (provides properties independent of the underlying numeric type).
    /// </summary>
    public abstract class Scale : Proxy
    {
        #region Constants
        public static readonly RuntimeTypeHandle GenericTypeHandle = typeof(Scale<>).TypeHandle;
        public static readonly string GenericTypeFullName = typeof(Scale<>).FullName!;
        public static readonly string GenericInterfaceFullName = typeof(ILevel<>).FullName!;
        #endregion

        #region Properties
        /// <summary>
        /// Scale family id.
        /// </summary>
        public abstract int Family { get; }

        /// <summary>
        /// Scale unit.
        /// </summary>
        public abstract Unit Unit { get; }

        /// <summary>
        /// Level formatting string.
        /// </summary>
        public abstract string Format { get; set; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Scale"/> constructor.
        /// </summary>
        /// <param name="scale">The original scale (of any value type) to be represented by this proxy.</param>
        /// <exception cref="NotSupportedException">Thrown by base class in the .NET Compact Framework environment.</exception>
        protected Scale(Type scale) :
            base(scale)
        {
        }
        #endregion

        #region Formatting
        public override string ToString() => $"[{Unit.Sense}] {Type.Name} {Unit.Symbol}";
        #endregion
    }

    /// <summary>
    /// Scale proxy
    /// (for the specific value type <typeparamref name="T"/>).
    /// </summary>
    /// <typeparam name="T">Value type underlying the scale (<see cref="double"/>, <see cref="decimal"/>, <see cref="float"/>).</typeparam>
    public abstract class Scale<T> : Scale
        where T : struct
    {
        #region Properties
        /// <summary>
        /// Offset relative to (distance from) the common reference point of the scale family.
        /// </summary>
        public abstract IQuantity<T> Offset { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Scale{T}"/> constructor.
        /// </summary>
        /// <param name="scale">The original scale (implementing the <see cref="ILevel{T}"/> interface) to be represented by this proxy.</param>
        /// <exception cref="System.ArgumentException"><paramref name="scale"/> argument is not a value type implementing <see cref="ILevel{T}"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown by base class in the .NET Compact Framework environment.</exception>
        protected Scale(Type scale) :
            base(scale)
        {
            if (!IsAssignableFrom(scale))
            {
                throw new ArgumentException($"\"{scale.Name}\" is not a scale type implementing {typeof(ILevel<T>).Name} interface.");
            }
        }
        /// <summary>
        /// Verifies whether the type is a scale i.e. a value type that implements the <see cref="ILevel{T}"/> interface.
        /// </summary>
        /// <param name="t">Type to be verified.</param>
        /// <returns><see langword="true"/> for a scale type, otherwise <see langword="false"/>.</returns>
        public static bool IsAssignableFrom(Type t) => t.IsValueType && typeof(ILevel<T>).IsAssignableFrom(t);
        #endregion

        #region Methods
        /// <summary>
        /// Creates <see cref="ILevel{T}"/> from a plain numeric <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Plain numeric value to be assigned to the level.</param>
        /// <returns><see cref="ILevel{T}"/> for the given numeric <paramref name="value"/>.</returns>
        public abstract ILevel<T> From(T value);

        /// <summary>
        /// Converts <paramref name="level"/> from one scale to another.
        /// </summary>
        /// <param name="level">Input level to be converted.</param>
        /// <returns>Converted level.</returns>
        public abstract ILevel<T> From(ILevel<T> level);

        /// <summary>
        /// Converts <see cref="IQuantity{T}"/> to <see cref="ILevel{T}"/> (pins <see cref="IQuantity{T}"/> to a <see cref="Scale{T}"/>).
        /// </summary>
        /// <param name="quantity">Input quantity to be converted.</param>
        /// <returns>Level corresponding to the <paramref name="quantity"/>.</returns>
        public abstract ILevel<T> From(IQuantity<T> quantity);
        #endregion
    }
}
