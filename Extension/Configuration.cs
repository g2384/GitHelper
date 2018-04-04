using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using GitHelper.Extension.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GitHelper.Extension
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

        public List<IGitHelperExtensionFile> Extensions { get; set; } = new List<IGitHelperExtensionFile>();

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
                    var se = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                    };
                    se.Converters.Add(new CogConverter());
                    var configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigFile),
                        se);

                    if (configuration.Extensions == null)
                    {
                        configuration.Extensions = new List<IGitHelperExtensionFile>();
                    }

                    return configuration;
                }
                catch (Exception e)
                { }
            }

            return new Configuration();
        }

        public void Save()
        {
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class CogConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IGitHelperExtensionFile);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var target = serializer.Deserialize<Newtonsoft.Json.Linq.JObject>(reader);
            var fileInfo = new GitHelperScriptFile(target["Name"].ToString(), target["Description"].ToString(),
                target["FilePath"].ToString(), target["WorkingDirectory"].ToString());
            //serializer.Populate(target.CreateReader(), target);
            return fileInfo;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

}
