using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CALINE3;

namespace Benchmark
{
    public class Sample
    {
        // The following (unnecessary) Params declaration(s) makes
        // BenchmarkDotNet reporting the DIMENSIONAL_ANALYSIS symbol status:
#if DIMENSIONAL_ANALYSIS
        [Params("On")]
        public string DimensionalAnalysis { get; set; } = "On";
#else
        [Params("Off")]
        public string DimensionalAnalysis { get; set; } = "Off";
#endif

        private const string INPUT_DATA = @"EXAMPLE FOUR                             60.100.   0.   0.12        1.
RECP. 1                  -350.       30.       1.8
RECP. 2                     0.       30.       1.8
RECP. 3                   750.      100.       1.8
RECP. 4                   850.       30.       1.8
RECP. 5                  -850.     -100.       1.8
RECP. 6                  -550.     -100.       1.8
RECP. 7                  -350.     -100.       1.8
RECP. 8                    50.     -100.       1.8
RECP. 9                   450.     -100.       1.8
RECP. 10                  800.     -100.       1.8
RECP. 11                 -550.       25.       1.8
RECP. 12                 -550.       25.       6.1
URBAN LOCATION: MULTIPLE LINKS, ETC.      6  4
LINK A              AG   500.     0.  3000.     0.   9700. 30.  0. 23.
LINK B              DP   500.     0.  1000.   100.   1200.150. -2. 13.
LINK C              AG -3000.     0.   500.     0.  10900. 30.  0. 23.
LINK D              AG -3000.   -75.  3000.   -75.   9300. 30.  0. 23.
LINK E              BR  -500.   200.  -500.  -300.   4000. 50. 6.1 27.
LINK F              BR  -100.   200.  -100.  -200.   5000. 50. 6.1 27.
 1.  0.6 1000.12.0
 1. 90.6 1000. 7.0
 1.180.6 1000. 5.0
 1.270.6 1000. 6.7
";
        private readonly Job job;

        public Sample()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                System.Globalization.CultureInfo.InvariantCulture;

            using (System.IO.StringReader input = new(INPUT_DATA))
            {
                JobReader rdr = new(input);
                job = rdr.Read()!;
            }
        }

        [Benchmark]
        public void CALINE3()
        {
            foreach (var meteo in job.Meteos)
            {
                // Mass concentration matrix
                Microgram_Meter3[][] MC = new Microgram_Meter3[job.Links.Count][];

                foreach (var link in job.Links)
                {
                    MC[link.ORDINAL] = new Microgram_Meter3[job.Receptors.Count];

                    // Gaussian plume calculator
                    Plume plume = new(job, meteo, link);

                    foreach (var receptor in job.Receptors)
                    {
                        MC[link.ORDINAL][receptor.ORDINAL] = plume.ConcentrationAt(receptor);
                    }
                }
            }
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Sample>();
        }
    }
}

/*
 * SAMPLE RESULTS:
 * 
 * BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
 * Intel Core i5-3360M CPU 2.80GHz (Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
 * .NET SDK=6.0.100
 *   [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
 *   DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
 * 
 * +--- Summary (DIMESIONAL_ANALYSIS = OFF) --------------------------+
 * |                                                                  |
 * |  Method | DimensionalAnalysis |     Mean |     Error |    StdDev |
 * |-------- |-------------------- |---------:|----------:|----------:|
 * | CALINE3 |                 Off | 2.177 ms | 0.0134 ms | 0.0112 ms |
 * 
 * Outliers
 *   Sample.CALINE3: Default -> 2 outliers were removed, 3 outliers were detected (2.15 ms, 2.21 ms, 2.30 ms)
 * 
 * Run time: 00:00:18 (18.98 sec), executed benchmarks: 1
 * Global total time: 00:00:41 (41.44 sec), executed benchmarks: 1
 * 
 * 
 * +--- Summary (DIMESIONAL_ANALYSIS = ON) ---------------------------+
 * |                                                                  |
 * |  Method | DimensionalAnalysis |     Mean |     Error |    StdDev |
 * |-------- |-------------------- |---------:|----------:|----------:|
 * | CALINE3 |                  On | 2.701 ms | 0.0300 ms | 0.0251 ms |
 * 
 * Outliers
 *   Sample.CALINE3: Default -> 2 outliers were removed (2.78 ms, 2.89 ms)
 * 
 * Run time: 00:00:18 (18.2 sec), executed benchmarks: 1
 * Global total time: 00:00:39 (39.92 sec), executed benchmarks: 1 * 
 * 
 * Legends
 *   DimensionalAnalysis : Value of the 'DimensionalAnalysis' parameter
 *   Mean                : Arithmetic mean of all measurements
 *   Error               : Half of 99.9% confidence interval
 *   StdDev              : Standard deviation of all measurements
 *   1 ms                : 1 Millisecond (0.001 sec)
 * 
 */ 