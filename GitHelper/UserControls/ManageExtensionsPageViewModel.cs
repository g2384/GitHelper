using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitHelper.Extension;
using GitHelper.Extension.Interfaces;
using GitHelper.Helpers;

namespace GitHelper.UserControls
{
    public class ManageExtensionEventArgs : EventArgs
    {
        public List<IGitHelperExtensionFile> ExtensionFiles { get; }
        public ManageExtensionEventArgs(List<IGitHelperExtensionFile> extensionFiles)
        {
            ExtensionFiles = extensionFiles;
        }
    }

    public class ManageExtensionsPageViewModel : ViewModelBase
    {
        private IGitHelperExtensionFile _extension;
        public IGitHelperExtensionFile Extension
        {
            get => _extension;
            set
            {
                _extension = value;
                ExtensionName = Extension.Name;
                RaisePropertyChanged(nameof(Extension));
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

        private string _extensionName;
        public string ExtensionName
        {
            get => _extensionName;
            set
            {
                _extensionName = value;
                RaisePropertyChanged(nameof(ExtensionName));
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

        public bool CanDelete => Extension != null;

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
            OpenFileCommand = new RelayCommand(OpenFile);
            AddScriptCommand = new RelayCommand(AddScript);
            DeleteExtensionCommand = new RelayCommand(DeleteExtension);
            SaveCommand = new RelayCommand(Save);
        }

        private void Save()
        {
            _configuration.Save();
        }

        private void DeleteExtension()
        {
            if (Extension == null)
            {
                return;
            }

            _configuration.Extensions.Remove(Extension);
            var extensionArgs = new ManageExtensionEventArgs(new List<IGitHelperExtensionFile>() { Extension });
            OnDeleteExtension?.Invoke(this, extensionArgs);
            Extension = null;
        }

        public event EventHandler<ManageExtensionEventArgs> OnDeleteExtension;
        public event EventHandler<ManageExtensionEventArgs> OnAddExtension;

        private void AddScript()
        {
            var extensionFiles = ExtensionViewModelHelper.GetExtensionFiles(_selectedFilePaths, true);
            _configuration.Extensions.AddRange(extensionFiles);
            _selectedFilePaths = new List<string>();
            SelectedPath = null;
            var extensionArgs = new ManageExtensionEventArgs(extensionFiles);
            OnAddExtension?.Invoke(this, extensionArgs);
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
