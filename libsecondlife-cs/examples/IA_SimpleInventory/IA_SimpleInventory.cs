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

using libsecondlife;
using libsecondlife.InventorySystem;

namespace IA_SimpleInventory
{
    /// <summary>
    /// A simple base application for building console applications that access SL Inventory
    /// </summary>
    public class SimpleInventory
    {
        protected SecondLife client;
        protected InventoryManager AgentInventory;

        protected bool DownloadInventoryOnConnect = true;

        public static void Main(string[] args)
        {

            if (args.Length < 3)
            {
                Console.WriteLine("Usage: SimpleInventory [loginfirstname] [loginlastname] [password]");
                return;
            }

            SimpleInventory simple = new SimpleInventory();
            simple.Connect(args[0], args[1], args[2]);
            simple.doStuff();
            simple.Disconnect();
        }

        protected SimpleInventory()
        {
            try
            {
                client = new SecondLife();
            }
            catch (Exception e)
            {
                // Error initializing the client
                Console.WriteLine();
                Console.WriteLine(e.ToString());
            }
        }

        protected bool Connect(string FirstName, string LastName, string Password)
        {
            Console.WriteLine("Attempting to connect and login to SecondLife.");

            // Setup Login to Second Life
            Dictionary<string, object> loginParams = NetworkManager.DefaultLoginValues(FirstName, LastName,
                Password, "00:00:00:00:00:00", "last", "Win", "0", "createnotecard", "static.sprocket@gmail.com");
            Dictionary<string, object> loginReply = new Dictionary<string, object>();

            // Login
            if (!client.Network.Login(loginParams))
            {
                // Login failed
                Console.WriteLine("Error logging in: " + client.Network.LoginError);
                return false;
            }

            // Login was successful
            Console.WriteLine("Login was successful.");
            Console.WriteLine("AgentID:   " + client.Network.AgentID);
            Console.WriteLine("SessionID: " + client.Network.SessionID);

            // Get Root Inventory Folder UUID
            Console.WriteLine("Pulling root folder UUID from login data.");
            List<object> alInventoryRoot = (List<object>)client.Network.LoginValues["inventory-root"];
            Dictionary<string, object> htInventoryRoot = (Dictionary<string, object>)alInventoryRoot[0];
            LLUUID agentRootFolderID = new LLUUID((string)htInventoryRoot["folder_id"]);

            // Initialize Inventory Manager object
            Console.WriteLine("Initializing Inventory Manager.");
            AgentInventory = new InventoryManager(client, agentRootFolderID);

            if (DownloadInventoryOnConnect)
            {
                // and request an inventory download
                Console.WriteLine("Downloading Inventory.");
                AgentInventory.DownloadInventory();
            }
            return true;
        }

        protected void Disconnect()
        {
            // Logout of Second Life
            Console.WriteLine("Request logout");
            client.Network.Logout();
        }

        protected void doStuff()
        {
            Console.WriteLine("Dumping a copy of " + client.Self.FirstName + "'s inventory to the console.");
            Console.WriteLine();

            Console.WriteLine(AgentInventory.getRootFolder().toXML(false));
        }
    }
}
