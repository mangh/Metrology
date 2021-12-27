<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="text" version="1.0" encoding="UTF-8" indent="yes"/>

<xsl:template match="unit">
/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology

<!--<xsl:value-of select="@tm"/>-->
********************************************************************************/
<xsl:variable name="VALUE">
<!--
  Compile-time units can access (internally) m_value field of other compile-time units.
  Late units can access it via Value property only.
-->
  <xsl:choose>
    <xsl:when test="@late='yes'">
      <xsl:text>Value</xsl:text>
    </xsl:when>
    <xsl:otherwise>
      <xsl:text>m_value</xsl:text>
    </xsl:otherwise>
  </xsl:choose>
</xsl:variable>
namespace <xsl:value-of select="@ns"/>
{
    using Mangh.Metrology;
    using System;

    public partial struct <xsl:value-of select="@name"/> :
        IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt;,
        IEquatable&lt;<xsl:value-of select="@name"/>&gt;,
        IComparable&lt;<xsl:value-of select="@name"/>&gt;,
        IFormattable
    {
        #region Fields
        internal readonly <xsl:value-of select="valuetype/name"/> m_value;
        #endregion

        #region Properties / IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt;
        public <xsl:value-of select="valuetype/name"/> Value =&gt; m_value;
        Unit&lt;<xsl:value-of select="valuetype/name"/>&gt; IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt;.Unit =&gt; Proxy;
        #endregion

        #region Constructor(s)
        public <xsl:value-of select="@name"/>(<xsl:value-of select="valuetype/name"/> value) =&gt; m_value = value;
        public static explicit operator <xsl:value-of select="@name"/>(<xsl:value-of select="valuetype/name"/> q) =&gt; new(q);
        #endregion

        #region Conversions
        // dimensionless:
        <xsl:for-each select="family/relative">public static <xsl:value-of select="../../valuetype/name"/> From<xsl:value-of select="."/>(<xsl:value-of select="../../valuetype/name"/> q) =&gt; (Factor / <xsl:value-of select="."/>.Factor) * q;
        </xsl:for-each>
        // dimensional:
        <!-- Conversion via cast expression is risky: may be misinterpreted as a constructor expression and will not be made!!!
        <xsl:for-each select="family/relative">// public static explicit operator <xsl:value-of select="../../@name"/>(<xsl:value-of select="."/> q) =&gt; new(From<xsl:value-of select="."/>(q.<xsl:value-of select="$VALUE"/>));
        </xsl:for-each>
        -->
        <xsl:for-each select="family/relative">public static <xsl:value-of select="../../@name"/> From(<xsl:value-of select="."/> q) =&gt; new(From<xsl:value-of select="."/>(q.<xsl:value-of select="$VALUE"/>));
        </xsl:for-each>
        public static <xsl:value-of select="@name"/> From(IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt; q)
        {
            return q.Unit.Family == Family ?
                new <xsl:value-of select="@name"/>((Factor / q.Unit.Factor) * q.Value) :
                throw new InvalidOperationException($"Cannot convert \"{q.GetType().Name}\" to \"<xsl:value-of select="@name"/>\"");
        }
        #endregion

        #region Equality / IEquatable&lt;<xsl:value-of select="@name"/>&gt;
        public override int /* Object */ GetHashCode() =&gt; m_value.GetHashCode();
        public override bool /* Object */ Equals(object? obj) =&gt; (obj is <xsl:value-of select="@name"/> q) &amp;&amp; Equals(q);
        public bool /* IEquatable&lt;<xsl:value-of select="@name"/>&gt; */ Equals(<xsl:value-of select="@name"/> other) =&gt; m_value == other.<xsl:value-of select="$VALUE"/>;
        #endregion

        #region Comparison / IComparable&lt;<xsl:value-of select="@name"/>&gt;
        public static bool operator ==(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$VALUE"/> == rhs.<xsl:value-of select="$VALUE"/>;
        public static bool operator !=(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$VALUE"/> != rhs.<xsl:value-of select="$VALUE"/>;
        public static bool operator &lt;(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$VALUE"/> &lt; rhs.<xsl:value-of select="$VALUE"/>;
        public static bool operator &gt;(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$VALUE"/> &gt; rhs.<xsl:value-of select="$VALUE"/>;
        public static bool operator &lt;=(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$VALUE"/> &lt;= rhs.<xsl:value-of select="$VALUE"/>;
        public static bool operator &gt;=(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$VALUE"/> &gt;= rhs.<xsl:value-of select="$VALUE"/>;
        public int /* IComparable&lt;<xsl:value-of select="@name"/>&gt; */ CompareTo(<xsl:value-of select="@name"/> other) =&gt; m_value.CompareTo(other.<xsl:value-of select="$VALUE"/>);
        #endregion

        #region Arithmetic
        // Inner:
        public static <xsl:value-of select="@name"/> operator +(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; new(lhs.<xsl:value-of select="$VALUE"/> + rhs.<xsl:value-of select="$VALUE"/>);
        public static <xsl:value-of select="@name"/> operator -(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; new(lhs.<xsl:value-of select="$VALUE"/> - rhs.<xsl:value-of select="$VALUE"/>);
        public static <xsl:value-of select="@name"/> operator ++(<xsl:value-of select="@name"/> q) =&gt; new(q.<xsl:value-of select="$VALUE"/> + <xsl:value-of select="valuetype/one"/>);
        public static <xsl:value-of select="@name"/> operator --(<xsl:value-of select="@name"/> q) =&gt; new(q.<xsl:value-of select="$VALUE"/> - <xsl:value-of select="valuetype/one"/>);
        public static <xsl:value-of select="@name"/> operator -(<xsl:value-of select="@name"/> q) =&gt; new(-q.<xsl:value-of select="$VALUE"/>);
        public static <xsl:value-of select="@name"/> operator *(<xsl:value-of select="valuetype/name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; new(lhs * rhs.<xsl:value-of select="$VALUE"/>);
        public static <xsl:value-of select="@name"/> operator *(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="valuetype/name"/> rhs) =&gt; new(lhs.<xsl:value-of select="$VALUE"/> * rhs);
        public static <xsl:value-of select="@name"/> operator /(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="valuetype/name"/> rhs) =&gt; new(lhs.<xsl:value-of select="$VALUE"/> / rhs);
        public static <xsl:value-of select="valuetype/name"/> operator /(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$VALUE"/> / rhs.<xsl:value-of select="$VALUE"/>;
        <xsl:if test="count(outer/operation)>0">
        <xsl:text>// Outer:</xsl:text>
        <xsl:apply-templates select="outer/operation">
          <xsl:with-param name="VALUE" select="$VALUE"/>
        </xsl:apply-templates>
        </xsl:if>
        #endregion

        #region Formatting
        public static string String(<xsl:value-of select="valuetype/name"/> q, string? format = null, IFormatProvider? fp = null)
            =&gt; string.Format(fp, format ?? Format, q, Symbol.Default);

        public override string ToString() =&gt; String(m_value);
        public string ToString(string format) =&gt; String(m_value, format);
        public string ToString(IFormatProvider fp) =&gt; String(m_value, null, fp);
        public string /* IFormattable */ ToString(string? format, IFormatProvider? fp) =&gt; String(m_value, format, fp);
        #endregion

        #region Static fields and properties (DO NOT CHANGE!)
        public static readonly Dimension Sense = <xsl:value-of select="sense"/>;
        public const int Family = <xsl:value-of select="family/@id"/>;
        public static readonly SymbolCollection Symbol = new(<xsl:for-each select="tags/tag">&quot;<xsl:value-of select="."/>&quot;<xsl:if test="not(position()=last())"><xsl:text>, </xsl:text></xsl:if></xsl:for-each>);
        public static readonly Unit&lt;<xsl:value-of select="valuetype/name"/>&gt; Proxy = new <xsl:value-of select="@name"/>_Proxy();
        <xsl:choose>
        <xsl:when test="@monetary='yes'">public static <xsl:value-of select="valuetype/name"/> Factor { get; set; } = <xsl:value-of select="factor"/>;</xsl:when>
        <xsl:otherwise>public const <xsl:value-of select="valuetype/name"/> Factor = <xsl:value-of select="factor"/>;</xsl:otherwise>
        </xsl:choose>
        public static string Format { get; set; } = "<xsl:value-of select="format"/>";
        #endregion
        <!--
        #region Predefined quantities
        public static readonly <xsl:value-of select="@name"/> One = new(<xsl:value-of select="valuetype/one"/>);
        public static readonly <xsl:value-of select="@name"/> Zero = new(<xsl:value-of select="valuetype/zero"/>);
        #endregion
        -->
    }

    public partial class <xsl:value-of select="@name"/>_Proxy : Unit&lt;<xsl:value-of select="valuetype/name"/>&gt;
    {
        #region Properties
        public override Dimension Sense =&gt; <xsl:value-of select="@name"/>.Sense;
        public override int Family =&gt; <xsl:value-of select="@name"/>.Family;
        <xsl:choose>
        <xsl:when test="@monetary='yes'">public override <xsl:value-of select="valuetype/name"/> Factor { get { return <xsl:value-of select="@name"/>.Factor; } set { <xsl:value-of select="@name"/>.Factor = value; } }</xsl:when>
        <xsl:otherwise>public override <xsl:value-of select="valuetype/name"/> Factor =&gt; <xsl:value-of select="@name"/>.Factor;</xsl:otherwise>
        </xsl:choose>
        public override SymbolCollection Symbol =&gt; <xsl:value-of select="@name"/>.Symbol;
        public override string Format { get { return <xsl:value-of select="@name"/>.Format; } set { <xsl:value-of select="@name"/>.Format = value; } }
        #endregion

        #region Constructor(s)
        public <xsl:value-of select="@name"/>_Proxy() :
            base(typeof(<xsl:value-of select="@name"/>))
        {
        }
        #endregion

        #region Methods
        public override IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt; From(<xsl:value-of select="valuetype/name"/> value) =&gt; new <xsl:value-of select="@name"/>(value);
        public override IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt; From(IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt; quantity) =&gt; <xsl:value-of select="@name"/>.From(quantity);
        #endregion
    }
}
</xsl:template>

<!-- Outer operations -->
<xsl:template match="operation">
  <xsl:param name="VALUE" />

  <!-- Builtin types have no $VALUE field -->
  <xsl:variable name="LHS">
    <xsl:choose>
      <xsl:when test="lhs/@builtin='yes'">
        <xsl:text>lhs</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>lhs.</xsl:text><xsl:value-of select="$VALUE"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- Builtin types have no $VALUE field -->
  <xsl:variable name="RHS">
    <xsl:choose>
      <xsl:when test="rhs/@builtin='yes'">
        <xsl:text>rhs</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>rhs.</xsl:text><xsl:value-of select="$VALUE"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- "*" implements "^" operator  -->
  <xsl:variable name="OPERATOR">
    <xsl:choose>
      <xsl:when test="@op = '^'">
        <xsl:text>*</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="@op"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="EXPRESSION">
    <xsl:value-of select="$LHS"/><xsl:text> </xsl:text><xsl:value-of select="$OPERATOR"/><xsl:text> </xsl:text><xsl:value-of select="$RHS"/>
  </xsl:variable>
  
  <!-- operator declaration -->
  <xsl:text>
        public static </xsl:text><xsl:value-of select="ret"/>
  <xsl:text> operator </xsl:text><xsl:value-of select="@op"/>
  <xsl:text>(</xsl:text><xsl:value-of select="lhs"/><xsl:text> lhs</xsl:text>
  <xsl:text>, </xsl:text><xsl:value-of select="rhs"/><xsl:text> rhs</xsl:text>
  <xsl:text>) =&gt; </xsl:text>
  <!-- operator body -->
  <xsl:choose>
    <xsl:when test="ret/@builtin='yes'">
      <xsl:value-of select="$EXPRESSION"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:text>new(</xsl:text>
      <xsl:value-of select="$EXPRESSION"/>
      <xsl:text>)</xsl:text>
    </xsl:otherwise>
  </xsl:choose>
  <xsl:text>;</xsl:text>
</xsl:template>

</xsl:stylesheet>
