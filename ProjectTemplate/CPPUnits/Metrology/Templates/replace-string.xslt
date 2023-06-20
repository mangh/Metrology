<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!-- Simple implementation of the "replace" function
       not available in XSLT 1.0 -->

  <xsl:template name="replace-string">
    <xsl:param name="where"/>
    <xsl:param name="what"/>
    <xsl:param name="with"/>
    <xsl:choose>
      <xsl:when test="contains($where,$what)">
        <xsl:value-of select="substring-before($where,$what)"/>
        <xsl:value-of select="$with"/>
        <xsl:call-template name="replace-string">
          <xsl:with-param name="where" select="substring-after($where,$what)"/>
          <xsl:with-param name="what" select="$what"/>
          <xsl:with-param name="with" select="$with"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$where"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
