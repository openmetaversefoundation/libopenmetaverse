using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Drawing;
using libsecondlife;
using libsecondlife.Capabilities;

namespace libsecondlife.TestClient
{
    public class UploadImageCommand : Command
    {
        AutoResetEvent UploadCompleteEvent = new AutoResetEvent(false);
        LLUUID TextureID = LLUUID.Zero;

        public UploadImageCommand(TestClient testClient)
        {
            Name = "uploadimage";
            Description = "Upload an image to your inventory. Usage: uploadimage [inventoryname] [timeout] [filename]";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            string inventoryName;
            uint timeout;
            string fileName;

            if (args.Length != 3)
                return "Usage: uploadimage [inventoryname] [timeout] [filename]";

            TextureID = LLUUID.Zero;
            inventoryName = args[0];
            fileName = args[2];
            if (!UInt32.TryParse(args[1], out timeout))
                return "Usage: uploadimage [inventoryname] [timeout] [filename]";

            Console.WriteLine("Loading image " + fileName);
            byte[] jpeg2k = LoadImage(fileName);
            if (jpeg2k == null)
                return "Failed to compress image to JPEG2000";
            Console.WriteLine("Finished compressing image to JPEG2000, uploading...");

            DoUpload(jpeg2k, inventoryName);

            if (UploadCompleteEvent.WaitOne((int)timeout, false))
            {
                return String.Format("Texture upload {0}: {1}", (TextureID != LLUUID.Zero) ? "succeeded" : "failed",
                    TextureID);
            }
            else
            {
                return "Texture upload timed out";
            }
        }

        private void DoUpload(byte[] UploadData, string FileName)
        {
            if (UploadData != null)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(FileName);

                Client.Inventory.RequestCreateItemFromAsset(UploadData, name, "Uploaded with TestClient",
                    AssetType.Texture, InventoryType.Texture, Client.Inventory.FindFolderForType(AssetType.Texture),

                    delegate(CapsClient client, long bytesReceived, long bytesSent, long totalBytesToReceive, long totalBytesToSend)
                    {
                        if (bytesSent > 0)
                            Console.WriteLine(String.Format("Texture upload: {0} / {1}", bytesSent, totalBytesToSend));
                    },

                    delegate(bool success, string status, LLUUID itemID, LLUUID assetID)
                    {
                        Console.WriteLine(String.Format(
                            "RequestCreateItemFromAsset() returned: Success={0}, Status={1}, ItemID={2}, AssetID={3}",
                            success, status, itemID, assetID));

                        TextureID = assetID;
                        UploadCompleteEvent.Set();
                    }
                );
            }
        }

        private byte[] LoadImage(string fileName)
        {
            byte[] UploadData;
            string lowfilename = fileName.ToLower();
            Bitmap bitmap = null;

            try
            {
                if (lowfilename.EndsWith(".jp2") || lowfilename.EndsWith(".j2c"))
                {
                    // Upload JPEG2000 images untouched
                    UploadData = System.IO.File.ReadAllBytes(fileName);
                    bitmap = (Bitmap)OpenJPEGNet.OpenJPEG.DecodeToImage(UploadData);
                }
                else
                {
                    if (lowfilename.EndsWith(".tga"))
                        bitmap = OpenJPEGNet.LoadTGAClass.LoadTGA(fileName);
                    else
                        bitmap = (Bitmap)System.Drawing.Image.FromFile(fileName);

                    int oldwidth = bitmap.Width;
                    int oldheight = bitmap.Height;

                    if (!IsPowerOfTwo((uint)oldwidth) || !IsPowerOfTwo((uint)oldheight))
                    {
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

                        Bitmap resized = new Bitmap(newwidth, newheight, bitmap.PixelFormat);
                        Graphics graphics = Graphics.FromImage(resized);

                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.InterpolationMode =
                           System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(bitmap, 0, 0, newwidth, newheight);

                        bitmap.Dispose();
                        bitmap = resized;
                    }

                    UploadData = OpenJPEGNet.OpenJPEG.EncodeFromImage(bitmap, false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() + " SL Image Upload ");
                return null;
            }
            return UploadData;
        }

        private static bool IsPowerOfTwo(uint n)
        {
            return (n & (n - 1)) == 0 && n != 0;
        }
    }
}
