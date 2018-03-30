using System;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace GitHelper
{
    public class BranchInfo
    {
        public string Name { get; set; }
        public string IsTracking { get; set; }
        public string IsLocal { get; set; }
        public string LastCommit { get; set; }
        public string LastCommitDate { get; set; }
        public string MergedInDate { get; set; }
        public string MergedInDateTime { get; set; }
        public string LastCommitDateTime { get; set; }
        public const string CheckMark = "\u2713";

        public BranchInfo(string name, bool isTracking, bool isLocal, Commit lastCommit, Commit mergedInDate)
        {
            Name = name;
            IsTracking = isTracking ? CheckMark : "";
            IsLocal = isLocal ? CheckMark : "";
            LastCommit = lastCommit.Message.Trim();
            LastCommit = new Regex(@"[\r\n]+").Replace(LastCommit, "\n");
            LastCommitDate = GetDateDifference(lastCommit.Author.When);
            LastCommitDateTime = lastCommit.Author.When.ToString("dddd, dd MMM yyyy HH:mm:ss");
            if (mergedInDate != null)
            {
                MergedInDateTime = mergedInDate.Author.When.ToString("dddd, dd MMM yyyy HH:mm:ss");
                MergedInDate = GetDateDifference(mergedInDate.Author.When);
            }
        }

        public BranchInfo(string name)
        {
            Name = name;
        }

        public static string GetDateDifference(DateTimeOffset time)
        {
            var diff = DateTime.Now - time;
            if (diff.TotalSeconds < 60)
            {
                return Math.Round(diff.TotalSeconds) + " secs ago";
            }

            if (diff.TotalMinutes < 60)
            {
                return Math.Round(diff.TotalMinutes, 1) + " mins ago";
            }

            if (diff.TotalHours < 24)
            {
                return Math.Round(diff.TotalHours, 1) + " hrs ago";
            }

            //if (diff.TotalDays < 30)
            {
                return Math.Round(diff.TotalDays) + " days ago";
            }
        }
    }
}
