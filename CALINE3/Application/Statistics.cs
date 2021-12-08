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
    /// Mathematical statistics.
    /// </summary>
    internal class Statistics
    {
        #region Methods
        /// <summary>
        /// Gauss error function (maximum error: 1.5×10−7).
        /// </summary>
        /// <remarks>
        /// See: Abramowitz and Stegun approximation in <a href="http://en.wikipedia.org/wiki/Error_function">Wikipedia</a>.
        /// </remarks>
        /// <param name="x">Any (positive or negative) value.</param>
        /// <returns>
        /// for a random variable Y that is normally distributed with mean 0 and standard deviation 1/√2,
        /// erf(x) is the probability that Y falls in the range [−x, x].
        /// </returns>
        public static double Erf(double x)
        {
            double t = 1.0 / (1.0 + 0.3275911 * ((x < 0) ? -x : x));
            double p = System.Math.Exp(-x * x) * t * (0.254829592 + t * (-0.284496736 + t * (1.421413741 + t * (-1.453152027 + t * 1.061405429))));
            return (x < 0) ? p - 1.0 : 1.0 - p;
        }
        #endregion
    }
}
