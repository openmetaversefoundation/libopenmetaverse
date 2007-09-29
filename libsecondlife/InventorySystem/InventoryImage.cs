using System;

using libsecondlife;
using libsecondlife.AssetSystem;

namespace libsecondlife.InventorySystem
{
	/// <summary>
	/// Summary description for InventoryNotecard.
	/// </summary>
	public class InventoryImage : InventoryItem
	{
		public byte[] J2CData
		{
			get
			{
                if (_Asset != null)
                {
					return ((AssetImage)Asset).J2CData;
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
                        _Asset = new AssetImage(AssetID, request.GetAssetData());

                        return ((AssetImage)Asset).J2CData;
					}
				}

				return null;
			}

			set
			{
				base._Asset = new AssetImage( LLUUID.Random(), value );
				LLUUID TransactionID = base.iManager.AssetManager.UploadAsset( Asset );
                base.SetAssetTransactionIDs( Asset.AssetID, TransactionID );
			}

		}

		internal InventoryImage( InventoryManager manager, string name, string description,  LLUUID folderID, LLUUID uuidOwnerCreater ) 
			: base(manager, name, description, folderID, 0, 0, uuidOwnerCreater)
		{

		}

		internal InventoryImage( InventoryManager manager, InventoryItem ii )
			: base( manager, ii._Name, ii._Description, ii._FolderID, ii._InvType, ii._Type, ii._CreatorID)
		{
			if( (ii.InvType != 0) || (ii.Type != (sbyte)Asset.AssetType.Texture) )
			{
				throw new Exception("The InventoryItem cannot be converted to a Image/Texture, wrong InvType/Type.");
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


		override internal void SetAssetData( byte[] assetData )
		{
			if( _Asset == null )
			{
				if( _AssetID != null )
				{
					_Asset = new AssetImage( _AssetID, assetData );
				} 
				else 
				{
					_Asset   = new AssetImage( LLUUID.Random(), assetData );
					_AssetID = _Asset.AssetID;
				}
			} 
			else 
			{
				Asset.SetAssetData(assetData);
			}

		}

        public override string GetDisplayType()
        {
            return "Image";
        }


        /// <summary>
        /// Output this image as XML
        /// </summary>
        /// <param name="outputAssets">Include an asset data as well, TRUE/FALSE</param>
        override public string toXML(bool outputAssets)
		{
			string output = "<image ";

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

			output += ">\n";

			if( outputAssets )
			{
				output += xmlSafe(base.Asset.AssetDataToString());
			}

			output += "</image>";


			return output;
		}

	}
}
