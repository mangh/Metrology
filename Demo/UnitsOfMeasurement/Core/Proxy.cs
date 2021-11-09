/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/unitsofmeasurement


********************************************************************************/
using System;
using System.Reflection;

namespace Demo.UnitsOfMeasurement
{
    /// <summary>
    /// Proxy for either a unit type or a scale type (unit and scale common properties).
    /// </summary>
    public abstract class Proxy : IEquatable<Proxy>
    {
        #region Fields
        private readonly RuntimeTypeHandle m_handle;
        #endregion

        #region Properties
        public RuntimeTypeHandle Handle => m_handle;
        public Type Type => Type.GetTypeFromHandle(m_handle);
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Proxy constructor.
        /// </summary>
        /// <param name="t">unit or scale type to be represented by this proxy.</param>
        protected Proxy(Type t) => m_handle = t.TypeHandle;
        #endregion

        #region Equality
        public override int /* IObject */ GetHashCode() => m_handle.GetHashCode();
        public override bool /* IObject */ Equals(object? obj) => (obj is Proxy m) && Equals(m);
        public bool /* IEquatable<Proxy> */ Equals(Proxy? other) => (other is not null) && m_handle.Equals(other.Handle);
        #endregion

        #region Statics
        /// <summary>
        /// Retrieves <c>Unit&lt;T&gt;</c> (or <c>Scale&lt;T&gt;</c>) proxy from a type expected to implement interface <c>IQuantity&lt;T&gt;</c> (or <c>ILevel&lt;T&gt;</c> as appropriate).
        /// </summary>
        /// <param name="t">Input type to retrieve proxy from.</param>
        /// <returns><c>Unit&lt;T&gt;</c> (or <c>Scale&lt;T&gt;</c>) proxy for a unit (or a scale) input type; <c>null</c> for any other types.</returns>
        public static Proxy? TryRetrieveFrom(Type? t)
        {
            if ((t is not null) && (t.IsValueType))
            {
                foreach (Type ifc in t.GetInterfaces())
                {
                    if ((ifc.FullName is not null) && 
                        (ifc.FullName.StartsWith(Unit.GenericInterfaceFullName) || 
                        ifc.FullName.StartsWith(Scale.GenericInterfaceFullName)))
                    {
                        FieldInfo? fieldInfo = t.GetField("Proxy", BindingFlags.Static | BindingFlags.Public);
                        if (fieldInfo is not null)
                        {
                            return fieldInfo.GetValue(t) as Proxy;
                        }
                    }
                }
            }
            return null;
        }
        #endregion
    }
}
