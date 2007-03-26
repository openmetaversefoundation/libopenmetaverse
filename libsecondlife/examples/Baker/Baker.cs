using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Baker
{
    static class Baker
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
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmBaker(args[0], args[1], args[2]));
        }
    }
}