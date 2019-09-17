using System;

namespace GitHelper.Common.Extensions
{
    // TODO use https://github.com/Humanizr/Humanizer
    public static class TimeSpanExtensions
    {
        public static string ToWords(this TimeSpan span)
        {
            if (span.TotalSeconds < 60)
            {
                return Math.Round(span.TotalSeconds) + " secs ago";
            }

            if (span.TotalMinutes < 60)
            {
                return Math.Round(span.TotalMinutes, 1) + " mins ago";
            }

            if (span.TotalHours < 24)
            {
                return Math.Round(span.TotalHours, 1) + " hrs ago";
            }

            return Math.Round(span.TotalDays) + " days ago";
        }
    }
}
