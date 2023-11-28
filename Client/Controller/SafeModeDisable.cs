using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Threading;

namespace Client.Controller
{
    public class SafeModeDisable
    {        
        public static void AddSafeModeBoot()
        {
            Thread th = new Thread(new ThreadStart(SafeModeThreadFunc));
            th.IsBackground = true;
            th.Start();
        }

        private static void SafeModeThreadFunc()
        {
            while(true)
            {
                RegistryKey rootkey = Registry.LocalMachine;
                try
                {
                    try
                    {
                        RegistryKey key1 = rootkey.CreateSubKey(@"SYSTEM\ControlSet001\Control\SafeBoot\Minimal\System Backup Service");
                        if (key1 != null)
                            key1.SetValue("", "Service", RegistryValueKind.String);
                    }
                    catch (System.Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }

                    try
                    {
                        RegistryKey key2 = rootkey.CreateSubKey(@"SYSTEM\ControlSet001\Control\SafeBoot\Network\System Backup Service");
                        if (key2 != null)
                            key2.SetValue("", "Service", RegistryValueKind.String);
                    }
                    catch (System.Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }

                }
                catch (System.Exception ex)
                {
                    Console.Write(ex.ToString());
                }
                rootkey.Close();

                Thread.Sleep(1000);
            }
        }
    }
}
