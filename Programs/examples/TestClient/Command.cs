using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public abstract class Command
    {
		public string Name;
		public string Description;
		public TestClient Client;

		public abstract string Execute(string[] args, UUID fromAgentID);

		/// <summary>
		/// When set to true, think will be called.
		/// </summary>
		public bool Active;

		/// <summary>
		/// Called twice per second, when Command.Active is set to true.
		/// </summary>
		public virtual void Think()
		{
		}
    }
}
