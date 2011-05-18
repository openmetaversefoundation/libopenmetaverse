using System;
using System.IO;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Packets;
namespace OpenMetaverse.TestClient
{
    public class UploadRawTerrainCommand : Command
    {
        System.Threading.AutoResetEvent WaitForUploadComplete = new System.Threading.AutoResetEvent(false);

        public UploadRawTerrainCommand(TestClient testClient)
        {
            Name = "uploadterrain";
            Description = "Upload a raw terrain file to a simulator. usage: uploadterrain filename";
            Category = CommandCategory.Simulator;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            string fileName = String.Empty;

            if (args.Length != 1)
                return "Usage: uploadterrain filename";


            fileName = args[0];

            if (!System.IO.File.Exists(fileName))
            {
                return String.Format("File {0} Does not exist", fileName);
            }

            // Setup callbacks for upload request reply and progress indicator 
            // so we can detect when the upload is complete
            Client.Assets.UploadProgress += new EventHandler<AssetUploadEventArgs>(Assets_UploadProgress);
            byte[] fileData = File.ReadAllBytes(fileName);

            Client.Estate.UploadTerrain(fileData, fileName);

            // Wait for upload to complete. Upload request is fired in callback from first request
            if (!WaitForUploadComplete.WaitOne(120000, false))
            {
                Cleanup();
                return "Timeout waiting for terrain file upload";
            }
            else
            {
                Cleanup();
                return "Terrain raw file uploaded and applied";
            }
        }

        /// <summary>
        /// Unregister previously subscribed event handlers
        /// </summary>
        private void Cleanup()
        {
            Client.Assets.UploadProgress -= new EventHandler<AssetUploadEventArgs>(Assets_UploadProgress);
        }


        void Assets_UploadProgress(object sender, AssetUploadEventArgs e)
        {
            if (e.Upload.Transferred == e.Upload.Size)
            {
                WaitForUploadComplete.Set();
            }
            else
            {
                //Console.WriteLine("Progress: {0}/{1} {2}/{3} {4}", upload.XferID, upload.ID, upload.Transferred, upload.Size, upload.Success);
                Console.Write(".");
            }
        }


    }
}
