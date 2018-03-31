using GitHelper.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitHelper.Helpers
{
    public class FileDialogService:IFileDialogService
    {
        public Stream OpenFile(string path)
        {
            throw new NotImplementedException();
        }

        public IList<string> SelectFilesDialog(out bool? result, string defaultPath = null, string extFilter = null)
        {
            var fd = new OpenFileDialog();
            fd.Multiselect = true;
            if (!string.IsNullOrWhiteSpace(extFilter))
            {
                fd.Filter = extFilter;
            }
            result = fd.ShowDialog();

            return fd.FileNames.ToList();
        }
    }
}
