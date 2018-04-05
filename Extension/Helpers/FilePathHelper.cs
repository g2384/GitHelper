using System;
using System.Collections.Generic;
using System.IO;

namespace GitHelper.Extension.Helpers
{
    public class FilePathHelper
    {
        public static bool IsFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) != -1 || !Path.IsPathRooted(path))
                return false;

            var pathRoot = Path.GetPathRoot(path);
            if (pathRoot.Length <= 2 && pathRoot != "/") // Accepts X:\ and \\UNC\PATH, rejects empty string, \ and X:, but accepts / to support Linux
                return false;

            return !(pathRoot == path && pathRoot.StartsWith("\\\\") && pathRoot.IndexOf('\\', 2) == -1); // A UNC server name without a share name (e.g "\\NAME") is invalid
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

        public static List<string> GetRelativePaths(List<string> filePaths, string folder)
        {
            var relativePaths = new List<string>();
            foreach (var path in filePaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                relativePaths.Add(GetRelativePath(path, folder));
            }

            return relativePaths;
        }

        public static List<string> GetAbsolutePaths(List<string> filePaths, string currentPath)
        {
            var absolutePaths = new List<string>();
            foreach (var path in filePaths)
            {
                if (string.IsNullOrWhiteSpace(path)
                    || IsFullPath(path))
                {
                    absolutePaths.Add(path);
                    continue;
                }

                absolutePaths.Add(GetAbsolutePath(currentPath, path));
            }

            return absolutePaths;
        }

        public static string GetAbsolutePath(string filePath, string currentPath)
        {
            var path = Path.Combine(currentPath, filePath);
            return Path.GetFullPath(new Uri(path).LocalPath);
        }

        public static string GetCurrentDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }
    }
}
