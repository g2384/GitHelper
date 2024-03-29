using System;
using System.Text.RegularExpressions;
using GitHelper.Common.Extensions;
using LibGit2Sharp;

namespace GitHelper.Extension
{
    public class BranchInfo
    {
        public string Name { get; set; }
        public string IsTracking { get; set; }
        public string IsLocal { get; set; }
        public string IsGone { get; set; }
        public string LastCommit { get; set; }
        public string LastCommitBy { get; set; }
        public string LastCommitByFullName { get; set; }
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
            LastCommitByFullName = LastCommitBy = lastCommit.Author.Name;
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
            return diff.ToWords();
        }
    }
}
