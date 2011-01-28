<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
    <xsl:output method="xml" encoding="UTF-8" indent="yes" />

    <xsl:param name="reference" select="'reference.xml'" />

    <xsl:template match="method[@excluded='false' and @instrumented='false']">
        <xsl:variable name="methodName"><xsl:value-of select="@name" /></xsl:variable>
        <xsl:variable name="className"><xsl:value-of select="@class" /></xsl:variable>
        <method name="{$methodName}" excluded="false" instrumented="true" class="{$className}">
            <xsl:for-each select="seqpnt">
                <xsl:variable name="line"><xsl:value-of select="@line" /></xsl:variable>
                <xsl:variable name="column"><xsl:value-of select="@column" /></xsl:variable>
                <xsl:variable name="endline"><xsl:value-of select="@endline" /></xsl:variable>
                <xsl:variable name="endcolumn"><xsl:value-of select="@endcolumn" /></xsl:variable>
                <xsl:if test="document($reference)/coverage/module/method[@name=$methodName and @class=$className]/seqpnt[@line=$line and @column=$column and @endline=$endline and @endcolumn=$endcolumn]">
                    <xsl:text>
      </xsl:text>
                    <seqpnt visitcount="0" line="{@line}" column="{@column}" endline="{@endline}" endcolumn="{@endcolumn}" excluded="{@excluded}" document="{@document}" />
                </xsl:if>
            </xsl:for-each>
            <xsl:text>
    </xsl:text>
        </method>
    </xsl:template>

    <!-- Pass-through (identity transform) template -->
    <xsl:template match="* | @* | node()">
        <xsl:copy>
            <xsl:apply-templates select="* | @* | node()" />
        </xsl:copy>
    </xsl:template>

</xsl:stylesheet>