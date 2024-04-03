# Unit Generator for C# and C++

## Description

- Standalone [dotnet tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) for generating unit of measurement structures for C# or C++ languages from the console.

## Installation

* Install the tool as a _"global tool"_:
  ```sh
  dotnet tool install --global Mangh.Metrology.UnitGenerator
  ```
  
  The tool is user-specific, not global to the machine (as you might think).
  It is only available to the user that installed the tool.

  NOTE: You can also install the tool as a _"global tool in a custom location"_ or as a _"local tool"_.
  If you do it, please amend the __Usage__ instruction (below) accordingly.
  For details, see article ["How to manage .NET tools"](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools).
  Don't forget to amend your scripts (e.g. CMakeLists.txt files) that make use of the tool.

## Usage

* Use the command line with the following syntax to run the tool:
  ```sh
  UnitGenerator targetLanguage targetNamespace templateFolder targetFolder [--with-models]
  ```
  where:

  ```txt
  targetLanguage  : CS | CPP.
  targetNamespace : namespace string, e.g.
                   "Demo.Metrology" for CS language,
                   "Demo::Metrology" for CPP language,
  templateFolder  : path to the folder containing definitions and templates,
  targetFolder    : path to the target folder for generated units and scales,
  --with-models   : request to save models in files (in the targetFolder)
  ```

* Example use for C#:
  ```cmd
  UnitGenerator CS "Demo.UnitsOfMeasurement" .\UnitsOfMeasurement\Templates .\UnitsOfMeasurement\Units
  ```

* Example use for C++:
  ```cmd
  UnitGenerator CPP "CALINE3::Metrology" Metrology/Templates Metrology/Units
  ```

* The tool requires the following templates in the `templateFolder` directory:

  C#              |  C++                | purpose
  --------------- | ------------------- | -----------------------------
  Definitions.txt | definitions.txt     | _units and scales definitions_
  Unit.xslt       | unit.xslt           | _unit template_
  Scale.xslt      | scale.xslt          | _scale template_
  `n/a`           | math-constants.xslt | _math constants_
  `n/a`           | replace-string.xslt | _replace function_
  Catalog.xslt    | `n/a`               | _catalog template_
  Aliases.xslt    | `n/a`               | _aliases template_
  Report.xslt     | report.xslt         | _report template_

  Sample templates can be found on the [project website](https://github.com/mangh/Metrology).

## Replaces

- The package replaces the `XsltGeneratorApp` console application
available in the source code of Mangh.Metrology version 1.x.
