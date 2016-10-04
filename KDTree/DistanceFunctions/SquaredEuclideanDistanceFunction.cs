namespace KDSharp.DistanceFunctions
{
    using System;
    
    /// <summary>
    /// A distance function for our KD-Tree which returns squared euclidean distances.
    /// </summary>
    public class SquaredEuclideanDistanceFunction : IDistanceFunction
    {
        /// <summary>
        /// Find the squared distance between two n-dimensional points.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <returns>The n-dimensional squared distance.</returns>
        public double Distance(double[] point1, double[] point2)
        {
            double sum = 0;
            for (int i = 0; i < point1.Length; i++)
            {
                double difference = (point1[i] - point2[i]);
                sum += difference * difference;
            }
            return sum;
        }

        /// <summary>
        /// Find the shortest distance from a point to an axis aligned rectangle in n-dimensional space.
        /// </summary>
        /// <param name="point">The point of interest.</param>
        /// <param name="minimum">The minimum coordinate of the rectangle.</param>
        /// <param name="maximum">The maximum coorindate of the rectangle.</param>
        /// <returns>The shortest squared n-dimensional squared distance between the point and rectangle.</returns>
        public double DistanceToRectangle(double[] point, double[] minimum, double[] maximum)
        {
            double sum = 0;
            double difference = 0;
            for (int i = 0; i < point.Length; ++i)
            {
                difference = 0;
                if (point[i] > maximum[i])
                    difference = (point[i] - maximum[i]);
                else if (point[i] < minimum[i])
                    difference = (point[i] - minimum[i]);
                sum += difference * difference;
            }
            return sum;
        }
    }
}
