namespace KDTree
{
    using System;
    
    /// <summary>
    /// A KD-Tree node which supports a generic number of dimensions.
    /// All data items need the same number of dimensions.
    /// This node splits based on the largest range of any dimension.
    /// </summary>
    /// <remarks>This is based on this: https://bitbucket.org/rednaxela/knn-benchmark/src/tip/ags/utils/dataStructures/trees/thirdGenKD/ </remarks>
    sealed class KDTreeNode
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
            this.Points = new double[BucketCapacity][];
            this.Data = new int[BucketCapacity];
        }
        #endregion
        
        #region Internals
        /// <summary>
        /// Initial Bucket Capacity
        /// </summary>
        readonly int BucketCapacity;
        /// <summary>
        /// Number of Dimensions
        /// </summary>
        public readonly int Dimensions;
        
        /// <summary>
        /// Number of Items within the Tree 
        /// </summary>
        public int Size { get; private set; }
        
        /// <summary>
        /// Array of locations
        /// </summary>
        public double[][] Points;
        /// <summary>
        /// Array of data indices
        /// </summary>
        public int[] Data;
        
        /// <summary>
        /// Left and Right Children Nodes
        /// </summary>
        public KDTreeNode Left, Right;
        /// <summary>
        /// Index of the Split Dimension
        /// </summary>
        public int SplitDimension;
        /// <summary>
        /// Split Value on Split Dimension (larger go into the right, smaller go into left)
        /// </summary>
        public double SplitValue;
        
        /// <summary>
        /// Bounding Box for this Node on all Dimensions
        /// </summary>
        public double[] MinBound, MaxBound;
        
        /// <summary>
        /// Is this Node a Single Point ?
        /// </summary>
        public bool SinglePoint;
        
        /// <summary>
        /// Is this Node a Leaf ?
        /// </summary>
        public bool IsLeaf { get { return this.Points != null; } }
        #endregion
        
        #region Operations
        /// <summary>
        /// Insert a new point from this node.
        /// </summary>
        /// <param name="Point">The position which represents the data.</param>
        /// <param name="Index">The index of the data.</param>
        internal void AddPoint(double[] Point, int Index)
        {
            // Find the correct leaf node.
            var pCursor = this;
            while (!pCursor.IsLeaf)
            {
                // Extend the size of the leaf.
                pCursor.ExtendBounds(Point);
                // Increment Size.
                ++pCursor.Size;

                pCursor = Point[pCursor.SplitDimension] > pCursor.SplitValue ? pCursor.Right : pCursor.Left;
            }

            // Insert it into the leaf.
            pCursor.AddLeafPoint(Point, Index);
        }
        
        #endregion 
        
        #region Insides
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
        /// <param name="Point">The point to insert the data at.</param>
        /// <param name="Index">The index of the point.</param>
        void AddLeafPoint(double[] Point, int Index)
        {
            // Add the data point to this node.
            Points[Size] = Point;
            Data[Size] = Index;
            ExtendBounds(Point);
            ++Size;

            // Split if the node is getting too large in terms of data.
            if (Size == Points.Length)
            {
                // If the node is getting too physically large.
                if (CalculateSplit())
                {
                    // If the node successfully had it's split value calculated, split node.
                    SplitLeafNode();
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
        void SplitLeafNode()
        {
            // Create the new children.
            Right = new KDTreeNode(Dimensions, BucketCapacity);
            Left  = new KDTreeNode(Dimensions, BucketCapacity);

            // Move each item in this leaf into the children.
            for (int i = 0; i < Size; ++i)
            {
                // Store.
                double[] tOldPoint = Points[i];
                int kOldData = Data[i];

                // If larger, put it in the right.
                if (tOldPoint[SplitDimension] > SplitValue)
                    Right.AddPoint(tOldPoint, kOldData);

                // If smaller, put it in the left.
                else
                    Left.AddPoint(tOldPoint, kOldData);
            }

            // Wipe the data from this KDTreeNode.
            Points = null;
            Data = null;
        }
        
        /// <summary>
        /// Increment the capacity of this leaf by BucketCapacity
        /// </summary>
        void IncreaseLeafCapacity()
        {
            Array.Resize<double[]>(ref Points, Points.Length + BucketCapacity);
            Array.Resize<int>(ref Data, Data.Length + BucketCapacity);
        }

        #endregion
    }
}
