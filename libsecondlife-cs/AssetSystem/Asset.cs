using System;

using libsecondlife;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// Summary description for Asset.
	/// </summary>
	public class Asset
	{
		public const sbyte ASSET_TYPE_NOTECARD = 7;
		public const sbyte ASSET_TYPE_IMAGE    = 0;

		public LLUUID AssetID;

		public sbyte Type;
		public bool Tempfile;


		private byte[] assetdata;
		public byte[] AssetData
		{
			get { return assetdata; }
			set
			{
				assetdata = value;
			}
		}


		public Asset(LLUUID assetID, sbyte type, bool tempfile, byte[] assetData)
		{
			AssetID		= assetID;
			Type		= (sbyte)type;
			Tempfile	= tempfile;
			AssetData	= assetData;
		}

		public Asset(LLUUID assetID, sbyte type, byte[] assetData)
		{
			AssetID		= assetID;
			Type		= (sbyte)type;
			Tempfile	= false;
			AssetData	= assetData;
		}


		public string AssetDataToString()
		{
			return libsecondlife.Utils.ByteArrayToString((byte[])AssetData);
		}
	}
}
