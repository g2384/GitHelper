using GitHelper.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

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

            var showedErrorMessage = new List<string>();
            foreach (var filePath in extensionPaths)
            {
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
                    var ext = Path.GetExtension(filePath).ToLower();
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
                        MessageBox.Show(e.Message);
                    }
                }
            }

            return extensions;
        }
    }
}
