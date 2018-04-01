using GitHelper.Attributes;
using GitHelper.Base;
using System.Collections.Generic;

namespace GitHelper.Plugins
{
    [GitHelperAction]
    public class ExportCommitMessagesMeta : GitHelperActionBase<ExportCommitMessages, ExportCommitMessageViewModel>
    {
        public override string Name => "Export Commit Messages";

        public override string Description => "Export commit messages in a specified format";

        public override IList<ExtensionFeatures> Features => new List<ExtensionFeatures>()
        {
            ExtensionFeatures.HasNewWindow, ExtensionFeatures.IsAssembly
        };
    }
}
