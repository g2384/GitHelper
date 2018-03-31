using System.Collections.Generic;

namespace GitHelper.Interfaces
{
    public interface IGitHelperExtension
    {
        string Name { get; }
        string Description { get; }
        IList<ActionFeatures> Features { get; }
    }
}