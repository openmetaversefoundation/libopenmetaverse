using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

namespace Nii.JSON
{
	/// <summary>
	/// Summary description for JsonFacade.
	/// </summary>
	public sealed class JsonFacade
	{
    /// <summary>
    /// Parse a Hashtable and return a JSON formatted string
    /// </summary>
    /// <param name="idict"></param>
    /// <returns></returns>
		public static string toJSON(IDictionary idict)
		{
			JSONObject jsob = new JSONObject(idict);
			return jsob.ToString();
		}
    /// <summary>
    /// Parse JSON formatted string and return a Hashtable
    /// </summary>
    /// <param name="sJSON"></param>
    /// <returns></returns>
		public static IDictionary fromJSON(string sJSON)
		{
			JSONObject jsob = new JSONObject(sJSON);
			IDictionary idict = jsob.getDictionary();
			return idict;
		}
	}
}
