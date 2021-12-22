<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="text" version="1.0" encoding="UTF-8" indent="yes"/>

<xsl:template match="scale">
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
<xsl:variable name="LEVEL">
<!--
  Compile-time scales can access (internally) m_level field of other compile-time units.
  Late units can access it via Level property only.
-->
  <xsl:choose>
    <xsl:when test="@late='yes'">
      <xsl:text>Level</xsl:text>
    </xsl:when>
    <xsl:otherwise>
      <xsl:text>m_level</xsl:text>
    </xsl:otherwise>
  </xsl:choose>
</xsl:variable>
namespace <xsl:value-of select="@ns"/>
{
    using System;

    <xsl:if test="string-length(refpoint)>0">[ScaleReferencePoint("<xsl:value-of select="refpoint"/>")]</xsl:if>
    public partial struct <xsl:value-of select="@name"/> :
        ILevel&lt;<xsl:value-of select="valuetype/name"/>&gt;,
        IEquatable&lt;<xsl:value-of select="@name"/>&gt;,
        IComparable&lt;<xsl:value-of select="@name"/>&gt;,
        IFormattable
    {
        #region Constants
        // offset (dimensionless) to the "<xsl:value-of select="refpoint/@normalized"/>" family common reference point
        public const <xsl:value-of select="valuetype/name"/> OFFSET = <xsl:value-of select="offset"/>;
        #endregion
    
        #region Fields
        internal readonly <xsl:value-of select="@unit"/> m_level;
        #endregion

        #region Properties / ILevel&lt;<xsl:value-of select="valuetype/name"/>&gt;
        public <xsl:value-of select="@unit"/> Level =&gt; m_level;

        IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt; ILevel&lt;<xsl:value-of select="valuetype/name"/>&gt;.Level =&gt; m_level;
        IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt; ILevel&lt;<xsl:value-of select="valuetype/name"/>&gt;.ConvertibleLevel =&gt; m_level - Offset;
        Scale&lt;<xsl:value-of select="valuetype/name"/>&gt; ILevel&lt;<xsl:value-of select="valuetype/name"/>&gt;.Scale =&gt; Proxy;
        #endregion

        #region Constructor(s)
        public <xsl:value-of select="@name"/>(<xsl:value-of select="@unit"/> level) =&gt; m_level = level;
        public static explicit operator <xsl:value-of select="@name"/>(<xsl:value-of select="@unit"/> q) =&gt; new(q);
        
        public <xsl:value-of select="@name"/>(<xsl:value-of select="valuetype/name"/> level) : this(new <xsl:value-of select="@unit"/>(level)) { }
        public static explicit operator <xsl:value-of select="@name"/>(<xsl:value-of select="valuetype/name"/> q) =&gt; new(q);
        #endregion

        #region Conversions
        // dimensionless:
        <xsl:for-each select="family/relative">public static <xsl:value-of select="../../valuetype/name"/> From<xsl:value-of select="."/>(<xsl:value-of select="../../valuetype/name"/> q) =&gt; <xsl:value-of select="../../@unit"/>.From<xsl:value-of select="@unit"/>(q - <xsl:value-of select="."/>.OFFSET) + OFFSET;
        </xsl:for-each>
        // dimensional:
        <!-- Conversion via cast expression is risky: may be misinterpreted as a constructor expression and will not be made!!!
        <xsl:for-each select="family/relative">// public static explicit operator <xsl:value-of select="../../@name"/>(<xsl:value-of select="."/> q) =&gt; new(From<xsl:value-of select="."/>(q.<xsl:value-of select="$LEVEL"/>.<xsl:value-of select="$VALUE"/>));
        </xsl:for-each>
        -->
        <xsl:for-each select="family/relative">public static <xsl:value-of select="../../@name"/> From(<xsl:value-of select="."/> q) =&gt; new(From<xsl:value-of select="."/>(q.<xsl:value-of select="$LEVEL"/>.<xsl:value-of select="$VALUE"/>));
        </xsl:for-each>
        public static <xsl:value-of select="@name"/> From(ILevel&lt;<xsl:value-of select="valuetype/name"/>&gt; q)
        {
            return q.Scale.Family == Family ?
                new <xsl:value-of select="@name"/>(<xsl:value-of select="@unit"/>.From(q.ConvertibleLevel) + Offset) :
                throw new InvalidOperationException($"Cannot convert \"{q.GetType().Name}\" to \"<xsl:value-of select="@name"/>\".");
        }

        public static <xsl:value-of select="@name"/> From(IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt; q)
        {
            Scale&lt;<xsl:value-of select="valuetype/name"/>&gt;? scale = Catalog.Scale(Family, q.Unit);
            return scale is not null ?
                From(scale.From(q.Value)) :
                throw new InvalidOperationException($"Cannot convert \"{q.GetType().Name}\" to \"<xsl:value-of select="@name"/>\".");
        }
        #endregion

        #region Equality / IEquatable&lt;<xsl:value-of select="@name"/>&gt;
        public override int /* Object */ GetHashCode() =&gt; m_level.GetHashCode();
        public override bool /* Object */ Equals(object? obj) =&gt; (obj is <xsl:value-of select="@name"/> level) &amp;&amp; Equals(level);
        public bool /* IEquatable&lt;<xsl:value-of select="@name"/>&gt; */ Equals(<xsl:value-of select="@name"/> other) =&gt; m_level == other.<xsl:value-of select="$LEVEL"/>;
        #endregion

        #region Comparison / IComparable&lt;<xsl:value-of select="@name"/>&gt;
        public static bool operator ==(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$LEVEL"/> == rhs.<xsl:value-of select="$LEVEL"/>;
        public static bool operator !=(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$LEVEL"/> != rhs.<xsl:value-of select="$LEVEL"/>;
        public static bool operator &lt;(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$LEVEL"/> &lt; rhs.<xsl:value-of select="$LEVEL"/>;
        public static bool operator &gt;(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$LEVEL"/> &gt; rhs.<xsl:value-of select="$LEVEL"/>;
        public static bool operator &lt;=(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$LEVEL"/> &lt;= rhs.<xsl:value-of select="$LEVEL"/>;
        public static bool operator &gt;=(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$LEVEL"/> &gt;= rhs.<xsl:value-of select="$LEVEL"/>;
        public int /* IComparable&lt;<xsl:value-of select="@name"/>&gt; */ CompareTo(<xsl:value-of select="@name"/> other) =&gt; m_level.CompareTo(other.<xsl:value-of select="$LEVEL"/>);
        #endregion

        #region Arithmetic
        public static <xsl:value-of select="@name"/> operator +(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@unit"/> rhs) =&gt; new(lhs.<xsl:value-of select="$LEVEL"/> + rhs);
        public static <xsl:value-of select="@name"/> operator +(<xsl:value-of select="@unit"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; new(lhs + rhs.<xsl:value-of select="$LEVEL"/>);
        public static <xsl:value-of select="@name"/> operator -(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@unit"/> rhs) =&gt; new(lhs.<xsl:value-of select="$LEVEL"/> - rhs);
        public static <xsl:value-of select="@unit"/> operator -(<xsl:value-of select="@name"/> lhs, <xsl:value-of select="@name"/> rhs) =&gt; lhs.<xsl:value-of select="$LEVEL"/> - rhs.<xsl:value-of select="$LEVEL"/>;
        public static <xsl:value-of select="@name"/> operator -(<xsl:value-of select="@name"/> q) =&gt; new(-q.<xsl:value-of select="$LEVEL"/>);
        public static <xsl:value-of select="@name"/> operator ++(<xsl:value-of select="@name"/> q) =&gt; new(q.<xsl:value-of select="$LEVEL"/>.<xsl:value-of select="$VALUE"/> + <xsl:value-of select="valuetype/one"/>);
        public static <xsl:value-of select="@name"/> operator --(<xsl:value-of select="@name"/> q) =&gt; new(q.<xsl:value-of select="$LEVEL"/>.<xsl:value-of select="$VALUE"/> - <xsl:value-of select="valuetype/one"/>);
        #endregion

        #region Formatting
        public static string String(<xsl:value-of select="valuetype/name"/> level, string? format = null, IFormatProvider? fp = null) =&gt;
            <xsl:value-of select="@unit"/>.String(level, format ?? <xsl:value-of select="@name"/>.Format, fp);

        public override string ToString() =&gt; String(m_level.m_value);
        public string ToString(string format) =&gt; String(m_level.m_value, format);
        public string ToString(IFormatProvider fp) =&gt; String(m_level.m_value, null, fp);
        public string /* IFormattable */ ToString(string? format, IFormatProvider? fp) =&gt; String(m_level.m_value, format, fp);
        #endregion

        #region Static fields and properties (DO NOT CHANGE!)
        public const int Family = <xsl:value-of select="family/@id"/>;
        public static string Format { get; set; } = "<xsl:value-of select="format"/>";
        public static readonly <xsl:value-of select="@unit"/> Offset = new(OFFSET);
        public static readonly Scale&lt;<xsl:value-of select="valuetype/name"/>&gt; Proxy = new <xsl:value-of select="@name"/>_Proxy();
        #endregion
        <!--
        #region Predefined levels
        public static readonly <xsl:value-of select="@name"/> Zero = new(<xsl:value-of select="valuetype/zero"/>);
        #endregion
        -->
    }

    public partial class <xsl:value-of select="@name"/>_Proxy : Scale&lt;<xsl:value-of select="valuetype/name"/>&gt;
    {
        #region Properties
        public override int Family =&gt; <xsl:value-of select="@name"/>.Family;
        public override string Format { get =&gt; <xsl:value-of select="@name"/>.Format; set { <xsl:value-of select="@name"/>.Format = value; } }
        public override IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt; Offset =&gt; <xsl:value-of select="@name"/>.Offset;
        public override Unit Unit =&gt; <xsl:value-of select="@unit"/>.Proxy;
        #endregion

        #region Constructor(s)
        public <xsl:value-of select="@name"/>_Proxy() :
            base(typeof(<xsl:value-of select="@name"/>))
        {
        }
        #endregion

        #region Methods
        public override ILevel&lt;<xsl:value-of select="valuetype/name"/>&gt; From(<xsl:value-of select="valuetype/name"/> value) =&gt; new <xsl:value-of select="@name"/>(value);
        public override ILevel&lt;<xsl:value-of select="valuetype/name"/>&gt; From(ILevel&lt;<xsl:value-of select="valuetype/name"/>&gt; level) =&gt; <xsl:value-of select="@name"/>.From(level);
        public override ILevel&lt;<xsl:value-of select="valuetype/name"/>&gt; From(IQuantity&lt;<xsl:value-of select="valuetype/name"/>&gt; quantity) =&gt; <xsl:value-of select="@name"/>.From(quantity);
        #endregion
    }
}
</xsl:template>
</xsl:stylesheet>
