/*
 * Copyright (c) 2006-2009, openmetaverse.org
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
    /// 
    /// </summary>
    public enum ReportType : uint
    {
        /// <summary>No report</summary>
        None = 0,
        /// <summary>Unknown report type</summary>
        Unknown = 1,
        /// <summary>Bug report</summary>
        Bug = 2,
        /// <summary>Complaint report</summary>
        Complaint = 3,
        /// <summary>Customer service report</summary>
        CustomerServiceRequest = 4
    }

    /// <summary>
    /// Bitflag field for ObjectUpdateCompressed data blocks, describing 
    /// which options are present for each object
    /// </summary>
    [Flags]
    public enum CompressedFlags : uint
    {
        None = 0x00,
        /// <summary>Unknown</summary>
        ScratchPad = 0x01,
        /// <summary>Whether the object has a TreeSpecies</summary>
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
    /// Specific Flags for MultipleObjectUpdate requests
    /// </summary>
    [Flags]
    public enum UpdateType : uint
    {
        /// <summary>None</summary>
        None = 0x00,
        /// <summary>Change position of prims</summary>
        Position = 0x01,
        /// <summary>Change rotation of prims</summary>
        Rotation = 0x02,
        /// <summary>Change size of prims</summary>
        Scale = 0x04,
        /// <summary>Perform operation on link set</summary>
        Linked = 0x08,
        /// <summary>Scale prims uniformly, same as selecing ctrl+shift in the
        /// viewer. Used in conjunction with Scale</summary>
        Uniform = 0x10
    }

    /// <summary>
    /// Special values in PayPriceReply. If the price is not one of these
    /// literal value of the price should be use
    /// </summary>
    public enum PayPriceType : int
    {
        /// <summary>
        /// Indicates that this pay option should be hidden
        /// </summary>
        Hide = -1,
        
        /// <summary>
        /// Indicates that this pay option should have the default value
        /// </summary>
        Default = -2
    }

    #endregion Enums

    #region Structs

    /// <summary>
    /// Contains the variables sent in an object update packet for objects. 
    /// Used to track position and movement of prims and avatars
    /// </summary>
    public struct ObjectMovementUpdate
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
        public Primitive.TextureEntry Textures;
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
       
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<PrimEventArgs> m_ObjectUpdate;

        ///<summary>Raises the ObjectUpdate Event</summary>
        /// <param name="e">A ObjectUpdateEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnObjectUpdate(PrimEventArgs e)
        {
            EventHandler<PrimEventArgs> handler = m_ObjectUpdate;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ObjectUpdateLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// A Primitive, Foliage or Attachment</summary>
        public event EventHandler<PrimEventArgs> ObjectUpdate
        {
            add { lock (m_ObjectUpdateLock) { m_ObjectUpdate += value; } }
            remove { lock (m_ObjectUpdateLock) { m_ObjectUpdate -= value; } }
        }
                     
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ObjectPropertiesEventArgs> m_ObjectProperties;

        ///<summary>Raises the ObjectProperties Event</summary>
        /// <param name="e">A ObjectPropertiesEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnObjectProperties(ObjectPropertiesEventArgs e)
        {
            EventHandler<ObjectPropertiesEventArgs> handler = m_ObjectProperties;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ObjectPropertiesLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<ObjectPropertiesEventArgs> ObjectProperties
        {
            add { lock (m_ObjectPropertiesLock) { m_ObjectProperties += value; } }
            remove { lock (m_ObjectPropertiesLock) { m_ObjectProperties -= value; } }
        }
       
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ObjectPropertiesUpdatedEventArgs> m_ObjectPropertiesUpdated;

        ///<summary>Raises the ObjectPropertiesUpdated Event</summary>
        /// <param name="e">A ObjectPropertiesUpdatedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnObjectPropertiesUpdated(ObjectPropertiesUpdatedEventArgs e)
        {
            EventHandler<ObjectPropertiesUpdatedEventArgs> handler = m_ObjectPropertiesUpdated;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ObjectPropertiesUpdatedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// Primitive.ObjectProperties for an object we are currently tracking</summary>
        public event EventHandler<ObjectPropertiesUpdatedEventArgs> ObjectPropertiesUpdated
        {
            add { lock (m_ObjectPropertiesUpdatedLock) { m_ObjectPropertiesUpdated += value; } }
            remove { lock (m_ObjectPropertiesUpdatedLock) { m_ObjectPropertiesUpdated -= value; } }
        }
        
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ObjectPropertiesFamilyEventArgs> m_ObjectPropertiesFamily;

        ///<summary>Raises the ObjectPropertiesFamily Event</summary>
        /// <param name="e">A ObjectPropertiesFamilyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnObjectPropertiesFamily(ObjectPropertiesFamilyEventArgs e)
        {
            EventHandler<ObjectPropertiesFamilyEventArgs> handler = m_ObjectPropertiesFamily;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ObjectPropertiesFamilyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<ObjectPropertiesFamilyEventArgs> ObjectPropertiesFamily
        {
            add { lock (m_ObjectPropertiesFamilyLock) { m_ObjectPropertiesFamily += value; } }
            remove { lock (m_ObjectPropertiesFamilyLock) { m_ObjectPropertiesFamily -= value; } }
        }
        
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarUpdateEventArgs> m_AvatarUpdate;

        ///<summary>Raises the AvatarUpdate Event</summary>
        /// <param name="e">A AvatarUpdateEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarUpdate(AvatarUpdateEventArgs e)
        {
            EventHandler<AvatarUpdateEventArgs> handler = m_AvatarUpdate;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarUpdateLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<AvatarUpdateEventArgs> AvatarUpdate
        {
            add { lock (m_AvatarUpdateLock) { m_AvatarUpdate += value; } }
            remove { lock (m_AvatarUpdateLock) { m_AvatarUpdate -= value; } }
        }
                     
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<TerseObjectUpdateEventArgs> m_TerseObjectUpdate;

        ///<summary>Raises the TerseObjectUpdate Event</summary>
        /// <param name="e">A TerseObjectUpdateEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnTerseObjectUpdate(TerseObjectUpdateEventArgs e)
        {
            EventHandler<TerseObjectUpdateEventArgs> handler = m_TerseObjectUpdate;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_TerseObjectUpdateLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<TerseObjectUpdateEventArgs> TerseObjectUpdate
        {
            add { lock (m_TerseObjectUpdateLock) { m_TerseObjectUpdate += value; } }
            remove { lock (m_TerseObjectUpdateLock) { m_TerseObjectUpdate -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<ObjectDataBlockUpdateEventArgs> m_ObjectDataBlockUpdate;

        ///<summary>Raises the ObjectDataBlockUpdate Event</summary>
        /// <param name="e">A ObjectDataBlockUpdateEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnObjectDataBlockUpdate(ObjectDataBlockUpdateEventArgs e)
        {
            EventHandler<ObjectDataBlockUpdateEventArgs> handler = m_ObjectDataBlockUpdate;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_ObjectDataBlockUpdateLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<ObjectDataBlockUpdateEventArgs> ObjectDataBlockUpdate
        {
            add { lock (m_ObjectDataBlockUpdateLock) { m_ObjectDataBlockUpdate += value; } }
            remove { lock (m_ObjectDataBlockUpdateLock) { m_ObjectDataBlockUpdate -= value; } }
        }
       
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<KillObjectEventArgs> m_KillObject;

        ///<summary>Raises the KillObject Event</summary>
        /// <param name="e">A KillObjectEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnKillObject(KillObjectEventArgs e)
        {
            EventHandler<KillObjectEventArgs> handler = m_KillObject;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_KillObjectLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<KillObjectEventArgs> KillObject
        {
            add { lock (m_KillObjectLock) { m_KillObject += value; } }
            remove { lock (m_KillObjectLock) { m_KillObject -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AvatarSitChangedEventArgs> m_AvatarSitChanged;

        ///<summary>Raises the AvatarSitChanged Event</summary>
        /// <param name="e">A AvatarSitChangedEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAvatarSitChanged(AvatarSitChangedEventArgs e)
        {
            EventHandler<AvatarSitChangedEventArgs> handler = m_AvatarSitChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AvatarSitChangedLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<AvatarSitChangedEventArgs> AvatarSitChanged
        {
            add { lock (m_AvatarSitChangedLock) { m_AvatarSitChanged += value; } }
            remove { lock (m_AvatarSitChangedLock) { m_AvatarSitChanged -= value; } }
        }
        
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<PayPriceReplyEventArgs> m_PayPriceReply;

        ///<summary>Raises the PayPriceReply Event</summary>
        /// <param name="e">A PayPriceReplyEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnPayPriceReply(PayPriceReplyEventArgs e)
        {
            EventHandler<PayPriceReplyEventArgs> handler = m_PayPriceReply;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_PayPriceReplyLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<PayPriceReplyEventArgs> PayPriceReply
        {
            add { lock (m_PayPriceReplyLock) { m_PayPriceReply += value; } }
            remove { lock (m_PayPriceReplyLock) { m_PayPriceReply -= value; } }
        }

        #endregion Delegates

        /// <summary>Reference to the GridClient object</summary>
        protected GridClient Client;
        /// <summary>Does periodic dead reckoning calculation to convert
        /// velocity and acceleration to new positions for objects</summary>
        private Timer InterpolationTimer;

        /// <summary>
        /// Instantiates a new ObjectManager class
        /// </summary>
        /// <param name="client">A reference to the client</param>
        public ObjectManager(GridClient client)
        {
            Client = client;
            Client.Network.RegisterCallback(PacketType.ObjectUpdate, ObjectUpdateHandler);
            Client.Network.RegisterCallback(PacketType.ImprovedTerseObjectUpdate, ImprovedTerseObjectUpdateHandler);
            Client.Network.RegisterCallback(PacketType.ObjectUpdateCompressed, ObjectUpdateCompressedHandler);
            Client.Network.RegisterCallback(PacketType.ObjectUpdateCached, ObjectUpdateCachedHandler);
            Client.Network.RegisterCallback(PacketType.KillObject, KillObjectHandler);
            Client.Network.RegisterCallback(PacketType.ObjectPropertiesFamily, ObjectPropertiesFamilyHandler);
            Client.Network.RegisterCallback(PacketType.ObjectProperties, ObjectPropertiesHandler);
            Client.Network.RegisterCallback(PacketType.PayPriceReply, PayPriceReplyHandler);
        }
        
        private void Network_OnDisconnected(NetworkManager.DisconnectType reason, string message)
        {
            if (InterpolationTimer != null)
            {
                InterpolationTimer.Dispose();
                InterpolationTimer = null;
            }
        }

        private void Network_OnConnected(object sender)
        {
            if (Client.Settings.USE_INTERPOLATION_TIMER)
            {
                InterpolationTimer = new Timer(InterpolationTimer_Elapsed, null, Settings.INTERPOLATION_INTERVAL, Timeout.Infinite);
            }
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
        /// Request prices that should be displayed in pay dialog. This will triggger the simulator
        /// to send us back a PayPriceReply which can be handled by OnPayPriceReply event
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="objectID"><seealso cref="UUID"/> of the object we are requesting pay price</param>
        public void RequestPayPrice(Simulator simulator, UUID objectID)
        {
            RequestPayPricePacket payPriceRequest = new RequestPayPricePacket();

            payPriceRequest.ObjectData = new RequestPayPricePacket.ObjectDataBlock();
            payPriceRequest.ObjectData.ObjectID = objectID;

            Client.Network.SendPacket(payPriceRequest, simulator);
        }


        /// <summary>
        /// Select a single object. This will trigger the simulator to send us back 
        /// an ObjectProperties packet so we can get the full information for
        /// this object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="automaticDeselect">Should objects be deselected immediately after selection</param>
        public void SelectObject(Simulator simulator, uint localID, bool automaticDeselect)
        {
            ObjectSelectPacket select = new ObjectSelectPacket();

            select.AgentData.AgentID = Client.Self.AgentID;
            select.AgentData.SessionID = Client.Self.SessionID;

            select.ObjectData = new ObjectSelectPacket.ObjectDataBlock[1];
            select.ObjectData[0] = new ObjectSelectPacket.ObjectDataBlock();
            select.ObjectData[0].ObjectLocalID = localID;

            Client.Network.SendPacket(select, simulator);
            if (automaticDeselect)
            {
                DeselectObject(simulator, localID);
            }
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
            SelectObject(simulator, localID, true);
        }

        /// <summary>
        /// Select multiple objects. This will trigger the simulator to send us
        /// back ObjectProperties for each object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to select</param>
        /// <param name="automaticDeselect">Should objects be deselected immediately after selection</param>
        public void SelectObjects(Simulator simulator, uint[] localIDs, bool automaticDeselect)
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
            if (automaticDeselect)
            {
                DeselectObjects(simulator, localIDs);
            }
        }

        /// <summary>
        /// Select multiple objects. This will trigger the simulator to send us
        /// back ObjectProperties for each object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the objects reside</param>
        /// <param name="localIDs">An array which contains the IDs of the objects to select</param>
        public void SelectObjects(Simulator simulator, uint[] localIDs)
        {
            SelectObjects(simulator, localIDs, true);
        }


        /// <summary>
        /// Sets and object's flags (physical, temporary, phantom, casts shadow)
        /// </summary>
        /// <param name="localID"></param>
        /// <param name="physical"></param>
        /// <param name="temporary"></param>
        /// <param name="phantom"></param>
        /// <param name="castsShadow"></param>
        public void SetFlags(uint localID, bool physical, bool temporary, bool phantom, bool castsShadow)
        {
            ObjectFlagUpdatePacket flags = new ObjectFlagUpdatePacket();
            flags.AgentData.AgentID = Client.Self.AgentID;
            flags.AgentData.SessionID = Client.Self.SessionID;
            flags.AgentData.ObjectLocalID = localID;
            flags.AgentData.UsePhysics = physical;
            flags.AgentData.IsTemporary = temporary;
            flags.AgentData.IsPhantom = phantom;
            flags.AgentData.CastsShadows = castsShadow;

            Client.Network.SendPacket(flags);
        }

        /// <summary>
        /// Sets an object's sale information
        /// </summary>
        /// <param name="localID"></param>
        /// <param name="saleType"></param>
        /// <param name="price"></param>
        public void SetSaleInfo(uint localID, SaleType saleType, int price)
        {
            ObjectSaleInfoPacket sale = new ObjectSaleInfoPacket();
            sale.AgentData.AgentID = Client.Self.AgentID;
            sale.AgentData.SessionID = Client.Self.SessionID;
            sale.ObjectData = new ObjectSaleInfoPacket.ObjectDataBlock[1];
            sale.ObjectData[0] = new ObjectSaleInfoPacket.ObjectDataBlock();
            sale.ObjectData[0].LocalID = localID;
            sale.ObjectData[0].SalePrice = price;
            sale.ObjectData[0].SaleType = (byte)saleType;

            Client.Network.SendPacket(sale);
        }

        /// <summary>
        /// Sets sale info for multiple objects
        /// </summary>
        /// <param name="localIDs"></param>
        /// <param name="saleType"></param>
        /// <param name="price"></param>
        public void SetSaleInfo(List<uint> localIDs, SaleType saleType, int price)
        {
            ObjectSaleInfoPacket sale = new ObjectSaleInfoPacket();
            sale.AgentData.AgentID = Client.Self.AgentID;
            sale.AgentData.SessionID = Client.Self.SessionID;
            sale.ObjectData = new ObjectSaleInfoPacket.ObjectDataBlock[localIDs.Count];
            for (int i = 0; i < localIDs.Count; i++)
            {
                sale.ObjectData[i] = new ObjectSaleInfoPacket.ObjectDataBlock();
                sale.ObjectData[i].LocalID = localIDs[i];
                sale.ObjectData[i].SalePrice = price;
                sale.ObjectData[i].SaleType = (byte)saleType;
            }

            Client.Network.SendPacket(sale);
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
            ClickObject(simulator, localID, Vector3.Zero, Vector3.Zero, 0, Vector3.Zero, Vector3.Zero, Vector3.Zero);
        }

        /// <summary>
        /// Perform a click action on an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="uvCoord"></param>
        /// <param name="stCoord"></param>
        /// <param name="faceIndex"></param>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <param name="binormal"></param>
        public void ClickObject(Simulator simulator, uint localID, Vector3 uvCoord, Vector3 stCoord, int faceIndex, Vector3 position,
            Vector3 normal, Vector3 binormal)
        {
            ObjectGrabPacket grab = new ObjectGrabPacket();
            grab.AgentData.AgentID = Client.Self.AgentID;
            grab.AgentData.SessionID = Client.Self.SessionID;
            grab.ObjectData.GrabOffset = Vector3.Zero;
            grab.ObjectData.LocalID = localID;
            grab.SurfaceInfo = new ObjectGrabPacket.SurfaceInfoBlock[1];
            grab.SurfaceInfo[0] = new ObjectGrabPacket.SurfaceInfoBlock();
            grab.SurfaceInfo[0].UVCoord = uvCoord;
            grab.SurfaceInfo[0].STCoord = stCoord;
            grab.SurfaceInfo[0].FaceIndex = faceIndex;
            grab.SurfaceInfo[0].Position = position;
            grab.SurfaceInfo[0].Normal = normal;
            grab.SurfaceInfo[0].Binormal = binormal;

            Client.Network.SendPacket(grab, simulator);

            // TODO: If these hit the server out of order the click will fail 
            // and we'll be grabbing the object
            Thread.Sleep(50);

            ObjectDeGrabPacket degrab = new ObjectDeGrabPacket();
            degrab.AgentData.AgentID = Client.Self.AgentID;
            degrab.AgentData.SessionID = Client.Self.SessionID;
            degrab.ObjectData.LocalID = localID;
            degrab.SurfaceInfo = new ObjectDeGrabPacket.SurfaceInfoBlock[1];
            degrab.SurfaceInfo[0] = new ObjectDeGrabPacket.SurfaceInfoBlock();
            degrab.SurfaceInfo[0].UVCoord = uvCoord;
            degrab.SurfaceInfo[0].STCoord = stCoord;
            degrab.SurfaceInfo[0].FaceIndex = faceIndex;
            degrab.SurfaceInfo[0].Position = position;
            degrab.SurfaceInfo[0].Normal = normal;
            degrab.SurfaceInfo[0].Binormal = binormal;

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
        public void AddPrim(Simulator simulator, Primitive.ConstructionData prim, UUID groupID, Vector3 position,
            Vector3 scale, Quaternion rotation)
        {
            AddPrim(simulator, prim, groupID, position, scale, rotation, PrimFlags.CreateSelected);
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
        /// <param name="createFlags">Specify the <seealso cref="PrimFlags"/></param>
        /// <remarks>Due to the way client prim rezzing is done on the server,
        /// the requested position for an object is only close to where the prim
        /// actually ends up. If you desire exact placement you'll need to 
        /// follow up by moving the object after it has been created. This
        /// function will not set textures, light and flexible data, or other 
        /// extended primitive properties</remarks>
        public void AddPrim(Simulator simulator, Primitive.ConstructionData prim, UUID groupID, Vector3 position,
            Vector3 scale, Quaternion rotation, PrimFlags createFlags)
        {
            ObjectAddPacket packet = new ObjectAddPacket();

            packet.AgentData.AgentID = Client.Self.AgentID;
            packet.AgentData.SessionID = Client.Self.SessionID;
            packet.AgentData.GroupID = groupID;

            packet.ObjectData.State = prim.State;
            packet.ObjectData.AddFlags = (uint)createFlags;
            packet.ObjectData.PCode = (byte)PCode.Prim;

            packet.ObjectData.Material = (byte)prim.Material;
            packet.ObjectData.Scale = scale;
            packet.ObjectData.Rotation = rotation;

            packet.ObjectData.PathCurve = (byte)prim.PathCurve;
            packet.ObjectData.PathBegin = Primitive.PackBeginCut(prim.PathBegin);
            packet.ObjectData.PathEnd = Primitive.PackEndCut(prim.PathEnd);
            packet.ObjectData.PathRadiusOffset = Primitive.PackPathTwist(prim.PathRadiusOffset);
            packet.ObjectData.PathRevolutions = Primitive.PackPathRevolutions(prim.PathRevolutions);
            packet.ObjectData.PathScaleX = Primitive.PackPathScale(prim.PathScaleX);
            packet.ObjectData.PathScaleY = Primitive.PackPathScale(prim.PathScaleY);
            packet.ObjectData.PathShearX = (byte)Primitive.PackPathShear(prim.PathShearX);
            packet.ObjectData.PathShearY = (byte)Primitive.PackPathShear(prim.PathShearY);
            packet.ObjectData.PathSkew = Primitive.PackPathTwist(prim.PathSkew);
            packet.ObjectData.PathTaperX = Primitive.PackPathTaper(prim.PathTaperX);
            packet.ObjectData.PathTaperY = Primitive.PackPathTaper(prim.PathTaperY);
            packet.ObjectData.PathTwist = Primitive.PackPathTwist(prim.PathTwist);
            packet.ObjectData.PathTwistBegin = Primitive.PackPathTwist(prim.PathTwistBegin);

            packet.ObjectData.ProfileCurve = prim.profileCurve;
            packet.ObjectData.ProfileBegin = Primitive.PackBeginCut(prim.ProfileBegin);
            packet.ObjectData.ProfileEnd = Primitive.PackEndCut(prim.ProfileEnd);
            packet.ObjectData.ProfileHollow = Primitive.PackProfileHollow(prim.ProfileHollow);

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
        public void SetTextures(Simulator simulator, uint localID, Primitive.TextureEntry textures)
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
        public void SetTextures(Simulator simulator, uint localID, Primitive.TextureEntry textures, string mediaUrl)
        {
            ObjectImagePacket image = new ObjectImagePacket();

            image.AgentData.AgentID = Client.Self.AgentID;
            image.AgentData.SessionID = Client.Self.SessionID;
            image.ObjectData = new ObjectImagePacket.ObjectDataBlock[1];
            image.ObjectData[0] = new ObjectImagePacket.ObjectDataBlock();
            image.ObjectData[0].ObjectLocalID = localID;
            image.ObjectData[0].TextureEntry = textures.GetBytes();
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
            extra.ObjectData[0].ParamType = (byte)ExtraParamType.Light;
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
            extra.ObjectData[0].ParamType = (byte)ExtraParamType.Flexible;
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
            extra.ObjectData[0].ParamType = (byte)ExtraParamType.Sculpt;
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
        public void SetExtraParamOff(Simulator simulator, uint localID, ExtraParamType type)
        {
            ObjectExtraParamsPacket extra = new ObjectExtraParamsPacket();

            extra.AgentData.AgentID = Client.Self.AgentID;
            extra.AgentData.SessionID = Client.Self.SessionID;
            extra.ObjectData = new ObjectExtraParamsPacket.ObjectDataBlock[1];
            extra.ObjectData[0] = new ObjectExtraParamsPacket.ObjectDataBlock();
            extra.ObjectData[0].ObjectLocalID = localID;
            extra.ObjectData[0].ParamType = (byte)type;
            extra.ObjectData[0].ParamInUse = false;
            extra.ObjectData[0].ParamData = Utils.EmptyBytes;
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
        /// Drop an attached object from this avatar
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/>
        /// object where the objects reside. This will always be the simulator the avatar is currently in
        /// </param>
        /// <param name="localID">The object's ID which is local to the simulator the object is in</param>
        public void DropObject(Simulator simulator, uint localID)
        {
            ObjectDropPacket dropit = new ObjectDropPacket();
            dropit.AgentData.AgentID = Client.Self.AgentID;
            dropit.AgentData.SessionID = Client.Self.SessionID;
            dropit.ObjectData = new ObjectDropPacket.ObjectDataBlock[1];
            dropit.ObjectData[0] = new ObjectDropPacket.ObjectDataBlock();
            dropit.ObjectData[0].ObjectLocalID = localID;

            Client.Network.SendPacket(dropit, simulator);
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

        /// <summary>
        /// Set the ownership of a list of objects to the specified group
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the objects reside</param>
        /// <param name="localIds">An array which contains the IDs of the objects to set the group id on</param>
        /// <param name="groupID">The Groups ID</param>
        public void SetObjectsGroup(Simulator simulator, List<uint> localIds, UUID groupID)
        {
            ObjectGroupPacket packet = new ObjectGroupPacket();
            packet.AgentData.AgentID = Client.Self.AgentID;
            packet.AgentData.GroupID = groupID;
            packet.AgentData.SessionID = Client.Self.SessionID;

            packet.ObjectData = new ObjectGroupPacket.ObjectDataBlock[localIds.Count];
            for (int i = 0; i < localIds.Count; i++)
            {
                packet.ObjectData[i] = new ObjectGroupPacket.ObjectDataBlock();
                packet.ObjectData[i].ObjectLocalID = localIds[i];
            }

            Client.Network.SendPacket(packet, simulator);
        }
        #endregion

        #region Packet Handlers

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ObjectUpdateHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

            ObjectUpdatePacket update = (ObjectUpdatePacket)packet;
            UpdateDilation(e.Simulator, update.RegionData.TimeDilation);
            
            for (int b = 0; b < update.ObjectData.Length; b++)
            {
                ObjectUpdatePacket.ObjectDataBlock block = update.ObjectData[b];

                ObjectMovementUpdate objectupdate = new ObjectMovementUpdate();
                //Vector4 collisionPlane = Vector4.Zero;
                //Vector3 position;
                //Vector3 velocity;
                //Vector3 acceleration;
                //Quaternion rotation;
                //Vector3 angularVelocity;
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
                        case PCode.Prim:
                            if (m_ObjectUpdate == null) continue;
                            break;
                        case PCode.Avatar:
                            // Make an exception for updates about our own agent
                            if (block.FullID != Client.Self.AgentID && m_AvatarUpdate == null) continue;
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
                Primitive.ConstructionData data = new Primitive.ConstructionData();
                data.State = block.State;
                data.Material = (Material)block.Material;
                data.PathCurve = (PathCurve)block.PathCurve;
                data.profileCurve = block.ProfileCurve;
                data.PathBegin = Primitive.UnpackBeginCut(block.PathBegin);
                data.PathEnd = Primitive.UnpackEndCut(block.PathEnd);
                data.PathScaleX = Primitive.UnpackPathScale(block.PathScaleX);
                data.PathScaleY = Primitive.UnpackPathScale(block.PathScaleY);
                data.PathShearX = Primitive.UnpackPathShear((sbyte)block.PathShearX);
                data.PathShearY = Primitive.UnpackPathShear((sbyte)block.PathShearY);
                data.PathTwist = Primitive.UnpackPathTwist(block.PathTwist);
                data.PathTwistBegin = Primitive.UnpackPathTwist(block.PathTwistBegin);
                data.PathRadiusOffset = Primitive.UnpackPathTwist(block.PathRadiusOffset);
                data.PathTaperX = Primitive.UnpackPathTaper(block.PathTaperX);
                data.PathTaperY = Primitive.UnpackPathTaper(block.PathTaperY);
                data.PathRevolutions = Primitive.UnpackPathRevolutions(block.PathRevolutions);
                data.PathSkew = Primitive.UnpackPathTwist(block.PathSkew);
                data.ProfileBegin = Primitive.UnpackBeginCut(block.ProfileBegin);
                data.ProfileEnd = Primitive.UnpackEndCut(block.ProfileEnd);
                data.ProfileHollow = Primitive.UnpackProfileHollow(block.ProfileHollow);
                data.PCode = pcode;
                #endregion

                #region Decode Additional packed parameters in ObjectData
                int pos = 0;
                switch (block.ObjectData.Length)
                {
                    case 76:
                        // Collision normal for avatar
                        objectupdate.CollisionPlane = new Vector4(block.ObjectData, pos);
                        pos += 16;

                        goto case 60;
                    case 60:
                        // Position
                        objectupdate.Position = new Vector3(block.ObjectData, pos);
                        pos += 12;
                        // Velocity
                        objectupdate.Velocity = new Vector3(block.ObjectData, pos);
                        pos += 12;
                        // Acceleration
                        objectupdate.Acceleration = new Vector3(block.ObjectData, pos);
                        pos += 12;
                        // Rotation (theta)
                        objectupdate.Rotation = new Quaternion(block.ObjectData, pos, true);
                        pos += 12;
                        // Angular velocity (omega)
                        objectupdate.AngularVelocity = new Vector3(block.ObjectData, pos);
                        pos += 12;

                        break;
                    case 48:
                        // Collision normal for avatar
                        objectupdate.CollisionPlane = new Vector4(block.ObjectData, pos);
                        pos += 16;

                        goto case 32;
                    case 32:
                        // The data is an array of unsigned shorts

                        // Position
                        objectupdate.Position = new Vector3(
                            Utils.UInt16ToFloat(block.ObjectData, pos, -0.5f * 256.0f, 1.5f * 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 2, -0.5f * 256.0f, 1.5f * 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 3.0f * 256.0f));
                        pos += 6;
                        // Velocity
                        objectupdate.Velocity = new Vector3(
                            Utils.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;
                        // Acceleration
                        objectupdate.Acceleration = new Vector3(
                            Utils.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;
                        // Rotation (theta)
                        objectupdate.Rotation = new Quaternion(
                            Utils.UInt16ToFloat(block.ObjectData, pos, -1.0f, 1.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 2, -1.0f, 1.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 4, -1.0f, 1.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 6, -1.0f, 1.0f));
                        pos += 8;
                        // Angular velocity (omega)
                        objectupdate.AngularVelocity = new Vector3(
                            Utils.UInt16ToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f),
                            Utils.UInt16ToFloat(block.ObjectData, pos + 4, -256.0f, 256.0f));
                        pos += 6;

                        break;
                    case 16:
                        // The data is an array of single bytes (8-bit numbers)

                        // Position
                        objectupdate.Position = new Vector3(
                            Utils.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Velocity
                        objectupdate.Velocity = new Vector3(
                            Utils.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Accleration
                        objectupdate.Acceleration = new Vector3(
                            Utils.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
                        pos += 3;
                        // Rotation
                        objectupdate.Rotation = new Quaternion(
                            Utils.ByteToFloat(block.ObjectData, pos, -1.0f, 1.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 1, -1.0f, 1.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 2, -1.0f, 1.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 3, -1.0f, 1.0f));
                        pos += 4;
                        // Angular Velocity
                        objectupdate.AngularVelocity = new Vector3(
                            Utils.ByteToFloat(block.ObjectData, pos, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 1, -256.0f, 256.0f),
                            Utils.ByteToFloat(block.ObjectData, pos + 2, -256.0f, 256.0f));
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

                        // Textures
                        objectupdate.Textures = new Primitive.TextureEntry(block.TextureEntry, 0,
                            block.TextureEntry.Length);
                                                
                        OnObjectDataBlockUpdate(new ObjectDataBlockUpdateEventArgs(simulator, prim, data, block, objectupdate, nameValues));

                        #region Update Prim Info with decoded data
                        prim.Flags = (PrimFlags)block.UpdateFlags;

                        if ((prim.Flags & PrimFlags.ZlibCompressed) != 0)
                        {
                            Logger.Log("Got a ZlibCompressed ObjectUpdate, implement me!",
                                Helpers.LogLevel.Warning, Client);
                            continue;
                        }

                        // Automatically request ObjectProperties for prim if it was rezzed selected.
                        if ((prim.Flags & PrimFlags.CreateSelected) != 0)
                        {
                            SelectObject(simulator, prim.LocalID);
                        }

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
                        prim.SoundFlags = (SoundFlags)block.Flags;
                        prim.SoundGain = block.Gain;
                        prim.SoundRadius = block.Radius;

                        // Joint information
                        prim.Joint = (JointType)block.JointType;
                        prim.JointPivot = block.JointPivot;
                        prim.JointAxisOrAnchor = block.JointAxisOrAnchor;

                        // Object parameters
                        prim.PrimData = data;

                        // Textures, texture animations, particle system, and extra params
                        prim.Textures = objectupdate.Textures;

                        prim.TextureAnim = new Primitive.TextureAnimation(block.TextureAnim, 0);
                        prim.ParticleSys = new Primitive.ParticleSystem(block.PSBlock, 0);
                        prim.SetExtraParamsFromBytes(block.ExtraParams, 0);

                        // PCode-specific data
                        switch (pcode)
                        {
                            case PCode.Grass:
                            case PCode.Tree:
                            case PCode.NewTree:
                                if (block.Data.Length == 1)
                                    prim.TreeSpecies = (Tree)block.Data[0];
                                else
                                    Logger.Log("Got a foliage update with an invalid TreeSpecies field", Helpers.LogLevel.Warning);
                                prim.ScratchPad = Utils.EmptyBytes;
                                break;
                            default:
                                prim.ScratchPad = new byte[block.Data.Length];
                                if (block.Data.Length > 0)
                                    Buffer.BlockCopy(block.Data, 0, prim.ScratchPad, 0, prim.ScratchPad.Length);
                                break;
                        }

                        // Packed parameters
                        prim.CollisionPlane = objectupdate.CollisionPlane;
                        prim.Position = objectupdate.Position;
                        prim.Velocity = objectupdate.Velocity;
                        prim.Acceleration = objectupdate.Acceleration;
                        prim.Rotation = objectupdate.Rotation;
                        prim.AngularVelocity = objectupdate.AngularVelocity;
                        #endregion

                        OnObjectUpdate(new PrimEventArgs(simulator, prim, update.RegionData.TimeDilation, attachment));                        

                        break;
                    #endregion Prim and Foliage
                    #region Avatar
                    case PCode.Avatar:
                        // Update some internals if this is our avatar
                        if (block.FullID == Client.Self.AgentID && simulator == Client.Network.CurrentSim)
                        {
                            #region Update Client.Self

                            // We need the local ID to recognize terse updates for our agent
                            Client.Self.localID = block.ID;

                            // Packed parameters
                            Client.Self.collisionPlane = objectupdate.CollisionPlane;
                            Client.Self.relativePosition = objectupdate.Position;
                            Client.Self.velocity = objectupdate.Velocity;
                            Client.Self.acceleration = objectupdate.Acceleration;
                            Client.Self.relativeRotation = objectupdate.Rotation;
                            Client.Self.angularVelocity = objectupdate.AngularVelocity;

                            #endregion
                        }

                        #region Create an Avatar from the decoded data

                        Avatar avatar = GetAvatar(simulator, block.ID, block.FullID);

                        objectupdate.Avatar = true;
                        // Textures
                        objectupdate.Textures = new Primitive.TextureEntry(block.TextureEntry, 0,                 
                            block.TextureEntry.Length);

                        OnObjectDataBlockUpdate(new ObjectDataBlockUpdateEventArgs(simulator, avatar, data, block, objectupdate, nameValues));

                        uint oldSeatID = avatar.ParentID;

                        avatar.ID = block.FullID;
                        avatar.LocalID = block.ID;
                        avatar.CollisionPlane = objectupdate.CollisionPlane;
                        avatar.Position = objectupdate.Position;
                        avatar.Velocity = objectupdate.Velocity;
                        avatar.Acceleration = objectupdate.Acceleration;
                        avatar.Rotation = objectupdate.Rotation;
                        avatar.AngularVelocity = objectupdate.AngularVelocity;
                        avatar.NameValues = nameValues;
                        avatar.PrimData = data;
                        if (block.Data.Length > 0)
                        {
                            Logger.Log("Unexpected Data field for an avatar update, length " + block.Data.Length, Helpers.LogLevel.Warning);
                        }
                        avatar.ParentID = block.ParentID;
                        avatar.RegionHandle = update.RegionData.RegionHandle;

                        SetAvatarSittingOn(simulator, avatar, block.ParentID, oldSeatID);

                        // Textures
                        avatar.Textures = objectupdate.Textures;

                        #endregion Create an Avatar from the decoded data

                        OnAvatarUpdate(new AvatarUpdateEventArgs(simulator, avatar, update.RegionData.TimeDilation));

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
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ImprovedTerseObjectUpdateHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

            ImprovedTerseObjectUpdatePacket terse = (ImprovedTerseObjectUpdatePacket)packet;
            UpdateDilation(simulator, terse.RegionData.TimeDilation);

            for (int i = 0; i < terse.ObjectData.Length; i++)
            {
                ImprovedTerseObjectUpdatePacket.ObjectDataBlock block = terse.ObjectData[i];

                try
                {
                    int pos = 4;
                    uint localid = Utils.BytesToUInt(block.Data, 0);

                    // Check if we are interested in this update
                    if (!Client.Settings.ALWAYS_DECODE_OBJECTS
                        && localid != Client.Self.localID
                        && m_TerseObjectUpdate == null)
                    {
                        continue;
                    }

                    #region Decode update data

                    ObjectMovementUpdate update = new ObjectMovementUpdate();

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
                        Utils.UInt16ToFloat(block.Data, pos, -128.0f, 128.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 2, -128.0f, 128.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 4, -128.0f, 128.0f));
                    pos += 6;
                    // Acceleration
                    update.Acceleration = new Vector3(
                        Utils.UInt16ToFloat(block.Data, pos, -64.0f, 64.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 2, -64.0f, 64.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 4, -64.0f, 64.0f));
                    pos += 6;
                    // Rotation (theta)
                    update.Rotation = new Quaternion(
                        Utils.UInt16ToFloat(block.Data, pos, -1.0f, 1.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 2, -1.0f, 1.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 4, -1.0f, 1.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 6, -1.0f, 1.0f));
                    pos += 8;
                    // Angular velocity (omega)
                    update.AngularVelocity = new Vector3(
                        Utils.UInt16ToFloat(block.Data, pos, -64.0f, 64.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 2, -64.0f, 64.0f),
                        Utils.UInt16ToFloat(block.Data, pos + 4, -64.0f, 64.0f));
                    pos += 6;

                    // Textures
                    // FIXME: Why are we ignoring the first four bytes here?
                    if (block.TextureEntry.Length != 0)
                        update.Textures = new Primitive.TextureEntry(block.TextureEntry, 4, block.TextureEntry.Length - 4);

                    #endregion Decode update data

                    Primitive obj = (update.Avatar) ?
                        (Primitive)GetAvatar(simulator, update.LocalID, UUID.Zero) :
                        (Primitive)GetPrimitive(simulator, update.LocalID, UUID.Zero);

                    // Fire the pre-emptive notice (before we stomp the object)
                    OnTerseObjectUpdate(new TerseObjectUpdateEventArgs(simulator, obj, update, terse.RegionData.TimeDilation));                    

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
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, Helpers.LogLevel.Warning, Client, ex);
                }
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ObjectUpdateCompressedHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

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
                            case PCode.Prim:
                                if (m_ObjectUpdate == null) continue;
                                break;
                        }
                    }

                    #endregion Relevance check

                    Primitive prim = GetPrimitive(simulator, LocalID, FullID);

                    prim.LocalID = LocalID;
                    prim.ID = FullID;
                    prim.Flags = (PrimFlags)block.UpdateFlags;
                    prim.PrimData.PCode = pcode;

                    #region Decode block and update Prim

                    // State
                    prim.PrimData.State = block.Data[i++];
                    // CRC
                    i += 4;
                    // Material
                    prim.PrimData.Material = (Material)block.Data[i++];
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
                    CompressedFlags flags = (CompressedFlags)Utils.BytesToUInt(block.Data, i);
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
                        prim.TreeSpecies = (Tree)block.Data[i++];
                        prim.ScratchPad = Utils.EmptyBytes;
                    }
                    // Scratch pad
                    else if ((flags & CompressedFlags.ScratchPad) != 0)
                    {
                        prim.TreeSpecies = (Tree)0;

                        int size = block.Data[i++];
                        prim.ScratchPad = new byte[size];
                        Buffer.BlockCopy(block.Data, i, prim.ScratchPad, 0, size);
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

                        prim.SoundGain = Utils.BytesToFloat(block.Data, i);
                        i += 4;
                        prim.SoundFlags = (SoundFlags)block.Data[i++];
                        prim.SoundRadius = Utils.BytesToFloat(block.Data, i);
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

                    prim.PrimData.PathCurve = (PathCurve)block.Data[i++];
                    ushort pathBegin = Utils.BytesToUInt16(block.Data, i); i += 2;
                    prim.PrimData.PathBegin = Primitive.UnpackBeginCut(pathBegin);
                    ushort pathEnd = Utils.BytesToUInt16(block.Data, i); i += 2;
                    prim.PrimData.PathEnd = Primitive.UnpackEndCut(pathEnd);
                    prim.PrimData.PathScaleX = Primitive.UnpackPathScale(block.Data[i++]);
                    prim.PrimData.PathScaleY = Primitive.UnpackPathScale(block.Data[i++]);
                    prim.PrimData.PathShearX = Primitive.UnpackPathShear((sbyte)block.Data[i++]);
                    prim.PrimData.PathShearY = Primitive.UnpackPathShear((sbyte)block.Data[i++]);
                    prim.PrimData.PathTwist = Primitive.UnpackPathTwist((sbyte)block.Data[i++]);
                    prim.PrimData.PathTwistBegin = Primitive.UnpackPathTwist((sbyte)block.Data[i++]);
                    prim.PrimData.PathRadiusOffset = Primitive.UnpackPathTwist((sbyte)block.Data[i++]);
                    prim.PrimData.PathTaperX = Primitive.UnpackPathTaper((sbyte)block.Data[i++]);
                    prim.PrimData.PathTaperY = Primitive.UnpackPathTaper((sbyte)block.Data[i++]);
                    prim.PrimData.PathRevolutions = Primitive.UnpackPathRevolutions(block.Data[i++]);
                    prim.PrimData.PathSkew = Primitive.UnpackPathTwist((sbyte)block.Data[i++]);

                    prim.PrimData.profileCurve = block.Data[i++];
                    ushort profileBegin = Utils.BytesToUInt16(block.Data, i); i += 2;
                    prim.PrimData.ProfileBegin = Primitive.UnpackBeginCut(profileBegin);
                    ushort profileEnd = Utils.BytesToUInt16(block.Data, i); i += 2;
                    prim.PrimData.ProfileEnd = Primitive.UnpackEndCut(profileEnd);
                    ushort profileHollow = Utils.BytesToUInt16(block.Data, i); i += 2;
                    prim.PrimData.ProfileHollow = Primitive.UnpackProfileHollow(profileHollow);

                    // TextureEntry
                    int textureEntryLength = (int)Utils.BytesToUInt(block.Data, i);
                    i += 4;
                    prim.Textures = new Primitive.TextureEntry(block.Data, i, textureEntryLength);
                    i += textureEntryLength;

                    // Texture animation
                    if ((flags & CompressedFlags.TextureAnimation) != 0)
                    {
                        //int textureAnimLength = (int)Utils.BytesToUIntBig(block.Data, i);
                        i += 4;
                        prim.TextureAnim = new Primitive.TextureAnimation(block.Data, i);
                    }

                    #endregion

                    #region Raise Events

                    if ((flags & CompressedFlags.HasNameValues) != 0 && prim.ParentID != 0)
                    {
                        OnObjectUpdate(new PrimEventArgs(simulator, prim, update.RegionData.TimeDilation, true));
                    }
                    else
                    {
                        OnObjectUpdate(new PrimEventArgs(simulator, prim, update.RegionData.TimeDilation, false));
                    }

                    #endregion
                }
                catch (IndexOutOfRangeException ex)
                {
                    Logger.Log("Error decoding an ObjectUpdateCompressed packet", Helpers.LogLevel.Warning, Client, ex);
                    Logger.Log(block, Helpers.LogLevel.Warning);
                }
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ObjectUpdateCachedHandler(object sender, PacketReceivedEventArgs e)
        {
            if (Client.Settings.ALWAYS_REQUEST_OBJECTS)
            {
                Packet packet = e.Packet;
                Simulator simulator = e.Simulator;

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

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void KillObjectHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

            KillObjectPacket kill = (KillObjectPacket)packet;

            // Notify first, so that handler has a chance to get a
            // reference from the ObjectTracker to the object being killed
            for (int i = 0; i < kill.ObjectData.Length; i++)
            {
                OnKillObject(new KillObjectEventArgs(simulator, kill.ObjectData[i].ID));
            }
                

            lock (simulator.ObjectsPrimitives.Dictionary)
            {
                List<uint> removeAvatars = new List<uint>();
                List<uint> removePrims = new List<uint>();

                if (Client.Settings.OBJECT_TRACKING)
                {
                    uint localID;
                    for (int i = 0; i < kill.ObjectData.Length; i++)
                    {
                        localID = kill.ObjectData[i].ID;

                        if (simulator.ObjectsPrimitives.Dictionary.ContainsKey(localID))
                            removePrims.Add(localID);

                        foreach (KeyValuePair<uint, Primitive> prim in simulator.ObjectsPrimitives.Dictionary)
                        {
                            if (prim.Value.ParentID == localID)
                            {
                                OnKillObject(new KillObjectEventArgs(simulator, prim.Key));                                
                                removePrims.Add(prim.Key);
                            }
                        }
                    }
                }

                if (Client.Settings.AVATAR_TRACKING)
                {
                    lock (simulator.ObjectsAvatars.Dictionary)
                    {
                        uint localID;
                        for (int i = 0; i < kill.ObjectData.Length; i++)
                        {
                            localID = kill.ObjectData[i].ID;

                            if (simulator.ObjectsAvatars.Dictionary.ContainsKey(localID))
                                removeAvatars.Add(localID);

                            List<uint> rootPrims = new List<uint>();

                            foreach (KeyValuePair<uint, Primitive> prim in simulator.ObjectsPrimitives.Dictionary)
                            {
                                if (prim.Value.ParentID == localID)
                                {
                                    OnKillObject(new KillObjectEventArgs(simulator, prim.Key));                                    
                                    removePrims.Add(prim.Key);
                                    rootPrims.Add(prim.Key);
                                }
                            }

                            foreach (KeyValuePair<uint, Primitive> prim in simulator.ObjectsPrimitives.Dictionary)
                            {
                                if (rootPrims.Contains(prim.Value.ParentID))
                                {
                                    OnKillObject(new KillObjectEventArgs(simulator, prim.Key));                                    
                                    removePrims.Add(prim.Key);
                                }
                            }
                        }

                        //Do the actual removing outside of the loops but still inside the lock.
                        //This safely prevents the collection from being modified during a loop.
                        foreach (uint removeID in removeAvatars)
                            simulator.ObjectsAvatars.Dictionary.Remove(removeID);
                    }
                }

                foreach (uint removeID in removePrims)
                    simulator.ObjectsPrimitives.Dictionary.Remove(removeID);
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ObjectPropertiesHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

            ObjectPropertiesPacket op = (ObjectPropertiesPacket)packet;
            ObjectPropertiesPacket.ObjectDataBlock[] datablocks = op.ObjectData;

            for (int i = 0; i < datablocks.Length; ++i)
            {
                ObjectPropertiesPacket.ObjectDataBlock objectData = datablocks[i];
                Primitive.ObjectProperties props = new Primitive.ObjectProperties();

                props.ObjectID = objectData.ObjectID;
                props.AggregatePerms = objectData.AggregatePerms;
                props.AggregatePermTextures = objectData.AggregatePermTextures;
                props.AggregatePermTexturesOwner = objectData.AggregatePermTexturesOwner;
                props.Permissions = new Permissions(objectData.BaseMask, objectData.EveryoneMask, objectData.GroupMask,
                    objectData.NextOwnerMask, objectData.OwnerMask);
                props.Category = (ObjectCategory)objectData.Category;                                    
                props.CreationDate = Utils.UnixTimeToDateTime((uint)objectData.CreationDate);
                props.CreatorID = objectData.CreatorID;
                props.Description = Utils.BytesToString(objectData.Description);
                props.FolderID = objectData.FolderID;
                props.FromTaskID = objectData.FromTaskID;
                props.GroupID = objectData.GroupID;
                props.InventorySerial = objectData.InventorySerial;
                props.ItemID = objectData.ItemID;
                props.LastOwnerID = objectData.LastOwnerID;
                props.Name = Utils.BytesToString(objectData.Name);
                props.OwnerID = objectData.OwnerID;
                props.OwnershipCost = objectData.OwnershipCost;
                props.SalePrice = objectData.SalePrice;
                props.SaleType = (SaleType)objectData.SaleType;
                props.SitName = Utils.BytesToString(objectData.SitName);
                props.TouchName = Utils.BytesToString(objectData.TouchName);

                int numTextures = objectData.TextureID.Length / 16;
                props.TextureIDs = new UUID[numTextures];
                for (int j = 0; j < numTextures; ++j)
                    props.TextureIDs[j] = new UUID(objectData.TextureID, j * 16);

                if (Client.Settings.OBJECT_TRACKING)
                {
                    Primitive findPrim = simulator.ObjectsPrimitives.Find(
                        delegate(Primitive prim) { return prim.ID == props.ObjectID; });

                    if (findPrim != null)
                    {
                        OnObjectPropertiesUpdated(new ObjectPropertiesUpdatedEventArgs(simulator, findPrim, props));

                        lock (simulator.ObjectsPrimitives.Dictionary)
                        {
                            if (simulator.ObjectsPrimitives.Dictionary.ContainsKey(findPrim.LocalID))
                                simulator.ObjectsPrimitives.Dictionary[findPrim.LocalID].Properties = props;
                        }
                    }
                }

                OnObjectProperties(new ObjectPropertiesEventArgs(simulator, props));
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void ObjectPropertiesFamilyHandler(object sender, PacketReceivedEventArgs e)
        {
            Packet packet = e.Packet;
            Simulator simulator = e.Simulator;

            ObjectPropertiesFamilyPacket op = (ObjectPropertiesFamilyPacket)packet;
            Primitive.ObjectProperties props = new Primitive.ObjectProperties();

            ReportType requestType = (ReportType)op.ObjectData.RequestFlags;

            props.ObjectID = op.ObjectData.ObjectID;
            props.Category = (ObjectCategory)op.ObjectData.Category;
            props.Description = Utils.BytesToString(op.ObjectData.Description);
            props.GroupID = op.ObjectData.GroupID;
            props.LastOwnerID = op.ObjectData.LastOwnerID;
            props.Name = Utils.BytesToString(op.ObjectData.Name);
            props.OwnerID = op.ObjectData.OwnerID;
            props.OwnershipCost = op.ObjectData.OwnershipCost;
            props.SalePrice = op.ObjectData.SalePrice;
            props.SaleType = (SaleType)op.ObjectData.SaleType;
            props.Permissions.BaseMask = (PermissionMask)op.ObjectData.BaseMask;
            props.Permissions.EveryoneMask = (PermissionMask)op.ObjectData.EveryoneMask;
            props.Permissions.GroupMask = (PermissionMask)op.ObjectData.GroupMask;
            props.Permissions.NextOwnerMask = (PermissionMask)op.ObjectData.NextOwnerMask;
            props.Permissions.OwnerMask = (PermissionMask)op.ObjectData.OwnerMask;

            if (Client.Settings.OBJECT_TRACKING)
            {
                Primitive findPrim = simulator.ObjectsPrimitives.Find(
                        delegate(Primitive prim) { return prim.ID == op.ObjectData.ObjectID; });

                if (findPrim != null)
                {
                    lock (simulator.ObjectsPrimitives.Dictionary)
                    {
                        if (simulator.ObjectsPrimitives.Dictionary.ContainsKey(findPrim.LocalID))
                        {
                            if (simulator.ObjectsPrimitives.Dictionary[findPrim.LocalID].Properties == null)
                                simulator.ObjectsPrimitives.Dictionary[findPrim.LocalID].Properties = new Primitive.ObjectProperties();
                            simulator.ObjectsPrimitives.Dictionary[findPrim.LocalID].Properties.SetFamilyProperties(props);
                        }
                    }
                }
            }

            OnObjectPropertiesFamily(new ObjectPropertiesFamilyEventArgs(simulator, props, requestType));
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void PayPriceReplyHandler(object sender, PacketReceivedEventArgs e)
        {
            if (m_PayPriceReply != null)
            {
                Packet packet = e.Packet;
                Simulator simulator = e.Simulator;

                PayPriceReplyPacket p = (PayPriceReplyPacket)packet;
                UUID objectID = p.ObjectData.ObjectID;
                int defaultPrice = p.ObjectData.DefaultPayPrice;
                int[] buttonPrices = new int[p.ButtonData.Length];

                for (int i = 0; i < p.ButtonData.Length; i++)
                {
                    buttonPrices[i] = p.ButtonData[i].PayButton;
                }

                OnPayPriceReply(new PayPriceReplyEventArgs(simulator, objectID, defaultPrice, buttonPrices));
            }
        }

        #endregion Packet Handlers

        #region Utility Functions

        /// <summary>
        /// Setup construction data for a basic primitive shape
        /// </summary>
        /// <param name="type">Primitive shape to construct</param>
        /// <returns>Construction data that can be plugged into a <seealso cref="Primitive"/></returns>
        public static Primitive.ConstructionData BuildBasicShape(PrimType type)
        {
            Primitive.ConstructionData prim = new Primitive.ConstructionData();
            prim.PCode = PCode.Prim;
            prim.Material = Material.Wood;

            switch (type)
            {
                case PrimType.Box:
                    prim.ProfileCurve = ProfileCurve.Square;
                    prim.PathCurve = PathCurve.Line;
                    prim.ProfileEnd = 1f;
                    prim.PathEnd = 1f;
                    prim.PathScaleX = 1f;
                    prim.PathScaleY = 1f;
                    prim.PathRevolutions = 1f;
                    break;
                case PrimType.Cylinder:
                    prim.ProfileCurve = ProfileCurve.Circle;
                    prim.PathCurve = PathCurve.Line;
                    prim.ProfileEnd = 1f;
                    prim.PathEnd = 1f;
                    prim.PathScaleX = 1f;
                    prim.PathScaleY = 1f;
                    prim.PathRevolutions = 1f;
                    break;
                case PrimType.Prism:
                    prim.ProfileCurve = ProfileCurve.Square;
                    prim.PathCurve = PathCurve.Line;
                    prim.ProfileEnd = 1f;
                    prim.PathEnd = 1f;
                    prim.PathScaleX = 0f;
                    prim.PathScaleY = 0f;
                    prim.PathRevolutions = 1f;
                    break;
                case PrimType.Ring:
                    prim.ProfileCurve = ProfileCurve.EqualTriangle;
                    prim.PathCurve = PathCurve.Circle;
                    prim.ProfileEnd = 1f;
                    prim.PathEnd = 1f;
                    prim.PathScaleX = 1f;
                    prim.PathScaleY = 0.25f;
                    prim.PathRevolutions = 1f;
                    break;
                case PrimType.Sphere:
                    prim.ProfileCurve = ProfileCurve.HalfCircle;
                    prim.PathCurve = PathCurve.Circle;
                    prim.ProfileEnd = 1f;
                    prim.PathEnd = 1f;
                    prim.PathScaleX = 1f;
                    prim.PathScaleY = 1f;
                    prim.PathRevolutions = 1f;
                    break;
                case PrimType.Torus:
                    prim.ProfileCurve = ProfileCurve.Circle;
                    prim.PathCurve = PathCurve.Circle;
                    prim.ProfileEnd = 1f;
                    prim.PathEnd = 1f;
                    prim.PathScaleX = 1f;
                    prim.PathScaleY = 0.25f;
                    prim.PathRevolutions = 1f;
                    break;
                case PrimType.Tube:
                    prim.ProfileCurve = ProfileCurve.Square;
                    prim.PathCurve = PathCurve.Circle;
                    prim.ProfileEnd = 1f;
                    prim.PathEnd = 1f;
                    prim.PathScaleX = 1f;
                    prim.PathScaleY = 0.25f;
                    prim.PathRevolutions = 1f;
                    break;
                default:
                    throw new NotSupportedException("Unsupported shape: " + type.ToString());
            }

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
            if (Client.Network.CurrentSim == sim && av.LocalID == Client.Self.localID)
            {
                Client.Self.sittingOn = localid;
            }
            
            av.ParentID = localid;
            

            if (m_AvatarSitChanged != null && oldSeatID != localid)
            {
                OnAvatarSitChanged(new AvatarSitChangedEventArgs(sim, av, localid, oldSeatID));
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


        /// <summary>
        /// Set the Shape data of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="prim">Data describing the prim shape</param>
        public void SetShape(Simulator simulator, uint localID, Primitive.ConstructionData prim)
        {
            ObjectShapePacket shape = new ObjectShapePacket();

            shape.AgentData.AgentID = Client.Self.AgentID;
            shape.AgentData.SessionID = Client.Self.SessionID;

            shape.ObjectData = new OpenMetaverse.Packets.ObjectShapePacket.ObjectDataBlock[1];
            shape.ObjectData[0] = new OpenMetaverse.Packets.ObjectShapePacket.ObjectDataBlock();

            shape.ObjectData[0].ObjectLocalID = localID;

            shape.ObjectData[0].PathCurve = (byte)prim.PathCurve;
            shape.ObjectData[0].PathBegin = Primitive.PackBeginCut(prim.PathBegin);
            shape.ObjectData[0].PathEnd = Primitive.PackEndCut(prim.PathEnd);
            shape.ObjectData[0].PathScaleX = Primitive.PackPathScale(prim.PathScaleX);
            shape.ObjectData[0].PathScaleY = Primitive.PackPathScale(prim.PathScaleY);
            shape.ObjectData[0].PathShearX = (byte)Primitive.PackPathShear(prim.PathShearX);
            shape.ObjectData[0].PathShearY = (byte)Primitive.PackPathShear(prim.PathShearY);
            shape.ObjectData[0].PathTwist = Primitive.PackPathTwist(prim.PathTwist);
            shape.ObjectData[0].PathTwistBegin = Primitive.PackPathTwist(prim.PathTwistBegin);
            shape.ObjectData[0].PathRadiusOffset = Primitive.PackPathTwist(prim.PathRadiusOffset);
            shape.ObjectData[0].PathTaperX = Primitive.PackPathTaper(prim.PathTaperX);
            shape.ObjectData[0].PathTaperY = Primitive.PackPathTaper(prim.PathTaperY);
            shape.ObjectData[0].PathRevolutions = Primitive.PackPathRevolutions(prim.PathRevolutions);
            shape.ObjectData[0].PathSkew = Primitive.PackPathTwist(prim.PathSkew);

            shape.ObjectData[0].ProfileCurve = prim.profileCurve;
            shape.ObjectData[0].ProfileBegin = Primitive.PackBeginCut(prim.ProfileBegin);
            shape.ObjectData[0].ProfileEnd = Primitive.PackEndCut(prim.ProfileEnd);
            shape.ObjectData[0].ProfileHollow = Primitive.PackProfileHollow(prim.ProfileHollow);

            Client.Network.SendPacket(shape, simulator);
        }

        /// <summary>
        /// Set the Material data of an object
        /// </summary>
        /// <param name="simulator">A reference to the <seealso cref="OpenMetaverse.Simulator"/> object where the object resides</param>
        /// <param name="localID">The objects ID which is local to the simulator the object is in</param>
        /// <param name="material">The new material of the object</param>
        public void SetMaterial(Simulator simulator, uint localID, Material material)
        {
            ObjectMaterialPacket matPacket = new ObjectMaterialPacket();

            matPacket.AgentData.AgentID = Client.Self.AgentID;
            matPacket.AgentData.SessionID = Client.Self.SessionID;

            matPacket.ObjectData = new ObjectMaterialPacket.ObjectDataBlock[1];
            matPacket.ObjectData[0] = new ObjectMaterialPacket.ObjectDataBlock();

            matPacket.ObjectData[0].ObjectLocalID = localID;
            matPacket.ObjectData[0].Material = (byte)material;

            Client.Network.SendPacket(matPacket, simulator);
        }


        #endregion Utility Functions
        
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
                lock (simulator.ObjectsPrimitives.Dictionary)
                {

                    Primitive prim;

                    if (simulator.ObjectsPrimitives.Dictionary.TryGetValue(localID, out prim))
                    {
                        return prim;
                    }
                    else
                    {
                        prim = new Primitive();
                        prim.LocalID = localID;
                        prim.ID = fullID;
                        prim.RegionHandle = simulator.Handle;

                        simulator.ObjectsPrimitives.Dictionary[localID] = prim;

                        return prim;
                    }
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
                lock (simulator.ObjectsAvatars.Dictionary)
                {

                    Avatar avatar;

                    if (simulator.ObjectsAvatars.Dictionary.TryGetValue(localID, out avatar))
                    {
                        return avatar;
                    }
                    else
                    {
                        avatar = new Avatar();
                        avatar.LocalID = localID;
                        avatar.ID = fullID;
                        avatar.RegionHandle = simulator.Handle;

                        simulator.ObjectsAvatars.Dictionary[localID] = avatar;

                        return avatar;
                    }
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
            int elapsed = 0;

            if (Client.Network.Connected)
            {
                int start = Environment.TickCount;

                int interval = Environment.TickCount - Client.Self.lastInterpolation;
                float seconds = (float)interval / 1000f;

                // Iterate through all of the simulators
                Simulator[] sims = Client.Network.Simulators.ToArray();
                for (int i = 0; i < sims.Length; i++)
                {
                    Simulator sim = sims[i];

                    float adjSeconds = seconds * sim.Stats.Dilation;

                    // Iterate through all of this sims avatars
                    sim.ObjectsAvatars.ForEach(
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
                    sim.ObjectsPrimitives.ForEach(
                        delegate(Primitive prim)
                        {
                            if (prim.Joint == JointType.Invalid)
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
                            else if (prim.Joint == JointType.Hinge)
                            {
                                //FIXME: Hinge movement extrapolation
                            }
                            else if (prim.Joint == JointType.Point)
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

                // Make sure the last interpolated time is always updated
                Client.Self.lastInterpolation = Environment.TickCount;

                elapsed = Client.Self.lastInterpolation - start;
            }

            // Start the timer again. Use a minimum of a 50ms pause in between calculations
            int delay = Math.Max(50, Settings.INTERPOLATION_INTERVAL - elapsed);
            if (InterpolationTimer != null)
            {
                InterpolationTimer.Change(delay, Timeout.Infinite);
            }
            
        }                        
    }
    #region EventArgs classes
    
    /// <summary>Provides data for the <see cref="ObjectManager.ObjectUpdate"/> event</summary>
    /// <remarks><para>The <see cref="ObjectManager.ObjectUpdate"/> event occurs when the simulator sends
    /// an <see cref="ObjectUpdatePacket"/> containing a Primitive, Foliage or Attachment data</para>
    /// <para>Note 1: The <see cref="ObjectManager.ObjectUpdate"/> event will not be raised when the object is an Avatar</para>
    /// <para>Note 2: It is possible for the <see cref="ObjectManager.ObjectUpdate"/> or <see cref="ObjectManager.AttachmentUpdate"/> to be 
    /// raised twice for the same object if for example the primitive moved to a new simulator, then returned to the current simulator or
    /// if an Avatar crosses the border into a new simulator and returns to the current simulator</para>
    /// </remarks>
    /// <example>
    /// The following code example uses the <see cref="PrimEventArgs.Prim"/>, <see cref="PrimEventArgs.Simulator"/>, and <see cref="PrimEventArgs.IsAttachment"/>
    /// properties to display new Primitives and Attachments on the <see cref="Console"/> window.
    /// <code>
    ///     // Subscribe to the event that gives us new prims and foliage
    ///     Client.Objects.ObjectUpdate += Objects_ObjectUpdate;
    ///     
    ///
    ///     private void Objects_ObjectUpdate(object sender, PrimEventArgs e)
    ///     {
    ///         Console.WriteLine("Primitive {0} {1} in {2} is an attachment {3}", e.Prim.ID, e.Prim.LocalID, e.Simulator.Name, e.IsAttachment);
    ///     }
    /// </code>
    /// </example>
    /// <seealso cref="ObjectManager.AvatarUpdate"/>
    /// <seealso cref="AvatarUpdateEventArgs"/>
    public class PrimEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly bool m_IsAttachment;
        private readonly Primitive m_Prim;
        private readonly ushort m_TimeDilation;

        /// <summary>Get the simulator the object originated from</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the primitive details</summary>
        public Primitive Prim { get { return m_Prim; } }
        public bool IsAttachment { get { return m_IsAttachment; } }
        /// <summary>Get the simulator Time Dilation</summary>
        public ushort TimeDilation { get { return m_TimeDilation; } } 

        /// <summary>
        /// Construct a new instance of the PrimEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the object originated from</param>
        /// <param name="prim">The Primitive</param>
        /// <param name="isAttachment">true of the primitive represents an attachment to an agent</param>
        /// <param name="timeDilation">The simulator time dilation</param>
        public PrimEventArgs(Simulator simulator, Primitive prim, ushort timeDilation, bool isAttachment)
        {
            this.m_Simulator = simulator;
            this.m_IsAttachment = IsAttachment;
            this.m_Prim = prim;
            this.m_TimeDilation = timeDilation;
        }
    }

    /// <summary>Provides data for the <see cref="ObjectManager.AvatarUpdate"/> event</summary>
    /// <remarks><para>The <see cref="ObjectManager.AvatarUpdate"/> event occurs when the simulator sends
    /// an <see cref="ObjectUpdatePacket"/> containing Avatar data</para>    
    /// <para>Note 1: The <see cref="ObjectManager.AvatarUpdate"/> event will not be raised when the object is an Avatar</para>
    /// <para>Note 2: It is possible for the <see cref="ObjectManager.AvatarUpdate"/> to be 
    /// raised twice for the same avatar if for example the avatar moved to a new simulator, then returned to the current simulator</para>
    /// </remarks>
    /// <example>
    /// The following code example uses the <see cref="AvatarUpdateEventArgs.Avatar"/> property to make a request for the top picks
    /// using the <see cref="AvatarManager.RequestAvatarPicks"/> method in the <see cref="AvatarManager"/> class to display the names
    /// of our own agents picks listings on the <see cref="Console"/> window.
    /// <code>
    ///     // subscribe to the AvatarUpdate event to get our information
    ///     Client.Objects.AvatarUpdate += Objects_AvatarUpdate;
    ///     Client.Avatars.AvatarPicksReply += Avatars_AvatarPicksReply;
    ///     
    ///     private void Objects_AvatarUpdate(object sender, AvatarUpdateEventArgs e)
    ///     {
    ///         // we only want our own data
    ///         if (e.Avatar.LocalID == Client.Self.LocalID)
    ///         {    
    ///             // Unsubscribe from the avatar update event to prevent a loop
    ///             // where we continually request the picks every time we get an update for ourselves
    ///             Client.Objects.AvatarUpdate -= Objects_AvatarUpdate;
    ///             // make the top picks request through AvatarManager
    ///             Client.Avatars.RequestAvatarPicks(e.Avatar.ID);
    ///         }
    ///     }
    ///
    ///     private void Avatars_AvatarPicksReply(object sender, AvatarPicksReplyEventArgs e)
    ///     {
    ///         // we'll unsubscribe from the AvatarPicksReply event since we now have the data 
    ///         // we were looking for
    ///         Client.Avatars.AvatarPicksReply -= Avatars_AvatarPicksReply;
    ///         // loop through the dictionary and extract the names of the top picks from our profile
    ///         foreach (var pickName in e.Picks.Values)
    ///         {
    ///             Console.WriteLine(pickName);
    ///         }
    ///     }
    /// </code>
    /// </example>
    /// <seealso cref="ObjectManager.AttachmentUpdate"/>
    /// <seealso cref="ObjectManager.ObjectUpdate"/>
    /// <seealso cref="PrimEventArgs"/>
    public class AvatarUpdateEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly Avatar m_Avatar;
        private readonly ushort m_TimeDilation;

        /// <summary>Get the simulator the object originated from</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the <see cref="Avatar"/> data</summary>
        public Avatar Avatar { get { return m_Avatar; } }
        /// <summary>Get the simulator time dilation</summary>
        public ushort TimeDilation { get { return m_TimeDilation; } }

        /// <summary>
        /// Construct a new instance of the AvatarUpdateEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the packet originated from</param>
        /// <param name="avatar">The <see cref="Avatar"/> data</param>
        /// <param name="timeDilation">The simulator time dilation</param>
        public AvatarUpdateEventArgs(Simulator simulator, Avatar avatar, ushort timeDilation)
        {
            this.m_Simulator = simulator;
            this.m_Avatar = avatar;
            this.m_TimeDilation = timeDilation;
        }
    }

    /// <summary>Provides additional primitive data for the <see cref="ObjectManager.ObjectProperties"/> event</summary>
    /// <remarks><para>The <see cref="ObjectManager.ObjectProperties"/> event occurs when the simulator sends
    /// an <see cref="ObjectPropertiesPacket"/> containing additional details for a Primitive, Foliage data or Attachment data</para>
    /// <para>The <see cref="ObjectManager.ObjectProperties"/> event is also raised when a <see cref="ObjectManager.SelectObject"/> request is
    /// made.</para>
    /// </remarks>
    /// <example>
    /// The following code example uses the <see cref="PrimEventArgs.Prim"/>, <see cref="PrimEventArgs.Simulator"/> and
    /// <see cref="ObjectPropertiesEventArgs.Properties"/>
    /// properties to display new attachments and send a request for additional properties containing the name of the
    /// attachment then display it on the <see cref="Console"/> window.
    /// <code>
    ///     // Subscribe to the event that gives us new Attachments worn
    ///     // by yours or another agent
    ///     Client.Objects.AttachmentUpdate += Objects_AttachmentUpdate;
    ///     // Subscribe to the event that provides additional primitive details
    ///     Client.Objects.ObjectProperties += Objects_ObjectProperties;
    ///      
    ///     private void Objects_AttachmentUpdate(object sender, PrimEventArgs e)
    ///     {
    ///         Console.WriteLine("New Attachment {0} {1} in {2}", e.Prim.ID, e.Prim.LocalID, e.Simulator.Name);
    ///         // send a request that causes the simulator to send us back the ObjectProperties
    ///         Client.Objects.SelectObject(e.Simulator, e.Prim.LocalID);
    ///         
    ///     }
    ///     
    ///     // handle the properties data that arrives
    ///     private void Objects_ObjectProperties(object sender, ObjectPropertiesEventArgs e)
    ///     {
    ///         Console.WriteLine("Primitive Properties: {0} Name is {1}", e.Properties.ObjectID, e.Properties.Name);
    ///     }   
    /// </code>
    /// </example>
    public class ObjectPropertiesEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly Primitive.ObjectProperties m_Properties;

        /// <summary>Get the simulator the object is located</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the primitive properties</summary>
        public Primitive.ObjectProperties Properties { get { return m_Properties; } }

        /// <summary>
        /// Construct a new instance of the ObjectPropertiesEventArgs class
        /// </summary>
        /// <param name="simulator">The simulator the object is located</param>
        /// <param name="props">The primitive Properties</param>
        public ObjectPropertiesEventArgs(Simulator simulator, Primitive.ObjectProperties props)
        {
            this.m_Simulator = simulator;
            this.m_Properties = props;
        }
    }

    /// <summary>Provides additional primitive data for the <see cref="ObjectManager.ObjectPropertiesUpdated"/> event</summary>
    /// <remarks><para>The <see cref="ObjectManager.ObjectPropertiesUpdated"/> event occurs when the simulator sends
    /// an <see cref="ObjectPropertiesPacket"/> containing additional details for a Primitive or Foliage data that is currently
    /// being tracked in the <see cref="Simulator.ObjectsPrimitives"/> dictionary</para>
    /// <para>The <see cref="ObjectManager.ObjectPropertiesUpdated"/> event is also raised when a <see cref="ObjectManager.SelectObject"/> request is
    /// made and <see cref="Settings.OBJECT_TRACKING"/> is enabled</para>    
    /// </remarks>    
    public class ObjectPropertiesUpdatedEventArgs : EventArgs
    {

        private readonly Simulator m_Simulator;
        private readonly Primitive m_Prim;
        private readonly Primitive.ObjectProperties m_Properties;
                
        /// <summary>Get the simulator the object is located</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the primitive details</summary>
        public Primitive Prim { get { return m_Prim; } }
        /// <summary>Get the primitive properties</summary>
        public Primitive.ObjectProperties Properties { get { return m_Properties; } }

        /// <summary>
        /// Construct a new instance of the ObjectPropertiesUpdatedEvenrArgs class
        /// </summary>                
        /// <param name="simulator">The simulator the object is located</param>
        /// <param name="prim">The Primitive</param>
        /// <param name="props">The primitive Properties</param>
        public ObjectPropertiesUpdatedEventArgs(Simulator simulator, Primitive prim, Primitive.ObjectProperties props)
        {
            this.m_Simulator = simulator;
            this.m_Prim = prim;
            this.m_Properties = props;
        }
    }

    /// <summary>Provides additional primitive data, permissions and sale info for the <see cref="ObjectManager.ObjectPropertiesFamily"/> event</summary>
    /// <remarks><para>The <see cref="ObjectManager.ObjectPropertiesFamily"/> event occurs when the simulator sends
    /// an <see cref="ObjectPropertiesPacket"/> containing additional details for a Primitive, Foliage data or Attachment. This includes
    /// Permissions, Sale info, and other basic details on an object</para>
    /// <para>The <see cref="ObjectManager.ObjectProperties"/> event is also raised when a <see cref="ObjectManager.RequestObjectPropertiesFamily"/> request is
    /// made, the viewer equivalent is hovering the mouse cursor over an object</para>
    /// </remarks>    
    public class ObjectPropertiesFamilyEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly Primitive.ObjectProperties m_Properties;
        private readonly ReportType m_Type;

        /// <summary>Get the simulator the object is located</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary></summary>
        public Primitive.ObjectProperties Properties { get { return m_Properties; } }
        /// <summary></summary>
        public ReportType Type { get { return m_Type; } }

        public ObjectPropertiesFamilyEventArgs(Simulator simulator, Primitive.ObjectProperties props, ReportType type)
        {
            this.m_Simulator = simulator;
            this.m_Properties = props;
            this.m_Type = type;
        }
    }

    /// <summary>Provides primitive data containing updated location, velocity, rotation, textures for the <see cref="ObjectManager.TerseObjectUpdate"/> event</summary>
    /// <remarks><para>The <see cref="ObjectManager.TerseObjectUpdate"/> event occurs when the simulator sends updated location, velocity, rotation, etc</para>        
    /// </remarks>
    public class TerseObjectUpdateEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly Primitive m_Prim;
        private readonly ObjectMovementUpdate m_Update;
        private readonly ushort m_TimeDilation;

        /// <summary>Get the simulator the object is located</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the primitive details</summary>
        public Primitive Prim { get { return m_Prim; } }
        /// <summary></summary>
        public ObjectMovementUpdate Update { get { return m_Update; } }
        /// <summary></summary>
        public ushort TimeDilation { get { return m_TimeDilation; } }

        public TerseObjectUpdateEventArgs(Simulator simulator, Primitive prim, ObjectMovementUpdate update, ushort timeDilation)
        {
            this.m_Simulator = simulator;
            this.m_Prim = prim;
            this.m_Update = update;
            this.m_TimeDilation = timeDilation;                
        }
    }
   
    /// <summary>
    /// 
    /// </summary>
    public class ObjectDataBlockUpdateEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly Primitive m_Prim;
        private readonly Primitive.ConstructionData m_ConstructionData;
        private readonly ObjectUpdatePacket.ObjectDataBlock m_Block;
        private readonly ObjectMovementUpdate m_Update;
        private readonly NameValue[] m_NameValues;

        /// <summary>Get the simulator the object is located</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary>Get the primitive details</summary>
        public Primitive Prim { get { return m_Prim; } }
        /// <summary></summary>
        public Primitive.ConstructionData ConstructionData { get { return m_ConstructionData; } }
        /// <summary></summary>
        public ObjectUpdatePacket.ObjectDataBlock Block { get { return m_Block; } }
        /// <summary></summary>
        public ObjectMovementUpdate Update { get { return m_Update; } }
        /// <summary></summary>
        public NameValue[] NameValues { get { return m_NameValues; } } 

        public ObjectDataBlockUpdateEventArgs(Simulator simulator, Primitive prim, Primitive.ConstructionData constructionData, 
            ObjectUpdatePacket.ObjectDataBlock block, ObjectMovementUpdate objectupdate, NameValue[] nameValues)
        {
            this.m_Simulator = simulator;
            this.m_Prim = prim;
            this.m_ConstructionData = constructionData;
            this.m_Block = block;
            this.m_Update = objectupdate;
            this.m_NameValues = nameValues;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class KillObjectEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly uint m_ObjectLocalID;

        /// <summary>Get the simulator the object is located</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary></summary>
        public uint ObjectLocalID { get { return m_ObjectLocalID; } } 

        public KillObjectEventArgs(Simulator simulator, uint objectID)
        {
            this.m_Simulator = simulator;
            this.m_ObjectLocalID = objectID;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class AvatarSitChangedEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly Avatar m_Avatar;
        private readonly uint m_SittingOn;
        private readonly uint m_OldSeat;

        /// <summary>Get the simulator the object is located</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary></summary>
        public Avatar Avatar { get { return m_Avatar; } }
        /// <summary></summary>
        public uint SittingOn { get { return m_SittingOn; } }
        /// <summary></summary>
        public uint OldSeat { get { return m_OldSeat; } } 

        public AvatarSitChangedEventArgs(Simulator simulator, Avatar avatar, uint sittingOn, uint oldSeat)
        {
            this.m_Simulator = simulator;
            this.m_Avatar = avatar;
            this.m_SittingOn = sittingOn;
            this.m_OldSeat = oldSeat;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class PayPriceReplyEventArgs : EventArgs
    {
        private readonly Simulator m_Simulator;
        private readonly UUID m_ObjectID;
        private readonly int m_DefaultPrice;
        private readonly int[] m_ButtonPrices;

        /// <summary>Get the simulator the object is located</summary>
        public Simulator Simulator { get { return m_Simulator; } }
        /// <summary></summary>
        public UUID ObjectID { get { return m_ObjectID; } }
        /// <summary></summary>
        public int DefaultPrice { get { return m_DefaultPrice; } }
        /// <summary></summary>
        public int[] ButtonPrices { get { return m_ButtonPrices; } } 

        public PayPriceReplyEventArgs(Simulator simulator, UUID objectID, int defaultPrice, int[] buttonPrices)
        {
            this.m_Simulator = simulator;
            this.m_ObjectID = objectID;
            this.m_DefaultPrice = defaultPrice;
            this.m_ButtonPrices = buttonPrices;
        }
    }
    #endregion
}
