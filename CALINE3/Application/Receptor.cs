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
    /// RECEPTOR data:
    /// <list type="bullet">
    /// <item><description>RCP - description</description></item>
    /// <item><description>XR - X-coordinate</description></item>
    /// <item><description>YR - Y-coordinate</description></item>
    /// <item><description>ZR - Z-coordinate</description></item>
    /// </list>
    /// </summary>
    public class Receptor
    {
        #region Properties
        /// <summary>Receptor description.</summary>
        public readonly string RCP;

        /// <summary>Receptor X-coordinate [m].</summary>
        public readonly Meter XR;

        /// <summary>Receptor Y-coordinate [m].</summary>
        public readonly Meter YR;

        /// <summary>Receptor Z-coordinate [m].</summary>
        /// <remarks>
        /// Limits: ZR &#8805; 0 (Gaussian plume reflected at air-surface
        /// interface; model assumes plume transport over horizontal plane).
        /// <para>
        /// NOTE: for depressed sections ZR &#8805; H (where H &#8804; 0)
        /// is permitted for receptors within the section.
        /// </para>
        /// </remarks>
        public readonly Meter ZR;

        /// <summary>Receptor ordinal number.</summary>
        public readonly int ORDINAL;
        #endregion

        #region Constructor(s)
        public Receptor(int ordinal, string rcp, Meter xr, Meter yr, Meter zr)
        {
            ORDINAL = ordinal;

            RCP = rcp;
            XR = xr;
            YR = yr;
            ZR = zr;
        }
        #endregion

        #region Formatting
        /// <summary>Returns (basic) receptor information in a text form.</summary>
        public override string ToString() => $"{ORDINAL}. {RCP} :: X={XR} :: Y={YR} :: Z={ZR}";

        /// <summary>Returns receptor information, including pollutant concentration in its location, formatted for the output report.</summary>
        /// <example>The header the receptor string is to be matched to:
        /// <code>                            *        COORDINATES (M)        *  CO  </code>
        /// <code>       RECEPTOR             *      X        Y        Z      * (PPM)</code>
        /// <code>   -------------------------*-------------------------------*-------</code>
        /// </example>
        public string ToReport(int SEQNO, PPM VCSUM)
        {
            string coordFormat = "{0,9:F1}";
            string ppmFormat = "{0,5:F1}";

            return $"{SEQNO,5}. {RCP,-21}*  {_Coord(XR)}{_Coord(YR)}{_Coord(ZR)}   *{_Ppm(VCSUM)}";

#if DIMENSIONAL_ANALYSIS
            string _Coord(Meter q) =>   q.ToString(coordFormat);
            string _Ppm(PPM q) =>       q.ToString(ppmFormat);
#else
            string _Coord(Meter q) =>   Metrology.Meter.String(q, coordFormat);
            string _Ppm(PPM q) =>       Metrology.PPM.String(q, ppmFormat);
#endif
        }
        #endregion
    }
}
