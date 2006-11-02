using System;
using System.IO;
using System.Diagnostics;

namespace IA_ImageTool
{
	/// <summary>
	/// Summary description for ImageTools.
	/// </summary>
	public class KakaduWrap
	{
        private KakaduWrap()
		{
		}

		public static bool Check4Tools()
		{
			bool status = true;
            /*
			if( File.Exists("kdu_expand.exe") == false )
			{
				status = false;
				Console.WriteLine("You need kdu_expand.exe to save SL images.");
			}
            */
			if( File.Exists("kdu_compress.exe") == false )
			{
				status = false;
				Console.WriteLine("You need kdu_compress.exe to load images into SL.");
			}
			
			return status;
		}

		public static void WriteJ2CToFile( string j2c_filename, byte[] J2CData )
		{
			FileStream fs = System.IO.File.OpenWrite( j2c_filename );
			fs.Write(J2CData, 0, J2CData.Length);
			fs.Close();
		}


        public static void Convert2Tiff(string j2c_filename, string tif_filename)
        {
            if (File.Exists("kdu_expand.exe") == false)
            {
                throw new Exception("You must have kdu_expand.exe");
            }

            if (tif_filename.ToLower().EndsWith(".tif") == false)
            {
                tif_filename += ".tif";
            }

            string args = "-i " + j2c_filename + " -o " + tif_filename;
            Console.WriteLine(args);

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "kdu_expand.exe";
            p.StartInfo.Arguments = args;
            p.Start();
            p.WaitForExit();
        }

        public static void Convert2Bmp(string j2c_filename, string bmp_filename)
        {
            if (File.Exists("kdu_expand.exe") == false)
            {
                throw new Exception("You must have kdu_expand.exe");
            }

            if (bmp_filename.ToLower().EndsWith(".bmp") == false)
            {
                bmp_filename += ".bmp";
            }

            string args = "-i " + j2c_filename + " -o " + bmp_filename;
            Console.WriteLine(args);

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "kdu_expand.exe";
            p.StartInfo.Arguments = args;
            p.Start();
            p.WaitForExit();
        }

        public static void WriteJ2CAsTiff(string tif_filename, byte[] J2CData)
		{
			String tempname = tif_filename + ".j2c";
			WriteJ2CToFile( tempname, J2CData );
			Convert2Tiff( tempname, tif_filename );
			File.Delete( tempname );
		}

        public static void WriteJ2CAsBmp(string bmp_filename, byte[] J2CData)
        {
            String tempname = bmp_filename + ".j2c";
            WriteJ2CToFile(tempname, J2CData);
            Convert2Bmp(tempname, bmp_filename);
            File.Delete(tempname);
        }

		/*
		 * kdu_compress -no_info -no_weights -no_palette -i TestTexture.tif -o TestTexture.J2C
		 */
		public static void Convert2J2C( string tif_filename, string j2c_filename, double rate )
		{
			if( File.Exists("kdu_compress.exe") == false )
			{
				throw new Exception("You must have kdu_compress.exe");
			}

			if( j2c_filename.ToLower().EndsWith(".j2c") == false )
			{
				j2c_filename += ".j2c";
			}

			Process p = new Process();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.FileName  = "kdu_compress.exe";
			p.StartInfo.Arguments = "-no_info -no_weights -no_palette -rate " + rate + " -i " + tif_filename + " -o " + j2c_filename;
			p.Start();
			p.WaitForExit();
            if (p.ExitCode != 0)
                throw new Exception("Error in calling kdu_compress");
		}

		public static byte[] ReadJ2CData( string filename, double rate )
		{
			if (!filename.ToLower().EndsWith(".j2c") )
			{
				string tempname = filename + ".j2c";
				Convert2J2C( filename, tempname, rate );
				filename = tempname;
			}

			FileStream fs = File.OpenRead( filename );

			byte[] data = new byte[fs.Length];

			if( fs.Length > int.MaxValue )
			{
				throw new Exception("AssetImage.cs: Bad stuff going to happen because length bigger then Max Integer");
			}

			fs.Read(data, 0, (int)fs.Length);

			return data;
		}
	}
}
