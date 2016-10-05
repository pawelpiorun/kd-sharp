namespace KDSharp.KDTree
{
    using System;
    using System.Linq;

    /// <summary>
    /// A binary interval heap is double-ended priority queue is a priority queue that it allows
    /// for efficient removal of both the maximum and minimum element.
    /// </summary>
    /// <typeparam name="T">The data type contained at each key.</typeparam>
    /// <remarks>This is based on this: https://bitbucket.org/rednaxela/knn-benchmark/src/tip/ags/utils/dataStructures/trees/thirdGenKD/ </remarks>
    public class IntervalHeap<T>
    {
        /// <summary>
        /// The default size for a new interval heap.
        /// </summary>
        public const int DEFAULT_SIZE = 64;

        /// <summary>
        /// The internal data array which contains the stored objects.
        /// </summary>
        T[] Data;

        /// <summary>
        /// The array of keys which 
        /// </summary>
        double[] Keys;

        /// <summary>
        /// Construct a new interval heap with the default capacity.
        /// </summary>
        public IntervalHeap() : this(DEFAULT_SIZE)
        {
        }

        /// <summary>
        /// Construct a new interval heap with a custom capacity.
        /// </summary>
        /// <param name="capacity"></param>
        public IntervalHeap(int capacity)
        {
            this.Data = new T[capacity];
            this.Keys = new double[capacity];
            this.Capacity = capacity;
            this.Size = 0;
        }

        /// <summary>
        /// The number of items in this interval heap.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// The current capacity of this interval heap.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Get the data with the smallest key.
        /// </summary>
        public T Min
        {
            get
            {
                if (Size == 0)
                    throw new InvalidOperationException("Collection contains no elements");
                return Data[0];
            }
        }

        /// <summary>
        /// Get the data with the largest key.
        /// </summary>
        public T Max
        {
            get
            {
                if (Size == 0)
                    throw new InvalidOperationException("Collection contains no elements");
                
                return Size == 1 ? Data[0] : Data[1];
            }
        }

        /// <summary>
        /// Get the smallest key.
        /// </summary>
        public double MinKey
        {
            get
            {
                if (Size == 0)
                    throw new InvalidOperationException("Collection contains no elements");
                
                return Keys[0];
            }
        }

        /// <summary>
        /// Get the largest key.
        /// </summary>
        public double MaxKey
        {
            get
            {
                if (Size == 0)
                    throw new InvalidOperationException("Collection contains no elements");
                
                return Size == 1 ? Keys[0] : Keys[1];
            }
        }

        /// <summary>
        /// Insert a new data item at a given key.
        /// </summary>
        /// <param name="key">The value which represents our data (i.e. a distance).</param>
        /// <param name="value">The data we want to store.</param>
        public void Insert(double key, T value)
        {
            // If more room is needed, double the array size.
            if (Size >= Capacity)
            {
                // Double the capacity.
                Capacity *= 2;

                // Expand the data array.
                var newData = new T[Capacity];
                Array.Copy(Data, newData, Data.Length);
                Data = newData;

                // Expand the key array.
                var newKeys = new double[Capacity];
                Array.Copy(Keys, newKeys, Keys.Length);
                Keys = newKeys;
            }

            // Insert the new value at the end.
            Size++;
            Data[Size-1] = value;
            Keys[Size-1] = key;

            // Ensure it is in the right place.
            SiftInsertedValueUp();
        }

        /// <summary>
        /// Remove the item with the smallest key from the queue.
        /// </summary>
        public void RemoveMin()
        {
            // Check for errors.
            if (Size == 0)
                throw new InvalidOperationException("Collection contains no elements");

            // Remove the item by 
            Size--;
            Data[0] = Data[Size];
            Keys[0] = Keys[Size];
            Data[Size] = default(T);
            SiftDownMin(0);
        }

        /// <summary>
        /// Replace the item with the smallest key in the queue.
        /// </summary>
        /// <param name="key">The new minimum key.</param>
        /// <param name="value">The new minumum data value.</param>
        public void ReplaceMin(double key, T value)
        {
            // Check for errors.
            if (Size == 0)
                throw new InvalidOperationException("Collection contains no elements");

            // Add the data.
            Data[0] = value;
            Keys[0] = key;

            // If we have more than one item.
            if (Size > 1)
            {
                // Swap with pair if necessary.
                if (Keys[1] < key)
                    Swap(0, 1);
                SiftDownMin(0);
            }
        }

        /// <summary>
        /// Remove the item with the largest key in the queue.
        /// </summary>
        public void RemoveMax()
        {
            // If we have no items in the queue.
            if (Size == 0)
                throw new InvalidOperationException("Collection contains no elements");

            // If we have one item, remove the min.
            if (Size == 1)
            {
                RemoveMin();
                return;
            }

            // Remove the max.
            Size--;
            Data[1] = Data[Size];
            Keys[1] = Keys[Size];
            Data[Size] = default(T);
            SiftDownMax(1);
        }

        /// <summary>
        /// Swap out the item with the largest key in the queue.
        /// </summary>
        /// <param name="key">The new key for the largest item.</param>
        /// <param name="value">The new data for the largest item.</param>
        public void ReplaceMax(double key, T value)
        {
            if (Size == 0)
                throw new InvalidOperationException("Collection contains no elements");
            
            if (Size == 1)
            {
                ReplaceMin(key, value);
                return;
            }

            Data[1] = value;
            Keys[1] = key;
            // Swap with pair if necessary
            if (key < Keys[0])
            {
                Swap(0, 1);
            }
            SiftDownMax(1);
        }


        /// <summary>
        /// Internal helper method which swaps two values in the arrays.
        /// This swaps both data and key entries.
        /// </summary>
        /// <param name="x">The first index.</param>
        /// <param name="y">The second index.</param>
        /// <returns>The second index.</returns>
        int Swap(int x, int y)
        {
            // Store temp.
            T data = Data[y];
            double dist = Keys[y];

            // Swap
            Data[y] = Data[x];
            Keys[y] = Keys[x];
            Data[x] = data;
            Keys[x] = dist;

            // Return.
            return y;
        }

        /**
         * Min-side (u % 2 == 0):
         * - leftchild:  2u + 2
         * - rightchild: 2u + 4
         * - parent:     (x/2-1)&~1
         *
         * Max-side (u % 2 == 1):
         * - leftchild:  2u + 1
         * - rightchild: 2u + 3
         * - parent:     (x/2-1)|1
         */

        /// <summary>
        /// Place a newly inserted element a into the correct tree position.
        /// </summary>
        void SiftInsertedValueUp()
        {
            // Work out where the element was inserted.
            int u = Size-1;

            if (u == 0)
            {
                // If it is the only element, nothing to do.
            }
            else if (u == 1)
            {
                // If it is the second element, sort with it's pair.
                // Swap if less than paired item.
                if  (Keys[u] < Keys[u-1])
                    Swap(u, u-1);
            }
            else if (u % 2 == 1)
            {
                // If it is on the max side, 
                // Already paired. Ensure pair is ordered right
                int p = (u/2-1)|1; // The larger value of the parent pair
                if  (Keys[u] < Keys[u-1])
                { // If less than it's pair
                    u = Swap(u, u-1); // Swap with it's pair
                    if (Keys[u] < Keys[p-1])
                    { // If smaller than smaller parent pair
                        // Swap into min-heap side
                        u = Swap(u, p-1);
                        SiftUpMin(u);
                    }
                }
                else
                {
                    if (Keys[u] > Keys[p])
                    { // If larger that larger parent pair
                        // Swap into max-heap side
                        u = Swap(u, p);
                        SiftUpMax(u);
                    }
                }
            }
            else
            {
                // Inserted in the lower-value slot without a partner
                // The larger value of the parent pair
                int p = (u/2-1)|1;
                if (Keys[u] > Keys[p])
                {
                    // If larger that larger parent pair
                    // Swap into max-heap side
                    u = Swap(u, p);
                    SiftUpMax(u);
                }
                else if (Keys[u] < Keys[p-1])
                {
                    // If smaller than smaller parent pair
                    // Swap into min-heap side
                    u = Swap(u, p-1);
                    SiftUpMin(u);
                }
            }
        }

        /// <summary>
        /// Bubble elements up the min side of the tree.
        /// </summary>
        /// <param name="Child">The child index.</param>
        void SiftUpMin(int Child)
        {
            // Min-side parent: (x/2-1)&~1
            for (int Parent = (Child/2-1)&~1; 
                Parent >= 0 && Keys[Child] < Keys[Parent]; 
                Child = Parent, Parent = (Child/2-1)&~1)
            {
                Swap(Child, Parent);
            }
        }

        /// <summary>
        /// Bubble elements up the max side of the tree.
        /// </summary>
        /// <param name="Child">The child index.</param>
        void SiftUpMax(int Child)
        {
            // Max-side parent: (x/2-1)|1
            for (int Parent = (Child/2-1)|1; 
                Parent >= 0 && Keys[Child] > Keys[Parent]; 
                Child = Parent, Parent = (Child/2-1)|1)
            {
                Swap(Child, Parent);
            }
        }

        /// <summary>
        /// Bubble elements down the min side of the tree.
        /// </summary>
        /// <param name="Parent">The parent index.</param>
        void SiftDownMin(int Parent)
        {
            // For each child of the parent.
            for (int Child = Parent * 2 + 2; Child < Size; Parent = Child, Child = Parent * 2 + 2)
            {
                // If the next child is less than the current child, select the next one.
                if (Child + 2 < Size && Keys[Child + 2] < Keys[Child])
                {
                    Child += 2;
                }

                // If it is less than our parent swap.
                if (Keys[Child] < Keys[Parent])
                {
                    Swap(Parent, Child);

                    // Swap the pair if necessary.
                    if (Child+1 < Size && Keys[Child+1] < Keys[Child])
                    {
                        Swap(Child, Child+1);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Bubble elements down the max side of the tree.
        /// </summary>
        /// <param name="Parent"></param>
        void SiftDownMax(int Parent)
        {
            // For each child on the max side of the tree.
            for (int Child = Parent * 2 + 1; Child <= Size; Parent = Child, Child = Parent * 2 + 1)
            {
                // If the child is the last one (and only has half a pair).
                if (Child == Size)
                {
                    // CHeck if we need to swap with th parent.
                    if (Keys[Child - 1] > Keys[Parent])
                        Swap(Parent, Child - 1);
                    break;
                }

                // If there is only room for a right child lower pair.
                if (Child + 2 == Size)
                {
                    // Swap the children.
                    if (Keys[Child + 1] > Keys[Child])
                    {
                        // Swap with the parent.
                        if (Keys[Child + 1] > Keys[Parent])
                           Swap(Parent, Child + 1);
                        break;
                    }
                }
                else if (Child + 2 < Size)
                {
                    // If there is room for a right child upper pair
                    if (Keys[Child + 2] > Keys[Child])
                    {
                        Child += 2;
                    }
                }
                
                if (Keys[Child] > Keys[Parent])
                {
                    Swap(Parent, Child);
                    // Swap with pair if necessary
                    if (Keys[Child-1] > Keys[Child])
                    {
                        Swap(Child, Child-1);
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}