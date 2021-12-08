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

using System.IO;

namespace CALINE3
{
    /// <summary>
    /// LINK (raw data independent of <see cref="Meteo"/> conditions):
    /// <list type="bullet">
    /// <item><description>LNK - link title</description></item>
    /// <item><description>TYP - highway type</description></item>
    /// <item><description>XL1 - x-coordinates of link endpoint 1</description></item>
    /// <item><description>YL1 - y-coordinates of link endpoint 1</description></item>
    /// <item><description>XL2 - x-coordinates of link endpoint 2</description></item>
    /// <item><description>YL2 - y-coordinates of link endpoint 2</description></item>
    /// <item><description>VPHL - traffic volume</description></item>
    /// <item><description>EFL - emission factor</description></item>
    /// <item><description>HL - source height</description></item>
    /// <item><description>WL - mixing zone width</description></item>
    /// </list>
    /// </summary>
    public class Link
    {
        #region Constants
        public static readonly Meter MIN_HEIGHT = (Meter)(-10.0);
        public static readonly Meter MAX_HEIGHT = (Meter)10.0;

        public static readonly Meter MAX_LENGTH = (Meter)10000.0;

        public static readonly Meter DEPRESSED_SECTION_BOUNDARY = (Meter)(-1.5);

        /// <value>Link tags</value>
        private static readonly string[] TAG =
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
            "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T"
        };
        #endregion

        #region Basic Properties
        /// <summary>Link title (description)</summary>
        public readonly string LNK;

        /// <summary>Highway type: AJ=At-Grade, FL=Fill, BR=Bridge, DP=Depressed.</summary>
        public readonly string TYP;

        /// <summary>X-coordinate of link endpoint 1 [m].</summary>
        public readonly Meter XL1;

        /// <summary>Y-coordinate of link endpoint 1 [m].</summary>
        public readonly Meter YL1;

        /// <summary>X-coordinate of link endpoint 2 [m].</summary>
        public readonly Meter XL2;

        /// <summary>Y-coordinate of link endpoint 2 [m].</summary>
        public readonly Meter YL2;

        /// <summary>Traffic volume [vehicles/hour].</summary>
        public readonly Event_Hour VPHL;

        /// <summary>Emission factor [g/mile/vehicle].</summary>
        public readonly Gram_Mile EFL;

        /// <summary>Link height [m].</summary>
        /// <remarks>Limits: -10 m &#8804; HL &#8804; 10 m: not verified outside of that range.</remarks>
        public readonly Meter HL;

        /// <summary>Link width [m] (mixing zone width).</summary>
        /// <remarks>Limits: WL &#8805; 10 m: minimum of 1 lane plus 3 meters per side of link.</remarks>
        public readonly Meter WL;

        /// <summary>Link ordinal number.</summary>
        public readonly int ORDINAL;

        /// <summary>Link tag</summary>
        public string COD => TAG[ORDINAL];
        #endregion

        #region Derived Properties

        /// <summary>Height <see cref="HL"/> [m] adjusted for the link type.</summary>
        public readonly Meter H;

        /// <summary>Link length [m] as defined by endpoint coordinates.</summary>
        /// <remarks>
        /// Limits: <see cref="WL"/> &#8804; <see cref="LL"/> &#8804; 10 km (there
        /// must be <see cref="WL"/> &#8804; <see cref="LL"/> for correct element resolution,
        /// and <see cref="LL"/> &#8804; 10 km since vertical dispersion curve approximations
        /// are only valid for downwind distances of 10 km or less).
        /// </remarks>
        public readonly Meter LL;

        /// <summary>Link bearing [degrees] for output.</summary>
        public readonly Degree LBRG;

        /// <summary>Highway half-width [m] (that is: <see cref="WL"/> / 2).</summary>
        public readonly Meter W2;

        /// <summary>Lineal source strength parallel to the highway [&#956;g/(m*s)].</summary>
        /// <remarks>
        /// <see cref="Q1"/> = 0,172603109 * <see cref="VPHL"/> * <see cref="EFL"/>
        /// <para>
        /// [vehicles/hr] * [g/mile/vehicle] = 
        ///      10^6/3600/1609,344 [&#956;g/(meter*sec)] =
        ///      0,172603109 [&#956;g/(meter*sec)]
        /// </para>
        /// </remarks>
        public readonly Microgram_MeterSec Q1;

        /// <summary>Height <see cref="HL"/> [m] adjusted for the depressed section.</summary>
        private readonly Meter HDS;

        /// <summary>Residence time factor for depressed section.</summary>
        public readonly double DSTR;
        #endregion

        #region Constructor(s)
        public Link(int ordinal, string lnk, string typ, Meter xl1, Meter yl1, Meter xl2, Meter yl2, Event_Hour vphl, Gram_Mile efl, Meter hl, Meter wl)
        {
            ORDINAL = ordinal;

            LNK = lnk;
            TYP = typ;
            XL1 = xl1;
            YL1 = yl1;
            XL2 = xl2;
            YL2 = yl2;
            VPHL = vphl;
            EFL = efl;
            HL = hl;
            WL = wl;

            LL = Euclid2D.Distance(XL1, YL1, XL2, YL2);

            if (LL < WL)
            {
                throw new InvalidDataException($"Link {LNK} length ({LL}) must be greater than or equal to link width ({WL})");
            }
            if ((HL < MIN_HEIGHT) || (MAX_HEIGHT < HL))
            {
                throw new InvalidDataException($"Source must be within 10 meters of datum (HL[{LNK}] = {HL})");
            }

            // Height adjusted for the link type
            H = ((TYP == "DP") || (TYP == "FL")) ? (Meter)0.0 : HL;

            // Highway half-width
            W2 = WL / 2.0;

            // Adjustments for a depressed section:
            if (HL < DEPRESSED_SECTION_BOUNDARY)
            {
                // For depressed sections greater than 1.5 meters deep, CALINE3 increases the
                // residence time within the mixing zone by the following empirically derived
                // factor based on Los Angeles data:
                HDS = HL;
                DSTR = 0.72 * Pow(Abs(HDS), 0.83);
            }
            else
            {
                HDS = (Meter)1.0;
                DSTR = 1.0;
            }

            // Link bearing
#if DIMENSIONAL_ANALYSIS
            LBRG = Degree.From(Euclid2D.Azimuth(XL1, YL1, XL2, YL2));
#else
            LBRG = Metrology.Degree.FromRadian(Euclid2D.Azimuth(XL1, YL1, XL2, YL2));
#endif

            // Lineal source strength
#if DIMENSIONAL_ANALYSIS
            Q1 = Microgram_Meter.From(EFL) *
                Hertz.From(VPHL);
#else
            Q1 = Metrology.Microgram_Meter.FromGram_Mile(EFL) *
                Metrology.Hertz.FromEvent_Hour(VPHL);
#endif
        }
        #endregion

        #region CALINE3 calculations
        /// <summary>Get receptor coordinates relative to the link start position.</summary>
        /// <param name="rcp">receptor</param>
        /// <returns>
        /// A tuple (D, L, Z) where:
        /// <list type="bullet">
        /// <item><description>D - receptor-link distance, measured perpendicular to the link,</description></item>
        /// <item><description>L - receptor offset relative to the link start position, measured parallel to the link,</description></item>
        /// <item><description>Z - receptor level adjusted for the link type.</description></item>
        /// </list>
        /// </returns>
        public (Meter D, Meter L, Meter Z) TransformReceptorCoordinates(Receptor rcp)
        {
            Meter LR = Euclid2D.Distance(XL1, YL1, rcp.XR, rcp.YR);

            // Receptor angle with respect to link
#if DIMENSIONAL_ANALYSIS
            Radian lbrg = Radian.From(LBRG);
#else
            Radian lbrg = Metrology.Radian.FromDegree(LBRG);
#endif
            Radian GAMMA = Euclid2D.Azimuth(XL1, YL1, rcp.XR, rcp.YR) - lbrg;

            Meter D = LR * Sin(GAMMA);
            Meter L = LR * Cos(GAMMA) - LL;

            Meter Z = rcp.ZR;
            if ((TYP != "AG") && (TYP != "BR"))
            {
                Meter D1 = W2 + 2.0 * Abs(HL);
                Meter D2 = W2;
                if (Abs(D) < D1)
                {
                    // 2:1 SLOPE ASSUMED
                    Z -= (Abs(D) <= D2) ? HL : HL * (1.0 - (Abs(D) - W2) / (2.0 * Abs(HL)));
                }
            }

            return (D, L, Z);
        }

        /// <summary>
        /// Depressed section factor for a receptor at the distance <paramref name="D"/>.
        /// </summary>
        /// <param name="D">receptor-link distance</param>
        /// <returns>Depressed section factor.</returns>
        public double DepressedSectionFactor(Meter D) =>
            ((HDS >= DEPRESSED_SECTION_BOUNDARY) || (Abs(D) >= (W2 - 3.0 * HDS))) ? 1.0 :
                (Abs(D) <= W2) ? DSTR : DSTR - (DSTR - 1.0) * (Abs(D) - W2) / (-3.0 * HDS);

        #endregion

        #region Formatting
        /// <summary>Returns (basic) link information in a text form.</summary>
        public override string ToString()
            => $"{ORDINAL}. {LNK} ({TYP}) :: BRG={LBRG} :: LL={LL} :: ({XL1}, {YL1}) to ({XL2}, {YL2}) :: VPHL={VPHL} :: EFL={EFL}";

        /// <summary>Returns link information, formatted for the output report.</summary>
        /// <example>The header the link string is to be matched to:
        /// <code>       LINK DESCRIPTION     *      LINK COORDINATES (M)      * LINK LENGTH  LINK BRG   TYPE  VPH     EF     H    W   </code>
        /// <code>                            *   X1      Y1      X2      Y2   *     (M)       (DEG)                 (G/MI)  (M)  (M)  </code>
        /// <code>   -------------------------*--------------------------------*-------------------------------------------------------</code>
        /// </example>
        public string ToReport()
        {
            string coordFormat = "{0,8:F1}";
            string lengthFormat = "{0,9:F0}";
            string brgFormat = "{0,10:F0}";
            string vphlFormat = "{0,7:F0}";
            string eflFormat = "{0,7:F1}";
            string sizeFormat = "{0,6:F1}";

            return $"{COD,4}. {LNK,-22}*{_Coord(XL1)}{_Coord(YL1)}{_Coord(XL2)}{_Coord(YL2)}*{_Length(LL)}{_Brg(LBRG)}{TYP,9}{_Vphl(VPHL)}{_Efl(EFL)}{_Size(HL)}{_Size(WL)}";

#if DIMENSIONAL_ANALYSIS
            string _Coord(Meter q) =>       q.ToString(coordFormat);
            string _Length(Meter q) =>      q.ToString(lengthFormat);
            string _Brg(Degree q) =>        q.ToString(brgFormat);
            string _Vphl(Event_Hour q) =>   q.ToString(vphlFormat);
            string _Efl(Gram_Mile q) =>     q.ToString(eflFormat);
            string _Size(Meter q) =>        q.ToString(sizeFormat);
#else
            string _Coord(Meter q) =>       Metrology.Meter.String(q, coordFormat);
            string _Length(Meter q) =>      Metrology.Meter.String(q, lengthFormat);
            string _Brg(Degree q) =>        Metrology.Degree.String(q, brgFormat);
            string _Vphl(Event_Hour q) =>   Metrology.Event_Hour.String(q, vphlFormat);
            string _Efl(Gram_Mile q) =>     Metrology.Gram_Mile.String(q, eflFormat);
            string _Size(Meter q) =>        Metrology.Meter.String(q, sizeFormat);
#endif
        }
        #endregion
    }
}
