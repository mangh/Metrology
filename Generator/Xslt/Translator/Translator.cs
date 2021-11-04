/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology
{
    /// <summary>
    /// Base Translator.
    /// </summary>
    public class Translator
    {
        #region Properties
        /// <summary>
        /// Namespace for units and scales being generated.
        /// </summary>
        public string TargetNamespace { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Base translator constructor.
        /// </summary>
        /// <param name="targetNamespace">namespace for units and scales being generated.</param>
        public Translator(string targetNamespace)
        {
            TargetNamespace = targetNamespace;
        }
        #endregion

        #region Methods
        #endregion
    }
}
