/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using libsecondlife;
using libsecondlife.InventorySystem;

namespace IA_NotecardTool
{
    class NotecardTool
    {
        private string FileName;
        private string NotecardName;

        private SecondLife _Client;
        private ManualResetEvent ConnectedSignal = new ManualResetEvent(false);


        static void Main(string[] args)
        {
            if (args.Length < 6)
            {
                Console.WriteLine("Usage: NotecardTool [first] [last] [password] [put] [name] [notecard.txt] ");
                return;
            }

            if( !File.Exists(args[5]) )
            {
                Console.WriteLine("Cannot find file: " + args[5]);
                return;
            }

            NotecardTool tool = new NotecardTool();
            tool.NotecardName = args[4] + " : " + Helpers.GetUnixTime();
            tool.FileName = args[5];

            tool.Connect(args[0], args[1], args[2]);

            if (tool.ConnectedSignal.WaitOne(TimeSpan.FromMinutes(1), false))
            {
                tool.doStuff();
                tool.Disconnect();
            }
        }

        private void doStuff()
        {
            Console.WriteLine("Reading " + FileName);
            StreamReader sr = File.OpenText(FileName);
            string Body = sr.ReadToEnd();

            Console.WriteLine("Getting Notecard Folder");
            InventoryFolder iFolder = _Client.Inventory.getFolder("Notecards");


            Console.WriteLine("Creating Notecard");
            iFolder.NewNotecard(NotecardName, "Imported by libsl Notecard Tool", Body);

            Console.WriteLine("Done.");
        }

        public NotecardTool()
        {
            try
            {
                _Client = new SecondLife();
                _Client.Network.OnConnected += new NetworkManager.ConnectedCallback(Network_OnConnected);
            }
            catch (Exception e)
            {
                // Error initializing the client
                Console.WriteLine();
                Console.WriteLine(e.ToString());
            }
        }

        void Network_OnConnected(object sender)
        {
            ConnectedSignal.Set();
        }

        protected bool Connect(string FirstName, string LastName, string Password)
        {
            Console.WriteLine("Attempting to connect and login to SecondLife.");

            // Setup Login to Second Life
            Dictionary<string, object> loginReply = new Dictionary<string, object>();

            // Login
            if (!_Client.Network.Login(FirstName, LastName, Password, "createnotecard", "static.sprocket@gmail.com"))
            {
                // Login failed
                Console.WriteLine("Error logging in: " + _Client.Network.LoginError);
                return false;
            }

            // Login was successful
            Console.WriteLine("Login was successful.");
            Console.WriteLine("AgentID:   " + _Client.Network.AgentID);
            Console.WriteLine("SessionID: " + _Client.Network.SessionID);

            return true;
        }

        protected void Disconnect()
        {
            // Logout of Second Life
            Console.WriteLine("Request logout");
            _Client.Network.Logout();
        }
    }
}
