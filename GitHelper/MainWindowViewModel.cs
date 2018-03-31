using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GitHelper.Attributes;
using GitHelper.Helpers;
using GitHelper.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;

namespace GitHelper
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<ExtensionInfo> _actions;
        public ObservableCollection<ExtensionInfo> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                RaisePropertyChanged("Actions");
            }
        }

        private ExtensionInfo _selectedAction;
        public ExtensionInfo SelectedAction
        {
            get => _selectedAction;
            set
            {
                _selectedAction = value;
                DisplayFullInfo();
                RaisePropertyChanged("SelectedAction");
            }
        }

        public Configuration Config { get; set; }

        private static Logger _log = LogManager.GetCurrentClassLogger();

        private string _fullInfo;
        public string FullInfo
        {
            get => _fullInfo;
            set
            {
                _fullInfo = value;
                RaisePropertyChanged("FullInfo");
            }
        }

        public ICommand OpenSettingsCommand { get; private set; }

        public ICommand OpenManageExtensionCommand { get; private set; }

        public ICommand RefreshCommand { get; private set; }

        public MainWindowViewModel()
        {
            DisplayHomeScreen();
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            OpenManageExtensionCommand = new RelayCommand(OpenManageExtension);
            RefreshCommand = new RelayCommand(Refresh);
        }

        private void DisplayHomeScreen()
        {
            Config = Configuration.GetConfiguration();
            var extensions = new ObservableCollection<ExtensionInfo>();
            AddPlugins(extensions);
            AddScripts(extensions);
            FullInfo = StartInfo();
            Actions = new ObservableCollection<ExtensionInfo>(extensions.OrderBy(a => a.Name));
        }

        private void Refresh()
        {
            DisplayHomeScreen();
        }

        private void AddScripts(ObservableCollection<ExtensionInfo> extensions)
        {
            var newExtensions = ExtensionViewModelHelper.GetExtensionFiles(Config.ExtensionPaths);
            foreach (var e in newExtensions)
            {
                var extensionInfo = new ExtensionInfo(e);
                extensions.Add(extensionInfo);
            }
        }

        private void AddPlugins(ObservableCollection<ExtensionInfo> extensions)
        {
            var types = GetTypesWith<GitHelperActionAttribute>(false).ToList();
            foreach (var type in types)
            {
                var instance = (IGitHelperActionMeta)Activator.CreateInstance(type);
                var extensionInfo = new ExtensionInfo(instance, Config);
                extensions.Add(extensionInfo);
            }
        }

        private void OpenManageExtension()
        {
            WindowService.ShowWindow<ManageExtensions>(new ManageExtensionsViewModel(Config));
        }

        public void OpenSettings()
        {
            WindowService.ShowWindow<Settings>(new SettingsViewModel());
        }

        private void DisplayFullInfo()
        {
            if (SelectedAction == null)
            {
                return;
            }

            FullInfo = SelectedAction.GetFullInfo();
        }

        IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
                              where TAttribute : Attribute
        {
            return from a in AppDomain.CurrentDomain.GetAssemblies()
                   from t in a.GetTypes()
                   where t.IsDefined(typeof(TAttribute), inherit)
                   select t;
        }

        private string StartInfo()
        {
            var uri = new Uri("pack://application:,,,/HtmlPages/StartPage.html");
            var stream = Application.GetResourceStream(uri);
            using (var reader = new StreamReader(stream.Stream, Encoding.GetEncoding("UTF-8")))
            {
                return reader.ReadToEnd();
            }
            return @"<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
<html>
<style>
    html {
        overflow: hidden;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif
    }

    h4 {
        line-height: 1.1em;
        margin-bottom: 0.2em;
    }

    .button-icon {
        width: 22px;
        height: 22px;
        border: 1px solid #eee;
        border-radius: 3px;
        display: inline-block;
        vertical-align: text-bottom;
    }

    .inline-code {
        display: inline;
        font-family: 'Consolas', Courier, monospace
    }

    .inline-header {
        display: inline;
        background: #f1f1f1;
        border-radius: 3px;
        padding: 2px 5px;
    }
</style>

<body>
    <h2>Git Helper</h2>
    <h4>This application can...</h4>
    <p>
        <span class=inline-header>Make
            <span class=inline-code>git</span> easier</span>
        Click a button on the right to run an action. Move mouse over a button to see the detail.
    </p>
    <p>
        <span class=inline-header>Run
            <span class=inline-code>.bat</span> files</span>
        Click
        <img class=button-icon src=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAN1wAADdcBQiibeAAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAJvSURBVGiB7Zi7btRAFIa/mbGTZS/eJYREhIsiKgoKmiCoaECCJhUSDRKPAXkDaBBPgICKJhFFpFAQBZoIUdFwiSgoIhCCiFy8m0121x460O5Y2qw9dhLhr5vj8Tn/75ljjQ05OTk5Of8zIiqon3GGkIfAFWB0j5nuiTs8sKhtj2V70I85jeQ9MBIjW+YmpBFRPCKOeADNff2Uuwk1DYRpQHMtUcaMTZhb6Aka14XxMSgVQamstEQTBiE7uz/Y2b4trq8t9V42V8B14ewkeJX9Fw8glaRYnKB2dFEvHJsyLhs3jI8dDOG9SCU4UpozwsbEcikTPbEoFE72hhxjkjQ9xcEPFPN+la+tYUadDje934w4QbKkUho9a0dtBLN+jY+7BZpasNp2eeHXUqmTmoHV1lDX+GfHTaVOagZOOO2u8YTbSqWO2QMxaYSKtUCxFSj8QNLQ3c9mI3BYbpSoqBBPhow6HUoyYU9gycByo8TLhofWkWdDAH51HBbq1b9jITQ3yj6Xi/VEta1sodd9xEehtWCpXk5c24qB5oDi/92XvHxqTZwVh96AlSa+VVvnlNPCEZq32yXeNCo20u4JKytwfrhJTQWUZcjVss/kUDrv/ChS2ULHVbv/JEukYuBbZ6j/JEtYMfBhp8B2KKmHknm/yvd2OueeKKw08fPNeP8AbHDoX6O5AQAv5qmyqpKfRq0YmK5uUBlQjCcDpisbiWub/4XendsEvMSZ02FTXPzc9W0atQKLGYmJw6vegGlA6BkE65nIGQTBOqGY6Q0bBsTUygpCXQBmAT8LbX3YAuYQnSlx6dOX/RaTk5OTk3Ow+APS7Js+6ienVwAAAABJRU5ErkJggg==""
        /> button to add/delete
        <span class=inline-code>.bat</span> files.
    </p>
</body>

</html>";
            var doc = XmlToFlowDocument();
            return doc.ToXaml();
        }

        private static FlowDocument XmlToFlowDocument()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(@"<FlowDocument FontSize='14'>
<Paragraph>
<Bold>Git Helper</Bold>
</Paragraph>
<Paragraph>
<Image>pack://application:,,,/Images/Settings.png</Image>: This 
</Paragraph>
</FlowDocument>");
            var doc = new FlowDocument();
            if (xmlDoc == null)
            {
                return doc;
            }

            if (xmlDoc.DocumentElement.HasAttributes)
            {
                foreach (XmlAttribute attr in xmlDoc.DocumentElement.Attributes)
                {
                    var name = attr.Name.ToLower();
                    switch (name)
                    {
                        case "fontsize":
                            doc.FontSize = double.Parse(attr.Value);
                            break;
                    }
                }
                foreach (XmlNode child in xmlDoc.DocumentElement.ChildNodes)
                {
                    var name = child.Name.ToLower();
                    switch (name)
                    {
                        case "paragraph":
                            doc.Blocks.Add(GetParagraph(child));
                            break;
                    }
                }
            }

            doc.FontSize = 14;
            return doc;
        }

        private static Block GetParagraph(XmlNode node)
        {
            var paragraph = new Paragraph();
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = child.Name.ToLower();
                Inline inline = null;
                switch (name)
                {
                    case "bold":
                        var run = new Run(child.InnerText);
                        SetAttribute(child, run);
                        inline = new Bold(run);
                        break;
                    case "#text":
                        inline = new Run(child.Value);
                        break;
                    case "image":
                        BitmapImage bi = new BitmapImage(new Uri(child.InnerText));
                        Image image = new Image();
                        image.Source = bi;
                        image.Width = 20;
                        image.Height = 20;
                        inline = new InlineUIContainer(image);
                        break;
                }
                if (inline != null)
                {
                    paragraph.Inlines.Add(inline);
                }
            }
            return paragraph;
        }

        private static void SetAttribute(XmlNode node, TextElement e)
        {
            if (node.Attributes == null || node.Attributes.Count == 0)
            {
                return;
            }

            foreach (XmlAttribute attr in node.Attributes)
            {
                var name = attr.Name.ToLower();
                switch (name)
                {
                    case "fontsize":
                        e.FontSize = double.Parse(attr.Value);
                        break;
                }
            }
        }
    }
}
