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

using static System.Console;

namespace CALINE3
{
    /*
     * Model results of CO (carbon monoxide) concentration are given in parts
     * per million (ppm) for each receptor-link combination, and are totaled
     * (including ambient) for each receptor.
     *
     * Other inert gaseous pollutants (such as SF6 tracer) may be run by changing
     * the molecular weight variable (MOWT) within the program to the appropriate
     * value, and modifying the output headings. Similarly, to run the model for
     * particulates, set FPPM = 1 and again modify the headings.
     */

    public static class Report
    {
        #region Properties
        /// <summary>
        /// Molecular mass (weight) of CO [g/mol].
        /// </summary>
        /// <remarks>
        /// NOTE: according to <a href="https://en.wikipedia.org/wiki/Carbon_monoxide">Wikipedia</a>
        /// molar mass of CO = 28.010 [g/mol] that is greater than the value 28.0 [g/mol]
        /// used in the original CALINE3 application! Is that relevant?
        /// </remarks>
        private static readonly Gram_Mole MOWT = (Gram_Mole)28.0;

        /// <summary>
        /// Molar volume of an ideal gas [m3/mol]. 
        /// </summary>
        /// <remarks>
        /// The <a href="https://en.wikipedia.org/wiki/Molar_volume">molar volume</a>
        /// of an ideal gas at 1 atmosphere of pressure is:
        /// <list type="bullet">
        /// <item><description><c>0.022413969545014... [m3/mol]</c> at <c>0 °C</c>,</description></item>
        /// <item><description><c>0.024465403697038... [m3/mol]</c> at <c>25 °C</c>.</description></item>
        /// </list>
        /// </remarks>
        private static readonly Meter3_Mole MOVL = (Meter3_Mole)0.0245;

        /// <summary>
        /// PPM factor [µg/m3] to convert concentration from [µg/m3] to [ppm]. 
        /// </summary>
        private static readonly Microgram_Meter3 FPPM =
#if DIMENSIONAL_ANALYSIS
            Microgram_Meter3.From(MOWT / MOVL);
#else
            Metrology.Microgram_Meter3.FromGram_Meter3(MOWT / MOVL);
#endif

        private static readonly string LINE = new('-', 102);

        private static int PageCount = 0;
        #endregion

        #region Methods
        public static void Output(Job site, Meteo meteo, Microgram_Meter3[][] MC)
        {
            // CALINE3: CALIFORNIA LINE SOURCE DISPERSION MODEL - SEPTEMBER, 1979 VERSION
            // I. SITE VARIABLES
            WriteJobAndMeteoHeader(site, meteo);

            // II.  LINK VARIABLES
            WriteLinksHeader(site.Links);

            // III.  RECEPTOR LOCATIONS AND MODEL RESULTS
            if (site.Links.Count <= 1)
            {
                WriteReceptorsHeader();

                WriteLine("                            *        COORDINATES (M)        *  CO");
                WriteLine("       RECEPTOR             *      X        Y        Z      * (PPM)");
                WriteLine("   -------------------------*-------------------------------*-------");

                for (int I = 0; I < site.Receptors.Count; I++)
                {
                    WriteLine(site.Receptors[I].ToReport(I + 1, ReceptorTotalConcentration(MC, I) + meteo.AMB));
                }
            }
            else if (site.Links.Count <= 10)
            {
                WriteReceptorsHeader();

                string linkCodes = string.Join("    ", LinkCodes(site.Links));
                WriteLine("                            *                               * TOTAL *             CO/LINK");
                WriteLine("                            *        COORDINATES (M)        * + AMB *              (PPM)");
                WriteLine($"       RECEPTOR             *      X        Y        Z      * (PPM) *   {linkCodes}");
                WriteLine($"   -------------------------*-------------------------------*-------*---{new string('-', linkCodes.Length)}-");

                for (int I = 0; I < site.Receptors.Count; I++)
                {
                    Write(site.Receptors[I].ToReport(I + 1, ReceptorTotalConcentration(MC, I) + meteo.AMB));
                    WriteLine($"  *{ReceptorLinkConcentrations(MC, I)}");
                }
            }
            else
            {
                WriteJobAndMeteoHeader(site, meteo);

                WriteReceptorsHeader();

                WriteLine("                            *                               * TOTAL *");
                WriteLine("                            *        COORDINATES (M)        * + AMB *");
                WriteLine("       RECEPTOR             *      X        Y        Z      * (PPM) *");
                WriteLine("   -------------------------*-------------------------------*-------*");

                for (int I = 0; I < site.Receptors.Count; I++)
                {
                    WriteLine(site.Receptors[I].ToReport(I + 1, ReceptorTotalConcentration(MC, I) + meteo.AMB));
                }

                WriteJobAndMeteoHeader(site, meteo);

                string linkCodes = string.Join("    ", LinkCodes(site.Links));
                WriteLine();
                WriteLine("IV.  MODEL RESULTS (RECEPTOR-LINK MATRIX)");
                WriteLine();
                WriteLine("                            *                                                     CO/LINK");
                WriteLine("                            *        COORDINATES (M)        * + AMB *              (PPM)");
                WriteLine($"       RECEPTOR             *      X        Y        Z      * (PPM) * {linkCodes}");
                WriteLine(LINE);
                WriteLine();

                for (int I = 0; I < site.Receptors.Count; I++)
                {
                    WriteLine($"{I + 1}.{site.Receptors[I].RCP,-21} {ReceptorLinkConcentrations(MC, I)}");
                }
            }
        }

        /// <summary>
        /// Computes a fraction (share) of a concentration.
        /// </summary>
        /// <param name="mc">mass concentration [µg/m3].</param>
        /// <returns>fraction in [ppm] of the given concentration.</returns>
        static PPM ConcentrationFraction(Microgram_Meter3 mc) =>
#if DIMENSIONAL_ANALYSIS
            Round(PPM.From(mc / FPPM), 1);
#else
            Round(Metrology.PPM.FromFraction(mc / FPPM), 1);
#endif

        private static PPM ReceptorTotalConcentration(Microgram_Meter3[][] MC, int R /*receptor index*/)
        {
            PPM CSUM = (PPM)0.0;
            for (int L = 0; L < MC.Length /*Links.Count*/; L++)
            {
                CSUM += ConcentrationFraction(MC[L][R]);
            }
            return CSUM;
        }

        private static string ReceptorLinkConcentrations(Microgram_Meter3[][] MC, int R /*receptor index*/)
        {
            string ppmFormat = "{0,5:F1}";
            string concentrations = string.Empty;
            for (int L = 0; L < MC.Length /*Links.Count*/; L++)
            {
                concentrations += $"{_Ppm(ConcentrationFraction(MC[L][R]))}";
            }
            return concentrations;

#if DIMENSIONAL_ANALYSIS
            string _Ppm(PPM q) => q.ToString(ppmFormat);
#else
            string _Ppm(PPM q) => Metrology.PPM.String(q, ppmFormat);
#endif
        }

        private static IEnumerable<string> LinkCodes(List<Link> links)
        {
            for (int J = 0; J < links.Count; J++) yield return links[J].COD;
        }

        private static void WriteJobAndMeteoHeader(Job site, Meteo met)
        {
            string msecFormat = "{0,5:F1}";
            string atimFormat = "{0,5:F1}";
            string mixhFormat = "{0,7:F1}";
            string brgFormat = "{0,5:F1}";
            string z0Format = "{0,5:F1}";
            string cmsecFormat = "{0,5:F1}";
            string ambFormat = "{0,5:F1}";

            WriteLine($"                            CALINE3: CALIFORNIA LINE SOURCE DISPERSION MODEL - SEPTEMBER, 1979/2021 C# VERSION             PAGE {++PageCount,2}");

            WriteLine();
            WriteLine();
            WriteLine($"     JOB: {site.JOB,-53}RUN: {site.RUN,-40}");
            WriteLine();
            WriteLine();

            WriteLine();
            WriteLine("       I.  SITE VARIABLES");
            WriteLine();
            WriteLine();
            WriteLine($"      U = {_Msec(met.U)} M/S           CLAS = {met.CLAS,3}  ({met.TAG})        VS = {_Cmsec(site.VS1)} CM/S       ATIM = {_Atim(site.ATIM)} MINUTES                  MIXH = {_Mixh(met.MIXH)} M");
            WriteLine($"    BRG = {_Brg(met.BRG1)} DEGREES         Z0 = {_Z0(site.Z0)} CM        VD = {_Cmsec(site.VD1)} CM/S        AMB = {_Amb(met.AMB)} PPM");

#if DIMENSIONAL_ANALYSIS
            string _Msec(Meter_Sec q) =>        q.ToString(msecFormat);
            string _Cmsec(Centimeter_Sec q) =>  q.ToString(cmsecFormat);
            string _Atim(Minute q) =>           q.ToString(atimFormat);
            string _Mixh(Meter q) =>            q.ToString(mixhFormat);
            string _Brg(Degree q) =>            q.ToString(brgFormat);
            string _Z0(Centimeter q) =>         q.ToString(z0Format);
            string _Amb(PPM q) =>               q.ToString(ambFormat);
#else
            string _Msec(Meter_Sec q) =>        Metrology.Meter.String(q, msecFormat);
            string _Cmsec(Centimeter_Sec q) =>  Metrology.Meter.String(q, cmsecFormat);
            string _Atim(Minute q) =>           Metrology.Meter.String(q, atimFormat);
            string _Mixh(Meter q) =>            Metrology.Meter.String(q, mixhFormat);
            string _Brg(Degree q) =>            Metrology.Meter.String(q, brgFormat);
            string _Z0(Centimeter q) =>         Metrology.Meter.String(q, z0Format);
            string _Amb(PPM q) =>               Metrology.Meter.String(q, ambFormat);
#endif
        }

        private static void WriteLinksHeader(List<Link> links)
        {
            WriteLine();
            WriteLine();
            WriteLine();
            WriteLine("      II.  LINK VARIABLES");
            WriteLine();
            WriteLine();
            WriteLine("       LINK DESCRIPTION     *      LINK COORDINATES (M)      * LINK LENGTH  LINK BRG   TYPE  VPH     EF     H    W");
            WriteLine("                            *   X1      Y1      X2      Y2   *     (M)       (DEG)                 (G/MI)  (M)  (M)");
            WriteLine("   -------------------------*--------------------------------*-------------------------------------------------------");
            for (int I = 0; I < links.Count; I++)
            {
                WriteLine(links[I].ToReport());
            }
            WriteLine();
        }

        private static void WriteReceptorsHeader()
        {
            WriteLine();
            WriteLine("     III.  RECEPTOR LOCATIONS AND MODEL RESULTS");
            WriteLine();
        }
        #endregion
    }
}
