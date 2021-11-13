/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/
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
        private readonly Definitions m_definitions; // in-memory (compile-time) units & scales
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

            m_definitions = new();
            m_parser = new(this, m_definitions);
            m_generator = new(this, m_definitions);
            m_compiler = new(this);
        }
        #endregion

        #region Methods
        /// <summary>Supplement the <see cref="Catalog"/> with late unit/scale definitions input from a text file.</summary>
        /// <param name="definitionsTxtPath">path to the late definitions text file</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool LoadFromFile(string definitionsTxtPath)
        {
            Errors.Clear();

            string? definitionsDllPath = PathChangeExtension(definitionsTxtPath, "dll");
            if (definitionsDllPath is not null)
            {
                Assembly? supplement = null;
                // Check whether the late units DLL saved on a previous run
                // is older than the current definition file:
                FileInfo textfile = new(definitionsTxtPath);
                FileInfo assembly = new(definitionsDllPath);
                if (textfile.LastWriteTime < assembly.LastWriteTime)
                {
                    // Load late units from the (still up to date) DLL previously saved:
                    supplement = m_compiler.AssemblyLoadFrom(definitionsDllPath);
                }
                else
                {
                    // Compile late units and replace the outdated DLL with a new one:
                    using StreamReader? definitionStream = FileOpenText(definitionsTxtPath);
                    if (definitionStream is not null)
                    {
                        supplement = Compile(definitionsTxtPath, definitionStream, outputAssemblyPath: definitionsDllPath);
                    }
                }
                if (supplement is not null)
                {
                    // Supplement the Catalog
                    Catalog.AppendFromAssembly(supplement);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Supplement the <see cref="Catalog"/> with late unit/scale definitions input from a text string.</summary>
        /// <remarks>
        /// NOTE: the method is NOT RECOMMENDED as the units loaded this way -
        /// while present in the <see cref="Catalog"/> and fully functional - 
        /// will be NOT AVAILABLE as (<see cref="Assembly"/>) references
        /// when running the <see cref="RuntimeLoader"/> the next time.
        /// </remarks>
        /// <param name="definitions">late definitions text string</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool LoadFromString(string definitions)
        {
            Errors.Clear();

            using (StringReader definitionStream = new(definitions))
            {
                Assembly? supplement = Compile(null, definitionStream, outputAssemblyPath: null);
                if (supplement is not null)
                {
                    Catalog.AppendFromAssembly(supplement);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Compile late unit/scale definitions into an assembly (DLL).
        /// </summary>
        /// <param name="definitionPath">definition file path; <c>null</c> for definitions being loaded from a string</param>
        /// <param name="definitionStream">definition stream</param>
        /// <param name="outputAssemblyPath">path to the output assembly file; <c>null</c> if the assembly is not to be saved to a file (not recommended)</param>
        /// <returns>An assembly with late units/scales; <c>null</c> on failure.</returns>
        private Assembly? Compile(string? definitionPath, TextReader definitionStream, string? outputAssemblyPath)
        {
            // Auxiliary CSharp file (perfectly needless when everything goes right
            // but useful in case of compilation errors):
            string? outputCSharpPath = (outputAssemblyPath is null) ? null : PathChangeExtension(outputAssemblyPath, "cs");

            // Remove the CSharp file that was generated on previous run:
            if (outputCSharpPath is not null)
                FileDelete(outputCSharpPath);   // ignore I/O errors

            // Retrieve compile-time definitions from the Catalog:
            m_definitions.Decompile();

            int unitCount = m_definitions.Units.Count;   // start index for (possible) new units
            int scaleCount = m_definitions.Scales.Count; // start index for (possible) new scales

            // Parse (append) new definitions (if any) to the definition lists:
            if (m_parser.Parse(definitionPath, definitionStream) &&
               ((m_definitions.Units.Count > unitCount) ||
                (m_definitions.Scales.Count > scaleCount)))
            {
                // Generate source code (for the new definitions only):
                string? generatedSource = m_generator.Transform(unitCount, scaleCount);
                if (generatedSource is not null)
                {
                    // Save the generated source code to file:
                    if (outputCSharpPath is not null)
                        FileSaveText(outputCSharpPath, generatedSource);    // ignore I/O errors

                    // References to assemblies containing units & scales that might be referenced from the source.
                    // NOTE: references to units/scales appended previously from a string (thus not saved to a DLL) are not available!!!
                    IEnumerable<Assembly>? catalogAssemblies = Catalog.All
                            .Select(m => m.Type.Assembly).Distinct()
                            .Where(asm => !string.IsNullOrWhiteSpace(asm.Location));

                    // Compile the generated source code (to the indicated output assembly file):
                    return m_compiler.CompileFromSource(generatedSource, catalogAssemblies, outputAssemblyPath);
                }
            }
            return null;
        }

        private void ReportError(string message) => Errors.Add(message);
        #endregion
    }
}
