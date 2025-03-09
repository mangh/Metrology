## Release 2.0.3 (03/09/2025)

### No functional changes, but:

* `.NET 9` is the target platform (all projects and project templates target `.NET 9`).
* `Mangh.Metrology.SourceGenerator` implements the `IIncrementalGenerator` interface (instead of the `ISourceGenerator`, which has been deprecated).

## Release 2.0.2 (05/06/2024)

### Fixed "(expr)" encoding bug

* Some C++ units could be generated with an invalid conversion factor, detected as an invalid expression during compilation.
For example, for a Degree unit defined as:
  ```txt
  unit Degree "deg" : "%f%s" = (180 * Radian) / "Math.PI";
  ```
  the conversion factor was encoded as:
  ```c++
  static constexpr T factor{ ((180.0 * Radian::factor) / 3.141592653589793238462643383279502884L) };
  ```
  whereas it should be:
  ```c++
  static constexpr T factor{ ((180.0 * 1.0) / 3.141592653589793238462643383279502884L) };
  ```

* C# units were free of this bug.

## Release 2.0.1 (03/04/2024)

### .NET 8 as a target framework

* No functional changes, but now all projects target .NET 8 (to benefit from .NET 8 performance gain).

## Release 2.0 (07/21/2023)

### Redesigned solution

<!-- The solution has been redesigned: -->
* Unit definitions have not changed (old definitions will work). :white_check_mark:
* The layout of the source folders has been changed. :boom:
* Some classes have been rewritten from scratch. :boom:
* New classes have been introduced. :boom:
* XSLT templates have been adapted to the changes. :boom:

### C++ Support (new)

As part of the new solution, you get:
* [UnitGenerator](https://www.nuget.org/packages/Mangh.Metrology.UnitGenerator) _dotnet tool_,
which can generate C++ (as well as C#) units from console,
* [CPPUnits](https://www.nuget.org/packages/Mangh.Metrology.CPPUnits) package,
which provides a [C++ Project](Docs/ProjectCPP.md) template
(see also: [Using C++ projects](Docs/UsingCPP.md)).


### NuGet packages renamed

* Name changes:

  | old (release 1.x)| new (release 2.x) |
  |-----|-----|
  | Mangh.Metrology.DefinitionParser    | Mangh.Metrology.Definitions |
  | Mangh.Metrology.SourceGeneratorXslt | Mangh.Metrology.SourceGenerator |
  | Mangh.Metrology.UnitsOfMeasurement  | Mangh.Metrology.CSUnits |
  | Mangh.Metrology.UnitTranslatorXslt  | Mangh.Metrology.Model |
  | ~~XsltGeneratorApp~~ | Mangh.Metrology.UnitGenerator<sup>[1]</sup> |
  | _n/a_ | Mangh.Metrology.CPPUnits |

  <sup>[1]</sup> Mangh.Metrology.UnitGenerator is a _dotnet tool_ that
  replaces the `XsltGeneratorApp` _console application_ available in the source code of the previous version.


### Special string literals used in definitions

* The definition parser again recognizes the literals `"Math.PI"` and `"Math.E"`
(as well as `"System.Math.PI"` and `"System.Math.E"`).
Now the literals can be used regardless of the numeric type underlying the unit (and not just for `unit<double>` definitions).


## Release 1.0.4 (12/27/2021)

### Translator and Template: unit factor bug fix

* Unit factor is a get/set PROPERTY for monetary units ONLY,
  for other units that is a CONST FIELD.

## Release 1.0.3 (12/22/2021)

### Selected string literals are recognized

* Parser recognizes `"System.Math.PI"` and `"System.Math.E"` literals to validate
 factors in `"unit<double>"` and `"scale<double>"` definitions that make use of
 these literals. (Other literals or in other definition types are still taken as
 numbers of unknown value - factor validation cannot be made).

### XML structures timestamped

* Translator generates a timestamp attribute `"tm"` in the root node of XML structures.
  This can be used, via XSLT, to generate timestamped .cs files.
  The feature is by default commented out in XSLT as it makes `git` annoyingly overactive.

### CancellationToken

* `Parser`, `Translator` and `SourceGenerator` polling for cancellation request
  (via `CancellationToken`) to end operations as soon as possible when it happens.
