using System;
using System.IO;
using System.Diagnostics;
using Gtk;

namespace GridProxyGUI
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            try
            {
                InitLogging();
                StartGtkApp();
            }
            catch (Exception ex)
            {
                if (ex is TypeInitializationException || ex is TypeLoadException || ex is System.IO.FileNotFoundException)
                {
                    NativeApi.ExitWithMessage("Failed to start", ex.Message + "\n\nMake sure tha application install isn't missing accompanied files and that Gtk# is installed.", 1);
                }
                throw;
            }
        }

        static void StartGtkApp()
        {
                Gtk.Application.Init();
                MainWindow win = new MainWindow();
                win.Show();
                Application.Run();
        }

        static bool InitLogging()
        {
            try
            {
                string userDir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "GridProxyGUI");

                if (!Directory.Exists(userDir))
                {
                    Directory.CreateDirectory(userDir);
                }

                string settingsFile = Path.Combine(userDir, "Settings.xml");
                Options.CreateInstance(settingsFile);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public static class NativeApi
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);

        public static void LinuxMessageBox(string title, string msg, string type)
        {
            try
            {
                ProcessStartInfo p = new ProcessStartInfo("zenity", string.Format("--{0} --title=\"{1}\" --text=\"{2}\"", type, title.Replace("\"", "\\\""), msg.Replace("\"", "\\\"")));
                p.CreateNoWindow = true;
                p.ErrorDialog = false;
                p.UseShellExecute = true;
                var process = Process.Start(p);
                process.WaitForExit();
            }
            catch { }
        }

        public static void ExitWithMessage(string title, string msg, int exitCode)
        {
            Console.Error.WriteLine(title + ": " + msg);
            if (PlatformDetection.IsWindows)
            {
                MessageBox(IntPtr.Zero, msg, title, 0x10);
            }
            else if (PlatformDetection.IsMac)
            {
            }
            else
            {
                LinuxMessageBox(title, msg, "error");
            }

            Environment.Exit(exitCode);
        }
    }
}
