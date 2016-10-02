namespace KDVisuals
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Timers;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int TimerSecondResolution = 10000000 * 2;
        
        object LockSync = new object();
        
        /// <summary>
        /// KD-Tree which stores the points.
        /// </summary>
        KDTree.KDTree<EllipseWrapper> Tree;

        /// <summary>
        /// Bitmap which renders them quickly.
        /// </summary>
        WriteableBitmap Bitmap;
        
        int NumNeighbours;
        
        double DistThreshold;
        
        Point MousePosition;
        
        Random Random = new Random();
        
        Timer MoveChange;
        
        double RandomSpeed { get { return (Random.NextDouble() - 0.5) / TimerSecondResolution; } }
        
        /// <summary>
        /// A data item which is stored in each kd node.
        /// </summary>
        class EllipseWrapper
        {
            public double x, y, speedx, speedy, start;

            public EllipseWrapper(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public EllipseWrapper(double x, double y, double speedx, double speedy, double start)
                : this(x, y)
            {
                this.speedx = speedx;
                this.speedy = speedy;
                this.start = start;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// On Window Created Attach to Render Event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            CompositionTarget.Rendering += this.OnUpdate;
        }
        
        /// <summary>
        /// On Window Destoyed Dettach from Render Event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            CompositionTarget.Rendering -= this.OnUpdate;
        }
        
        /// <summary>
        /// On Size Changed Regen Canvas Bitmap
        /// </summary>
        /// <param name="sizeInfo"></param>
        
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            RegenBitmap();
        }
        
        void OnUpdate(object sender, object e)
        {
            DrawTree();
        }
        
        void RegenBitmap()
        {
            lock (LockSync)
            {
                Bitmap = BitmapFactory.New((int)cnvPoints.ActualWidth, (int)cnvPoints.ActualHeight);
                var pBrush = new ImageBrush();
                pBrush.ImageSource = Bitmap;
                cnvPoints.Background = pBrush;
                
                // Clear the bitmap to light blue.
                using (Bitmap.GetBitmapContext())
                    Bitmap.Clear(Colors.LightBlue);
                
                // Clear Canvas
                cnvPoints.Children.Clear();
            }
        }
        
        void DrawTree()
        {
            // Bail if nothing to draw on.
            if (Bitmap == null || Tree == null)
                return;
            
            lock (LockSync)
            {
                RegenUIValues();
                var cnvWidth = cnvPoints.ActualWidth;
                var cnvHeight = cnvPoints.ActualHeight;
                
                // Get Squared Threshold if not negative
                var threshold = DistThreshold / cnvWidth;
                if (DistThreshold >= 0.0)
                    threshold *= threshold;
                    
                // Get the drawing context.
                using (Bitmap.GetBitmapContext())
                {
                    // Clean Up
                    Bitmap.Clear(Colors.LightBlue);
                    
                    // Match Neighbours
                    var matched = Tree.Dimensions > 2
                        ? Tree.NearestNeighbors(new [] { MousePosition.X / cnvWidth, MousePosition.Y / cnvHeight, 0.0, 0.0, 0.0 }, NumNeighbours, threshold).ToArray()
                        : Tree.NearestNeighbors(new [] { MousePosition.X / cnvWidth, MousePosition.Y / cnvHeight }, NumNeighbours, threshold).ToArray();
                    
                    foreach (var match in matched)
                    {
                        var positionX = match.x;
                        var positionY = match.y;
                        
                        if (Tree.Dimensions > 2)
                        {
                            var elapsed = DateTime.UtcNow.Ticks - match.start;
                            positionX += match.speedx * elapsed;
                            positionY += match.speedy * elapsed;
                        }
                        
                        if (positionX > 1.0 || positionX < 0.0 || positionY > 1.0 || positionY < 0.0)
                            continue;
                        
                        Bitmap.FillEllipse((int)(positionX * cnvWidth - 2), (int)(positionY * cnvHeight - 2), (int)(positionX * cnvWidth + 2), (int)(positionY * cnvHeight + 2), Colors.Red);
                        Bitmap.DrawEllipse((int)(positionX * cnvWidth - 2), (int)(positionY * cnvHeight - 2), (int)(positionX * cnvWidth + 2), (int)(positionY * cnvHeight + 2), Colors.Green);
                    }
                    
                    foreach (var nomatch in Tree.Except(matched))
                    {
                        var positionX = nomatch.x;
                        var positionY = nomatch.y;
                        
                        if (Tree.Dimensions > 2)
                        {
                            var elapsed = DateTime.UtcNow.Ticks - nomatch.start;
                            positionX += nomatch.speedx * elapsed;
                            positionY += nomatch.speedy * elapsed;
                        }
                        
                        if (positionX > 1.0 || positionX < 0.0 || positionY > 1.0 || positionY < 0.0)
                            continue;
                        
                       Bitmap.DrawEllipse((int)(positionX * cnvWidth - 2), (int)(positionY * cnvHeight - 2), (int)(positionX * cnvWidth + 2), (int)(positionY * cnvHeight + 2), Colors.Orange);
                    }
                    
                    // Draw Cursor
                    Bitmap.DrawEllipse((int)(MousePosition.X - DistThreshold), (int)(MousePosition.Y - (DistThreshold / cnvWidth) * cnvHeight), (int)(MousePosition.X + DistThreshold), (int)(MousePosition.Y + (DistThreshold / cnvWidth) * cnvHeight), Colors.Black);
                }
            }
        }
        
        void RegenUIValues()
        {
            txtFindMax.Foreground = Brushes.Black;
            if (!int.TryParse(txtFindMax.Text, out NumNeighbours))
                txtFindMax.Foreground = Brushes.Red;

            txtFindThreshold.Foreground = Brushes.Black;
            if (!double.TryParse(txtFindThreshold.Text, out DistThreshold))
                txtFindThreshold.Foreground = Brushes.Red;
        }

        /// <summary>
        /// Randomise the layout of points.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button_Click(object sender, RoutedEventArgs e)
        {
            RegenBitmap();
            
            lock (LockSync)
            {
                // Get the number we want to generate and update the UI.
                var iResult = 0;
                
                if (!int.TryParse(txtPoints.Text, out iResult))
                {
                    txtPoints.Foreground = Brushes.Red;
                    return;
                }
                
                if (iResult < 0)
                {
                    txtPoints.Foreground = Brushes.Red;
                    return;
                }
                
                txtPoints.Foreground = Brushes.Black;
                
                // Regen Tree
                if (Tree != null && Tree.Dimensions > 2)
                {
                    Tree = new KDTree.KDTree<EllipseWrapper>(new KDTree.DistanceFunctions.SquaredEuclideanDistanceWithTranslation(() => DateTime.UtcNow.Ticks, 2), 5);
                }
                else
                {
                    Tree = new KDTree.KDTree<EllipseWrapper>(2);
                }
                
                // Add Point to the tree
                var pRandom = new Random();
                for (int i = 0; i < iResult; ++i)
                {
                    // Position it and add it to the canvas.
                    var x = pRandom.NextDouble();
                    var y = pRandom.NextDouble();
                    if (Tree.Dimensions > 2)
                    {
                        var speedx = RandomSpeed;
                        var speedy = RandomSpeed;
                        var time = DateTime.UtcNow.Ticks;
                        Tree.AddPoint(new [] { x, y, speedx, speedy, time }, new EllipseWrapper(x, y, speedx, speedy, time));
                    }
                    else
                    {
                        Tree.AddPoint(new [] { x, y }, new EllipseWrapper(x, y));
                    }
                }
            }
        }
        
        /// <summary>
        /// Randomize Moving Points
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Move_Click(object sender, RoutedEventArgs e)
        {
            // Bail if no tree...
            if (Tree == null)
                return;
            
            RegenBitmap();
            
            if (MoveChange != null)
            {
                MoveChange.Stop();
                MoveChange = null;
            }
            
            lock (LockSync)
            {
                if (Tree.Dimensions > 2)
                {
                    var data = Tree.ToArray();
                    Tree = new KDTree.KDTree<EllipseWrapper>(2);
                    foreach (var item in data)
                    {
                        var elapsed = DateTime.UtcNow.Ticks - item.start;
                        var nomove = new EllipseWrapper(item.x + item.speedx * elapsed, item.y + item.speedy * elapsed);
                        Tree.AddPoint(new [] { nomove.x, nomove.y }, nomove);
                    }
                }
                else
                {
                    var data = Tree.ToArray();
                    Tree = new KDTree.KDTree<EllipseWrapper>(new KDTree.DistanceFunctions.SquaredEuclideanDistanceWithTranslation(() => DateTime.UtcNow.Ticks, 2), 5);
                    
                    var rand = new Random();
                    foreach (var item in data)
                    {
                        var speedx = RandomSpeed;
                        var speedy = RandomSpeed;
                        var move = new EllipseWrapper(item.x, item.y, speedx, speedy, DateTime.UtcNow.Ticks);
                        Tree.AddPoint(new [] { move.x, move.y, move.speedx, move.speedy, move.start }, move);
                    }
                    
                    MoveChange = new Timer(1000);
                    MoveChange.Elapsed += (object sndr, ElapsedEventArgs ev) => ChangeMovement();
                    MoveChange.Start();
                }
            }
        }
        
        void ChangeMovement()
        {
            lock (LockSync)
            {
                if (Tree.Dimensions > 2)
                {
                    foreach (var item in Tree)
                    {
                        var elapsed = DateTime.UtcNow.Ticks - item.start;
                        item.x += item.speedx * elapsed;
                        item.y += item.speedy * elapsed;
                        item.speedx = RandomSpeed;
                        item.speedy = RandomSpeed;
                        
                        item.start = DateTime.UtcNow.Ticks;
                        Tree.MovePoint(new [] { item.x, item.y, item.speedx, item.speedy, item.start }, item);
                    }
                }
            }
        }

        /// <summary>
        /// When the mouse is moved, highlight the nearby nodes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cnvPoints_MouseMove(object sender, MouseEventArgs e)
        {
            // Get Current MousePoint
            MousePosition = e.GetPosition(cnvPoints);
        }
    }
}
