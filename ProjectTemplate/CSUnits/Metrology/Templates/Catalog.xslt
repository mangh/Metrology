<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="text" version="1.0" encoding="UTF-8" indent="yes"/>

<xsl:template match="catalog">
/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Mangh.Metrology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace <xsl:value-of select="@ns"/>
{
    /// &lt;summary&gt;
    /// Catalog of all unit and scale proxies available at compile time&lt;br/&gt;
    /// (may be supplemented with late definitions at run-time).
    /// &lt;/summary&gt;
    /// &lt;remarks&gt;
    /// NOTE:&lt;br/&gt;
    /// It is assumed that the following conditions hold in the Catalog:&lt;br/&gt;
    /// 1. no measure has the same name as another,&lt;br/&gt;
    /// 2. no measure has the same symbol as another.&lt;br/&gt;
    /// The above conditions are checked by the &lt;see cref="Parser"/&gt; and are not verified again&lt;br/&gt;
    /// when adding a proxy to the Catalog.&lt;br/&gt;
    /// This means that proxies from other sources may violate these conditions&lt;br/&gt;
    /// unless you take care of it.
    /// &lt;/remarks&gt;
    public static partial class Catalog
    {
        #region Fields
        private static readonly List&lt;Unit&gt; m_units;
        private static readonly List&lt;Scale&gt; m_scales;
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

        #region Methods

        ///////////////////////////////////////////////////////////////////////
        // 
        //      Populate
        //

        /// &lt;summary&gt;
        /// Populate the &lt;see cref="Catalog"/&gt; with items available at compile time.
        /// &lt;/summary&gt;
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
        /// Adds measure (unit or scale proxy) to the &lt;see cref="Catalog"/&gt;.
        /// &lt;/summary&gt;
        /// &lt;param name="measure"&gt;Unit or scale proxy.&lt;/param&gt;
        /// &lt;exception cref="ArgumentException"&gt;
        /// Thrown when the measure is duplicated or is neither a unit nor a scale.
        /// &lt;/exception&gt;
        public static void Add(Proxy measure)
        {
            if (measure is Unit u)
            {
                if (m_units.Any(m =&gt; m.Equals(u))) throw new ArgumentException($"{u}: duplicated unit.");

                m_units.Add(u);
            }
            else if (measure is Scale s)
            {
                if (m_scales.Any(m =&gt; m.Equals(s))) throw new ArgumentException($"{s}: duplicated scale.");

                m_scales.Add(s);
            }
            else
            {
                throw new ArgumentException($"{measure}: neither unit nor scale.");
            }
        }

        /// &lt;summary&gt;
        /// Clears the &lt;see cref="Catalog"/&gt;
        /// (removes all items registred so far).
        /// &lt;/summary&gt;
        public static void Clear()
        {
            m_units.Clear();
            m_scales.Clear();
        }

        /// &lt;summary&gt;
        /// Resets the &lt;see cref="Catalog"/&gt;
        /// (removes all items registred so far and repopulates with the original items available at compile-time).
        /// &lt;/summary&gt;
        public static void Reset()
        {
            Clear();
            Populate();
        }

        /// &lt;summary&gt;
        /// Append measures from an &lt;see cref="System.Reflection.Assembly"/&gt; to the &lt;see cref="Catalog"/&gt;
        /// (in addition to those available at compile time).
        /// &lt;/summary&gt;
        /// &lt;param name="assembly"&gt;The assembly with measures to append.&lt;/param&gt;
        /// &lt;exception cref="ArgumentException"&gt;&lt;/exception&gt;
        /// &lt;exception cref="FieldAccessException"&gt;&lt;/exception&gt;
        /// &lt;exception cref="FileNotFoundException"&gt;&lt;/exception&gt;
        /// &lt;exception cref="NotSupportedException"&gt;&lt;/exception&gt;
        /// &lt;exception cref="TargetException"&gt;&lt;/exception&gt;
        /// &lt;exception cref="TargetInvocationException"&gt;&lt;/exception&gt;
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

        ///////////////////////////////////////////////////////////////////////
        // 
        //      Units
        //

        /// &lt;summary&gt;Returns a unit with the specified numeric type and symbol.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Numeric type of the unit sought (&lt;see cref="double"/&gt;, &lt;see cref="decimal"/&gt;, &lt;see cref="float"/&gt;).&lt;/typeparam&gt;
        /// &lt;param name="symbol"&gt;Symbol of the unit sought.&lt;/param&gt;
        public static Unit&lt;T&gt;? Unit&lt;T&gt;(string symbol) where T : struct
            =&gt; Units&lt;T&gt;().FirstOrDefault(u =&gt; u.Symbol.IndexOf(symbol) &gt;= 0);

        /// &lt;summary&gt;Returns collection of units with the specified numeric type that match the specified predicate.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Numeric type of the units sought (&lt;see cref="double"/&gt;, &lt;see cref="decimal"/&gt;, &lt;see cref="float"/&gt;).&lt;/typeparam&gt;
        /// &lt;param name="match"&gt;Predicate used for selecting units.&lt;/param&gt;
        public static IEnumerable&lt;Unit&lt;T&gt;&gt; Units&lt;T&gt;(Predicate&lt;Unit&lt;T&gt;&gt;? match = null) where T : struct
        {
            IEnumerable&lt;Unit&lt;T&gt;&gt; units = m_units.OfType&lt;Unit&lt;T&gt;&gt;();
            return match is null ? units : units.Where(u =&gt; match(u));
        }

        /// &lt;summary&gt;Returns collection of units with the specified numeric type and family.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Numeric type of the units sought (&lt;see cref="double"/&gt;, &lt;see cref="decimal"/&gt;, &lt;see cref="float"/&gt;).&lt;/typeparam&gt;
        /// &lt;param name="family"&gt;Family of the units sought.&lt;/param&gt;
        public static IEnumerable&lt;Unit&lt;T&gt;&gt; Units&lt;T&gt;(int family) where T : struct
            =&gt; Units&lt;T&gt;(u =&gt; u.Family == family);

        /// &lt;summary&gt;Returns collection of units with the specified numeric type and dimension.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Numeric type of the units sought (&lt;see cref="double"/&gt;, &lt;see cref="decimal"/&gt;, &lt;see cref="float"/&gt;).&lt;/typeparam&gt;
        /// &lt;param name="sense"&gt;Dimension of the units sought.&lt;/param&gt;
        public static IEnumerable&lt;Unit&lt;T&gt;&gt; Units&lt;T&gt;(Dimension sense) where T : struct
            =&gt; Units&lt;T&gt;(u =&gt; u.Sense == sense);

        /// &lt;summary&gt;Returns units underlying the selected scales.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Numeric type of the units sought (&lt;see cref="double"/&gt;, &lt;see cref="decimal"/&gt;, &lt;see cref="float"/&gt;).&lt;/typeparam&gt;
        /// &lt;param name="scales"&gt;Scale collection.&lt;/param&gt;
        public static IEnumerable&lt;Unit&lt;T&gt;&gt; Units&lt;T&gt;(IEnumerable&lt;Scale&lt;T&gt;&gt; scales) where T : struct
            =&gt; scales.Where(s =&gt; s.Unit is not null).Select(s =&gt; (Unit&lt;T&gt;)s!.Unit);

        ///////////////////////////////////////////////////////////////////////
        // 
        //      Scales
        //

        /// &lt;summary&gt;Returns a scale with the specified numeric type, family and symbol.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Numeric type of the scale sought (&lt;see cref="double"/&gt;, &lt;see cref="decimal"/&gt;, &lt;see cref="float"/&gt;).&lt;/typeparam&gt;
        /// &lt;param name="family"&gt;Family of the scale sought.&lt;/param&gt;
        /// &lt;param name="symbol"&gt;Symbol (tag) of the unit that underlies the scale sought.&lt;/param&gt;
        public static Scale&lt;T&gt;? Scale&lt;T&gt;(int family, string symbol) where T : struct
            =&gt; Scales&lt;T&gt;(family).FirstOrDefault(s =&gt; s.Unit.Symbol.IndexOf(symbol) &gt;= 0);

        /// &lt;summary&gt;Returns a scale with the specified numeric type, family and unit.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Numeric type of the scale sought (&lt;see cref="double"/&gt;, &lt;see cref="decimal"/&gt;, &lt;see cref="float"/&gt;).&lt;/typeparam&gt;
        /// &lt;param name="family"&gt;Family of the scale sought.&lt;/param&gt;
        /// &lt;param name="unit"&gt;Unit of the scale sought.&lt;/param&gt;
        public static Scale&lt;T&gt;? Scale&lt;T&gt;(int family, Unit&lt;T&gt; unit) where T : struct
            =&gt; Scales&lt;T&gt;(family).FirstOrDefault(s =&gt; s.Unit.Equals(unit));

        /// &lt;summary&gt;Returns collection of scales with the specified numeric type that match the specified predicate.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Numeric type of the scales sought (&lt;see cref="double"/&gt;, &lt;see cref="decimal"/&gt;, &lt;see cref="float"/&gt;).&lt;/typeparam&gt;
        /// &lt;param name="match"&gt;Predicate used for selecting scales.&lt;/param&gt;
        public static IEnumerable&lt;Scale&lt;T&gt;&gt; Scales&lt;T&gt;(Predicate&lt;Scale&lt;T&gt;&gt;? match = null) where T : struct
        {
            IEnumerable&lt;Scale&lt;T&gt;&gt; scales = m_scales.OfType&lt;Scale&lt;T&gt;&gt;();
            return match is null ? scales : scales.Where(s =&gt; match(s));
        }

        /// &lt;summary&gt;Returns collection of scales with the specified numeric type and family.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Numeric type of the scales sought (&lt;see cref="double"/&gt;, &lt;see cref="decimal"/&gt;, &lt;see cref="float"/&gt;).&lt;/typeparam&gt;
        /// &lt;param name="family"&gt;Family of the scales sought.&lt;/param&gt;
        public static IEnumerable&lt;Scale&lt;T&gt;&gt; Scales&lt;T&gt;(int family) where T : struct
            =&gt; Scales&lt;T&gt;(u =&gt; u.Family == family);

        /// &lt;summary&gt;Returns collection of scales with the specified numeric type and dimension.&lt;/summary&gt;
        /// &lt;typeparam name="T"&gt;Numeric type of the scales sought (&lt;see cref="double"/&gt;, &lt;see cref="decimal"/&gt;, &lt;see cref="float"/&gt;).&lt;/typeparam&gt;
        /// &lt;param name="sense"&gt;Dimension of the scales sought.&lt;/param&gt;
        /// &lt;returns&gt;&lt;/returns&gt;
        public static IEnumerable&lt;Scale&lt;T&gt;&gt; Scales&lt;T&gt;(Dimension sense) where T : struct
            =&gt; Scales&lt;T&gt;(s =&gt; s.Unit.Sense == sense);
        #endregion
    }
}
</xsl:template>
</xsl:stylesheet>
