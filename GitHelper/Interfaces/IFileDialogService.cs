using System.Collections.Generic;
using System.IO;

namespace GitHelper.Interfaces
{
    public interface IFileDialogService
    {
        IList<string> SelectFilesDialog(out bool? result, string defaultPath = null, string extFilter = null);

        //Other similar untestable IO operations
        Stream OpenFile(string path);
    }
}
