using System.Collections.Generic;

namespace GitHelper
{
    public static class Utility
    {
        public static bool IsNullOrEmpty(List<CommitExportFormat> formats)
        {
            return formats == null || formats.Count == 0;
        }
    }
}
