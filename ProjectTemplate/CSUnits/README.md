# C# Unit of Measurement Library Project Template

## Description

* The package provides a project template for the C# units of measurement class library (DLL).
* When installed, the template can be referenced from the command line using the _short name_ `csunits`.


## Installation

* Use the following command line to install the template:

  ```sh
  dotnet new install Mangh.Metrology.CSUnits
  ```

## Usage

* Create a project `<PROJECT_NAME>` in `<PROJECT_FOLDER>` for units of measurement in namespace `<PROJECT_NAMESPACE>`:
  ```sh
  dotnet new csunits -n <PROJECT_NAME> -o <PROJECT_FOLDER> -ns <PROJECT_NAMESPACE>
  ```

* To continue under Visual Studio IDE, attach the project to a [Visual Studio solution](https://docs.microsoft.com/en-us/visualstudio/get-started/tutorial-projects-solutions?view=vs-2022):
  * open Visual Studio IDE,
  * create a new or select an existing solution,
  * right click the solution root folder (or a solution subfolder),
  * perform `Add` -> `Existing Project...`,
  * select the project created above.

## Replaces

- The package replaces the
[Mangh.Metrology.UnitsOfMeasurement](https://www.nuget.org/packages/Mangh.Metrology.UnitsOfMeasurement)
package version 1.x.
