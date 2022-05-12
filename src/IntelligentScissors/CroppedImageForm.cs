using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IntelligentScissors
{
    public partial class CroppedImageForm : Form
    {
        Image image;
        public CroppedImageForm()
        {
            InitializeComponent();
        }

        private void CroppedImageForm_Load(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(image);
            Bitmap finalImage = CropImageHelpers.CropImage(bmp, MainForm.lasso, MainForm.AnchorPaths);
            pictureBox1.Image = finalImage;
        }
        public void setImage(Image img)
        {
            if (image != null)
                return;
            image = img;
        }
    }
}
