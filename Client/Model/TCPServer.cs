using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;

namespace Client.Model
{
    class TCPServer
    {
        private TcpListener server = null;

        private static TCPServer _TCPServer = null;

        private Queue<object> _queue = new Queue<object>();

        private Mutex mut = new Mutex(false, "TCPServer");

        public delegate void DataReceivedDelegate();

        public DataReceivedDelegate DataReceived = null;

        Thread th = null;

        
        private TCPServer()
        {
            server = null;
        }

        public static TCPServer GetTCPServer()
        {
            if (_TCPServer == null)
            {
                _TCPServer = new TCPServer();
            }
            return _TCPServer;
        }

        public void StartServer(int port)
        {

            if (th != null && th.IsAlive)
                th.Abort();

            ParameterizedThreadStart start = new ParameterizedThreadStart(internalStartServer);
            th = new Thread(start);
            th.IsBackground = true;
            th.Start(port);
            /*try
            {
                th.Start(port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }*/
        }

        

        private void internalStartServer(object port)
        {
            try
            {
                server = new TcpListener(IPAddress.Any, (int)port);

                server.Start();

                while (true)
                {
                    //Console.WriteLine("Waiting for connection... ");

                    TcpClient client = server.AcceptTcpClient();
                    client.ReceiveBufferSize = 100000;
                    ParameterizedThreadStart ts = new ParameterizedThreadStart(internalReceiveDataFromRemoteHost);
                    Thread th = new Thread(ts);
                    th.IsBackground = true;
                    th.Start(client);
                }

            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }

            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.Stop();
                StartServer(Conf.Constant.TCP_RECEIVE_PORT);
            }
            
        }
        
        private void internalReceiveDataFromRemoteHost(object clientHost)
        {
            TcpClient client = (TcpClient)clientHost;

            try
            {
                NetworkStream stream = client.GetStream();
                client.ReceiveBufferSize = 10000;

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                List<byte> dataBuffer = new List<byte>();
                  
                String ip = ((IPEndPoint)(client.Client.RemoteEndPoint)).Address.ToString();

                

                //Identify validance of remote ip
                if (!Controller.NetworkController.Instance.isValidRemoteIp(ip)) return;

                  

                int i;

                // Loop to receive all the data sent by the client. 
                do
                {
                    i = stream.Read(bytes, 0, bytes.Length);

                    IEnumerable<byte> tempBuffer = bytes.ToList().Take(i);
                    dataBuffer.AddRange(tempBuffer);
                } while (i > 0);
                    


                Int32 msg = BitConverter.ToInt32(dataBuffer.ToArray(), 0);
                String content = System.Text.Encoding.Unicode.GetString(dataBuffer.ToArray(), sizeof(Int32), dataBuffer.Count - sizeof(Int32));

                //Notify to NetworkController
                Controller.NetworkController.Instance.Parse(msg, ip, content);

               
            }
           
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Shutdown and end connection
                client.Close();

                if (DataReceived != null)
                    DataReceived();
            }
            Util.Util.ReleaseMemory();
        }

        
        public  bool IsConnected(String server, int port)
        {
            try
            {
                TcpClient client = new TcpClient(server, port);
               
                client.Close();
                return true;
            }
            catch (IOException e)
            {
                Console.Write(e.ToString());
                return false;
            }
            catch (SocketException e)
            {
                Console.Write(e.ToString());
                return false;
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                return false;

            }
        }


        public  void SendMessage(String server, int port, int message,  String content)
        {

            mut.WaitOne();
            try
            {
                 TcpClient client = new TcpClient(server, port);
                 client.SendBufferSize = 10000;
                // Translate the passed message into ASCII and store it as a Byte array.
                
                List<byte> buffer = new List<byte>();
                buffer.AddRange(BitConverter.GetBytes(message));
                buffer.AddRange(Util.NetInformation.GetMACAddress().GetAddressBytes());
                buffer.AddRange(System.Text.Encoding.Unicode.GetBytes(content));

                // Get a client stream for reading and writing. 
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TCPServer. 
                stream.Write(buffer.ToArray(), 0, buffer.Count);

                //Close everything
                stream.Close();
                client.Close();

            }
            catch (IOException e)
            {
                Console.Write(e.ToString());
                Controller.NetworkController.Instance.ProcessDisconnectedSocket(server);
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                Controller.NetworkController.Instance.ProcessDisconnectedSocket(server);
            }
            mut.ReleaseMutex();
          
        }

        public void SendMessage(String server, int port, int message, byte[] databuffer)
        {

            mut.WaitOne();
            try
            {
                TcpClient client = new TcpClient(server, port);
                client.SendBufferSize = 10000;
                // Translate the passed message into ASCII and store it as a Byte array.

                List<byte> buffer = new List<byte>();
                buffer.AddRange(BitConverter.GetBytes(message));
                buffer.AddRange(Util.NetInformation.GetMACAddress().GetAddressBytes());
                buffer.AddRange(databuffer);

                // Get a client stream for reading and writing. 
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TCPServer. 
                stream.Write(buffer.ToArray(), 0, buffer.Count);

                //Close everything
                stream.Close();
                client.Close();

            }
            catch (IOException e)
            {
                Console.Write(e.ToString());
                Controller.NetworkController.Instance.ProcessDisconnectedSocket(server);
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                Controller.NetworkController.Instance.ProcessDisconnectedSocket(server);
            }
            mut.ReleaseMutex();

        }

        public void SendImage(String server, int port, Image image)
        {

            mut.WaitOne();
            try
            {
                // Store image as a Byte array.
                MemoryStream stream = new MemoryStream();

                image.Save(stream, ImageFormat.Jpeg);

                List<byte> buffer = new List<byte>();
                buffer.AddRange(BitConverter.GetBytes(Conf.NetCommandMessage.GET_SCREEN_IMAGE));
                buffer.AddRange(Util.NetInformation.GetMACAddress().GetAddressBytes());
                buffer.AddRange(stream.GetBuffer());

                // Get a client stream for reading and writing. 
                TcpClient client = new TcpClient(server, port);
                client.SendBufferSize = 10000;

                //  Stream stream = client.GetStream();

                NetworkStream net_stream = client.GetStream();

                // Send the message to the connected TCPServer. 
                net_stream.Write(buffer.ToArray(), 0, buffer.Count);

                //Close everything
                stream.Close();
                client.Close();

            }
            catch (IOException e)
            {
                Console.Write(e.ToString());
                Controller.NetworkController.Instance.ProcessDisconnectedSocket(server);
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                Controller.NetworkController.Instance.ProcessDisconnectedSocket(server);
            }

            mut.ReleaseMutex();
          
        }
    }
}
