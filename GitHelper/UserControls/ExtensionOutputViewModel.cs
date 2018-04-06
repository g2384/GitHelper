using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GitHelper.UserControls
{
    public class ExtensionOutputViewModel : ViewModelBase
    {
        private string _output;
        private readonly string _filePath;
        private readonly string _workingDirectory;
        private readonly TaskScheduler _taskScheduler;

        public ICommand RunCommand { get; }

        public ExtensionOutputViewModel(string filePath, string workingDirectory, TaskScheduler taskScheduler)
        {
            _filePath = filePath;
            _workingDirectory = workingDirectory;
            _taskScheduler = taskScheduler;
            RunCommand = new RelayCommand(Run);
        }

        public string Output
        {
            get => _output;
            set => Set(ref _output, value);
        }

        public void Run()
        {
            var executeCommand = new Helpers.ExecuteCommand();
            executeCommand.OutputDataReceived += OutputDataReceived;
            executeCommand.ErrorDataReceived += ErrorDataReceived;
            Task.Factory.StartNew(() =>
            {
                executeCommand.Execute(_workingDirectory, _filePath);
            }).ContinueWith(task => { }, CancellationToken.None, TaskContinuationOptions.None, _taskScheduler);
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
