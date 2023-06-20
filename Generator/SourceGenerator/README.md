# Unit Source Generator for C# (only)

## Description

The generator:
- loads definitions of units of measurement from a text file (using [Mangh.Metrology.Definitions](https://www.nuget.org/packages/Mangh.Metrology.Definitions)),
- translates these definitions into C# in-memory structs
(using [Mangh.Metrology.Model](https://www.nuget.org/packages/Mangh.Metrology.Model) and user-defined templates)
- and then passes them to the C# compiler (at compile-time).

## Installation

- The package does not need to be installed separately.
It is loaded (restored) automatically (along with the packages it depends on) when it is needed
i.e. when a project being built references it (directly or indirectly).
 That is the case of C# projects created from the [Mangh.Metrology.CSUnits](https://www.nuget.org/packages/Mangh.Metrology.CSUnits) project template.

## Replaces 

- The package replaces the
[Mangh.Metrology.SourceGeneratorXslt](https://www.nuget.org/packages/Mangh.Metrology.SourceGeneratorXslt)
package version 1.x.

  NOTE: the package (like its predecessor) is a replacement for a _T4 Text Template_ generator used in
[UnitsOfMeasurement](https://marketplace.visualstudio.com/items?itemName=MarekAniola.UnitsOfMeasurement-20210510)
C# projects for _.NET Framework_.
