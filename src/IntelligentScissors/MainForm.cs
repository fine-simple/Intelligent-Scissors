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
        Point anchorPoint, lastPoint, currentPoint;
        Pen pen;
        List<Point> lasso;
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();

            Graph.Init(ImageMatrix, 4);
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            pen = new Pen(Color.FromArgb(255, 255, 0, 0));
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (EnableLasso)
            {
                for (int i = 0; i < lasso.Count - 1; i++)
                {
                    e.Graphics.DrawLine(pen, lasso[i], lasso[i + 1]);
                }
            }
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        { 
            if (!EnableLasso) // first click on picture
            {
                initializeLasso(e.Location);
                mousePos.Text = anchorPoint.ToString();

            }
            else
            {
                updateLasso(e.Location);
                mousePos.Text = currentPoint.ToString();
            }

            // used to force the picture box to re-draw (aka call pictureBox1_Paint)
            pictureBox1.Invalidate();
        }

        private void initializeLasso(Point mousePosition)
        {
            lasso = new List<Point>();
            anchorPoint = new Point();
            anchorPoint = mousePosition;
            lastPoint = anchorPoint;

            currentPoint = new Point();
            currentPoint = anchorPoint;

            lasso.Add(lastPoint);
            lasso.Add(currentPoint);

            EnableLasso = true;
        }

        private void updateLasso(Point mousePosition)
        {
            lastPoint = currentPoint;
            currentPoint = mousePosition;

            lasso.Add(currentPoint);
        }
    }
}