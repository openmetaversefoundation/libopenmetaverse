using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using libsecondlife;

namespace FastImageApp
{
    class FastImageApp
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: fastimageapp.exe first last password image-key [image-key2 [imagekey3 ...]]]");
                return;
            }

            SecondLife client;

            try
            {
                client = new SecondLife();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            Dictionary<string, object> loginValues = NetworkManager.DefaultLoginValues(args[0], args[1], args[2], "FastImageApp", "Alpha Zaius - alpha.zaius@gmail.com");

            try
            {
                client.Network.Login(loginValues);
            }
            catch
            {
                Console.WriteLine("Could not login: " + client.Network.LoginError);
                return;
            }

            //instantiate the imagemanager class

            libsecondlife.AssetSystem.FastImageTool.ImageManager im = new libsecondlife.AssetSystem.FastImageTool.ImageManager(client, new libsecondlife.AssetSystem.FastImageTool.ImageFinishedCallback(ImageProcessor));
            for(int i = 3; i < args.Length; i++)
            {
                im.Add(new LLUUID(args[i]));
            }

            while(!im.AllImagesDone())
            {
                im.Update();
                System.Threading.Thread.Sleep(250); //give it some time between packets. 250 ms is a good time for me.
            }

            Console.WriteLine("Operation completed.. Have a nice day :)");
            client.Network.Logout();
        }


        public static void ImageProcessor(LLUUID image_key, byte[] data)
        {
            try
            {
                System.IO.File.WriteAllBytes(image_key + ".jpc", data);
                Console.WriteLine("Saved imagedata to file: " + image_key + ".jpc!");
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not save " + image_key + ".jpc to file:");
                Console.WriteLine("\t" + e.Message);
            }
        }
    }

}
