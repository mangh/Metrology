# Project template for Units of Measurement C# class library for .NET Core / Standard

Usage instructions:

1. Install `Mangh.Metrology.UnitsOfMeasurement` NuGet package.
```powershell
  dotnet new --install Mangh.Metrology.UnitsOfMeasurement
```

2. Create `unitsofmeasurement` project in a `PROJECT_FOLDER` and `PROJECT_NAMESPACE`:
```powershell
  md PROJECT_FOLDER
  cd PROJECT_FOLDER
  dotnet new unitsofmeasurement [-n PROJECT_NAMESPACE]
```

3. To continue under Visual Studio IDE, attach the project to a [Visual Studio solution](https://docs.microsoft.com/en-us/visualstudio/get-started/tutorial-projects-solutions?view=vs-2022):
* open Visual Studio IDE,
* create a new or select an existing solution,
* right click the solution root folder (or a solution subfolder),
* perform `Add` -> `Existing Project...`,
* select the project created above.
