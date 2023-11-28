using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Client.Controller
{
    public class USBController
    {
        static RegistryMonitor g_regMonitor = null;
        static bool g_isBlocked = false;

        static public void SetBlock(bool isBlock)
        {
            return;
            if(isBlock && !IsBlocked())
            {
                setConfig(4);
                setRegConf(0);
                g_regMonitor = new RegistryMonitor(RegistryHive.LocalMachine, "SYSTEM\\CurrentControlSet\\Services");
                g_regMonitor.RegChanged += new EventHandler(g_regMonitor_RegChanged);
                g_regMonitor.Start();
                g_isBlocked = true;
            }
            else if (!isBlock)
            {
                if (g_regMonitor != null)
                {
                    g_regMonitor.Stop();
                    g_regMonitor = null;
                }
                setRegConf(1);
                setConfig(3);
                g_isBlocked = false;
            }
        }

        static void g_regMonitor_RegChanged(object sender, EventArgs e)
        {
            setConfig(4);
        }

        static private void setConfig(int value)
        {
            try
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\USBSTOR", "Start", value, Microsoft.Win32.RegistryValueKind.DWord);
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\VMUSB", "Start", value, Microsoft.Win32.RegistryValueKind.DWord);
            }
            catch (System.Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        static public bool IsBlocked()
        {
            return g_isBlocked;
        }

        static private void setRegConf(int value)
        {
            try
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services", "DefaultService", value, Microsoft.Win32.RegistryValueKind.DWord);
            }
            catch (System.Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        static private int getRegConf()
        {
            return (int)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services", "DefaultService", 0);
        }

        static public void StartProtectSystem()
        {
            SetBlock(true);
        }
    }
}
