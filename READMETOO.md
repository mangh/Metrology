## How to use it without Source Generator?
You can modify the project (from the [README](README.md) page) to use external application [XsltGeneratorApp](https://github.com/mangh/Metrology/tree/main/Generator/Xslt/GeneratorApp) (at design time) instead of the Source Generator (at compile time) to generate units of measurement.

The advantage of this approach is that you can see the generated unit/scales, which can be important - especially in the _Visual Studio Code_ environment.
The disadvantage is that you have to remember to regenerate units/scales whenever definitions or templates have changed (unless you automate this process).

To use this approach you need to:
* download and build the [XsltGeneratorApp](https://github.com/mangh/Metrology/tree/main/Generator/Xslt/GeneratorApp) console application.
* modify the `.csproj` project file - created in step 1 of the the [README](README.md) instructions - to disable Source Generator and replace it with "regular" components; the new `.csproj` file should resemble the following:
    ```XML
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <TargetFramework>net6.0</TargetFramework>
        <ImplicitUsings>disable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <!-- The RootNamespace property is essential: it must be specified explicitly: -->
        <RootNamespace>PROJECT_NAMESPACE</RootNamespace>
        <!-- It is also good to have an AssemblyName property (though not necessary): -->
        <AssemblyName>PROJECT_NAMESPACE</AssemblyName>
      </PropertyGroup>
    <!--
      <ItemGroup>
        <AdditionalFiles Include=".\Templates\definitions.txt" />
        <AdditionalFiles Include=".\Templates\catalog.xslt" />
        <AdditionalFiles Include=".\Templates\aliases.xslt" />
        <AdditionalFiles Include=".\Templates\report.xslt" />
        <AdditionalFiles Include=".\Templates\scale.xslt">
          <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </AdditionalFiles>
        <AdditionalFiles Include=".\Templates\unit.xslt">
          <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </AdditionalFiles>
      </ItemGroup>
      <ItemGroup>
        <PackageReference Include="Mangh.Metrology.SourceGeneratorXslt" Version="1.0.4" />
        <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.0.1" />
      </ItemGroup>
    -->
      <ItemGroup>
        <None Include=".\Templates\scale.xslt" CopyToOutputDirectory="PreserveNewest" />
        <None Include=".\Templates\unit.xslt" CopyToOutputDirectory="PreserveNewest" />
      </ItemGroup>

      <ItemGroup>
        <PackageReference Include="Mangh.Metrology.DefinitionParser" Version="1.0.3" />
        <PackageReference Include="Mangh.Metrology.UnitTranslatorXslt" Version="1.0.4" />
        <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.0.1" />
      </ItemGroup>
    </Project>
    ```
* create a folder to store units of measurement, e.g.:
    ```Powershell
    md Units
    ```

* generate units of measurement in the new folder:

  NOTE: this step must be performed every time you change definitions or XSLT templates!
   
    ```Powershell
    # remove all previous contents of the Units folder:
    rm ./Units/*

    # Usage: XsltGeneratorApp targetNamespace [templateFolder [targetFolder]]
    /path/to/the/XsltGeneratorApp.exe PROJECT_NAMESPACE ./Templates ./Units
    ```

<br/>

----