namespace KDTest
{
    using System;
    using System.Linq;
    using KDTree;
    using NUnit.Framework;

    /// <summary>
    /// KDTree NearestNeighbour Tests
    /// </summary>
    [TestFixture]
    public class KDTreeNearestNeighbour
    {
        public KDTreeNearestNeighbour()
        {
        }
        
        #region Enumerate
        
        [Test]
        public void EmptyTree_EmptyNearestNeighbors()
        {
            var tree = new KDTree<int>(3);
            
            var neighbours = tree.NearestNeighbors(new [] { 0.0, 0.0, 0.0 }, 100).ToArray();
            
            CollectionAssert.IsEmpty(neighbours);
        }
        
        [Test]
        public void NoDistance_OneNearestNeighbors()
        {
            var tree = new KDTree<int>(3);
            
            tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            var neighbours = tree.NearestNeighbors(new [] { 1000.0, 1000.0, 1000.0 }, 100).ToArray();
            
            CollectionAssert.Contains(neighbours, 0);
        }
        
        [Test]
        public void NoDistance_HundredNearestNeighbors()
        {
            var tree = new KDTree<double>(3);
            
            for (double d = 0 ; d < 1000 ; d++)
                tree.AddPoint(new [] { d, d, d }, d);
            
            var neighbours = tree.NearestNeighbors(new [] { 0.0, 0.0, 0.0 }, 100).ToArray();
            
            CollectionAssert.AreEqual(Enumerable.Range(0, 100), neighbours);
        }
        
        [Test]
        public void ZeroDistance_TenNearestNeighbors()
        {
            var tree = new KDTree<int>(3);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, i);
            
            for (int i = 100 ; i < 110 ; i++)
                tree.AddPoint(new [] { 10.0, 10.0, 10.0 }, i);
            
            var neighbours = tree.NearestNeighbors(new [] { 0.0, 0.0, 0.0 }, 100, 0.0).ToArray();
            
            CollectionAssert.AreEquivalent(Enumerable.Range(0, 10), neighbours);
        }
        
        [Test]
        public void SomeDistance_TenNearestNeighbors()
        {
            var tree = new KDTree<double>(3);
            
            for (double d = 0 ; d < 1000 ; d++)
                tree.AddPoint(new [] { d, d, d }, d);
            
            var neighbours = tree.NearestNeighbors(new [] { 0.0, 0.0, 0.0 }, 100, 243.0).ToArray();
            
            CollectionAssert.AreEqual(Enumerable.Range(0, 10), neighbours);
        }
        
        [Test]
        public void LotDistance_HundredNearestNeighbors()
        {
            var tree = new KDTree<double>(3);
            
            for (double d = 0 ; d < 1000 ; d++)
                tree.AddPoint(new [] { d, d, d }, d);
            
            var neighbours = tree.NearestNeighbors(new [] { 0.0, 0.0, 0.0 }, 100, 750000.0).ToArray();
            
            CollectionAssert.AreEqual(Enumerable.Range(0, 100), neighbours);
        }
        
        #endregion
    }
}
