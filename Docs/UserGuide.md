# User Guide
Projects created from the [Mangh.Metrology.UnitsOfMeasurement](https://www.nuget.org/packages/Mangh.Metrology.UnitsOfMeasurement/) template provide you with [source components](./Components.md), i.e.:
* XSLT templates,
* C # source files

to define and manage unit of measurement types for C# applications. 

<br/>

## Build process

Basically what you have to do - after the project is created - is to specify the required units in the `Templates/definitions.txt` text file. When building the project, the source generator - invoked by the C# compiler - loads these definitions and generates the appropriate C# types according to the `Templates/*.xslt` templates:
* `unit.xslt` : template for a single unit _struct_,
* `scale.xslt` : template for a single scale _struct_,
* `catalog.xslt` : template for the `Catalog` _class_ i.e. the catalog of all defined unit and scale (proxy) types,
* `aliases.xslt` : template for the `Aliases.inc` file that can be used to import namespaces of defined entities into dependent projects,
* `report.xslt` : template for the `generator_report.txt` file giving a summary of generated units and scales.

The process is shown in the following diagram:

![Overview](./image/Overview.png)

The diagram does not show the two files provided with the project template which are initially empty (do nothing) i.e.:
* `Math.cs` : source file that can provide some extra math supporting your units e.g. _Sin(Radian)_, _Cos(Radian)_, etc.,
* `Directory.Build.targets` : .csproj extension that can be used to import the generated `Aliases.inc` file to dependent project(s).

<br/>

## Sample definitions

Suppose you have prepared the following `definitions.txt` file:
```
///////////////////////////////////////////////////////
//
//  Length
//
unit Meter "m" = <Length>; 
unit Kilometer "km" = Meter / 1000;
unit Inch "in" = 100 * Meter / 2.54;
unit Foot "ft" = Inch / 12;
unit Yard "yd" = Foot / 3;
unit Mile "mil" = Yard / 1760;

///////////////////////////////////////////////////////
//
//  Time
//
unit Second "s" = <Time>;
unit Minute "min" = Second / 60;
unit Hour "h" = Minute / 60;

///////////////////////////////////////////////////////
//
//  Velocity
//
unit Meter_Sec "m/s" = Meter / Second;
unit Kilometer_Hour "km/h" = Kilometer / Hour | Meter_Sec * (1/1000) / ((1/60)/60);
unit MPH "mph" "mi/h" = Mile / Hour | Meter_Sec * ((((100/2.54)/12)/3)/1760) / ((1/60)/60);

///////////////////////////////////////////////////////
//
//  Temperature
//
unit DegKelvin "K" "deg.K" = <Temperature>;
unit DegCelsius "\u00B0C" "deg.C" = DegKelvin;
unit DegFahrenheit "\u00B0F" "deg.F" = (9 / 5) * DegKelvin;

scale Kelvin AbsoluteZero = DegKelvin 0.0;
scale Celsius AbsoluteZero = DegCelsius -273.15;
scale Fahrenheit AbsoluteZero = DegFahrenheit -273.15 * (9 / 5) + 32;

///////////////////////////////////////////////////////
// 
//  Currency
//  Note: the rates (conversion factors) below
//  are to be updated on application startup. 
//  See Demo/Currencies application for an example. 
//
unit<decimal> EUR "EUR" = <Money>;          // Euro 
unit<decimal> USD "USD" = 1.3433 * EUR;     // US Dollar 
unit<decimal> GBP "GBP" = 0.79055 * EUR;    // British Pound 
unit<decimal> PLN "PLN" = 4.1437 * EUR;     // Polish Zloty 
```

A single _unit_ definition above, such as that for the `Kilometer_Hour`, specifies:

| property | value |
|:----------------------|:---------------------------------------------------------------------------------------------|
| unit type name        | `Kilometer\_Hour`                                                                            |
| unit symbol(s)        | `"km/h"`                                                                                     |
| underlying value type | `double` is the default, but you can explicitly specify `decimal` or `float` instead         |
| relationship to:      | `Kilometer` and `Hour` units, through the arithmetic expression: `Kilometer / Hour`,         |
| and relationship to:  | `Meter_Sec` unit; through the conversion expression: `Meter_Sec * (1/1000) / ((1/60)/60)`.   |

A single _scale_ definition above, such as that for the `Fahrenheit`, specifies:

| property | value |
|:----------------------|:---------------------------------------------------------------------------------------------|
| scale type name       | `Fahrenheit`                                                                                 |
| reference level name  | `AbsoluteZero`                                                                               |
| underlying unit type  | `DegFahrenheit`                                                                              |
| position (offset) of the reference level relative to the zero level of the scale | `-273.15 * (9 / 5) + 32`          |

For more information on definitions, see: [Definition syntax and semantics](./Definitions.md). You can also look at a [more extensive example of definitions](https://github.com/mangh/Metrology/blob/main/Demo/UnitsOfMeasurement/Templates/definitions.txt).


The library built from these definitions would provide the following units and scales (as C# _partial struct_), each asigned to the appropriate [conversion family](./Families.md):

| units / scales     | C# types                                              | family             |
|:-------------------|:------------------------------------------------------|:-------------------|
| length units       | `Meter`, `Kilometer`, `Inch`, `Foot`, `Yard`, `Mile` | `Meter.Family`     |
| time units         | `Second`, `Minute`, `Hour`                           | `Second.Family`    |
| velocity units     | `Meter_Sec`, `Kilometer_Hour`, `MPH`                 | `Meter_Sec.Family` |
| temperature units  | `DegKelvin`, `DegCelsius`, `DegFahrenheit`           | `DegKelvin.Family` |
| temperature scales | `Kelvin`, `Celsius`, `Fahrenheit`                    | `Kelvin.Family`    |
| currency units     | `EUR`, `USD`, `GBP`, `PLN`                           | `EUR.Family`       |

That's it (basically). Now you can use the generated library in your application.

<br/>

## Basic Usage

> NOTE: throughout the documentation, I use the term _"quantity"_ for a variable of a _unit_ type and the term _"level"_ for a variable of a _scale_ type. See ["Quantities and Levels - Units and Scales"](./Quantities-vs-Levels.md) to find out more about that distinction.

Each unit implements arithmetic and comparison operators:
  * `+`, `++`, `-`, `--`, `*`, `/`
  * `==`, `!=`, `<`, `<=`, `>`, `>=`

just like primitive numeric types (e.g. `double`). Therefore, for _quantities_ in the SAME UNIT, you can always do the following:
```C#
// declare two quantities in the same unit:
Yard length1 = (Yard)123.0;             // 123 yd
Yard length2 = (Yard)321.0;             // 321 yd

Yard sum = length1 + length2;           // add: 123 yd + 321 yd = 444 yd
Yard dif = length2 - length1;           // subtract: 321 yd - 123 yd = 198 yd
Yard neg = -length2;                    // negation: -(321 yd) = -321 yd
Yard mul = 2.0 * length1;               // multiply or divide by a number: 2.0 * 123 yd = 246 yd
double ratio = length2 / length1;       // divide giving a ratio: 321 yd / 123 yd = 2,6097560975609757
Yard incr = ++length1;                  // pre- & post-increment: ++(123 yd) = 124 yd
Yard decr = length1--;                  // pre- & post-decrement: (124 yd)-- = 124 yd

bool isLessThan = length1 < length2;    // comparison: 123 yd < 321 yd = True
```

However, you cannot perform:
```C#
var area = length1 * length2; // ERROR: "Operator '*' cannot be applied to operands of type 'Yard' and 'Yard'
```
unless you define such an operation in the `definitions.txt` file, e.g.:
```
unit SquareYard "sq yd" = Yard * Yard;
```

Likewise, _quantities_ in DIFFERENT UNITS can only be used in an arithmetic expression if that expression is allowed in the `definitions.txt` file:
```C#
// two quantities in different units:
Meter distance = (Meter)306.0;  // 306 m
Second duration = (Second)8.5;  // 8,5 s

// the definitions used, namely:
//    unit Meter_Sec "m/s" = Meter / Second;
// allow you to divide Meters by Seconds giving the speed in Meter/Sec:
Meter_Sec speed = distance / duration;  // 36 m/s
```
_Quantities_ in DIFFERENT UNITS cannot be compared: one of them has to be first converted to the unit of the other before you compare.

Conversions must also be declared explicitly in the `definitions.txt` file:
```C#
// the definitions used allow you to make the following conversions:
Kilometer_Hour kmph = Kilometer_Hour.From(speed);
MPH mph = MPH.From(speed);

WriteLine($"{distance} / {duration} = {speed} ({kmph}; {mph})");
// 306 m / 8,5 s = 36 m/s (129,6 km/h; 80,52970651395849 mph)

// any dimensional conversion methods has its dimensionless counterpart:
double kmph_ = Kilometer_Hour.FromMeter_Sec(speed.Value);
double mph_ = MPH.FromMeter_Sec(speed.Value);

WriteLine($"{distance.Value} / {duration.Value} = {speed.Value} ({kmph_}; {mph_})");
// 306 / 8,5 = 36 (129,6; 80,52970651395849)
```

<br/>

## Interfaces, Proxies, Parsers and the Catalog

All of the previous examples refer to quantities in units known at design-time, i.e. refer explicitly to unit names. 
Sometimes, however, it is necessary to handle quantities/levels in units that we cannot name directly, for example: handling a "length" variable that uses any length unit allowed in the application. 

Interfaces:
* `IQuantity<T>` for quantities,
* `ILevel<T>` for levels

and _proxy_ classes (to handle unit/scale TYPES, without using Reflection):
* `Unit<T>` proxy,
* `Scale<T>` proxy

where:
* `T` = `double` | `float` | `decimal`

are designed for such cases: they allow to process quantities/levels, regardless of the units/scales they use.

This approach has been used by the `QuantityParser<T>` and `LevelParser<T>` classes:
* designed to parse quantities/levels given in text form as a number with a unit symbol, e.g.: `"1.093613 yd"`,
* an instance of a parser is initialized - in the constructor - with an `IEnumerable<Unit<T>>` / `IEnumerable<Scale<T>>` collection of units/scales allowed in the input text:
  ```C#
  public QuantityParser(IEnumerable<Unit<T>> allowedUnits)
  public LevelParser(IEnumerable<Scale<T>> allowedScales)
  ```
* the `TryParse` method provides a result of type `IQuantity<T>` / `ILevel<T>` when it finds a valid number with a unit symbol in the input text:
  ```C#
  public bool TryParse(string input, out IQuantity<T>? result)
  public bool TryParse(string input, out ILevel<T>? result)
  ```

Now - going back to the example at the top of the section - if you want the user to be able to enter the length in any of the defined length units, you can do so as follows:
```C#
// allowed units are from the Meter.Family (Meter, Kilometer, Inch, Foot, Yard, Mile):
QuantityParser<double> parser = new(Meter.Family);

Write($"Enter the length (in unit: \"{string.Join("\", \"", parser.Units.SelectMany(m => m.Symbol))}\"): ");
string? input = ReadLine();

if ((input is not null) && parser.TryParse(input, out IQuantity<double>? length))
{
    // Note: here, you can either stay with the received "IQuantity" value,
    // or convert it to an explicit unit (preferred for some reason), or both:
    WriteLine($"Length = {length} ({Meter.From(length!)})");
}
else
{
    WriteLine($"{input ?? "null"}: invalid input; bad number or unit of measurement.");
}

// Sample output for a unit that has been defined (Yard):
//   Enter the length (in unit: "m", "km", "in", "ft", "yd", "mil"): 10 yd
//   Length = 10 yd (9,144 m)

// Sample output for a unit that has NOT been defined (Centimeter):
//   Enter the length (in unit: "m", "km", "in", "ft", "yd", "mil"): 10 cm
//   10 cm: invalid input; bad number or unit of measurement.
```
Note that `QuantityParser<double>` in the example above uses `Meter.Family` in the constructor instead of the collection `IEnumerable<Unit<double>>`: this is because it can query the `Catalog` for the required family and get the collection in return.

The `Catalog` is a collection of `Unit<T>` and `Scale<T>` proxies for all units and scales available at build time. You can use the following methods to query the `Catalog` for:

| Catalog method:                                               | To query for:                                   |
|---------------------------------------------------------------|-------------------------------------------------|
| ` Unit<T>? Unit<T>(string symbol)`                            | a unit with the selected symbol                 |
| `IEnumerable<Unit<T>> Units<T>(int family)`                   | units of the selected family                    |
| `IEnumerable<Unit<T>> Units<T>(Dimension sense)`              | units of the selected dimension                 |
| `IEnumerable<Unit<T>> Units<T>(IEnumerable<Scale<T>> scales)` | units underlying the selected scales            |
|||
| `Scale<T>? Scale<T>(int family, string symbol)`               | a scale for the selected family and unit symbol |
| `Scale<T>? Scale<T>(int family, Unit<T> unit)`                | a scale for the selected family and unit        |
| `IEnumerable<Scale<T>> Scales<T>(int family)`                 | scales of the selected family                   |
| `IEnumerable<Scale<T>> Scales<T>(Dimension sense)`            | scales of the selected dimension                |
|||

The previous example can now be modified as follows:
```C#
// Select a target unit the result is to be converted into:
Unit<double> targetUnit = Catalog.Unit<double>("in")!;  // == Inch.Proxy

// input units restricted to the family of the target unit:
QuantityParser<double> parser = new(targetUnit.Family);

Write($"Enter the length (in unit: \"{string.Join("\", \"", parser.Units.SelectMany(m => m.Symbol))}\"): ");
string? input = ReadLine();

if ((input is not null) && parser.TryParse(input, out IQuantity<double>? length))
{
    WriteLine($"Length = {length} ({targetUnit.From(length!)}; {Meter.From(length!)})");
}
else
{
    WriteLine($"{input ?? "null"}: invalid input; bad number or unit of measurement.");
}

// Sample output:
//   Enter the length (in unit: "m", "km", "in", "ft", "yd", "mil"): 10 yd
//   Length = 10 yd (360 in; 9,144 m)
```

<br/>

## RuntimeLoader

You can use this functionality to extend the `Catalog` with "late" units/scales  at runtime; "late" means units/scales that have not been included at compile time.

Late units/scales are not known to the compile time units/scales but can refer to them, which is perfectly sufficient to make any conversions, including conversions to/from the compile-time units/scales.

Sample usage with the following definitions in the (example) `LateUnits.txt` file:
```
// NOTE: the definitions below refer to compile-time units (Foot and Meter):
unit Fathom "ftm" = Foot / 6;
unit Cable "cb" = Meter / 185.2;
unit NauticalMile "nmi" = Meter / 1852;
```

```C#
RuntimeLoader ldr = new();
if (!ldr.LoadFromFile("LateUnits.txt"))
{
    WriteLine("Invalid definitions:");
    ldr.Errors.ForEach(e => WriteLine(e));
    return;
}

// Input units limited to the family of the new unit (Fathom)
// i.e. again to the Meter.Family, just extended with new units:
Unit<double> targetUnit = Catalog.Unit<double>("ftm")!;

QuantityParser<double> parser = new(targetUnit.Family);

Write($"Enter the length (in unit: \"{string.Join("\", \"", parser.Units.SelectMany(m => m.Symbol))}\"): ");
string? input = ReadLine();

if ((input is not null) && parser.TryParse(input, out IQuantity<double>? length))
{
    WriteLine($"Length = {length} ({targetUnit.From(length!)}; {Meter.From(length!)})");
}
else
{
    WriteLine($"{input ?? "null"}: invalid input; bad number or unit of measurement.");
}

// Sample output:
//   Enter the length (in unit: "m", "km", "in", "ft", "yd", "mil", "ftm", "cb", "nmi"): 10 yd
//   Length = 10 yd (5 ftm; 9,144 m)
```

See [Demo Applications](https://github.com/mangh/unitsofmeasurement/tree/master/Demo) and [Demo Unit Tests](https://github.com/mangh/unitsofmeasurement/tree/master/Demo/UnitsOfMeasurement.Test) for more examples.

<br/>

## Dimensional Analysis vs. Performance
Algorithms formulated in units of measure are slower than their counterparts using "plain" numbers only.
Performance test results can be seen in the comments pasted at the end of the source code of the sample benchmarks.
Under .NET 6.0, the results are:
* [Bullet Application Benchmark](https://github.com/mangh/Metrology/blob/main/Demo/Benchmark2/Program.cs) - approx. 5.45-fold increase in execution time (or 5.45-fold drop in performance),
* [CALINE3 Application Benchmark](https://github.com/mangh/CALINE3.CS/tree/main/Benchmark) - approx. 1.25-fold increase in execution time (or 1.24-fold drop in performance; 1.24 &#8776; 2.701 ms / 2.177 ms),

and under .NET 7.0:
* [Bullet Application Benchmark](https://github.com/mangh/Metrology/blob/main/Demo/Benchmark2/Program.cs) - approx. 2,38-fold increase in execution time (or 2,38-fold drop in performance),
* [CALINE3 Application Benchmark](https://github.com/mangh/CALINE3.CS/tree/main/Benchmark) - approx. 1.12-fold increase in execution time (or 1.12-fold drop in performance; 1.12 &#8776; 1.244 ms / 1.109 ms ms),

These benchmark show how units of measurement can affect performance. 

The first focuses on a single, somewhat artificial method ([Bullet.Measured.Calculator.CalculateRange](https://github.com/mangh/Metrology/blob/main/Demo/Bullet/Bullet.Measured.cs)) that is formulated entirely in units of measurement - there is not a single expression that is free from units. As can be seen, such characteristics translates into a large drop in performance: 5.45 (under .NET 6.0) and 2.38 (under .NET 7.0).

The second ([CALINE3](https://github.com/mangh/CALINE3.CS/tree/main/CALINE3)) is closer to real-world applications but - interestingly - is not as restrained in using units, as a much better result (1.24/1.12) would suggest. In this case, the algorithm is spread over several cooperating classes that have to perform a number of standard operations (such as creating objects, handling collections, etc.) being integral to the algorithm and contributing to the overall execution time, regardless of whether the units are used or not. It does not make much sense to consider only the operations referring to units, in isolation from this context. As you can see in the profiler screenshot below, these operations make only a small contribution (cca. 6%) to the overall execution time and this is what accounts for a much better benchmark result, even when they are 5x slower:

![profiling](image/Profiling.png)

<br/>

As you may have noticed in the source code of previous examples, there is a way to completely eliminate the performance degradation without major changes to the source code - albeit at the cost of not being able to detect dimensional inconsistencies.

The key idea of this solution is to force the compiler to replace - on the fly - all references to unit structures with references to the corresponding primitive number types, so that, for example, the following excerpt:
```C#
Meter distance = (Meter)306.0;          // 306 m
Second duration = (Second)8.5;          // 8,5 s
Meter_Sec speed = distance / duration;  // 36 m/s
```
would be compiled as:
```C#
double distance = (double)306.0;        // 306
double duration = (double)8.5;          // 8,5
double speed = distance / duration;     // 36
```
This is achieved by:
* `DIMENSIONAL_ANALYSIS` symbol - to be used in dependent projects to control conditional compilation,
* `./Templates/Aliases.inc` file - built automatically at compile-time to provide unit structures or a map to the corresponding primitive number types, for example:
  ```C#
  // Note: the "global using" directives require C# 10
  #if DIMENSIONAL_ANALYSIS
      global using Demo.UnitsOfMeasurement;
      global using static Demo.UnitsOfMeasurement.Math;
  #else
      global using Meter = System.Double;
      global using Centimeter = System.Double;
      // ...
      global using Radian = System.Double;
      global using Degree = System.Double;
      // ...
      global using static System.Math;
  #endif
  ```

* `./Directory.Build.targets` file - to export/copy `Aliases.inc` to dependend project(s), for example:
  ```XML
  <Project>
    <ItemGroup>
      <SourceAliases Include="Templates\Aliases.inc"/>
      <TargetAliases Include="..\Bullet\Aliases.cs"/>
    </ItemGroup>
    <Target Name="CopyAliasesToDependentProjects" AfterTargets="AfterBuild" Inputs="@(SourceAliases)" Outputs="@(TargetAliases)">
      <Copy SourceFiles="@(SourceAliases)" DestinationFiles="@(TargetAliases)" SkipUnchangedFiles="true" />
    </Target>
  </Project>
  ```

* `./Math.cs` file - to enable uniform use of `System.Math` methods, for example:
  ```C#
  // Math.cs
  namespace Demo.UnitsOfMeasurement
  {
      public static class Math
      {
          // the following Sin() definition allows the use of an expression:
          //     Sin(angle)
          // regardless of the type used for the angle: Radian or double
          public static double Sin(Radian angle) => System.Math.Sin(angle.Value);
          // ...
      }
  }
  ```

* conversions - as an exception - require special treatment, directly in the source code, to provide the appropriate (dimensional or dimensionless) version:
  ```C#
  #if DIMENSIONAL_ANALYSIS
    Radian angle = Radian.From(slope);
  #else
    Radian angle = Demo.UnitsOfMeasurement.Radian.FromDegree(slope);
  #endif
  ```

When all these components are ready, just define --or-- remove the `DIMENSIONAL_ANALYSIS` symbol in the dependent project to compile it in the appropriate mode: *with units and reduced performance* --or-- *without units and maximum performance*.

<br/>

## Customizing
In addition to modifying the `definitions.txt` file - in which you specify units and scales - you can also:
* modify XSLT templates to change the structure and/or functionality of (all) generated units/scales,
* create extensions to the generated _partial structs_ for an additional functionality that is required only for selected units/scales and cannot be put in XSLT templates as that would make unwanted changes in all other units/scales. 
  
  For example, you can create the following extensions for the units `Ohm` and `Siemens` to include the relationship `Ohm = 1/Siemens` that applies for these two units only:
  ```C#
  // OhmExt.cs (extension for Ohm.cs)
  public partial struct Ohm { public Siemens Conductance => new(1.0 / Value); }
  ```
  and
  ```C#
  // SiemensExt.cs (extension for Siemens.cs)
  public partial struct Siemens { public Ohm Resistance => new(1.0 / Value); }
  ```
* modify the source code provided with the project template.

<br/>

----