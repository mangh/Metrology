/*******************************************************************************

    Units of Measurement for C#/C++ applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Mangh.Metrology
{
    /// <summary>
    /// Simplified version (replacement) of <see cref="System.IO.Path"/>.
    /// </summary>
    /// <remarks>
    /// NOTE: the replacement is to eliminate annoyingly frequent<br/>
    /// validation of path characters that can raise <see cref="System.ArgumentException"/><br/>
    /// (see <a href="https://referencesource.microsoft.com/#mscorlib/system/io/path.cs,090eca8621a248ee">Microsoft Reference Source</a>).
    /// </remarks>
    public static class FilePath
    {
        #region Methods
        /// <summary>
        /// Returns <see langword="true"/> for a character that can be used as a directory separator.
        /// </summary>
        public static bool IsDirectorySeparator(char ch)
            => (ch == System.IO.Path.DirectorySeparatorChar) || (ch == System.IO.Path.AltDirectorySeparatorChar);

        /// <summary>
        /// Returns <see langword="true"/> for a character that can be used as a volume separator.
        /// </summary>
        public static bool IsVolumeSeparator(char ch)
            => ch == System.IO.Path.VolumeSeparatorChar;

        /// <summary>
        /// Returns <see langword="true"/> for a path string containing a root.
        /// </summary>
        public static bool IsPathRooted(string path)
        {
            int length = path.Length;
            return
                (length >= 1 && IsDirectorySeparator(path[0])) ||
                (length >= 2 && IsVolumeSeparator(path[1]));
        }

        /// <summary>
        /// Returns the directory name for the specified path string.
        /// </summary>
        /// <param name="path">The path string.</param>
        public static string GetDirectoryName(string path)
        {
            for (int i = path.Length - 1; i >= 0; --i)
            {
                char ch = path[i];
                if (IsDirectorySeparator(ch) || IsVolumeSeparator(ch))
                {
                    return path.Substring(0, i);
                }
            }
            return path;

        }

        /// <summary>
        /// Gets the filename (with extension if any) from the file path string.
        /// </summary>
        /// <param name="path">The path string.</param>
        public static string GetFileName(string path)
        {
            for (int i = path.Length - 1; i >= 0; --i)
            {
                char ch = path[i];
                if (IsDirectorySeparator(ch) || IsVolumeSeparator(ch))
                {
                    return path.Substring(i + 1, path.Length - (i + 1));
                }
            }
            return path;
        }

        /// <summary>
        /// Gets the filename (without extension) from the file path string.
        /// </summary>
        /// <param name="path">The path string.</param>
        public static string GetFileNameWithoutExtension(string path)
        {
            string name = GetFileName(path);
            int i = name.LastIndexOf('.');
            return (i == -1) ? name : name.Substring(0, i);
        }

        /// <summary>
        /// Combines folder and filename strings into a path.
        /// </summary>
        /// <param name="folder">File directory.</param>
        /// <param name="filename">File name.</param>
        /// <returns>Path composed of the <paramref name="folder"/> and the <paramref name="filename"/> strings.</returns>
        public static string Combine(string folder, string filename)
        {
            if (filename.Length == 0)
                return folder;

            if (folder.Length == 0)
                return filename;

            if (IsPathRooted(filename))
                return filename;

            char ch = folder[folder.Length - 1];

            return (IsDirectorySeparator(ch) || IsVolumeSeparator(ch)) ? folder + filename : folder + System.IO.Path.DirectorySeparatorChar + filename;
        }

        /// <summary>
        /// Changes the extension of a path string.
        /// </summary>
        /// <param name="path">The path to modify.</param>
        /// <param name="ext">
        /// The new extension (with or without a leading period).
        /// Specify <see langword="null"/> to remove an existing extension from the path.
        /// </param>
        /// <returns>The modified path.</returns>
        public static string ChangeExtension(string path, string? ext)
        {
            string fnm = path;
            for (int i = path.Length - 1; i >= 0; --i)
            {
                char c = path[i];
                if (c == '.')
                {
                    fnm = path.Substring(0, i);
                    break;
                }
                if (IsDirectorySeparator(c) || IsVolumeSeparator(c))
                {
                    break;
                }
            }
            if (ext is not null)
            {
                if (ext.Length == 0 || ext[0] != '.')
                {
                    fnm += ".";
                }
                fnm += ext;
            }
            return fnm;
        }
        #endregion
    }
}
