namespace KDTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KDTree.DistanceFunctions;

    /// <summary>
    /// A KDTree class represents the root of a variable-dimension KD-Tree.
    /// </summary>
    /// <typeparam name="T">The generic data type we want this tree to contain.</typeparam>
    /// <remarks>This is based on this: https://bitbucket.org/rednaxela/knn-benchmark/src/tip/ags/utils/dataStructures/trees/thirdGenKD/ </remarks>
    public class KDTree<T> : ICollection<T>
    {
        /// <summary>
        /// Constant Default Bucket Capacity.
        /// </summary>
        public const int DEFAULT_BUCKET_CAPACITY = 24;
        
        /// <summary>
        /// Distance Implementation for default Nearest Neighbour Search.
        /// </summary>
        DistanceFunction DistanceFunction { get; set; }
        
        /// <summary>
        /// Objects Stored in the Tree.
        /// </summary>
        T[] Data;
        
        /// <summary>
        /// Next Index to Insert Object.
        /// </summary>
        int NextIndex;
        
        /// <summary>
        /// Available Indices for Data Array Holes
        /// </summary>
        Stack<int> AvailableIndexes;
        
        /// <summary>
        /// Valid Indexes Only
        /// </summary>
        IEnumerable<int> ValidIndexes { get { return Enumerable.Range(0, NextIndex).Except(AvailableIndexes); } }
        
        /// <summary>
        /// Tree Root Node.
        /// </summary>
        internal readonly KDTreeNode Root;
        
        /// <summary>
        /// Retrieve Object from Index.
        /// </summary>
        internal T this[int index] { get { return Data[index]; } }
        
        /// <summary>
        /// Retrieve the Tree Object Count
        /// </summary>
        public int Size { get { return Root.Size; } }
        
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
        /// <param name="iDimensions">The number of data sorting dimensions. i.e. 3 for a 3D point.</param>
        /// <param name="iBucketCapacity">The default number of items that can be stored in each node.</param>
        public KDTree(DistanceFunction DistanceFunction, int iDimensions, int iBucketCapacity)
        {
            this.DistanceFunction = DistanceFunction;
            this.Data = new T[iBucketCapacity];
            this.Root = new KDTreeNode(iDimensions, iBucketCapacity);
            this.NextIndex = 0;
            this.AvailableIndexes = new Stack<int>();
        }

        #region Tree Methods
        /// <summary>
        /// Add Object with Point Position in Tree.
        /// </summary>
        /// <param name="Point">Position of the Object.</param>
        /// <param name="Value">Object to Add.</param>
        public void AddPoint(double[] Point, T Value)
        {
            // Check if Data Array have Hole
            if (AvailableIndexes.Count > 0)
            {
                var index = AvailableIndexes.Pop();
                Root.AddPoint(Point, index);
                Data[index] = Value;
            }
            else
            {
                if (NextIndex >= Data.Length)
                    ResizeData();
                
                var index = NextIndex++;
                Root.AddPoint(Point, index);
                Data[index] = Value;
            }
        }
        
        /// <summary>
        /// Remove Object From Tree.
        /// </summary>
        /// <param name="Value">Object to Remove.</param>
        /// <returns>true if Object is found.</returns>
        public bool RemovePoint(T Value)
        {
            var result = false;
            foreach(var index in ValidIndexes.Where(valid => Value.Equals(Data[valid])))
            {
                result = Root.RemovePoint(index);
                
                // Remove index from valid Indexes
                if (index == NextIndex - 1)
                    --NextIndex;
                else
                    AvailableIndexes.Push(index);
                
                break;
            }
            
            return result;
        }
        
        /// <summary>
        /// Move Point in tree to a new Position.
        /// </summary>
        /// <param name="Point">New Position to be Moved to.</param>
        /// <param name="Value">Object to Move.</param>
        /// <returns>true if Object is found.</returns>
        public bool MovePoint(double[] Point, T Value)
        {
            var result = false; 
            foreach(var index in ValidIndexes.Where(valid => Value.Equals(Data[valid])))
            {
                result = Root.MovePoint(Point, index);
                break;
            }
            
            return result;
        }
        
        /// <summary>
        /// Get Point Poisition.
        /// </summary>
        /// <param name="Value">Object to Get Position From.</param>
        /// <returns>Position Point or null if not found.</returns>
        public double[] GetPoint(T Value)
        {
            foreach(var index in ValidIndexes.Where(valid => Value.Equals(Data[valid])))
            {
                KDTreeNode node;
                var foundIndex = Root.GetPointNode(index, out node);
                return node.Points[foundIndex];
            }
            
            return null;
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Resize Data Array to double Size.
        /// </summary>
        void ResizeData()
        {
            Array.Resize<T>(ref Data, Data.Length * 2);
        }
        
        /// <summary>
        /// Clear the whole Tree.
        /// </summary>
        void ClearTree()
        {
            Root.Clear();
            Data = new T[Root.BucketCapacity];
            NextIndex = 0;
            AvailableIndexes = new Stack<int>();
        }
        #endregion
        
        #region ICollection Implementation
        public int Count { get { return Size; } }
        public bool IsReadOnly { get { return false; } }
        public void Add(T item) { throw new NotImplementedException(); }
        public void Add(double[] point, T item) { AddPoint(point, item); }
        public void Clear() { ClearTree(); }
        public bool Contains(T item) { throw new NotImplementedException(); }
        public void CopyTo(T[] array, int arrayIndex) { throw new NotImplementedException(); }
        public bool Remove(T item) { return RemovePoint(item); }
        #endregion
        
        #region Enumerable Implementation
        public IEnumerator<T> GetEnumerator()
        {
            return ValidIndexes.Select(index => Data[index]).GetEnumerator();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        
        #region Nearest Neighbour
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
        #endregion
    }
}