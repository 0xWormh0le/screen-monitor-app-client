using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Management;


namespace Client.Util
{
    public struct MEMORYSTATUS
    {
        public UInt32 dwLength;
        public UInt32 dwMemoryLoad;
        public ulong dwTotalPhys;
        public ulong dwAvailPhys;
        public ulong dwTotalPageFile;
        public ulong dwAvailPageFile;
        public ulong dwTotalVirtual;
        public ulong dwAvailVirtual;

    }
    class Util
    {
        [DllImport("Kernel32.dll")]
        private extern static uint WinExec(string cmdline, uint cmdshow);

        [DllImport("Kernel32.dll")]
        private extern static void GlobalMemoryStatus(out MEMORYSTATUS lpBuffer);

        public static String GetUserName()
        {
            RegistryKey rootKey = Registry.LocalMachine;
            RegistryKey regUserKey = rootKey.CreateSubKey(@"Software\AgentUser");

            
            String userName = (String)regUserKey.GetValue(@"User", @"NoRegister");

            return userName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"><cmd:int><MAC address : 6byte string><content : string></param>
        /// <returns></returns>
        public static String GetMacAddressFromPacket(byte[] packet)
        {

            if (packet.Length < 10) return "";
            string pAddr = string.Empty;

            for (int i = 4; i < 10; i++)
            {
                pAddr += packet[i].ToString("X2");
                if (i != 9)
                {
                    pAddr += "-";
                }
            }
            return pAddr;
        }


        public static void ReleaseMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public static void Restart()
        {
            try
            {
                string path = "shutdown -r -f -t 0";
                WinExec(path, 5);
            }
            catch (System.Exception)
            {
            	
            }
        }
        public static void Shutdown()
        {
            try
            {
                string path = "shutdown -s -f -t 0";
                WinExec(path, 5);
            }
            catch (System.Exception)
            {

            }
        }

        public static void Logoff()
        {
            try
            {
                string path = "shutdown -l -f -t 0";
                WinExec(path, 5);
            }
            catch (System.Exception)
            {

            }
        }

        public static void ExecProgram(object path)
        {
            WinExec((string)path, 5);
        }

        public static void ShowMessageBox(object msg)
        {
            try
            {
                MessageBox.Show((string)msg);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void KillProcess(object id)
        {
            int pid = (int)id;
            try
            {
                Process p = Process.GetProcessById(pid);
                if (p != null)
                {
                    p.Kill();
                }

            }
            catch (System.Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }

        public static string GetDeviceInfoAsXML()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

            xml += "<senddata>";

            xml += "<record>";
            xml += "<name>Computer Name</name>";
            xml += "<value>" + System.Environment.MachineName + "</value>";
            xml += "</record>";

            xml += "<record>";
            xml += "<name>OS Version</name>";
            xml += "<value>" + System.Environment.OSVersion + "</value>";
            xml += "</record>";

            xml += "<record>";
            xml += "<name>User Domain Name</name>";
            xml += "<value>" + System.Environment.UserDomainName + "</value>";
            xml += "</record>";

            xml += "<record>";
            xml += "<name>User Name</name>";
            xml += "<value>" + System.Environment.UserName + "</value>";
            xml += "</record>";

            xml += "<record>";
            xml += "<name>Logical Drives</name>";
            String[] drives = Environment.GetLogicalDrives();
            xml += "<value>" + string.Join(", ", drives) + "</value>";
            xml += "</record>";

            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                xml += "<record>";
                xml += "<name>Network Adapter Description</name>";
                xml += "<value>" + adapter.Description + "</value>";
                xml += "</record>";
            }

            SelectQuery query = new SelectQuery("Win32_DiskDrive");

            // Initialize an object searcher with this query
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            // Get the resulting collection and loop through it
            foreach (ManagementObject envVar in searcher.Get())
            {
                try
                {
                    string modelName = (string)envVar["Model"];

                    xml += "<record>";
                    xml += "<name>HardDisk ModelName</name>";
                    xml += "<value>" + modelName + "</value>";
                    xml += "</record>";
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                try
                {
                    string deviceID = (string)envVar["DeviceID"];

                    xml += "<record>";
                    xml += "<name>HardDisk DeviceID</name>";
                    xml += "<value>" + deviceID + "</value>";
                    xml += "</record>";
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                try
                {
                    UInt64 size = (UInt64)envVar["Size"] / (1024 * 1024 * 1024);

                    xml += "<record>";
                    xml += "<name>HardDisk Capacity</name>";
                    xml += "<value>" + size.ToString() + " GB" + "</value>";
                    xml += "</record>";
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            try
            {
                MEMORYSTATUS st;
                GlobalMemoryStatus(out st);

                ulong size = st.dwTotalPhys / (1024 * 1024);
                xml += "<record>";
                xml += "<name>Physical Memory</name>";
                xml += "<value>" + size.ToString() + " MB" + "</value>";
                xml += "</record>";

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            SelectQuery query2 = new SelectQuery("Win32_Processor");

            // Initialize an object searcher with this query
            ManagementObjectSearcher searcher2 = new ManagementObjectSearcher(query2);
            foreach (ManagementObject envVar in searcher2.Get())
            {
                try
                {
                    string cpuName = (string)envVar["Name"];

                    xml += "<record>";
                    xml += "<name>CPU Name</name>";
                    xml += "<value>" + cpuName + "</value>";
                    xml += "</record>";
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                try
                {
                    string cpuManu = (string)envVar["Manufacturer"];

                    xml += "<record>";
                    xml += "<name>CPU Manufacturer</name>";
                    xml += "<value>" + cpuManu + "</value>";
                    xml += "</record>";
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                try
                {
                    UInt32 cpuClock = (UInt32)envVar["MaxClockSpeed"];

                    xml += "<record>";
                    xml += "<name>CPU Clock</name>";
                    xml += "<value>" + cpuClock + "MHz</value>";
                    xml += "</record>";
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            xml += "</senddata>";
            return xml;
        }

        public static string GetInstalledAppAsXML()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

            xml += "<senddata>";

            RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");

            string[] subkeyNames = key.GetSubKeyNames();
            foreach (string subKeyName in subkeyNames)
            {
                try
                {
                    RegistryKey subkey = key.OpenSubKey(subKeyName);
                    string dispName = (string)subkey.GetValue("DisplayName");
                    string installDir = (string)subkey.GetValue("InstallLocation");
                    string date = (string)subkey.GetValue("InstallDate");

                    if (string.IsNullOrEmpty(dispName))
                        continue;

                    xml += "<record>";
                    xml += "<name>" + dispName + "</name>";
                    if (string.IsNullOrEmpty(date))
                    {
                        date = "unknown";
                    }
                    else
                    {
                        try
                        {
                            string year = date.Substring(0, 4);
                            string month = date.Substring(4, 2);
                            string day = date.Substring(6, 2);

                            date = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day)).ToString();
                        }
                        catch (System.Exception)
                        {
                            date = "unknown";
                        }


                    }
                    xml += "<date>" + date + "</date>";
                    xml += "</record>";

                    subkey.Close();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            xml += "</senddata>";
            key.Close();
            return xml;
        }

       
    }
}
