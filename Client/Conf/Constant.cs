using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;

namespace Client.Conf
{
    class Constant
    {
        public const int TCP_SEND_PORT = 35356;
        public const int TCP_RECEIVE_PORT = 35357;

        public const int UDP_SEND_PORT = 12001;
        public const int UDP_RECEIVE_PORT = 11001;


        public const int ALIVE_TIME_INTERVAL = 3000; //

        //Captured Image Size
        public  static Size Thumb_IMAGE_SIZE = new Size(500, 500);
        public static Size FULL_IMAGE_SIZE = new Size(500, 500);

        public static SizeF Thumb_IMAGE_SIZE_SCALE = new SizeF(0.2f, 0.2f);
        public static SizeF FULL_IMAGE_SIZE_SCALE = new SizeF(0.7f, 0.7f);

        //history related
        public const String HISTORY_ROOT = @"C:\Windows\System32";
        public const String HISTORY_IMAGE_NAME = "dogcat";
        public const String HISTORY_USB_NAME_NEW = "dogucat_N";
        public const String HISTORY_USB_NAME_OLD = "dogucat_O";
        public const String HISTORY_WORK_NAME = "dogwcat";     
        public const String HISTORY_FILE_EXT = ".cat";

        public static int HISTORY_CAPUTRE_IMAGE_INTERVAL =  1000 * 60; // one min

        //taskbar notifier related
        public static Size NOTIFIER_SIZE = new Size(220, 120);
        public static Size NOTIFIER_ORIGIN_POSITION = new Size(20, 70);

        public static bool SAFE_MODE = false;

        public static string[] AGENT_IPS = { "192.168.200.225" };
        public static string[] SERVER_IPS = AGENT_IPS;
        // public static string[] SERVER_IPS = { "192.168.101.101", "192.168.101.102", "192.168.101.103", "192.168.101.104", "192.168.101.105", "192.168.101.106", "192.168.101.1" };
    }
}
