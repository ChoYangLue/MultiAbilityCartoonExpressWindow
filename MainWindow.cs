using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace MultiAbilityCartoonExpressWindow
{
    public partial class MainWindow : Form
    {
        //マウスのクリック位置を記憶
        private Point mousePoint;
        public struct MediaInfo
        {
            public string path;
            public string dir;
        }
        List<MediaInfo> mediaInfoList = new List<MediaInfo>();
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".BMP", ".GIF", ".PNG" };
        public struct OverSizeInfo
        {
            public bool over_flag;
            public bool is_width;
            public int over_pix;
        }

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
            if (mediaInfoList.Count > 4) return;
            MediaInfo medinfo = new MediaInfo();
            medinfo.path = fileName;
            medinfo.dir = System.IO.Path.GetDirectoryName(fileName);

            mediaInfoList.Add(medinfo);
        }

        private void insertMediaInfoList(string fileName, int index)
        {
            if (index > 4) return;
            if (mediaInfoList.Count() < index) return;
            
            MediaInfo medinfo = new MediaInfo();
            medinfo.path = fileName;
            medinfo.dir = System.IO.Path.GetDirectoryName(fileName);
            
            mediaInfoList[index] = medinfo;
        }

        private void updatePictureBox()
        {
            if (mediaInfoList.Count < 1) return;

            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            //ImageオブジェクトのGraphicsオブジェクトを作成する
            Graphics g = Graphics.FromImage(canvas);

            int width_index = 0;
            int height_index = 0;
            for (int i = 0; i < mediaInfoList.Count; i++)
            {
                Image img_obj = Image.FromFile(mediaInfoList[i].path);
                float raito = (float)img_obj.Width / (float)img_obj.Height;
                bool is_portrait = false;
                if (raito < 1) is_portrait = true;

                int width_tmp = img_obj.Width;
                int height_tmp = img_obj.Height;

                var info = new OverSizeInfo();
                getOverSizeInfo(img_obj.Width, img_obj.Height, ref info);

                if (info.over_flag)
                {
                    if (info.is_width)
                    {
                        width_tmp = pictureBox1.Width;
                        height_tmp = (int)(pictureBox1.Width / raito);
                    }
                    else
                    {
                        width_tmp = (int)(pictureBox1.Height * raito);
                        height_tmp = pictureBox1.Height;
                    }
                }
                g.DrawImage(img_obj, width_index, height_index, width_tmp, height_tmp);

                if (is_portrait)
                {
                    width_index += width_tmp;
                }
                else
                {
                    height_index += height_tmp;
                }

                // 残しとく
                /*
                if (is_portrait)
                {
                    if (pictureBox1.Height > img_obj.Height)
                    {
                        width_tmp = (int)(img_obj.Height * raito);
                        height_tmp = img_obj.Height;
                        //g.DrawImage(img_obj, width_index, 0, (int)img_obj.Height * raito, img_obj.Height);
                    }
                    else
                    {
                        width_tmp = (int)(pictureBox1.Height * raito);
                        height_tmp = pictureBox1.Height;
                        //g.DrawImage(img_obj, width_index, 0, (int)pictureBox1.Height * raito, pictureBox1.Height);
                    }

                    g.DrawImage(img_obj, width_index, height_index, width_tmp, height_tmp);
                    width_index += width_tmp;
                }
                else
                {
                    if (pictureBox1.Width > img_obj.Width)
                    {
                        width_tmp = img_obj.Width;
                        height_tmp = (int)(img_obj.Width / raito);
                        //g.DrawImage(img_obj, width_index, 0, img_obj.Width, (int)img_obj.Width / raito);
                    }
                    else
                    {
                        width_tmp = pictureBox1.Width;
                        height_tmp = (int)(pictureBox1.Width / raito);
                        //g.DrawImage(img_obj, width_index, 0, pictureBox1.Width, (int)pictureBox1.Width / raito);
                    }
                    g.DrawImage(img_obj, width_index, height_index, width_tmp, height_tmp);
                    height_index += height_tmp;
                }*/

                img_obj.Dispose();
            }


            //Graphicsオブジェクトのリソースを解放する
            g.Dispose();
            //pictureBox1に表示する
            pictureBox1.Image = canvas;
        }

        private string searchDir(string fileName, bool next_flag)
        {
            string dir = System.IO.Path.GetDirectoryName(fileName);
            IEnumerable<string> files = Directory.EnumerateFiles(dir, "*");

            string previus = files.ElementAt(files.Count() -1);
            int i = 0;
            foreach (string str in files)
            {
                if (ImageExtensions.Contains(Path.GetExtension(str).ToUpperInvariant()))
                {
                    if (str == fileName)
                    {
                        Console.WriteLine("[" + i + "]" + " " + str);
                        if (next_flag)
                        {
                            if (i+1 < files.Count()) return files.ElementAt(i + 1);
                            return files.ElementAt(0);
                        }
                        return previus;
                    }
                    previus = str;

                }
                i += 1;
            }

            return fileName;
        }

        private bool isWindowPortrait()
        {
            // 縦長
            if (this.Width < this.Height) return true;
            return false;
        }

        private void getOverSizeInfo(int width, int height, ref OverSizeInfo info)
        {
            int width_over_pix = width - this.Width;
            int height_over_pix = height - this.Height;

            // そもそもオーバーサイズなのか
            if (width_over_pix <= 0 && height_over_pix <= 0)
            {
                info.over_flag = false;
                return;
            }
            info.over_flag = true;
            
            // 縦横どちらがより多くオーバーしているか
            if (width_over_pix > height_over_pix)
            {
                info.is_width = true;
                info.over_pix = width_over_pix;
            }
            else
            {
                info.is_width = false;
                info.over_pix = height_over_pix;
            }
        }

        /* 設定保存 */

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.Width = Properties.Settings.Default.WindowHeightSetting;
            this.Height = Properties.Settings.Default.WindowWidthSetting;
            this.Left = Properties.Settings.Default.WindowLeftSetting;
            this.Top = Properties.Settings.Default.WindowTopSetting;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.WindowHeightSetting = this.Width;
            Properties.Settings.Default.WindowWidthSetting = this.Height;
            Properties.Settings.Default.WindowLeftSetting = this.Left;
            Properties.Settings.Default.WindowTopSetting = this.Top;
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

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space) this.WindowState = FormWindowState.Minimized;
            else if (e.KeyCode == Keys.Left)
            {
                // 前の画像
                string filename = searchDir(mediaInfoList[0].path, false);
                Console.WriteLine("pre:"+ filename);
                
                insertMediaInfoList(filename, 0);
                updatePictureBox();
            }
            else if (e.KeyCode == Keys.Right)
            {
                // 次の画像
                string filename = searchDir(mediaInfoList[0].path, true);
                Console.WriteLine("next:" + filename);

                insertMediaInfoList(filename, 0);
                updatePictureBox();
            }
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

        private void MainWindow_ResizeEnd(object sender, EventArgs e)
        {
            Control c = (Control)sender;
            Console.WriteLine("フォームのサイズが{0}x{1}に変更されました", c.Width, c.Height);
            updatePictureBox();
        }

        private void MainWindow_DoubleClick(object sender, EventArgs e)
        {
            //自分自身のフォームの状態を調べる
            switch (this.WindowState)
            {
                case FormWindowState.Normal:
                    Console.WriteLine("普通の状態です");
                    this.WindowState = FormWindowState.Maximized;
                    break;
                case FormWindowState.Minimized:
                    Console.WriteLine("最小化されています");
                    break;
                case FormWindowState.Maximized:
                    Console.WriteLine("最大化されています");
                    this.WindowState = FormWindowState.Normal;
                    break;
            }
            updatePictureBox();
        }


    }
}
