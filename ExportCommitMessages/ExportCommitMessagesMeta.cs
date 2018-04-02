using GitHelper.Extension;
using GitHelper.Extension.Attributes;
using GitHelper.UIExtension;
using System.Collections.Generic;

namespace ExportCommitMessages
{
    [GitHelperAction]
    public class ExportCommitMessagesMeta : GitHelperActionBase<MainWindow, ExportCommitMessageViewModel>
    {
        public override string Name => "Export Commit Messages";

        public override string Description => "Export commit messages in a specified format";

        public override IList<ExtensionFeatures> Features => new List<ExtensionFeatures>()
        {
            ExtensionFeatures.HasNewWindow, ExtensionFeatures.IsAssembly
        };
    }
}
