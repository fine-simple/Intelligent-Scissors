using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace IntelligentScissors
{
    public partial class MainForm : Form
    {
        Stopwatch sw;
        bool FreqEnabled = false;
        bool lassoEnabled = true;
        int Frequency = -1;
        Point freePoint;
        Pen drawPen, finalPen;
        RGBPixel[,] ImageMatrix;
        // Shortest Path between each Anchor and the one before it (starts from second anchor)
        LinkedList<KeyValuePair<Point, List<Point>>> lasso;
        HashSet<Point> anchors;
        List<Point> LiveWire;

        public MainForm()
        {
            InitializeComponent();
        }
        private void initGraph()
        {
            sw = new Stopwatch();
            sw.Start();
            Graph.Init(ImageMatrix, 4);
            sw.Stop();
            Invoke(new Action(() => {
                panel1.Show();
                graphInfoLbl.Text = $"Graph Constructed in {(sw.ElapsedMilliseconds / 1000.0).ToString()} seconds";
                //fileWriteLbl.Text = "Saving graph...";
            }));
            //using (StreamWriter sw = new StreamWriter("graph.txt"))
            //{
            //    sw.WriteLine("Graph");
            //    sw.WriteLine("Vertex [vertex number]: (Vertex, weight),..");
                
            //    for (int i = 0; i < Graph.adj.Length; i++)
            //    {
            //        sw.Write("Vertex " + i + ": ");
            //        for (int j = 0; j < Graph.adj[i].Count; j++)
            //        {
            //            sw.Write($"({Graph.adj[i][j].Key},{Graph.adj[i][j].Value})");
            //            if ( j < Graph.adj[i].Count - 1)
            //                sw.Write(",");
            //        }
            //        sw.Write("\n");
            //    }
            //}
            //Invoke(new Action(() =>
            //{
            //    fileWriteLbl.Text = "Saved to graph.txt";
            //}));
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            fileWriteLbl.Text = "";
            graphInfoLbl.Text = "";
            //Open the browsed image and display it
            string OpenedFilePath = openFileDialog1.FileName;
            ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox1);

            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();

            // set zoom mode if image smaller than container
            if (pictureBox1.Image.Width > pictureBox1.MinimumSize.Width || pictureBox1.Image.Height > pictureBox1.MinimumSize.Height)
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
                DrawHelpers.removeScaling();
            }
            else
            {
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                DrawHelpers.applyScaling(pictureBox1.Image.Width, pictureBox1.Image.Height, pictureBox1.Width, pictureBox1.Height);
            }

            // Construct Graph
            panel1.Hide();
            Thread graphConstructThread = new Thread(initGraph);
            graphConstructThread.Start();


            ShortestPathHelpers.setBounds(panel1.Width, panel1.Height);
        }
        
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (ImageMatrix == null)
                return;

            foreach(var anchor in lasso)
            {
                DrawPath(anchor.Value, e);
                if (lassoEnabled)
                    e.Graphics.DrawRectangle(drawPen, DrawHelpers.getAnchorRect(DrawHelpers.scaledPos(anchor.Key)));
            }
            if(lasso.Count > 0 && lassoEnabled)
                DrawLiveWire(e);
        }

        private void DrawLiveWire(PaintEventArgs pe)
        {
            LiveWire = ShortestPathHelpers.GetShortestPath(lasso.Last.Value.Key, freePoint, Graph.adj);

            if (FreqEnabled && LiveWire.Count >= Frequency && Graph.validIndex(freePoint.Y, freePoint.X))
                updateLasso();

            DrawPath(LiveWire, pe);
        }
        private void DrawPath(List<Point>path, PaintEventArgs e)
        {
            Pen pen = lassoEnabled ? drawPen : finalPen;

            for (int i = 1; i < path.Count; i++)
            {
                e.Graphics.DrawLine(pen, DrawHelpers.scaledPos(path[i - 1]), DrawHelpers.scaledPos(path[i]));
            }
        }
        
        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            //TODO: Add gui logic to choose type of test
            switch(testsBox.Text)
            {
                case "Sample1":
                    Test.Graph.sample(Graph.adj, Test.SampleType.Sample1);
                    break;
                case "Sample2":
                    Test.Graph.sample(Graph.adj, Test.SampleType.Sample2);
                    break;
                case "Sample3":
                    Test.Graph.sample(Graph.adj, Test.SampleType.Sample3);
                    break;
                case "Complete1":
                    Test.Graph.complete(Graph.adj, Test.CompleteType.Complete1);
                    break;
                case "Complete2":
                    Test.Graph.complete(Graph.adj, Test.CompleteType.Complete2);
                    break;

            }

        }
        private void enableLasso()
        {
            lassoEnabled = true;
            cropBtn.Enabled = false;
        }
        private void disableLasso()
        {
            lassoEnabled = false;
            cropBtn.Enabled = true;
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (ImageMatrix == null)
                return;

            if (e.Button == MouseButtons.Right)
            {
                enableLasso();
                if (lasso.Count > 1)
                {
                    anchors.Remove(lasso.Last.Value.Key);
                    lasso.RemoveLast();
                    lasso.Last.Value.Value.Clear();
                }
                else
                {
                    lasso.Clear();
                    anchors.Clear();
                }
            }
            else if (lassoEnabled && e.Button == MouseButtons.Left)
                updateLasso();

            // used to force the picture box to re-draw (aka call pictureBox1_Paint)
            pictureBox1.Invalidate();
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (ImageMatrix == null)
                return;
            Point p = DrawHelpers.unscaledPos(e.Location);
            txtMousePos.Text = $"{p.X}, {p.Y}";
            // mouse within image boundries
            if (p.X < 0 || p.X > pictureBox1.Image.Width - 1
             || p.Y < 0 || p.Y > pictureBox1.Image.Height - 1)
                return;
            freePoint = p;
            pictureBox1.Invalidate();
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs eMouse = (MouseEventArgs)e;
            if(eMouse.Button == MouseButtons.Left)
            {
                List<Point> LastToFirstPath = ShortestPathHelpers.GetShortestPath(lasso.Last.Value.Key, lasso.First.Value.Key, Graph.adj);
                lasso.AddLast(new KeyValuePair<Point, List<Point>>(lasso.First.Value.Key, LastToFirstPath));
                disableLasso();
            }
            else if(eMouse.Button == MouseButtons.Right)
            {
                lasso.Clear();
                anchors.Clear();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            drawPen = new Pen(Color.FromArgb(255, 255, 0, 0), 2);
            finalPen = new Pen(Color.FromArgb(255, 0, 255, 0), 2);
            testsBox.Items.Add("Sample1");
            testsBox.Items.Add("Sample2");
            testsBox.Items.Add("Sample3");
            testsBox.Items.Add("Complete1");
            testsBox.Items.Add("Complete2");
            testsBox.SelectedIndex = 0;

            lasso = new LinkedList<KeyValuePair<Point, List<Point>>>();
            anchors = new HashSet<Point>();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (freqTextBox.TextLength == 0)
                FreqEnabled = false;
            else
            {
                FreqEnabled = true;
                Frequency = int.Parse(freqTextBox.Text);
            }

        }

        private void updateLasso()
        {
            if (anchors.Contains(freePoint) && freePoint != lasso.First.Value.Key)
                return;
            lasso.AddLast(new KeyValuePair<Point, List<Point>>(freePoint, new List<Point>()));
            anchors.Add(freePoint);
            updateAnchorPaths();
            if (lasso.Count > 1 && DrawHelpers.getAnchorRect(lasso.Last.Value.Key).IntersectsWith(DrawHelpers.getAnchorRect(lasso.First.Value.Key)))
                disableLasso();
        }

        private void cropBtn_Click(object sender, EventArgs e)
        {
            // Add all points in one list
            List<Point> points = new List<Point>(lasso.Count);
            foreach(var anchor in lasso)
            {
                foreach (var point in anchor.Value)
                {
                    points.Add(point);
                }
            }
            Image cropped;
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddPolygon(points.ToArray());
                RectangleF cropRect = path.GetBounds();
                Image mask = new Bitmap(pictureBox1.Image.Width, pictureBox1.Image.Height);
                using(Graphics g = Graphics.FromImage(mask))
                {
                    g.SetClip(new Region(path), CombineMode.Replace);
                    g.DrawImage(pictureBox1.Image, 0, 0);
                }
                cropped = new Bitmap((int)cropRect.Width, (int)cropRect.Height);
                using (Graphics g = Graphics.FromImage(cropped))
                {
                    g.DrawImage(mask, new RectangleF(0, 0, cropRect.Width, cropRect.Height), cropRect, GraphicsUnit.Pixel);
                }
            }
            CroppedPreviewForm croppedPreview = new CroppedPreviewForm(cropped);
            croppedPreview.ShowDialog();
        }


        private void updateAnchorPaths()
        {
            if (lasso.Count < 2)
                return;
            Point srcAnchor = lasso.Last.Previous.Value.Key;
            Point destAnchor = lasso.Last.Value.Key;

            lasso.Last.Previous.Value = new KeyValuePair<Point, List<Point>>(lasso.Last.Previous.Value.Key, LiveWire);
        }

        #region smart anchor
        private double averageWeight(Point p1, Point p2)
        {

            //p1 = DrawHelpers.unscaledPos(p1);
            //p2 = DrawHelpers.unscaledPos(p2);

            int diff;
            double w1, w2;
            if (p1.X == p2.X) // horizontal (diff in column)
            {
                diff = Math.Abs(p1.Y - p2.Y);
                if (p1.Y > p2.Y) // p1 right of p2
                {
                    w1 = Graph.getNeighbourWeight(p1.Y, p1.X, Graph.Neighbour.Left);
                    w2 = Graph.getNeighbourWeight(p2.Y, p2.X, Graph.Neighbour.Right);
                }
                else
                {
                    w1 = Graph.getNeighbourWeight(p1.Y, p1.X, Graph.Neighbour.Right);
                    w2 = Graph.getNeighbourWeight(p2.Y, p2.X, Graph.Neighbour.Left);
                }
            }
            else // vertical (diff in row)
            {
                diff = Math.Abs(p1.X - p2.X);
                if (p1.X > p2.X) // p1 below p2
                {
                    w1 = Graph.getNeighbourWeight(p1.Y, p1.X, Graph.Neighbour.Top);
                    w2 = Graph.getNeighbourWeight(p2.Y, p2.X, Graph.Neighbour.Bot);
                }
                else
                {
                    w1 = Graph.getNeighbourWeight(p1.Y, p1.X, Graph.Neighbour.Bot);
                    w2 = Graph.getNeighbourWeight(p2.Y, p2.X, Graph.Neighbour.Top);
                }
            }
            return (w1 + w2) * diff / 2;
        }
        #endregion
    }
}