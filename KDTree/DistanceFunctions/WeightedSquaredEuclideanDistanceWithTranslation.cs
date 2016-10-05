namespace KDTree.DistanceFunctions
{
	using System;
	
    /// <summary>
    /// A distance function for our KD-Tree which returns squared euclidean distances with translation movement and weight ratio.
    /// Vector Array Storage
    /// - Position : [0..Dimensions-1]
    /// - Translation Vector : [Dimensions..Dimensions*2-1] (Length is speed in unit/timeunit)
    /// - Start Time : [Dimensions*2]
    /// </summary>
	public sealed class WeightedSquaredEuclideanDistanceWithTranslation
	{
    	/// <summary>
    	/// Get Current Time Tick.
    	/// </summary>
        Func<double> GetTime { get; set; }
        /// <summary>
        /// Size of Position/Translation Vector.
        /// </summary>
        int Dimensions { get; set; }        
		/// <summary>
		/// Weight Ratio for each dimensions.
		/// </summary>
		readonly double[] Weights;

		/// <summary>
        /// Create a new Instance of <see cref="WeightedSquaredEuclideanDistanceWithTranslation"/> with given weights ratio.
        /// </summary>
        /// <param name="GetTime">GetTime Function to retrieve Translated Position.</param>
        /// <param name="Dimensions">Number of Dimensions for Position and Movement Vector.</param>
        /// <param name="Weights">Weights Ratio Array for each Dimensions.</param>
        public WeightedSquaredEuclideanDistanceWithTranslation(Func<double> GetTime, int Dimensions, double[] Weights)
        {
        	if (GetTime == null)
        		throw new ArgumentNullException("GetTime");
        	if (Dimensions < 1)
        		throw new ArgumentException("Dimensions count must be at least 1.", "Dimensions");
        	if (Weights == null)
        		throw new ArgumentNullException("Weights");
        	if (Weights.Length != Dimensions)
        		throw new ArgumentException("Weights Length must be the same as Dimensions count.", "Weights");

        	this.GetTime = GetTime;
            this.Dimensions = Dimensions;
 			this.Weights = new double[Weights.Length];
			Array.Copy(Weights, this.Weights, this.Weights.Length);
        }
        
        /// <summary>
        /// Find the squared distance between two n-dimensional points with translation vector.
        /// </summary>
        /// <param name="point1">The first point.</param>
        /// <param name="point2">The second point.</param>
        /// <returns>The n-dimensional squared distance.</returns>
        public double Distance(double[] point1, double[] point2)
        {
            var currentTime = GetTime();
            var elapsed1 = currentTime - point1[Dimensions*2];
            var elapsed2 = currentTime - point2[Dimensions*2];
            
            var position1 = new double[Dimensions];
            var position2 = new double[Dimensions];
            
            for (int d = 0 ; d < Dimensions ; ++d)
            {
                position1[d] = point1[d] + (point1[d+Dimensions] * elapsed1);
                position2[d] = point2[d] + (point2[d+Dimensions] * elapsed2);
            }

            double sum = 0;
            for (int i = 0; i < Dimensions; ++i)
            {
            	double difference = (position1[i] - position2[i]) * Weights[i];
                sum += difference * difference;
            }
            return sum;
        }

        /// <summary>
        /// Find the shortest distance from a point to an axis aligned rectangle in n-dimensional space with translation vector.
        /// </summary>
        /// <param name="point">The point of interest.</param>
        /// <param name="minimum">The minimum coordinate of the rectangle.</param>
        /// <param name="maximum">The maximum coorindate of the rectangle.</param>
        /// <returns>The shortest squared n-dimensional squared distance between the point and rectangle.</returns>
        public double DistanceToRectangle(double[] point, double[] minimum, double[] maximum)
        {
            var currentTime = GetTime();
            var elapsed = currentTime - point[Dimensions*2];
            var elapsedmin = currentTime - minimum[Dimensions*2];
            var elapsedmax = currentTime - maximum[Dimensions*2];
            
            var pos = new double[Dimensions];
            var posmin = new double[Dimensions];
            var posmax = new double[Dimensions];
            
            for (var d = 0 ; d < Dimensions ; ++d)
            {
                pos[d] = point[d] + (point[d+Dimensions] * elapsed);
                posmin[d] = minimum[d] + (minimum[d+Dimensions] * elapsedmin);
                posmax[d] = maximum[d] + (maximum[d+Dimensions] * elapsedmax);
            }

            double sum = 0;
            double difference = 0;
            for (int i = 0; i < Dimensions; ++i)
            {
                difference = 0;
                if (pos[i] > posmax[i])
                    difference = (pos[i] - posmax[i]) * Weights[i];
                else if (pos[i] < posmin[i])
                    difference = (pos[i] - posmin[i]) * Weights[i];
                sum += difference * difference;
            }
            return sum;
        }
	}
}
