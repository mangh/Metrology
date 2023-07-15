## Families

A _family_ is a subset of units (or a subset of scales) that can be converted to each other. For example, _Meter_ and _Centimeter_ units form a family as long as they are combined by a conversion relationship similar to the following:

```JS
unit Centimeter "cm" = 100 * Meter;
```

Internally family is implemented as an integer number (family id) assigned automatically to each unit/scale when they are generated. Units/scales in a family have assigned the same family id and implement conversion methods to/from all other members of the family.

In case of units, it is only the [conversion relationship](./Definitions.md) that is deciding about family membership. In case of scales, family membership is determined by a [reference point name](./Definitions.md) that is common to all scales in the family.

Family relationship is transitive. In the example below all units belong to the same family (of _Meter_): you can convert _Inch_ to/from _Yard_ even though there is no conversion formula combining them (explicitly):

```JS
unit Meter "m" = <Length>; 
unit Inch "in" = 100 * Meter / 2.54;
unit Foot "ft" = Inch / 12;
unit Yard "yd" = Foot / 3;
unit Mile "mil" = Yard / 1760;
```

All units in a family have the same dimension but the reverse statement might not be true i.e. units of the same dimension might belong to disjoint families e.g. energy and torque.

Note that families are based on your definitions that may not follow general engineering conventions. In the example below _Centimeter_ is not included into the family of _Meter_ and _Inch_ i.e. it cannot be converted to/from _Meter_ or _Inch_, which may be surprising:

```JS
unit Centimeter "cm" = 100 * <Length>;
unit Inch "in" = 100 * Meter / 2.54;
```

<br/>

----