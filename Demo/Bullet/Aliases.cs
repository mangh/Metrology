﻿#if DIMENSIONAL_ANALYSIS
    global using Demo.UnitsOfMeasurement;
    global using static Demo.UnitsOfMeasurement.Meter;
    global using static Demo.UnitsOfMeasurement.Second;
    global using static Demo.UnitsOfMeasurement.Kilogram;
    global using static Demo.UnitsOfMeasurement.DegKelvin;
    global using static Demo.UnitsOfMeasurement.Ampere;
    global using static Demo.UnitsOfMeasurement.Mole;
    global using static Demo.UnitsOfMeasurement.Candela;
    global using static Demo.UnitsOfMeasurement.EUR;
    global using static Demo.UnitsOfMeasurement.Centimeter;
    global using static Demo.UnitsOfMeasurement.Millimeter;
    global using static Demo.UnitsOfMeasurement.Kilometer;
    global using static Demo.UnitsOfMeasurement.Inch;
    global using static Demo.UnitsOfMeasurement.Foot;
    global using static Demo.UnitsOfMeasurement.Yard;
    global using static Demo.UnitsOfMeasurement.Mile;
    global using static Demo.UnitsOfMeasurement.Minute;
    global using static Demo.UnitsOfMeasurement.Hour;
    global using static Demo.UnitsOfMeasurement.Gram;
    global using static Demo.UnitsOfMeasurement.Tonne;
    global using static Demo.UnitsOfMeasurement.Pound;
    global using static Demo.UnitsOfMeasurement.Ounce;
    global using static Demo.UnitsOfMeasurement.DegCelsius;
    global using static Demo.UnitsOfMeasurement.DegRankine;
    global using static Demo.UnitsOfMeasurement.DegFahrenheit;
    global using static Demo.UnitsOfMeasurement.USD;
    global using static Demo.UnitsOfMeasurement.GBP;
    global using static Demo.UnitsOfMeasurement.PLN;
    global using static Demo.UnitsOfMeasurement.Radian;
    global using static Demo.UnitsOfMeasurement.Degree;
    global using static Demo.UnitsOfMeasurement.Grad;
    global using static Demo.UnitsOfMeasurement.Turn;
    global using static Demo.UnitsOfMeasurement.Hertz;
    global using static Demo.UnitsOfMeasurement.Radian_Sec;
    global using static Demo.UnitsOfMeasurement.Degree_Sec;
    global using static Demo.UnitsOfMeasurement.RPM;
    global using static Demo.UnitsOfMeasurement.Meter2;
    global using static Demo.UnitsOfMeasurement.Meter3;
    global using static Demo.UnitsOfMeasurement.Meter_Sec;
    global using static Demo.UnitsOfMeasurement.Centimeter_Sec;
    global using static Demo.UnitsOfMeasurement.Kilometer_Hour;
    global using static Demo.UnitsOfMeasurement.MPH;
    global using static Demo.UnitsOfMeasurement.Meter_Sec2;
    global using static Demo.UnitsOfMeasurement.Meter2_Sec2;
    global using static Demo.UnitsOfMeasurement.Newton;
    global using static Demo.UnitsOfMeasurement.PoundForce;
    global using static Demo.UnitsOfMeasurement.Poundal;
    global using static Demo.UnitsOfMeasurement.Dyne;
    global using static Demo.UnitsOfMeasurement.Joule;
    global using static Demo.UnitsOfMeasurement.Watt;
    global using static Demo.UnitsOfMeasurement.NewtonMeter;
    global using static Demo.UnitsOfMeasurement.Joule_Kelvin;
    global using static Demo.UnitsOfMeasurement.Joule_Kelvin_Kilogram;
    global using static Demo.UnitsOfMeasurement.Pascal;
    global using static Demo.UnitsOfMeasurement.Bar;
    global using static Demo.UnitsOfMeasurement.AtmTechnical;
    global using static Demo.UnitsOfMeasurement.AtmStandard;
    global using static Demo.UnitsOfMeasurement.MillimeterHg;
    global using static Demo.UnitsOfMeasurement.Coulomb;
    global using static Demo.UnitsOfMeasurement.Volt;
    global using static Demo.UnitsOfMeasurement.Ohm;
    global using static Demo.UnitsOfMeasurement.Siemens;
    global using static Demo.UnitsOfMeasurement.Farad;
    global using static Demo.UnitsOfMeasurement.Weber;
    global using static Demo.UnitsOfMeasurement.Math;
#else
    global using Meter = System.Double;
    global using Second = System.Double;
    global using Kilogram = System.Double;
    global using DegKelvin = System.Double;
    global using Ampere = System.Double;
    global using Mole = System.Double;
    global using Candela = System.Double;
    global using EUR = System.Decimal;
    global using Centimeter = System.Double;
    global using Millimeter = System.Double;
    global using Kilometer = System.Double;
    global using Inch = System.Double;
    global using Foot = System.Double;
    global using Yard = System.Double;
    global using Mile = System.Double;
    global using Minute = System.Double;
    global using Hour = System.Double;
    global using Gram = System.Double;
    global using Tonne = System.Double;
    global using Pound = System.Double;
    global using Ounce = System.Double;
    global using DegCelsius = System.Double;
    global using DegRankine = System.Double;
    global using DegFahrenheit = System.Double;
    global using USD = System.Decimal;
    global using GBP = System.Decimal;
    global using PLN = System.Decimal;
    global using Radian = System.Double;
    global using Degree = System.Double;
    global using Grad = System.Double;
    global using Turn = System.Double;
    global using Hertz = System.Double;
    global using Radian_Sec = System.Double;
    global using Degree_Sec = System.Double;
    global using RPM = System.Double;
    global using Meter2 = System.Double;
    global using Meter3 = System.Double;
    global using Meter_Sec = System.Double;
    global using Centimeter_Sec = System.Double;
    global using Kilometer_Hour = System.Double;
    global using MPH = System.Double;
    global using Meter_Sec2 = System.Double;
    global using Meter2_Sec2 = System.Double;
    global using Newton = System.Double;
    global using PoundForce = System.Double;
    global using Poundal = System.Double;
    global using Dyne = System.Double;
    global using Joule = System.Double;
    global using Watt = System.Double;
    global using NewtonMeter = System.Double;
    global using Joule_Kelvin = System.Double;
    global using Joule_Kelvin_Kilogram = System.Double;
    global using Pascal = System.Double;
    global using Bar = System.Double;
    global using AtmTechnical = System.Double;
    global using AtmStandard = System.Double;
    global using MillimeterHg = System.Double;
    global using Coulomb = System.Double;
    global using Volt = System.Double;
    global using Ohm = System.Double;
    global using Siemens = System.Double;
    global using Farad = System.Double;
    global using Weber = System.Double;
    global using Kelvin = System.Double;
    global using Celsius = System.Double;
    global using Rankine = System.Double;
    global using Fahrenheit = System.Double;
    global using static System.Math;
#endif
