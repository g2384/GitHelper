﻿using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using GitHelper.Interfaces;

namespace GitHelper
{
    public class GitHelperScriptFile : IGitHelperExtensionFile
    {
        public string Name { get; private set; }

        public string Description { get; private set; }

        public string ShortDescription { get; private set; }

        public IList<ExtensionFeatures> Features { get; private set; }

        public string WorkingDirectory { get; private set; }

        public string FilePath { get; set; }

        /// <summary>
        /// Extract extension info from .bat file
        /// </summary>
        /// <param name="lines">.bat file contents</param>
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
                Features = new List<ExtensionFeatures>();
            }

            if (!Features.Contains(ExtensionFeatures.IsScript))
            {
                Features.Add(ExtensionFeatures.IsScript);
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

            return Utility.IsNullOrEmpty(errors);
        }
    }
}