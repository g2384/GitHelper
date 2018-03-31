using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace GitHelper
{
    public class Configuration
    {
        [JsonIgnore]
        public const string ConfigFile = "config.json";

        public string RepoPath { get; set; } = @"D:\MyRepo";

        public string MergedInBranch { get; set; } = "master";

        public List<string> IgnoredBranches { get; set; } = new List<string>();

        public bool OnlyDeleteMerged { get; set; } = true;

        public bool ShowWarningBeforeDeletingBranch { get; set; } = true;

        public bool ShowWarningBeforeExecutingScript { get; set; } = true;

        public List<string> ExtensionPaths { get; set; } = new List<string>();

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

        public static Configuration GetConfiguration()
        {
            if (File.Exists(ConfigFile))
            {
                try
                {
                    return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigFile));
                }
                catch
                { }
            }

            return new Configuration();
        }

        public void Save()
        {
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
