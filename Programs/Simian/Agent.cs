using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class Agent
    {
        public UUID AgentID;
        public UUID SessionID;
        public UUID SecureSessionID;
        public uint CircuitCode;
        public string FirstName;
        public string LastName;
        public Avatar Avatar = new Avatar();
        public int Balance;
        public bool Running;
        public int TickFall;
        public int TickJump;
        public AgentManager.ControlFlags ControlFlags = AgentManager.ControlFlags.NONE;
        public AnimationSet Animations = new AnimationSet();
        public Dictionary<UUID, InventoryObject> Inventory = new Dictionary<UUID, InventoryObject>();
        public Dictionary<UUID, InventoryObject> Library = new Dictionary<UUID, InventoryObject>();
        public Dictionary<WearableType, UUID> Wearables = new Dictionary<WearableType, UUID>();
        public byte[] VisualParams = new byte[218];
        // TODO: Replace byte with enum
        public byte State;
        public PrimFlags Flags;
        public UUID InventoryRoot;
        public UUID InventoryLibRoot;
    }
}
