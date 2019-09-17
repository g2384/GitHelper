using System.Collections.Generic;

namespace GitHelper.Extension.Interfaces
{
    public interface IGitHelperExtension
    {
        string Name { get; }
        string Description { get; }
        IList<ExtensionFeature> Features { get; }
    }
}
