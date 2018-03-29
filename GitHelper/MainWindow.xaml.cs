using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using LibGit2Sharp;
using Newtonsoft.Json;
using NLog;

namespace GitHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string GitBranchvv = "git branch -vv";
        public const string ConfigFile = "config.json";
        public const string CommitOutputFile = "commits.txt";
        public List<BranchInfo> Branches { get; set; }
        public Configuration Config { get; set; }

        private static Logger _log = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            Config = new Configuration();
            if (File.Exists(ConfigFile))
            {
                try
                {
                    Config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigFile));
                }
                catch
                { }
            }

            InitializeComponent();

            RepoTextBox.Text = Config.RepoPath;
            MergedInBranchTextBox.Text = Config.MergedInBranch;
            IgnoredBranchesTextBox.Text = Config.IgnoredBranchesString;
            OnlyDeleteMergedCheckBox.IsChecked = Config.OnlyDeleteMerged;
            LoadBranches();
        }

        private void GetBranchInfoFromGitCommand(List<BranchInfo> branches)
        {
            var output = RunCommand(Config.RepoPath, GitBranchvv);
            var lines = output.Split('\n').ToList();
            var startIndex = lines.FindIndex(l => l.Contains(GitBranchvv));
            if (startIndex >= 0)
            {
                lines.RemoveRange(0, startIndex + 1);
            }
            lines.RemoveRange(lines.Count - 2, 2);
            var hashStartIndex = new int[lines.Count];
            var hashRegex = new Regex(@" [a-zA-Z0-9]+ \[origin/");
            var gitCmdStartLine = -1;
            var gitCmdEndLine = -1;
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var match = hashRegex.Match(line);
                if (match == Match.Empty)
                {
                    if (gitCmdEndLine < 0 && gitCmdStartLine >= 0)
                    {
                        gitCmdEndLine = i - 1;
                    }
                    continue;
                }

                if (gitCmdStartLine < 0)
                {
                    gitCmdStartLine = i;
                }
                hashStartIndex[i] = match.Captures[0].Index + 1;
            }
            if (gitCmdEndLine < 0 && gitCmdStartLine >= 0)
            {
                gitCmdEndLine = lines.Count - 1;
            }
            if (gitCmdStartLine < 0)
            {
                _log.Info($"did not find local branches with command {GitBranchvv}");
                return;
            }

            var hashStartIndexList = hashStartIndex.ToList();
            var removeLineCount = hashStartIndexList.Count - (gitCmdEndLine + 1);
            hashStartIndexList.RemoveRange(gitCmdEndLine + 1, removeLineCount);
            hashStartIndexList.RemoveRange(0, gitCmdStartLine);
            lines.RemoveRange(gitCmdEndLine + 1, removeLineCount);
            lines.RemoveRange(0, gitCmdStartLine);
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

                if (string.IsNullOrWhiteSpace(b.IsLocal))
                {
                    b.IsLocal = BranchInfo.CheckMark;
                    b.MergedInDate = "(unkown)";
                    b.MergedInDateTime = "Could not find the PR in " + Config.MergedInBranch;
                }
            }
        }

        private static string RunCommand(string repoPath, string cmd)
        {
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                }
            };
            p.Start();
            var drive = repoPath.Split(':').First();
            p.StandardInput.WriteLine($"{drive}:");
            p.StandardInput.WriteLine($"cd \"{repoPath}\"");
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.Close();
            var output = p.StandardOutput.ReadToEnd();
            p.Close();
            return output;
        }

        private void GetBranches()
        {
            Branches = new List<BranchInfo>();
            if (!Directory.Exists(Config.RepoPath))
            {
                MessageBox.Show($"Cannot find folder \"{Config.RepoPath}\"", "Error");
            }

            using (var repo = new Repository(Config.RepoPath))
            {
                var developCommits = repo.Branches.FirstOrDefault(b => b.FriendlyName == Config.MergedInBranch)?.Commits;
                if (developCommits == null)
                {
                    MessageBox.Show($"Cannot find branch \"{Config.MergedInBranch}\" in the repo", "Error", MessageBoxButton.OK);
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
                    Branches.Add(new BranchInfo(branch.FriendlyName, branch.IsTracking, isLocal, branch.Commits.First(), mergedInCommit));
                }
            }
            Branches = Branches.OrderBy(b => b.Name).ToList();
        }

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            LoadBranches();
        }

        private void LoadBranches()
        {
            try
            {
                GetSettingsFromUi();
                SaveSettings();
                GetBranches();
                GetBranchInfoFromGitCommand(Branches);
                BranchListView.ItemsSource = Branches;
                DeleteButton.IsEnabled = false;
                ExportCommitMessageButton.IsEnabled = false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK);
                _log.Error(e, "Exception");
            }
        }

        private void GetSettingsFromUi()
        {
            Config.RepoPath = RepoTextBox.Text;
            Config.MergedInBranch = MergedInBranchTextBox.Text;
            Config.IgnoredBranchesString = IgnoredBranchesTextBox.Text;
            Config.OnlyDeleteMerged = OnlyDeleteMergedCheckBox.IsChecked == true;
        }

        private void SaveSettings()
        {
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (BranchListView.SelectedItem is BranchInfo branch)
            {
                var output = RunCommand(Config.RepoPath, "git branch -D " + branch.Name);
                LoadBranches();
            }
        }

        private void BranchListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BranchListView.SelectedItem is BranchInfo branch)
            {
                DeleteButton.IsEnabled = !Config.OnlyDeleteMerged || !string.IsNullOrWhiteSpace(branch.IsLocal);
                ExportCommitMessageButton.IsEnabled = true;
            }
        }

        private void ExportCommitMessageButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (BranchListView.SelectedItem is BranchInfo branch)
            {
                ExportCommitMessages ex = new ExportCommitMessages(branch, Config);
                ex.ShowDialog();
            }
        }

        private void OnlyDeleteMergedCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            Config.OnlyDeleteMerged = OnlyDeleteMergedCheckBox.IsChecked == true;
            if (BranchListView.SelectedItem is BranchInfo branch)
            {
                DeleteButton.IsEnabled = !Config.OnlyDeleteMerged || !string.IsNullOrWhiteSpace(branch.IsLocal);
                ExportCommitMessageButton.IsEnabled = true;
            }

            SaveSettings();
        }
    }
}
