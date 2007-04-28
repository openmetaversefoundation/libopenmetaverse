using System;
using System.Collections.Generic;
using System.Net;

namespace libslupdater
{
    public class Updater
    {
        public const string CheckURI = "http://www.libsecondlife.org/ccnet/update/";

        private bool busy = false;
        public bool Busy
        {
            get { return busy; }
        }

        public Updater()
        {
        }

        public void UpdateProcessBegin()
        {
            busy = true;

            // Start a thread that:
            ///////////////////////

            // Connects to the check URI and see if we're running the latest

            // If not, download from the latest update link given by the check server

            // Confirm the download checksum, start the waiting thread, and return a kill
            // signal to the calling app

            // Wait for the app to die and replace the target file

            // Re-launch the app and kill this process
        }

        public int UpdateProcess()
        {
            // Call UpdateProcessBegin and wait for a response, return the response
            return 0;
        }
    }

    public class Program
    {
        static int Main(string[] args)
        {
            return 0;
        }
    }
}
