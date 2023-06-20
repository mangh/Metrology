/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

using System;

namespace %NAMESPACE%
{
    /// <summary>
    /// Scale reference point name (to distinguish scales belonging to different families).
    /// </summary>
    /// <remarks>
    /// NOTE:<br/>
    /// Scales belonging to the same family share the same reference point.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class ScaleReferencePointAttribute : Attribute
    {
        public string Name { get; private set; }
        public ScaleReferencePointAttribute(string name) => Name = name;
    }
}
