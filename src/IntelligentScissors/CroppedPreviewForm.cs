using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace IntelligentScissors
{
    public partial class CroppedPreviewForm : Form
    {
        Image cropped;
        public CroppedPreviewForm(Image cropped)
        {
            InitializeComponent();
            this.cropped = cropped;
        }

        private void CroppedPreviewForm_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = cropped;
            pictureBox1.Invalidate();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.AddExtension = true;
            dialog.Filter = "JPEG|*.jpg" +
                "|GIF|*.gif" +
                "|PNG|*.png" +
                "|BMP|*.bmp" +
                "|ICON|*.ico" +
                "|TIFF|*.tiff" +
                "|EMF|*.emf";
            dialog.DefaultExt = "jpg";
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                ImageFormat format;
                switch (dialog.FilterIndex)
                {
                    case 0:
                        format = ImageFormat.Jpeg;
                        break;
                    case 1:
                        format = ImageFormat.Png;
                        break;
                    case 2:
                        format = ImageFormat.Bmp;
                        break;
                    case 3:
                        format = ImageFormat.Icon;
                        break;
                    case 4:
                        format = ImageFormat.Tiff;
                        break;
                    case 5:
                        format = ImageFormat.Emf;
                        break;
                    case 6:
                        format = ImageFormat.Gif;
                        break;
                    default:
                        format = ImageFormat.Jpeg;
                        break;
                }
                Console.WriteLine(dialog.FilterIndex);
                pictureBox1.Image.Save(dialog.FileName, format);
                MessageBox.Show("Image Saved Successfully");
            }
        }
    }
}