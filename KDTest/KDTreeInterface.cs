namespace KDTest
{
    using System;
    using NUnit.Framework;
    using KDTree;
    
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
            
            Assert.IsNotNull(tree);
        }

        [Test]
        public void InstantiateKDTree_2Dimensions()
        {
            var tree = new KDTree<int>(2);
            
            Assert.IsNotNull(tree);
        }

        [Test]
        public void InstantiateKDTree_3Dimensions()
        {
            var tree = new KDTree<int>(3);
            
            Assert.IsNotNull(tree);
        }

        [Test]
        public void InstantiateKDTree_NoBucketCapacity()
        {
            var tree = new KDTree<int>(1, 0);
            
            Assert.IsNotNull(tree);
        }
        
        [Test]
        public void InstantiateKDTree_8BucketCapacity()
        {
            var tree = new KDTree<int>(1, 8);
            
            Assert.IsNotNull(tree);
        }
        
        #endregion
        
        #region Element Add

        [Test]
        public void AddKDTreeElement_OneElementSize()
        {
            var tree = new KDTree<int>(3, 8);
            
            tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            Assert.AreEqual(1, tree.Size);
        }
        
        [Test]
        public void AddKDTreeElement_TenElementSize()
        {
            var tree = new KDTree<int>(3, 8);
            
            for (int i = 0 ; i < 10 ; i++)
                tree.AddPoint(new [] { 0.0, 0.0, 0.0 }, 0);
            
            Assert.AreEqual(10, tree.Size);
        }
        
        #endregion
    }
}