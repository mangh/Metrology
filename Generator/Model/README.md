# Modeling Metrology Structures

## Description

The package, based on data received from [Mangh.Metrology.Definitions](https://www.nuget.org/packages/Mangh.Metrology.Definitions):
- creates models of metrology structures (units, scales, etc.) suitable for the transformation engine being used,
- transforms these models into the target structures of the selected language (C# or C++).

NOTE: currently, the package uses XML models and XSLT templates for transformation.
You can customize the XSLT templates to get the target language structures that suit you best.

## Installation

- The package does not need to be installed separately.
It is loaded (restored) automatically (along with the packages it depends on) when it is needed
i.e. when a project being built references it (directly or indirectly).

## Replaces

* The package replaces the
[Mangh.Metrology.UnitTranslatorXslt](https://www.nuget.org/packages/Mangh.Metrology.UnitTranslatorXslt)
package version 1.x.
