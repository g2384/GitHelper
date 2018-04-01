using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitHelper.Attributes;
using GitHelper.Helpers;
using GitHelper.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace GitHelper
{
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

        public ICommand OpenSettingsCommand { get; private set; }

        public ICommand OpenManageExtensionCommand { get; private set; }

        public ICommand RefreshCommand { get; private set; }

        public MainWindowViewModel()
        {
            DisplayHomeScreen();
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenManageExtensionCommand = new RelayCommand(OpenManageExtension);
            RefreshCommand = new RelayCommand(Refresh);
        }

        private void DisplayHomeScreen()
        {
            Config = Configuration.GetConfiguration();
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
                var extensionInfo = new ExtensionInfo(e);
                extensions.Add(extensionInfo);
            }
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

        private void OpenManageExtension()
        {
            WindowService.ShowWindow<ManageExtensions>(new ManageExtensionsViewModel(Config));
        }

        public void OpenSettings()
        {
            WindowService.ShowWindow<Settings>(new SettingsViewModel());
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
