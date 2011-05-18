using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class SetMasterCommand: Command
    {
		public DateTime Created = DateTime.Now;
        private UUID resolvedMasterKey = UUID.Zero;
        private ManualResetEvent keyResolution = new ManualResetEvent(false);
        private UUID query = UUID.Zero;

        public SetMasterCommand(TestClient testClient)
		{
			Name = "setmaster";
            Description = "Sets the user name of the master user. The master user can IM to run commands. Usage: setmaster [name]";
            Category = CommandCategory.TestClient;
		}

        public override string Execute(string[] args, UUID fromAgentID)
		{
			string masterName = String.Empty;
			for (int ct = 0; ct < args.Length;ct++)
				masterName = masterName + args[ct] + " ";
            masterName = masterName.TrimEnd();

            if (masterName.Length == 0)
                return "Usage: setmaster [name]";

            EventHandler<DirPeopleReplyEventArgs> callback = KeyResolvHandler;
            Client.Directory.DirPeopleReply += callback;

            query = Client.Directory.StartPeopleSearch(masterName, 0);

            if (keyResolution.WaitOne(TimeSpan.FromMinutes(1), false))
            {
                Client.MasterKey = resolvedMasterKey;
                keyResolution.Reset();
                Client.Directory.DirPeopleReply -= callback;
            }
            else
            {
                keyResolution.Reset();
                Client.Directory.DirPeopleReply -= callback;
                return "Unable to obtain UUID for \"" + masterName + "\". Master unchanged.";
            }
            
            // Send an Online-only IM to the new master
            Client.Self.InstantMessage(
                Client.MasterKey, "You are now my master.  IM me with \"help\" for a command list.");

            return String.Format("Master set to {0} ({1})", masterName, Client.MasterKey.ToString());
		}

        private void KeyResolvHandler(object sender, DirPeopleReplyEventArgs e)
        {
            if (query != e.QueryID)
                return;

            resolvedMasterKey = e.MatchedPeople[0].AgentID;
            keyResolution.Set();
            query = UUID.Zero;
        }
    }
}
