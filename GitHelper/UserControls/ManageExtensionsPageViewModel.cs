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
                ExtensionName = _extension.Name;
                ExtensionDescription = _extension.Description;
                WorkingDirectory = _extension.WorkingDirectory;
                RaisePropertyChanged(nameof(Extension));
                DeleteExtensionCommand.RaiseCanExecuteChanged();
            }
        }

        private string _selectedPath;
        public string SelectedPath
        {
            get => _selectedPath;
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

        private string _extensionDescription;

        public string ExtensionDescription
        {
            get => _extensionDescription;
            set
            {
                _extensionDescription = value;
                RaisePropertyChanged(nameof(ExtensionDescription));
            }
        }

        private string _workingDirectory;

        public string WorkingDirectory
        {
            get => _workingDirectory;
            set
            {
                _workingDirectory = value;
                RaisePropertyChanged(nameof(WorkingDirectory));
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

        public bool CanAdd => !string.IsNullOrWhiteSpace(SelectedPath);

        private List<string> _selectedFilePaths;

        public ICommand OpenFileCommand { get; }

        public ICommand AddScriptCommand { get; }

        public RelayCommand DeleteExtensionCommand { get; }

        public ICommand SaveCommand { get; }

        private Configuration _configuration;

        public ManageExtensionsPageViewModel(Configuration configuration)
        {
            _configuration = configuration;
            OpenFileCommand = new RelayCommand(OpenFile);
            AddScriptCommand = new RelayCommand(AddScript);
            DeleteExtensionCommand = new RelayCommand(DeleteExtension, () => Extension != null);
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
