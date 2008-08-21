using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Simian.Extensions
{
    public class Messaging : ISimianExtension
    {
        Simian Server;

        public Messaging(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDPServer.RegisterPacketCallback(PacketType.ChatFromViewer, new UDPServer.PacketCallback(ChatFromViewerHandler));
            Server.UDPServer.RegisterPacketCallback(PacketType.ImprovedInstantMessage, new UDPServer.PacketCallback(ImprovedInstantMessageHandler));
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

        void ImprovedInstantMessageHandler(Packet packet, Agent agent)
        {
            ImprovedInstantMessagePacket im = (ImprovedInstantMessagePacket)packet;
            InstantMessageDialog dialog = (InstantMessageDialog)im.MessageBlock.Dialog;

            if (dialog == InstantMessageDialog.MessageFromAgent)
            {
                lock (Server.Agents)
                {
                    foreach (Agent recipient in Server.Agents.Values)
                    {
                        if (recipient.AgentID == im.MessageBlock.ToAgentID)
                        {
                            ImprovedInstantMessagePacket sendIM = new ImprovedInstantMessagePacket();
                            sendIM.MessageBlock.RegionID = UUID.Random(); //FIXME
                            sendIM.MessageBlock.ParentEstateID = 1;
                            sendIM.MessageBlock.FromGroup = false;
                            sendIM.MessageBlock.FromAgentName = Utils.StringToBytes(agent.Avatar.Name);
                            sendIM.MessageBlock.ToAgentID = im.MessageBlock.ToAgentID;
                            sendIM.MessageBlock.Dialog = (byte)InstantMessageDialog.MessageFromAgent;
                            sendIM.MessageBlock.Offline = (byte)InstantMessageOnline.Online;
                            sendIM.MessageBlock.ID = agent.AgentID;
                            sendIM.MessageBlock.Message = im.MessageBlock.Message;
                            sendIM.MessageBlock.BinaryBucket = new byte[0];
                            sendIM.MessageBlock.Timestamp = 0;
                            sendIM.MessageBlock.Position = agent.Avatar.Position;                            
                            recipient.SendPacket(sendIM);

                            break;
                        }
                    }
                }
            }
        }

    }
}
