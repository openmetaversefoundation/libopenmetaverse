using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simian.Extensions
{
    class AvatarPropertiesReply : ISimianExtension
    {
        Simian Server;

        public AvatarPropertiesReply(Simian server)
        {
            Server = server;
        }

        public void Start()
        {
            Server.UDPServer.RegisterPacketCallback(PacketType.AvatarPropertiesRequest, new UDPServer.PacketCallback(AvatarPropertiesRequestHandler));
        }

        public void Stop()
        {
        }

        void AvatarPropertiesRequestHandler(Packet packet, Agent agent)
        {
            AvatarPropertiesRequestPacket request = (AvatarPropertiesRequestPacket)packet;

            lock (Server.Agents)
            {
                foreach (Agent agt in Server.Agents.Values)
                {
                    if (agent.AgentID == request.AgentData.AvatarID)
                    {
                        AvatarPropertiesReplyPacket reply = new AvatarPropertiesReplyPacket();
                        reply.AgentData.AgentID = agt.AgentID;
                        reply.AgentData.AvatarID = request.AgentData.AvatarID;
                        reply.PropertiesData.AboutText = Utils.StringToBytes("Profile info unavailable");
                        reply.PropertiesData.BornOn = Utils.StringToBytes("Unknown");
                        reply.PropertiesData.CharterMember = Utils.StringToBytes("Test User");
                        reply.PropertiesData.FLAboutText = Utils.StringToBytes("First life info unavailable");
                        reply.PropertiesData.Flags = 0;
                        //TODO: at least generate static image uuids based on name.
                        //this will prevent re-caching the default image for the same av name.
                        reply.PropertiesData.FLImageID = agent.AgentID; //temporary hack
                        reply.PropertiesData.ImageID = agent.AgentID; //temporary hack
                        reply.PropertiesData.PartnerID = UUID.Zero;
                        reply.PropertiesData.ProfileURL = Utils.StringToBytes(String.Empty);

                        agent.SendPacket(reply);

                        break;
                    }
                }
            }
        }

    }
}
