using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Accord.Surf
{
    public class IntegralImage:IDisposable
    {
        const float cR = .2989f;
        const float cG = .5870f;
        const float cB = .1140f;

        internal float[,] Matrix;
        public int Width, Height;

        public float this[int y, int x]
        {
            get { return Matrix[y, x]; }
            set { Matrix[y, x] = value; }
        }

        private IntegralImage(int width, int height)
        {

            this.Width = width;
            this.Height = height;

            this.Matrix = new float[height, width];
        }

        public static IntegralImage FromImage(Bitmap image)
        {
            IntegralImage pic = new IntegralImage(image.Width,image.Height);
            
            float rowsum = 0;
           /*      for (int y = 0; y < image.Height; y++)
            {
                rowsum = 0;
                for (int x = 0; x < image.Width; x++)
                {
                    Color c = image.GetPixel(x, y);
                    rowsum += (cR * c.R + cG * c.G + cB * c.B) / 255f;

                    // integral image is rowsum + value above   
                    if(y==0)
                        pic[0, x] = rowsum;
                    else
                        pic[y, x] = rowsum + pic[y - 1, x];
                }
            }*/

            BitmapData dataIn = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                unsafe
                {
                    byte* pIn = (byte*)(dataIn.Scan0.ToPointer());
                    for (int y = 0; y < dataIn.Height; y++)
                    {
                        rowsum = 0;
                        for (int x = 0; x < dataIn.Width; x++)
                        {
                            int cb = (byte)(pIn[0]);
                            int cg = (byte)(pIn[1]);
                            int cr = (byte)(pIn[2]);

                            // 0 1 2代表的次序是B G R
                            rowsum += (cR * cr + cG * cg + cB * cb) / 255f;
                            // integral image is rowsum + value above   
                            if (y == 0)
                                pic[0, x] = rowsum;
                            else
                                pic[y, x] = rowsum + pic[y - 1, x];

                            pIn += 3;
                        }
                        pIn += dataIn.Stride - dataIn.Width * 3;
                    }
                }
                image.UnlockBits(dataIn);
 
            return pic;
        }


        public float BoxIntegral(int row, int col, int rows, int cols)
        {
            // The subtraction by one for row/col is because row/col is inclusive.
            int r1 = Math.Min(row, Height) - 1;
            int c1 = Math.Min(col, Width) - 1;
            int r2 = Math.Min(row + rows, Height) - 1;
            int c2 = Math.Min(col + cols, Width) - 1;

            float A = 0, B = 0, C = 0, D = 0;
            if (r1 >= 0 && c1 >= 0) A = Matrix[r1, c1];
            if (r1 >= 0 && c2 >= 0) B = Matrix[r1, c2];
            if (r2 >= 0 && c1 >= 0) C = Matrix[r2, c1];
            if (r2 >= 0 && c2 >= 0) D = Matrix[r2, c2];

            return Math.Max(0, A - B - C + D);
        }

        /// <summary>
        /// Get Haar Wavelet X repsonse
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public float HaarX(int row, int column, int size)
        {
            return BoxIntegral(row - size / 2, column, size, size / 2)
              - 1 * BoxIntegral(row - size / 2, column - size / 2, size, size / 2);
        }

        /// <summary>
        /// Get Haar Wavelet Y repsonse
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public float HaarY(int row, int column, int size)
        {
            return BoxIntegral(row, column - size / 2, size / 2, size)
              - 1 * BoxIntegral(row - size / 2, column - size / 2, size / 2, size);
        }

        public void Dispose()
        {
            Matrix = null;
        }
    }
}
