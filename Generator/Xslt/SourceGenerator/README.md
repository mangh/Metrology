# Metrology. Generator for Units of Measurement (in C#)
The generator:
- reads (via `DefinitionParser`) definitions of units of measurement from a text file,
- translates that definitions (with `UnitTranslator` and user-defined XSLT-templates) into C# in-memory structs,
- relays those structs to C# compiler (at compile-time).

NOTE: the package is a .NET Standard replacement for a T4 Text Template-based generator used in [UnitsOfMeasurement](https://marketplace.visualstudio.com/items?itemName=MarekAniola.UnitsOfMeasurement-20210510) projects for .NET Framework C# class libraries.