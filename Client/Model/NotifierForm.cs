using System;
using System.Windows.Forms;

namespace Client.Model
{
    public partial class NotifierForm : Form
    {
        public NotifierForm()
        {
            InitializeComponent();
        }

        public void SetText(string content)
        {
            this.textBox.Text = content;
            this.Show();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
