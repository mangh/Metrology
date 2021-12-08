/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/unitsofmeasurement


********************************************************************************/

using System;

namespace CALINE3.Metrology
{
    /// <summary>
    /// Scale proxy base (common properties independent of value type).
    /// </summary>
    public abstract class Scale : Proxy
    {
        #region Constants
        public static readonly RuntimeTypeHandle GenericTypeHandle = typeof(Scale<>).TypeHandle;
        public static readonly string GenericTypeFullName = typeof(Scale<>).FullName!;
        public static readonly string GenericInterfaceFullName = typeof(ILevel<>).FullName!;
        #endregion

        #region Properties
        public abstract int Family { get; }
        public abstract Unit Unit { get; }
        public abstract string Format { get; set; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Create scale proxy from a <paramref name="scale"/> type.
        /// </summary>
        /// <param name="scale">scale of any value type.</param>
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
    /// Scale proxy (specific for the value type parameter <typeparamref name="T"/>).
    /// </summary>
    /// <typeparam name="T">value type underlying the scale (<see cref="double"/>, <see cref="decimal"/>, <see cref="float"/>).</typeparam>
    public abstract class Scale<T> : Scale
        where T : struct
    {
        #region Properties
        public abstract IQuantity<T> Offset { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Creates scale proxy from a <paramref name="scale"/> type.
        /// </summary>
        /// <param name="scale">scale value type implementing <see cref="ILevel{T}"/> interface.</param>
        /// <exception cref="System.ArgumentException">thrown when <paramref name="scale"/> argument is not a value type implementing <see cref="ILevel{T}"/>.</exception>
        protected Scale(Type scale) :
            base(scale)
        {
            if (!IsAssignableFrom(scale))
                throw new ArgumentException($"\"{scale.Name}\" is not a scale type implementing {typeof(ILevel<T>).Name} interface.");
        }
        /// <summary>
        /// Verifies whether the type is a value type implementing <see cref="ILevel{T}"/> interface.
        /// </summary>
        /// <param name="t">type to be verified.</param>
        /// <returns>"true" for valid scale type, "false" otherwise.</returns>
        public static bool IsAssignableFrom(Type t) => t.IsValueType && typeof(ILevel<T>).IsAssignableFrom(t);
        #endregion

        #region Methods
        /// <summary>
        /// Creates <see cref="Scale{T}"/> level from a plain <paramref name="value"/>.
        /// </summary>
        /// <param name="value">value to be assigned to the level.</param>
        /// <returns><see cref="Scale{T}"/>level for the given <paramref name="value"/>.</returns>
        public abstract ILevel<T> From(T value);

        /// <summary>
        /// Converts <paramref name="level"/> from its scale the level on this <see cref="Scale{T}"/>.
        /// </summary>
        /// <param name="level">level (likely on another scale) to be converted from.</param>
        /// <returns>level converted to this <see cref="Scale{T}"/>.</returns>
        public abstract ILevel<T> From(ILevel<T> level);

        /// <summary>
        /// Converts <paramref name="quantity"/> to a level of this <see cref="Scale{T}"/>.
        /// </summary>
        /// <param name="quantity">quantity to be converted from.</param>
        /// <returns>level on this <see cref="Scale{T}"/>.</returns>
        public abstract ILevel<T> From(IQuantity<T> quantity);
        #endregion
    }
}
