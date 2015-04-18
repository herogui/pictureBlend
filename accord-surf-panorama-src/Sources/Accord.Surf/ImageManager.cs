using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Drawing.Drawing2D;

namespace Accord.Surf
{
    public class ImageManager
    {
        Bitmap Bit1, Bit2;
        int CutSize;
        float Thread1 = 0.0001f, Thread2 = 0.0001f;

        List<List<IPoint2>> ImgListFt1;
        List<List<IPoint2>> ImgListFt2;

        public ImageManager(Bitmap bit1, Bitmap bit2, float thread1, float thread2, int cutSize)
        {
            this.Bit1 = bit1;
            this.Bit2 = bit2;
            this.CutSize = cutSize;
            this.Thread1 = thread1;
            this.Thread2 = thread2;
        }

        public ImageManager(Bitmap bit1, Bitmap bit2, int cutSize)
        {
            this.Bit1 = bit1;
            this.Bit2 = bit2;
            this.CutSize = cutSize;
        }

        public List<IPoint2>[] GetMatchPoint()
        {
            Thread thd1 = new Thread(new ThreadStart(delegate() { this.Cut1(); }));
            thd1.Name = "线程1";
            Thread thd2 = new Thread(new ThreadStart(delegate() { this.Cut2(); }));
            thd2.Name = "线程2";
            thd1.Start();
            thd2.Start();
            thd1.Join();
            thd2.Join();
            thd1.Abort();
            thd2.Abort();

            List<IPoint2>[] matchesAll = new List<IPoint2>[2];
            matchesAll[0] = new List<IPoint2>();
            matchesAll[1] = new List<IPoint2>();

            foreach (List<IPoint2> kv1 in this.ImgListFt1)
            {
                foreach (List<IPoint2> kv2 in ImgListFt2)
                {
                    List<IPoint2>[] matches = Utils.getMatches(kv1, kv2);

                    matchesAll[0].AddRange(matches[0]);
                    matchesAll[1].AddRange(matches[1]);
                }
            }
            return matchesAll;
        }

        List<List<IPoint2>> Cut(Bitmap bit, float thread)
        {
            int I1Width = bit.Width;
            int I1Height = bit.Height;

            int I1CutX = (int)(I1Width / this.CutSize) + 1;
            int I1CutY = (int)(I1Height / this.CutSize) + 1;

            List<List<IPoint2>> listListFt = new List<List<IPoint2>>();

            if (I1CutX == 0 || I1CutY == 0)
            {
                listListFt.Add(GetFtPntList(bit, thread));
                return listListFt;
            }

            for (int cutX = 0; cutX < I1CutX; cutX++)
            {
                for (int cutY = 0; cutY < I1CutY; cutY++)
                {
                    int xmin = this.CutSize * cutX;
                    int ymin = this.CutSize * cutY;

                    int cutDisx, cutDixY;
                    if (cutX != I1CutX - 1)
                        cutDisx = this.CutSize;
                    else
                        cutDisx = I1Width - this.CutSize * cutX;

                    if (cutY != I1CutY - 1)
                        cutDixY = this.CutSize;
                    else
                        cutDixY = I1Height - this.CutSize * cutY;

                    Rectangle rect = new Rectangle(xmin, ymin, cutDisx, cutDixY);
                    List<IPoint2> pntList1;

                    //Graphics重新绘制 这样可以降低内存的使用
                    using (Bitmap cutImg = new Bitmap(cutDisx, cutDixY, bit.PixelFormat))
                    {
                        cutImg.SetResolution(bit.HorizontalResolution, bit.VerticalResolution);
                        using (Graphics g = Graphics.FromImage(cutImg))
                        {

                            // 用白色清空 
                            g.Clear(Color.White);

                            // 指定高质量的双三次插值法。执行预筛选以确保高质量的收缩。此模式可产生质量最高的转换图像。 
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                            // 指定高质量、低速度呈现。 
                            g.SmoothingMode = SmoothingMode.HighQuality;

                            // 在指定位置并且按指定大小绘制指定的 Image 的指定部分。 
                            g.DrawImage(bit, new Rectangle(0, 0, cutImg.Width, cutImg.Height), rect, GraphicsUnit.Pixel);

                            pntList1 = GetFtPntList(cutImg, thread);
                        }
                    }

                    foreach (IPoint2 pnt in pntList1)
                    {
                        pnt.x += this.CutSize * cutX;
                        pnt.y += this.CutSize * cutY;
                    }

                    listListFt.Add(pntList1);
                }
            }

            return listListFt;
        }

        void Cut1()
        {
            this.ImgListFt1 = this.Cut(this.Bit1, this.Thread1);
        }

        void Cut2()
        {
            this.ImgListFt2 = this.Cut(this.Bit2, this.Thread2);
        }

        List<IPoint2> GetFtPntList(Bitmap bitImg, float thread)
        {
            List<IPoint2> ipts1 = new List<IPoint2>();//图片1的特征点          

            //--------------------------------------------------
            // Create Integral Image
            IntegralImage iimg = IntegralImage.FromImage(bitImg);

            // Extract the interest points
            ipts1 = FastHessian.getIpoints(thread, //此值越小，特征点越多
                                                                    5, 2, iimg);

            // Describe the interest points
            SurfDescriptor.DecribeInterestPoints(ipts1, false, //是否表示方向
                                                 false, //false为64，true为128
                                                 iimg);
            iimg.Dispose();
            iimg = null;

            return ipts1;
        }
    }
}
