using System;
using System.Collections.Generic;
using System.IO;

using IA_SimpleInventory;
using IA_ImageTool;

using libsecondlife;
using libsecondlife.InventorySystem;
using libsecondlife.AssetSystem;

namespace IA_MultiImageUpload
{
    class MultiImageUpload : SimpleInventory
    {
        protected string ImageDirectory;

        static new void Main(string[] args)
        {
            if (args.Length < 4)
            {
                MultiImageUpload.Usage();
                return;
            }

            string fullpath = Path.GetFullPath(args[3]);

            if (!Directory.Exists(fullpath))
            {
                Console.WriteLine("Directory does not exist: " + fullpath);
                return;
            }

            MultiImageUpload app = new MultiImageUpload( fullpath );
            app.Connect(args[0], args[1], args[2]);
            app.doStuff();
            app.Disconnect();
        }

        public MultiImageUpload(string dir)
        {
            ImageDirectory = dir;
        }

        public static void Usage()
        {
            System.Console.WriteLine("MultiImageUpload [FirstName] [LastName] [Password] [Directory]");
        }

        protected new void doStuff()
        {
            InventoryFolder iFolder = AgentInventory.getFolder("Textures");
            iFolder = iFolder.CreateFolder(Helpers.GetUnixTime().ToString());

            Console.WriteLine("Uploading images:");

            string[] files = Directory.GetFiles(ImageDirectory, "*.tif");

            int filesUploaded = 0;
            foreach (string file in files)
            {
                byte[] j2cdata = null;
                try
                {
                    j2cdata = KakaduWrap.ReadJ2CData(file);
                } catch ( Exception e )
                {
                    Console.WriteLine(e.Message);
                }
                if ( j2cdata != null )
                {
                    Console.WriteLine(file);
                    iFolder.NewImage(Path.GetFileName(file), "ImageTool Upload", j2cdata);

                    if (++filesUploaded >= 20)
                    {
                        break;
                    }
                }
            }
        }
    }
}
