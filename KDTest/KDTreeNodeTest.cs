namespace KDSharp.KDTest
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using KDSharp.KDTree;

    /// <summary>
    /// KDTreeNode Unit Tests.
    /// </summary>
    [TestFixture]
    public class KDTreeNodeTest
    {
        public KDTreeNodeTest()
        {
        }
        
        #region Constructors
        [Test]
        public void InstantiateKDTreeNode_ValidParams_InitialValues()
        {
            var node = new KDTreeNode(1, 1);
            
            Assert.AreEqual(1, node.Dimensions);
            Assert.AreEqual(1, node.BucketCapacity);
            Assert.AreEqual(0, node.Size);
            Assert.IsNull(node.Left);
            Assert.IsNull(node.Right);
            Assert.IsTrue(node.IsLeaf);
            Assert.IsTrue(node.SinglePoint);
        }
        
        [Test]
        public void InstantiateKDTreeNode_InvalidDimensions_Exception()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new KDTreeNode(0, 1));
        }
        
        [Test]
        public void InstantiateKDTreeNode_InvalidBucket_Exception()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => new KDTreeNode(1, 0));
        }
        #endregion
        
        #region Leaf Splitting
        [Test]
        public void LeafSplitting_EqualPoints_CannotSplit()
        {
            var tree = new KDTree<int>(1, 2);
            
            for (int i = 0 ; i < 4 ; ++i)
                tree.Add(i);
            
            Assert.IsTrue(tree.RootNode.IsLeaf);
        }
        
        [Test]
        public void LeafSplitting_DifferentPoints_Split()
        {
            var tree = new KDTree<int>(1, 2);
            
            for (int i = 0 ; i < 3 ; ++i)
                tree.AddPoint(new [] { (double)i }, i);
            
            Assert.IsFalse(tree.RootNode.IsLeaf);
            Assert.AreEqual(3, tree.RootNode.Size);
            Assert.IsFalse(tree.RootNode.SinglePoint);
        }
        
        [Test]
        public void LeafSplitting_DifferentPoints_BalancedSplit()
        {
            var tree = new KDTree<int>(1, 2);
            
            for (int i = 0 ; i < 4 ; ++i)
                tree.AddPoint(new [] { (double)i * (i % 2 == 0 ? -1 : 1) }, i);
            
            Assert.IsFalse(tree.RootNode.IsLeaf);
            Assert.AreEqual(2, tree.RootNode.Right.Size);
            Assert.AreEqual(2, tree.RootNode.Left.Size);
        }
        #endregion
        
        #region Indexer
        [Test]
        public void Indexer_InvalidIndex_Exception()
        {
            var node = new KDTreeNode(1, 1);
            int idx;
            Assert.Throws(typeof(ArgumentOutOfRangeException), () => idx = node[0]);
        }
        
        [Test]
        public void Indexer_IndexOnStem_Exception()
        {
            var tree = new KDTree<int>(1, 1);
            tree.AddPoint(new [] { 1.0 }, 1);
            tree.AddPoint(new [] { 2.0 }, 2);
            
            var node = tree.RootNode;
            int idx;
            Assert.Throws(typeof(NotSupportedException), () => idx = node[0]);
        }
        
        [Test]
        public void Indexer_FirstItem_BaseIndex()
        {
            var tree = new KDTree<int>(1, 1);
            tree.Add(1);
            var node = tree.RootNode;
            
            Assert.AreEqual(0, node[0]);
        }
        #endregion
    }
}
