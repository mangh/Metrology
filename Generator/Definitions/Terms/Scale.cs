/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology
{
    /// <summary>
    /// Scale type definition.
    /// </summary>
    public class Scale : Measure
    {
        /// <summary>
        /// Default name of the common reference point (family).
        /// </summary>
        /// <remarks>
        /// NOTE: It is recommended to explicitly name the family (reference point)
        /// for each scale in the definition file instead of relying on the default value.
        /// </remarks>
        public static readonly string DefaultRefPoint = "<common reference point>";

        #region Properties
        /// <summary>
        /// The unit underlying the scale.
        /// </summary>
        public Unit Unit { get; private set; }

        /// <summary>
        /// Location of the family reference point relative to the scale point "0"
        /// (in <see cref="Unit"/> units).
        /// </summary>
        public NumExpr Offset { get; private set; }

        /// <summary>
        /// Family reference point name (id).
        /// </summary>
        public string RefPoint { get; private set; }

        /// <summary>
        /// Non-empty <see cref="RefPoint"/>.
        /// </summary>
        public string RefPointNormalized => string.IsNullOrWhiteSpace(RefPoint) ? DefaultRefPoint : RefPoint;

        /// <summary>
        /// Format for scale levels.
        /// </summary>
        /// <example>
        /// <c>"{0} {1}"</c> to have <c>273.15 Celsius</c> formatted as <c>"273.15 deg.C"</c>.
        /// </example>
        public string Format { get; private set; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Scale"/> constructor.
        /// </summary>
        /// <param name="name">Scale name.</param>
        /// <param name="type">Underlying numeric type.</param>
        /// <param name="refpoint">Family reference point name.</param>
        /// <param name="unit">Scale unit.</param>
        /// <param name="offset">Offset relative to the family reference point.</param>
        /// <param name="format">Formatting string.</param>
        public Scale(string name, Term type, string refpoint, Unit unit, NumExpr offset, string format)
            : base(name, type)
        {
            RefPoint = refpoint;
            Unit = unit;
            Offset = offset;
            Format = format;
        }
        #endregion

        #region Formatting
        /// <summary>
        /// Scale type info in a text form.
        /// </summary>
        /// <returns>Scale type stringified.</returns>
        public override string ToString() => $"[{Unit.Sense.Value}] {(Prime ?? this).TargetKeyword}::{TargetKeyword} : {RefPointNormalized} = {Unit.TargetKeyword} {Offset}";
        #endregion
    }
}
