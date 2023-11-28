using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Client.Controller
{
    public partial class AgentIP : Form
    {
        public AgentIP()
        {
            InitializeComponent();
            RegistryKey rootkey = Registry.LocalMachine;
            RegistryKey regUserKey = rootkey.CreateSubKey(@"Software\AgentUser");
            string ip = (string)regUserKey.GetValue(@"IP", "none");

            if (ip != "none")
            {
                string[] segments = ip.Split(new char[] { '.' });
                txtIp.Text = String.Format("{0:D3}.{1:D3}.{2:D3}.{3:D3}", byte.Parse(segments[0]), byte.Parse(segments[1]), byte.Parse(segments[2]), byte.Parse(segments[3]));
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string ip = null;
            try
            {
                string[] segments = txtIp.Text.Split(new char[] { '.' });

                foreach (string seg in segments)
                {
                    int s = int.Parse(seg);
                    if (s < 0 || s > 255 || seg.Length < 3)
                    {
                        MessageBox.Show("Invalid IP", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        DialogResult = System.Windows.Forms.DialogResult.None;
                        return;
                    }
                    ip += s.ToString() + ".";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid IP", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RegistryKey rootkey = Registry.LocalMachine;
            RegistryKey regUserKey = rootkey.CreateSubKey(@"Software\AgentUser");
            regUserKey.SetValue(@"IP", ip.Substring(0, ip.Length - 1), RegistryValueKind.String);

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
