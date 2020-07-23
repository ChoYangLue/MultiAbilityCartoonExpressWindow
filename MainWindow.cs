using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiAbilityCartoonExpressWindow
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.Width = Properties.Settings.Default.WindowHeightSetting;
            this.Height = Properties.Settings.Default.WindowWidthSetting;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.WindowHeightSetting = this.Width;
            Properties.Settings.Default.WindowWidthSetting = this.Height;
            Properties.Settings.Default.Save();
        }


    }
}
