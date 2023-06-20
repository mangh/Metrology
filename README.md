# Units of Measurement for `dotnet` _(under reconstruction)_
## Product description

* _dotnet_ project template for creating Units of Measurement C# class library.

* _units_ and _scales_ are generated from a user definitions in a text file, via customizable XSLT templates.

* _units_ and _scales_ are generated as types (_partial structs_) that implement arithmetic and comparison operators as if they were primitive numeric types:
  * `+`, `++`, `-`, `--`, `*`, `/`
  * `==`, `!=`, `<`, `<=`, `>`, `>=`
   
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
   
* conversions of _quantities_ to/from other (but compatible) unit types.

* entire algorithms can be formulated in units of measurement, so that _dimensional analysis_ can be performed as a syntax check at compile time (dimensional issues are displayed in _Visual Studio_ as syntax errors, see: [example](Docs/DimAnalysisExample.md)).

Go to the [User Guide](Docs/UserGuide.md) for more information.

<br/>

## How to use it?
Assuming you have already installed the ["Project template for Units of Measurement C# class library"](https://www.nuget.org/packages/Mangh.Metrology.UnitsOfMeasurement/) NuGet package:
```powershell
dotnet new --install "Mangh.Metrology.UnitsOfMeasurement"
```
follow this general process to create a library:

1. Create `unitsofmeasurement` project in a `PROJECT_FOLDER` and a `PROJECT_NAMESPACE`:
    ```powershell
    md PROJECT_FOLDER
    cd PROJECT_FOLDER
    dotnet new unitsofmeasurement [-n PROJECT_NAMESPACE]
    ```
2. (*optional*) To continue under Visual Studio IDE, attach the project to a [Visual Studio solution](https://docs.microsoft.com/en-us/visualstudio/get-started/tutorial-projects-solutions?view=vs-2022):
   - open Visual Studio IDE,
   - create a new or select an existing solution,
   - right click the solution root folder (or a solution subfolder),
   - perform: `Add` -> `Existing Project...`,
   - select the project created above.

3. Go to the `./Templates` folder:
    - edit `definitions.txt` to specify units of measurement for your solution(s), 
    - (*optional*) customize XSLT templates:
      - `unit.xslt`: template for a single unit (_struct_),
      - `scale.xslt`: template for a single scale (_struct_),
      - `catalog.xslt`: template for _Catalog class_ (catalog of all defined units and scales),
      - `aliases.xslt`: template for `Aliases.inc` file that might be used to import defined unit and scale types to dependent projects,
      - `report.xslt`: template for `generator_report.txt` file (a summary of generated units and scales).
  
4. (*optional*) Update `Directory.Build.targets` file (initially empty) if you want to import generated `Aliases.inc` file to a dependent project(s). See: [Demo/UnitsOfMeasurement](https://github.com/mangh/Metrology/tree/main/Demo/UnitsOfMeasurement) or [CALINE3/UnitsOfMeasurement](https://github.com/mangh/Metrology/tree/main/CALINE3/UnitsOfMeasurement) projects for a sample usage. You can safely delete the file if you are not going to use it.
5. (*optional*) Update `Math.cs` source file (initially empty) if you need some extra math to support your units. See: [Demo/UnitsOfMeasurement](https://github.com/mangh/Metrology/tree/main/Demo/UnitsOfMeasurement) or [CALINE3/UnitsOfMeasurement](https://github.com/mangh/Metrology/tree/main/CALINE3/UnitsOfMeasurement) projects for samples. You can safely delete the file if you are not going to use it.
6. Compile the project, e.g.:
    ```powershell
    dotnet build
    ```
&#128073; The project created in step 1 can be modified to use an external application  _at design time_ instead of the Source Generator _at compile time_ to generate units of measurement - see [READMETOO](READMETOO.md) for instruction.

<br/>

----