using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitHelper.Extension;
using GitHelper.Extension.Attributes;
using GitHelper.Extension.Interfaces;
using GitHelper.UserControls;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        private ObservableCollection<ExtensionInfo> _extensions;
        public ObservableCollection<ExtensionInfo> Extensions
        {
            get => _extensions;
            set
            {
                _extensions = value;
                RaisePropertyChanged(nameof(Extensions));
            }
        }

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

        private ExtensionInfo _selectedAction;
        public ExtensionInfo SelectedAction
        {
            get => _selectedAction;
            set
            {
                _selectedAction = value;
                HomePageViewModel.ShowExtensionInfo(_selectedAction);
                RaisePropertyChanged("SelectedAction");
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
            ManageExtensionsPageViewModel = new ManageExtensionsPageViewModel(Config);
            SettingsPageViewModel = new SettingsPageViewModel();
            HomePageViewModel = new HomePageViewModel(Config);
            DisplayHomeScreen();
            OpenSettingsCommand = new RelayCommand(() => ShowTabItem(MainWindowControlType.Settings));
            OpenManageExtensionCommand = new RelayCommand(() => ShowManageExtension(HomePageViewType.Settings));
            RefreshCommand = new RelayCommand(Refresh);
            OpenHomeCommand = new RelayCommand(() => ShowManageExtension(HomePageViewType.Info));
        }

        private void ShowManageExtension(HomePageViewType viewType)
        {
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

        private ManageExtensionsPageViewModel _manageExtensionsPageViewModel;
        public ManageExtensionsPageViewModel ManageExtensionsPageViewModel
        {
            get => _manageExtensionsPageViewModel; set
            {
                _manageExtensionsPageViewModel = value;
                RaisePropertyChanged("ManageExtensionsPageViewModel");
            }
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
            var extensions = new ObservableCollection<ExtensionInfo>();
            AddPlugins(extensions);
            AddScripts(extensions);
            HomePageViewModel.ShowStartInfo();
            Extensions = new ObservableCollection<ExtensionInfo>(extensions.OrderBy(a => a.Name));
        }

        private void Refresh()
        {
            DisplayHomeScreen();
        }

        private void AddScripts(ObservableCollection<ExtensionInfo> extensions)
        {
            var newExtensions = ExtensionViewModelHelper.GetExtensionFiles(Config.ExtensionPaths);
            foreach (var e in newExtensions)
            {
                var extensionInfo = new ExtensionInfo(e, ShowExtensionOutput);
                extensions.Add(extensionInfo);
            }
        }

        private void ShowExtensionOutput(string filePath)
        {
            var isOK = MessageBox.Show("Do you want to run this script" +
                Environment.NewLine + "---------" + Environment.NewLine + File.ReadAllText(filePath),
                "Run Script", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (isOK != MessageBoxResult.Yes)
            {
                return;
            }
            var viewModel = new ExtensionOutputViewModel(filePath);
            ExtensionOutput extensionOutput = new ExtensionOutput()
            {
                DataContext = viewModel
            };
            Utility.ShowDialog(extensionOutput);
        }


        private void AddPlugins(ObservableCollection<ExtensionInfo> extensions)
        {
            var types = GetTypesWith<GitHelperActionAttribute>(false).ToList();
            foreach (var type in types)
            {
                var instance = (IGitHelperActionMeta)Activator.CreateInstance(type);
                var extensionInfo = new ExtensionInfo(instance, Config);
                extensions.Add(extensionInfo);
            }
        }



        IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
                              where TAttribute : Attribute
        {
            return from a in AppDomain.CurrentDomain.GetAssemblies()
                   from t in a.GetTypes()
                   where t.IsDefined(typeof(TAttribute), inherit)
                   select t;
        }


    }
}
