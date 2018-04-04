using System.Collections.Generic;
using JsonSubTypes;
using Newtonsoft.Json;

namespace GitHelper.Extension.Interfaces
{
    public interface IGitHelperExtension
    {
        string Name { get; }
        string Description { get; }
        IList<ExtensionFeatures> Features { get; }
    }
}
