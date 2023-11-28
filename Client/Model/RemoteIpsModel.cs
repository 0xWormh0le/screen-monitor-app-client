using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client.Model
{
   class RemoteIpsModel
    {
      
     
       Dictionary<String, bool> _isConnected;
  


        public RemoteIpsModel()
        {
            _isConnected = new Dictionary<string, bool>();
          
        }
       
       
        public List<String> remoteIps
        {
            get
            {
                return _isConnected.Keys.ToList();
            }
        }


        public  bool this[String ip]
        {
            get
            {
                return _isConnected[ip];
            }
            set
            {
                if (IsValidIp(ip))
                    _isConnected[ip] = value; 
            }

        }
        
        public void AddIp(String ip)
        {
            _isConnected[ip] = false;
        }

       public void Clear()
        {
            _isConnected.Clear();
        }


       // Get first ip from established connection
        public String mainIp
        {
            get
            {
                foreach (String ip in remoteIps)
                    if (_isConnected[ip]) return ip;
                return "";
            }
        }


        #region Identify validance of  ip

        public bool IsValidIp(String ip)
        {
            return (_isConnected.ContainsKey(ip));
        }
        #endregion

    }
}
