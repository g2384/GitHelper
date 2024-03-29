﻿using GitHelper.Extension.Attributes;
using System.Collections.Generic;
using GitHelper.UIExtension;
using GitHelper.Extension;

namespace ManageBranches
{
    [GitHelperAction]
    public class ManageBranchesMeta : GitHelperActionBase<MainWindow, ManageBranchesViewModel>
    {
        public override string Name => "Manage Branches";

        public override string Description => "Show which branch has been merged into a specified branch";

        public override IList<ExtensionFeature> Features => new List<ExtensionFeature>()
        {
            ExtensionFeature.HasNewWindow, ExtensionFeature.IsAssembly
        };
    }
}
