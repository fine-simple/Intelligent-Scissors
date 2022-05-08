using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IntelligentScissors
{
    public partial class MainForm : Form
    {
        bool FreqEnabled = false;
        int Frequency = -1;
        Point freePoint;
        Pen pen;
        List<Point> lasso;
        RGBPixel[,] ImageMatrix;
        // Shortest Path between each Anchor and the one before it (starts from second anchor)
        Dictionary<Point, List<Point>> AnchorPaths;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                double sigma = 1;
                int maskSize = 3;
                //ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();

            Graph.Init(ImageMatrix, 4);

            //check if zoom mode vertical scaled or horizontal scaled
            float imageRatio = pictureBox1.Image.Width / (float)pictureBox1.Image.Height;
            float boxRatio = pictureBox1.Width / (float)pictureBox1.Height;
            if (imageRatio >= boxRatio)
            {
                DrawHelpers.horizontalScaled = true;
                DrawHelpers.scaleFactor = pictureBox1.Width / (float)pictureBox1.Image.Width;
                float scaledSize = pictureBox1.Image.Height * DrawHelpers.scaleFactor;
                DrawHelpers.filler = Math.Abs(pictureBox1.Height - scaledSize) / 2;
            }
            else
            {
                DrawHelpers.scaleFactor = pictureBox1.Height / (float)pictureBox1.Image.Height;
                float scaledSize = pictureBox1.Image.Width * DrawHelpers.scaleFactor;
                DrawHelpers.filler = Math.Abs(pictureBox1.Width - scaledSize) / 2;
            }
        }
        
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (ImageMatrix == null)
                return;

            for (int i = 0; i < lasso.Count; i++)
            {
                if (AnchorPaths.ContainsKey(DrawHelpers.unscaledPos(lasso[i])))
                    DrawPath(AnchorPaths[DrawHelpers.unscaledPos(lasso[i])], e);
                e.Graphics.DrawRectangle(pen, DrawHelpers.getAnchorRect(lasso[i]));
            }
            if(lasso.Count > 0)
                DrawLiveWire(e);
        }

        private void DrawLiveWire(PaintEventArgs pe)
        {
            Point p = DrawHelpers.unscaledPos(freePoint);
            List<Point> LiveWire = ShortestPathHelpers.GetShortestPath(DrawHelpers.unscaledPos(lasso[lasso.Count - 1]), p, Graph.adj);
            if (FreqEnabled && LiveWire.Count >= Frequency && Graph.validIndex(p.Y, p.X))
                updateLasso();
            DrawPath(LiveWire, pe);
        }
        private void DrawPath(List<Point>path, PaintEventArgs e)
        {
            for (int i = 1; i < path.Count; i++)
            {
                e.Graphics.DrawLine(pen, path[i - 1], path[i]);
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

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (ImageMatrix == null) // first click on picture
                return;
            if (e.Button == MouseButtons.Left)
                updateLasso();
            else if (e.Button == MouseButtons.Right)
            {
                if(lasso.Count > 1)
                    lasso.RemoveAt(lasso.Count - 1);
                else
                    lasso.Clear();
            }
            // used to force the picture box to re-draw (aka call pictureBox1_Paint)
            pictureBox1.Invalidate();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            pen = new Pen(Color.FromArgb(255, 255, 0, 0));
            testsBox.Items.Add("Sample1");
            testsBox.Items.Add("Sample2");
            testsBox.Items.Add("Sample3");
            testsBox.Items.Add("Complete1");
            testsBox.Items.Add("Complete2");

            testsBox.SelectedIndex = 0;
            lasso = new List<Point>();
            AnchorPaths = new Dictionary<Point, List<Point>>();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = DrawHelpers.unscaledPos(e.Location);
            txtMousePos.Text = $"{p.X}, {p.Y}";
            // mouse within image boundries
            if (p.X < 0 || p.X > pictureBox1.Image.Width - 1
             || p.Y < 0 || p.Y > pictureBox1.Image.Height - 1)
                return;
            freePoint = e.Location;
            pictureBox1.Invalidate();
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
            lasso.Add(freePoint);
            updateAnchorPaths();
        }

        private void updateAnchorPaths()
        {
            if (lasso.Count < 2)
                return;
            Point srcAnchor = DrawHelpers.unscaledPos(lasso[lasso.Count - 2]);
            Point destAnchor = DrawHelpers.unscaledPos(lasso[lasso.Count - 1]);

            AnchorPaths[destAnchor] = ShortestPathHelpers.GetShortestPath(srcAnchor, destAnchor, Graph.adj);
        }
    }
}