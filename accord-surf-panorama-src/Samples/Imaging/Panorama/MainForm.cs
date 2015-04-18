using System;
using System.Drawing;
using System.Windows.Forms;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math;
using AForge;
using System.Diagnostics;
using Accord.Surf;
using System.Collections.Generic;
using System.Threading;

namespace Panorama
{
    public partial class MainForm : Form
    {
        private Bitmap img1;
        private Bitmap img2;     

        Thread thd;

        float thread = 0.0005f;//值越小   匹配点越多
        int cutSize = 2000; //长或宽大于cutSize 则做切割处理

        public MainForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.label1.ForeColor = Color.Red;
        }

        private void btnSurf_Click(object sender, EventArgs e)
        {
            if ((null != this.textBox1.Text && this.textBox1.Text.Length > 0) && (null != this.textBox2.Text && this.textBox2.Text.Length > 0))
            {
                if (!System.IO.File.Exists(this.textBox1.Text))
                {
                    this.textBox1.Focus();
                    MessageBox.Show("    亲，图片1不存在！");
                    return;
                }

                if (!System.IO.File.Exists(this.textBox2.Text))
                {
                    this.textBox2.Focus();
                    MessageBox.Show("    亲，图片2不存在！");
                    return;
                }

                this.label1.Text = "正在拼接......";
                thd = new Thread(new ThreadStart(delegate() { this.SurfMatch(this.img1,this.img2); }));
                thd.Start();

                this.btnBlend.Enabled = false;
            }
            else
            {
                MessageBox.Show("    亲，还没打开图片呢！");
            }
        }

        void SurfMatch(Bitmap img1, Bitmap img2)
        { 
            Stopwatch watch = Stopwatch.StartNew();

            ////主线程
            //List<IPoint2> ipts1 = GetFtPntList(img1, thread);//图片1的特征点
            //List<IPoint2> ipts2 = GetFtPntList(img2, thread);//图片2的特征点 
            //List<IPoint2>[] matches = Utils.getMatches(ipts1, ipts2);

            //多线程且对图像进行分割           
            ImageManager imgM = new ImageManager(img1, img2, thread, thread, cutSize);
            List<IPoint2>[] matches = imgM.GetMatchPoint();

            IntPoint[] correlationPoints1 = new IntPoint[matches[0].Count];
            IntPoint[] correlationPoints2 = new IntPoint[matches[1].Count];

            List<IPoint2> list1 = matches[0];
            int num = 0;
            foreach (IPoint2 kv in list1)
            {
                correlationPoints1[num] = new IntPoint { X = (int)kv.x, Y = (int)kv.y };
                num++;
            }

            int num1 = 0;
            List<IPoint2> list2 = matches[1];
            foreach (IPoint2 kv in list2)
            {
                correlationPoints2[num1] = new IntPoint { X = (int)kv.x, Y = (int)kv.y };
                num1++;
            }

            if (correlationPoints1.Length > 0)
            {
                RansacHomographyEstimator ransac = new RansacHomographyEstimator(0.001, 0.99);
                MatrixH homography = ransac.Estimate(correlationPoints1, correlationPoints2); 

                Blend blend = new Blend(homography, img1);
                pictureBox.Image = blend.Apply(img2);

                //计算时间
                long matchTime = watch.ElapsedMilliseconds;
                this.Invoke(new Action(delegate()
                {
                    if (matchTime < 1000)
                        this.label1.Text = "完成！耗时 " + matchTime.ToString() + " 毫秒！";
                    else
                        this.label1.Text = "完成！耗时 " + (matchTime / 1000.0).ToString() + " 秒！";

                    this.btnSave.Visible = true;
                    this.btnBlend.Enabled = true;
                }));
            }
            else
            {
                //计算时间
                long matchTime = watch.ElapsedMilliseconds;
                this.Invoke(new Action(delegate()
                {
                    this.label1.Text = "没有找到相同点！耗时 " + matchTime.ToString() + " 毫秒！";
                }));
            }

            watch.Stop();
            thd.Abort();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //string localFilePath, fileNameExt, newFileName, FilePath;  
            string localFilePath = String.Empty;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            //设置文件类型  
            saveFileDialog1.Filter = fileFilter;    

            //保存对话框是否记忆上次打开的目录  
            saveFileDialog1.RestoreDirectory = true;

            //点了保存按钮进入  
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //获得文件路径  
                localFilePath = saveFileDialog1.FileName.ToString();
                this.pictureBox.Image.Save(localFilePath);
            }
        }

        string fileFilter = "jpg files (*.jpg)|*.jpg|png files (*.png)|*.png|所有文件(*.*)|*.*";
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择图片1";
            fileDialog.Filter = fileFilter;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = fileDialog.FileName;
                this.img1 = new Bitmap(this.textBox1.Text);
                this.pictureBox.Image = this.img1;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择图片2";
            fileDialog.Filter = fileFilter;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {  
                if (this.textBox1.Text.Trim() == fileDialog.FileName.Trim())
                {
                    MessageBox.Show("亲，图片1和图片2相同了！");
                    return;
                }

                this.textBox2.Text = fileDialog.FileName;
                this.img2 = new Bitmap(this.textBox2.Text);

                // Concatenate and show entire image at start
                Concatenate concatenate = new Concatenate(this.img1);
                pictureBox.Image = concatenate.Apply(this.img2);
            }            
        }      
    }
}
