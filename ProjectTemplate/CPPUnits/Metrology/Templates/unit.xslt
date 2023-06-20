<?xml version="1.0"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  
  <xsl:output method="text" version="1.0" encoding="UTF-8" indent="yes"/>
  <xsl:include href="math-constants.xslt"/>

  <xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
  <xsl:variable name="TT" select="'&lt;T&gt;'" />

  <xsl:template match="unit">
  <xsl:variable name="Unit" select="@name" />
  <xsl:variable name="_Unit" select="concat('_', @name)" />
  <xsl:variable name="_UnitT" select="concat($_Unit, $TT)" />
  <xsl:variable name="UNIT" select="translate(@name, $smallcase, $uppercase)" />
/*******************************************************************************

    Units of Measurement for C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology

    <!--<xsl:value-of select="@tm"/>-->
********************************************************************************/
#ifndef <xsl:value-of select="$UNIT"/>_H
#define <xsl:value-of select="$UNIT"/>_H

#include &lt;ostream&gt;
#include "detail/to_string.h"

namespace <xsl:value-of select="@ns"/>
{
    ///////////////////////////////////////////////////////////////////
    ///
    ///     Relationships
    ///

    <!-- relatives -->
    <xsl:for-each select="family/relative">
      <xsl:text>/*family*/ template&lt;typename T&gt; struct </xsl:text>
      <xsl:value-of select="concat('_', .)"/>
      <xsl:text>;
    </xsl:text>
    </xsl:for-each>
    <!-- fellows -->
    <xsl:for-each select="fellow/operation/lhs|fellow/operation/rhs|fellow/operation/ret">
      <xsl:if test="not(.=$Unit) and not(@builtin='yes') and not(.=../preceding-sibling::*/lhs) and not(.=../preceding-sibling::*/rhs) and not(.=../preceding-sibling::*/ret)">
        <xsl:text>/*fellow*/ template&lt;typename T&gt; struct </xsl:text>
        <xsl:value-of select="concat('_', .)"/>
        <xsl:text>;
    </xsl:text>
      </xsl:if>
    </xsl:for-each>
    template&lt;typename T = <xsl:value-of select="valuetype/@keywd"/>&gt;
    struct <xsl:value-of select="$_Unit"/>
    {
        ///////////////////////////////////////////////////////////////////
        ///
        ///     Statics
        ///

        static constexpr int family{ <xsl:value-of select="family/@id"/>
          <xsl:text> };</xsl:text><xsl:if test="family/@prime">
            <xsl:text> // </xsl:text>
            <xsl:value-of select="family/@prime"/>
          </xsl:if>
          <xsl:text>
        </xsl:text>
        <xsl:choose>
          <xsl:when test="@monetary='yes'">
            <xsl:text>inline static </xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>static constexpr </xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>T factor{ </xsl:text><xsl:call-template name="apply-math-constants">
          <xsl:with-param name="where" select="factor"/>
        </xsl:call-template>
        <xsl:text> }; // </xsl:text>
        <xsl:value-of select="factor/@expr"/>
        static constexpr const char* symbol[] { <xsl:for-each select="tags/tag">
          <xsl:text>&quot;</xsl:text>
          <xsl:value-of select="."/>
          <xsl:text>&quot;</xsl:text>
          <xsl:if test="not(position()=last())">
            <xsl:text>, </xsl:text>
          </xsl:if>
        </xsl:for-each>, nullptr };
        static constexpr const char* format{ <xsl:text>&quot;</xsl:text><xsl:value-of select="format"/><xsl:text>&quot; }</xsl:text>;

        ///////////////////////////////////////////////////////////////////
        ///
        ///     Constructors
        ///

        /// @brief Default constructor.
        <xsl:value-of select="$_Unit"/>() = default;

        /// @brief Copy constructor.
        constexpr <xsl:value-of select="$_Unit"/>(const <xsl:value-of select="$_Unit"/>&amp; source)
            : <xsl:value-of select="$_Unit"/>(source.m_value)
        {}

        /// @brief Converting constructor: T -> <xsl:value-of select="$_UnitT"/>.
        explicit constexpr <xsl:value-of select="$_Unit"/>(T value)
            : m_value(value)
        {}

        /// @brief Copy-assignment.
        <xsl:value-of select="$_Unit"/>&amp; operator=(const <xsl:value-of select="$_Unit"/>&amp; rhs)
        {
            if (this != &amp;rhs) m_value = rhs.m_value;
            return *this;
        }

        ///////////////////////////////////////////////////////////////////
        ///
        ///     Conversions
        ///

        <xsl:for-each select="family/relative">
          <xsl:text>explicit </xsl:text><xsl:value-of select="$_Unit"/><xsl:text>(const </xsl:text>
          <xsl:variable name="relative" select="concat(concat('_', .), $TT)" />
          <xsl:value-of select="$relative"/>
          <xsl:text> &amp; q) : </xsl:text>
          <xsl:value-of select="$_Unit"/>
          <xsl:text>((factor / </xsl:text>
          <xsl:value-of select="$relative"/>
          <xsl:text>::factor) * q.value()) {}
        </xsl:text>
        </xsl:for-each>
        ///////////////////////////////////////////////////////////////////
        ///
        ///     Comparison
        ///

        friend bool operator==(const <xsl:value-of select="$_Unit"/>&amp; lhs, const <xsl:value-of select="$_Unit"/>&amp; rhs) { return lhs.m_value == rhs.m_value; }
        friend bool operator&lt; (const <xsl:value-of select="$_Unit"/>&amp; lhs, const <xsl:value-of select="$_Unit"/>&amp; rhs) { return lhs.m_value &lt; rhs.m_value; }
        friend bool operator&gt; (const <xsl:value-of select="$_Unit"/>&amp; lhs, const <xsl:value-of select="$_Unit"/>&amp; rhs) { return lhs.m_value &gt; rhs.m_value; }
        friend bool operator!=(const <xsl:value-of select="$_Unit"/>&amp; lhs, const <xsl:value-of select="$_Unit"/>&amp; rhs) { return !(lhs == rhs); }
        friend bool operator&lt;=(const <xsl:value-of select="$_Unit"/>&amp; lhs, const <xsl:value-of select="$_Unit"/>&amp; rhs) { return !(lhs &gt; rhs); }
        friend bool operator&gt;=(const <xsl:value-of select="$_Unit"/>&amp; lhs, const <xsl:value-of select="$_Unit"/>&amp; rhs) { return !(lhs &lt; rhs); }
            
        ///////////////////////////////////////////////////////////////////
        ///
        ///     Arithmetic
        ///

        <xsl:value-of select="$_Unit"/>&amp; operator*=(T rhs) { m_value *= rhs; return *this; }
        <xsl:value-of select="$_Unit"/>&amp; operator/=(T rhs) { m_value /= rhs; return *this; }
        <xsl:value-of select="$_Unit"/>&amp; operator+=(const <xsl:value-of select="$_Unit"/>&amp; rhs) { m_value += rhs.m_value; return *this; }
        <xsl:value-of select="$_Unit"/>&amp; operator-=(const <xsl:value-of select="$_Unit"/>&amp; rhs) { m_value -= rhs.m_value; return *this; }

        <xsl:value-of select="$_Unit"/>&amp; operator++() { ++m_value; return *this; }
        <xsl:value-of select="$_Unit"/>  operator++(int) { <xsl:value-of select="$_Unit"/> old = *this; operator++(); return old; }
        <xsl:value-of select="$_Unit"/>&amp; operator--() { --m_value; return *this; }
        <xsl:value-of select="$_Unit"/>  operator--(int) { <xsl:value-of select="$_Unit"/> old = *this; operator--(); return old; }

        friend <xsl:value-of select="$_Unit"/> operator-(<xsl:value-of select="$_Unit"/> lhs, const <xsl:value-of select="$_Unit"/>&amp; rhs) { lhs -= rhs; return lhs; }
        friend <xsl:value-of select="$_Unit"/> operator+(<xsl:value-of select="$_Unit"/> lhs, const <xsl:value-of select="$_Unit"/>&amp; rhs) { lhs += rhs; return lhs; }
        friend <xsl:value-of select="$_Unit"/> operator-(const <xsl:value-of select="$_Unit"/>&amp; lhs) { return <xsl:value-of select="$_Unit"/>{ -lhs.m_value }; }
        friend <xsl:value-of select="$_Unit"/> operator *(T lhs, <xsl:value-of select="$_Unit"/> rhs) { rhs *= lhs; return rhs; }
        friend <xsl:value-of select="$_Unit"/> operator *(<xsl:value-of select="$_Unit"/> lhs, T rhs) { lhs *= rhs; return lhs; }
        friend <xsl:value-of select="$_Unit"/> operator /(<xsl:value-of select="$_Unit"/> lhs, T rhs) { lhs /= rhs; return lhs; }
        friend T operator /(const <xsl:value-of select="$_Unit"/>&amp; lhs, const <xsl:value-of select="$_Unit"/>&amp; rhs) { return lhs.m_value / rhs.m_value; }

        ///////////////////////////////////////////////////////////////////
        ///
        ///     Formatting
        ///

        /// @brief Extraction operator (sends quantity and its primary unit to the output stream).
        friend std::ostream&amp; operator&lt;&lt;(std::ostream&amp; os, const <xsl:value-of select="$_Unit"/>&amp; q)
        {
            os &lt;&lt; q.m_value &lt;&lt; " " &lt;&lt; symbol[0];
            return os;
        }

        /// @brief Converts <xsl:value-of select="$_UnitT"/> quantity to a text form.
        /// @param format - formatting string (default <xsl:value-of select="$_UnitT"/>::format).
        /// @returns quantity string with <xsl:value-of select="$_UnitT"/> unit applied.
        friend std::string to_string(<xsl:value-of select="$_Unit"/> quantity, const char* format = nullptr)
        {
            return to_string(quantity.m_value, symbol[0], format ? format : <xsl:value-of select="$_UnitT"/>::format);
        }

        ///////////////////////////////////////////////////////////////////
        ///
        ///     Properties
        ///

        T value() const { return m_value; }

    private:

        ///////////////////////////////////////////////////////////////////
        ///
        ///     Fields
        ///

        T m_value;
    };

    ///////////////////////////////////////////////////////////////////
    ///
    ///     Fellow-related arithmetic
    ///
    <xsl:apply-templates select="fellow/operation" />

    ///////////////////////////////////////////////////////////////////
    ///
    ///     Type-alias for the unit
    ///

    using <xsl:value-of select="$Unit"/> = <xsl:value-of select="$_Unit"/>&lt;<xsl:value-of select="valuetype/@keywd"/>&gt;;
}

#endif /* !<xsl:value-of select="$UNIT"/>_H */
</xsl:template>

  <xsl:template match="operation">
    <!--declaration--> 
    <xsl:text>
    template&lt;typename T&gt; </xsl:text>
    <xsl:call-template name="operand"><xsl:with-param name="node" select="ret" /></xsl:call-template>
    <xsl:text> operator</xsl:text>
    <xsl:value-of select="@op"/>
    <xsl:text>(const </xsl:text>
    <xsl:call-template name="operand"><xsl:with-param name="node" select="lhs" /></xsl:call-template>
    <xsl:text>&amp; lhs</xsl:text>
    <xsl:text>, const </xsl:text>
    <xsl:call-template name="operand"><xsl:with-param name="node" select="rhs" /></xsl:call-template>
    <xsl:text>&amp; rhs)</xsl:text>
    <!--body--> 
    <xsl:text> { return </xsl:text>
    <xsl:call-template name="operand"><xsl:with-param name="node" select="ret" /></xsl:call-template>
    <xsl:text>(</xsl:text>
    <xsl:call-template name="argument"><xsl:with-param name="node" select="lhs" /></xsl:call-template>
    <xsl:text> </xsl:text>
    <xsl:choose>
      <xsl:when test="@op='^'">
        <xsl:text>*</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="@op"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text> </xsl:text>
    <xsl:call-template name="argument"><xsl:with-param name="node" select="rhs" /></xsl:call-template>
    <xsl:text>); }</xsl:text>
  </xsl:template>

  <xsl:template name="operand">
    <xsl:param name="node" />
    <xsl:choose>
      <xsl:when test="not($node/@builtin='yes')">
        <xsl:value-of select="concat(concat('_', $node), $TT)" />
      </xsl:when>
      <xsl:when test="$node=/unit/valuetype/@keywd">
        <xsl:value-of select="'T'" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$node" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="argument">
    <xsl:param name="node" />
    <xsl:value-of select="name($node)" />
    <xsl:if test="not($node/@builtin='yes')">
      <xsl:text>.value()</xsl:text>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
