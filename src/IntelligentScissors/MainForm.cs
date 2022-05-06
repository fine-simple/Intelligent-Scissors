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
        public bool EnableLasso = false;
        Point anchorPoint, lastPoint, currentPoint, freePoint;
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
                int maskSize = 2;
                ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();

            Graph.Init(ImageMatrix, 4);
        }

        private Rectangle getAnchorRect(Point mousePos)
        {
            int size = 5;
            return new Rectangle((int)(mousePos.X - 0.5 * size), (int)(mousePos.Y - 0.5 * size), size, size);
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (EnableLasso)
            {
                for (int i = 1; i < lasso.Count; i++)
                {
                    if (AnchorPaths.ContainsKey(lasso[i]))
                        DrawPath(AnchorPaths[lasso[i]], e);
                    e.Graphics.DrawRectangle(pen, getAnchorRect(lasso[i]));
                }

                List<Point> LiveWire = ShortestPathHelpers.GetShortestPath(lasso[lasso.Count - 1], freePoint, Graph.adj);
                DrawPath(LiveWire, e);
            }
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
            Test.Graph.sample(Graph.adj, Test.SampleType.Sample1);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!EnableLasso) // first click on picture
            {
                if(e.Button == MouseButtons.Left)
                    initializeLasso(e.Location);
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                    updateLasso(e.Location);
                else if (e.Button == MouseButtons.Right)
                {
                    if(lasso.Count > 2)
                        lasso.RemoveAt(lasso.Count - 1);
                    else
                    {
                        lasso.Clear();
                        EnableLasso = false;
                    }
                }
            }
            // used to force the picture box to re-draw (aka call pictureBox1_Paint)
            pictureBox1.Invalidate();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            pen = new Pen(Color.FromArgb(255, 255, 0, 0));
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            txtMousePos.Text = $"{e.Location.X}, {e.Location.Y}";
            freePoint = e.Location;
            if (EnableLasso)
                pictureBox1.Invalidate();
        }

        private void initializeLasso(Point mousePosition)
        {
            if (ImageMatrix == null)
                return;
            lasso = new List<Point>();
            anchorPoint = new Point();
            anchorPoint = mousePosition;
            lastPoint = anchorPoint;

            currentPoint = new Point();
            currentPoint = anchorPoint;

            lasso.Add(lastPoint);
            lasso.Add(currentPoint);

            EnableLasso = true;

            AnchorPaths = new Dictionary<Point, List<Point>>();
        }

        private void updateLasso(Point mousePosition)
        {
            lastPoint = currentPoint;
            currentPoint = mousePosition;

            lasso.Add(currentPoint);

            if (lasso.Count > 1)
                updateAnchorPaths();
        }

        private void updateAnchorPaths()
        {
            Point srcAnchor = lasso[lasso.Count - 2];
            Point destAnchor = lasso[lasso.Count - 1];

            AnchorPaths[destAnchor] = ShortestPathHelpers.GetShortestPath(srcAnchor, destAnchor, Graph.adj);
        }
    }
}