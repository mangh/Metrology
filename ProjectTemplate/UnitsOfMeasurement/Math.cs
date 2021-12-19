/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Metrological.Namespace
{
    /// <summary>
    /// Mathematical methods supporting the Units of Measurement library.
    /// </summary>
    public static class Math
    {
        /*
         * Uncomment the below (and/or add new) math methods that are suitable
         * (or required) for your set of Units of Measurement.
         */

        #region Constants
        // public static readonly Radian PI = (Radian)System.Math.PI;
        #endregion

        #region Math methods bound to Units of Measurement
        // public static double Sin(Radian angle) => System.Math.Sin(angle.Value);
        // public static double Cos(Radian angle) => System.Math.Cos(angle.Value);
        // public static Radian Atan(double x) => new(System.Math.Atan(x));
        // public static Degree Abs(Degree d) => new(System.Math.Abs(d.Value));
        // public static Meter Abs(Meter d) => new(System.Math.Abs(d.Value));
        // public static Meter Sqrt(Meter2 area) => new(System.Math.Sqrt(area.Value));
        #endregion

        #region Some standard Math methods (not related to units but still required within their namespace)
        // public static double Abs(double x) => System.Math.Abs(x);
        // public static double Log(double x) => System.Math.Log(x);
        // public static double Exp(double x) => System.Math.Exp(x);
        // public static double Pow(double x, double y) => System.Math.Pow(x, y);
        // public static double Sqrt(double x) => System.Math.Sqrt(x);
        #endregion
    }
}
