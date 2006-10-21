using System;
using System.Collections.Generic;
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
    /// Parse JSON formatted string and return a Hashtable
    /// </summary>
    /// <param name="sJSON"></param>
    /// <returns></returns>
		public static Dictionary<string, object> fromJSON(string sJSON)
		{
			JSONObject jsob = new JSONObject(sJSON);
			Dictionary<string, object> idict = jsob.getDictionary();
			return idict;
		}
	}
}
