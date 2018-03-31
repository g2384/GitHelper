using GitHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;

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
                            var lines = File.ReadAllLines(filePath);
                            extension = new GitHelperScriptFile(lines, filePath);
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

    public static class FlowDocumentExtensions
    {
        public static string ToXaml(this FlowDocument flowDocument)
        {
            if (flowDocument == null)
            {
                return null;
            }

            // write XAML out to a MemoryStream
            TextRange tr = new TextRange(
                flowDocument.ContentStart,
                flowDocument.ContentEnd);

            string xamlText;
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                xamlText = ASCIIEncoding.Default.GetString(ms.ToArray());
            }

            return xamlText.ToString();
        }
    }
}
