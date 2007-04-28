using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Baker
{
    public partial class frmBaker : Form
    {
        public frmBaker()
        {
            InitializeComponent();
        }

        private void frmBaker_Load(object sender, EventArgs e)
        {
        	Stream stream = libsecondlife.Helpers.GetResourceStream("shirt_sleeve_alpha.tga");
        	
        	if (stream != null)
        	{
        		Bitmap alphaMask = OpenJPEGNet.LoadTGAClass.LoadTGA(stream);
        		pic1.Image = Oven.ModifyAlphaMask(alphaMask, 245, 0.0f);
        	}
        	else
        	{
        		;
        	}
        }

        private void frmBaker_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void cmdLoadPic_Click(object sender, EventArgs e)
        {
            Button caller = (Button)sender;
            PictureBox pic = null;

            switch (caller.Name)
            {
                case "cmdLoadPic1":
                    pic = pic1;
                    break;
                case "cmdLoadPic2":
                    pic = pic2;
                    break;
                case "cmdLoadPic3":
                    pic = pic3;
                    break;
            }

            if (pic != null)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "JPEG2000 (*.jp2,*.j2c,*.j2k)|";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] j2kdata = File.ReadAllBytes(dialog.FileName);
                        Image image = OpenJPEGNet.OpenJPEG.DecodeToImage(j2kdata);
                        pic.Image = image;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
    }
}
