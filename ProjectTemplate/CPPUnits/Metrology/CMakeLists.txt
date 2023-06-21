# Copyright (©) MAN. This program is provided to you under the terms
# of the license as published at https://github.com/mangh/Metrology.

#[=======================================================================[.rst:
Units of Measurement C++ Header-only Library
--------------------------------------------

Generate unit of measurement header files:

  Units/*.h,

based on unit definitions from the file:

  Templates/definitions.txt

and XSLT-templates for unit/scale structures:

  Templates/unit.xslt,
  Templates/scale.xslt,
  Templates/report.xslt

that use XSLT helper functions:

  Templates/replace-string.xslt,
  Templates/math-constants.xslt.

This project (i.e the above folders and files, including this script)
is intended to be used from within a parent project using CMake
add_subdirectory() command.


Targets defined
^^^^^^^^^^^^^^^

  %PROJECT%_LIBRARY - interface (header-only) library (main target),

  %PROJECT%_HEADERS - custom target that triggers rebuild (custom command).


Result Variables
^^^^^^^^^^^^^^^^

  %PROJECT%_NAMESPACE - namespace for units of measurement.

  %PROJECT%_CHANGE_TIP - a file that serves as an indicator
                        (guard) of changes in metrological units.
                        It triggers a rebuild when tracked as a
                        file-level dependency (see below).

#]=======================================================================]

cmake_minimum_required (VERSION ${CMAKE_VERSION})

##########################################################################
#
#   %PROJECT% project
#

project(
  %PROJECT%
  LANGUAGES C CXX
  DESCRIPTION "Unit of measurement headers-only library."
)

set(target %PROJECT%_LIBRARY)

##########################################################################
#
#   %PROJECT%_SOURCE_DIR subfolders
#

set(_templates_subdir "Templates")
set(_units_subdir "Units")

##########################################################################
#
#   %PROJECT% namespace
#

set(%PROJECT%_NAMESPACE "%NAMESPACE%" CACHE STRING "Namespace for units of measurement.")

##########################################################################
#
#   %PROJECT%_CHANGE_TIP
#
#   Each time the metrological units are reconstructed, 
#   the "Units/generator_report.txt" file is created anew. 
#   Therefore, it can serve as an indicator (guard) of changes
#   in metrological units and trigger a rebuild when tracked
#   as a file-level dependency for source file(s) that depend
#   on units.
#
#   You can explicitly set this file-level dependency with
#   the following command:
#
#     set_property(
#       SOURCE file... 
#       PROPERTY OBJECT_DEPENDS
#       ${%PROJECT%_CHANGE_TIP}
#     )
#
#   for each source file(s) that need to be recompiled when
#   unit definitions and/or templates change.
#

set(
  %PROJECT%_CHANGE_TIP
  "${%PROJECT%_SOURCE_DIR}/${_units_subdir}/generator_report.txt"
  CACHE FILEPATH
  "File that serves as an indicator (guard) of changes in metrological units."
  FORCE
)

##########################################################################
#
#   %PROJECT% rebuild command (UnitGenerator)
#
#   NOTE: Please amend the command:
#
#       "cmake -E chdir ... UnitGenerator ..."
#
#   (bellow) depending on how the Mangh.Metrology.UnitGenerator tool
#   is installed on your computer: as a "global tool" (as assumed here), 
#   or a "global tool in a custom location" or as a "local tool").
#   For details, see article "How to manage .NET tools",
#   https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools.
#

add_custom_command(
  OUTPUT
    "${%PROJECT%_CHANGE_TIP}"
  COMMAND cmake -E echo
  COMMAND cmake -E rm -f "${%PROJECT%_SOURCE_DIR}/${_units_subdir}/*.h"
  COMMAND cmake -E chdir "${%PROJECT%_SOURCE_DIR}" UnitGenerator CPP "${%PROJECT%_NAMESPACE}" "${_templates_subdir}" "${_units_subdir}"
  COMMAND cmake -E echo
  DEPENDS
    "${%PROJECT%_SOURCE_DIR}/${_templates_subdir}/definitions.txt"
    "${%PROJECT%_SOURCE_DIR}/${_templates_subdir}/unit.xslt"
    "${%PROJECT%_SOURCE_DIR}/${_templates_subdir}/scale.xslt"
    "${%PROJECT%_SOURCE_DIR}/${_templates_subdir}/report.xslt"
    "${%PROJECT%_SOURCE_DIR}/${_templates_subdir}/replace-string.xslt"
    "${%PROJECT%_SOURCE_DIR}/${_templates_subdir}/math-constants.xslt"

  COMMENT "Generate %PROJECT% (headers-only) Library."
  USES_TERMINAL
)

##########################################################################
#
#   %PROJECT%_HEADERS target
#
#   Custom target that triggers the REBUILD COMMAND
#

add_custom_target(%PROJECT%_HEADERS DEPENDS "${%PROJECT%_CHANGE_TIP}")

##########################################################################
#
#   %PROJECT%_LIBRARY target
#
#   %PROJECT%_LIBRARY (as an INTERFACE library) does not build itself
#   but target(s) it depends on (such as %PROJECT%_HEADERS) are followed
#   (transitively) in its place and trigger a rebuild when needed.
#

add_library(${target} INTERFACE)
target_include_directories(${target} INTERFACE Units)
add_dependencies(${target} %PROJECT%_HEADERS)
