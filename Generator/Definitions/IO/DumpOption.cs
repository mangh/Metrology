/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/
using System;

namespace Mangh.Metrology
{
    /// <summary>
    /// Options to dump intermediate (temporary) translation objects to files<br/>
    /// (debugging aid).
    /// </summary>
    [Flags]
    public enum DumpOption
    {
        /// <summary>
        /// No dump.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Model dump.
        /// </summary>
        Model = 0x1,

        /// <summary>
        /// Source code dump.
        /// </summary>
        SourceCode = 0x2,

        /// <summary>
        /// Assembly dump.
        /// </summary>
        Assembly = 0x4
    }
}
