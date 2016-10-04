namespace KDSharp.KDTree
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using KDSharp.DistanceFunctions;

    /// <summary>
    /// A NearestNeighbour iterator for the KD-tree which intelligently iterates and captures relevant data in the search space.
    /// </summary>
    /// <typeparam name="T">The type of data the iterator should handle.</typeparam>
    public sealed class NearestNeighbour<T> : IEnumerator<T>, IEnumerable<T>
    {
        KDTree<T> Tree;
        /// <summary>The point from which are searching in n-dimensional space.</summary>
        private double[] tSearchPoint;
        /// <summary>A distance function which is used to compare nodes and value positions.</summary>
        private DistanceFunction kDistanceFunction;
        /// <summary>The tree nodes which have yet to be evaluated.</summary>
        private MinHeap<KDTreeNode> pPending;
        /// <summary>The values which have been evaluated and selected.</summary>
        private IntervalHeap<T> pEvaluated;
        /// <summary>The root of the kd tree to begin searching from.</summary>
        private KDTreeNode pRoot = null;

        /// <summary>The max number of points we can return through this iterator.</summary>
        private int iMaxPointsReturned = 0;
        /// <summary>The number of points we can still test before conclusion.</summary>
        private int iPointsRemaining;
        /// <summary>Threshold to apply to tree iteration.  Negative numbers mean no threshold applied.</summary>
        private double fThreshold;

        /// <summary>Current value distance.</summary>
        private double _CurrentDistance = -1;
        /// <summary>Current value reference.</summary>
        private T _Current = default(T);

        /// <summary>
        /// Construct a new nearest neighbour iterator.
        /// </summary>
        /// <param name="Tree">The tree to begin searching from.</param>
        /// <param name="tSearchPoint">The point in n-dimensional space to search.</param>
        /// <param name="kDistance">The distance function used to evaluate the points.</param>
        /// <param name="iMaxPoints">The max number of points which can be returned by this iterator.  Capped to max in tree.</param>
        /// <param name="fThreshold">Threshold to apply to the search space.  Negative numbers indicate that no threshold is applied.</param>
        public NearestNeighbour(KDTree<T> Tree, double[] tSearchPoint, DistanceFunction kDistance, int iMaxPoints, double fThreshold)
        {
            // Check the dimensionality of the search point.
            if (tSearchPoint.Length != Tree.Dimensions)
                throw new ArgumentException("Dimensionality of search point and kd-tree are not the same.", "tSearchPoint");

            // Store the Tree
            this.Tree = Tree;
            this.pRoot = this.Tree.RootNode;
            
            // Store the search point.
            this.tSearchPoint = tSearchPoint.ToArray();

            // Store the point count, distance function and tree root.
            this.iPointsRemaining = Math.Min(iMaxPoints, pRoot.Size);
            this.fThreshold = fThreshold;
            this.kDistanceFunction = kDistance;
            this.iMaxPointsReturned = iMaxPoints;
            this._CurrentDistance = -1;

            // Create an interval heap for the points we check.
            this.pEvaluated = new IntervalHeap<T>();

            // Create a min heap for the things we need to check.
            this.pPending = new MinHeap<KDTreeNode>();
            this.pPending.Insert(0, this.pRoot);
        }

        /// <summary>
        /// Check for the next iterator item.
        /// </summary>
        /// <returns>True if we have one, false if not.</returns>
        public bool MoveNext()
        {
            // Bail if we are finished.
            if (iPointsRemaining == 0)
            {
                _Current = default(T);
                return false;
            }

            // While we still have paths to evaluate.
            while (pPending.Size > 0 && (pEvaluated.Size == 0 || (pPending.MinKey < pEvaluated.MinKey)))
            {
                // If there are pending paths possibly closer than the nearest evaluated point, check it out
                var pCursor = pPending.RemoveMin();

                // Descend the tree, recording paths not taken
                while (!pCursor.IsLeaf)
                {
                    KDTreeNode pNotTaken;

                    // If the seach point is larger, select the right path.
                    if (tSearchPoint[pCursor.SplitDimension] > pCursor.SplitValue)
                    {
                        pNotTaken = pCursor.Left;
                        pCursor = pCursor.Right;
                    }
                    else
                    {
                        pNotTaken = pCursor.Right;
                        pCursor = pCursor.Left;
                    }

                    // Calculate the shortest distance between the search point and the min and max bounds of the kd-node.
                    double fDistance = kDistanceFunction.DistanceToRectangle(tSearchPoint, pNotTaken.MinimumBound, pNotTaken.MaximumBound);

                    // If it is greater than the threshold, skip.
                    if (fThreshold >= 0 && fDistance > fThreshold)
                    {
                        //pPending.Insert(fDistance, pNotTaken);
                        continue;
                    }

                    // Only add the path we need more points or the node is closer than furthest point on list so far.
                    if (pEvaluated.Size < iPointsRemaining || fDistance <= pEvaluated.MaxKey)
                    {
                        pPending.Insert(fDistance, pNotTaken);
                    }
                }
                
                if (pCursor.Size > 0)
                {
                    if (pCursor.SinglePoint)
                    {
                        // If all the points in this KD node are in one place.
                        // Work out the distance between this point and the search point.
                        double fDistance = kDistanceFunction.Distance(Tree.GetPointAt(pCursor[0]), tSearchPoint);
    
                        // Skip if the point exceeds the threshold.
                        // Technically this should never happen, but be prescise.
                        if (fThreshold >= 0 && fDistance > fThreshold)
                            continue;
    
                        // Add the point if either need more points or it's closer than furthest on list so far.
                        if (pEvaluated.Size < iPointsRemaining || fDistance <= pEvaluated.MaxKey)
                        {
                            for (int i = 0; i < pCursor.Size; ++i)
                            {
                                // If we don't need any more, replace max
                                if (pEvaluated.Size == iPointsRemaining)
                                    pEvaluated.ReplaceMax(fDistance, Tree[pCursor[i]]);
    
                                // Otherwise insert.
                                else
                                    pEvaluated.Insert(fDistance, Tree[pCursor[i]]);
                            }
                        }
                    }
                    else
                    {
                        // If the points in the KD node are spread out.
                        // Treat the distance of each point seperately.
                        for (int i = 0; i < pCursor.Size; ++i)
                        {
                            // Compute the distance between the points.
                            double fDistance = kDistanceFunction.Distance(Tree.GetPointAt(pCursor[i]), tSearchPoint);
                            
                            // Skip if it exceeds the threshold.
                            if (fThreshold >= 0 && fDistance > fThreshold)
                                continue;
                            
                            // Insert the point if we have more to take.
                            if (pEvaluated.Size < iPointsRemaining)
                                pEvaluated.Insert(fDistance, Tree[pCursor[i]]);
                            
                            // Otherwise replace the max.
                            else if (fDistance < pEvaluated.MaxKey)
                                pEvaluated.ReplaceMax(fDistance, Tree[pCursor[i]]);
                        }
                    }
                }
            }

            // Select the point with the smallest distance.
            if (pEvaluated.Size == 0)
                return false;

            iPointsRemaining--;
            _CurrentDistance = pEvaluated.MinKey;
            _Current = pEvaluated.Min;
            pEvaluated.RemoveMin();
            return true;
        }

        /// <summary>
        /// Reset the iterator.
        /// </summary>
        public void Reset()
        {
            // Store the point count and the distance function.
            this.iPointsRemaining = Math.Min(iMaxPointsReturned, pRoot.Size);
            _CurrentDistance = -1;

            // Create an interval heap for the points we check.
            this.pEvaluated = new IntervalHeap<T>();

            // Create a min heap for the things we need to check.
            this.pPending = new MinHeap<KDTreeNode>();
            this.pPending.Insert(0, pRoot);
        }

        /// <summary>
        /// Return the distance of the current value to the search point.
        /// </summary>
        public double CurrentDistance
        {
            get { return _CurrentDistance; }
        }

        /// <summary>
        /// Return the current value referenced by the iterator as an object.
        /// </summary>
        object IEnumerator.Current
        {
            get { return _Current; }
        }
  
        /// <summary>
        /// Return the current value referenced by the iterator.
        /// </summary>
        public T Current
        {
            get { return _Current; }
        }

        public void Dispose()
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }
    }
}
