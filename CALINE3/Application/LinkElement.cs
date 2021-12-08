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
    /// An element (section) of the <see cref="Link"/> as seen from a <see cref="Receptor"/> position.
    /// </summary>
    /// <remarks>
    /// <see cref="Link"/>s are divided into a series of elements from which
    /// incremental concentrations are computed and then summed
    /// to form a total concentration estimate for a particular <see cref="Receptor"/> location.
    /// </remarks>
    public readonly struct LinkElement
    {
        #region Properties
        /// <summary>Parent link.</summary>
        private readonly LinkActive _parent;

        /// <summary>Element half-length.</summary>
        public readonly Meter EL2;

        /// <summary>Element centerline distance.</summary>
        public readonly Meter ECLD;

        /// <summary>Equivalent line half-length.</summary>
        public readonly Meter ELL2;

        /// <summary>Central sub-element half-length.</summary>
        public readonly Meter CSL2;

        /// <summary>Central sub-element half-width.</summary>
        public readonly Meter EM2;

        /// <summary>Peripheral sub-element width. </summary>
        public readonly Meter EN2;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="LinkElement"/> constructor.
        /// </summary>
        /// <param name="parent">parent link (element's root)</param>
        /// <param name="ED1">element start position</param>
        /// <param name="ED2">element end position</param>
        public LinkElement(LinkActive parent, Meter ED1, Meter ED2)
        {
            _parent = parent;

            // Element half-length
            EL2 = Abs(ED2 - ED1) / 2.0;

            // Element centerline distance
            ECLD = -(ED1 + ED2) / 2.0;

            // Equivalent line half-length
            ELL2 = _parent.link.W2 * Cos(_parent.TETA) + EL2 * Sin(_parent.TETA);

            // Central sub-element half-length
            CSL2 = (_parent.TETA >= Atan(_parent.link.W2 / EL2)) ? _parent.link.W2 / Sin(_parent.TETA) : EL2 / Cos(_parent.TETA);

            // Central sub-element half-width
            EM2 = Abs(EL2 * Sin(_parent.TETA) - _parent.link.W2 * Cos(_parent.TETA));

            // Peripheral sub-element width
            EN2 = (ELL2 - EM2) / 2.0;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Computes <see cref="LinkElement"/> profile as seen from the receptor at the distance <paramref name="D"/>.
        /// </summary>
        /// <param name="D">receptor-link distance (perpendicular to the link) [m]</param>
        /// <param name="QE">central subelement lineal strength [&#956;g/(m*s)]</param>
        /// <param name="YE">plume centerline offset [m]</param>
        /// <param name="FET">element fetch [m]</param>
        /// <returns>
        /// <c>true</c> if the element contributes to the pollution and the profile
        /// (<paramref name="QE"/>, <paramref name="YE"/>, <paramref name="FET"/>) has been computed;
        /// <c>false</c> otherwise.
        /// </returns>
        public bool GetProfile(Meter D, out Microgram_MeterSec QE, out Meter YE, out Meter FET)
        {
            // Y distance from element center to receptor (plume centerline offset)
            YE = ECLD * Sin(_parent.PHI) - D * Cos(_parent.PHI);

            // Element fetch
            FET = ECLD * Cos(_parent.PHI) + D * Sin(_parent.PHI);

            // Central sub-element lineal source strength
            if (FET <= -CSL2)
            {
                // element does not contribute
                QE = (Microgram_MeterSec)double.NaN;
                return false;
            }
            else if (FET < CSL2)
            {
                // receptor within element
                FET = (CSL2 + FET) / 2.0;
                QE = _parent.link.Q1 * (FET / _parent.link.W2);
            }
            else
            {
                QE = _parent.link.Q1 * (CSL2 / _parent.link.W2);
            }

            return true;
        }

        public Microgram_MeterSec SourceStrength(Microgram_MeterSec QE, Meter SGY, Meter YE)
        {
            // Weighting factor
            double[] WT = { 0.25, 0.75, 1.0, 0.75, 0.25 };

            // Sub-element source strength loop
            Meter[] Y = new Meter[6];

            Y[0] = YE + ELL2;
            Y[1] = Y[0] - EN2;
            Y[2] = Y[1] - EN2;
            Y[3] = Y[2] - 2 * EM2;
            Y[4] = Y[3] - EN2;
            Y[5] = Y[4] - EN2;

            // Add up strengths of all subelements
            Microgram_MeterSec FAC2 = (Microgram_MeterSec)0.0;
            for (int j = 0; j < 5; j++)
            {
                FAC2 += QE * WT[j] *
                    /* PD = normal probability density = */
                    (Statistics.Erf(Y[j] / SGY / Const.SQRT_2) - Statistics.Erf(Y[j + 1] / SGY / Const.SQRT_2)) / 2.0;
            }

            return FAC2;
        }
        #endregion

        #region Formatting
        /// <summary><see cref="LinkElement"/> information in a text form.</summary>
        public override string ToString() => $"ECLD={ECLD} : EL2={EL2} :: ELL2={ELL2} : CSL2={CSL2} :: EM2={EM2} : EN2={EN2}";
        #endregion
    }
}
