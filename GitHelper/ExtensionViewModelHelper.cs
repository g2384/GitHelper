using GitHelper.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using GitHelper.Extension;
using GitHelper.Extension.Helpers;

namespace GitHelper
{
    public static class ExtensionViewModelHelper
    {
        public static List<IGitHelperExtensionFile> GetExtensionFiles(List<string> extensionPaths, bool showWariningIfFileIsNotFound = false)
        {
            var extensions = new List<IGitHelperExtensionFile>();
            if (Utility.IsNullOrEmpty(extensionPaths))
            {
                return extensions;
            }

            var currentPath = FilePathHelper.GetCurrentDirectory();
            var showedErrorMessage = new List<string>();
            foreach (var f in extensionPaths)
            {
                var filePath = f;
                var useRelativePath = !FilePathHelper.IsFullPath(filePath);
                if (useRelativePath)
                {
                    filePath = FilePathHelper.GetAbsolutePath(filePath, currentPath);
                }

                if (!File.Exists(filePath))
                {
                    if (showWariningIfFileIsNotFound)
                    {
                        MessageBox.Show($"Cannot find file \"{filePath}\"");
                    }

                    continue;
                }

                try
                {
                    IGitHelperExtensionFile extension = null;
                    var ext = Path.GetExtension(filePath)?.ToLower();
                    switch (ext)
                    {
                        case ".bat":
                            extension = new GitHelperScriptFile(filePath);
                            break;
                    }

                    if (extension != null)
                    {
                        if (extension.IsValid(out var extensionError))
                        {
                            if (useRelativePath)
                            {
                                extension.ToRelatvePath();
                            }

                            extensions.Add(extension);
                        }
                        else
                        {
                            MessageBox.Show(string.Join(", ", extensionError));
                        }
                    }
                }
                catch (Exception e)
                {
                    if (!showedErrorMessage.Contains(e.Message))
                    {
                        showedErrorMessage.Add(e.Message);
                        MessageBox.Show(e.Message);
                    }
                }
            }

            return extensions;
        }
    }
}
