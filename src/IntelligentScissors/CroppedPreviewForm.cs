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
    }
}
