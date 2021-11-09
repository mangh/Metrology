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

namespace Demo.UnitsOfMeasurement
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

            #region Methods
            public Assembly? CompileFromSource(string source, IEnumerable<Assembly> catalogAssemblies, string? targetAssemblyPath)
            {
                string? assemblyName = targetAssemblyPath is null ? DEFAULT_ASSEMBLY_NAME : Path.GetFileNameWithoutExtension(targetAssemblyPath);

                SyntaxTree? syntaxTree = CSharpSyntaxTree.ParseText(source);

                // Required assemblies (with several excess items):
                IEnumerable<Assembly>? compilationAssemblies = catalogAssemblies
                    .Concat(System.Runtime.Loader.AssemblyLoadContext.Default.Assemblies);

                IEnumerable<PortableExecutableReference>? references = compilationAssemblies
                    .Where(ass => !ass.IsDynamic)
                    .Select(ass => MetadataReference.CreateFromFile(ass.Location));

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
                            m_loader.m_io.AssemblySave(peStream, targetAssemblyPath);
                        }
                        return m_loader.m_io.AssemblyLoad(peStream, targetAssemblyPath ?? DEFAULT_ASSEMBLY_NAME);
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

            //private bool SaveAssembly(MemoryStream peStream, string? dllPath)
            //{
            //    try
            //    {
            //        using (FileStream dllStream = new(dllPath, FileMode.Create))
            //        {
            //            peStream.Position = 0;
            //            peStream.CopyTo(dllStream);
            //        }
            //        return true;
            //    }
            //    //catch (ArgumentOutOfRangeException ex)
            //    catch (ArgumentException ex)
            //    {
            //        m_errors.Add(FormatErrorMessage(ex.Message));
            //    }
            //    catch (NotSupportedException ex)
            //    {
            //        m_errors.Add(FormatErrorMessage(ex.Message));
            //    }
            //    catch (System.Security.SecurityException ex)
            //    {
            //        m_errors.Add(FormatErrorMessage(ex.Message));
            //    }
            //    catch (UnauthorizedAccessException ex)
            //    {
            //        m_errors.Add(FormatErrorMessage(ex.Message));
            //    }
            //    //catch (FileNotFoundException ex)
            //    //catch (DirectoryNotFoundException ex)
            //    //catch (PathTooLongException ex)
            //    catch (IOException ex)
            //    {
            //        m_errors.Add(FormatErrorMessage(ex.Message));
            //    }

            //    return false;

            //    string FormatErrorMessage(string message)
            //        => string.Format("\"{0}\": DLL could not be saved :: {1}", dllPath, message);
            //}

            //private Assembly? LoadAssembly(MemoryStream peStream, string? dllPath)
            //{
            //    try
            //    {
            //        return Assembly.Load(peStream.GetBuffer());
            //    }
            //    catch (ArgumentNullException ex)
            //    {
            //        m_errors.Add(FormatErrorMessage(ex.Message));
            //    }
            //    catch (BadImageFormatException ex)
            //    {
            //        m_errors.Add(FormatErrorMessage(ex.Message));
            //    }
            //    return null;

            //    string FormatErrorMessage(string message)
            //        => string.Format("\"{0}\": DLL could not be loaded (from stream) :: {1}", dllPath is null ? "null" : dllPath, message);

            //}
            #endregion
        }
    }
}
