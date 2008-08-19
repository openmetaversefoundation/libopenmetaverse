using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class SceneManager : ISimianExtension
    {
        Simian server;
        int currentLocalID = 0;
        int currentWearablesSerialNum = 0;

        public SceneManager(Simian server)
        {
            this.server = server;
        }

        public void Start()
        {
            server.UDPServer.RegisterPacketCallback(PacketType.CompleteAgentMovement, new UDPServer.PacketCallback(CompleteAgentMovementHandler));
            server.UDPServer.RegisterPacketCallback(PacketType.AgentWearablesRequest, new UDPServer.PacketCallback(AgentWearablesRequestHandler));
        }

        public void Stop()
        {
        }

        void CompleteAgentMovementHandler(Packet packet, Agent agent)
        {
            CompleteAgentMovementPacket request = (CompleteAgentMovementPacket)packet;

            // Create a representation for this agent
            Avatar avatar = new Avatar();
            avatar.ID = agent.AgentID;
            avatar.NameValues = agent.Avatar.NameValues;
            avatar.LocalID = (uint)Interlocked.Increment(ref currentLocalID);
            avatar.Position = new Vector3(128f, 128f, 25f);
            avatar.Rotation = Quaternion.Identity;
            avatar.Scale = new Vector3(1f, 1f, 3f);

            NameValue[] name = new NameValue[2];
            name[0] = new NameValue();
            name[0].Name = "FirstName";
            name[0].Value = agent.FirstName;
            name[1] = new NameValue();
            name[1].Name = "LastName";
            name[1].Value = agent.LastName;
            avatar.NameValues = name;

            // Link this avatar up with the corresponding agent
            agent.Avatar = avatar;

            AgentMovementCompletePacket complete = new AgentMovementCompletePacket();
            complete.AgentData.AgentID = agent.AgentID;
            complete.AgentData.SessionID = agent.SessionID;
            complete.Data.LookAt = Vector3.UnitX;
            complete.Data.Position = avatar.Position;
            complete.Data.RegionHandle = server.RegionHandle;
            complete.Data.Timestamp = Utils.DateTimeToUnixTime(DateTime.Now);
            complete.SimData.ChannelVersion = Utils.StringToBytes("Simian");

            agent.SendPacket(complete);
        }

        void AgentWearablesRequestHandler(Packet packet, Agent agent)
        {
            /*AgentWearablesUpdatePacket update = new AgentWearablesUpdatePacket();
            update.AgentData.AgentID = agent.AgentID;
            update.AgentData.SessionID = agent.SessionID;
            // Technically this should be per-agent, but if the only requirement is that it
            // increments this is easier
            update.AgentData.SerialNum = (uint)Interlocked.Increment(ref currentWearablesSerialNum);
            update.WearableData = new AgentWearablesUpdatePacket.WearableDataBlock[0];

            agent.SendPacket(update);*/
        }

    }
}
