using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chart
{
    partial class Dialog : Form
    {
        public Dialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PLik PNG (*.png)|*.png";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveDialog.FileName;
                chart1.SaveImage(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
