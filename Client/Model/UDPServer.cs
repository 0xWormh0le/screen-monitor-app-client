using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Client.Model
{
    public class UDPServer
    {


        private static Thread receiveThread = null;

        private static UdpClient listener = null;

       

        public UDPServer()
        {

        }

        #region Send Bradcast Message

        public static void BroadCastingMSG(int message, string msgContent)
        {
            BroadCastingMSG("255.255.255.255", message, msgContent);
        }

        public static void BroadCastingMSG(string ip, int message, string msgContent)
        {
         

            //Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //IPAddress broadcast = IPAddress.Parse(ip);

            List<byte> buf = new List<byte>();
            byte[] tempbuf = BitConverter.GetBytes(message);
            buf.AddRange(tempbuf);

            PhysicalAddress addr = Util.NetInformation.GetMACAddress();
            if (addr != null)
                buf.AddRange(addr.GetAddressBytes());
            else
                buf.AddRange(new byte[] { 0, 0, 0, 0, 0, 0 });

            buf.AddRange(Encoding.Unicode.GetBytes(msgContent));

            byte[] sendbuf = buf.ToArray();

           

            try
            {
                //IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), Conf.Constant.UDP_SEND_PORT);
                UdpClient client = new UdpClient();
                client.Send(sendbuf, sendbuf.Count(), ip, Conf.Constant.UDP_SEND_PORT);
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
               
            }
            
            
        }

        
        public static void BroadCastingMSG(String ip, byte[] buff)
        {
            

           // Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

           

           
            try
            {
                //IPAddress broadcast_ip = IPAddress.Parse(ip);
                //IPEndPoint ep = new IPEndPoint(broadcast_ip, Conf.Constant.UDP_SEND_PORT);
                UdpClient client = new UdpClient();
                client.Send(buff, buff.Count(), ip, Conf.Constant.UDP_SEND_PORT);
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
               
            }

        }
        #endregion


        #region Receive Broadcasting Message
        public  static void StartReceiveForBroadCasting()
        {
            if (receiveThread != null && receiveThread.IsAlive)
                receiveThread.Abort();

            ThreadStart ts = new ThreadStart(receiveForBroadCasting);

            receiveThread = new Thread(ts);

            receiveThread.IsBackground = true;
            receiveThread.Start();
            /*try
            {
                receiveThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }*/
        }

        private static void receiveForBroadCasting()
        {
            try
            {
                listener = new UdpClient(Conf.Constant.UDP_RECEIVE_PORT);
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, Conf.Constant.UDP_RECEIVE_PORT);

                while (true)
                {
                    byte[] bytes = listener.Receive(ref groupEP);

                    String ip = groupEP.Address.ToString();

                    //Identify validance of remote ip

                    if (!Controller.NetworkController.Instance.isValidRemoteIp(ip)) return;
                    
                    try
                    {
                        Int32 msg = BitConverter.ToInt32(bytes, 0);
                        String content = System.Text.Encoding.Unicode.GetString(bytes, sizeof(Int32), bytes.Length - sizeof(Int32));
                        //Notify to NetworkController
                        Controller.NetworkController.Instance.Parse(msg, ip, content);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    Util.Util.ReleaseMemory();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if(listener != null)
                    listener.Close();
                StartReceiveForBroadCasting();
            }
        }
        #endregion
        public static void EndReceive()
        {
            if (listener != null)
            {
                listener.Close();
                listener = null;
            }

        }
        

    }
}
