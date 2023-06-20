# C++ Unit of Measurement Library Project Template

## Description

* The package provides a CMake project template for the C++ units of measurement library (header only).
* When installed, the template can be referenced from the command line using the _short name_ `cppunits`.

## Installation

* Install the project template:

  ```sh
  dotnet new install Mangh.Metrology.CPPUnits
  ```

* Projects created from the _cppunits_ template do their work with the dotnet tool
[Mangh.Metrology.UnitGenerator](https://www.nuget.org/packages/Mangh.Metrology.UnitGenerator)
, so it is also necessary to install it:

  ```sh
  dotnet tool install --global Mangh.Metrology.UnitGenerator
  ```

## Usage

* Use the following command to create a project `<PROJECT_NAME>` in `<PROJECT_FOLDER>` 
for units of measurement in namespace `<PROJECT_NAMESPACE>`:

  ```sh
  dotnet new cppunits -n <PROJECT_NAME> -o <PROJECT_FOLDER> -ns <PROJECT_NAMESPACE>
  ```
  This will create the following folder and file structure:

  ```txt
  <PROJECT_FOLDER>
  |   CMakeLists.txt
  |
  +---Templates
  |      definitions.txt
  |      math-constants.xslt
  |      replace-string.xslt
  |      report.xslt
  |      scale.xslt
  |      unit.xslt
  |
  \---Units
      |
      \--detail
            to_string.h
  ```
  The structure is intended to be used from within a parent project by means of the CMake `add_subdirectory("<PROJECT_FOLDER>")` command.
  This command will make available (to the parent as well as any dependent project):
  * the `<PROJECT_NAME>_LIBRARY` library target, which is a header-only library of units of measurement (INTERFACE library target),
  * the `<PROJECT_NAME>_CHANGE_TIP` property, which allows you to make dependent projects sensitive to changes in unit and template definitions.

  For more information on the targets and variables made available to the parent and dependent projects,
  see the `CMakeLists.txt` file.
  
  Units of measure (`Units/*.h` header files) are generated at compile time,
  based on definitions (`Templates/definitions.txt`) and
  templates (`Templates/*.xslt`) for unit/scale structures.

### Sample usage

* Generate unit headers directly from the command line (using only the CMakeLists.txt script from the \<PROJECT_FOLDER\> subdir):
 
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

  dotnet new cppunits -n METROLOGY -o METROLOGY -ns "MYAPP::Units"
  ```
  
  The above commands would create folder and file structure that resembles the following:
  ```sh
  # Folder structure
  SAMPLEPROJECT
  |   CMakeLists.txt
  |
  +---MYAPP
  |       CMakeLists.txt
  |       MYAPP.cpp
  |       source1.cpp
  |       source2.cpp
  |       ...
  |
  \---METROLOGY
      |   CMakeLists.txt
      |
      +---Templates
      |      definitions.txt
      |      math-constants.xslt
      |      replace-string.xslt
      |      report.xslt
      |      scale.xslt
      |      unit.xslt
      |
      \---Units
          |
          \--detail
                to_string.h
  ```
  
  The top-level project could look like this:
  ```cmake
  # SAMPLEPROJECT/CMakeLists.txt
  # Top-level CMake project file; 
  # do global configuration and include sub-projects here
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
  # MYAPP executable subproject
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
