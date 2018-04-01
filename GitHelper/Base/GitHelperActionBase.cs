using GitHelper.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;

namespace GitHelper.Base
{
    public abstract class GitHelperActionBase<TWindow, TViewModel> : IGitHelperActionMeta
        where TWindow : Window, new()
        where TViewModel: ActionViewModelBase
    {
        public abstract string Name { get; }
               
        public abstract string Description { get; }

        public abstract IList<ExtensionFeatures> Features { get; }

        public void ShowDialog(Configuration configuration)
        {
            var dialog = new TWindow()
            {
                DataContext = Activator.CreateInstance(typeof(TViewModel), configuration)
            };

            dialog.ShowDialog();
        }
    }
}
