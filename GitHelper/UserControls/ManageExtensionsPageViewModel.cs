using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitHelper.Extension;
using GitHelper.Extension.Helpers;
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
                if (Set(ref _extension, value))
                {
                    ExtensionName = _extension?.Name;
                    ExtensionDescription = _extension?.Description;
                    WorkingDirectory = _extension?.WorkingDirectory;
                    DeleteExtensionCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _selectedPath;
        public string SelectedPath
        {
            get => _selectedPath;
            set
            {
                if (Set(ref _selectedPath, value))
                {
                    RaisePropertyChanged(nameof(CanAdd));
                }
            }
        }

        private string _extensionName;
        public string ExtensionName
        {
            get => _extensionName;
            set => Set(ref _extensionName, value);
        }

        private string _extensionDescription;

        public string ExtensionDescription
        {
            get => _extensionDescription;
            set => Set(ref _extensionDescription, value);
        }

        private string _workingDirectory;

        public string WorkingDirectory
        {
            get => _workingDirectory;
            set => Set(ref _workingDirectory, value);
        }

        private bool _useRelativePath;
        public bool UseRelativePath
        {
            get => _useRelativePath;
            set
            {
                _useRelativePath = value;

                SetSelectedPath();
                RaisePropertyChanged(nameof(UseRelativePath));
            }
        }

        private readonly string _currentDirectory = FilePathHelper.GetCurrentDirectory();

        public bool CanAdd => !string.IsNullOrWhiteSpace(SelectedPath);

        private List<string> _selectedFilePaths;

        public ICommand OpenFileCommand { get; }

        public ICommand AddScriptCommand { get; }

        public RelayCommand DeleteExtensionCommand { get; }

        public ICommand SaveCommand { get; }

        private readonly Configuration _configuration;

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

            _configuration.Extensions.RemoveAll(
                e => e.Name == Extension.Name
                     && e.Description == Extension.Description
                     );
            _configuration.Save();
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
            _configuration.Save();
            _selectedFilePaths = new List<string>();
            SelectedPath = null;
            var extensionArgs = new ManageExtensionEventArgs(extensionFiles);
            OnAddExtension?.Invoke(this, extensionArgs);
        }

        private void OpenFile()
        {
            _selectedFilePaths = new FileDialogService().SelectFilesDialog(out var result, null, "Extensions|*.bat;*.dll;*.exe").ToList();
            if (result != true)
            {
                return;
            }

            SetSelectedPath();
        }

        private void SetSelectedPath()
        {
            _selectedFilePaths = _useRelativePath
                            ? FilePathHelper.GetRelativePaths(_selectedFilePaths, _currentDirectory)
                            : FilePathHelper.GetAbsolutePaths(_selectedFilePaths, _currentDirectory);
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
