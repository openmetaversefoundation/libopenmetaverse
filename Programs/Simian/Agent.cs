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
        // Account
        public Guid AgentID;
        public string FirstName;
        public string LastName;
        public string PasswordHash;
        public uint CreationTime;
        public uint LastLoginTime;
        public string AccessLevel;
        public int GodLevel;
        public int Balance;

        // Inventory
        public Guid InventoryRoot;
        public Guid InventoryLibraryRoot;
        public Guid InventoryLibraryOwner;

        // Location
        public ulong HomeRegionHandle;
        public Vector3 HomePosition;
        public Vector3 HomeLookAt;
        public ulong CurrentRegionHandle;
        public Vector3 CurrentPosition;
        public Vector3 CurrentLookAt;

        // Profile
        public Guid PartnerID;
        public int ProfileCanDo;
        public int ProfileWantDo;
        public string ProfileAboutText;
        public string ProfileFirstText;
        public string ProfileBornOn;
        public string ProfileURL;
        public Guid ProfileImage;
        public Guid ProfileFirstImage;
        public ProfileFlags ProfileFlags;

        // Appearance
        public byte[] VisualParams;
        //public byte[] Texture;
        public float Height;
        public Guid ShapeItem;
        public Guid SkinItem;
        public Guid HairItem;
        public Guid EyesItem;
        public Guid ShirtItem;
        public Guid PantsItem;
        public Guid ShoesItem;
        public Guid SocksItem;
        public Guid JacketItem;
        public Guid GlovesItem;
        public Guid UndershirtItem;
        public Guid UnderpantsItem;
        public Guid SkirtItem;

        // Temporary
        [NonSerialized]
        public Avatar Avatar = new Avatar();
        [NonSerialized]
        public Guid SessionID;
        [NonSerialized]
        public Guid SecureSessionID;
        [NonSerialized]
        public uint CircuitCode;
        [NonSerialized]
        public bool Running;
        [NonSerialized]
        public int TickFall;
        [NonSerialized]
        public int TickJump;
        [NonSerialized]
        public int TickLastPacketReceived;
        [NonSerialized]
        public AgentManager.ControlFlags ControlFlags = AgentManager.ControlFlags.NONE;
        [NonSerialized]
        public AnimationSet Animations = new AnimationSet();
        // TODO: Replace byte with enum
        [NonSerialized]
        public byte State;
        [NonSerialized]
        public PrimFlags Flags;

        public string FullName
        {
            get
            {
                bool hasFirst = !String.IsNullOrEmpty(FirstName);
                bool hasLast = !String.IsNullOrEmpty(LastName);

                if (hasFirst && hasLast)
                    return String.Format("{0} {1}", FirstName, LastName);
                else if (hasFirst)
                    return FirstName;
                else if (hasLast)
                    return LastName;
                else
                    return String.Empty;
            }
        }
    }
}
