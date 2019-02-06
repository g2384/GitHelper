using System;

namespace GitHelper.Extension.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RequireConfigAttribute : Attribute
    {
        public bool RequireConfig { get; }

        public RequireConfigAttribute(bool requireConfig)
        {
            RequireConfig = requireConfig;
        }
    }
}