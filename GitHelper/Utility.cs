using System;
using System.Collections.Generic;
using System.IO;

namespace GitHelper
{
    public static class Utility
    {
        public static bool IsNullOrEmpty<T>(List<T> formats)
        {
            return formats == null || formats.Count == 0;
        }

        public static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            string pathToUnescape = folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar);
            return Uri.UnescapeDataString(pathToUnescape);
        }

        internal static List<string> GetRelativePaths(List<string> selectedFilePaths, string folder)
        {
            var relativePaths = new List<string>();
            foreach(var path in selectedFilePaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                relativePaths.Add(GetRelativePath(path, folder));
            }

            return relativePaths;
        }
    }
}
