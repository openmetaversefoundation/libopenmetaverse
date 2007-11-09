using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife.AssetSystem
{
    public class AssetScript : Asset
    {
        private string _Source;
        public string Source
        {
            get { return _Source; }
            set
            {
                _Source = value.Replace("\r", "");
                setAsset(_Source);
            }
        }

        public AssetScript(LLUUID assetID, string source)
            : base(assetID, (sbyte)Asset.AssetType.LSLText, false, null)
        {
            _Source = source;
            setAsset(source);
        }

        public AssetScript(LLUUID assetID, byte[] assetData)
            : base(assetID, (sbyte)Asset.AssetType.LSLText, false, assetData)
        {
            _Source = System.Text.Encoding.UTF8.GetString(assetData).Trim();
        }

        private void setAsset(string source)
        {
            // Assume this is a string, add 1 for the null terminator
            byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] assetData = new byte[stringBytes.Length + 1];
            Buffer.BlockCopy(stringBytes, 0, assetData, 0, stringBytes.Length);
            SetAssetData(assetData);
        }

    }
}
