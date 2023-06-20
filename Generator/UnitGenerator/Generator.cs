/*******************************************************************************

    Units of Measurement for C# and C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Mangh.Metrology.Language;
using Mangh.Metrology.XML;

namespace Mangh.Metrology.UnitGenerator
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
            : base(tc.DefinitionsFilePath, tc)
        {
            _tc = tc;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Execute <see cref="Generator"/> routines.
        /// </summary>
        /// <returns><see langword="true"/> on successful generation, <see langword="false"/> otherwise.</returns>
        public bool Execute()
        {
            bool done = false;
            if (_tc.Language.Id == ID.CPP)
            {
                done =
                    // DO NOT CHANGE THE ORDER OF METHOD CALLS!
                    Load() &&
                    MakeUnits() &&
                    MakeScales() &&
                    MakeReport();
            }
            else if (_tc.Language.Id == ID.CS)
            {
                done =
                    // DO NOT CHANGE THE ORDER OF METHOD CALLS!
                    Load() &&
                    MakeUnits() &&
                    MakeScales() &&
                    MakeCatalog() &&
                    MakeAliases() &&
                    MakeReport();
            }
            return done;
        }

        /// <summary>
        /// Generate unit structures in the target language.
        /// </summary>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/>.</returns>
        private bool MakeUnits()
        {
            using UnitModel model = new(_tc.UnitTemplateFilePath, _tc, late: false);
            return model.ToFile(this, _tc.TargetDirectory);
        }

        /// <summary>
        /// Generate scale structures in the target language.
        /// </summary>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/>.</returns>
        private bool MakeScales()
        {
            using ScaleModel model = new(_tc.ScaleTemplateFilePath, _tc, late: false);
            return model.ToFile(this, _tc.TargetDirectory);
        }

        /// <summary>
        /// Generate "<c>Catalog</c>" class in the target language.
        /// </summary>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/>.</returns>
        private bool MakeCatalog()
        {
            using CatalogModel model = new(_tc.CatalogTemplateFilePath, _tc);
            return model.ToFile(this, _tc.CatalogFilePath);
        }

        /// <summary>
        /// Generate Aliasing Statements for the target language.
        /// </summary>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/>.</returns>
        private bool MakeAliases()
        {
            using AliasingModel model = new(_tc.AliasesTemplateFilePath, _tc, global: true);
            return model.ToFile(this, _tc.AliasesFilePath);
        }

        /// <summary>
        /// Generate report on units and scales that have been generated.
        /// </summary>
        /// <returns><see langword="true"/> on success, otherwise <see langword="false"/>.</returns>
        private bool MakeReport()
        {
            using ReportModel model = new(_tc.ReportTemplateFilePath, _tc);
            return model.ToFile(this, _tc.ReportFilePath);
        }
        #endregion
    }
}
