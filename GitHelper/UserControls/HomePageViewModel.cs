using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GitHelper.Extension;

namespace GitHelper.UserControls
{
    public enum HomePageViewType
    {
        Info,
        Settings
    }

    public class HomePageViewModel : ViewModelBase
    {
        public ExtensionInfoViewModel ExtensionInfoViewModel { get; private set; }

        public ManageExtensionsPageViewModel ManageExtensionsPageViewModel { get; }

        private int _tabIndex;
        public int TabIndex
        {
            get => _tabIndex;
            set
            {
                _tabIndex = value;
                if ((HomePageViewType)_tabIndex == HomePageViewType.Info)
                {
                    ShowStartInfo();
                }

                RaisePropertyChanged("TabIndex");
            }
        }

        public HomePageViewModel(Configuration configuration)
        {
            ManageExtensionsPageViewModel = new ManageExtensionsPageViewModel(configuration);
            ExtensionInfoViewModel = new ExtensionInfoViewModel();
        }

        public void ShowExtensionInfo(ExtensionInfo extensionInfo)
        {
            if (extensionInfo == null)
            {
                return;
            }

            ExtensionInfoViewModel.FullInfo = extensionInfo.GetFullInfo();
        }

        public void ShowStartInfo()
        {
            ExtensionInfoViewModel.FullInfo = Utility.GetResourceText("pack://application:,,,/HtmlPages/StartPage.html");
        }

        public void ShowTabItem(HomePageViewType viewType)
        {
            TabIndex = (int) viewType;
        }
    }
}
