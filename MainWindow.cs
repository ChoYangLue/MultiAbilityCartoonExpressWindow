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
        public struct MediaInfo
        {
            public string path;
            public Image img_obj;
            public float raito;
            public bool is_portrait;
        }

        MediaInfo[] mediaInfoList = new MediaInfo[4];
        int mediaCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }


        private string getFileNameToDragEvent(DragEventArgs e)
        {
            string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (System.IO.File.Exists(fileName[0]) == true) return fileName[0];
            return null;
        }

        private void addMediaInfoList(string fileName)
        {
            if (mediaCount > 4) return;
            Image img = Image.FromFile(fileName);
            MediaInfo medinfo = new MediaInfo();
            medinfo.path = fileName;
            medinfo.img_obj = img;
            medinfo.raito = (float)img.Width / (float)img.Height;
            if (medinfo.raito < 1) medinfo.is_portrait = true;
            mediaInfoList[0] = medinfo;

            mediaCount = 1;
        }

        private void updatePictureBox()
        {
            if (mediaCount == 0) return;

            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);

            if (mediaInfoList[0].is_portrait)
            {
                if (pictureBox1.Height > mediaInfoList[0].img_obj.Height)
                {
                    g.DrawImage(mediaInfoList[0].img_obj, 0, 0, (int)mediaInfoList[0].img_obj.Height * mediaInfoList[0].raito, mediaInfoList[0].img_obj.Height);
                }
                else
                {
                    g.DrawImage(mediaInfoList[0].img_obj, 0, 0, (int)pictureBox1.Height * mediaInfoList[0].raito, pictureBox1.Height);
                }
            }
            else
            {
                if (pictureBox1.Width > mediaInfoList[0].img_obj.Width)
                {
                    g.DrawImage(mediaInfoList[0].img_obj, 0, 0, mediaInfoList[0].img_obj.Width, (int)mediaInfoList[0].img_obj.Width / mediaInfoList[0].raito);
                }
                else
                {
                    g.DrawImage(mediaInfoList[0].img_obj, 0, 0, pictureBox1.Width, (int)pictureBox1.Width / mediaInfoList[0].raito);
                }
                
            }

            //Graphicsオブジェクトのリソースを解放する
            g.Dispose();
            //pictureBox1に表示する
            pictureBox1.Image = canvas;
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
        
        /* window操作関連 */

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

        /* D&D関連 */

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            string fileName = this.getFileNameToDragEvent(e);
            Console.WriteLine(fileName);

            addMediaInfoList(fileName);
            updatePictureBox();

        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.All;
            else e.Effect = DragDropEffects.None;

        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            Control c = (Control)sender;
            //Console.WriteLine("フォームのサイズが{0}x{1}に変更されました", c.Width, c.Height);
        }

        private void MainWindow_ResizeEnd(object sender, EventArgs e)
        {
            Control c = (Control)sender;
            Console.WriteLine("フォームのサイズが{0}x{1}に変更されました", c.Width, c.Height);
            updatePictureBox();
        }
    }
}
