using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitHelper.Helpers;
using GitHelper.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace GitHelper
{
    public class ManageExtensionsPageViewModel : ViewModelBase
    {
        private ObservableCollection<IGitHelperExtensionFile> _extensions;
        public ObservableCollection<IGitHelperExtensionFile> Extensions
        {
            get => _extensions;
            set
            {
                _extensions = value;
                RaisePropertyChanged("Extensions");
            }
        }

        private IGitHelperExtensionFile _selectedExtensions;
        public IGitHelperExtensionFile SelectedExtension
        {
            get => _selectedExtensions;
            set
            {
                _selectedExtensions = value;
                RaisePropertyChanged("SelectedExtension");
                RaisePropertyChanged("CanDelete");
            }
        }

        private string _selectedPath;
        public string SelectedPath
        {
            get { return _selectedPath; }
            set
            {
                _selectedPath = value;
                RaisePropertyChanged("SelectedPath");
                RaisePropertyChanged("CanAdd");
            }
        }

        private bool _useRelativePath;
        public bool UseRelativePath
        {
            get => _useRelativePath;
            set
            {
                _useRelativePath = value;
                var newPaths = new List<string>();
                var currentPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                if (_useRelativePath)
                {
                    newPaths = Utility.GetRelativePaths(_selectedFilePaths, currentPath);
                }
                else
                {
                    newPaths = Utility.GetAbsolutePaths(_selectedFilePaths, currentPath);
                }
                SetSelectedPath(newPaths);
                RaisePropertyChanged("SelectedPath");
                RaisePropertyChanged("UseRelativePath");
            }
        }

        public bool CanDelete => SelectedExtension != null;

        public bool CanAdd => !string.IsNullOrWhiteSpace(SelectedPath);

        private List<string> _selectedFilePaths;

        public ICommand OpenFileCommand { get; private set; }

        public ICommand AddScriptCommand { get; private set; }

        public ICommand DeleteExtensionCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }

        private Configuration _configuration;

        public ManageExtensionsPageViewModel(Configuration configuration)
        {
            _configuration = configuration;
            Extensions = new ObservableCollection<IGitHelperExtensionFile>(ExtensionViewModelHelper.GetExtensionFiles(configuration.ExtensionPaths));
            OpenFileCommand = new RelayCommand(OpenFile);
            AddScriptCommand = new RelayCommand(AddScript);
            DeleteExtensionCommand = new RelayCommand(DeleteExtension);
            SaveCommand = new RelayCommand(Save);
        }

        private void Save()
        {
            var paths = Extensions.Select(e => e.FilePath).ToList();
            _configuration.ExtensionPaths = paths;
            _configuration.Save();
        }

        private void DeleteExtension()
        {
            if (SelectedExtension == null)
            {
                return;
            }

            var index = Extensions.IndexOf(SelectedExtension);
            Extensions.Remove(SelectedExtension);

            if (index < 0)
            {
                index = 0;
            }

            if (index >= Extensions.Count)
            {
                index = Extensions.Count - 1;
            }

            if (index >= 0)
            {
                SelectedExtension = Extensions[index];
            }
        }

        private void AddScript()
        {
            var extensions = ExtensionViewModelHelper.GetExtensionFiles(_selectedFilePaths, true);
            foreach (var e in extensions)
            {
                Extensions.Add(e);
            }

            _selectedFilePaths = new List<string>();
            SelectedPath = null;
        }

        private void OpenFile()
        {
            _selectedFilePaths = new FileDialogService().SelectFilesDialog(out bool? result, null, "Extensions|*.bat;*.dll;*.exe").ToList();
            if (result != true)
            {
                return;
            }

            SetSelectedPath(_selectedFilePaths);
        }

        private void SetSelectedPath(List<string> paths)
        {
            if (Utility.IsNullOrEmpty(paths))
            {
                SelectedPath = null;
            }

            if (paths.Count == 1)
            {
                SelectedPath = paths[0];
            }
            else
            {
                SelectedPath = "\"" + string.Join("\", \"", paths) + "\"";
            }
        }
    }
}