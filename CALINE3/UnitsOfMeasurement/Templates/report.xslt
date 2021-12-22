<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="text" version="1.0" encoding="UTF-8" indent="no"/>

<xsl:template match="report"><!--<xsl:value-of select="@tm"/>
-->////////////////////////////////////////////////////////////////////////////////
//
//  U N I T S :: <xsl:value-of select="@ns"/> :: <xsl:value-of select="@uct"/> unit(s) :: <xsl:value-of select="@ufct"/> family(ies)
//
////////////////////////////////////////////////////////////////////////////////

[dim] family::unit {symbol(s)} : factor
<xsl:for-each select="unit">
  <xsl:text>
</xsl:text><xsl:value-of select="synopsis"/><xsl:text>

</xsl:text>
<xsl:for-each select="family/relative">
  <xsl:text>    </xsl:text><xsl:value-of select="../../@name"/><xsl:text>.From(</xsl:text><xsl:value-of select="."/><xsl:text>)
</xsl:text>
</xsl:for-each>
<xsl:for-each select="outer/operation">
  <xsl:text>    </xsl:text><xsl:value-of select="ret"/> = <xsl:value-of select="lhs"/><xsl:text> </xsl:text><xsl:value-of select="@op"/><xsl:text> </xsl:text><xsl:value-of select="rhs"/><xsl:text>
</xsl:text>
</xsl:for-each>
</xsl:for-each>

////////////////////////////////////////////////////////////////////////////////
//
//  S C A L E S :: <xsl:value-of select="@ns"/> :: <xsl:value-of select="@sct"/> scale(s) :: <xsl:value-of select="@sfct"/> family(ies)
//
////////////////////////////////////////////////////////////////////////////////

[dim] family::scale : refpoint = unit offset
<xsl:for-each select="scale">
  <xsl:text>
</xsl:text><xsl:value-of select="synopsis"/><xsl:text>

</xsl:text>
<xsl:for-each select="family/relative">
  <xsl:text>    </xsl:text><xsl:value-of select="../../@name"/><xsl:text>.From(</xsl:text><xsl:value-of select="."/><xsl:text>)
</xsl:text>
</xsl:for-each>
</xsl:for-each>

// end of report ///////////////////////////////////////////////////////////////
</xsl:template>
</xsl:stylesheet>
