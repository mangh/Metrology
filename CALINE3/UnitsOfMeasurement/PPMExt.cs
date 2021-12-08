/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace CALINE3.Metrology
{
    public partial struct PPM
    {
        #region Conversions
        // dimensionless:
        public static double FromFraction(double fraction) => Factor * fraction;

        // dimensional:
        public static PPM From(double fraction) => new(FromFraction(fraction));
        #endregion
    }
}
