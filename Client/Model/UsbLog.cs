namespace Client.Model
{
    public class UsbLog
    {
        private string _userName = string.Empty;

        private string _macAddress = string.Empty;

        private string _ipAddress = string.Empty;

        private string _deviceName = string.Empty;

        private string _plugTime=string.Empty;

        private string _ejectTime=string.Empty;

        private bool _state = false;

        private string _content = string.Empty;

        private string _id = string.Empty;

        public string UserName
        {
            get { return this._userName; }
            set { _userName = value;}
        }

        public string MACAddress
        {
            get { return this._macAddress; }
            set { _macAddress = value;}
        }

        public string IPAddress
        {
            get { return this._ipAddress; }
            set { _ipAddress = value;}
        }

        public string DeviceName
        {
            get { return _deviceName; }
            set { _deviceName = value;}
        }

        public string PlugTime
        {
            get { return _plugTime; }
            set { _plugTime = value;}
        }

        public string EjectTime
        {
            get { return _ejectTime; }
            set { _ejectTime = value;}
        }

        public bool State
        {
            get { return _state; }
            set { _state = value;}
        }

        public string Content
        {
            get { return _content; }
            set { _content = value;}
        }

        public string id
        {
            get { return _id; }
            set { _id = value; }
        }
    }

    public class UsbLogList 
    {
        public UsbLog[] logs ;

        public void Add(UsbLog log)
        {            
            int len = (this.logs == null) ? 0 : this.logs.Length;
                
            UsbLog[] templog = new UsbLog[len + 1];

            for (int i = 0; i < len; i++)
            {
                templog[i]=this.logs[i];
            }

            templog[len] = log;
            this.logs=new UsbLog[len+1];
            this.logs=templog;
        }
    }
}
