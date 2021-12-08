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
    /*
     * Algorithms in this module make use of the power-law formula:
     * 
     *    σy[m] = a * (x[m])^b  (horizontal dispersion parameter)
     *    σz[m] = c * (x[m])^d  (vertical dispersion parameter)
     * 
     *    a, b, c, d - constants dependent on stability class
     * 
     * which is an analytical approximation of the plot of experimental data.
     * The formula is assumed to give results (σy, σz) in [m] for the distance
     * argument (x) in [m] although such a relationship cannot be derived from
     * the formula! This may be the source of dimensional inconsistencies!
     *
     * The time averaging and surface roughness corrections applied here - both
     * dimensionally unclear - give rise to further issues.
     *
     * Therefore I had to "hack" in places - mainly in the constructor - to make
     * the algorithm dimensionally consistent and getting still the same results
     * as the original CALINE3.
     *
     * I hope I did it right.
     * 
     */

    public class Plume
    {
        #region Constants
        public static readonly Meter_Sec ZERO_VELOCITY = (Meter_Sec)0.0;
        #endregion

        #region Power-curve coefficients & Gaussian plume dispersion parameters
        /// <summary>
        /// Coefficient "a" in the power-curve formula: &#963;y[m] = a * (distance[m])^b.
        /// </summary>
        public Meter PY1;

        /// <summary>
        /// Coefficient "b" in the power-curve formula: &#963;y[m] = a * (distance[m])^b.
        /// </summary>
        public double PY2;

        /// <summary>
        /// Coefficient "c" in power-curve formula: &#963;z[m] = c * (distance[m])^d.
        /// </summary>
        public Meter PZ1;

        /// <summary>
        /// Coefficient "d" in the power-curve formula: &#963;z[m] = c * (distance[m])^d.
        /// </summary>
        public double PZ2;

        /// <summary>&#963;y - horizontal dispersion parameter [m].</summary>
        private Meter SGY;

        /// <summary>&#963;z - vertical dispersion parameter [m].</summary>
        private Meter SGZ;

        /// <summary>Vertical diffusivity estimate [m2/s].</summary>
        private Meter2_Sec KZ;
        #endregion

        #region Environmental conditions
        /// <summary><see cref="Job"/> site factors.</summary>
        private readonly Job _site;

        /// <summary><see cref="Meteo"/> conditions.</summary>
        private readonly Meteo _met;

        /// <summary>
        /// Active link (the plume/pollution source) i.e. a <see cref="Link"/> 
        /// under the influence of <see cref="Meteo"/> conditions.
        /// </summary>
        private readonly LinkActive _active;
        #endregion

        #region Constructor
        /// <summary>
        /// Plume constructor.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="met"></param>
        /// <param name="link"></param>
        public Plume(Job site, Meteo met, Link link)
        {
            _site = site;
            _met = met;
            _active = new(met, link);

            /***************************************
             * 
             * σy(1[m])     = a * (1[m])^b = a = a * e^(b*ln(1[m])     = _met.AY1 * _site.RFAC_3CM_02 * _site.AFAC_3MIN_02;
             * σy(10000[m]) = a * (10000[m])^b = a * e^(b*ln(10000[m]) = _met.AY2 * _site.RFAC_3CM_007 * _site.AFAC_3MIN_02;
             * 
             * b = (ln(σy(10000[m]) - ln(σy(1[m]))) / (ln(10000[m]) - ln(1[m])
             * 
             */

            // σy(1[m]) = a-coefficient [m]
            PY1 = (Meter)_met.AY1 * _site.RFAC_3CM_02 * _site.AFAC_3MIN_02;

            // σy(10000[m])
            Meter PY10 = (Meter)_met.AY2 * _site.RFAC_3CM_007 * _site.AFAC_3MIN_02;

            // b-coefficient [dimensionless]
            PY2 = Log(PY10 / PY1) / Log(Link.MAX_LENGTH / (Meter)1.0);

            /***************************************
             * 
             * To relate SGZI (σz initial vertical dispersion parameter) and TR
             * (mixing zone residence time) the following equation:
             * 
             *      SGZI[m] = 1.8[?] + 0.11[?] * TR[s]
             * 
             * has been derived from the General Motors Data Base.
             * 
             * NOTE THE MISSING UNITS [?].
             */

            // I guess it should be like this:
            Meter SGZI = (Meter)1.8 + (Meter_Sec)0.11 * _active.TR;

            // SGZI need to be adjusted for the averaging time (but it is considered
            // to be independent of surface roughness and atmospheric stability class).
            // It is always defined as occurring at a distance W2 downwind from the
            // element centerpoint i.e. it equals σz(W2[m]):
            Meter SGZ1 = SGZI * _site.AFAC_30MIN_02;

            // σz(10000[m]) adjusted for roughness and averaging time:
            Meter SZ10 = (Meter)_met.AZ * _site.RFAC_10CM_007 * _site.AFAC_3MIN_02;

            /***************************************
             * 
             * σz(W2[m])    = c * (W2[m])^d    = c * e^(d∙ln(W2[m]))
             * σz(10000[m]) = c * (10000[m])^d = c * e^(d∙ln(10000[m]))
             * 
             * d = (ln(σz(10000)) - ln(σz(W2))) / (ln(10000) - ln(W2))
             * 
             * ln(σz(W2)/c(W2)) = ln(σz(W2)) - ln(c(W2)) = d * ln(W2)
             * ln(σz(10000)/c(10000)) = ln(σz(10000)) - ln(c(10000)) = d * ln(10000)
             * 
             * ln(c) = (ln(c(10000)) - ln(c(W2))) / 2
             *       = (ln(σz(10000)) + ln(σz(W2)) - d * (ln(10000) + ln(W2))) / 2
             *       = (ln(σz(10000) * σz(W2)) - d*(ln(10000 * W2))) / 2
             * 
             * c = sqrt(σz(10000) * σz(W2)) / pow(sqrt(10000 * W2), d)
             * 
             */

            // d-coefficient [dimesionless]
            PZ2 = Log(SZ10 / SGZ1) / Log(Link.MAX_LENGTH / _active.link.W2);

            // c-coefficient [m]
            PZ1 = Sqrt(SZ10 * SGZ1) / Pow(Sqrt(Link.MAX_LENGTH * _active.link.W2), PZ2);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Pollutant concentration [µg/m3] at the <paramref name="receptor"/> location.
        /// </summary>
        /// <param name="receptor">receptor</param>
        /// <returns></returns>
        public Microgram_Meter3 ConcentrationAt(Receptor receptor)
        {
            // D - distance (perpendicular to the link)
            // L - offset (parallel to the link, relative to its start position)
            // Z - level (adjusted for the link type)
            (Meter D, Meter L, Meter Z) = _active.link.TransformReceptorCoordinates(receptor);

            // Assuming point 0 at the receptor orthogonal projection on link line:
            Meter DWL = -(_active.link.LL + L);
            Meter UWL = -L;

            // Mass Concentration
            Microgram_Meter3 C = (Microgram_Meter3)0.0;

            foreach (LinkElement elem in _active.UpwindElements(DWL, UWL))
            {
                C += ConcentrationFrom(elem, D, Z);
            }

            foreach (LinkElement elem in _active.DownwindElements(DWL, UWL))
            {
                C += ConcentrationFrom(elem, D, Z);
            }
            return C;
        }

        /// <summary>
        /// Incremental concentration [µg/m3] from the <paramref name="element"/>
        /// at the distance <paramref name="D"/> and at the level <paramref name="Z"/>.
        /// </summary>
        /// <param name="element">link element</param>
        /// <param name="D">receptor-link distance [m]</param>
        /// <param name="Z">receptor level (adjusted to the <see cref="Link"/> type) [m]</param>
        /// <returns></returns>
        private Microgram_Meter3 ConcentrationFrom(LinkElement element, Meter D, Meter Z)
        {
            // Get element profile:
            //    QE  - central subelement lineal strength [µg/(m*s)]
            //    YE  - plume centerline offset [m]
            //    FET - element fetch [m]
            if (!element.GetProfile(D, out Microgram_MeterSec QE, out Meter YE, out Meter FET))
            {
                return (Microgram_Meter3)0.0;  // Element does NOT contribute.
            }

            // σy - horizontal standard deviation of the emission distribution:
            SGY = PY1 * Pow(FET, PY2);

            // σz - vertical standard deviation of the emission distribution:
            SGZ = PZ1 * Pow(FET, PZ2);

            // Vertical diffusivity:
            KZ = SGZ * SGZ / (2.0 * FET / _met.U);

            Microgram_Meter3 FACT = 
                element.SourceStrength(QE, SGY, YE) / (Const.SQRT_2PI * SGZ * _met.U);

            // Adjust for depressed section wind speed
            FACT *= _active.link.DepressedSectionFactor(D);

            // Deposition correction
            double FAC3 = DepositionFactor(Z, _active.link.H, _site.V1);
            if (double.IsNaN(FAC3))
            {
                return (Microgram_Meter3)0.0;
            }
            else
            {
                // Settling correction
                FACT *= SettlingFactor(Z, _active.link.H, _site.VS);

                // Incremental concentration from the element
                double FAC5 = GaussianFactor(Z, _active.link.H, _met.MIXH);
                return FACT * (FAC5 - FAC3);
            }
        }

        private double DepositionFactor(Meter Z, Meter H, Meter_Sec V1)
        {
            double FAC3 = 0.0;
            if (V1 != ZERO_VELOCITY)
            {
                double ARG = (V1 * SGZ / KZ + (Z + H) / SGZ) / Const.SQRT_2;
                if (ARG > 5.0)
                {
                    FAC3 = double.NaN;
                }
                else
                {
                    FAC3 = 
                        Const.SQRT_2PI * V1 * SGZ
                        * Exp(V1 * (Z + H) / KZ + 0.5 * (V1 * SGZ / KZ) * (V1 * SGZ / KZ))
                        * Statistics.Erf(ARG)
                        / KZ;

                    if (FAC3 > 2.0) FAC3 = 2.0;
                }
            }
            return FAC3;
        }

        private double SettlingFactor(Meter Z, Meter H, Meter_Sec VS)
        {
            return (VS == ZERO_VELOCITY) ? 1.0 : System.Math.Exp(-VS * (Z - H) / (2.0 * KZ) - (VS * SGZ / KZ) * (VS * SGZ / KZ) / 8.0);
        }

        private double GaussianFactor(Meter Z, Meter H, Meter MIXH)
        {
            double FAC5 = 0.0;
            double CNT = 0.0;
            double EXLS = 0.0;
            while (true)
            {
                double ARG1 = -0.5 * ((Z + H + 2.0 * CNT * MIXH) / SGZ) * ((Z + H + 2.0 * CNT * MIXH) / SGZ);
                double EXP1 = (ARG1 < -44.0) ? 0.0 : Exp(ARG1);

                double ARG2 = -0.5 * ((Z - H + 2.0 * CNT * MIXH) / SGZ) * ((Z - H + 2.0 * CNT * MIXH) / SGZ);
                double EXP2 = (ARG2 < -44.0) ? 0.0 : Exp(ARG2);

                FAC5 += EXP1 + EXP2;

                if (MIXH >= (Meter)1000.0)
                    break;  // Bypass mixing height calculation

                if (((EXP1 + EXP2 + EXLS) == 0.0) && (CNT <= 0.0))
                    break;

                if (CNT <= 0.0)
                {
                    CNT = Abs(CNT) + 1.0;
                    EXLS = 0.0;
                }
                else
                {
                    CNT = -1.0 * CNT;
                    EXLS = EXP1 + EXP2;
                }
            }
            return FAC5;
        }
        #endregion
    }
}
