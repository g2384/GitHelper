using System.Diagnostics;
using System.Linq;

namespace GitHelper.Common
{
    public static class CommandLineRunner
    {
        public static string Run(string repoPath, string cmd)
        {
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                }
            };
            p.Start();
            var drive = repoPath.Split(':').First();
            p.StandardInput.WriteLine($"{drive}:");
            p.StandardInput.WriteLine($"cd \"{repoPath}\"");
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.Close();
            var output = p.StandardOutput.ReadToEnd();
            p.Close();
            return output;
        }
    }
}
