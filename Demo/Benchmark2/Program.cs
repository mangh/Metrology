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

        [BenchmarkCategory("Bullet"), Benchmark(Baseline = true)]
        public (double, double, double, double) Bullet()
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
 * BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19043.1348 (21H1/May2021Update)
 * Intel Core i5-3360M CPU 2.80GHz (Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
 * .NET SDK=6.0.100
 *   [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
 *   DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
 * 
 * 
 * Summary (DIMENSIONAL_ANALYSIS: ON):
 * 
 * |  Method | DimensionalAnalysis |        Mean |    Error |   StdDev | Ratio | RatioSD |
 * |-------- |-------------------- |------------:|---------:|---------:|------:|--------:|
 * |     Add |                  On |    29.55 μs | 0.110 μs | 0.086 μs |  1.00 |    0.00 |
 * |    QAdd |                  On |    65.69 μs | 0.405 μs | 0.379 μs |  2.22 |    0.02 |
 * |         |                     |             |          |          |       |         |
 * |     Sub |                  On |    29.62 μs | 0.155 μs | 0.137 μs |  1.00 |    0.00 |
 * |    QSub |                  On |    66.00 μs | 0.341 μs | 0.319 μs |  2.23 |    0.02 |
 * |         |                     |             |          |          |       |         |
 * |     Mul |                  On |    29.57 μs | 0.160 μs | 0.150 μs |  1.00 |    0.00 |
 * |    QMul |                  On |    66.75 μs | 0.292 μs | 0.273 μs |  2.26 |    0.01 |
 * |         |                     |             |          |          |       |         |
 * |     Div |                  On |    41.17 μs | 0.265 μs | 0.235 μs |  1.00 |    0.00 |
 * |    QDiv |                  On |    66.16 μs | 0.466 μs | 0.413 μs |  1.61 |    0.01 |
 * |         |                     |             |          |          |       |         |
 * |     Cvt |                  On |    26.67 μs | 0.173 μs | 0.144 μs |  1.00 |    0.00 |
 * |    QCvt |                  On |    63.95 μs | 0.804 μs | 0.672 μs |  2.40 |    0.03 |
 * |         |                     |             |          |          |       |         |
 * |     Mix |                  On |    83.30 μs | 0.352 μs | 0.329 μs |  1.00 |    0.00 |
 * |    QMix |                  On |   181.88 μs | 3.039 μs | 3.252 μs |  2.18 |    0.04 |
 * |         |                     |             |          |          |       |         |
 * |  Bullet |                  On |   199.83 μs | 1.232 μs | 0.962 μs |  1.00 |    0.00 |
 * | QBullet |                  On | 1,089.00 μs | 4.977 μs | 4.156 μs |  5.45 |    0.03 |
 * 
 * Run time: 00:04:24 (264.47 sec), executed benchmarks: 14
 * Global total time: 00:04:47 (287.24 sec), executed benchmarks: 14
 * 
 * 
 * Summary (DIMENSIONAL_ANALYSIS: OFF):
 * 
 * |  Method | DimensionalAnalysis |      Mean |    Error |   StdDev | Ratio |
 * |-------- |-------------------- |----------:|---------:|---------:|------:|
 * |     Add |                 Off |  29.73 μs | 0.114 μs | 0.101 μs |  1.00 |
 * |    QAdd |                 Off |  29.73 μs | 0.151 μs | 0.134 μs |  1.00 |
 * |         |                     |           |          |          |       |
 * |     Sub |                 Off |  29.64 μs | 0.148 μs | 0.138 μs |  1.00 |
 * |    QSub |                 Off |  29.58 μs | 0.165 μs | 0.155 μs |  1.00 |
 * |         |                     |           |          |          |       |
 * |     Mul |                 Off |  29.64 μs | 0.167 μs | 0.148 μs |  1.00 |
 * |    QMul |                 Off |  29.59 μs | 0.200 μs | 0.156 μs |  1.00 |
 * |         |                     |           |          |          |       |
 * |     Div |                 Off |  41.30 μs | 0.257 μs | 0.214 μs |  1.00 |
 * |    QDiv |                 Off |  41.34 μs | 0.264 μs | 0.247 μs |  1.00 |
 * |         |                     |           |          |          |       |
 * |     Cvt |                 Off |  26.76 μs | 0.146 μs | 0.137 μs |  1.00 |
 * |    QCvt |                 Off |  26.65 μs | 0.109 μs | 0.097 μs |  1.00 |
 * |         |                     |           |          |          |       |
 * |     Mix |                 Off |  83.45 μs | 0.558 μs | 0.522 μs |  1.00 |
 * |    QMix |                 Off |  83.06 μs | 0.453 μs | 0.424 μs |  1.00 |
 * |         |                     |           |          |          |       |
 * |  Bullet |                 Off | 198.78 μs | 0.908 μs | 0.850 μs |  1.00 |
 * | QBullet |                 Off | 199.85 μs | 1.128 μs | 1.055 μs |  1.01 |
 * 
 * Run time: 00:05:11 (311.17 sec), executed benchmarks: 14
 * Global total time: 00:05:33 (333.2 sec), executed benchmarks: 14
 * 
 */ 