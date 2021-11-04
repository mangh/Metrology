/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Man.Metrology
{
    /// <summary>
    /// Scale type info.
    /// </summary>
    public class ScaleType : MeasureType
    {
        /// <summary>
        /// Default name for a scale&apos; family common reference point.
        /// </summary>
        /// <remarks>
        /// NOTE: It is recommended to explicitly name the common reference point in
        /// definition file instead of relying on this default.
        /// </remarks>
        public static readonly string DefaultRefPoint = "<common reference point>";

        #region Properties
        /// <summary>
        /// Unit underlying the scale.
        /// </summary>
        public UnitType Unit { get; private set; }

        /// <summary>
        /// Offset to the scale family common reference point.
        /// (in <see cref="Unit"/> units).
        /// </summary>
        public NumeralExpression Offset { get; private set; }

        /// <summary>
        /// Name for a scale&apos; family common reference point as specified definition file.
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
        /// Scale type constructor.
        /// </summary>
        /// <param name="name">scale type name.</param>
        /// <param name="refpoint">name of the scale&apos; family reference point.</param>
        /// <param name="unit">unit underlying the scale.</param>
        /// <param name="offset">Offset to the scale&apos; family common reference point</param>
        /// <param name="format">format for the scale levels.</param>
        public ScaleType(string name, string refpoint, UnitType unit, NumeralExpression offset, string format) :
            base(name)
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
        /// <returns>ScaleType stringified.</returns>
        public override string ToString() => $"[{Unit.Sense.Value}] {(Prime ?? this).Typename}::{Typename} : {RefPointNormalized} = {Unit.Typename} {Offset.Value}";
        #endregion
    }
}
