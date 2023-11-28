using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;

namespace Client.Util
{
    public class NetInformation
    {
        private  static PhysicalAddress MAC_ADDRESS = null;
        private static string IP_Address = null;

        public static  PhysicalAddress GetMACAddress()
        {
            if (MAC_ADDRESS != null)
                return MAC_ADDRESS;

            List<PhysicalAddress> list = new List<PhysicalAddress>();
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            if (nics == null || nics.Length < 1)
            {
                //  Console.WriteLine("  No network interfaces found.");
                return null;
            }

            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties properties = adapter.GetIPProperties(); //  .GetIPInterfaceProperties();
                UnicastIPAddressInformationCollection anyCast = properties.UnicastAddresses;
                if (anyCast.Count == 0) continue;

                PhysicalAddress address = adapter.GetPhysicalAddress();
                list.Add(address);
            }

            if (list.Count > 0)
                return list[0];
            else
                return null;
        }

        
    }
}
