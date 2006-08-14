using System;

using libsecondlife;

namespace libsecondlife.AssetSystem
{
	/// <summary>
	/// Summary description for NotecardAsset.
	/// </summary>
	public class NotecardAsset : Asset
	{
		private string _Body = "";
		public string Body
		{
			get { return _Body; }
			set
			{
				Console.WriteLine("Setting notecard body to: " + value);
				_Body = value;
				setAsset( _Body );
			}
		}

		public NotecardAsset(LLUUID assetID, string body) : base( assetID, Asset.ASSET_TYPE_NOTECARD, false, null )
		{
			_Body = body;
			setAsset( body );
		}

		public NotecardAsset(LLUUID assetID, byte[] assetData) : base( assetID, Asset.ASSET_TYPE_NOTECARD, false, null )
		{
			base.AssetData = assetData;

			string temp	= System.Text.Encoding.UTF8.GetString(assetData).Trim();

			// TODO: Calculate the correct header size to look for
			// it's usually around 80 or so...
			if( temp.Length > 50 )
			{
				// Trim trailing null terminator
				temp = temp.Substring(0,temp.Length-1);

				// Remove the header
				temp = temp.Substring(temp.IndexOf("}") + 2);
				temp = temp.Substring(temp.IndexOf('\n') + 1);

				// Remove trailing close brace
				temp = temp.Substring(0,temp.Length-2);
			}
			_Body = temp;
		}		

		private void setAsset( string body )
		{
			// Format the string body into Linden text
			string lindenText = "Linden text version 1\n";
			lindenText += "{\n";
			lindenText += "LLEmbeddedItems version 1\n";
			lindenText += "{\n";
			lindenText += "count 0\n";
			lindenText += "}\n";
			lindenText += "Text length " + body.Length + "\n";
			lindenText += body;
			lindenText += "}\n";
			


			// Assume this is a string, add 1 for the null terminator
			byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes((string)lindenText);
			byte[] assetData = new byte[stringBytes.Length + 1];
			Array.Copy(stringBytes, 0, assetData, 0, stringBytes.Length);

			base.AssetData = assetData;
		}
	}
}
