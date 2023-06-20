/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace %NAMESPACE%
{
    public partial class RuntimeLoader
    {
        /// <summary>
        /// Late units' <see cref="Assembly"/>.
        /// </summary>
        private class LateUnits
        {
            #region Field
            public TranslationContext _tc;
            #endregion

            #region Properties
            /// <summary>
            /// Path to the DLL library file.
            /// </summary>
            public string Path { get; }

            /// <summary>
            /// Is it a temporary (<see langword="false"/>) or a permanent <see cref="Assembly"/> (<see langword="true"/>)
            /// saved in the DLL library file?
            /// </summary>
            public bool Persistent { get; }
            #endregion

            #region Constructor(s)
            /// <summary>
            /// <see cref="LateUnits"/> constructor/
            /// </summary>
            /// <param name="path">Path to the DLL library file (may not be the real path).</param>
            /// <param name="tc">Translation context.</param>
            /// <param name="persistent">Is it a persistent (<see langword="true"/>), or a temporary (<see langword="false"/>) <see cref="Assembly"/>?</param>
            public LateUnits(string path, TranslationContext tc, bool persistent)
            {
                _tc = tc;
                Path = path;
                Persistent = persistent;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Checks if the DLL library is out of date (relative to the source file)?
            /// </summary>
            /// <param name="sourcePath">Path to the source file (the DLL is built from).</param>
            /// <returns><see langword="true"/> when the DLL is outdated (or cannot be verified), otherwise <see langword="false"/>.</returns>
            public bool IsOutdated(string sourcePath)
            {
                if (Persistent)
                {
                    try
                    {
                        return File.GetLastWriteTime(sourcePath) >= File.GetLastWriteTime(Path);
                    }
                    catch (ArgumentException ex) { ReportException(ex); }
                    catch (IOException ex) { ReportException(ex); }
                    catch (NotSupportedException ex) { ReportException(ex); }
                    catch (UnauthorizedAccessException ex) { ReportException(ex); }
                }
                return true;    // <= NOTE!

                void ReportException(Exception ex)
                    => _tc.Report(Path, $"could not verify whether assembly is out of date (relative to the definitions file: {sourcePath}).", ex);
            }

            /// <summary>
            /// Loads <see cref="Assembly"/> from the DLL library file (with path <see cref="Path"/>).
            /// </summary>
            /// <returns>Loaded <see cref="Assembly"/> or <see langword="null"/> if loading fails.</returns>
            public Assembly? Load()
            {
                try
                {
                    return Assembly.LoadFrom(Path);
                }
                catch (ArgumentException ex) { ReportException(ex); }
                catch (BadImageFormatException ex) { ReportException(ex); }
                catch (IOException ex) { ReportException(ex); }
                catch (System.Security.SecurityException ex) { ReportException(ex); }

                return null;

                void ReportException(Exception ex)
                    => _tc.Report(Path, "could not load assembly.", ex);
            }

            /// <summary>
            /// Loads <see cref="Assembly"/> from the Portable Executable (PE) stream.
            /// </summary>
            /// <param name="peStream">Portable Executable stream.</param>
            /// <returns>Loaded <see cref="Assembly"/> or <see langword="null"/> if loading fails.</returns>
            public Assembly? Load(MemoryStream peStream)
            {
                try
                {
                    return Assembly.Load(peStream.GetBuffer());
                }
                catch (ArgumentException ex) { ReportException(ex); }
                catch (BadImageFormatException ex) { ReportException(ex); }
                catch (UnauthorizedAccessException ex) { ReportException(ex); }

                return null;

                void ReportException(Exception ex)
                    => _tc.Report(Path, "could not load assembly from PE (Portable Executable) stream.", ex);
            }

            /// <summary>
            /// Saves the compiled late definitions to a library file (DLL).
            /// </summary>
            /// <param name="peStream">Portable Executable (PE) stream of the compiled late definitions.</param>
            /// <returns><see langword="true"/> on successful save, <see langword="false"/> on errors (reported to error log).</returns>
            public bool Save(MemoryStream peStream)
            {
                try
                {
                    using FileStream dllStream = new(Path, FileMode.Create);
                    peStream.Position = 0;
                    peStream.CopyTo(dllStream);
                    return true;
                }
                catch (ArgumentException ex) { ReportException(ex); }
                catch (IOException ex) { ReportException(ex); }
                catch (NotSupportedException ex) { ReportException(ex); }
                catch (ObjectDisposedException ex) { ReportException(ex); }
                catch (System.Security.SecurityException ex) { ReportException(ex); }
                catch (UnauthorizedAccessException ex) { ReportException(ex); }

                return false;

                void ReportException(Exception ex)
                    => _tc.Report(Path, "could not save assembly.", ex);
            }

            /// <summary>
            /// Compiles late definitions source code into an <see cref="Assembly"/>.
            /// </summary>
            /// <param name="sources">Source code generated from late definitions.</param>
            /// <returns>Late units <see cref="Assembly"/> on successful compilation, <see langword="null"/> when compilation fails.</returns>
            private Assembly? Compile(List<SyntaxTree> sources)
            {
                string assemblyName = Mangh.Metrology.FilePath.GetFileNameWithoutExtension(Path);
                string errmsg = "could not compile assembly.";

                try
                {
                    // References to assemblies containing units & scales that might be referenced in the late definitions
                    // NOTE: references to units/scales appended previously from a string (thus not saved to a DLL) are not available!!!
                    IEnumerable<Assembly> references =
                        Catalog.All
                        .Select(m => m.Type.Assembly).Distinct()
                        .Where(asm => !string.IsNullOrWhiteSpace(asm.Location));

                    IEnumerable<PortableExecutableReference> portableReferences = references
                        .Concat(System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies)
                        .Where(asm => !asm.IsDynamic)
                        .Select(asm => AssemblyMetadata.CreateFromFile(asm.Location).GetReference());

                    CSharpCompilationOptions options = new(OutputKind.DynamicallyLinkedLibrary);

                    CSharpCompilation? compilation = CSharpCompilation.Create(
                        assemblyName: assemblyName,
                        syntaxTrees: sources,
                        references: portableReferences,
                        options: options.WithNullableContextOptions(NullableContextOptions.Enable)
                    );

                    // Portable Executable (PE) stream of the compiled late definitions:
                    using MemoryStream peStream = new();
                    Microsoft.CodeAnalysis.Emit.EmitResult result = compilation.Emit(peStream);
                    if (result.Success)
                    {
                        return (Persistent && !Save(peStream)) ? null : Load(peStream);
                    }

                    _tc.Report(Path, $"errors in assembly source code:{Environment.NewLine}", null);

                    foreach (Diagnostic d in result.Diagnostics)
                    {
                        _tc.Report(d.Id, d.ToString(), null);
                    }
                }
                catch (ArgumentException ex) { _tc.Report(Path, errmsg, ex); }
                catch (IOException ex) { _tc.Report(Path, errmsg, ex); }
                catch (NotSupportedException ex) { _tc.Report(Path, errmsg, ex); }

                return null;
            }

            /// <summary>
            /// Adds late units <see cref="Assembly"/> (loaded from a DLL library) to the <see cref="Catalog"/>.
            /// </summary>
            /// <returns><see langword="true"/> on success, <see langword="true"/> on failure.</returns>
            public bool AddToCatalog()
                => AddToCatalog(Load());

            /// <summary>
            /// Adds late units <see cref="Assembly"/> (compiled from late definitions translated into source code) to the <see cref="Catalog"/>.
            /// </summary>
            /// <param name="sources">Source code generated from text definitions of late units.</param>
            /// <returns><see langword="true"/> on success, <see langword="true"/> on failure.</returns>
            public bool AddToCatalog(List<SyntaxTree> sources)
                => AddToCatalog(Compile(sources));

            private bool AddToCatalog(Assembly? asm)
            {
                if (asm is not null)
                {
                    try
                    {
                        Catalog.AppendFromAssembly(asm);
                        return true;
                    }
                    catch (ArgumentException ex) { ReportException(ex); }
                    catch (FieldAccessException ex) { ReportException(ex); }
                    catch (FileNotFoundException ex) { ReportException(ex); }
                    catch (NotSupportedException ex) { ReportException(ex); }
                    catch (TargetException ex) { ReportException(ex); }
                    catch (TargetInvocationException ex) { ReportException(ex); }
                }

                return false;

                void ReportException(Exception ex)
                    => _tc.Report(Path, $"could not extend Catalog with assembly \"{(asm.FullName ?? "null")}\"", ex);
            }

            #endregion
        }
    }
}
