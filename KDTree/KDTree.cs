namespace KDSharp.KDTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KDSharp.DistanceFunctions;

    /// <summary>
    /// A KDTree class represents the root of a variable-dimension KD-Tree.
    /// </summary>
    /// <typeparam name="T">The generic data type we want this tree to contain.</typeparam>
    /// <remarks>This is based on this: https://bitbucket.org/rednaxela/knn-benchmark/src/tip/ags/utils/dataStructures/trees/thirdGenKD/ </remarks>
    public class KDTree<T> : IList<T>
    {
        /// <summary>
        /// Constant Default Bucket Capacity.
        /// </summary>
        public const int DEFAULT_BUCKET_CAPACITY = 24;
        
        /// <summary>
        /// Distance Implementation for default Nearest Neighbour Search.
        /// </summary>
        IDistanceFunction DistanceFunction { get; set; }
        
        /// <summary>
        /// Objects Stored in the Tree.
        /// </summary>
        T[] Data;
        
        /// <summary>
        /// Objects Locations Stored in the Tree.
        /// </summary>
        double[][] Points;

        /// <summary>
        /// Next Index to Insert Object.
        /// </summary>
        int DataSize;
        
        /// <summary>
        /// Available Indices for Data Array Holes.
        /// </summary>
        SortedList<int, int> AvailableIndices;
        
        /// <summary>
        /// Tree Root Node.
        /// </summary>
        readonly KDTreeNode Root;
        
        /// <summary>
        /// Get this Tree Root Node.
        /// </summary>
        public KDTreeNode RootNode { get { return Root; } }
        
        /// <summary>
        /// Retrieve the Tree Dimension Count.
        /// </summary>
        public int Dimensions { get { return Root.Dimensions; } }
        
        /// <summary>
        /// Count of Element Removed From Nodes, High number can lead to Unbalanced Tree.
        /// </summary>
        public uint RemovalCount { get; private set; }
        
        /// <summary>
        /// Create a new KD-Tree given a number of dimensions.
        /// </summary>
        /// <param name="Dimensions">The number of data sorting dimensions. i.e. 3 for a 3D point.</param>
        public KDTree(int Dimensions)
            : this(new SquaredEuclideanDistanceFunction(), Dimensions, DEFAULT_BUCKET_CAPACITY)
        {
        }

        /// <summary>
        /// Create a new KD-Tree given a number of dimensions and initial bucket capacity.
        /// </summary>
        /// <param name="Dimensions">The number of data sorting dimensions. i.e. 3 for a 3D point.</param>
        /// <param name="BucketCapacity">The default number of items that can be stored in each node.</param>
        public KDTree(int Dimensions, int BucketCapacity)
            : this(new SquaredEuclideanDistanceFunction(), Dimensions, BucketCapacity)
        {
        }
        
        /// <summary>
        /// Create a new instance of <see cref="KDTree{T}"/> given a number of dimensions and a <see cref="DistanceFunctions.IDistanceFunction"/>.
        /// </summary>
        /// <param name="DistanceFunction">The Default Distance Function to use for NearestNeighbours.</param>
        /// <param name="Dimensions">The number of data sorting dimensions. i.e. 3 for a 3D point.</param>
        public KDTree(IDistanceFunction DistanceFunction, int Dimensions)
            : this(DistanceFunction, Dimensions, DEFAULT_BUCKET_CAPACITY)
        {
        }
        
        /// <summary>
        /// Create a new instance of <see cref="KDTree{T}"/> given a number of dimensions, an initial bucket capacity and a <see cref="DistanceFunctions.IDistanceFunction"/>.
        /// </summary>
        /// <param name="DistanceFunction">The Default Distance Function to use for NearestNeighbours.</param>
        /// <param name="Dimensions">The number of data sorting dimensions. i.e. 3 for a 3D point.</param>
        /// <param name="BucketCapacity">The default number of items that can be stored in each node.</param>
        public KDTree(IDistanceFunction DistanceFunction, int Dimensions, int BucketCapacity)
        {
            this.DistanceFunction = DistanceFunction;
            this.Data = new T[BucketCapacity];
            this.Points = new double[BucketCapacity][];
            this.Root = new KDTreeNode(Dimensions, BucketCapacity);
            this.DataSize = 0;
            this.AvailableIndices = new SortedList<int, int>();
        }

        #region Tree Methods
        /// <summary>
        /// Add Object with Point Position in Tree.
        /// </summary>
        /// <param name="Point">Position of the Object.</param>
        /// <param name="Value">Object to Add.</param>
        public void AddPoint(double[] Point, T Value)
        {
            if (Point.Length != Dimensions)
                throw new ArgumentException(string.Format("Point should have same lenght as Tree Dimensions ({0})...", Dimensions), "Point");
            
            var point = new double[Point.Length];
            Array.Copy(Point, point, Point.Length);
            
            // Check if Data Array have Holes
            if (AvailableIndices.Count > 0)
            {
                var last = AvailableIndices.Count - 1;
                var index = AvailableIndices.Values[last];
                
                Data[index] = Value;
                Points[index] = point;
                Root.AddPoint(index, Points);
                AvailableIndices.RemoveAt(last);
            }
            else
            {
                var index = DataSize;
                if (index >= Data.Length)
                    ResizeData();
                
                Data[index] = Value;
                Points[index] = point;
                Root.AddPoint(index, Points);
                ++DataSize;
            }
        }
        
        /// <summary>
        /// Remove Object From Tree.
        /// </summary>
        /// <param name="Value">Object to Remove.</param>
        /// <returns>true if Object is found.</returns>
        public bool Remove(T Value)
        {
            var idx = IndexOf(Value);
            
            if (idx > -1)
            {
                RemoveAt(idx);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Move Point in tree to a new Position.
        /// </summary>
        /// <param name="Point">New Position to be Moved to.</param>
        /// <param name="Value">Object to Move.</param>
        /// <returns>true if Object is found.</returns>
        public bool MovePoint(double[] Point, T Value)
        {
            if (Point.Length != Dimensions)
                throw new ArgumentException(string.Format("Point should have same length as Tree Dimensions ({0})...", Dimensions), "Point");

            var idx = IndexOf(Value);
            
            if (idx > -1)
            {
                var oldPoint = new double[Point.Length];
                Array.Copy(Points[idx], oldPoint, Point.Length);
                Array.Copy(Point, Points[idx], Point.Length);
                if (!Root.MovePoint(oldPoint, idx, Points))
                    ++RemovalCount;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get Point Position.
        /// </summary>
        /// <param name="Value">Object to Get Position From.</param>
        /// <returns>Position Point or null if not found.</returns>
        public double[] GetPoint(T Value)
        {
            var idx = IndexOf(Value);
            if (idx > -1)
            {
                var result = new double[Dimensions];
                Array.Copy(Points[idx], result, Dimensions);
                return result;
            }
            
            return null;
        }
        
        /// <summary>
        /// Get Point Poisition At Index.
        /// </summary>
        /// <param name="Index">Index to Get Position At.</param>
        /// <returns>Position Point.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Index does not match a recorded item.</exception>
        public double[] GetPointAt(int Index)
        {
            if (Index < 0 || Index >= DataSize || AvailableIndices.ContainsValue(Index))
                throw new ArgumentOutOfRangeException("Index", string.Format("Given Index ({0}) is not valued", Index));
            
            var result = new double[Dimensions];
            Array.Copy(Points[Index], result, Dimensions);
            return result;
        }
        
        /// <summary>
        /// Regenerate Tree from Current Collection.
        /// Can Solve Unbalancing when moving Data between nodes.
        /// </summary>
        public void Regen()
        {
            Root.Clear();
            RemovalCount = 0;
            
            var hole = 0;
            for (int i = AvailableIndices.Count - 1 ; i > -1 ; --i)
            {
                var nextHole = AvailableIndices.Values[i];
                
                while (hole < nextHole)
                    Root.AddPoint(hole++, Points);
                
                ++hole;
            }
            
            while (hole < DataSize)
                Root.AddPoint(hole++, Points);
            
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Resize Data Array Increment by Bucket Size * 2.
        /// </summary>
        void ResizeData()
        {
            var newSize = Data.Length + Root.BucketCapacity * 2;
            Array.Resize<T>(ref Data, newSize);
            Array.Resize<double[]>(ref Points, newSize);
        }
        #endregion
        
        #region ICollection Implementation
        public int Count
        {
            get
            {
                return Root.Size;
            }
        }

        public void Clear()
        {
            Root.Clear();
            Data = new T[Root.BucketCapacity];
            DataSize = 0;
            AvailableIndices = new SortedList<int, int>();
        }
        public bool Contains(T item)
        {
            return IndexOf(item) > -1;
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            // Segment Copy from Array Holes
            var hole = 0;
            
            for (int i = AvailableIndices.Count - 1 ; i > -1 ; --i)
            {
                var nextHole = AvailableIndices.Values[i];
                var lengthHole = nextHole - hole;
                Array.Copy(Data, hole, array, arrayIndex, lengthHole);
                hole = nextHole + 1;
                arrayIndex += lengthHole;
            }
            
            Array.Copy(Data, hole, array, arrayIndex, DataSize - hole);
        }        
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        public void Add(T item)
        {
            AddPoint(Enumerable.Repeat(default(double), Dimensions).ToArray(), item);
        }
        #endregion
        
        #region Enumerable Implementation
        public IEnumerator<T> GetEnumerator()
        {
            // Enumerate around Array Holes
            var valids = new int[DataSize-AvailableIndices.Count];
            var hole = 0;
            var idx = 0;
            
            for (int i = AvailableIndices.Count - 1 ; i > -1 ; --i)
            {
                var nextHole = AvailableIndices.Values[i];
                
                while (hole < nextHole)
                    valids[idx++] = hole++;
                
                ++hole;
            }
            
            while (hole < DataSize)
                valids[idx++] = hole++;
            
            return valids.Select(index => Data[index]).GetEnumerator();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        
        #region IList Implementation
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= DataSize || AvailableIndices.ContainsValue(index))
                    throw new ArgumentOutOfRangeException("index", string.Format("Given Index ({0}) is not valued", index));
                
                return Data[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        
        public int IndexOf(T item)
        {
            // Check only Valids Indices
            var hole = 0;
            for (int i = AvailableIndices.Count - 1 ; i > -1 ; --i)
            {
                var nextHole = AvailableIndices.Values[i];
                
                for (int j = hole ; j < nextHole ; ++j)
                {
                    if (Data[j].Equals(item))
                        return j;
                }
                
                hole = nextHole + 1;
            }
            
            for (int i = hole ; i < DataSize ; ++i)
            {
                if (Data[i].Equals(item))
                    return i;
            }
            
            return -1;
        }
        
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= DataSize || AvailableIndices.ContainsValue(index))
                throw new ArgumentOutOfRangeException("index", string.Format("Given Index ({0}) is not valued", index));

            Root.RemovePoint(index, Points[index]);
            Data[index] = default(T);
            Points[index] = null;
            ++RemovalCount;
            
            // Remove index from valid Indices
            if (index == DataSize - 1)
            {
                --DataSize;
                // Compact Data Collection
                while (AvailableIndices.Count > 0 && AvailableIndices.Values[0] == DataSize - 1)
                {
                    --DataSize;
                    AvailableIndices.RemoveAt(0);
                }
            }
            else
            {
                AvailableIndices.Add(-index, index);
            }
        }
        
        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }
        #endregion
        
        #region Nearest Neighbour
        /// <summary>
        /// Get the nearest neighbours to a point in the kd tree using a square euclidean distance function.
        /// </summary>
        /// <param name="SearchPoint">The point of interest.</param>
        /// <param name="MaxReturned">The maximum number of points which can be returned by the iterator.</param>
        /// <param name="Distance">A threshold distance to apply.  Optional.  Negative values mean that it is not applied.</param>
        /// <returns>A new nearest neighbour iterator with the given parameters.</returns>
        public NearestNeighbour<T> NearestNeighbors(double[] SearchPoint, int MaxReturned, double Distance = -1)
        {
            return NearestNeighbors(DistanceFunction, SearchPoint, MaxReturned, Distance);
        }

        /// <summary>
        /// Get the nearest neighbours to a point in the kd tree using a user defined distance function.
        /// </summary>
        /// <param name="DistanceFunction">The distance function to use.</param>
        /// <param name="SearchPoint">The point of interest.</param>
        /// <param name="MaxReturned">The maximum number of points which can be returned by the iterator.</param>
        /// <param name="Distance">A threshold distance to apply.  Optional.  Negative values mean that it is not applied.</param>
        /// <returns>A new nearest neighbour iterator with the given parameters.</returns>
        public NearestNeighbour<T> NearestNeighbors(IDistanceFunction DistanceFunction, double[] SearchPoint, int MaxReturned, double Distance)
        {
            return new NearestNeighbour<T>(this, SearchPoint, DistanceFunction, MaxReturned, Distance);
        }
        #endregion
    }
}