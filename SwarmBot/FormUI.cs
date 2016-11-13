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
    }
}
