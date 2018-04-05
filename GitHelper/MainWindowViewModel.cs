using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitHelper.Extension;
using GitHelper.UserControls;
using NLog;
using System;
using System.Windows;
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
            set => Set(ref _disabledButton, value);
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

        public ICommand OpenSettingsCommand { get; }

        public ICommand OpenManageExtensionCommand { get; }

        public ICommand RefreshCommand { get; }

        public ICommand OpenHomeCommand { get; }

        public HomePageViewModel HomePageViewModel { get; }

        public MainWindowViewModel()
        {
            Config = Configuration.GetConfiguration();
            SettingsPageViewModel = new SettingsPageViewModel(Config);
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
            get => _settingsPageViewModel;
            set => Set(ref _settingsPageViewModel, value);
        }

        private void DisplayHomeScreen()
        {
            try
            {
                TabIndex = (int) MainWindowControlType.Home;
                HomePageViewModel.LoadExtensions();
                HomePageViewModel.ShowStartInfo();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
                _log.Error(e, "Exception");
            }
        }

        private void Refresh()
        {
            DisplayHomeScreen();
        }
    }
}
