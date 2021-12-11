/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Mangh.Metrology;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Demo.UnitsOfMeasurement
{
    using Exponent = System.Int32;

    [TestClass]
    public class Dimensions
    {

        // Valid range of dimension exponent, for any single magnitude is:
        // [-128,127] for 64-bit dimension exponents (8-bit exponents).
        private static readonly int MAXEXP = 127;   // exponent max value
        private static readonly int MINEXP = -128;  // exponent min value

        [TestClass]
        public class Basics
        {
            [TestMethod]
            public void Constants()
            {
                Assert.AreEqual(Dimension.None, new Dimension(0, 0, 0, 0, 0, 0, 0, 0));
                Assert.AreEqual(Dimension.Length, new Dimension(1, 0, 0, 0, 0, 0, 0, 0));
                Assert.AreEqual(Dimension.Time, new Dimension(0, 1, 0, 0, 0, 0, 0, 0));
                Assert.AreEqual(Dimension.Mass, new Dimension(0, 0, 1, 0, 0, 0, 0, 0));
                Assert.AreEqual(Dimension.Temperature, new Dimension(0, 0, 0, 1, 0, 0, 0, 0));
                Assert.AreEqual(Dimension.ElectricCurrent, new Dimension(0, 0, 0, 0, 1, 0, 0, 0));
                Assert.AreEqual(Dimension.AmountOfSubstance, new Dimension(0, 0, 0, 0, 0, 1, 0, 0));
                Assert.AreEqual(Dimension.LuminousIntensity, new Dimension(0, 0, 0, 0, 0, 0, 1, 0));
                Assert.AreEqual(Dimension.Other, new Dimension(0, 0, 0, 0, 0, 0, 0, 1));
                Assert.AreEqual(Dimension.Money, new Dimension(0, 0, 0, 0, 0, 0, 0, 1));
            }

            [TestMethod]
            public void Magnitudes()
            {
                Assert.AreEqual(Dimension.None, new());
                Assert.AreEqual(Dimension.Length, new(Magnitude.Length));
                Assert.AreEqual(Dimension.Time, new(Magnitude.Time));
                Assert.AreEqual(Dimension.Mass, new(Magnitude.Mass));
                Assert.AreEqual(Dimension.Temperature, new(Magnitude.Temperature));
                Assert.AreEqual(Dimension.ElectricCurrent, new(Magnitude.ElectricCurrent));
                Assert.AreEqual(Dimension.AmountOfSubstance, new(Magnitude.AmountOfSubstance));
                Assert.AreEqual(Dimension.LuminousIntensity, new(Magnitude.LuminousIntensity));
                Assert.AreEqual(Dimension.Other, new(Magnitude.Other));
                Assert.AreEqual(Dimension.Money, new(Magnitude.Money));
            }

            [TestMethod]
            [ExpectedException(typeof(System.OverflowException))]
            public void OutOfRangeLargeExponentThrowException()
            {
                _ = new Dimension(MAXEXP + 1, 0, 0, 0, 0, 0, 0, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(System.OverflowException))]
            public void OutOfRangeSmallExponentThrowException()
            {
                _ = new Dimension(0, MINEXP - 1, 0, 0, 0, 0, 0, 0);
            }
        }

        [TestClass]
        public class Operators
        {
            [TestMethod]
            public void Equality()
            {
                Assert.IsTrue(Dimension.None == new Dimension(0, 0, 0, 0, 0, 0, 0, 0));
                Assert.IsTrue(Dimension.Length == new Dimension(1, 0, 0, 0, 0, 0, 0, 0));
                Assert.IsTrue(Dimension.Time == new Dimension(0, 1, 0, 0, 0, 0, 0, 0));
                Assert.IsTrue(Dimension.Mass == new Dimension(0, 0, 1, 0, 0, 0, 0, 0));
                Assert.IsTrue(Dimension.Temperature == new Dimension(0, 0, 0, 1, 0, 0, 0, 0));
                Assert.IsTrue(Dimension.ElectricCurrent == new Dimension(0, 0, 0, 0, 1, 0, 0, 0));
                Assert.IsTrue(Dimension.AmountOfSubstance == new Dimension(0, 0, 0, 0, 0, 1, 0, 0));
                Assert.IsTrue(Dimension.LuminousIntensity == new Dimension(0, 0, 0, 0, 0, 0, 1, 0));
                Assert.IsTrue(Dimension.Other == new Dimension(0, 0, 0, 0, 0, 0, 0, 1));
                Assert.IsTrue(Dimension.Money == new Dimension(0, 0, 0, 0, 0, 0, 0, 1));
            }

            [TestMethod]
            public void Inequality()
            {
                for (int i = 0; i < 8; i++)
                {
                    Dimension lhs = new((Magnitude)i);
                    for(int j = 0; j < 8; j++)
                    {
                        Dimension rhs = new((Magnitude)j);
                        Assert.IsTrue((j == i) ? lhs == rhs : lhs != rhs);
                    }
                }
            }

            [TestMethod]
            public void Product()
            {
                CheckOperation((a, b) => a * b, (a, b) => a + b);
            }

            [TestMethod]
            [ExpectedException(typeof(System.OverflowException))]
            public void ProductGivingExponentOutOfRangeThrowsException()
            {
                Dimension a = new(MAXEXP / 2 + 1, 0, 0, 0, 0, 0, 0, 0);
                Dimension b = new(MAXEXP / 2 + 1, 0, 0, 0, 0, 0, 0, 0);
                _ = a * b;
            }

            [TestMethod]
            public void Quotient()
            {
                CheckOperation((a, b) => a / b, (a, b) => a - b);
            }

            [TestMethod]
            [ExpectedException(typeof(System.OverflowException))]
            public void QuotienGivingExponentOutOfRangeThrowsException()
            {
                Dimension a = new(MINEXP / 2 - 1, 0, 0, 0, 0, 0, 0, 0);
                Dimension b = new(MAXEXP / 2 + 1, 0, 0, 0, 0, 0, 0, 0);
                _ = a / b;
            }

            private static void CheckOperation(System.Func<Dimension, Dimension, Dimension> operation, System.Func<Exponent, Exponent, Exponent> expected)
            {
                Exponent[] lexp = { 0, 0, 0, 0, 0, 0, 0, 0 };   // 8 exponents for left-hand side expression
                Exponent[] rexp = { 0, 0, 0, 0, 0, 0, 0, 0 };   // 8 exponents for right-hand side expression

                for (int m = 0; m < lexp.Length; m++)
                {
                    for (int lval = MINEXP; lval <= MAXEXP; lval++)
                    {
                        lexp[m] = lval;
                        Dimension lhs = new(
                            length: lexp[0],
                            time: lexp[1],
                            mass: lexp[2],
                            temperature: lexp[3],
                            current: lexp[4],
                            substance: lexp[5],
                            intensity: lexp[6],
                            other: lexp[7]
                        );

                        for (int rval = MINEXP; rval <= MAXEXP; rval++)
                        {
                            Exponent foreseen = expected(lval, rval);
                            if ((MINEXP <= foreseen) && (foreseen <= MAXEXP))
                            {
                                Dimension rhs = new(
                                    length: rexp[0],
                                    time: rexp[1],
                                    mass: rexp[2],
                                    temperature: rexp[3],
                                    current: rexp[4],
                                    substance: rexp[5],
                                    intensity: rexp[6],
                                    other: rexp[7]
                                );

                                Dimension result = operation(lhs, rhs);

                                for (int f = 0; f < rexp.Length; f++)
                                {
                                    foreseen = expected(lexp[f], rexp[f]);
                                    Assert.IsTrue(MINEXP <= foreseen && foreseen <= MAXEXP);
                                    Assert.AreEqual(result[(Magnitude)f], foreseen);
                                    Assert.AreEqual(lhs[(Magnitude)f], lexp[f]);
                                    Assert.AreEqual(rhs[(Magnitude)f], rexp[f]);
                                }
                            }
                        }
                    }
                }
            }

            [TestMethod]
            public void MechanicalSamples()
            {
                Dimension velocity = Dimension.Length / Dimension.Time;
                Assert.IsTrue(velocity == new Dimension(1, -1, 0, 0, 0, 0, 0, 0));
                Assert.AreEqual(velocity[Magnitude.Length], 1);
                Assert.AreEqual(velocity[Magnitude.Time], -1);

                Dimension acceleration = velocity / Dimension.Time;
                Assert.IsTrue(acceleration == new Dimension(1, -2, 0, 0, 0, 0, 0, 0));
                Assert.AreEqual(acceleration[Magnitude.Length], 1);
                Assert.AreEqual(acceleration[Magnitude.Time], -2);

                Dimension force = Dimension.Mass * acceleration;
                Assert.IsTrue(force == new Dimension(1, -2, 1, 0, 0, 0, 0, 0));
                Assert.AreEqual(force[Magnitude.Mass], 1);
                Assert.AreEqual(force[Magnitude.Length], 1);
                Assert.AreEqual(force[Magnitude.Time], -2);
            }
        }

        [TestClass]
        public class Power
        {
            [TestMethod]
            [ExpectedException(typeof(System.ArgumentException))]
            public void PowerGivingFractionalExponentsThrowsException()
            {
                // Example: length^(2/3) -> exception!
                _ = Dimension.Pow(Dimension.Length, 2, 3); // {2/3, 0, 0, 0, 0, 0, 0, 0} -> exception
            }

            [TestMethod]
            public void PowerGivingIntegralExponents()
            {
                // Example: (length^3)^(1/3) -> OK
                Dimension a = new(Magnitude.Length);
                Dimension vol = a * a * a;
                Dimension area = Dimension.Pow(vol, 2, 3); // {3*(2/3), 0, 0, 0, 0, 0, 0, 0} -> OK
                Assert.AreEqual(area[Magnitude.Length], 2);
            }

            [TestMethod]
            [ExpectedException(typeof(System.OverflowException))]
            public void PowerGivingExponentsOutOfRangeThrowsException()
            {
                Dimension a = new(MINEXP/2 - 1, 0, 0, 0, 0, 0, 0, 0);
                _ = Dimension.Pow(a, 2);
            }

            [TestMethod]
            public void SqrtGivingIntegralExponents()
            {
                // Example: (length^2)^(1/2) -> OK
                Dimension area = Dimension.Pow(Dimension.Length, 2);
                Assert.AreEqual(area[Magnitude.Length], 2);
                Dimension len = Dimension.Sqrt(area);
                Assert.AreEqual(len[Magnitude.Length], 1);
            }

            [TestMethod]
            [ExpectedException(typeof(System.ArgumentException))]
            public void SqrtGivingFractionalExponentsThrowsException()
            {
                // Example: (length^3)^(1/2) -> exception!
                Dimension vol = Dimension.Pow(Dimension.Length, 3);
                _ = Dimension.Sqrt(vol);
            }

            [TestMethod]
            public void CubrtGivingIntegralExponents()
            {
                // Example: (length^3)^(1/3) -> OK
                Dimension vol = Dimension.Pow(Dimension.Length, 3);
                Assert.AreEqual(vol[Magnitude.Length], 3);
                Dimension len = Dimension.Cubrt(vol);
                Assert.AreEqual(len[Magnitude.Length], 1);
            }

            [TestMethod]
            [ExpectedException(typeof(System.ArgumentException))]
            public void CubrtGivingFractionalExponentsThrowsException()
            {
                // Example: (length^2)^(1/3) -> panic!
                Dimension area = Dimension.Pow(Dimension.Length, 2);
                _ = Dimension.Cubrt(area);
            }
        }
    }
}