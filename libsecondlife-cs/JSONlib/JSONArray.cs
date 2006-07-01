using System;
using System.Collections;
using System.Text;

namespace Nii.JSON
{
  /// <summary>
  /// <para>
  /// A JSONArray is an ordered sequence of values. Its external form is a string
  /// wrapped in square brackets with commas between the values. The internal form
  /// is an object having get() and opt() methods for accessing the values by
  /// index, and put() methods for adding or replacing values. The values can be
  /// any of these types: Boolean, JSONArray, JSONObject, Number, String, or the
  /// JSONObject.NULL object.
  /// </para>
  /// <para>
  /// The constructor can convert a JSON external form string into an
  /// internal form Java object. The toString() method creates an external
  /// form string.
  /// </para>
  /// <para>
  /// A get() method returns a value if one can be found, and throws an exception
  /// if one cannot be found. An opt() method returns a default value instead of
  /// throwing an exception, and so is useful for obtaining optional values.
  /// </para>
  /// <para>
  /// The generic get() and opt() methods return an object which you can cast or
  /// query for type. There are also typed get() and opt() methods that do typing
  /// checking and type coersion for you.
  ///</para>
  /// <para>
  /// The texts produced by the toString() methods are very strict.
  /// The constructors are more forgiving in the texts they will accept.
  /// </para>
  /// <para>
  /// <list type="bullet">
  /// <item><description>An extra comma may appear just before the closing bracket.</description></item>
  /// <item><description>Strings may be quoted with single quotes.</description></item>
  /// <item><description>Strings do not need to be quoted at all if they do not contain leading
  ///     or trailing spaces, and if they do not contain any of these characters:
  ///     { } [ ] / \ : , </description></item>
  /// <item><description>Numbers may have the 0- (octal) or 0x- (hex) prefix.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// Public Domain 2002 JSON.org
  /// @author JSON.org
  /// @version 0.1
  ///</para>
  /// Ported to C# by Are Bjolseth, teleplan.no
  /// TODO:
  /// 1. Implement Custom exceptions
  /// 2. Add indexer JSONObject[i] = object,     and object = JSONObject[i];
  /// 3. Add indexer JSONObject["key"] = object, and object = JSONObject["key"]
  /// 4. Add unit testing
  /// 5. Add log4net
  /// 6. Make get/put methods private, to force use of indexer instead?
  /// </summary>
	public class JSONArray
	{
		/// <summary>The ArrayList where the JSONArray's properties are kept.</summary>
		private ArrayList myArrayList;

		#region JSONArray Constructors
		/// <summary>
		/// Construct an empty JSONArray
		/// </summary>
		public JSONArray()
		{
			myArrayList = new ArrayList();
		}

		/// <summary>
		/// Construct a JSONArray from a JSONTokener.
		/// </summary>
		/// <param name="x">A JSONTokener</param>
		public JSONArray(JSONTokener x) : this()
		{
			if (x.nextClean() != '[') 
			{
				throw new Exception("A JSONArray must start with '['");
			}
			if (x.nextClean() == ']') 
			{
				return;
			}
			x.back();
			while (true) 
			{
				myArrayList.Add(x.nextObject());
				switch (x.nextClean()) 
				{
					case ',':
						if (x.nextClean() == ']') 
						{
							return;
						}
						x.back();
						break;
					case ']':
						return;
					default:
						throw new Exception("Expected a ',' or ']'");
				}
			}
		}

		/// <summary>
		/// Construct a JSONArray from a source string.
		/// </summary>
		/// <param name="s">A string that begins with '[' and ends with ']'.</param>
		public JSONArray(string s) : this(new JSONTokener(s))
		{
		}

		/// <summary>
		/// Construct a JSONArray from a Collection.
		/// </summary>
		/// <param name="collection">A Collection.</param>
		public JSONArray(ICollection collection)
		{
			myArrayList = new ArrayList(collection);
		}
		#endregion

		#region C# extensions. Indexer and property
		/// <summary>
		/// Alternate to Java get/put method, by using indexer
		/// </summary>
		public object this[int i]
		{
			get
			{
				return opt(i);
			}
			set
			{
				//myArrayList[i] = value;
				put(i, value);
			}
		}

		/// <summary>
		/// Alternativ to Java, getArrayList, by using propery
		/// </summary>
		public IList List
		{
			get
			{
				return myArrayList;
			}
		}
		#endregion

		#region Getters for a value associated with an index.
		/// <summary>
		/// Get the object value associated with an index.
		/// Use indexer instead!!! Added to be true to the original Java implementation
		/// </summary>
		/// <param name="i">index subscript. The index must be between 0 and length()-1</param>
		/// <returns>An object value.</returns>
		public object getValue(int i)
		{
			object obj = opt(i);
			if (obj == null)
			{
				string msg = string.Format("JSONArray[{0}] not found", i);
				throw new Exception(msg);
			}
			return obj;
			//return myArrayList[i];
		}

		/// <summary>
		/// Get the ArrayList which is holding the elements of the JSONArray.
		/// Use the indexer instead!! Added to be true to the orignal Java src
		/// </summary>
		/// <returns>The ArrayList</returns>
		public IList getArrayList()
		{
			return myArrayList;
		}

		/// <summary>
		/// Get the boolean value associated with an index.
		/// The string values "true" and "false" are converted to boolean.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>The truth</returns>
		public bool getBoolean(int i)
		{
			object obj = getValue(i);
			if (obj is bool)
			{
				return (bool)obj;
			}
			string msg = string.Format("JSONArray[{0}]={1} not a Boolean", i, obj);
			throw new Exception(msg);
		}

		/// <summary>
		/// Get the double value associated with an index.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>A double value</returns>
		public double getDouble(int i)
		{
			object obj = getValue(i);
			if (obj is double)
				return (double)obj;
			if (obj is float)
				return (double)obj;
			if (obj is string)
			{
				return Convert.ToDouble(obj);
			}
			string msg = string.Format("JSONArray[{0}]={1} not a double", i, obj);
			throw new Exception(msg);
		}

		/// <summary>
		/// Get the int value associated with an index.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>The int value</returns>
		public int getInt(int i)
		{
			object obj = getValue(i);
			if (obj is int)
				return (int)obj;
			if (obj is string)
			{
				return Convert.ToInt32(obj);
			}
			string msg = string.Format("JSONArray[{0}]={1} not a int", i, obj);
			throw new Exception(msg);
		}

		/// <summary>
		/// Get the JSONArray associated with an index.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>A JSONArray value</returns>
		public JSONArray getJSONArray(int i)
		{
			object obj = getValue(i);
			if (obj is JSONArray)
			{
				return (JSONArray)obj;
			}
			string msg = string.Format("JSONArray[{0}]={1} not a JSONArray", i, obj);
			throw new Exception(msg);
		}

		/// <summary>
		/// Get the JSONObject associated with an index.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>A JSONObject value</returns>
		public JSONObject getJSONObject(int i)
		{
			object obj = getValue(i);
			if (obj is JSONObject)
			{
				return (JSONObject)obj;
			}
			string msg = string.Format("JSONArray[{0}]={1} not a JSONObject", i, obj);
			throw new Exception(msg);
		}

		/// <summary>
		/// Get the string associated with an index.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>A string value.</returns>
		public string getString(int i)
		{
			object obj = getValue(i);
			if (obj is string)
			{
				return (string)obj;
			}
			string msg = string.Format("JSONArray[{0}]={1} not a string", i, obj);
			throw new Exception(msg);
		}
		#endregion

		/// <summary>
		/// Determine if the value is null.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>true if the value at the index is null, or if there is no value.</returns>
		public bool isNull(int i)
		{
			object obj = opt(i);
			return (obj == null || obj.Equals(null));
		}

		/// <summary>
		/// Make a string from the contents of this JSONArray. The separator string
		/// is inserted between each element.
		/// Warning: This method assumes that the data structure is acyclical.
		/// </summary>
		/// <param name="separator">separator A string that will be inserted between the elements.</param>
		/// <returns>A string.</returns>
		public string join(string separator)
		{
			object obj;
			StringBuilder sb = new StringBuilder();
			for (int i=0; i<myArrayList.Count; i++)
			{
				if (i > 0)
				{
					sb.Append(separator);
				}
				obj = myArrayList[i];

				if (obj == null)
				{
					sb.Append("");
				}
				else if (obj is string)
				{
					sb.Append(JSONUtils.Enquote((string)obj));
				}
				else if (obj is int)
				{
					sb.Append(((int)obj).ToString());
				}
				else
				{
					sb.Append(obj.ToString());
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Get the length of the JSONArray.
		/// Added to be true to the original Java implementation
		/// </summary>
		/// <returns>Number of JSONObjects in array</returns>
		public int Length()
		{
			return myArrayList.Count;
		}
		/// <summary>
		/// Get the length of the JSONArray.
		/// Using a propery instead of method
		/// </summary>
		public int Count
		{
			get
			{
				return myArrayList.Count;
			}
		}


		#region Get the optional value associated with an index.
		/// <summary>
		/// Get the optional object value associated with an index.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>object at that index.</returns>
		public object opt(int i)
		{
			if (i < 0 || i >= myArrayList.Count)
				throw new ArgumentOutOfRangeException("int i", i, "Index out of bounds!");

			return myArrayList[i];
		}

		/// <summary>
		/// Get the optional boolean value associated with an index.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>The truth</returns>
		public bool optBoolean(int i)
		{
			return optBoolean(i,false);
		}

		/// <summary>
		/// Get the optional boolean value associated with an index.
		/// It returns the defaultValue if there is no value at that index or if it is not
		/// a Boolean or the String "true" or "false".
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <param name="defaultValue"></param>
		/// <returns>The truth.</returns>
		public bool optBoolean(int i, bool defaultValue)
		{
			object obj = opt(i);
			if (obj != null)
			{
				return (bool)obj;
			}
			return defaultValue;
		}

		/// <summary>
		/// Get the optional double value associated with an index.
		/// NaN is returned if the index is not found,
		/// or if the value is not a number and cannot be converted to a number.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>The double value object</returns>
		public double optDouble(int i)
		{
			return optDouble(i,double.NaN);
		}

		/// <summary>
		/// Get the optional double value associated with an index.
		/// NaN is returned if the index is not found,
		/// or if the value is not a number and cannot be converted to a number.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <param name="defaultValue"></param>
		/// <returns>The double value object</returns>
		public double optDouble(int i, double defaultValue)
		{
			object obj = opt(i);
			if (obj != null)
			{
				if (obj is double)
					return (double)obj;
				if (obj is float)
					return (float)obj;
				if (obj is string)
				{
					return Convert.ToDouble(obj);
				}				
				string msg = string.Format("JSONArray[{0}]={1} not a double", i, obj);
				throw new Exception(msg);
			}
			return defaultValue;
		}

		/// <summary>
		/// Get the optional int value associated with an index.
		/// Zero is returned if the index is not found,
		/// or if the value is not a number and cannot be converted to a number.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>The int value object</returns>
		public int optInt(int i)
		{
			return optInt(i,0);
		}

		/// <summary>
		/// Get the optional int value associated with an index.
		/// The defaultValue is returned if the index is not found,
		/// or if the value is not a number and cannot be converted to a number.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <param name="defaultValue">The default value</param>
		/// <returns>The int value object</returns>
		public int optInt(int i, int defaultValue)
		{
			object obj = opt(i);
			if (obj != null)
			{
				if (obj is int)
					return (int)obj;
				if (obj is string)
					return Convert.ToInt32(obj);			
				string msg = string.Format("JSONArray[{0}]={1} not a int", i, obj);
				throw new Exception(msg);
			}
			return defaultValue;
		}

		/// <summary>
		/// Get the optional JSONArray associated with an index.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>A JSONArray value, or null if the index has no value, or if the value is not a JSONArray.</returns>
		public JSONArray optJSONArray(int i)
		{
			object obj = opt(i);
			if (obj is JSONArray)
				return (JSONArray)obj;
			return null;
		}

		/// <summary>
		/// Get the optional JSONObject associated with an index.
		/// Null is returned if the key is not found, or null if the index has
		/// no value, or if the value is not a JSONObject.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>A JSONObject value</returns>
		public JSONObject optJSONObject(int i)
		{
			object obj = opt(i);
			if (obj is JSONObject)
			{
				return (JSONObject)obj;
			}
			return null;
		}

		/// <summary>
		/// Get the optional string value associated with an index. It returns an
		/// empty string if there is no value at that index. If the value
		/// is not a string and is not null, then it is coverted to a string.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <returns>A String value</returns>
		public string optString(int i)
		{
			return optString(i, "");
		}

		/// <summary>
		/// Get the optional string associated with an index.
		/// The defaultValue is returned if the key is not found.
		/// </summary>
		/// <param name="i">index subscript</param>
		/// <param name="defaultValue">The default value</param>
		/// <returns>A string value</returns>
		public string optString(int i, string defaultValue)
		{
			object obj = opt(i);
			if (obj != null)
			{
				return obj.ToString();
			}
			return defaultValue;
		}

		#endregion

		#region Put methods - use indexer instead
/**
 * OMITTED:
 * public JSONArray put(bool val)
 * public JSONArray put(double val)
 * public JSONArray put(int val)		
*/
		/// <summary>
		/// Append an object value.
		/// </summary>
		/// <param name="val">An object value.  The value should be a Boolean, Double, Integer, JSONArray, JSObject, or String, or the JSONObject.NULL object</param>
		/// <returns>this (JSONArray)</returns>
		public JSONArray put(object val)
		{
			myArrayList.Add(val);
			return this;
		}

/*
 * OMITTED:
 * public JSONArray put(int index, boolean value)
 * public JSONArray put(int index, double value)
 * public JSONArray put(int index, int value)
 */
		/// <summary>
		/// Put or replace a boolean value in the JSONArray.
		/// </summary>
		/// <param name="i">
		/// The subscript. If the index is greater than the length of
		/// the JSONArray, then null elements will be added as necessary to pad it out.
		/// </param>
		/// <param name="val">An object value.</param>
		/// <returns>this (JSONArray)</returns>
		public JSONArray put(int i, object val)
		{
			if (i < 0)
			{
				throw new ArgumentOutOfRangeException("i", i, "Negative indexes illegal");
			}
			else if (val == null)
			{
				throw new ArgumentNullException("val", "Object cannt be null");
			}
			else if (i < myArrayList.Count)
			{
				myArrayList.Insert(i, val);
			}
			// NOTE! Since i is >= Count, fill null vals before index i, then append new object at i
			else
			{
				while (i != myArrayList.Count)
				{
					myArrayList.Add(null);
				}
				myArrayList.Add(val);
			}
			return this;
		}
		#endregion

		/// <summary>
		/// Produce a JSONObject by combining a JSONArray of names with the values
		/// of this JSONArray.
		/// </summary>
		/// <param name="names">
		/// A JSONArray containing a list of key strings. These will be paired with the values.
		/// </param>
		/// <returns>A JSONObject, or null if there are no names or if this JSONArray</returns>
		public JSONObject toJSONObject(JSONArray names)
		{
			if (names == null || names.Length() == 0 || this.Length() == 0) 
			{
				return null;
			}
			JSONObject jo = new JSONObject();
			for (int i=0; i <names.Length(); i++)
			{
				jo.put(names.getString(i),this.opt(i));
			}
			return jo;
		}

		/// <summary>
		/// Make an JSON external form string of this JSONArray. For compactness, no
		/// unnecessary whitespace is added.
		/// </summary>
		/// <returns>a printable, displayable, transmittable representation of the array.</returns>
		public override string ToString()
		{
			return '['+ join(",") + ']';
		}
	}
}
