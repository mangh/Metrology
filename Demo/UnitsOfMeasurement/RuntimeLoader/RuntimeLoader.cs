/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Reflection;

namespace Demo.UnitsOfMeasurement
{
    /// <summary>
    /// Runtime loader
    /// (to extend <see cref="Catalog"/> with late units, i.e. those that were not available at compile time).
    /// </summary>
    public partial class RuntimeLoader
    {
        #region Fields
        private readonly TranslationContext _tc;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="RuntimeLoader"/> constructor.
        /// </summary>
        public RuntimeLoader(TranslationContext tc)
        {
            _tc = tc;
        }
        #endregion

        #region Methods
        /// <summary>Supplements <see cref="Catalog"/> with late unit/scale definitions from a text file.</summary>
        /// <param name="late_txt">Path to the late definitions text file.</param>
        /// <returns><see langword="true"/> on success, <see langword="false"/> on failure (reported to error log).</returns>
        public bool LoadFromFile(string late_txt)
        {
            LateUnits lateUnits = new(
                path: Mangh.Metrology.FilePath.ChangeExtension(late_txt, "dll"), 
                tc: _tc, 
                persistent: _tc.DumpOptions.HasFlag(Mangh.Metrology.DumpOption.Assembly)
            );

            // Check whether the late units DLL saved on a previous run is obsolete:
            if (!lateUnits.IsOutdated(late_txt))
            {
                return lateUnits.AddToCatalog();
            }
            else
            {
                Definitions definitions = new(late_txt, _tc);

                // Retrieve compile-time definitions from the Catalog and then append late definitions (from file):
                if (definitions.Decompile() && definitions.Load())
                {
                    // Generate late units source code:
                    List<SyntaxTree>? sources = definitions.Generate();
                    if (sources is not null)
                    {
                        // Compile late units and add the resulting assembly to Catalog
                        // (the outdated DLL is replaced with a new one):
                        return lateUnits.AddToCatalog(sources);
                    }
                }
            }

            return false;
        }

        /// <summary>Supplements <see cref="Catalog"/> with late unit/scale definitions from a text string.</summary>
        /// <param name="lateDefinitions">Late definitions text string.</param>
        /// <returns><see langword="true"/> on success, <see langword="false"/> on failure (reported to error log).</returns>
        /// <remarks>
        /// NOTE:<br/>
        /// The method is NOT RECOMMENDED as the units loaded this way - while present in the <see cref="Catalog"/> and<br/>
        /// fully functional - will be NOT AVAILABLE (as <see cref="Assembly"/> references) when running <see cref="RuntimeLoader"/> the next time.<br/>
        /// In other words, you can do it only once - on subsequent runs <see cref="RuntimeLoader"/> may not work properly.
        /// </remarks>
        public bool LoadFromString(string lateDefinitions)
        {
            string pseudoPath = "LateMetrologyUnits";
            Definitions definitions = new(pseudoPath, _tc);
            LateUnits lateUnits = new(pseudoPath, _tc, persistent: false);

            // Retrieve compile-time definitions from the Catalog and then append late definitions (from string):
            if (definitions.Decompile() && definitions.Load(lateDefinitions))
            {
                // Generate late units source code:
                List<SyntaxTree>? sources = definitions.Generate();
                if (sources is not null)
                {
                    // Compile late units and add the resulting assembly to Catalog
                    // (the assembly is not persisted to DLL):
                    return lateUnits.AddToCatalog(sources);
                }
            }
            return false;
        }
        #endregion
    }
}
