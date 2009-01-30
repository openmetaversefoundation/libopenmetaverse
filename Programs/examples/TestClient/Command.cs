using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public enum CommandCategory : int
    {
        Parcel,
        Appearance,
        Movement,
        Simulator,
        Communication,
        Inventory,
        Objects,
        Voice,
        TestClient,
        Friends,
        Groups,
        Other,
        Unknown
    }

    public abstract class Command : IComparable
    {
		public string Name;
		public string Description;
        public CommandCategory Category;

		public TestClient Client;

		public abstract string Execute(string[] args, Guid fromAgentID);

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

        public int CompareTo(object obj)
        {
            if (obj is Command)
            {
                Command c2 = (Command)obj;
                return Category.CompareTo(c2.Category);
            }
            else
                throw new ArgumentException("Object is not of type Command.");
        }

    }
}
