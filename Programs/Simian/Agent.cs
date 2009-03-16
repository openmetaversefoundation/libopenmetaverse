using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace Simian
{
    public class AgentInfo
    {
        // Account
        public UUID ID;
        public string FirstName;
        public string LastName;
        public string PasswordHash;
        public uint CreationTime;
        public uint LastLoginTime;
        public string AccessLevel;
        public int GodLevel;
        public int Balance;

        // Inventory
        public UUID InventoryRoot;
        public UUID InventoryLibraryRoot;
        public UUID InventoryLibraryOwner;

        // Location
        public ulong HomeRegionHandle;
        public Vector3 HomePosition;
        public Vector3 HomeLookAt;

        // Profile
        public UUID PartnerID;
        public int ProfileCanDo;
        public int ProfileWantDo;
        public string ProfileAboutText;
        public string ProfileFirstText;
        public string ProfileBornOn;
        public string ProfileURL;
        public UUID ProfileImage;
        public UUID ProfileFirstImage;
        public ProfileFlags ProfileFlags;

        // Appearance
        public byte[] VisualParams;
        //public byte[] Texture;
        public float Height;
        public UUID ShapeItem;
        public UUID SkinItem;
        public UUID HairItem;
        public UUID EyesItem;
        public UUID ShirtItem;
        public UUID PantsItem;
        public UUID ShoesItem;
        public UUID SocksItem;
        public UUID JacketItem;
        public UUID GlovesItem;
        public UUID UndershirtItem;
        public UUID UnderpantsItem;
        public UUID SkirtItem;
    }

    public class Agent
    {
        public AgentInfo Info;
        public SimulationObject Avatar;
        public UUID SessionID;
        public UUID SecureSessionID;
        public uint CircuitCode;
        public bool Running;
        public int TickFall;
        public int TickJump;
        public int TickLastPacketReceived;
        public AgentManager.ControlFlags ControlFlags = AgentManager.ControlFlags.NONE;
        public AnimationSet Animations = new AnimationSet();
        public AgentState State;
        public bool HideTitle;
        public Uri SeedCapability;
        public Vector3 CurrentLookAt;
        public UUID RequestedSitTarget;
        public Vector3 RequestedSitOffset;
        public bool[] NeighborConnections = new bool[8];

        public UUID ID
        {
            get { return Avatar.Prim.ID; }
        }

        public string FullName
        {
            get
            {
                bool hasFirst = !String.IsNullOrEmpty(Info.FirstName);
                bool hasLast = !String.IsNullOrEmpty(Info.LastName);

                if (hasFirst && hasLast)
                    return String.Format("{0} {1}", Info.FirstName, Info.LastName);
                else if (hasFirst)
                    return Info.FirstName;
                else if (hasLast)
                    return Info.LastName;
                else
                    return String.Empty;
            }
        }

        public Agent(SimulationObject avatar, AgentInfo info)
        {
            Avatar = avatar;
            Info = info;
        }
    }
}
