/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

namespace Metrological.Namespace
{
    /// <summary>
    /// Scale level interface.
    /// </summary>
    /// <typeparam name="T">
    /// numeric type underlying the level and its scale: <see cref="double"/>, <see cref="float"/> or <see cref="decimal"/>.
    /// </typeparam>
    public interface ILevel<T> where T : struct
    {
        /// <summary>
        /// Level relative to the <see cref="Scale"/> point zero
        /// (in <see cref="Scale.Unit"/> units).
        /// </summary>
        IQuantity<T> Level { get; }

        /// <summary>
        /// Level relative to the <see cref="Scale"/> family common reference point
        /// (in <see cref="ILevel{t}.Scale.Unit"/> units).
        /// </summary>
        IQuantity<T> ConvertibleLevel { get; }

        /// <summary>
        /// The scale against which the <see cref="Level"/> is measured.
        /// </summary>
        Scale<T> Scale { get; }
    }
}
