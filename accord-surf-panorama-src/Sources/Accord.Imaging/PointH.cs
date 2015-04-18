// Accord Imaging Library
// Accord.NET framework
// http://www.crsouza.com
//
// Copyright © César Souza, 2009-2010
// cesarsouza at gmail.com
//

namespace Accord.Imaging
{
    using System.Drawing;
    using System;

    /// <summary>
    ///   Represents an ordered pair of real x- and y-coordinates and scalar w that defines
    ///   a point in a two-dimensional plane using homogeneous coordinates.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    ///   In mathematics, homogeneous coordinates are a system of coordinates used in
    ///   projective geometry much as Cartesian coordinates are used in Euclidean geometry.</para>
    /// <para>
    ///   They have the advantage that the coordinates of a point, even those at infinity,
    ///   can be represented using finite coordinates. Often formulas involving homogeneous
    ///   coordinates are simpler and more symmetric than their Cartesian counterparts.</para>
    /// <para>
    ///   Homogeneous coordinates have a range of applications, including computer graphics,
    ///   where they allow affine transformations and, in general, projective transformations
    ///   to be easily represented by a matrix.</para>
    ///   
    /// <para>
    ///   References: 
    ///   <list type="bullet">
    ///     <item><description>
    ///       http://alumnus.caltech.edu/~woody/docs/3dmatrix.html</description></item>
    ///     <item><description>
    ///       http://simply3d.wordpress.com/2009/05/29/homogeneous-coordinates/</description></item>
    ///   </list></para>
    /// </remarks>
    /// 
    public struct PointH
    {
        private float px, py, pw;

        /// <summary>
        ///   The first coordinate.
        /// </summary>
        public float X
        {
            get { return px; }
            set { px = value; }
        }

        /// <summary>
        ///   The second coordinate.
        /// </summary>
        public float Y
        {
            get { return py; }
            set { py = value; }
        }

        /// <summary>
        ///   The inverse scaling factor for X and Y.
        /// </summary>
        public float W
        {
            get { return pw; }
            set { pw = value; }
        }

        /// <summary>
        ///   Creates a new point.
        /// </summary>
        public PointH(float x, float y)
        {
            px = x;
            py = y;
            pw = 1;
        }

        /// <summary>
        ///   Creates a new point.
        /// </summary>
        public PointH(float x, float y, float w)
        {
            px = x;
            py = y;
            pw = w;
        }

        /// <summary>
        ///   Transforms a point using a projection matrix.
        /// </summary>
        public void Transform(float[,] matrix)
        {
            px = matrix[0, 0] * px + matrix[0, 1] * py + matrix[0, 2] * pw;
            py = matrix[1, 0] * px + matrix[1, 1] * py + matrix[1, 2] * pw;
            pw = matrix[2, 0] * px + matrix[2, 1] * py + matrix[2, 2] * pw;
        }

        /// <summary>
        ///   Normalizes the point to have unit scale.
        /// </summary>
        public void Normalize()
        {
            px = px / pw;
            py = py / pw;
            pw = 1;
        }

        /// <summary>
        ///   Gets whether this point is normalized (w = 1).
        /// </summary>
        public bool IsNormalized
        {
            get { return pw == 1f; }
        }

        /// <summary>
        ///   Gets whether this point is at infinity (w = 0).
        /// </summary>
        public bool IsAtInfinity
        {
            get { return pw == 0f; }
        }

        /// <summary>
        ///   Gets whether this point is at the origin.
        /// </summary>
        public bool IsEmpty
        {
            get { return px == 0 && py == 0; }
        }

        /// <summary>
        ///   Converts the point to a array representation.
        /// </summary>
        public double[] ToArray()
        {
            return new double[] { px, py, pw };
        }

        /// <summary>
        ///   Multiplication by scalar.
        /// </summary>
        public static PointH operator *(PointH a, float b)
        {
            return new PointH(b * a.X, b * a.Y, b * a.W);
        }

        /// <summary>
        ///   Multiplication by scalar.
        /// </summary>
        public static PointH operator *(float b, PointH a)
        {
            return a * b;
        }

        /// <summary>
        ///   Subtraction.
        /// </summary>
        public static PointH operator -(PointH a, PointH b)
        {
            return new PointH(a.X - b.X, a.Y - b.Y, a.W - b.W);
        }

        /// <summary>
        ///   Addition.
        /// </summary>
        public static PointH operator +(PointH a, PointH b)
        {
            return new PointH(a.X + b.X, a.Y + b.Y, a.W + b.W);
        }

        /// <summary>
        ///   Equality
        /// </summary>
        public static bool operator ==(PointH a, PointH b)
        {
            return (a.px / a.pw == b.px / b.pw && a.py / a.pw == b.py / b.pw);
        }

        /// <summary>
        ///   Inequality
        /// </summary>
        public static bool operator !=(PointH a, PointH b)
        {
            return (a.px / a.pw != b.px / b.pw || a.py / a.pw != b.py / b.pw);
        }

        /// <summary>
        ///   PointF Conversion
        /// </summary>
        public static implicit operator PointF(PointH a)
        {
            return new PointF((float)(a.px / a.pw), (float)(a.py / a.pw));
        }

        /// <summary>
        ///   Converts to a Integer point by computing the ceiling of the point coordinates. 
        /// </summary>
        public static Point Ceiling(PointH point)
        {
            return new Point(
                (int)System.Math.Ceiling(point.px / point.pw),
                (int)System.Math.Ceiling(point.py / point.pw));
        }

        /// <summary>
        ///   Converts to a Integer point by rounding the point coordinates. 
        /// </summary>
        public static Point Round(PointH point)
        {
            return new Point(
                (int)System.Math.Round(point.px / point.pw),
                (int)System.Math.Round(point.py / point.pw));
        }

        /// <summary>
        ///   Converts to a Integer point by truncating the point coordinates. 
        /// </summary>
        public static Point Truncate(PointH point)
        {
            return new Point(
                (int)System.Math.Truncate(point.px / point.pw),
                (int)System.Math.Truncate(point.py / point.pw));
        }

        /// <summary>
        ///   Compares two objects for equality.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is PointH)
            {
                PointH p = (PointH)obj;
                if (px / pw == p.px / p.pw &&
                    py / pw == p.py / p.pw)
                    return true;
            }

            return false;
        }

        /// <summary>
        ///   Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return px.GetHashCode() ^ py.GetHashCode() ^ pw.GetHashCode();
        }



        /// <summary>
        ///   Returns the empty point.
        /// </summary>
        public static readonly PointH Empty = new PointH(0, 0, 1);
    }
}
