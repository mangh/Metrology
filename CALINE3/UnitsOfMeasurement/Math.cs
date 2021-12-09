/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace CALINE3.Metrology
{
    public static class Math
    {
        #region Constants
        public static readonly Radian PI = (Radian)System.Math.PI;
        #endregion

        #region Metrology Math (natural)
        public static double Sin(Radian angle) => System.Math.Sin(angle.Value);
        public static double Cos(Radian angle) => System.Math.Cos(angle.Value);
        public static Radian Atan(double x) => new(System.Math.Atan(x));
        public static Degree Abs(Degree d) => new(System.Math.Abs(d.Value));
        public static Meter Abs(Meter d) => new(System.Math.Abs(d.Value));
        public static Meter Sqrt(Meter2 area) => new(System.Math.Sqrt(area.Value));
        public static PPM Round(PPM ppm, int digits) => new(System.Math.Round(ppm.Value, digits));
        #endregion

        #region Metrology Math (hacks to cope with dimensionality in CALINE3 power-law formulas).
        /// <summary>
        /// Power of the length specified in <see cref="Meter"/> unit.
        /// </summary>
        /// <param name="length">length [m].</param>
        /// <param name="y">exponent (dimensionless)</param>
        /// <returns>the power of the specified length as a dimensionless number (i.e. w/o any unit).</returns>
        /// <remarks>
        /// NOTE: the method is dimensionally safe on the input but questionable
        /// on the output: the result has no (dimensional) relationship with the
        /// input unit <see cref="Meter"/>; there is a risk of using it in a wrong context.
        /// </remarks>
        public static double Pow(Meter length, double y) => System.Math.Pow(length.Value, y);
        #endregion

        #region Some standard Math
        public static double Abs(double x) => System.Math.Abs(x);
        public static double Log(double x) => System.Math.Log(x);
        public static double Exp(double x) => System.Math.Exp(x);
        public static double Pow(double x, double y) => System.Math.Pow(x, y);
        public static double Sqrt(double x) => System.Math.Sqrt(x);
        #endregion
    }
}
