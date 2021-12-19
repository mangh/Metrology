/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Metrological.Namespace
{
    public partial class RuntimeLoader
    {
        private class Compiler
        {
            #region Constants
            private const string DEFAULT_ASSEMBLY_NAME = "LateUnits";
            #endregion

            #region Fields
            private readonly RuntimeLoader m_loader;
            #endregion

            #region Properties
            #endregion

            #region Constructor(s)
            public Compiler(RuntimeLoader loader)
            {
                m_loader = loader;
            }
            #endregion

            #region Main methods
            public Assembly? CompileFromSource(string source, IEnumerable<Assembly> assemblies, string? targetAssemblyPath)
            {
                IEnumerable<PortableExecutableReference>? references = GetPortableExecutableReferences(assemblies);
                if (references is null)
                    return null;

                string? assemblyName = targetAssemblyPath is null ? DEFAULT_ASSEMBLY_NAME : m_loader.PathGetFileNameWithoutExtension(targetAssemblyPath);
                if (assemblyName is null)
                    return null;

                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

                CSharpCompilationOptions compileOptions = new(OutputKind.DynamicallyLinkedLibrary);

                CSharpCompilation? compilation = CSharpCompilation.Create(
                    assemblyName: assemblyName,
                    syntaxTrees: new[] { syntaxTree },
                    references: references,
                    options: compileOptions.WithNullableContextOptions(NullableContextOptions.Enable)
                );

                using (MemoryStream peStream = new())
                {
                    Microsoft.CodeAnalysis.Emit.EmitResult result = compilation.Emit(peStream);
                    if (result.Success)
                    {
                        if (targetAssemblyPath is not null)
                        {
                            AssemblySave(peStream, targetAssemblyPath);
                        }
                        return AssemblyLoad(peStream);
                    }
                    else
                    {
                        foreach (Diagnostic d in result.Diagnostics)
                        {
                            m_loader.ReportError(d.ToString());
                        }
                    }
                }
                return null;
            }
            #endregion

            #region Assembly methods
            private IEnumerable<PortableExecutableReference>? GetPortableExecutableReferences(IEnumerable<Assembly> assemblies)
            {
                // Required assemblies (with several excess items):
                IEnumerable<Assembly>? compilationAssemblies = 
                    assemblies
                    .Concat(System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies)
                    .Where(asm => !asm.IsDynamic);

                try
                {
                    //return compilationAssemblies.Select(asm => MetadataReference.CreateFromFile(asm.Location));
                    return compilationAssemblies.Select(asm => AssemblyMetadata.CreateFromFile(asm.Location).GetReference());
                }
                catch (ArgumentException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (IOException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (NotSupportedException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                return null;

                static string FormatErrorMessage(string message) =>
                    $"{nameof(RuntimeLoader)}.{nameof(GetPortableExecutableReferences)}(): {message}.";
            }

            public Assembly? AssemblyLoadFrom(string assemblyPath)
            {
                try
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
                // catch (ArgumentNullException ex) : ArgumentException
                catch (ArgumentException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                // catch (FileNotFoundException ex) : IOException
                // catch (FileLoadException ex) : IOException
                // catch (PathTooLongException ex) : IOException
                catch (IOException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (BadImageFormatException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (System.Security.SecurityException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                return null;

                string FormatErrorMessage(string message) =>
                    $"{nameof(RuntimeLoader)}.{nameof(AssemblyLoadFrom)}(\"{assemblyPath}\"): {message}.";
            }

            private Assembly? AssemblyLoad(MemoryStream portableExecutableStream)
            {
                try
                {
                    return Assembly.Load(portableExecutableStream.GetBuffer());
                }
                //catch (ArgumentNullException ex) : unlikely
                catch (UnauthorizedAccessException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (BadImageFormatException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                return null;

                static string FormatErrorMessage(string message) =>
                    $"{nameof(RuntimeLoader)}.{nameof(AssemblyLoad)}({nameof(portableExecutableStream)}): {message}.";
            }

            private bool AssemblySave(MemoryStream portableExecutableStream, string assemblyPath)
            {
                try
                {
                    using (FileStream dllStream = new(assemblyPath, FileMode.Create))
                    {
                        portableExecutableStream.Position = 0;
                        portableExecutableStream.CopyTo(dllStream);
                    }
                    return true;
                }
                //catch (ObjectDisposedException ex) : unlikely
                //catch (ArgumentNullException ex) : ArgumentException
                //catch (ArgumentOutOfRangeException ex) : ArgumentException
                catch (ArgumentException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (NotSupportedException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (System.Security.SecurityException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (UnauthorizedAccessException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                //catch (FileNotFoundException ex) : IOException
                //catch (DirectoryNotFoundException ex) : IOException
                //catch (PathTooLongException ex) : IOException
                catch (IOException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }

                return false;

                string FormatErrorMessage(string message) =>
                    $"{nameof(RuntimeLoader)}.{nameof(AssemblySave)}({nameof(portableExecutableStream)}, \"{assemblyPath}\"): {message}.";
            }
            #endregion

        }
    }
}
