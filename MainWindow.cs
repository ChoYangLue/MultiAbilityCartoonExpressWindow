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
            public int now_width;
            public int now_height;
            public int now_left;
            public int now_top;
        }
        List<MediaInfo> mediaInfoList = new List<MediaInfo>();
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPEG", ".BMP", ".GIF", ".PNG" };
        public struct OverSizeInfo
        {
            public bool over_flag;
            public bool is_width;
            public int over_pix;
        }
        public int image_forcus;

        public MainWindow()
        {
            InitializeComponent();
            image_forcus = 0;
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
            medinfo.now_width = 0;
            medinfo.now_height = 0;
            medinfo.now_top = 0;
            medinfo.now_left = 0;

            mediaInfoList.Add(medinfo);

            Console.WriteLine("add :"+fileName);
        }

        private void insertMediaInfoList(string fileName, int index)
        {
            if (index > 4) return;
            if (mediaInfoList.Count() < index) return;
            
            MediaInfo medinfo = new MediaInfo();
            medinfo.path = fileName;
            medinfo.dir = System.IO.Path.GetDirectoryName(fileName);
            medinfo.now_width = 0;
            medinfo.now_height = 0;
            medinfo.now_top = 0;
            medinfo.now_left = 0;

            mediaInfoList[index] = medinfo;
        }

        private void removeMediaInfoList(int index)
        {
            if (index < 0 || index >= 4) return;

            mediaInfoList.RemoveAt(index);
            Console.WriteLine("remove: "+index.ToString());
        }

        private void updatePictureBox()
        {
            //描画先とするImageオブジェクトを作成する
            Bitmap canvas = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            Graphics g = Graphics.FromImage(canvas);
            //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
            //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            if (mediaInfoList.Count <= 0)
            {
                // 要素がない場合は元の画像を削除してから処理を抜ける
                g.Dispose();

                pictureBox1.Image = canvas;
                return;
            }

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

                MediaInfo med_info = mediaInfoList[i];
                med_info.now_width = width_tmp;
                med_info.now_height = height_tmp;
                med_info.now_left = width_index;
                med_info.now_top = height_index;
                mediaInfoList[i] = med_info;

                if (is_portrait)
                {
                    width_index += width_tmp;
                }
                else
                {
                    height_index += height_tmp;
                }

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

        private int updateImageFocus(Point mouse)
        {
            for (int index = 0; index < mediaInfoList.Count; index++)
            {
                // クリックした場所に画像があるかどうか
                if (mediaInfoList[index].now_left < mouse.X && mouse.X < mediaInfoList[index].now_left + mediaInfoList[index].now_width)
                {
                    if (mediaInfoList[index].now_top < mouse.Y && mouse.Y < mediaInfoList[index].now_top + mediaInfoList[index].now_height)
                    {
                        image_forcus = index;
                        return index;
                    }
                }
            }

            return -1;
        }

        private void walkToImage(bool next_f)
        {
            if (mediaInfoList.Count <= 0) return;

            string filename = searchDir(mediaInfoList[image_forcus].path, next_f);
            Console.WriteLine("pre:" + filename);

            insertMediaInfoList(filename, image_forcus);
            updatePictureBox();
        }

        /* 設定保存 */

        private void MainWindow_Load(object sender, EventArgs e)
        {
            this.Width = Properties.Settings.Default.WindowHeightSetting;
            this.Height = Properties.Settings.Default.WindowWidthSetting;
            this.Left = Properties.Settings.Default.WindowLeftSetting;
            this.Top = Properties.Settings.Default.WindowTopSetting;

            Console.WriteLine(Properties.Settings.Default.ImageBufferSetting0);
            Console.WriteLine(Properties.Settings.Default.ImageBufferSetting1);
            Console.WriteLine(Properties.Settings.Default.ImageBufferSetting2);
            Console.WriteLine(Properties.Settings.Default.ImageBufferSetting3);

            if (Properties.Settings.Default.ImageBufferSetting0 != "") addMediaInfoList(Properties.Settings.Default.ImageBufferSetting0);
            if (Properties.Settings.Default.ImageBufferSetting1 != "") addMediaInfoList(Properties.Settings.Default.ImageBufferSetting1);
            if (Properties.Settings.Default.ImageBufferSetting2 != "") addMediaInfoList(Properties.Settings.Default.ImageBufferSetting2);
            if (Properties.Settings.Default.ImageBufferSetting3 != "") addMediaInfoList(Properties.Settings.Default.ImageBufferSetting3);

            updatePictureBox();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.WindowHeightSetting = this.Width;
            Properties.Settings.Default.WindowWidthSetting = this.Height;
            Properties.Settings.Default.WindowLeftSetting = this.Left;
            Properties.Settings.Default.WindowTopSetting = this.Top;
            Properties.Settings.Default.Save();
        }

        private void saveSettingByMediaInfo()
        {
            Console.WriteLine("save at "+mediaInfoList.Count().ToString()+"files.");
            if (mediaInfoList.Count() == 0)
            {
                Properties.Settings.Default.ImageBufferSetting0 = "";
                Properties.Settings.Default.ImageBufferSetting1 = "";
                Properties.Settings.Default.ImageBufferSetting2 = "";
                Properties.Settings.Default.ImageBufferSetting3 = "";
            }
            else if (mediaInfoList.Count() == 1)
            {
                Properties.Settings.Default.ImageBufferSetting0 = mediaInfoList[0].path;
                Properties.Settings.Default.ImageBufferSetting1 = "";
                Properties.Settings.Default.ImageBufferSetting2 = "";
                Properties.Settings.Default.ImageBufferSetting3 = "";
            }
            else if (mediaInfoList.Count() == 2)
            {
                Properties.Settings.Default.ImageBufferSetting0 = mediaInfoList[0].path;
                Properties.Settings.Default.ImageBufferSetting1 = mediaInfoList[1].path;
                Properties.Settings.Default.ImageBufferSetting2 = "";
                Properties.Settings.Default.ImageBufferSetting3 = "";
            }
            else if (mediaInfoList.Count() == 3)
            {
                Properties.Settings.Default.ImageBufferSetting0 = mediaInfoList[0].path;
                Properties.Settings.Default.ImageBufferSetting1 = mediaInfoList[1].path;
                Properties.Settings.Default.ImageBufferSetting2 = mediaInfoList[2].path;
                Properties.Settings.Default.ImageBufferSetting3 = "";
            }
            else if (mediaInfoList.Count() == 4)
            {
                Properties.Settings.Default.ImageBufferSetting0 = mediaInfoList[0].path;
                Properties.Settings.Default.ImageBufferSetting1 = mediaInfoList[1].path;
                Properties.Settings.Default.ImageBufferSetting2 = mediaInfoList[2].path;
                Properties.Settings.Default.ImageBufferSetting3 = mediaInfoList[3].path;
            }
        }
        
        /* window操作関連 */

        private void MainWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);

                updateImageFocus(mousePoint);

                Console.WriteLine("image focus: "+image_forcus.ToString());
            }
            else if ((e.Button & MouseButtons.Middle) == MouseButtons.Middle)
            {
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);

                int tmp = updateImageFocus(mousePoint);

                if (tmp < 0)
                {
                    // 履歴を保存してから終了
                    saveSettingByMediaInfo();

                    this.Close();
                    Application.Exit();
                }

                if(mediaInfoList.Count == 0)
                {
                    this.Close();
                    Application.Exit();
                }

                removeMediaInfoList(tmp);

                updatePictureBox();
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
                walkToImage(false);
            }
            else if (e.KeyCode == Keys.Right)
            {
                // 次の画像
                walkToImage(true);
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
