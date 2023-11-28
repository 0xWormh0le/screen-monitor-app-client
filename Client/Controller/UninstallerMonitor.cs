using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Client.Controller
{
    class UninstallerMonitor
    {
        public static void StartMonitoring()
        {
            Thread t = new Thread(new ThreadStart(CheckModuleExist));
            t.IsBackground = true;
            t.Start();
        }
        public static void CheckModuleExist()
        {
            while (true)
            {
                try
                {
                    Process p = Process.GetCurrentProcess();
                    String str = p.MainWindowTitle;

                    //if (p.Count() > 0)
                    //    break;
                  
                }
                catch (System.Exception ex)
                {
                    continue;
                }

                Thread.Sleep(300);
                continue;
            }
            MessageBox.Show("You are trying to do against protected system.");
            //Util.Util.Shutdown();
        }
    }
}
