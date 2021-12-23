/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

using Mangh.Metrology;
using Microsoft.CodeAnalysis.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Man.Metrology.XsltGeneratorApp
{
    internal class Generator
    {
        #region Properties
        public string TemplateFolder { get; }
        public string TargetFolder { get; }
        public string TargetNamespace { get; }

        /// <summary>
        /// Units loaded from a definition stream.
        /// </summary>
        public List<UnitType> Units { get; }
        public int UnitFamilyCount { get; private set; }

        /// <summary>
        /// Scales loaded from a definition stream.
        /// </summary>
        public List<ScaleType> Scales { get; }
        public int ScaleFamilyCount { get; private set; }
        #endregion

        #region Constructor(s)
        public Generator(string targetNamespace, string templateFolder, string targetFolder)
        {
            TargetNamespace = targetNamespace;
            TemplateFolder = templateFolder;
            TargetFolder = targetFolder;
            Units = new();
            Scales = new();
        }
        #endregion

        #region Methods
        public bool LoadDefinitions(string definitionsFilename, CancellationToken ct)
        {
            int errorCount = 0;

            string? defsPath = MakePath(TemplateFolder, definitionsFilename);
            if (defsPath is not null)
            {
                using (StreamReader reader = new(defsPath!))
                {
                    try
                    {
                        Lexer lexer = new(reader);
                        Parser parser = new(lexer, ReportParseError, Units, Scales);
                        parser.Parse(ct);
                        return errorCount == 0;
                    }
                    catch (IOException ex)
                    {
                        ReportException(FormatErrorMessage(ex.Message));
                    }
                    catch (ObjectDisposedException ex)
                    {
                        ReportException(FormatErrorMessage(ex.Message));
                    }
                    catch (InvalidOperationException ex)
                    {
                        ReportException(FormatErrorMessage(ex.Message));
                    }
                }
            }
            return false;

            string FormatErrorMessage(string message)
                => string.Format("{0} : definitions could not be read : {1}", defsPath, message);

            void ReportParseError(TextSpan extent, LinePositionSpan span, string message)
            {
                errorCount++;
                Console.WriteLine($"\"{defsPath}\" :: ({span}) :: {message}");
            }
        }

        public bool MakeUnits(string templateFilename, CancellationToken ct)
        {
            XslCompiledTransform? template = CompileTemplate(templateFilename);
            if (template is null)
                return false;

            UnitTranslator translator = new(TargetNamespace, late: false);
            foreach ((string name, string contents) in translator.Translate(ct, template, Units, initialFamily: 0, startIndex: 0))
            {
                if (!SaveToFile(name + ".cs", contents)) return false;
            }
            UnitFamilyCount = translator.Family - /*initialFamily*/ 0;
            return true;
        }

        public bool MakeScales(string templateFilename, CancellationToken ct)
        {
            XslCompiledTransform? template = CompileTemplate(templateFilename);
            if (template is null)
                return false;

            ScaleTranslator translator = new(TargetNamespace, late: false);
            foreach ((string name, string contents) in translator.Translate(ct, template, Scales, initialFamily: UnitFamilyCount, startIndex: 0))
            {
                if (!SaveToFile(name + ".cs", contents)) return false;
            }
            ScaleFamilyCount = translator.Family - UnitFamilyCount;
            return true;
        }

        public bool MakeCatalog(string templateFilename, CancellationToken ct)
        {
            XslCompiledTransform? template = CompileTemplate(templateFilename);
            if (template is null)
                return false;

            CatalogTranslator translator = new(TargetNamespace);
            (string name, string contents) = translator.Translate(ct, template, UnitFamilyCount, Units, ScaleFamilyCount, Scales);
            return SaveToFile(name + ".cs", contents);
        }

        public bool MakeAliases(string templateFilename, CancellationToken ct, bool global = true)
        {
            XslCompiledTransform? template = CompileTemplate(templateFilename);
            if (template is null)
                return false;

            AliasTranslator translator = new(TargetNamespace);
            return SaveToFile(translator.Translate(ct, template, Units, Scales, global));
        }

        public bool MakeReport(string templateFilename, CancellationToken ct)
        {
            XslCompiledTransform? template = CompileTemplate(templateFilename);
            if (template is null)
                return false;

            ReportTranslator translator = new(TargetNamespace);
            return SaveToFile(translator.Translate(ct, template, UnitFamilyCount, Units, ScaleFamilyCount, Scales));
        }

        private string? MakePath(string? folder, string? filename, bool requisite = true)
        {
            try
            {
                string path = Path.Combine(folder!, filename!);
                if (!requisite || File.Exists(path))
                {
                    return path;
                }
                Console.WriteLine($"{Quote(filename)} not found in {Quote(folder)}");
            }
            catch (System.ArgumentException ex)
            {
                Console.WriteLine($"{nameof(MakePath)}(folder: {Quote(folder)}, filename: {Quote(filename)}) :: {ex.Message}");
            }
            return null;

            static string Quote(string? arg)
            {
                return (arg is null) ? "null" : '"' + arg + '"';
            }
        }

        /// <summary>
        /// Load XSLT style sheet
        /// </summary>
        /// <param name="templateFilename">XSLT template file name (with extension)</param>
        /// <returns></returns>
        private XslCompiledTransform? CompileTemplate(string templateFilename)
        {
            string? templatePath = MakePath(TemplateFolder, templateFilename);
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
                catch (System.ArgumentException ex)
                {
                    ReportException(FormatErrorMessage(ex.Message));
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    ReportException(FormatErrorMessage(ex.Message));
                }
                catch (System.NotSupportedException ex)
                {
                    ReportException(FormatErrorMessage(ex.Message));
                }
                catch (System.IO.IOException ex)
                {
                    ReportException(FormatErrorMessage(ex.Message));
                }
                catch (XsltException ex)
                {
                    ReportException(FormatXsltErrorMessage(ex.LineNumber, ex.LinePosition, ex.Message));
                }
            }
            return null;

            string FormatErrorMessage(string message)
                => string.Format("\"{0}\": template could not be read :: {1}", templatePath, message);

            string FormatXsltErrorMessage(int lineNumber, int linePosition, string message)
                => string.Format("\"{0}({1}, {2})\": template could not be read :: {3}", templatePath, lineNumber, linePosition, message);
        }

        private bool SaveToFile(string filename, string contents)
        {
            string? filepath = MakePath(TargetFolder, filename, requisite: false);
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
                catch (System.ArgumentException ex)
                {
                    ReportException(FormatErrorMessage(ex.Message));
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    ReportException(FormatErrorMessage(ex.Message));
                }
                catch (System.NotSupportedException ex)
                {
                    ReportException(FormatErrorMessage(ex.Message));
                }
                catch (System.IO.IOException ex)
                {
                    ReportException(FormatErrorMessage(ex.Message));
                }
            }
            return false;

            string FormatErrorMessage(string message)
                => string.Format("\"{0}\": file could not be saved :: {1}", filepath, message);
        }

        private bool SaveToFile((string name, string contents) file)
            => SaveToFile(file.name, file.contents);
        
        private static void ReportException(string message)
            => Console.WriteLine(message);

        #endregion
    }
}
