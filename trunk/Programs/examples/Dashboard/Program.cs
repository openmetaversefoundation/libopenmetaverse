using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Dashboard
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string firstName = args.Length > 0 ? args[0] : String.Empty;
            string lastName = args.Length > 2 ? args[1] : String.Empty;
            string password = args.Length > 2 ? args[2] : String.Empty;

            Application.Run(new Dashboard(firstName, lastName, password));
        }
    }
}