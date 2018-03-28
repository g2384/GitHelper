using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using LibGit2Sharp;

namespace GitHelper
{
    /// <summary>
    /// Interaction logic for ExportCommitMessages.xaml
    /// </summary>
    public partial class ExportCommitMessages : Window
    {
        public Configuration Config { get; set; }
        public BranchInfo Branch { get; set; }
        public List<CommitExportFormat> Formats { get; set; }

        public ExportCommitMessages(BranchInfo branch, Configuration config)
        {
            InitializeComponent();
            var examples = new List<string>();
            foreach (var keyValuePair in CommitExportFormat.KeywordsExample)
            {
                examples.Add(keyValuePair.Key + ": " + keyValuePair.Value);
            }
            KeywordsBlock.Text = string.Join(Environment.NewLine, examples);
            Config = config;
            Branch = branch;
            FormatTextBlock.Text = "[author-name]\\t[date-g]\\t[message]";
        }

        private void Export(List<CommitExportFormat> formats, DateTime startDate)
        {
            if (formats == null || formats.Count == 0)
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
                    if (commit.Author.When.Date < startDate)
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
            return oneLine;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DateTime startDate = StartDatePicker.SelectedDate ?? new DateTime();
            Export(Formats, startDate);
        }

        private void FormatTextBlock_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = FormatTextBlock.Text;
            Formats = Parse(text);
            var example = GetExample(Formats);
            ExampleTextBlock.Text = example;
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
