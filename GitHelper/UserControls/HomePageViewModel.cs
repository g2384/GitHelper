using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GitHelper.Extension;
using GitHelper.Extension.Attributes;
using GitHelper.Extension.Helpers;
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
            get => _manageExtensionsPageViewModel;
            set => Set(ref _manageExtensionsPageViewModel, value);
        }

        private int _tabIndex;

        public int TabIndex
        {
            get => _tabIndex;
            set
            {
                if (Set(ref _tabIndex, value))
                {
                    if ((HomePageViewType)_tabIndex == HomePageViewType.Info)
                    {
                        ShowStartInfo();
                    }
                }
            }
        }

        private ObservableCollection<ExtensionInfo> _extensions;

        public ObservableCollection<ExtensionInfo> Extensions
        {
            get => _extensions;
            set => Set(ref _extensions, value);
        }

        private ExtensionInfo _selectedExtension;

        public ExtensionInfo SelectedExtension
        {
            get => _selectedExtension;
            set
            {
                if (Set(ref _selectedExtension, value))
                {
                    ShowExtensionInfo(_selectedExtension);
                }
            }
        }

        private readonly Configuration _configuration;

        private readonly string _currentDirectory = FilePathHelper.GetCurrentDirectory();

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
            var allAssemblies = new List<Assembly>();
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (var dll in Directory.GetFiles(path, "*.dll"))
                allAssemblies.Add(Assembly.LoadFile(dll));
            foreach (var dll in Directory.GetFiles(path, "*.exe"))
            {
                if (!dll.Contains("GitHelper.exe"))
                {
                    allAssemblies.Add(Assembly.LoadFile(dll));
                }
            }

            var types = AssemblyHelper.GetTypesWith<GitHelperActionAttribute>(false).ToList();
            if (types.Any())
            {
                AddToExtensions(extensions, types);
            }
            else
            {
                foreach (var assembly in allAssemblies)
                {
                    var attributeTypes = AssemblyHelper.GetTypesWith<GitHelperActionAttribute>(false, assembly).ToList();
                    AddToExtensions(extensions, attributeTypes);
                }
            }
        }

        private void AddToExtensions(ObservableCollection<ExtensionInfo> extensions, List<Type> types)
        {
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

        private void ShowExtensionOutput(string filePath, string workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (!FilePathHelper.IsFullPath(filePath))
            {
                filePath = FilePathHelper.GetAbsolutePath(filePath, _currentDirectory);
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show(string.Format("Cannot find file \"{0}\"", filePath));
                return;
            }

            if ((HomePageViewType)TabIndex == HomePageViewType.Settings)
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

            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                workingDirectory = _configuration.RepoPath;
            }


            TaskScheduler syncContextScheduler;
            if (SynchronizationContext.Current != null)
            {
                syncContextScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            }
            else
            {
                // If there is no SyncContext for this thread (e.g. we are in a unit test
                // or console scenario instead of running in an app), then just use the
                // default scheduler because there is no UI thread to sync with.
                syncContextScheduler = TaskScheduler.Current;
            }

            var viewModel = new ExtensionOutputViewModel(filePath, workingDirectory, syncContextScheduler);
            var extensionOutput = new ExtensionOutput()
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
            TabIndex = (int)viewType;
        }
    }
}
