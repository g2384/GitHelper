using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace GitHelper
{
    public static class Utility
    {
        public static bool? ShowDialog(Window window)
        {
            window.Owner = Application.Current.MainWindow;
            return window.ShowDialog();
        }

        public static bool IsNullOrEmpty<T>(IList<T> formats)
        {
            return formats == null || formats.Count == 0;
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
