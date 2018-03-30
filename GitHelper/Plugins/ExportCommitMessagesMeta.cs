using GitHelper.Attributes;
using GitHelper.Base;
using System.Collections.Generic;

namespace GitHelper.Plugins
{
    [GitHelperAction]
    public class ExportCommitMessagesMeta : GitHelperActionBase<ExportCommitMessages, ExportCommitMessageViewModel>
    {
        public override string Title => "Export Commit Messages";

        public override string Description => "Export commit messages in a specified format";

        public override IList<ActionFeatures> Features => new List<ActionFeatures>()
        {
            ActionFeatures.HasNewWindow, ActionFeatures.IsPlugin
        };
    }
}
