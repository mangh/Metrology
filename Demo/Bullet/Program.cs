/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Bullet
{
    class Program
    {
        // Bullet slope: range and step values
        const double MIN = 0.0;
        const double MAX = 90.0;
        const double STEP = 0.1;

        // Output formats
        const string FMT_ANGLE = "{0,5:F2}{1}";     // quantity value {0}, width 5, format "F2" followed by unit symbol {1} (no blank between value and symbol)
        const string FMT_TIME = "{0,3:F0} {1}";     // quantity value {0}, width 3, format "F0" followed by blank and unit symbol {1}
        const string FMT_COORD = "{0,5:F0} {1}";    // quantity value {0}, width 5, format "F0" followed by blank and unit symbol {1}

#if DIMENSIONAL_ANALYSIS
        const string DIMENSIONAL_ANALYSIS_STATUS = "dimensional analysis: ON";
#else
        const string DIMENSIONAL_ANALYSIS_STATUS = "dimensional analysis: OFF";
#endif

        static void Main(/*string[] args*/)
        {
            Console.WriteLine(
                "Units of Measurement for C# applications. Copyright (©) Marek Anioła.\n" +
                "This program is provided to you under the terms of the license\n" +
                "as published at https://github.com/mangh/metrology."
            );

            Console.WriteLine("\nRange of a bullet (demo)");

            // Plain and measured (with units) bullet samplers
            var plain = new Plain.Sampler(height: 0.0, velocity: 715.0);
            var measured = new Measured.Sampler(height: (Meter)0.0, velocity: (Meter_Sec)715.0);

            // Measured/plain performance ratio
            var ratio = new Benchmark();

            // A warmup of 1000 - 1500 mS. Stabilizes the CPU cache and pipeline
            WarmUp(plain, measured);

            do
            {
                // calculate bullet ranges while measure the time spent for the calculations
                List<(double, double, double, double)> p = plain.Sample(MIN, MAX, STEP);
                List<(Degree, Second, Meter, Meter)> m = measured.Sample((Degree)MIN, (Degree)MAX, (Degree)STEP);

                // update performance ratio
                ratio.Add(measured.timer.elapsed / plain.timer.elapsed);

                PrintResults(m, p);

                Console.WriteLine("\n{0}. quantity/plain performance ratio: {1:F0} / {2:F0} = {3:F2} ({4})",
                    measured.benchmark.Count,
                    measured.timer.elapsed,
                    plain.timer.elapsed,
                    measured.timer.elapsed / plain.timer.elapsed,
                    DIMENSIONAL_ANALYSIS_STATUS
                );
                Console.WriteLine("Press Esc to conclude, any other key to retry...");
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            Console.WriteLine("\nAverage performance ratio for {0} run(s): {1:F0}±{2:F0}({3:F0}%) / {4:F0}±{5:F0}({6:F0}%) = {7:F2}±{8:F2}({9:F0}%).",
                measured.benchmark.Count,
                measured.benchmark.Average, measured.benchmark.StdDev, measured.benchmark.StdDevPercentage,
                plain.benchmark.Average, plain.benchmark.StdDev, plain.benchmark.StdDevPercentage,
                ratio.Average, ratio.StdDev, ratio.StdDevPercentage
            );
        }

        static void WarmUp(Plain.Sampler plain, Measured.Sampler measured)
        {
            // Process and thread properties set as described in CodeProject article by Thomas Maierhofer
            // "Performance Tests: Precise Run Time Measurements with System.Diagnostics.Stopwatch"
            // http://www.codeproject.com/Articles/61964/Performance-Tests-Precise-Run-Time-Measurements-wi
            Process currentProcess = Process.GetCurrentProcess();
#if (Windows || Linux)
            currentProcess.ProcessorAffinity = new IntPtr(2);   // Use only the 2nd core
#endif
            currentProcess.PriorityClass = ProcessPriorityClass.RealTime;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            // Warmp up
            Stopwatch sw = new();
            sw.Restart();
            while (sw.ElapsedMilliseconds < 1000)
            {
                plain.Sample(MIN, MAX, STEP);
                measured.Sample((Degree)MIN, (Degree)MAX, (Degree)STEP);
            }
            sw.Stop();

            // Clear statistics
            plain.benchmark.Reset();
            measured.benchmark.Reset();
        }

        static void PrintResults(List<(Degree, Second, Meter, Meter)> m, List<(double, double, double, double)> p)
        {
            Console.WriteLine();
            Console.WriteLine(" angle |  tmax |  xmax   |  ymax");
            Console.WriteLine("-------+-------+---------+--------");

            for (int i = 0; i < m.Count; i++)
            {
                (Degree slope, Second tmax, Meter xmax, Meter ymax) = m[i];

#if DIMENSIONAL_ANALYSIS
                // Print measured results:
                Console.WriteLine(
                    $"{slope.ToString(FMT_ANGLE)} | {tmax.ToString(FMT_TIME)} | {xmax.ToString(FMT_COORD)} | {ymax.ToString(FMT_COORD)}"
                );

                // Measured and plain results are to be the same. Is this the case?
                CheckResults(
                    slope.Value - p[i].Item1, 
                    tmax.Value - p[i].Item2, 
                    xmax.Value - p[i].Item3, 
                    ymax.Value - p[i].Item4
                );
#else
                Console.WriteLine("{0} | {1} | {2} | {3}",
                    Demo.UnitsOfMeasurement.Degree.String(slope, FMT_ANGLE),
                    Demo.UnitsOfMeasurement.Second.String(tmax, FMT_TIME),
                    Demo.UnitsOfMeasurement.Meter.String(xmax, FMT_COORD),
                    Demo.UnitsOfMeasurement.Meter.String(ymax, FMT_COORD));

                CheckResults(
                    slope - p[i].Item1,
                    tmax - p[i].Item2,
                    xmax - p[i].Item3,
                    ymax - p[i].Item4
                );
#endif
            }

            static void CheckResults(double slopeErr, double tmaxErr, double xmaxErr, double ymaxErr)
            {
                if ((slopeErr != 0.0) || (tmaxErr != 0.0) || (xmaxErr != 0.0) || (ymaxErr != 0.0))
                {
                    Console.WriteLine("{0} | {1} | {2} | {3}  *** (m - p) diff ***",
                        Demo.UnitsOfMeasurement.Degree.String(slopeErr),
                        Demo.UnitsOfMeasurement.Second.String(tmaxErr),
                        Demo.UnitsOfMeasurement.Meter.String(xmaxErr),
                        Demo.UnitsOfMeasurement.Meter.String(ymaxErr)
                    );
                }
            }
        }
    }
}
/* Sample output (.NET 7.0, dimensional analysis: OFF):
 * 
 * Units of Measurement for C# applications. Copyright (c) Marek Aniola.
 * This program is provided to you under the terms of the license
 * as published at https://github.com/mangh/unitsofmeasurement.
 * 
 * Range of a bullet (demo)
 * 
 *  angle |  tmax |  xmax   |  ymax
 *   (°)  |  (s)  |  (m)    |  (m)
 *  ------+-------+---------+---------
 *  0,00° |   0 s |     0 m |     0 m
 *  0,10° |   0 s |   182 m |     0 m
 *  0,20° |   1 s |   364 m |     0 m
 *  0,30° |   1 s |   546 m |     0 m
 *  0,40° |   1 s |   728 m |     0 m
 *  0,50° |   1 s |   910 m |     0 m
 *  0,60° |   2 s |  1092 m |     0 m
 *  0,70° |   2 s |  1274 m |     0 m
 *  0,80° |   2 s |  1456 m |     0 m
 *  0,90° |   2 s |  1637 m |     1 m
 *  1,00° |   3 s |  1819 m |     1 m
 *  1,10° |   3 s |  2001 m |     2 m
 * ...
 * ...
 * ...
 * 89,00° | 146 s |  1819 m |  6513 m
 * 89,10° | 146 s |  1637 m |  6514 m
 * 89,20° | 146 s |  1456 m |  6514 m
 * 89,30° | 146 s |  1274 m |  6514 m
 * 89,40° | 146 s |  1092 m |  6515 m
 * 89,50° | 146 s |   910 m |  6515 m
 * 89,60° | 146 s |   728 m |  6515 m
 * 89,70° | 146 s |   546 m |  6515 m
 * 89,80° | 146 s |   364 m |  6515 m
 * 89,90° | 146 s |   182 m |  6515 m
 * 90,00° | 146 s |     0 m |  6515 m
 * 
 * 32. quantity/plain performance ratio: 2199 / 2421 = 0,91 (dimensional analysis: OFF)
 * Press Esc to conclude, any other key to retry...
 * 
 * Average performance ratio for 32 run(s): 1863±364(20%) / 1863±387(21%) = 1,01±0,14(14%).
 * 
 * 
 ********************************************************************************************
 *
 * Sample output (.NET 7.0, dimensional analysis: ON):
 * 
 *  angle |  tmax |  xmax   |  ymax
 * -------+-------+---------+--------
 *  0,00° |   0 s |     0 m |     0 m
 *  0,10° |   0 s |   182 m |     0 m
 *  0,20° |   1 s |   364 m |     0 m
 *  0,30° |   1 s |   546 m |     0 m
 * 
 * 
 * 88,90° | 146 s |  2001 m |  6513 m
 * 89,00° | 146 s |  1819 m |  6513 m
 * 89,10° | 146 s |  1637 m |  6514 m
 * 89,20° | 146 s |  1456 m |  6514 m
 * 89,30° | 146 s |  1274 m |  6514 m
 * 89,40° | 146 s |  1092 m |  6515 m
 * 89,50° | 146 s |   910 m |  6515 m
 * 89,60° | 146 s |   728 m |  6515 m
 * 89,70° | 146 s |   546 m |  6515 m
 * 89,80° | 146 s |   364 m |  6515 m
 * 89,90° | 146 s |   182 m |  6515 m
 * 90,00° | 146 s |     0 m |  6515 m
 * 
 * 32. quantity/plain performance ratio: 5204 / 2241 = 2,32 (dimensional analysis: ON)
 * Press Esc to conclude, any other key to retry...
 * 
 * Average performance ratio for 32 run(s): 4730±857(18%) / 1924±348(18%) = 2,48±0,38(15%).
 *
 *
 ********************************************************************************************
 *
 * Sample output (.NET 8.0, dimensional analysis: ON):
 *
 *  angle |  tmax |  xmax   |  ymax
 * -------+-------+---------+--------
 *  0,00° |   0 s |     0 m |     0 m
 *  0,10° |   0 s |   182 m |     0 m
 *  0,20° |   1 s |   364 m |     0 m
 *  0,30° |   1 s |   546 m |     0 m
 * 
 * 88,90° | 146 s |  2001 m |  6513 m
 * 89,00° | 146 s |  1819 m |  6513 m
 * 89,10° | 146 s |  1637 m |  6514 m
 * 89,20° | 146 s |  1456 m |  6514 m
 * 89,30° | 146 s |  1274 m |  6514 m
 * 89,40° | 146 s |  1092 m |  6515 m
 * 89,50° | 146 s |   910 m |  6515 m
 * 89,60° | 146 s |   728 m |  6515 m
 * 89,70° | 146 s |   546 m |  6515 m
 * 89,80° | 146 s |   364 m |  6515 m
 * 89,90° | 146 s |   182 m |  6515 m
 * 90,00° | 146 s |     0 m |  6515 m
 * 
 * 32. quantity/plain performance ratio: 2181 / 2329 = 0,94 (dimensional analysis: ON)
 * Press Esc to conclude, any other key to retry...
 * 
 * Average performance ratio for 32 run(s): 2087+177(8%) / 2007+175(9%) = 1,04+0,03(3%).
 *
 *
 ********************************************************************************************
 *
 * Sample output (.NET 9.0, dimensional analysis: ON):
 *
 *  angle |  tmax |  xmax   |  ymax
 * -------+-------+---------+--------
 *  0,00° |   0 s |     0 m |     0 m
 *  0,10° |   0 s |   182 m |     0 m
 *  0,20° |   1 s |   364 m |     0 m
 * ...
 * 89,80° | 146 s |   364 m |  6515 m
 * 89,90° | 146 s |   182 m |  6515 m
 * 90,00° | 146 s |     0 m |  6515 m
 *
 * 32. quantity/plain performance ratio: 2120 / 2539 = 0,83 (dimensional analysis: ON)
 * Press Esc to conclude, any other key to retry...
 * 
 * Average performance ratio for 32 run(s): 1789+332(19%) / 1753+349(20%) = 1,02+0,05(5%).
 * 
 */
