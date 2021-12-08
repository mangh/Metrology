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

using System.Collections.Generic;

namespace CALINE3
{
    /// <summary>JOB data:
    /// <list type="bullet">
    /// <item><description>JOB - job title (description)</description></item>
    /// <item><description>ATIM - averaging time</description></item>
    /// <item><description>Z0 - surface roughness</description></item>
    /// <item><description>VS - settling velocity</description></item>
    /// <item><description>VD - deposition velocity</description></item>
    /// <item><description>NR - number of receptors</description></item>
    /// <item><description>SCAL - scale factor to convert lengths in meters</description></item>
    /// <item><description>RUN - run description</description></item>
    /// </list>
    /// and
    /// <list type="bullet">
    /// <item><description>Links - link list</description></item>
    /// <item><description>Receptors - receptor list</description></item>
    /// <item><description>Meteos - meteo conditions</description></item>
    /// </list>
    /// </summary>
    public class Job
    {
        #region Constants
        // Averaging times 
        public static readonly Minute ATIM_3MIN = (Minute)3.0;
        public static readonly Minute ATIM_30MIN = (Minute)30.0;

        // Surface roughness
        public static readonly Centimeter Z0_3CM = (Centimeter)3.0;
        public static readonly Centimeter Z0_10CM = (Centimeter)10.0;
        #endregion

        #region Properties
        /// <summary>Job title (description).</summary>
        public readonly string JOB;

        /// <summary>Averaging time [minutes].</summary>
        /// <remarks>Limits: 3 min &#8804; ATIM &#8804; 120 min (reasonable limits of power law approximation).</remarks>
        public readonly Minute ATIM;

        /// <summary>Surface roughness [cm].</summary>
        /// <remarks>Limits: 3 cm &#8804; Z0 &#8804; 400 cm (reasonable limits of power law approximation).</remarks>
        public readonly Centimeter Z0;

        /// <summary>
        /// Settling velocity [m/sec]
        /// i.e. the rate at which a particle falls with respect to its immediate surroundings.
        /// </summary>
        public readonly Meter_Sec VS;

        /// <summary>Settling velocity [cm/sec] (for output, see: <see cref="VS"/>).</summary>
        public readonly Centimeter_Sec VS1;

        /// <summary>
        /// Deposition velocity [m/sec]
        /// i.e. the rate at which pollutant can be adsorbed or assimilated by a surface.
        /// </summary>
        /// <remarks>If the settling velocity <see cref="VS"/> > 0, the deposition velocity should be set equal to the settling velocity.</remarks>
        public readonly Meter_Sec VD;

        /// <summary>Deposition velocity [cm/sec] (for output, see: <see cref="VD"/>).</summary>
        public readonly Centimeter_Sec VD1;

        /// <summary>(<see cref="VD"/> - <see cref="VS"/>) / 2.0 [m/sec].</summary>
        public readonly Meter_Sec V1;

        /// <summary>Number of receptors.</summary>
        /// <remarks>Limits: NR &#8804; 20.</remarks>
        public readonly int NR;

        /// <summary>Scale factor to convert coordinates, link height and width to meters.</summary>
        /// <remarks>SCAL=1.0 if coordinates, link height and width are input in meters.</remarks>
        public readonly double SCAL;

        /// <summary>Run title (description).</summary>
        public string? RUN { get; set; }

        /// <summary>Job ordinal number.</summary>
        public readonly int ORDINAL;
        #endregion

        #region Time averaging factors
        /// <summary>Time averaging factor = <c>(<see cref="ATIM"/>/3.0)^0.2.</c></summary>
        public readonly double AFAC_3MIN_02;

        /// <summary>Time averaging factor = <c>(<see cref="ATIM"/>/30.0)^0.2.</c></summary>
        public readonly double AFAC_30MIN_02;
        #endregion

        #region Surface roughness factors
        /// <summary>Surface roughness related factor = <c>(<see cref="Z0"/>/3.0)^0.2.</c></summary>
        public readonly double RFAC_3CM_02;

        /// <summary>Surface roughness related factor = <c>(<see cref="Z0"/>/3.0)^0.07.</c></summary>
        public readonly double RFAC_3CM_007;

        /// <summary>Surface roughness related factor = <c>(<see cref="Z0"/>/10.0)^0.07.</c></summary>
        public readonly double RFAC_10CM_007;
        #endregion

        #region Job details (meteo conditions, links, receptors)
        /// <summary>Meteo conditions collection.</summary>
        public readonly List<Meteo> Meteos;

        /// <summary>Link collection.</summary>
        public readonly List<Link> Links;

        /// <summary>Receptor collection.</summary>
        public readonly List<Receptor> Receptors;
        #endregion

        #region Constructor(s)
        public Job(int ordinal, string job, Minute atim, Centimeter z0, Centimeter_Sec vs, Centimeter_Sec vd, int nr, double scal)
        {
            ORDINAL = ordinal;

            JOB = job;
            ATIM = atim;
            Z0 = z0;
            VS1 = vs;
            VD1 = vd;
            NR = nr;
            SCAL = scal;

            // Convert [cm/s] to [m/s]
#if DIMENSIONAL_ANALYSIS
            VS = Meter_Sec.From(VS1);
            VD = Meter_Sec.From(VD1);
#else
            VS = Metrology.Meter_Sec.FromCentimeter_Sec(VS1);
            VD = Metrology.Meter_Sec.FromCentimeter_Sec(VD1);
#endif
            V1 = VD - VS / 2.0;

            // Averaging time factors:
            AFAC_3MIN_02 = Pow(ATIM / ATIM_3MIN, 0.2);
            AFAC_30MIN_02 = Pow(ATIM / ATIM_30MIN, 0.2);

            // Surface roughness related factors:
            RFAC_3CM_02 = Pow(Z0 / Z0_3CM, 0.2);
            RFAC_3CM_007 = Pow(Z0 / Z0_3CM, 0.07);
            RFAC_10CM_007 = Pow(Z0 / Z0_10CM, 0.07);

            Receptors = new List<Receptor>();
            Links = new List<Link>();
            Meteos = new List<Meteo>();
        }
        #endregion

        #region Formatting
        public override string ToString() => $"{ORDINAL}. {JOB}: {NR} receptor(s); {RUN}";
        #endregion
    }
}
