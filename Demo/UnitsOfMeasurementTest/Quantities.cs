/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Demo.UnitsOfMeasurement
{
    [TestClass]
    public class Quantities
    {
        [TestClass]
        public class Conversions
        {
            [TestMethod]
            [ExpectedException(typeof(System.InvalidOperationException))]
            public void InconvertiblesThrowsException()
            {
                Meter meters = new(5.0);    // = (Meter)5.0

                // The following is rejected at compile time with error message: Cannot convert type 'Meter' to 'Kilogram'
                //var badSyntax = (Kilogram)meters;

                // The following is rejected at compile time too with error message: Operator '/' cannot be applied to operands of type 'Meter' and 'Kilogram'
                //var badSyntax = meters / (Kilogram)10.0;

                // But the following fails only at runtime throwing InvalidOperationException: Cannot convert type "Meter" to "Kilogram"
                /*var badConversion*/ _ = Kilogram.From(meters);
            }

            [TestMethod]
            [ExpectedException(typeof(System.InvalidCastException))]
            public void InvalidCastingThrowsException()
            {
                IQuantity<double> quantity = (Meter)10.0;

                // The following is a valid conversion:
                Yard yards = Yard.From((Meter)quantity);
                Assert.AreEqual((Yard)10.936132983377078, yards);

                // ... and the following is a valid conversion:
                yards = Yard.From(quantity);
                Assert.AreEqual((Yard)10.936132983377078, yards);

                // ... but the following fails at runtime throwing InvalidCastException: Specified cast is not valid
                /*var badCast*/ _ = (Yard)quantity;
            }

            [TestMethod]
            public void FloatingPointInaccuracy()
            {
                var expected = (Meter)100.0;
                var calculated = 
                    Meter.From(
                        Yard.From(
                            Foot.From(
                                Inch.From(
                                    Centimeter.From(expected)
                                )
                            )
                        )
                    );

                // Unfortunately, due to the floating-point quirks ...:
                Assert.AreNotEqual(expected, calculated);
                // but...:
                Assert.AreEqual((Meter)99.999999999999986, calculated);
            }


            [TestMethod]
            public void Lengths()
            {
                Meter meters = new(100.0);

                Centimeter centimeters = Centimeter.From(meters);
                Millimeter millimeters = Millimeter.From(centimeters);
                Kilometer kilometers = Kilometer.From(millimeters);
                Inch inches = Inch.From(kilometers);
                Foot feet = Foot.From(inches);
                Yard yards = Yard.From(feet);
                Mile miles = Mile.From(yards);
                
                meters = Meter.From(miles);

                Assert.AreEqual((Centimeter)10000.0, centimeters);
                Assert.AreEqual((Millimeter)100000, millimeters);
                Assert.AreEqual((Kilometer)0.099999999999999992, kilometers);
                Assert.AreEqual((Inch)3937.0078740157473, inches);
                Assert.AreEqual((Foot)328.08398950131226, feet);
                Assert.AreEqual((Yard)109.36132983377075, yards);
                Assert.AreEqual((Mile)0.062137119223733391, miles);
                Assert.AreEqual((Meter)99.999999999999972, meters);
            }

            [TestMethod]
            public void TimeUnits()
            {
                Second seconds = (Second)3600.0;

                Minute minutes = Minute.From(seconds);
                Hour hours = Hour.From(minutes);
                
                seconds = Second.From(hours);

                Assert.AreEqual((Minute)60.0, minutes);
                Assert.AreEqual((Hour)1.0, hours);
                Assert.AreEqual((Second)3600.0, seconds);
            }

            [TestMethod]
            public void Masses()
            {
                Kilogram kilograms = (Kilogram)1.0;

                Gram grams = Gram.From(kilograms);
                Tonne tonnes = Tonne.From(grams);
                Pound pounds = Pound.From(tonnes);
                Ounce ounces = Ounce.From(pounds);

                kilograms = Kilogram.From(ounces);

                Assert.AreEqual((Gram)1000.0, grams);
                Assert.AreEqual((Tonne)0.001, tonnes);
                Assert.AreEqual((Pound)2.2046226218487757, pounds);
                Assert.AreEqual((Ounce)35.273961949580411, ounces);
                Assert.AreEqual((Kilogram)1.0, kilograms);

                Assert.AreEqual(16.0, ounces.Value / pounds.Value);

                pounds = (Pound)1.0;
                kilograms = Kilogram.From(pounds);
                grams = Gram.From(kilograms);
                tonnes = Tonne.From(grams);
                ounces = Ounce.From(tonnes);

                pounds = Pound.From(ounces);

                Assert.AreEqual((Kilogram)0.45359237, kilograms);
                Assert.AreEqual((Gram)453.59237, grams);
                Assert.AreEqual((Tonne)0.00045359237, tonnes);
                Assert.AreEqual((Ounce)15.999999999999996, ounces);
                Assert.AreEqual((Pound)0.99999999999999978, pounds);

                Assert.AreEqual(16.0, ounces.Value / pounds.Value);
            }

            [TestMethod]
            public void Temperatures()
            {
                DegCelsius celsius = new(100.0);

                DegFahrenheit fahrenheit = DegFahrenheit.From(celsius);
                DegRankine rankine = DegRankine.From(fahrenheit);
                DegKelvin kelvin = DegKelvin.From(rankine);

                celsius = DegCelsius.From(kelvin);

                Assert.AreEqual((DegFahrenheit)180.0, fahrenheit);
                Assert.AreEqual((DegRankine)180.0, rankine);
                Assert.AreEqual((DegKelvin)100.0, kelvin);
                Assert.AreEqual((DegCelsius)100.0, celsius);
            }

            [TestMethod]
            public void PlaneAngles()
            {
                Turn turns = (Turn)1.0;

                Radian radians = Radian.From(turns);
                Degree degrees = Degree.From(radians);
                Grad grads = Grad.From(degrees);

                turns = Turn.From(grads);

                Assert.AreEqual((Radian)(2.0 * System.Math.PI), radians);
                Assert.AreEqual((Degree)360.0, degrees);
                Assert.AreEqual((Grad)400.0, grads);
                Assert.AreEqual((Turn)1.0, turns);
            }

            //[TestMethod]
            //public void Frequencies()
            //{
            //    Hertz frequency = (Hertz)1000.0;
            //    //Events_Sec evts_sec = Events_Sec.From(frequency);
            //    //Events_Min evts_min = Events_Min.From(evts_sec);
            //    Event_Hour evts_hr = Event_Hour.From(frequency);   // evts_min;

            //    frequency = Hertz.From(evts_hr);

            //    Assert.AreEqual((Hertz)1000.0, frequency);
            //    Assert.AreEqual((Event_Hour)3600000.0, evts_hr);
            //    //Assert.AreEqual((Events_Min)60000.0, evts_min);
            //    //Assert.AreEqual((Events_Sec)1000.0, evts_sec);
            //}

            [TestMethod]
            public void RotationalSpeed()
            {
                RPM rpm = (RPM)7200;

                Degree_Sec degrees_sec = Degree_Sec.From(rpm);
                Radian_Sec angularvelocity = Radian_Sec.From(degrees_sec);

                rpm = RPM.From(angularvelocity);

                Assert.AreEqual((Degree_Sec)(120.0 * 360.0), degrees_sec);
                Assert.AreEqual((Radian_Sec)(240.0 * System.Math.PI), angularvelocity - (Radian_Sec)1.1368683772161603E-13);
                Assert.AreEqual((RPM)7200.0, rpm - (RPM)0.000000000001);

                Second duration = (Second)1.0;
                Turn turns = rpm * Minute.From(duration);
                Assert.AreEqual((Turn)120.0, turns - (Turn)0.00000000000001);
            }

            [TestMethod]
            public void TorqueAndEnergy()
            {
                // Joule and NewtonMeter dimensions are the same...
                Assert.AreEqual(Joule.Sense, NewtonMeter.Sense);
                IEnumerable<Unit<double>> jouleSense = Catalog.Units<double>(Joule.Sense);
                Assert.IsTrue(jouleSense.Contains(Joule.Proxy));
                Assert.IsTrue(jouleSense.Contains(NewtonMeter.Proxy));

                // ...but they belong to disjoint families
                Assert.AreNotEqual(Joule.Family, NewtonMeter.Family);
                IEnumerable<Unit<double>> jouleFamily = Catalog.Units<double>(Joule.Family);
                Assert.IsTrue(jouleFamily.Contains(Joule.Proxy));
                Assert.IsFalse(jouleFamily.Contains(NewtonMeter.Proxy));

                // Besides, they are not very different (except the operators used):
                Meter distance = new(10.0);
                Newton force = new(100.0);

                Joule energy = force * distance;
                Assert.AreEqual((Joule)1000.0, energy);

                NewtonMeter torque = force ^ distance;
                Assert.AreEqual((NewtonMeter)1000.0, torque);

                Meter distanceFromEnergy = energy / force;
                Meter distanceFromTorque = torque / force;
                Assert.AreEqual(distanceFromEnergy, distanceFromTorque);

                Newton forceFromEnergy = energy / distance;
                Newton forceFromTorque = torque / distance;
                Assert.AreEqual(forceFromEnergy, forceFromTorque);
            }

            [TestMethod]
            public void Electrical()
            {
                // Hearing aid (typically 1 mW at 1.4 V): 0.7 mA
                Volt haVoltage = (Volt)1.4;
                Ampere haAmps = (Watt)0.001 / haVoltage;
                Assert.IsTrue((Ampere)0.00071428571428571439 == haAmps, "Hearing aid amperage calculation failed");
                Ohm haOhms = haVoltage / haAmps;
                Assert.IsTrue((Ohm)1959.9999999999995 == haOhms, "Hearing aid resistance calculation failed");

                // A typical motor vehicle has a 12 V battery.
                Volt batteryVoltage = (Volt)12.0;

                // The various accessories that are powered by the battery might include:

                // Instrument panel light (typically 2 W): 166 mA.
                Ampere panelAmps = (Watt)2.0 / batteryVoltage;
                Assert.IsTrue((Ampere)0.16666666666666666 == panelAmps, "Car instrument panel amperage calculation failed");

                Ohm panelOhms = batteryVoltage / panelAmps;
                Assert.IsTrue((Ohm)72.0 == panelOhms, "Car instrument panel resistance calculation failed");

                // Headlights (typically 60 W): 5 A each.
                Ampere lightsAmps = (Watt)60.0 / batteryVoltage;
                Assert.IsTrue((Ampere)5.0 == lightsAmps, "Car headlights amperage calculation failed");

                Ohm lightsOhms = batteryVoltage / lightsAmps;
                Assert.IsTrue((Ohm)2.4 == lightsOhms, "Car headlights resistance calculation failed");

                // Starter Motor (typically 1–2 kW): 80-160 A
                Ampere starterAmps = (Watt)1500.0 / batteryVoltage;
                Assert.IsTrue((Ampere)125.0 == starterAmps, "Car starter motor amperage calculation failed");

                Ohm starterOhms = batteryVoltage / starterAmps;
                Assert.IsTrue((Ohm)0.096 == starterOhms, "Car starter motor resistance calculation failed");
            }
        }

        [TestClass]
        public class Operators
        {
            [TestMethod]
            public void MassComparison()
            {
                {
                    // 5.0 tonnes == 5000 kilograms
                    Tonne tonnes = (Tonne)5.0;
                    Kilogram kilograms = (Kilogram)5000.0;
                    Assert.AreNotEqual(tonnes.Value, kilograms.Value);
                    Assert.IsTrue(tonnes == Tonne.From(kilograms));
                }
                {
                    // 5 pounds < 5 kilograms
                    Pound pounds = (Pound)5.0;
                    Kilogram kilograms = (Kilogram)5.0;
                    Assert.AreEqual(kilograms.Value, pounds.Value);
                    Assert.IsTrue(pounds < Pound.From(kilograms));
                    Assert.IsTrue(kilograms >= Kilogram.From(pounds));
                }
                {
                    // 5 tonnes > 3000 kilograms
                    Tonne tonnes = (Tonne)5.0;
                    Kilogram kilograms = (Kilogram)3000.0;
                    Assert.IsTrue(tonnes.Value < kilograms.Value);
                    Assert.IsTrue(tonnes != Tonne.From(kilograms));
                    Assert.IsTrue(Kilogram.From(tonnes) > kilograms);
                    Assert.IsTrue(Tonne.From(kilograms) <= tonnes);
                }
            }

            [TestMethod]
            public void LengthComparison()
            {
                Meter x = new(123.0);
                Meter y = new(345.0);

                Assert.IsTrue(x != y);
                Assert.IsTrue(x < y);
                Assert.IsTrue(x + y == (Meter)(123.0 + 345.0));
                Assert.IsTrue(y - x == (Meter)(345.0 - 123.0));
                Assert.AreEqual(y / x, 345.0 / 123.0);
            }

            [TestMethod]
            public void Additive()
            {
                Meter meters = (Meter)5.0;  // 5 meters
                Centimeter centimeters = (Centimeter)25.0;  // 25 centimeters

                Meter m = meters + Meter.From(centimeters);
                Assert.IsTrue((Meter)5.25 == m);

                Centimeter cm = Centimeter.From(meters) + centimeters;
                Assert.IsTrue((Centimeter)525.0 == cm);

                m = meters - Meter.From(centimeters);
                Assert.IsTrue((Meter)4.75 == m);

                cm = Centimeter.From(meters) - centimeters;
                Assert.IsTrue((Centimeter)475.0 == cm);
            }

            [TestMethod]
            public void IncrementDecrement()
            {
                Meter meters = (Meter)5.0;
                Meter post = meters++;
                Assert.IsTrue(meters == (Meter)6.0);
                Meter pre = --meters;
                Assert.IsTrue((post == pre) && (pre == meters) && (meters == (Meter)5.0));
            }

            [TestMethod]
            public void Multiplicative()
            {
                MPH speed = (MPH)100.0; // 100 mph
                Minute duration = (Minute)30.0;     // 30 minutes

                Mile miles = speed * Hour.From(duration);
                Assert.IsTrue((Mile)50.0 == miles);

                Kilometer kilometers = Kilometer.From(miles);
                Assert.IsTrue((Kilometer)80.467199999999991 == kilometers);

                speed = miles / Hour.From(duration);
                Assert.IsTrue((MPH)100.0 == speed);
            }

            [TestMethod]
            public void MultiplyOrDivideByNumber()
            {
                Meter meters = (Meter)5.0;  // 5 meters

                meters *= 2.0;  // 10 meters
                Assert.IsTrue((Meter)10.0 == meters);

                meters /= 5.0;  // 2 meters
                Assert.IsTrue((Meter)2.0 == meters);
            }
        }
    }
}
