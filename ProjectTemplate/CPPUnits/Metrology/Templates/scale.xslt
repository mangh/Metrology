<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="text" version="1.0" encoding="UTF-8" indent="yes"/>
<xsl:include href="math-constants.xslt"/>

<xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
<xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />
<xsl:variable name="TT" select="'&lt;T&gt;'" />
  
<xsl:template match="scale">
<xsl:variable name="Scale" select="@name" />
<xsl:variable name="_Scale" select="concat('_', @name)" />
<xsl:variable name="_ScaleT" select="concat($_Scale, $TT)" />
<xsl:variable name="SCALE" select="translate(@name, $smallcase, $uppercase)" />
<xsl:variable name="Unit" select="@unit" />
<xsl:variable name="_Unit" select="concat('_', @unit)" />
<xsl:variable name="_UnitT" select="concat($_Unit, $TT)" />
/*******************************************************************************

    Units of Measurement for C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology

<!--<xsl:value-of select="@tm"/>-->
********************************************************************************/
#ifndef <xsl:value-of select="$SCALE"/>_H
#define <xsl:value-of select="$SCALE"/>_H

#include "<xsl:value-of select="$Unit"/>.h"

namespace <xsl:value-of select="@ns"/>
{
    ///////////////////////////////////////////////////////////////////
    ///
    ///     Relationships
    ///

    <!-- relatives -->
    <xsl:for-each select="family/relative">
      <xsl:text>/*family*/ template&lt;typename T&gt; struct </xsl:text><xsl:value-of select="concat('_', .)"/><xsl:text>;
    </xsl:text>
    </xsl:for-each>

    template&lt;typename T = <xsl:value-of select="valuetype/@keywd"/>&gt;
    struct <xsl:value-of select="$_Scale"/>
    {
        ///////////////////////////////////////////////////////////////////
        ///
        ///     Constants
        ///

        static constexpr int family{ <xsl:value-of select="family/@id"/>
        <xsl:text> };</xsl:text>
        <xsl:if test="family/@prime">
          <xsl:text> // </xsl:text>
          <xsl:value-of select="family/@prime"/>
        </xsl:if>
        static constexpr <xsl:value-of select="$_UnitT"/><xsl:text> offset{ </xsl:text>
        <xsl:call-template name="apply-math-constants">
          <xsl:with-param name="where" select="offset"/>
        </xsl:call-template>
        <xsl:text> }; // from </xsl:text>
        <xsl:value-of select="refpoint/@normalized"/>
        static constexpr const char* format{ <xsl:text>&quot;</xsl:text><xsl:value-of select="format"/><xsl:text>&quot; }</xsl:text>;

        ///////////////////////////////////////////////////////////////////
        ///
        ///     Constructors
        ///

        /// @brief Default constructor.
        <xsl:value-of select="$_Scale"/>() = default;

        /// @brief Converting constructor: T -> <xsl:value-of select="$_ScaleT"/>.
        explicit constexpr <xsl:value-of select="$_Scale"/>(T level)
            : m_level{level}
        {}

        /// @brief Basic constructor.
        constexpr <xsl:value-of select="$_Scale"/>(<xsl:value-of select="$_UnitT"/> level)
            : m_level{level}
        {}

        /// @brief Copy constructor.
        constexpr <xsl:value-of select="$_Scale"/>(const <xsl:value-of select="$_Scale"/>&amp; source)
            : <xsl:value-of select="$_Scale"/>(source.m_level)
        {}

        /// @brief Copy-assignment.
        <xsl:value-of select="$_Scale"/>&amp; operator=(const <xsl:value-of select="$_Scale"/>&amp; rhs)
        {
            if (this != &amp;rhs) m_level = rhs.m_level;
            return *this;
        }

        ///////////////////////////////////////////////////////////////////
        ///
        ///     Conversions
        ///

        <xsl:for-each select="family/relative">
          <xsl:text>explicit </xsl:text>
          <xsl:value-of select="$_Scale"/>
          <xsl:text>(</xsl:text>
          <xsl:value-of select="concat(concat('_', .),$TT)"/>
          <xsl:text> level) : </xsl:text>
          <xsl:value-of select="$_Scale"/>
          <xsl:text>(</xsl:text>
          <xsl:value-of select="$_Unit"/>
          <xsl:text>(level.net_level()) + </xsl:text>
          <xsl:value-of select="$_Scale"/>
          <xsl:text>::offset) {}
        </xsl:text>
        </xsl:for-each>
        ///////////////////////////////////////////////////////////////////
        ///
        ///     Comparison
        ///

        friend bool operator==(const <xsl:value-of select="$_Scale"/>&amp; lhs, const <xsl:value-of select="$_Scale"/>&amp; rhs) { return lhs.m_level == rhs.m_level; }
        friend bool operator&lt; (const <xsl:value-of select="$_Scale"/>&amp; lhs, const <xsl:value-of select="$_Scale"/>&amp; rhs) { return lhs.m_level &lt; rhs.m_level; }
        friend bool operator&gt; (const <xsl:value-of select="$_Scale"/>&amp; lhs, const <xsl:value-of select="$_Scale"/>&amp; rhs) { return lhs.m_level &gt; rhs.m_level; }
        friend bool operator!=(const <xsl:value-of select="$_Scale"/>&amp; lhs, const <xsl:value-of select="$_Scale"/>&amp; rhs) { return !(lhs == rhs); }
        friend bool operator&lt;=(const <xsl:value-of select="$_Scale"/>&amp; lhs, const <xsl:value-of select="$_Scale"/>&amp; rhs) { return !(lhs &gt; rhs); }
        friend bool operator&gt;=(const <xsl:value-of select="$_Scale"/>&amp; lhs, const <xsl:value-of select="$_Scale"/>&amp; rhs) { return !(lhs &lt; rhs); }

        ///////////////////////////////////////////////////////////////////
        ///
        ///     Arithmetic
        ///

        <xsl:value-of select="$_Scale"/>&amp; operator++() { ++m_level; return *this; }
        <xsl:value-of select="$_Scale"/>  operator++(int) { auto old = *this; operator++(); return old; }
        <xsl:value-of select="$_Scale"/>&amp; operator--() { --m_level; return *this; }
        <xsl:value-of select="$_Scale"/>  operator--(int) { auto old = *this; operator--(); return old; }

        <xsl:value-of select="$_Scale"/>&amp; operator+=(const <xsl:value-of select="$_UnitT"/>&amp; rhs) { m_level += rhs; return *this; }
        <xsl:value-of select="$_Scale"/>&amp; operator-=(const <xsl:value-of select="$_UnitT"/>&amp; rhs) { m_level -= rhs; return *this; }

        friend <xsl:value-of select="$_Scale"/> operator+(<xsl:value-of select="$_Scale"/> lhs, const <xsl:value-of select="$_UnitT"/>&amp; rhs) { lhs += rhs; return lhs; }
        friend <xsl:value-of select="$_Scale"/> operator+(const <xsl:value-of select="$_UnitT"/>&amp; lhs, <xsl:value-of select="$_Scale"/> rhs) { rhs += lhs; return rhs; }
        friend <xsl:value-of select="$_Scale"/> operator-(<xsl:value-of select="$_Scale"/> lhs, const <xsl:value-of select="$_UnitT"/>&amp; rhs) { lhs -= rhs; return lhs; }
        friend <xsl:value-of select="$_Scale"/> operator-(const <xsl:value-of select="$_Scale"/>&amp; lhs) { return <xsl:value-of select="$_Scale"/>{ -lhs.m_level }; }

        friend <xsl:value-of select="$_UnitT"/> operator-(const <xsl:value-of select="$_Scale"/>&amp; lhs, const <xsl:value-of select="$_Scale"/>&amp; rhs) { return lhs.m_level - rhs.m_level; }

        ///////////////////////////////////////////////////////////////////
        ///
        ///     Formatting
        ///

        /// @brief Extraction operator (sends quantity and its primary unit to the output stream).
        friend std::ostream&amp; operator&lt;&lt;(std::ostream&amp; os, const <xsl:value-of select="$_Scale"/>&amp; level)
        {
            os &lt;&lt; level.m_level;
            return os;
        }

        /// @brief Converts <xsl:value-of select="$_ScaleT"/> level to a text form.
        /// @param level - level to convert,
        /// @param format - formatting string (default <xsl:value-of select="$_ScaleT"/>::format).
        /// @returns level string with <xsl:value-of select="$_ScaleT"/> unit applied.
        friend std::string to_string(<xsl:value-of select="$_Scale"/> level, const char* format = nullptr)
        {
            return to_string(level.m_level, format ? format : <xsl:value-of select="$_ScaleT"/>::format);
        }

        ///////////////////////////////////////////////////////////////////
        ///
        ///     Properties
        ///

        /// @brief Level relative to the point zero of the <xsl:value-of select="$Scale"/> scale.
        <xsl:value-of select="$_UnitT"/> level() const { return m_level; }

        /// @brief Level relative to the family common reference point (<xsl:value-of select="refpoint/@normalized"/>).
        <xsl:value-of select="$_UnitT"/> net_level() const { return m_level - <xsl:value-of select="$_ScaleT"/>::offset; }

    private:    
    
        ///////////////////////////////////////////////////////////////////
        ///
        ///     Fields
        ///

        <xsl:value-of select="$_UnitT"/> m_level;
    };

    ///////////////////////////////////////////////////////////////////
    ///
    ///     Type-alias for the scale
    ///

    using <xsl:value-of select="$Scale"/> = <xsl:value-of select="$_Scale"/>&lt;<xsl:value-of select="valuetype/@keywd"/>&gt;;
}

#endif /* !<xsl:value-of select="$SCALE"/>_H */
</xsl:template>
</xsl:stylesheet>
