using System.Collections;
using System;
using System.Xml;

using libsecondlife;
using libsecondlife.AssetSystem;

namespace libsecondlife.InventorySystem
{
	/// <summary>
	/// Summary description for InventoryFolder.
	/// </summary>
	public class InventoryItem : InventoryBase
	{
		private const uint FULL_MASK_PERMISSIONS = 2147483647;

		public   string Name
		{
			get{ return base._Name; }
			set
			{
				_Name = value;
				UpdateItem();
			}
		}

		internal LLUUID _FolderID = new LLUUID();
		public   LLUUID FolderID
		{
			get{ return _FolderID; }
			set
			{
				InventoryFolder iTargetFolder = base.iManager.getFolder( value );
				if( iTargetFolder == null )
				{
					throw new Exception("Target Folder [" + value + "] does not exist.");
				}

				base.iManager.getFolder( this.FolderID ).alContents.Remove( this );
				iTargetFolder.alContents.Add( this );

				_FolderID = value;
				UpdateItem();
			}
		}

		internal LLUUID _ItemID = new LLUUID();
		public   LLUUID ItemID
		{
			get{ return _ItemID; }
		}

		internal sbyte _InvType = 0;
		public   sbyte InvType
		{
			get{ return _InvType; }
		}

		internal sbyte _Type = 0;
		public   sbyte Type
		{
			get{ return _Type; }
			set
			{
				_Type = value;
				UpdateItem();
			}
		}

		
		internal string _Description = "";
		public   string Description
		{
			get{ return _Description; }
			set
			{
				_Description = value;
				UpdateItem();
			}
		}

		internal uint _CRC = 0;
		public   uint CRC
		{
			get { return _CRC; }
			set
			{
				_CRC = value;
			}
		}


		internal LLUUID _OwnerID = new LLUUID();
		public   LLUUID OwnerID
		{
			get { return _OwnerID; }
		}

		internal LLUUID _CreatorID = new LLUUID();
		public   LLUUID CreatorID
		{
			get { return _CreatorID; }
		}

		internal Asset  _Asset;
		public   Asset  Asset
		{
			get { 
				if( _Asset != null )
				{
					return _Asset;
				} 
				else 
				{
					if( (AssetID != null) && (AssetID != new LLUUID()) )
					{
						base.iManager.AssetManager.GetInventoryAsset( this );
						return Asset;
					}
				}
				return null;
			}
		}

		internal LLUUID _AssetID = new LLUUID();
		public   LLUUID AssetID
		{
			get { return _AssetID; }
			set
			{
				_AssetID = value;
				UpdateItem();
			}
		}


		internal LLUUID _GroupID = new LLUUID();
		public   LLUUID GroupID
		{
			get { return _GroupID; }
			set
			{
				_GroupID = value;
				UpdateItem();
			}
		}

		internal bool _GroupOwned = false;
		public   bool GroupOwned
		{
			get { return _GroupOwned; }
			set
			{
				_GroupOwned = value;
				UpdateItem();
			}
		}

		internal int _CreationDate = (int)((TimeSpan)(DateTime.UtcNow - new DateTime(1970, 1, 1))).TotalSeconds;
		public   int CreationDate
		{
			get { return _CreationDate; }
		}

		internal byte _SaleType = 0;
		public   byte SaleType
		{
			get { return _SaleType; }
			set
			{
				_SaleType = value;
				UpdateItem();
			}
		}

		internal uint _BaseMask = FULL_MASK_PERMISSIONS;
		public   uint BaseMask
		{
			get { return _BaseMask; }
		}

		internal int _SalePrice = 0;
		public   int SalePrice
		{
			get { return _SalePrice; }
			set
			{
				_SalePrice = value;
				UpdateItem();
			}
		}

		internal uint _EveryoneMask = 0;
		public   uint EveryoneMask
		{
			get { return _EveryoneMask; }
			set
			{
				_EveryoneMask = value;
				UpdateItem();
			}
		}

		internal uint _Flags = 0;
		public   uint Flags
		{
			get { return _Flags; }
			set
			{
				_Flags = value;
				UpdateItem();
			}
		}

		internal uint _NextOwnerMask = FULL_MASK_PERMISSIONS;
		public   uint NextOwnerMask
		{
			get { return _NextOwnerMask; }
			set
			{
				_NextOwnerMask = value;
				UpdateItem();
			}
		}

		internal uint _GroupMask = 0;
		public   uint GroupMask
		{
			get { return _GroupMask; }
			set
			{
				_GroupMask = value;
				UpdateItem();
			}
		}

		internal uint _OwnerMask = FULL_MASK_PERMISSIONS;
		public   uint OwnerMask
		{
			get { return _OwnerMask; }
		}



		internal InventoryItem(InventoryManager manager) : base(manager)
		{
		}

		internal InventoryItem( InventoryManager manager, string name, LLUUID id, LLUUID folderID, sbyte invType, sbyte type, LLUUID uuidOwnerCreater ) : base(manager)
		{
			_Name			= name;
			_ItemID			= id;
			_FolderID		= folderID;
			_InvType		= invType;
			_Type			= type;
			_OwnerID		= uuidOwnerCreater;
			_CreatorID		= uuidOwnerCreater;

			UpdateCRC();
		}

		internal InventoryItem( InventoryManager manager, string name, string description, LLUUID id, LLUUID folderID, sbyte invType, sbyte type, LLUUID uuidOwnerCreater ) : base(manager)
		{
			_Name			= name;
			_Description    = description;
			_ItemID			= id;
			_FolderID		= folderID;
			_InvType		= invType;
			_Type			= type;
			_OwnerID		= uuidOwnerCreater;
			_CreatorID		= uuidOwnerCreater;

			UpdateCRC();
		}

		public override bool Equals(object o)
		{
			if( (o is InventoryItem) == false )
			{
				return false;
			}

			return this._ItemID == ((InventoryItem)o)._ItemID;
		}

		public override int GetHashCode()
		{
			return this._ItemID.GetHashCode();
		}

		public int CompareTo(object obj) 
		{
			if(obj is InventoryBase) 
			{
				InventoryBase temp = (InventoryBase) obj;
				return this._Name.CompareTo(temp._Name);
			}
			throw new ArgumentException("object is not an InventoryItem");    
		}

		private void UpdateItem()
		{
			UpdateCRC();
			base.iManager.ItemUpdate( this );
		}

		private void UpdateCRC()
		{
			_CRC = PacketHelpers.UpdateInventoryItem.BuildCRC(this);
		}

		public void MoveTo( InventoryFolder targetFolder )
		{
			this.FolderID = targetFolder.FolderID;
		}
		public void MoveTo( LLUUID targetFolderID )
		{
			this.FolderID = targetFolderID;
		}

		public void CopyTo( LLUUID targetFolder )
		{
			base.iManager.ItemCopy( this.ItemID, targetFolder );
		}

		public void GiveTo( LLUUID ToAgentID )
		{
			base.iManager.ItemGiveTo( this, ToAgentID );
		}

		public void Delete()
		{
			base.iManager.getFolder( this.FolderID ).alContents.Remove( this );
			base.iManager.ItemRemove( this );

		}

		public void ClearAssetTest()
		{
			_Asset = null;
		}

		virtual internal void SetAssetData( byte[] assetData )
		{
			if( _Asset == null )
			{
				if( AssetID != null )
				{
					_Asset = new Asset( AssetID, Type, assetData );
				} else {
					_Asset = new Asset( LLUUID.GenerateUUID(), Type, assetData );
					AssetID = _Asset.AssetID;
				}
			} else {
				_Asset.AssetData = assetData;
			}
		}

		override public string toXML( bool outputAssets )
		{
			string output = "<item ";

			output += "name = '" + xmlSafe(Name) + "' ";
			output += "uuid = '" + ItemID + "' ";
			output += "invtype = '" + InvType + "' ";
			output += "type = '" + Type + "' ";



			output += "description = '" + xmlSafe(Description) + "' ";
			output += "crc = '" + CRC + "' ";
			output += "debug = '" + PacketHelpers.UpdateInventoryItem.BuildCRC(this) + "' ";
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

			output += "/>\n";

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