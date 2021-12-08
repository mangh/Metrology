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
    /// <summary>
    /// METEO conditions:
    /// <list type="bullet">
    /// <item><description>U - wind speed</description></item>
    /// <item><description>BRG - wind direction</description></item>
    /// <item><description>CLAS - stability class</description></item>
    /// <item><description>MIXH - mixing height</description></item>
    /// <item><description>AMB - ambient concentration of pollutant</description></item>
    /// </list>
    /// </summary>
    public class Meteo
    {
        #region Constants
        private static readonly double[] _AZ = { 1112.0, 556.0, 353.0, 219.0, 124.0, 56.0 };
        private static readonly double[] _AY1 = { 0.46, 0.29, 0.18, 0.11, 0.087, 0.057 };
        private static readonly double[] _AY2 = { 1831.0, 1155.0, 717.0, 438.0, 346.0, 227.0 };

        /// <summary>Stability class tags.</summary>
        private static readonly string[] STB = { "A", "B", "C", "D", "E", "F" };
        #endregion

        #region Properties
        /// <summary>Wind speed [m/s].</summary>
        /// <remarks>
        /// Limits: U &#8805; 1 m/s.
        /// <para>
        /// NOTE: along-wind diffusion can be considered negligible relative to U &#8805; 1 m/s (Gaussian assumption).
        /// </para>
        /// </remarks>
        public readonly Meter_Sec U;

        /// <summary>
        /// Wind azimuth bearing [degrees] measured relative to positive Y-axis.
        /// <para>
        /// Example: BRG = 270&#176; for a wind from the West.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Limits: 0&#176; &#8804; BRG &#8804; 360&#176;.
        /// </remarks>
        public readonly Degree BRG;

        /// <summary>Wind angle for output (see: <see cref="BRG"/>).</summary>
        public readonly Degree BRG1;

        /// <summary>Mixing height [m].</summary>
        public readonly Meter MIXH;

        /// <summary>Ambient concentration of pollutant [ppm].</summary>
        public readonly PPM AMB;

        /// <summary>Stability class in numeric format (1-6=A-F).</summary>
        public readonly int CLAS;

        /// <summary>Stability class tag.</summary>
        public string TAG => STB[CLAS - 1];

        /// <summary>Meteo ordinal number.</summary>
        public readonly int ORDINAL;
        #endregion

        #region Coefficients (dependent on stability class) for the power-law formulas used in the Plume module
        /// <summary>
        /// Coefficient (1/2) used in computing σy (horizontal dispersion).
        /// </summary>
        public double AY1 => _AY1[CLAS - 1];

        /// <summary>
        /// Coefficient (2/2) used in computing σy (horizontal dispersion).
        /// </summary>
        public double AY2 => _AY2[CLAS - 1];

        /// <summary>
        /// Coefficient used in computing σz (vertical dispersion).
        /// </summary>
        public double AZ => _AZ[CLAS - 1];
        #endregion

        #region Constructor(s)
        public Meteo(int ordinal, Meter_Sec u, Degree brg, int clas, Meter mixh, PPM amb)
        {
            ORDINAL = ordinal;

            U = u;
            BRG1 = brg;
            CLAS = clas;
            MIXH = mixh;
            AMB = amb;

            // conversion to vector orientation
            brg += Const.DEGREE_180;
            BRG = (brg < Const.DEGREE_360) ? brg : brg - Const.DEGREE_360;
        }
        #endregion

        #region Formatting
        public override string ToString() => $"{ORDINAL}. Class {TAG} :: U={U} :: BRG={BRG1} :: MIXH={MIXH} :: AMB={AMB}";
        #endregion
    }
}
