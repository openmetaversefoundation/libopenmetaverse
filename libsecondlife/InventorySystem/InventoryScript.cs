using System;
using System.Collections.Generic;
using System.Text;
using libsecondlife.AssetSystem;

namespace libsecondlife.InventorySystem
{
    public class InventoryScript : InventoryItem
    {
        public string Source
        {
            get
            {
                if (_Asset != null)
                {
                    return ((AssetScript)Asset).Source;
                }
                else
                {
                    if ( (AssetID != null) )
                    {
                        AssetRequestDownload request = base.iManager.AssetManager.RequestInventoryAsset(this);
                        if (request.Wait(libsecondlife.AssetSystem.AssetManager.DefaultTimeout) != AssetRequestDownload.RequestStatus.Success)
                        {
                            throw new Exception("Asset (" + AssetID.ToStringHyphenated() + ") unavailable (" + request.StatusMsg + ") for " + this.Name);
                        }
                        _Asset = new AssetScript(AssetID, request.GetAssetData());
                        return ((AssetScript)Asset).Source;
                    }
                }
                return null;
            }
            set
            {
                base._Asset = new AssetScript(LLUUID.Random(), value);
                LLUUID TransactionID = base.iManager.AssetManager.UploadAsset(Asset);
                base.SetAssetTransactionIDs(Asset.AssetID, TransactionID);
            }
        }

        public InventoryScript(InventoryManager manager, string name, string description, LLUUID folderID, LLUUID uuidOwnerCreater)
            : base(manager, name, description, folderID, 10, 10, uuidOwnerCreater)
        {
        }

        public InventoryScript(InventoryManager manager, InventoryItem ii)
            : base(manager, ii.Name, ii.Description, ii.FolderID, ii.InvType, ii.Type, ii.CreatorID)
        {
            if (ii.InvType != 10 || ii.Type != (sbyte)Asset.AssetType.LSLText)
                throw new Exception("The InventoryItem cannot be converted to a Script, wrong InvType/Type.");
            this.iManager = manager;
            this._ItemID = ii._ItemID;
            this._Asset = ii._Asset;
            this._AssetID = ii._AssetID;
            this._BaseMask = ii._BaseMask;
            this._CRC = ii._CRC;
            this._CreationDate = ii._CreationDate;
            this._EveryoneMask = ii._EveryoneMask;
            this._Flags = ii._Flags;
            this._GroupID = ii._GroupID;
            this._GroupMask = ii._GroupMask;
            this._GroupOwned = ii._GroupOwned;
            this._InvType = ii._InvType;
            this._NextOwnerMask = ii._NextOwnerMask;
            this._OwnerID = ii._OwnerID;
            this._OwnerMask = ii._OwnerMask;
            this._SalePrice = ii._SalePrice;
            this._SaleType = ii._SaleType;
            this._Type = ii._Type;
        }

        internal override void SetAssetData(byte[] assetData)
        {
            if ( _Asset == null )
            {
                if ( AssetID == null )
                {
                    _Asset = new AssetScript(AssetID, assetData);
                }
                else
                {
                    _Asset = new AssetScript(LLUUID.Random(), assetData);
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
            return "Script";
        }

        override public string toXML(bool outputAssets)
        {
            string output = "<script ";

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

            if ( outputAssets )
            {
                output += xmlSafe(Source);
            }

            output += "</script>";


            return output;
        }

    }
}
