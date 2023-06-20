<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="text" version="1.0" encoding="UTF-8" indent="no"/>
  <xsl:template match="aliases">

    <xsl:variable name="GLOBAL">
      <xsl:choose>
        <xsl:when test="@global='yes'">
          <xsl:text>    global </xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>    /*global*/ </xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
   
    <xsl:text>#if DIMENSIONAL_ANALYSIS
</xsl:text>

    <xsl:value-of select="$GLOBAL"/><xsl:text>using </xsl:text><xsl:value-of select="@ns"/><xsl:text>;
</xsl:text>

    <xsl:value-of select="$GLOBAL"/><xsl:text>using static </xsl:text><xsl:value-of select="@ns"/><xsl:text>.Math;
</xsl:text>

    <xsl:text>#else
</xsl:text>

    <xsl:for-each select="unit">
      <xsl:value-of select="$GLOBAL"/><xsl:text>using </xsl:text><xsl:value-of select="@name"/><xsl:text> = </xsl:text><xsl:value-of select="@alias"/><xsl:text>;
</xsl:text>
    </xsl:for-each>

    <xsl:for-each select="scale">
      <xsl:value-of select="$GLOBAL"/><xsl:text>using </xsl:text><xsl:value-of select="@name"/><xsl:text> = </xsl:text><xsl:value-of select="@alias"/><xsl:text>;
</xsl:text>
    </xsl:for-each>

    <xsl:value-of select="$GLOBAL"/><xsl:text>using static System.Math;
</xsl:text>

    <xsl:text>#endif
</xsl:text>

  </xsl:template>
</xsl:stylesheet>
