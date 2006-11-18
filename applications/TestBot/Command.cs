using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace TestBot
{
    public abstract class Command
    {
		public string Name;
		public string Description;

		public TestBot Bot;

		public SecondLife Client
		{
			get { return Bot.Client; }
		}

		public abstract string Execute(string[] args, LLUUID fromAgentID);
    }
}
