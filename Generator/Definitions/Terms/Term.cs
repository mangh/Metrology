/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology
{
    /// <summary>
    /// Base type for the parser generated types.
    /// </summary>
    public class Term
    {
        #region Properties
        /// <summary>
        /// The type keyword that is used in the definition file.
        /// </summary>
        public string SourceKeyword { get; }

        /// <summary>
        /// The type keyword that is used in the target language.
        /// <example>For example:
        /// <code>
        /// "Meter"  // for the Meter unit of measurement,
        /// "double" // for the predefined 64-bit floating-point type.
        /// </code>
        /// </example>
        /// </summary>
        public string TargetKeyword { get; }

        /// <summary>
        /// The type name (fully qualified) in the target language.
        /// <example>For example:
        /// <code>
        /// null            // for the Meter unit,
        /// "System.Double" // for the type "double" in C#,
        /// "double"        // for the type "double" in C++.
        /// </code>
        /// </example>
        /// </summary>
        /// <remarks>NOTE:<br/>
        /// Must NOT be <see langword="null"/> for target language built-in types!<br/>
        /// Must BE <see langword="null"/> for other types!
        /// </remarks>
        public string? TargetTypename { get; }

        /// <summary>Returns <see langword="true"/> for a predefined type, <see langword="false"/> for any other.</summary>
        public bool IsPredefined => TargetTypename is not null;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="Term"/> constructor.
        /// </summary>
        /// <param name="sourceKeyword">Keyword denoting the type in the definition file.</param>
        /// <param name="targetKeyword">Keyword denoting the type in the target language.</param>
        /// <param name="targetTypename">The type name (fully qualified) in the target language.</param>
        public Term(string sourceKeyword, string targetKeyword, string? targetTypename = null)
        {
            SourceKeyword = sourceKeyword;
            TargetKeyword = targetKeyword;
            TargetTypename = targetTypename;
        }
        #endregion
    }
}
