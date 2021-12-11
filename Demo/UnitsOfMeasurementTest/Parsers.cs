/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/unitsofmeasurement


********************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;

namespace Demo.UnitsOfMeasurement
{
    [TestClass]
    public class Parsers
    {
        //[TestMethod]
        //public void Currencies()
        //{
        //    // The parser accepting any currency from EUR.Family:
        //    QuantityParser<decimal> parser = new(EUR.Family);

        //    bool done = parser.TryParse("100 EUR", CultureInfo.InvariantCulture, out IQuantity<decimal>? amount);
        //    Assert.IsTrue(done && amount is EUR, "Parsing '100 EUR' failed");
        //    Assert.AreEqual((USD)USD.FromEUR(100m), USD.From(amount!), "Converting '100 EUR' into USD failed");
        //}

        [TestMethod]
        public void Lengths()
        {
            // The parser accepting any length from Meter.Family:
            QuantityParser<double> parser = new(Meter.Family);

            bool done = parser.TryParse("100 in", CultureInfo.InvariantCulture, out IQuantity<double>? length);
            Assert.IsTrue(done && length is Inch, "Parsing '100 in' failed");
            Assert.AreEqual((Meter)2.54, Meter.From(length!), "Converting '100 in' into Meter failed");
        }

        /// <summary>
        /// Getting temperatures using <c><see cref="LevelParser{T}"/></c> functionality.
        /// </summary>
        [TestMethod]
        public void Temperatures()
        {
            // The parser accepting any temperature from Kelvin.Family:
            LevelParser<double> parser = new(Kelvin.Family);

            bool done = parser.TryParse("100 °C", CultureInfo.InvariantCulture, out ILevel<double>? level);
            Assert.IsTrue(done && level is Celsius, "Parsing '100 °C' level failed");
            Assert.AreEqual((Fahrenheit)212.0, Fahrenheit.From(level!), "Converting '100 °C' level into Fahrenheit failed");
        }

        /// <summary>
        /// Getting temperatures using (likely recommended) <c><see cref="QuantityParser{T}"/></c> functionality.
        /// </summary>
        /// <remarks>
        /// This test does the same as <c><see cref="Temperatures"/></c> test but
        /// uses a <c><see cref="QuantityParser{T}"/></c> instead of (likely
        /// redundant) <c><see cref="LevelParser{T}"/></c> method.
        /// </remarks>
        [TestMethod]
        public void TemperaturesViaQuantities()
        {
            // The parser accepting any temperature from DegKelvin.Family:
            IEnumerable<Scale<double>> allowedScales = Catalog.Scales<double>(Kelvin.Family);
            QuantityParser<double> parser = new(allowedScales);

            bool done = parser.TryParse("100 °C", CultureInfo.InvariantCulture, out IQuantity<double>? temperature);
            Assert.IsTrue(done && temperature is DegCelsius, "Parsing '100 °C' quantity failed");
            Assert.AreEqual((Fahrenheit)212.0, Fahrenheit.From(temperature!), "Converting '100 °C' quantity into Fahrenheit failed");
        }
    }
}
