# Quantities and Units. Levels and Scales

_Quantity_ is simply an amount (number) of a given _unit_ of measurement. Formally: a pair {number, unit}. For example `5 m` (5 meters) is a _quantity_. 

`5°C` (5 degrees Celsius) can, however, be interpreted in two ways.
It can be a _quantity_ e.g. as an increase in temperature of 5°C in a physical system.
Just as well, it could be an initial temperature _level_ at which the physical process starts.
_Level_ is understood here as a _quantity_ (position) measured relative to a zero level on a given _scale_.
Formally: a pair {quantity, scale} - zero level is fixed for the scale as its intrinsic property.

Given the above duality, how can we convert `5°C` into Kelvin? As a _quantity_ (increase) it would be converted to `5 K` (an increase of 5 degree Kelvin), but as a _level_ it would be converted to `278.15 K` (the corresponding level on the Kelvin scale). It is not clear which is correct until the temperature is explicitly qualified as a _quantity_ or as a _level_.


## Defining units and scales

For the above reason unit and scale types are designed as distinct (though interrelated). At definition stage they are defined separately under different names. A unit has to be specified first before it can be assigned to a scale (along with a suitable reference level). For example:

```JS
unit DegKelvin "K" "deg.K" = <Temperature>;
unit DegCelsius "\u00B0C" "deg.C" = DegKelvin;
unit DegRankine "\u00B0R" "deg.R" = (9/5) * DegKelvin;
unit DegFahrenheit "\u00B0F" "deg.F" = (9/5) * DegKelvin;

// The following scales take AbsoluteZero temperature
// as a common reference level that is used when
// converting levels from one scale to another:
scale Kelvin AbsoluteZero = DegKelvin 0.0;
scale Celsius AbsoluteZero = DegCelsius -273.15;
scale Rankine AbsoluteZero = DegRankine 0.0;
scale Fahrenheit AbsoluteZero = DegFahrenheit -273.15 * (9 / 5) + 32;
```
Scale is built on top of its associated unit: the unit is embedded in the scale and provides the scale with dimension, conversion factor and symbols (tags). Offset to the reference level (specific to the scale) seems to be the only factor that makes it (structurally) different from unit. More important (functional) distinction lies in arithmetic which is different for units and scales (see below) and this is vital for dimensional analysis.

Note that all scales in the example above share the same `AbsoluteZero` identifier (as a name of the reference level that is common to all scales). This way the scales are combined into a [conversion family](./Families.md) i.e. subset of scales that can be converted to each other.


## Quantities are instances of units, levels are instances of scales

I use the term "_quantity_" for any variable (instance) of a "_unit type_" and the term "_level_" for any variable (instance) of a "_scale type_".

Here is an example of how this distinction (_quantity_ vs. _level_) works for `5°C`:


```C#
// C#
Celsius initial = (Celsius)5.0;      // initial temperature (level) at 5°C
DegCelsius delta = (DegCelsius)5.0;  // increase in temperature of 5°C (quantity)

// Output the above values, converted to Kelvin:
Writeline(Kelvin.From(initial));     // output: 278.15 K
Writeline(DegKelvin.From(delta));    // output: 5 K
```

```C++
// C++
Celsius initial{ 5.0 };              // initial temperature (level) at 5°C
DegCelsius delta{ 5.0 };             // increase in temperature of 5°C (quantity)

// Output the above values, converted to Kelvin:
cout << Kelvin{initial} << endl;     // output: 278.15 K
cout << DegKelvin{delta} << endl;    // output: 5 K
```
NOTE:  
In C# (exclusively) all _unit types_ implement the `IQuantity<T>` interface,
and all _scale types_ implement the `ILevel<T>` interface (`T` = `double` | `float` | `decimal`).
These interfaces allow _quantities_ (_levels_, respectively) to be handled in a uniform way, even if they are in different units.


## Arithmetic of quantities and levels

Arithmetic of levels is different from that of quantities: you cannot add, multiply or divide levels as you can do it with quantities. You can only subtract levels (giving quantity) and add/subtract quantity to/from level (giving level). It is shown in the following table:

operator | quantity arithmetic                        | level arithmetic             |
:-------:| ------------------------------------------ | ---------------------------- |
`+`      | `quantity + quantity -> quantity`          | `level + quantity -> level`  |
&nbsp;   | &nbsp;                                     | `quantity + level -> level`  |
`-`      | `quantity - quantity -> quantity`          | `level - level -> quantity`  |
&nbsp;   | &nbsp;                                     | `level - quantity -> level`  | 
`++`     | `++quantity -> quantity`                   | `++level -> level`           |
&nbsp;   | `quantity++ -> quantity`                   | `level++ -> level`           |
`--`     | `--quantity -> quantity`                   | `--level -> level`           |
&nbsp;   | `quantity-- -> quantity`                   | `level-- -> level`           |
`*`      | `quantity * number -> quantity`            | NOT AVAILABLE                |
&nbsp;   | `number * quantity -> quantity`            | NOT AVAILABLE                |
&nbsp;   | `quantity[a] * quantity[b] -> quantity[c]` | NOT AVAILABLE                |
`/`      | `quantity / number -> quantity`            | NOT AVAILABLE                |
&nbsp;   | `number / quantity[a] -> quantity[b]`      | NOT AVAILABLE                |
&nbsp;   | `quantity[a] / quantity[b] -> quantity[c]` | NOT AVAILABLE                |

where: `quantity[a]` is the quantity in (some) unit `a`.

Continuing previous example:
```C#
Celsius final = initial + delta; // final level at 10°C (level = level + quantity)
delta = final - initial;         // change of 5°C (quantity = level - level)
initial = final – delta;         // back to initial level at 5°C (level = level - quantity)
```


## Annoying duality

_Quantity_-_Level_ duality has an annoying consequence: whenever you build some functionality for _levels_ (_scales_) then you may find quickly that it is needed for _quantities_ (_units_) too. In the end you build that functionality twice: one for _scales_ and its twin copy for _units_.

### C# only (no equivalent in C++)

`QuantityParser` and `LevelParser` classes (see [Demo/UnitsOfMeasurement/Parsers](https://github.com/mangh/Metrology/tree/main/Demo/UnitsOfMeasurement/Parsers)) are a good example of such redundancy: each of the classes is almost an exact copy of the other one, except that `Scale<T>` replace `Unit<T>` and `ILevel<T>` replace `IQuantity<T>`. Both classes serve the same purpose i.e. parsing input text, checked by a set of allowed units. They differ only in the returned value (`ILevel<T>` or `IQuantity<T>`). To put it another way: the input text (e.g. "`100 deg.C`") looks the same, is processed the same and checked by the same units (e.g. DegKelvin.Family) - the only difference is the interpretation of the result (as a _level_ or as a _quantity_). This raises the question of whether we really need both classes.

To mitigate the problem you can use _scale_ conversion methods that accept `IQuantity<>` as an input parameter:

```C#
// Fahrenheit scale (for example)
public static Fahrenheit From(IQuantity<double> q)
```
and

```C#
// Scale<T> proxy (any)
public abstract ILevel<T> From(IQuantity<T> quantity);
```

They are to ease interpreting _quantity_ as a _level_ (or attaching _quantity_ to a _scale_) and defer issues resulting from distinguishing _levels_ and _quantities_ to a moment when the distinction is really essential. 

Here is an example of how this could be used to get rid of the (redundant?) `LevelParser<T>` in favor of `QuantityParser<T>`:
```C#
ILevel<double>? GetTemperatureLevel(string input)
{
    // Scales from the Kelvin.Family:
    IEnumerable<Scale<double>> allowedScales = Catalog.Scales<double>(Kelvin.Family);

    // QuantityParser can accept scales as input parameter to get the required units:
    QuantityParser<double> parser = new(allowedScales); 
    if (parser.TryParse(input, out IQuantity<double>? temperature))
    {
        // To return ILevel appropriate for the input IQuantity do the following:
        // 1. find the scale that correponds to the unit found in the input string:
        Scale<double>? scale = Catalog.Scale(Kelvin.Family, temperature!.Unit);
        // 2. attach the input quantity to the found scale:
        return /*ILevel<double>*/ scale!.From(temperature);
    }
    return null;
}
```

<br/>

----