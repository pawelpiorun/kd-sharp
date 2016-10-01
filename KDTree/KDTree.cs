namespace KDTree
{
    using System;
    using System.Linq;
    using KDTree.DistanceFunctions;

    /// <summary>
    /// A KDTree class represents the root of a variable-dimension KD-Tree.
    /// </summary>
    /// <typeparam name="T">The generic data type we want this tree to contain.</typeparam>
    /// <remarks>This is based on this: https://bitbucket.org/rednaxela/knn-benchmark/src/tip/ags/utils/dataStructures/trees/thirdGenKD/ </remarks>
    public class KDTree<T> : KDNode<T>
    {
        public const int DEFAULT_BUCKET_CAPACITY = 24;
        
        
        private DistanceFunction DistanceFunction { get; set; }
        
        /// <summary>
        /// Create a new KD-Tree given a number of dimensions.
        /// </summary>
        /// <param name="iDimensions">The number of data sorting dimensions. i.e. 3 for a 3D point.</param>
        public KDTree(int iDimensions)
            : this(new SquaredEuclideanDistanceFunction(), iDimensions, DEFAULT_BUCKET_CAPACITY)
        {
        }

        /// <summary>
        /// Create a new KD-Tree given a number of dimensions and initial bucket capacity.
        /// </summary>
        /// <param name="iDimensions">The number of data sorting dimensions. i.e. 3 for a 3D point.</param>
        /// <param name="iBucketCapacity">The default number of items that can be stored in each node.</param>
        public KDTree(int iDimensions, int iBucketCapacity)
            : this(new SquaredEuclideanDistanceFunction(), iDimensions, iBucketCapacity)
        {
        }
        
        /// <summary>
        /// Create a new instance of <see cref="KDTree{T}"/> given a number of dimensions, an initial bucket capacity and a <see cref="DistanceFunctions.DistanceFunction"/>.
        /// </summary>
        /// <param name="DistanceFunction"></param>
        /// <param name="iDimensions"></param>
        /// <param name="iBucketCapacity"></param>
        public KDTree(DistanceFunction DistanceFunction, int iDimensions, int iBucketCapacity)
            : base(iDimensions, iBucketCapacity)
        {
            this.DistanceFunction = DistanceFunction;
        }

        /// <summary>
        /// Get the nearest neighbours to a point in the kd tree using a square euclidean distance function.
        /// </summary>
        /// <param name="tSearchPoint">The point of interest.</param>
        /// <param name="iMaxReturned">The maximum number of points which can be returned by the iterator.</param>
        /// <param name="fDistance">A threshold distance to apply.  Optional.  Negative values mean that it is not applied.</param>
        /// <returns>A new nearest neighbour iterator with the given parameters.</returns>
        public NearestNeighbour<T> NearestNeighbors(double[] tSearchPoint, int iMaxReturned, double fDistance = -1)
        {
            return NearestNeighbors(DistanceFunction, tSearchPoint, iMaxReturned, fDistance);
        }

        /// <summary>
        /// Get the nearest neighbours to a point in the kd tree using a user defined distance function.
        /// </summary>
        /// <param name="tSearchPoint">The point of interest.</param>
        /// <param name="iMaxReturned">The maximum number of points which can be returned by the iterator.</param>
        /// <param name="kDistanceFunction">The distance function to use.</param>
        /// <param name="fDistance">A threshold distance to apply.  Optional.  Negative values mean that it is not applied.</param>
        /// <returns>A new nearest neighbour iterator with the given parameters.</returns>
        public NearestNeighbour<T> NearestNeighbors(DistanceFunction kDistanceFunction, double[] tSearchPoint, int iMaxReturned, double fDistance)
        {
            return new NearestNeighbour<T>(this, tSearchPoint, kDistanceFunction, iMaxReturned, fDistance);
        }
    }
}