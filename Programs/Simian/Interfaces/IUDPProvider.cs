using System;
using System.Net;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public enum PacketCategory
    {
        /// <summary>Any sort of transactional message, such as
        /// AgentMovementComplete</summary>
        Transaction = 0,
        /// <summary>Chat such as intra-simulator chat or instant
        /// messaging</summary>
        Messaging,
        /// <summary>State synchronization, such as animations or
        /// object updates</summary>
        State,
        /// <summary>State synchronization of inventory</summary>
        Inventory,
        /// <summary>State synchronization of terrain, LayerData
        /// packets</summary>
        Terrain,
        /// <summary>Asset transfer packets</summary>
        Asset,
        /// <summary>Texture transfer packets</summary>
        Texture,
        /// <summary>Protocol overhead such as PacketAck</summary>
        Overhead,
    }

    /// <summary>
    /// Coupled with RegisterCallback(), this is triggered whenever a packet
    /// of a registered type is received
    /// </summary>
    public delegate void PacketCallback(Packet packet, /*ISceneProvider scene,*/ Agent agent);
    /// <summary>
    /// Triggered whenever a packet is going to be sent out to one or more
    /// clients
    /// </summary>
    /// <param name="packet">The packet that will be sent out</param>
    /// <param name="agentID">The UUID of the agent receiving this packet. Equal
    /// to UUID.Zero if this packet will be broadcast to all connected agents</param>
    /// <param name="category">The specified category of the outgoing packet</param>
    /// <returns>True to continue sending this packet, otherwise false</returns>
    public delegate bool OutgoingPacketCallback(Packet packet, UUID agentID, PacketCategory category);

    public interface IUDPProvider
    {
        event OutgoingPacketCallback OnOutgoingPacket;

        void AddClient(Agent agent, IPEndPoint endpoint);
        bool RemoveClient(Agent agent);
        uint CreateCircuit(Agent agent);

        void SendPacket(UUID agentID, Packet packet, PacketCategory category);
        void BroadcastPacket(Packet packet, PacketCategory category);

        void RegisterPacketCallback(PacketType type, PacketCallback callback);
    }
}
