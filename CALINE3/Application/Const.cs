/*******************************************************************************

    Units of Measurement for C# applications applied to
    the CALINE3 Model algorithm.

    For more information on CALINE3 and its status see:
    • https://www.epa.gov/scram/air-quality-dispersion-modeling-alternative-models#caline3
    • https://www.epa.gov/scram/2017-appendix-w-final-rule.

    Copyright (C) mangh

    This program is provided to you under the terms of the license 
    as published at https://github.com/mangh/metrology.

********************************************************************************/

namespace CALINE3
{
    internal static class Const
    {
        public static readonly double SQRT_2 = Sqrt(2.0);
        public static readonly double SQRT_2PI = Sqrt(2.0 * System.Math.PI);

        // Angles
        public static readonly Degree DEGREE_360 = (Degree)360.0;
        public static readonly Degree DEGREE_270 = (Degree)270.0;
        public static readonly Degree DEGREE_180 = (Degree)180.0;
        public static readonly Degree DEGREE_90 = (Degree)90.0;
    }
}
