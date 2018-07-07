using System;
using System.Diagnostics;

namespace GitHelper.Helpers
{
    public class ExecuteCommand
    {
        public ExecuteCommand()
        { }

        public void Execute(string workingDirectory, string command)
        {
            // create the ProcessStartInfo using "cmd" as the program to be run, and "/c " as the parameters.
            // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.
            var procStartInfo = new ProcessStartInfo("cmd", "/c " + command);

            procStartInfo.WorkingDirectory = workingDirectory;

            //redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            //redirected to the Process.StandardError StreamReader. (same as StdOutput)
            procStartInfo.RedirectStandardError = true;

            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            var proc = new Process();

            //This is important, else some Events will not fire!
            proc.EnableRaisingEvents = true;
            // passing the Startinfo to the process
            proc.StartInfo = procStartInfo;
            // The given function will be raised if the Process wants to print an output to console                    
            proc.OutputDataReceived += OutputDataReceived;
            // Std Error
            proc.ErrorDataReceived += ErrorDataReceived;
            // If Batch File is finished this Event will be raised
            proc.Exited += Exited;
            proc.Start();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
        }

        public event EventHandler Exited;

        public event DataReceivedEventHandler ErrorDataReceived;

        public event DataReceivedEventHandler OutputDataReceived;
    }
}
