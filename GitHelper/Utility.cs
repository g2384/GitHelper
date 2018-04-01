using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace GitHelper
{
    public static class Utility
    {
        public static bool IsNullOrEmpty<T>(IList<T> formats)
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

        public static string GetResourceText(string uriLink, Encoding encoding = null)
        {
            var text = "";
            var uri = new Uri(uriLink);
            var stream = Application.GetResourceStream(uri);
            if (encoding == null)
            {
                encoding = Encoding.GetEncoding("UTF-8");
            }

            using (var reader = new StreamReader(stream.Stream, encoding))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }
    }
}
