using System;
using System.Diagnostics;
using System.IO;

namespace SwarmRestarter
{
    class Program
    {
        static void RestartApp(int pid, string applicationName)
        {
            // Wait for the process to terminate
            Process process = null;
            try
            {
                process = Process.GetProcessById(pid);
                //process.Close();
                process.WaitForExit(500);
                //process.Close();
                process.Kill();
                //process.CloseMainWindow();
            }
            catch (ArgumentException ex)
            {
                // ArgumentException to indicate that the 
                // process doesn't exist?   LAME!!
            }
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(applicationName)
            {
                UseShellExecute = false
            };
            //Process.Start(applicationName, "");
            p.Start();
        }

        static void Main(string[] args)
        {
            string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SwarmRestarter\\");
            string botPath = "";
            using (StreamReader sr = File.OpenText(Path.Combine(configDir, "config.txt")))
            {
                //Match info = Regex.Match(sr.ReadToEnd(), @"(.+);(.+);(.+);(.+);(.+)");
                botPath = sr.ReadToEnd();
            }
            RestartApp(int.Parse(args[0]), botPath);
        }
    }
}
