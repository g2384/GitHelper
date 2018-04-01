using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitHelper.Attributes;
using GitHelper.Interfaces;
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
        ManageExtensions = 1,
        Settings = 2,
        Help = 3,
    }

    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<ExtensionInfo> _actions;
        public ObservableCollection<ExtensionInfo> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                RaisePropertyChanged("Actions");
            }
        }

        private ExtensionInfo _selectedAction;
        public ExtensionInfo SelectedAction
        {
            get => _selectedAction;
            set
            {
                _selectedAction = value;
                DisplayFullInfo();
                RaisePropertyChanged("SelectedAction");
            }
        }

        public Configuration Config { get; set; }

        private static Logger _log = LogManager.GetCurrentClassLogger();

        private string _fullInfo;
        public string FullInfo
        {
            get => _fullInfo;
            set
            {
                _fullInfo = value;
                RaisePropertyChanged("FullInfo");
            }
        }

        private int _tabIndex;
        public int TabIndex
        {
            get => _tabIndex;
            set
            {
                _tabIndex = value;
                if ((MainWindowControlType)_tabIndex == MainWindowControlType.Home)
                {
                    FullInfo = StartInfo();
                }

                RaisePropertyChanged("TabIndex");
            }
        }

        public ICommand OpenSettingsCommand { get; private set; }

        public ICommand OpenManageExtensionCommand { get; private set; }

        public ICommand RefreshCommand { get; private set; }

        public ICommand OpenHomeCommand { get; private set; }

        public MainWindowViewModel()
        {
            Config = Configuration.GetConfiguration();
            ManageExtensionsPageViewModel = new ManageExtensionsPageViewModel(Config);
            SettingsPageViewModel = new SettingsPageViewModel();
            DisplayHomeScreen();
            OpenSettingsCommand = new RelayCommand(() => ShowTabItem(MainWindowControlType.Settings));
            OpenManageExtensionCommand = new RelayCommand(() => ShowTabItem(MainWindowControlType.ManageExtensions));
            RefreshCommand = new RelayCommand(Refresh);
            OpenHomeCommand = new RelayCommand(() => ShowTabItem(MainWindowControlType.Home));
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

        public SettingsPageViewModel _settingsPageViewModel { get; private set; }
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
            FullInfo = StartInfo();
            Actions = new ObservableCollection<ExtensionInfo>(extensions.OrderBy(a => a.Name));
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

        private void DisplayFullInfo()
        {
            if (SelectedAction == null)
            {
                return;
            }

            FullInfo = SelectedAction.GetFullInfo();
        }

        IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
                              where TAttribute : Attribute
        {
            return from a in AppDomain.CurrentDomain.GetAssemblies()
                   from t in a.GetTypes()
                   where t.IsDefined(typeof(TAttribute), inherit)
                   select t;
        }

        private string StartInfo()
        {
            return Utility.GetResourceText("pack://application:,,,/HtmlPages/StartPage.html");
        }
    }
}
