using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Management;
using System.Diagnostics;


namespace Client.Controller
{
    public class ProcessMonitor
    {
        private static ProcessMonitor _pMonitor = null;
        private bool _isStartMornitoring = false;

        public delegate void NewProcessDetectDelegate(String xml);
        public event NewProcessDetectDelegate NewProcessDetect = null; 

        private ProcessMonitor() { }

        public static ProcessMonitor Instace
        {
            get
            {
                if (_pMonitor == null)
                    _pMonitor = new ProcessMonitor();

                return _pMonitor;
            }
        }
        

        public void StartMonitoring()
        {
            if (_isStartMornitoring)
                return;

            ThreadStart ts = new ThreadStart(MonitorProcess);
            Thread th = new Thread(ts);
            th.IsBackground = true;
            th.Start();
        }

        private void MonitorProcess()
        {
            // Create event query to be notified within 1 second of 
            // a change in a service
            EventQuery query = new EventQuery();
            query.QueryString = "SELECT * FROM" +
                " __InstanceCreationEvent WITHIN 1 " +
                "WHERE TargetInstance isa \"Win32_Process\"";

            // Initialize an event watcher and subscribe to events 
            // that match this query
            ManagementEventWatcher watcher = new ManagementEventWatcher(query);

            while (true)
            {
                ManagementBaseObject e = watcher.WaitForNextEvent();

                string processName = (string)((ManagementBaseObject)e["TargetInstance"])["Name"];
                string path = (string)((ManagementBaseObject)e["TargetInstance"])["ExecutablePath"];
                string cmdLine = (string)((ManagementBaseObject)e["TargetInstance"])["CommandLine"];

                if (NewProcessDetect != null)
                    NewProcessDetect(GetProcessListAsXML());

                Util.Util.ReleaseMemory();
            }

            //Cancel the subscription
            watcher.Stop();
        }

        private String GetXmlFromProcess(Process p)
        {
           // string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            String xml = "";
            string pName = p.MainModule.ModuleName;
            string path = p.MainModule.FileName;
            
            int id = p.Id;
            int priority = p.BasePriority;

            xml += "<record>";
            xml += "<name>" + pName + "</name>";
            xml += "<id>" + id.ToString() + "</id>";
            xml += "<priority>";
            switch (priority)
            {
                case 4:
                    {
                        xml += "Idle";
                        break;
                    }
                case 8:
                    {
                        xml += "Normal";
                        break;
                    }
                case 13:
                    {
                        xml += "High";
                        break;
                    }
                case 24:
                    {
                        xml += "RealTime";
                        break;
                    }
            }
            xml += "</priority>";
            xml += "<path>" + path + "</path>";
            xml += "</record>";
            return xml;
        }

       public  String GetProcessListAsXML()
        {
            Process[] processes = Process.GetProcesses();

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            xml += "<senddata>";
            foreach(Process p in processes)
            {
                try
                {
                    xml += GetXmlFromProcess(p);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            xml += "</senddata>";

            return xml;
        }

       
    }
}
