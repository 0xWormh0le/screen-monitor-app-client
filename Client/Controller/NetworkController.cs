using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Management;


namespace Client.Controller
{
    class NetworkController
    {
       static NetworkController _instance = null;

       Model.RemoteIpsModel _agentIpsModel;
       Model.RemoteIpsModel _serverIpsModel;
       Mutex agentIpMut = new Mutex();


       public delegate void ShowMessageDelegate(String content);
       public ShowMessageDelegate _ShowMessage = null;
       
       Timer alive_timer;
       
        public static NetworkController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NetworkController();
                return _instance;
            }
        }

        private NetworkController()
        {

            _agentIpsModel = new Model.RemoteIpsModel();
            _serverIpsModel = new Model.RemoteIpsModel();
        }

        public bool LoadAgentIP()
        {
            RegistryKey rootkey = Registry.LocalMachine;
            RegistryKey regUserKey = rootkey.CreateSubKey(@"Software\AgentUser");
            string ip = (string)regUserKey.GetValue(@"IP", "none");
            if (ip == "none")
            {
                return false;
            }
            agentIpMut.WaitOne();
            _agentIpsModel.Clear();
            _agentIpsModel.AddIp(ip);
            _serverIpsModel.Clear();
            _serverIpsModel.AddIp(ip);
            agentIpMut.ReleaseMutex();
            TryConnectRemoteHostThread();
            return true;
        }

        public void Start()
        {

            // RequestAgentHostIP();

            //construct IP models

            /* Uncomment if you would set ip via dynamically */
            /*
            if (!LoadAgentIP())
            {
                AgentIP agentIP = new AgentIP();
                if (agentIP.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadAgentIP();
                }
            }
            */

            /**************/

            foreach (String ip in Conf.Constant.AGENT_IPS)
            {
                _agentIpsModel.AddIp(ip);
            }


            foreach (String ip in Conf.Constant.SERVER_IPS)
            {
                _serverIpsModel.AddIp(ip);
            }

            /**************/

            //1.Try Tcp connection with server and agents

            TryConnectRemoteHostThread();

            //2.Run a thread for send alive-packet to agents per ALIVE_TIME_INTERVAL
            alive_timer = new Timer(new TimerCallback(sendAlivePacket), null, 0, Conf.Constant.ALIVE_TIME_INTERVAL);
         

            //3.Run a thread for receive broadcast packet from server or agetns.
            Model.UDPServer.StartReceiveForBroadCasting();

            //4.Start Tcp Listener
            Model.TCPServer.GetTCPServer().StartServer(Conf.Constant.TCP_RECEIVE_PORT);

            //5. Monitor Network Adapter Change
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(NetworkChange_NetworkAddressChanged);


        }

        
        public void Parse(Int32 message, String remoteIp, String content)
        {
            switch (message)
            {
                case Conf.NetCommandMessage.REQUEST_AGENT_IP_REPLY:
                    {
                        _agentIpsModel.AddIp(remoteIp);
                        break;
                    }

                case Conf.NetCommandMessage.CMD_SERVER_IDENTIFY_REQ:
                    {
                       //Connect remoteIp
                        if (_agentIpsModel.IsValidIp(remoteIp))
                            _agentIpsModel[remoteIp] = true;
                        else if (_serverIpsModel.IsValidIp(remoteIp))
                            _serverIpsModel[remoteIp] = true;

                        //Send initial information
                        SendInitialInformaion(remoteIp);   
                        
                        break;
                    }
                case Conf.NetCommandMessage.CMD_SET_SCREEN_SHOT_INTERVAL:
                    {
                        try
                        {
                            ScreenCapture.SetHistoryInterval(Int32.Parse(content) * 1000);
                        }
                        catch (Exception e) { }
                        break;
                    }

                case Conf.NetCommandMessage.GET_SCREEN_FULL_IMAGE:
                    {
                        startSendImageThread(remoteIp, Conf.NetCommandMessage.GET_SCREEN_FULL_IMAGE);
                        break;
                    }
                case Conf.NetCommandMessage.GET_SCREEN_THUMB_IMAGE:
                    {
                        startSendImageThread(remoteIp, Conf.NetCommandMessage.GET_SCREEN_THUMB_IMAGE);
                        break;
                    }
               
                case Conf.NetCommandMessage.CMD_USBON_REQ:
                    {
                        Controller.USBController.SetBlock(false);
                        Thread th = new Thread(new ThreadStart(SendUsbState));
                        th.IsBackground = true;
                        th.Start();
                        break;
                    }
                case Conf.NetCommandMessage.CMD_USBOFF_REQ:
                    {
                        Controller.USBController.SetBlock(true);
                        Thread th = new Thread(new ThreadStart(SendUsbState));
                        th.IsBackground = true;
                        th.Start();
                        break;
                    }
                case Conf.NetCommandMessage.CMD_SHUTDOWN_REQ:
                    {
                        Thread th = new Thread(new ThreadStart(Util.Util.Shutdown));
                        th.IsBackground = true;
                        th.Start();
                        break;
                    }
                case Conf.NetCommandMessage.CMD_RESTART_REQ:
                    {
                        Thread th = new Thread(new ThreadStart(Util.Util.Restart));
                        th.IsBackground = true;
                        th.Start();
                        break;
                    }
                case Conf.NetCommandMessage.CMD_LOGOFF_REQ:
                    {
                        Thread th = new Thread(new ThreadStart(Util.Util.Logoff));
                        th.IsBackground = true;
                        th.Start();
                        break;
                    }
               
                case Conf.NetCommandMessage.GET_PROCESS_LIST:
                    {
                        Thread th = new Thread(new ParameterizedThreadStart(SendProcessList));
                        th.IsBackground = true;
                        th.Start(remoteIp);
                        break;
                    }
                case Conf.NetCommandMessage.CMD_SENDMESSAGE_REQ:
                    {

                        if (_ShowMessage != null)
                            _ShowMessage(content);
                        break;
                    }
                    
                case Conf.NetCommandMessage.CMD_EXEC_COMMON_COMMAND:
                     {

                       Thread th = new Thread(new ParameterizedThreadStart(Util.Util.ExecProgram));
                       th.IsBackground = true;
                       th.Start(content);
                       break;
                     }
                case Conf.NetCommandMessage.GET_DEVICE_INFO:
                     {
                       Thread th = new Thread(new ParameterizedThreadStart(SendDeviceInfoList));
                       th.IsBackground = true;
                       th.Start(remoteIp);
                       break;
                     }
                 case Conf.NetCommandMessage.GET_INSTALLED_APPLICATION:
                   {
                       Thread th = new Thread(new ParameterizedThreadStart(SendInstalledApplicationList));
                       th.IsBackground = true;
                       th.Start(remoteIp);
                       break;
                   }
                case Conf.NetCommandMessage.CMD_KILL_PROCESS:
                   {
                       String[] processIds = content.Split(',');
                       foreach (String id in processIds)
                       {
                           try
                           {
                               int pid = int.Parse(id);
                               Thread th = new Thread(new ParameterizedThreadStart(Util.Util.KillProcess));
                               th.IsBackground = true;
                               th.Start(pid);
                           }
                           catch (System.Exception ex)
                           {
                               Console.Write(ex.ToString());
                           }
                       }
                      //Send ProcessList again
                       SendProcessList();
                       break;
                   }
                case Conf.NetCommandMessage.LOG_USB:    //Send Agent local usb log
                   {
                       Thread th = new Thread(new ParameterizedThreadStart(SendUsbLog));
                       th.IsBackground = true;
                       th.Start(remoteIp);
                       break;
                   }
                case Conf.NetCommandMessage.CMD_GET_LOCAL_LOG:  //Send Server local usb and image log
                   {
                      // System.Windows.Forms.MessageBox.Show(remoteIp);
                       Thread th_usb = new Thread(new ParameterizedThreadStart(HistoryController.Instance.SendLocalUsbLogToServer));
                       th_usb.IsBackground = true;
                       th_usb.Start(remoteIp);

                       Thread th_image = new Thread(new ParameterizedThreadStart(HistoryController.Instance.SendLocalImageLogToServer));
                       th_image.IsBackground = true;
                       th_image.Start(remoteIp);
                       break;
                   }

                case Conf.NetCommandMessage.GET_INSTALLED_DATE:
                   {
                       Thread th = new Thread(new ParameterizedThreadStart(SendWorkHistory));
                       th.IsBackground = true;
                       th.Start(remoteIp);
                       break;
                   }
            }
        }


        private void RequestAgentHostIP()
        {
            Model.UDPServer.BroadCastingMSG(Conf.NetCommandMessage.REQUEST_AGENT_IP, "");
        }

        #region initial network connection related
        public void TryConnectRemoteHostThread()
        {
            foreach (String ip in _agentIpsModel.remoteIps)
            {
                //Thread thread = new Thread(new ParameterizedThreadStart(TryConnectRemoteHost));
                //thread.IsBackground = true;
                //thread.Start(ip);
                TryConnectRemoteHost(ip);
            }
            foreach (String ip in _serverIpsModel.remoteIps)
            {
                //Thread serverthread = new Thread(new ParameterizedThreadStart(TryConnectServer));
                //serverthread.IsBackground = true;
                //serverthread.Start(ip);
                TryConnectServer(ip);
            }
          
           
        }

        private void TryConnectRemoteHost(Object ipObj)
        {
            String ip = ipObj.ToString();

            agentIpMut.WaitOne();
            _agentIpsModel[ip] = true; //  Model.TCPServer.GetTCPServer().IsConnected(ip, Conf.Constant.TCP_SEND_PORT);
            if (_agentIpsModel[ip]) SendInitialInformaion(ip);
            agentIpMut.ReleaseMutex();

        }

        private void TryConnectServer(Object ipObj)
        {
            String ip = ipObj.ToString();
            _serverIpsModel[ip] = true; //  Model.TCPServer.GetTCPServer().IsConnected(ip, Conf.Constant.TCP_SEND_PORT);
            if (_serverIpsModel[ip]) SendInitialInformaion(ip);
        }

        /// <summary>
        /// Send MAC address and user name to remoteIp
        /// </summary>
        /// <param name="remoteIp">Ip of server and agents</param>
        private void SendInitialInformaion(String remoteIp)
        {
            List<byte> buffer = new List<byte>();

            //buffer.AddRange(BitConverter.GetBytes(Conf.NetCommandMessage.CMD_SERVER_IDENTIFY_REQ));
            //buffer.AddRange(Util.NetInformation.GetMACAddress().GetAddressBytes());
            buffer.AddRange(Encoding.Unicode.GetBytes(HistoryController.Instance.GetSystemInstalledDate()));        //16byte
            buffer.AddRange(Encoding.Unicode.GetBytes(Util.Util.GetUserName()));

           //Model.UDPServer.BroadCastingMSG(remoteIp, buffer.ToArray());
            String str = HistoryController.Instance.GetSystemInstalledDate();
            if (!USBController.IsBlocked())
                str += "1";
            else
                str += "0";
            str += Util.Util.GetUserName();
                
            Model.UDPServer.BroadCastingMSG(remoteIp, Conf.NetCommandMessage.CMD_SERVER_IDENTIFY_REQ, str);
           
           // Model.TCPServer.GetTCPServer().SendMessage (remoteIp, Conf.Constant.TCP_SEND_PORT, Conf.NetCommandMessage.CMD_SERVER_IDENTIFY_REQ, buffer.ToArray());
            
        }

      /// <summary>
      /// Timer Callback delegate
      /// </summary>
      /// <param name="state"></param>
        private void sendAlivePacket(object state)
        {
            String str = new String(' ', 100);
            foreach (String ip in _agentIpsModel.remoteIps)
            {
                if (_agentIpsModel[ip])
                   Model.UDPServer.BroadCastingMSG(ip, Conf.NetCommandMessage.CMD_CLIENT_RUNNING, str);
            }
            Util.Util.ReleaseMemory();
        }

        #endregion


        #region Send Captured Image
        private void startSendImageThread(Object ip, int command)
        {
            Thread t = null;
            if (command == Conf.NetCommandMessage.GET_SCREEN_FULL_IMAGE)
                t = new Thread(new ParameterizedThreadStart(sendScreenFullImage));
            else if (command == Conf.NetCommandMessage.GET_SCREEN_THUMB_IMAGE)
                t = new Thread(new ParameterizedThreadStart(sendScreenThumbImage));
            t.IsBackground = true;
            t.Start(ip);
        }

        
        private void sendScreenFullImage(Object ip)
        {
            Image img = Controller.ScreenCapture.GetScreenFullImage();
            if (img == null) return;
            Model.TCPServer.GetTCPServer().SendImage(ip.ToString(), Conf.Constant.TCP_SEND_PORT, img);
        }

        private void sendScreenThumbImage(Object ip)
        {
            Image img = Controller.ScreenCapture.GetScreenThumbnailImage();
            if (img == null) return;
            Model.TCPServer.GetTCPServer().SendImage(ip.ToString(), Conf.Constant.TCP_SEND_PORT, img);
        }
        #endregion


        #region Send Process List
   
        public void SendProcessList()
        {
            String xml = ProcessMonitor.Instace.GetProcessListAsXML();
            
            List<String> agent_ips = _agentIpsModel.remoteIps;
            foreach (String ip in agent_ips)
            {
                if (!_agentIpsModel[ip]) continue;
                SendMessage(ip, Conf.NetCommandMessage.GET_PROCESS_LIST, xml.ToString());
            }

        }

        public void SendProcessListXml(Object xml)
        {
            List<String> agent_ips = _agentIpsModel.remoteIps;
            foreach (String ip in agent_ips)
            {
                if (!_agentIpsModel[ip]) continue;
                SendMessage(ip, Conf.NetCommandMessage.GET_PROCESS_LIST, xml.ToString());
            }

        }

        private void SendProcessList(Object ip)
        {
            SendMessage(ip.ToString(), Conf.NetCommandMessage.GET_PROCESS_LIST, ProcessMonitor.Instace.GetProcessListAsXML());
        }

        #endregion

        #region Send Installed Application List

        private void SendInstalledApplicationList(Object ip)
        {
            SendMessage(ip.ToString(), Conf.NetCommandMessage.GET_INSTALLED_APPLICATION, Util.Util.GetInstalledAppAsXML());
        }
        #endregion

        #region Send Device Information list

        private void SendDeviceInfoList(Object ip)
        {
            SendMessage(ip.ToString(), Conf.NetCommandMessage.GET_DEVICE_INFO, Util.Util.GetDeviceInfoAsXML());
        }
        #endregion


        public void SendWorkHistory(Object ip )
        {
            SendMessage(ip.ToString(), Conf.NetCommandMessage.GET_INSTALLED_DATE, HistoryController.Instance.GetSystemInstalledDate());
        }

        #region Send Usb Log        

        private void SendUsbLog(Object ip)
        {
            SendMessage(ip.ToString(), Conf.NetCommandMessage.LOG_USB, HistoryController.Instance.GetLocalUsbLog());
        }

        public void SendUsbLogXmlToAgent(Object xml)
        {
            
            List<String> agent_ips = _agentIpsModel.remoteIps;
            foreach (String ip in agent_ips)
            {
                if (!_agentIpsModel[ip]) continue;
                SendMessage(ip, Conf.NetCommandMessage.DETECT_USB, xml.ToString());
            }

        }

        private void SendUsbState()
        {
            Thread.Sleep(2000);
           
            List<String> agent_ips = _agentIpsModel.remoteIps;
            foreach (String ip in agent_ips)
            {
                if (!_agentIpsModel[ip]) continue;
                Model.UDPServer.BroadCastingMSG(ip.ToString(), Conf.NetCommandMessage.CMD_USBON_REQ, (!USBController.IsBlocked()).ToString());
            }
           // Model.UDPServer.BroadCastingMSG(ip.ToString(), Conf.NetCommandMessage.CMD_USBON_REQ, (!USBController.IsBlocked()).ToString());
            //SendMessage(ip.ToString(), Conf.NetCommandMessage.CMD_USBON_REQ, Util.Util.GetInstalledAppAsXML());
        }
        #endregion

        #region history-related
        public void SendHistoryToServerThread()
        {
        }
        #endregion

        #region Tcp communication

        public void SendMessage(String ip, int message, String content)
        {
            Model.TCPServer.GetTCPServer().SendMessage(ip, Conf.Constant.TCP_SEND_PORT, message, content);
        }

        public void SendMessage(String ip, int message, byte[] buffer)
        {
            Model.TCPServer.GetTCPServer().SendMessage(ip, Conf.Constant.TCP_SEND_PORT, message, buffer);
        }
        #endregion

        #region Network Adapter Changed
        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {

            TryConnectRemoteHostThread();

         

         
            //TryConnectRemoteHostThread();
           
           
            //Thread th = new Thread(new ThreadStart(NetworkEnableThread));
            //th.IsBackground = true;
            //th.Start();
            
        }

        

        private void NetworkEnableThread()
        {
            Thread.Sleep(60000);
            try
            {
                SelectQuery query = new SelectQuery("Select * from Win32_NetworkAdapter Where NetEnabled=False");

                // Initialize an object searcher with this query
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                // Get the resulting collection and loop through it
                foreach (ManagementObject envVar in searcher.Get())
                {
                    try
                    {
                        envVar.InvokeMethod("Enable", null);
                    }
                    catch (System.Exception ex)
                    {
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        
        #endregion


        #region Remote IP related

        public bool isValidRemoteIp(String ip)
        {
            return _agentIpsModel.IsValidIp(ip) || _serverIpsModel.IsValidIp(ip);
        }

        //get server ip for sending history data
        public String ServerIP()
        {
            return _serverIpsModel.mainIp;
        }

        //
        public void ProcessDisconnectedSocket(String ip)
        {
            if (_agentIpsModel.IsValidIp(ip))
                _agentIpsModel[ip] = false;
            else if (_serverIpsModel.IsValidIp(ip))
                _serverIpsModel[ip] = false;
        }

        #endregion

        #region message-related

        public void SendMsg2Agent(String text)
        {
            Thread.Sleep(2000);

            List<String> agent_ips = _agentIpsModel.remoteIps;
            foreach (String ip in agent_ips)
            {
                if (!_agentIpsModel[ip]) continue;
                Model.UDPServer.BroadCastingMSG(ip.ToString(), Conf.NetCommandMessage.SEND_MESSAGE_TEXT, text);
            }
          
        }
        #endregion
    }
}
