using System;
using System.Collections.Generic;
using System.IO;

using IA_SimpleInventory;

using libsecondlife;
using libsecondlife.InventorySystem;
using libsecondlife.AssetSystem;

namespace IA_ImageTool
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    class ImageTool : SimpleInventory
    {
        private LLUUID _ImageID;
        private string _FileName;
        private bool _Put;
        private double _Rate;

        /// <summary>
        /// Used to upload/download images.
        /// </summary>
        [STAThread]
        static new void Main(string[] args)
        {
            if ( (File.Exists("libjasper.dll") == false) )
            {
                Console.WriteLine("You need a copy of libjasper.dll, it can be found in SVN in the main trunk inside libjaspernet");
                return;
            }

            if (args.Length < 5)
            {
                ImageTool.Usage();
                return;
            }


            LLUUID id = null;
            string filename = "";
            bool put = false;
            double rate = 0;

            if (args[3].ToLower().Equals("put"))
            {
                put = true;
                if (args.Length == 6)
                {
                    double.TryParse(args[4], out rate);
                    filename = args[5];
                }
                else
                {
                    filename = args[4];
                }
            }
            else
            {
                if (args.Length < 6)
                {
                    ImageTool.Usage();
                    return;
                }

                id = new LLUUID(args[4]);
                if (args.Length == 6)
                {
                    filename = args[5];
                }
                else if (!args[4].ToLower().EndsWith(".j2c"))
                {
                    filename = args[4] + ".j2c";
                }
            }

            ImageTool it = new ImageTool(id, filename, put, rate);

            // Only download the inventory tree if we're planning on putting/uploading files.
            it.DownloadInventoryOnConnect = put; 

            if (it.Connect(args[0], args[1], args[2]))
            {
                it.doStuff();
                it.Disconnect();

                System.Threading.Thread.Sleep(500);

                Console.WriteLine("Done logging out.");
            }
        }

        protected ImageTool(LLUUID imageID, string filename, bool put, double rate)
        {
            _ImageID = imageID;
            _FileName = filename;
            _Put = put;
            _Rate = rate;
        }

        protected new void doStuff()
        {
            if (_Put)
            {
                Console.WriteLine("Reading: " + _FileName);

                byte[] j2cdata;

                if (_Rate != 0)
                {
                    j2cdata = KakaduWrap.ReadJ2CData(_FileName, _Rate);
                }
                else
                {
                    j2cdata = KakaduWrap.ReadJ2CData(_FileName);
                }
                

                Console.WriteLine("Connecting to your Texture folder...");
                InventoryFolder iFolder = AgentInventory.getFolder("Textures");

                Console.WriteLine("Uploading Texture...");
                iFolder.NewImage(_FileName, "ImageTool Upload", j2cdata);
            }
            else
            {
                Console.WriteLine("Downloading: " + _ImageID);


                int start = Environment.TickCount;
                ImageManager im = new ImageManager(base.client, ImageManager.CacheTypes.Disk);
                byte[] j2cdata = im.RequestImage(_ImageID);
                int end = Environment.TickCount;
                Console.WriteLine("Elapsed download time, in TickCounts: " + (end - start));

                Console.WriteLine("Image Data Length :" + j2cdata.Length);

                Console.WriteLine("Writing to: " + _FileName + ".tif");
                File.WriteAllBytes(_FileName + ".tif", JasperWrapper.jasper_decode_j2c_to_tiff(j2cdata));

                Console.WriteLine("Writing to: " + _FileName + ".tga");
                File.WriteAllBytes(_FileName + ".tga", JasperWrapper.jasper_decode_j2c_to_tga(j2cdata));

            }

            Console.WriteLine("Done...");

        }

        protected static void Usage()
        {
            Console.WriteLine("Usage: ImageTool [first] [last] [password] [get] [uuid] [(filename)]");
            Console.WriteLine("Usage: ImageTool [first] [last] [password] [put] [filename]");
            Console.WriteLine("Usage: ImageTool [first] [last] [password] [put] [bit-rate] [filename]");

            Console.WriteLine();
            Console.WriteLine("Example: ImageTool John Doe Password get 0444bf21-f77e-7f63-89e9-b839ec66bc15 cloud (this will output a bmp and a tiff)");
            Console.WriteLine("Example: ImageTool John Doe Password put Sample.tiff");
            Console.WriteLine("Example: ImageTool John Doe Password put 1.0 BigImage.tiff (this will compress the file with the given bit-rate)");
        }
    }
}