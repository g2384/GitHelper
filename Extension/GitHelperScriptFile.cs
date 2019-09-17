using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using GitHelper.Common.Extensions;
using GitHelper.Extension.Helpers;
using GitHelper.Extension.Interfaces;
using Newtonsoft.Json;

namespace GitHelper.Extension
{
    public class GitHelperScriptFile : IGitHelperExtensionFile, IJsonTypeConvertible<IGitHelperExtensionFile>
    {
        public string Name { get; private set; }

        public string Description { get; private set; }

        [JsonIgnore]
        public string ShortDescription { get; private set; }

        public IList<ExtensionFeature> Features { get; private set; }

        public string WorkingDirectory { get; }

        public string FilePath { get; set; }

        public GitHelperScriptFile()
        { }

        /// <summary>
        /// Extract extension info from .bat file
        /// </summary>
        /// <param name="lines">.bat file contents</param>
        /// <param name="filePath"></param>
        public GitHelperScriptFile(string[] lines, string filePath)
        {
            Init(lines, filePath);
        }

        private void Init(string[] lines, string filePath)
        {
            FilePath = filePath;
            var metaRegex = new Regex(@"\s*::\s*@(\w*)(.*)");
            foreach (var line in lines)
            {
                var match = metaRegex.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                if (match.Groups.Count < 3)
                {
                    continue;
                }

                var key = match.Groups[1].ToString().ToLower().Trim();
                var value = match.Groups[2].ToString().Trim();
                switch (key)
                {
                    case "name":
                        Name = value;
                        break;
                    case "description":
                        Description = value;
                        ShortDescription = value.Truncate(50);
                        break;
                }
            }

            if (Features == null)
            {
                Features = new List<ExtensionFeature>();
            }

            if (!Features.Contains(ExtensionFeature.IsScript))
            {
                Features.Add(ExtensionFeature.IsScript);
            }
        }

        public GitHelperScriptFile(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            Init(lines, filePath);
        }

        public GitHelperScriptFile(string filePath, string workingDirectory) : this(filePath)
        {
            WorkingDirectory = workingDirectory;
        }

        public GitHelperScriptFile(string name, string description, string filePath, string workingDirectory)
        {
            Name = name;
            Description = description;
            WorkingDirectory = workingDirectory;
            FilePath = filePath;
        }

        public void ToRelatvePath()
        {
            var currentPath = FilePathHelper.GetCurrentDirectory();
            FilePath = FilePathHelper.GetRelativePath(FilePath, currentPath);
        }

        public bool IsValid()
        {
            return IsValid(out var errors);
        }

        public bool IsValid(out List<string> errors)
        {
            errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add("name is empty");
            }

            return errors.IsNullOrEmpty();
        }

        public IGitHelperExtensionFile ConvertTo(Newtonsoft.Json.Linq.JObject obj)
        {
            return new GitHelperScriptFile(obj["Name"].ToString(), obj["Description"].ToString(),
                obj["FilePath"].ToString(), obj["WorkingDirectory"].ToString());
        }
    }

    public interface IJsonTypeConvertible<T>
    {
        T ConvertTo(Newtonsoft.Json.Linq.JObject obj);
    }
}
