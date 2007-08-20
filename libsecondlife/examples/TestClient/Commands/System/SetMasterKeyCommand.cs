using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class SetMasterKeyCommand : Command
    {
        public DateTime Created = DateTime.Now;

        public SetMasterKeyCommand(TestClient testClient)
        {
            Name = "setMasterKey";
            Description = "Sets the key of the master user.  The master user can IM to run commands.";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            Client.MasterKey = LLUUID.Parse(args[0]);

            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    lock (Client.Network.Simulators[i].Objects.Avatars)
                    {
                        foreach (Avatar avatar in Client.Network.Simulators[i].Objects.Avatars.Values)
                        {
                            if (avatar.ID == Client.MasterKey)
                            {
                                Client.Self.InstantMessage(avatar.ID, 
                                    "You are now my master. IM me with \"help\" for a command list.");
                                break;
                            }
                        }
                    }
                }
            }

            return "Master set to " + Client.MasterKey.ToStringHyphenated();
        }
    }
}
