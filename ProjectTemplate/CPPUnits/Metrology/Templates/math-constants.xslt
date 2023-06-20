<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:include href="replace-string.xslt"/>

  <!-- Replace the symbolic names of math constants ("System.Math.PI", "System.Math.E")
       with the appropriate numeric constants. -->

  <xsl:template name="apply-math-constants">
    <xsl:param name="where"/>
    <xsl:variable name="where_">
      <xsl:call-template name="replace-string">
        <xsl:with-param name="where" select="$where"/>
        <xsl:with-param name="what" select="'System.Math.PI'" />
        <xsl:with-param name="with" select="'3.141592653589793238462643383279502884L'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:call-template name="replace-string">
      <xsl:with-param name="where" select="$where_"/>
      <xsl:with-param name="what" select="'System.Math.E'" />
      <xsl:with-param name="with" select="'2.718281828459045235360287471352662498L'"/>
    </xsl:call-template>
  </xsl:template>

</xsl:stylesheet>
