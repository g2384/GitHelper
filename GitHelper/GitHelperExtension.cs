using System.Collections.Generic;
using System.Text.RegularExpressions;
using GitHelper.Interfaces;

namespace GitHelper
{
    public class GitHelperExtension : IGitHelperActionScriptBase
    {
        public GitHelperExtension(string[] lines)
        {
            var metaRegex = new Regex(@"\s*::\s*@(\w*)(.*)");
            foreach (var line in lines)
            {
                var match = metaRegex.Match(line);
                if (!match.Success)
                {
                    return;
                }

                if (match.Groups.Count < 3)
                {
                    return;
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
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string ShortDescription { get; private set; }

        public IList<ActionFeatures> Features { get; private set; }
    }
}