namespace KDTest
{
    using System;
    using System.Linq;
    using KDTree;
    using KDTree.DistanceFunctions;
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
        
        #region Default Distance       
        [Test]
        public void EmptyTree_EmptyNearestNeighbors()
        {
            var tree = new KDTree<int>(3, 2);
            
            var neighbours = tree.NearestNeighbors(new [] { 0.0, 0.0, 0.0 }, 100).ToArray();
            
            CollectionAssert.IsEmpty(neighbours);
        }
        
        [Test]
        public void NoDistance_OneNearestNeighbors()
        {
            var tree = new KDTree<int>(3, 2);
            
            tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            var neighbours = tree.NearestNeighbors(new [] { 1000.0, 1000.0, 1000.0 }, 100).ToArray();
            
            CollectionAssert.Contains(neighbours, 0);
        }
        
        [Test]
        public void NoDistance_HundredNearestNeighbors()
        {
            var tree = new KDTree<double>(3, 2);
            
            for (double d = 0 ; d < 1000 ; d++)
                tree.AddPoint(new [] { d, d, d }, d);
            
            var neighbours = tree.NearestNeighbors(new [] { 0.0, 0.0, 0.0 }, 100).ToArray();
            
            CollectionAssert.AreEqual(Enumerable.Range(0, 100), neighbours);
        }
        
        [Test]
        public void ZeroDistance_TenNearestNeighbors()
        {
            var tree = new KDTree<int>(3, 2);
            
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
            var tree = new KDTree<double>(3, 2);
            
            for (double d = 0 ; d < 1000 ; d++)
                tree.AddPoint(new [] { d, d, d }, d);
            
            var neighbours = tree.NearestNeighbors(new [] { 0.0, 0.0, 0.0 }, 100, 243.0).ToArray();
            
            CollectionAssert.AreEqual(Enumerable.Range(0, 10), neighbours);
        }
        
        [Test]
        public void LotDistance_HundredNearestNeighbors()
        {
            var tree = new KDTree<double>(3, 2);
            
            for (double d = 0 ; d < 1000 ; d++)
                tree.AddPoint(new [] { d, d, d }, d);
            
            var neighbours = tree.NearestNeighbors(new [] { 0.0, 0.0, 0.0 }, 100, 750000.0).ToArray();
            
            CollectionAssert.AreEqual(Enumerable.Range(0, 100), neighbours);
        }
        
        #endregion
        
        #region WithTranslation
        [Test]
        public void WithTranslation_EmptyTree_EmptyNearestNeighbors()
        {
            var tree = new KDTree<int>(new SquaredEuclideanDistanceWithTranslation(() => DateTime.UtcNow.Ticks, 3), 7, 2);
            
            var neighbours = tree.NearestNeighbors(new [] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }, 100).ToArray();
            
            CollectionAssert.IsEmpty(neighbours);
        }
        
        [Test]
        public void WithTranslation_TenPoints_FindTowardsCenter()
        {
            var tree = new KDTree<int>(7, 2);
            
            foreach (var point in Enumerable.Range(1, 5))
            {
                tree.AddPoint(new [] { 10.0, 10.0, 10.0, 0.0, 0.0, 0.0, 0.0 }, point);
            }
            
            foreach (var point in Enumerable.Range(6, 5))
            {
                tree.AddPoint(new [] { 10.0, 10.0, 10.0, -0.5, -0.5, -0.5, 0.0 }, point);
            }
            
            CollectionAssert.IsEmpty(tree.NearestNeighbors(new SquaredEuclideanDistanceWithTranslation(() => 0, 3), new [] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }, 100, 299.0));
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 10), tree.NearestNeighbors(new SquaredEuclideanDistanceWithTranslation(() => 0, 3), new [] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }, 100, 300.0));
            CollectionAssert.AreEquivalent(Enumerable.Range(6, 5), tree.NearestNeighbors(new SquaredEuclideanDistanceWithTranslation(() => 20, 3), new [] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }, 100, 1.0));
        }
        
        [Test]
        public void WithTranslation_TenPoints_NothingTowardsCenter()
        {
            var tree = new KDTree<int>(7, 2);
            
            foreach (var point in Enumerable.Range(1, 5))
            {
                tree.AddPoint(new [] { 10.0, 10.0, 10.0, 0.0, 0.0, 0.0, 0.0 }, point);
            }
            
            foreach (var point in Enumerable.Range(6, 5))
            {
                tree.AddPoint(new [] { 10.0, 10.0, 10.0, 0.5, 0.5, 0.5, 0.0 }, point);
            }
            
            CollectionAssert.IsEmpty(tree.NearestNeighbors(new SquaredEuclideanDistanceWithTranslation(() => 0, 3), new [] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }, 100, 299.0));
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 10), tree.NearestNeighbors(new SquaredEuclideanDistanceWithTranslation(() => 0, 3), new [] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }, 100, 300.0));
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 5), tree.NearestNeighbors(new SquaredEuclideanDistanceWithTranslation(() => 20, 3), new [] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }, 100, 599));
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 10), tree.NearestNeighbors(new SquaredEuclideanDistanceWithTranslation(() => 0, 3), new [] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 }, 100, 600.0));
        }
        #endregion
    }
}
