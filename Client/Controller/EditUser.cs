using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Client
{
    public partial class EditUser : Form
    {
        public EditUser()
        {
            InitializeComponent();
            initTextControl();
        }

        private void initTextControl()
        {
            try
            {
                RegistryKey rootkey = Registry.LocalMachine;
                RegistryKey regUserKey = rootkey.CreateSubKey(@"Software\AgentUser");

                string userName = (string)regUserKey.GetValue(@"User");

                this.tbx_username.Text = userName;
            }
            catch (System.Exception ex)
            {

            }

        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            string userName = this.tbx_username.Text;
            if (string.IsNullOrEmpty(userName.Trim()))
            {
                MessageBox.Show("Enter your name correctly.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                RegistryKey rootkey = Registry.LocalMachine;
                RegistryKey regUserKey = rootkey.CreateSubKey(@"Software\AgentUser");
                regUserKey.SetValue(@"User", userName, RegistryValueKind.String);
                Client.Controller.NetworkController.Instance.TryConnectRemoteHostThread();
            }
            catch (System.Exception ex)
            {
            }

            this.Close();
        }
    }
}
