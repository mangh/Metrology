<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="text" version="1.0" encoding="UTF-8" indent="yes"/>

<xsl:template match="catalog">
/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology

<!--<xsl:value-of select="@tm"/>-->
********************************************************************************/
using Mangh.Metrology;
using System;
using System.Collections.Generic;
using System.Linq;

namespace <xsl:value-of select="@ns"/>
{
    /// &lt;summary&gt;
    /// Catalog of all unit and scale proxies available at compile-time,
    /// possibly supplemented with late proxies at run-time.
    /// &lt;/summary&gt;
    public static partial class Catalog
    {
        #region Fields
        private static readonly List&lt;Unit&gt; m_units = default!;
        private static readonly List&lt;Scale&gt; m_scales = default!;
        #endregion

        #region Properties
        public static IEnumerable&lt;Unit&gt; AllUnits =&gt; m_units;
        public static IEnumerable&lt;Scale&gt; AllScales =&gt; m_scales;
        public static IEnumerable&lt;Proxy&gt; All =&gt; (m_units as IEnumerable&lt;Proxy&gt;).Union(m_scales);
        #endregion

        #region Constructor(s)
        static Catalog()
        {
            // <xsl:value-of select="@uct"/> units + 1 extra entry for each of <xsl:value-of select="@ufct"/> families for possible late units
            m_units = new List&lt;Unit&gt;(<xsl:value-of select="@uct"/> + <xsl:value-of select="@ufct"/>);

            // <xsl:value-of select="@sct"/> scales + 1 extra entry for each of <xsl:value-of select="@sfct"/> families for possible late scales
            m_scales = new List&lt;Scale&gt;(<xsl:value-of select="@sct"/> + <xsl:value-of select="@sfct"/>);

            Populate();
        }
        #endregion

        #region Populate
        private static void Populate()
        {
            // units                               [dim] family::unit {symbol(s)} : factor
            <xsl:for-each select="unit">
            Add(<xsl:value-of select="@name"/>.Proxy);<xsl:value-of select="."/>
            </xsl:for-each>

            // scales                              [dim] family::scale : refpoint = unit offset
            <xsl:for-each select="scale">
            Add(<xsl:value-of select="@name"/>.Proxy);<xsl:value-of select="."/>
            </xsl:for-each>
        }

        /// &lt;summary&gt;
        /// Adds measure (unit or scale proxy) to the collection.
        /// &lt;/summary&gt;
        /// &lt;param name="measure"&gt;Unit or scale proxy.&lt;/param&gt;
        /// &lt;exception cref="ArgumentException"&gt;Thrown when the measure is neither unit nor scale.&lt;/exception&gt;
        /// &lt;remarks&gt;
        /// This method assumes that all measures originate from the Parser and relies on the validations made there i.e.:
        /// 1. unit and scale names are unique (no unit has the same name as other unit or scale),
        /// 2. units are identified uniquely by their symbols (no unit has the same symbol as other unit).
        /// &lt;/remarks&gt;
        public static void Add(Proxy measure)
        {
            if (Contains(measure))
            {
                throw new ArgumentException(string.Format("{0}: duplicate proxy.", measure));
            }
            else if (measure is Unit u)
            {
                m_units.Add(u);
            }
            else if (measure is Scale s)
            {
                m_scales.Add(s);
            }
            else
            {
                throw new ArgumentException(string.Format("{0}: neither unit nor scale.", measure));
            }
        }
        public static void Clear()
        {
            m_units.Clear();
            m_scales.Clear();
        }
        public static void Reset()
        {
            Clear();
            Populate();
        }
        public static void AppendFromAssembly(System.Reflection.Assembly assembly)
        {
            foreach (Type t in assembly.GetExportedTypes())
            {
                Proxy? proxy = Proxy.TryRetrieveFrom(t);
                if (proxy is not null)
                {
                    Add(proxy);
                }
            }
        }
        public static bool Contains(Proxy measure)
        {
            bool equals(Proxy m) =&gt; m.Equals(measure);
            return m_units.Any((Func&lt;Proxy, bool&gt;)equals) || m_scales.Any((Func&lt;Proxy, bool&gt;)equals);
        }
        #endregion

        #region Unit&lt;T&gt;, IEnumerable&lt;Unit&lt;T&gt;&gt;
        /// &lt;summary&gt;Returns unit of the given type and symbol.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Type of the unit to be selected.&lt;/typeparam&gt;
        /// &lt;param name="symbol"&gt;Symbol (tag) of the unit to be selected.&lt;/param&gt;
        public static Unit&lt;T&gt;? Unit&lt;T&gt;(string symbol) where T : struct
            =&gt; Units&lt;T&gt;().FirstOrDefault(u =&gt; u.Symbol.IndexOf(symbol) &gt;= 0);

        /// &lt;summary&gt;Returns units of the required type and matching the given predicate.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Type of units to be selected.&lt;/typeparam&gt;
        /// &lt;param name="match"&gt;Predicate to be applied for selecting units.&lt;/param&gt;
        public static IEnumerable&lt;Unit&lt;T&gt;&gt; Units&lt;T&gt;(Predicate&lt;Unit&lt;T&gt;&gt;? match = null) where T : struct
        {
            IEnumerable&lt;Unit&lt;T&gt;&gt; units = m_units.OfType&lt;Unit&lt;T&gt;&gt;();
            return match is null ? units : units.Where(u =&gt; match(u));
        }

        /// &lt;summary&gt;Returns units of the required type and family.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Type of units to be selected.&lt;/typeparam&gt;
        /// &lt;param name="family"&gt;Family of units to be selected.&lt;/param&gt;
        public static IEnumerable&lt;Unit&lt;T&gt;&gt; Units&lt;T&gt;(int family) where T : struct
            =&gt; Units&lt;T&gt;(u =&gt; u.Family == family);

        /// &lt;summary&gt;Returns units of the required type and dimension.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Type of units to be selected.&lt;/typeparam&gt;
        /// &lt;param name="sense"&gt;Required dimension.&lt;/param&gt;
        public static IEnumerable&lt;Unit&lt;T&gt;&gt; Units&lt;T&gt;(Dimension sense) where T : struct
            =&gt; Units&lt;T&gt;(u =&gt; u.Sense == sense);

        /// &lt;summary&gt;Returns units underlying selected scales.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Type of units to be selected.&lt;/typeparam&gt;
        /// &lt;param name="scales"&gt;Selected scales.&lt;/param&gt;
        public static IEnumerable&lt;Unit&lt;T&gt;&gt; Units&lt;T&gt;(IEnumerable&lt;Scale&lt;T&gt;&gt; scales) where T : struct
            =&gt; scales.Where(s =&gt; s.Unit is not null).Select(s =&gt; (Unit&lt;T&gt;)s!.Unit);
        #endregion

        #region Scale&lt;T&gt;, IEnumerable&lt;Scale&lt;T&gt;&gt;
        /// &lt;summary&gt;Returns scale of the given type, family and symbol.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Type of the scale to be selected.&lt;/typeparam&gt;
        /// &lt;param name="family"&gt;Family of the scale to be selected.&lt;/param&gt;
        /// &lt;param name="symbol"&gt;Symbol (tag) of unit that underlies the required scale.&lt;/param&gt;
        public static Scale&lt;T&gt;? Scale&lt;T&gt;(int family, string symbol) where T : struct
            =&gt; Scales&lt;T&gt;(family).FirstOrDefault(s =&gt; s.Unit.Symbol.IndexOf(symbol) &gt;= 0);

        /// &lt;summary&gt;Returns scale of the given type, family and unit.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Type of the scale to be selected.&lt;/typeparam&gt;
        /// &lt;param name="family"&gt;Family of the scale to be selected.&lt;/param&gt;
        /// &lt;param name="unit"&gt;Unit of the scale to be selected.&lt;/param&gt;
        public static Scale&lt;T&gt;? Scale&lt;T&gt;(int family, Unit&lt;T&gt; unit) where T : struct
            =&gt; Scales&lt;T&gt;(family).FirstOrDefault(s =&gt; s.Unit.Equals(unit));

        /// &lt;summary&gt;Returns scales of the given type and matching the given predicate.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Type of scales.&lt;/typeparam&gt;
        /// &lt;param name="match"&gt;Predicate to be applied for selecting scales.&lt;/param&gt;
        public static IEnumerable&lt;Scale&lt;T&gt;&gt; Scales&lt;T&gt;(Predicate&lt;Scale&lt;T&gt;&gt;? match = null) where T : struct
        {
            IEnumerable&lt;Scale&lt;T&gt;&gt; scales = m_scales.OfType&lt;Scale&lt;T&gt;&gt;();
            return match is null ? scales : scales.Where(s =&gt; match(s));
        }

        /// &lt;summary&gt;Returns scales of the required type and family.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Type of scales to be selected.&lt;/typeparam&gt;
        /// &lt;param name="family"&gt;Family of scales to be selected.&lt;/param&gt;
        public static IEnumerable&lt;Scale&lt;T&gt;&gt; Scales&lt;T&gt;(int family) where T : struct
            =&gt; Scales&lt;T&gt;(u =&gt; u.Family == family);

        /// &lt;summary&gt;Returns scales of the given type and dimension.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Type of scales to be selected.&lt;/typeparam&gt;
        /// &lt;param name="sense"&gt;Dimension of scales to be selected.&lt;/param&gt;
        /// &lt;returns&gt;&lt;/returns&gt;
        public static IEnumerable&lt;Scale&lt;T&gt;&gt; Scales&lt;T&gt;(Dimension sense) where T : struct
            =&gt; Scales&lt;T&gt;(s =&gt; s.Unit.Sense == sense);
        #endregion
    }
}
</xsl:template>
</xsl:stylesheet>
