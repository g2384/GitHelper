namespace GitHelper.Helpers
{
    public static class StringExtensions
    {
        public static string Truncate(this string text, int length)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (text.Length <= length)
            {
                return text;
            }

            return text.Substring(0, length - 1) + "…";
        }
    }
}
