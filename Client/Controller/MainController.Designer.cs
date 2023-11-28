namespace Client.Controller
{
    partial class MainController
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainController));
            this.file_timer = new System.Windows.Forms.Timer(this.components);
            this.view_timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // file_timer
            // 
            this.file_timer.Enabled = true;
            this.file_timer.Interval = 30000;
            this.file_timer.Tick += new System.EventHandler(this.file_timer_Tick);
            // 
            // view_timer
            // 
            this.view_timer.Enabled = true;
            this.view_timer.Tick += new System.EventHandler(this.view_timer_Tick);
            // 
            // MainController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(57, 48);
            this.Cursor = System.Windows.Forms.Cursors.NoMove2D;
            this.ForeColor = System.Drawing.Color.CornflowerBlue;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainController";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "I am your friend...";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Fuchsia;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainController_FormClosing);
            this.Load += new System.EventHandler(this.MainController_Load);
            this.DoubleClick += new System.EventHandler(this.MainController_DoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainController_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainController_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MainController_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer file_timer;
        private System.Windows.Forms.Timer view_timer;

    }
}

