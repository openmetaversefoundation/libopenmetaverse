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
	}
}
