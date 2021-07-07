using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gemsofwar.window
{
    public partial class InfoPanel : UserControl
    {
        public InfoPanel()
        {
            InitializeComponent();
        }

        public void SetText(string[] text)
        {
            InfoTextBox.Lines = text;
        }
    }
}
