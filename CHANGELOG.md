### Release 1.0.4 (12/27/2021)

###### Translator and Template: unit factor bug fix
* Unit factor is a get/set PROPERTY for monetary units ONLY,
  for other units that is a CONST FIELD.

### Release 1.0.3 (12/22/2021)

###### Selected string literals are recognized
* Parser recognizes `"System.Math.PI"` and `"System.Math.E"` literals to validate
 factors in `"unit<double>"` and `"scale<double>"` definitions that make use of
 these literals. (Other literals or in other definition types are still taken as
 numbers of unknown value - factor validation cannot be made).

###### XML structures timestamped
* Translator generates a timestamp attribute `"tm"` in the root node of XML structures.
  This can be used, via XSLT, to generate timestamped .cs files.
  The feature is by default commented out in XSLT as it makes `git` annoyingly overactive.

###### CancellationToken
* `Parser`, `Translator` and `SourceGenerator` polling for cancellation request
  (via `CancellationToken`) to end operations as soon as possible when it happens.
