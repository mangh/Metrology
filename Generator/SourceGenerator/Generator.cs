/*******************************************************************************

    Units of Measurement for C# and C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

using Mangh.Metrology.XML;

namespace Mangh.Metrology
{
    public partial class SourceGenerator
    {
        internal class Generator : Definitions
        {
            #region Fields
            private readonly TranslationContext _tc;
            #endregion

            #region Properties
            #endregion

            #region Constructor(s)
            public Generator(TranslationContext tc)
                : base(tc[Template.DEFINITIONS].Path, tc)
            {
                _tc = tc;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Execute <see cref="Generator"/> routines.
            /// </summary>
            /// <returns><see langword="true"/> on success, <see langword="false"/> otherwise.</returns>
            public bool Execute()
            {
                return
                    // DO NOT CHANGE THE ORDER OF METHOD CALLS!
                    Load() &&
                    MakeUnits() &&
                    MakeScales() &&
                    MakeCatalog() &&
                    MakeAliases() &&
                    MakeReport();
            }

            /// <summary>
            /// Generate unit structures in the target language.
            /// </summary>
            /// <returns><see langword="true"/> on success, otherwise <see langword="false"/>.</returns>
            private bool MakeUnits()
            {
                using UnitModel model = new(_tc[Template.UNIT].Path, _tc, late: false);
                return model.ToSourceText(this, targetDirectory: string.Empty, _tc.GeneratorContext.AddSource);
            }

            /// <summary>
            /// Generate scale structures in the target language.
            /// </summary>
            /// <returns><see langword="true"/> on success, otherwise <see langword="false"/>.</returns>
            private bool MakeScales()
            {
                using ScaleModel model = new(_tc[Template.SCALE].Path, _tc, late: false);
                return model.ToSourceText(this, targetDirectory: string.Empty, _tc.GeneratorContext.AddSource);
            }

            /// <summary>
            /// Generate "<c>Catalog</c>" class in the target language.
            /// </summary>
            /// <returns><see langword="true"/> on success, otherwise <see langword="false"/>.</returns>
            private bool MakeCatalog()
            {
                using CatalogModel model = new(_tc[Template.CATALOG].Path, _tc);
                return model.ToSourceText(this, hintPath: _tc.CatalogFileName, _tc.GeneratorContext.AddSource);
            }

            /// <summary>
            /// Generate Aliasing Statements for the target language.
            /// </summary>
            /// <returns><see langword="true"/> on success, otherwise <see langword="false"/>.</returns>
            private bool MakeAliases()
            {
                using AliasingModel model = new(_tc[Template.ALIASES].Path, _tc, global: true);
                return model.ToFile(this, _tc.AliasesFilePath);
            }

            /// <summary>
            /// Generate report on units and scales that have been generated.
            /// </summary>
            /// <returns><see langword="true"/> on success, otherwise <see langword="false"/>.</returns>
            private bool MakeReport()
            {
                // It is acceptable to have no report template:
                if (_tc[Template.REPORT] is null)
                    return true;

                using ReportModel model = new(_tc[Template.REPORT].Path, _tc);
                return model.ToFile(this, _tc.ReportFilePath);
            }
            #endregion
        }
    }
}
