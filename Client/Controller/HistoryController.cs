using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.AccessControl;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Threading;

namespace Client.Controller
{
    class HistoryController
    {

        static HistoryController _instance = null;
        private Mutex mut = new Mutex(false, "History");

        public static HistoryController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HistoryController();
                    SetWorkHistory();
                }
                return _instance;
            }
        }


        HistoryController()
        {
           
        }
        public void SaveImageHistory(byte[] data)
        {
            //if server is alive, save history to server , or save history to local.

                if (Controller.NetworkController.Instance.ServerIP() != "")
                    saveToServer(Conf.Constant.HISTORY_IMAGE_NAME, data);
          //      else
          //          saveToLocal(Conf.Constant.HISTORY_IMAGE_NAME, data);
        }

        public void SaveUsbHistory(Model.UsbLog log)
        {

            if (Controller.NetworkController.Instance.ServerIP() != "")
                saveToServer(log);
            else
                saveToLocal(log);

            
        }
        public string GetSystemInstalledDate()
        {
            String file_name = Conf.Constant.HISTORY_ROOT + @"\" + Conf.Constant.HISTORY_WORK_NAME + Conf.Constant.HISTORY_FILE_EXT;
            if (!File.Exists(file_name))
            {
                DateTime date = DateTime.Now;
                return date.Year + "-" + date.ToString("MM") + "-" + date.ToString("dd");
            }

            DateTime current = DateTime.Now;
            DateTimeOffset datetimeOffset = new DateTimeOffset(current);
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(File.ReadAllText(file_name));
                XmlNodeList nodeList = doc.DocumentElement.SelectNodes("work");
                int cnt = nodeList.Count;
                if (cnt == 0)
                {
                    DateTime date = DateTime.Now;
                    return date.Year + "-" + date.ToString("MM") + "-" + date.ToString("dd");
                }

               
                datetimeOffset = datetimeOffset.AddDays(cnt * -1);
            }
            catch (Exception e)
            {
               
            }

          
            return datetimeOffset.Year + "-" + datetimeOffset.ToString("MM") + "-" + datetimeOffset.ToString("dd");

        }
        public  static void  SetWorkHistory()
        {
            DateTime current = DateTime.Now;
            String dateStr = current.ToString("yy_MM_dd");
            String file_name = Conf.Constant.HISTORY_ROOT + @"\" + Conf.Constant.HISTORY_WORK_NAME + Conf.Constant.HISTORY_FILE_EXT;

            try
            {
                if (!File.Exists(file_name))
                {
                   // File.Create(file_name);
                    File.WriteAllText(file_name, "<?xml version=\"1.0\" encoding=\"utf-8\"?><root></root>");
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(file_name);
                
                XmlNode node = doc.DocumentElement.SelectSingleNode("work[.=\"" + dateStr + "\"]");
                if (node != null) return;

                XmlElement newNode = doc.CreateElement("work");
                newNode.InnerText = dateStr;
                doc.DocumentElement.AppendChild(newNode);
                doc.Save(file_name);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #region Save Image history
        private void saveToLocal(String history_name, byte[] data)
        {
            //get file count
            DateTime current = DateTime.Now;
            String directory_name = Conf.Constant.HISTORY_ROOT + @"\" + history_name;
            try
            {
                Directory.CreateDirectory(directory_name);
                String file_suffix = current.ToString("MM_dd_HH_mm_ss");//current.Month.ToString() + @"_" + current.Day.ToString() + "_" + current.Hour + "_" + current.Minute + "_" + current.Second ;

                String file_name = Conf.Constant.HISTORY_IMAGE_NAME + @"#" + file_suffix + Conf.Constant.HISTORY_FILE_EXT;
                FileStream filestream =  File.Create(directory_name + @"\" + file_name, data.Length, FileOptions.Encrypted);
                StreamWriter streamwriter = new StreamWriter(filestream);
                    
                //encrypt data
                String encrypted_string = Convert.ToBase64String(data);

                streamwriter.Write(encrypted_string);
                streamwriter.Close();


                //streamwriter.WriteAsync(encrypted_string);
                
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }
        }

       

        private void saveToServer(String history_name, byte[] data)
        {


            String date = DateTime.Now.ToString("MM/dd/yy HH:mm:ss");
            
            List<byte> buffer = new List<byte>();
            buffer.AddRange(System.Text.Encoding.Unicode.GetBytes(date));
            buffer.AddRange(data);

            String server_ip = Controller.NetworkController.Instance.ServerIP();
            if (server_ip != "")
            {
                NetworkController.Instance.SendMessage(server_ip, Conf.NetCommandMessage.GET_SCREEN_IMAGE_STORE, buffer.ToArray());
            }
        }
        
        public void SendLocalImageLogToServer(Object server_ipobj)
        {
            String directory_name = Conf.Constant.HISTORY_ROOT + @"\" + Conf.Constant.HISTORY_IMAGE_NAME;
            String[] files = Directory.GetFiles(directory_name);

            String server_ip = server_ipobj.ToString();
            if (server_ip == "") return;

            mut.WaitOne();
            foreach (String file in files)
            {
                //String file_name = Path.GetFileNameWithoutExtension(file);
                String[] spiltString = Path.GetFileNameWithoutExtension(file).Split('#');
                String date = "";
                try
                {
                    date = spiltString[1];
                }
                catch (Exception e)
                {
                    date = DateTime.Now.ToString("MM_dd_HH_mm_ss");
                }

                List<byte> buffer = new List<byte>();

                try
                {
                    buffer.AddRange(System.Text.Encoding.Unicode.GetBytes(date));
                    buffer.AddRange(Convert.FromBase64String(File.ReadAllText(file)));

                    NetworkController.Instance.SendMessage(server_ip, Conf.NetCommandMessage.GET_SCREEN_IMAGE_STORE, buffer.ToArray());

                    File.Delete(file);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.ToString());
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            mut.ReleaseMutex();
            NetworkController.Instance.SendMessage(server_ip, Conf.NetCommandMessage.GET_SCREEN_IMAGE_STORE, "");
        }

        
        #endregion

        #region usb related

        private void saveToLocal(Model.UsbLog log)
        {
            String old_filePath = Conf.Constant.HISTORY_ROOT + @"\" +Conf.Constant.HISTORY_USB_NAME_OLD;
            String new_filePath = Conf.Constant.HISTORY_ROOT + @"\" +Conf.Constant.HISTORY_USB_NAME_NEW;

            Model.XmlDataSerializer.Serializer(new_filePath, log);
            Model.XmlDataSerializer.Serializer(old_filePath, log);
        }


        private void saveToServer(Model.UsbLog log)
        {

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            xml += "<usblog>";
            xml += "<record>";
            xml += "<username>" + log.UserName + "</username>";
            xml += "<devname>" + log.DeviceName + "</devname>";
            xml += "<plugtime>" + log.PlugTime + "</plugtime>";
            xml += "<ejecttime>" + log.EjectTime + "</ejecttime>";
            xml += "<state>" + log.State + "</state>";
            xml += "<content>" + log.Content + "</content>";
            xml += "<id>" + log.id + "</id>";
            xml += "</record>";
            xml += "</usblog>";

            

            //send to server
            String server_ip = Controller.NetworkController.Instance.ServerIP();
            if (server_ip != "")
                NetworkController.Instance.SendMessage(server_ip, Conf.NetCommandMessage.LOG_USB, xml.ToString());


            //save to local
            String old_filePath = Conf.Constant.HISTORY_ROOT + @"\" + Conf.Constant.HISTORY_USB_NAME_OLD;
            Model.XmlDataSerializer.Serializer(old_filePath, log);

        }

        public void SendLocalUsbLogToServer(Object server_ipobj)
        {
            mut.WaitOne();
            try
            {
                    String new_filePath = Conf.Constant.HISTORY_ROOT + @"\" + Conf.Constant.HISTORY_USB_NAME_NEW;
                    String xml = getUsbLog(new_filePath);


                    String server_ip = server_ipobj.ToString();
                    if (server_ip != "")
                    {

                        NetworkController.Instance.SendMessage(server_ip, Conf.NetCommandMessage.LOG_USB, xml);
                        NetworkController.Instance.SendMessage(server_ip, Conf.NetCommandMessage.LOG_USB, "");

                        File.Delete(new_filePath);
                    }
                    
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            mut.ReleaseMutex();
        }

       
        public String GetLocalUsbLog()
        {
            String old_filePath = Conf.Constant.HISTORY_ROOT + @"\" + Conf.Constant.HISTORY_USB_NAME_OLD;
            return getUsbLog(old_filePath);
        }

        private String getUsbLog(String filepath)
        {
            Model.UsbLogList loglist = Model.XmlDataSerializer.Deserializer(filepath);

            if (loglist == null || loglist.logs == null || loglist.logs.Count() == 0) return "";

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
            xml += "<usblog>";
            foreach (Model.UsbLog log in loglist.logs)
            {
                xml += "<record>";
                xml += "<username>" + log.UserName + "</username>";
                xml += "<macaddr>" + log.MACAddress + "</macaddr>";
                xml += "<ipaddr>" + log.IPAddress + "</ipaddr>";
                xml += "<devname>" + log.DeviceName + "</devname>";
                xml += "<plugtime>" + log.PlugTime + "</plugtime>";
                xml += "<ejecttime>" + log.EjectTime + "</ejecttime>";
                xml += "<state>" + log.State + "</state>";
                xml += "<content>" + log.Content + "</content>";
                xml += "<id>" + log.id + "</id>";
                xml += "</record>";
            }
            xml += "</usblog>";
            return xml;
        }

        #endregion
    }
}
