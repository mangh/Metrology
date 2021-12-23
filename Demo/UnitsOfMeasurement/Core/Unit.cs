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
    /// Unit proxy base (common unit properties independent of value type).
    /// </summary>
    public abstract class Unit : Proxy
    {
        #region Constants
        public static readonly RuntimeTypeHandle GenericTypeHandle = typeof(Unit<>).TypeHandle;
        public static readonly string GenericTypeFullName = typeof(Unit<>).FullName!;
        public static readonly string GenericInterfaceFullName = typeof(IQuantity<>).FullName!;
        #endregion

        #region Properties
        public abstract int Family { get; }
        public abstract Dimension Sense { get; }
        public abstract SymbolCollection Symbol { get; }
        public abstract string Format { get; set; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Create unit proxy from a <paramref name="unit"/> type.
        /// </summary>
        /// <param name="unit">unit of any value type.</param>
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
    /// Unit proxy (specific for the value type parameter <typeparamref name="T"/>).
    /// </summary>
    /// <typeparam name="T">value type underlying the unit (<see cref="double"/>, <see cref="decimal"/>, <see cref="float"/>).</typeparam>
    public abstract class Unit<T> : Unit
        where T : struct
    {
        #region Properties
        public string Alias { get; private set; }

        // Basically, factors for all units - except monetary - are constant. 
        // Therefore monetary units have to override both getter and setter accessors.
        // All other, "normal" units have to override the getter only and leave the setter as-is
        // i.e. raise exception on attempt to change the factor.
        public virtual T Factor
        {
            get { throw new NotImplementedException($"{Alias}.Factor Proxy getter not implemented."); }
            set { throw new InvalidOperationException($"{Alias}.Factor is constant and cannot be re-set via its Proxy."); }
        }
        #endregion

        #region Constructor(s)

        /// <summary>Creates unit proxy from a <paramref name="unit"/> type.</summary>
        /// <param name="unit">unit type implementing <see cref="IQuantity{T}"/> interface.</param>
        /// <exception cref="System.ArgumentException">thrown when <paramref name="unit"/> argument is not a value type implementing <see cref="IQuantity{T}"/> interface.</exception>
        protected Unit(Type unit) :
            base(unit)
        {
            Alias = unit.Name;
            if (!IsAssignableFrom(unit))
            {
                throw new ArgumentException($"\"{Alias}\" is not a unit type implementing {typeof(IQuantity<T>).Name} interface.");
            }
        }
        /// <summary>Verifies whether the type is a unit type implementing <see cref="IQuantity{T}"/> interface.</summary>
        /// <param name="t">type to be verified.</param>
        /// <returns>"true" for a valid unit type, "false" otherwise.</returns>
        public static bool IsAssignableFrom(Type t) => t.IsValueType && typeof(IQuantity<T>).IsAssignableFrom(t);
        #endregion

        #region Methods
        /// <summary>Creates <see cref="Unit{T}"/> quantity from a plain <paramref name="value"/>.</summary>
        /// <param name="value">value to be assigned to the quantity.</param>
        /// <returns><see cref="Unit{T}"/> quantity for the the given <paramref name="value"/>.</returns>
        public abstract IQuantity<T> From(T value);

        /// <summary>Converts quantity to the unit of measurement represented by this <see cref="Unit{T}"/>.</summary>
        /// <param name="quantity">Quantity to be converted from.</param>
        /// <returns>quantity converted to the unit of measurement represented by this <see cref="Unit{T}"/>.</returns>
        public abstract IQuantity<T> From(IQuantity<T> quantity);
        #endregion
    }
}
