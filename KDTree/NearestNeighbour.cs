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
        double[] SearchPoint;
        /// <summary>A distance function which is used to compare nodes and value positions.</summary>
        IDistanceFunction DistanceFunction;
        /// <summary>The tree nodes which have yet to be evaluated.</summary>
        MinHeap<KDTreeNode> Pending;
        /// <summary>The values which have been evaluated and selected.</summary>
        IntervalHeap<T> Evaluated;
        /// <summary>The root of the kd tree to begin searching from.</summary>
        KDTreeNode Root;
        /// <summary>The max number of points we can return through this iterator.</summary>
        int MaxPointsReturned;
        /// <summary>The number of points we can still test before conclusion.</summary>
        int PointsRemaining;
        /// <summary>Threshold to apply to tree iteration.  Negative numbers mean no threshold applied.</summary>
        double Threshold;
        /// <summary>Distance of the current value to the search point.</summary>
        public double CurrentDistance { get; private set; }
        /// <summary>Current value referenced by the iterator as an object.</summary>
        public T Current { get; private set; }
        
        #region IEnumerator Implementation
        object IEnumerator.Current { get { return Current; } }
        #endregion

        /// <summary>
        /// Construct a new nearest neighbour iterator.
        /// </summary>
        /// <param name="Tree">The tree to begin searching from.</param>
        /// <param name="SearchPoint">The point in n-dimensional space to search.</param>
        /// <param name="Distance">The distance function used to evaluate the points.</param>
        /// <param name="MaxPoints">The max number of points which can be returned by this iterator.  Capped to max in tree.</param>
        /// <param name="Threshold">Threshold to apply to the search space.  Negative numbers indicate that no threshold is applied.</param>
        public NearestNeighbour(KDTree<T> Tree, double[] SearchPoint, IDistanceFunction Distance, int MaxPoints, double Threshold)
        {
            // Check the dimensionality of the search point.
            if (SearchPoint.Length != Tree.Dimensions)
                throw new ArgumentException("Dimensionality of search point and kd-tree are not the same.", "SearchPoint");

            // Store the Tree
            this.Tree = Tree;
            this.Root = this.Tree.RootNode;
            
            // Store the search point.
            this.SearchPoint = SearchPoint.ToArray();

            // Store the point count, distance function and tree root.
            this.PointsRemaining = Math.Min(MaxPoints, Root.Size);
            this.Threshold = Threshold;
            this.DistanceFunction = Distance;
            this.MaxPointsReturned = MaxPoints;
            this.CurrentDistance = -1;

            // Create an interval heap for the points we check.
            this.Evaluated = new IntervalHeap<T>();

            // Create a min heap for the things we need to check.
            this.Pending = new MinHeap<KDTreeNode>();
            this.Pending.Insert(0, this.Root);
        }

        /// <summary>
        /// Check for the next iterator item.
        /// </summary>
        /// <returns>True if we have one, false if not.</returns>
        public bool MoveNext()
        {
            // Bail if we are finished.
            if (PointsRemaining == 0)
            {
                Current = default(T);
                return false;
            }

            // While we still have paths to evaluate.
            while (Pending.Size > 0 && (Evaluated.Size == 0 || (Pending.MinKey < Evaluated.MinKey)))
            {
                // If there are pending paths possibly closer than the nearest evaluated point, check it out
                var cursor = Pending.RemoveMin();

                // Descend the tree, recording paths not taken
                while (!cursor.IsLeaf)
                {
                    KDTreeNode notTaken;

                    // If the seach point is larger, select the right path.
                    if (SearchPoint[cursor.SplitDimension] > cursor.SplitValue)
                    {
                        notTaken = cursor.Left;
                        cursor = cursor.Right;
                    }
                    else
                    {
                        notTaken = cursor.Right;
                        cursor = cursor.Left;
                    }

                    // Calculate the shortest distance between the search point and the min and max bounds of the kd-node.
                    double distance = DistanceFunction.DistanceToRectangle(SearchPoint, notTaken.MinimumBound, notTaken.MaximumBound);

                    // If it is greater than the threshold, skip.
                    if (Threshold >= 0 && distance > Threshold)
                    {
                        //pPending.Insert(fDistance, pNotTaken);
                        continue;
                    }

                    // Only add the path we need more points or the node is closer than furthest point on list so far.
                    if (Evaluated.Size < PointsRemaining || distance <= Evaluated.MaxKey)
                    {
                        Pending.Insert(distance, notTaken);
                    }
                }
                
                if (cursor.Size > 0)
                {
                    if (cursor.SinglePoint)
                    {
                        // If all the points in this KD node are in one place.
                        // Work out the distance between this point and the search point.
                        double distance = DistanceFunction.Distance(Tree.GetPointAt(cursor[0]), SearchPoint);
    
                        // Skip if the point exceeds the threshold.
                        // Technically this should never happen, but be prescise.
                        if (Threshold >= 0 && distance > Threshold)
                            continue;
    
                        // Add the point if either need more points or it's closer than furthest on list so far.
                        if (Evaluated.Size < PointsRemaining || distance <= Evaluated.MaxKey)
                        {
                            for (int i = 0; i < cursor.Size; ++i)
                            {
                                // If we don't need any more, replace max
                                if (Evaluated.Size == PointsRemaining)
                                    Evaluated.ReplaceMax(distance, Tree[cursor[i]]);
    
                                // Otherwise insert.
                                else
                                    Evaluated.Insert(distance, Tree[cursor[i]]);
                            }
                        }
                    }
                    else
                    {
                        // If the points in the KD node are spread out.
                        // Treat the distance of each point seperately.
                        for (int i = 0; i < cursor.Size; ++i)
                        {
                            // Compute the distance between the points.
                            double distance = DistanceFunction.Distance(Tree.GetPointAt(cursor[i]), SearchPoint);
                            
                            // Skip if it exceeds the threshold.
                            if (Threshold >= 0 && distance > Threshold)
                                continue;
                            
                            // Insert the point if we have more to take.
                            if (Evaluated.Size < PointsRemaining)
                                Evaluated.Insert(distance, Tree[cursor[i]]);
                            
                            // Otherwise replace the max.
                            else if (distance < Evaluated.MaxKey)
                                Evaluated.ReplaceMax(distance, Tree[cursor[i]]);
                        }
                    }
                }
            }

            // Select the point with the smallest distance.
            if (Evaluated.Size == 0)
                return false;

            PointsRemaining--;
            CurrentDistance = Evaluated.MinKey;
            Current = Evaluated.Min;
            Evaluated.RemoveMin();
            return true;
        }

        /// <summary>
        /// Reset the iterator.
        /// </summary>
        public void Reset()
        {
            // Store the point count and the distance function.
            PointsRemaining = Math.Min(MaxPointsReturned, Root.Size);
            CurrentDistance = -1;

            // Create an interval heap for the points we check.
            Evaluated = new IntervalHeap<T>();

            // Create a min heap for the things we need to check.
            Pending = new MinHeap<KDTreeNode>();
            Pending.Insert(0, Root);
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
