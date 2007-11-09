/*
 * Copyright (c) 2006-2007, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Contains the variables sent in an object update packet for objects. 
    /// Used to track position and movement of prims and avatars
    /// </summary>
    public struct ObjectUpdate
    {
        /// <summary></summary>
        public bool Avatar;
        /// <summary></summary>
        public LLVector4 CollisionPlane;
        /// <summary></summary>
        public byte State;
        /// <summary></summary>
        public uint LocalID;
        /// <summary></summary>
        public LLVector3 Position;
        /// <summary></summary>
        public LLVector3 Velocity;
        /// <summary></summary>
        public LLVector3 Acceleration;
        /// <summary></summary>
        public LLQuaternion Rotation;
        /// <summary></summary>
        public LLVector3 AngularVelocity;
        /// <summary></summary>
        public LLObject.TextureEntry Textures;
    }

    /// <summary>
    /// Handles all network traffic related to prims and avatar positions and 
    /// movement.
    /// </summary>
    public class ObjectManager
    {
        #region CallBack Definitions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="prim"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        public delegate void NewPrimCallback(Simulator simulator, Primitive prim, ulong regionHandle,
            ushort timeDilation);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="prim"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        public delegate void NewAttachmentCallback(Simulator simulator, Primitive prim, ulong regionHandle,
            ushort timeDilation);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="properties"></param>
        public delegate void ObjectPropertiesCallback(Simulator simulator, LLObject.ObjectProperties properties);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="properties"></param>
        public delegate void ObjectPropertiesFamilyCallback(Simulator simulator, 
            LLObject.ObjectPropertiesFamily properties);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="avatar"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        public delegate void NewAvatarCallback(Simulator simulator, Avatar avatar, ulong regionHandle,
            ushort timeDilation);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="foliage"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        public delegate void NewFoliageCallback(Simulator simulator, Primitive foliage, ulong regionHandle,
            ushort timeDilation);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="update"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        public delegate void ObjectUpdatedCallback(Simulator simulator, ObjectUpdate update, ulong regionHandle,
            ushort timeDilation);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="objectID"></param>
        public delegate void KillObjectCallback(Simulator simulator, uint objectID);
        /// <summary>
        /// Called whenever the client avatar sits down or stands up
        /// </summary>
        /// <param name="simulator">Simulator the packet was received from</param>
        /// <param name="sittingOn">The local ID of the object that is being sat
        /// on. If this is zero the avatar is not sitting on an object</param>
        public delegate void AvatarSitChanged(Simulator simulator, Avatar avatar, uint sittingOn, uint oldSeat);
		
        #endregion


        #region Object/Prim Enums

        /// <summary>
        /// 
        /// </summary>
        public enum PCode : byte
        {
            /// <summary></summary>
            None = 0,
            /// <summary></summary>
            Prim = 9,
            /// <summary></summary>
            Avatar = 47,
            /// <summary></summary>
            Grass = 95,
            /// <summary></summary>
            NewTree = 111,
            /// <summary></summary>
            ParticleSystem = 143,
            /// <summary></summary>
            Tree = 255
        }

        /// <summary>
        /// 
        /// </summary>
        public enum AttachmentPoint : byte
        {
            /// <summary></summary>
            Default = 0,
            /// <summary></summary>
            Chest = 1,
            /// <summary></summary>
            Skull,
            /// <summary></summary>
            LeftShoulder,
            /// <summary></summary>
            RightShoulder,
            /// <summary></summary>
            LeftHand,
            /// <summary></summary>
            RightHand,
            /// <summary></summary>
            LeftFoot,
            /// <summary></summary>
            RightFoot,
            /// <summary></summary>
            Spine,
            /// <summary></summary>
            Pelvis,
            /// <summary></summary>
            Mouth,
            /// <summary></summary>
            Chin,
            /// <summary></summary>
            LeftEar,
            /// <summary></summary>
            RightEar,
            /// <summary></summary>
            LeftEyeball,
            /// <summary></summary>
            RightEyeball,
            /// <summary></summary>
            Nose,
            /// <summary></summary>
            RightUpperArm,
            /// <summary></summary>
            RightForearm,
            /// <summary></summary>
            LeftUpperArm,
            /// <summary></summary>
            LeftForearm,
            /// <summary></summary>
            RightHip,
            /// <summary></summary>
            RightUpperLeg,
            /// <summary></summary>
            RightLowerLeg,
            /// <summary></summary>
            LeftHip,
            /// <summary></summary>
            LeftUpperLeg,
            /// <summary></summary>
            LeftLowerLeg,
            /// <summary></summary>
            Stomach,
            /// <summary></summary>
            LeftPec,
            /// <summary></summary>
            RightPec,
            /// <summary></summary>
            HUDCenter2,
            /// <summary></summary>
            HUDTopRight,
            /// <summary></summary>
            HUDTop,
            /// <summary></summary>
            HUDTopLeft,
            /// <summary></summary>
            HUDCenter,
            /// <summary></summary>
            HUDBottomLeft,
            /// <summary></summary>
            HUDBottom,
            /// <summary></summary>
            HUDBottomRight
        }

        /// <summary>
        /// Bitflag field for ObjectUpdateCompressed data blocks, describing 
        /// which options are present for each object
        /// </summary>
        [Flags]
        public enum CompressedFlags : uint
        {
            /// <summary>Hasn't been spotted in the wild yet</summary>
            ScratchPad = 0x01,
            /// <summary>This may be incorrect</summary>
            Tree = 0x02,
            /// <summary>Whether the object has floating text ala llSetText</summary>
            HasText = 0x04,
            /// <summary>Whether the object has an active particle system</summary>
            HasParticles = 0x08,
            /// <summary>Whether the object has sound attached to it</summary>
            HasSound = 0x10,
            /// <summary>Whether the object is attached to a root object or not</summary>
            HasParent = 0x20,
            /// <summary>Whether the object has texture animation settings</summary>
            TextureAnimation = 0x40,
            /// <summary>Whether the object has an angular velocity</summary>
            HasAngularVelocity = 0x80,
            /// <summary>Whether the object has a name value pairs string</summary>
            HasNameValues = 0x100,
            /// <summary>Whether the object has a Media URL set</summary>
            MediaURL = 0x200
        }

        /// <summary>
        /// 
        /// </summary>
        public enum Tree : byte
        {
            /// <summary></summary>
            Pine1 = 0,
            /// <summary></summary>
            Oak,
            /// <summary></summary>
            TropicalBush1,
            /// <summary></summary>
            Palm1,
            /// <summary></summary>
            Dogwood,
            /// <summary></summary>
            TropicalBush2,
            /// <summary></summary>
            Palm2,
            /// <summary></summary>
            Cypress1,
            /// <summary></summary>
            Cypress2,
            /// <summary></summary>
            Pine2,
            /// <summary></summary>
            Plumeria,
            /// <summary></summary>
            WinterPine1,
            /// <summary></summary>
            WinterAspen,
            /// <summary></summary>
            WinterPine2,
            /// <summary></summary>
            Eucalyptus,
            /// <summary></summary>
            Fern,
            /// <summary></summary>
            Eelgrass,
            /// <summary></summary>
            SeaSword,
            /// <summary></summary>
            Kelp1,
            /// <summary></summary>
            BeachGrass1,
            /// <summary></summary>
            Kelp2
        }

        /// <summary>
        /// 
        /// </summary>
        public enum Grass : byte
        {
            /// <summary></summary>
            Grass0 = 0,
            /// <summary></summary>
            Grass1,
            /// <summary></summary>
            Grass2,
            /// <summary></summary>
            Grass3,
            /// <summary></summary>
            Grass4,
            /// <summary></summary>
            Undergrowth1
        }

        /// <summary>
        /// 
        /// </summary>
        public enum ClickAction : byte
        {
            /// <summary></summary>
            Touch = 0,
            /// <summary></summary>
            Sit = 1,
            /// <summary></summary>
            Buy = 2
        }

        #endregion


        #region Events

        /// <summary>
        /// This event will be raised for every ObjectUpdate block that 
        /// contains a prim that isn't attached to an avatar.
        /// </summary>
        /// <remarks>Depending on the circumstances a client could 
        /// receive two or more of these events for the same object, if you 
        /// or the object left the current sim and returned for example. Client
        /// applications are responsible for tracking and storing objects.
        /// </remarks>
        public event NewPrimCallback OnNewPrim;
        /// <summary>
        /// This event will be raised for every ObjectUpdate block that 
        /// contains an avatar attachment.
        /// </summary>
        /// <remarks>Depending on the circumstances a client could 
        /// receive two or more of these events for the same object, if you 
        /// or the object left the current sim and returned for example. Client
        /// applications are responsible for tracking and storing objects.
        /// </remarks>
        public event NewAttachmentCallback OnNewAttachment;
        /// <summary>
        /// This event will be raised for every ObjectUpdate block that 
        /// contains a new avatar.
        /// </summary>
        /// <remarks>Depending on the circumstances a client 
        /// could receive two or more of these events for the same avatar, if 
        /// you or the other avatar left the current sim and returned for 
        /// example. Client applications are responsible for tracking and 
        /// storing objects.
        /// </remarks>
        public event NewAvatarCallback OnNewAvatar;
        /// <summary>
        /// This event will be raised for every ObjectUpdate block that 
        /// contains a new tree or grass patch.
        /// </summary>
        /// <remarks>Depending on the circumstances a client could 
        /// receive two or more of these events for the same object, if you 
        /// or the object left the current sim and returned for example. Client
        /// applications are responsible for tracking and storing objects.
        /// </remarks>
        public event NewFoliageCallback OnNewFoliage;
        /// <summary>
        /// This event will be raised when a terse object update packet is 
        /// received, containing the updated position, rotation, and 
        /// movement-related vectors
        /// </summary>
        public event ObjectUpdatedCallback OnObjectUpdated;
        /// <summary>
        /// This event will be raised when an avatar sits on an object
        /// or stands up, with a local ID of the current seat or zero.
        /// </summary>
        public event AvatarSitChanged OnAvatarSitChanged;
        /// <summary>
        /// This event will be raised when an object is removed from a 
        /// simulator.
        /// </summary>
        public event KillObjectCallback OnObjectKilled;
        /// <summary>
        /// This event will be raised when an objects properties are received
        /// from the simulator
        /// </summary>
        public event ObjectPropertiesCallback OnObjectProperties;
        /// <summary>
        /// Thie event will be raised when an objects properties family 
        /// information is recieved from the simulator. ObjectPropertiesFamily
        /// is a subset of the fields found in ObjectProperties
        /// </summary>
        public event ObjectPropertiesFamilyCallback OnObjectPropertiesFamily;

        #endregion


        private const float HAVOK_TIMESTEP = 1.0f / 45.0f;


        /// <summary>
        /// Reference to the SecondLife client
        /// </summary>
        protected SecondLife Client;


        private System.Timers.Timer InterpolationTimer;


        /// <summary>
        /// Instantiates a new ObjectManager class. This class should only be accessed
        /// through SecondLife.Objects, client applications should never create their own
        /// </summary>
        /// <param name="client">A reference to the client</param>
        public ObjectManager(SecondLife client)
        {
            Client = client;
            RegisterCallbacks();
        }

        /// <summary>
        /// Instantiates a new ObjectManager class. This class should only be 
        /// accessed through SecondLife.Objects, client applications should 
        /// never create their own
        /// </summary>
        /// <param name="client">A reference to the client</param>
        /// <param name="registerCallbacks">If false, the ObjectManager won't
        /// register any packet callbacks and won't decode incoming object
        /// packets</param>
        protected ObjectManager(SecondLife client, bool registerCallbacks)
        {
            Client = client;

            if (registerCallbacks)
            {
                RegisterCallbacks();
            }
        }

        protected void RegisterCallbacks()
        {
            Client.Network.RegisterCallback(PacketType.ObjectUpdate, new NetworkManager.PacketCallback(UpdateHandler));
            Client.Network.RegisterCallback(PacketType.ImprovedTerseObjectUpdate, new NetworkManager.PacketCallback(TerseUpdateHandler));
            Client.Network.RegisterCallback(PacketType.ObjectUpdateCompressed, new NetworkManager.PacketCallback(CompressedUpdateHandler));
            Client.Network.RegisterCallback(PacketType.ObjectUpdateCached, new NetworkManager.PacketCallback(CachedUpdateHandler));
            Client.Network.RegisterCallback(PacketType.KillObject, new NetworkManager.PacketCallback(KillObjectHandler));
            Client.Network.RegisterCallback(PacketType.ObjectPropertiesFamily, new NetworkManager.PacketCallback(ObjectPropertiesFamilyHandler));
            Client.Network.RegisterCallback(PacketType.ObjectProperties, new NetworkManager.PacketCallback(ObjectPropertiesHandler));

            // If the callbacks aren't registered there's not point in doing client-side path prediction,
            // so we set it up here
            InterpolationTimer = new System.Timers.Timer(Settings.INTERPOLATION_INTERVAL);
            InterpolationTimer.Elapsed += new System.Timers.ElapsedEventHandler(InterpolationTimer_Elapsed);
            InterpolationTimer.Start();
        }

        #region Action Methods

        /// <summary>
        /// Request object information from the sim, primarily used for stale 
        /// or missing cache entries
        /// </summary>
        /// <param name="simulator">The simulator containing the object you're 
        /// looking for</param>
        /// <param name="localID">The local ID of the object</param>
        public void RequestObject(Simulator simulator, uint localID)
        {
            RequestMultipleObjectsPacket request = new RequestMultipleObjectsPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.ObjectData = new RequestMultipleObjectsPacket.ObjectDataBlock[1];
            request.ObjectData[0] = new RequestMultipleObjectsPacket.ObjectDataBlock();
            request.ObjectData[0].ID = localID;
            request.ObjectData[0].CacheMissType = 0;

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Request object information for multiple objects all contained in
        /// the same sim, primarily used for stale or missing cache entries
        /// </summary>
        /// <param name="simulator">The simulator containing the object you're 
        /// looking for</param>
        /// <param name="localIDs">A list of local IDs of the objects</param>
        public void RequestObjects(Simulator simulator, List<uint> localIDs)
        {
            int i = 0;

            RequestMultipleObjectsPacket request = new RequestMultipleObjectsPacket();
            request.AgentData.AgentID = Client.Self.AgentID;
            request.AgentData.SessionID = Client.Self.SessionID;
            request.ObjectData = new RequestMultipleObjectsPacket.ObjectDataBlock[localIDs.Count];

            foreach (uint localID in localIDs)
            {
                request.ObjectData[i] = new RequestMultipleObjectsPacket.ObjectDataBlock();
                request.ObjectData[i].ID = localID;
                request.ObjectData[i].CacheMissType = 0;

                i++;
            }

            Client.Network.SendPacket(request, simulator);
        }

        /// <summary>
        /// Attempt to purchase an original object, a copy, or the contents of
        /// an object
        /// </summary>
        /// <param name="simulator">Simulator where the object resides</param>
        /// <param name="localID">Sim-local ID of the object</param>
        /// <param name="saleType">Whether the original, a copy, or the object
        /// contents are on sale. This is used for verification, if the this
        /// sale type is not valid for the object the purchase will fail</param>
        /// <param name="price">Price of the object. This is used for 
        /// verification, if it does not match the actual price the purchase
        /// will fail</param>
        /// <param name="groupID">Group ID that will be associated with the new
        /// purchase</param>
        /// <param name="categoryID">Inventory folder UUID where the purchase
        /// should go</param>
        /// <example>BuyObject(Client.Network.CurrentSim, 500, SaleType.Copy, 
        /// 100, LLUUID.Zero, Client.Self.InventoryRootFolderUUID);</example>
        public void BuyObject(Simulator simulator, uint localID, SaleType saleType, int price, LLUUID groupID, 
            LLUUID categoryID)
        {
            ObjectBuyPacket buy = new ObjectBuyPacket();

            buy.AgentData.AgentID = Client.Self.AgentID;
            buy.AgentData.SessionID = Client.Self.SessionID;
            buy.AgentData.GroupID = groupID;
            buy.AgentData.CategoryID = categoryID;

            buy.ObjectData = new ObjectBuyPacket.ObjectDataBlock[1];
            buy.ObjectData[0] = new ObjectBuyPacket.ObjectDataBlock();
            buy.ObjectData[0].ObjectLocalID = localID;
            buy.ObjectData[0].SaleType = (byte)saleType;
            buy.ObjectData[0].SalePrice = price;

            Client.Network.SendPacket(buy, simulator);
        }

        /// <summary>
        /// Select an object. This will trigger the simulator to send us back 
        /// an ObjectProperties packet so we can get the full information for
        /// this object
        /// </summary>
        /// <param name="simulator">Simulator where the object resides</param>
        /// <param name="localID">Sim-local ID of the object to select</param>
        public void SelectObject(Simulator simulator, uint localID)
        {
            ObjectSelectPacket select = new ObjectSelectPacket();

            select.AgentData.AgentID = Client.Self.AgentID;
            select.AgentData.SessionID = Client.Self.SessionID;

            select.ObjectData = new ObjectSelectPacket.ObjectDataBlock[1];
            select.ObjectData[0] = new ObjectSelectPacket.ObjectDataBlock();
            select.ObjectData[0].ObjectLocalID = localID;

            Client.Network.SendPacket(select, simulator);
        }

        /// <summary>
        /// Select multiple objects. This will trigger the simulator to send us
        /// back ObjectProperties for each object
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localIDs"></param>
        public void SelectObjects(Simulator simulator, uint[] localIDs)
        {
            ObjectSelectPacket select = new ObjectSelectPacket();

            select.AgentData.AgentID = Client.Self.AgentID;
            select.AgentData.SessionID = Client.Self.SessionID;

            select.ObjectData = new ObjectSelectPacket.ObjectDataBlock[localIDs.Length];

            for (int i = 0; i < localIDs.Length; i++)
            {
                select.ObjectData[i] = new ObjectSelectPacket.ObjectDataBlock();
                select.ObjectData[i].ObjectLocalID = localIDs[i];
            }

            Client.Network.SendPacket(select, simulator);
        }

        public void DeselectObject(Simulator simulator, uint localID)
        {
            ObjectDeselectPacket deselect = new ObjectDeselectPacket();

            deselect.AgentData.AgentID = Client.Self.AgentID;
            deselect.AgentData.SessionID = Client.Self.SessionID;

            deselect.ObjectData = new ObjectDeselectPacket.ObjectDataBlock[1];
            deselect.ObjectData[0] = new ObjectDeselectPacket.ObjectDataBlock();
            deselect.ObjectData[0].ObjectLocalID = localID;

            Client.Network.SendPacket(deselect, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        public void ClickObject(Simulator simulator, uint localID)
        {
            ObjectGrabPacket grab = new ObjectGrabPacket();
            grab.AgentData.AgentID = Client.Self.AgentID;
            grab.AgentData.SessionID = Client.Self.SessionID;
            grab.ObjectData.GrabOffset = LLVector3.Zero;
            grab.ObjectData.LocalID = localID;

            Client.Network.SendPacket(grab, simulator);

            // TODO: If these hit the server out of order the click will fail 
            // and we'll be grabbing the object

            ObjectDeGrabPacket degrab = new ObjectDeGrabPacket();
            degrab.AgentData.AgentID = Client.Self.AgentID;
            degrab.AgentData.SessionID = Client.Self.SessionID;
            degrab.ObjectData.LocalID = localID;

            Client.Network.SendPacket(degrab, simulator);
        }

        /// <summary>
        /// Create, or "rez" a new prim object in a simulator
        /// </summary>
        /// <param name="simulator">The target simulator</param>
        /// <param name="prim">Data describing the prim object to rez</param>
        /// <param name="groupID">Group ID that this prim is set to, or LLUUID.Zero</param>
        /// <param name="position">An approximation of the position at which to rez the prim</param>
        /// <param name="scale">Scale vector to size this prim</param>
        /// <param name="rotation">Rotation quaternion to rotate this prim</param>
        /// <remarks>Due to the way client prim rezzing is done on the server,
        /// the requested position for an object is only close to where the prim
        /// actually ends up. If you desire exact placement you'll need to 
        /// follow up by moving the object after it has been created. This
        /// function will not set textures, light and flexible data, or other 
        /// extended primitive properties</remarks>
        public void AddPrim(Simulator simulator, LLObject.ObjectData prim, LLUUID groupID, LLVector3 position, 
            LLVector3 scale, LLQuaternion rotation)
        {
            ObjectAddPacket packet = new ObjectAddPacket();

            packet.AgentData.AgentID = Client.Self.AgentID;
            packet.AgentData.SessionID = Client.Self.SessionID;
            packet.AgentData.GroupID = groupID;

            packet.ObjectData.State = (byte)prim.State;
            packet.ObjectData.AddFlags = (uint)LLObject.ObjectFlags.CreateSelected;
            packet.ObjectData.PCode = (byte)PCode.Prim;

            packet.ObjectData.Material = (byte)prim.Material;
            packet.ObjectData.Scale = scale;
            packet.ObjectData.Rotation = rotation;

            packet.ObjectData.PathCurve = (byte)prim.PathCurve;
            packet.ObjectData.PathBegin = LLObject.PathBeginUInt16(prim.PathBegin);
            packet.ObjectData.PathEnd = LLObject.PathEndUInt16(prim.PathEnd);
            packet.ObjectData.PathRadiusOffset = LLObject.PathRadiusOffsetByte(prim.PathRadiusOffset);
            packet.ObjectData.PathRevolutions = LLObject.PathRevolutionsByte(prim.PathRevolutions);
            packet.ObjectData.PathScaleX = LLObject.PathScaleByte(prim.PathScaleX);
            packet.ObjectData.PathScaleY = LLObject.PathScaleByte(prim.PathScaleY);
            packet.ObjectData.PathShearX = LLObject.PathShearByte(prim.PathShearX);
            packet.ObjectData.PathShearY = LLObject.PathShearByte(prim.PathShearY);
            packet.ObjectData.PathSkew = LLObject.PathSkewByte(prim.PathSkew);
            packet.ObjectData.PathTaperX = LLObject.PathTaperByte(prim.PathTaperX);
            packet.ObjectData.PathTaperY = LLObject.PathTaperByte(prim.PathTaperY);
            packet.ObjectData.PathTwist = (sbyte)prim.PathTwist;
            packet.ObjectData.PathTwistBegin = (sbyte)prim.PathTwistBegin;

            packet.ObjectData.ProfileCurve = (byte)prim.ProfileCurve;
            packet.ObjectData.ProfileBegin = LLObject.ProfileBeginUInt16(prim.ProfileBegin);
            packet.ObjectData.ProfileEnd = LLObject.ProfileEndUInt16(prim.ProfileEnd);
            packet.ObjectData.ProfileHollow = LLObject.ProfileHollowUInt16(prim.ProfileHollow);

            packet.ObjectData.RayStart = position;
            packet.ObjectData.RayEnd = position;
            packet.ObjectData.RayEndIsIntersection = 0;
            packet.ObjectData.RayTargetID = LLUUID.Zero;
            packet.ObjectData.BypassRaycast = 1;

            Client.Network.SendPacket(packet, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="scale"></param>
        /// <param name="rotation"></param>
        /// <param name="position"></param>
        /// <param name="treeType"></param>
        /// <param name="groupOwner"></param>
        /// <param name="newTree"></param>
        public void AddTree(Simulator simulator, LLVector3 scale, LLQuaternion rotation, LLVector3 position, 
            Tree treeType, LLUUID groupOwner, bool newTree)
        {
            ObjectAddPacket add = new ObjectAddPacket();

            add.AgentData.AgentID = Client.Self.AgentID;
            add.AgentData.SessionID = Client.Self.SessionID;
            add.AgentData.GroupID = groupOwner;
            add.ObjectData.BypassRaycast = 1;
            add.ObjectData.Material = 3;
            add.ObjectData.PathCurve = 16;
            add.ObjectData.PCode = newTree ? (byte)PCode.NewTree : (byte)PCode.Tree;
            add.ObjectData.RayEnd = position;
            add.ObjectData.RayStart = position;
            add.ObjectData.RayTargetID = LLUUID.Zero;
            add.ObjectData.Rotation = rotation;
            add.ObjectData.Scale = scale;
            add.ObjectData.State = (byte)treeType;

            Client.Network.SendPacket(add, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="scale"></param>
        /// <param name="rotation"></param>
        /// <param name="position"></param>
        /// <param name="grassType"></param>
        /// <param name="groupOwner"></param>
        public void AddGrass(Simulator simulator, LLVector3 scale, LLQuaternion rotation, LLVector3 position,
            Grass grassType, LLUUID groupOwner)
        {
            ObjectAddPacket add = new ObjectAddPacket();

            add.AgentData.AgentID = Client.Self.AgentID;
            add.AgentData.SessionID = Client.Self.SessionID;
            add.AgentData.GroupID = groupOwner;
            add.ObjectData.BypassRaycast = 1;
            add.ObjectData.Material = 3;
            add.ObjectData.PathCurve = 16;
            add.ObjectData.PCode = (byte)PCode.Grass;
            add.ObjectData.RayEnd = position;
            add.ObjectData.RayStart = position;
            add.ObjectData.RayTargetID = LLUUID.Zero;
            add.ObjectData.Rotation = rotation;
            add.ObjectData.Scale = scale;
            add.ObjectData.State = (byte)grassType;

            Client.Network.SendPacket(add, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="textures"></param>
        public void SetTextures(Simulator simulator, uint localID, LLObject.TextureEntry textures)
        {
            SetTextures(simulator, localID, textures, String.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="textures"></param>
        /// <param name="mediaUrl"></param>
        public void SetTextures(Simulator simulator, uint localID, LLObject.TextureEntry textures, string mediaUrl)
        {
            ObjectImagePacket image = new ObjectImagePacket();

            image.AgentData.AgentID = Client.Self.AgentID;
            image.AgentData.SessionID = Client.Self.SessionID;
            image.ObjectData = new ObjectImagePacket.ObjectDataBlock[1];
            image.ObjectData[0] = new ObjectImagePacket.ObjectDataBlock();
            image.ObjectData[0].ObjectLocalID = localID;
            image.ObjectData[0].TextureEntry = textures.ToBytes();
            image.ObjectData[0].MediaURL = Helpers.StringToField(mediaUrl);

            Client.Network.SendPacket(image, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="light"></param>
        public void SetLight(Simulator simulator, uint localID, Primitive.LightData light)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();

            extra.AgentData.AgentID = Client.Self.AgentID;
            extra.AgentData.SessionID = Client.Self.SessionID;
            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)Primitive.ExtraParamType.Light;
            extra.ObjectData[0].ParamInUse = true;
            extra.ObjectData[0].ParamData = light.GetBytes();
            extra.ObjectData[0].ParamSize = (uint)extra.ObjectData[0].ParamData.Length;

            Client.Network.SendPacket(extra, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="flexible"></param>
        public void SetFlexible(Simulator simulator, uint localID, Primitive.FlexibleData flexible)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();

            extra.AgentData.AgentID = Client.Self.AgentID;
            extra.AgentData.SessionID = Client.Self.SessionID;
            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)Primitive.ExtraParamType.Flexible;
            extra.ObjectData[0].ParamInUse = true;
            extra.ObjectData[0].ParamData = flexible.GetBytes();
            extra.ObjectData[0].ParamSize = (uint)extra.ObjectData[0].ParamData.Length;

            Client.Network.SendPacket(extra, simulator);
        }

        public void SetSculpt(Simulator simulator, uint localID, Primitive.SculptData sculpt)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();

            extra.AgentData.AgentID = Client.Self.AgentID;
            extra.AgentData.SessionID = Client.Self.SessionID;

            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)Primitive.ExtraParamType.Sculpt;
            extra.ObjectData[0].ParamInUse = true;
            extra.ObjectData[0].ParamData = sculpt.GetBytes();
            extra.ObjectData[0].ParamSize = (uint)extra.ObjectData[0].ParamData.Length;

            Client.Network.SendPacket(extra, simulator);

            // Not sure why, but if you don't send this the sculpted prim disappears
            ObjectShapePacket shape = new ObjectShapePacket();

            shape.AgentData.AgentID = Client.Self.AgentID;
            shape.AgentData.SessionID = Client.Self.SessionID;

            shape.ObjectData = new libsecondlife.Packets.ObjectShapePacket.ObjectDataBlock[1];
            shape.ObjectData[0] = new libsecondlife.Packets.ObjectShapePacket.ObjectDataBlock();
            shape.ObjectData[0].ObjectLocalID = localID;
            shape.ObjectData[0].PathScaleX = 100;
            shape.ObjectData[0].PathScaleY = 150;
            shape.ObjectData[0].PathCurve = 32;

            Client.Network.SendPacket(shape, simulator);
        }

        public void SetExtraParamOff(Simulator simulator, uint localID, Primitive.ExtraParamType type)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();

            extra.AgentData.AgentID = Client.Self.AgentID;
            extra.AgentData.SessionID = Client.Self.SessionID;
            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)type;
            extra.ObjectData[0].ParamInUse = false;
            extra.ObjectData[0].ParamData = new byte[0];
            extra.ObjectData[0].ParamSize = 0;

            Client.Network.SendPacket(extra, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localIDs"></param>
        public void LinkPrims(Simulator simulator, List<uint> localIDs)
        {
            ObjectLinkPacket packet = new ObjectLinkPacket();

            packet.AgentData.AgentID = Client.Self.AgentID;
            packet.AgentData.SessionID = Client.Self.SessionID;

            packet.ObjectData = new ObjectLinkPacket.ObjectDataBlock[localIDs.Count];

            int i = 0;
            foreach (uint localID in localIDs)
            {
                packet.ObjectData[i] = new ObjectLinkPacket.ObjectDataBlock();
                packet.ObjectData[i].ObjectLocalID = localID;

                i++;
            }

            Client.Network.SendPacket(packet, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="rotation"></param>
        public void SetRotation(Simulator simulator, uint localID, LLQuaternion rotation)
        {
            ObjectRotationPacket objRotPacket = new ObjectRotationPacket();
            objRotPacket.AgentData.AgentID = Client.Self.AgentID;
            objRotPacket.AgentData.SessionID = Client.Self.SessionID;

            objRotPacket.ObjectData = new ObjectRotationPacket.ObjectDataBlock[1];

            objRotPacket.ObjectData[0] = new ObjectRotationPacket.ObjectDataBlock();
            objRotPacket.ObjectData[0].ObjectLocalID = localID;
            objRotPacket.ObjectData[0].Rotation = rotation;
            Client.Network.SendPacket(objRotPacket, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="name"></param>
        public void SetName(Simulator simulator, uint localID, string name)
        {
            SetNames(simulator, new uint[] { localID }, new string[] { name });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localIDs"></param>
        /// <param name="names"></param>
        public void SetNames(Simulator simulator, uint[] localIDs, string[] names)
        {
            ObjectNamePacket namePacket = new ObjectNamePacket();
            namePacket.AgentData.AgentID = Client.Self.AgentID;
            namePacket.AgentData.SessionID = Client.Self.SessionID;

            namePacket.ObjectData = new ObjectNamePacket.ObjectDataBlock[localIDs.Length];

            for (int i = 0; i < localIDs.Length; ++i)
            {
                namePacket.ObjectData[i] = new ObjectNamePacket.ObjectDataBlock();
                namePacket.ObjectData[i].LocalID = localIDs[i];
                namePacket.ObjectData[i].Name = Helpers.StringToField(names[i]);
            }

            Client.Network.SendPacket(namePacket, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="description"></param>
        public void SetDescription(Simulator simulator, uint localID, string description)
        {
            SetDescriptions(simulator, new uint[] { localID }, new string[] { description });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localIDs"></param>
        /// <param name="descriptions"></param>
        public void SetDescriptions(Simulator simulator, uint[] localIDs, string[] descriptions)
        {
            ObjectDescriptionPacket descPacket = new ObjectDescriptionPacket();
            descPacket.AgentData.AgentID = Client.Self.AgentID;
            descPacket.AgentData.SessionID = Client.Self.SessionID;

            descPacket.ObjectData = new ObjectDescriptionPacket.ObjectDataBlock[localIDs.Length];

            for (int i = 0; i < localIDs.Length; ++i)
            {
                descPacket.ObjectData[i] = new ObjectDescriptionPacket.ObjectDataBlock();
                descPacket.ObjectData[i].LocalID = localIDs[i];
                descPacket.ObjectData[i].Description = Helpers.StringToField(descriptions[i]);
            }

            Client.Network.SendPacket(descPacket, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="attachPoint"></param>
        /// <param name="rotation"></param>
        public void AttachObject(Simulator simulator, uint localID, AttachmentPoint attachPoint, LLQuaternion rotation)
        {
            ObjectAttachPacket attach = new ObjectAttachPacket();
            attach.AgentData.AgentID = Client.Self.AgentID;
            attach.AgentData.SessionID = Client.Self.SessionID;
            attach.AgentData.AttachmentPoint = (byte)attachPoint;

            attach.ObjectData = new ObjectAttachPacket.ObjectDataBlock[1];
            attach.ObjectData[0] = new ObjectAttachPacket.ObjectDataBlock();
            attach.ObjectData[0].ObjectLocalID = localID;
            attach.ObjectData[0].Rotation = rotation;

            Client.Network.SendPacket(attach, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localIDs"></param>
        public void DetachObjects(Simulator simulator, List<uint> localIDs)
        {
            ObjectDetachPacket detach = new ObjectDetachPacket();
            detach.AgentData.AgentID = Client.Self.AgentID;
            detach.AgentData.SessionID = Client.Self.SessionID;
            detach.ObjectData = new ObjectDetachPacket.ObjectDataBlock[localIDs.Count];

            int i = 0;
            foreach (uint localid in localIDs)
            {
                detach.ObjectData[i] = new ObjectDetachPacket.ObjectDataBlock();
                detach.ObjectData[i].ObjectLocalID = localid;
                i++;
            }

            Client.Network.SendPacket(detach, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="position"></param>
        public void SetPosition(Simulator simulator, uint localID, LLVector3 position)
        {
            ObjectPositionPacket objPosPacket = new ObjectPositionPacket();
            objPosPacket.AgentData.AgentID = Client.Self.AgentID;
            objPosPacket.AgentData.SessionID = Client.Self.SessionID;

            objPosPacket.ObjectData = new ObjectPositionPacket.ObjectDataBlock[1];

            objPosPacket.ObjectData[0] = new ObjectPositionPacket.ObjectDataBlock();
            objPosPacket.ObjectData[0].ObjectLocalID = localID;
            objPosPacket.ObjectData[0].Position = position;

            Client.Network.SendPacket(objPosPacket, simulator);
        }

        /// <summary>
        /// Deed an object (prim) to a group, Object must be shared with group which
        /// can be accomplished with SetPermissions()
        /// </summary>
        /// <param name="simulator">Simulator containing object</param>
        /// <param name="LocalID">LocalID of Object</param>
        /// <param name="group">Group to deed object to</param>
        public void DeedObject(Simulator simulator, uint localID, LLUUID group)
        {
            ObjectOwnerPacket objDeedPacket = new ObjectOwnerPacket();
            objDeedPacket.AgentData.AgentID = Client.Self.AgentID;
            objDeedPacket.AgentData.SessionID = Client.Self.SessionID;

            // Can only be use in God mode
            objDeedPacket.HeaderData.Override = false;
            objDeedPacket.HeaderData.OwnerID = LLUUID.Zero;
            objDeedPacket.HeaderData.GroupID = group;

            objDeedPacket.ObjectData = new ObjectOwnerPacket.ObjectDataBlock[1];
            objDeedPacket.ObjectData[0] = new ObjectOwnerPacket.ObjectDataBlock();
            
            objDeedPacket.ObjectData[0].ObjectLocalID = localID;
            
            Client.Network.SendPacket(objDeedPacket, simulator);
        }

        /// <summary>
        /// Deed multiple objects (prims) to a group, Objects must be shared with group which
        /// can be accomplished with SetPermissions()
        /// </summary>
        /// <param name="simulator">Simulator containing objects</param>
        /// <param name="LocalIDs">List of LocalIDs</param>
        /// <param name="group">Group to deed objects to.</param>
        public void DeedObjects(Simulator simulator, List<uint> localIDs, LLUUID group)
        {
            ObjectOwnerPacket packet = new ObjectOwnerPacket();
            packet.AgentData.AgentID = Client.Self.AgentID;
            packet.AgentData.SessionID = Client.Self.SessionID;

            // Can only be use in God mode
            packet.HeaderData.Override = false;
            packet.HeaderData.OwnerID = LLUUID.Zero;
            packet.HeaderData.GroupID = group;

            packet.ObjectData = new ObjectOwnerPacket.ObjectDataBlock[localIDs.Count];

            for (int i = 0; i < localIDs.Count; i++)
            {
                packet.ObjectData[i] = new ObjectOwnerPacket.ObjectDataBlock();
                packet.ObjectData[i].ObjectLocalID = localIDs[i];
            }
            Client.Network.SendPacket(packet, simulator);
        }
            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localIDs"></param>
        /// <param name="who"></param>
        /// <param name="permissions"></param>
        /// <param name="set"></param>
        public void SetPermissions(Simulator simulator, List<uint> localIDs, PermissionWho who, 
            PermissionMask permissions, bool set)
        {
            ObjectPermissionsPacket packet = new ObjectPermissionsPacket();

            packet.AgentData.AgentID = Client.Self.AgentID;
            packet.AgentData.SessionID = Client.Self.SessionID;

            // Override can only be used by gods
            packet.HeaderData.Override = false;

            packet.ObjectData = new ObjectPermissionsPacket.ObjectDataBlock[localIDs.Count];

            for (int i = 0; i < localIDs.Count; i++)
            {
                packet.ObjectData[i] = new ObjectPermissionsPacket.ObjectDataBlock();

                packet.ObjectData[i].ObjectLocalID = localIDs[i];
                packet.ObjectData[i].Field = (byte)who;
                packet.ObjectData[i].Mask = (uint)permissions;
                packet.ObjectData[i].Set = Convert.ToByte(set);
            }

            Client.Network.SendPacket(packet, simulator);
        }

        /// <summary>
        /// Request additional properties for an object
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="objectID"></param>
        public void RequestObjectPropertiesFamily(Simulator simulator, LLUUID objectID)
        {
            RequestObjectPropertiesFamily(simulator, objectID, true);
        }

        /// <summary>
        /// Request additional properties for an object
        /// </summary>
        /// <param name="simulator">Simulator containing the object</param>
        /// <param name="objectID">Absolute UUID of the object</param>
        /// <param name="reliable">Whether to require server acknowledgement of this request</param>
        public void RequestObjectPropertiesFamily(Simulator simulator, LLUUID objectID, bool reliable)
        {
            RequestObjectPropertiesFamilyPacket properties = new RequestObjectPropertiesFamilyPacket();
            properties.AgentData.AgentID = Client.Self.AgentID;
            properties.AgentData.SessionID = Client.Self.SessionID;
            properties.ObjectData.ObjectID = objectID;
            // TODO: RequestFlags is typically only for bug report submissions, but we might be able to
            // use it to pass an arbitrary uint back to the callback
            properties.ObjectData.RequestFlags = 0;

            properties.Header.Reliable = reliable;

            Client.Network.SendPacket(properties, simulator);
        }

        #endregion
        
        #region Packet Handlers

        /// <summary>
        /// Used for new prims, or significant changes to existing prims
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        protected void UpdateHandler(Packet packet, Simulator simulator)
        {
            ObjectUpdatePacket update = (ObjectUpdatePacket)packet;
			UpdateDilation(simulator, update.RegionData.TimeDilation);

            for (int b = 0; b < update.ObjectData.Length; b++)
            {
                ObjectUpdatePacket.ObjectDataBlock block = update.ObjectData[b];

                LLVector4 collisionPlane = LLVector4.Zero;
                LLVector3 position;
                LLVector3 velocity;
                LLVector3 acceleration;
                LLQuaternion rotation;
                LLVector3 angularVelocity;
                NameValue[] nameValues;
                bool attachment = false;
                PCode pcode = (PCode)block.PCode;

                #region Relevance check

                // Check if we are interested in this object
                if (!Client.Settings.ALWAYS_DECODE_OBJECTS)
                {
                    switch (pcode)
                    {
                        case PCode.Grass:
                        case PCode.Tree:
                        case PCode.NewTree:
                            if (OnNewFoliage == null) continue;
                            break;
                        case PCode.Prim:
                            if (OnNewPrim == null) continue;
                            break;
                        case PCode.Avatar:
                            // Make an exception for updates about our own agent
                            if (block.FullID != Client.Self.AgentID && OnNewAvatar == null) continue;
                            break;
                        case PCode.ParticleSystem:
                            continue; // TODO: Do something with these
                    }
                }

                #endregion Relevance check

                #region NameValue parsing

                string nameValue = Helpers.FieldToUTF8String(block.NameValue);
                if (nameValue.Length > 0)
                {
                    string[] lines = nameValue.Split('\n');
                    nameValues = new NameValue[lines.Length];

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (!String.IsNullOrEmpty(lines[i]))
                        {
                            NameValue nv = new NameValue(lines[i]);
                            if (nv.Name == "AttachItemID") attachment = true;
                            nameValues[i] = nv;
                        }
                    }
                }
                else
                {
                    nameValues = new NameValue[0];
                }

                #endregion NameValue parsing

                #region Decode Object (primitive) parameters
                LLObject.ObjectData data = new LLObject.ObjectData();
                data.State = block.State;
                data.Material = (LLObject.MaterialType)block.Material;
                data.PathCurve = (LLObject.PathCurve)block.PathCurve;
                data.ProfileCurve = (LLObject.ProfileCurve)block.ProfileCurve;
                data.PathBegin = LLObject.PathBeginFloat(block.PathBegin);
                data.PathEnd = LLObject.PathEndFloat(block.PathEnd);
                data.PathScaleX = LLObject.PathScaleFloat(block.PathScaleX);
                data.PathScaleY = LLObject.PathScaleFloat(block.PathScaleY);
                data.PathShearX = LLObject.PathShearFloat(block.PathShearX);
                data.PathShearY = LLObject.PathShearFloat(block.PathShearY);
                data.PathTwist = block.PathTwist;
                data.PathTwistBegin = block.PathTwistBegin;
                data.PathRadiusOffset = LLObject.PathRadiusOffsetFloat(block.PathRadiusOffset);
                data.PathTaperX = LLObject.PathTaperFloat(block.PathTaperX);
                data.PathTaperY = LLObject.PathTaperFloat(block.PathTaperY);
                data.PathRevolutions = LLObject.PathRevolutionsFloat(block.PathRevolutions);
                data.PathSkew = LLObject.PathSkewFloat(block.PathSkew);
                data.ProfileBegin = LLObject.ProfileBeginFloat(block.ProfileBegin);
                data.ProfileEnd = LLObject.ProfileEndFloat(block.ProfileEnd);
                data.ProfileHollow = LLObject.ProfileHollowFloat(block.ProfileHollow);
                data.PCode = pcode;
                #endregion

                #region Decode Additional packed parameters in ObjectData
                int pos = 0;
                switch (block.ObjectData.Length)
                {
                    case 76:
                        // Collision normal for avatar
                        collisionPlane = new LLVector4(block.ObjectData, pos);
                        pos += 16;

                        goto case 60;
                    case 60:
                        // Position
                        position = new LLVector3(block.ObjectData, pos);
                        pos += 12;
                        // Velocity
                        velocity = new LLVector3(block.ObjectData, pos);
                        pos += 12;
                        // Acceleration
                        acceleration = new LLVector3(block.ObjectData, pos);
                        pos += 12;
                        // Rotation (theta)
                        rotation = new LLQuaternion(block.ObjectData, pos, true);
                        pos += 12;
                        // Angular velocity (omega)
                        angularVelocity = new LLVector3(block.ObjectData, pos);
                        pos += 12;

                        break;
                    case 48:
                        // Collision normal for avatar
                        collisionPlane = new LLVector4(block.ObjectData, pos);
                        pos += 16;

                        goto case 32;
                    case 32:
                        // The data is an array of unsigned shorts

                        // Position
                        position = new LLVector3(
                            Helpers.UInt16ToFloat(block.ObjectData, pos, -0.5f * 256.0f, 1.5f * 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 2, -0.5f * 256.0f, 1.5f * 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 3.0f * 256.0f));
                        pos += 6;
                        // Velocity
                        velocity = new LLVector3(
                            Helpers.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;
                        // Acceleration
                        acceleration = new LLVector3(
                            Helpers.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;
                        // Rotation (theta)
                        rotation = new LLQuaternion(
                            Helpers.UInt16ToFloat(block.ObjectData, pos, -1.0f, 1.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 2, -1.0f, 1.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 4, -1.0f, 1.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 6, -1.0f, 1.0f));
                        pos += 8;
                        // Angular velocity (omega)
                        angularVelocity = new LLVector3(
                            Helpers.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;

                        break;
                    case 16:
                        // The data is an array of single bytes (8-bit numbers)

                        // Position
                        position = new LLVector3(
                            Helpers.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Velocity
                        velocity = new LLVector3(
                            Helpers.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Accleration
                        acceleration = new LLVector3(
                            Helpers.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Rotation
                        rotation = new LLQuaternion(
                            Helpers.ByteToFloat(block.ObjectData, pos, -1.0f, 1.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 1, -1.0f, 1.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 2, -1.0f, 1.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 3, -1.0f, 1.0f));
                        pos += 4;
                        // Angular Velocity
                        angularVelocity = new LLVector3(
                            Helpers.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;

                        break;
                    default:
                        Client.Log("Got an ObjectUpdate block with ObjectUpdate field length of " +
                            block.ObjectData.Length, Helpers.LogLevel.Warning);

                        continue;
                }
                #endregion

                // Determine the object type and create the appropriate class
                switch (pcode)
                {
                    #region Prim and Foliage
                    case PCode.Grass:
                    case PCode.Tree:
                    case PCode.NewTree:
                    case PCode.Prim:
                        Primitive prim = GetPrimitive(simulator, block.ID, block.FullID);

                        #region Update Prim Info with decoded data                            
                        prim.Flags = (LLObject.ObjectFlags)block.UpdateFlags;

                        if ((prim.Flags & LLObject.ObjectFlags.ZlibCompressed) != 0)
                        {
                            Client.Log("Got a ZlibCompressed ObjectUpdate, implement me!", 
                                Helpers.LogLevel.Warning);
                            continue;
                        }

                        prim.NameValues = nameValues;
                        prim.LocalID = block.ID;
                        prim.ID = block.FullID;
                        prim.ParentID = block.ParentID;
					    prim.RegionHandle = update.RegionData.RegionHandle;
                        prim.Scale = block.Scale;
                        prim.ClickAction = (ClickAction)block.ClickAction;
                        prim.OwnerID = block.OwnerID;
                        prim.MediaURL = Helpers.FieldToUTF8String(block.MediaURL);
                        prim.Text = Helpers.FieldToUTF8String(block.Text);
                        prim.TextColor = new LLColor(block.TextColor, 0);
                        // Alpha is inversed to help zero encoding
                        prim.TextColor.A = (byte)(255 - prim.TextColor.A);

                        // Sound information
                        prim.Sound = block.Sound;
                        prim.SoundFlags = block.Flags;
                        prim.SoundGain = block.Gain;
                        prim.SoundRadius = block.Radius;

                        // Joint information
                        prim.Joint = (Primitive.JointType)block.JointType;
                        prim.JointPivot = block.JointPivot;
                        prim.JointAxisOrAnchor = block.JointAxisOrAnchor;
                        
                        // Object parameters
                        prim.Data = data;

                        // Textures, texture animations, particle system, and extra params
                        prim.Textures = new LLObject.TextureEntry(block.TextureEntry, 0, 
                            block.TextureEntry.Length);

                        LLUUID test = new LLUUID("73818c3a-acc3-30b8-5060-0e6cf693cddf");

                        prim.TextureAnim = new Primitive.TextureAnimation(block.TextureAnim, 0);
                        prim.ParticleSys = new Primitive.ParticleSystem(block.PSBlock, 0);
                        prim.SetExtraParamsFromBytes(block.ExtraParams, 0);

                        // PCode-specific data
                        prim.GenericData = block.Data;

                        // Packed parameters
                        prim.CollisionPlane = collisionPlane;
                        prim.Position = position;
                        prim.Velocity = velocity;
                        prim.Acceleration = acceleration;
                        prim.Rotation = rotation;
                        prim.AngularVelocity = angularVelocity;
                        #endregion

                        if (attachment)
                            FireOnNewAttachment(simulator, prim, update.RegionData.RegionHandle, 
                                update.RegionData.TimeDilation);
                        else if (pcode == PCode.Prim)
                            FireOnNewPrim(simulator, prim, update.RegionData.RegionHandle, 
                                update.RegionData.TimeDilation);
                        else
                            FireOnNewFoliage(simulator, prim, update.RegionData.RegionHandle, 
                                update.RegionData.TimeDilation);

                        break;
                    #endregion Prim and Foliage
                    #region Avatar
                    case PCode.Avatar:
                        // Update some internals if this is our avatar
                        if (block.FullID == Client.Self.AgentID)
                        {
                            #region Update Client.Self
                            
                            // We need the local ID to recognize terse updates for our agent
                            Client.Self.localID = block.ID;
                            
                            // Packed parameters
                            Client.Self.collisionPlane = collisionPlane;
                            Client.Self.relativePosition = position;
                            Client.Self.velocity = velocity;
                            Client.Self.acceleration = acceleration;
                            Client.Self.relativeRotation = rotation;
                            Client.Self.angularVelocity = angularVelocity;

                            #endregion
                        }

                        #region Create an Avatar from the decoded data

                        Avatar avatar = GetAvatar(simulator, block.ID, block.FullID);
                        uint oldSeatID = avatar.sittingOn;

                        avatar.ID = block.FullID;
                        avatar.LocalID = block.ID;
                        avatar.CollisionPlane = collisionPlane;
                        avatar.Position = position;
                        avatar.Velocity = velocity;
                        avatar.Acceleration = acceleration;
                        avatar.Rotation = rotation;
                        avatar.AngularVelocity = angularVelocity;
                        avatar.NameValues = nameValues;
                        avatar.Data = data;
                        avatar.GenericData = block.Data;
                        avatar.sittingOn = block.ParentID;

                        SetAvatarSittingOn(simulator, avatar, block.ParentID, oldSeatID);

                        // Set this avatar online and in a region
                        avatar.Online = true;
                        avatar.CurrentSim = simulator;

                        // Textures
                        avatar.Textures = new Primitive.TextureEntry(block.TextureEntry, 0, 
                            block.TextureEntry.Length);

                        #endregion Create an Avatar from the decoded data

                        FireOnNewAvatar(simulator, avatar, update.RegionData.RegionHandle, 
                            update.RegionData.TimeDilation);

                        break;
                    #endregion Avatar
                    case PCode.ParticleSystem:
                        DecodeParticleUpdate(block);
                        // TODO: Create a callback for particle updates
                        break;
                    default:
                        Client.DebugLog("Got an ObjectUpdate block with an unrecognized PCode " + pcode.ToString());
                        break;
                }
            }
        }

        protected void DecodeParticleUpdate(ObjectUpdatePacket.ObjectDataBlock block)
        {
            // TODO: Handle ParticleSystem ObjectUpdate blocks

            // float bounce_b
            // LLVector4 scale_range
            // LLVector4 alpha_range
            // LLVector3 vel_offset
            // float dist_begin_fadeout
            // float dist_end_fadeout
            // LLUUID image_uuid
            // long flags
            // byte createme
            // LLVector3 diff_eq_alpha
            // LLVector3 diff_eq_scale
            // byte max_particles
            // byte initial_particles
            // float kill_plane_z
            // LLVector3 kill_plane_normal
            // float bounce_plane_z
            // LLVector3 bounce_plane_normal
            // float spawn_range
            // float spawn_frequency
            // float spawn_frequency_range
            // LLVector3 spawn_direction
            // float spawn_direction_range
            // float spawn_velocity
            // float spawn_velocity_range
            // float speed_limit
            // float wind_weight
            // LLVector3 current_gravity
            // float gravity_weight
            // float global_lifetime
            // float individual_lifetime
            // float individual_lifetime_range
            // float alpha_decay
            // float scale_decay
            // float distance_death
            // float damp_motion_factor
            // LLVector3 wind_diffusion_factor
        }

        /// <summary>
        /// A terse object update, used when a transformation matrix or
        /// velocity/acceleration for an object changes but nothing else
        /// (scale/position/rotation/acceleration/velocity)
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        protected void TerseUpdateHandler(Packet packet, Simulator simulator)
        {
            ImprovedTerseObjectUpdatePacket terse = (ImprovedTerseObjectUpdatePacket)packet;
			UpdateDilation(simulator, terse.RegionData.TimeDilation);
			
            for (int i = 0; i < terse.ObjectData.Length; i++)
            {
                ImprovedTerseObjectUpdatePacket.ObjectDataBlock block = terse.ObjectData[i];

                try
                {
                    int pos = 4;
                    uint localid = Helpers.BytesToUIntBig(block.Data, 0);

                    // Check if we are interested in this update
                    if (!Client.Settings.ALWAYS_DECODE_OBJECTS && localid != Client.Self.localID && OnObjectUpdated == null)
                        continue;

                    #region Decode update data

                    ObjectUpdate update = new ObjectUpdate();

                    // LocalID
                    update.LocalID = localid;
                    // State
                    update.State = block.Data[pos++];
                    // Avatar boolean
                    update.Avatar = (block.Data[pos++] != 0);
                    // Collision normal for avatar
                    if (update.Avatar)
                    {
                        update.CollisionPlane = new LLVector4(block.Data, pos);
                        pos += 16;
                    }
                    // Position
                    update.Position = new LLVector3(block.Data, pos);
                    pos += 12;
                    // Velocity
                    update.Velocity = new LLVector3(
                        Helpers.UInt16ToFloat(block.Data, pos, -128.0f, 128.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 2, -128.0f, 128.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 4, -128.0f, 128.0f));
                    pos += 6;
                    // Acceleration
                    update.Acceleration = new LLVector3(
                        Helpers.UInt16ToFloat(block.Data, pos, -64.0f, 64.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 2, -64.0f, 64.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 4, -64.0f, 64.0f));
                    pos += 6;
                    // Rotation (theta)
                    update.Rotation = new LLQuaternion(
                        Helpers.UInt16ToFloat(block.Data, pos, -1.0f, 1.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 2, -1.0f, 1.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 4, -1.0f, 1.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 6, -1.0f, 1.0f));
                    pos += 8;
                    // Angular velocity
                    update.AngularVelocity = new LLVector3(
                        Helpers.UInt16ToFloat(block.Data, pos, -64.0f, 64.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 2, -64.0f, 64.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 4, -64.0f, 64.0f));
                    pos += 6;

                    // Textures
                    // FIXME: Why are we ignoring the first four bytes here?
                    update.Textures = new LLObject.TextureEntry(block.TextureEntry, 4, block.TextureEntry.Length - 4);

                    #endregion Decode update data

                    LLObject obj = (update.Avatar) ?
                        (LLObject)GetAvatar(simulator, update.LocalID, null):
                        (LLObject)GetPrimitive(simulator, update.LocalID, null);

                    #region Update Client.Self
                    if (update.LocalID == Client.Self.localID)
                    {
                        Client.Self.collisionPlane = update.CollisionPlane;
                        Client.Self.relativePosition = update.Position;
                        Client.Self.velocity = update.Velocity;
                        Client.Self.acceleration = update.Acceleration;
                        Client.Self.relativeRotation = update.Rotation;
                        Client.Self.angularVelocity = update.AngularVelocity;
                    }
                    #endregion Update Client.Self

                    obj.Acceleration = update.Acceleration;
                    obj.AngularVelocity = update.AngularVelocity;
                    obj.CollisionPlane = update.CollisionPlane;
                    obj.Position = update.Position;
                    obj.Rotation = update.Rotation;
                    obj.Velocity = update.Velocity;
                    if (update.Textures != null)
                        obj.Textures = update.Textures;
                    
                    // Fire the callback
                    FireOnObjectUpdated(simulator, update, terse.RegionData.RegionHandle, terse.RegionData.TimeDilation);
                }
                catch (Exception e)
                {
                    Client.Log(e.ToString(), Helpers.LogLevel.Warning);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        protected void CompressedUpdateHandler(Packet packet, Simulator simulator)
        {
            ObjectUpdateCompressedPacket update = (ObjectUpdateCompressedPacket)packet;

            for (int b = 0; b < update.ObjectData.Length; b++)
            {
                ObjectUpdateCompressedPacket.ObjectDataBlock block = update.ObjectData[b];
                int i = 0;

                try
                {
                    // UUID
                    LLUUID FullID = new LLUUID(block.Data, 0);
                    i += 16;
                    // Local ID
                    uint LocalID = (uint)(block.Data[i++] + (block.Data[i++] << 8) +
                        (block.Data[i++] << 16) + (block.Data[i++] << 24));
                    // PCode
                    PCode pcode = (PCode)block.Data[i++];

                    #region Relevance check
                    if (!Client.Settings.ALWAYS_DECODE_OBJECTS)
                    {
                        switch (pcode)
                        {
                            case PCode.Grass:
                            case PCode.Tree:
                            case PCode.NewTree:
                                if (OnNewFoliage == null) continue;
                                break;
                            case PCode.Prim:
                                if (OnNewPrim == null) continue;
                                break;
                        }
                    }
                    #endregion Relevance check

                    Primitive prim = GetPrimitive(simulator, LocalID, FullID);

                    prim.LocalID = LocalID;
                    prim.ID = FullID;
                    prim.Flags = (LLObject.ObjectFlags)block.UpdateFlags;
                    prim.Data.PCode = pcode;

                    switch (pcode)
                    {
                        case PCode.Grass:
                        case PCode.Tree:
                        case PCode.NewTree:
                            #region Foliage Decoding

                            // State
                            prim.Data.State = (uint)block.Data[i++];
                            // CRC
                            i += 4;
                            // Material
                            prim.Data.Material = (LLObject.MaterialType)block.Data[i++];
                            // Click action
                            prim.ClickAction = (ClickAction)block.Data[i++];
                            // Scale
                            prim.Scale = new LLVector3(block.Data, i);
                            i += 12;
                            // Position
                            prim.Position = new LLVector3(block.Data, i);
                            i += 12;
                            // Rotation
                            prim.Rotation = new LLQuaternion(block.Data, i, true);
                            i += 12;

                            #endregion Foliage Decoding

                            // FIXME: We are leaving a lot of data left undecoded here, including the
                            // tree species. Need to understand what is going on with these packets
                            // and fix it soon!

                            FireOnNewFoliage(simulator, prim, update.RegionData.RegionHandle, update.RegionData.TimeDilation);

                            break;
                        case PCode.Prim:
                            #region Decode block and update Prim
                            // State
                            prim.Data.State = (uint)block.Data[i++];
                            // CRC
                            i += 4;
                            // Material
                            prim.Data.Material = (LLObject.MaterialType)block.Data[i++];
                            // Click action
                            prim.ClickAction = (ClickAction)block.Data[i++];
                            // Scale
                            prim.Scale = new LLVector3(block.Data, i);
                            i += 12;
                            // Position
                            prim.Position = new LLVector3(block.Data, i);
                            i += 12;
                            // Rotation
                            prim.Rotation = new LLQuaternion(block.Data, i, true);
                            i += 12;
                            // Compressed flags
                            CompressedFlags flags = (CompressedFlags)Helpers.BytesToUIntBig(block.Data, i);
                            i += 4;

                            prim.OwnerID = new LLUUID(block.Data, i);
                            i += 16;
			    

                            // Angular velocity
                            if ((flags & CompressedFlags.HasAngularVelocity) != 0)
                            {
                                prim.AngularVelocity = new LLVector3(block.Data, i);
                                i += 12;
                            }

                            // Parent ID
                            if ((flags & CompressedFlags.HasParent) != 0)
                            {
                                prim.ParentID = (uint)(block.Data[i++] + (block.Data[i++] << 8) +
                                (block.Data[i++] << 16) + (block.Data[i++] << 24));
                            }
                            else
                            {
                                prim.ParentID = 0;
                            }

                            // Tree data
                            if ((flags & CompressedFlags.Tree) != 0)
                            {
                                prim.GenericData = new byte[1];
                                prim.GenericData[0] = block.Data[i++];
                            }
                            // Scratch pad
                            else if ((flags & CompressedFlags.ScratchPad) != 0)
                            {
                                int size = block.Data[i++];
                                prim.GenericData = new byte[size];
                                Buffer.BlockCopy(block.Data, i, prim.GenericData, 0, size);
                                i += size;
                            }

                            // Floating text
                            if ((flags & CompressedFlags.HasText) != 0)
                            {
                                string text = String.Empty;
                                while (block.Data[i] != 0)
                                {
                                    text += (char)block.Data[i];
                                    i++;
                                }
                                i++;

                                // Floating text
                                prim.Text = text;

                                // Text color
                                prim.TextColor = new LLColor(block.Data, i);
                                i += 4;
                            }
                            else
                            {
                                prim.Text = String.Empty;
                            }

                            // Media URL
                            if ((flags & CompressedFlags.MediaURL) != 0)
                            {
                                string text = String.Empty;
                                while (block.Data[i] != 0)
                                {
                                    text += (char)block.Data[i];
                                    i++;
                                }
                                i++;

                                prim.MediaURL = text;
                            }

                            // Particle system
                            if ((flags & CompressedFlags.HasParticles) != 0)
                            {
                                prim.ParticleSys = new Primitive.ParticleSystem(block.Data, i);
                                i += 86;
                            }

                            // Extra parameters
                            i += prim.SetExtraParamsFromBytes(block.Data, i);

                            //Sound data
                            if ((flags & CompressedFlags.HasSound) != 0)
                            {
                                prim.Sound = new LLUUID(block.Data, i);
                                i += 16;

                                if (!BitConverter.IsLittleEndian)
                                {
                                    Array.Reverse(block.Data, i, 4);
                                    Array.Reverse(block.Data, i + 5, 4);
                                }

                                prim.SoundGain = BitConverter.ToSingle(block.Data, i);
                                i += 4;
                                prim.SoundFlags = block.Data[i++];
                                prim.SoundRadius = BitConverter.ToSingle(block.Data, i);
                                i += 4;
                            }

                            // Name values
                            if ((flags & CompressedFlags.HasNameValues) != 0)
                            {
                                string text = String.Empty;
                                while (block.Data[i] != 0)
                                {
                                    text += (char)block.Data[i];
                                    i++;
                                }
                                i++;

                                // Parse the name values
                                if (text.Length > 0)
                                {
                                    string[] lines = text.Split('\n');
                                    prim.NameValues = new NameValue[lines.Length];

                                    for (int j = 0; j < lines.Length; j++)
                                    {
                                        if (!String.IsNullOrEmpty(lines[j]))
                                        {
                                            NameValue nv = new NameValue(lines[j]);
                                            prim.NameValues[j] = nv;
                                        }
                                    }
                                }
                            }

                            prim.Data.PathCurve = (LLObject.PathCurve)block.Data[i++];
                            ushort pathBegin = Helpers.BytesToUInt16(block.Data, i); i += 2;
                            prim.Data.PathBegin = LLObject.PathBeginFloat(pathBegin);
                            ushort pathEnd = Helpers.BytesToUInt16(block.Data, i); i += 2;
                            prim.Data.PathEnd = LLObject.PathEndFloat(pathEnd);
                            prim.Data.PathScaleX = LLObject.PathScaleFloat(block.Data[i++]);
                            prim.Data.PathScaleY = LLObject.PathScaleFloat(block.Data[i++]);
                            prim.Data.PathShearX = LLObject.PathShearFloat(block.Data[i++]);
                            prim.Data.PathShearY = LLObject.PathShearFloat(block.Data[i++]);
                            prim.Data.PathTwist = (int)block.Data[i++];
                            prim.Data.PathTwistBegin = (int)block.Data[i++];
                            prim.Data.PathRadiusOffset = LLObject.PathRadiusOffsetFloat((sbyte)block.Data[i++]);
                            prim.Data.PathTaperX = LLObject.PathTaperFloat((sbyte)block.Data[i++]);
                            prim.Data.PathTaperY = LLObject.PathTaperFloat((sbyte)block.Data[i++]);
                            prim.Data.PathRevolutions = LLObject.PathRevolutionsFloat(block.Data[i++]);
                            prim.Data.PathSkew = LLObject.PathSkewFloat((sbyte)block.Data[i++]);

                            prim.Data.ProfileCurve = (LLObject.ProfileCurve)block.Data[i++];
                            ushort profileBegin = Helpers.BytesToUInt16(block.Data, i); i += 2;
                            prim.Data.ProfileBegin = LLObject.ProfileBeginFloat(profileBegin);
                            ushort profileEnd = Helpers.BytesToUInt16(block.Data, i); i += 2;
                            prim.Data.ProfileEnd = LLObject.ProfileEndFloat(profileEnd);
                            ushort profileHollow = Helpers.BytesToUInt16(block.Data, i); i += 2;
                            prim.Data.ProfileHollow = LLObject.ProfileHollowFloat(profileHollow);

                            LLUUID test = new LLUUID("73818c3a-acc3-30b8-5060-0e6cf693cddf");

                            // TextureEntry
                            int textureEntryLength = (int)Helpers.BytesToUIntBig(block.Data, i);
                            i += 4;
                            prim.Textures = new LLObject.TextureEntry(block.Data, i, textureEntryLength);
                            i += textureEntryLength;

                            // Texture animation
                            if ((flags & CompressedFlags.TextureAnimation) != 0)
                            {
                                //int textureAnimLength = (int)Helpers.BytesToUIntBig(block.Data, i);
                                i += 4;
                                prim.TextureAnim = new Primitive.TextureAnimation(block.Data, i);
                            }

                            #endregion

                            #region Fire Events

                            // Fire the appropriate callback
                            // TODO: We should use a better check to see if this is actually an attachment
                            if ((flags & CompressedFlags.HasNameValues) != 0)
                                FireOnNewAttachment(simulator, prim, update.RegionData.RegionHandle, 
                                    update.RegionData.TimeDilation);
                            else if ((flags & CompressedFlags.Tree) != 0)
                                FireOnNewFoliage(simulator, prim, update.RegionData.RegionHandle, 
                                    update.RegionData.TimeDilation);
                            else
                                FireOnNewPrim(simulator, prim, update.RegionData.RegionHandle, 
                                    update.RegionData.TimeDilation);

                            #endregion

                            break;
                        default:
                            Client.DebugLog("Got an ObjectUpdateCompressed for PCode " + pcode.ToString() +
                                ", implement this!");
                            break;
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    Client.Log("Had a problem decoding an ObjectUpdateCompressed packet: " +
                        e.ToString(), Helpers.LogLevel.Warning);
                    Client.Log(block.ToString(), Helpers.LogLevel.Warning);
                }
            }
        }

        protected void CachedUpdateHandler(Packet packet, Simulator simulator)
        {
            if (Client.Settings.ALWAYS_REQUEST_OBJECTS)
            {
                ObjectUpdateCachedPacket update = (ObjectUpdateCachedPacket)packet;
                List<uint> ids = new List<uint>(update.ObjectData.Length);

                // No object caching implemented yet, so request updates for all of these objects
                for (int i = 0; i < update.ObjectData.Length; i++)
                {
                    ids.Add(update.ObjectData[i].ID);
                }

                RequestObjects(simulator, ids);
            }
        }

        protected void KillObjectHandler(Packet packet, Simulator simulator)
        {
            KillObjectPacket kill = (KillObjectPacket)packet;

            if (Client.Settings.OBJECT_TRACKING)
            {
                for (int i = 0; i < kill.ObjectData.Length; i++)
                {
                    uint localID = kill.ObjectData[i].ID;

                    if (simulator.Objects.Prims.ContainsKey(localID))
                    {
                        lock (simulator.Objects.Prims)
                            simulator.Objects.Prims.Remove(localID);
                    }
                    if (simulator.Objects.Avatars.ContainsKey(localID))
                    {
                        lock (simulator.Objects.Avatars)
                            simulator.Objects.Avatars.Remove(localID);
                    }

                    FireOnObjectKilled(simulator, localID);
                }
            }
            else
            {
                for (int i = 0; i < kill.ObjectData.Length; i++)
                    FireOnObjectKilled(simulator, kill.ObjectData[i].ID);
            }
        }

        protected void ObjectPropertiesHandler(Packet p, Simulator sim)
        {
            ObjectPropertiesPacket op = (ObjectPropertiesPacket)p;
            ObjectPropertiesPacket.ObjectDataBlock[] datablocks = op.ObjectData;

            for (int i = 0; i < datablocks.Length; ++i)
            {
                ObjectPropertiesPacket.ObjectDataBlock objectData = datablocks[i];
                LLObject.ObjectProperties props = new LLObject.ObjectProperties();

                props.AggregatePerms = objectData.AggregatePerms;
                props.AggregatePermTextures = objectData.AggregatePermTextures;
                props.AggregatePermTexturesOwner = objectData.AggregatePermTexturesOwner;
                props.Permissions = new Permissions(objectData.BaseMask, objectData.EveryoneMask, objectData.GroupMask,
                    objectData.NextOwnerMask, objectData.OwnerMask);
                props.Category = objectData.Category;
                props.CreationDate = objectData.CreationDate;
                props.CreatorID = objectData.CreatorID;
                props.Description = Helpers.FieldToUTF8String(objectData.Description);
                props.FolderID = objectData.FolderID;
                props.FromTaskID = objectData.FromTaskID;
                props.GroupID = objectData.GroupID;
                props.InventorySerial = objectData.InventorySerial;
                props.ItemID = objectData.ItemID;
                props.LastOwnerID = objectData.LastOwnerID;
                props.Name = Helpers.FieldToUTF8String(objectData.Name);
                props.ObjectID = objectData.ObjectID;
                props.OwnerID = objectData.OwnerID;
                props.OwnershipCost = objectData.OwnershipCost;
                props.SalePrice = objectData.SalePrice;
                props.SaleType = objectData.SaleType;
                props.SitName = Helpers.FieldToUTF8String(objectData.SitName);
                props.TouchName = Helpers.FieldToUTF8String(objectData.TouchName);

                int numTextures = objectData.TextureID.Length / 16;
                props.TextureIDs = new LLUUID[numTextures];
                for (int j = 0; j < numTextures; ++j)
                    props.TextureIDs[j] = new LLUUID(objectData.TextureID, j * 16);

                Primitive findPrim = sim.Objects.Find(
                    delegate(Primitive prim) { return prim.ID == props.ObjectID; });

                if (findPrim != null)
                {
                    lock (sim.Objects.Prims)
                    {
                        if (sim.Objects.Prims.ContainsKey(findPrim.LocalID))
                            sim.Objects.Prims[findPrim.LocalID].Properties = props;
                    }
                }

                FireOnObjectProperties(sim, props);
            }
        }

        protected void ObjectPropertiesFamilyHandler(Packet p, Simulator sim)
        {
            ObjectPropertiesFamilyPacket op = (ObjectPropertiesFamilyPacket)p;
            LLObject.ObjectPropertiesFamily props = new LLObject.ObjectPropertiesFamily();

            props.RequestFlags = (LLObject.ObjectPropertiesFamily.RequestFlagsType)op.ObjectData.RequestFlags;
            props.Category = op.ObjectData.Category;
            props.Description = Helpers.FieldToUTF8String(op.ObjectData.Description);
            props.GroupID = op.ObjectData.GroupID;
            props.LastOwnerID = op.ObjectData.LastOwnerID;
            props.Name = Helpers.FieldToUTF8String(op.ObjectData.Name);
            props.ObjectID = op.ObjectData.ObjectID;
            props.OwnerID = op.ObjectData.OwnerID;
            props.OwnershipCost = op.ObjectData.OwnershipCost;
            props.SalePrice = op.ObjectData.SalePrice;
            props.SaleType = op.ObjectData.SaleType;
            props.Permissions.BaseMask = (PermissionMask)op.ObjectData.BaseMask;
            props.Permissions.EveryoneMask = (PermissionMask)op.ObjectData.EveryoneMask;
            props.Permissions.GroupMask = (PermissionMask)op.ObjectData.GroupMask;
            props.Permissions.NextOwnerMask = (PermissionMask)op.ObjectData.NextOwnerMask;
            props.Permissions.OwnerMask = (PermissionMask)op.ObjectData.OwnerMask;

            FireOnObjectPropertiesFamily(sim, props);
        }

        #endregion Packet Handlers

        #region Utility Functions

        /// <summary>
        /// Setup the ObjectData parameters for a basic wooden cube prim
        /// </summary>
        /// <returns>ObjectData struct representing a basic wooden cube prim</returns>
        public static LLObject.ObjectData BuildCube()
        {
            LLObject.ObjectData prim = new LLObject.ObjectData();

            prim.PCode = ObjectManager.PCode.Prim;
            prim.Material = LLObject.MaterialType.Wood;
            prim.ProfileCurve = LLObject.ProfileCurve.ProfileSquare;
            prim.PathCurve = LLObject.PathCurve.Line;
            prim.ProfileEnd = 1.0f;
            prim.PathEnd = 1.0f;
            prim.PathRevolutions = 1.0f;

            return prim;
        }

        protected void SetAvatarSittingOn(Simulator sim, Avatar av, uint localid, uint oldSeatID)
        {
            if (av.LocalID == Client.Self.localID) Client.Self.sittingOn = localid;
            av.sittingOn = localid;
                        

            if (OnAvatarSitChanged != null && oldSeatID != localid)
            {
                try { OnAvatarSitChanged(sim, av, localid, oldSeatID); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

		protected void UpdateDilation(Simulator s, uint dilation)
		{
            s.Stats.Dilation = (float)dilation / 65535.0f;
        }

        #endregion Utility Functions

        #region Event Notification

        protected void FireOnObjectProperties(Simulator sim, LLObject.ObjectProperties props)
        {
            if (OnObjectProperties != null)
            {
                try { OnObjectProperties(sim, props); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        protected void FireOnObjectPropertiesFamily(Simulator sim, LLObject.ObjectPropertiesFamily props)
        {
            if (OnObjectPropertiesFamily != null)
            {
                try { OnObjectPropertiesFamily(sim, props); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        protected void FireOnObjectKilled(Simulator simulator, uint localid)
        {
            if (OnObjectKilled != null)
            {
                try { OnObjectKilled(simulator, localid); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        protected void FireOnNewPrim(Simulator simulator, Primitive prim, ulong RegionHandle, ushort TimeDilation)
        {
            if (OnNewPrim != null)
            {
                try { OnNewPrim(simulator, prim, RegionHandle, TimeDilation); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        protected void FireOnNewFoliage(Simulator simulator, Primitive prim, ulong RegionHandle, ushort TimeDilation)
        {
            if (OnNewFoliage != null)
            {
                try { OnNewFoliage(simulator, prim, RegionHandle, TimeDilation); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        protected void FireOnNewAttachment(Simulator simulator, Primitive prim, ulong RegionHandle, ushort TimeDilation)
        {
            if (OnNewAttachment != null)
            {
                try { OnNewAttachment(simulator, prim, RegionHandle, TimeDilation); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        protected void FireOnNewAvatar(Simulator simulator, Avatar avatar, ulong RegionHandle, ushort TimeDilation)
        {
            if (OnNewAvatar != null)
            {
                try { OnNewAvatar(simulator, avatar, RegionHandle, TimeDilation); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        protected void FireOnObjectUpdated(Simulator simulator, ObjectUpdate update, ulong RegionHandle, ushort TimeDilation)
        {
            if (OnObjectUpdated != null)
            {
                try { OnObjectUpdated(simulator, update, RegionHandle, TimeDilation); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        #endregion

        #region Object Tracking Link

        protected Primitive GetPrimitive(Simulator simulator, uint localID, LLUUID fullID)
        {
            if (Client.Settings.OBJECT_TRACKING)
            {
                Primitive prim;

                if (simulator.Objects.Prims.TryGetValue(localID, out prim))
                {
                    return prim;
                }
                else
                {
                    prim = new Primitive();
                    prim.LocalID = localID;
                    prim.ID = fullID;
                    lock (simulator.Objects.Prims)
                        simulator.Objects.Prims[localID] = prim;

                    return prim;
                }
            }
            else
            {
                return new Primitive();
            }
        }

        protected Avatar GetAvatar(Simulator simulator, uint localID, LLUUID fullID)
        {
            if (Client.Settings.OBJECT_TRACKING)
            {
                Avatar avatar;

                if (simulator.Objects.Avatars.TryGetValue(localID, out avatar))
                {
                    return avatar;
                }
                else
                {
                    avatar = new Avatar();
                    avatar.LocalID = localID;
                    avatar.ID = fullID;
                    lock (simulator.Objects.Avatars)
                        simulator.Objects.Avatars[localID] = avatar;

                    return avatar;
                }
            }
            else
            {
                return new Avatar();
            }
        }

        #endregion Object Tracking Link

        protected void InterpolationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Client.Network.Connected)
            {
                int interval = Environment.TickCount - Client.Self.lastInterpolation;
                float seconds = (float)interval / 1000f;

                // Iterate through all of the simulators
                lock (Client.Network.Simulators)
                {
                    for (int i = 0; i < Client.Network.Simulators.Count; i++)
                    {
                        float adjSeconds = seconds * Client.Network.Simulators[i].Stats.Dilation;

                        // Iterate through all of this sims avatars
                        lock (Client.Network.Simulators[i].Objects.Avatars)
                        {
                            foreach (Avatar avatar in Client.Network.Simulators[i].Objects.Avatars.Values)
                            {
                                #region Linear Motion
                                // Only do movement interpolation (extrapolation) when there is a non-zero velocity but 
                                // no acceleration
                                if (avatar.Acceleration != LLVector3.Zero && avatar.Velocity == LLVector3.Zero)
                                {
                                    avatar.Position += (avatar.Velocity + (0.5f * (adjSeconds - HAVOK_TIMESTEP)) *
                                        avatar.Acceleration) * adjSeconds;
                                    avatar.Velocity = avatar.Velocity + avatar.Acceleration * adjSeconds;
                                }
                                #endregion Linear Motion
                            }
                        }

                        // Iterate through all of this sims primitives
                        lock (Client.Network.Simulators[i].Objects.Prims)
                        {
                            foreach (Primitive prim in Client.Network.Simulators[i].Objects.Prims.Values)
                            {
                                if (prim.Joint == Primitive.JointType.Invalid)
                                {
                                    #region Angular Velocity
                                    LLVector3 angVel = prim.AngularVelocity;
                                    float omega = LLVector3.MagSquared(angVel);

                                    if (omega > 0.00001f)
                                    {
                                        omega = (float)Math.Sqrt(omega);
                                        float angle = omega * adjSeconds;
                                        angVel *= 1.0f / omega;
                                        LLQuaternion dQ = new LLQuaternion(angle, angVel);

                                        prim.Rotation *= dQ;
                                    }
                                    #endregion Angular Velocity

                                    #region Linear Motion
                                    // Only do movement interpolation (extrapolation) when there is a non-zero velocity but 
                                    // no acceleration
                                    if (prim.Acceleration != LLVector3.Zero && prim.Velocity == LLVector3.Zero)
                                    {
                                        prim.Position += (prim.Velocity + (0.5f * (adjSeconds - HAVOK_TIMESTEP)) *
                                        prim.Acceleration) * adjSeconds;
                                        prim.Velocity = prim.Velocity + prim.Acceleration * adjSeconds;
                                    }
                                    #endregion Linear Motion
                                }
                                else if (prim.Joint == Primitive.JointType.Hinge)
                                {
                                    //FIXME: Hinge movement extrapolation
                                }
                                else if (prim.Joint == Primitive.JointType.Point)
                                {
                                    //FIXME: Point movement extrapolation
                                }
                                else
                                {
                                    Client.Log("Unhandled joint type " + prim.Joint, Helpers.LogLevel.Warning);
                                }
                            }
                        }
                    }
                }

                // Make sure the last interpolated time is always updated
                Client.Self.lastInterpolation = Environment.TickCount;
            }
        }
    }
}
