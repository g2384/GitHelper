using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GitHelper.Extension;
using GitHelper.Extension.Attributes;
using GitHelper.Extension.Interfaces;

namespace GitHelper.UserControls
{
    public enum HomePageViewType
    {
        Info,
        Settings
    }

    public class HomePageViewModel : ViewModelBase
    {
        public ExtensionInfoViewModel ExtensionInfoViewModel { get; }
        
        private ManageExtensionsPageViewModel _manageExtensionsPageViewModel;
        public ManageExtensionsPageViewModel ManageExtensionsPageViewModel
        {
            get => _manageExtensionsPageViewModel; set
            {
                _manageExtensionsPageViewModel = value;
                RaisePropertyChanged(nameof(ManageExtensionsPageViewModel));
            }
        }

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

        private ExtensionInfo _selectedExtension;
        public ExtensionInfo SelectedExtension
        {
            get => _selectedExtension;
            set
            {
                _selectedExtension = value;
                ShowExtensionInfo(_selectedExtension);
                RaisePropertyChanged(nameof(SelectedExtension));
            }
        }

        private Configuration _configuration;

        public HomePageViewModel(Configuration configuration)
        {
            _configuration = configuration;
            ManageExtensionsPageViewModel = new ManageExtensionsPageViewModel(configuration);
            ManageExtensionsPageViewModel.OnAddExtension += ManageExtensionsPageViewModel_OnAddExtension;
            ManageExtensionsPageViewModel.OnDeleteExtension += ManageExtensionsPageViewModel_OnDeleteExtension;
            ExtensionInfoViewModel = new ExtensionInfoViewModel();
        }

        private void ManageExtensionsPageViewModel_OnDeleteExtension(object sender, ManageExtensionEventArgs e)
        {
            var extensionFile = e?.ExtensionFiles?.FirstOrDefault();
            if (extensionFile == null)
            {
                return;
            }

            var extension = Extensions.FirstOrDefault(i => i.Name == extensionFile.Name);
            Extensions.Remove(extension);
            RaisePropertyChanged(nameof(Extensions));
        }

        private void ManageExtensionsPageViewModel_OnAddExtension(object sender, ManageExtensionEventArgs e)
        {
            foreach (var extensionFile in e.ExtensionFiles)
            {
                Extensions.Add(new ExtensionInfo(extensionFile, ShowExtensionOutput));
            }
            RaisePropertyChanged(nameof(Extensions));
        }

        public void LoadExtensions()
        {
            var extensions = new ObservableCollection<ExtensionInfo>();
            AddPlugins(extensions);
            LoadExtensions(extensions);
            Extensions = new ObservableCollection<ExtensionInfo>(extensions.OrderBy(a => a.Name));
        }

        private void AddPlugins(ObservableCollection<ExtensionInfo> extensions)
        {
            var types = GetTypesWith<GitHelperActionAttribute>(false).ToList();
            foreach (var type in types)
            {
                var instance = (IGitHelperActionMeta)Activator.CreateInstance(type);
                var extensionInfo = new ExtensionInfo(instance, _configuration);
                extensions.Add(extensionInfo);
            }
        }

        private void LoadExtensions(ObservableCollection<ExtensionInfo> extensions)
        {
            foreach (var e in _configuration.Extensions)
            {
                var extensionInfo = new ExtensionInfo(e, ShowExtensionOutput);
                extensions.Add(extensionInfo);
            }            
        }

        private void ShowExtensionOutput(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if((HomePageViewType)TabIndex == HomePageViewType.Settings)
            {
                ManageExtensionsPageViewModel.Extension = new GitHelperScriptFile(filePath);
                return;
            }

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
