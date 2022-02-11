using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using GitHelper.Common;
using GitHelper.Extension;
using GitHelper.UIExtension;
using LibGit2Sharp;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using LogManager = NLog.LogManager;

namespace ManageBranches
{
    public class BranchInfoViewModel : ActionViewModelBase
    {
        private BranchInfo _branchInfo;
        public BranchInfo BranchInfo { get => _branchInfo; set => Set(ref _branchInfo, value); }

        private bool isSelected;

        public bool IsSelected
        {
            get => isSelected;
            set => Set(ref isSelected, value);
        }
    }

    public class ManageBranchesViewModel : ActionViewModelBase
    {
        private const string GitBranchvv = "git branch -vv";

        public List<BranchInfo> Branches { get; set; }

        public List<BranchInfoViewModel> BranchViewModels { get; set; }

        public GitHelper.Extension.Configuration Config { get; set; }

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public ICommand DeleteBranchCommand { get; }
        public ICommand LoadBranchCommand { get; }

        private string _repo;
        public string Repo
        {
            get => _repo;
            set => Set(ref _repo, value);
        }

        private string _mergedInBranch;
        public string MergedInBranch
        {
            get => _mergedInBranch;
            set => Set(ref _mergedInBranch, value);
        }

        private string _ignoredBranches;
        public string IgnoredBranches
        {
            get => _ignoredBranches;
            set => Set(ref _ignoredBranches, value);
        }

        private bool _onlyDeleteMerged;
        public bool OnlyDeleteMerged
        {
            get => _onlyDeleteMerged;
            set
            {
                if (Set(ref _onlyDeleteMerged, value))
                {
                    Config.OnlyDeleteMerged = _onlyDeleteMerged;
                    IsDeleteEnabled = GetIsDeleteEnabled();
                    Config.Save();
                }
            }
        }

        private bool GetIsDeleteEnabled()
        {
            return !Config.OnlyDeleteMerged || SelectedBranch?.All(e => !string.IsNullOrWhiteSpace(e.BranchInfo.IsLocal)) == true;
        }

        private bool _isDeleteEnabled;
        public bool IsDeleteEnabled
        {
            get => _isDeleteEnabled;
            set => Set(ref _isDeleteEnabled, value);
        }

        private ObservableCollection<BranchInfoViewModel> _selectedBranch;
        public ObservableCollection<BranchInfoViewModel> SelectedBranch
        {
            get => _selectedBranch;
            set
            {
                if (Set(ref _selectedBranch, value))
                {
                    IsDeleteEnabled = GetIsDeleteEnabled();
                }
            }
        }

        public ManageBranchesViewModel(GitHelper.Extension.Configuration config)
        {
            Config = config;
            Repo = Config.RepoPath;
            MergedInBranch = Config.MergedInBranch;
            IgnoredBranches = Config.IgnoredBranchesString;
            OnlyDeleteMerged = Config.OnlyDeleteMerged;
            DeleteBranchCommand = new RelayCommand(DeleteBranchMethod);
            LoadBranchCommand = new RelayCommand(LoadBranchMethod);
            LoadBranches();
        }

        private void LoadBranchMethod()
        {
            LoadBranches();
        }

        private void DeleteBranchMethod()
        {
            if (SelectedBranch == null)
            {
                return;
            }

            foreach (var branch in SelectedBranch)
            {
                CommandLineRunner.Run(Config.RepoPath, "git branch -D " + branch.BranchInfo.Name);
            }

            LoadBranches();
        }

        private void GetBranchInfoFromGitCommand(List<BranchInfo> branches)
        {
            var output = CommandLineRunner.Run(Config.RepoPath, GitBranchvv);
            var lines = output.Split('\n').ToList();
            var startIndex = lines.FindIndex(l => l.Contains(GitBranchvv));
            if (startIndex >= 0)
            {
                lines.RemoveRange(0, startIndex + 1);
            }
            lines.RemoveRange(lines.Count - 2, 2);
            var hashStartIndex = new int[lines.Count];
            var hashRegex = new Regex(@" [a-zA-Z0-9]+ \[origin/");
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var match = hashRegex.Match(line);
                if (match.Success)
                {
                    hashStartIndex[i] = match.Captures[0].Index + 1;
                }
            }

            var goneBranches = lines.Where(l => l.IndexOf(": gone] ", StringComparison.Ordinal) >= 0).ToList();
            if (goneBranches.Count == 0)
            {
                return;
            }
            foreach (var b in branches)
            {
                if (goneBranches.All(e => e.IndexOf(b.Name + " ", StringComparison.Ordinal) < 0))
                {
                    continue;
                }

                b.IsGone = BranchInfo.CheckMark;
                if (string.IsNullOrWhiteSpace(b.IsLocal))
                {
                    b.IsLocal = BranchInfo.CheckMark;
                    b.MergedInDate = "(unkown)";
                    b.MergedInDateTime = "Could not find the PR in " + Config.MergedInBranch;
                }
            }
        }

        private void GetBranches()
        {
            Branches = new List<BranchInfo>();
            if (!Directory.Exists(Config.RepoPath))
            {
                MessageBox.ShowError($"Cannot find folder \"{Config.RepoPath}\"");
                return;
            }

            var cmd = "git config user.name";
            var userInfo = CommandLineRunner.Run(Config.RepoPath, cmd).Split('\n');
            var userName = "";
            for (var i = 0; i < userInfo.Length; i++)
            {
                if (userInfo[i].Contains(cmd))
                {
                    userName = userInfo[i + 1];
                    break;
                }
            }

            using (var repo = new Repository(Config.RepoPath))
            {
                var developCommits = repo.Branches.FirstOrDefault(b => b.FriendlyName == Config.MergedInBranch)?.Commits;
                if (developCommits == null)
                {
                    MessageBox.ShowError($"Cannot find branch \"{Config.MergedInBranch}\" in the repo");
                }

                var remoteBranches = repo.Branches.Where(b => b.IsRemote).ToList();
                foreach (var branch in repo.Branches.Where(branch => !branch.IsRemote))
                {
                    if (Config.IgnoredBranches.Contains(branch.FriendlyName))
                    {
                        continue;
                    }

                    var remoteName = "origin/" + branch.FriendlyName;
                    var isLocal = remoteBranches.All(b => b.FriendlyName != remoteName);
                    Commit mergedInCommit = null;
                    if (isLocal && developCommits != null)
                    {
                        var branchName = branch.FriendlyName.Split('/').Last();
                        mergedInCommit = developCommits.FirstOrDefault(c => c.Message.Contains(branchName));
                        if (mergedInCommit == null)
                        {
                            isLocal = false;
                        }
                    }
                    var branchInfo = new BranchInfo(branch.FriendlyName, branch.IsTracking, isLocal, branch.Commits.First(), mergedInCommit);
                    branchInfo.LastCommitBy = branchInfo.LastCommitBy == userName ? "(You)" : branchInfo.LastCommitBy;
                    Branches.Add(branchInfo);
                }
            }
            Branches = Branches.OrderBy(b => b.Name).ToList();
            BranchViewModels = Branches.Select(e => new BranchInfoViewModel() {BranchInfo = e}).ToList();
        }

        private void LoadBranches()
        {
            try
            {
                GetSettingsFromUi();
                Config.Save();
                GetBranches();
                GetBranchInfoFromGitCommand(Branches);
                RaisePropertyChanged(nameof(Branches));
                IsDeleteEnabled = false;
            }
            catch (Exception e)
            {
                MessageBox.ShowError(e.Message);
                _log.Error(e, "Exception");
            }
        }

        private void GetSettingsFromUi()
        {
            Config.RepoPath = Repo;
            Config.MergedInBranch = MergedInBranch;
            Config.IgnoredBranchesString = IgnoredBranches;
            Config.OnlyDeleteMerged = OnlyDeleteMerged;
        }
    }
}
