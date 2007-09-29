using System;

using libsecondlife;
using libsecondlife.AssetSystem;

namespace libsecondlife.InventorySystem
{
	/// <summary>
	/// Base class for Inventory items
	/// </summary>
	abstract public class InventoryBase
	{
		protected InventoryManager iManager;

		internal string _Name;

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        internal InventoryBase(InventoryManager manager)
		{
			if( manager == null )
			{
				throw new Exception( "Inventory Manager cannot be null" );
			}
			iManager = manager;
		}

        /// <summary>
        /// Output this item as XML
        /// </summary>
        /// <param name="outputAssets">Include an asset data as well, TRUE/FALSE</param>
        abstract public string toXML(bool outputAssets);

        /// <summary>
        /// Get a short string describing this item's type
        /// </summary>
        abstract public string GetDisplayType();

        /// <summary>
        /// Utility function to simply making text XML safe
        /// </summary>
        /// <param name="str"></param>
        protected string xmlSafe(string str)
		{
			if( str != null )
			{
				string clean = str.Replace("&","&amp;");
				clean = clean.Replace("<","&lt;");
				clean = clean.Replace(">","&gt;");
				clean = clean.Replace("'","&apos;");
				clean = clean.Replace("\"","&quot;");
				return clean;
			} 
			else 
			{
				return "";
			}
		}

	}
}
