/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Demo.UnitsOfMeasurement
{
    /// <summary>
    /// Runtime loader (to extend the <see cref="Catalog"/> with late units, that were not included at compile-time).
    /// </summary>
    public partial class RuntimeLoader
    {
        #region Fields
        private readonly IO m_io;                   // RuntimeLoader I/O routines
        private readonly Decompiler m_decompiler;   // decompiles in-memory (compile-time) units
        private readonly Parser m_parser;           // parses late definitions
        private readonly Generator m_generator;     // generates C# source code from late definitions
        private readonly Compiler m_compiler;       // compiles new generated units & scales
        #endregion

        #region Properties
        /// <summary>
        /// Errors that occurred on the last run of the <see cref="RuntimeLoader"/>.
        /// </summary>
        public List<string> Errors { get; }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="RuntimeLoader"/> constructor.
        /// </summary>
        public RuntimeLoader()
        {
            Errors = new();

            m_io = new(this);
            m_decompiler = new();
            m_parser = new(this, m_decompiler);
            m_generator = new(this);
            m_compiler = new(this);
        }
        #endregion

        #region Methods
        /// <summary>Supplement the <see cref="Catalog"/> with late unit/scale definitions input from a text file.</summary>
        /// <param name="lateDefinitionsTxtPath">path to the late definitions text file</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool LoadFromFile(string lateDefinitionsTxtPath)
        {
            Errors.Clear();

            Assembly? supplement = null;
            string? lateDefinitionsDllPath = m_io.ChangeExtension(lateDefinitionsTxtPath, "dll");
            if (lateDefinitionsDllPath is not null)
            {
                // Check whether the late units DLL saved on a previous run
                // is older than the current definition file:
                FileInfo textfile = new(lateDefinitionsTxtPath);
                FileInfo assembly = new(lateDefinitionsDllPath);
                if (textfile.LastWriteTime < assembly.LastWriteTime)
                {
                    // Load late units from the (still up to date) DLL previously saved:
                    supplement = m_io.AssemblyLoad(lateDefinitionsDllPath);
                }
                else
                {
                    // Compile (updated) late units and replace the outdated DLL with a new one:
                    using StreamReader? definitionStream = m_io.FileOpen(lateDefinitionsTxtPath);
                    if (definitionStream is not null)
                    {
                        supplement = Compile(lateDefinitionsTxtPath, definitionStream, outputAssemblyPath: lateDefinitionsDllPath);
                    }
                }
                // Supplement the Catalog
                if (supplement is not null)
                {
                    Catalog.AppendFromAssembly(supplement);
                }
            }
            return supplement is not null;
        }

        /// <summary>Supplement the <see cref="Catalog"/> with late unit/scale definitions input from a text string.</summary>
        /// <remarks>
        /// NOTE: the method is NOT RECOMMENDED as the units loaded this way -
        /// while present in the <see cref="Catalog"/> and fully functional - 
        /// will be NOT AVAILABLE as (<see cref="Assembly"/>) references
        /// when running the <see cref="RuntimeLoader"/> the next time.
        /// </remarks>
        /// <param name="lateDefinitions">definition text string</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool LoadFromString(string lateDefinitions)
        {
            Errors.Clear();

            using (StringReader definitionStream = new(lateDefinitions))
            {
                Assembly? supplement = Compile("<definition string>", definitionStream, outputAssemblyPath: null);
                if (supplement is not null)
                {
                    Catalog.AppendFromAssembly(supplement);
                }
                return supplement is not null;
            }
        }

        /// <summary>
        /// Compile late unit/scale definitions into an assembly (DLL).
        /// </summary>
        /// <param name="definitionStream">definition stream</param>
        /// <param name="definitionHint">definition hint (path or other id)</param>
        /// <param name="outputAssemblyPath">path to the output assembly file; <c>null</c> if the assembly is not to be saved to a file (not recommended)</param>
        /// <returns>An assembly with late units/scales; <c>null</c> on failure.</returns>
        private Assembly? Compile(string definitionHint, TextReader definitionStream, string? outputAssemblyPath)
        {
            // Retrieve compile-time definitions of all Catalog entities:
            m_decompiler.Decompile();

            int unitCount = m_decompiler.Units.Count;   // start index for (possible) new units
            int scaleCount = m_decompiler.Scales.Count; // start index for (possible) new scales

            Assembly? supplement = null;

            // Parse (append) new definitions:
            if (m_parser.Parse(definitionHint, definitionStream) && ((m_decompiler.Units.Count > unitCount) || (m_decompiler.Scales.Count > scaleCount)))
            {
                // Generate source code (for the new definitions only):
                string? generatedSourceCode = m_generator.Transform(m_decompiler, unitCount, scaleCount);
                if (generatedSourceCode is not null)
                {
                    if (outputAssemblyPath is not null)
                    {
                        m_io.FileSave(m_io.ChangeExtension(outputAssemblyPath, "cs"), generatedSourceCode);
                    }

                    // References to assemblies containing units & scales that might be referenced from the source.
                    // NOTE: references to measures appended previously from a string (and not saved to a DLL) are not available!!!
                    IEnumerable<Assembly>? catalogAssemblies = Catalog.All
                            .Select(m => m.Type.Assembly).Distinct()
                            .Where(ass => !string.IsNullOrWhiteSpace(ass.Location));

                    // Compile the generated source code:
                    supplement = m_compiler.CompileFromSource(generatedSourceCode, catalogAssemblies, outputAssemblyPath);
                }
            }

            return supplement;
        }

        private void ReportError(string message) => Errors.Add(message);
        #endregion
    }
}
