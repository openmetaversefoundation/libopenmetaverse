using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Simian.Extensions
{
    public class Chat : ISimianExtension
    {
        Simian Server;

        public Chat(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDPServer.RegisterPacketCallback(PacketType.ChatFromViewer, new UDPServer.PacketCallback(ChatFromViewerHandler));
        }

        public void Stop()
        {
        }

        void ChatFromViewerHandler(Packet packet, Agent agent)
        {
            ChatFromViewerPacket viewerChat = (ChatFromViewerPacket)packet;

            if (viewerChat.ChatData.Channel != 0) return; //not public chat

            //TODO: add distance constraints to AudibleLevel and Message 

            ChatFromSimulatorPacket chat = new ChatFromSimulatorPacket();
            chat.ChatData.Audible = (byte)ChatAudibleLevel.Fully;
            chat.ChatData.ChatType = viewerChat.ChatData.Type;
            chat.ChatData.OwnerID = agent.AgentID;
            chat.ChatData.SourceID = agent.AgentID;
            chat.ChatData.SourceType = (byte)ChatSourceType.Agent;
            chat.ChatData.Position = agent.Avatar.Position;
            chat.ChatData.FromName = Utils.StringToBytes(agent.Avatar.Name);
            chat.ChatData.Message = viewerChat.ChatData.Message;

            lock (Server.Agents)
            {
                foreach(Agent recipient in Server.Agents.Values)
                    recipient.SendPacket(chat);
            }
        }

    }
}
