/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology
{
    /// <summary>
    /// Abstract type for deriving parser generated types (units, scales, numerals etc).
    /// </summary>
    public abstract class AbstractType
    {
        #region Properties
        /// <summary>
        /// Typename (keyword) that is used in a target language.
        /// <example>For example:
        /// <code>
        /// "Meter"  // for the Meter unit of measurement,
        /// "double" // for the predefined 64-bit floating-point type.
        /// </code>
        /// </example>
        /// </summary>
        public string Typename { get; }

        /// <summary>
        /// Fully qualified type name (if any).
        /// <example>For example:
        /// <code>
        /// null            // for the Meter unit
        /// "System.Double" // for the predefined type "double".
        /// </code>
        /// </example>
        /// </summary>
        public string? PredefinedTypename { get; }

        /// <summary>Returns True for a predefined type, False for any other.</summary>
        public bool IsPredefined => PredefinedTypename is not null;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Abstract type constructor.
        /// </summary>
        /// <param name="typename">typename (keyword) that is used in a target language.</param>
        /// <param name="predefinedTypename">fully qualified (predefined) type name (if any).</param>
        protected AbstractType(string typename, string? predefinedTypename = null)
        {
            Typename = typename;
            PredefinedTypename = predefinedTypename;
        }
        #endregion
    }
}
