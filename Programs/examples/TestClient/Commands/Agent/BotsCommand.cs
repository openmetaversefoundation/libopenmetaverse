using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class BotsCommand : Command
    {
        private Dictionary<UUID, bool> m_AgentList = new Dictionary<UUID, bool>();

        public BotsCommand(TestClient testClient)
        {
            Name = "bots";
            Description = "detects avatars that appear to be bots.";
            Category = CommandCategory.Other;

            testClient.Avatars.ViewerEffect += new EventHandler<ViewerEffectEventArgs>(Avatars_ViewerEffect);
            testClient.Avatars.ViewerEffectLookAt += new EventHandler<ViewerEffectLookAtEventArgs>(Avatars_ViewerEffectLookAt);
            testClient.Avatars.ViewerEffectPointAt += new EventHandler<ViewerEffectPointAtEventArgs>(Avatars_ViewerEffectPointAt);
        }

        void Avatars_ViewerEffectPointAt(object sender, ViewerEffectPointAtEventArgs e)
        {
            lock (m_AgentList)
            {
                if (m_AgentList.ContainsKey(e.SourceID))
                    m_AgentList[e.SourceID] = true;
                else
                    m_AgentList.Add(e.SourceID, true);
            }
        }

        void Avatars_ViewerEffectLookAt(object sender, ViewerEffectLookAtEventArgs e)
        {
            lock (m_AgentList)
            {
                if (m_AgentList.ContainsKey(e.SourceID))
                    m_AgentList[e.SourceID] = true;
                else
                    m_AgentList.Add(e.SourceID, true);
            }
        }

        void Avatars_ViewerEffect(object sender, ViewerEffectEventArgs e)
        {
            lock (m_AgentList)
            {
                if (m_AgentList.ContainsKey(e.SourceID))
                    m_AgentList[e.SourceID] = true;
                else
                    m_AgentList.Add(e.SourceID, true);
            }
        }
        
        public override string Execute(string[] args, UUID fromAgentID)
        {
            StringBuilder result = new StringBuilder();

            lock (Client.Network.Simulators)
            {
                for (int i = 0; i < Client.Network.Simulators.Count; i++)
                {
                    Client.Network.Simulators[i].ObjectsAvatars.ForEach(
                        delegate(Avatar av)
                        {
                            lock (m_AgentList)
                            {
                                if (!m_AgentList.ContainsKey(av.ID))
                                {
                                    result.AppendLine();
                                    result.AppendFormat("{0} (Group: {1}, Location: {2}, UUID: {3} LocalID: {4}) Is Probably a bot",
                                        av.Name, av.GroupName, av.Position, av.ID, av.LocalID);
                                }
                            }
                        }
                    );
                }
            }

            return result.ToString();
        }
    }
}
