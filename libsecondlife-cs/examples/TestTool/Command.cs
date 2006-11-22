using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestTool
{
    public abstract class Command
    {
		public string Name;
		public string Description;

		public TestTool TestTool;

		public SecondLife Client
		{
			get { return TestTool.Client; }
		}

		public abstract string Execute(string[] args, LLUUID fromAgentID);
    }
}
