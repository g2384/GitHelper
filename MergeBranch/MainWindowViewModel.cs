using GitHelper.Common;
using Prism.Commands;
using Prism.Mvvm;
using System.Linq;
using System.Windows.Input;

namespace MergeBranch
{
    public sealed class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel(GitHelper.Extension.Configuration config)
        {
            Config = config;
            Repo = Config.RepoPath;
        }

        private readonly GitHelper.Extension.Configuration Config;

        private string _currentBranch;

        public string CurrentBranch
        {
            get => _currentBranch;
            set => SetProperty(ref _currentBranch, value);
        }

        private string _targetBranch;

        public string TargetBranch
        {
            get => _targetBranch;
            set => SetProperty(ref _targetBranch, value);
        }

        private string _repo;
        public string Repo
        {
            get => _repo;
            set => SetProperty(ref _repo, value);
        }

        private ICommand _mergeCommand;

        public ICommand MergeCommand => _mergeCommand ?? (_mergeCommand = new DelegateCommand(Merge, CanMerge));

        private static void Merge()
        {
            var RepoPath = @"C:\Users\guang113\repos\IFRS";
            var targetBranchName = "develop";
            var currentBranchName = CommandLineRunner.Run(RepoPath, "git rev-parse --abbrev-ref HEAD")
                .Split("\r\n")
                .Skip(3)
                .First(e => !e.Contains(":") && !string.IsNullOrWhiteSpace(e))
                .Trim();
            if (currentBranchName != targetBranchName)
            {
                CommandLineRunner.Run(RepoPath, $"git -c diff.mnemonicprefix=false -c core.quotepath=false --no-optional-locks checkout {targetBranchName} --progress");

                CommandLineRunner.Run(RepoPath, "git -c diff.mnemonicprefix=false -c core.quotepath=false fetch --prune origin");
                CommandLineRunner.Run(RepoPath, "git -c diff.mnemonicprefix=false -c core.quotepath=false pull --no-commit --rebase origin");

                CommandLineRunner.Run(RepoPath, $"git -c diff.mnemonicprefix=false -c core.quotepath=false --no-optional-locks checkout {currentBranchName} --progress");
                CommandLineRunner.Run(RepoPath, $"git -c diff.mnemonicprefix=false -c core.quotepath=false --no-optional-locks merge {targetBranchName}");
            }
        }

        private bool CanMerge()
        {
            return !string.IsNullOrEmpty(TargetBranch);
        }
    }
}
