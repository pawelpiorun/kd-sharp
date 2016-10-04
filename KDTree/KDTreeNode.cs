namespace KDSharp.KDTree
{
    using System;
    using System.Collections.Generic;
    
    /// <summary>
    /// A KD-Tree node which supports a generic number of dimensions.
    /// All data items need the same number of dimensions.
    /// This node splits based on the largest range of any dimension.
    /// </summary>
    /// <remarks>This is based on this: https://bitbucket.org/rednaxela/knn-benchmark/src/tip/ags/utils/dataStructures/trees/thirdGenKD/ </remarks>
    public sealed class KDTreeNode
    {
        #region Constructors
        public KDTreeNode(int Dimensions, int BucketCapacity)
        {
            // Error Checking
            if (BucketCapacity < 1)
                throw new ArgumentOutOfRangeException("BucketCapacity", BucketCapacity, "Initial Bucket Capacity must be at least 1.");
            if (Dimensions < 1)
                throw new ArgumentOutOfRangeException("Dimensions", BucketCapacity, "Node Dimensions Count must be at least 1.");
            
            // Variables.
            this.Dimensions = Dimensions;
            this.BucketCapacity = BucketCapacity;
            this.Size = 0;
            this.SinglePoint = true;

            // Setup leaf elements.
            this.DataIndices = new int[BucketCapacity];
        }
        #endregion
        
        #region Internals
        /// <summary>
        /// Initial Bucket Capacity.
        /// </summary>
        public readonly int BucketCapacity;
        /// <summary>
        /// Number of Dimensions.
        /// </summary>
        public readonly int Dimensions;
        
        /// <summary>
        /// Number of Items within the Tree.
        /// </summary>
        public int Size { get; private set; }
        
        /// <summary>
        /// Array of data indices.
        /// </summary>
        int[] DataIndices;
        
        /// <summary>
        /// Left and Right Children Nodes.
        /// </summary>
        public KDTreeNode Left { get; private set; }
        public KDTreeNode Right { get; private set; }
        /// <summary>
        /// Index of the Split Dimension.
        /// </summary>
        public int SplitDimension { get; private set; }
        /// <summary>
        /// Split Value on Split Dimension (larger go into the right, smaller go into left).
        /// </summary>
        public double SplitValue { get; private set; }
        
        /// <summary>
        /// Bounding Box for this Node on all Dimensions.
        /// </summary>
        double[] MinBound, MaxBound;
        
        /// <summary>
        /// Get Minimum Bound Point.
        /// </summary>
        public double[] MinimumBound
        {
            get
            {
                if (MinBound == null)
                    return null;
                
                var result = new double[MinBound.Length];
                Array.Copy(MinBound, result, MinBound.Length);
                return result;
            }
        }
        
        /// <summary>
        /// Get Maximum Bound Point.
        /// </summary>
        public double[] MaximumBound
        {
            get
            {
                if (MaxBound == null)
                    return null;
                
                var result = new double[MaxBound.Length];
                Array.Copy(MaxBound, result, MaxBound.Length);
                return result;
            }
        }
        
        /// <summary>
        /// Is this Node a Single Point ?
        /// </summary>
        public bool SinglePoint { get; private set; }
        
        /// <summary>
        /// Is this Node a Leaf ?
        /// </summary>
        public bool IsLeaf { get { return DataIndices != null; } }
        
        /// <summary>
        /// Get the Stored Data Index at given Index.
        /// </summary>
        public int this[int Index]
        {
            get
            {
                if (Index < 0 || Index >= Size)
                    throw new IndexOutOfRangeException(string.Format("Given Index ({0}) out of Range (0..{1})", Index, Size - 1));
                
                return DataIndices[Index];
            }
        }
        #endregion
        
        #region Operations
        /// <summary>
        /// Insert a new point from this node.
        /// </summary>
        /// <param name="Index">The index of the data.</param>
        /// <param name="Points">The points collection with Point at Index.</param>
        internal void AddPoint(int Index, double[][] Points)
        {
            // Find the correct leaf node.
            var cursor = this;
            var point = Points[Index];
            
            while (!cursor.IsLeaf)
            {
                // Extend the size of the leaf.
                cursor.ExtendBounds(Points[Index]);
                // Increment Size.
                ++cursor.Size;

                cursor = point[cursor.SplitDimension] > cursor.SplitValue ? cursor.Right : cursor.Left;
            }

            // Insert it into the leaf.
            cursor.AddLeafPoint(Index, Points);
        }
        
        /// <summary>
        /// Remove a Point from this Node.
        /// </summary>
        /// <param name="Index">The Data Index to Remove.</param>
        /// <param name="Point">The Data Index Point.</param>
        /// <returns>true if Index found and removed.</returns>
        internal bool RemovePoint(int Index, double[] Point)
        {
            var cursor = this;
            var walkPath = new Stack<KDTreeNode>();
            while (!cursor.IsLeaf)
            {
                walkPath.Push(cursor);
                cursor = Point[cursor.SplitDimension] > cursor.SplitValue ? cursor.Right : cursor.Left;
            }
            
            var idx = cursor.IndexOf(Index);
            
            if (idx > -1)
            {
                while (walkPath.Count > 0)
                    --walkPath.Pop().Size;
                
                // Shift Array Left
                Array.Copy(cursor.DataIndices, idx + 1, cursor.DataIndices, idx, cursor.Size - (idx + 1));
                --cursor.Size;
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Move a Point from this Node.
        /// </summary>
        /// <param name="OldPoint">The Old Point Position.</param>
        /// <param name="Index">Data Index to Search for Moving.</param>
        /// <param name="Points">The points collection with Point at Index.</param>
        /// <returns>true if the Data Index was moved inside the same node.</returns>
        internal bool MovePoint(double[] OldPoint, int Index, double[][] Points)
        {
            // Find the Old leaf node.
            var cursor = this;
            var walkPath = new Stack<KDTreeNode>();
            var point = Points[Index];
            
            while (!cursor.IsLeaf)
            {
                walkPath.Push(cursor);
                
                // Extends Bounds of each node on the path.
                cursor.ExtendBounds(point);
                cursor = point[cursor.SplitDimension] > cursor.SplitValue ? cursor.Right : cursor.Left;
            }
                        
            // Check if Item is in the target node
            var idx = cursor.IndexOf(Index);
            
            if (idx > -1)
            {
                cursor.ExtendBounds(point);
                return true;
            }
            
            RemovePoint(Index, OldPoint);                    
            cursor.AddLeafPoint(Index, Points);
            
            while (walkPath.Count > 0)
                ++walkPath.Pop().Size;
            
            return false;
        }
        
        /// <summary>
        /// Empty Tree.
        /// </summary>
        internal void Clear()
        {
            Size = 0;
            SinglePoint = true;

            DataIndices = new int[BucketCapacity];
            
            Right = null;
            Left = null;
            
            SplitDimension = 0;
            SplitValue = 0;
            
            MinBound = null;
            MaxBound = null;
        }
        #endregion 
        
        #region Insides
        /// <summary>
        /// Get First Index of Data Index-Value in this Node. 
        /// </summary>
        /// <param name="Index">Index Value to search.</param>
        /// <returns>Index position in this node or -1 if not found.</returns>
        int IndexOf(int Index)
        {
            if (!IsLeaf)
                return -1;
            
            for (int i = 0 ; i < Size ; ++i)
            {
                if (DataIndices[i] == Index)
                    return i;
            }
            
            return -1;
        }

        /// <summary>
        /// Extend this node to contain a new point.
        /// </summary>
        /// <param name="Point">The point to contain.</param>
        void ExtendBounds(double[] Point)
        {
            // If we don't have bounds, create them using the new point then bail.
            if (MinBound == null) 
            {
                MinBound = new double[Dimensions];
                MaxBound = new double[Dimensions];
                Array.Copy(Point, MinBound, Dimensions);
                Array.Copy(Point, MaxBound, Dimensions);
                return;
            }

            // For each dimension check if bound need expansion.
            for (int i = 0; i < Dimensions; ++i)
            {
                if (Double.IsNaN(Point[i]))
                {
                    if (!Double.IsNaN(MinBound[i]) || !Double.IsNaN(MaxBound[i]))
                        SinglePoint = false;
                    
                    MinBound[i] = Double.NaN;
                    MaxBound[i] = Double.NaN;
                }
                else if (MinBound[i] > Point[i])
                {
                    MinBound[i] = Point[i];
                    SinglePoint = false;
                }
                else if (MaxBound[i] < Point[i])
                {
                    MaxBound[i] = Point[i];
                    SinglePoint = false;
                }
            }
        }
        
        /// <summary>
        /// Insert a point into the leaf.
        /// </summary>
        /// <param name="Index">The index of the point.</param>
        /// <param name="Points">The points collection with Point at Index.</param>
        void AddLeafPoint(int Index, double[][] Points)
        {
            // Add the data point to this node.
            DataIndices[Size] = Index;
            ExtendBounds(Points[Index]);
            ++Size;

            // Split if the node is getting too large in terms of data.
            if (Size == DataIndices.Length)
            {
                // If the node is getting too physically large.
                if (CalculateSplit())
                {
                    // If the node successfully had it's split value calculated, split node.
                    SplitLeafNode(Points);
                }
                else
                {
                    // If the node could not be split, enlarge node data capacity.
                    IncreaseLeafCapacity();
                }
            }
        }
        
        /// <summary>
        /// Work out if this leaf node should split.  If it should, a new split value and dimension is calculated
        /// based on the dimension with the largest range.
        /// </summary>
        /// <returns>True if the node split, false if not.</returns>
        bool CalculateSplit()
        {
            // Don't split if we are just one point.
            if (SinglePoint)
                return false;

            // Find the dimension with the largest range.  This will be our split dimension.
            double fWidth = 0;
            for (int i = 0; i < Dimensions; i++)
            {
                double fDelta = (MaxBound[i] - MinBound[i]);
                if (Double.IsNaN(fDelta))
                    fDelta = 0;

                if (fDelta > fWidth)
                {
                    SplitDimension = i;
                    fWidth = fDelta;
                }
            }

            // If we are not wide (i.e. all the points are in one place), don't split.
            if (fWidth == 0)
                return false;

            // Split in the middle of the node along the widest dimension.
            SplitValue = (MinBound[SplitDimension] + MaxBound[SplitDimension]) * 0.5;

            // Never split on infinity or NaN.
            if (SplitValue == Double.PositiveInfinity)
                SplitValue = Double.MaxValue;
            else if (SplitValue == Double.NegativeInfinity)
                SplitValue = Double.MinValue;
            
            // Don't let the split value be the same as the upper value as
            // can happen due to rounding errors!
            if (SplitValue == MaxBound[SplitDimension])
                SplitValue = MinBound[SplitDimension];

            // Success
            return true;
        }
        
        /// <summary>
        /// Split this leaf node by creating left and right children, then moving all the children of
        /// this node into the respective buckets.
        /// </summary>
        void SplitLeafNode(double[][] Points)
        {
            // Create the new children.
            Right = new KDTreeNode(Dimensions, BucketCapacity);
            Left  = new KDTreeNode(Dimensions, BucketCapacity);

            // Move each item in this leaf into the children.
            for (int i = 0; i < Size; ++i)
            {
                // Store.
                int oldDataIndex = DataIndices[i];
                double[] oldPoint = Points[oldDataIndex];

                // If larger, put it in the right.
                if (oldPoint[SplitDimension] > SplitValue)
                    Right.AddPoint(oldDataIndex, Points);

                // If smaller, put it in the left.
                else
                    Left.AddPoint(oldDataIndex, Points);
            }

            // Wipe the data from this KDTreeNode.
            DataIndices = null;
        }
        
        /// <summary>
        /// Increment the capacity of this leaf by BucketCapacity
        /// </summary>
        void IncreaseLeafCapacity()
        {
            Array.Resize<int>(ref DataIndices, DataIndices.Length + BucketCapacity);
        }   
        #endregion
    }
}
