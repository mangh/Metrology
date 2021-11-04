# Metrology: Units Definition Parser
The parser:
- reads the definitions of measurement units from a text file,
- parses and transforms these definitions into an intermediate form that can be further translated into metrology unit types (classes) for applications requiring effective dimensional verification (analysis).

1. Parses definitions of units of measurement and transforms them into an intermediate form that can be further...
2. ...translated into metrology unit types (in a target language e.g. CSharp) that enable verification (dimensional analysis) in applications sensitive to dimensional issues. 

This package takes care of the 1st stage above. Another package (UnitTranslator) takes care of the 2nd stage.