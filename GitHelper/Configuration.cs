using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace GitHelper
{
    public class Configuration
    {
        public string RepoPath { get; set; } = @"D:\MyRepo";

        public string MergedInBranch { get; set; } = "master";

        public List<string> IgnoredBranches { get; set; } = new List<string>();

        public bool OnlyDeleteMerged { get; set; } = true;

        [JsonIgnore]
        public string IgnoredBranchesString
        {
            get
            {
                if (IgnoredBranches == null || IgnoredBranches.Count == 0)
                {
                    return "";
                }

                return string.Join(", ", IgnoredBranches);
            }
            set => IgnoredBranches = new Regex(@"[; ,]+").Split(value).ToList();
        }
    }
}
