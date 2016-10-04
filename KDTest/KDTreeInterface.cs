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
        #endregion
        
        #region Element Remove
        [Test]
        public void RemoveElement_EmptyTree_NotRemoved()
        {
            var tree = new KDTree<int>(1, 2);
            
            Assert.IsFalse(tree.Remove(1));
            Assert.AreEqual(0, tree.Count);
        }
        
        [Test]
        public void RemoveElement_OneElement_Removed()
        {
            var tree = new KDTree<int>(1, 2);
            
            tree.AddPoint(new [] { 0.0 }, 1);
            
            Assert.IsTrue(tree.Remove(1));
            Assert.AreEqual(0, tree.Count);
        }
        
        [Test]
        public void RemoveElement_OneElement_NotRemoved()
        {
            var tree = new KDTree<int>(1, 2);
            
            tree.AddPoint(new [] { 0.0 }, 1);
            
            Assert.IsFalse(tree.Remove(2));
            Assert.AreEqual(1, tree.Count);
        }
        
        [Test]
        public void RemoveElement_TenElement_Removed()
        {
            var tree = new KDTree<int>(3, 2);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            tree.Remove(0);
            
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
    }
}