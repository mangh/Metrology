/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

namespace Metrological.Namespace
{
    /// <summary>
    /// Quantity interface.
    /// </summary>
    /// <typeparam name="T">
    /// numeric type underlying the quantity and its unit: <see cref="double"/>, <see cref="float"/> or <see cref="decimal"/>.
    /// </typeparam>
    public interface IQuantity<T> where T : struct
    {
        /// <summary>
        /// Quantity value (as a number of <see cref="Unit"/>s).
        /// </summary>
        T Value { get; }

        /// <summary>
        /// The unit used to derive the <see cref="Value"/>.
        /// </summary>
        Unit<T> Unit { get; }
    }
}
