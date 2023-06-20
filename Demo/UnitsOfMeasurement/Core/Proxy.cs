/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/
using System;
using System.Reflection;

namespace Demo.UnitsOfMeasurement
{
    /// <summary>
    /// Unit or scale type proxy.
    /// </summary>
    public abstract class Proxy : IEquatable<Proxy>
    {
        #region Fields
        private readonly RuntimeTypeHandle m_handle;
        #endregion

        #region Properties
        public RuntimeTypeHandle Handle => m_handle;
        public Type Type => Type.GetTypeFromHandle(m_handle)!;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Proxy"/> constructor.
        /// </summary>
        /// <param name="t">Unit or scale type to be represented by this proxy.</param>
        /// <exception cref="NotSupportedException">The .NET Compact Framework does not support <see cref="Type.TypeHandle"/> property.</exception>
        protected Proxy(Type t) => m_handle = t.TypeHandle;
        #endregion

        #region Equality
        public override int /* IObject */ GetHashCode() => m_handle.GetHashCode();
        public override bool /* IObject */ Equals(object? obj) => (obj is Proxy m) && Equals(m);
        public bool /* IEquatable<Proxy> */ Equals(Proxy? other) => (other is not null) && m_handle.Equals(other.Handle);
        #endregion

        #region Statics
        /// <summary>
        /// Tries to retrieve <see cref="Proxy"/> from a type.
        /// </summary>
        /// <param name="t">Input type to retrieve proxy from.</param>
        /// <returns>Proxy for a <see cref="Unit{T}"/> (or <see cref="Scale{T}"/>) input type; <see langword="null"/> for any other types.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FieldAccessException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="TargetException"></exception>
        /// <exception cref="TargetInvocationException"></exception>
        public static Proxy? TryRetrieveFrom(Type? t)
        {
            if ((t is not null) && t.IsValueType)
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
