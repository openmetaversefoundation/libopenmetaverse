using System;

using libsecondlife;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// Summary description for AssetNotecard.
	/// </summary>
	public class AssetImage : Asset
	{
		public byte[] J2CData
		{
			get
			{
				return base.AssetData;
			}
		}

		public AssetImage(LLUUID assetID, byte[] assetData) : base( assetID, Asset.ASSET_TYPE_IMAGE, false, assetData )
		{
		}



	}
}
