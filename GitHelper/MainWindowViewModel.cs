using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitHelper.Extension;
using GitHelper.UserControls;
using NLog;
using System;
using System.Windows.Input;

namespace GitHelper
{
    public enum MainWindowControlType
    {
        Home = 0,
        Settings = 1,
        Help = 2,
    }

    public enum MainWindowDisabledButton
    {
        Home = 0,
        ManageExtensions = 1,
        Settings = 2,
        Help = 3
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private int _disabledButton;

        public int DisabledButton
        {
            get => _disabledButton;
            set
            {
                _disabledButton = value;
                RaisePropertyChanged(nameof(DisabledButton));
            }
        }

        public Configuration Config { get; set; }

        private static Logger _log = LogManager.GetCurrentClassLogger();

        private int _tabIndex;
        public int TabIndex
        {
            get => _tabIndex;
            set
            {
                _tabIndex = value;
                var controlType = (MainWindowControlType)_tabIndex;
                if ((MainWindowControlType)_tabIndex == MainWindowControlType.Home)
                {
                    HomePageViewModel.ShowTabItem(HomePageViewType.Info);
                }

                switch (controlType)
                {
                    case MainWindowControlType.Home:
                        break;
                    case MainWindowControlType.Settings:
                        DisabledButton = (int)MainWindowDisabledButton.Settings;
                        break;
                    case MainWindowControlType.Help:
                        DisabledButton = (int)MainWindowDisabledButton.Help;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                RaisePropertyChanged("TabIndex");
            }
        }

        public ICommand OpenSettingsCommand { get; private set; }

        public ICommand OpenManageExtensionCommand { get; private set; }

        public ICommand RefreshCommand { get; private set; }

        public ICommand OpenHomeCommand { get; private set; }

        public HomePageViewModel HomePageViewModel { get; private set; }

        public MainWindowViewModel()
        {
            Config = Configuration.GetConfiguration();
            SettingsPageViewModel = new SettingsPageViewModel();
            HomePageViewModel = new HomePageViewModel(Config);
            DisplayHomeScreen();
            OpenSettingsCommand = new RelayCommand(() => ShowTabItem(MainWindowControlType.Settings));
            OpenManageExtensionCommand = new RelayCommand(() => ShowHomeTabItem(HomePageViewType.Settings));
            RefreshCommand = new RelayCommand(Refresh);
            OpenHomeCommand = new RelayCommand(() => ShowHomeTabItem(HomePageViewType.Info));
        }

        private void ShowHomeTabItem(HomePageViewType viewType)
        {
            TabIndex = (int)MainWindowControlType.Home;
            HomePageViewModel.ShowTabItem(viewType);
            switch (viewType)
            {
                case HomePageViewType.Info:
                    DisabledButton = (int)MainWindowDisabledButton.Home;
                    break;
                case HomePageViewType.Settings:
                    DisabledButton = (int)MainWindowDisabledButton.ManageExtensions;
                    break;
            }
        }

        private void ShowTabItem(MainWindowControlType controlType)
        {
            TabIndex = (int)controlType;
        }

        private SettingsPageViewModel _settingsPageViewModel;
        public SettingsPageViewModel SettingsPageViewModel
        {
            get => _settingsPageViewModel; set
            {
                _settingsPageViewModel = value;
                RaisePropertyChanged("SettingsPageViewModel");
            }
        }

        private void DisplayHomeScreen()
        {
            TabIndex = (int)MainWindowControlType.Home;
            HomePageViewModel.LoadExtensions();
            HomePageViewModel.ShowStartInfo();
        }

        private void Refresh()
        {
            DisplayHomeScreen();
        }
    }
}
