using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GitHelper.Extension;

namespace GitHelper.UserControls
{
    public class SettingsPageViewModel : ViewModelBase
    {
        private readonly Configuration _configuration;

        private string _repoPath;

        public string RepoPath
        {
            get => _repoPath;
            set => Set(ref _repoPath, value);
        }

        private bool _showWarningBeforeExecutingScript;

        public bool ShowWarningBeforeExecutingScript
        {
            get => _showWarningBeforeExecutingScript;
            set => Set(ref _showWarningBeforeExecutingScript, value);
        }

        public ICommand SaveCommand { get; }

        public SettingsPageViewModel(Configuration configuration)
        {
            _configuration = configuration;
            RepoPath = configuration.RepoPath;
            ShowWarningBeforeExecutingScript = configuration.ShowWarningBeforeExecutingScript;
            SaveCommand = new RelayCommand(Save);
        }

        private void Save()
        {
            _configuration.RepoPath = RepoPath;
            _configuration.ShowWarningBeforeExecutingScript = ShowWarningBeforeExecutingScript;
            _configuration.Save();
        }
    }
}
