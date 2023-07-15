# Definitions

## Syntax
```
<Definition> ::= <Unit> | <Scale>
<Unit> ::= 'unit'[<ValueType>] unit-name <Tags> [<Format>] '=' <Dim Expr> [ '|' <Dim Expr> ] ';'
<Scale> ::= 'scale' scale-name [<Format>] [refpoint] '=' unit-name <Num Expr> ';'
    
<ValueType> ::= '<double>' | '<float>' | '<decimal>' (for C#)
            ::= '<double>' | '<float>' | '<longdouble>' (for C++)

        Numeric type underlying the unit. Optional. The default is '<double>'.

unit-, scale-name ::= identifier
 
        The identifier has to be valid both as a C# class name and a file name.
        For example: Meter.

<Tags> ::= StringLiteral+

        One or more (string) symbols of the unit, separated by spaces.
        For example: "m" "meter".

<Format> ::= ':' StringLiteral

        String to be used as a composite format parameter in ToString(format, ...)
        or String.Format(format, ...) output methods. Optional. The default is
        "{0} {1}": {0} as a value placeholder, {1} as a placeholder for
        unit symbol (tag).

refpoint ::= identifier
 
        Symbolic name (reference point identifier) to distinguish scale families
        derived from the same units but bound to different reference levels.
        For example: AbsoluteZero.

<Dim Expr> ::= dimensional expression

        An expression build of numbers, string literals, unit names,
        dimensional keywords (magnitudes) and multiplicative operators (*|^, /).
        It sets dimension of the unit, its conversion factor and arithmetic
        (operator) relationship with other units. There might be several
        expressions (ways) to obtain the unit, separated by the pipe symbol '|'.

<Magnitude> ::= '<Length>'
             |  '<Time>'
             |  '<Mass>'
             |  '<Temperature>'
             |  '<ElectricCurrent>'
             |  '<AmountOfSubstance>'
             |  '<LuminousIntensity>'
             |  '<Other>' | '<Money>'
             |  '<>'    // for dimensionless quantities

<Num Expr> ::= numeric expression

        An expression build of numbers, string literals and arithmetic
        operators (*, /, +, -). It specifies a reference level for the
        associated scale.
```

* Parser makes no assumptions of any conventions applied in engineering or measurement practice. Therefore you can choose any names, symbols and expressions (relationships) for your units.

* All numbers, regardless of their syntax, are handled as floating-point numbers of the specified `<ValueType>`, with dot `'.'` as a decimal point (invariant culture). For example:
  *  `123`,
  *  `456.78`,
  *  `1.0e-10`.

* Single-line (`//`) and block (`/* */`) comments can be used to clarify (or exclude) sections of definitions.
  
* String literals (e.g. `"Sqrt2"`) in `<Dim Expr>` and `<Num Expr>` expressions are handled - as a general rule - as numbers of unknown value: their evaluation and syntax check is left to the C# compiler. Of course, using a string literal (of an unknown numeric value) in a formula makes it impossible to verify the conversion factor derived from the formula. The literals `"Math.PI"` and `"Math.E"` (if written exactly this way) are the exceptions to this rule: the parser can recognize them and use the appropriate numeric value to perform the validation.
   
   So formulas in the definition:
   ```
   unit Degree_Sec "degree/s" = Degree / Second | (180 / "Math.PI") * Radian_Sec;
   ```
   can be verified for both consistent dimensions and conversion factors, but in the definition:
   ```
   unit Degree_Sec "degree/s" = Degree / Second | (180 / "PI") * Radian_Sec;
   ```
   they can be verified for dimensions only: conversion factors have to be verified by other means (e.g. unit tests).

* Wedge operator `'^'` is an alternative operator for multiplying units (and only units) in addition to the standard operator `'*'`.
  It allows to make a distinction between scalar (\*) and vector (^) product that create different units from the same factors.
  For example:

    ```
    unit Joule "J" = Newton * Meter;          /*energy*/
    unit NewtonMeter "N*m" = Newton ^ Meter;  /*torque*/
    ```

    which translates into:

    ```C#
    public static Joule operator *(Newton lhs, Meter rhs) { ... }
    public static Joule operator *(Meter lhs, Newton rhs) { ... }
    public static NewtonMeter operator ^(Newton lhs, Meter rhs) { ... }
    public static NewtonMeter operator ^(Meter lhs, Newton rhs) { ... }
    ```

* &#128073; Product (or quotient) of (exactly) 2 units, i.e. `<Dim Expr>` expression of the form:

    * `unit * unit`
    * `unit ^ unit`
    * `unit / unit`

    generates ___binary (2-argument) operators___. For example the definition:

    ```
    unit Watt "W" = Joule / Second;
    ```
    is translated to:
    ```C#
    public static Watt operator /(Joule lhs, Second rhs)
    public static Joule operator *(Watt lhs, Second rhs)
    public static Joule operator *(Second lhs, Watt rhs)
    ```
* &#128073; Product (or quotient) of a unit and a numeric expression, i.e. `<Dim Expr>` expression of the form:

    * `unit * numexpr`
    * `numexpr * unit`
    * `unit / numexpr`

    generates ___conversion methods___. For example the definition:

    ```
    unit Kilometer "km" = Meter / 1000;
    ```
    is translated to:

    ```C#
    public static Meter From(Kilometer q)
    public static Kilometer From(Meter q)
    ```

<br/>

## Semantics
### Primary units
```
unit Meter "m" = <Length>; 
unit Second "s" = <Time>;
```
The above definitions specify: _Meter_ as a _length_ unit (of symbol "m") and _Second_ as a _time_ unit (of symbol "s"); each unit of (implicit) conversion factor 1.0. They do not introduce - neither explicitly nor implicitly - any relationship with other units. Units defined this way can be considered as _primary_.

### Conversion relationship
```
unit Centimeter "cm" = 100 * Meter;
```
It defines _Centimeter_ in terms of (the previously defined) _Meter_, as a unit that can be obtained (among others) from _Meter_ by conversion. It has symbol "cm", dimension _length_ (inherited from _Meter_) and (explicit) conversion factor 100.0, meaning:

    (length in Centimeters) = 100.0 * (length in Meters).

The relationships would be implemented as conversion methods in _Meter_ and _Centimeter_ classes:
```C#
public static Meter From(Centimeter q)    // in Meter.cs
public static Centimeter From(Meter q)    // in Centimeter.cs
```

### Arithmetic (product or quotient) relationship
```
unit Meter_Sec "m/s" = Meter / Second;
```
It derives _Meter_Sec_ unit as a quotient of _Meter_ and _Second_ units. It has symbol "m/s", dimension _velocity_ (_length/time_ as derived from the underlying units) and conversion factor 1.0 (calculated from the underlying units). The relationship *Meter_Sec = Meter / Second* between (quantities of type) _Meter_, _Second_ and *Meter_Sec* would be implemented as arithmetic operators:
```C#
public static Meter_Sec operator /(Meter lhs, Second rhs) // in Meter.cs
public static Second operator /(Meter lhs, Meter_Sec rhs) // in Meter.cs
public static Meter operator *(Meter_Sec lhs, Second rhs) // in Meter_Sec.cs
public static Meter operator *(Second lhs, Meter_Sec rhs) // in Meter_Sec.cs
```

### Multiple relationships
```
unit Kilometer_Hour "km/h" = Kilometer / Hour | Meter_Sec * (1/1000) / ((1/60)/60);
```
The *Kilometer_Hour* unit (specified in terms of the previously defined _Kilometer_, _Hour_ and *Meter_Sec* units) can be obtained both as the quotient _Kilometer / Hour_ and by conversion from *Meter_Sec*.

It has symbol "km/h" and dimension _velocity_. Its conversion factor is calculated from factors of the defining units using (quotient and conversion) expressions given in the definition. Both expressions must produce the same dimension and conversion factor: otherwise the definition will be rejected as an error.

The quotient relationship would be implemented as arithmetic operators:
```C#
public static Kilometer_Hour operator /(Kilometer lhs, Hour rhs) // Kilometer.cs
public static Hour operator /(Kilometer lhs, Kilometer_Hour rhs) // Kilometer.cs
public static Kilometer operator *(Kilometer_Hour lhs, Hour rhs) // Kilometer_Hour.cs
public static Kilometer operator *(Hour lhs, Kilometer_Hour rhs) // Kilometer_Hour.cs
```
The conversion relationship would be implemented as:
```C#
public static Kilometer_Hour From(Meter_Sec q) // Kilometer_Hour.cs
public static Meter_Sec From(Kilometer_Hour q) // Meter_Sec.cs
```

The weird factor `(1/1000) / ((1/60)/60)` uses the same numbers and in the same role (numerator, denominator) as previously used in the definition of _Kilometer_ and _Hour_ units (and further units deeper in the expansion). This ensures consistent conversion factors produced by both expressions in the definition i.e. factors equal up to the maximum internal precision built into the C# floating-point engine.

<br/>

----