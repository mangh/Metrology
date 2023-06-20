# Metrology Definitions

## Description 

The package is used to:
- read unit of measurement definitions from a text stream,
- verify them
- and transform into a form suitable for subsequent modeling of unit structures in a target language (C#, C++).

The modeling itself is provided by other packages
(such as [Mangh.Metrology.Model](https://www.nuget.org/packages/Mangh.Metrology.Model))
to allow the use of various translation engines.

## Installation 

- The package does not need to be installed separately.
It is loaded (restored) automatically (along with the packages it depends on) when it is needed
i.e. when a project being built references it (directly or indirectly).

## Replaces

* The package replaces the
[Mangh.Metrology.DefinitionParser](https://www.nuget.org/packages/Mangh.Metrology.DefinitionParser)
package version 1.x.
