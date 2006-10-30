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

        /// <summary>
        /// Used to upload/download images.
        /// </summary>
        [STAThread]
        static new void Main(string[] args)
        {
            if (KakaduWrap.Check4Tools() == false)
            {
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
            if (args[3].ToLower().Equals("put"))
            {
                put = true;
                filename = args[4];
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

            ImageTool it = new ImageTool(id, filename, put);
            it.Connect(args[0], args[1], args[2]);
            it.doStuff();
            it.Disconnect();

            System.Threading.Thread.Sleep(500);

            Console.WriteLine("Done logging out.");
        }

        protected ImageTool(LLUUID imageID, string filename, bool put)
        {
            _ImageID = imageID;
            _FileName = filename;
            _Put = put;
        }

        protected new void doStuff()
        {
            if (_Put)
            {
                Console.WriteLine("Reading: " + _FileName);

                byte[] j2cdata = KakaduWrap.ReadJ2CData(_FileName);

                Console.WriteLine("Connecting to your Texture folder...");
                InventoryFolder iFolder = AgentInventory.getFolder("Textures");

                Console.WriteLine("Uploading Texture...");
                iFolder.NewImage(_FileName, "ImageTool Upload", j2cdata);
            }
            else
            {
                Console.WriteLine("Downloading: " + _ImageID);


                int start = Environment.TickCount;
                ImageManager im = new ImageManager(base.client);
                byte[] j2cdata = im.RequestImage(_ImageID);
                int end = Environment.TickCount;
                Console.WriteLine("Elapsed download time, in TickCounts: " + (end - start));

                Console.WriteLine("Writing to: " + _FileName + ".tif");
                KakaduWrap.WriteJ2CAsTiff(_FileName + ".tif", j2cdata);

                Console.WriteLine("Writing to: " + _FileName + ".bmp");
                KakaduWrap.WriteJ2CAsBmp(_FileName + ".bmp", j2cdata);
            }

            Console.WriteLine("Done...");

        }

        protected static void Usage()
        {
            Console.WriteLine("Usage: ImageTool [first] [last] [password] [get] [uuid] [(filename)]");
            Console.WriteLine("Usage: ImageTool [first] [last] [password] [put] [filename]");

            Console.WriteLine();
            Console.WriteLine("Example: ImageTool John Doe Password get 0444bf21-f77e-7f63-89e9-b839ec66bc15 cloud.tif");
            Console.WriteLine("Example: ImageTool John Doe Password put Sample (this will output a bmp and a tiff)");
        }
    }
}