using System.Collections.Generic;

namespace ExportCommitMessages
{
    public class Utility
    {
        public static bool IsNullOrEmpty<T>(IList<T> formats)
        {
            return formats == null || formats.Count == 0;
        }
    }
}
