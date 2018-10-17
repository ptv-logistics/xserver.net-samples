using System;
using System.Windows.Forms;

namespace Ptv.XServer.Demo.Resources.MessageBox
{
    public partial class MsgBox : Form
    {
        private readonly String caption;
        private readonly String message;

        public MsgBox(String caption, String message)
        {
            InitializeComponent();
            this.caption = caption;
            this.message = message;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MsgBox_Load(object sender, EventArgs e)
        {
            Text = caption;
            label1.Text = message;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "https://ae.mapandguide.de/xservershop/?lingua=en",
                    Verb = "Open",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                }
            };
            process.Start();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "http://xserver.ptvgroup.com/en-uk/products/ptv-xserver-internet/test/",
                    Verb = "Open",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                }
            };
            process.Start();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "http://xserverinternet.azurewebsites.net/xserver.net/",
                    Verb = "Open",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                }
            };
            process.Start();
        }
    }
}
