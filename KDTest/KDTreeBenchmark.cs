namespace KDSharp.KDTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using KDSharp.KDTree;
    using NUnit.Framework;
    using System.Diagnostics;
    
    /// <summary>
    /// Benchmark of KDTree Methods
    /// </summary>
    [TestFixture, Explicit]
    public class KDTreeBenchmark
    {
        public KDTreeBenchmark()
        {
        }
        
        private static double GetRandomNumber(double minimum, double maximum)
        {
            var random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        
        private IEnumerable<Tuple<double, double, double, int>> RandomSparsedCollection(int count, double max)
        {
            var result = new List<Tuple<double, double, double, int>>(count);
            for (int i = 0 ; i < count ; i++)
                result.Add(new Tuple<double, double, double, int>(GetRandomNumber(0.0, max), GetRandomNumber(0.0, max), GetRandomNumber(0.0, max), i));
            
            return result;
        }
        
        private IEnumerable<Tuple<double, double, double, int>> RandomPackedCollection(int count, double max, int division)
        {
            var gridsize = max / division;
            var pergrid = count / (division * division * division);
            
            var result = new List<Tuple<double, double, double, int>>(count);
            for (int x = 0 ; x < division ; x++)
            {
                for (int y = 0 ; y < division ; y++)
                {
                    for (int z = 0 ; z < division ; z++)
                    {
                        foreach (var element in RandomSparsedCollection(pergrid, gridsize / 2)
                                 .Select(el => new Tuple<double, double, double, int>(el.Item1 + (x * gridsize) + gridsize / 2,
                                                                                      el.Item2 + (y * gridsize) + gridsize / 2,
                                                                                      el.Item3 + (z * gridsize) + gridsize / 2,
                                                                                      el.Item4)))
                            result.Add(element);
                    }
                }
            }
            
            return result;
        }
        
        [Test]
        public void SmallSparsedCollectionBenchmark()
        {
            Console.WriteLine("SmallSparsedCollectionBenchmark");
            var watchAdd = new Stopwatch();
            var watchNeighbours = new Stopwatch();
            
            for (int i = 0 ; i < 100 ; i++)
            {
                var rands = RandomSparsedCollection(512, 65535.0 * 8);
                var tree = new KDTree<int>(3, 8);
                
                watchAdd.Start();
                foreach (var element in rands)
                    tree.AddPoint(new [] { element.Item1, element.Item2, element.Item3 }, element.Item4);
                watchAdd.Stop();
                
                watchNeighbours.Start();
                for (int j = 0 ; j < 10 ; j++)
                    tree.NearestNeighbors(new [] { 65535.0 * (j - 1), 65535.0 * (j - 1), 65535.0 * (j - 1) }, 100, 65535.0).ToArray();
                watchNeighbours.Stop();
            }
            
            Console.WriteLine("Average Adding Time : {0}ms", watchAdd.ElapsedMilliseconds / 100.0);
            Console.WriteLine("Average Searching Neighbours Time: {0}ms", watchNeighbours.ElapsedMilliseconds / 100.0);
        }
        
        [Test]
        public void AverageSparsedCollectionBenchmark()
        {
            Console.WriteLine("AverageSparsedCollectionBenchmark");
            var watchAdd = new Stopwatch();
            var watchNeighbours = new Stopwatch();
            
            for (int i = 0 ; i < 50 ; i++)
            {
                var rands = RandomSparsedCollection(512 * 64, 65535.0 * 8);
                var tree = new KDTree<int>(3, 24);
                
                watchAdd.Start();
                foreach (var element in rands)
                    tree.AddPoint(new [] { element.Item1, element.Item2, element.Item3 }, element.Item4);
                watchAdd.Stop();
                
                watchNeighbours.Start();
                for (int j = 0 ; j < 10 ; j++)
                    tree.NearestNeighbors(new [] { 65535.0 * (j - 1), 65535.0 * (j - 1), 65535.0 * (j - 1) }, 100, 65535.0).ToArray();
                watchNeighbours.Stop();
            }
            
            Console.WriteLine("Average Adding Time : {0}ms", watchAdd.ElapsedMilliseconds / 50.0);
            Console.WriteLine("Average Searching Neighbours Time: {0}ms", watchNeighbours.ElapsedMilliseconds / 50.0);
        }
        
        [Test]
        public void BigSparsedCollectionBenchmark()
        {
            Console.WriteLine("BigSparsedCollectionBenchmark");
            var watchAdd = new Stopwatch();
            var watchNeighbours = new Stopwatch();
            
            for (int i = 0 ; i < 20 ; i++)
            {
                var rands = RandomSparsedCollection(512 * 64 * 16, 65535.0 * 8);
                var tree = new KDTree<int>(3, 64);
                
                watchAdd.Start();
                foreach (var element in rands)
                    tree.AddPoint(new [] { element.Item1, element.Item2, element.Item3 }, element.Item4);
                watchAdd.Stop();
                
                watchNeighbours.Start();
                for (int j = 0 ; j < 10 ; j++)
                    tree.NearestNeighbors(new [] { 65535.0 * (j - 1), 65535.0 * (j - 1), 65535.0 * (j - 1) }, 100, 65535.0).ToArray();
                watchNeighbours.Stop();
            }
            
            Console.WriteLine("Average Adding Time : {0}ms", watchAdd.ElapsedMilliseconds / 20.0);
            Console.WriteLine("Average Searching Neighbours Time: {0}ms", watchNeighbours.ElapsedMilliseconds / 20.0);
        }
        
        [Test]
        public void SmallPackedCollectionBenchmark()
        {
            Console.WriteLine("SmallPackedCollectionBenchmark");
            var watchAdd = new Stopwatch();
            var watchNeighbours = new Stopwatch();
            
            for (int i = 0 ; i < 100 ; i++)
            {
                var rands = RandomPackedCollection(512, 65535.0 * 8, 4);
                var tree = new KDTree<int>(3, 8);
                
                watchAdd.Start();
                foreach (var element in rands)
                    tree.AddPoint(new [] { element.Item1, element.Item2, element.Item3 }, element.Item4);
                watchAdd.Stop();
                
                watchNeighbours.Start();
                for (int j = 0 ; j < 10 ; j++)
                    tree.NearestNeighbors(new [] { 65535.0 * (j - 1), 65535.0 * (j - 1), 65535.0 * (j - 1) }, 100, 65535.0).ToArray();
                watchNeighbours.Stop();
            }
            
            Console.WriteLine("Average Adding Time : {0}ms", watchAdd.ElapsedMilliseconds / 100.0);
            Console.WriteLine("Average Searching Neighbours Time: {0}ms", watchNeighbours.ElapsedMilliseconds / 100.0);
        }
        
        [Test]
        public void AveragePackedCollectionBenchmark()
        {
            Console.WriteLine("AveragePackedCollectionBenchmark");
            var watchAdd = new Stopwatch();
            var watchNeighbours = new Stopwatch();
            
            for (int i = 0 ; i < 50 ; i++)
            {
                var rands = RandomPackedCollection(512 * 64, 65535.0 * 8, 4 * 2);
                var tree = new KDTree<int>(3, 24);
                
                watchAdd.Start();
                foreach (var element in rands)
                    tree.AddPoint(new [] { element.Item1, element.Item2, element.Item3 }, element.Item4);
                watchAdd.Stop();
                
                watchNeighbours.Start();
                for (int j = 0 ; j < 10 ; j++)
                    tree.NearestNeighbors(new [] { 65535.0 * (j - 1), 65535.0 * (j - 1), 65535.0 * (j - 1) }, 100, 65535.0).ToArray();
                watchNeighbours.Stop();
            }
            
            Console.WriteLine("Average Adding Time : {0}ms", watchAdd.ElapsedMilliseconds / 50.0);
            Console.WriteLine("Average Searching Neighbours Time: {0}ms", watchNeighbours.ElapsedMilliseconds / 50.0);
        }
        
        [Test]
        public void BigPackedCollectionBenchmark()
        {
            Console.WriteLine("BigPackedCollectionBenchmark");
            var watchAdd = new Stopwatch();
            var watchNeighbours = new Stopwatch();
            
            for (int i = 0 ; i < 20 ; i++)
            {
                var rands = RandomPackedCollection(512 * 64 * 16, 65535.0 * 8, 4 * 2 * 2);
                var tree = new KDTree<int>(3, 64);
                
                watchAdd.Start();
                foreach (var element in rands)
                    tree.AddPoint(new [] { element.Item1, element.Item2, element.Item3 }, element.Item4);
                watchAdd.Stop();
                
                watchNeighbours.Start();
                for (int j = 0 ; j < 10 ; j++)
                    tree.NearestNeighbors(new [] { 65535.0 * (j - 1), 65535.0 * (j - 1), 65535.0 * (j - 1) }, 100, 65535.0).ToArray();
                watchNeighbours.Stop();
            }
            
            Console.WriteLine("Average Adding Time : {0}ms", watchAdd.ElapsedMilliseconds / 20.0);
            Console.WriteLine("Average Searching Neighbours Time: {0}ms", watchNeighbours.ElapsedMilliseconds / 20.0);
        }
    }
}
