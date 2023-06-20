/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

namespace Demo.UnitsOfMeasurement
{
    /// <summary>
    /// Level interface
    /// (to be implemented by scales).
    /// </summary>
    /// <typeparam name="T">
    /// A numeric type for implementing level arithmetic: <see cref="double"/>, <see cref="float"/> or <see cref="decimal"/>.
    /// </typeparam>
    public interface ILevel<T> where T : struct
    {
        /// <summary>
        /// Level relative to (distance from) the zero point of the <see cref="Scale"/>
        /// (in scale units).
        /// </summary>
        IQuantity<T> Level { get; }

        /// <summary>
        /// Level relative to (distance from) the common reference point of the family to which the <see cref="Scale"/> belongs
        /// (in scale units).
        /// </summary>
        IQuantity<T> ConvertibleLevel { get; }

        /// <summary>
        /// The scale against which the <see cref="Level"/> is measured.
        /// </summary>
        Scale<T> Scale { get; }
    }
}
