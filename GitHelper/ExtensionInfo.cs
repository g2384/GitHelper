using GalaSoft.MvvmLight.Command;
using GitHelper.Extension;
using GitHelper.Extension.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace GitHelper
{
    public class ExtensionInfo
    {
        public const string OpensANewWindow = "opens a new window";
        public const string NewWindowToolTip = "This extension " + OpensANewWindow;

        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public IList<ExtensionFeatures> Features { get; set; }

        public ICommand ExecuteCommand { get; private set; }

        public string WorkingDirectory { get; set; }

        private IGitHelperActionMeta _actionMeta;
        private Configuration _config;

        public bool IsPlugin => Features.Contains(ExtensionFeatures.IsAssembly);

        public bool IsScript => Features.Contains(ExtensionFeatures.IsScript);

        public bool HasNewWindow => Features.Contains(ExtensionFeatures.HasNewWindow);

        private string _filePath;

        public ExtensionInfo(IGitHelperActionMeta actionMeta, Configuration config)
        {
            Name = actionMeta.Name;
            Description = actionMeta.Description;
            ShortDescription = actionMeta.Description.Truncate(50);
            Features = actionMeta.Features;
            _actionMeta = actionMeta;
            _config = config;
            ExecuteCommand = new RelayCommand(ShowExtensionDialog);
        }

        public ExtensionInfo(IGitHelperExtensionFile extensionFile, Action<string> showDialog)
        {
            Name = extensionFile.Name;
            Description = extensionFile.Description;
            ShortDescription = extensionFile.ShortDescription;
            Features = extensionFile.Features;
            _filePath = extensionFile.FilePath;
            _showDialog = showDialog;
            ExecuteCommand = new RelayCommand(RunFile);
        }

        private Action<string> _showDialog;

        private void RunFile()
        {
            _showDialog(_filePath);
        }

        private void ShowExtensionDialog()
        {
            _actionMeta.ShowDialog(_config);
        }

        internal string GetFullInfo()
        {
            var html = Utility.GetResourceText("pack://application:,,,/HtmlPages/ExtensionInfo.html");
            html = html.Replace("{Name}", Name);
            html = html.Replace("{Description}", Description);
            string featureInfo = string.Empty;
            if (!Utility.IsNullOrEmpty(Features))
            {
                var features = new List<string>();
                foreach (var f in Features)
                {
                    switch (f)
                    {
                        case ExtensionFeatures.HasNewWindow:
                            features.Add(OpensANewWindow);
                            break;
                        case ExtensionFeatures.IsScript:
                            break;
                        case ExtensionFeatures.IsAssembly:
                            break;
                    }
                }
                if (!Utility.IsNullOrEmpty(features))
                {

                    var featuresString = string.Join(", ", features);
                    featureInfo = $@"<div class='bottom'>
        <span class='grey'>This extension {featuresString}</span>
    </div>";
                }
            }

            html = html.Replace("{Features}", featureInfo);
            return html;
        }
    }
}
