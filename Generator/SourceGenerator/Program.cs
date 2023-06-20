/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using Mangh.Metrology.XML;
using Microsoft.CodeAnalysis;

//#if DEBUG
//using System.Diagnostics;
//#endif 

namespace Mangh.Metrology
{
    /// <summary>
    /// Metrology Source Generator (for the C# language).
    /// </summary>
    [Generator]
    public partial class SourceGenerator : ISourceGenerator
    {
        #region Methods
        /// <summary>
        /// Initializes the generator.
        /// </summary>
        /// <param name="context">Source generator context.</param>
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif
        }

        /// <summary>
        /// Generates source code for <c>Unit</c> and <c>Scale</c> structs, their <c>Catalog</c> as well as <c>Aliases</c> and a summary <c>Report</c>.
        /// </summary>
        /// <param name="context">Source generator context.</param>
        public void Execute(GeneratorExecutionContext context)
        {
            TranslationContext tc = new(context);
            if (tc.IsReady())
            {
                Generator generator = new(tc);
                generator.Execute();
            }
        }
        #endregion
    }
}
