using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Collections;
using System.Threading;

namespace Client.Controller
{
    public class UsbDevice : IEquatable<UsbDevice>
    {
        public string devName;
        public string decCaption;

        public UsbDevice()
        {
            this.devName = string.Empty;
            this.decCaption = string.Empty;
        }

        public bool Equals(UsbDevice other)
        {
            if (this.devName == other.devName & this.decCaption == other.decCaption)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

    class USBDetector
    {
         List<UsbDevice> deviceList;

         Timer usbDetectTimer;

        private bool bAddRemove;

        public static Hashtable _usbdevice = new Hashtable();

        public delegate void USBDeviceEventHandler(Object sender, USBDeviceEventArgs e);

        public event USBDeviceEventHandler DeviceDetected;

        public USBDetector()
        {
            
        }

        public void Start()
        {
           
            deviceList = RefreshDisk();
            usbDetectTimer = new Timer(new TimerCallback(usbDetectTimer_Tick), null, 0, 1000);
        }

        public void Stop()
        {
          
        }


        private List<UsbDevice> RefreshDisk()
        {
            List<UsbDevice> devList = new List<UsbDevice>();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                new ManagementScope(@"\root\cimv2"),
                new WqlObjectQuery("Select * From Win32_DiskDrive "),
                null);
            
            foreach (ManagementObject mo in searcher.Get())
            {
                UsbDevice dev = new UsbDevice();

                try
                {
                    dev.decCaption = (string)mo.GetPropertyValue("Caption");
                    dev.devName = mo["Name"].ToString();
                }
                catch (SystemException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                devList.Add(dev);
            }
            
            return devList;
        }

        private List<UsbDevice> DetectDevChange(List<UsbDevice> oldList, List<UsbDevice> newList)
        {
            List<UsbDevice> newDevs = new List<UsbDevice>();

            

            if (oldList.Count < newList.Count)
            {
                foreach (UsbDevice dev in newList)
                {
                    if (oldList.Contains(dev) == false)
                    {
                        newDevs.Add(dev);
                    }
                }
                bAddRemove = true;
            }
            else if (oldList.Count > newList.Count)
            {
                foreach (UsbDevice dev in oldList)
                {
                    if (newList.Contains(dev) == false)
                        newDevs.Add(dev);
                }
                bAddRemove = false;
            }

            return newDevs;
        }

        private void usbDetectTimer_Tick(object state)
        {
          
            //usbDetectTimer.

            List<UsbDevice> nowDevs = RefreshDisk();

            List<UsbDevice> newDevs = DetectDevChange(this.deviceList, nowDevs);

            deviceList = nowDevs;

            foreach (UsbDevice dev in newDevs)
            {
                if (DeviceDetected != null)
                {
                    USBDeviceEventArgs earg = new USBDeviceEventArgs();
                    earg.DeviceName = dev.decCaption;
                    
                    DateTime nowTime = DateTime.Now;
                    string now = nowTime.ToString("yyyy년 M월 d일 tt hh시 mm분 ss초");//nowTime.Year.ToString() + "-" + nowTime.Month.ToString() + "-" + nowTime.Day.ToString() + "-" + nowTime.Hour.ToString() + "-" + nowTime.Minute.ToString() + "-" + nowTime.Second.ToString();
                    if (bAddRemove == true)
                    {
                        earg.id = Guid.NewGuid().ToString();
                        _usbdevice.Add(dev.devName, earg.id);
                        earg.PlugTime = now;
                    }
                    else
                    {
                        earg.id = (string)_usbdevice[dev.devName];
                        _usbdevice.Remove(dev.devName);
                        earg.EjectTime = now;
                    }
                    if (USBController.IsBlocked())
                        earg.State = true;
                    else
                        earg.State = false;
                    DeviceDetected(this, earg);
                }
            }

            Util.Util.ReleaseMemory();
        }    

        public class USBDeviceEventArgs : EventArgs
    {
        public string DeviceName = "Unknown device";
        public string PlugTime = string.Empty;
        public string EjectTime = string.Empty;
        public bool State;
        public string Content = string.Empty;
        public string id = string.Empty;
    }    
    }
}
