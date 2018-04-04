using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace GitHelper.UserControls
{
    public class ExtensionOutputViewModel : ViewModelBase
    {
        private string _output;
        private string _filePath;

        public ICommand RunCommand { get; }

        public ExtensionOutputViewModel(string filePath)
        {
            _filePath = filePath;
            RunCommand = new RelayCommand(Run);
        }

        public string Output
        {
            get => _output;
            set
            {
                _output = value;
                RaisePropertyChanged(nameof(Output));
            }
        }

        public void Run()
        {
            var executeCommand = new Helpers.ExecuteCommand();
            executeCommand.OutputDataReceived += OutputDataReceived;
            executeCommand.ErrorDataReceived += ErrorDataReceived;
            executeCommand.Execute(_filePath, _filePath);//Path.GetFileName(_filePath)
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Output += e.Data + Environment.NewLine;
        }

        private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Output += e.Data + Environment.NewLine;
        }
    }
}
