# Quantities and Levels - Units and Scales
_Quantity_ is simply an amount (number) of a given _unit_ of measurement. Formally: a pair {number, unit}. For example 5 m (5 meters) is a quantity. 

5°C (5 degrees Celsius) can, however, be interpreted in two ways. It can be a _quantity_ e.g. as an increase in temperature of 5°C in a physical system. Just as well, it could be an initial temperature _level_ at which the physical process starts. The _level_ is understood here as a position relative to a reference point (zero level) on a given _scale_. In other words: a quantity measured relative to the zero level of the scale. Formally: a pair {quantity, scale-reference-level}.

Given the above duality, how can we convert 5°C into Kelvin? As a _quantity_ (increase) it would be converted to 5 K (increase of 5 degree Kelvin), but as a _level_ it would be converted to 278.15 K (corresponding level on Kelvin scale). It is not clear which is correct unless the temperature is clearly qualified as either _quantity_ or _level_.

<br/>

## Defining units and scales

For the above reason unit and scale types are designed as distinct (though interrelated). At definition stage they are defined separately under different names. A unit has to be specified first before it can be assigned to a scale (along with a suitable reference level). For example:
```
unit DegKelvin "K" "deg.K" = <Temperature>;
unit DegCelsius "\u00B0C" "deg.C" = DegKelvin;
unit DegRankine "\u00B0R" "deg.R" = (9/5) * DegKelvin;
unit DegFahrenheit "\u00B0F" "deg.F" = (9/5) * DegKelvin;

// The following scales take Absolute Zero temperature
// as the common reference level that is used when
// converting levels from one scale to another:
scale Kelvin AbsoluteZero = DegKelvin 0.0;
scale Celsius AbsoluteZero = DegCelsius -273.15;
scale Rankine AbsoluteZero = DegRankine 0.0;
scale Fahrenheit AbsoluteZero = DegFahrenheit -273.15 * (9 / 5) + 32;
```
Scale is built on top of its associated unit: the unit is embedded in the scale and provides the scale with dimension, conversion factor and symbols (tags). Offset to the reference level (specific to the scale) seems to be the only factor that makes it (structurally) different from unit. More important (functional) distinction lies in arithmetic which is different for units and scales (see below) and this is vital for dimensional analysis.

Note that all scales in the example above share the same `AbsoluteZero` identifier (as a name of the reference level that is common to all scales). This way the scales are combined into a [conversion family](./Families.md) i.e. subset of scales that can be converted to each other.

<br/>

## Quantities and levels as instances of units and scales

Unit instances (objects) play the role of _quantities_ (implement `IQuantity<T>` interface), whereas scale instances (objects) play the role of _levels_ (implement `ILevel<T>` interface), `T` = `double` |  `float` | `decimal`:
```C#
Celsius initial = (Celsius)5.0;             // initial temperature (level) at 5°C
DegCelsius delta = (DegCelsius)5.0;         // increase in temperature of 5°C (quantity)

// Output the above values, converted to Kelvin:
Console.Writeline(Kelvin.From(initial));    // output: 278.15 K
Console.Writeline(DegKelvin.From(delta));   // output: 5 K
```

<br/>

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

<br/>

## Annoying duality

_Quantity_-_Level_ duality has an annoying consequence: whenever you build some functionality for _levels_ (_scales_) then you may find quickly that it is needed for _quantities_ (_units_) too. In the end you build that functionality twice: one for _scales_ and its twin copy for _units_.

`QuantityParser` and `LevelParser` classes (see [Demo/UnitsOfMeasurement/Parsers](https://github.com/mangh/Metrology/tree/main/Demo/UnitsOfMeasurement/Parsers)) is a good example of such redundancy: each of the classes is almost an exact copy of the other one, except that `Scale<T>` replace `Unit<T>` and `ILevel<T>` replace `IQuantity<T>`. Yet both classes serve the same purpose i.e. to parse the same input text with the same collections of allowed units: after all, input text of `"100 deg.C"` looks the same and refers to the same units (e.g. DegKelvin.Family) regardless of its final interpretation as either a _level_ or a _quantity_.

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