/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

using Mangh.Metrology;
using System;

namespace Demo.UnitsOfMeasurement
{
    /// <summary>
    /// Unit's base proxy
    /// (provides properties independent of the underlying numeric type).
    /// </summary>
    public abstract class Unit : Proxy
    {
        #region Constants
        public static readonly RuntimeTypeHandle GenericTypeHandle = typeof(Unit<>).TypeHandle;
        public static readonly string GenericTypeFullName = typeof(Unit<>).FullName!;
        public static readonly string GenericInterfaceFullName = typeof(IQuantity<>).FullName!;
        #endregion

        #region Properties
        /// <summary>
        /// Unit family id.
        /// </summary>
        public abstract int Family { get; }

        /// <summary>
        /// Unit dimension.
        /// </summary>
        public abstract Dimension Sense { get; }

        /// <summary>
        /// Unit symbols (tags).
        /// </summary>
        public abstract SymbolCollection Symbol { get; }

        /// <summary>
        /// Quantity formatting string.
        /// </summary>
        public abstract string Format { get; set; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Unit"/> constructor.
        /// </summary>
        /// <param name="unit">The original unit (of any value type) to be represented by this proxy.</param>
        /// <exception cref="NotSupportedException">Thrown by base class in the .NET Compact Framework environment.</exception>
        protected Unit(Type unit) :
            base(unit)
        {
        }
        #endregion

        #region Formatting
        public override string ToString() => $"[{Sense}] {Type.Name} {Symbol}";
        #endregion
    }

    /// <summary>
    /// Unit proxy (for the specific value type <typeparamref name="T"/>).
    /// </summary>
    /// <typeparam name="T">Value type underlying the unit (<see cref="double"/>, <see cref="decimal"/>, <see cref="float"/>).</typeparam>
    public abstract class Unit<T> : Unit
        where T : struct
    {
        #region Properties
        /// <summary>
        /// Unit alias (name) to be used when reporting exceptions.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Unit conversion factor.
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// As a rule, conversion factors are constant for all units - except monetary.<br/>
        /// Monetary units have to override both getter and setter below (to allow for a factor change).<br/>
        /// All other units have to override the getter only and leave the setter as-is i.e. raise exception<br/>
        /// on an attempt to change the conversion factor.
        /// </remarks>
        public virtual T Factor
        {
            get { throw new NotImplementedException($"{Alias}.Factor Proxy getter not implemented."); }
            set { throw new InvalidOperationException($"{Alias}.Factor is constant and cannot be re-set via its Proxy."); }
        }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Unit{T}"/> constructor.
        /// </summary>
        /// <param name="unit">The original unit (implementing the <see cref="IQuantity{T}"/> interface) to be represented by this proxy.</param>
        /// <exception cref="System.ArgumentException"><paramref name="unit"/> argument is not a value type implementing <see cref="IQuantity{T}"/> interface.</exception>
        /// <exception cref="NotSupportedException">Thrown by base class in the .NET Compact Framework environment.</exception>
        protected Unit(Type unit) :
            base(unit)
        {
            Alias = unit.Name;
            if (!IsAssignableFrom(unit))
            {
                throw new ArgumentException($"\"{Alias}\" is not a unit type implementing {typeof(IQuantity<T>).Name} interface.");
            }
        }
        /// <summary>
        /// Verifies whether the type is a unit i.e. a value type implementing the <see cref="IQuantity{T}"/> interface.
        /// </summary>
        /// <param name="t">Type to be verified.</param>
        /// <returns><see langword="true"/> for a unit type, otherwise <see langword="false"/>.</returns>
        public static bool IsAssignableFrom(Type t) => t.IsValueType && typeof(IQuantity<T>).IsAssignableFrom(t);
        #endregion

        #region Methods
        /// <summary>
        /// Creates quantity (in <see cref="Unit{T}"/> units) from a plain numeric (unitless) <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Plain numeric value.</param>
        /// <returns>Quantity (in <see cref="Unit{T}"/> units) for the the given numeric <paramref name="value"/>.</returns>
        public abstract IQuantity<T> From(T value);

        /// <summary>
        /// Converts <paramref name="quantity"/> from one unit to another.
        /// </summary>
        /// <param name="quantity">Input quantity to be converted.</param>
        /// <returns>Quantity (in <see cref="Unit{T}"/> units) converted from the <paramref name="quantity"/> (in some other units).</returns>
        public abstract IQuantity<T> From(IQuantity<T> quantity);
        #endregion
    }
}
