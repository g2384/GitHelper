using GalaSoft.MvvmLight.Command;
using GitHelper.Extension;
using GitHelper.UIExtension;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using GitHelper.Common.Extensions;

namespace ExportCommitMessages
{
    public class ExportCommitMessageViewModel : ActionViewModelBase
    {
        public const string CommitOutputFile = "commits.txt";
        public GitHelper.Extension.Configuration Configuration { get; set; }
        public List<CommitExportFormat> Formats { get; set; }
        public ICommand ExportCommand { get; }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set => Set(ref _startDate, value);
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set => Set(ref _endDate, value);
        }

        private string _keywords;
        public string Keywords
        {
            get => _keywords;
            set => Set(ref _keywords, value);
        }

        private string _example;
        public string Example
        {
            get => _example;
            set => Set(ref _example, value);
        }


        private string _format;
        public string Format
        {
            get => _format;
            set
            {
                if (Set(ref _format, value))
                {
                    Formats = Parse(_format);
                    var example = GetExample(Formats);
                    Example = example;
                }
            }
        }

        private List<string> _branchNames;
        public List<string> BranchNames
        {
            get => _branchNames;
            set => Set(ref _branchNames, value);
        }

        private string _repoPath;
        public string RepoPath
        {
            get => _repoPath;
            set => Set(ref _repoPath, value);
        }

        private string _selectedBranchname;
        public string SelectedBranchName
        {
            get => _selectedBranchname;
            set => Set(ref _selectedBranchname, value);
        }

        public ICommand LoadBranchCommand { get; }

        public ExportCommitMessageViewModel()
        {
            var explainations = new List<string>();
            foreach (var keyValuePair in CommitExportFormat.KeywordsExample)
            {
                explainations.Add(keyValuePair.Key + ": " + keyValuePair.Value);
            }
            Keywords = string.Join(Environment.NewLine, explainations);
            RepoPath = Configuration?.RepoPath;
            LoadBranches();
            Format = "[author-name]\\t[date-g]\\t[message]";
            ExportCommand = new RelayCommand(ExportMethod);
            LoadBranchCommand = new RelayCommand(LoadBranches);
            StartDate = new DateTime(2018, 1, 1);
            EndDate = DateTime.Now;
        }

        public ExportCommitMessageViewModel(GitHelper.Extension.Configuration configuration) : this()
        {
            Configuration = configuration;
        }

        private void LoadBranches()
        {
            var selectedIndex = 0;
            if (!BranchNames.IsNullOrEmpty())
            {
                selectedIndex = BranchNames.IndexOf(SelectedBranchName);
            }

            BranchNames = GetBranches(RepoPath)?.Select(e => e.Name).ToList();
            if (BranchNames.IsNullOrEmpty())
            {
                return;
            }

            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            SelectedBranchName = BranchNames[0];
        }

        private static List<BranchInfo> GetBranches(string repoPath)
        {
            var branches = new List<BranchInfo>();
            if (!Directory.Exists(repoPath))
            {
                MessageBox.Show($"Cannot find folder \"{repoPath}\"", "Error");
                return branches;
            }

            using (var repo = new Repository(repoPath))
            {
                foreach (var branch in repo.Branches.Where(branch => !branch.IsRemote))
                {
                    branches.Add(new BranchInfo(branch.FriendlyName));
                }
            }

            branches = branches.OrderBy(b => b.Name).ToList();
            return branches;
        }

        private void ExportMethod()
        {
            Export(SelectedBranchName, Formats, StartDate, EndDate);
        }

        private void Export(string branchName, List<CommitExportFormat> formats, DateTime? startDate, DateTime? endDate)
        {
            if (formats.IsNullOrEmpty())
            {
                MessageBox.Show("please specify a valid format.");
                return;
            }

            if (string.IsNullOrWhiteSpace(branchName))
            {
                MessageBox.Show("please choose a branch.");
                return;
            }

            using (var repo = new Repository(RepoPath))
            {
                var selectedBranch = repo.Branches.FirstOrDefault(b => b.FriendlyName == branchName);
                if (selectedBranch == null)
                {
                    MessageBox.ShowError($"Cannot find branch \"{branchName}\"");
                    return;
                }

                var commits = selectedBranch.Commits;
                var commitInfos = new List<string>();
                foreach (var commit in commits)
                {
                    var commitDate = commit.Author.When.Date;
                    if (endDate != null && commitDate > endDate)
                    {
                        continue;
                    }

                    if (startDate != null && commitDate < startDate)
                    {
                        break;
                    }

                    var commitInfo = GetCommitInfo(formats, commit);
                    commitInfos.Add(commitInfo);
                }

                var fileNameRegex = new Regex("[/.]+");
                var fileName = fileNameRegex.Replace(branchName, "_") + "_" + CommitOutputFile;
                File.WriteAllLines(fileName, commitInfos);
                MessageBox.Show($"Commit messages are exported successfully into file \"{fileName}\"", "Exported Successfully");
            }
        }

        private string GetCommitInfo(List<CommitExportFormat> formats, Commit commit)
        {
            var result = new List<string>();
            foreach (var format in formats)
            {
                switch (format.MessageType)
                {
                    case CommitMessageType.CommitShort:
                        result.Add(commit.Sha.Substring(0, 9));
                        break;
                    case CommitMessageType.Commit:
                        result.Add(commit.Sha);
                        break;
                    case CommitMessageType.AuthorName:
                        result.Add(commit.Author.Name);
                        break;
                    case CommitMessageType.Date:
                        result.Add(commit.Author.When.ToString());
                        break;
                    case CommitMessageType.DateWithFormat:
                        result.Add(commit.Author.When.ToString(format.SecondPart));
                        break;
                    case CommitMessageType.MessageShort:
                        var messageShort = commit.MessageShort;
                        result.Add(messageShort.Trim());
                        break;
                    case CommitMessageType.Message:
                        var message = commit.Message;
                        result.Add(message.Trim());
                        break;
                    case CommitMessageType.Text:
                        result.Add(format.SecondPart);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var oneLine = string.Join("", result);
            oneLine = oneLine.Trim();
            return oneLine;
        }

        private string GetExample(List<CommitExportFormat> formats)
        {
            var exampleParts = new List<string>();
            foreach (var format in formats)
            {
                switch (format.MessageType)
                {
                    case CommitMessageType.CommitShort:
                        exampleParts.Add("e1a293c51d192eaf2247591009a834df9e97d2d9 ");
                        break;
                    case CommitMessageType.Commit:
                        exampleParts.Add("e1a293c51");
                        break;
                    case CommitMessageType.AuthorName:
                        exampleParts.Add("Bob");
                        break;
                    case CommitMessageType.Date:
                        exampleParts.Add(DateTime.Now.ToString());
                        break;
                    case CommitMessageType.DateWithFormat:
                        exampleParts.Add(DateTime.Now.ToString(format.SecondPart));
                        break;
                    case CommitMessageType.MessageShort:
                        exampleParts.Add("Merged PR 100: bug100: fix sth");
                        break;
                    case CommitMessageType.Message:
                        exampleParts.Add("Merged PR 100: bug100: fix something");
                        break;
                    case CommitMessageType.Text:
                        exampleParts.Add(format.SecondPart);
                        break;
                }
            }

            return string.Join("", exampleParts);
        }

        private List<CommitExportFormat> Parse(string text)
        {
            var format = new List<CommitExportFormat>();
            if (string.IsNullOrWhiteSpace(text))
            {
                return format;
            }

            var EscapedOpenSqureBracket = "@@EscapedOpenBracket";
            var EscapedCloseSqureBracket = "@@EscapedCloseBracket";
            text = new Regex(@"(?<!\\)\\t").Replace(text, "\t");
            text = new Regex(@"(?<!\\)\\\[").Replace(text, EscapedOpenSqureBracket);
            text = new Regex(@"(?<!\\)\\\]").Replace(text, EscapedCloseSqureBracket);

            var regex = new Regex(@"([^\[\]]*)\[((\w+)\-?([\w:\-%.,]+)?)\]([^\[\]]*)");
            var matches = regex.Matches(text);
            if (matches.Count == 0)
            {
                return format;
            }

            foreach (Match match in matches)
            {
                if (!match.Success)
                {
                    continue;
                }

                var prefixText = GetCaptureValue(match, 1, 0);
                var keyword = GetCaptureValue(match, 3, 0);
                var keywordSecond = GetCaptureValue(match, 4, 0);
                var suffixText = GetCaptureValue(match, 5, 0);
                if (!string.IsNullOrEmpty(prefixText))
                {
                    prefixText = new Regex(EscapedOpenSqureBracket).Replace(prefixText, "[");
                    prefixText = new Regex(EscapedCloseSqureBracket).Replace(prefixText, "]");
                    format.Add(new CommitExportFormat(CommitMessageType.Text, prefixText));
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    format.Add(new CommitExportFormat(keyword, keywordSecond));
                }

                if (!string.IsNullOrEmpty(suffixText))
                {
                    suffixText = new Regex(EscapedOpenSqureBracket).Replace(suffixText, "[");
                    suffixText = new Regex(EscapedCloseSqureBracket).Replace(suffixText, "]");
                    format.Add(new CommitExportFormat(CommitMessageType.Text, suffixText));
                }
            }
            return format;
        }

        private static string GetCaptureValue(Match match, int groupIndex, int captureIndex)
        {
            if (match == null)
            {
                return null;
            }

            if (groupIndex >= match.Groups.Count)
            {
                return null;
            }

            if (captureIndex >= match.Groups[groupIndex].Captures.Count)
            {
                return null;
            }

            return match.Groups[groupIndex].Captures[captureIndex].Value;
        }
    }
}
