using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

namespace Client.Controller
{
    public partial class MainController : Form
    {
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem3;

        List<Client.Controller.TaskbarNotifier> notifiers = new List<Client.Controller.TaskbarNotifier>();

        private Rectangle _rectScreen;
        bool isShowDialog = false;

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hwnd, uint msg, int wparam, int lparam);

        int _count = 0;

        private int x_orig = -1;
        private int y_orig = -1;


        public MainController()
        {
            InitializeComponent();
            InitializeTray();
            this.view_timer.Enabled = false;
            Init();
           // Util.BitmapRegion.CreateControlRegion(this, Resource1.ma_00000);

        }

        private void InitializeTray()
        {
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();

            // Initialize contextMenu1 
            this.contextMenu1.MenuItems.AddRange(
                        new System.Windows.Forms.MenuItem[] {
                        //    this.menuItem3,
                            this.menuItem2,
                            this.menuItem1
                        });

            // Initialize menuItem1 
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "&Name";
            this.menuItem1.Click += new System.EventHandler(this.MainController_DoubleClick);

            // Initialize   menuItem2 
            this.menuItem2.Index = 1;
            this.menuItem2.Text = "&Message";
            this.menuItem2.Click += new System.EventHandler(this.MainController_SendMsg);

            // Initialize menuItem3 
            this.menuItem3.Index = 2;
            this.menuItem3.Text = "Server &IP";
            this.menuItem3.Click += new System.EventHandler(this.MainController_ServerIP);

            // Set up how the form should be displayed. 
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Text = "Client";

            // Create the NotifyIcon. 
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);

            // The Icon property sets the icon that will appear 
            // in the systray for this application.

            try
            {
                notifyIcon1.Icon = Resource1.icon;
            }
            catch (Exception e) { }

            // The ContextMenu property sets the menu that will 
            // appear when the systray icon is right clicked.
            notifyIcon1.ContextMenu = this.contextMenu1;

            // The Text property sets the text that will be displayed, 
            // in a tooltip, when the mouse hovers over the systray icon.
            notifyIcon1.Text = "Client";
            notifyIcon1.Visible = true;
        }

        private void Init()
        {
            //modify const
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            Conf.Constant.FULL_IMAGE_SIZE.Width = (int)(screenSize.Width * Conf.Constant.FULL_IMAGE_SIZE_SCALE.Width);
            Conf.Constant.FULL_IMAGE_SIZE.Height = (int)(screenSize.Height * Conf.Constant.FULL_IMAGE_SIZE_SCALE.Height);

            Conf.Constant.Thumb_IMAGE_SIZE.Width = (int)(screenSize.Width * Conf.Constant.Thumb_IMAGE_SIZE_SCALE.Width);
            Conf.Constant.Thumb_IMAGE_SIZE.Height = (int)(screenSize.Height * Conf.Constant.Thumb_IMAGE_SIZE_SCALE.Height);



            // Usb related
            USBController.StartProtectSystem();
            USBDetector usbDetector = new USBDetector();
            usbDetector.DeviceDetected += new USBDetector.USBDeviceEventHandler(detector_DeviceDetected);
            usbDetector.Start();

            FirewallMonitor.FireWallMonitorThread();


            NetworkController.Instance.Start();

            ScreenCapture.StartHistoryTimer();

            ProcessMonitor.Instace.NewProcessDetect += new ProcessMonitor.NewProcessDetectDelegate(NewProcessDetect);
            ProcessMonitor.Instace.StartMonitoring();



            SafeModeDisable.AddSafeModeBoot();

            //Message
            NetworkController.Instance._ShowMessage += new NetworkController.ShowMessageDelegate(InvokeShowMessage);
            this.Hide();

            Thread th = new Thread(new ThreadStart(Watcher));
            th.IsBackground = true;
            th.Start();

        }

        private void Watcher()
        {
            while (true)
            {
                if (Process.GetProcessesByName("watson").Length == 0)
                {
                    //Util.Util.Restart();
                }
                Thread.Sleep(500);
            }
        }

        #region delegate
        private void NewProcessDetect(String xml)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(NetworkController.Instance.SendProcessListXml));
            thread.Start(xml);
        }

        private void detector_DeviceDetected(object sender, Controller.USBDetector.USBDeviceEventArgs e)
        {

            Model.UsbLog log = new Model.UsbLog();
            log.UserName = Util.Util.GetUserName();
            log.DeviceName = e.DeviceName;
            log.PlugTime = e.PlugTime;
            log.EjectTime = e.EjectTime;
            log.State = e.State;
            log.Content = e.Content;
            log.id = e.id;

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

            //send to agent
            NetworkController.Instance.SendUsbLogXmlToAgent(xml);

            //save history
            HistoryController.Instance.SaveUsbHistory(log);

        }


        private void InvokeShowMessage(String content)
        {
            this.Invoke(new NetworkController.ShowMessageDelegate(ShowMessage), content);
        }
        private void ShowMessage(String content)
        {
            Client.Controller.TaskbarNotifier notifier = new Client.Controller.TaskbarNotifier(content);
            notifier.ContentClick += new Client.Model.ContentClickEventHandler(taskbarNotifier_ContentClick);
            notifiers.Add(notifier);
            RefreshNotifiers();

        }

        private void RefreshNotifiers()
        {
            for (int i = notifiers.Count - 1; i >= 0; i--)
            {
                notifiers[i].position = notifiers.Count - 1 - i;
                if (i != notifiers.Count - 1)
                {
                    notifiers[i].PushNotifier();
                }
                else
                {
                    notifiers[i].ShowNotifier();
                }
            }
        }

        private void RefreshNotifiers(int index)
        {
            for (int i = 0; i < index; i++)
            {
                notifiers[i].FallNotifier();
            }
        }

        public void taskbarNotifier_ContentClick(object sender, Client.Model.ContentClickEventArgs e)
        {
            Client.Controller.TaskbarNotifier notifier = (Client.Controller.TaskbarNotifier)sender;
            notifier.Hide();
            int index = notifiers.IndexOf(notifier);
            notifiers.Remove(notifier);
            RefreshNotifiers(index);
            Client.Model.NotifierForm frmNotifier = new Client.Model.NotifierForm();
            frmNotifier.SetText(e.ContentObject.ToString());
        }

        #endregion

        private void file_timer_Tick(object sender, EventArgs e)
        {
            file_timer.Enabled = false;
            //delete file
            FileManager.Instance.ProcessFiles("config.xml");
        }


        #region view-related
        private void MainController_Load(object sender, EventArgs e)
        {
            _rectScreen = Screen.PrimaryScreen.Bounds;
            this.SetDesktopLocation((_rectScreen.Width - this.Width) / 2, 0);
            this.TopLevel = true;
            this.TopMost = true;
            this.Hide();
        }


        private void MainController_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point desk_location = this.DesktopLocation;
                desk_location.X = desk_location.X + (e.X - x_orig);
                desk_location.Y = desk_location.Y + (e.Y - y_orig);
                this.SetDesktopLocation(desk_location.X, desk_location.Y);
            }
        }

        private void MainController_MouseDown(object sender, MouseEventArgs e)
        {
            this.TopLevel = true;
            this.TopMost = true;

            x_orig = e.X;
            y_orig = e.Y;
        }
        private long makeLong(int a, int b)
        {
            long val = ((long)(((ushort)(((ulong)(a)) & 0xffff)) | ((ulong)((ushort)(((ulong)(b)) & 0xffff))) << 16));
            return val;
        }


        private void view_timer_Tick(object sender, EventArgs e)
        {
            return;
            this.Show();

            if (!isShowDialog)
            {
                //this.TopLevel = true;
                //this.TopMost = true;
            }

            _count++;
            String img_name = "ma_" + _count.ToString("00000");return;
            Util.BitmapRegion.CreateControlRegion(this, (Bitmap)Resource1.ResourceManager.GetObject(img_name));
            if (_count == 47)
                _count = -1;
        }

        private void MainController_DoubleClick(object sender, EventArgs e)
        {
            if (isShowDialog) return;
            isShowDialog = true;
            (new EditUser()).ShowDialog(this);
            isShowDialog = false;
        }

        private void MainController_SendMsg(object sender, EventArgs e)
        {
            if (isShowDialog) return;
            isShowDialog = true;
            SendMessage messageForm = new SendMessage();
            if (messageForm.ShowDialog(this) == DialogResult.OK)
            {
                Controller.NetworkController.Instance.SendMsg2Agent(messageForm.messageText);
                MessageBox.Show("Successfully sent.", "Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            isShowDialog = false;
        }

        private void MainController_ServerIP(object sender, EventArgs e)
        {
            if (isShowDialog) return;
            isShowDialog = true;
            AgentIP agentIP = new AgentIP();
            if (agentIP.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                NetworkController.Instance.LoadAgentIP();
            }
            isShowDialog = false;
        }

        private void MainController_MouseUp(object sender, MouseEventArgs e)
        {
            Point point = this.DesktopLocation;
            int x_desk_loc = point.X;
            int y_desk_loc = point.Y;

            if ((point.X + this.Width) > _rectScreen.Width)
                x_desk_loc = _rectScreen.Width - this.Width;
            if ((point.Y + this.Height) > _rectScreen.Height)
                y_desk_loc = _rectScreen.Height - this.Height;

            if (point.X < 0)
                x_desk_loc = 0;
            if (point.Y < 0)
                y_desk_loc = 0;

            this.SetDesktopLocation(x_desk_loc, y_desk_loc);
        }
        #endregion

        private void MainController_FormClosing(object sender, FormClosingEventArgs e)
        {
            Util.Util.Restart();
        }
    }
}
