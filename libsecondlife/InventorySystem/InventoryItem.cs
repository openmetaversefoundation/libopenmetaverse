using System.Collections.Generic;
using System;
using System.Xml;

using libsecondlife;
using libsecondlife.AssetSystem;
using libsecondlife.Packets;

namespace libsecondlife.InventorySystem
{
    /// <summary>
    /// Base class for most inventory items, providing a lot of general inventory management functions.
    /// </summary>
    public class InventoryItem : InventoryBase
    {
        private const uint FULL_MASK_PERMISSIONS = 2147483647;

        public string Name
        {
            get { return base._Name; }
            set
            {
                _Name = value;
                UpdateItem();
            }
        }

        internal LLUUID _FolderID = LLUUID.Zero;
        public LLUUID FolderID
        {
            get { return _FolderID; }
            set
            {
                InventoryFolder iTargetFolder = base.iManager.getFolder(value);
                if (iTargetFolder == null)
                {
                    throw new Exception("Target Folder [" + value + "] does not exist.");
                }

                base.iManager.getFolder(this.FolderID)._Contents.Remove(this);
                iTargetFolder._Contents.Add(this);

                _FolderID = value;
                base.iManager.MoveItem(ItemID, FolderID);
            }
        }

        internal LLUUID _ItemID = null;
        public LLUUID ItemID
        {
            set
            {
                if (_ItemID == null)
                {
                    _ItemID = value;
                }
                else
                {
                    throw new Exception("You can not change an item's ID once it's been set.");
                }
            }
            get 
            {
                return _ItemID; 
            }
        }

        internal sbyte _InvType = 0;
        public sbyte InvType
        {
            get { return _InvType; }
        }

        internal sbyte _Type = 0;
        public sbyte Type
        {
            get { return _Type; }
            set
            {
                _Type = value;
                UpdateItem();
            }
        }


        internal string _Description = "";
        public string Description
        {
            get { return _Description; }
            set
            {
                _Description = value;
                UpdateItem();
            }
        }

        internal uint _CRC = 0;
        public uint CRC
        {
            get { return _CRC; }
            set
            {
                _CRC = value;
            }
        }


        internal LLUUID _OwnerID = LLUUID.Zero;
        public LLUUID OwnerID
        {
            get { return _OwnerID; }
        }

        internal LLUUID _CreatorID = LLUUID.Zero;
        public LLUUID CreatorID
        {
            get { return _CreatorID; }
        }

        internal Asset _Asset;
        public Asset Asset
        {
            get
            {
                if (_Asset != null)
                {
                    return _Asset;
                }
                else
                {
                    if ((AssetID != null))
                    {
                        AssetRequestDownload request = base.iManager.AssetManager.RequestInventoryAsset(this);
                        if (request.Wait(libsecondlife.AssetSystem.AssetManager.DefaultTimeout) != AssetRequestDownload.RequestStatus.Success)
                        {
                            throw new Exception("Asset (" + AssetID.ToStringHyphenated() + ") unavailable (" + request.StatusMsg + ") for " + this.Name);
                        }
                        switch (Type)
                        {
                            case (sbyte)Asset.AssetType.Clothing:
                                _Asset = new AssetWearable_Clothing(AssetID, request.GetAssetData());
                                break;
                            case (sbyte)Asset.AssetType.Bodypart:
                                _Asset = new AssetWearable_Body(AssetID, request.GetAssetData());
                                break;
                            case (sbyte)Asset.AssetType.LSLText:
                                _Asset = new AssetScript(AssetID, request.GetAssetData());
                                break;
                            case (sbyte)Asset.AssetType.Notecard:
                                _Asset = new AssetNotecard(AssetID, request.GetAssetData());
                                break;
                            case (sbyte)Asset.AssetType.Texture:
                                _Asset = new AssetImage(AssetID, request.GetAssetData());
                                break;
                            default:
                                _Asset = new Asset(AssetID, Type, request.GetAssetData());
                                break;
                        }
                        
                        return Asset;
                    }
                }
                return null;
            }
        }

        internal LLUUID _TransactionID = LLUUID.Zero;
        public LLUUID TransactionID
        {
            get { return _TransactionID; }
        }

        internal LLUUID _AssetID = LLUUID.Zero;
        public LLUUID AssetID
        {
            get { return _AssetID; }
        }


        internal LLUUID _GroupID = LLUUID.Zero;
        public LLUUID GroupID
        {
            get { return _GroupID; }
            set
            {
                _GroupID = value;
                UpdateItem();
            }
        }

        internal bool _GroupOwned = false;
        public bool GroupOwned
        {
            get { return _GroupOwned; }
            set
            {
                _GroupOwned = value;
                UpdateItem();
            }
        }

        internal int _CreationDate = (int)((TimeSpan)(DateTime.UtcNow - new DateTime(1970, 1, 1))).TotalSeconds;
        public int CreationDate
        {
            get { return _CreationDate; }
        }

        internal byte _SaleType = 0;
        public byte SaleType
        {
            get { return _SaleType; }
            set
            {
                _SaleType = value;
                UpdateItem();
            }
        }

        internal uint _BaseMask = FULL_MASK_PERMISSIONS;
        public uint BaseMask
        {
            get { return _BaseMask; }
        }

        internal int _SalePrice = 0;
        public int SalePrice
        {
            get { return _SalePrice; }
            set
            {
                _SalePrice = value;
                UpdateItem();
            }
        }

        internal uint _EveryoneMask = 0;
        public uint EveryoneMask
        {
            get { return _EveryoneMask; }
            set
            {
                _EveryoneMask = value;
                UpdateItem();
            }
        }

        internal uint _Flags = 0;
        public uint Flags
        {
            get { return _Flags; }
            set
            {
                _Flags = value;
                UpdateItem();
            }
        }

        internal uint _NextOwnerMask = FULL_MASK_PERMISSIONS;
        public uint NextOwnerMask
        {
            get { return _NextOwnerMask; }
            set
            {
                _NextOwnerMask = value;
                UpdateItem();
            }
        }

        internal uint _GroupMask = 0;
        public uint GroupMask
        {
            get { return _GroupMask; }
            set
            {
                _GroupMask = value;
                UpdateItem();
            }
        }

        internal uint _OwnerMask = FULL_MASK_PERMISSIONS;
        public uint OwnerMask
        {
            get { return _OwnerMask; }
        }



        internal InventoryItem(InventoryManager manager)
            : base(manager)
        {
        }

        internal InventoryItem(InventoryManager manager, InventoryDescendentsPacket.ItemDataBlock itemData)
            : base(manager)
        {

            _Name = System.Text.Encoding.UTF8.GetString(itemData.Name).Trim().Replace("\0", "");
            _Description = System.Text.Encoding.UTF8.GetString(itemData.Description).Trim().Replace("\0", "");
            _CreationDate = itemData.CreationDate;

            _InvType = itemData.InvType;
            _Type = itemData.Type;

            _ItemID = itemData.ItemID;
            _AssetID = itemData.AssetID;
            _FolderID = itemData.FolderID;

            _GroupOwned = itemData.GroupOwned;
            _GroupID = itemData.GroupID;
            _GroupMask = itemData.GroupMask;

            _CreatorID = itemData.CreatorID;
            _OwnerID = itemData.OwnerID;
            _OwnerMask = itemData.OwnerMask;


            _Flags = itemData.Flags;
            _BaseMask = itemData.BaseMask;
            _EveryoneMask = itemData.EveryoneMask;
            _NextOwnerMask = itemData.NextOwnerMask;

            _SaleType = itemData.SaleType;
            _SalePrice = itemData.SalePrice;

            _CRC = itemData.CRC;
        }

        internal InventoryItem(InventoryManager manager, string name, LLUUID folderID, sbyte invType, sbyte type, LLUUID uuidOwnerCreater)
            : base(manager)
        {
            _Name = name;
            _FolderID = folderID;
            _InvType = invType;
            _Type = type;
            _OwnerID = uuidOwnerCreater;
            _CreatorID = uuidOwnerCreater;

            UpdateCRC();
        }

        internal InventoryItem(InventoryManager manager, string name, string description, LLUUID folderID, sbyte invType, sbyte type, LLUUID uuidOwnerCreater)
            : base(manager)
        {
            _Name = name;
            _Description = description;
            _FolderID = folderID;
            _InvType = invType;
            _Type = type;
            _OwnerID = uuidOwnerCreater;
            _CreatorID = uuidOwnerCreater;

            UpdateCRC();
        }

        /// <summary></summary>
        /// 
        protected void SetAssetTransactionIDs(LLUUID assetID, LLUUID transactionID)
        {
            _AssetID = assetID;
            _TransactionID = transactionID;
            UpdateItem();
        }

        /// <summary>
        /// </summary>
        /// <param name="o"></param>
        public override bool Equals(object o)
        {
            if ((o is InventoryItem) == false)
            {
                return false;
            }

            return this._ItemID == ((InventoryItem)o)._ItemID;
        }

        /// <summary>
        /// </summary>
        public override int GetHashCode()
        {
            return this._ItemID.GetHashCode();
        }

        /// <summary>
        /// CompareTo provided so that items can be sorted by name
        /// </summary>
        /// <param name="obj"></param>
        public int CompareTo(object obj)
        {
            if (obj is InventoryBase)
            {
                InventoryBase temp = (InventoryBase)obj;
                return this._Name.CompareTo(temp._Name);
            }
            throw new ArgumentException("object is not an InventoryItem");
        }

        private void UpdateItem()
        {
            UpdateCRC();
            base.iManager.ItemUpdate(this);
        }

        private void UpdateCRC()
        {
            _CRC = InventoryPacketHelper.InventoryUpdateCRC(this);
        }

        /// <summary>
        /// Move this item to the target folder
        /// </summary>
        /// <param name="targetFolder"></param>
        public void MoveTo(InventoryFolder targetFolder)
        {
            this.FolderID = targetFolder.FolderID;
        }

        /// <summary>
        /// Move this item to the target folder
        /// </summary>
        /// <param name="targetFolderID"></param>
        public void MoveTo(LLUUID targetFolderID)
        {
            this.FolderID = targetFolderID;
        }

        /// <summary>
        /// If you have Copy permission, a copy is placed in the target folder
        /// </summary>
        /// <param name="targetFolder"></param>
        public void CopyTo(LLUUID targetFolder)
        {
            base.iManager.ItemCopy(this.ItemID, targetFolder);
        }

        /// <summary>
        /// Give this item to another agent.  If you have Copy permission, a copy will be given
        /// </summary>
        /// <param name="ToAgentID"></param>
        public void GiveTo(LLUUID ToAgentID)
        {
            base.iManager.ItemGiveTo(this, ToAgentID);
        }

        /// <summary>
        /// Delete this item from Second Life
        /// </summary>
        public void Delete()
        {
            iManager.getFolder(this.FolderID)._Contents.Remove(this);
            iManager.ItemRemove(this);

        }

        /// <summary>
        /// Attempt to rez this inventory item at the given point
        /// </summary>
        /// <param name="TargetPos">Region/Sim coordinates</param>
        public void RezObject(LLVector3 TargetPos)
        {
            RezObject(TargetPos, null);
        }

        /// <summary>
        /// Attempt to rez this inventory item at the given point, in the given simulator
        /// </summary>
        /// <param name="TargetPos">Region/Sim coords</param>
        /// <param name="TargetSim"></param>
        public void RezObject(LLVector3 TargetPos, Simulator TargetSim)
        {
            iManager.ItemRezObject(this, TargetSim, TargetPos);
        }

        /// <summary>
        /// Attempt to attach this item
        /// </summary>
        public void Attach()
        {
            Attach(0); //Use default attach point.
        }

        /// <summary>
        /// Attempt to attach this item.
        /// </summary>
        /// <param name="AttachmentPt">Where to attach to</param>
        public void Attach(ObjectManager.AttachmentPoint AttachmentPt)
        {
            iManager.ItemRezAttach(this, 0);
        }

        /// <summary>
        /// Attempt to detach this item
        /// </summary>
        public void Detach()
        {
            iManager.ItemDetach(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="assetData"></param>
        virtual internal void SetAssetData(byte[] assetData)
        {
            if (_Asset == null)
            {
                if (AssetID != null)
                {
                    _Asset = new Asset(AssetID, Type, assetData);
                }
                else
                {
                    _Asset = new Asset(LLUUID.Random(), Type, assetData);
                    _AssetID = _Asset.AssetID;
                }
            }
            else
            {
                _Asset.SetAssetData(assetData);
            }
        }

        public override string GetDisplayType()
        {
            return "Unknown_Item";
        }

        /// <summary>
        /// Output this item as XML
        /// </summary>
        /// <param name="outputAssets">Include an asset data as well, TRUE/FALSE</param>
        override public string toXML(bool outputAssets)
        {
            string output = "<item ";

            output += "name = '" + xmlSafe(Name) + "' ";
            output += "uuid = '" + ItemID + "' ";
            output += "invtype = '" + InvType + "' ";
            output += "type = '" + Type + "' ";



            output += "description = '" + xmlSafe(Description) + "' ";
            output += "crc = '" + CRC + "' ";
            output += "debug = '" + InventoryPacketHelper.InventoryUpdateCRC(this) + "' ";
            output += "ownerid = '" + OwnerID + "' ";
            output += "creatorid = '" + CreatorID + "' ";

            output += "assetid = '" + AssetID + "' ";
            output += "groupid = '" + GroupID + "' ";

            output += "groupowned = '" + GroupOwned + "' ";
            output += "creationdate = '" + CreationDate + "' ";
            output += "flags = '" + Flags + "' ";

            output += "saletype = '" + SaleType + "' ";
            output += "saleprice = '" + SalePrice + "' ";
            output += "basemask = '" + BaseMask + "' ";
            output += "everyonemask = '" + EveryoneMask + "' ";
            output += "nextownermask = '" + NextOwnerMask + "' ";
            output += "groupmask = '" + GroupMask + "' ";
            output += "ownermask = '" + OwnerMask + "' ";

            output += ">";

            if (outputAssets)
            {
                if (AssetID != LLUUID.Zero)
                {
                    output += xmlSafe(Helpers.FieldToUTF8String(Asset.AssetData));
                }
            }
            output += "</item>";

            return output;
        }
    }
}

/*
	1044 ItemData (Variable)
		0047 GroupOwned (BOOL / 1)
		0149 CRC (U32 / 1)
		0159 CreationDate (S32 / 1)
		0345 SaleType (U8 / 1)
		0395 BaseMask (U32 / 1)
		0506 Name (Variable / 1)
		0562 InvType (S8 / 1)
		0630 Type (S8 / 1)
		0680 AssetID (LLUUID / 1)
		0699 GroupID (LLUUID / 1)
		0716 SalePrice (S32 / 1)
		0719 OwnerID (LLUUID / 1)
		0736 CreatorID (LLUUID / 1)
		0968 ItemID (LLUUID / 1)
		1025 FolderID (LLUUID / 1)
		1084 EveryoneMask (U32 / 1)
		1101 Description (Variable / 1)
		1189 Flags (U32 / 1)
		1348 NextOwnerMask (U32 / 1)
		1452 GroupMask (U32 / 1)
		1505 OwnerMask (U32 / 1)
*/
