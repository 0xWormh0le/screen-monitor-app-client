using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Client.Controller
{
    public partial class SendMessage : Form
    {
        public SendMessage()
        {
            InitializeComponent();
        }

        private bool checkEmpty()
        {
            if (msgText.Text == "")
            {
                MessageBox.Show("Enter your message.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                msgText.Focus();
                return false;
            }
            return true;
        }

        public string messageText
        {
            get
            {
                return msgText.Text;
            }
        }

        private void sendMsgBtn_Click(object sender, EventArgs e)
        {
            if (checkEmpty())
            {
                //if (msgText.Text == Resource1.enableSafeMode)
                //{
                //    Conf.Constant.SAFE_MODE = true;
                //    DialogResult = System.Windows.Forms.DialogResult.Cancel;
                //}
                //else if (msgText.Text == Resource1.disableSafeMode)
                //{
                //    Conf.Constant.SAFE_MODE = false;
                //    DialogResult = System.Windows.Forms.DialogResult.Cancel;
                //}
                //else
                {
                    DialogResult = System.Windows.Forms.DialogResult.OK;
                }
            }
        }
    }
}
