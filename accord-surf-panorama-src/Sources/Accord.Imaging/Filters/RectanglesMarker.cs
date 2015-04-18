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
    ///   Filter to mark (highlight) rectangles in a image.
    /// </summary>
    /// 
    public class RectanglesMarker : BaseInPlaceFilter
    {
        private Color markerColor = Color.White;
        private Rectangle[] rectangles;
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
        ///   The set of rectangles.
        /// </summary>
        public Rectangle[] Rectangles
        {
            get { return rectangles; }
            set { rectangles = value; }
        }


        /// <summary>
        ///   Format translations dictionary.
        /// </summary>
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return formatTranslations; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RectanglesMarker"/> class.
        /// </summary>
        /// 
        /// <param name="rectangles">Set of rectangles to be drawn.</param>
        /// 
        public RectanglesMarker(params Rectangle[] rectangles)
            : this(rectangles, Color.White)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RectanglesMarker"/> class.
        /// </summary>
        /// 
        /// <param name="rectangle">A rectangle to be drawn.</param>
        /// 
        public RectanglesMarker(Rectangle rectangle)
            : this(new Rectangle[] { rectangle }, Color.White)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RectanglesMarker"/> class.
        /// </summary>
        /// 
        /// <param name="rectangles">Set of rectangles to be drawn.</param>
        /// <param name="markerColor">The color to use to drawn the rectangles.</param>
        /// 
        public RectanglesMarker(Rectangle[] rectangles, Color markerColor)
        {
            this.rectangles = rectangles;
            this.markerColor = markerColor;

            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
        }

        /// <summary>
        ///   Applies the filter to the image.
        /// </summary>
        protected override void ProcessFilter(UnmanagedImage image)
        {
            // mark all rectangular regions
            for (int i = 0; i < rectangles.Length; i++)
            {
                Drawing.Rectangle(image, rectangles[i], markerColor);
            }
        }
    }
}