using System.Collections.Generic;

namespace ExportCommitMessages
{
    public class CommitExportFormat
    {
        public CommitMessageType MessageType { get; set; }

        /// <summary>
        /// this can be date format if type is a DateWithFormat, or text if type is Text
        /// </summary>
        public string SecondPart { get; set; }

        private const string CommitShortKey = "commit-short";
        private const string CommitKey = "commit";
        private const string AuthorNameKey = "author-name";
        private const string MessageShortKey = "message-short";
        private const string MessageKey = "message";
        private const string DateKey = "date";

        public static readonly Dictionary<string, CommitMessageType> KeywordsDictionary =
            new Dictionary<string, CommitMessageType>()
            {
                {CommitShortKey, CommitMessageType.CommitShort },
                {CommitKey, CommitMessageType.Commit },
                {AuthorNameKey,CommitMessageType.AuthorName },
                {MessageShortKey,CommitMessageType.MessageShort},
                {MessageKey,CommitMessageType.Message},
                {DateKey,CommitMessageType.Date }
            };

        public static readonly Dictionary<string, string> KeywordsExample = new Dictionary<string, string>()
            {
                {$"[{CommitShortKey}]","short hash code, e.g. e1a293c51"},
                {$"[{CommitKey}]", "long hash code, e.g. e1a293c51d192eaf2247591009a834df9e97d2d9"},
                {$"[{AuthorNameKey}]", "auther name, e.g. Bob"},
                {$"[{DateKey}]", "date, e.g. 01 January 1978 23:00:00"},
                {$"[{DateKey}-g]", "date with a specified format \"g\""},
                {$"[{MessageShortKey}]", "short message"},
                {$"[{MessageKey}]", "full message"},
                {"\\t", " tab"},
                {"\\[", "escape["},
                {"\\]", "escape]"}
            };

        public CommitExportFormat(CommitMessageType type, string dateFormat = null)
        {
            MessageType = type;
            SecondPart = dateFormat;
        }

        public CommitExportFormat(string keyword, string keywordSecondPart)
        {
            if (keyword == "date" && !string.IsNullOrEmpty(keywordSecondPart))
            {
                MessageType = CommitMessageType.DateWithFormat;
                SecondPart = keywordSecondPart;
                return;
            }

            if (KeywordsDictionary.ContainsKey(keyword + "-" + keywordSecondPart))
            {
                MessageType = KeywordsDictionary[keyword + "-" + keywordSecondPart];
                return;
            }

            if (KeywordsDictionary.ContainsKey(keyword))
            {
                MessageType = KeywordsDictionary[keyword];
                return;
            }

            MessageType = CommitMessageType.Text;
            SecondPart = keyword;
        }
    }
}
