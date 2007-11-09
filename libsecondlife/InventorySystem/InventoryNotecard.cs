using System;

using libsecondlife;
using libsecondlife.AssetSystem;

namespace libsecondlife.InventorySystem
{
	/// <summary>
	/// Summary description for InventoryNotecard.
	/// </summary>
	public class InventoryNotecard : InventoryItem
	{
		public string Body
		{
			get
			{
				if( _Asset != null ) 
				{
					return ((AssetNotecard)Asset).Body;
				} else {
                    if ((AssetID != null))
					{
                        AssetRequestDownload request = base.iManager.AssetManager.RequestInventoryAsset(this);
                        if (request.Wait(libsecondlife.AssetSystem.AssetManager.DefaultTimeout) != AssetRequestDownload.RequestStatus.Success)
                        {
                            throw new Exception("Asset (" + AssetID.ToStringHyphenated() + ") unavailable (" + request.StatusMsg + ") for " + this.Name);
                        }
                        _Asset = new AssetNotecard(AssetID, request.GetAssetData());

                        return ((AssetNotecard)Asset).Body;
					}
				}

				return null;
			}

			set
			{
				base._Asset          = new AssetNotecard( LLUUID.Random(), value );
				LLUUID TransactionID = base.iManager.AssetManager.UploadAsset( Asset );
                base.SetAssetTransactionIDs(Asset.AssetID, TransactionID);
			}
		}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="folderID"></param>
        /// <param name="uuidOwnerCreater"></param>
        internal InventoryNotecard(InventoryManager manager, string name, string description, LLUUID folderID, LLUUID uuidOwnerCreater) 
			: base(manager, name, description, folderID, 7, 7, uuidOwnerCreater)
		{

		}

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="ii"></param>
        internal InventoryNotecard(InventoryManager manager, InventoryItem ii)
			: base( manager, ii._Name, ii._Description, ii._FolderID, ii._InvType, ii._Type, ii._CreatorID)
		{
            if ((ii.InvType != 7) || (ii.Type != (sbyte)Asset.AssetType.Notecard))
			{
				throw new Exception("The InventoryItem cannot be converted to a Notecard, wrong InvType/Type.");
			}

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

        /// <summary>
        /// </summary>
        /// <param name="assetData"></param>
        override internal void SetAssetData(byte[] assetData)
		{
			if( _Asset == null )
			{
				if( AssetID != null )
				{
					_Asset = new AssetNotecard( AssetID, assetData );
				} 
				else 
				{
					_Asset   = new AssetNotecard( LLUUID.Random(), assetData );
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
            return "Notecard";
        }

        /// <summary>
        /// Output this item as XML
        /// </summary>
        /// <param name="outputAssets">Include an asset data as well, TRUE/FALSE</param>
        override public string toXML(bool outputAssets)
		{
			string output = "<notecard ";

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

			if( outputAssets )
			{
				output += xmlSafe(Body);
			}

			output += "</notecard>";


			return output;
		}

	}
}
