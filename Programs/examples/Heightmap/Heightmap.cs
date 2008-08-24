using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Heightmap
{
    static class Heightmap
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: [FirstName] [LastName] [password]");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmHeightmap(args[0], args[1], args[2]));
        }
    }
}