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
            if (args.Length > 2) Application.Run(new Dashboard(args[0], args[1], args[2]));
            else MessageBox.Show("Usage: dashboard.exe <firstName> <lastName> <password>", "Dashboard", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
    }
}