// Accord Imaging Library
// Accord.NET framework
// http://www.crsouza.com
//
// Copyright © César Souza, 2009-2010
// cesarsouza at gmail.com
//

namespace Accord.Imaging
{
    using System;
    using System.Drawing;
    using Accord.Math;
    using Accord.Math.Decompositions;
    using System.Drawing.Imaging;
    using AForge.Imaging;
    using AForge;

    /// <summary>
    ///   Static tool functions for imaging.
    /// </summary>
    /// 
    public static class Tools
    {

        private const double SQRT2 = 1.4142135623730951;


        #region Algebra and geometry tools

        /// <summary>
        ///   Creates an homography matrix matching points
        ///   from a set of points to another.
        /// </summary>
        public static MatrixH Homography(PointH[] points1, PointH[] points2)
        {
            // Initial argument checkings
            if (points1.Length != points2.Length)
                throw new ArgumentException("The number of points should be equal.");

            if (points1.Length < 4)
                throw new ArgumentException("At least four points are required to fit an homography");


            int N = points1.Length;

            MatrixH T1, T2; // Normalize input points
            points1 = Tools.Normalize(points1, out T1);
            points2 = Tools.Normalize(points2, out T2);

            // Create the matrix A
            double[,] A = new double[3 * N, 9];
            for (int i = 0; i < N; i++)
            {
                PointH X = points1[i];
                double x = points2[i].X;
                double y = points2[i].Y;
                double w = points2[i].W;
                int r = 3 * i;

                A[r, 0] = 0;
                A[r, 1] = 0;
                A[r, 2] = 0;
                A[r, 3] = -w * X.X;
                A[r, 4] = -w * X.Y;
                A[r, 5] = -w * X.W;
                A[r, 6] = y * X.X;
                A[r, 7] = y * X.Y;
                A[r, 8] = y * X.W;

                r++;
                A[r, 0] = w * X.X;
                A[r, 1] = w * X.Y;
                A[r, 2] = w * X.W;
                A[r, 3] = 0;
                A[r, 4] = 0;
                A[r, 5] = 0;
                A[r, 6] = -x * X.X;
                A[r, 7] = -x * X.Y;
                A[r, 8] = -x * X.W;

                r++;
                A[r, 0] = -y * X.X;
                A[r, 1] = -y * X.Y;
                A[r, 2] = -y * X.W;
                A[r, 3] = x * X.X;
                A[r, 4] = x * X.Y;
                A[r, 5] = x * X.W;
                A[r, 6] = 0;
                A[r, 7] = 0;
                A[r, 8] = 0;
            }


            // Create the singular value decomposition
            SingularValueDecomposition svd = new SingularValueDecomposition(A, false, true);
            double[,] V = svd.RightSingularVectors;


            // Extract the homography matrix
            MatrixH H = new MatrixH((float)V[0, 8], (float)V[1, 8], (float)V[2, 8],
                                    (float)V[3, 8], (float)V[4, 8], (float)V[5, 8],
                                    (float)V[6, 8], (float)V[7, 8], (float)V[8, 8]);

            // Denormalize
            H = T2.Inverse().Multiply(H.Multiply(T1));

            return H;
        }

        /// <summary>
        ///   Creates an homography matrix matching points
        ///   from a set of points to another.
        /// </summary>
        public static MatrixH Homography(PointF[] points1, PointF[] points2)
        {
            // Initial argument checkings
            if (points1.Length != points2.Length)
                throw new ArgumentException("The number of points should be equal.");

            if (points1.Length < 4)
                throw new ArgumentException("At least four points are required to fit an homography");


            int N = points1.Length;

            MatrixH T1, T2; // Normalize input points
            points1 = Tools.Normalize(points1, out T1);
            points2 = Tools.Normalize(points2, out T2);

            // Create the matrix A
            double[,] A = new double[3 * N, 9];
            for (int i = 0; i < N; i++)
            {
                PointF X = points1[i];
                double x = points2[i].X;
                double y = points2[i].Y;
                int r = 3 * i;

                A[r, 0] = 0;
                A[r, 1] = 0;
                A[r, 2] = 0;
                A[r, 3] = -X.X;
                A[r, 4] = -X.Y;
                A[r, 5] = -1;
                A[r, 6] = y * X.X;
                A[r, 7] = y * X.Y;
                A[r, 8] = y;

                r++;
                A[r, 0] = X.X;
                A[r, 1] = X.Y;
                A[r, 2] = 1;
                A[r, 3] = 0;
                A[r, 4] = 0;
                A[r, 5] = 0;
                A[r, 6] = -x * X.X;
                A[r, 7] = -x * X.Y;
                A[r, 8] = -x;

                r++;
                A[r, 0] = -y * X.X;
                A[r, 1] = -y * X.Y;
                A[r, 2] = -y;
                A[r, 3] = x * X.X;
                A[r, 4] = x * X.Y;
                A[r, 5] = x;
                A[r, 6] = 0;
                A[r, 7] = 0;
                A[r, 8] = 0;
            }


            // Create the singular value decomposition
            SingularValueDecomposition svd = new SingularValueDecomposition(A, false, true);
            double[,] V = svd.RightSingularVectors;


            // Extract the homography matrix
            MatrixH H = new MatrixH((float)V[0, 8], (float)V[1, 8], (float)V[2, 8],
                                    (float)V[3, 8], (float)V[4, 8], (float)V[5, 8],
                                    (float)V[6, 8], (float)V[7, 8], (float)V[8, 8]);

            // Denormalize
            H = T2.Inverse().Multiply(H.Multiply(T1));

            return H;
        }

        /// <summary>
        ///   Normalizes a set of homogeneous points so that the origin is located
        ///   at the centroid and the mean distance to the origin is sqrt(2).
        /// </summary>
        public static PointH[] Normalize(this PointH[] points, out MatrixH T)
        {
            float n = points.Length;
            float xmean = 0, ymean = 0;
            for (int i = 0; i < points.Length; i++)
            {
                points[i].X = points[i].X / points[i].W;
                points[i].Y = points[i].Y / points[i].W;
                points[i].W = 1;

                xmean += points[i].X;
                ymean += points[i].Y;
            }
            xmean /= n; ymean /= n;


            float scale = 0;
            for (int i = 0; i < points.Length; i++)
            {
                float x = points[i].X - xmean;
                float y = points[i].Y - ymean;

                scale += (float)System.Math.Sqrt(x * x + y * y);
            }

            scale = (float)(SQRT2 * n / scale);


            T = new MatrixH
                (
                    scale, 0, -scale * xmean,
                    0, scale, -scale * ymean,
                    0, 0, 1
                );

            return T.TransformPoints(points);
        }

        /// <summary>
        ///   Normalizes a set of homogeneous points so that the origin is located
        ///   at the centroid and the mean distance to the origin is sqrt(2).
        /// </summary>
        public static PointF[] Normalize(this PointF[] points, out MatrixH T)
        {
            float n = points.Length;
            float xmean = 0, ymean = 0;
            for (int i = 0; i < points.Length; i++)
            {
                points[i].X = points[i].X;
                points[i].Y = points[i].Y;

                xmean += points[i].X;
                ymean += points[i].Y;
            }
            xmean /= n; ymean /= n;


            float scale = 0;
            for (int i = 0; i < points.Length; i++)
            {
                float x = points[i].X - xmean;
                float y = points[i].Y - ymean;

                scale += (float)System.Math.Sqrt(x * x + y * y);
            }

            scale = (float)(SQRT2 * n / scale);


            T = new MatrixH
                (
                    scale, 0, -scale * xmean,
                    0, scale, -scale * ymean,
                    0, 0, 1
                );

            return T.TransformPoints(points);
        }

        /// <summary>
        ///   Detects if three points are colinear.
        /// </summary>
        public static bool Colinear(PointF pt1, PointF pt2, PointF pt3)
        {
            return System.Math.Abs(
                 (pt1.Y - pt2.Y) * pt3.X +
                 (pt2.X - pt1.X) * pt3.Y +
                 (pt1.X * pt2.Y - pt1.Y * pt2.X)) < Special.SingleEpsilon;
        }

        /// <summary>
        ///   Detects if three points are colinear.
        /// </summary>
        public static bool Colinear(PointH pt1, PointH pt2, PointH pt3)
        {
            return System.Math.Abs(
             (pt1.Y * pt2.W - pt1.W * pt2.Y) * pt3.X +
             (pt1.W * pt2.X - pt1.X * pt2.W) * pt3.Y +
             (pt1.X * pt2.Y - pt1.Y * pt2.X) * pt3.W) < Special.SingleEpsilon;
        }
        #endregion


        #region Image tools
        /// <summary>
        ///   Computes the sum of the pixels in a given image.
        /// </summary>
        public static int Sum(this BitmapData image)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new UnsupportedImageFormatException("Only grayscale images are supported");

            int width = image.Width;
            int height = image.Height;
            int offset = image.Stride - image.Width;

            int sum = 0;

            unsafe
            {
                byte* src = (byte*)image.Scan0.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src++)
                        sum += (*src);
                    src += offset;
                }
            }

            return sum;
        }

        /// <summary>
        ///   Computes the sum of the pixels in a given image.
        /// </summary>
        public static int Sum(this BitmapData image, Rectangle rectangle)
        {
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                throw new UnsupportedImageFormatException("Only grayscale images are supported");

            int width = image.Width;
            int height = image.Height;
            int stride = image.Stride;
            int offset = image.Stride - image.Width;

            int rwidth = rectangle.Width;
            int rheight = rectangle.Height;
            int rx = rectangle.X;
            int ry = rectangle.Y;

            int sum = 0;

            unsafe
            {
                byte* src = (byte*)image.Scan0.ToPointer();

                for (int y = 0; y < rheight; y++)
                {
                    byte* p = src + stride * (ry + y) + rx;

                    for (int x = 0; x < rwidth; x++)
                        sum += (*p++);
                }
            }

            return sum;
        }

        /// <summary>
        ///   Computes the sum of the pixels in a given image.
        /// </summary>
        public static int Sum(this Bitmap image)
        {
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            int sum = Sum(data);

            image.UnlockBits(data);

            return sum;
        }

        /// <summary>
        ///   Computes the sum of the pixels in a given image.
        /// </summary>
        public static int Sum(this Bitmap image, Rectangle rectangle)
        {
            BitmapData data = image.LockBits(rectangle,
                ImageLockMode.ReadOnly, image.PixelFormat);

            int sum = Sum(data);

            image.UnlockBits(data);

            return sum;
        }

        /// <summary>
        ///   Converts a given image into a array of double-precision
        ///   floating-point numbers scaled between -1 and 1.
        /// </summary>
        public static double[] ToDoubleArray(this Bitmap image, int channel)
        {
            return ToDoubleArray(image, channel, -1, 1);
        }

        /// <summary>
        ///   Converts a given image into a array of double-precision
        ///   floating-point numbers scaled between the given range.
        /// </summary>
        public static double[] ToDoubleArray(this Bitmap image, int channel, double min, double max)
        {
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            double[] array = ToDoubleArray(data, channel, min, max);

            image.UnlockBits(data);

            return array;
        }

        /// <summary>
        ///   Converts a given image into a array of double-precision
        ///   floating-point numbers scaled between -1 and 1.
        /// </summary>
        public static double[] ToDoubleArray(this BitmapData image, int channel)
        {
            return ToDoubleArray(image, channel, -1, 1);
        }

        /// <summary>
        ///   Converts a given image into a array of double-precision
        ///   floating-point numbers scaled between the given range.
        /// </summary>
        public static double[] ToDoubleArray(this BitmapData image, int channel, double min, double max)
        {
            int width = image.Width;
            int height = image.Height;
            int offset = image.Stride - image.Width;

            double[] data = new double[width * height];
            int dst = 0;

            unsafe
            {
                byte* src = (byte*)image.Scan0.ToPointer() + channel;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src++, dst++)
                    {
                        data[dst] = Accord.Math.Tools.Scale(0, 255, min, max, *src);
                    }
                    src += offset;
                }
            }

            return data;
        }

        /// <summary>
        ///   Converts a given image into a array of double-precision
        ///   floating-point numbers scaled between -1 and 1.
        /// </summary>
        public static double[][] ToDoubleArray(this Bitmap image)
        {
            return ToDoubleArray(image, -1, 1);
        }

        /// <summary>
        ///   Converts a given image into a array of double-precision
        ///   floating-point numbers scaled between the given range.
        /// </summary>
        public static double[][] ToDoubleArray(this Bitmap image, double min, double max)
        {
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            double[][] array = ToDoubleArray(data, min, max);

            image.UnlockBits(data);

            return array;
        }

        /// <summary>
        ///   Converts a given image into a array of double-precision
        ///   floating-point numbers scaled between the given range.
        /// </summary>
        public static double[][] ToDoubleArray(this BitmapData image, double min, double max)
        {
            int width = image.Width;
            int height = image.Height;
            int pixelSize = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) / 8;
            int offset = image.Stride - image.Width * pixelSize;

            double[][] data = new double[width * height][];
            int dst = 0;

            unsafe
            {
                byte* src = (byte*)image.Scan0.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, dst++)
                    {
                        double[] pixel = data[dst] = new double[pixelSize];
                        for (int i = pixel.Length-1; i >= 0; i--, src++)
                            pixel[i] = Accord.Math.Tools.Scale(0, 255, min, max, *src);
                    }
                    src += offset;
                }
            }

            return data;
        }
        #endregion


        #region Conversions

        /// <summary>
        ///   Converts an image given as a array of pixel values into
        ///   a <see cref="System.Drawing.Bitmap"/>.
        /// </summary>
        /// <param name="pixels">An array containing the grayscale pixel
        /// values as <see cref="System.Double">doubles</see>.</param>
        /// <param name="width">The width of the resulting image.</param>
        /// <param name="height">The height of the resulting image.</param>
        /// <param name="min">The minimum value representing a color value of 0.</param>
        /// <param name="max">The maximum value representing a color value of 255. </param>
        /// <returns>A <see cref="System.Drawing.Bitmap"/> of given width and height
        /// containing the given pixel values.</returns>
        public static Bitmap ToBitmap(this double[] pixels, int width, int height, double min, double max)
        {
            Bitmap bitmap = AForge.Imaging.Image.CreateGrayscaleImage(width, height);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, bitmap.PixelFormat);

            int offset = data.Stride - width;
            int src = 0;

            unsafe
            {
                byte* dst = (byte*)data.Scan0.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src++, dst++)
                    {
                        *dst = (byte)Accord.Math.Tools.Scale(min, max, 0, 255, pixels[src]);
                    }
                    dst += offset;
                }
            }

            bitmap.UnlockBits(data);

            return bitmap;
        }

        /// <summary>
        ///   Converts an image given as a array of pixel values into
        ///   a <see cref="System.Drawing.Bitmap"/>.
        /// </summary>
        /// <param name="pixels">An jagged array containing the pixel values
        /// as double arrays. Each element of the arrays will be converted to
        /// a R, G, B, A value. The bits per pixel of the resulting image
        /// will be set according to the size of these arrays.</param>
        /// <param name="width">The width of the resulting image.</param>
        /// <param name="height">The height of the resulting image.</param>
        /// <param name="min">The minimum value representing a color value of 0.</param>
        /// <param name="max">The maximum value representing a color value of 255. </param>
        /// <returns>A <see cref="System.Drawing.Bitmap"/> of given width and height
        /// containing the given pixel values.</returns>
        public static Bitmap ToBitmap(this double[][] pixels, int width, int height, double min, double max)
        {
            PixelFormat format;
            int channels = pixels[0].Length;

            switch (channels)
            {
                case 1:
                    format = PixelFormat.Format8bppIndexed;
                    break;

                case 3:
                    format = PixelFormat.Format24bppRgb;
                    break;

                case 4:
                    format = PixelFormat.Format32bppArgb;
                    break;

                default:
                    throw new ArgumentException("pixels");
            }


            Bitmap bitmap = new Bitmap(width, height, format);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, format);

            int pixelSize = System.Drawing.Image.GetPixelFormatSize(format) / 8;
            int offset = data.Stride - width * pixelSize;
            int src = 0;

            unsafe
            {
                byte* dst = (byte*)data.Scan0.ToPointer();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++, src++)
                    {
                        for (int c = channels - 1; c >= 0; c--, dst++)
                        {
                            *dst = (byte)Accord.Math.Tools.Scale(min, max, 0, 255, pixels[src][c]);
                        }
                    }
                    dst += offset;
                }
            }

            bitmap.UnlockBits(data);

            return bitmap;
        }

        /// <summary>
        ///   Converts an image given as a array of pixel values into
        ///   a <see cref="System.Drawing.Bitmap"/>.
        /// </summary>
        /// <param name="pixels">An jagged array containing the pixel values
        /// as double arrays. Each element of the arrays will be converted to
        /// a R, G, B, A value. The bits per pixel of the resulting image
        /// will be set according to the size of these arrays.</param>
        /// <param name="width">The width of the resulting image.</param>
        /// <param name="height">The height of the resulting image.</param>
        /// <returns>A <see cref="System.Drawing.Bitmap"/> of given width and height
        /// containing the given pixel values.</returns>
        public static Bitmap ToBitmap(this double[][] pixels, int width, int height)
        {
            return ToBitmap(pixels, width, height, -1, 1);
        }
        #endregion

    }
}
