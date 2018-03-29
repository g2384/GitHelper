using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace GitHelper.ViewModel
{
    public class ExportCommitMessageViewModel : ViewModelBase
    {
        public Configuration Config { get; set; }
        public BranchInfo Branch { get; set; }
        public List<CommitExportFormat> Formats { get; set; }
        public ICommand ExportCommand { get; private set; }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                RaisePropertyChanged("StartDate");
            }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                RaisePropertyChanged("EndDate");
            }
        }

        private string _keywords;
        public string Keywords
        {
            get { return _keywords; }
            set
            {
                _keywords = value;
                RaisePropertyChanged("Keywords");
            }
        }

        private string _example;
        public string Example
        {
            get { return _example; }
            set
            {
                _example = value;
                RaisePropertyChanged("Example");
            }
        }


        private string _format;
        public string Format
        {
            get { return _format; }
            set
            {
                _format = value;
                Formats = Parse(_format);
                var example = GetExample(Formats);
                Example = example;
                RaisePropertyChanged("Format");
            }
        }

        public ExportCommitMessageViewModel(BranchInfo branch, Configuration config)
        {
            var explainations = new List<string>();
            foreach (var keyValuePair in CommitExportFormat.KeywordsExample)
            {
                explainations.Add(keyValuePair.Key + ": " + keyValuePair.Value);
            }
            Keywords = string.Join(Environment.NewLine, explainations);
            Config = config;
            Branch = branch;
            Format = "[author-name]\\t[date-g]\\t[message]";
            ExportCommand = new RelayCommand(ExportMethod);
            StartDate = new DateTime(2018, 1, 1);
            EndDate = DateTime.Now;
        }

        private void ExportMethod()
        {
            Export(Formats, StartDate, EndDate);
        }

        private void Export(List<CommitExportFormat> formats, DateTime? startDate, DateTime? endDate)
        {
            if (Utility.IsNullOrEmpty(formats))
            {
                return;
            }

            using (var repo = new Repository(Config.RepoPath))
            {
                var selectedBranch = repo.Branches.FirstOrDefault(b => b.FriendlyName == Branch.Name);
                if (selectedBranch == null)
                {
                    MessageBox.Show($"Cannot find branch \"{Branch.Name}\"", "Error");
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
                var fileName = fileNameRegex.Replace(Branch.Name, "_") + "_" + MainWindow.CommitOutputFile;
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
                        result.Add(commit.MessageShort);
                        break;
                    case CommitMessageType.Message:
                        result.Add(commit.Message);
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
            List<string> exampleParts = new List<string>();
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

            string EscapedOpenSqureBracket = "@@EscapedOpenBracket";
            string EscapedCloseSqureBracket = "@@EscapedCloseBracket";
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
