/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) mangh

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System;

namespace Benchmark
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class Arithmetic
    {
        // The following (unnecessary) Params declaration(s) makes BenchmarkDotNet
        // reporting the DIMENSIONAL_ANALYSIS symbol status:
#if DIMENSIONAL_ANALYSIS
        [Params("On")]
        public string DimensionalAnalysis { get; set; } = "On";
#else
        [Params("Off")]
        public string DimensionalAnalysis { get; set; } = "Off";
#endif

        static readonly Bullet.Plain.Calculator plain = new(0.0, 715.0);
        static readonly Bullet.Measured.Calculator measured = new((Meter)0.0, (Meter_Sec)715.0);

        static readonly double XMAX = 10000.0;
        static readonly double XSTART = 2.0;
        static readonly double YSTART = 1.0;

        static readonly Meter XMAXM = (Meter)XMAX;
        static readonly Meter XSTARTM = (Meter)XSTART;
        static readonly Meter YSTARTM = (Meter)YSTART;

        public Arithmetic()
        {
        }

        public static double LoopDDD(Func<double, double, double> func)
        {
            double ret = 0.0;
            for (double x = XSTART, y = YSTART; x < XMAX; x++, y++) ret = func(x, y);
            return ret;
        }

        public static double LoopDD(Func<double, double> func)
        {
            double ret = 0.0;
            for (double x = XSTART; x < XMAX; x++) ret = func(x);
            return ret;
        }

        public static Meter LoopMMM(Func<Meter, Meter, Meter> func)
        {
            Meter ret = (Meter)0.0;
            for (Meter x = XSTARTM, y = YSTARTM; x < XMAXM; x++, y++) ret = func(x, y);
            return ret;
        }
        public static Meter2 LoopMMM2(Func<Meter, Meter, Meter2> func)
        {
            Meter2 ret = (Meter2)0.0;
            for (Meter x = XSTARTM, y = YSTARTM; x < XMAXM; x++, y++) ret = func(x, y);
            return ret;
        }
        public static double LoopMMD(Func<Meter, Meter, double> func)
        {
            double ret = 0.0;
            for (Meter x = XSTARTM, y = YSTARTM; x < XMAXM; x++, y++) ret = func(x, y);
            return ret;
        }
        public static Yard LoopMY(Func<Meter, Yard> func)
        {
            Yard ret = (Yard)0.0;
            for (Meter x = XSTARTM; x < XMAXM; x++) ret = func(x);
            return ret;
        }

        [BenchmarkCategory("Add"), Benchmark(Baseline = true)] public double Add() => LoopDDD((x, y) => x + y);
        [BenchmarkCategory("Sub"), Benchmark(Baseline = true)] public double Sub() => LoopDDD((x, y) => x - y);
        [BenchmarkCategory("Mul"), Benchmark(Baseline = true)] public double Mul() => LoopDDD((x, y) => x * y);
        [BenchmarkCategory("Div"), Benchmark(Baseline = true)] public double Div() => LoopDDD((x, y) => x / y);
        [BenchmarkCategory("Cvt"), Benchmark(Baseline = true)] public double Cvt() => LoopDD(x => Demo.UnitsOfMeasurement.Yard.FromMeter(x));
        [BenchmarkCategory("Mix"), Benchmark(Baseline = true)] public double Mix() => LoopDDD((x, y) =>
        {
            double sum = x + y;
            double diff = x - y;
            double product = x * y;
            double quotient = x / y;
            double yards = Demo.UnitsOfMeasurement.Yard.FromMeter(sum);
            return product / (sum + diff) * quotient + Demo.UnitsOfMeasurement.Meter.FromYard(yards);
        });

        [BenchmarkCategory("Bullet"), Benchmark(Baseline = true)] public (double, double, double, double) Bullet()
        {
            var result = (0.0, 0.0, 0.0, 0.0);
            for (double slope = 0.0; slope < 90.0; slope += 0.1)
            {
                result = plain.CalculateRange(slope);
            }
            return result;
        }

        [BenchmarkCategory("Add"), Benchmark] public Meter QAdd() => LoopMMM((Meter p, Meter q) => p + q);
        [BenchmarkCategory("Sub"), Benchmark] public Meter QSub() => LoopMMM((Meter p, Meter q) => p - q);
        [BenchmarkCategory("Mul"), Benchmark] public Meter2 QMul() => LoopMMM2((Meter p, Meter q) => p * q);
        [BenchmarkCategory("Div"), Benchmark] public double QDiv() => LoopMMD((Meter p, Meter q) => p / q);
        [BenchmarkCategory("Cvt"), Benchmark] public Yard QCvt() => LoopMY(x =>
#if DIMENSIONAL_ANALYSIS
            Yard.From(x)
#else
            Demo.UnitsOfMeasurement.Yard.FromMeter(x)
#endif
        );
        [BenchmarkCategory("Mix"), Benchmark] public Meter QMix() => LoopMMM((Meter p, Meter q) =>
        {
            Meter sum = p + q;
            Meter diff = p - q;
            Meter2 product = p * q;
            double quotient = p / q;
#if DIMENSIONAL_ANALYSIS
            Yard yards = Yard.From(sum);
            return product / (sum + diff) * quotient + Meter.From(yards);
#else
            Yard yards = Demo.UnitsOfMeasurement.Yard.FromMeter(sum);
            return product / (sum + diff) * quotient + Demo.UnitsOfMeasurement.Meter.FromYard(yards);
#endif
        });

        [BenchmarkCategory("Bullet"), Benchmark]
        public (Degree, Second, Meter, Meter) QBullet()
        {
            var result = ((Degree)0.0, (Second)0.0, (Meter)0.0, (Meter)0.0);
            for (double slope = 0.0; slope < 90.0; slope += 0.1)
            {
                result = measured.CalculateRange((Degree)slope);
            }
            return result;
        }
    }

    public class Program
    {
        public static void Main()
        {
            _ = BenchmarkRunner.Run<Arithmetic>();
        }
    }
}

/*
 * Sample results (shortened):
 * 
 * BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2)
 * 11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
 * .NET SDK=7.0.304
 *   [Host]     : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
 *   DefaultJob : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
 * 
 * 
 * Summary (DIMENSIONAL_ANALYSIS: ON):
 * 
 * |  Method | DimensionalAnalysis |      Mean |    Error |   StdDev |    Median | Ratio | RatioSD |
 * |-------- |-------------------- |----------:|---------:|---------:|----------:|------:|--------:|
 * |     Add |                  On |  14.58 μs | 0.287 μs | 0.455 μs |  14.58 μs |  1.00 |    0.00 |
 * |    QAdd |                  On |  46.05 μs | 0.922 μs | 1.062 μs |  46.49 μs |  3.18 |    0.10 |
 * |         |                     |           |          |          |           |       |         |
 * |  Bullet |                  On | 132.10 μs | 2.565 μs | 2.519 μs | 132.89 μs |  1.00 |    0.00 |
 * | QBullet |                  On | 320.80 μs | 6.412 μs | 6.298 μs | 319.45 μs |  2.43 |    0.07 |
 * |         |                     |           |          |          |           |       |         |
 * |     Cvt |                  On |  14.61 μs | 0.291 μs | 0.335 μs |  14.66 μs |  1.00 |    0.00 |
 * |    QCvt |                  On |  38.63 μs | 0.757 μs | 1.156 μs |  38.42 μs |  2.64 |    0.11 |
 * |         |                     |           |          |          |           |       |         |
 * |     Div |                  On |  14.79 μs | 0.294 μs | 0.475 μs |  14.85 μs |  1.00 |    0.00 |
 * |    QDiv |                  On |  47.60 μs | 0.938 μs | 1.080 μs |  47.41 μs |  3.24 |    0.12 |
 * |         |                     |           |          |          |           |       |         |
 * |     Mix |                  On |  24.72 μs | 1.141 μs | 3.200 μs |  24.02 μs |  1.00 |    0.00 |
 * |    QMix |                  On |  50.50 μs | 1.001 μs | 2.089 μs |  51.36 μs |  1.92 |    0.22 |
 * |         |                     |           |          |          |           |       |         |
 * |     Mul |                  On |  14.88 μs | 0.297 μs | 0.386 μs |  15.02 μs |  1.00 |    0.00 |
 * |    QMul |                  On |  45.84 μs | 0.910 μs | 1.151 μs |  45.45 μs |  3.08 |    0.11 |
 * |         |                     |           |          |          |           |       |         |
 * |     Sub |                  On |  14.94 μs | 0.293 μs | 0.360 μs |  15.05 μs |  1.00 |    0.00 |
 * |    QSub |                  On |  46.85 μs | 0.910 μs | 1.215 μs |  47.35 μs |  3.15 |    0.13 |
 * 
 * Run time: 00:07:34 (454.59 sec), executed benchmarks: 14
 * Global total time: 00:07:53 (473.35 sec), executed benchmarks: 14
 * 
 * 
 * Summary (DIMENSIONAL_ANALYSIS: OFF):
 * 
 * |  Method | DimensionalAnalysis |      Mean |    Error |   StdDev |    Median | Ratio | RatioSD |
 * |-------- |-------------------- |----------:|---------:|---------:|----------:|------:|--------:|
 * |     Add |                 Off |  14.65 μs | 0.288 μs | 0.481 μs |  14.73 μs |  1.00 |    0.00 |
 * |    QAdd |                 Off |  14.66 μs | 0.291 μs | 0.398 μs |  14.83 μs |  1.01 |    0.04 |
 * |         |                     |           |          |          |           |       |         |
 * |  Bullet |                 Off | 133.89 μs | 2.061 μs | 1.928 μs | 135.22 μs |  1.00 |    0.00 |
 * | QBullet |                 Off | 133.90 μs | 2.103 μs | 1.967 μs | 134.73 μs |  1.00 |    0.02 |
 * |         |                     |           |          |          |           |       |         |
 * |     Cvt |                 Off |  14.71 μs | 0.291 μs | 0.368 μs |  14.90 μs |  1.00 |    0.00 |
 * |    QCvt |                 Off |  14.79 μs | 0.287 μs | 0.353 μs |  14.94 μs |  1.01 |    0.04 |
 * |         |                     |           |          |          |           |       |         |
 * |     Div |                 Off |  14.91 μs | 0.298 μs | 0.377 μs |  15.06 μs |  1.00 |    0.00 |
 * |    QDiv |                 Off |  14.87 μs | 0.297 μs | 0.454 μs |  14.74 μs |  1.00 |    0.04 |
 * |         |                     |           |          |          |           |       |         |
 * |     Mix |                 Off |  21.73 μs | 0.430 μs | 0.512 μs |  22.05 μs |  1.00 |    0.00 |
 * |    QMix |                 Off |  21.45 μs | 0.420 μs | 0.653 μs |  21.63 μs |  0.99 |    0.04 |
 * |         |                     |           |          |          |           |       |         |
 * |     Mul |                 Off |  14.83 μs | 0.292 μs | 0.464 μs |  14.97 μs |  1.00 |    0.00 |
 * |    QMul |                 Off |  14.86 μs | 0.294 μs | 0.402 μs |  14.79 μs |  1.01 |    0.04 |
 * |         |                     |           |          |          |           |       |         |
 * |     Sub |                 Off |  14.90 μs | 0.288 μs | 0.375 μs |  14.80 μs |  1.00 |    0.00 |
 * |    QSub |                 Off |  14.87 μs | 0.292 μs | 0.479 μs |  15.08 μs |  1.00 |    0.04 |
 * 
 * 
 * Run time: 00:06:31 (391.17 sec), executed benchmarks: 14
 * Global total time: 00:06:49 (409.08 sec), executed benchmarks: 14
 * 
 * 
 * Summary (DIMENSIONAL_ANALYSIS: ON, NET 8):
 * 
 * BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3296/23H2/2023Update/SunValley3)
 * 11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
 * .NET SDK 8.0.202
 *   [Host]     : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
 *   DefaultJob : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
 * 
 * | Method  | DimensionalAnalysis | Mean       | Error     | StdDev    | Ratio | RatioSD |
 * |-------- |-------------------- |-----------:|----------:|----------:|------:|--------:|
 * | Add     | On                  |  10.791 μs | 0.0255 μs | 0.0226 μs |  1.00 |    0.00 |
 * | QAdd    | On                  |  10.512 μs | 0.0225 μs | 0.0199 μs |  0.97 |    0.00 |
 * |         |                     |            |           |           |       |         |
 * | Bullet  | On                  | 129.410 μs | 1.4048 μs | 1.3141 μs |  1.00 |    0.00 |
 * | QBullet | On                  | 131.919 μs | 1.3563 μs | 1.2687 μs |  1.02 |    0.01 |
 * |         |                     |            |           |           |       |         |
 * | Cvt     | On                  |  10.662 μs | 0.0252 μs | 0.0236 μs |  1.00 |    0.00 |
 * | QCvt    | On                  |  10.618 μs | 0.0221 μs | 0.0196 μs |  1.00 |    0.00 |
 * |         |                     |            |           |           |       |         |
 * | Div     | On                  |   8.603 μs | 0.0146 μs | 0.0137 μs |  1.00 |    0.00 |
 * | QDiv    | On                  |   8.783 μs | 0.0446 μs | 0.0417 μs |  1.02 |    0.01 |
 * |         |                     |            |           |           |       |         |
 * | Mix     | On                  |  18.165 μs | 0.2443 μs | 0.2285 μs |  1.00 |    0.00 |
 * | QMix    | On                  |  18.590 μs | 0.2858 μs | 0.2673 μs |  1.02 |    0.02 |
 * |         |                     |            |           |           |       |         |
 * | Mul     | On                  |  10.753 μs | 0.0175 μs | 0.0146 μs |  1.00 |    0.00 |
 * | QMul    | On                  |  10.506 μs | 0.0162 μs | 0.0152 μs |  0.98 |    0.00 |
 * |         |                     |            |           |           |       |         |
 * | Sub     | On                  |  10.752 μs | 0.0176 μs | 0.0147 μs |  1.00 |    0.00 |
 * | QSub    | On                  |  10.506 μs | 0.0217 μs | 0.0192 μs |  0.98 |    0.00 |
 * 
 * // * Hints *
 * Outliers
 *   Arithmetic.Add: Default  -> 1 outlier  was  removed (10.87 μs)
 *   Arithmetic.QAdd: Default -> 1 outlier  was  removed (10.60 μs)
 *   Arithmetic.QCvt: Default -> 1 outlier  was  removed (11.06 μs)
 *   Arithmetic.Mul: Default  -> 2 outliers were removed (10.86 μs, 11.02 μs)
 *   Arithmetic.Sub: Default  -> 2 outliers were removed (10.80 μs, 10.83 μs)
 *   Arithmetic.QSub: Default -> 1 outlier  was  removed (10.69 μs)
 * 
 * // * Legends *
 *   DimensionalAnalysis : Value of the 'DimensionalAnalysis' parameter
 *   Mean                : Arithmetic mean of all measurements
 *   Error               : Half of 99.9% confidence interval
 *   StdDev              : Standard deviation of all measurements
 *   Ratio               : Mean of the ratio distribution ([Current]/[Baseline])
 *   RatioSD             : Standard deviation of the ratio distribution ([Current]/[Baseline])
 *   1 μs                : 1 Microsecond (0.000001 sec)
 * 
 * // ***** BenchmarkRunner: End *****
 * Run time: 00:03:47 (227.14 sec), executed benchmarks: 14
 * Global total time: 00:03:51 (231.93 sec), executed benchmarks: 14
 * 
 */
