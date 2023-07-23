# Units of Measurement for `dotnet`

## Product description

* _dotnet_ project templates for creating Units of Measurement C# or C++ library.

* _units_ and _scales_ are generated from a simple user definitions (in a text file)
translated into the target language structures using customizable XSLT templates.

* _units_ and _scales_ provide the same arithmetic and comparison operators as built-in numeric types:
 
  * `+`, `++`, `-`, `--`, `*`, `/`
  * `==`, `!=`, `<`, `<=`, `>`, `>=`

  so they can be used in expressions just like built-in types.
   
* all fundamental dimensions (and their combinations) are supported:
  - _Length_,
  - _Time_,
  - _Mass_,
  - _Temperature_,
  - _ElectricCurrent_,
  - _AmountOfSubstance_,
  - _LuminousIntensity_
  
  and in addition:
  - _Other_ (e.g. _Money_ for currency units),
   
* conversions to/from other (but compatible) unit types.

* algorithms can explicitly use _units_ and _scales_ (as types of  variables)
so that the syntax check performed at compile time is equivalent to performing _dimensional analysis_
(dimensional issues are reported as syntax errors, see: [example](Docs/DimAnalysisExample.md)).

Go to the [User Guide](Docs/UserGuide.md) for more information.


## Instructions for use

For instructions on creating and using unit of measurement projects, see:

* ["Using C# projects"](Docs/UsingCS.md) page,
* ["Using C++ projects"](Docs/UsingCPP.md) page.

<br/>

----