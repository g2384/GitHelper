using GalaSoft.MvvmLight.Command;
using GitHelper.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace GitHelper
{
    public class ActionInfo
    {
        public const string NewWindowToolTip = "This plugin opens a new window";

        public string Title { get; set; }

        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public IList<ActionFeatures> Features { get; set; }

        public ICommand ExecuteCommand { get; private set; }

        private IGitHelperActionMeta _actionMeta;
        private Configuration _config;

        public bool IsPlugin => Features.Contains(ActionFeatures.IsPlugin);

        public bool IsScript => Features.Contains(ActionFeatures.IsScript);

        public bool HasNewWindow => Features.Contains(ActionFeatures.HasNewWindow);

        public ActionInfo(IGitHelperActionMeta actionMeta, Configuration config)
        {
            Title = actionMeta.Title;
            Description = actionMeta.Description;
            ShortDescription = actionMeta.Description.Truncate(50);
            Features = actionMeta.Features;
            _actionMeta = actionMeta;
            _config = config;
            ExecuteCommand = new RelayCommand(Execute);
        }

        private void Execute()
        {
            _actionMeta.ShowDialog(_config);
        }

        internal string GetFullInfo()
        {
            var doc = new FlowDocument();
            var titleRun = new Run(Title);
            titleRun.FontSize = 16;
            var title = new Bold(titleRun);
            var titleParagraph = new Paragraph(title);

            var descriptionRun = new Run(Description);
            var descriptionParagraph = new Paragraph(descriptionRun);

            var opensNewWindow = new Run(NewWindowToolTip);
            var opensNewWindowParagraph = new Paragraph(opensNewWindow);

            doc.Blocks.Add(titleParagraph);
            doc.Blocks.Add(descriptionParagraph);
            doc.Blocks.Add(opensNewWindowParagraph);
            var result = ToXaml(doc)?.ToString();
            return result;
        }

        public string ToXaml(FlowDocument flowDocument)
        {
            if (flowDocument == null)
            {
                return null;
            }

            // write XAML out to a MemoryStream
            TextRange tr = new TextRange(
                flowDocument.ContentStart,
                flowDocument.ContentEnd);

            string xamlText;
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Xaml);
                xamlText = ASCIIEncoding.Default.GetString(ms.ToArray());
            }

            return xamlText.ToString();
        }
    }
}
