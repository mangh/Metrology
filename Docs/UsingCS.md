# Using C# projects

## Installing the required tools

* install the [CSUnits](https://www.nuget.org/packages/Mangh.Metrology.CSUnits/) package,
which provides project template for C# unit of measurement library:

  ```powershell
  dotnet new install Mangh.Metrology.CSUnits
  ```

## Creating a project

1. Create a project `<PROJECT_NAME>` in `<PROJECT_FOLDER>` for units of measurement in namespace `<PROJECT_NAMESPACE>`:
    
    ```powershell
    dotnet new csunits -n <PROJECT_NAME> -o <PROJECT_FOLDER> -ns <PROJECT_NAMESPACE>
    ```
    
    For more information on the folders and files created with this command, see the ["C# project"](./ProjectCS.md) page.

2. (*optional*) To continue under Visual Studio IDE, attach the project to a [Visual Studio solution](https://docs.microsoft.com/en-us/visualstudio/get-started/tutorial-projects-solutions?view=vs-2022):
   - open Visual Studio IDE,
   - create a new or select an existing solution,
   - right click the solution root folder (or a solution subfolder),
   - perform: `Add` -> `Existing Project...`,
   - select the project created above.

3. Go to the `./Templates` folder:
    - edit `Definitions.txt` to specify units of measurement for your solution(s), 
    - (*optional*) customize XSLT templates:
      - `Unit.xslt`: template for a single unit (_struct_),
      - `Scale.xslt`: template for a single scale (_struct_),
      - `Catalog.xslt`: template for the _Catalog class_ (catalog of all defined units and scales),
      - `Aliases.xslt`: template for `Aliases.inc` file that might be used to import unit and scale types in dependent projects,
      - `Report.xslt`: template for `generator_report.txt` file (a summary of generated units and scales).
  
4. (*optional*) Modify the `Directory.Build.targets` file so that the generated `Aliases.inc` file
is (automatically) exported to all dependent projects (those that require units of measurement).
See: [Demo/UnitsOfMeasurement](https://github.com/mangh/Metrology/tree/main/Demo/UnitsOfMeasurement) or
[CALINE3/Metrology](https://github.com/mangh/CALINE3.CS/tree/main/Metrology) projects for an example.
You can safely delete the file if you are not going to use it.

5. (*optional*) Modify the `Math.cs` source file to provide the additional math required to handle the units.
See: [Demo/UnitsOfMeasurement](https://github.com/mangh/Metrology/tree/main/Demo/UnitsOfMeasurement) or
[CALINE3/Metrology](https://github.com/mangh/CALINE3.CS/tree/main/Metrology) projects for an example.
You can safely delete the file if you are not going to use it.

6. Compile the project to create the library:
   ```powershell
   dotnet build
   ```

<br/>

---