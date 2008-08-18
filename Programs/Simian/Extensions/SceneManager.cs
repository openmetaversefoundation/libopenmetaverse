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
        ulong regionHandle;

        public SceneManager(Simian server)
        {
            this.server = server;
        }

        public void Start()
        {
            uint regionX = 256000;
            uint regionY = 256000;
            regionHandle = Helpers.UIntsToLong(regionX, regionY);

            server.UDPServer.RegisterPacketCallback(PacketType.CompleteAgentMovement, new UDPServer.PacketCallback(CompleteAgentMovementHandler));
            server.UDPServer.RegisterPacketCallback(PacketType.AgentUpdate, new UDPServer.PacketCallback(AgentUpdateHandler));
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
            avatar.LocalID = (uint)Interlocked.Increment(ref currentLocalID);
            avatar.Position = new Vector3(128f, 128f, 25f);
            avatar.Rotation = Quaternion.Identity;
            avatar.Scale = new Vector3(1f, 1f, 3f);

            // Add this agent to the scene graph
            lock (server.SceneAvatars)
                server.SceneAvatars[avatar.ID] = avatar;

            AgentMovementCompletePacket complete = new AgentMovementCompletePacket();
            complete.AgentData.AgentID = agent.AgentID;
            complete.AgentData.SessionID = agent.SessionID;
            complete.Data.LookAt = Vector3.UnitX;
            complete.Data.Position = avatar.Position;
            complete.Data.RegionHandle = regionHandle;
            complete.Data.Timestamp = Utils.DateTimeToUnixTime(DateTime.Now);
            complete.SimData.ChannelVersion = Utils.StringToBytes("Simian");

            agent.SendPacket(complete);
        }

        void AgentUpdateHandler(Packet packet, Agent agent)
        {
            AgentUpdatePacket update = (AgentUpdatePacket)packet;

            Avatar avatar;
            if (server.SceneAvatars.TryGetValue(agent.AgentID, out avatar))
            {
                SendFullUpdate(agent, avatar, update.AgentData.State, update.AgentData.Flags);
            }
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

        void SendFullUpdate(Agent agent, LLObject obj, byte state, uint flags)
        {
            ObjectUpdatePacket update = new ObjectUpdatePacket();
            update.RegionData.RegionHandle = regionHandle;
            update.RegionData.TimeDilation = Helpers.FloatToByte(1f, 0f, 1f);
            update.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
            update.ObjectData[0] = new ObjectUpdatePacket.ObjectDataBlock();
            update.ObjectData[0].ClickAction = (byte)0;
            update.ObjectData[0].CRC = 0;
            update.ObjectData[0].ExtraParams = new byte[0];
            update.ObjectData[0].Flags = 0;
            update.ObjectData[0].FullID = obj.ID;
            update.ObjectData[0].Gain = 0;
            update.ObjectData[0].ID = obj.LocalID; 
            update.ObjectData[0].JointAxisOrAnchor = Vector3.Zero;
            update.ObjectData[0].JointPivot = Vector3.Zero;
            update.ObjectData[0].JointType = (byte)0;
            update.ObjectData[0].Material = (byte)3;
            update.ObjectData[0].MediaURL = new byte[0];
            update.ObjectData[0].NameValue = new byte[0];
            update.ObjectData[0].ObjectData = new byte[60];
            update.ObjectData[0].OwnerID = UUID.Zero;
            update.ObjectData[0].ParentID = 0;
            update.ObjectData[0].PathBegin = 0;
            update.ObjectData[0].PathCurve = (byte)32;
            update.ObjectData[0].PathEnd = 0;
            update.ObjectData[0].PathRadiusOffset = (sbyte)0;
            update.ObjectData[0].PathRevolutions = (byte)0;
            update.ObjectData[0].PathScaleX = (byte)100;
            update.ObjectData[0].PathScaleY = (byte)150;
            update.ObjectData[0].PathShearX = (byte)0;
            update.ObjectData[0].PathShearY = (byte)0;
            update.ObjectData[0].PathSkew = (sbyte)0;
            update.ObjectData[0].PathTaperX = (sbyte)0;
            update.ObjectData[0].PathTaperY = (sbyte)0;
            update.ObjectData[0].PathTwist = (sbyte)0;
            update.ObjectData[0].PathTwistBegin = (sbyte)0;
            update.ObjectData[0].PCode = (byte)PCode.Avatar;
            update.ObjectData[0].ProfileBegin = 0;
            update.ObjectData[0].ProfileCurve = (byte)0;
            update.ObjectData[0].ProfileEnd = 0;
            update.ObjectData[0].ProfileHollow = 0;
            update.ObjectData[0].PSBlock = new byte[0];
            update.ObjectData[0].TextColor = Vector3.Zero.GetBytes();
            update.ObjectData[0].TextureAnim = new byte[0];
            update.ObjectData[0].TextureEntry = new byte[63];
            update.ObjectData[0].Radius = 0f;
            update.ObjectData[0].Scale = obj.Scale;
            update.ObjectData[0].Sound = UUID.Zero;
            update.ObjectData[0].State = state;
            update.ObjectData[0].Text = new byte[0];
            update.ObjectData[0].UpdateFlags = flags;
            update.ObjectData[0].Data = new byte[0];

            agent.SendPacket(update);
        }
    }
}
