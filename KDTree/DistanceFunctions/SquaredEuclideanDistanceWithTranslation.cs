namespace KDTree.DistanceFunctions
{
    using System;
    
    /// <summary>
    /// A distance function for our KD-Tree which returns squared euclidean distances with translation movement.
    /// Vector Array Storage
    /// - Position : [0..Dimensions-1]
    /// - Translation Vector : [Dimensions..Dimensions*2-1] (Length is speed in unit/timeunit)
    /// - Start Time : [Dimensions*2]
    /// </summary>
    public class SquaredEuclideanDistanceWithTranslation : DistanceFunction
    {
        private Func<double> GetTime { get; set; }
        private int Dimensions { get; set; }
        
        /// <summary>
        /// Create a new Instance of <see cref="SquaredEuclideanDistanceWithTranslation"/>
        /// </summary>
        /// <param name="GetTime">GetTime Function to retrieve Translated Position</param>
        /// <param name="Dimensions">Number of Dimensions for Position and Movement Vector</param>
        public SquaredEuclideanDistanceWithTranslation(Func<double> GetTime, int Dimensions)
        {
            this.GetTime = GetTime;
            this.Dimensions = Dimensions;
        }
        
        /// <summary>
        /// Find the squared distance between two n-dimensional points with translation vector.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <returns>The n-dimensional squared distance.</returns>
        public double Distance(double[] p1, double[] p2)
        {
            var currentTime = GetTime();
            var elapsed1 = currentTime - p1[Dimensions*2];
            var elapsed2 = currentTime - p2[Dimensions*2];
            
            var pos1 = new double[Dimensions];
            var pos2 = new double[Dimensions];
            
            for (int d = 0 ; d < Dimensions ; ++d)
            {
                pos1[d] = p1[d] + (p1[d+Dimensions] * elapsed1);
                pos2[d] = p2[d] + (p2[d+Dimensions] * elapsed2);
            }

            double fSum = 0;
            for (int i = 0; i < Dimensions; ++i)
            {
                double fDifference = (pos1[i] - pos2[i]);
                fSum += fDifference * fDifference;
            }
            return fSum;
        }

        /// <summary>
        /// Find the shortest distance from a point to an axis aligned rectangle in n-dimensional space with translation vector.
        /// </summary>
        /// <param name="point">The point of interest.</param>
        /// <param name="min">The minimum coordinate of the rectangle.</param>
        /// <param name="max">The maximum coorindate of the rectangle.</param>
        /// <returns>The shortest squared n-dimensional squared distance between the point and rectangle.</returns>
        public double DistanceToRectangle(double[] point, double[] min, double[] max)
        {
            var currentTime = GetTime();
            var elapsed = currentTime - point[Dimensions*2];
            var elapsedmin = currentTime - min[Dimensions*2];
            var elapsedmax = currentTime - max[Dimensions*2];
            
            var pos = new double[Dimensions];
            var posmin = new double[Dimensions];
            var posmax = new double[Dimensions];
            
            for (var d = 0 ; d < Dimensions ; ++d)
            {
                pos[d] = point[d] + (point[d+Dimensions] * elapsed);
                posmin[d] = min[d] + (min[d+Dimensions] * elapsedmin);
                posmax[d] = max[d] + (max[d+Dimensions] * elapsedmax);
            }

            double fSum = 0;
            double fDifference = 0;
            for (int i = 0; i < Dimensions; ++i)
            {
                fDifference = 0;
                if (pos[i] > posmax[i])
                    fDifference = (pos[i] - posmax[i]);
                else if (pos[i] < posmin[i])
                    fDifference = (pos[i] - posmin[i]);
                fSum += fDifference * fDifference;
            }
            return fSum;
        }
    }
}
