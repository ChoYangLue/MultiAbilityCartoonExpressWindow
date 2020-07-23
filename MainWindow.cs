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
        //マウスのクリック位置を記憶
        private Point mousePoint;

        public MainWindow()
        {
            InitializeComponent();
        }

        /* 設定保存 */

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


        private void MainWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);
            }
            else if ((e.Button & MouseButtons.Middle) == MouseButtons.Middle)
            {
                this.Close();
                Application.Exit();
            }
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
                //または、つぎのようにする
                //this.Location = new Point(
                //    this.Location.X + e.X - mousePoint.X,
                //    this.Location.Y + e.Y - mousePoint.Y);
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space) this.WindowState = FormWindowState.Minimized;
        }

    }
}
