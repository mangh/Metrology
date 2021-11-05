# Xslt-based generator components:
* `UnitTranslatorXslt`: gets unit/scale information read by the `DefinitionParser` and translates them into C#  (in-memory) structs, according to XSLT-templates,
* `SourceGeneratorXslt`: relays those structs to the C# compiler (at compile-time).
* `XsltGeneratorApp`: console application to be used in design time; it does basically the same as `SourceGeneratorXslt` but saves the C# structs into files.