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
    /// 2-dimensional Euclidean geometry.
    /// </summary>
    public class Euclid2D
    {
        #region Methods
        /// <summary>
        /// Clockwise angle [rad] formed by North direction (Y-axis) and vector starting from 
        /// the point (a,b) and terminating at the point (x,y).
        /// </summary>
        /// <param name="a">x-coordinate of start point</param>
        /// <param name="b">y-coordinate of start point</param>
        /// <param name="x">x-coordinate of end point</param>
        /// <param name="y">y-coordinate of end point</param>
        /// <returns>0 &#8804; angle &lt; 2&#960; [rad].</returns>
        public static Radian Azimuth(Meter a, Meter b, Meter x, Meter y)
            =>  (x > a) ? PI / 2.0 - Atan((y - b) / (x - a)) :
                (x < a) ? 3.0 * PI / 2.0 - Atan((y - b) / (x - a)) :
                (y < b) ? PI : (Radian)0.0;

        /// <summary>
        /// Distance between points on a 2D-plane.
        /// </summary>
        /// <param name="x1">X-coordinate of the 1st point [m].</param>
        /// <param name="y1">Y-coordinate of the 1st point [m].</param>
        /// <param name="x2">X-coordinate of the 2nd point [m].</param>
        /// <param name="y2">Y-coordinate of the 2nd point [m].</param>
        public static Meter Distance(Meter x1, Meter y1, Meter x2, Meter y2)
            =>  Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));

        #endregion
    }
}
