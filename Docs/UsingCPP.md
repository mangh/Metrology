# Using C++ projects

## Installing the required tools

Before creating a C++ project, install the following tools:

* install the [CPPUnits](https://www.nuget.org/packages/Mangh.Metrology.CPPUnits) package,
which provides project template for C++ unit of measurement library:

  ```sh
  dotnet new install Mangh.Metrology.CPPUnits
  ```

* install the [UnitGenerator](https://www.nuget.org/packages/Mangh.Metrology.UnitGenerator) package,
which provides a standalone [dotnet tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools)
for generating unit structures in C++ (as well as C#):

  ```sh
  dotnet tool install --global Mangh.Metrology.UnitGenerator
  ```

The above steps only need to be done once (you don't need to repeat them for each new project).

## Creating a project

* Use the following command to create a project `<PROJECT_NAME>` in `<PROJECT_FOLDER>` for units of measurement in namespace `<PROJECT_NAMESPACE>`:

   ```sh
   dotnet new cppunits -n <PROJECT_NAME> -o <PROJECT_FOLDER> -ns <PROJECT_NAMESPACE>
   ```
 
   This will create a [CMake](https://cmake.org/cmake/help/latest/) project: 
   
   ```
   <PROJECT_FOLDER>/
   ├── CMakeLists.txt
   ├── README.md
   ├── Templates
   │   ├── definitions.txt
   │   ├── math-constants.xslt
   │   ├── replace-string.xslt
   │   ├── report.xslt
   │   ├── scale.xslt
   │   └── unit.xslt
   └── Units
       └── detail
           └── to_string.h
   ```

   For more information on project folders and files, see the ["C++ project"](./ProjectCPP.md) page.

   The project is intended to be used as a subproject of some parent project (the one that requires units of measurements)
   i.e. it is expected to be run from the parent project using the CMake command:

   ```CMake
   add_subdirectory("<PROJECT_FOLDER>")
   ```

   This command creates the following:

    * units of measurement (`*.h` files in the `Units` folder) based on the `definitions.txt` file and XSLT templates,
    * the `<PROJECT_NAME>_LIBRARY` target, which can be used in dependent projects as a reference to the units of measurement library (which is a header-only library),
    * the `<PROJECT_NAME>_CHANGE_TIP` property, which can be used in dependent projects to trigger rebuilding in response to changes in unit and/or template definitions.

   See the generated `CMakeLists.txt` file for details.

## Using the generated project

* Unit headers can be generated regardless of whether there is a parent project or not.
  All you need to do is run CMake for the `<PROJECT_FOLDER>` subdirectory itself. This can be done as follows:
 
  ```sh
  cd <PROJECT_FOLDER>
  mkdir build
  cd build
  cmake ..
  cmake --build . --target <PROJECT_NAME>_HEADERS
  ```

* Sample CMake project (SAMPLEPROJECT) to build MYAPP executable that uses METROLOGY (units of measurement) library created from the _cppunits_ template:

  ```sh
  mkdir SAMPLEPROJECT
  cd SAMPLEPROJECT
  vim ./CMakeLists.txt

  mkdir MYAPP
  vim MYAPP/CMakeLists.txt
  vim MYAPP/main.cpp
  vim MYAPP/source1.cpp
  vim MYAPP/source2.cpp
  ...

  # still in the SAMPLEPROJECT folder:
  dotnet new cppunits -n METROLOGY -o METROLOGY -ns "MYAPP::Units"
  ```
  
  The above commands would create folder and file structure that resembles the following:
  ```
  # Folder structure
  SAMPLEPROJECT
  ├── CMakeLists.txt
  ├── MYAPP
  │   ├── CMakeLists.txt
  │   ├── main.cpp
  │   ├── source1.cpp
  │   ├── source2.cpp
  ...
  └── METROLOGY
      ├── CMakeLists.txt
      ├── Templates
      │   ├── definitions.txt
      │   ├── math-constants.xslt
      │   ├── replace-string.xslt
      │   ├── report.xslt
      │   ├── scale.xslt
      │   └── unit.xslt
      └──Units
          └──detail
             └──to_string.h
  ```
  
  The top-level project could look like this:
  ```cmake
  # SAMPLEPROJECT/CMakeLists.txt
  # Top-level CMake project file. 
  # Here you need to make a global configuration and attach sub-projects.
  #

  cmake_minimum_required (VERSION 3.24)
  project(SAMPLEPROJECT VERSION "1.0.0" LANGUAGES C CXX)
  
  # Global configuration
  ...
  
  # METROLOGY subproject (units of measurement header-only library):
  add_subdirectory ("METROLOGY")
  
  # MYAPP executable subproject:
  add_subdirectory ("MYAPP")
  ```

  The MYAPP project could look like this:
  ```cmake
  # MYAPP/CMakeLists.txt
  # A subproject for the MYAPP executable file.
  #
  ...
  # Source files for the MYAPP executable:
  set(_source_files
    main.cpp
    source1.cpp
    source2.cpp
    ...
  )

  # We want MYAPP to be rebuilt whenever METROLOGY headers change.
  # To this end we can set explicit dependency of the source files
  # on METROLOGY_CHANGE_TIP file that is built anew whenever the
  # headers change (for simplicity, it is assumed here that all
  # source files depend on those headers):
  set_property(
    SOURCE ${_source_files}
    PROPERTY OBJECT_DEPENDS "${METROLOGY_CHANGE_TIP}"
  )

  # Add MYAPP executable target to the project using the specified source files:
  add_executable(
    MYAPP
    ${_source_files}
  )
  
  # target_link_libraries command imports the so called Usage Requirements of
  # the METROLOGY_LIBRARY. This means, among other things, import of the library
  # include-directories (without using the target_include_directories command):
  target_link_libraries(MYAPP PRIVATE METROLOGY_LIBRARY)
  ```

  The MYAPP executable could be finally built like this:
  ```sh
  # Assuming SAMPLEPROJECT is the current directory:
  mkdir build
  cd build
  cmake ..
  cmake --build .
  ```

* Go to [CALINE3.CPP](https://github.com/mangh/CALINE3.CPP) page to look at a more extensive and working example of a C++ project.

<br/>

---