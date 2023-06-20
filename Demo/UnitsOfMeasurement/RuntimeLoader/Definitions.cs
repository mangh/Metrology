/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Demo.UnitsOfMeasurement
{
    public partial class RuntimeLoader
    {
        /// <summary>
        /// Definitions extended for the purpose of <see cref="RuntimeLoader"/>.
        /// </summary>
        private class Definitions : Mangh.Metrology.Definitions
        {
            #region Fields
            private readonly Dictionary<int, Mangh.Metrology.Measure> _family;
            private readonly TranslationContext _tc;
            private int _compileTimeUnitCount;
            private int _compileTimeScaleCount;
            #endregion

            #region Constructor(s)
            /// <summary>
            /// <see cref="Definitions"/> constructor.
            /// </summary>
            /// <param name="path">Path to the definitions file.</param>
            /// <param name="tc">Translation context.</param>
            public Definitions(string path, TranslationContext tc)
                : base(path, tc)
            {
                _tc = tc;
                _family = new Dictionary<int, Mangh.Metrology.Measure>(16);
                _compileTimeUnitCount = Units.Count;     // == 0
                _compileTimeScaleCount = Scales.Count;   // == 0
            }
            #endregion

            #region Methods: Decompiler
            /// <summary>
            /// Decompiles all <see cref="Unit"/> and <see cref="Scale"/> structures
            /// registered in <see cref="Catalog"/> at compile time.
            /// </summary>
            /// <returns>"<c>true</c>" on success, otherwise "<c>false</c>".</returns>
            public bool Decompile()
            {
                try
                {
                    Mangh.Metrology.Language.NumericAgent<double> doubleAgent = NumericAgentOfType<double>();
                    Mangh.Metrology.Language.NumericAgent<decimal> decimalAgent = NumericAgentOfType<decimal>();
                    Mangh.Metrology.Language.NumericAgent<float> floatAgent = NumericAgentOfType<float>();

                    // Recover units:
                    foreach (var u in Catalog.Units<double>()) Decompile(u, doubleAgent);
                    foreach (var u in Catalog.Units<decimal>()) Decompile(u, decimalAgent);
                    foreach (var u in Catalog.Units<float>()) Decompile(u, floatAgent);

                    _compileTimeUnitCount = Units.Count;

                    // Recover scales:
                    foreach (var s in Catalog.Scales<double>()) Decompile(s, doubleAgent);
                    foreach (var s in Catalog.Scales<decimal>()) Decompile(s, decimalAgent);
                    foreach (var s in Catalog.Scales<float>()) Decompile(s, floatAgent);

                    _compileTimeScaleCount = Scales.Count;

                    return true;
                }
                catch (ArgumentException ex) { ReportException(ex); }
                catch (InvalidOperationException ex) { ReportException(ex); }
                catch (NotSupportedException ex) { ReportException(ex); }
                catch (System.Reflection.AmbiguousMatchException ex) { ReportException(ex); }
                catch (TypeLoadException ex) { ReportException(ex); }

                return false;

                void ReportException(Exception ex)
                    => _tc.Report(Path, "the Catalog could not be decompiled (to restore the compile-time units).", ex);
            }

            /// <summary>
            /// Returns <see cref="Mangh.Metrology.Language.NumericAgent{T}"/> for the selected numeric type T.
            /// </summary>
            /// <typeparam name="T">
            /// Target language supported numeric type (e.g. <see cref="double"/>, <see cref="float"/>, <see cref="decimal"/>).
            /// </typeparam>
            /// <exception cref="InvalidOperationException">
            /// Thrown when the language context does not provide a numeric agent of the requested type (unlikely for C#).
            /// </exception>
            private Mangh.Metrology.Language.NumericAgent<T> NumericAgentOfType<T>() where T : struct, IEquatable<T>
                => Context.Language.NumericAgents.OfType<Mangh.Metrology.Language.NumericAgent<T>>().First();

            /// <summary>
            /// Recreates the original <see cref="Mangh.Metrology.Unit"/> (required by the parser)
            /// from the <see cref="Unit{T}"/> in the <see cref="Catalog"/>.
            /// </summary>
            /// <typeparam name="T">Value type underlying the <paramref name="unit"/>.</typeparam>
            /// <param name="unit"><see cref="Unit{T}"/> object to decompile.</param>
            /// <param name="agent">Target language numeric agent.</param>
            /// <exception cref="ArgumentException"></exception>
            private void Decompile<T>(Unit<T> unit, Mangh.Metrology.Language.NumericAgent<T> agent)
                where T : struct, IEquatable<T>
            {
                Mangh.Metrology.Numeric<T> factor = new(unit.Factor);
                string factorCode = agent.ToTargetString(unit.Factor);

                Mangh.Metrology.Unit decompiledUnit = new(
                    name: unit.Type.Name,
                    type: agent.NumericTerm,
                    sense: new Mangh.Metrology.DimExpr(unit.Sense, string.Empty /* dummy (needless?) code */),
                    factor: new Mangh.Metrology.NumExpr<T>(true, factor, factorCode),
                    format: unit.Format,
                    tags: new List<string>(unit.Symbol)
                );

                RestoreFamily(decompiledUnit, unit.Family);
                Units.Add(decompiledUnit);
            }

            /// <summary>
            /// Recreates the original <see cref="Mangh.Metrology.Scale"/> (required by the parser)
            /// from the <see cref="Scale{T}"/> in the <see cref="Catalog"/>.
            /// </summary>
            /// <typeparam name="T">Value type underlying the <paramref name="scale"/>.</typeparam>
            /// <param name="scale"><see cref="Scale{T}"/> object to decompile.</param>
            /// <param name="agent">Target language numeric agent.</param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="InvalidOperationException"></exception>
            /// <exception cref="NotSupportedException"></exception>
            /// <exception cref="System.Reflection.AmbiguousMatchException"></exception>
            /// <exception cref="TypeLoadException"></exception>
            private void Decompile<T>(Scale<T> scale, Mangh.Metrology.Language.NumericAgent<T> agent)
                where T : struct, IEquatable<T>
            {
                string scaleRefPoint = 
                    (Attribute.GetCustomAttribute(scale.Type, typeof(ScaleReferencePointAttribute)) is ScaleReferencePointAttribute refPointAttribute) ?
                        refPointAttribute.Name : string.Empty;

                Mangh.Metrology.Numeric<T> offset = new(scale.Offset.Value);
                string offsetCode = agent.ToTargetString(scale.Offset.Value);
                Mangh.Metrology.Unit scaleUnit = Units.First(u => u.TargetKeyword == scale.Unit.Type.Name);

                Mangh.Metrology.Scale decompiledScale = new (
                    name: scale.Type.Name,
                    type: agent.NumericTerm,
                    refpoint: scaleRefPoint,
                    unit: scaleUnit,
                    offset: new Mangh.Metrology.NumExpr<T>(true, offset, offsetCode),
                    format: scale.Format
                );

                RestoreFamily(decompiledScale, scale.Family);
                Scales.Add(decompiledScale);
            }

            /// <summary>
            /// Restores the family for a decompiled <see cref="Mangh.Metrology.Measure"/>.
            /// </summary>
            /// <param name="decompiled">The decompiled measure.</param>
            /// <param name="family">Family Id to be restored in the <paramref name="decompiled"/> measure.</param>
            /// <exception cref="ArgumentException"></exception>
            private void RestoreFamily(Mangh.Metrology.Measure decompiled, int family)
            {
                RestoreFamilyFor(decompiled, family);

                // Restore the relationship between relatives (within the family)
                if (_family.TryGetValue(family, out Mangh.Metrology.Measure? primary))
                    primary.AddRelative(decompiled);
                else
                    _family.Add(family, decompiled);
            }
            #endregion

            #region Methods: Generator
            public List<SyntaxTree>? Generate()
            {
                List<SyntaxTree> syntaxtrees = new();
                if ((Units.Count > _compileTimeUnitCount) || (Scales.Count > _compileTimeScaleCount))
                {
                    if (GenerateUnits() && GenerateScales())
                    {
                        return syntaxtrees;
                    }
                }
                return null;

                /////////////////////////////////////

                void ParseSourceText(string path, SourceText text)
                {
                    syntaxtrees.Add(CSharpSyntaxTree.ParseText(text, null, path));
                }
                bool GenerateUnits()
                {
                    using Mangh.Metrology.XML.UnitModel model = new(_tc.UnitTemplateFilePath, _tc, late: true);
                    return model.ToSourceText(this, _tc.DumpDirectory, ParseSourceText, _compileTimeUnitCount);
                }
                bool GenerateScales()
                {
                    using Mangh.Metrology.XML.ScaleModel model = new(_tc.ScaleTemplateFilePath, _tc, late: true);
                    return model.ToSourceText(this, _tc.DumpDirectory, ParseSourceText, _compileTimeScaleCount);
                }
            }
            #endregion
        }
    }
}
