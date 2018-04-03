using System.Collections.Generic;

namespace GitHelper.Extension
{
    public class Utility
    {
        public static bool IsNullOrEmpty<T>(IList<T> formats)
        {
            return formats == null || formats.Count == 0;
        }
    }
}
