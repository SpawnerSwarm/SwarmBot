using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SwarmBot.UI
{
    delegate void writeLineToConsoleCallback(string text);

    public partial class FormUI : Form
    {
        public FormUI()
        {
            InitializeComponent();
        }

        private void FormUI_Load(object sender, EventArgs e)
        {
            Program.onUIReadyCallback();
            Program.logHandler += (text) => writeLineToConsole(text);
        }

        private void restartButton_Click(object sender, EventArgs e)
        {

        }

        private void debugEnableButton_Click(object sender, EventArgs e)
        {
            Program.toggleDebug();
            if(Config.debugModeActive) { debugEnableButton.Text = "Disable Debug Mode"; }
            else { debugEnableButton.Text = "Enable Debug Mode"; }
        }

        public void writeLineToConsole(string text)
        {
            if(!SwConsole.InvokeRequired)
            {
                SwConsole.Text += text + "\n";
            }
            else
            {
                writeLineToConsoleCallback d = new writeLineToConsoleCallback(writeLineToConsole);
                Invoke(d, new object[] { text });
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if(Config.debugModeActive) { debugEnableButton.PerformClick(); }
        }

        private void FormUI_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon.Visible = true;
                Hide();
            }

            else if (WindowState == FormWindowState.Normal)
            {
                notifyIcon.Visible = false;
            }
        }

        private void showMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Show();
            if(WindowState == FormWindowState.Minimized) { WindowState = FormWindowState.Normal; }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            if(Config.debugModeActive) { debugEnableButton.PerformClick(); }
            Close();
        }
    }
}
