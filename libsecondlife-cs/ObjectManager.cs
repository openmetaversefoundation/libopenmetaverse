/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
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
using System.Xml.Serialization;
using System.Text;
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Contains all of the variables sent in an object update packet for a 
    /// prim object. Used to track position and movement of prims.
    /// </summary>
    public struct PrimUpdate
    {
        /// <summary></summary>
        public uint LocalID;
        /// <summary></summary>
        public byte State;
        /// <summary></summary>
        public LLVector3 Position;
        /// <summary></summary>
        public LLVector3 Velocity;
        /// <summary></summary>
        public LLVector3 Acceleration;
        /// <summary></summary>
        public LLQuaternion Rotation;
        /// <summary></summary>
        public LLVector3 RotationVelocity;
        /// <summary></summary>
        public TextureEntry Textures;
    }

    /// <summary>
    /// Contains all of the variables sent in an object update packet for an 
    /// avatar. Used to track position and movement of avatars.
    /// </summary>
    public struct AvatarUpdate
    {
        /// <summary></summary>
        public uint LocalID;
        /// <summary></summary>
        public byte State;
        /// <summary></summary>
        public LLVector4 CollisionPlane;
        /// <summary></summary>
        public LLVector3 Position;
        /// <summary></summary>
        public LLVector3 Velocity;
        /// <summary></summary>
        public LLVector3 Acceleration;
        /// <summary></summary>
        public LLQuaternion Rotation;
        /// <summary></summary>
        public LLVector3 RotationVelocity;
        /// <summary></summary>
        public TextureEntry Textures;
    }

    /// <summary>
    /// Handles all network traffic related to prims and avatar positions and 
    /// movement.
    /// </summary>
    public class ObjectManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="prim"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        public delegate void NewPrimCallback(Simulator simulator, PrimObject prim, ulong regionHandle,
            ushort timeDilation);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="prim"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        public delegate void NewAttachmentCallback(Simulator simulator, PrimObject prim, ulong regionHandle,
            ushort timeDilation);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="properties"></param>
        public delegate void ObjectPropertiesFamilyCallback(Simulator simulator, ObjectProperties properties);
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
        /// <param name="prim"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        public delegate void NewFoliageCallback(Simulator simulator, PrimObject foliage, ulong regionHandle,
            ushort timeDilation);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="prim"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        public delegate void PrimMovedCallback(Simulator simulator, PrimUpdate prim, ulong regionHandle,
            ushort timeDilation);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="avatar"></param>
        /// <param name="regionHandle"></param>
        /// <param name="timeDilation"></param>
        public delegate void AvatarMovedCallback(Simulator simulator, AvatarUpdate avatar, ulong regionHandle,
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
        public delegate void AvatarSitChanged(Simulator simulator, uint sittingOn);

        /// <summary>
        /// 
        /// </summary>
        public enum PCode
        {
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
        public enum AttachmentPoint
        {
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
            RightForarm,
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
            RightPec
        }

        /// <summary>
        /// Bitflag field for ObjectUpdateCompressed data blocks, describing 
        /// which options are present for each object
        /// </summary>
        [Flags]
        public enum CompressedFlags
        {
            /// <summary>Hasn't been spotted in the wild yet</summary>
            Unknown1 = 0x01,
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
            /// <summary>Semi-common flag, currently unknown</summary>
            Unknown2 = 0x40,
            /// <summary>Whether the object has an angular velocity</summary>
            HasAngularVelocity = 0x80,
            /// <summary>Whether the object is an attachment or not</summary>
            Attachment = 0x100
        }

        /// <summary>
        /// 
        /// </summary>
        public enum Tree
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
        public enum Grass
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
            undergrowth_1
        }

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
        /// This event will be raised when a prim movement packet is received, 
        /// containing the updated position, rotation, and movement-related 
        /// vectors.
        /// </summary>
        public event PrimMovedCallback OnPrimMoved;
        /// <summary>
        /// This event will be raised when an avatar movement packet is 
        /// received, containing the updated position, rotation, and 
        /// movement-related vectors.
        /// </summary>
        public event AvatarMovedCallback OnAvatarMoved;
        /// <summary>
        /// This event will be raised when the main avatar sits on an 
        /// object or stands up, with a local ID of the current seat or
        /// zero.
        /// </summary>
        public event AvatarSitChanged OnAvatarSitChanged;
        /// <summary>
        /// This event will be raised when an object is removed from a 
        /// simulator.
        /// </summary>
        public event KillObjectCallback OnObjectKilled;
        /// <summary>
        /// Thie event will be raised when an object's properties are recieved
        /// from the simulator
        /// </summary>
        public event ObjectPropertiesFamilyCallback OnObjectProperties;
        /// <summary>
        /// If true, when a cached object check is received from the server 
        /// the full object info will automatically be requested.
        /// </summary>
        /// 
        public bool RequestAllObjects = false;        

        private SecondLife Client;

        /// <summary>
        /// Instantiates a new ObjectManager class. This class should only be accessed
        /// through SecondLife.Objects, client applications should never create their own
        /// </summary>
        /// <param name="client">A reference to the client</param>
        public ObjectManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.ObjectUpdate, new NetworkManager.PacketCallback(UpdateHandler));
            Client.Network.RegisterCallback(PacketType.ImprovedTerseObjectUpdate, new NetworkManager.PacketCallback(TerseUpdateHandler));
            Client.Network.RegisterCallback(PacketType.ObjectUpdateCompressed, new NetworkManager.PacketCallback(CompressedUpdateHandler));
            Client.Network.RegisterCallback(PacketType.ObjectUpdateCached, new NetworkManager.PacketCallback(CachedUpdateHandler));
            Client.Network.RegisterCallback(PacketType.KillObject, new NetworkManager.PacketCallback(KillObjectHandler));
            Client.Network.RegisterCallback(PacketType.ObjectPropertiesFamily, new NetworkManager.PacketCallback(ObjectPropertiesFamilyHandler));
        }

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
            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
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
            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;
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
        /// Create, or "rez" a new prim object in a simulator
        /// </summary>
        /// <param name="simulator">The target simulator</param>
        /// <param name="prim">The prim object to rez</param>
        /// <param name="position">An approximation of the position at which to rez the prim</param>
        /// <remarks>Due to the way client prim rezzing is done on the server,
        /// the requested position for an object is only close to where the prim
        /// actually ends up. If you desire exact placement you'll need to 
        /// follow up by moving the object after it has been created.</remarks>
        public void AddPrim(Simulator simulator, PrimObject prim, LLVector3 position)
        {
            ObjectAddPacket packet = new ObjectAddPacket();

            packet.AgentData.AgentID = Client.Network.AgentID;
            packet.AgentData.SessionID = Client.Network.SessionID;
            packet.AgentData.GroupID = prim.GroupID;

            packet.ObjectData.State = (byte)prim.State;
            packet.ObjectData.AddFlags = (uint)ObjectFlags.CreateSelected;
            packet.ObjectData.PCode = (byte)PCode.Prim;

            packet.ObjectData.Material = (byte)prim.Material;
            packet.ObjectData.Scale = prim.Scale;
            packet.ObjectData.Rotation = prim.Rotation;

            packet.ObjectData.PathBegin = PrimObject.PathBeginByte(prim.PathBegin);
            packet.ObjectData.PathCurve = (byte)prim.PathCurve;
            packet.ObjectData.PathEnd = PrimObject.PathEndByte(prim.PathEnd);
            packet.ObjectData.PathRadiusOffset = PrimObject.PathRadiusOffsetByte(prim.PathRadiusOffset);
            packet.ObjectData.PathRevolutions = PrimObject.PathRevolutionsByte(prim.PathRevolutions);
            packet.ObjectData.PathScaleX = PrimObject.PathScaleByte(prim.PathScaleX);
            packet.ObjectData.PathScaleY = PrimObject.PathScaleByte(prim.PathScaleY);
            packet.ObjectData.PathShearX = PrimObject.PathShearByte(prim.PathShearX);
            packet.ObjectData.PathShearY = PrimObject.PathShearByte(prim.PathShearY);
            packet.ObjectData.PathSkew = PrimObject.PathSkewByte(prim.PathSkew);
            packet.ObjectData.PathTaperX = PrimObject.PathTaperByte(prim.PathTaperX);
            packet.ObjectData.PathTaperY = PrimObject.PathTaperByte(prim.PathTaperY);
            packet.ObjectData.PathTwist = (sbyte)prim.PathTwist;
            packet.ObjectData.PathTwistBegin = (sbyte)prim.PathTwistBegin;

            packet.ObjectData.ProfileCurve = (byte)prim.ProfileCurve;
            packet.ObjectData.ProfileBegin = PrimObject.ProfileBeginByte(prim.ProfileBegin);
            packet.ObjectData.ProfileEnd = PrimObject.ProfileEndByte(prim.ProfileEnd);
            packet.ObjectData.ProfileHollow = (byte)prim.ProfileHollow;

            packet.ObjectData.RayStart = position;
            packet.ObjectData.RayEnd = position;
            packet.ObjectData.RayEndIsIntersection = 0;
            packet.ObjectData.RayTargetID = LLUUID.Zero;
            packet.ObjectData.BypassRaycast = 1;

            // TODO: This is no longer a field in ObjectAdd. Detect if there actually is 
            // texture information for this prim and send an ObjectUpdate
            //packet.ObjectData.TextureEntry = prim.Textures.GetBytes();

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

            add.AgentData.AgentID = Client.Network.AgentID;
            add.AgentData.SessionID = Client.Network.SessionID;
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

            add.AgentData.AgentID = Client.Network.AgentID;
            add.AgentData.SessionID = Client.Network.SessionID;
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
        public void SetTextures(Simulator simulator, uint localID, TextureEntry textures)
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
        public void SetTextures(Simulator simulator, uint localID, TextureEntry textures, string mediaUrl)
        {
            ObjectImagePacket image = new ObjectImagePacket();

            image.AgentData.AgentID = Client.Network.AgentID;
            image.AgentData.SessionID = Client.Network.SessionID;
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
        public void SetLight(Simulator simulator, uint localID, PrimLightData light)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();

            extra.AgentData.AgentID = Client.Network.AgentID;
            extra.AgentData.SessionID = Client.Network.SessionID;
            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)ExtraParamType.Light;
            if (light == null)
            {
                extra.ObjectData[0].ParamInUse = false;
                extra.ObjectData[0].ParamData = new byte[0];
            }
            else
            {
                extra.ObjectData[0].ParamInUse = true;
                extra.ObjectData[0].ParamData = light.GetBytes();
            }
            extra.ObjectData[0].ParamSize = (uint)extra.ObjectData[0].ParamData.Length;

            Client.Network.SendPacket(extra, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localID"></param>
        /// <param name="flexible"></param>
        public void SetFlexible(Simulator simulator, uint localID, PrimFlexibleData flexible)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();

            extra.AgentData.AgentID = Client.Network.AgentID;
            extra.AgentData.SessionID = Client.Network.SessionID;
            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)ExtraParamType.Flexible;
            if (flexible == null)
            {
                extra.ObjectData[0].ParamInUse = false;
                extra.ObjectData[0].ParamData = new byte[0];
            }
            else
            {
                extra.ObjectData[0].ParamInUse = true;
                extra.ObjectData[0].ParamData = flexible.GetBytes();
            }
            extra.ObjectData[0].ParamSize = (uint)extra.ObjectData[0].ParamData.Length;

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

            packet.AgentData.AgentID = Client.Network.AgentID;
            packet.AgentData.SessionID = Client.Network.SessionID;

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
            objRotPacket.AgentData.AgentID = Client.Network.AgentID;
            objRotPacket.AgentData.SessionID = Client.Network.SessionID;

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
        /// <param name="attachPoint"></param>
        /// <param name="rotation"></param>
        public void AttachObject(Simulator simulator, uint localID, AttachmentPoint attachPoint, LLQuaternion rotation)
        {
            ObjectAttachPacket attach = new ObjectAttachPacket();
            attach.AgentData.AgentID = Client.Network.AgentID;
            attach.AgentData.SessionID = Client.Network.SessionID;
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
            detach.AgentData.AgentID = Client.Network.AgentID;
            detach.AgentData.SessionID = Client.Network.SessionID;
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
            objPosPacket.AgentData.AgentID = Client.Self.ID;
            objPosPacket.AgentData.SessionID = Client.Network.SessionID;

            objPosPacket.ObjectData = new ObjectPositionPacket.ObjectDataBlock[1];

            objPosPacket.ObjectData[0] = new ObjectPositionPacket.ObjectDataBlock();
            objPosPacket.ObjectData[0].ObjectLocalID = localID;
            objPosPacket.ObjectData[0].Position = position;

            Client.Network.SendPacket(objPosPacket, simulator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="localIDs"></param>
        /// <param name="who"></param>
        /// <param name="permissions"></param>
        /// <param name="set"></param>
        public void SetPermissions(Simulator simulator, List<uint> localIDs, Helpers.PermissionWho who, 
            Helpers.PermissionType permissions, bool set)
        {
            ObjectPermissionsPacket packet = new ObjectPermissionsPacket();

            packet.AgentData.AgentID = Client.Network.AgentID;
            packet.AgentData.SessionID = Client.Network.SessionID;

            packet.HeaderData.Override = false;

            packet.ObjectData = new ObjectPermissionsPacket.ObjectDataBlock[localIDs.Count];

            int i = 0;
            foreach (uint localID in localIDs)
            {
                packet.ObjectData[i] = new ObjectPermissionsPacket.ObjectDataBlock();
                packet.ObjectData[i].ObjectLocalID = localID;
                packet.ObjectData[i].Field = (byte)who;
                packet.ObjectData[i].Mask = (uint)permissions;
                packet.ObjectData[i].Set = Convert.ToByte(set);

                i++;
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
            RequestObjectPropertiesFamilyPacket properties = new RequestObjectPropertiesFamilyPacket();
            properties.AgentData.AgentID = Client.Network.AgentID;
            properties.AgentData.SessionID = Client.Network.SessionID;
            properties.ObjectData.ObjectID = objectID;
            properties.ObjectData.RequestFlags = 0;

            Client.Network.SendPacket(properties, simulator);
        }

        private void ParseAvName(string name, ref string firstName, ref string lastName, ref string groupName)
        {
            // FIXME: This needs to be reworked completely. It fails on anything containing unicode
            // (which would break FieldToString as well), or name strings that don't contain the 
            // most common attributes which is all we handle right now.
            string[] lines = name.Split('\n');

            foreach (string line in lines)
            {
                if (line.Substring(0, 19) == "Title STRING RW SV ")
                {
                    groupName = line.Substring(19);
                }
                else if (line.Substring(0, 23) == "FirstName STRING RW SV ")
                {
                    firstName = line.Substring(23);
                }
                else if (line.Substring(0, 22) == "LastName STRING RW SV ")
                {
                    lastName = line.Substring(22);
                }
                else
                {
                    Client.Log("Unhandled line in an avatar name: " + line, Helpers.LogLevel.Warning);
                }
            }
        }

        private void UpdateHandler(Packet packet, Simulator simulator)
        {
            if (OnNewPrim != null || OnNewAttachment != null || OnNewAvatar != null || OnNewFoliage != null)
            {
                ObjectUpdatePacket update = (ObjectUpdatePacket)packet;

                foreach (ObjectUpdatePacket.ObjectDataBlock block in update.ObjectData)
                {
                    byte pcode = block.PCode;
                    switch (pcode)
                    {
                        case (byte)PCode.Grass:
                        case (byte)PCode.Tree:
                        case (byte)PCode.Prim:
                            string name = Helpers.FieldToString(block.NameValue);

                            // New prim spotted
                            PrimObject prim = new PrimObject();

                            prim.Name = name;

							prim.RegionHandle = update.RegionData.RegionHandle;
                            prim.Position = new LLVector3(block.ObjectData, 0);
                            prim.Rotation = new LLQuaternion(block.ObjectData, 36, true);
                            // TODO: Parse the rest of the ObjectData byte array fields

                            prim.LocalID = block.ID;
                            prim.State = block.State;
                            prim.ID = block.FullID;
                            prim.ParentID = block.ParentID;
                            //block.OwnerID Sound-related
                            prim.Material = block.Material;
                            prim.PathCurve = block.PathCurve;
                            prim.ProfileCurve = block.ProfileCurve;
                            prim.PathBegin = PrimObject.PathBeginFloat(block.PathBegin);
                            prim.PathEnd = PrimObject.PathEndFloat(block.PathEnd);
                            prim.PathScaleX = PrimObject.PathScaleFloat(block.PathScaleX);
                            prim.PathScaleY = PrimObject.PathScaleFloat(block.PathScaleY);
                            prim.PathShearX = PrimObject.PathShearFloat(block.PathShearX);
                            prim.PathShearY = PrimObject.PathShearFloat(block.PathShearY);
                            prim.PathTwist = block.PathTwist;
                            prim.PathTwistBegin = block.PathTwistBegin;
                            prim.PathRadiusOffset = PrimObject.PathRadiusOffsetFloat(block.PathRadiusOffset);
                            prim.PathTaperX = PrimObject.PathTaperFloat(block.PathTaperX);
                            prim.PathTaperY = PrimObject.PathTaperFloat(block.PathTaperY);
                            prim.PathRevolutions = PrimObject.PathRevolutionsFloat(block.PathRevolutions);
                            prim.PathSkew = PrimObject.PathSkewFloat(block.PathSkew);
                            prim.ProfileBegin = PrimObject.ProfileBeginFloat(block.ProfileBegin);
                            prim.ProfileEnd = PrimObject.ProfileEndFloat(block.ProfileEnd);
                            prim.ProfileHollow = block.ProfileHollow;


                            //block.Data ?
                            prim.Text = ASCIIEncoding.ASCII.GetString(block.Text);
                            //block.TextColor LLColor4U of the hovering text
                            //block.MediaURL Quicktime stream
                            prim.Textures = new TextureEntry(block.TextureEntry, 0, block.TextureEntry.Length);
                            prim.TextureAnim = new TextureAnimation(block.TextureAnim, 0);
                            //block.JointType ?
                            //block.JointPivot ?
                            //block.JointAxisOrAnchor ?
                            prim.ParticleSys = new ParticleSystem(block.PSBlock, 0);
                            prim.SetExtraParamsFromBytes(block.ExtraParams, 0);
                            prim.Scale = block.Scale;
                            //block.Flags ?
                            prim.Flags = (ObjectFlags)block.UpdateFlags;
                            //block.ClickAction ?
                            //block.Gain Sound-related
                            //block.Sound Sound-related
                            //block.Radius Sound-related

                            if (prim.Name.StartsWith("AttachItemID"))
                            {
                                if (OnNewAttachment != null)
                                {
                                    OnNewAttachment(simulator, prim, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                                }
                            }
                            else if (block.PCode == (byte)PCode.Tree || block.PCode == (byte)PCode.Grass)
                            {
                                if (OnNewFoliage != null)
                                {
                                    OnNewFoliage(simulator, prim, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                                }
                            }
                            else if (OnNewPrim != null)
                            {
                                OnNewPrim(simulator, prim, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                            }

                            break;
                        case (byte)PCode.Avatar:
                            if (block.FullID == Client.Network.AgentID)
                            {
                                // Detect if we are sitting or standing
                                uint oldSittingOn = Client.Self.sittingOn;
                                Client.Self.sittingOn = block.ParentID;

                                // Fire the callback for our sitting orientation changing
                                if (Client.Self.sittingOn != oldSittingOn && OnAvatarSitChanged != null)
                                {
                                    OnAvatarSitChanged(simulator, Client.Self.sittingOn);
                                }
                            }

                            if (OnNewAvatar != null)
                            {
                                Avatar avatar = new Avatar();

                                string FirstName = "";
                                string LastName = "";
                                string GroupName = "";

                                //avatar.CollisionPlane = new LLQuaternion(block.ObjectData, 0);
                                avatar.Position = new LLVector3(block.ObjectData, 16);
                                avatar.Rotation = new LLQuaternion(block.ObjectData, 52, true);

                                // TODO: Parse the rest of the ObjectData byte array fields

                                ParseAvName(Helpers.FieldToString(block.NameValue), ref FirstName, ref LastName, ref GroupName);

                                avatar.ID = block.FullID;
                                avatar.LocalID = block.ID;
                                avatar.Name = FirstName + " " + LastName;
                                avatar.GroupName = GroupName;
                                avatar.Online = true;
                                avatar.CurrentRegion = simulator.Region;

                                avatar.Textures = new TextureEntry(block.TextureEntry, 0, block.TextureEntry.Length);

                                if (FirstName == Client.Self.FirstName && LastName == Client.Self.LastName)
                                {
                                    // Update our avatar
                                    Client.Self.LocalID = avatar.LocalID;
                                    Client.Self.Position = avatar.Position;
                                    Client.Self.Rotation = avatar.Rotation;
                                }
                                else
                                {
                                    if (OnNewAvatar != null)
                                    {
                                        OnNewAvatar(simulator, avatar, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                                    }
                                }
                            }
                            break;
                        case (byte)PCode.ParticleSystem:
                            // FIXME: Handle ParticleSystem
                            Client.DebugLog("Got an ObjectUpdate block with a ParticleSystem PCode");
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void TerseUpdateHandler(Packet packet, Simulator simulator)
        {
            float x, y, z, w;
            uint localid;
            LLVector4 CollisionPlane = LLVector4.Zero;
            LLVector3 Position;
            LLVector3 Velocity;
            LLVector3 Acceleration;
            LLQuaternion Rotation;
            LLVector3 RotationVelocity;

            ImprovedTerseObjectUpdatePacket update = (ImprovedTerseObjectUpdatePacket)packet;

            foreach (ImprovedTerseObjectUpdatePacket.ObjectDataBlock block in update.ObjectData)
            {
                int i = 0;
                bool avatar;

                localid = (uint)(block.Data[i++] + (block.Data[i++] << 8) +
                    (block.Data[i++] << 16) + (block.Data[i++] << 24));

                byte state = block.Data[i++];

                avatar = Convert.ToBoolean(block.Data[i++]);

                if (avatar)
                {
                    if (OnAvatarMoved == null) return;

                    CollisionPlane = new LLVector4(block.Data, i);
                    i += 16;
                }
                else
                {
                    if (OnPrimMoved == null) return;
                }

                // Position
                Position = new LLVector3(block.Data, i);
                i += 12;
                // Velocity
                x = Dequantize(block.Data, i, -128.0F, 128.0F);
                i += 2;
                y = Dequantize(block.Data, i, -128.0F, 128.0F);
                i += 2;
                z = Dequantize(block.Data, i, -128.0F, 128.0F);
                i += 2;
                Velocity = new LLVector3(x, y, z);
                // Acceleration
                x = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                y = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                z = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                Acceleration = new LLVector3(x, y, z);
                // Rotation
                x = Dequantize(block.Data, i, -1.0F, 1.0F);
                i += 2;
                y = Dequantize(block.Data, i, -1.0F, 1.0F);
                i += 2;
                z = Dequantize(block.Data, i, -1.0F, 1.0F);
                i += 2;
                w = Dequantize(block.Data, i, -1.0F, 1.0F);
                i += 2;
                Rotation = new LLQuaternion(x, y, z, w);
                // Rotation velocity
                x = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                y = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                z = Dequantize(block.Data, i, -64.0F, 64.0F);
                i += 2;
                RotationVelocity = new LLVector3(x, y, z);

                if (avatar)
                {
                    if (localid == Client.Self.LocalID)
                    {
                        Client.Self.Position = Position;
                        Client.Self.Rotation = Rotation;
                    }

                    AvatarUpdate avupdate = new AvatarUpdate();
                    avupdate.LocalID = localid;
                    avupdate.State = state;
                    avupdate.Position = Position;
                    avupdate.CollisionPlane = CollisionPlane;
                    avupdate.Velocity = Velocity;
                    avupdate.Acceleration = Acceleration;
                    avupdate.Rotation = Rotation;
                    avupdate.RotationVelocity = RotationVelocity;
                    avupdate.Textures = new TextureEntry(block.TextureEntry, 4, block.TextureEntry.Length - 4);

                    if (OnAvatarMoved != null)
                    {
                        OnAvatarMoved(simulator, avupdate, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                    }
                }
                else
                {
                    // TODO: Is there an easy way to distinguish prims from trees in this packet,
                    // or would the client have to do it's own lookup to determine whether it's a
                    // prim or a tree? If the latter, we should rename this update to something 
                    // less prim specific

                    PrimUpdate primupdate = new PrimUpdate();
                    primupdate.LocalID = localid;
                    primupdate.State = state;
                    primupdate.Position = Position;
                    primupdate.Velocity = Velocity;
                    primupdate.Acceleration = Acceleration;
                    primupdate.Rotation = Rotation;
                    primupdate.RotationVelocity = RotationVelocity;
                    primupdate.Textures = new TextureEntry(block.TextureEntry, 4, block.TextureEntry.Length - 4);

                    if (OnPrimMoved != null)
                    {
                        OnPrimMoved(simulator, primupdate, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                    }
                }
            }
        }

#pragma warning disable 0219 // disable "value assigned but never used" while this function is incomplete
        private void CompressedUpdateHandler(Packet packet, Simulator simulator)
        {
            if (OnNewPrim != null || OnNewAvatar != null || OnNewAttachment != null || OnNewFoliage != null)
            {
                ObjectUpdateCompressedPacket update = (ObjectUpdateCompressedPacket)packet;
                PrimObject prim;

                foreach (ObjectUpdateCompressedPacket.ObjectDataBlock block in update.ObjectData)
                {
                    int i = 0;
                    prim = new PrimObject();

                    prim.Flags = (ObjectFlags)block.UpdateFlags;

                    try
                    {
                        prim.ID = new LLUUID(block.Data, 0);
                        i += 16;
                        prim.LocalID = (uint)(block.Data[i++] + (block.Data[i++] << 8) +
                            (block.Data[i++] << 16) + (block.Data[i++] << 24));

                        byte pcode = block.Data[i++];
                        if (pcode == (byte)PCode.Prim)
                        {
                            #region Prim
                            prim.State = (uint)block.Data[i++];
                            i += 4; // CRC
                            prim.Material = (uint)block.Data[i++];
                            i++; // TODO: ClickAction

                            prim.Scale = new LLVector3(block.Data, i);
                            i += 12;
                            prim.Position = new LLVector3(block.Data, i);
                            i += 12;
                            prim.Rotation = new LLQuaternion(block.Data, i, true);
                            i += 12;

                            int flagsValue = (int)block.Data[i++] + (int)(block.Data[i++] << 8) +
                                (int)(block.Data[i++] << 16) + (int)(block.Data[i++] << 24);
                            CompressedFlags flags = (CompressedFlags)flagsValue;

                            if ((flags & CompressedFlags.Tree) != 0)
                            {
                                // FIXME: I don't think this is even Tree data, as it would have
                                // a different PCode. Figure out what this flag is and how to 
                                // decode it
                                byte Unknown1 = block.Data[i++];
                                byte Unknown2 = block.Data[i++];

                                Client.DebugLog("Compressed object with Tree flag set: " + Environment.NewLine +
                                    "Unknown byte 1: " + Unknown1 + Environment.NewLine + "Unknown byte 2: " + Unknown2);
                            }

                            if ((flags & CompressedFlags.HasParent) != 0)
                            {
                                prim.ParentID = (uint)(block.Data[i++] + (block.Data[i++] << 8) +
                                (block.Data[i++] << 16) + (block.Data[i++] << 24));
                            }
                            else
                            {
                                prim.ParentID = 0;
                            }

                            if ((flags & CompressedFlags.HasAngularVelocity) != 0)
                            {
                                // TODO: Use this
                                LLVector3 Omega = new LLVector3(block.Data, i);
                                i += 12;
                            }

                            if ((flags & CompressedFlags.HasText) != 0)
                            {
                                string text = "";
                                while (block.Data[i] != 0)
                                {
                                    text += (char)block.Data[i];
                                    i++;
                                }
                                i++;

                                prim.Text = text;

                                // TODO: Text color
                                i += 4;
                            }
                            else
                            {
                                prim.Text = "";
                            }

                            if ((flags & CompressedFlags.HasParticles) != 0)
                            {
                                prim.ParticleSys = new ParticleSystem(block.Data, i);
                                i += 86;
                            }

                            i += prim.SetExtraParamsFromBytes(block.Data, i);

                            //Sound data
                            if ((flags & CompressedFlags.HasSound) != 0)
                            {
                                //TODO: use this data
                                LLUUID SoundUUID = new LLUUID(block.Data, i);
                                i += 16;
                                LLUUID OwnerUUID = new LLUUID(block.Data, i);
                                i += 16;

                                if (!BitConverter.IsLittleEndian)
                                {
                                    Array.Reverse(block.Data, i, 4);
                                    Array.Reverse(block.Data, i + 5, 4);
                                }

                                float SoundGain = BitConverter.ToSingle(block.Data, i);
                                i += 4;
                                byte SoundFlags = block.Data[i++];
                                float SoundRadius = BitConverter.ToSingle(block.Data, i);
                                i += 4;
                            }

                            // Indicates that this is an attachment
                            if ((flags & CompressedFlags.Attachment) != 0)
                            {
                                // Get the attachment string
                                // Example: "AttachItemID STRING RW SV fa9a5ab8-1bad-b449-9873-cf5b803e664e"
                                string text = "";
                                while (block.Data[i] != 0)
                                {
                                    text += (char)block.Data[i];
                                    i++;
                                }
                                i++;

                                prim.Name = text;
                            }
                            else
                            {
                                prim.Name = "";
                            }

                            if ((flags & CompressedFlags.Unknown1) != 0)
                            {
                                // TODO: Is this even a valid flag?
                                Client.DebugLog("Compressed object with Unknown1 flag set: " + Environment.NewLine +
                                    "Flags: " + flags.ToString() + Environment.NewLine +
                                    Helpers.FieldToString(block.Data));
                            }

                            if ((flags & CompressedFlags.Unknown2) != 0)
                            {
                                // FIXME: Implement CompressedFlags.Unknown2
                                //Client.DebugLog("Compressed object with Unknown2 flag set: " + Environment.NewLine +
                                //    "Flags: " + flags.ToString() + Environment.NewLine +
                                //    Helpers.FieldToString(block.Data));
                            }

                            prim.PathCurve = (uint)block.Data[i++];
                            prim.PathBegin = PrimObject.PathBeginFloat(block.Data[i++]);
                            prim.PathEnd = PrimObject.PathEndFloat(block.Data[i++]);
                            prim.PathScaleX = PrimObject.PathScaleFloat(block.Data[i++]);
                            prim.PathScaleY = PrimObject.PathScaleFloat(block.Data[i++]);
                            prim.PathShearX = PrimObject.PathShearFloat(block.Data[i++]);
                            prim.PathShearY = PrimObject.PathShearFloat(block.Data[i++]);
                            prim.PathTwist = (int)block.Data[i++];
                            prim.PathTwistBegin = (int)block.Data[i++];
                            prim.PathRadiusOffset = PrimObject.PathRadiusOffsetFloat((sbyte)block.Data[i++]);
                            prim.PathTaperX = PrimObject.PathTaperFloat((sbyte)block.Data[i++]);
                            prim.PathTaperY = PrimObject.PathTaperFloat((sbyte)block.Data[i++]);
                            prim.PathRevolutions = PrimObject.PathRevolutionsFloat(block.Data[i++]);
                            prim.PathSkew = PrimObject.PathSkewFloat((sbyte)block.Data[i++]);

                            prim.ProfileCurve = (uint)block.Data[i++];
                            prim.ProfileBegin = PrimObject.ProfileBeginFloat(block.Data[i++]);
                            prim.ProfileEnd = PrimObject.ProfileEndFloat(block.Data[i++]);
                            prim.ProfileHollow = (uint)block.Data[i++];

                            int textureEntryLength = (int)(block.Data[i++] + (block.Data[i++] << 8) +
                                (block.Data[i++] << 16) + (block.Data[i++] << 24));

                            prim.Textures = new TextureEntry(block.Data, i, textureEntryLength);

                            i += textureEntryLength;

                            // Assume everything else is texture animation data
                            if (i < block.Data.Length)
                            {
                                int textureAnimLength = (int)(block.Data[i++] + (block.Data[i++] << 8) +
                                    (block.Data[i++] << 16) + (block.Data[i++] << 24));

                                prim.TextureAnim = new TextureAnimation(block.Data, i);
                            }

                            // Fire the appropriate callback
                            if ((flags & CompressedFlags.Attachment) != 0)
                            {
                                if (OnNewAttachment != null)
                                {
                                    OnNewAttachment(simulator, prim, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                                }
                            }
                            else if ((flags & CompressedFlags.Tree) != 0)
                            {
                                if (OnNewFoliage != null)
                                {
                                    OnNewFoliage(simulator, prim, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                                }
                            }
                            else if (OnNewPrim != null)
                            {
                                OnNewPrim(simulator, prim, update.RegionData.RegionHandle, update.RegionData.TimeDilation);
                            }
                            #endregion Prim
                        }
                        else if (pcode == (byte)PCode.Grass || pcode == (byte)PCode.Tree || pcode == (byte)PCode.NewTree)
                        {
                            // FIXME: Implement this
                            //Client.DebugLog("######### Got an ObjectUpdateCompressed for grass/tree, implement this! #########");
                        }
                        else
                        {
                            Client.Log("######### Got an ObjectUpdateCompressed for PCode=" + pcode.ToString() + 
                                ", implement this! #########", Helpers.LogLevel.Debug);
                        }
                    }
                    catch (System.IndexOutOfRangeException e)
                    {
                        Client.Log("Had a problem decoding an ObjectUpdateCompressed packet: " +
                            e.ToString(), Helpers.LogLevel.Warning);
                        Client.Log(block.ToString(), Helpers.LogLevel.Warning);
                    }
                }
            }
        }
#pragma warning restore 0219

        private void CachedUpdateHandler(Packet packet, Simulator simulator)
        {
            if (RequestAllObjects)
            {
                List<uint> ids = new List<uint>();
                ObjectUpdateCachedPacket update = (ObjectUpdateCachedPacket)packet;

                // Assume clients aren't caching objects for now, so request updates for all of these objects
                foreach (ObjectUpdateCachedPacket.ObjectDataBlock block in update.ObjectData)
                {
                    ids.Add(block.ID);
                }

                RequestObjects(simulator, ids);
            }
        }

        private void KillObjectHandler(Packet packet, Simulator simulator)
        {
            if (OnObjectKilled != null)
            {
                foreach (KillObjectPacket.ObjectDataBlock block in ((KillObjectPacket)packet).ObjectData)
                {
                    OnObjectKilled(simulator, block.ID);
                }
            }
        }

        /// <summary>
        /// Takes a quantized 16-bit value from a byte array and its range and returns 
        /// a float representation of the continuous value. For example, a value of 
        /// 32767 and a range of -128.0 to 128.0 would return 0.0. The endian conversion 
        /// from the 16-bit little endian to the native platform will also be handled.
        /// </summary>
        /// <param name="byteArray">The byte array containing the short value</param>
        /// <param name="pos">The beginning position of the short (quantized) value</param>
        /// <param name="lower">The lower quantization range</param>
        /// <param name="upper">The upper quantization range</param>
        /// <returns>A 32-bit floating point representation of the dequantized value</returns>
        private float Dequantize(byte[] byteArray, int pos, float lower, float upper)
        {
            ushort value = (ushort)(byteArray[pos] + (byteArray[pos + 1] << 8));
            float QV = (float)value;
            float range = upper - lower;
            float QF = range / 65536.0F;
            return (float)((QV * QF - (0.5F * range)) + QF);
        }

        private void ObjectPropertiesFamilyHandler(Packet p, Simulator sim)
        {
            if (OnObjectProperties != null)
            {
                ObjectPropertiesFamilyPacket op = (ObjectPropertiesFamilyPacket)p;
                ObjectProperties props = new ObjectProperties();

                props.BaseMask = op.ObjectData.BaseMask;
                props.Category = op.ObjectData.Category;
                props.Description = Helpers.FieldToString(op.ObjectData.Description);
                props.EveryoneMask = op.ObjectData.EveryoneMask;
                props.GroupID = op.ObjectData.GroupID;
                props.GroupMask = op.ObjectData.GroupMask;
                props.LastOwnerID = op.ObjectData.LastOwnerID;
                props.Name = Helpers.FieldToString(op.ObjectData.Name);
                props.NextOwnerMask = op.ObjectData.NextOwnerMask;
                props.ObjectID = op.ObjectData.ObjectID;
                props.OwnerID = op.ObjectData.OwnerID;
                props.OwnerMask = op.ObjectData.OwnerMask;
                props.OwnershipCost = op.ObjectData.OwnershipCost;
                props.SalePrice = op.ObjectData.SalePrice;
                props.SaleType = op.ObjectData.SaleType;

                OnObjectProperties(sim, props);
            }
        }
    }
}
