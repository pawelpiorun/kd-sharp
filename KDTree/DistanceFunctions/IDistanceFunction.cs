namespace KDSharp.DistanceFunctions
{
    using System;
    using System.Linq;

    /// <summary>
    /// An interface which enables flexible distance functions.
    /// </summary>
    public interface IDistanceFunction
    {
        /// <summary>
        /// Compute a distance between two n-dimensional points.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <returns>The n-dimensional distance.</returns>
        double Distance(double[] point1, double[] point2);

        /// <summary>
        /// Find the shortest distance from a point to an axis aligned rectangle in n-dimensional space.
        /// </summary>
        /// <param name="point">The point of interest.</param>
        /// <param name="minimum">The minimum coordinate of the rectangle.</param>
        /// <param name="maximum">The maximum coorindate of the rectangle.</param>
        /// <returns>The shortest n-dimensional distance between the point and rectangle.</returns>
        double DistanceToRectangle(double[] point, double[] minimum, double[] maximum);
    }
}
