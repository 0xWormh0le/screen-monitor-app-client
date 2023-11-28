using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Client.Model
{
    public class XmlDataSerializer
    {
       

        public static void Serializer(string filePath,UsbLog log)            
        {
            FileStream fs = null;
            TextWriter tw = null;           

            try
            {
               
                UsbLogList list = XmlDataSerializer.Deserializer(filePath);

                if (list == null)
                    list = new UsbLogList();

                if (log.EjectTime == string.Empty)
                    list.Add(log);
                else
                {
                    for (int i = 0; i < list.logs.Length; i++)
                    {
                        if (list.logs[i].id == log.id)
                        {
                            list.logs[i].EjectTime = log.EjectTime;
                            break;
                        }
                    }
                }
                
                XmlSerializer writer = new XmlSerializer(typeof(UsbLogList));
                fs = new FileStream(@"c:\temp.xml", FileMode.OpenOrCreate);
                tw = new StreamWriter(fs, Encoding.UTF8);
                
                writer.Serialize(tw, list);

                tw.Close();

                tw = null;

                byte[] bytes = File.ReadAllBytes(@"c:\temp.xml");
                string str=Convert.ToBase64String(bytes);
                File.WriteAllText(filePath, str);
                File.Delete(@"c:\temp.xml");

                Thread.Sleep(100);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (tw != null)
                {
                    tw.Close();
                    tw = null;
                }               
            }            
        }

        public static UsbLogList Deserializer(string filePath)
        {
            FileStream fs = null;
            TextReader tr = null;
            UsbLogList list = new UsbLogList();

            if (!File.Exists(filePath))
            {
                    return null;
            }

            try
            {
                byte[] bytes=Convert.FromBase64String(File.ReadAllText(filePath));
                File.WriteAllBytes(@"c:\temp.xml", bytes);
                XmlSerializer reader = new XmlSerializer(typeof(UsbLogList));
                fs = new FileStream(@"c:\temp.xml", FileMode.OpenOrCreate);
                tr = new StreamReader(fs, Encoding.UTF8);
                
                list = (UsbLogList)reader.Deserialize(tr);
                tr.Close();
                tr = null;
                File.Delete(@"c:\temp.xml");
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.ToString());               
                if (tr != null) tr.Close();
                tr = null;                
            }

           
            return list;
        }
    }
}
