/*******************************************************************************

    Units of Measurement for C# and C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology
{
    /// <summary>
    /// <see cref="TemplateName"/> array index.
    /// </summary>
    internal enum Template
    {
        /// <summary>
        /// Aliases template.
        /// </summary>
        ALIASES,

        /// <summary>
        /// Catalog  template.
        /// </summary>
        CATALOG,

        /// <summary>
        /// Definitions file.
        /// </summary>
        DEFINITIONS,

        /// <summary>
        /// Report template.
        /// </summary>
        REPORT,

        /// <summary>
        /// Scale template.
        /// </summary>
        SCALE,

        /// <summary>
        /// Unit template.
        /// </summary>
        UNIT,

        //////////////////////////////////////////////////////////////////
        //
        //  Template count
        //

        /// <summary>
        /// Number of <see cref="TemplateName"/> entries.
        /// </summary>
        COUNT
    }

    /// <summary>
    /// Template file names
    /// (in the template paths expected to be provided in the project file).
    /// </summary>
    internal class TemplateName
    {
        #region Constants
        /// <summary>
        /// Number of <see cref="TemplateName"/> entries.
        /// </summary>
        public const int SIZE = (int)Template.COUNT;
        #endregion

        #region Fields
        private readonly string[] _name = new string[SIZE];
        #endregion

        #region Properties
        /// <summary>
        /// Gets the <see cref="TemplateName"/> entry.
        /// </summary>
        /// <param name="tidx"><see cref="TemplateName"/> entry index.</param>
        /// <returns></returns>
        public string this[Template tidx]
        {
            get { return _name[(int)tidx]; }
            private set { _name[(int)tidx] = value; }
        }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="TemplateName"/> constructor.
        /// </summary>
        public TemplateName(TranslationContext tc)
        {
            this[Template.ALIASES] = tc.AliasesTemplateFileName;
            this[Template.CATALOG] = tc.CatalogTemplateFileName;
            this[Template.DEFINITIONS] = tc.DefinitionsFileName;
            this[Template.REPORT] = tc.ReportTemplateFileName;
            this[Template.SCALE] = tc.ScaleTemplateFileName;
            this[Template.UNIT] = tc.UnitTemplateFileName;
        }
        #endregion
    }
}
