/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Demo.UnitsOfMeasurement
{
    [TestClass]
    public class Levels
    {
        [TestClass]
        public class Conversions
        {
            [TestMethod]
            public void Temperatures()
            {
                Celsius celsius = (Celsius)100.0;   // == new(100.0) == new((DegCelsius)100.0)
                Fahrenheit fahrenheit = Fahrenheit.From(celsius);
                Rankine rankine = Rankine.From(fahrenheit);
                Kelvin kelvin = Kelvin.From(rankine);
                celsius = Celsius.From(kelvin);

                Assert.AreEqual((Fahrenheit)212.0, fahrenheit);
                Assert.AreEqual((Rankine)671.67, rankine);
                Assert.AreEqual((Kelvin)373.15, kelvin);
                Assert.AreEqual((Celsius)100.0, celsius);
            }

            /// <summary>
            /// Converting temperatures using <c>scale.From(<see cref="ILevel{T}"/>)</c> method.
            /// </summary>
            /// <remarks>
            /// This test does the same as <see cref="Temperatures"/> test
            /// but uses a <c>scale.From(<see cref="ILevel{T}"/>)</c> method
            /// that casts its argument the the <see cref="ILevel{T}"/> interface;
            /// that causes (as for any interface) so called "boxing" and
            /// makes the method slower than its counterpart with an explicit argument.
            /// </remarks>
            [TestMethod]
            public void TemperaturesFrom()
            {
                ILevel<double> celsius = (Celsius)100.0;
                ILevel<double> fahrenheit = Fahrenheit.From(celsius);
                ILevel<double> rankine = Rankine.From(fahrenheit);
                ILevel<double> kelvin = Kelvin.From(rankine);
                celsius = Celsius.From(kelvin);

                Assert.AreEqual((Fahrenheit)212.0, fahrenheit);
                Assert.AreEqual((Rankine)671.67, rankine);
                Assert.AreEqual((Kelvin)373.15, kelvin);
                Assert.AreEqual((Celsius)100.0, celsius);
            }

            [TestMethod]
            public void FloatingPointInaccuracy()
            {
                var expected = (Fahrenheit)123.45;
                var calculated = 
                    Fahrenheit.From(
                        Kelvin.From(
                            Rankine.From(
                                Celsius.From(expected)
                            )
                        )
                    );

                // Unfortunately, due to the floating-point quirks ...:
                Assert.AreNotEqual(expected, calculated);
                // but...:
                Assert.AreEqual((Fahrenheit)123.45000000000005, calculated);
            }
        }

        [TestClass]
        public class Operators
        {

            [TestMethod]
            public void Comparisons()
            {
                {
                    // 100 C == 671.67 R
                    Rankine rankine = (Rankine)671.67;
                    Celsius celsius = (Celsius)100.0;
                    Assert.IsTrue(rankine == Rankine.From(celsius));
                    Assert.IsTrue(Celsius.From(rankine) == celsius);
                }
                {
                    // 100 C > 100 R
                    Rankine rankine = (Rankine)100.0;
                    Celsius celsius = (Celsius)100.0;
                    Assert.IsTrue(rankine < Rankine.From(celsius));
                    Assert.IsTrue(celsius >= Celsius.From(rankine));
                }
                {
                    Celsius celsius = (Celsius)100.0;
                    Assert.IsTrue(celsius == Celsius.From((Kelvin)(100.0 + 273.15)));
                    Assert.IsTrue(celsius.Equals(Celsius.From((Kelvin)(100.0 + 273.15))));
                    Assert.IsTrue(Kelvin.From(celsius) > (Kelvin)100.0);
                    Assert.IsTrue(Kelvin.From(celsius) >= (Kelvin)100.0);
                    Assert.IsTrue((Kelvin)100.0 < Kelvin.From(celsius));
                    Assert.IsTrue((Kelvin)100.0 <= Kelvin.From(celsius));
                }
            }

            [TestMethod]
            public void Additive()
            {
                DegKelvin kelvins = new(5.0);
                DegCelsius growth = DegCelsius.From(kelvins);

                Assert.IsTrue((Celsius)100.0 + growth == (Celsius)105.0);
                Assert.IsTrue((Celsius)105.0 - growth == (Celsius)100.0);
                Assert.IsTrue((Celsius)105.0 - (Celsius)100.0 == growth);

                DegRankine rankines = new(9.0);
                growth = DegCelsius.From(rankines);

                Assert.IsTrue((Celsius)100.0 + growth == (Celsius)105.0);
                Assert.IsTrue(growth + (Celsius)100.0 == Celsius.From((Fahrenheit)221.0));
                Assert.IsTrue((Celsius)100.0 + growth == growth + (Celsius)100.0);
                Assert.IsTrue((Celsius)105.0 - growth == (Celsius)100.0);
                Assert.IsTrue((Celsius)105.0 - (Celsius)100.0 == growth);
            }

            [TestMethod]
            public void IncrementDecrement()
            {
                Fahrenheit temperature = new(5.0);
                var post = temperature++;
                Assert.IsTrue(temperature == (Fahrenheit)6.0);
                var pre = --temperature;
                Assert.IsTrue((post == pre) && (pre == temperature) && (temperature == (Fahrenheit)5.0));
            }
        }
    }
}
