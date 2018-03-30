using System.Collections.Generic;

namespace GitHelper
{
    public static class Utility
    {
        public static bool IsNullOrEmpty<T>(List<T> formats)
        {
            return formats == null || formats.Count == 0;
        }
    }
}
