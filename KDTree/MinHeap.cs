namespace KDSharp.KDTree
{
    using System;

    /// <summary>
    /// A MinHeap is a smallest-first queue based around a binary heap so it is quick to insert / remove items.
    /// </summary>
    /// <typeparam name="T">The type of data this MinHeap stores.</typeparam>
    /// <remarks>This is based on this: https://bitbucket.org/rednaxela/knn-benchmark/src/tip/ags/utils/dataStructures/trees/thirdGenKD/ </remarks>
    public sealed class MinHeap<T>
    {
        /// <summary>
        /// The default size for a min heap.
        /// </summary>
        public const int DEFAULT_SIZE = 64;

        /// <summary>
        /// The data array.  This stores the data items in the heap.
        /// </summary>
        T[] Data;

        /// <summary>
        /// The key array.  This determines how items are ordered. Smallest first.
        /// </summary>
        double[] Keys;

        /// <summary>
        /// Create a new min heap with the default capacity.
        /// </summary>
        public MinHeap() : this(DEFAULT_SIZE)
        {
        }

        /// <summary>
        /// Create a new min heap with a given capacity.
        /// </summary>
        /// <param name="iCapacity"></param>
        public MinHeap(int iCapacity)
        {
            this.Data = new T[iCapacity];
            this.Keys = new double[iCapacity];
            this.Capacity = iCapacity;
            this.Size = 0;
        }

        /// <summary>
        /// The number of items in this queue.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// The amount of space in this queue.
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// Insert a new element.
        /// </summary>
        /// <param name="key">The key which represents its position in the priority queue (ie. distance).</param>
        /// <param name="value">The value to be stored at the key.</param>
        public void Insert(double key, T value)
        {
            // If we need more room, double the space.
            if (Size >= Capacity)
            {
                // Calcualte the new capacity.
                Capacity *= 2;

                // Copy the data array.
                var newData = new T[Capacity];
                Array.Copy(Data, newData, Data.Length);
                Data = newData;

                // Copy the key array.
                var newKeys = new double[Capacity];
                Array.Copy(Keys, newKeys, Keys.Length);
                Keys = newKeys;
            }

            // Insert new value at the end
            Data[Size] = value;
            Keys[Size] = key;
            SiftUp(Size);
            Size++;
        }

        /// <summary>
        /// Remove the smallest element.
        /// </summary>
        public T RemoveMin()
        {
            if (Size == 0)
                throw new InvalidOperationException("Collection contains no elements");

            Size--;
            var min = Data[0];
            Data[0] = Data[Size];
            Keys[0] = Keys[Size];
            Data[Size] = default(T);
            SiftDown(0);
            
            return min;
        }

        /// <summary>
        /// Get the data stored at the minimum element.
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
        /// Get the key which represents the minimum element.
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
        /// Bubble a child item up the tree.
        /// </summary>
        /// <param name="Child"></param>
        void SiftUp(int Child)
        {
            // For each parent above the child, if the parent is smaller then bubble it up.
            for (int Parent = (Child - 1) / 2; 
                Child != 0 && Keys[Child] < Keys[Parent]; 
                Child = Parent, Parent = (Child - 1) / 2)
            {
                T kData = Data[Parent];
                double dDist = Keys[Parent];

                Data[Parent] = Data[Child];
                Keys[Parent] = Keys[Child];

                Data[Child] = kData;
                Keys[Child] = dDist;
            }
        }

        /// <summary>
        /// Bubble a parent down through the children so it goes in the right place.
        /// </summary>
        /// <param name="Parent">The index of the parent.</param>
        void SiftDown(int Parent)
        {
            // For each child.
            for (int Child = Parent * 2 + 1; Child < Size; Parent = Child, Child = Parent * 2 + 1)
            {
                // If the child is larger, select the next child.
                if (Child + 1 < Size && Keys[Child] > Keys[Child + 1])
                    Child++;

                // If the parent is larger than the largest child, swap.
                if (Keys[Parent] > Keys[Child])
                {
                    // Swap the points
                    T pData = Data[Parent];
                    double pDist = Keys[Parent];

                    Data[Parent] = Data[Child];
                    Keys[Parent] = Keys[Child];

                    Data[Child] = pData;
                    Keys[Child] = pDist;
                }
                else
                {
                    // TODO: REMOVE THE BREAK
                    break;
                }
            }
        }
    }
}