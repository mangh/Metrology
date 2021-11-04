# Xslt-based generator components:
* `UnitTranslatorXslt`: translates units information (received from the `DefinitionParser`) into C# structs (in-memory texts),
* `SourceGeneratorXslt`: passes those C# structs to the C# compiler (at a compile-time).