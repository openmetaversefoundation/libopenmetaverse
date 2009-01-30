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
    public delegate void PacketCallback(Packet packet, Agent agent);

    public interface IUDPProvider
    {
        void AddClient(Agent agent, IPEndPoint endpoint);
        bool RemoveClient(Agent agent);
        bool RemoveClient(Agent agent, IPEndPoint endpoint);
        uint CreateCircuit(Agent agent);

        void SendPacket(Guid agentID, Packet packet, PacketCategory category);
        void BroadcastPacket(Packet packet, PacketCategory category);

        void RegisterPacketCallback(PacketType type, PacketCallback callback);
    }
}
