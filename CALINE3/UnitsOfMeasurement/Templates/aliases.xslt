<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="text" version="1.0" encoding="UTF-8" indent="no"/>
  <xsl:template match="aliases">
    <xsl:variable name="GLOBAL">
      <xsl:choose>
        <xsl:when test="@global='yes'">
          <xsl:text>global</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>/*global*/</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable><!--// <xsl:value-of select="@tm"/>
-->#if DIMENSIONAL_ANALYSIS
    <xsl:value-of select="$GLOBAL"/> using <xsl:value-of select="@ns"/>;
    <xsl:value-of select="$GLOBAL"/> using static <xsl:value-of select="@ns"/>.Math;
#else
    <xsl:for-each select="unit"><xsl:value-of select="$GLOBAL"/> using <xsl:value-of select="@name"/> = <xsl:value-of select="@alias"/>;
    </xsl:for-each>
    <xsl:for-each select="scale"><xsl:value-of select="$GLOBAL"/> using <xsl:value-of select="@name"/> = <xsl:value-of select="@alias"/>;
    </xsl:for-each>
    <xsl:value-of select="$GLOBAL"/> using static System.Math;
#endif
  </xsl:template>
</xsl:stylesheet>
