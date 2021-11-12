/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/Metrology


********************************************************************************/
using System;
using System.IO;

namespace Demo.UnitsOfMeasurement
{
    // Exception-aware I/O
    public partial class RuntimeLoader
    {
        #region RuntimeLoader directories
        private string? GetRootFolder()
        {
            try
            {
                return PathGetDirectoryName(typeof(RuntimeLoader).Assembly.Location);
            }
            catch (NotSupportedException ex)
            {
                ReportError($"{nameof(RuntimeLoader)}.{nameof(GetRootFolder)}(): could not get / find root directory: {ex.Message}.");
            }
            return null;
        }

        private string? GetSubfolder(string subfolderName)
        {
            string? rootFolder = GetRootFolder();
            return (rootFolder is null) ? null : PathCombine(rootFolder, subfolderName);
        }
        #endregion

        #region Path/Folder Methods
        private string? PathGetDirectoryName(string path)
        {
            try
            {
                return Path.GetDirectoryName(path);
            }
            catch (ArgumentException ex)
            {
                ReportError(FormatErrorMessage(ex.Message));
            }
            catch (/*PathTooLongException*/IOException ex)
            {
                ReportError(FormatErrorMessage(ex.Message));
            }
            return null;

            string FormatErrorMessage(string message) =>
                $"{nameof(RuntimeLoader)}.{nameof(PathGetDirectoryName)}(\"{path}\"): {message}.";
        }

        private string? PathCombine(string folder, string filename)
        {
            try
            {
                return Path.Combine(folder, filename);
            }
            catch (ArgumentException ex)
            {
                ReportError($"{nameof(RuntimeLoader)}.{nameof(PathCombine)}(\"{folder}\", \"{filename}\"): {ex.Message}.");
            }
            return null;
        }

        private string? PathGetFileNameWithoutExtension(string path)
        {
            try
            {
                return Path.GetFileNameWithoutExtension(path);
            }
            catch (ArgumentException ex)
            {
                ReportError($"{nameof(RuntimeLoader)}.{nameof(PathGetFileNameWithoutExtension)}(\"{path}\"): {ex.Message}.");
            }
            return null;
        }

        private string? PathChangeExtension(string path, string ext)
        {
            try
            {
                return Path.ChangeExtension(path, ext);
            }
            catch (ArgumentException ex)
            {
                ReportError($"{nameof(RuntimeLoader)}.{nameof(PathChangeExtension)}(\"{path}\", \"{ext}\"): {ex.Message}.");
            }
            return null;
        }
        #endregion

        #region Text File Methods
        private StreamReader? FileOpenText(string filePath)
        {
            try
            {
                return File.OpenText(filePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                ReportError(FormatErrorMessage(ex.Message));
            }
            //catch (ArgumentNullException ex) : ArgumentException
            catch (ArgumentException ex)
            {
                ReportError(FormatErrorMessage(ex.Message));
            }
            //catch (PathTooLongException ex) : IOException
            //catch (DirectoryNotFoundException ex) : IOException
            //catch (FileNotFoundException ex) : IOException
            catch (IOException ex)
            {
                ReportError(FormatErrorMessage(ex.Message));
            }
            catch (NotSupportedException ex)
            {
                ReportError(FormatErrorMessage(ex.Message));
            }

            return null;

            string FormatErrorMessage(string message) =>
                $"{nameof(RuntimeLoader)}.{nameof(FileOpenText)}(\"{filePath}\"): {message}.";
        }

        private bool FileSaveText(string filePath, string contents)
        {
            try
            {
                using (StreamWriter writer = new(filePath))
                {
                    writer.Write(contents);
                }
                return true;
            }
            catch (ArgumentException ex)
            {
                ReportError(FormatErrorMessage(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                ReportError(FormatErrorMessage(ex.Message));
            }
            catch (NotSupportedException ex)
            {
                ReportError(FormatErrorMessage(ex.Message));
            }
            catch (IOException ex)
            {
                ReportError(FormatErrorMessage(ex.Message));
            }
            return false;

            string FormatErrorMessage(string message) =>
                $"{nameof(RuntimeLoader)}.{nameof(FileSaveText)}(\"{filePath}\", ...): {message}.";
        }
        #endregion

   }
}