using System.Collections.Generic;

namespace GitHelper.Interfaces
{
    public interface IGitHelperActionMeta
    {
        string Title { get; }
        string Description { get; }
        IList<ActionFeatures> Features { get; }

        void ShowDialog(Configuration configuration);
    }
}