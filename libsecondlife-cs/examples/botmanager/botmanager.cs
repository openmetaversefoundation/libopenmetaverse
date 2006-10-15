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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using libsecondlife;
using libsecondlife.Packets;

namespace botmanager
{
    public class Bot
    {
        public delegate void BotKilledHandler(Bot bot);

        public SecondLife Client;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Password;

        protected bool loggedIn = false;
        public bool LoggedIn { get { return loggedIn; } }

        protected BotManager Manager;
        protected BotKilledHandler KillHandler;

        public Bot(BotManager parent, BotKilledHandler killHandler, 
            string firstName, string lastName, string password)
        {
            Manager = parent;
            KillHandler = killHandler;
            FirstName = firstName;
            LastName = lastName;
            Password = password;

            Client = new SecondLife();
            Client.Network.OnDisconnected += new DisconnectCallback(DisconnectHandler);
        }

        public bool Spawn()
        {
            if (!loggedIn)
            {
                Kill();
            }
            
            Hashtable loginParams = NetworkManager.DefaultLoginValues(FirstName, LastName,
                Password, "00:00:00:00:00:00", "last", 1, 50, 50, 50, "Win", "0",
                "botmanager", "contact@libsecondlife.org");

            if (Client.Network.Login(loginParams))
            {
                loggedIn = true;
                return true;
            }
            else
            {
                loggedIn = false;
                return false;
            }
        }

        public void Kill()
        {
            if (loggedIn)
            {
                Client.Network.Logout();
            }
        }

        private void DisconnectHandler(DisconnectType type, string reason)
        {
            KillHandler(this);
        }
    }

    public class BotManager
    {
        protected List<Bot> Bots;

        public BotManager()
        {
            Bots = new List<Bot>();
        }

        public void AddBot(string firstName, string lastName, string password)
        {
            Bot bot = new Bot(this, new Bot.BotKilledHandler(KillHandler), firstName, lastName, password);
            Bots.Add(bot);
            AddBotToDB(bot);
        }

        protected void AddBotToDB(Bot bot)
        {
            ;
        }

        private void PollDBEvents()
        {
            ;
        }

        private void KillHandler(Bot bot)
        {
            Console.WriteLine(bot.ToString() + " was killed");

            if (Bots.Contains(bot))
            {
                Bots.Remove(bot);
            }
        }
    }

    class runbotmanager
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            BotManager manager = new BotManager();

		}
	}
}
