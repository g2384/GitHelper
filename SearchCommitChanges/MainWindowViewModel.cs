using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LibGit2Sharp;

namespace DeepBlameLine
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _result;

        public string Result
        {
            get => _result;
            set => Set(ref _result, value);
        }

        private string _queryText;

        public string QueryText
        {
            get => _queryText;
            set => Set(ref _queryText, value);
        }

        private string _repoPath;

        public string RepoPath
        {
            get => _repoPath;
            set => Set(ref _repoPath, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        private double _progress;

        public double Progress
        {
            get => _progress;
            set => Set(ref _progress, value);
        }

        public RelayCommand SearchCommand { get; }

        public MainWindowViewModel()
        {
            SearchCommand = new RelayCommand(Search);
        }

        public async void Search()
        {
            await Task.Run(() =>
            {
                try

                {
                    Search2();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
        }

        public void Search2()
        {
            var inputQuery = _queryText;
            using (var repo = new Repository(RepoPath))
            {
                repo.ArgumentNullCheck();
                var currentBranch = repo.Branches.FirstOrDefault(b => b.IsCurrentRepositoryHead);
                if (currentBranch == null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(Result))
                {
                    Result = string.Empty;
                }

                var totalCommits = currentBranch.Commits.Count();
                Status = totalCommits.ToString();
                var processedCommits = 0;
                Parallel.ForEach(currentBranch.Commits, commit =>
                {
                    if (commit?.Parents?.Any() != true) return;
                    // ReSharper disable once AccessToDisposedClosure
                    var patch = repo.Diff.Compare<Patch>(commit.Parents.First().Tree, commit.Tree);
                    Parallel.ForEach(patch, t =>
                    {
                        var a = t.Patch;
                        if (a.Contains(inputQuery))
                        {
                            Result += a;
                        }
                    });

                    processedCommits++;
                    Status = processedCommits + "/" + totalCommits;
                    Progress = (double)processedCommits * 100 / totalCommits;
                });

                Status = "Finished";
            }
        }
    }
}
