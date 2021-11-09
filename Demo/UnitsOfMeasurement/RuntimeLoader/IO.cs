using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;

namespace Demo.UnitsOfMeasurement
{
    public partial class RuntimeLoader
    {
        internal class IO
        {
            #region Fields
            private readonly RuntimeLoader m_loader;
            #endregion

            #region Properties
            #endregion

            #region Constructor(s)
            public IO(RuntimeLoader loader)
            {
                m_loader = loader;
            }
            #endregion

            #region Path/Folder Methods
            public string? GetSubfolder(string templateSubfolder)
            {
                string? rootFolder = null;
                try
                {
                    rootFolder = Path.GetDirectoryName(typeof(RuntimeLoader).Assembly.Location);
                    if (!string.IsNullOrWhiteSpace(rootFolder))
                    {
                        string? subFolder = MakePath(rootFolder, templateSubfolder);
                        if ((subFolder is null) || Directory.Exists(subFolder))
                        {
                            // It can be null when errors occured in MakePath
                            // but these where already reported:
                            return subFolder;
                        }
                    }
                    m_loader.ReportError(FormatErrorMessage());
                }
                catch (ArgumentException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (/*PathTooLongException*/IOException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (NotSupportedException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                return null;

                string FormatErrorMessage(string? message = null) =>
                    string.Format("{0}.{1}.{2}(\"{3}\"): folder not found / not available at: \"{4}\"{5}.",
                            nameof(RuntimeLoader),
                            nameof(IO),
                            nameof(GetSubfolder),
                            templateSubfolder,
                            rootFolder,
                            message is null ? string.Empty : " :: " + message
                    );
            }

            public string? MakePath(string folder, string filename)
            {
                try
                {
                    return Path.Combine(folder, filename);
                }
                catch (ArgumentException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                return null;

                string FormatErrorMessage(string message) =>
                        string.Format("{0}.{1}.{2}(\"{3}\", \"{4}\"): {5}",
                            nameof(RuntimeLoader),
                            nameof(IO),
                            nameof(MakePath),
                            folder,
                            filename,
                            message
                        );
            }

            public string? ChangeExtension(string path, string ext)
            {
                try
                {
                    return Path.ChangeExtension(path, ext);
                }
                catch (ArgumentException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                return null;

                string FormatErrorMessage(string message) =>
                    string.Format("{0}.{1}.{2}(\"{3}\", \"{4}\"): invalid path :: {5}",
                        nameof(RuntimeLoader),
                        nameof(IO),
                        nameof(ChangeExtension),
                        path,
                        ext,
                        message
                    );
            }
            #endregion

            #region Assembly Methods
            public Assembly? AssemblyLoad(string assemblyPath)
            {
                try
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
                // catch (ArgumentNullException ex)
                catch (ArgumentException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                // catch (FileNotFoundException ex)
                // catch (FileLoadException ex)
                // catch (PathTooLongException ex)
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
                    string.Format("{0}.{1}.{2}(\"{3}\"): assembly could not be loaded :: {4}.",
                        nameof(RuntimeLoader),
                        nameof(IO),
                        nameof(AssemblyLoad),
                        assemblyPath,
                        message
                    );
            }

            public Assembly? AssemblyLoad(MemoryStream peStream, string assemblyPath)
            {
                try
                {
                    return Assembly.Load(peStream.GetBuffer());
                }
                catch (ArgumentNullException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (BadImageFormatException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                return null;

                string FormatErrorMessage(string message) =>
                    string.Format("{0}.{1}.{2}(..., \"{3}\"): assembly could not be loaded from stream :: {4}",
                        nameof(RuntimeLoader),
                        nameof(IO),
                        nameof(AssemblyLoad),
                        assemblyPath, 
                        message
                    );
            }

            public bool AssemblySave(MemoryStream peStream, string assemblyPath)
            {
                try
                {
                    using (FileStream dllStream = new(assemblyPath, FileMode.Create))
                    {
                        peStream.Position = 0;
                        peStream.CopyTo(dllStream);
                    }
                    return true;
                }
                //catch (ArgumentOutOfRangeException ex)
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
                //catch (FileNotFoundException ex)
                //catch (DirectoryNotFoundException ex)
                //catch (PathTooLongException ex)
                catch (IOException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }

                return false;

                string FormatErrorMessage(string message) =>
                    string.Format("{0}.{1}.{2}(..., \"{3}\"): assembly could not be saved from stream:: {4}",
                        nameof(RuntimeLoader),
                        nameof(IO),
                        nameof(AssemblySave),
                        assemblyPath, 
                        message
                    );
            }
            #endregion

            #region Text File Methods
            public StreamReader? FileOpen(string lateDefinitionsTxtPath)
            {
                try
                {
                    return File.OpenText(lateDefinitionsTxtPath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                //catch (ArgumentNullException ex)
                catch (ArgumentException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                //catch (PathTooLongException ex)
                //catch (DirectoryNotFoundException ex)
                //catch (FileNotFoundException ex)
                catch (IOException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }
                catch (NotSupportedException ex)
                {
                    m_loader.ReportError(FormatErrorMessage(ex.Message));
                }

                return null;

                string FormatErrorMessage(string message) =>
                    string.Format("{0}.{1}.{2}(\"{3}\",...): file could not be open :: {4}",
                        nameof(RuntimeLoader),
                        nameof(IO),
                        nameof(FileOpen),
                        lateDefinitionsTxtPath,
                        message
                    );
            }

            public bool FileSave(string? filepath, string contents)
            {
                if (filepath is not null)
                {
                    try
                    {
                        using (StreamWriter writer = new(filepath))
                        {
                            writer.Write(contents);
                        }
                        return true;
                    }
                    catch (ArgumentException ex)
                    {
                        m_loader.ReportError(FormatErrorMessage(ex.Message));
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        m_loader.ReportError(FormatErrorMessage(ex.Message));
                    }
                    catch (NotSupportedException ex)
                    {
                        m_loader.ReportError(FormatErrorMessage(ex.Message));
                    }
                    catch (IOException ex)
                    {
                        m_loader.ReportError(FormatErrorMessage(ex.Message));
                    }
                }

                return false;

                string FormatErrorMessage(string message) =>
                    string.Format("{0}.{1}.{2}(\"{3}\",...): file could not be saved :: {4}",
                        nameof(RuntimeLoader),
                        nameof(IO),
                        nameof(FileSave),
                        filepath,
                        message
                    );
            }
            #endregion

            #region Xslt-Template Methods
            public XslCompiledTransform? CompileXsltTemplate(string templateFolder, string templateFilename)
            {
                string? templatePath = m_loader.m_io.MakePath(templateFolder, templateFilename);
                if (templatePath is not null)
                {
                    try
                    {
                        using (XmlReader rdr = XmlReader.Create(File.OpenText(templatePath)))
                        {
                            XslCompiledTransform xslt = new();
                            xslt.Load(rdr);
                            return xslt;
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        m_loader.ReportError(FormatErrorMessage(ex.Message));
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        m_loader.ReportError(FormatErrorMessage(ex.Message));
                    }
                    catch (NotSupportedException ex)
                    {
                        m_loader.ReportError(FormatErrorMessage(ex.Message));
                    }
                    catch (IOException ex)
                    {
                        m_loader.ReportError(FormatErrorMessage(ex.Message));
                    }
                    catch (XsltException ex)
                    {
                        m_loader.ReportError(FormatXsltErrorMessage(ex.LineNumber, ex.LinePosition, ex.Message));
                    }
                }

                return null;

                string FormatErrorMessage(string message)
                    => string.Format("\"{0}\": template could not be read :: {1}", templatePath, message);

                string FormatXsltErrorMessage(int lineNumber, int linePosition, string message)
                    => string.Format("\"{0}({1}, {2})\": template could not be read :: {3}", templatePath, lineNumber, linePosition, message);

            }
            #endregion
        }
    }
}