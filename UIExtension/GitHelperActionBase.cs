using GitHelper.Extension;
using GitHelper.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using GitHelper.Extension.Attributes;

namespace GitHelper.UIExtension
{
    public abstract class GitHelperActionBase<TWindow, TViewModel> : IGitHelperActionMeta
        where TWindow : Window, new()
        where TViewModel: ActionViewModelBase
    {
        public abstract string Name { get; }
               
        public abstract string Description { get; }

        public abstract IList<ExtensionFeature> Features { get; }

        public void ShowDialog(Configuration configuration)
        {
            var requireConfig = typeof(TWindow).GetCustomAttribute(typeof(RequireConfigAttribute), true) as RequireConfigAttribute;
            var dialog = new TWindow();
            if (requireConfig?.RequireConfig != false) {
                dialog.DataContext = Activator.CreateInstance(typeof(TViewModel), configuration);
            }

            Utility.ShowDialog(dialog);
        }
    }
}
