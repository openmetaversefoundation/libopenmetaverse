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
        public Vector3 CurrentLookAt;
        public ulong CurrentRegionHandle;

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

        // Temporary
        [NonSerialized]
        public Avatar Avatar = new Avatar();
        [NonSerialized]
        public UUID SessionID;
        [NonSerialized]
        public UUID SecureSessionID;
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
