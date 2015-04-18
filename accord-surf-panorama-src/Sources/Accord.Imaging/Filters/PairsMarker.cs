// Accord Imaging Library
// Accord.NET framework
// http://www.crsouza.com
//
// Copyright © César Souza, 2009-2010
// cesarsouza at gmail.com
//

namespace Accord.Imaging.Filters
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using AForge.Imaging.Filters;
    using AForge;
    using AForge.Imaging;

    /// <summary>
    ///   Filter to mark (highlight) pairs of points in a image.
    /// </summary>
    /// 
    public class PairsMarker : BaseInPlaceFilter
    {
        private Color markerColor = Color.White;
        private IntPoint[] points1;
        private IntPoint[] points2;
        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();


        /// <summary>
        /// Color used to mark pairs.
        /// </summary>
        public Color MarkerColor
        {
            get { return markerColor; }
            set { markerColor = value; }
        }

        /// <summary>
        ///   The first set of points.
        /// </summary>
        public IntPoint[] Points1
        {
            get { return points1; }
            set { points1 = value; }
        }

        /// <summary>
        ///   The corresponding points to the first set of points.
        /// </summary>
        public IntPoint[] Points2
        {
            get { return points2; }
            set { points2 = value; }
        }

        /// <summary>
        ///   Format translations dictionary.
        /// </summary>
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return formatTranslations; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PairsMarker"/> class.
        /// </summary>
        /// 
        /// <param name="points1">Set of starting points.</param>
        /// <param name="points2">Set of corresponding points.</param>
        /// 
        public PairsMarker(IntPoint[] points1, IntPoint[] points2)
            : this(points1, points2, Color.White)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PairsMarker"/> class.
        /// </summary>
        /// 
        /// <param name="points1">Set of starting points.</param>
        /// <param name="points2">Set of corresponding points.</param>
        /// <param name="markerColor">The color of the lines to be marked.</param>
        /// 
        public PairsMarker(IntPoint[] points1, IntPoint[] points2, Color markerColor)
        {
            this.points1 = points1;
            this.points2 = points2;
            this.markerColor = markerColor;

            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
        }

        /// <summary>
        /// Process the filter on the specified image.
        /// </summary>
        /// 
        /// <param name="image">Source image data.</param>
        ///
        protected override void ProcessFilter(UnmanagedImage image)
        {
            // mark all lines
            for (int i = 0; i < points1.Length; i++)
            {
                Drawing.Line(image, points1[i], points2[i], markerColor);
            }
        }
    }
}