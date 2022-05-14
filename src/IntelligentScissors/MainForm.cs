using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        List<Point> lasso;
        RGBPixel[,] ImageMatrix;
        // Shortest Path between each Anchor and the one before it (starts from second anchor)
        Dictionary<Point, List<Point>> AnchorPaths;

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
            }));
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            //Open the browsed image and display it
            string OpenedFilePath = openFileDialog1.FileName;
            ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
            double sigma = 1;
            int maskSize = 3;
            //ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
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
        }
        
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (ImageMatrix == null)
                return;

            for (int i = 0; i < lasso.Count; i++)
            {
                if (AnchorPaths.ContainsKey(DrawHelpers.unscaledPos(lasso[i])))
                    DrawPath(AnchorPaths[DrawHelpers.unscaledPos(lasso[i])], e);
                if (lassoEnabled)
                    e.Graphics.DrawRectangle(drawPen, DrawHelpers.getAnchorRect(lasso[i]));
            }
            if(lasso.Count > 0 && lassoEnabled)
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
            Pen pen = lassoEnabled ? drawPen : finalPen;
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

                if(lasso.Count > 1)
                    lasso.RemoveAt(lasso.Count - 1);
                else
                    lasso.Clear();
            }
            if (!lassoEnabled)
                return;
            else if (e.Button == MouseButtons.Left)
                updateLasso();

            // used to force the picture box to re-draw (aka call pictureBox1_Paint)
            pictureBox1.Invalidate();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            drawPen = new Pen(Color.FromArgb(255, 255, 0, 0));
            finalPen = new Pen(Color.FromArgb(255, 0, 255, 0));
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
            if (ImageMatrix == null)
                return;
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
            Point lastAnchor = freePoint;
            lasso.Add(lastAnchor);
            updateAnchorPaths();
            if (lasso.Count > 1 && DrawHelpers.getAnchorRect(lastAnchor).IntersectsWith(DrawHelpers.getAnchorRect(lasso[0])))
                disableLasso();
        }

        private void cropBtn_Click(object sender, EventArgs e)
        {
            // Add all points in one list
            List<Point> points = new List<Point>(AnchorPaths.Values.Count);
            for (int i=1; i < lasso.Count; i++) {
                Point unscaled = DrawHelpers.unscaledPos(lasso[i]);
                foreach (var point in AnchorPaths[unscaled])
                {
                    points.Add(DrawHelpers.unscaledPos(point));
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
            Point srcAnchor = DrawHelpers.unscaledPos(lasso[lasso.Count - 2]);
            Point destAnchor = DrawHelpers.unscaledPos(lasso[lasso.Count - 1]);

            AnchorPaths[destAnchor] = ShortestPathHelpers.GetShortestPath(srcAnchor, destAnchor, Graph.adj);
        }
    }
}