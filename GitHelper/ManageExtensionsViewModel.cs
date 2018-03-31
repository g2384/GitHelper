using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitHelper.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GitHelper
{
    public class ManageExtensionsViewModel : ViewModelBase
    {
        private ObservableCollection<GitHelperExtension> _extensions;
        public ObservableCollection<GitHelperExtension> Extensions
        {
            get => _extensions;
            set
            {
                _extensions = value;
                RaisePropertyChanged("Extensions");
            }
        }

        private GitHelperExtension _selectedExtensions;
        public GitHelperExtension SelectedExtension
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

        public bool CanDelete => SelectedExtension != null;

        public bool CanAdd => !string.IsNullOrWhiteSpace(SelectedPath);

        private List<string> _selectedFilePaths;

        public ICommand OpenFileCommand { get; private set; }

        public ICommand AddScriptCommand { get; private set; }

        public ICommand DeleteExtensionCommand { get; private set; }

        public ManageExtensionsViewModel(Configuration configuration)
        {
            Extensions = GetExtensions(configuration.ExtensionPaths);
            OpenFileCommand = new RelayCommand(OpenFile);
            AddScriptCommand = new RelayCommand(AddScript);
            DeleteExtensionCommand = new RelayCommand(DeleteExtension);
        }

        private ObservableCollection<GitHelperExtension> GetExtensions(List<string> extensionPaths)
        {
            var extensions = new ObservableCollection<GitHelperExtension>();
            if (Utility.IsNullOrEmpty(extensionPaths))
            {
                return extensions;
            }

            foreach(var path in extensionPaths)
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    var lines = File.ReadAllLines(path);
                    var extension = new GitHelperExtension(lines);
                    extensions.Add(extension);
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            return extensions;
        }

        private void DeleteExtension()
        {
            if (SelectedPath == null)
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
            if (Utility.IsNullOrEmpty(_selectedFilePaths))
            {
                return;
            }

            foreach (var filePath in _selectedFilePaths)
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"Cannot find file \"{filePath}\"");
                    continue;
                }

                try
                {
                    var lines = File.ReadAllLines(filePath);
                    var extension = new GitHelperExtension(lines);
                    Extensions.Add(extension);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }

            _selectedFilePaths = new List<string>();
            SelectedPath = string.Empty;
        }

        private void OpenFile()
        {
            _selectedFilePaths = new FileDialogService().OpenFilesDialog(null).ToList();
            if (Utility.IsNullOrEmpty(_selectedFilePaths))
            {
                SelectedPath = string.Empty;
            }

            if (_selectedFilePaths.Count == 1)
            {
                SelectedPath = _selectedFilePaths[0];
            }
            else
            {
                SelectedPath = "\"" + string.Join("\", \"", _selectedFilePaths) + "\"";
            }
        }
    }
}