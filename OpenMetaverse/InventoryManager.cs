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
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;
using System.Runtime.Serialization;
using OpenMetaverse.Http;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    #region Enums
    /// <summary>
    /// Inventory Item Types, eg Script, Notecard, Folder, etc
    /// </summary>
    public enum InventoryType : sbyte
    {
        /// <summary>Unknown</summary>
        Unknown = -1,
        /// <summary>Texture</summary>
        Texture = 0,
        /// <summary>Sound</summary>
        Sound = 1,
        /// <summary>Calling Card</summary>
        CallingCard = 2,
        /// <summary>Landmark</summary>
        Landmark = 3,
        /*
        /// <summary>Script</summary>
        //[Obsolete("See LSL")] Script = 4,
        /// <summary>Clothing</summary>
        //[Obsolete("See Wearable")] Clothing = 5,
        /// <summary>Object, both single and coalesced</summary>
         */
        Object = 6,
        /// <summary>Notecard</summary>
        Notecard = 7,
        /// <summary></summary>
        Category = 8,
        /// <summary>Folder</summary>
        Folder = 8,
        /// <summary></summary>
        RootCategory = 9,
        /// <summary>an LSL Script</summary>
        LSL = 10,
        /*
        /// <summary></summary>
        //[Obsolete("See LSL")] LSLBytecode = 11,
        /// <summary></summary>
        //[Obsolete("See Texture")] TextureTGA = 12,
        /// <summary></summary>
        //[Obsolete] Bodypart = 13,
        /// <summary></summary>
        //[Obsolete] Trash = 14,
         */
        /// <summary></summary>
        Snapshot = 15,
        /*
        /// <summary></summary>
        //[Obsolete] LostAndFound = 16,
         */
        /// <summary></summary>
        Attachment = 17,
        /// <summary></summary>
        Wearable = 18,
        /// <summary></summary>
        Animation = 19,
        /// <summary></summary>
        Gesture = 20
    }

    /// <summary>
    /// Item Sale Status
    /// </summary>
    public enum SaleType : byte
    {
        /// <summary>Not for sale</summary>
        Not = 0,
        /// <summary>The original is for sale</summary>
        Original = 1,
        /// <summary>Copies are for sale</summary>
        Copy = 2,
        /// <summary>The contents of the object are for sale</summary>
        Contents = 3
    }

    [Flags]
    public enum InventorySortOrder : int
    {
        /// <summary>Sort by name</summary>
        ByName = 0,
        /// <summary>Sort by date</summary>
        ByDate = 1,
        /// <summary>Sort folders by name, regardless of whether items are
        /// sorted by name or date</summary>
        FoldersByName = 2,
        /// <summary>Place system folders at the top</summary>
        SystemFoldersToTop = 4
    }

    /// <summary>
    /// Possible destinations for DeRezObject request
    /// </summary>
    public enum DeRezDestination : byte
    {
        /// <summary></summary>
        AgentInventorySave = 0,
        /// <summary>Copy from in-world to agent inventory</summary>
        AgentInventoryCopy = 1,
        /// <summary>Derez to TaskInventory</summary>
        TaskInventory = 2,
        /// <summary></summary>
        Attachment = 3,
        /// <summary>Take Object</summary>
        AgentInventoryTake = 4,
        /// <summary></summary>
        ForceToGodInventory = 5,
        /// <summary>Delete Object</summary>
        TrashFolder = 6,
        /// <summary>Put an avatar attachment into agent inventory</summary>
        AttachmentToInventory = 7,
        /// <summary></summary>
        AttachmentExists = 8,
        /// <summary>Return an object back to the owner's inventory</summary>
        ReturnToOwner = 9,
        /// <summary>Return a deeded object back to the last owner's inventory</summary>
        ReturnToLastOwner = 10
    }

    /// <summary>
    /// Upper half of the Flags field for inventory items
    /// </summary>
    [Flags]
    public enum InventoryItemFlags
    {
        None = 0,
        LandmarkVisited = 1,
        ObjectSlamPerm = 0x100,
        ObjectSlamSale = 0x1000,
        ObjectPermOverwriteBase = 0x010000,
        ObjectPermOverwriteOwner = 0x020000,
        ObjectPermOverwriteGroup = 0x040000,
        ObjectPermOverwriteEveryone = 0x080000,
        ObjectPermOverwriteNextOwner = 0x100000,
        ObjectHasMultipleItems = 0x200000,
        SharedSingleReference = 0x40000000,
    }

    #endregion Enums

    #region Inventory Object Classes
    /// <summary>
    /// Base Class for Inventory Items
    /// </summary>
    [Serializable()]
    public abstract class InventoryBase : ISerializable
    {
        /// <summary><seealso cref="OpenMetaverse.UUID"/> of item/folder</summary>
        public readonly UUID UUID;
        /// <summary><seealso cref="OpenMetaverse.UUID"/> of parent folder</summary>
        public UUID ParentUUID;
        /// <summary>Name of item/folder</summary>
        public string Name;
        /// <summary>Item/Folder Owners <seealso cref="OpenMetaverse.UUID"/></summary>
        public UUID OwnerID;

        /// <summary>
        /// Constructor, takes an itemID as a parameter
        /// </summary>
        /// <param name="itemID">The <seealso cref="OpenMetaverse.UUID"/> of the item</param>
        public InventoryBase(UUID itemID)
        {
            if (itemID == UUID.Zero)
                Logger.Log("Initializing an InventoryBase with UUID.Zero", Helpers.LogLevel.Warning);
            UUID = itemID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("UUID", UUID);
            info.AddValue("ParentUUID",ParentUUID );
            info.AddValue("Name", Name);
            info.AddValue("OwnerID", OwnerID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public InventoryBase(SerializationInfo info, StreamingContext ctxt)
        {
            UUID = (UUID)info.GetValue("UUID", typeof(UUID));
            ParentUUID = (UUID)info.GetValue("ParentUUID", typeof(UUID));
            Name = (string)info.GetValue("Name", typeof(string));
            OwnerID = (UUID)info.GetValue("OwnerID", typeof(UUID));
        }

        /// <summary>
        /// Generates a number corresponding to the value of the object to support the use of a hash table,
        /// suitable for use in hashing algorithms and data structures such as a hash table
        /// </summary>
        /// <returns>A Hashcode of all the combined InventoryBase fields</returns>
        public override int GetHashCode()
        {
            return UUID.GetHashCode() ^ ParentUUID.GetHashCode() ^ Name.GetHashCode() ^ OwnerID.GetHashCode();
        }

        /// <summary>
        /// Determine whether the specified <seealso cref="OpenMetaverse.InventoryBase"/> object is equal to the current object
        /// </summary>
        /// <param name="o">InventoryBase object to compare against</param>
        /// <returns>true if objects are the same</returns>
        public override bool Equals(object o)
        {
            InventoryBase inv = o as InventoryBase;
            return inv != null && Equals(inv);
        }

        /// <summary>
        /// Determine whether the specified <seealso cref="OpenMetaverse.InventoryBase"/> object is equal to the current object
        /// </summary>
        /// <param name="o">InventoryBase object to compare against</param>
        /// <returns>true if objects are the same</returns>
        public virtual bool Equals(InventoryBase o)
        {
            return o.UUID == UUID
                && o.ParentUUID == ParentUUID
                && o.Name == Name
                && o.OwnerID == OwnerID;
        }
    }

    /// <summary>
    /// An Item in Inventory
    /// </summary>
    public class InventoryItem : InventoryBase
    {
        /// <summary>The <seealso cref="OpenMetaverse.UUID"/> of this item</summary>
        public UUID AssetUUID;
        /// <summary>The combined <seealso cref="OpenMetaverse.Permissions"/> of this item</summary>
        public Permissions Permissions;
        /// <summary>The type of item from <seealso cref="OpenMetaverse.AssetType"/></summary>
        public AssetType AssetType;
        /// <summary>The type of item from the <seealso cref="OpenMetaverse.InventoryType"/> enum</summary>
        public InventoryType InventoryType;
        /// <summary>The <seealso cref="OpenMetaverse.UUID"/> of the creator of this item</summary>
        public UUID CreatorID;
        /// <summary>A Description of this item</summary>
        public string Description;
        /// <summary>The <seealso cref="OpenMetaverse.Group"/>s <seealso cref="OpenMetaverse.UUID"/> this item is set to or owned by</summary>
        public UUID GroupID;
        /// <summary>If true, item is owned by a group</summary>
        public bool GroupOwned;
        /// <summary>The price this item can be purchased for</summary>
        public int SalePrice;
        /// <summary>The type of sale from the <seealso cref="OpenMetaverse.SaleType"/> enum</summary>
        public SaleType SaleType;
        /// <summary>Combined flags from <seealso cref="OpenMetaverse.InventoryItemFlags"/></summary>
        public uint Flags;
        /// <summary>Time and date this inventory item was created, stored as
        /// UTC (Coordinated Universal Time)</summary>
        public DateTime CreationDate;

        /// <summary>
        ///  Construct a new InventoryItem object
        /// </summary>
        /// <param name="itemID">The <seealso cref="OpenMetaverse.UUID"/> of the item</param>
        public InventoryItem(UUID itemID) 
            : base(itemID) { }

        /// <summary>
        /// Construct a new InventoryItem object of a specific Type
        /// </summary>
        /// <param name="type">The type of item from <seealso cref="OpenMetaverse.InventoryType"/></param>
        /// <param name="itemID"><seealso cref="OpenMetaverse.UUID"/> of the item</param>
        public InventoryItem(InventoryType type, UUID itemID) : base(itemID) { InventoryType = type; }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        new public void GetObjectData(SerializationInfo info, StreamingContext ctxt) 
        {
            base.GetObjectData(info,ctxt);
            info.AddValue("AssetUUID",AssetUUID,typeof(UUID));
            info.AddValue("Permissions", Permissions,typeof(Permissions));
            info.AddValue("AssetType", AssetType);
            info.AddValue("InventoryType", InventoryType);
            info.AddValue("CreatorID", CreatorID);
            info.AddValue("Description", Description);
            info.AddValue("GroupID", GroupID);
            info.AddValue("GroupOwned", GroupOwned);
            info.AddValue("SalePrice", SalePrice);
            info.AddValue("SaleType", SaleType);
            info.AddValue("Flags", Flags);
            info.AddValue("CreationDate", CreationDate);  
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public InventoryItem(SerializationInfo info, StreamingContext ctxt) : base (info,ctxt)
        {
           AssetUUID = (UUID)info.GetValue("AssetUUID", typeof(UUID));
           Permissions =(Permissions)info.GetValue("Permissions",typeof(Permissions));
           AssetType = (AssetType)info.GetValue("AssetType", typeof(AssetType));
           InventoryType = (InventoryType)info.GetValue("InventoryType", typeof(InventoryType));
           CreatorID = (UUID)info.GetValue("CreatorID", typeof(UUID));
           Description = (string)info.GetValue("Description", typeof(string));
           GroupID= (UUID)info.GetValue("GroupID", typeof(UUID));
           GroupOwned = (bool)info.GetValue("GroupOwned", typeof(bool));
           SalePrice = (int)info.GetValue("SalePrice", typeof(int));
           SaleType = (SaleType)info.GetValue("SaleType", typeof(SaleType));
           Flags = (uint)info.GetValue("Flags", typeof(uint));
           CreationDate = (DateTime)info.GetValue("CreationDate", typeof(DateTime));
        }

        /// <summary>
        /// Generates a number corresponding to the value of the object to support the use of a hash table.
        /// Suitable for use in hashing algorithms and data structures such as a hash table
        /// </summary>
        /// <returns>A Hashcode of all the combined InventoryItem fields</returns>
        public override int GetHashCode()
        {
            return AssetUUID.GetHashCode() ^ Permissions.GetHashCode() ^ AssetType.GetHashCode() ^
                InventoryType.GetHashCode() ^ Description.GetHashCode() ^ GroupID.GetHashCode() ^
                GroupOwned.GetHashCode() ^ SalePrice.GetHashCode() ^ SaleType.GetHashCode() ^
                Flags.GetHashCode() ^ CreationDate.GetHashCode();
        }

        /// <summary>
        /// Compares an object
        /// </summary>
        /// <param name="o">The object to compare</param>
        /// <returns>true if comparison object matches</returns>
        public override bool Equals(object o)
        {
            InventoryItem item = o as InventoryItem;
            return item != null && Equals(item);
        }

        /// <summary>
        /// Determine whether the specified <seealso cref="OpenMetaverse.InventoryBase"/> object is equal to the current object
        /// </summary>
        /// <param name="o">The <seealso cref="OpenMetaverse.InventoryBase"/> object to compare against</param>
        /// <returns>true if objects are the same</returns>
        public override bool Equals(InventoryBase o)
        {
            InventoryItem item = o as InventoryItem;
            return item != null && Equals(item);
        }

        /// <summary>
        /// Determine whether the specified <seealso cref="OpenMetaverse.InventoryItem"/> object is equal to the current object
        /// </summary>
        /// <param name="o">The <seealso cref="OpenMetaverse.InventoryItem"/> object to compare against</param>
        /// <returns>true if objects are the same</returns>
        public bool Equals(InventoryItem o)
        {
            return base.Equals(o as InventoryBase)
                && o.AssetType == AssetType
                && o.AssetUUID == AssetUUID
                && o.CreationDate == CreationDate
                && o.Description == Description
                && o.Flags == Flags
                && o.GroupID == GroupID
                && o.GroupOwned == GroupOwned
                && o.InventoryType == InventoryType
                && o.Permissions.Equals(Permissions)
                && o.SalePrice == SalePrice
                && o.SaleType == SaleType;
        }
    }

    /// <summary>
    /// InventoryTexture Class representing a graphical image
    /// </summary>
    /// <seealso cref="ManagedImage"/>
    public class InventoryTexture : InventoryItem 
    { 
        /// <summary>
        /// Construct an InventoryTexture object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryTexture(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.Texture; 
        } 

        /// <summary>
        /// Construct an InventoryTexture object from a serialization stream
        /// </summary>
        public InventoryTexture(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.Texture; 
        }
    }

    /// <summary>
    /// InventorySound Class representing a playable sound
    /// </summary>
    public class InventorySound : InventoryItem 
    {
        /// <summary>
        /// Construct an InventorySound object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventorySound(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.Sound; 
        } 

        /// <summary>
        /// Construct an InventorySound object from a serialization stream
        /// </summary>
        public InventorySound(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.Sound; 
        }
    }

    /// <summary>
    /// InventoryCallingCard Class, contains information on another avatar
    /// </summary>
    public class InventoryCallingCard : InventoryItem 
    {
        /// <summary>
        /// Construct an InventoryCallingCard object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryCallingCard(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.CallingCard; 
        }

        /// <summary>
        /// Construct an InventoryCallingCard object from a serialization stream
        /// </summary>
        public InventoryCallingCard(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.CallingCard; 
        }
    }

    /// <summary>
    /// InventoryLandmark Class, contains details on a specific location
    /// </summary>
    public class InventoryLandmark : InventoryItem 
    {
        /// <summary>
        /// Construct an InventoryLandmark object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryLandmark(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.Landmark; 
        }

        /// <summary>
        /// Construct an InventoryLandmark object from a serialization stream
        /// </summary>
        public InventoryLandmark(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.Landmark;

        }

        /// <summary>
        /// Landmarks use the ObjectType struct and will have a flag of 1 set if they have been visited
        /// </summary>
        public ObjectType LandmarkType
        {
            get { return (ObjectType)Flags; }
            set { Flags = (uint)value; }
        }
    }

    /// <summary>
    /// InventoryObject Class contains details on a primitive or coalesced set of primitives
    /// </summary>
    public class InventoryObject : InventoryItem 
    {
        /// <summary>
        /// Construct an InventoryObject object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryObject(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.Object; 
        }

        /// <summary>
        /// Construct an InventoryObject object from a serialization stream
        /// </summary>
        public InventoryObject(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.Object; 
        }

        /// <summary>
        /// Get the Objects permission override settings
        /// 
        /// These will indicate the which permissions that 
        /// will be overwritten when the object is rezzed in-world
        /// </summary>
        public ObjectType ObjectType
        {
            get { return (ObjectType)Flags; }
            set { Flags = (uint)value; }
        }
    }

    /// <summary>
    /// InventoryNotecard Class, contains details on an encoded text document
    /// </summary>
    public class InventoryNotecard : InventoryItem 
    {
        /// <summary>
        /// Construct an InventoryNotecard object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryNotecard(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.Notecard; 
        }

        /// <summary>
        /// Construct an InventoryNotecard object from a serialization stream
        /// </summary>
        public InventoryNotecard(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.Notecard; 
        }
    }

    /// <summary>
    /// InventoryCategory Class
    /// </summary>
    /// <remarks>TODO: Is this even used for anything?</remarks>
    public class InventoryCategory : InventoryItem 
    {
        /// <summary>
        /// Construct an InventoryCategory object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryCategory(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.Category; 
        } 

        /// <summary>
        /// Construct an InventoryCategory object from a serialization stream
        /// </summary>
        public InventoryCategory(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.Category; 
        }
    }

    /// <summary>
    /// InventoryLSL Class, represents a Linden Scripting Language object
    /// </summary>
    public class InventoryLSL : InventoryItem 
    {
        /// <summary>
        /// Construct an InventoryLSL object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryLSL(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.LSL; 
        } 

        /// <summary>
        /// Construct an InventoryLSL object from a serialization stream
        /// </summary>
        public InventoryLSL(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.LSL; 
        }
    }

    /// <summary>
    /// InventorySnapshot Class, an image taken with the viewer
    /// </summary>
    public class InventorySnapshot : InventoryItem 
    {
        /// <summary>
        /// Construct an InventorySnapshot object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventorySnapshot(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.Snapshot; 
        } 

        /// <summary>
        /// Construct an InventorySnapshot object from a serialization stream
        /// </summary>
        public InventorySnapshot(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.Snapshot; 
        }
    }

    /// <summary>
    /// InventoryAttachment Class, contains details on an attachable object
    /// </summary>
    public class InventoryAttachment  : InventoryItem 
    {
        /// <summary>
        /// Construct an InventoryAttachment object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryAttachment(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.Attachment; 
        }

        /// <summary>
        /// Construct an InventoryAttachment object from a serialization stream
        /// </summary>
        public InventoryAttachment(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.Attachment; 
        }

        /// <summary>
        /// Get the last AttachmentPoint this object was attached to
        /// </summary>
        public AttachmentPoint AttachmentPoint
        {
            get { return (AttachmentPoint)Flags; }
            set { Flags = (uint)value; }
        }
    }

    /// <summary>
    /// InventoryWearable Class, details on a clothing item or body part
    /// </summary>
    public class InventoryWearable : InventoryItem
    {
        /// <summary>
        /// Construct an InventoryWearable object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryWearable(UUID itemID) : base(itemID) { InventoryType = InventoryType.Wearable; }

        /// <summary>
        /// Construct an InventoryWearable object from a serialization stream
        /// </summary>
        public InventoryWearable(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            InventoryType = InventoryType.Wearable; 
        }

        /// <summary>
        /// The <seealso cref="OpenMetaverse.WearableType"/>, Skin, Shape, Skirt, Etc
        /// </summary>
        public WearableType WearableType
        {
            get { return (WearableType)Flags; }
            set { Flags = (uint)value; }
        }
    }

    /// <summary>
    /// InventoryAnimation Class, A bvh encoded object which animates an avatar
    /// </summary>
    public class InventoryAnimation : InventoryItem 
    {
        /// <summary>
        /// Construct an InventoryAnimation object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryAnimation(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.Animation; 
        }

	/// <summary>
        /// Construct an InventoryAnimation object from a serialization stream
        /// </summary>
        public InventoryAnimation(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.Animation; 
        }
 

    }

    /// <summary>
    /// InventoryGesture Class, details on a series of animations, sounds, and actions
    /// </summary>
    public class InventoryGesture : InventoryItem 
    {
        /// <summary>
        /// Construct an InventoryGesture object
        /// </summary>
        /// <param name="itemID">A <seealso cref="OpenMetaverse.UUID"/> which becomes the 
        /// <seealso cref="OpenMetaverse.InventoryItem"/> objects AssetUUID</param>
        public InventoryGesture(UUID itemID) : base(itemID) 
        { 
            InventoryType = InventoryType.Gesture; 
        } 

        /// <summary>
        /// Construct an InventoryGesture object from a serialization stream
        /// </summary>
        public InventoryGesture(SerializationInfo info, StreamingContext ctxt): base(info, ctxt)
        {
            InventoryType = InventoryType.Gesture; 
        }
    }
    
    /// <summary>
    /// A folder contains <seealso cref="T:OpenMetaverse.InventoryItem"/>s and has certain attributes specific 
    /// to itself
    /// </summary>
    public class InventoryFolder : InventoryBase
    {
        /// <summary>The Preferred <seealso cref="T:OpenMetaverse.AssetType"/> for a folder.</summary>
        public AssetType PreferredType;
        /// <summary>The Version of this folder</summary>
        public int Version;
        /// <summary>Number of child items this folder contains.</summary>
        public int DescendentCount;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="itemID">UUID of the folder</param>
        public InventoryFolder(UUID itemID)
            : base(itemID) { }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Get Serilization data for this InventoryFolder object
        /// </summary>
        new public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            base.GetObjectData(info,ctxt);
            info.AddValue("PreferredType", PreferredType, typeof(AssetType));
            info.AddValue("Version", Version);
            info.AddValue("DescendentCount", DescendentCount);
        }

        /// <summary>
        /// Construct an InventoryFolder object from a serialization stream
        /// </summary>
        public InventoryFolder(SerializationInfo info, StreamingContext ctxt) : base (info, ctxt)
        {
            PreferredType = (AssetType)info.GetValue("PreferredType", typeof(AssetType));
            Version=(int)info.GetValue("Version",typeof(int));
            DescendentCount = (int)info.GetValue("DescendentCount", typeof(int));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return PreferredType.GetHashCode() ^ Version.GetHashCode() ^ DescendentCount.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            InventoryFolder folder = o as InventoryFolder;
            return folder != null && Equals(folder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(InventoryBase o)
        {
            InventoryFolder folder = o as InventoryFolder;
            return folder != null && Equals(folder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool Equals(InventoryFolder o)
        {
            return base.Equals(o as InventoryBase)
                && o.DescendentCount == DescendentCount
                && o.PreferredType == PreferredType
                && o.Version == Version;
        }
    }

    #endregion Inventory Object Classes

    /// <summary>
    /// Tools for dealing with agents inventory
    /// </summary>
    public class InventoryManager
    {
        protected struct InventorySearch
        {
            public UUID Folder;
            public UUID Owner;
            public string[] Path;
            public int Level;
        }

        #region Delegates

        /// <summary>
        /// Callback for inventory item creation finishing
        /// </summary>
        /// <param name="success">Whether the request to create an inventory
        /// item succeeded or not</param>
        /// <param name="item">Inventory item being created. If success is
        /// false this will be null</param>
        public delegate void ItemCreatedCallback(bool success, InventoryItem item);

        /// <summary>
        /// Callback for an inventory item being create from an uploaded asset
        /// </summary>
        /// <param name="success">true if inventory item creation was successful</param>
        /// <param name="status"></param>
        /// <param name="itemID"></param>
        /// <param name="assetID"></param>
        public delegate void ItemCreatedFromAssetCallback(bool success, string status, UUID itemID, UUID assetID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public delegate void ItemCopiedCallback(InventoryBase item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public delegate void ItemReceivedCallback(InventoryItem item);

        /// <summary>
        /// Callback for an inventory folder updating
        /// </summary>
        /// <param name="folderID">UUID of the folder that was updated</param>
        public delegate void FolderUpdatedCallback(UUID folderID);

        /// <summary>
        /// Callback for when an inventory item is offered to us by another avatar or an object
        /// </summary>
        /// <param name="offerDetails">A <seealso cref="InstantMessage"/> object containing specific
        /// details on the item being offered, eg who its from</param>
        /// <param name="type">The <seealso cref="AssetType"/>AssetType being offered</param>
        /// <param name="objectID">Will be null if item is offered from an object</param>
        /// <param name="fromTask">will be true of item is offered from an object</param>
        /// <returns>Return true to accept the offer, or false to decline it</returns>
        public delegate bool ObjectOfferedCallback(InstantMessage offerDetails, AssetType type, UUID objectID, bool fromTask);

        /// <summary>
        /// Callback when an inventory object is accepted and received from a
        /// task inventory. This is the callback in which you actually get
        /// the ItemID, as in ObjectOfferedCallback it is null when received
        /// from a task.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="folderID"></param>
        /// <param name="creatorID"></param>
        /// <param name="assetID"></param>
        /// <param name="type"></param>
        public delegate void TaskItemReceivedCallback(UUID itemID, UUID folderID, UUID creatorID, 
            UUID assetID, InventoryType type);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="inventoryObjectID"></param>
        public delegate void FindObjectByPathCallback(string path, UUID inventoryObjectID);

        /// <summary>
        /// Reply received after calling <code>RequestTaskInventory</code>,
        /// contains a filename that can be used in an asset download request
        /// </summary>
        /// <param name="itemID">UUID of the inventory item</param>
        /// <param name="serial">Version number of the task inventory asset</param>
        /// <param name="assetFilename">Filename of the task inventory asset</param>
        public delegate void TaskInventoryReplyCallback(UUID itemID, short serial, string assetFilename);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="success"></param>
        /// <param name="status"></param>
        /// <param name="itemID"></param>
        /// <param name="assetID"></param>
        public delegate void NotecardUploadedAssetCallback(bool success, string status, UUID itemID, UUID assetID);

        #endregion Delegates

        #region Events

        /// <summary>
        /// Fired when a reply to a RequestFetchInventory() is received
        /// </summary>
        /// <seealso cref="InventoryManager.RequestFetchInventory"/>
        public event ItemReceivedCallback OnItemReceived;

        /// <summary>
        /// Fired when a response to a RequestFolderContents() is received 
        /// </summary>
        /// <seealso cref="InventoryManager.RequestFolderContents"/>
        public event FolderUpdatedCallback OnFolderUpdated;

        /// <summary>
        /// Fired when an object or another avatar offers us an inventory item
        /// </summary>
        public event ObjectOfferedCallback OnObjectOffered;
       
        /// <summary>
        /// Fired when a response to FindObjectByPath() is received
        /// </summary>
        /// <seealso cref="InventoryManager.FindObjectByPath"/>
        public event FindObjectByPathCallback OnFindObjectByPath;

        /// <summary>
        /// Fired when a task inventory item is received
        /// 
        /// This may occur when an object that's rezzed in world is
        /// taken into inventory, when an item is created using the CreateInventoryItem
        /// packet, or when an object is purchased
        /// </summary>
        public event TaskItemReceivedCallback OnTaskItemReceived;

        /// <summary>
        /// Fired in response to a request for a tasks (primitive) inventory
        /// </summary>
        /// <seealso cref="InventoryManager.GetTaskInventory"/>
        /// <seealso cref="InventoryManager.RequestTaskInventory"/>
        public event TaskInventoryReplyCallback OnTaskInventoryReply;

        #endregion Events

        private GridClient _Client;
        private Inventory _Store;
        //private Random _RandNumbers = new Random();
        private object _CallbacksLock = new object();
        private uint _CallbackPos;
        private Dictionary<uint, ItemCreatedCallback> _ItemCreatedCallbacks = new Dictionary<uint, ItemCreatedCallback>();
        private Dictionary<uint, ItemCopiedCallback> _ItemCopiedCallbacks = new Dictionary<uint,ItemCopiedCallback>();
        private List<InventorySearch> _Searches = new List<InventorySearch>();

        #region String Arrays

        /// <summary>Partial mapping of AssetTypes to folder names</summary>
        private static readonly string[] _NewFolderNames = new string[]
        {
            "Textures",
            "Sounds",
            "Calling Cards",
            "Landmarks",
            "Scripts",
            "Clothing",
            "Objects",
            "Notecards",
            "New Folder",
            "Inventory",
            "Scripts",
            "Scripts",
            "Uncompressed Images",
            "Body Parts",
            "Trash",
            "Photo Album",
            "Lost And Found",
            "Uncompressed Sounds",
            "Uncompressed Images",
            "Uncompressed Images",
            "Animations",
            "Gestures"
        };

        private static readonly string[] _AssetTypeNames = new string[]
        {
            "texture",
	        "sound",
	        "callcard",
	        "landmark",
	        "script",
	        "clothing",
	        "object",
	        "notecard",
	        "category",
	        "root",
	        "lsltext",
	        "lslbyte",
	        "txtr_tga",
	        "bodypart",
	        "trash",
	        "snapshot",
	        "lstndfnd",
	        "snd_wav",
	        "img_tga",
	        "jpeg",
	        "animatn",
	        "gesture",
	        "simstate"
        };

        private static readonly string[] _InventoryTypeNames = new string[]
        {
            "texture",
	        "sound",
	        "callcard",
	        "landmark",
	        String.Empty,
	        String.Empty,
	        "object",
	        "notecard",
	        "category",
	        "root",
	        "script",
	        String.Empty,
	        String.Empty,
	        String.Empty,
	        String.Empty,
	        "snapshot",
	        String.Empty,
	        "attach",
	        "wearable",
	        "animation",
	        "gesture",
        };

        private static readonly string[] _SaleTypeNames = new string[]
        {
            "not",
            "orig",
            "copy",
            "cntn"
        };

        #endregion String Arrays

        #region Properties

        /// <summary>
        /// Get this agents Inventory data
        /// </summary>
        public Inventory Store { get { return _Store; } }

        #endregion Properties

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the GridClient object</param>
        public InventoryManager(GridClient client)
        {
            _Client = client;

            _Client.Network.RegisterCallback(PacketType.UpdateCreateInventoryItem, new NetworkManager.PacketCallback(UpdateCreateInventoryItemHandler));
            _Client.Network.RegisterCallback(PacketType.SaveAssetIntoInventory, new NetworkManager.PacketCallback(SaveAssetIntoInventoryHandler));
            _Client.Network.RegisterCallback(PacketType.BulkUpdateInventory, new NetworkManager.PacketCallback(BulkUpdateInventoryHandler));
            _Client.Network.RegisterCallback(PacketType.MoveInventoryItem, new NetworkManager.PacketCallback(MoveInventoryItemHandler));
            _Client.Network.RegisterCallback(PacketType.InventoryDescendents, new NetworkManager.PacketCallback(InventoryDescendentsHandler));
            _Client.Network.RegisterCallback(PacketType.FetchInventoryReply, new NetworkManager.PacketCallback(FetchInventoryReplyHandler));
            _Client.Network.RegisterCallback(PacketType.ReplyTaskInventory, new NetworkManager.PacketCallback(ReplyTaskInventoryHandler));
            
            // Watch for inventory given to us through instant message
            _Client.Self.OnInstantMessage += new AgentManager.InstantMessageCallback(Self_OnInstantMessage);

            // Register extra parameters with login and parse the inventory data that comes back
            _Client.Network.RegisterLoginResponseCallback(
                new NetworkManager.LoginResponseCallback(Network_OnLoginResponse),
                new string[] {
                    "inventory-root", "inventory-skeleton", "inventory-lib-root",
                    "inventory-lib-owner", "inventory-skel-lib"});
        }

        #region Fetch

        /// <summary>
        /// Fetch an inventory item from the dataserver
        /// </summary>
        /// <param name="itemID">The items <seealso cref="UUID"/></param>
        /// <param name="ownerID">The item Owners <seealso cref="OpenMetaverse.UUID"/></param>
        /// <param name="timeoutMS">a integer representing the number of milliseconds to wait for results</param>
        /// <returns>An <seealso cref="InventoryItem"/> object on success, or null if no item was found</returns>
        /// <remarks>Items will also be sent to the <seealso cref="InventoryManager.OnItemReceived"/> event</remarks>
        public InventoryItem FetchItem(UUID itemID, UUID ownerID, int timeoutMS)
        {
            AutoResetEvent fetchEvent = new AutoResetEvent(false);
            InventoryItem fetchedItem = null;

            ItemReceivedCallback callback =
                delegate(InventoryItem item)
                {
                    if (item.UUID == itemID)
                    {
                        fetchedItem = item;
                        fetchEvent.Set();
                    }
                };

            OnItemReceived += callback;
            RequestFetchInventory(itemID, ownerID);

            fetchEvent.WaitOne(timeoutMS, false);
            OnItemReceived -= callback;

            return fetchedItem;
        }

        /// <summary>
        /// Request A single inventory item
        /// </summary>
        /// <param name="itemID">The items <seealso cref="OpenMetaverse.UUID"/></param>
        /// <param name="ownerID">The item Owners <seealso cref="OpenMetaverse.UUID"/></param>
        /// <seealso cref="InventoryManager.OnItemReceived"/>
        public void RequestFetchInventory(UUID itemID, UUID ownerID)
        {
            FetchInventoryPacket fetch = new FetchInventoryPacket();
            fetch.AgentData = new FetchInventoryPacket.AgentDataBlock();
            fetch.AgentData.AgentID = _Client.Self.AgentID;
            fetch.AgentData.SessionID = _Client.Self.SessionID;

            fetch.InventoryData = new FetchInventoryPacket.InventoryDataBlock[1];
            fetch.InventoryData[0] = new FetchInventoryPacket.InventoryDataBlock();
            fetch.InventoryData[0].ItemID = itemID;
            fetch.InventoryData[0].OwnerID = ownerID;

            _Client.Network.SendPacket(fetch);
        }

        /// <summary>
        /// Request inventory items
        /// </summary>
        /// <param name="itemIDs">Inventory items to request</param>
        /// <param name="ownerIDs">Owners of the inventory items</param>
        /// <seealso cref="InventoryManager.OnItemReceived"/>
        public void RequestFetchInventory(List<UUID> itemIDs, List<UUID> ownerIDs)
        {
            if (itemIDs.Count != ownerIDs.Count)
                throw new ArgumentException("itemIDs and ownerIDs must contain the same number of entries");

            FetchInventoryPacket fetch = new FetchInventoryPacket();
            fetch.AgentData = new FetchInventoryPacket.AgentDataBlock();
            fetch.AgentData.AgentID = _Client.Self.AgentID;
            fetch.AgentData.SessionID = _Client.Self.SessionID;

            fetch.InventoryData = new FetchInventoryPacket.InventoryDataBlock[itemIDs.Count];
            for (int i = 0; i < itemIDs.Count; i++)
            {
                fetch.InventoryData[i] = new FetchInventoryPacket.InventoryDataBlock();
                fetch.InventoryData[i].ItemID = itemIDs[i];
                fetch.InventoryData[i].OwnerID = ownerIDs[i];
            }

            _Client.Network.SendPacket(fetch);
        }

        /// <summary>
        /// Get contents of a folder
        /// </summary>
        /// <param name="folder">The <seealso cref="UUID"/> of the folder to search</param>
        /// <param name="owner">The <seealso cref="UUID"/> of the folders owner</param>
        /// <param name="folders">true to retrieve folders</param>
        /// <param name="items">true to retrieve items</param>
        /// <param name="order">sort order to return results in</param>
        /// <param name="timeoutMS">a integer representing the number of milliseconds to wait for results</param>
        /// <returns>A list of inventory items matching search criteria within folder</returns>
        /// <seealso cref="InventoryManager.RequestFolderContents"/>
        /// <remarks>InventoryFolder.DescendentCount will only be accurate if both folders and items are
        /// requested</remarks>
        public List<InventoryBase> FolderContents(UUID folder, UUID owner, bool folders, bool items,
            InventorySortOrder order, int timeoutMS)
        {
            List<InventoryBase> objects = null;
            AutoResetEvent fetchEvent = new AutoResetEvent(false);

            FolderUpdatedCallback callback =
                delegate(UUID folderID)
                {
                    if (folderID == folder
                        && _Store[folder] is InventoryFolder)
                    {
                        // InventoryDescendentsHandler only stores DescendendCount if both folders and items are fetched.
                        if (_Store.GetContents(folder).Count >= ((InventoryFolder)_Store[folder]).DescendentCount)
                        {
                            
                            fetchEvent.Set();
                        }
                    }
                    else
                    {
                        fetchEvent.Set();
                    }
                };

            OnFolderUpdated += callback;

            RequestFolderContents(folder, owner, folders, items, order);
            if (fetchEvent.WaitOne(timeoutMS, false))
                objects = _Store.GetContents(folder);

            OnFolderUpdated -= callback;

            return objects;
        }

        /// <summary>
        /// Request the contents of an inventory folder
        /// </summary>
        /// <param name="folder">The folder to search</param>
        /// <param name="owner">The folder owners <seealso cref="UUID"/></param>
        /// <param name="folders">true to return <seealso cref="InventoryManager.InventoryFolder"/>s contained in folder</param>
        /// <param name="items">true to return <seealso cref="InventoryManager.InventoryItem"/>s containd in folder</param>
        /// <param name="order">the sort order to return items in</param>
        /// <seealso cref="InventoryManager.FolderContents"/>
        public void RequestFolderContents(UUID folder, UUID owner, bool folders, bool items, 
            InventorySortOrder order)
        {
            FetchInventoryDescendentsPacket fetch = new FetchInventoryDescendentsPacket();
            fetch.AgentData.AgentID = _Client.Self.AgentID;
            fetch.AgentData.SessionID = _Client.Self.SessionID;

            fetch.InventoryData.FetchFolders = folders;
            fetch.InventoryData.FetchItems = items;
            fetch.InventoryData.FolderID = folder;
            fetch.InventoryData.OwnerID = owner;
            fetch.InventoryData.SortOrder = (int)order;

            _Client.Network.SendPacket(fetch);
        }

        #endregion Fetch

        #region Find

        /// <summary>
        /// Returns the UUID of the folder (category) that defaults to
        /// containing 'type'. The folder is not necessarily only for that
        /// type
        /// </summary>
        /// <remarks>This will return the root folder if one does not exist</remarks>
        /// <param name="type"></param>
        /// <returns>The UUID of the desired folder if found, the UUID of the RootFolder
        /// if not found, or UUID.Zero on failure</returns>
        public UUID FindFolderForType(AssetType type)
        {
            if (_Store == null)
            {
                Logger.Log("Inventory is null, FindFolderForType() lookup cannot continue",
                    Helpers.LogLevel.Error, _Client);
                return UUID.Zero;
            }

            // Folders go in the root
            if (type == AssetType.Folder)
                return _Store.RootFolder.UUID;

            // Loop through each top-level directory and check if PreferredType
            // matches the requested type
            List<InventoryBase> contents = _Store.GetContents(_Store.RootFolder.UUID);
            foreach (InventoryBase inv in contents)
            {
                if (inv is InventoryFolder)
                {
                    InventoryFolder folder = inv as InventoryFolder;

                    if (folder.PreferredType == type)
                        return folder.UUID;
                }
            }

            // No match found, return Root Folder ID
            return _Store.RootFolder.UUID;
        }

        /// <summary>
        /// Find an object in inventory using a specific path to search
        /// </summary>
        /// <param name="baseFolder">The folder to begin the search in</param>
        /// <param name="inventoryOwner">The object owners <seealso cref="UUID"/></param>
        /// <param name="path">A string path to search</param>
        /// <param name="timeoutMS">milliseconds to wait for a reply</param>
        /// <returns>Found items <seealso cref="UUID"/> or <seealso cref="UUID.Zero"/> if 
        /// timeout occurs or item is not found</returns>
        public UUID FindObjectByPath(UUID baseFolder, UUID inventoryOwner, string path, int timeoutMS)
        {
            AutoResetEvent findEvent = new AutoResetEvent(false);
            UUID foundItem = UUID.Zero;

            FindObjectByPathCallback callback =
                delegate(string thisPath, UUID inventoryObjectID)
                {
                    if (thisPath == path)
                    {
                        foundItem = inventoryObjectID;
                        findEvent.Set();
                    }
                };

            OnFindObjectByPath += callback;

            RequestFindObjectByPath(baseFolder, inventoryOwner, path);
            findEvent.WaitOne(timeoutMS, false);

            OnFindObjectByPath -= callback;

            return foundItem;
        }

        /// <summary>
        /// Find inventory items by path
        /// </summary>
        /// <param name="baseFolder">The folder to begin the search in</param>
        /// <param name="inventoryOwner">The object owners <seealso cref="UUID"/></param>
        /// <param name="path">A string path to search, folders/objects separated by a '/'</param>
        /// <remarks>Results are sent to the <seealso cref="InventoryManager.OnFindObjectByPath"/> event</remarks>
        public void RequestFindObjectByPath(UUID baseFolder, UUID inventoryOwner, string path)
        {
            if (path == null || path.Length == 0)
                throw new ArgumentException("Empty path is not supported");

            // Store this search
            InventorySearch search;
            search.Folder = baseFolder;
            search.Owner = inventoryOwner;
            search.Path = path.Split('/');
            search.Level = 0;
            lock (_Searches) _Searches.Add(search);

            // Start the search
            RequestFolderContents(baseFolder, inventoryOwner, true, true, InventorySortOrder.ByName);
        }

        /// <summary>
        /// Search inventory Store object for an item or folder
        /// </summary>
        /// <param name="baseFolder">The folder to begin the search in</param>
        /// <param name="path">An array which creates a path to search</param>
        /// <param name="level">Number of levels below baseFolder to conduct searches</param>
        /// <param name="firstOnly">if True, will stop searching after first match is found</param>
        /// <returns>A list of inventory items found</returns>
        public List<InventoryBase> LocalFind(UUID baseFolder, string[] path, int level, bool firstOnly)
        {
            List<InventoryBase> objects = new List<InventoryBase>();
            //List<InventoryFolder> folders = new List<InventoryFolder>();
            List<InventoryBase> contents = _Store.GetContents(baseFolder);

            foreach (InventoryBase inv in contents)
            {
                if (inv.Name.CompareTo(path[level]) == 0)
                {
                    if (level == path.Length - 1)
                    {
                        objects.Add(inv);
                        if (firstOnly) return objects;
                    }
                    else if (inv is InventoryFolder)
                        objects.AddRange(LocalFind(inv.UUID, path, level + 1, firstOnly));
                }
            }

            return objects;
        }

        #endregion Find

        #region Move/Rename
        
        /// <summary>
        /// Move an inventory item or folder to a new location
        /// </summary>
        /// <param name="item">The <seealso cref="T:InventoryBase"/> item or folder to move</param>
        /// <param name="newParent">The <seealso cref="T:InventoryFolder"/> to move item or folder to</param>
        public void Move(InventoryBase item, InventoryFolder newParent)
        {
            if (item is InventoryFolder)
                MoveFolder(item.UUID, newParent.UUID);
            else
                MoveItem(item.UUID, newParent.UUID);
        }

        /// <summary>
        /// Move an inventory item or folder to a new location and change its name
        /// </summary>
        /// <param name="item">The <seealso cref="T:InventoryBase"/> item or folder to move</param>
        /// <param name="newParent">The <seealso cref="T:InventoryFolder"/> to move item or folder to</param>
        /// <param name="newName">The name to change the item or folder to</param>
        public void Move(InventoryBase item, InventoryFolder newParent, string newName)
        {
            if (item is InventoryFolder)
                MoveFolder(item.UUID, newParent.UUID, newName);
            else
                MoveItem(item.UUID, newParent.UUID, newName);
        }

        /// <summary>
        /// Move and rename a folder
        /// </summary>
        /// <param name="folderID">The source folders <seealso cref="UUID"/></param>
        /// <param name="newparentID">The destination folders <seealso cref="UUID"/></param>
        /// <param name="newName">The name to change the folder to</param>
        public void MoveFolder(UUID folderID, UUID newparentID, string newName)
        {
            lock (Store)
            {
                if (_Store.Contains(folderID))
                {
                    InventoryBase inv = Store[folderID];
                    inv.Name = newName;
                    _Store.UpdateNodeFor(inv);
                }
            }

            UpdateInventoryFolderPacket move = new UpdateInventoryFolderPacket();
            move.AgentData.AgentID = _Client.Self.AgentID;
            move.AgentData.SessionID = _Client.Self.SessionID;
            move.FolderData = new UpdateInventoryFolderPacket.FolderDataBlock[1];
            move.FolderData[0] = new UpdateInventoryFolderPacket.FolderDataBlock();
            move.FolderData[0].FolderID = folderID;
            move.FolderData[0].ParentID = newparentID;
            move.FolderData[0].Name = Utils.StringToBytes(newName);
            move.FolderData[0].Type = -1;

            _Client.Network.SendPacket(move);
        }

        /// <summary>
        /// Move a folder
        /// </summary>
        /// <param name="folderID">The source folders <seealso cref="UUID"/></param>
        /// <param name="newParentID">The destination folders <seealso cref="UUID"/></param>
        public void MoveFolder(UUID folderID, UUID newParentID)
        {
            lock (Store)
            {
                if (_Store.Contains(folderID))
                {
                    InventoryBase inv = Store[folderID];
                    inv.ParentUUID = newParentID;
                    _Store.UpdateNodeFor(inv);
                }
            }

            MoveInventoryFolderPacket move = new MoveInventoryFolderPacket();
            move.AgentData.AgentID = _Client.Self.AgentID;
            move.AgentData.SessionID = _Client.Self.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryFolderPacket.InventoryDataBlock[1];
            move.InventoryData[0] = new MoveInventoryFolderPacket.InventoryDataBlock();
            move.InventoryData[0].FolderID = folderID;
            move.InventoryData[0].ParentID = newParentID;
            
            _Client.Network.SendPacket(move);
        }
 
        /// <summary>
        /// Move multiple folders, the keys in the Dictionary parameter,
        /// to a new parents, the value of that folder's key.
        /// </summary>
        /// <param name="foldersNewParents">A Dictionary containing the 
        /// <seealso cref="UUID"/> of the source as the key, and the 
        /// <seealso cref="UUID"/> of the destination as the value</param>
        public void MoveFolders(Dictionary<UUID, UUID> foldersNewParents)
        {
            // FIXME: Use two List<UUID> to stay consistent

            lock (Store)
            {
                foreach (KeyValuePair<UUID, UUID> entry in foldersNewParents)
                {
                    if (_Store.Contains(entry.Key))
                    {
                        InventoryBase inv = _Store[entry.Key];
                        inv.ParentUUID = entry.Value;
                        _Store.UpdateNodeFor(inv);
                    }
                }
            }

            //TODO: Test if this truly supports multiple-folder move
            MoveInventoryFolderPacket move = new MoveInventoryFolderPacket();
            move.AgentData.AgentID = _Client.Self.AgentID;
            move.AgentData.SessionID = _Client.Self.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryFolderPacket.InventoryDataBlock[foldersNewParents.Count];

            int index = 0;
            foreach (KeyValuePair<UUID, UUID> folder in foldersNewParents)
            {
                MoveInventoryFolderPacket.InventoryDataBlock block = new MoveInventoryFolderPacket.InventoryDataBlock();
                block.FolderID = folder.Key;
                block.ParentID = folder.Value;
                move.InventoryData[index++] = block;
            }

            _Client.Network.SendPacket(move);
        }


        /// <summary>
        /// Move an inventory item to a new folder
        /// </summary>
        /// <param name="itemID">The <seealso cref="UUID"/> of the source item to move</param>
        /// <param name="folderID">The <seealso cref="UUID"/> of the destination folder</param>
        public void MoveItem(UUID itemID, UUID folderID)
        {
            MoveItem(itemID, folderID, String.Empty);
        }

        /// <summary>
        /// Move and rename an inventory item
        /// </summary>
        /// <param name="itemID">The <seealso cref="UUID"/> of the source item to move</param>
        /// <param name="folderID">The <seealso cref="UUID"/> of the destination folder</param>
        /// <param name="newName">The name to change the folder to</param>
        public void MoveItem(UUID itemID, UUID folderID, string newName)
        {
            lock (_Store)
            {
                    if (_Store.Contains(itemID))
                    {
                        InventoryBase inv = _Store[itemID];
                        inv.ParentUUID = folderID;
                        _Store.UpdateNodeFor(inv);
                    }
            }

            MoveInventoryItemPacket move = new MoveInventoryItemPacket();
            move.AgentData.AgentID = _Client.Self.AgentID;
            move.AgentData.SessionID = _Client.Self.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryItemPacket.InventoryDataBlock[1];
            move.InventoryData[0] = new MoveInventoryItemPacket.InventoryDataBlock();
            move.InventoryData[0].ItemID = itemID;
            move.InventoryData[0].FolderID = folderID;
            move.InventoryData[0].NewName = Utils.StringToBytes(newName);

            _Client.Network.SendPacket(move);
        }

        /// <summary>
        /// Move multiple inventory items to new locations
        /// </summary>
        /// <param name="itemsNewParents">A Dictionary containing the 
        /// <seealso cref="UUID"/> of the source item as the key, and the 
        /// <seealso cref="UUID"/> of the destination folder as the value</param>
        public void MoveItems(Dictionary<UUID, UUID> itemsNewParents)
        {
            lock (_Store)
            {
                foreach (KeyValuePair<UUID, UUID> entry in itemsNewParents)
                {
                    if (_Store.Contains(entry.Key))
                    {
                        InventoryBase inv = _Store[entry.Key];
                        inv.ParentUUID = entry.Value;
                        _Store.UpdateNodeFor(inv);
                    }
                }
            }

            MoveInventoryItemPacket move = new MoveInventoryItemPacket();
            move.AgentData.AgentID = _Client.Self.AgentID;
            move.AgentData.SessionID = _Client.Self.SessionID;
            move.AgentData.Stamp = false; //FIXME: ??

            move.InventoryData = new MoveInventoryItemPacket.InventoryDataBlock[itemsNewParents.Count];

            int index = 0;
            foreach (KeyValuePair<UUID, UUID> entry in itemsNewParents)
            {
                MoveInventoryItemPacket.InventoryDataBlock block = new MoveInventoryItemPacket.InventoryDataBlock();
                block.ItemID = entry.Key;
                block.FolderID = entry.Value;
                block.NewName = Utils.EmptyBytes;
                move.InventoryData[index++] = block;
            }

            _Client.Network.SendPacket(move);
        }

        #endregion Move

        #region Remove

        /// <summary>
        /// Remove descendants of a folder
        /// </summary>
        /// <param name="folder">The <seealso cref="UUID"/> of the folder</param>
        public void RemoveDescendants(UUID folder)
        {
            PurgeInventoryDescendentsPacket purge = new PurgeInventoryDescendentsPacket();
            purge.AgentData.AgentID = _Client.Self.AgentID;
            purge.AgentData.SessionID = _Client.Self.SessionID;
            purge.InventoryData.FolderID = folder;
            _Client.Network.SendPacket(purge);

            // Update our local copy
            lock (_Store)
            {
                if (_Store.Contains(folder))
                {
                    List<InventoryBase> contents = _Store.GetContents(folder);
                    foreach (InventoryBase obj in contents)
                    {
                        _Store.RemoveNodeFor(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Remove a single item from inventory
        /// </summary>
        /// <param name="item">The <seealso cref="UUID"/> of the inventory item to remove</param>
        public void RemoveItem(UUID item)
        {
            List<UUID> items = new List<UUID>(1);
            items.Add(item);

            Remove(items, null);
        }

        /// <summary>
        /// Remove a folder from inventory
        /// </summary>
        /// <param name="folder">The <seealso cref="UUID"/> of the folder to remove</param>
        public void RemoveFolder(UUID folder)
        {
            List<UUID> folders = new List<UUID>(1);
            folders.Add(folder);

            Remove(null, folders);
        }

        /// <summary>
        /// Remove multiple items or folders from inventory
        /// </summary>
        /// <param name="items">A List containing the <seealso cref="UUID"/>s of items to remove</param>
        /// <param name="folders">A List containing the <seealso cref="UUID"/>s of the folders to remove</param>
        public void Remove(List<UUID> items, List<UUID> folders)
        {
            if ((items == null || items.Count == 0) && (folders == null || folders.Count == 0))
                return;

            RemoveInventoryObjectsPacket rem = new RemoveInventoryObjectsPacket();
            rem.AgentData.AgentID = _Client.Self.AgentID;
            rem.AgentData.SessionID = _Client.Self.SessionID;

            if (items == null || items.Count == 0)
            {
                // To indicate that we want no items removed:
                rem.ItemData = new RemoveInventoryObjectsPacket.ItemDataBlock[1];
                rem.ItemData[0] = new RemoveInventoryObjectsPacket.ItemDataBlock();
                rem.ItemData[0].ItemID = UUID.Zero;
            }
            else
            {
                lock (_Store)
                {
                    rem.ItemData = new RemoveInventoryObjectsPacket.ItemDataBlock[items.Count];
                    for (int i = 0; i < items.Count; i++)
                    {
                        rem.ItemData[i] = new RemoveInventoryObjectsPacket.ItemDataBlock();
                        rem.ItemData[i].ItemID = items[i];

                        // Update local copy
                        if (_Store.Contains(items[i]))
                            _Store.RemoveNodeFor(Store[items[i]]);
                    }
                }
            }

            if (folders == null || folders.Count == 0)
            {
                // To indicate we want no folders removed:
                rem.FolderData = new RemoveInventoryObjectsPacket.FolderDataBlock[1];
                rem.FolderData[0] = new RemoveInventoryObjectsPacket.FolderDataBlock();
                rem.FolderData[0].FolderID = UUID.Zero;
            }
            else
            {
                lock (_Store)
                {
                    rem.FolderData = new RemoveInventoryObjectsPacket.FolderDataBlock[folders.Count];
                    for (int i = 0; i < folders.Count; i++)
                    {
                        rem.FolderData[i] = new RemoveInventoryObjectsPacket.FolderDataBlock();
                        rem.FolderData[i].FolderID = folders[i];

                        // Update local copy
                        if (_Store.Contains(folders[i]))
                            _Store.RemoveNodeFor(Store[folders[i]]);
                    }
                }
            }
            _Client.Network.SendPacket(rem);
        }
    
        /// <summary>
        /// Empty the Lost and Found folder
        /// </summary>
        public void EmptyLostAndFound()
        {
            EmptySystemFolder(AssetType.LostAndFoundFolder);
        }

        /// <summary>
        /// Empty the Trash folder
        /// </summary>
        public void EmptyTrash()
        {
            EmptySystemFolder(AssetType.TrashFolder);
        }

        private void EmptySystemFolder(AssetType folderType)
        {
            List<InventoryBase> items = _Store.GetContents(_Store.RootFolder);

            UUID folderKey = UUID.Zero;
            foreach (InventoryBase item in items)
            {
                if ((item as InventoryFolder) != null)
                {
                    InventoryFolder folder = item as InventoryFolder;
                    if (folder.PreferredType == folderType)
                    {
                        folderKey = folder.UUID;
                        break;
                    }
                }
            }
            items = _Store.GetContents(folderKey);
            List<UUID> remItems = new List<UUID>();
            List<UUID> remFolders = new List<UUID>();
            foreach (InventoryBase item in items)
            {
                if ((item as InventoryFolder) != null)
                {
                    remFolders.Add(item.UUID);
                }
                else
                {
                    remItems.Add(item.UUID);
                }
            }
            Remove(remItems, remFolders);
        }   
        #endregion Remove

        #region Create
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="type"></param>
        /// <param name="assetTransactionID">Proper use is to upload the inventory's asset first, then provide the Asset's TransactionID here.</param>
        /// <param name="invType"></param>
        /// <param name="nextOwnerMask"></param>
        /// <param name="callback"></param>
        public void RequestCreateItem(UUID parentFolder, string name, string description, AssetType type, UUID assetTransactionID,
            InventoryType invType, PermissionMask nextOwnerMask, ItemCreatedCallback callback)
        {
            // Even though WearableType 0 is Shape, in this context it is treated as NOT_WEARABLE
            RequestCreateItem(parentFolder, name, description, type, assetTransactionID, invType, (WearableType)0, nextOwnerMask, 
                callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="type"></param>
        /// <param name="assetTransactionID">Proper use is to upload the inventory's asset first, then provide the Asset's TransactionID here.</param>
        /// <param name="invType"></param>
        /// <param name="wearableType"></param>
        /// <param name="nextOwnerMask"></param>
        /// <param name="callback"></param>
        public void RequestCreateItem(UUID parentFolder, string name, string description, AssetType type, UUID assetTransactionID,
            InventoryType invType, WearableType wearableType, PermissionMask nextOwnerMask, ItemCreatedCallback callback)
        {
            CreateInventoryItemPacket create = new CreateInventoryItemPacket();
            create.AgentData.AgentID = _Client.Self.AgentID;
            create.AgentData.SessionID = _Client.Self.SessionID;

            create.InventoryBlock.CallbackID = RegisterItemCreatedCallback(callback);
            create.InventoryBlock.FolderID = parentFolder;
            create.InventoryBlock.TransactionID = assetTransactionID;
            create.InventoryBlock.NextOwnerMask = (uint)nextOwnerMask;
            create.InventoryBlock.Type = (sbyte)type;
            create.InventoryBlock.InvType = (sbyte)invType;
            create.InventoryBlock.WearableType = (byte)wearableType;
            create.InventoryBlock.Name = Utils.StringToBytes(name);
            create.InventoryBlock.Description = Utils.StringToBytes(description);

            _Client.Network.SendPacket(create);
        }

        /// <summary>
        /// Creates a new inventory folder
        /// </summary>
        /// <param name="parentID">ID of the folder to put this folder in</param>
        /// <param name="name">Name of the folder to create</param>
        /// <returns>The UUID of the newly created folder</returns>
        public UUID CreateFolder(UUID parentID, string name)
        {
            return CreateFolder(parentID, name, AssetType.Unknown);
        }

        /// <summary>
        /// Creates a new inventory folder
        /// </summary>
        /// <param name="parentID">ID of the folder to put this folder in</param>
        /// <param name="name">Name of the folder to create</param>
        /// <param name="preferredType">Sets this folder as the default folder
        /// for new assets of the specified type. Use <code>AssetType.Unknown</code>
        /// to create a normal folder, otherwise it will likely create a
        /// duplicate of an existing folder type</param>
        /// <returns>The UUID of the newly created folder</returns>
        /// <remarks>If you specify a preferred type of <code>AsseType.Folder</code>
        /// it will create a new root folder which may likely cause all sorts
        /// of strange problems</remarks>
        public UUID CreateFolder(UUID parentID, string name, AssetType preferredType)
        {
            UUID id = UUID.Random();

            // Assign a folder name if one is not already set
            if (String.IsNullOrEmpty(name))
            {
                if (preferredType >= AssetType.Texture && preferredType <= AssetType.Gesture)
                {
                    name = _NewFolderNames[(int)preferredType];
                }
                else
                {
                    name = "New Folder";
                }
            }

            // Create the new folder locally
            InventoryFolder newFolder = new InventoryFolder(id);
            newFolder.Version = 1;
            newFolder.DescendentCount = 0;
            newFolder.ParentUUID = parentID;
            newFolder.PreferredType = preferredType;
            newFolder.Name = name;
            newFolder.OwnerID = _Client.Self.AgentID;

            // Update the local store
            try { _Store[newFolder.UUID] = newFolder; }
            catch (InventoryException ie) { Logger.Log(ie.Message, Helpers.LogLevel.Warning, _Client, ie); }

            // Create the create folder packet and send it
            CreateInventoryFolderPacket create = new CreateInventoryFolderPacket();
            create.AgentData.AgentID = _Client.Self.AgentID;
            create.AgentData.SessionID = _Client.Self.SessionID;

            create.FolderData.FolderID = id;
            create.FolderData.ParentID = parentID;
            create.FolderData.Type = (sbyte)preferredType;
            create.FolderData.Name = Utils.StringToBytes(name);

            _Client.Network.SendPacket(create);

            return id;
        }

        public void RequestCreateItemFromAsset(byte[] data, string name, string description, AssetType assetType,
            InventoryType invType, UUID folderID, CapsClient.ProgressCallback progCallback, ItemCreatedFromAssetCallback callback)
        {
            if (_Client.Network.CurrentSim == null || _Client.Network.CurrentSim.Caps == null)
                throw new Exception("NewFileAgentInventory capability is not currently available");

            Uri url = _Client.Network.CurrentSim.Caps.CapabilityURI("NewFileAgentInventory");

            if (url != null)
            {
                OSDMap query = new OSDMap();
                query.Add("folder_id", OSD.FromUUID(folderID));
                query.Add("asset_type", OSD.FromString(AssetTypeToString(assetType)));
                query.Add("inventory_type", OSD.FromString(InventoryTypeToString(invType)));
                query.Add("name", OSD.FromString(name));
                query.Add("description", OSD.FromString(description));

                // Make the request
                CapsClient request = new CapsClient(url);
                request.OnComplete += new CapsClient.CompleteCallback(CreateItemFromAssetResponse);
                request.UserData = new object[] { progCallback, callback, data };

                request.StartRequest(query);
            }
            else
            {
                throw new Exception("NewFileAgentInventory capability is not currently available");
            }
        }

        #endregion Create

        #region Copy

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newParent"></param>
        /// <param name="newName"></param>
        /// <param name="callback"></param>
        public void RequestCopyItem(UUID item, UUID newParent, string newName, ItemCopiedCallback callback)
        {
            RequestCopyItem(item, newParent, newName, _Client.Self.AgentID, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newParent"></param>
        /// <param name="newName"></param>
        /// <param name="oldOwnerID"></param>
        /// <param name="callback"></param>
        public void RequestCopyItem(UUID item, UUID newParent, string newName, UUID oldOwnerID,
            ItemCopiedCallback callback)
        {
            List<UUID> items = new List<UUID>(1);
            items.Add(item);

            List<UUID> folders = new List<UUID>(1);
            folders.Add(newParent);

            List<string> names = new List<string>(1);
            names.Add(newName);

            RequestCopyItems(items, folders, names, oldOwnerID, callback);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="targetFolders"></param>
        /// <param name="newNames"></param>
        /// <param name="oldOwnerID"></param>
        /// <param name="callback"></param>
        public void RequestCopyItems(List<UUID> items, List<UUID> targetFolders, List<string> newNames,
            UUID oldOwnerID, ItemCopiedCallback callback)
        {
            if (items.Count != targetFolders.Count || (newNames != null && items.Count != newNames.Count))
                throw new ArgumentException("All list arguments must have an equal number of entries");

            uint callbackID = RegisterItemsCopiedCallback(callback);

            CopyInventoryItemPacket copy = new CopyInventoryItemPacket();
            copy.AgentData.AgentID = _Client.Self.AgentID;
            copy.AgentData.SessionID = _Client.Self.SessionID;

            copy.InventoryData = new CopyInventoryItemPacket.InventoryDataBlock[items.Count];
            for (int i = 0; i < items.Count; ++i)
            {
                copy.InventoryData[i] = new CopyInventoryItemPacket.InventoryDataBlock();
                copy.InventoryData[i].CallbackID = callbackID;
                copy.InventoryData[i].NewFolderID = targetFolders[i];
                copy.InventoryData[i].OldAgentID = oldOwnerID;
                copy.InventoryData[i].OldItemID = items[i];

                if (newNames != null && !String.IsNullOrEmpty(newNames[i]))
                    copy.InventoryData[i].NewName = Utils.StringToBytes(newNames[i]);
                else
                    copy.InventoryData[i].NewName = Utils.EmptyBytes;
            }

            _Client.Network.SendPacket(copy);
        }

        /// <summary>
        /// Request a copy of an asset embedded within a notecard
        /// </summary>
        /// <param name="objectID">Usually UUID.Zero for copying an asset from a notecard</param>
        /// <param name="notecardID">UUID of the notecard to request an asset from</param>
        /// <param name="folderID">Target folder for asset to go to in your inventory</param>
        /// <param name="itemID">UUID of the embedded asset</param>
        /// <param name="callback">callback to run when item is copied to inventory</param>
        public void RequestCopyItemFromNotecard(UUID objectID, UUID notecardID, UUID folderID, UUID itemID, ItemCopiedCallback callback)
        {
            CopyInventoryFromNotecardPacket copy = new CopyInventoryFromNotecardPacket();
            copy.AgentData.AgentID = _Client.Self.AgentID;
            copy.AgentData.SessionID = _Client.Self.SessionID;

            copy.NotecardData.ObjectID = objectID;
            copy.NotecardData.NotecardItemID = notecardID;

            copy.InventoryData = new CopyInventoryFromNotecardPacket.InventoryDataBlock[1];
            copy.InventoryData[0] = new CopyInventoryFromNotecardPacket.InventoryDataBlock();
            copy.InventoryData[0].FolderID = folderID;
            copy.InventoryData[0].ItemID = itemID;
           
            _ItemCopiedCallbacks[0] = callback; //Notecards always use callback ID 0

            _Client.Network.SendPacket(copy);
        }

        #endregion Copy

        #region Update

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void RequestUpdateItem(InventoryItem item)
        {
            List<InventoryItem> items = new List<InventoryItem>(1);
            items.Add(item);

            RequestUpdateItems(items, UUID.Random());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public void RequestUpdateItems(List<InventoryItem> items)
        {
            RequestUpdateItems(items, UUID.Random());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="transactionID"></param>
        public void RequestUpdateItems(List<InventoryItem> items, UUID transactionID)
        {
            UpdateInventoryItemPacket update = new UpdateInventoryItemPacket();
            update.AgentData.AgentID = _Client.Self.AgentID;
            update.AgentData.SessionID = _Client.Self.SessionID;
            update.AgentData.TransactionID = transactionID;

            update.InventoryData = new UpdateInventoryItemPacket.InventoryDataBlock[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                InventoryItem item = items[i];

                UpdateInventoryItemPacket.InventoryDataBlock block = new UpdateInventoryItemPacket.InventoryDataBlock();
                block.BaseMask = (uint)item.Permissions.BaseMask;
                block.CRC = ItemCRC(item);
                block.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
                block.CreatorID = item.CreatorID;
                block.Description = Utils.StringToBytes(item.Description);
                block.EveryoneMask = (uint)item.Permissions.EveryoneMask;
                block.Flags = (uint)item.Flags;
                block.FolderID = item.ParentUUID;
                block.GroupID = item.GroupID;
                block.GroupMask = (uint)item.Permissions.GroupMask;
                block.GroupOwned = item.GroupOwned;
                block.InvType = (sbyte)item.InventoryType;
                block.ItemID = item.UUID;
                block.Name = Utils.StringToBytes(item.Name);
                block.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
                block.OwnerID = item.OwnerID;
                block.OwnerMask = (uint)item.Permissions.OwnerMask;
                block.SalePrice = item.SalePrice;
                block.SaleType = (byte)item.SaleType;
                block.TransactionID = UUID.Zero;
                block.Type = (sbyte)item.AssetType;

                update.InventoryData[i] = block;
            }

            _Client.Network.SendPacket(update);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="notecardID"></param>
        /// <param name="callback"></param>
        public void RequestUploadNotecardAsset(byte[] data, UUID notecardID, NotecardUploadedAssetCallback callback)
        {
            if (_Client.Network.CurrentSim == null || _Client.Network.CurrentSim.Caps == null)
                throw new Exception("UpdateNotecardAgentInventory capability is not currently available");

            Uri url = _Client.Network.CurrentSim.Caps.CapabilityURI("UpdateNotecardAgentInventory");

            if (url != null)
            {
                OSDMap query = new OSDMap();
                query.Add("item_id", OSD.FromUUID(notecardID));

                byte[] postData = StructuredData.OSDParser.SerializeLLSDXmlBytes(query);

                // Make the request
                CapsClient request = new CapsClient(url);
                request.OnComplete += new CapsClient.CompleteCallback(UploadNotecardAssetResponse);
                request.UserData = new object[2] { new KeyValuePair<NotecardUploadedAssetCallback, byte[]>(callback, data), notecardID };
                request.StartRequest(postData);
            }
            else
            {
                throw new Exception("UpdateNotecardAgentInventory capability is not currently available");
            }
        }
        #endregion Update

        #region Rez/Give

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryItem object containing item details</param>
        public UUID RequestRezFromInventory(Simulator simulator, Quaternion rotation, Vector3 position,
            InventoryItem item)
        {
            return RequestRezFromInventory(simulator, rotation, position, item, _Client.Self.ActiveGroup,
                UUID.Random(), false);
        }

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryItem object containing item details</param>
        /// <param name="groupOwner">UUID of group to own the object</param>
        public UUID RequestRezFromInventory(Simulator simulator, Quaternion rotation, Vector3 position,
            InventoryItem item, UUID groupOwner)
        {
            return RequestRezFromInventory(simulator, rotation, position, item, groupOwner, UUID.Random(), false);
        }

        /// <summary>
        /// Rez an object from inventory
        /// </summary>
        /// <param name="simulator">Simulator to place object in</param>
        /// <param name="rotation">Rotation of the object when rezzed</param>
        /// <param name="position">Vector of where to place object</param>
        /// <param name="item">InventoryItem object containing item details</param>
        /// <param name="groupOwner">UUID of group to own the object</param>        
        /// <param name="queryID">User defined queryID to correlate replies</param>
        /// <param name="requestObjectDetails">if set to true the simulator
        /// will automatically send object detail packet(s) back to the client</param>
        public UUID RequestRezFromInventory(Simulator simulator, Quaternion rotation, Vector3 position,
            InventoryItem item, UUID groupOwner, UUID queryID, bool requestObjectDetails)
        {
            RezObjectPacket add = new RezObjectPacket();

            add.AgentData.AgentID = _Client.Self.AgentID;
            add.AgentData.SessionID = _Client.Self.SessionID;
            add.AgentData.GroupID = groupOwner;

            add.RezData.FromTaskID = UUID.Zero;
            add.RezData.BypassRaycast = 1;
            add.RezData.RayStart = position;
            add.RezData.RayEnd = position;
            add.RezData.RayTargetID = UUID.Zero;
            add.RezData.RayEndIsIntersection = false;
            add.RezData.RezSelected = requestObjectDetails;
            add.RezData.RemoveItem = false;
            add.RezData.ItemFlags = (uint)item.Flags;
            add.RezData.GroupMask = (uint)item.Permissions.GroupMask;
            add.RezData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            add.RezData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;

            add.InventoryData.ItemID = item.UUID;
            add.InventoryData.FolderID = item.ParentUUID;
            add.InventoryData.CreatorID = item.CreatorID;
            add.InventoryData.OwnerID = item.OwnerID;
            add.InventoryData.GroupID = item.GroupID;
            add.InventoryData.BaseMask = (uint)item.Permissions.BaseMask;
            add.InventoryData.OwnerMask = (uint)item.Permissions.OwnerMask;
            add.InventoryData.GroupMask = (uint)item.Permissions.GroupMask;
            add.InventoryData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            add.InventoryData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            add.InventoryData.GroupOwned = item.GroupOwned;
            add.InventoryData.TransactionID = queryID;
            add.InventoryData.Type = (sbyte)item.InventoryType;
            add.InventoryData.InvType = (sbyte)item.InventoryType;
            add.InventoryData.Flags = (uint)item.Flags;
            add.InventoryData.SaleType = (byte)item.SaleType;
            add.InventoryData.SalePrice = item.SalePrice;
            add.InventoryData.Name = Utils.StringToBytes(item.Name);
            add.InventoryData.Description = Utils.StringToBytes(item.Description);
            add.InventoryData.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);

            _Client.Network.SendPacket(add, simulator);

            return queryID;
        }

        /// <summary>
        /// DeRez an object from the simulator to the agents Objects folder in the agents Inventory
        /// </summary>
        /// <param name="objectLocalID">The simulator Local ID of the object</param>
        public void RequestDeRezToInventory(uint objectLocalID)
        {
            RequestDeRezToInventory(objectLocalID, DeRezDestination.AgentInventoryTake, 
                _Client.Inventory.FindFolderForType(AssetType.Object), UUID.Random());
        }

        /// <summary>
        /// DeRez an object from the simulator and return to inventory
        /// </summary>
        /// <param name="objectLocalID">The simulator Local ID of the object</param>
        /// <param name="destType">The type of destination from the <seealso cref="DeRezDestination"/> enum</param>
        /// <param name="destFolder">The destination inventory folders <seealso cref="UUID"/> -or- 
        /// if DeRezzing object to a tasks Inventory, the Tasks <seealso cref="UUID"/></param>
        /// <param name="transactionID">The transaction ID for this request which
        /// can be used to correlate this request with other packets</param>
        public void RequestDeRezToInventory(uint objectLocalID, DeRezDestination destType, UUID destFolder, UUID transactionID)
        {
            DeRezObjectPacket take = new DeRezObjectPacket();

            take.AgentData.AgentID = _Client.Self.AgentID;
            take.AgentData.SessionID = _Client.Self.SessionID;
            take.AgentBlock = new DeRezObjectPacket.AgentBlockBlock();
            take.AgentBlock.GroupID = UUID.Zero;
            take.AgentBlock.Destination = (byte)destType;
            take.AgentBlock.DestinationID = destFolder;
            take.AgentBlock.PacketCount = 1;
            take.AgentBlock.PacketNumber = 1;
            take.AgentBlock.TransactionID = transactionID;

            take.ObjectData = new DeRezObjectPacket.ObjectDataBlock[1];
            take.ObjectData[0] = new DeRezObjectPacket.ObjectDataBlock();
            take.ObjectData[0].ObjectLocalID = objectLocalID;
            
            _Client.Network.SendPacket(take);
        }


        /// <summary>
        /// Give an inventory item to another avatar
        /// </summary>
        /// <param name="itemID">The <seealso cref="UUID"/> of the item to give</param>
        /// <param name="itemName">The name of the item</param>
        /// <param name="assetType">The type of the item from the <seealso cref="AssetType"/> enum</param>
        /// <param name="recipient">The <seealso cref="UUID"/> of the recipient</param>
        /// <param name="doEffect">true to generate a beameffect during transfer</param>
        public void GiveItem(UUID itemID, string itemName, AssetType assetType, UUID recipient,
            bool doEffect)
        {
            byte[] bucket;


                bucket = new byte[17];
                bucket[0] = (byte)assetType;
                Buffer.BlockCopy(itemID.GetBytes(), 0, bucket, 1, 16);

            _Client.Self.InstantMessage(
                    _Client.Self.Name,
                    recipient,
                    itemName,
                    UUID.Random(),
                    InstantMessageDialog.InventoryOffered,
                    InstantMessageOnline.Online,
                    _Client.Self.SimPosition,
                    _Client.Network.CurrentSim.ID,
                    bucket);

            if (doEffect)
            {
                _Client.Self.BeamEffect(_Client.Self.AgentID, recipient, Vector3d.Zero,
                    _Client.Settings.DEFAULT_EFFECT_COLOR, 1f, UUID.Random());
            }
        }

        /// <summary>
        /// Give an inventory Folder with contents to another avatar
        /// </summary>
        /// <param name="folderID">The <seealso cref="UUID"/> of the Folder to give</param>
        /// <param name="folderName">The name of the folder</param>
        /// <param name="assetType">The type of the item from the <seealso cref="AssetType"/> enum</param>
        /// <param name="recipient">The <seealso cref="UUID"/> of the recipient</param>
        /// <param name="doEffect">true to generate a beameffect during transfer</param>
        public void GiveFolder(UUID folderID, string folderName, AssetType assetType, UUID recipient,
            bool doEffect)
        {
            byte[] bucket;

                List<InventoryItem> folderContents = new List<InventoryItem>();

                _Client.Inventory.FolderContents(folderID, _Client.Self.AgentID, false, true, InventorySortOrder.ByDate, 1000 * 15).ForEach(
                    delegate(InventoryBase ib)
                    {
                        folderContents.Add(_Client.Inventory.FetchItem(ib.UUID, _Client.Self.AgentID, 1000 * 10));
                    });
                bucket = new byte[17 * (folderContents.Count + 1)];

                //Add parent folder (first item in bucket)
                bucket[0] = (byte)assetType;
                Buffer.BlockCopy(folderID.GetBytes(), 0, bucket, 1, 16);

                //Add contents to bucket after folder
                for (int i = 1; i <= folderContents.Count; ++i)
                {
                    bucket[i * 17] = (byte)folderContents[i - 1].AssetType;
                    Buffer.BlockCopy(folderContents[i - 1].UUID.GetBytes(), 0, bucket, i * 17 + 1, 16);
                }

            _Client.Self.InstantMessage(
                    _Client.Self.Name,
                    recipient,
                    folderName,
                    UUID.Random(),
                    InstantMessageDialog.InventoryOffered,
                    InstantMessageOnline.Online,
                    _Client.Self.SimPosition,
                    _Client.Network.CurrentSim.ID,
                    bucket);

            if (doEffect)
            {
                _Client.Self.BeamEffect(_Client.Self.AgentID, recipient, Vector3d.Zero,
                    _Client.Settings.DEFAULT_EFFECT_COLOR, 1f, UUID.Random());
            }
        }

        #endregion Rez/Give

        #region Task

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectLocalID"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public UUID UpdateTaskInventory(uint objectLocalID, InventoryItem item)
        {
            UUID transactionID = UUID.Random();

            UpdateTaskInventoryPacket update = new UpdateTaskInventoryPacket();
            update.AgentData.AgentID = _Client.Self.AgentID;
            update.AgentData.SessionID = _Client.Self.SessionID;
            update.UpdateData.Key = 0;
            update.UpdateData.LocalID = objectLocalID;

            update.InventoryData.ItemID = item.UUID;
            update.InventoryData.FolderID = item.ParentUUID;
            update.InventoryData.CreatorID = item.CreatorID;
            update.InventoryData.OwnerID = item.OwnerID;
            update.InventoryData.GroupID = item.GroupID;
            update.InventoryData.BaseMask = (uint)item.Permissions.BaseMask;
            update.InventoryData.OwnerMask = (uint)item.Permissions.OwnerMask;
            update.InventoryData.GroupMask = (uint)item.Permissions.GroupMask;
            update.InventoryData.EveryoneMask = (uint)item.Permissions.EveryoneMask;
            update.InventoryData.NextOwnerMask = (uint)item.Permissions.NextOwnerMask;
            update.InventoryData.GroupOwned = item.GroupOwned;
            update.InventoryData.TransactionID = transactionID;
            update.InventoryData.Type = (sbyte)item.AssetType;
            update.InventoryData.InvType = (sbyte)item.InventoryType;
            update.InventoryData.Flags = (uint)item.Flags;
            update.InventoryData.SaleType = (byte)item.SaleType;
            update.InventoryData.SalePrice = item.SalePrice;
            update.InventoryData.Name = Utils.StringToBytes(item.Name);
            update.InventoryData.Description = Utils.StringToBytes(item.Description);
            update.InventoryData.CreationDate = (int)Utils.DateTimeToUnixTime(item.CreationDate);
            update.InventoryData.CRC = ItemCRC(item);

            _Client.Network.SendPacket(update);

            return transactionID;
        }

        /// <summary>
        /// Get the inventory of a Task (Primitive)
        /// </summary>
        /// <param name="objectID">The tasks <seealso cref="UUID"/></param>
        /// <param name="objectLocalID">The tasks simulator local ID</param>
        /// <param name="timeoutMS">milliseconds to wait for reply from simulator</param>
        /// <returns>A List containing the inventory items inside the task</returns>
        public List<InventoryBase> GetTaskInventory(UUID objectID, uint objectLocalID, int timeoutMS)
        {
            string filename = null;
            AutoResetEvent taskReplyEvent = new AutoResetEvent(false);

            TaskInventoryReplyCallback callback =
                delegate(UUID itemID, short serial, string assetFilename)
                {
                    if (itemID == objectID)
                    {
                        filename = assetFilename;
                        taskReplyEvent.Set();
                    }
                };

            OnTaskInventoryReply += callback;

            RequestTaskInventory(objectLocalID);

            if (taskReplyEvent.WaitOne(timeoutMS, false))
            {
                OnTaskInventoryReply -= callback;

                if (!String.IsNullOrEmpty(filename))
                {
                    byte[] assetData = null;
                    ulong xferID = 0;
                    AutoResetEvent taskDownloadEvent = new AutoResetEvent(false);

                    AssetManager.XferReceivedCallback xferCallback =
                        delegate(XferDownload xfer)
                        {
                            if (xfer.XferID == xferID)
                            {
                                assetData = xfer.AssetData;
                                taskDownloadEvent.Set();
                            }
                        };

                    _Client.Assets.OnXferReceived += xferCallback;

                    // Start the actual asset xfer
                    xferID = _Client.Assets.RequestAssetXfer(filename, true, false, UUID.Zero, AssetType.Unknown, true);

                    if (taskDownloadEvent.WaitOne(timeoutMS, false))
                    {
                        _Client.Assets.OnXferReceived -= xferCallback;

                        string taskList = Utils.BytesToString(assetData);
                        return ParseTaskInventory(taskList);
                    }
                    else
                    {
                        Logger.Log("Timed out waiting for task inventory download for " + filename, Helpers.LogLevel.Warning, _Client);
                        _Client.Assets.OnXferReceived -= xferCallback;
                        return null;
                    }
                }
                else
                {
                    Logger.DebugLog("Task is empty for " + objectLocalID, _Client);
                    return null;
                }
            }
            else
            {
                Logger.Log("Timed out waiting for task inventory reply for " + objectLocalID, Helpers.LogLevel.Warning, _Client);
                OnTaskInventoryReply -= callback;
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectLocalID"></param>
        public void RequestTaskInventory(uint objectLocalID)
        {
            RequestTaskInventory(objectLocalID, _Client.Network.CurrentSim);
        }

        /// <summary>
        /// Request the contents of a tasks (primitives) inventory
        /// </summary>
        /// <param name="objectLocalID">The simulator Local ID of the object</param>
        /// <param name="simulator">A reference to the simulator object that contains the object</param>
        public void RequestTaskInventory(uint objectLocalID, Simulator simulator)
        {
            RequestTaskInventoryPacket request = new RequestTaskInventoryPacket();
            request.AgentData.AgentID = _Client.Self.AgentID;
            request.AgentData.SessionID = _Client.Self.SessionID;
            request.InventoryData.LocalID = objectLocalID;

            _Client.Network.SendPacket(request, simulator);
        }
        
        /// <summary>
        /// Moves an Item from an objects (Prim) Inventory to the specified folder in the avatars inventory
        /// </summary>
        /// <param name="objectLocalID">LocalID of the object in the simulator</param>
        /// <param name="taskItemID">UUID of the task item to move</param>
        /// <param name="inventoryFolderID">UUID of the folder to move the item to</param>
        /// <param name="simulator">Simulator Object</param>
        public void MoveTaskInventory(uint objectLocalID, UUID taskItemID, UUID inventoryFolderID, Simulator simulator)
        {
            MoveTaskInventoryPacket request = new MoveTaskInventoryPacket();
            request.AgentData.AgentID = _Client.Self.AgentID;
            request.AgentData.SessionID = _Client.Self.SessionID;

            request.AgentData.FolderID = inventoryFolderID;

            request.InventoryData.ItemID = taskItemID;
            request.InventoryData.LocalID = objectLocalID;

            _Client.Network.SendPacket(request, simulator);
        }
        
        /// <summary>
        /// Remove an item from an objects (Prim) Inventory
        /// </summary>
        /// <param name="objectLocalID">LocalID of the object in the simulator</param>
        /// <param name="taskItemID">UUID of the task item to remove</param>
        /// <param name="simulator">Simulator Object</param>
        public void RemoveTaskInventory(uint objectLocalID, UUID taskItemID, Simulator simulator)
        {
            RemoveTaskInventoryPacket remove = new RemoveTaskInventoryPacket();
            remove.AgentData.AgentID = _Client.Self.AgentID;
            remove.AgentData.SessionID = _Client.Self.SessionID;

            remove.InventoryData.ItemID = taskItemID;
            remove.InventoryData.LocalID = objectLocalID;

            _Client.Network.SendPacket(remove, simulator);
        }

        #endregion Task

        #region Helper Functions

        /// <summary>
        /// Takes an AssetType and returns the string representation
        /// </summary>
        /// <param name="type">The source <seealso cref="AssetType"/></param>
        /// <returns>The string version of the AssetType</returns>
        public static string AssetTypeToString(AssetType type)
        {
            return _AssetTypeNames[(int)type];
        }

        /// <summary>
        /// Translate a string name of an AssetType into the proper Type
        /// </summary>
        /// <param name="type">A string containing the AssetType name</param>
        /// <returns>The AssetType which matches the string name, or AssetType.Unknown if no match was found</returns>
        public static AssetType StringToAssetType(string type)
        {
            for (int i = 0; i < _AssetTypeNames.Length; i++)
            {
                if (_AssetTypeNames[i] == type)
                    return (AssetType)i;
            }

            return AssetType.Unknown;
        }

        /// <summary>
        /// Convert an InventoryType to a string
        /// </summary>
        /// <param name="type">The <seealso cref="T:InventoryType"/> to convert</param>
        /// <returns>A string representation of the source </returns>
        public static string InventoryTypeToString(InventoryType type)
        {
            return _InventoryTypeNames[(int)type];
        }

        /// <summary>
        /// Convert a string into a valid InventoryType
        /// </summary>
        /// <param name="type">A string representation of the InventoryType to convert</param>
        /// <returns>A InventoryType object which matched the type</returns>
        public static InventoryType StringToInventoryType(string type)
        {
            for (int i = 0; i < _InventoryTypeNames.Length; i++)
            {
                if (_InventoryTypeNames[i] == type)
                    return (InventoryType)i;
            }

            return InventoryType.Unknown;
        }

        public static string SaleTypeToString(SaleType type)
        {
            return _SaleTypeNames[(int)type];
        }

        public static SaleType StringToSaleType(string value)
        {
            for (int i = 0; i < _SaleTypeNames.Length; i++)
            {
                if (value == _SaleTypeNames[i])
                    return (SaleType)i;
            }

            return SaleType.Not;
        }

        private uint RegisterItemCreatedCallback(ItemCreatedCallback callback)
        {
            lock (_CallbacksLock)
            {
                if (_CallbackPos == UInt32.MaxValue)
                    _CallbackPos = 0;

                _CallbackPos++;

                if (_ItemCreatedCallbacks.ContainsKey(_CallbackPos))
                    Logger.Log("Overwriting an existing ItemCreatedCallback", Helpers.LogLevel.Warning, _Client);

                _ItemCreatedCallbacks[_CallbackPos] = callback;

                return _CallbackPos;
            }
        }

        private uint RegisterItemsCopiedCallback(ItemCopiedCallback callback)
        {
            lock (_CallbacksLock)
            {
                if (_CallbackPos == UInt32.MaxValue)
                    _CallbackPos = 0;

                _CallbackPos++;

                if (_ItemCopiedCallbacks.ContainsKey(_CallbackPos))
                    Logger.Log("Overwriting an existing ItemsCopiedCallback", Helpers.LogLevel.Warning, _Client);

                _ItemCopiedCallbacks[_CallbackPos] = callback;

                return _CallbackPos;
            }
        }

        /// <summary>
        /// Create a CRC from an InventoryItem
        /// </summary>
        /// <param name="iitem">The source InventoryItem</param>
        /// <returns>A uint representing the source InventoryItem as a CRC</returns>
        public static uint ItemCRC(InventoryItem iitem)
        {
            uint CRC = 0;

            // IDs
            CRC += iitem.AssetUUID.CRC(); // AssetID
            CRC += iitem.ParentUUID.CRC(); // FolderID
            CRC += iitem.UUID.CRC(); // ItemID

            // Permission stuff
            CRC += iitem.CreatorID.CRC(); // CreatorID
            CRC += iitem.OwnerID.CRC(); // OwnerID
            CRC += iitem.GroupID.CRC(); // GroupID

            // CRC += another 4 words which always seem to be zero -- unclear if this is a UUID or what
            CRC += (uint)iitem.Permissions.OwnerMask; //owner_mask;      // Either owner_mask or next_owner_mask may need to be
            CRC += (uint)iitem.Permissions.NextOwnerMask; //next_owner_mask; // switched with base_mask -- 2 values go here and in my
            CRC += (uint)iitem.Permissions.EveryoneMask; //everyone_mask;   // study item, the three were identical.
            CRC += (uint)iitem.Permissions.GroupMask; //group_mask;

            // The rest of the CRC fields
            CRC += (uint)iitem.Flags; // Flags
            CRC += (uint)iitem.InventoryType; // InvType
            CRC += (uint)iitem.AssetType; // Type 
            CRC += (uint)Utils.DateTimeToUnixTime(iitem.CreationDate); // CreationDate
            CRC += (uint)iitem.SalePrice;    // SalePrice
            CRC += (uint)((uint)iitem.SaleType * 0x07073096); // SaleType

            return CRC;
        }

        /// <summary>
        /// Wrapper for creating a new <seealso cref="InventoryItem"/> object
        /// </summary>
        /// <param name="type">The type of item from the <seealso cref="InventoryType"/> enum</param>
        /// <param name="id">The <seealso cref="UUID"/> of the newly created object</param>
        /// <returns>An <seealso cref="InventoryItem"/> object with the type and id passed</returns>
        public static InventoryItem CreateInventoryItem(InventoryType type, UUID id)
        {
            switch (type)
            {
                case InventoryType.Texture: return new InventoryTexture(id);
                case InventoryType.Sound: return new InventorySound(id);
                case InventoryType.CallingCard: return new InventoryCallingCard(id);
                case InventoryType.Landmark: return new InventoryLandmark(id);
                case InventoryType.Object: return new InventoryObject(id);
                case InventoryType.Notecard: return new InventoryNotecard(id);
                case InventoryType.Category: return new InventoryCategory(id);
                case InventoryType.LSL: return new InventoryLSL(id);
                case InventoryType.Snapshot: return new InventorySnapshot(id);
                case InventoryType.Attachment: return new InventoryAttachment(id);
                case InventoryType.Wearable: return new InventoryWearable(id);
                case InventoryType.Animation: return new InventoryAnimation(id);
                case InventoryType.Gesture: return new InventoryGesture(id);
                default: return new InventoryItem(type, id);
            }
        }

        private InventoryItem SafeCreateInventoryItem(InventoryType InvType, UUID ItemID)
        {
            InventoryItem ret = null;

            if (_Store.Contains(ItemID))
                ret = _Store[ItemID] as InventoryItem;

            if (ret == null)
                ret = CreateInventoryItem(InvType, ItemID);

            return ret;
        }

        private static bool ParseLine(string line, out string key, out string value)
        {
            // Clean up and convert tabs to spaces
            line = line.Trim();
            line = line.Replace('\t', ' ');

            // Shrink all whitespace down to single spaces
            while (line.IndexOf("  ") > 0)
                line = line.Replace("  ", " ");

            if (line.Length > 2)
            {
                int sep = line.IndexOf(' ');
                if (sep > 0)
                {
                    key = line.Substring(0, sep);
                    value = line.Substring(sep + 1);

                    return true;
                }
            }
            else if (line.Length == 1)
            {
                key = line;
                value = String.Empty;
                return true;
            }

            key = null;
            value = null;
            return false;
        }

        /// <summary>
        /// Parse the results of a RequestTaskInventory() response
        /// </summary>
        /// <param name="taskData">A string which contains the data from the task reply</param>
        /// <returns>A List containing the items contained within the tasks inventory</returns>
        public static List<InventoryBase> ParseTaskInventory(string taskData)
        {
            List<InventoryBase> items = new List<InventoryBase>();
            int lineNum = 0;
            string[] lines = taskData.Replace("\r\n", "\n").Split('\n');

            while (lineNum < lines.Length)
            {
                string key, value;
                if (ParseLine(lines[lineNum++], out key, out value))
                {
                    if (key == "inv_object")
                    {
                        #region inv_object

                        // In practice this appears to only be used for folders
                        UUID itemID = UUID.Zero;
                        UUID parentID = UUID.Zero;
                        string name = String.Empty;
                        AssetType assetType = AssetType.Unknown;

                        while (lineNum < lines.Length)
                        {
                            if (ParseLine(lines[lineNum++], out key, out value))
                            {
                                if (key == "{")
                                {
                                    continue;
                                }
                                else if (key == "}")
                                {
                                    break;
                                }
                                else if (key == "obj_id")
                                {
                                    UUID.TryParse(value, out itemID);
                                }
                                else if (key == "parent_id")
                                {
                                    UUID.TryParse(value, out parentID);
                                }
                                else if (key == "type")
                                {
                                    assetType = StringToAssetType(value);
                                }
                                else if (key == "name")
                                {
                                    name = value.Substring(0, value.IndexOf('|'));
                                }
                            }
                        }

                        if (assetType == AssetType.Folder)
                        {
                            InventoryFolder folder = new InventoryFolder(itemID);
                            folder.Name = name;
                            folder.ParentUUID = parentID;

                            items.Add(folder);
                        }
                        else
                        {
                            InventoryItem item = new InventoryItem(itemID);
                            item.Name = name;
                            item.ParentUUID = parentID;
                            item.AssetType = assetType;

                            items.Add(item);
                        }

                        #endregion inv_object
                    }
                    else if (key == "inv_item")
                    {
                        #region inv_item

                        // Any inventory item that links to an assetID, has permissions, etc
                        UUID itemID = UUID.Zero;
                        UUID assetID = UUID.Zero;
                        UUID parentID = UUID.Zero;
                        UUID creatorID = UUID.Zero;
                        UUID ownerID = UUID.Zero;
                        UUID lastOwnerID = UUID.Zero;
                        UUID groupID = UUID.Zero;
                        bool groupOwned = false;
                        string name = String.Empty;
                        string desc = String.Empty;
                        AssetType assetType = AssetType.Unknown;
                        InventoryType inventoryType = InventoryType.Unknown;
                        DateTime creationDate = Utils.Epoch;
                        uint flags = 0;
                        Permissions perms = Permissions.NoPermissions;
                        SaleType saleType = SaleType.Not;
                        int salePrice = 0;

                        while (lineNum < lines.Length)
                        {
                            if (ParseLine(lines[lineNum++], out key, out value))
                            {
                                if (key == "{")
                                {
                                    continue;
                                }
                                else if (key == "}")
                                {
                                    break;
                                }
                                else if (key == "item_id")
                                {
                                    UUID.TryParse(value, out itemID);
                                }
                                else if (key == "parent_id")
                                {
                                    UUID.TryParse(value, out parentID);
                                }
                                else if (key == "permissions")
                                {
                                    #region permissions

                                    while (lineNum < lines.Length)
                                    {
                                        if (ParseLine(lines[lineNum++], out key, out value))
                                        {
                                            if (key == "{")
                                            {
                                                continue;
                                            }
                                            else if (key == "}")
                                            {
                                                break;
                                            }
                                            else if (key == "creator_mask")
                                            {
                                                // Deprecated
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.BaseMask = (PermissionMask)val;
                                            }
                                            else if (key == "base_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.BaseMask = (PermissionMask)val;
                                            }
                                            else if (key == "owner_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.OwnerMask = (PermissionMask)val;
                                            }
                                            else if (key == "group_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.GroupMask = (PermissionMask)val;
                                            }
                                            else if (key == "everyone_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.EveryoneMask = (PermissionMask)val;
                                            }
                                            else if (key == "next_owner_mask")
                                            {
                                                uint val;
                                                if (Utils.TryParseHex(value, out val))
                                                    perms.NextOwnerMask = (PermissionMask)val;
                                            }
                                            else if (key == "creator_id")
                                            {
                                                
                                                UUID.TryParse(value, out creatorID);
                                            }
                                            else if (key == "owner_id")
                                            {
                                                UUID.TryParse(value, out ownerID);
                                            }
                                            else if (key == "last_owner_id")
                                            {
                                                UUID.TryParse(value, out lastOwnerID);
                                            }
                                            else if (key == "group_id")
                                            {
                                                UUID.TryParse(value, out groupID);
                                            }
                                            else if (key == "group_owned")
                                            {
                                                uint val;
                                                if (UInt32.TryParse(value, out val))
                                                    groupOwned = (val != 0);
                                            }
                                        }
                                    }

                                    #endregion permissions
                                }
                                else if (key == "sale_info")
                                {
                                    #region sale_info

                                    while (lineNum < lines.Length)
                                    {
                                        if (ParseLine(lines[lineNum++], out key, out value))
                                        {
                                            if (key == "{")
                                            {
                                                continue;
                                            }
                                            else if (key == "}")
                                            {
                                                break;
                                            }
                                            else if (key == "sale_type")
                                            {
                                                saleType = StringToSaleType(value);
                                            }
                                            else if (key == "sale_price")
                                            {
                                                Int32.TryParse(value, out salePrice);
                                            }
                                        }
                                    }

                                    #endregion sale_info
                                }
                                else if (key == "shadow_id")
                                {
                                    //FIXME:
                                }
                                else if (key == "asset_id")
                                {
                                    UUID.TryParse(value, out assetID);
                                }
                                else if (key == "type")
                                {
                                    assetType = StringToAssetType(value);
                                }
                                else if (key == "inv_type")
                                {
                                    inventoryType = StringToInventoryType(value);
                                }
                                else if (key == "flags")
                                {
                                    UInt32.TryParse(value, out flags);
                                }
                                else if (key == "name")
                                {
                                    name = value.Substring(0, value.IndexOf('|'));
                                }
                                else if (key == "desc")
                                {
                                    desc = value.Substring(0, value.IndexOf('|'));
                                }
                                else if (key == "creation_date")
                                {
                                    uint timestamp;
                                    if (UInt32.TryParse(value, out timestamp))
                                        creationDate = Utils.UnixTimeToDateTime(timestamp);
                                    else
                                        Logger.Log("Failed to parse creation_date " + value, Helpers.LogLevel.Warning);
                                }
                            }
                        }

                        InventoryItem item = CreateInventoryItem(inventoryType, itemID);
                        item.AssetUUID = assetID;
                        item.AssetType = assetType;
                        item.CreationDate = creationDate;
                        item.CreatorID = creatorID;
                        item.Description = desc;
                        item.Flags = flags;
                        item.GroupID = groupID;
                        item.GroupOwned = groupOwned;
                        item.Name = name;
                        item.OwnerID = ownerID;
                        item.ParentUUID = parentID;
                        item.Permissions = perms;
                        item.SalePrice = salePrice;
                        item.SaleType = saleType;

                        items.Add(item);

                        #endregion inv_item
                    }
                    else
                    {
                        Logger.Log("Unrecognized token " + key + " in: " + Environment.NewLine + taskData,
                            Helpers.LogLevel.Error);
                    }
                }
            }

            return items;
        }
        
        #endregion Helper Functions

        #region Callbacks

        private void CreateItemFromAssetResponse(CapsClient client, OSD result, Exception error)
        {
            object[] args = (object[])client.UserData;
            CapsClient.ProgressCallback progCallback = (CapsClient.ProgressCallback)args[0];
            ItemCreatedFromAssetCallback callback = (ItemCreatedFromAssetCallback)args[1];
            byte[] itemData = (byte[])args[2];

            if (result == null)
            {
                try { callback(false, error.Message, UUID.Zero, UUID.Zero); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                return;
            }

            OSDMap contents = (OSDMap)result;

            string status = contents["state"].AsString().ToLower();

            if (status == "upload")
            {
                string uploadURL = contents["uploader"].AsString();

                Logger.DebugLog("CreateItemFromAsset: uploading to " + uploadURL);

                // This makes the assumption that all uploads go to CurrentSim, to avoid
                // the problem of HttpRequestState not knowing anything about simulators
                CapsClient upload = new CapsClient(new Uri(uploadURL));
                upload.OnProgress += progCallback;
                upload.OnComplete += new CapsClient.CompleteCallback(CreateItemFromAssetResponse);
                upload.UserData = new object[] { null, callback, itemData };
                upload.StartRequest(itemData, "application/octet-stream");
            }
            else if (status == "complete")
            {
                Logger.DebugLog("CreateItemFromAsset: completed"); 

                if (contents.ContainsKey("new_inventory_item") && contents.ContainsKey("new_asset"))
                {
                    try { callback(true, String.Empty, contents["new_inventory_item"].AsUUID(), contents["new_asset"].AsUUID()); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
                else
                {
                    try { callback(false, "Failed to parse asset and item UUIDs", UUID.Zero, UUID.Zero); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
            }
            else
            {
                // Failure
                try { callback(false, status, UUID.Zero, UUID.Zero); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
            }
        }

        private void SaveAssetIntoInventoryHandler(Packet packet, Simulator simulator)
        {
            //SaveAssetIntoInventoryPacket save = (SaveAssetIntoInventoryPacket)packet;

            // FIXME: Find this item in the inventory structure and mark the parent as needing an update
            //save.InventoryData.ItemID;
            Logger.Log("SaveAssetIntoInventory packet received, someone write this function!", Helpers.LogLevel.Error, _Client);
        }

        private void InventoryDescendentsHandler(Packet packet, Simulator simulator)
        {
            InventoryDescendentsPacket reply = (InventoryDescendentsPacket)packet;

            if (reply.AgentData.Descendents > 0)
            {
                // InventoryDescendantsReply sends a null folder if the parent doesnt contain any folders
                if (reply.FolderData[0].FolderID != UUID.Zero)
                {
                    // Iterate folders in this packet
                    for (int i = 0; i < reply.FolderData.Length; i++)
                    {
                        InventoryFolder folder = new InventoryFolder(reply.FolderData[i].FolderID);
                        folder.ParentUUID = reply.FolderData[i].ParentID;
                        folder.Name = Utils.BytesToString(reply.FolderData[i].Name);
                        folder.PreferredType = (AssetType)reply.FolderData[i].Type;
                        folder.OwnerID = reply.AgentData.OwnerID;

                        _Store[folder.UUID] = folder;
                    }
                }

                // InventoryDescendantsReply sends a null item if the parent doesnt contain any items.
                if (reply.ItemData[0].ItemID != UUID.Zero)
                {
                    // Iterate items in this packet
                    for (int i = 0; i < reply.ItemData.Length; i++)
                    {
                        if (reply.ItemData[i].ItemID != UUID.Zero)
                        {
                            InventoryItem item;
                            /* 
                             * Objects that have been attached in-world prior to being stored on the 
                             * asset server are stored with the InventoryType of 0 (Texture) 
                             * instead of 17 (Attachment) 
                             * 
                             * This corrects that behavior by forcing Object Asset types that have an 
                             * invalid InventoryType with the proper InventoryType of Attachment.
                             */
                            if ((AssetType)reply.ItemData[i].Type == AssetType.Object 
                                && (InventoryType)reply.ItemData[i].InvType == InventoryType.Texture)
                            {
                                item = CreateInventoryItem(InventoryType.Attachment, reply.ItemData[i].ItemID);
                                item.InventoryType = InventoryType.Attachment;
                            }
                            else
                            {
                                item = CreateInventoryItem((InventoryType)reply.ItemData[i].InvType, reply.ItemData[i].ItemID);
                                item.InventoryType = (InventoryType)reply.ItemData[i].InvType;
                            }
                            
                            item.ParentUUID = reply.ItemData[i].FolderID;
                            item.CreatorID = reply.ItemData[i].CreatorID;
                            item.AssetType = (AssetType)reply.ItemData[i].Type;
                            item.AssetUUID = reply.ItemData[i].AssetID;
                            item.CreationDate = Utils.UnixTimeToDateTime((uint)reply.ItemData[i].CreationDate);
                            item.Description = Utils.BytesToString(reply.ItemData[i].Description);
                            item.Flags = reply.ItemData[i].Flags;
                            item.Name = Utils.BytesToString(reply.ItemData[i].Name);
                            item.GroupID = reply.ItemData[i].GroupID;
                            item.GroupOwned = reply.ItemData[i].GroupOwned;
                            item.Permissions = new Permissions(
                                reply.ItemData[i].BaseMask,
                                reply.ItemData[i].EveryoneMask,
                                reply.ItemData[i].GroupMask,
                                reply.ItemData[i].NextOwnerMask,
                                reply.ItemData[i].OwnerMask);
                            item.SalePrice = reply.ItemData[i].SalePrice;
                            item.SaleType = (SaleType)reply.ItemData[i].SaleType;
                            item.OwnerID = reply.AgentData.OwnerID;

                            _Store[item.UUID] = item;
                        }
                    }
                }
            }

            InventoryFolder parentFolder = null;

            if (_Store.Contains(reply.AgentData.FolderID) &&
                _Store[reply.AgentData.FolderID] is InventoryFolder)
            {
                parentFolder = _Store[reply.AgentData.FolderID] as InventoryFolder;
            }
            else
            {
                Logger.Log("Don't have a reference to FolderID " + reply.AgentData.FolderID.ToString() +
                    " or it is not a folder", Helpers.LogLevel.Error, _Client);
                return;
            }

            if (reply.AgentData.Version < parentFolder.Version)
            {
                Logger.Log("Got an outdated InventoryDescendents packet for folder " + parentFolder.Name +
                    ", this version = " + reply.AgentData.Version + ", latest version = " + parentFolder.Version,
                    Helpers.LogLevel.Warning, _Client);
                return;
            }

            parentFolder.Version = reply.AgentData.Version;
            // FIXME: reply.AgentData.Descendants is not parentFolder.DescendentCount if we didn't 
            // request items and folders
            parentFolder.DescendentCount = reply.AgentData.Descendents;

            #region FindObjectsByPath Handling

            if (_Searches.Count > 0)
            {
                lock (_Searches)
                {
                StartSearch:

                    // Iterate over all of the outstanding searches
                    for (int i = 0; i < _Searches.Count; i++)
                    {
                        InventorySearch search = _Searches[i];
                        List<InventoryBase> folderContents = _Store.GetContents(search.Folder);

                        // Iterate over all of the inventory objects in the base search folder
                        for (int j = 0; j < folderContents.Count; j++)
                        {
                            // Check if this inventory object matches the current path node
                            if (folderContents[j].Name == search.Path[search.Level])
                            {
                                if (search.Level == search.Path.Length - 1)
                                {
                                    Logger.DebugLog("Finished path search of " + String.Join("/", search.Path), _Client);

                                    // This is the last node in the path, fire the callback and clean up
                                    if (OnFindObjectByPath != null)
                                    {
                                        try { OnFindObjectByPath(String.Join("/", search.Path), folderContents[j].UUID); }
                                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                                    }

                                    // Remove this entry and restart the loop since we are changing the collection size
                                    _Searches.RemoveAt(i);
                                    goto StartSearch;
                                }
                                else
                                {
                                    // We found a match but it is not the end of the path, request the next level
                                    Logger.DebugLog(String.Format("Matched level {0}/{1} in a path search of {2}",
                                        search.Level, search.Path.Length - 1, String.Join("/", search.Path)), _Client);

                                    search.Folder = folderContents[j].UUID;
                                    search.Level++;
                                    _Searches[i] = search;

                                    RequestFolderContents(search.Folder, search.Owner, true, true, 
                                        InventorySortOrder.ByName);
                                }
                            }
                        }
                    }
                }
            }

            #endregion FindObjectsByPath Handling

            // Callback for inventory folder contents being updated
            if (OnFolderUpdated != null)
            {
                try { OnFolderUpdated(parentFolder.UUID); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
            }
        }

        /// <summary>
        /// UpdateCreateInventoryItem packets are received when a new inventory item 
        /// is created. This may occur when an object that's rezzed in world is
        /// taken into inventory, when an item is created using the CreateInventoryItem
        /// packet, or when an object is purchased
        /// </summary>
        private void UpdateCreateInventoryItemHandler(Packet packet, Simulator simulator)
        {
            UpdateCreateInventoryItemPacket reply = packet as UpdateCreateInventoryItemPacket;

            foreach (UpdateCreateInventoryItemPacket.InventoryDataBlock dataBlock in reply.InventoryData)
            {
                if (dataBlock.InvType == (sbyte)InventoryType.Folder)
                {
                    Logger.Log("Received InventoryFolder in an UpdateCreateInventoryItem packet, this should not happen!",
                        Helpers.LogLevel.Error, _Client);
                    continue;
                }

                InventoryItem item = CreateInventoryItem((InventoryType)dataBlock.InvType,dataBlock.ItemID);
                item.AssetType = (AssetType)dataBlock.Type;
                item.AssetUUID = dataBlock.AssetID;
                item.CreationDate = Utils.UnixTimeToDateTime(dataBlock.CreationDate);
                item.CreatorID = dataBlock.CreatorID;
                item.Description = Utils.BytesToString(dataBlock.Description);
                item.Flags = dataBlock.Flags;
                item.GroupID = dataBlock.GroupID;
                item.GroupOwned = dataBlock.GroupOwned;
                item.Name = Utils.BytesToString(dataBlock.Name);
                item.OwnerID = dataBlock.OwnerID;
                item.ParentUUID = dataBlock.FolderID;
                item.Permissions = new Permissions(
                        dataBlock.BaseMask,
                        dataBlock.EveryoneMask,
                        dataBlock.GroupMask,
                        dataBlock.NextOwnerMask,
                        dataBlock.OwnerMask);
                item.SalePrice = dataBlock.SalePrice;
                item.SaleType = (SaleType)dataBlock.SaleType;

                /* 
                 * When attaching new objects, an UpdateCreateInventoryItem packet will be
                 * returned by the server that has a FolderID/ParentUUID of zero. It is up
                 * to the client to make sure that the item gets a good folder, otherwise
                 * it will end up inaccesible in inventory.
                 */
                if (item.ParentUUID == UUID.Zero)
                {
                    // assign default folder for type
                    item.ParentUUID = FindFolderForType(item.AssetType);

                    // send update to the sim
                    RequestUpdateItem(item);
                }

                // Update the local copy
                _Store[item.UUID] = item;

                // Look for an "item created" callback
                ItemCreatedCallback createdCallback;
                if (_ItemCreatedCallbacks.TryGetValue(dataBlock.CallbackID, out createdCallback))
                {
                    _ItemCreatedCallbacks.Remove(dataBlock.CallbackID);

                    try { createdCallback(true, item); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }

                // TODO: Is this callback even triggered when items are copied?
                // Look for an "item copied" callback
                ItemCopiedCallback copyCallback;
                if (_ItemCopiedCallbacks.TryGetValue(dataBlock.CallbackID, out copyCallback))
                {
                    _ItemCopiedCallbacks.Remove(dataBlock.CallbackID);

                    try { copyCallback(item); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
                
                //This is triggered when an item is received from a task
                if (OnTaskItemReceived != null)
                {
                    try { OnTaskItemReceived(item.UUID, dataBlock.FolderID, item.CreatorID, item.AssetUUID, 
                        item.InventoryType); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
            }
        }

        private void MoveInventoryItemHandler(Packet packet, Simulator simulator)
        {
            MoveInventoryItemPacket move = (MoveInventoryItemPacket)packet;

            for (int i = 0; i < move.InventoryData.Length; i++)
            {
                // FIXME: Do something here
                string newName = Utils.BytesToString(move.InventoryData[i].NewName);

                Logger.Log(String.Format(
                    "MoveInventoryItemHandler: Item {0} is moving to Folder {1} with new name \"{2}\". Someone write this function!",
                    move.InventoryData[i].ItemID.ToString(), move.InventoryData[i].FolderID.ToString(),
                    newName), Helpers.LogLevel.Warning, _Client);
            }
        }

        private void BulkUpdateInventoryHandler(Packet packet, Simulator simulator)
        {
            BulkUpdateInventoryPacket update = packet as BulkUpdateInventoryPacket;

            if (update.FolderData.Length > 0 && update.FolderData[0].FolderID != UUID.Zero)
            {
                foreach (BulkUpdateInventoryPacket.FolderDataBlock dataBlock in update.FolderData)
                {
                    if (!_Store.Contains(dataBlock.FolderID))
                        Logger.Log("Received BulkUpdate for unknown folder: " + dataBlock.FolderID, Helpers.LogLevel.Warning, _Client);

                    InventoryFolder folder = new InventoryFolder(dataBlock.FolderID);
                    folder.Name = Utils.BytesToString(dataBlock.Name);
                    folder.OwnerID = update.AgentData.AgentID;
                    folder.ParentUUID = dataBlock.ParentID;
                    _Store[folder.UUID] = folder;
                }
            }

            if (update.ItemData.Length > 0 && update.ItemData[0].ItemID != UUID.Zero)
            {
                for (int i = 0; i < update.ItemData.Length; i++)
                {
                    BulkUpdateInventoryPacket.ItemDataBlock dataBlock = update.ItemData[i];

                    // If we are given a folder of items, the item information might arrive before the folder
                    // (parent) is in the store
                    if (!_Store.Contains(dataBlock.ItemID))
                        Logger.Log("Received BulkUpdate for unknown item: " + dataBlock.ItemID, Helpers.LogLevel.Warning, _Client);

                    InventoryItem item = SafeCreateInventoryItem((InventoryType)dataBlock.InvType, dataBlock.ItemID);

                    item.AssetType = (AssetType)dataBlock.Type;
                    if (dataBlock.AssetID != UUID.Zero) item.AssetUUID = dataBlock.AssetID;
                    item.CreationDate = Utils.UnixTimeToDateTime(dataBlock.CreationDate);
                    item.CreatorID = dataBlock.CreatorID;
                    item.Description = Utils.BytesToString(dataBlock.Description);
                    item.Flags = dataBlock.Flags;
                    item.GroupID = dataBlock.GroupID;
                    item.GroupOwned = dataBlock.GroupOwned;
                    item.Name = Utils.BytesToString(dataBlock.Name);
                    item.OwnerID = dataBlock.OwnerID;
                    item.ParentUUID = dataBlock.FolderID;
                    item.Permissions = new Permissions(
                        dataBlock.BaseMask,
                        dataBlock.EveryoneMask,
                        dataBlock.GroupMask,
                        dataBlock.NextOwnerMask,
                        dataBlock.OwnerMask);
                    item.SalePrice = dataBlock.SalePrice;
                    item.SaleType = (SaleType)dataBlock.SaleType;

                    _Store[item.UUID] = item;

                    // Look for an "item created" callback
                    ItemCreatedCallback callback;
                    if (_ItemCreatedCallbacks.TryGetValue(dataBlock.CallbackID, out callback))
                    {
                        _ItemCreatedCallbacks.Remove(dataBlock.CallbackID);

                        try { callback(true, item); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                    }

                    // Look for an "item copied" callback
                    ItemCopiedCallback copyCallback;
                    if (_ItemCopiedCallbacks.TryGetValue(dataBlock.CallbackID, out copyCallback))
                    {
                        _ItemCopiedCallbacks.Remove(dataBlock.CallbackID);

                        try { copyCallback(item); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                    }
                }
            }
        }

        private void FetchInventoryReplyHandler(Packet packet, Simulator simulator)
        {
            FetchInventoryReplyPacket reply = packet as FetchInventoryReplyPacket;

            foreach (FetchInventoryReplyPacket.InventoryDataBlock dataBlock in reply.InventoryData) 
            {
                if (dataBlock.InvType == (sbyte)InventoryType.Folder)
                {
                    Logger.Log("Received FetchInventoryReply for an inventory folder, this should not happen!",
                        Helpers.LogLevel.Error, _Client);
                    continue;
                }

                InventoryItem item = CreateInventoryItem((InventoryType)dataBlock.InvType,dataBlock.ItemID);
                item.AssetType = (AssetType)dataBlock.Type;
                item.AssetUUID = dataBlock.AssetID;
                item.CreationDate = Utils.UnixTimeToDateTime(dataBlock.CreationDate);
                item.CreatorID = dataBlock.CreatorID;
                item.Description = Utils.BytesToString(dataBlock.Description);
                item.Flags = dataBlock.Flags;
                item.GroupID = dataBlock.GroupID;
                item.GroupOwned = dataBlock.GroupOwned;
                item.Name = Utils.BytesToString(dataBlock.Name);
                item.OwnerID = dataBlock.OwnerID;
                item.ParentUUID = dataBlock.FolderID;
                item.Permissions = new Permissions(
                    dataBlock.BaseMask, 
                    dataBlock.EveryoneMask, 
                    dataBlock.GroupMask, 
                    dataBlock.NextOwnerMask, 
                    dataBlock.OwnerMask);
                item.SalePrice = dataBlock.SalePrice;
                item.SaleType = (SaleType)dataBlock.SaleType;

                _Store[item.UUID] = item;

                // Fire the callback for an item being fetched
                if (OnItemReceived != null)
                {
                    try { OnItemReceived(item); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
            }
        }

        private void ReplyTaskInventoryHandler(Packet packet, Simulator simulator)
        {
            if (OnTaskInventoryReply != null)
            {
                ReplyTaskInventoryPacket reply = (ReplyTaskInventoryPacket)packet;

                try
                {
                    OnTaskInventoryReply(reply.InventoryData.TaskID, reply.InventoryData.Serial,
                        Utils.BytesToString(reply.InventoryData.Filename));
                }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
            }
        }

        private void Self_OnInstantMessage(InstantMessage im, Simulator simulator)
        {
            // TODO: MainAvatar.InstantMessageDialog.GroupNotice can also be an inventory offer, should we
            // handle it here?

            if (OnObjectOffered != null && 
                (im.Dialog == InstantMessageDialog.InventoryOffered 
                || im.Dialog == InstantMessageDialog.TaskInventoryOffered))
            {
                AssetType type = AssetType.Unknown;
                UUID objectID = UUID.Zero;
                bool fromTask = false;

                if (im.Dialog == InstantMessageDialog.InventoryOffered)
                {
                    if (im.BinaryBucket.Length == 17)
                    {
                        type = (AssetType)im.BinaryBucket[0];
                        objectID = new UUID(im.BinaryBucket, 1);
                        fromTask = false;
                    }
                    else
                    {
                        Logger.Log("Malformed inventory offer from agent", Helpers.LogLevel.Warning, _Client);
                        return;
                    }
                }
                else if (im.Dialog == InstantMessageDialog.TaskInventoryOffered)
                {
                    if (im.BinaryBucket.Length == 1)
                    {
                        type = (AssetType)im.BinaryBucket[0];
                        fromTask = true;
                    }
                    else
                    {
                        Logger.Log("Malformed inventory offer from object", Helpers.LogLevel.Warning, _Client);
                        return;
                    }
                }

                // Find the folder where this is going to go
                UUID destinationFolderID = FindFolderForType(type);

                // Fire the callback
                try
                {
                    ImprovedInstantMessagePacket imp = new ImprovedInstantMessagePacket();
                    imp.AgentData.AgentID = _Client.Self.AgentID;
                    imp.AgentData.SessionID = _Client.Self.SessionID;
                    imp.MessageBlock.FromGroup = false;
                    imp.MessageBlock.ToAgentID = im.FromAgentID;
                    imp.MessageBlock.Offline = 0;
                    imp.MessageBlock.ID = im.IMSessionID;
                    imp.MessageBlock.Timestamp = 0;
                    imp.MessageBlock.FromAgentName = Utils.StringToBytes(_Client.Self.Name);
                    imp.MessageBlock.Message = Utils.EmptyBytes;
                    imp.MessageBlock.ParentEstateID = 0;
                    imp.MessageBlock.RegionID = UUID.Zero;
                    imp.MessageBlock.Position = _Client.Self.SimPosition;

                    if (OnObjectOffered(im, type, objectID, fromTask))
                    {
                        // Accept the inventory offer
                        switch (im.Dialog)
                        {
                            case InstantMessageDialog.InventoryOffered:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.InventoryAccepted;
                                break;
                            case InstantMessageDialog.TaskInventoryOffered:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.TaskInventoryAccepted;
                                break;
                            case InstantMessageDialog.GroupNotice:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.GroupNoticeInventoryAccepted;
                                break;
                        }

                        imp.MessageBlock.BinaryBucket = destinationFolderID.GetBytes();
                    }
                    else
                    {
                        // Decline the inventory offer
                        switch (im.Dialog)
                        {
                            case InstantMessageDialog.InventoryOffered:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.InventoryDeclined;
                                break;
                            case InstantMessageDialog.TaskInventoryOffered:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.TaskInventoryDeclined;
                                break;
                            case InstantMessageDialog.GroupNotice:
                                imp.MessageBlock.Dialog = (byte)InstantMessageDialog.GroupNoticeInventoryDeclined;
                                break;
                        }

                        imp.MessageBlock.BinaryBucket = Utils.EmptyBytes;
                    }

                    _Client.Network.SendPacket(imp, simulator);
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e);
                }
            }
        }
        
        private void Network_OnLoginResponse(bool loginSuccess, bool redirect, string message, string reason, LoginResponseData replyData)
        {
            if (loginSuccess)
            {
                // Initialize the store here so we know who owns it:
                _Store = new Inventory(_Client, this, _Client.Self.AgentID);
                Logger.DebugLog("Setting InventoryRoot to " + replyData.InventoryRoot.ToString(), _Client);
                InventoryFolder rootFolder = new InventoryFolder(replyData.InventoryRoot);
                rootFolder.Name = String.Empty;
                rootFolder.ParentUUID = UUID.Zero;
                _Store.RootFolder = rootFolder;

                for (int i = 0; i < replyData.InventorySkeleton.Length; i++)
                    _Store.UpdateNodeFor(replyData.InventorySkeleton[i]);

                InventoryFolder libraryRootFolder = new InventoryFolder(replyData.LibraryRoot);
                libraryRootFolder.Name = String.Empty;
                libraryRootFolder.ParentUUID = UUID.Zero;
                _Store.LibraryFolder = libraryRootFolder;

                for(int i = 0; i < replyData.LibrarySkeleton.Length; i++)
                    _Store.UpdateNodeFor(replyData.LibrarySkeleton[i]);
            }
        }

        private void UploadNotecardAssetResponse(CapsClient client, OSD result, Exception error)
        {
            OSDMap contents = (OSDMap)result;
            KeyValuePair<NotecardUploadedAssetCallback, byte[]> kvp = (KeyValuePair<NotecardUploadedAssetCallback, byte[]>)(((object[])client.UserData)[0]);
            NotecardUploadedAssetCallback callback = kvp.Key;
            byte[] itemData = (byte[])kvp.Value;

            string status = contents["state"].AsString();

            if (status == "upload")
            {
                string uploadURL = contents["uploader"].AsString();

                // This makes the assumption that all uploads go to CurrentSim, to avoid
                // the problem of HttpRequestState not knowing anything about simulators
                CapsClient upload = new CapsClient(new Uri(uploadURL));
                upload.OnComplete += new CapsClient.CompleteCallback(UploadNotecardAssetResponse);
                upload.UserData = new object[2] { kvp, (UUID)(((object[])client.UserData)[1]) };
                upload.StartRequest(itemData, "application/octet-stream");
            }
            else if (status == "complete")
            {
                if (contents.ContainsKey("new_asset"))
                {
                    try { callback(true, String.Empty, (UUID)(((object[])client.UserData)[1]), contents["new_asset"].AsUUID()); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
                else
                {
                    try { callback(false, "Failed to parse asset and item UUIDs", UUID.Zero, UUID.Zero); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
                }
            }
            else
            {
                // Failure
                try { callback(false, status, UUID.Zero, UUID.Zero); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, _Client, e); }
            }
        }

        #endregion Callbacks
    }
}
