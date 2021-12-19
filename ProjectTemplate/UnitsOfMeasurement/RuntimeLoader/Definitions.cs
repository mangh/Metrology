/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metrological.Namespace
{
    public partial class RuntimeLoader
    {
        /// <summary>
        /// Unit and scale definitions (restored from entities registered in the Catalog at compile time)
        /// </summary>
        public interface IDefinitions
        {
            List<Mangh.Metrology.UnitType> Units { get; }
            List<Mangh.Metrology.ScaleType> Scales { get; }
            int MaxFamilyFound { get; }
        }

        public class Definitions : IDefinitions
        {
            #region Fields
            private readonly List<Mangh.Metrology.UnitType> m_units;
            private readonly List<Mangh.Metrology.ScaleType> m_scales;
            private readonly Dictionary<int, Mangh.Metrology.MeasureType> m_families;
            #endregion

            #region Properties
            public List<Mangh.Metrology.UnitType> Units => m_units;
            public List<Mangh.Metrology.ScaleType> Scales => m_scales;
            public int MaxFamilyFound { get { return m_families.Count > 0 ? m_families.Max(kv => kv.Key) : -1; } }
            #endregion

            #region Constructor(s)
            public Definitions()
            {
                m_units = new List<Mangh.Metrology.UnitType>();
                m_scales = new List<Mangh.Metrology.ScaleType>();
                m_families = new Dictionary<int, Mangh.Metrology.MeasureType>(16);
            }
            #endregion

            #region Methods
            public void Decompile()
            {
                foreach (var u in Catalog.Units<double>())
                    Decompile(u, Mangh.Metrology.CS.ConstantNumExpr.DoubleOne, Mangh.Metrology.CS.ConstantNumExpr.DoubleZero);

                foreach (var u in Catalog.Units<decimal>())
                    Decompile(u, Mangh.Metrology.CS.ConstantNumExpr.DecimalOne, Mangh.Metrology.CS.ConstantNumExpr.DecimalZero);

                foreach (var u in Catalog.Units<float>())
                    Decompile(u, Mangh.Metrology.CS.ConstantNumExpr.FloatOne, Mangh.Metrology.CS.ConstantNumExpr.FloatZero);

                foreach (var s in Catalog.Scales<double>()) Decompile(s);
                foreach (var s in Catalog.Scales<decimal>()) Decompile(s);
                foreach (var s in Catalog.Scales<float>()) Decompile(s);
            }

            private void Decompile<T>(Unit<T> unit, Mangh.Metrology.NumeralExpression one, Mangh.Metrology.NumeralExpression zero)
                where T : struct
            {
                // The Number.CreateFromObject method called here (for: double, float and decimal
                // units only) will not return null factor (though it is possible in general):
                Mangh.Metrology.Numeral? factor = Mangh.Metrology.Numeral.CreateFromObject(unit.Factor);

                Mangh.Metrology.UnitType decompiledUnit =
                    new (
                        name: unit.Type.Name,
                        sense: new Mangh.Metrology.DimensionalExpression(unit.Sense, string.Empty /* needless (dummy) code */),
                        factor: new Mangh.Metrology.NumeralExpression(factor is not null, factor!, string.Empty /* needless (dummy) code */),
                        one,
                        zero,
                        format: unit.Format,
                        tags: new List<string>(unit.Symbol)
                    );

                AddFamily(unit.Family, decompiledUnit);

                m_units.Add(decompiledUnit);
            }

            private void Decompile<T>(Scale<T> scale)
                where T : struct
            {
                string scaleRefPoint = 
                    (Attribute.GetCustomAttribute(scale.Type, typeof(ScaleReferencePointAttribute)) is ScaleReferencePointAttribute refPointAttribute) ?
                        refPointAttribute.Name : string.Empty;

                // The Number.CreateFromObject method called here (for: double, float and decimal
                // scales only) will not return null factor (though it is possible in general):
                Mangh.Metrology.Numeral? offset = Mangh.Metrology.Numeral.CreateFromObject(scale.Offset.Value);

                // Likewise here, it is expected to find non-null unit in the list of units
                // that were previously compiled without errors:
                Mangh.Metrology.UnitType? scaleUnitType = m_units.Find(u => u.Typename == scale.Unit.Type.Name);

                Mangh.Metrology.ScaleType decompiledScale =
                    new (
                        name: scale.Type.Name,
                        refpoint: scaleRefPoint,
                        unit: scaleUnitType!,
                        offset: new Mangh.Metrology.NumeralExpression(offset is not null, offset!, string.Empty /* needless (dummy) code */),
                        format: scale.Format
                    );

                AddFamily(scale.Family, decompiledScale);

                m_scales.Add(decompiledScale);
            }

            private void AddFamily(int family, Mangh.Metrology.MeasureType measureType)
            {
                if (m_families.TryGetValue(family, out Mangh.Metrology.MeasureType? primal))
                    primal.AddRelative(measureType);
                else
                    m_families.Add(family, measureType);
            }

            //private static Mangh.Metrology.Dimension TranslateDimension(Dimension sense)
            //{
            //    return new Mangh.Metrology.Dimension(
            //        sense[Magnitude.Length],
            //        sense[Magnitude.Time],
            //        sense[Magnitude.Mass],
            //        sense[Magnitude.Temperature],
            //        sense[Magnitude.ElectricCurrent],
            //        sense[Magnitude.AmountOfSubstance],
            //        sense[Magnitude.LuminousIntensity],
            //        sense[Magnitude.Other]
            //    );
            //}
            #endregion
        }
    }
}
