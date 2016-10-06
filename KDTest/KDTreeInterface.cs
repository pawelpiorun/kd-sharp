namespace KDSharp.KDTest
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using KDSharp.KDTree;
    
    /// <summary>
    /// Interface Test of the KDTree
    /// </summary>
    [TestFixture]
    public class KDTreeInterface
    {
        public KDTreeInterface()
        {
        }
        
        #region Instanciation
        [Test]
        public void InstantiateKDTree_1Dimensions()
        {
            var tree = new KDTree<int>(1);
            
            Assert.AreEqual(0, tree.Count);
        }

        [Test]
        public void InstantiateKDTree_2Dimensions()
        {
            var tree = new KDTree<int>(2);
            
            Assert.AreEqual(0, tree.Count);
        }

        [Test]
        public void InstantiateKDTree_3Dimensions()
        {
            var tree = new KDTree<int>(3);
            
            Assert.AreEqual(0, tree.Count);
        }

        [Test]
        public void InstantiateKDTree_NoBucketCapacity()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new KDTree<int>(1, 0));
        }
        
        [Test]
        public void InstantiateKDTree_8BucketCapacity()
        {
            var tree = new KDTree<int>(1, 8);
            
            Assert.AreEqual(0, tree.Count);
        }
        #endregion
        
        #region Iteration
        [Test]
        public void KDTree_NoElements_CollectionEmpty()
        {
            var tree = new KDTree<int>(1);
            
            CollectionAssert.IsEmpty(tree);
        }
        
        [Test]
        public void KDTree_TenElements_CollectionEquivalent()
        {
            var tree = new KDTree<int>(3, 8);
            
            var collection = new Tuple<double[], int>[] {
                new Tuple<double[], int>(new [] { 0.0, 0.0, 0.0 }, 1),
                new Tuple<double[], int>(new [] { 1.0, 1.0, 1.0 }, 2),
                new Tuple<double[], int>(new [] { 2.0, 2.0, 2.0 }, 3),
                new Tuple<double[], int>(new [] { 3.0, 3.0, 3.0 }, 4),
                new Tuple<double[], int>(new [] { 4.0, 4.0, 4.0 }, 5),
                new Tuple<double[], int>(new [] { 5.0, 5.0, 5.0 }, 6),
                new Tuple<double[], int>(new [] { 6.0, 6.0, 6.0 }, 7),
                new Tuple<double[], int>(new [] { 7.0, 7.0, 7.0 }, 8),
                new Tuple<double[], int>(new [] { 8.0, 8.0, 8.0 }, 9),
                new Tuple<double[], int>(new [] { 9.0, 9.0, 9.0 }, 10),
            };
            
            foreach (var item in collection)
            {
                tree.AddPoint(item.Item1, item.Item2);
            }
            
            CollectionAssert.AreEquivalent(collection.Select(obj => obj.Item2), tree);
        }
        
        [Test]
        public void KDTree_RemoveElements_CollectionEquivalent()
        {
            var tree = new KDTree<int>(3, 2);
            
            var collection = new Tuple<double[], int>[] {
                new Tuple<double[], int>(new [] { 0.0, 0.0, 0.0 }, 1),
                new Tuple<double[], int>(new [] { 1.0, 1.0, 1.0 }, 2),
                new Tuple<double[], int>(new [] { 2.0, 2.0, 2.0 }, 3),
                new Tuple<double[], int>(new [] { 3.0, 3.0, 3.0 }, 4),
                new Tuple<double[], int>(new [] { 4.0, 4.0, 4.0 }, 5),
                new Tuple<double[], int>(new [] { 5.0, 5.0, 5.0 }, 6),
                new Tuple<double[], int>(new [] { 6.0, 6.0, 6.0 }, 7),
                new Tuple<double[], int>(new [] { 7.0, 7.0, 7.0 }, 8),
                new Tuple<double[], int>(new [] { 8.0, 8.0, 8.0 }, 9),
                new Tuple<double[], int>(new [] { 9.0, 9.0, 9.0 }, 10),
            };
            
            foreach (var item in collection)
            {
                tree.AddPoint(item.Item1, item.Item2);
            }
            
            foreach (var idx in Enumerable.Range(3, 4))
            {
                tree.Remove(idx);
            }
            
            CollectionAssert.AreEquivalent(collection.Select(obj => obj.Item2).Except(Enumerable.Range(3, 4)), tree);
        }
        
        [Test]
        public void KDTree_RemoveAddElements_CollectionEquivalent()
        {
            var tree = new KDTree<int>(3, 2);
            
            var collection = new Tuple<double[], int>[] {
                new Tuple<double[], int>(new [] { 0.0, 0.0, 0.0 }, 1),
                new Tuple<double[], int>(new [] { 1.0, 1.0, 1.0 }, 2),
                new Tuple<double[], int>(new [] { 2.0, 2.0, 2.0 }, 3),
                new Tuple<double[], int>(new [] { 3.0, 3.0, 3.0 }, 4),
                new Tuple<double[], int>(new [] { 4.0, 4.0, 4.0 }, 5),
                new Tuple<double[], int>(new [] { 5.0, 5.0, 5.0 }, 6),
                new Tuple<double[], int>(new [] { 6.0, 6.0, 6.0 }, 7),
                new Tuple<double[], int>(new [] { 7.0, 7.0, 7.0 }, 8),
                new Tuple<double[], int>(new [] { 8.0, 8.0, 8.0 }, 9),
                new Tuple<double[], int>(new [] { 9.0, 9.0, 9.0 }, 10),
            };
            
            foreach (var item in collection)
            {
                tree.AddPoint(item.Item1, item.Item2);
            }
            
            foreach (var idx in Enumerable.Range(3, 4))
            {
                tree.Remove(idx);
            }
            
            var collection2 = new Tuple<double[], int>[] {
                new Tuple<double[], int>(new [] { 10.0, 10.0, 10.0 }, 11),
                new Tuple<double[], int>(new [] { 11.0, 11.0, 11.0 }, 12),
                new Tuple<double[], int>(new [] { 12.0, 12.0, 12.0 }, 13),
            };
            
            foreach (var item in collection2)
            {
                tree.AddPoint(item.Item1, item.Item2);
            }
            
            CollectionAssert.AreEquivalent(collection.Concat(collection2).Select(obj => obj.Item2).Except(Enumerable.Range(3, 4)), tree);
        }
        #endregion
        
        #region Element Add
        [Test]
        public void AddKDTreeElement_OneElementSize()
        {
            var tree = new KDTree<int>(3, 8);
            
            tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            Assert.AreEqual(1, tree.Count);
        }
        
        [Test]
        public void AddKDTreeElement_TenElementSize()
        {
            var tree = new KDTree<int>(3, 8);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            Assert.AreEqual(10, tree.Count);
        }
        
        [Test]
        public void AddKDTreeElement_WrongArraySize_ThrowsException()
        {
            var tree = new KDTree<int>(3, 8);
            
            Assert.Throws(typeof(ArgumentException), () => tree.AddPoint(new [] { 0.0, 0.0 }, 0));
            Assert.Throws(typeof(ArgumentException), () => tree.AddPoint(new [] { 0.0, 0.0, 0.0, 0.0 }, 0));
        }
        
        [Test]
        public void AddElement_CollectionImplementation_DefaultPosition()
        {
            var tree = new KDTree<int>(3, 8);
            
            tree.Add(1);
            
            CollectionAssert.AreEqual(new [] { default(double), default(double), default(double) }, tree.GetPoint(1));
        }
        #endregion
                
        #region Clear
        [Test]
        public void ClearTree_EmptyTree_IsEmpty()
        {
        	var tree = new KDTree<int>(1, 2);
        	
        	tree.Clear();
        	
        	CollectionAssert.IsEmpty(tree);
        }
        
        [Test]
        public void ClearTree_OneElement_IsEmpty()
        {
        	var tree = new KDTree<int>(1, 2);
        	
        	tree.Add(1);
        	
        	tree.Clear();
        	
        	CollectionAssert.IsEmpty(tree);
        }
        
        [Test]
        public void ClearTree_ManyElements_IsEmpty()
        {
        	var tree = new KDTree<int>(1, 2);
        	
        	for (int i = 0 ; i < 10 ; ++i)
        		tree.Add(i);
        	
        	tree.Clear();
        	
        	CollectionAssert.IsEmpty(tree);
        }
        
        [Test]
        public void ClearTree_ReAddElements_CollectionEquivalent()
        {
        	var tree = new KDTree<int>(1, 2);
        	
        	for (int i = 0 ; i < 10 ; ++i)
        		tree.Add(i);
        	
        	tree.Clear();
        	
        	for (int i = 10 ; i < 20 ; ++i)
        		tree.Add(i);
        	
        	CollectionAssert.AreEquivalent(Enumerable.Range(10, 10), tree);
        }
        #endregion
        
        #region Contains
        [Test]
        public void Contains_EmptyTree_False()
        {
        	var tree = new KDTree<int>(1, 2);
        	
        	Assert.IsFalse(tree.Contains(1));
        }
        
        [Test]
        public void Contains_OneElement_NonExistent()
        {
        	var tree = new KDTree<int>(1, 2);
        	
        	tree.Add(2);
        	
        	Assert.IsFalse(tree.Contains(1));
        }
        
        [Test]
        public void Contains_OneElement_Existent()
        {
        	var tree = new KDTree<int>(1, 2);
        	
        	tree.Add(2);
        	
        	Assert.IsTrue(tree.Contains(2));
        }
        
        [Test]
        public void Contains_ManyElement_NonExistent()
        {
        	var tree = new KDTree<int>(1, 2);
        	
        	for (int i = 10 ; i < 20 ; ++i)
        		tree.Add(i);
        	
        	Assert.IsFalse(tree.Contains(2));
        }
        
        [Test]
        public void Contains_ManyElement_Existent()
        {
        	var tree = new KDTree<int>(1, 2);
        	
        	for (int i = 10 ; i < 20 ; ++i)
        		tree.Add(i);
        	
        	Assert.IsTrue(tree.Contains(15));
        }
        #endregion
        
        #region CopyTo
        [Test]
        public void CopyTo_EmptyTree_EmptyArray()
        {
        	var tree = new KDTree<int>(1);
        	
        	var array = new int[0];
        	tree.CopyTo(array, 0);
        	
        	CollectionAssert.IsEmpty(array);
        }
        
        [Test]
        public void CopyTo_OneElement_OneElementArray()
        {
        	var tree = new KDTree<int>(1);
        	tree.Add(1);
        	var array = new int[1];
        	tree.CopyTo(array, 0);
        	
        	CollectionAssert.AreEquivalent(new [] { 1 }, array);
        }
        
        [Test]
        public void CopyTo_TenElement_TenElementArray()
        {
        	var tree = new KDTree<int>(1);
        	foreach (var item in Enumerable.Range(1, 10))
        		tree.Add(item);
        	
        	var array = new int[10];
        	
        	tree.CopyTo(array, 0);
        	
        	CollectionAssert.AreEquivalent(Enumerable.Range(1, 10), array);
        }
        
        [Test]
        public void CopyTo_TenElementFiveRemoved_FiveElementArray()
        {
        	var tree = new KDTree<int>(1);
        	foreach (var item in Enumerable.Range(1, 10))
        		tree.Add(item);
        	
        	foreach (var item in Enumerable.Range(1, 5))
        		tree.Remove(item * 2);
        	
        	var array = new int[5];
        	
        	tree.CopyTo(array, 0);
        	
        	CollectionAssert.AreEquivalent(Enumerable.Range(1, 10).Where(i => i % 2 != 0), array);
        }
        #endregion

        #region GetPoint
        [Test]
        public void GetPoint_EmptyTree_NullResult()
        {
        	var tree = new KDTree<int>(1);
        	
        	var point = tree.GetPoint(1);
        	
        	Assert.IsNull(point);
        }
        
        [Test]
        public void GetPoint_OneElement_NullResult()
        {
        	var tree = new KDTree<int>(1);
        	
        	tree.Add(2);
        	
        	var point = tree.GetPoint(1);
        	
        	Assert.IsNull(point);
        }
        
        [Test]
        public void GetPoint_OneElement_AreEquivalent()
        {
        	var tree = new KDTree<int>(1);
        	
        	var insertPoint = new [] { 3.0 };
        	
        	tree.AddPoint(insertPoint, 2);
        	
        	var point = tree.GetPoint(2);
        	
        	CollectionAssert.AreEquivalent(insertPoint, point);
        }
        
        [Test]
        public void GetPoint_ManyElements_NullResult()
        {
        	var tree = new KDTree<int>(1);
        	
        	for (int i = 10 ; i < 20 ; ++i)
        		tree.Add(i);
        	
        	var point = tree.GetPoint(2);
        	
        	Assert.IsNull(point);
        }
        
        [Test]
        public void GetPoint_ManyElements_AreEquivalent()
        {
        	var tree = new KDTree<int>(1);
        	
        	for (int i = 10 ; i < 20 ; ++i)
        		tree.AddPoint(new [] { (double)i }, i);
        	
        	var point = tree.GetPoint(15);
        	
        	CollectionAssert.AreEquivalent(new [] { 15.0 }, point);
        }
        #endregion
        
        #region GetPointAt
        [Test]
        public void GetPointAt_EmptyTree_Exception()
        {
            var tree = new KDTree<int>(1);
            
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => tree.GetPointAt(0));
        }
        
        [Test]
        public void GetPointAt_OutOfRange_Exception()
        {
            var tree = new KDTree<int>(1);
            
            tree.Add(1);
            
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => tree.GetPointAt(1));
        }
        
        [Test]
        public void GetPointAt_OneElement_DefaultPoint()
        {
            var tree = new KDTree<int>(1);
            
            tree.Add(1);
            
            var point = tree.GetPointAt(0);
            
            CollectionAssert.AreEquivalent(new [] { default(double) }, point);
        }
        
        [Test]
        public void GetPointAt_ManyElements_ExpectedPointValue()
        {
            var tree = new KDTree<int>(1);
            
            for (int i = 10 ; i < 20 ; ++i)
                tree.AddPoint(new [] { (double)i }, i);
            
            var point = tree.GetPointAt(5);
            
            CollectionAssert.AreEquivalent(new [] { (double)15 }, point);
        }
        #endregion
        
        #region IndexOf
        [Test]
        public void IndexOf_EmptyTree_Negative()
        {
            var tree = new KDTree<int>(1);
            
            var idx = tree.IndexOf(0);
            
            Assert.LessOrEqual(idx, -1);
        }
        
        [Test]
        public void IndexOf_OneElement_Negative()
        {
            var tree = new KDTree<int>(1);
            
            tree.Add(1);
            
            var idx = tree.IndexOf(0);
            
            Assert.LessOrEqual(idx, -1);
        }
        
        [Test]
        public void IndexOf_OneElement_FirstIndex()
        {
            var tree = new KDTree<int>(1);
            
            tree.Add(1);
            
            var idx = tree.IndexOf(1);
            
            Assert.AreEqual(0, idx);
        }
        
        [Test]
        public void IndexOf_ManyElements_ExpectedIndex()
        {
            var tree = new KDTree<int>(1);
            
            for (int i = 10 ; i < 20 ; ++i)
                tree.AddPoint(new [] { (double)i }, i);
            
            var idx = tree.IndexOf(15);
            
            Assert.AreEqual(5, idx);
        }
        #endregion
        
        #region Element Move
        [Test]
        public void MoveElement_EmptyTree_NotMoved()
        {
            var tree = new KDTree<int>(1, 2);
            
            Assert.IsFalse(tree.MovePoint(new [] { 0.0 }, 1));
            Assert.AreEqual(0, tree.Count);
        }
        
        [Test]
        public void MoveElement_OneEntry_Moved()
        {
            var tree = new KDTree<int>(1, 2);
            
            tree.AddPoint(new [] { 0.0 }, 1);
            
            Assert.IsTrue(tree.MovePoint(new [] { 1.0 }, 1));
            Assert.AreEqual(1, tree.Count);
        }
        
        [Test]
        public void MoveElement_OneEntry_AtPosition()
        {
            var tree = new KDTree<int>(1, 2);
            
            tree.AddPoint(new [] { 0.0 }, 1);
            
            tree.MovePoint(new [] { 1.0 }, 1);
            
            CollectionAssert.AreEquivalent(new [] { 1.0 }, tree.GetPoint(1));
            Assert.AreEqual(1, tree.Count);
        }
        
        [Test]
        public void MoveElement_OneEntry_NotMoved()
        {
            var tree = new KDTree<int>(1, 2);
            
            tree.AddPoint(new [] { 0.0 }, 1);
            
            Assert.IsFalse(tree.MovePoint(new [] { 1.0 }, 2));
            Assert.AreEqual(1, tree.Count);
        }
        
        [Test]
        public void MoveElements_ManyEntry_AtPositions()
        {
            var tree = new KDTree<int>(3, 2);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            tree.AddPoint(new [] { 2.0, 2.0, 2.0 }, 3);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            tree.MovePoint(new [] { 3.0, 3.0, 3.0 }, 3);
            
            CollectionAssert.AreEquivalent(new [] { 3.0, 3.0, 3.0 }, tree.GetPoint(3));
            Assert.AreEqual(21, tree.Count);
        }
        #endregion
        
        #region Regen
        [Test]
        public void Regen_EmptyTree_NoElement()
        {
            var tree = new KDTree<int>(1);
            
            tree.Regen();
            
            CollectionAssert.IsEmpty(tree);
        }
        
        [Test]
        public void Regen_OneElement_CollectionEquivalent()
        {
            var tree = new KDTree<int>(1);
            
            tree.Add(1);
            
            tree.Regen();
            
            CollectionAssert.AreEquivalent(new [] { 1 }, tree);
        }
        
        [Test]
        public void Regen_ManyElements_CollectionEquivalent()
        {
            var tree = new KDTree<int>(3, 2);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            tree.Regen();
            
            CollectionAssert.AreEquivalent(Enumerable.Repeat(0, 10), tree);
        }
        
        [Test]
        public void Regen_WithRemoves_CollectionEquivalent()
        {
            var tree = new KDTree<int>(3, 2);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, i);
            
            tree.RemoveAt(0);
            tree.RemoveAt(9);
            
            Assert.AreEqual(tree.RemovalCount, 2);
            
            tree.Regen();
            
            Assert.AreEqual(tree.RemovalCount, 0);
            
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 8), tree);
        }
        #endregion
        
        #region Element Remove
        [Test]
        public void RemoveElement_EmptyTree_NotRemoved()
        {
            var tree = new KDTree<int>(1, 2);
            
            Assert.IsFalse(tree.Remove(1));
            Assert.AreEqual(0, tree.RemovalCount);
            Assert.AreEqual(0, tree.Count);
        }
        
        [Test]
        public void RemoveElement_OneElement_Removed()
        {
            var tree = new KDTree<int>(1, 2);
            
            tree.AddPoint(new [] { 0.0 }, 1);
            
            Assert.IsTrue(tree.Remove(1));
            Assert.AreEqual(1, tree.RemovalCount);
            Assert.AreEqual(0, tree.Count);
        }
        
        [Test]
        public void RemoveElement_OneElement_NotRemoved()
        {
            var tree = new KDTree<int>(1, 2);
            
            tree.AddPoint(new [] { 0.0 }, 1);
            
            Assert.IsFalse(tree.Remove(2));
            Assert.AreEqual(0, tree.RemovalCount);
            Assert.AreEqual(1, tree.Count);
        }
        
        [Test]
        public void RemoveElement_TenElement_Removed()
        {
            var tree = new KDTree<int>(3, 2);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            tree.Remove(0);
            Assert.AreEqual(1, tree.RemovalCount);
            Assert.AreEqual(9, tree.Count);
        }
        
        [Test]
        public void RemoveElement_TenElement_NotRemoved()
        {
            var tree = new KDTree<int>(3, 2);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            Assert.IsFalse(tree.Remove(1));
            Assert.AreEqual(10, tree.Count);
        }
        #endregion
        
        #region RemoveAt
        [Test]
        public void RemoveAt_EmptyTree_Exception()
        {
            var tree = new KDTree<int>(1, 2);
            
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => tree.RemoveAt(0));
            Assert.AreEqual(0, tree.RemovalCount);
        }

        [Test]
        public void RemoveAt_OneElement_Exception()
        {
            var tree = new KDTree<int>(1, 2);
            
            tree.Add(1);
            
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => tree.RemoveAt(1));
            Assert.AreEqual(0, tree.RemovalCount);
        }
        
        [Test]
        public void RemoveAt_OneElement_Empty()
        {
            var tree = new KDTree<int>(1, 2);
            
            tree.Add(1);
            
            tree.RemoveAt(0);
            
            Assert.AreEqual(1, tree.RemovalCount);
            CollectionAssert.IsEmpty(tree);
        }
        
        [Test]
        public void RemoveAt_ManyElements_CollectionEquivalent()
        {
            var tree = new KDTree<int>(3, 2);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, i);
            
            tree.RemoveAt(0);
            tree.RemoveAt(9);
            
            Assert.AreEqual(tree.RemovalCount, 2);
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 8), tree);
        }
        #endregion
        
        #region Indexer
        [Test]
        public void Indexer_EmptyTree_Exception()
        {
            var tree = new KDTree<int>(1);
            
            int item;
            
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => item = tree[0]);
        }

        [Test]
        public void Indexer_OneElement_Exception()
        {
            var tree = new KDTree<int>(1);
            
            tree.Add(0);
            
            int item;
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => item = tree[1]);
        }
        
        [Test]
        public void Indexer_OneElementWithRemove_Exception()
        {
            var tree = new KDTree<int>(1);
            
            tree.Add(0);
            tree.Add(1);
            
            tree.RemoveAt(0);
            
            tree.Add(2);
            
            int item;
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => item = tree[2]);
        }
        
        [Test]
        public void Indexer_OneElement_Retrieved()
        {
            var tree = new KDTree<int>(1);
            
            tree.Add(1);
            
            Assert.AreEqual(1, tree[0]);
        }
        
        [Test]
        public void Indexer_ManyElementsWithRemove_CollectionEquivalent()
        {
            var tree = new KDTree<int>(3, 2);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, i);
            
            tree.RemoveAt(0);
            tree.RemoveAt(9);
            
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 8).Select(idx => tree[idx]), tree);
        }
        #endregion
    }
}