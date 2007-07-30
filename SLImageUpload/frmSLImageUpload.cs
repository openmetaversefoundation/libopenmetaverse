using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using libsecondlife;

namespace SLImageUpload
{
    public partial class frmSLImageUpload : Form
    {
        SecondLife Client;
        AssetManager Assets;
        byte[] UploadData = null;
        int Transferred = 0;

        public frmSLImageUpload()
        {
            InitializeComponent();

            Client = new SecondLife();
            Assets = new AssetManager(Client);
            Assets.OnUploadProgress += new AssetManager.UploadProgressCallback(Assets_OnUploadProgress);
            Assets.OnAssetUploaded += new AssetManager.AssetUploadedCallback(Assets_OnAssetUploaded);

            Client.Settings.MULTIPLE_SIMS = false;
            Client.Self.Status.Camera.Far = 32.0f;
            Client.Throttle.Cloud = 0.0f;
            Client.Throttle.Land = 0.0f;
            Client.Throttle.Wind = 0.0f;
        }

        private void EnableUpload(bool enable)
        {
            if (UploadData != null) cmdUpload.Enabled = enable;
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            if (cmdConnect.Text == "Connect")
            {
                cmdConnect.Text = "Disconnect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = false;

                if (Client.Network.Login(txtFirstName.Text, txtLastName.Text, txtPassword.Text, "SL Image Upload",
                    "jhurliman@metaverseindustries.com"))
                {
                    EnableUpload(true);
                }
                else
                {
                    MessageBox.Show(this, String.Format("Error logging in ({0}): {1}", Client.Network.LoginErrorKey,
                        Client.Network.LoginMessage));
                    cmdConnect.Text = "Connect";
                    txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                    EnableUpload(false);
                }
            }
            else
            {
                Client.Network.Logout();
                cmdConnect.Text = "Connect";
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                EnableUpload(false);
            }
        }

        private void cmdLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = 
                "Image Files (*.jp2,*.j2c,*.jpg,*.jpeg,*.gif,*.png,*.bmp,*.tga,*.tif,*.tiff,*.ico,*.wmf,*.emf)|" + 
                "*.jp2;*.j2c;*.jpg;*.jpeg;*.gif;*.png;*.bmp;*.tga;*.tif;*.tiff;*.ico;*.wmf;*.emf;";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string lowfilename = dialog.FileName.ToLower();
                Bitmap bitmap = null;

                try
                {
                    if (lowfilename.EndsWith(".jp2") || lowfilename.EndsWith(".j2c"))
                    {
                        // Upload JPEG2000 images untouched
                        UploadData = System.IO.File.ReadAllBytes(dialog.FileName);
                        bitmap = (Bitmap)OpenJPEGNet.OpenJPEG.DecodeToImage(UploadData);
                    }
                    else
                    {
                        if (lowfilename.EndsWith(".tga"))
                            bitmap = OpenJPEGNet.LoadTGAClass.LoadTGA(dialog.FileName);
                        else
                            bitmap = (Bitmap)Image.FromFile(dialog.FileName);

                        // Handle resizing to prevent excessively large images
                        if (bitmap.Width > 1024 || bitmap.Height > 1024)
                        {
                            int newwidth = (bitmap.Width > 1024) ? 1024 : bitmap.Width;
                            int newheight = (bitmap.Height > 1024) ? 1024: bitmap.Height;

                            Bitmap resized = new Bitmap(newwidth, newheight, bitmap.PixelFormat);
                            Graphics graphics = Graphics.FromImage(resized);

                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.InterpolationMode =
                               System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(bitmap, 0, 0, newwidth, newheight);

                            bitmap.Dispose();
                            bitmap = resized;
                        }

                        UploadData = OpenJPEGNet.OpenJPEG.EncodeFromImage(bitmap, chkLossless.Checked);
                    }
                }
                catch (Exception ex)
                {
                    UploadData = null;
                    cmdUpload.Enabled = false;
                    MessageBox.Show(ex.ToString(), "SL Image Upload", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                picPreview.Image = bitmap;
                lblSize.Text = Math.Round((double)UploadData.Length / 1024.0d, 2) + "KB";
                prgUpload.Maximum = UploadData.Length;
                if (Client.Network.Connected) cmdUpload.Enabled = true;
            }
        }

        private void cmdUpload_Click(object sender, EventArgs e)
        {
            if (UploadData != null)
            {
                prgUpload.Value = 0;
                cmdLoad.Enabled = false;
                cmdUpload.Enabled = false;
                grpLogin.Enabled = false;

                LLUUID assetID;
                LLUUID transactionid = Assets.RequestUpload(out assetID, AssetType.Texture, UploadData, false, false, true);
                txtAssetID.Text = assetID.ToStringHyphenated();
            }
        }

        private void Assets_OnUploadProgress(AssetUpload upload)
        {
            Transferred = upload.Transferred;

            if (this.InvokeRequired)
                BeginInvoke(new MethodInvoker(SetProgress));
            else
                SetProgress();
        }

        private void SetProgress()
        {
            prgUpload.Value = Transferred;
        }

        private void Assets_OnAssetUploaded(AssetUpload upload)
        {
            if (this.InvokeRequired)
                BeginInvoke(new MethodInvoker(EnableControls));
            else
                EnableControls();

            // Pay for the upload
            Client.Self.GiveMoney(LLUUID.Zero, Client.Settings.UPLOAD_COST, "SL Image Upload");

            if (upload.Success)
                MessageBox.Show("Image uploaded successfully");
            else
                MessageBox.Show("Image upload rejected (unknown cause)");

            // FIXME: Save this in to inventory and send it to the sendto name if there is one
        }

        private void EnableControls()
        {
            cmdLoad.Enabled = true;
            cmdUpload.Enabled = true;
            grpLogin.Enabled = true;
        }

        private void frmSLImageUpload_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Client.Network.Connected)
                Client.Network.Logout();
        }
    }
}
