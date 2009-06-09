using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Http;
using OpenMetaverse.Imaging;

namespace GridImageUpload
{
    public partial class frmGridImageUpload : Form
    {
        private GridClient Client;
        private byte[] UploadData = null;
        private int Transferred = 0;
        private string FileName = String.Empty;
        private UUID SendToID;
        private UUID AssetID;

        public frmGridImageUpload()
        {
            InitializeComponent();

            // Add login entries to the login combo box
            cboLoginURL.Items.Add(Settings.AGNI_LOGIN_SERVER);
            cboLoginURL.Items.Add(Settings.ADITI_LOGIN_SERVER);
            cboLoginURL.SelectedIndex = 0;

            InitClient();
        }

        private void InitClient()
        {
            Client = new GridClient();
            Client.Network.OnEventQueueRunning += new NetworkManager.EventQueueRunningCallback(Network_OnEventQueueRunning);
            Client.Network.OnLogin += new NetworkManager.LoginCallback(Network_OnLogin);

            // Turn almost everything off since we are only interested in uploading textures
            Settings.LOG_LEVEL = Helpers.LogLevel.None;
            Settings.ALWAYS_DECODE_OBJECTS = false;
            Settings.ALWAYS_REQUEST_OBJECTS = false;
            Settings.SEND_AGENT_UPDATES = true;
            Client.Settings.OBJECT_TRACKING = false;
            Settings.STORE_LAND_PATCHES = false;
            Settings.MULTIPLE_SIMS = false;
            Client.Self.Movement.Camera.Far = 32.0f;
            Client.Throttle.Cloud = 0.0f;
            Client.Throttle.Land = 0.0f;
            Client.Throttle.Wind = 0.0f;

            Client.Throttle.Texture = 446000.0f;
        }

        private void EnableUpload()
        {
            if (UploadData != null)
            {
                if (this.InvokeRequired)
                    BeginInvoke(new MethodInvoker(EnableUpload));
                else
                    cmdUpload.Enabled = true;
            }
        }

        private void DisableUpload()
        {
            if (this.InvokeRequired)
                BeginInvoke(new MethodInvoker(DisableUpload));
            else
                cmdUpload.Enabled = false;
        }

        private void UpdateAssetID()
        {
            if (this.InvokeRequired)
                BeginInvoke(new MethodInvoker(UpdateAssetID));
            else
                txtAssetID.Text = AssetID.ToString();
        }

        private void LoadImage()
        {
            if (String.IsNullOrEmpty(FileName))
                return;

            string extension = System.IO.Path.GetExtension(FileName).ToLower();
            Bitmap bitmap = null;

            try
            {
                if (extension == ".jp2" || extension == ".j2c")
                {
                    Image image;
                    ManagedImage managedImage;

                    // Upload JPEG2000 images untouched
                    UploadData = System.IO.File.ReadAllBytes(FileName);

                    OpenJPEG.DecodeToImage(UploadData, out managedImage, out image);
                    bitmap = (Bitmap)image;

                    Logger.Log("Loaded raw JPEG2000 data " + FileName, Helpers.LogLevel.Info, Client);
                }
                else
                {
                    if (extension == ".tga")
                        bitmap = LoadTGAClass.LoadTGA(FileName);
                    else
                        bitmap = (Bitmap)System.Drawing.Image.FromFile(FileName);

                    Logger.Log("Loaded image " + FileName, Helpers.LogLevel.Info, Client);

                    int oldwidth = bitmap.Width;
                    int oldheight = bitmap.Height;

                    if (!IsPowerOfTwo((uint)oldwidth) || !IsPowerOfTwo((uint)oldheight))
                    {
                        Logger.Log("Image has irregular dimensions " + oldwidth + "x" + oldheight + ", resizing to 256x256",
                            Helpers.LogLevel.Info, Client);

                        Bitmap resized = new Bitmap(256, 256, bitmap.PixelFormat);
                        Graphics graphics = Graphics.FromImage(resized);

                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.InterpolationMode =
                           System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(bitmap, 0, 0, 256, 256);

                        bitmap.Dispose();
                        bitmap = resized;

                        oldwidth = 256;
                        oldheight = 256;
                    }

                    // Handle resizing to prevent excessively large images
                    if (oldwidth > 1024 || oldheight > 1024)
                    {
                        int newwidth = (oldwidth > 1024) ? 1024 : oldwidth;
                        int newheight = (oldheight > 1024) ? 1024 : oldheight;

                        Logger.Log("Image has oversized dimensions " + oldwidth + "x" + oldheight + ", resizing to " +
                            newwidth + "x" + newheight, Helpers.LogLevel.Info, Client);

                        Bitmap resized = new Bitmap(newwidth, newheight, bitmap.PixelFormat);
                        Graphics graphics = Graphics.FromImage(resized);

                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.InterpolationMode =
                           System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(bitmap, 0, 0, newwidth, newheight);

                        bitmap.Dispose();
                        bitmap = resized;
                    }

                    Logger.Log("Encoding image...", Helpers.LogLevel.Info, Client);

                    UploadData = OpenJPEG.EncodeFromImage(bitmap, chkLossless.Checked);

                    Logger.Log("Finished encoding", Helpers.LogLevel.Info, Client);

                    //System.IO.File.WriteAllBytes("out.jp2", UploadData);
                }
            }
            catch (Exception ex)
            {
                UploadData = null;
                cmdSave.Enabled = false;
                cmdUpload.Enabled = false;
                MessageBox.Show(ex.ToString(), "SL Image Upload", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            picPreview.Image = bitmap;
            lblSize.Text = Math.Round((double)UploadData.Length / 1024.0d, 2) + "KB";
            prgUpload.Maximum = UploadData.Length;

            cmdSave.Enabled = true;
            if (Client.Network.Connected) cmdUpload.Enabled = true;
        }

        private void SaveImage()
        {
            if (String.IsNullOrEmpty(FileName))
                return;

            if (UploadData != null)
            {
                try
                {
                    System.IO.File.WriteAllBytes(FileName, UploadData);
                    MessageBox.Show("Saved " + UploadData.Length + " bytes to " + FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save " + FileName + ": " + ex.Message, Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            else
            {
                MessageBox.Show("No image data loaded", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            if (cmdConnect.Text == "Connect")
            {
                cmdConnect.Text = "Disconnect";
                cboLoginURL.Enabled = txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = false;
                LoginParams lp = Client.Network.DefaultLoginParams(txtFirstName.Text, txtLastName.Text, txtPassword.Text,
                    "GridImageUpload", Application.ProductVersion);
                lp.URI = cboLoginURL.Text;
                cmdConnect.Enabled = false;
                Client.Network.BeginLogin(lp);
            }
            else
            {
                Client.Network.Logout();
                cmdConnect.Text = "Connect";
                cboLoginURL.Enabled = txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                DisableUpload();
                InitClient();
            }
        }

        void Network_OnLogin(LoginStatus login, string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(
                    delegate()
                    {
                        Network_OnLogin(login, message);
                    }
                    ));
                return;
            }
            if (login == LoginStatus.Success)
            {
                MessageBox.Show("Connected: " + message);
                cmdConnect.Enabled = true;
            }
            else if (login == LoginStatus.Failed)
            {
                MessageBox.Show(this, String.Format("Error logging in ({0}): {1}", Client.Network.LoginErrorKey,
     Client.Network.LoginMessage));
                cmdConnect.Text = "Connect";
                cmdConnect.Enabled = true;
                txtFirstName.Enabled = txtLastName.Enabled = txtPassword.Enabled = true;
                DisableUpload();
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
                FileName = dialog.FileName;
                LoadImage();
            }
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "JPEG2000 File (*.j2c)|*.j2c;";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FileName = dialog.FileName;
                SaveImage();
            }
        }

        private void cmdUpload_Click(object sender, EventArgs e)
        {
            SendToID = UUID.Zero;
            string sendTo = txtSendtoName.Text.Trim();

            if (sendTo.Length > 0)
            {
                AutoResetEvent lookupEvent = new AutoResetEvent(false);
                UUID thisQueryID = UUID.Random();
                bool lookupSuccess = false;

                DirectoryManager.DirPeopleReplyCallback callback =
                    delegate(UUID queryID, List<DirectoryManager.AgentSearchData> matchedPeople)
                    {
                        if (queryID == thisQueryID)
                        {
                            if (matchedPeople.Count > 0)
                            {
                                SendToID = matchedPeople[0].AgentID;
                                lookupSuccess = true;
                            }

                            lookupEvent.Set();
                        }
                    };

                Client.Directory.OnDirPeopleReply += callback;
                Client.Directory.StartPeopleSearch(DirectoryManager.DirFindFlags.People, sendTo, 0, thisQueryID);

                bool eventSuccess = lookupEvent.WaitOne(10 * 1000, false);
                Client.Directory.OnDirPeopleReply -= callback;

                if (eventSuccess && lookupSuccess)
                {
                    Logger.Log("Will send uploaded image to avatar " + SendToID.ToString(), Helpers.LogLevel.Info, Client);
                }
                else
                {
                    MessageBox.Show("Could not find avatar \"" + sendTo + "\", upload cancelled");
                    return;
                }
            }

            if (UploadData != null)
            {
                prgUpload.Value = 0;
                cmdLoad.Enabled = false;
                cmdSave.Enabled = false;
                cmdUpload.Enabled = false;
                grpLogin.Enabled = false;

                string name = System.IO.Path.GetFileNameWithoutExtension(FileName);

                Client.Inventory.RequestCreateItemFromAsset(UploadData, name, "Uploaded with SL Image Upload", AssetType.Texture,
                    InventoryType.Texture, Client.Inventory.FindFolderForType(AssetType.Texture),
                    delegate(bool success, string status, UUID itemID, UUID assetID)
                    {
                        if (this.InvokeRequired)
                            BeginInvoke(new MethodInvoker(EnableControls));
                        else
                            EnableControls();

                        if (success)
                        {
                            AssetID = assetID;
                            UpdateAssetID();

                            // Fix the permissions on the new upload since they are fscked by default
                            InventoryItem item = Client.Inventory.FetchItem(itemID, Client.Self.AgentID, 1000 * 15);

                            Transferred = UploadData.Length;
                            BeginInvoke((MethodInvoker)delegate() { SetProgress(); });

                            if (item != null)
                            {
                                item.Permissions.EveryoneMask = PermissionMask.All;
                                item.Permissions.NextOwnerMask = PermissionMask.All;
                                Client.Inventory.RequestUpdateItem(item);

                                Logger.Log("Created inventory item " + itemID.ToString(), Helpers.LogLevel.Info, Client);
                                MessageBox.Show("Created inventory item " + itemID.ToString());

                                // FIXME: We should be watching the callback for RequestUpdateItem instead of a dumb sleep
                                System.Threading.Thread.Sleep(2000);

                                if (SendToID != UUID.Zero)
                                {
                                    Logger.Log("Sending item to " + SendToID.ToString(), Helpers.LogLevel.Info, Client);
                                    Client.Inventory.GiveItem(itemID, name, AssetType.Texture, SendToID, true);
                                    MessageBox.Show("Sent item to " + SendToID.ToString());
                                }
                            }
                            else
                            {
                                Logger.DebugLog("Created inventory item " + itemID.ToString() + " but failed to fetch it," +
                                    " cannot update permissions or send to another avatar", Client);
                                MessageBox.Show("Created inventory item " + itemID.ToString() + " but failed to fetch it," +
                                    " cannot update permissions or send to another avatar");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Asset upload failed: " + status);
                        }
                    }
                );
            }
        }

        private void SetProgress()
        {
            prgUpload.Value = Transferred;
        }

        private void Network_OnEventQueueRunning(Simulator simulator)
        {
            Logger.DebugLog("Event queue is running for " + simulator.ToString() + ", enabling uploads", Client);
            EnableUpload();
        }

        private void EnableControls()
        {
            cmdLoad.Enabled = true;
            cmdSave.Enabled = true;
            cmdUpload.Enabled = true;
            grpLogin.Enabled = true;
        }

        private void frmGridImageUpload_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Client.Network.Connected)
                Client.Network.Logout();
        }

        private void chkLossless_CheckedChanged(object sender, EventArgs e)
        {
            LoadImage();
        }

        private bool IsPowerOfTwo(uint n)
        {
            return (n & (n - 1)) == 0 && n != 0;
        }
    }
}
