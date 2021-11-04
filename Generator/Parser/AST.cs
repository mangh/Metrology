/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;

namespace Man.Metrology
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    internal abstract class ASTNode
    {
        #region Properties
        public virtual bool IsNumeric => false;
        public virtual UnitType? Unit => null;
        public virtual bool IsWedgeCompatible => false;
        #endregion

        #region Constructor(s)
        //protected ASTNode()
        //{
        //}
        #endregion

        #region Methods
        /// <summary>
        /// Encoder entry
        /// </summary>
        public abstract void Accept(IASTEncoder encoder);

        /// <summary>
        /// Rearrange unit expression tree into one of the following canonical forms:
        /// <list type="bullet">
        /// <item><code>&#958; * u</code></item>
        /// <item><code>u * &#958;</code></item>
        /// <item><code>&#958; / u</code></item>
        /// <item><code>u / &#958;</code></item>
        /// <item><code>u / v</code></item>
        /// </list>
        /// to get clear picture of units involved, and bind them in the target source code.
        /// </summary>
        /// <remarks><c>u, v</c> - units; <c>&#958;</c> - numeric expression (made of numbers and/or literals only)</remarks>
        /// <returns>Canonical expression or null in case no rearrangement could be made</returns>
        /// <example>(180.0 * Radian) / "System.Math.PI" --&gt; (180.0 / "System.Math.PI") * Radian</example>
        public virtual ASTNode? TryNormalize() => null;

        /// <summary>
        /// Move numeric argument, by means of product, down the expression tree.
        /// </summary>
        /// <param name="number">numeric argument found at the upper level of the expression tree</param>
        /// <returns>New expression or null in case no product could be applied</returns>
        public virtual ASTNode? TryMultiply(ASTNode number) => null;

        /// <summary>
        /// Move numeric argument, by means of quotient, down the expression tree.
        /// </summary>
        /// <param name="number">numeric argument found at the upper level of the expression tree</param>
        /// <returns>New expression or null in case no quotient could be applied</returns>
        public virtual ASTNode? TryDivide(ASTNode number) => null;

        /// <summary>
        /// Returns (preferably normalized) expression node.
        /// </summary>
        /// <returns>Normalized node (or the original one if it cannot be normalized).</returns>
        public virtual ASTNode Normalized()
        {
            ASTNode? normalized = TryNormalize(); 
            return normalized ?? this;
        }

        /// <summary>
        /// Bind result unit (the one being currently developed) to the units embedded in 
        /// the underlying (normalized) expression. Depending on the expression, the binding
        /// would be implemented as:
        /// <list type="bullet">
        /// <item>conversion operators for expressions: <code>&#958; * u, u * &#958;, u / &#958;</code></item>
        /// <item>binary operators for expressions: <code>u * v, u / v, &#958; / u</code></item>
        /// <item>not implemented (no binding) for any other expressions</item>
        /// </list>
        /// </summary>
        /// <remarks><c>u, v</c> - units, <c>&#958;</c> - numeric expression (made of numbers and/or literals only)</remarks>
        public virtual void Bind(UnitType candidate) { }

        #endregion
    }

    internal class ASTNumber : ASTNode
    {
        public Numeral Number { get; private set; }

        public override bool IsNumeric => true;

        public ASTNumber(Numeral number) :
            base() => Number = number;

        public override void Accept(IASTEncoder encoder) => encoder.Encode(this);

        public override string? ToString() => Number.ToString();
    }

    internal class ASTLiteral : ASTNode
    {
        public string Literal { get; private set; }

        public override bool IsNumeric => true;

        public ASTLiteral(string literal) :
            base() => Literal = literal;

        public override void Accept(IASTEncoder encoder) => encoder.Encode(this);

        public override string ToString() => $"\"{Literal}\"";
    }

    internal class ASTMagnitude : ASTNode
    {
        public int Exponent { get; private set; }

        public ASTMagnitude(Magnitude magnitude) :
            base() => Exponent = (int)magnitude;
        public ASTMagnitude() :
            base() => Exponent = -1;    // fake -1 to denote dimensionless magnitude

        public override void Accept(IASTEncoder encoder) => encoder.Encode(this);

        public override string ToString() => $"<{(Exponent == -1 ? string.Empty : ((Magnitude)Exponent).ToString())}>";
    }

    internal class ASTUnit : ASTNode
    {
        private readonly UnitType m_unit;
        public override UnitType Unit => m_unit;
        public override bool IsWedgeCompatible => true;

        public ASTUnit(UnitType unit) :
            base() => m_unit = unit;

        public override void Accept(IASTEncoder encoder) => encoder.Encode(this);

        public override void Bind(UnitType candidate) => m_unit.AddRelative(candidate);

        public override string ToString() => m_unit.Typename;
    }

    internal class ASTUnary : ASTNode
    {
        public bool Plus { get; private set; }
        public ASTNode Expr { get; private set; }

        public override bool IsNumeric => Expr.IsNumeric;

        public ASTUnary(bool plus, ASTNode expr) :
            base()
        {
            Plus = plus;
            Expr = expr;
        }

        public override void Accept(IASTEncoder encoder) { Expr.Accept(encoder); encoder.Encode(this); }

        public override string ToString() => $"{(Plus ? "+" : "-")}{Expr}";
    }

    internal class ASTParenthesized : ASTNode
    {
        public ASTNode Expr { get; private set; }

        public override bool IsNumeric => Expr.IsNumeric;
        public override UnitType? Unit => Expr.Unit;
        public override bool IsWedgeCompatible => Expr.IsWedgeCompatible;

        public ASTParenthesized(ASTNode expr) :
            base() => Expr = expr;

        public override void Accept(IASTEncoder encoder) { Expr.Accept(encoder); encoder.Encode(this); }

        public override ASTNode? TryNormalize()
        {
            ASTNode? normalized = Expr.TryNormalize();
            return (normalized is null) ? null : new ASTParenthesized(normalized);
        }
        public override ASTNode? TryMultiply(ASTNode number)
        {
            ASTNode? normalized = Expr.TryMultiply(number);
            return (normalized is null) ? null : new ASTParenthesized(normalized);
        }
        public override ASTNode? TryDivide(ASTNode number)
        {
            ASTNode? normalized = Expr.TryDivide(number);
            return (normalized is null) ? null : new ASTParenthesized(normalized);
        }

        public override void Bind(UnitType candidate) => Expr.Bind(candidate);

        public override string ToString() => $"({Expr})";
    }

    internal class ASTProduct : ASTNode
    {
        private readonly bool m_iswedgeproduct;

        public ASTNode Lhs { get; private set; }
        public ASTNode Rhs { get; private set; }
        public string Operation => m_iswedgeproduct ? "^" : "*";

        public override bool IsNumeric => Lhs.IsNumeric && Rhs.IsNumeric;
        public override bool IsWedgeCompatible => m_iswedgeproduct ||
                                                (Lhs.IsNumeric && Rhs.IsWedgeCompatible) ||
                                                (Lhs.IsWedgeCompatible && Rhs.IsNumeric);
        /// <summary>
        /// Multiply nodes.
        /// </summary>
        /// <param name="lhs">left-hand-side factor</param>
        /// <param name="rhs">right-hand-side factor</param>
        /// <param name="iswedgeproduct">Is it wedge "^" or scalar "*" product</param>
        /// <exception cref="ArgumentException">
        /// thrown for factor(s) incompatible with wedge "^" product (i.e. for non-unit and non-numeric factors).
        /// </exception>
        public ASTProduct(ASTNode lhs, ASTNode rhs, bool iswedgeproduct = false) :
            base()
        {
            if (m_iswedgeproduct && (!lhs.IsWedgeCompatible || !rhs.IsWedgeCompatible))
            {
                throw new ArgumentException($"{lhs} ^ {rhs}: factors not suitable for wedge-product \"^\".");
            }

            Lhs = lhs;
            Rhs = rhs;
            m_iswedgeproduct = iswedgeproduct;
        }

        public override void Accept(IASTEncoder encoder) { Lhs.Accept(encoder); Rhs.Accept(encoder); encoder.Encode(this); }

        public override ASTNode? TryNormalize() => 
            Lhs.IsNumeric ? Rhs.TryMultiply(Lhs) : 
            Rhs.IsNumeric ? Lhs.TryMultiply(Rhs) : 
            null;

        // [Lhs * Rhs] / number
        public override ASTNode? TryDivide(ASTNode number)
        {
            if (Lhs.IsNumeric)
            {
                // [Lhs * Rhs] / number --> [Lhs / number] * Rhs
                ASTNode quotient = new ASTQuotient(Lhs, number);
                ASTNode? normalized = Rhs.TryMultiply(quotient);
                return normalized ?? new ASTProduct(quotient, Rhs);
            }
            if (Rhs.IsNumeric)
            {
                // [Lhs * Rhs] / number --> Lhs * [Rhs / number]
                ASTNode quotient = new ASTQuotient(Rhs, number);
                ASTNode? normalized = Lhs.TryMultiply(quotient);
                return normalized ?? new ASTProduct(Lhs, quotient);
            }
            return null;
        }

        // [Lhs * Rhs] * number
        public override ASTNode? TryMultiply(ASTNode number)
        {
            if (Lhs.IsNumeric)
            {
                // [Lhs * Rhs] * number --> [Lhs * number] * Rhs
                ASTNode product = new ASTProduct(Lhs, number);
                ASTNode? normalized = Rhs.TryMultiply(product);
                return normalized ?? new ASTProduct(product, Rhs);
            }
            if (Rhs.IsNumeric)
            {
                // [Lhs * Rhs] * number --> Lhs * [Rhs * number]
                ASTNode product = new ASTProduct(Rhs, number);
                ASTNode? normalized = Lhs.TryMultiply(product);
                return normalized ?? new ASTProduct(Lhs, product);
            }
            return null;
        }

        public override void Bind(UnitType candidate)
        {
            UnitType? L = Lhs.Unit;
            UnitType? R = Rhs.Unit;

            if ((L is not null) && (R is not null))
            {
                // result = unitL * unitR (definition)

                // E.g.: Joule = Newton * Meter
                // Newton.cs (or Meter.cs): 
                //      public static Joule operator *(Newton lhs, Meter rhs) { return new Joule(lhs.Value * rhs.Value); }
                //      public static Joule operator *(Meter lhs, Newton rhs) { return new Joule(lhs.Value * rhs.Value); }
                L.AddOuterOperation(candidate, Operation, L, R);
                if (L.Typename != R.Typename) L.AddOuterOperation(candidate, Operation, R, L);

                // => result / unitL = unitR
                // => result / unitR = unitL
                //
                // Joule.cs: 
                //      public static Newton operator /(Joule lhs, Meter rhs) { return new Newton(lhs.Value / rhs.Value); }
                //      public static Meter operator /(Joule lhs, Newton rhs) { return new Meter(lhs.Value / rhs.Value); }
                candidate.AddOuterOperation(R, "/", candidate, L);
                if (L.Typename != R.Typename) candidate.AddOuterOperation(L, "/", candidate, R);
            }
            else if ((L is not null) && (Rhs.IsNumeric))
            {
                // result = unit * number (conversion)
                //
                // E.g. Centimeter = Meter * 100.0
                // Meter.cs:
                //      public static explicit operator Meter(Centimeter q) { return new Meter((Meter.Factor / Centimeter.Factor) * q.Value); }
                // Centimeter.cs:
                //      public static explicit operator Centimeter(Meter q) { return new Centimeter((Centimeter.Factor / Meter.Factor) * q.Value); }
                //
                Lhs.Bind(candidate);
            }
            else if ((Lhs.IsNumeric) && (R is not null))
            {
                // result = number * unit (conversion)

                // E.g. Centimeter = 100.0 * Meter
                //  Meter.cs:
                //      public static explicit operator Meter(Centimeter q) { return new Meter((Meter.Factor / Centimeter.Factor) * q.Value); }
                //  Centimeter.cs:
                //      public static explicit operator Centimeter(Meter q) { return new Centimeter((Centimeter.Factor / Meter.Factor) * q.Value); }
                //
                Rhs.Bind(candidate);
            }
        }

        public override string ToString() => $"{Lhs}{Operation}{Rhs}";
    }

    internal class ASTQuotient : ASTNode
    {
        public ASTNode Lhs { get; private set; }
        public ASTNode Rhs { get; private set; }

        public override bool IsNumeric => Lhs.IsNumeric && Rhs.IsNumeric;
        public override bool IsWedgeCompatible => 
            (Lhs.IsWedgeCompatible && Rhs.IsWedgeCompatible) ||
            (Lhs.IsNumeric && Rhs.IsWedgeCompatible) ||
            (Lhs.IsWedgeCompatible && Rhs.IsNumeric);

        public ASTQuotient(ASTNode lhs, ASTNode rhs) :
            base()
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public override void Accept(IASTEncoder encoder) { Lhs.Accept(encoder); Rhs.Accept(encoder); encoder.Encode(this); }

        public override ASTNode? TryNormalize() => Rhs.IsNumeric ? Lhs.TryDivide(Rhs) : null;

        // [Lhs / Rhs] / number
        public override ASTNode? TryDivide(ASTNode number)
        {
            if (Lhs.IsNumeric)
            {
                // [Lhs / Rhs] / number --> [Lhs / number] / Rhs
                return new ASTQuotient(new ASTQuotient(Lhs, number), Rhs);
            }
            if (Rhs.IsNumeric)
            {
                // [Lhs / Rhs] / number --> Lhs / [Rhs * number]
                ASTNode product = new ASTParenthesized( new ASTProduct(Rhs, number));
                ASTNode? normalized = Lhs.TryDivide(product);
                return normalized ?? new ASTQuotient(Lhs, product);
            }
            return null;
        }

        // [Lhs / Rhs] * number
        public override ASTNode? TryMultiply(ASTNode number)
        {
            if (Lhs.IsNumeric)
            {
                // [Lhs / Rhs] * number --> [Lhs * number] / Rhs
                return new ASTQuotient(new ASTProduct(Lhs, number), Rhs);
            }
            else if (Rhs.IsNumeric)
            {
                // [Lhs / Rhs] * number --> Lhs * [number / Rhs]
                ASTNode quotient = new ASTQuotient(number, Rhs);
                ASTNode? normalized = Lhs.TryDivide(quotient);
                return normalized ?? new ASTQuotient(Lhs, quotient);
            }
            return null;
        }

        public override void Bind(UnitType candidate)
        {
            UnitType? L = Lhs.Unit;
            UnitType? R = Rhs.Unit;

            if ((L is not null) && (R is not null))
            {
                // result = unitL / unitR (definition)

                // Quotient of the same unit is always included in the list of outer operations,
                // so we have to avoid to enroll it twice:
                if (L.Typename == R.Typename) return;

                // E.g.: MPH = Mile / Hour
                // Mile.cs (or Hour.cs): 
                //      public static MPH operator *(Mile lhs, Hour rhs) { return new MPH(lhs.Value / rhs.Value); }
                L.AddOuterOperation(candidate, "/", L, R);

                // => unitL / result = unitR
                // Mile.cs (or MPH.cs): 
                //      public static Hour operator /(Mile lhs, MPH rhs) { return new Hour(lhs.Value / rhs.Value); }
                L.AddOuterOperation(R, "/", L, candidate);

                // => result * unitR = unitL
                // Hour.cs (or MPH.cs): 
                //      public static Mile operator *(MPH lhs, Hour rhs) { return new Mile(lhs.Value / rhs.Value); }
                candidate.AddOuterOperation(L, "*", candidate, R);
                candidate.AddOuterOperation(L, "*", R, candidate);
            }
            else if ((L is not null) && (Rhs.IsNumeric))
            {
                // result = unit / real (conversion)
                //
                // E.g. Meter = Centimeter / 100.0
                // Meter.cs:
                //      public static explicit operator Meter(Centimeter q) { return new Meter((Meter.Factor / Centimeter.Factor) * q.Value); }
                // Centimeter.cs:
                //      public static explicit operator Centimeter(Meter q) { return new Centimeter((Centimeter.Factor / Meter.Factor) * q.Value); }
                //
                Lhs.Bind(candidate);
            }
            else if ((Lhs.IsNumeric) && (R is not null))
            {
                // result = real / unit (definition)
                AbstractType numType = R.Factor.Value;

                // E.g. Hertz "Hz" = 1.0 / Second
                // Second.cs
                //      public static Hertz operator /(double lhs, Second rhs) { return new Hertz(lhs / rhs.Value); }
                R.AddOuterOperation(candidate, "/", numType, R);

                // Hertz.cs
                //      public static Second operator /(double lhs, Hertz rhs) { return new Second(lhs / rhs.Value); }
                candidate.AddOuterOperation(R, "/", numType, candidate);

                // Hertz.cs (or Second.cs)
                //      public static double operator *(Hertz lhs, Second rhs) { return lhs.Value * rhs.Value; }
                //      public static double operator *(Second lhs, Hertz rhs) { return lhs.Value * rhs.Value; }
                candidate.AddOuterOperation(numType, "*", candidate, R);
                candidate.AddOuterOperation(numType, "*", R, candidate);
            }
        }

        public override string ToString() => $"{Lhs}/{Rhs}";
    }

    internal class ASTSum : ASTNode
    {
        public ASTNode Lhs { get; private set; }
        public ASTNode Rhs { get; private set; }

        public override bool IsNumeric => Lhs.IsNumeric && Rhs.IsNumeric;

        public ASTSum(ASTNode lhs, ASTNode rhs) :
            base()
        {
            Lhs = lhs;
            Rhs = rhs;
        }
        public override void Accept(IASTEncoder encoder) { Lhs.Accept(encoder); Rhs.Accept(encoder); encoder.Encode(this); }

        public override string ToString() => $"{Lhs}+{Rhs}";
    }

    internal class ASTDifference : ASTNode
    {
        public ASTNode Lhs { get; private set; }
        public ASTNode Rhs { get; private set; }

        public override bool IsNumeric => Lhs.IsNumeric && Rhs.IsNumeric;

        public ASTDifference(ASTNode lhs, ASTNode rhs) :
            base()
        {
            Lhs = lhs;
            Rhs = rhs;
        }
        public override void Accept(IASTEncoder encoder) { Lhs.Accept(encoder); Rhs.Accept(encoder); encoder.Encode(this); }

        public override string ToString() => $"{Lhs}-{Rhs}";
    }
}
        