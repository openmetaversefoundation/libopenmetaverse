/*
 * Copyright (c) 2006-2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
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
using System.Threading;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    #region Enums

    /// <summary>
    /// Identifier code for object types
    /// </summary>
    public enum PCode : byte
    {
        /// <summary>None</summary>
        None = 0,
        /// <summary>A Primitive</summary>
        Prim = 9,
        /// <summary>A Avatar</summary>
        Avatar = 47,
        /// <summary>Linden grass</summary>
        Grass = 95,
        /// <summary>Linden tree</summary>
        NewTree = 111,
        /// <summary>A primitive that acts as the source for a particle stream</summary>
        ParticleSystem = 143,
        /// <summary>A Linden tree</summary>
        Tree = 255
    }

    /// <summary>
    /// Attachment points for objects on avatar bodies
    /// </summary>
    /// <remarks>
    /// Both InventoryObject and InventoryAttachment types can be attached
    ///</remarks>
    public enum AttachmentPoint : byte
    {
        /// <summary>Right hand if object was not previously attached</summary>
        Default = 0,
        /// <summary>Chest</summary>
        Chest = 1,
        /// <summary>Skull</summary>
        Skull,
        /// <summary>Left shoulder</summary>
        LeftShoulder,
        /// <summary>Right shoulder</summary>
        RightShoulder,
        /// <summary>Left hand</summary>
        LeftHand,
        /// <summary>Right hand</summary>
        RightHand,
        /// <summary>Left foot</summary>
        LeftFoot,
        /// <summary>Right foot</summary>
        RightFoot,
        /// <summary>Spine</summary>
        Spine,
        /// <summary>Pelvis</summary>
        Pelvis,
        /// <summary>Mouth</summary>
        Mouth,
        /// <summary>Chin</summary>
        Chin,
        /// <summary>Left ear</summary>
        LeftEar,
        /// <summary>Right ear</summary>
        RightEar,
        /// <summary>Left eyeball</summary>
        LeftEyeball,
        /// <summary>Right eyeball</summary>
        RightEyeball,
        /// <summary>Nose</summary>
        Nose,
        /// <summary>Right upper arm</summary>
        RightUpperArm,
        /// <summary>Right forearm</summary>
        RightForearm,
        /// <summary>Left upper arm</summary>
        LeftUpperArm,
        /// <summary>Left forearm</summary>
        LeftForearm,
        /// <summary>Right hip</summary>
        RightHip,
        /// <summary>Right upper leg</summary>
        RightUpperLeg,
        /// <summary>Right lower leg</summary>
        RightLowerLeg,
        /// <summary>Left hip</summary>
        LeftHip,
        /// <summary>Left upper leg</summary>
        LeftUpperLeg,
        /// <summary>Left lower leg</summary>
        LeftLowerLeg,
        /// <summary>Stomach</summary>
        Stomach,
        /// <summary>Left pectoral</summary>
        LeftPec,
        /// <summary>Right pectoral</summary>
        RightPec,
        /// <summary>HUD Center position 2</summary>
        HUDCenter2,
        /// <summary>HUD Top-right</summary>
        HUDTopRight,
        /// <summary>HUD Top</summary>
        HUDTop,
        /// <summary>HUD Top-left</summary>
        HUDTopLeft,
        /// <summary>HUD Center</summary>
        HUDCenter,
        /// <summary>HUD Bottom-left</summary>
        HUDBottomLeft,
        /// <summary>HUD Bottom</summary>
        HUDBottom,
        /// <summary>HUD Bottom-right</summary>
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
    /// Tree foliage types
    /// </summary>
    public enum Tree : byte
    {
        /// <summary>Pine1 tree</summary>
        Pine1 = 0,
        /// <summary>Oak tree</summary>
        Oak,
        /// <summary>Tropical Bush1</summary>
        TropicalBush1,
        /// <summary>Palm1 tree</summary>
        Palm1,
        /// <summary>Dogwood tree</summary>
        Dogwood,
        /// <summary>Tropical Bush2</summary>
        TropicalBush2,
        /// <summary>Palm2 tree</summary>
        Palm2,
        /// <summary>Cypress1 tree</summary>
        Cypress1,
        /// <summary>Cypress2 tree</summary>
        Cypress2,
        /// <summary>Pine2 tree</summary>
        Pine2,
        /// <summary>Plumeria</summary>
        Plumeria,
        /// <summary>Winter pinetree1</summary>
        WinterPine1,
        /// <summary>Winter Aspen tree</summary>
        WinterAspen,
        /// <summary>Winter pinetree2</summary>
        WinterPine2,
        /// <summary>Eucalyptus tree</summary>
        Eucalyptus,
        /// <summary>Fern</summary>
        Fern,
        /// <summary>Eelgrass</summary>
        Eelgrass,
        /// <summary>Sea Sword</summary>
        SeaSword,
        /// <summary>Kelp1 plant</summary>
        Kelp1,
        /// <summary>Beach grass</summary>
        BeachGrass1,
        /// <summary>Kelp2 plant</summary>
        Kelp2
    }

    /// <summary>
    /// Grass foliage types
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
    /// Action associated with clicking on an object
    /// </summary>
    public enum ClickAction : byte
    {
        /// <summary>Touch object</summary>
        Touch = 0,
        /// <summary>Sit on object</summary>
        Sit = 1,
        /// <summary>Purchase object or contents</summary>
        Buy = 2,
        /// <summary>Pay the object</summary>
        Pay = 3,
        /// <summary>Open task inventory</summary>
        OpenTask = 4,
        /// <summary>Play parcel media</summary>
        PlayMedia = 5,
        /// <summary>Open parcel media</summary>
        OpenMedia = 6
    }

    /// <summary>
    /// Specific Flags for MultipleObjectUpdate requests
    /// </summary>
    [Flags]
    public enum UpdateType : uint
    {
        /// <summary>None</summary>
        None = 0x00,
        /// <summary>Change Position of prims</summary>
        Position = 0x01,
        /// <summary>Change Rotation of prims</summary>
        Rotation = 0x02,
        /// <summary>Change Size of Prims</summary>
        Scale = 0x04,
        /// <summary>Perform operation on link set</summary>
        Linked = 0x08,
        /// <summary>Scale prims uniformly, same as selecing ctrl+shift in viewer</summary>
        Uniform = 0x10
    }

    #endregion Enums

    #region Structs

    /// <summary>
    /// Contains the variables sent in an object update packet for objects. 
    /// Used to track position and movement of prims and avatars
    /// </summary>
    public struct ObjectUpdate
    {
        /// <summary></summary>
        public bool Avatar;
        /// <summary></summary>
        public Vector4 CollisionPlane;
        /// <summary></summary>
        public byte State;
        /// <summary></summary>
        public uint LocalID;
        /// <summary></summary>
        public Vector3 Position;
        /// <summary></summary>
        public Vector3 Velocity;
        /// <summary></summary>
        public Vector3 Acceleration;
        /// <summary></summary>
        public Quaternion Rotation;
        /// <summary></summary>
        public Vector3 AngularVelocity;
        /// <summary></summary>
        public LLObject.TextureEntry Textures;
    }

    #endregion Structs

    /// <summary>
    /// Handles all network traffic related to prims and avatar positions and 
    /// movement.
    /// </summary>
    public class ObjectManager
    {
        public const float HAVOK_TIMESTEP = 1.0f / 45.0f;

        #region Delegates

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
        /// Called whenever an object disappears
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
        /// <param name="avatar"></param>
        /// <param name="sittingOn">The local ID of the object that is being sat
        /// <param name="oldSeat"></param>
        /// on. If this is zero the avatar is not sitting on an object</param>
        public delegate void AvatarSitChanged(Simulator simulator, Avatar avatar, uint sittingOn, uint oldSeat);
		
        #endregion Delegates

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

        /// <summary>Reference to the GridClient object</summary>
        protected GridClient Client;
        /// <summary>Does periodic dead reckoning calculation to convert
        /// velocity and acceleration to new positions for objects</summary>
        private Timer InterpolationTimer;

        /// <summary>
        /// Instantiates a new ObjectManager class
        /// </summary>
        /// <param name="client">A reference to the client</param>
        internal ObjectManager(GridClient client)
        {
            Client = client;
            RegisterCallbacks();
        }

        /// <summary>
        /// Instantiates a new ObjectManager class
        /// </summary>
        /// <param name="client">A reference to the client</param>
        /// <param name="registerCallbacks">If false, the ObjectManager won't
        /// register any packet callbacks and won't decode incoming object
        /// packets</param>
        protected ObjectManager(GridClient client, bool registerCallbacks)
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
            InterpolationTimer = new Timer(new TimerCallback(InterpolationTimer_Elapsed), null, Settings.INTERPOLATION_INTERVAL,
                Settings.INTERPOLATION_INTERVAL);
        }

        #region Action Methods

        /// <summary>
        /// Request object information from the sim, primarily used for stale 
        /// or missing cache entries
        /// </summary>
        /// <param name="simulator">The simulator containing the object you're 
        /// looking for</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
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
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to request</param>
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
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the objects reside</param>        
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="saleType">Whether the original, a copy, or the object
        /// contents are on sale. This is used for verification, if the this
        /// sale type is not valid for the object the purchase will fail</param>
        /// <param name="price">Price of the object. This is used for 
        /// verification, if it does not match the actual price the purchase
        /// will fail</param>
        /// <param name="groupID">Group ID that will be associated with the new
        /// purchase</param>
        /// <param name="categoryID">Inventory folder UUID where the object or objects 
        /// purchased should be placed</param>
        /// <example>
        /// <code>
        /// BuyObject(Client.Network.CurrentSim, 500, SaleType.Copy, 
        /// 100, UUID.Zero, Client.Self.InventoryRootFolderUUID);
        /// </code> 
        ///</example>
        public void BuyObject(Simulator simulator, uint localID, SaleType saleType, int price, UUID groupID, 
            UUID categoryID)
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
        /// Select a single object. This will trigger the simulator to send us back 
        /// an ObjectProperties packet so we can get the full information for
        /// this object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
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
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to select</param>
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

        /// <summary>
        /// Deselect an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
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
        /// Deselect multiple objects.
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="libsecondlife.Simulator"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to select</param>
        public void DeselectObjects(Simulator simulator, uint[] localIDs)
        {
            ObjectDeselectPacket deselect = new ObjectDeselectPacket();

            deselect.AgentData.AgentID = Client.Self.AgentID;
            deselect.AgentData.SessionID = Client.Self.SessionID;

            deselect.ObjectData = new ObjectDeselectPacket.ObjectDataBlock[localIDs.Length];

            for (int i = 0; i < localIDs.Length; i++)
            {
                deselect.ObjectData[i] = new ObjectDeselectPacket.ObjectDataBlock();
                deselect.ObjectData[i].ObjectLocalID = localIDs[i];
            }

            Client.Network.SendPacket(deselect, simulator);
        }

        /// <summary>
        /// Perform a click action on an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        public void ClickObject(Simulator simulator, uint localID)
        {
            ObjectGrabPacket grab = new ObjectGrabPacket();
            grab.AgentData.AgentID = Client.Self.AgentID;
            grab.AgentData.SessionID = Client.Self.SessionID;
            grab.ObjectData.GrabOffset = Vector3.Zero;
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
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object to place the object in</param>
        /// <param name="prim">Data describing the prim object to rez</param>
        /// <param name="groupID">Group ID that this prim will be set to, or UUID.Zero if you
        /// do not want the object to be associated with a specific group</param>
        /// <param name="position">An approximation of the position at which to rez the prim</param>
        /// <param name="scale">Scale vector to size this prim</param>
        /// <param name="rotation">Rotation quaternion to rotate this prim</param>
        /// <remarks>Due to the way client prim rezzing is done on the server,
        /// the requested position for an object is only close to where the prim
        /// actually ends up. If you desire exact placement you'll need to 
        /// follow up by moving the object after it has been created. This
        /// function will not set textures, light and flexible data, or other 
        /// extended primitive properties</remarks>
        public void AddPrim(Simulator simulator, LLObject.ObjectData prim, UUID groupID, Vector3 position, 
            Vector3 scale, Quaternion rotation)
        {
            ObjectAddPacket packet = new ObjectAddPacket();

            packet.AgentData.AgentID = Client.Self.AgentID;
            packet.AgentData.SessionID = Client.Self.SessionID;
            packet.AgentData.GroupID = groupID;

            packet.ObjectData.State = prim.State;
            packet.ObjectData.AddFlags = (uint)LLObject.ObjectFlags.CreateSelected;
            packet.ObjectData.PCode = (byte)PCode.Prim;

            packet.ObjectData.Material = (byte)prim.Material;
            packet.ObjectData.Scale = scale;
            packet.ObjectData.Rotation = rotation;

            packet.ObjectData.PathCurve = (byte)prim.PathCurve;
            packet.ObjectData.PathBegin = LLObject.PackBeginCut(prim.PathBegin);
            packet.ObjectData.PathEnd = LLObject.PackEndCut(prim.PathEnd);
            packet.ObjectData.PathRadiusOffset = LLObject.PackPathTwist(prim.PathRadiusOffset);
            packet.ObjectData.PathRevolutions = LLObject.PackPathRevolutions(prim.PathRevolutions);
            packet.ObjectData.PathScaleX = LLObject.PackPathScale(prim.PathScaleX);
            packet.ObjectData.PathScaleY = LLObject.PackPathScale(prim.PathScaleY);
            packet.ObjectData.PathShearX = (byte)LLObject.PackPathShear(prim.PathShearX);
            packet.ObjectData.PathShearY = (byte)LLObject.PackPathShear(prim.PathShearY);
            packet.ObjectData.PathSkew = LLObject.PackPathTwist(prim.PathSkew);
            packet.ObjectData.PathTaperX = LLObject.PackPathTaper(prim.PathTaperX);
            packet.ObjectData.PathTaperY = LLObject.PackPathTaper(prim.PathTaperY);
            packet.ObjectData.PathTwist = LLObject.PackPathTwist(prim.PathTwist);
            packet.ObjectData.PathTwistBegin = LLObject.PackPathTwist(prim.PathTwistBegin);

            packet.ObjectData.ProfileCurve = prim.profileCurve;
            packet.ObjectData.ProfileBegin = LLObject.PackBeginCut(prim.ProfileBegin);
            packet.ObjectData.ProfileEnd = LLObject.PackEndCut(prim.ProfileEnd);
            packet.ObjectData.ProfileHollow = LLObject.PackProfileHollow(prim.ProfileHollow);

            packet.ObjectData.RayStart = position;
            packet.ObjectData.RayEnd = position;
            packet.ObjectData.RayEndIsIntersection = 0;
            packet.ObjectData.RayTargetID = UUID.Zero;
            packet.ObjectData.BypassRaycast = 1;

            Client.Network.SendPacket(packet, simulator);
        }

        /// <summary>
        /// Rez a Linden tree
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="scale">The size of the tree</param>
        /// <param name="rotation">The rotation of the tree</param>
        /// <param name="position">The position of the tree</param>
        /// <param name="treeType">The Type of tree</param>
        /// <param name="groupOwner">The <seealso cref="UUID"/> of the group to set the tree to, 
        /// or UUID.Zero if no group is to be set</param>
        /// <param name="newTree">true to use the "new" Linden trees, false to use the old</param>
        public void AddTree(Simulator simulator, Vector3 scale, Quaternion rotation, Vector3 position, 
            Tree treeType, UUID groupOwner, bool newTree)
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
            add.ObjectData.RayTargetID = UUID.Zero;
            add.ObjectData.Rotation = rotation;
            add.ObjectData.Scale = scale;
            add.ObjectData.State = (byte)treeType;

            Client.Network.SendPacket(add, simulator);
        }

        /// <summary>
        /// Rez grass and ground cover
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="scale">The size of the grass</param>
        /// <param name="rotation">The rotation of the grass</param>
        /// <param name="position">The position of the grass</param>
        /// <param name="grassType">The type of grass from the <seealso cref="Grass"/> enum</param>
        /// <param name="groupOwner">The <seealso cref="UUID"/> of the group to set the tree to, 
        /// or UUID.Zero if no group is to be set</param>
        public void AddGrass(Simulator simulator, Vector3 scale, Quaternion rotation, Vector3 position,
            Grass grassType, UUID groupOwner)
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
            add.ObjectData.RayTargetID = UUID.Zero;
            add.ObjectData.Rotation = rotation;
            add.ObjectData.Scale = scale;
            add.ObjectData.State = (byte)grassType;

            Client.Network.SendPacket(add, simulator);
        }

        /// <summary>
        /// Set the textures to apply to the faces of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="textures">The texture data to apply</param>
        public void SetTextures(Simulator simulator, uint localID, LLObject.TextureEntry textures)
        {
            SetTextures(simulator, localID, textures, String.Empty);
        }

        /// <summary>
        /// Set the textures to apply to the faces of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="textures">The texture data to apply</param>
        /// <param name="mediaUrl">A media URL (not used)</param>
        public void SetTextures(Simulator simulator, uint localID, LLObject.TextureEntry textures, string mediaUrl)
        {
            ObjectImagePacket image = new ObjectImagePacket();

            image.AgentData.AgentID = Client.Self.AgentID;
            image.AgentData.SessionID = Client.Self.SessionID;
            image.ObjectData = new ObjectImagePacket.ObjectDataBlock[1];
            image.ObjectData[0] = new ObjectImagePacket.ObjectDataBlock();
            image.ObjectData[0].ObjectLocalID = localID;
            image.ObjectData[0].TextureEntry = textures.ToBytes();
            image.ObjectData[0].MediaURL = Utils.StringToBytes(mediaUrl);

            Client.Network.SendPacket(image, simulator);
        }

        /// <summary>
        /// Set the Light data on an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="light">A <seealso cref="Primitive.LightData"/> object containing the data to set</param>
        public void SetLight(Simulator simulator, uint localID, Primitive.LightData light)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();

            extra.AgentData.AgentID = Client.Self.AgentID;
            extra.AgentData.SessionID = Client.Self.SessionID;
            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)Primitive.ExtraParamType.Light;
            if (light.Intensity == 0.0f) 
            {
                // Disables the light if intensity is 0
                extra.ObjectData[0].ParamInUse = false;
            } 
            else 
            {
                extra.ObjectData[0].ParamInUse = true;
            }
            extra.ObjectData[0].ParamData = light.GetBytes();
            extra.ObjectData[0].ParamSize = (uint)extra.ObjectData[0].ParamData.Length;

            Client.Network.SendPacket(extra, simulator);
        }

        /// <summary>
        /// Set the flexible data on an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="flexible">A <seealso cref="Primitive.FlexibleData"/> object containing the data to set</param>
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

        /// <summary>
        /// Set the sculptie texture and data on an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="sculpt">A <seealso cref="Primitive.SculptData"/> object containing the data to set</param>
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

            shape.ObjectData = new OpenMetaverse.Packets.ObjectShapePacket.ObjectDataBlock[1];
            shape.ObjectData[0] = new OpenMetaverse.Packets.ObjectShapePacket.ObjectDataBlock();
            shape.ObjectData[0].ObjectLocalID = localID;
            shape.ObjectData[0].PathScaleX = 100;
            shape.ObjectData[0].PathScaleY = 150;
            shape.ObjectData[0].PathCurve = 32;

            Client.Network.SendPacket(shape, simulator);
        }

        /// <summary>
        /// Set additional primitive parameters on an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="type">The extra parameters to set</param>
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
        /// Link multiple prims into a linkset
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to link</param>
        /// <remarks>The last object in the array will be the root object of the linkset TODO: Is this true?</remarks>
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
        /// Change the rotation of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="rotation">The new rotation of the object</param>
        public void SetRotation(Simulator simulator, uint localID, Quaternion rotation)
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
        /// Set the name of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="name">A string containing the new name of the object</param>
        public void SetName(Simulator simulator, uint localID, string name)
        {
            SetNames(simulator, new uint[] { localID }, new string[] { name });
        }

        /// <summary>
        /// Set the name of multiple objects
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to change the name of</param>
        /// <param name="names">An array which contains the new names of the objects</param>
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
                namePacket.ObjectData[i].Name = Utils.StringToBytes(names[i]);
            }

            Client.Network.SendPacket(namePacket, simulator);
        }

        /// <summary>
        /// Set the description of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="description">A string containing the new description of the object</param>
        public void SetDescription(Simulator simulator, uint localID, string description)
        {
            SetDescriptions(simulator, new uint[] { localID }, new string[] { description });
        }

        /// <summary>
        /// Set the descriptions of multiple objects
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to change the description of</param>
        /// <param name="descriptions">An array which contains the new descriptions of the objects</param>
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
                descPacket.ObjectData[i].Description = Utils.StringToBytes(descriptions[i]);
            }

            Client.Network.SendPacket(descPacket, simulator);
        }

        /// <summary>
        /// Attach an object to this avatar
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="attachPoint">The point on the avatar the object will be attached</param>
        /// <param name="rotation">The rotation of the attached object</param>
        public void AttachObject(Simulator simulator, uint localID, AttachmentPoint attachPoint, Quaternion rotation)
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
        /// Detach an object from yourself
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> 
        /// object where the objects reside
        /// 
        /// This will always be the simulator the avatar is currently in
        /// </param>
        /// <param name="localIDs">An array which contains the IDs of the objects to detach</param>
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
        /// Change the position of an object, Will change position of entire linkset
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="position">The new position of the object</param>
        public void SetPosition(Simulator simulator, uint localID, Vector3 position)
        {
            UpdateObject(simulator, localID, position, UpdateType.Position | UpdateType.Linked);
        }

        /// <summary>
        /// Change the position of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="position">The new position of the object</param>
        /// <param name="childOnly">if true, will change position of (this) child prim only, not entire linkset</param>
        public void SetPosition(Simulator simulator, uint localID, Vector3 position, bool childOnly)
        {
            UpdateType type = UpdateType.Position;

            if (!childOnly)
                type |= UpdateType.Linked;

            UpdateObject(simulator, localID, position, type);
        }

        /// <summary>
        /// Change the Scale (size) of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="scale">The new scale of the object</param>
        /// <param name="childOnly">If true, will change scale of this prim only, not entire linkset</param>
        /// <param name="uniform">True to resize prims uniformly</param>
        public void SetScale(Simulator simulator, uint localID, Vector3 scale, bool childOnly, bool uniform)
        {
            UpdateType type = UpdateType.Scale;

            if (!childOnly)
                type |= UpdateType.Linked;

            if (uniform)
                type |= UpdateType.Uniform;

            UpdateObject(simulator, localID, scale, type);
        }

        /// <summary>
        /// Change the Rotation of an object that is either a child or a whole linkset
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="quat">The new scale of the object</param>
        /// <param name="childOnly">If true, will change rotation of this prim only, not entire linkset</param>
        public void SetRotation(Simulator simulator, uint localID, Quaternion quat, bool childOnly)
        {
            UpdateType type = UpdateType.Rotation;

            if (!childOnly)
                type |= UpdateType.Linked;

            MultipleObjectUpdatePacket multiObjectUpdate = new MultipleObjectUpdatePacket();
            multiObjectUpdate.AgentData.AgentID = Client.Self.AgentID;
            multiObjectUpdate.AgentData.SessionID = Client.Self.SessionID;

            multiObjectUpdate.ObjectData = new MultipleObjectUpdatePacket.ObjectDataBlock[1];

            multiObjectUpdate.ObjectData[0] = new MultipleObjectUpdatePacket.ObjectDataBlock();
            multiObjectUpdate.ObjectData[0].Type = (byte)type;
            multiObjectUpdate.ObjectData[0].ObjectLocalID = localID;
            multiObjectUpdate.ObjectData[0].Data = quat.GetBytes();

            Client.Network.SendPacket(multiObjectUpdate, simulator);
        }

        /// <summary>
        /// Send a Multiple Object Update packet to change the size, scale or rotation of a primitive
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="data">The new rotation, size, or position of the target object</param>
        /// <param name="type">The flags from the <seealso cref="UpdateType"/> Enum</param>
        public void UpdateObject(Simulator simulator, uint localID, Vector3 data, UpdateType type)
        {
            MultipleObjectUpdatePacket multiObjectUpdate = new MultipleObjectUpdatePacket();
            multiObjectUpdate.AgentData.AgentID = Client.Self.AgentID;
            multiObjectUpdate.AgentData.SessionID = Client.Self.SessionID;

            multiObjectUpdate.ObjectData = new MultipleObjectUpdatePacket.ObjectDataBlock[1];

            multiObjectUpdate.ObjectData[0] = new MultipleObjectUpdatePacket.ObjectDataBlock();
            multiObjectUpdate.ObjectData[0].Type = (byte)type;
            multiObjectUpdate.ObjectData[0].ObjectLocalID = localID;
            multiObjectUpdate.ObjectData[0].Data = data.GetBytes();

            Client.Network.SendPacket(multiObjectUpdate, simulator);
        }

        /// <summary>
        /// Deed an object (prim) to a group, Object must be shared with group which
        /// can be accomplished with SetPermissions()
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="groupOwner">The <seealso cref="UUID"/> of the group to deed the object to</param>
        public void DeedObject(Simulator simulator, uint localID, UUID groupOwner)
        {
            ObjectOwnerPacket objDeedPacket = new ObjectOwnerPacket();
            objDeedPacket.AgentData.AgentID = Client.Self.AgentID;
            objDeedPacket.AgentData.SessionID = Client.Self.SessionID;

            // Can only be use in God mode
            objDeedPacket.HeaderData.Override = false;
            objDeedPacket.HeaderData.OwnerID = UUID.Zero;
            objDeedPacket.HeaderData.GroupID = groupOwner;

            objDeedPacket.ObjectData = new ObjectOwnerPacket.ObjectDataBlock[1];
            objDeedPacket.ObjectData[0] = new ObjectOwnerPacket.ObjectDataBlock();
            
            objDeedPacket.ObjectData[0].ObjectLocalID = localID;
            
            Client.Network.SendPacket(objDeedPacket, simulator);
        }

        /// <summary>
        /// Deed multiple objects (prims) to a group, Objects must be shared with group which
        /// can be accomplished with SetPermissions()
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to deed</param>
        /// <param name="groupOwner">The <seealso cref="UUID"/> of the group to deed the object to</param>
        public void DeedObjects(Simulator simulator, List<uint> localIDs, UUID groupOwner)
        {
            ObjectOwnerPacket packet = new ObjectOwnerPacket();
            packet.AgentData.AgentID = Client.Self.AgentID;
            packet.AgentData.SessionID = Client.Self.SessionID;

            // Can only be use in God mode
            packet.HeaderData.Override = false;
            packet.HeaderData.OwnerID = UUID.Zero;
            packet.HeaderData.GroupID = groupOwner;

            packet.ObjectData = new ObjectOwnerPacket.ObjectDataBlock[localIDs.Count];

            for (int i = 0; i < localIDs.Count; i++)
            {
                packet.ObjectData[i] = new ObjectOwnerPacket.ObjectDataBlock();
                packet.ObjectData[i].ObjectLocalID = localIDs[i];
            }
            Client.Network.SendPacket(packet, simulator);
        }
            
        /// <summary>
        /// Set the permissions on multiple objects
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to set the permissions on</param>
        /// <param name="who">The new Who mask to set</param>
        /// <param name="permissions">The new Permissions mark to set</param>
        /// <param name="set">TODO: What does this do?</param>
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
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="objectID"></param>
        public void RequestObjectPropertiesFamily(Simulator simulator, UUID objectID)
        {
            RequestObjectPropertiesFamily(simulator, objectID, true);
        }

        /// <summary>
        /// Request additional properties for an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="objectID">Absolute UUID of the object</param>
        /// <param name="reliable">Whether to require server acknowledgement of this request</param>
        public void RequestObjectPropertiesFamily(Simulator simulator, UUID objectID, bool reliable)
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

                Vector4 collisionPlane = Vector4.Zero;
                Vector3 position;
                Vector3 velocity;
                Vector3 acceleration;
                Quaternion rotation;
                Vector3 angularVelocity;
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

                string nameValue = Utils.BytesToString(block.NameValue);
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
                data.profileCurve = block.ProfileCurve;
                data.PathBegin = LLObject.UnpackBeginCut(block.PathBegin);
                data.PathEnd = LLObject.UnpackEndCut(block.PathEnd);
                data.PathScaleX = LLObject.UnpackPathScale(block.PathScaleX);
                data.PathScaleY = LLObject.UnpackPathScale(block.PathScaleY);
                data.PathShearX = LLObject.UnpackPathShear((sbyte)block.PathShearX);
                data.PathShearY = LLObject.UnpackPathShear((sbyte)block.PathShearY);
                data.PathTwist = LLObject.UnpackPathTwist(block.PathTwist);
                data.PathTwistBegin = LLObject.UnpackPathTwist(block.PathTwistBegin);
                data.PathRadiusOffset = LLObject.UnpackPathTwist(block.PathRadiusOffset);
                data.PathTaperX = LLObject.UnpackPathTaper(block.PathTaperX);
                data.PathTaperY = LLObject.UnpackPathTaper(block.PathTaperY);
                data.PathRevolutions = LLObject.UnpackPathRevolutions(block.PathRevolutions);
                data.PathSkew = LLObject.UnpackPathTwist(block.PathSkew);
                data.ProfileBegin = LLObject.UnpackBeginCut(block.ProfileBegin);
                data.ProfileEnd = LLObject.UnpackEndCut(block.ProfileEnd);
                data.ProfileHollow = LLObject.UnpackProfileHollow(block.ProfileHollow);
                data.PCode = pcode;
                #endregion

                #region Decode Additional packed parameters in ObjectData
                int pos = 0;
                switch (block.ObjectData.Length)
                {
                    case 76:
                        // Collision normal for avatar
                        collisionPlane = new Vector4(block.ObjectData, pos);
                        pos += 16;

                        goto case 60;
                    case 60:
                        // Position
                        position = new Vector3(block.ObjectData, pos);
                        pos += 12;
                        // Velocity
                        velocity = new Vector3(block.ObjectData, pos);
                        pos += 12;
                        // Acceleration
                        acceleration = new Vector3(block.ObjectData, pos);
                        pos += 12;
                        // Rotation (theta)
                        rotation = new Quaternion(block.ObjectData, pos, true);
                        pos += 12;
                        // Angular velocity (omega)
                        angularVelocity = new Vector3(block.ObjectData, pos);
                        pos += 12;

                        break;
                    case 48:
                        // Collision normal for avatar
                        collisionPlane = new Vector4(block.ObjectData, pos);
                        pos += 16;

                        goto case 32;
                    case 32:
                        // The data is an array of unsigned shorts

                        // Position
                        position = new Vector3(
                            Helpers.UInt16ToFloat(block.ObjectData, pos, -0.5f * 256.0f, 1.5f * 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 2, -0.5f * 256.0f, 1.5f * 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 3.0f * 256.0f));
                        pos += 6;
                        // Velocity
                        velocity = new Vector3(
                            Helpers.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;
                        // Acceleration
                        acceleration = new Vector3(
                            Helpers.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;
                        // Rotation (theta)
                        rotation = new Quaternion(
                            Helpers.UInt16ToFloat(block.ObjectData, pos, -1.0f, 1.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 2, -1.0f, 1.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 4, -1.0f, 1.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 6, -1.0f, 1.0f));
                        pos += 8;
                        // Angular velocity (omega)
                        angularVelocity = new Vector3(
                            Helpers.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Helpers.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;

                        break;
                    case 16:
                        // The data is an array of single bytes (8-bit numbers)

                        // Position
                        position = new Vector3(
                            Helpers.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Velocity
                        velocity = new Vector3(
                            Helpers.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Accleration
                        acceleration = new Vector3(
                            Helpers.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Rotation
                        rotation = new Quaternion(
                            Helpers.ByteToFloat(block.ObjectData, pos, -1.0f, 1.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 1, -1.0f, 1.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 2, -1.0f, 1.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 3, -1.0f, 1.0f));
                        pos += 4;
                        // Angular Velocity
                        angularVelocity = new Vector3(
                            Helpers.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Helpers.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;

                        break;
                    default:
                        Logger.Log("Got an ObjectUpdate block with ObjectUpdate field length of " +
                            block.ObjectData.Length, Helpers.LogLevel.Warning, Client);

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
                            Logger.Log("Got a ZlibCompressed ObjectUpdate, implement me!", 
                                Helpers.LogLevel.Warning, Client);
                            continue;
                        }

                        // Automatically request ObjectProperties for prim if it was rezzed selected.
                        if ((prim.Flags & LLObject.ObjectFlags.CreateSelected) == LLObject.ObjectFlags.CreateSelected)
                            SelectObject(simulator, prim.LocalID);

                        prim.NameValues = nameValues;
                        prim.LocalID = block.ID;
                        prim.ID = block.FullID;
                        prim.ParentID = block.ParentID;
					    prim.RegionHandle = update.RegionData.RegionHandle;
                        prim.Scale = block.Scale;
                        prim.ClickAction = (ClickAction)block.ClickAction;
                        prim.OwnerID = block.OwnerID;
                        prim.MediaURL = Utils.BytesToString(block.MediaURL);
                        prim.Text = Utils.BytesToString(block.Text);
                        prim.TextColor = new Color4(block.TextColor, 0, false, true);

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

                        // Set the current simulator for this avatar
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
                        Logger.DebugLog("Got an ObjectUpdate block with an unrecognized PCode " + pcode.ToString(), Client);
                        break;
                }
            }
        }

        protected void DecodeParticleUpdate(ObjectUpdatePacket.ObjectDataBlock block)
        {
            // TODO: Handle ParticleSystem ObjectUpdate blocks

            // float bounce_b
            // Vector4 scale_range
            // Vector4 alpha_range
            // Vector3 vel_offset
            // float dist_begin_fadeout
            // float dist_end_fadeout
            // UUID image_uuid
            // long flags
            // byte createme
            // Vector3 diff_eq_alpha
            // Vector3 diff_eq_scale
            // byte max_particles
            // byte initial_particles
            // float kill_plane_z
            // Vector3 kill_plane_normal
            // float bounce_plane_z
            // Vector3 bounce_plane_normal
            // float spawn_range
            // float spawn_frequency
            // float spawn_frequency_range
            // Vector3 spawn_direction
            // float spawn_direction_range
            // float spawn_velocity
            // float spawn_velocity_range
            // float speed_limit
            // float wind_weight
            // Vector3 current_gravity
            // float gravity_weight
            // float global_lifetime
            // float individual_lifetime
            // float individual_lifetime_range
            // float alpha_decay
            // float scale_decay
            // float distance_death
            // float damp_motion_factor
            // Vector3 wind_diffusion_factor
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
                        update.CollisionPlane = new Vector4(block.Data, pos);
                        pos += 16;
                    }
                    // Position
                    update.Position = new Vector3(block.Data, pos);
                    pos += 12;
                    // Velocity
                    update.Velocity = new Vector3(
                        Helpers.UInt16ToFloat(block.Data, pos, -128.0f, 128.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 2, -128.0f, 128.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 4, -128.0f, 128.0f));
                    pos += 6;
                    // Acceleration
                    update.Acceleration = new Vector3(
                        Helpers.UInt16ToFloat(block.Data, pos, -64.0f, 64.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 2, -64.0f, 64.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 4, -64.0f, 64.0f));
                    pos += 6;
                    // Rotation (theta)
                    update.Rotation = new Quaternion(
                        Helpers.UInt16ToFloat(block.Data, pos, -1.0f, 1.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 2, -1.0f, 1.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 4, -1.0f, 1.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 6, -1.0f, 1.0f));
                    pos += 8;
                    // Angular velocity
                    update.AngularVelocity = new Vector3(
                        Helpers.UInt16ToFloat(block.Data, pos, -64.0f, 64.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 2, -64.0f, 64.0f),
                        Helpers.UInt16ToFloat(block.Data, pos + 4, -64.0f, 64.0f));
                    pos += 6;

                    // Textures
                    // FIXME: Why are we ignoring the first four bytes here?
                    if (block.TextureEntry.Length != 0)
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
                    Logger.Log(e.Message, Helpers.LogLevel.Warning, Client, e);
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
                    UUID FullID = new UUID(block.Data, 0);
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
                            prim.Data.State = block.Data[i++];
                            // CRC
                            i += 4;
                            // Material
                            prim.Data.Material = (LLObject.MaterialType)block.Data[i++];
                            // Click action
                            prim.ClickAction = (ClickAction)block.Data[i++];
                            // Scale
                            prim.Scale = new Vector3(block.Data, i);
                            i += 12;
                            // Position
                            prim.Position = new Vector3(block.Data, i);
                            i += 12;
                            // Rotation
                            prim.Rotation = new Quaternion(block.Data, i, true);
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
                            prim.Data.State = block.Data[i++];
                            // CRC
                            i += 4;
                            // Material
                            prim.Data.Material = (LLObject.MaterialType)block.Data[i++];
                            // Click action
                            prim.ClickAction = (ClickAction)block.Data[i++];
                            // Scale
                            prim.Scale = new Vector3(block.Data, i);
                            i += 12;
                            // Position
                            prim.Position = new Vector3(block.Data, i);
                            i += 12;
                            // Rotation
                            prim.Rotation = new Quaternion(block.Data, i, true);
                            i += 12;
                            // Compressed flags
                            CompressedFlags flags = (CompressedFlags)Helpers.BytesToUIntBig(block.Data, i);
                            i += 4;

                            prim.OwnerID = new UUID(block.Data, i);
                            i += 16;
			    

                            // Angular velocity
                            if ((flags & CompressedFlags.HasAngularVelocity) != 0)
                            {
                                prim.AngularVelocity = new Vector3(block.Data, i);
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
                                prim.TextColor = new Color4(block.Data, i, false);
                                // FIXME: Is alpha inversed here as well?
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
                                prim.Sound = new UUID(block.Data, i);
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
                            prim.Data.PathBegin = LLObject.UnpackBeginCut(pathBegin);
                            ushort pathEnd = Helpers.BytesToUInt16(block.Data, i); i += 2;
                            prim.Data.PathEnd = LLObject.UnpackEndCut(pathEnd);
                            prim.Data.PathScaleX = LLObject.UnpackPathScale(block.Data[i++]);
                            prim.Data.PathScaleY = LLObject.UnpackPathScale(block.Data[i++]);
                            prim.Data.PathShearX = LLObject.UnpackPathShear((sbyte)block.Data[i++]);
                            prim.Data.PathShearY = LLObject.UnpackPathShear((sbyte)block.Data[i++]);
                            prim.Data.PathTwist = LLObject.UnpackPathTwist((sbyte)block.Data[i++]);
                            prim.Data.PathTwistBegin = LLObject.UnpackPathTwist((sbyte)block.Data[i++]);
                            prim.Data.PathRadiusOffset = LLObject.UnpackPathTwist((sbyte)block.Data[i++]);
                            prim.Data.PathTaperX = LLObject.UnpackPathTaper((sbyte)block.Data[i++]);
                            prim.Data.PathTaperY = LLObject.UnpackPathTaper((sbyte)block.Data[i++]);
                            prim.Data.PathRevolutions = LLObject.UnpackPathRevolutions(block.Data[i++]);
                            prim.Data.PathSkew = LLObject.UnpackPathTwist((sbyte)block.Data[i++]);

                            prim.Data.profileCurve = block.Data[i++];
                            ushort profileBegin = Helpers.BytesToUInt16(block.Data, i); i += 2;
                            prim.Data.ProfileBegin = LLObject.UnpackBeginCut(profileBegin);
                            ushort profileEnd = Helpers.BytesToUInt16(block.Data, i); i += 2;
                            prim.Data.ProfileEnd = LLObject.UnpackEndCut(profileEnd);
                            ushort profileHollow = Helpers.BytesToUInt16(block.Data, i); i += 2;
                            prim.Data.ProfileHollow = LLObject.UnpackProfileHollow(profileHollow);

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
                            if ((flags & CompressedFlags.HasNameValues) != 0 && prim.ParentID != 0)
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
                            Logger.DebugLog("Got an ObjectUpdateCompressed for PCode " + pcode.ToString() +
                                ", implement this!", Client);
                            break;
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    Logger.Log("Error decoding an ObjectUpdateCompressed packet", Helpers.LogLevel.Warning, Client, e);
                    Logger.Log(block, Helpers.LogLevel.Warning);
                }
            }
        }

        /// <summary>
        /// Handles cached object update packets from the simulator
        /// </summary>
        /// <param name="packet">The packet containing the object data</param>
        /// <param name="simulator">The simulator sending the data</param>
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

        /// <summary>
        /// Handle KillObject packets from the simulator
        /// </summary>
        /// <param name="packet">The packet containing the object data</param>
        /// <param name="simulator">The simulator sending the data</param>
        protected void KillObjectHandler(Packet packet, Simulator simulator)
        {
            KillObjectPacket kill = (KillObjectPacket)packet;

            // Notify first, so that handler has a chance to get a
            // reference from the ObjectTracker to the object being killed
            for (int i = 0; i < kill.ObjectData.Length; i++)
                FireOnObjectKilled(simulator, kill.ObjectData[i].ID);

            lock (simulator.ObjectsPrimitives.Dictionary)
            {
                List<uint> removePrims = new List<uint>();

                if (Client.Settings.OBJECT_TRACKING)
                {
                    uint localID;
                    for (int i = 0; i < kill.ObjectData.Length; i++)
                    {
                        localID = kill.ObjectData[i].ID;

                        if (simulator.ObjectsPrimitives.Dictionary.ContainsKey(localID)) removePrims.Add(localID);

                        foreach (KeyValuePair<uint, Primitive> prim in simulator.ObjectsPrimitives.Dictionary)
                        {
                            if (prim.Value.ParentID == localID)
                            {
                                FireOnObjectKilled(simulator, prim.Key);
                                removePrims.Add(prim.Key);
                            }
                        }
                    }
                }

                if (Client.Settings.AVATAR_TRACKING)
                {
                    uint localID;
                    for (int i = 0; i < kill.ObjectData.Length; i++)
                    {
                        localID = kill.ObjectData[i].ID;

                        if (simulator.ObjectsAvatars.Dictionary.ContainsKey(localID)) removePrims.Add(localID);                            

                        List<uint> rootPrims = new List<uint>();

                        foreach (KeyValuePair<uint, Primitive> prim in simulator.ObjectsPrimitives.Dictionary)
                        {
                            if (prim.Value.ParentID == localID)
                            {
                                FireOnObjectKilled(simulator, prim.Key);
                                removePrims.Add(prim.Key);
                                rootPrims.Add(prim.Key);
                            }
                        }

                        foreach (KeyValuePair<uint, Primitive> prim in simulator.ObjectsPrimitives.Dictionary)
                        {
                            if (rootPrims.Contains(prim.Value.ParentID))
                            {
                                FireOnObjectKilled(simulator, prim.Key);
                                removePrims.Add(prim.Key);
                            }
                        }
                    }
                }

                //Do the actual removing outside of the loops but still inside the lock.
                //This safely prevents the collection from being modified during a loop.
                foreach (uint removeID in removePrims)
                    simulator.ObjectsPrimitives.Remove(removeID);
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
                props.Description = Utils.BytesToString(objectData.Description);
                props.FolderID = objectData.FolderID;
                props.FromTaskID = objectData.FromTaskID;
                props.GroupID = objectData.GroupID;
                props.InventorySerial = objectData.InventorySerial;
                props.ItemID = objectData.ItemID;
                props.LastOwnerID = objectData.LastOwnerID;
                props.Name = Utils.BytesToString(objectData.Name);
                props.ObjectID = objectData.ObjectID;
                props.OwnerID = objectData.OwnerID;
                props.OwnershipCost = objectData.OwnershipCost;
                props.SalePrice = objectData.SalePrice;
                props.SaleType = objectData.SaleType;
                props.SitName = Utils.BytesToString(objectData.SitName);
                props.TouchName = Utils.BytesToString(objectData.TouchName);

                int numTextures = objectData.TextureID.Length / 16;
                props.TextureIDs = new UUID[numTextures];
                for (int j = 0; j < numTextures; ++j)
                    props.TextureIDs[j] = new UUID(objectData.TextureID, j * 16);

                if (Client.Settings.OBJECT_TRACKING)
                {
                    Primitive findPrim = sim.ObjectsPrimitives.Find(
                        delegate(Primitive prim) { return prim.ID == props.ObjectID; });

                    if (findPrim != null)
                    {
                        lock (sim.ObjectsPrimitives.Dictionary)
                        {
                            if (sim.ObjectsPrimitives.Dictionary.ContainsKey(findPrim.LocalID))
                                sim.ObjectsPrimitives.Dictionary[findPrim.LocalID].Properties = props;
                        }
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
            props.Description = Utils.BytesToString(op.ObjectData.Description);
            props.GroupID = op.ObjectData.GroupID;
            props.LastOwnerID = op.ObjectData.LastOwnerID;
            props.Name = Utils.BytesToString(op.ObjectData.Name);
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

            if (Client.Settings.OBJECT_TRACKING)
            {
                Primitive findPrim = sim.ObjectsPrimitives.Find(
                        delegate(Primitive prim) { return prim.ID == props.ObjectID; });

                if (findPrim != null)
                {
                    lock (sim.ObjectsPrimitives.Dictionary)
                    {
                        if (sim.ObjectsPrimitives.Dictionary.ContainsKey(findPrim.LocalID))
                            sim.ObjectsPrimitives.Dictionary[findPrim.LocalID].PropertiesFamily = props;
                    }
                }
            }

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

            prim.PCode = PCode.Prim;
            prim.Material = LLObject.MaterialType.Wood;
            prim.ProfileCurve = LLObject.ProfileCurve.Square;
            prim.PathCurve = LLObject.PathCurve.Line;
            prim.ProfileEnd = 1f;
            prim.PathEnd = 1f;
            prim.PathScaleX = 1f;
            prim.PathScaleY = 1f;
            prim.PathRevolutions = 1f;
            
            return prim;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sim"></param>
        /// <param name="av"></param>
        /// <param name="localid"></param>
        /// <param name="oldSeatID"></param>
        protected void SetAvatarSittingOn(Simulator sim, Avatar av, uint localid, uint oldSeatID)
        {
            if (av.LocalID == Client.Self.localID) Client.Self.sittingOn = localid;
            av.sittingOn = localid;
                        

            if (OnAvatarSitChanged != null && oldSeatID != localid)
            {
                try { OnAvatarSitChanged(sim, av, localid, oldSeatID); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="dilation"></param>
		protected void UpdateDilation(Simulator s, uint dilation)
		{
            s.Stats.Dilation = (float)dilation / 65535.0f;
        }

        #endregion Utility Functions

        #region Event Notification

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sim"></param>
        /// <param name="props"></param>
        protected void FireOnObjectProperties(Simulator sim, LLObject.ObjectProperties props)
        {
            if (OnObjectProperties != null)
            {
                try { OnObjectProperties(sim, props); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sim"></param>
        /// <param name="props"></param>
        protected void FireOnObjectPropertiesFamily(Simulator sim, LLObject.ObjectPropertiesFamily props)
        {
            if (OnObjectPropertiesFamily != null)
            {
                try { OnObjectPropertiesFamily(sim, props); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localid"></param>
        protected void FireOnObjectKilled(Simulator simulator, uint localid)
        {
            if (OnObjectKilled != null)
            {
                try { OnObjectKilled(simulator, localid); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="prim"></param>
        /// <param name="RegionHandle"></param>
        /// <param name="TimeDilation"></param>
        protected void FireOnNewPrim(Simulator simulator, Primitive prim, ulong RegionHandle, ushort TimeDilation)
        {
            if (OnNewPrim != null)
            {
                try { OnNewPrim(simulator, prim, RegionHandle, TimeDilation); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="prim"></param>
        /// <param name="RegionHandle"></param>
        /// <param name="TimeDilation"></param>
        protected void FireOnNewFoliage(Simulator simulator, Primitive prim, ulong RegionHandle, ushort TimeDilation)
        {
            if (OnNewFoliage != null)
            {
                try { OnNewFoliage(simulator, prim, RegionHandle, TimeDilation); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="prim"></param>
        /// <param name="RegionHandle"></param>
        /// <param name="TimeDilation"></param>
        protected void FireOnNewAttachment(Simulator simulator, Primitive prim, ulong RegionHandle, ushort TimeDilation)
        {
            if (OnNewAttachment != null)
            {
                try { OnNewAttachment(simulator, prim, RegionHandle, TimeDilation); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="avatar"></param>
        /// <param name="RegionHandle"></param>
        /// <param name="TimeDilation"></param>
        protected void FireOnNewAvatar(Simulator simulator, Avatar avatar, ulong RegionHandle, ushort TimeDilation)
        {
            if (OnNewAvatar != null)
            {
                try { OnNewAvatar(simulator, avatar, RegionHandle, TimeDilation); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="update"></param>
        /// <param name="RegionHandle"></param>
        /// <param name="TimeDilation"></param>
        protected void FireOnObjectUpdated(Simulator simulator, ObjectUpdate update, ulong RegionHandle, ushort TimeDilation)
        {
            if (OnObjectUpdated != null)
            {
                try { OnObjectUpdated(simulator, update, RegionHandle, TimeDilation); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        #endregion

        #region Object Tracking Link

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="fullID"></param>
        /// <returns></returns>
        protected Primitive GetPrimitive(Simulator simulator, uint localID, UUID fullID)
        {
            if (Client.Settings.OBJECT_TRACKING)
            {
                Primitive prim;

                if (simulator.ObjectsPrimitives.TryGetValue(localID, out prim))
                {
                    return prim;
                }
                else
                {
                    prim = new Primitive();
                    prim.LocalID = localID;
                    prim.ID = fullID;
                    lock (simulator.ObjectsPrimitives.Dictionary)
                        simulator.ObjectsPrimitives.Dictionary[localID] = prim;

                    return prim;
                }
            }
            else
            {
                return new Primitive();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="fullID"></param>
        /// <returns></returns>
        protected Avatar GetAvatar(Simulator simulator, uint localID, UUID fullID)
        {
            if (Client.Settings.AVATAR_TRACKING)
            {
                Avatar avatar;

                if (simulator.ObjectsAvatars.TryGetValue(localID, out avatar))
                {
                    return avatar;
                }
                else
                {
                    avatar = new Avatar();
                    avatar.LocalID = localID;
                    avatar.ID = fullID;
                    lock (simulator.ObjectsAvatars.Dictionary)
                        simulator.ObjectsAvatars.Dictionary[localID] = avatar;

                    return avatar;
                }
            }
            else
            {
                return new Avatar();
            }
        }

        #endregion Object Tracking Link

        protected void InterpolationTimer_Elapsed(object obj)
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
                        Client.Network.Simulators[i].ObjectsAvatars.ForEach(
                            delegate(Avatar avatar)
                            {
                                #region Linear Motion
                                // Only do movement interpolation (extrapolation) when there is a non-zero velocity but 
                                // no acceleration
                                if (avatar.Acceleration != Vector3.Zero && avatar.Velocity == Vector3.Zero)
                                {
                                    avatar.Position += (avatar.Velocity + avatar.Acceleration *
                                        (0.5f * (adjSeconds - HAVOK_TIMESTEP))) * adjSeconds;
                                    avatar.Velocity += avatar.Acceleration * adjSeconds;
                                }
                                #endregion Linear Motion
                            }
                        );

                        // Iterate through all of this sims primitives
                        Client.Network.Simulators[i].ObjectsPrimitives.ForEach(
                            delegate(Primitive prim)
                            {
                                if (prim.Joint == Primitive.JointType.Invalid)
                                {
                                    #region Angular Velocity
                                    Vector3 angVel = prim.AngularVelocity;
                                    float omega = angVel.LengthSquared();

                                    if (omega > 0.00001f)
                                    {
                                        omega = (float)Math.Sqrt(omega);
                                        float angle = omega * adjSeconds;
                                        angVel *= 1.0f / omega;
                                        Quaternion dQ = Quaternion.CreateFromAxisAngle(angVel, angle);

                                        prim.Rotation *= dQ;
                                    }
                                    #endregion Angular Velocity

                                    #region Linear Motion
                                    // Only do movement interpolation (extrapolation) when there is a non-zero velocity but 
                                    // no acceleration
                                    if (prim.Acceleration != Vector3.Zero && prim.Velocity == Vector3.Zero)
                                    {
                                        prim.Position += (prim.Velocity + prim.Acceleration *
                                            (0.5f * (adjSeconds - HAVOK_TIMESTEP))) * adjSeconds;
                                        prim.Velocity += prim.Acceleration * adjSeconds;
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
                                    Logger.Log("Unhandled joint type " + prim.Joint, Helpers.LogLevel.Warning, Client);
                                }
                            }
                        );
                    }
                }

                // Make sure the last interpolated time is always updated
                Client.Self.lastInterpolation = Environment.TickCount;
            }
        }
    }
}
