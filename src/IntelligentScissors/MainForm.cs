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

            Point controlLoc = this.PointToScreen(pictureBox1.Location); // location of the pitcture to screen(not form)
            /* last and current points co-ordinates are relative to picture however drawing is relative to
               the form window
               TODO: Map the co-ordinates to display the line on the picture
             */
            e.Graphics.DrawLine(pen, (float)lastPoint.X, (float)lastPoint.Y, (float)currentPoint.X, (float)currentPoint.Y);
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        { 
            if (!EnableLasso) // first click on picture
            {
                anchorPoint = new Point();
                anchorPoint.X = e.Location.X;
                anchorPoint.Y = e.Location.Y;
                lastPoint = anchorPoint;

                currentPoint = new Point();
                currentPoint.X = e.Location.X;
                currentPoint.Y = e.Location.Y;
                currentPoint = anchorPoint;

                mousePos.Text = anchorPoint.ToString();

                EnableLasso = true;

            }
            else
            {
                lastPoint.X = currentPoint.X;
                lastPoint.Y = currentPoint.Y;

                currentPoint.X = e.Location.X;
                currentPoint.Y = e.Location.Y;

                mousePos.Text = currentPoint.ToString();
            }

            // used to force the form to re-draw (aka call MainForm_Paint)
            this.Invalidate();
        }
    }
}