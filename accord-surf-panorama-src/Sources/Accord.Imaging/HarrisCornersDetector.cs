// Accord Imaging Library
// Accord.NET framework
// http://www.crsouza.com
//
// Copyright © César Souza, 2009-2010
// cesarsouza at gmail.com
//

namespace Accord.Imaging
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using AForge.Imaging;
    using AForge.Imaging.Filters;
    using AForge;

    /// <summary>
    ///   Harris Corners Detector.
    /// </summary>
    /// <remarks>
    /// <para>This class implements the Harris corners detector.</para>
    /// <para>Sample usage:</para>
    /// <code>
    /// // create corners detector's instance
    /// HarrisCornersDetector hcd = new HarrisCornersDetector( );
    /// // process image searching for corners
    /// Point[] corners = hcd.ProcessImage( image );
    /// // process points
    /// foreach ( Point corner in corners )
    /// {
    ///     // ... 
    /// }
    /// </code>
    /// 
    /// <para>
    ///   References:
    ///   <list type="bullet">
    ///     <item><description>
    ///       P. D. Kovesi. MATLAB and Octave Functions for Computer Vision and Image Processing.
    ///       School of Computer Science and Software Engineering, The University of Western Australia.
    ///       Available in: http://www.csse.uwa.edu.au/~pk/Research/MatlabFns/Spatial/harris.m</description></item>
    ///     <item><description>
    ///       C.G. Harris and M.J. Stephens. "A combined corner and edge detector", 
    ///       Proceedings Fourth Alvey Vision Conference, Manchester.
    ///       pp 147-151, 1988.</description></item>
    ///     <item><description>
    ///       Alison Noble, "Descriptions of Image Surfaces", PhD thesis, Department
    ///       of Engineering Science, Oxford University 1989, p45.</description></item>
    ///   </list>
    /// </para>
    /// </remarks>
    /// 
    /// <seealso cref="MoravecCornersDetector"/>
    /// <seealso cref="SusanCornersDetector"/>
    ///
    public class HarrisCornersDetector : ICornersDetector
    {

        private float k = 0.04f;
        private float threshold = 1000f;
        private double sigma = 1.4;
        private int r = 3;


        /// <summary>
        ///   Harris parameter k. Default value is 0.04.
        /// </summary>
        public float K
        {
            get { return k; }
            set { k = value; }
        }

        /// <summary>
        ///   Harris threshold. Default value is 1000.
        /// </summary>
        public float Threshold
        {
            get { return threshold; }
            set { threshold = value; }
        }

        /// <summary>
        ///   Gaussian smoothing sigma. Default value is 1.4.
        /// </summary>
        public double Sigma
        {
            get { return sigma; }
            set { sigma = value; }
        }

        /// <summary>
        ///   Non-maximum suppression window radius. Default value is 3.
        /// </summary>
        public int Suppression
        {
            get { return r; }
            set { r = value; }
        }



        /// <summary>
        ///   Initializes a new instance of the <see cref="HarrisCornersDetector"/> class.
        /// </summary>
        public HarrisCornersDetector()
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="HarrisCornersDetector"/> class.
        /// </summary>
        public HarrisCornersDetector(float k)
            : this()
        {
            this.k = k;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="HarrisCornersDetector"/> class.
        /// </summary>
        public HarrisCornersDetector(float k, float threshold)
            : this()
        {
            this.k = k;
            this.threshold = threshold;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="HarrisCornersDetector"/> class.
        /// </summary>
        public HarrisCornersDetector(float k, float threshold, double sigma)
            : this()
        {
            this.k = k;
            this.threshold = threshold;
            this.sigma = sigma;
        }



        /// <summary>
        /// Process image looking for corners.
        /// </summary>
        /// 
        /// <param name="image">Source image data to process.</param>
        /// 
        /// <returns>Returns list of found corners (X-Y coordinates).</returns>
        /// 
        /// <exception cref="UnsupportedImageFormatException">
        ///   The source image has incorrect pixel format.
        /// </exception>
        /// 
        public List<IntPoint> ProcessImage(UnmanagedImage image)
        {
            // check image format
            if (
                (image.PixelFormat != PixelFormat.Format8bppIndexed) &&
                (image.PixelFormat != PixelFormat.Format24bppRgb) &&
                (image.PixelFormat != PixelFormat.Format32bppRgb) &&
                (image.PixelFormat != PixelFormat.Format32bppArgb)
                )
            {
                throw new UnsupportedImageFormatException("Unsupported pixel format of the source image.");
            }

            // make sure we have grayscale image
            UnmanagedImage grayImage = null;

            if (image.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                grayImage = image;
            }
            else
            {
                // create temporary grayscale image
                grayImage = Grayscale.CommonAlgorithms.BT709.Apply(image);
            }


            // get source image size
            int width = grayImage.Width;
            int height = grayImage.Height;
            int stride = grayImage.Stride;
            int offset = stride - width;



            // 1. Calculate partial differences
            UnmanagedImage diffx = UnmanagedImage.Create(width, height, PixelFormat.Format8bppIndexed);
            UnmanagedImage diffy = UnmanagedImage.Create(width, height, PixelFormat.Format8bppIndexed);
            UnmanagedImage diffxy = UnmanagedImage.Create(width, height, PixelFormat.Format8bppIndexed);

            unsafe
            {
                // Compute dx and dy
                byte* src = (byte*)grayImage.ImageData.ToPointer();
                byte* dx = (byte*)diffx.ImageData.ToPointer();
                byte* dy = (byte*)diffy.ImageData.ToPointer();
                byte* dxy = (byte*)diffxy.ImageData.ToPointer();

                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src++, dx++, dy++)
                    {
                        // TODO: Place those verifications
                        // outside the innermost loop
                        if (x == 0 || x == width  - 1 ||
                            y == 0 || y == height - 1)
                        {
                            *dx = *dy = 0; continue;
                        }
                                                    
                        int h = -(src[-stride - 1] + src[-1] + src[stride - 1]) +
                                 (src[-stride + 1] + src[+1] + src[stride + 1]);
                        *dx = (byte)(h > 255 ? 255 : h < 0 ? 0 : h);

                        int v = -(src[-stride - 1] + src[-stride] + src[-stride + 1]) +
                                 (src[+stride - 1] + src[+stride] + src[+stride + 1]);
                        *dy = (byte)(v > 255 ? 255 : v < 0 ? 0 : v);
                    }
                    src += offset;
                    dx += offset;
                    dy += offset;
                }


                // Compute dxy
                dx = (byte*)diffx.ImageData.ToPointer();
                dxy = (byte*)diffxy.ImageData.ToPointer();

                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, dx++, dxy++)
                    {
                        if (x == 0 || x == width  - 1 ||
                            y == 0 || y == height - 1)
                        {
                            *dxy = 0; continue;
                        }

                        int v = -(dx[-stride - 1] + dx[-stride] + dx[-stride + 1]) +
                                 (dx[+stride - 1] + dx[+stride] + dx[+stride + 1]);
                        *dxy = (byte)(v > 255 ? 255 : v < 0 ? 0 : v);
                    }
                    dx += offset;
                    dxy += offset;
                }
            }


            // 2. Smooth the diff images
            if (sigma > 0.0)
            {
                GaussianBlur blur = new GaussianBlur(sigma);
                blur.ApplyInPlace(diffx);
                blur.ApplyInPlace(diffy);
                blur.ApplyInPlace(diffxy);
            }


            // 3. Compute Harris Corner Response
            float[,] H = new float[height, width];

            unsafe
            {
                byte* ptrA = (byte*)diffx.ImageData.ToPointer();
                byte* ptrB = (byte*)diffy.ImageData.ToPointer();
                byte* ptrC = (byte*)diffxy.ImageData.ToPointer();
                float M, A, B, C;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        A = *(ptrA++);
                        B = *(ptrB++);
                        C = *(ptrC++);

                        // Harris corner measure
                        M = (A * B - C * C) - (k * ((A + B) * (A + B)));

                        if (M > threshold)
                            H[y, x] = M;
                        else H[y, x] = 0;
                    }

                    ptrA += offset;
                    ptrB += offset;
                    ptrC += offset;
                }
            }


            // Free resources
            diffx.Dispose();
            diffy.Dispose();
            diffxy.Dispose();

            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
                grayImage.Dispose();


            // 4. Suppress non-maximum points
            List<IntPoint> cornersList = new List<IntPoint>();

            // for each row
            for (int y = r, maxY = height - r; y < maxY; y++)
            {
                // for each pixel
                for (int x = r, maxX = width - r; x < maxX; x++)
                {
                    float currentValue = H[y, x];

                    // for each windows' row
                    for (int i = -r; (currentValue != 0) && (i <= r); i++)
                    {
                        // for each windows' pixel
                        for (int j = -r; j <= r; j++)
                        {
                            if (H[y + i, x + j] > currentValue)
                            {
                                currentValue = 0;
                                break;
                            }
                        }
                    }

                    // check if this point is really interesting
                    if (currentValue != 0)
                    {
                        cornersList.Add(new IntPoint(x, y));
                    }
                }
            }


            return cornersList;
        }

        /// <summary>
        /// Process image looking for corners.
        /// </summary>
        /// 
        /// <param name="imageData">Source image data to process.</param>
        /// 
        /// <returns>Returns list of found corners (X-Y coordinates).</returns>
        /// 
        /// <exception cref="UnsupportedImageFormatException">
        ///   The source image has incorrect pixel format.
        /// </exception>
        /// 
        public List<IntPoint> ProcessImage(BitmapData imageData)
        {
            return ProcessImage(new UnmanagedImage(imageData));
        }

        /// <summary>
        /// Process image looking for corners.
        /// </summary>
        /// 
        /// <param name="image">Source image data to process.</param>
        /// 
        /// <returns>Returns list of found corners (X-Y coordinates).</returns>
        /// 
        /// <exception cref="UnsupportedImageFormatException">
        ///   The source image has incorrect pixel format.
        /// </exception>
        /// 
        public List<IntPoint> ProcessImage(Bitmap image)
        {
            // check image format
            if (
                (image.PixelFormat != PixelFormat.Format8bppIndexed) &&
                (image.PixelFormat != PixelFormat.Format24bppRgb) &&
                (image.PixelFormat != PixelFormat.Format32bppRgb) &&
                (image.PixelFormat != PixelFormat.Format32bppArgb)
                )
            {
                throw new UnsupportedImageFormatException("Unsupported pixel format of the source");
            }

            // lock source image
            BitmapData imageData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            List<IntPoint> corners;

            try
            {
                // process the image
                corners = ProcessImage(new UnmanagedImage(imageData));
            }
            finally
            {
                // unlock image
                image.UnlockBits(imageData);
            }

            return corners;
        }

    }
}
