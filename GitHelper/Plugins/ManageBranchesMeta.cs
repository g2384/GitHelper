using GitHelper.Attributes;
using GitHelper.Base;
using System.Collections.Generic;

namespace GitHelper.Plugins
{
    [GitHelperAction]
    public class ManageBranchesMeta : GitHelperActionBase<ManageBranches, ManageBranchesViewModel>
    {
        public override string Title => "Manage Branches";

        public override string Description => "Show which branch has been merged into a specific branch";

        public override IList<ActionFeatures> Features => new List<ActionFeatures>()
        {
            ActionFeatures.HasNewWindow, ActionFeatures.IsPlugin
        };
    }
}
