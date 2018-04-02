﻿using GitHelper.Extension;
using System.Collections.Generic;

namespace GitHelper.Interfaces
{
    public interface IGitHelperExtension
    {
        string Name { get; }
        string Description { get; }
        IList<ExtensionFeatures> Features { get; }
    }
}