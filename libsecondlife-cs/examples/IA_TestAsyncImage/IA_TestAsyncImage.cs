using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using IA_SimpleInventory;

using libsecondlife;
using libsecondlife.AssetSystem;

namespace IA_TestAsyncImage
{
    class TestAsync : SimpleInventory
    {
        ImageManager imgManager;

        Queue<LLUUID> TextureQueue = new Queue<LLUUID>();

        string OutputDirectory = "IA_TestAsyncImages";

        [STAThread]
        static new void Main(string[] args)
        {
            TestAsync app = new TestAsync();
            app.DownloadInventoryOnConnect = false;

            app.client.Objects.OnNewPrim += new NewPrimCallback(app.Objects_OnNewPrim);
            app.client.Objects.OnNewAvatar += new NewAvatarCallback(app.Objects_OnNewAvatar);


            app.Connect(args[0], args[1], args[2]);
            app.doStuff();
            app.Disconnect();

            System.Threading.Thread.Sleep(500);

            Console.WriteLine("Done...");
        }

        private void Objects_OnNewAvatar(Simulator simulator, Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            if (imgManager == null)
            {
                Console.WriteLine("ImageManager not ready yet, queueing Avatar textures.");
                TextureQueue.Enqueue(avatar.FirstLifeImage);
                TextureQueue.Enqueue(avatar.ProfileImage);

                foreach (TextureEntryFace tef in avatar.Textures.FaceTextures.Values)
                {
                    TextureQueue.Enqueue(tef.TextureID);
                }
            }
            else
            {
                if (avatar.FirstLifeImage != null)
                {
                    if (imgManager.isCachedImage(avatar.FirstLifeImage) == false)
                    {
                        imgManager.RequestImageAsync(avatar.FirstLifeImage);
                    }
                }

                if (avatar.ProfileImage != null)
                {
                    if (imgManager.isCachedImage(avatar.FirstLifeImage) == false)
                    {
                        imgManager.RequestImageAsync(avatar.ProfileImage);
                    }
                }

                if (avatar.Textures != null)
                {
                    foreach (TextureEntryFace tef in avatar.Textures.FaceTextures.Values)
                    {
                        if (imgManager.isCachedImage(tef.TextureID) == false)
                        {
                            imgManager.RequestImageAsync(tef.TextureID);
                        }
                        else
                        {
                            Console.WriteLine("Already cached: " + tef.TextureID);
                        }
                    }
                }
            }
        }

        private void Objects_OnNewPrim(Simulator simulator, PrimObject prim, ulong regionHandle, ushort timeDilation)
        {
            if (imgManager == null)
            {
                Console.WriteLine("ImageManager not ready yet, queueing Prim textures.");
                TextureQueue.Enqueue(prim.Textures.DefaultTexture.TextureID);

                foreach (TextureEntryFace tef in prim.Textures.FaceTextures.Values)
                {
                    TextureQueue.Enqueue(tef.TextureID);
                }
            }
            else
            {
                if ((prim.Textures.DefaultTexture != null) && (prim.Textures.DefaultTexture.TextureID != null))
                {
                    if (imgManager.isCachedImage(prim.Textures.DefaultTexture.TextureID) == false)
                    {
                        imgManager.RequestImageAsync(prim.Textures.DefaultTexture.TextureID);
                    }
                    else
                    {
                        Console.WriteLine("Already cached: " + prim.Textures.DefaultTexture.TextureID);
                    }
                }

                if (prim.Textures.FaceTextures != null)
                {
                    foreach (TextureEntryFace tef in prim.Textures.FaceTextures.Values)
                    {
                        if (imgManager.isCachedImage(tef.TextureID) == false)
                        {
                            imgManager.RequestImageAsync(tef.TextureID);
                        }
                        else
                        {
                            Console.WriteLine("Already cached: " + tef.TextureID);
                        }
                    }
                }
            }
        }

        private void NewImageRetrievedCallBack( LLUUID ImageID, byte[] data, bool wasCached, string statusMsg )
        {
            if (wasCached)
            {
                Console.WriteLine("Cache ( " + data.Length + "): " + ImageID);
            }
            else
            {
                if (data == null)
                {
                    Console.WriteLine("Image Data is null (" + statusMsg + "): " + ImageID);
                }
                else
                {
                    Console.WriteLine("Finished ( " + data.Length + "): " + ImageID);

                    String filename = Path.Combine(OutputDirectory, ImageID.ToStringHyphenated()) + ".tif";

                    TiffJob tj = new TiffJob(filename, data);
                    Thread t = new Thread(tj.RunMe);
                    t.Start();
                }
            }
        }

        protected new void doStuff()
        {
            imgManager = new ImageManager(client, ImageManager.CacheTypes.Disk, OutputDirectory);
            imgManager.OnImageRetrieved += new ImageRetrievedCallback(NewImageRetrievedCallBack);

            while (TextureQueue.Count > 0)
            {
                imgManager.RequestImageAsync(TextureQueue.Dequeue());
            }

            Console.WriteLine("Press any key to stop.");
            Console.Read();
        }

        protected class TiffJob
        {
            string filename;
            byte[] j2cdata;

            public TiffJob(string path, byte[] data)
            {
                filename = path;
                j2cdata  = data;
            }

            public void RunMe()
            {
                File.WriteAllBytes(filename, JasperWrapper.jasper_decode_j2c_to_tiff(j2cdata));
            }
        }
    }
}
