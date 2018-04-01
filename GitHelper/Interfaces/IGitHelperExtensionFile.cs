﻿using System.Collections.Generic;

namespace GitHelper.Interfaces
{
    public interface IGitHelperExtensionFile:IGitHelperExtension
    {
        string FilePath { get; }
        string ShortDescription { get; }
        string WorkingDirectory { get; }

        bool IsValid();
        bool IsValid(out List<string> errors);
    }
}
