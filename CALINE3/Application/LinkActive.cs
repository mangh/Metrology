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
    /// <summary>
    /// <see cref="Link"/> under the influence of <see cref="Meteo"/> conditions.
    /// <list type="bullet">
    /// <item><description>link - raw (steady) link</description></item>
    /// <item><description>BASE - element growth factor</description></item>
    /// <item><description>PHI - the angle between the wind and the link</description></item>
    /// <item><description>TETA - the normalized angle between the wind and the link (PHI % 90)</description></item>
    /// <item><description>TR - Mixing zone residence time</description></item>
    /// </list>
    /// </summary>
    public class LinkActive
    {
        #region Constants
        private static readonly Degree DEGREE_70  = (Degree)70.0;
        private static readonly Degree DEGREE_50  = (Degree)50.0;
        private static readonly Degree DEGREE_20  = (Degree)20.0;
        #endregion

        #region Properties
        /// <summary>
        /// Raw (steady) <see cref="Link"/> independent of <see cref="Meteo"/> conditions.
        /// </summary>
        public readonly Link link;

        /// <summary>
        /// Element growth factor [dimensionless].
        /// </summary>
        private readonly double BASE;

        /// <summary>
        /// The angle [rad] between the wind direction and the direction of the roadway.
        /// </summary>
        public readonly Radian PHI;

        /// <summary>
        /// The normalized angle [rad] between the wind and the roadway (that is <see cref="PHI"/> % 90 [rad]).
        /// </summary>
        public readonly Radian TETA;

        /// <summary>
        /// Mixing zone residence time [s].
        /// </summary>
        public readonly Second TR;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="LinkActive"/> constructor.
        /// </summary>
        /// <param name="meteo">meteo conditions</param>
        /// <param name="steady">raw (steady) link</param>
        public LinkActive(Meteo meteo, Link steady)
        {
            link = steady;

            // Wind angle with respect to link
            Degree phi = meteo.BRG - steady.LBRG;

            // teta = phi % 90
            Degree teta = Abs(phi);
            if (teta >= Const.DEGREE_270) teta = Const.DEGREE_360 - teta;
            else if (teta >= Const.DEGREE_180) teta -= Const.DEGREE_180;
            else if (teta > Const.DEGREE_90) teta = Const.DEGREE_180 - teta;

#if DIMENSIONAL_ANALYSIS
            PHI = Radian.From(phi);
            TETA = Radian.From(teta);
#else
            PHI = Metrology.Radian.FromDegree(phi);
            TETA = Metrology.Radian.FromDegree(teta);
#endif

            // Set element growth base
            BASE = (teta < DEGREE_20) ? 1.1 :
                   (teta < DEGREE_50) ? 1.5 :
                   (teta < DEGREE_70) ? 2.0 : 4.0;

            // Residence time
            TR = link.DSTR * link.W2 / meteo.U;
        }
        #endregion

        #region Methods
        /// <summary>Length [m] of subsequent <see cref="LinkElement"/> of the <see cref="Link"/>.</summary>
        /// <param name="NE">element sequence number: 0, 1, ...</param>
        /// <returns>Length of the element of the given (sequential) number <paramref name="NE"/>.</returns>
        private Meter ElementLength(int NE) => link.WL * Pow(BASE, NE);

        /// <summary>
        /// Upwind <see cref="LinkElement"/>(s) enumerator.
        /// </summary>
        /// <param name="linkStart">distance to the link start position (downwind from a receptor position)</param>
        /// <param name="linkEnd">distance to the link end position (upwind from a receptor position)</param>
        /// <returns>
        /// Collection of <see cref="LinkElement"/>(s) up-wind, within the given range (<paramref name="linkStart"/>, <paramref name="linkEnd"/>).
        /// </returns>
        public IEnumerable<LinkElement> UpwindElements(Meter linkStart, Meter linkEnd)
        {
            Meter elementEnd = (Meter)0.0;
            for (int NE = 0; elementEnd < linkEnd; NE++)
            {
                // Next element
                Meter elementStart = elementEnd;
                elementEnd += ElementLength(NE);
                // Any element reached?
                if (elementEnd > linkStart)
                {
                    yield return new LinkElement(
                        parent: this,
                        ED1: (elementStart < linkStart) ? linkStart : elementStart,
                        ED2: (elementEnd > linkEnd) ? linkEnd : elementEnd
                    );
                }
            }
        }

        /// <summary>
        /// Downwind <see cref="LinkElement"/>(s) enumerator.
        /// </summary>
        /// <param name="linkStart">distance to the link start position (downwind from a receptor position)</param>
        /// <param name="linkEnd">distance to the link end position (upwind from a receptor position)</param>
        /// <returns>
        /// Collection of <see cref="LinkElement"/>(s) downwind, within the given range (<paramref name="linkStart"/>, <paramref name="linkEnd"/>).
        /// </returns>
        public IEnumerable<LinkElement> DownwindElements(Meter linkStart, Meter linkEnd)
        {
            Meter elementEnd = (Meter)0.0;
            for (int NE = 0; elementEnd > linkStart; NE++)
            {
                // Next element
                Meter elementStart = elementEnd;
                elementEnd -= ElementLength(NE);
                // Any element reached?
                if (elementEnd < linkEnd)
                {
                    yield return new LinkElement(
                        parent: this,
                        ED1: (elementStart > linkEnd) ? linkEnd : elementStart,
                        ED2: (elementEnd < linkStart) ? linkStart : elementEnd
                    );
                }
            }
        }
        #endregion
    }
}
