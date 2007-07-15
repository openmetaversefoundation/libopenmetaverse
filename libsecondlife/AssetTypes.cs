using System;
using System.Text;

namespace libsecondlife
{
    public abstract class Asset
    {
        private byte[] _AssetData = new byte[0];

        private LLUUID _AssetID;
        public LLUUID AssetID
        {
            get { return _AssetID; }
            internal set
            {
                _AssetID = value;
            }
        }

        private bool _DecodeNeeded;
        /// <summary>
        /// <code>true</code> if the asset's properties need to be decoded
        /// from the asset's data byte array.
        /// </summary>
        public bool DecodeNeeded
        {
            private set { _DecodeNeeded = value; }
            get { return _DecodeNeeded; }
        }

        private bool _EncodeNeeded;
        /// <summary>
        /// <code>true</code> if the asset's data byte array needs to be 
        /// regenerated from the asset's properties, <code>false</code> otherwise.
        /// </summary>
        public bool EncodeNeeded
        {
            private set { _EncodeNeeded = value; }
            get { return _EncodeNeeded; }
        }

        public Asset() { }

        public void SetEncodedData(byte[] assetData)
        {
            byte[] copy = new byte[assetData.Length];
            Buffer.BlockCopy(assetData, 0, copy, 0, assetData.Length);
            _AssetData = copy;
            DecodeNeeded = true;
            EncodeNeeded = false;
        }

        public void GetEncodedData(byte[] dest, int off)
        {
            EncodeIfNeeded();
            Buffer.BlockCopy(_AssetData, 0, dest, off, _AssetData.Length);
        }

        public byte[] GetEncodedData()
        {
            byte[] copy = new byte[_AssetData.Length];
            GetEncodedData(copy, 0);
            return copy;
        }


        /// <summary>
        /// This method signals that the <code>AssetData</code> byte 
        /// array no longer corresponds to the state of the Asset.
        /// <remarks>
        /// Derived classes should call this method if their properties
        /// change. 
        /// </remarks>
        /// </summary>
        protected void InvalidateEncodedData()
        {
            EncodeNeeded = true;
        }

        /// <summary>
        /// Regenerates the <code>AssetData</code> array if and only if the properties
        /// of the class have been modified.
        /// <remarks>
        /// Call this method instead of calling <code>Encode</code> directly.
        /// </remarks>
        /// </summary>
        public void EncodeIfNeeded()
        {
            if (EncodeNeeded)
            {
                Encode(out _AssetData);
                EncodeNeeded = false;
            }
        }

        /// <summary>
        /// Decodes the AssetData byte array, if and only if it hasn't been
        /// decoded before.
        /// <remarks>
        /// Derived classes should call this method before returning a value in their
        /// properties' getters and before setting the value in their properties' setters.
        /// Call this method instead of calling <code>Decode</code> directly.
        /// </remarks>
        /// </summary>
        public void DecodeIfNeeded()
        {
            if (DecodeNeeded)
            {
                Decode(_AssetData);
                DecodeNeeded = false;
            }
        }

        /// <summary>
        /// Regenerates the <code>AssetData</code> byte array from the properties 
        /// of the derived class.
        /// </summary>
        protected abstract void Encode(out byte[] newAssetData);

        /// <summary>
        /// Decodes the AssetData, placing it in appropriate properties of the derived
        /// class.
        /// </summary>
        protected abstract void Decode(byte[] assetData);
    }

    public class AssetNotecard : Asset
    {
        private string _Text = null;
        public string Text
        {
            get
            {
                DecodeIfNeeded();
                return _Text;
            }
            set
            {
                DecodeIfNeeded();
                _Text = value;
                InvalidateEncodedData();
            }
        }

        protected override void Encode(out byte[] newAssetData)
        {
            newAssetData = Helpers.StringToField(_Text);
        }
        
        protected override void Decode(byte[] assetData)
        {
            _Text = Helpers.FieldToUTF8String(assetData);
        }
    }

    public class AssetScript : Asset
    {
        private string _Source = null;
        public string Source
        {
            get
            {
                DecodeIfNeeded();
                return _Source;
            }
            set
            {
                DecodeIfNeeded();
                _Source = value;
                InvalidateEncodedData();
            }
        }

        protected override void Encode(out byte[] newAssetData)
        {
            newAssetData = Helpers.StringToField(_Source);
        }

        protected override void Decode(byte[] assetData)
        {
            _Source = Helpers.FieldToUTF8String(assetData);
        }
    }
}
