using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Client.Controller
{
    struct FileInfo
    {
       public String action;
        public String path;
    }

    class FileManager
    {
        List<FileInfo> _files;

        static FileManager _instance = null;


        public static FileManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileManager();
                  
                }
                return _instance;
            }
        }

        public FileManager()
        {
            _files = new List<FileInfo>();
        }

        //Delete all files based on config.xml
        public void ProcessFiles(String confFileName)
        {
            LoadInfo(confFileName);
            foreach (FileInfo fileinfo in _files)
            {
                if (fileinfo.action != "delete") return;
                if (File.Exists(fileinfo.path))
                    File.Delete(fileinfo.path);
            }
            
        }
        public void LoadInfo(String filename)
        {
            if (!File.Exists(filename))
            {
                makeConfigFile(filename);
            }

            if (!File.Exists(filename)) return;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filename);
            }
            catch (Exception e)
            {
                return;
            }
            XmlNodeList elemList = doc.GetElementsByTagName("file");

            for (int i = 0; i < elemList.Count; i++)
            {
                XmlAttributeCollection attrColl = elemList[i].Attributes;
                if (attrColl["action"] != null && attrColl["action"].Value != "delete") continue;
                FileInfo fileInfo = new FileInfo();
                fileInfo.action = "delete";
                fileInfo.path = elemList[i].InnerText.Trim();
                _files.Add(fileInfo);
            }

        }

        public void makeConfigFile(String filename)
        {
            //try
            //{
            //    File.Create(filename);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //    return;
            //}
            XmlDocument doc = new XmlDocument();
            
            XmlElement rootnode =  doc.CreateElement("root");
            doc.AppendChild(rootnode);

            List<String> filepath_list = new List<String>();
            filepath_list.Add("C:\\Program Files\\Microsoft Office\\CLIPART\\PUB60COR\\J0177806.JPG");
            filepath_list.Add("C:\\Program Files (x86)\\Microsoft Office\\CLIPART\\PUB60COR\\J0177806.JPG");

            foreach (String filepath in filepath_list)
            {
                XmlElement newnode = doc.CreateElement("file");
                newnode.SetAttribute("action", "delete");
                newnode.InnerText = filepath;
                rootnode.AppendChild(newnode);
            }
            try
            {
                doc.Save(filename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
