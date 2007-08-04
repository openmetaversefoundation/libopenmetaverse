using System;
using System.Text;

namespace libsecondlife
{
    public abstract class Asset
    {
        public byte[] AssetData;

        private LLUUID _AssetID;
        public LLUUID AssetID
        {
            get { return _AssetID; }
            internal set { _AssetID = value; }
        }

        public abstract AssetType AssetType
        {
            get;
        }

        public Asset() { }

        public Asset(byte[] assetData)
        {
            AssetData = assetData;
        }

        /// <summary>
        /// Regenerates the <code>AssetData</code> byte array from the properties 
        /// of the derived class.
        /// </summary>
        public abstract void Encode();

        /// <summary>
        /// Decodes the AssetData, placing it in appropriate properties of the derived
        /// class.
        /// </summary>
        public abstract void Decode();
    }

    public class AssetNotecard : Asset
    {
        public override AssetType AssetType { get { return AssetType.Notecard; } }

        public string Text = null;

        public AssetNotecard() { }
        
        public AssetNotecard(byte[] assetData)
        {
            AssetData = assetData;
        }
        
        public AssetNotecard(string text)
        {
            Text = text;
        }

        public override void Encode()
        {
            AssetData = Helpers.StringToField(Text);
        }
        
        public override void Decode()
        {
            Text = Helpers.FieldToUTF8String(AssetData);
        }
    }

    public class AssetScriptText : Asset
    {
        public override AssetType AssetType { get { return AssetType.LSLText; } }

        public string Source;

        public AssetScriptText() { }
        
        public AssetScriptText(byte[] assetData)
        {
            AssetData = assetData;
        }
        
        public AssetScriptText(string source)
        {
            Source = source;
        }

        public override void Encode()
        {
            AssetData = Helpers.StringToField(Source);
        }

        public override void Decode()
        {
            Source = Helpers.FieldToUTF8String(AssetData);
        }
    }

    public class AssetScriptBinary : Asset
    {
        public override AssetType AssetType { get { return AssetType.LSLBytecode; } }

        public byte[] Bytecode;

        public AssetScriptBinary() { }
        
        public AssetScriptBinary(byte[] assetData)
        {
            AssetData = assetData;
            Bytecode = assetData;
        }

        public override void Encode() { AssetData = Bytecode; }
        public override void Decode() { Bytecode = AssetData; }
    }

    /*
    public class AssetTexture : Asset
    {
        public override AssetType AssetType { get { return AssetType.Texture; } }

        public Image Image;
        
        public AssetTexture() { }

        public AssetTexture(byte[] assetData)
        {
            AssetData = assetData;
        }
        
        public AssetTexture(Image image)
        {
            Image = image;
        }

        public override void Encode()
        {
            AssetData = OpenJPEGNet.OpenJPEG.Encode(Image);
        }
        
        public override void Decode()
        {
            Image = OpenJPEGNet.OpenJPEG.Decode(AssetData);
        }
    }
    */ 

    public class AssetObject : Asset
    {
        public override AssetType AssetType { get { return AssetType.Object; } }
        
        public AssetObject() { }

        public override void Encode() { }
        public override void Decode() { }
    }
}
