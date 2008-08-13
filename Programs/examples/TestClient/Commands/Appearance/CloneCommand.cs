using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class CloneCommand : Command
    {
        uint SerialNum = 2;

        public CloneCommand(TestClient testClient)
        {
            Name = "clone";
            Description = "Clone the appearance of a nearby avatar. Usage: clone [name]";
            Category = CommandCategory.Appearance;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            string targetName = String.Empty;
            List<DirectoryManager.AgentSearchData> matches;

            for (int ct = 0; ct < args.Length; ct++)
                targetName = targetName + args[ct] + " ";
            targetName = targetName.TrimEnd();

            if (targetName.Length == 0)
                return "Usage: clone [name]";

            if (Client.Directory.PeopleSearch(DirectoryManager.DirFindFlags.People, targetName, 0, 1000 * 10,
                out matches) && matches.Count > 0)
            {
                UUID target = matches[0].AgentID;
                targetName += String.Format(" ({0})", target);

                if (Client.Appearances.ContainsKey(target))
                {
                    #region AvatarAppearance to AgentSetAppearance

                    AvatarAppearancePacket appearance = Client.Appearances[target];

                    AgentSetAppearancePacket set = new AgentSetAppearancePacket();
                    set.AgentData.AgentID = Client.Self.AgentID;
                    set.AgentData.SessionID = Client.Self.SessionID;
                    set.AgentData.SerialNum = SerialNum++;
                    set.AgentData.Size = new Vector3(2f, 2f, 2f); // HACK

                    set.WearableData = new AgentSetAppearancePacket.WearableDataBlock[0];
                    set.VisualParam = new AgentSetAppearancePacket.VisualParamBlock[appearance.VisualParam.Length];

                    for (int i = 0; i < appearance.VisualParam.Length; i++)
                    {
                        set.VisualParam[i] = new AgentSetAppearancePacket.VisualParamBlock();
                        set.VisualParam[i].ParamValue = appearance.VisualParam[i].ParamValue;
                    }

                    set.ObjectData.TextureEntry = appearance.ObjectData.TextureEntry;

                    #endregion AvatarAppearance to AgentSetAppearance

                    // Detach everything we are currently wearing
                    Client.Appearance.AddAttachments(new List<ItemData>(0), true);

                    // Send the new appearance packet
                    Client.Network.SendPacket(set);

                    return "Cloned " + targetName;
                }
                else
                {
                    return "Don't know the appearance of avatar " + targetName;
                }
            }
            else
            {
                return "Couldn't find avatar " + targetName;
            }
        }
    }
}
