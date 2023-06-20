/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

namespace Demo.UnitsOfMeasurement
{
    /// <summary>
    /// Quantity interface
    /// (to be implemented by units).
    /// </summary>
    /// <typeparam name="T">
    /// A numeric type for implementing quantity arithmetic: <see cref="double"/>, <see cref="float"/> or <see cref="decimal"/>.
    /// </typeparam>
    public interface IQuantity<T> where T : struct
    {
        /// <summary>
        /// The numerical value of the quantity (in units <see cref="Unit"/>).
        /// </summary>
        T Value { get; }

        /// <summary>
        /// The unit of measurement in which <see cref="Value"/> was measured.
        /// </summary>
        Unit<T> Unit { get; }
    }
}
