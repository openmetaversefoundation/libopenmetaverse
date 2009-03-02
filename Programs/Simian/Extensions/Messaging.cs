using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class Messaging : IExtension<Simian>
    {
        Simian server;

        public Messaging()
        {
        }

        public void Start(Simian server)
        {
            this.server = server;

            server.UDP.RegisterPacketCallback(PacketType.ChatFromViewer, new PacketCallback(ChatFromViewerHandler));
            server.UDP.RegisterPacketCallback(PacketType.ImprovedInstantMessage, new PacketCallback(ImprovedInstantMessageHandler));
        }

        public void Stop()
        {
        }

        void ChatFromViewerHandler(Packet packet, Agent agent)
        {
            ChatFromViewerPacket viewerChat = (ChatFromViewerPacket)packet;

            server.Scene.ObjectChat(this, agent.Avatar.ID, agent.Avatar.ID, ChatAudibleLevel.Fully, (ChatType)viewerChat.ChatData.Type,
                ChatSourceType.Agent, agent.Avatar.Name, agent.GetSimulatorPosition(server.Scene), viewerChat.ChatData.Channel,
                Utils.BytesToString(viewerChat.ChatData.Message));
        }

        void ImprovedInstantMessageHandler(Packet packet, Agent agent)
        {
            ImprovedInstantMessagePacket im = (ImprovedInstantMessagePacket)packet;
            InstantMessageDialog dialog = (InstantMessageDialog)im.MessageBlock.Dialog;

            if (dialog == InstantMessageDialog.MessageFromAgent)
            {
                // HACK: Only works for agents currently online
                Agent recipient;
                if (server.Scene.TryGetAgent(im.MessageBlock.ToAgentID, out recipient))
                {
                    // FIXME: Look into the fields we are setting to default values
                    ImprovedInstantMessagePacket sendIM = new ImprovedInstantMessagePacket();
                    sendIM.MessageBlock.RegionID = server.Scene.RegionID;
                    sendIM.MessageBlock.ParentEstateID = 1;
                    sendIM.MessageBlock.FromGroup = false;
                    sendIM.MessageBlock.FromAgentName = Utils.StringToBytes(agent.Avatar.Name);
                    sendIM.MessageBlock.ToAgentID = im.MessageBlock.ToAgentID;
                    sendIM.MessageBlock.Dialog = im.MessageBlock.Dialog;
                    sendIM.MessageBlock.Offline = (byte)InstantMessageOnline.Online;
                    sendIM.MessageBlock.ID = agent.Avatar.ID;
                    sendIM.MessageBlock.Message = im.MessageBlock.Message;
                    sendIM.MessageBlock.BinaryBucket = new byte[0];
                    sendIM.MessageBlock.Timestamp = Utils.DateTimeToUnixTime(DateTime.Now);
                    sendIM.MessageBlock.Position = agent.GetSimulatorPosition(server.Scene);

                    sendIM.AgentData.AgentID = agent.Avatar.ID;

                    server.UDP.SendPacket(recipient.Avatar.ID, sendIM, PacketCategory.Messaging);
                }
            }
        }

    }
}
