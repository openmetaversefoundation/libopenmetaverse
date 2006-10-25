using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Globalization;

/*
 * A JSONObject is an unordered collection of name/value pairs. Its
 * external form is a string wrapped in curly braces with colons between the
 * names and values, and commas between the values and names. The internal form
 * is an object having get() and opt() methods for accessing the values by name,
 * and put() methods for adding or replacing values by name. The values can be
 * any of these types: Boolean, JSONArray, JSONObject, Number, String, or the
 * JSONObject.NULL object.
 * <p>
 * The constructor can convert an external form string into an internal form
 * Java object. The toString() method creates an external form string.
 * <p>
 * A get() method returns a value if one can be found, and throws an exception
 * if one cannot be found. An opt() method returns a default value instead of
 * throwing an exception, and so is useful for obtaining optional values.
 * <p>
 * The generic get() and opt() methods return an object, which you can cast or
 * query for type. There are also typed get() and opt() methods that do typing
 * checking and type coersion for you.
 * <p>
 * The texts produced by the toString() methods are very strict.
 * The constructors are more forgiving in the texts they will accept.
 * <ul>
 * <li>An extra comma may appear just before the closing brace.</li>
 * <li>Strings may be quoted with single quotes.</li>
 * <li>Strings do not need to be quoted at all if they do not contain leading
 *     or trailing spaces, and if they do not contain any of these characters:
 *     { } [ ] / \ : , </li>
 * <li>Numbers may have the 0- (octal) or 0x- (hex) prefix.</li>
 * </ul>
 * <p>
 * Public Domain 2002 JSON.org
 * @author JSON.org
 * @version 0.1
 * <p>
 * Ported to C# by Are Bjolseth, teleplan.no
 * TODO:
 * 1. Implement Custom exceptions
 * 2. Add indexer JSONObject[i] = object,     and object = JSONObject[i];
 * 3. Add indexer JSONObject["key"] = object, and object = JSONObject["key"]
 * 4. Add unit testing
 * 5. Add log4net
 */
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
    public class JSONObject
    {
        /// <summary>
    /// Make a Null object
    /// JSONObject.NULL is equivalent to the value that JavaScript calls null,
    /// whilst C#'s null is equivalent to the value that JavaScript calls undefined.
        /// </summary>
        public struct JSONNull
        {
            /*
            public object clone()
            {
                return this;
            }
            */
            /*
            public bool equals(object obj)
            {
                return (obj == null) || (obj == this);
            }
            */
      /// <summary>
      /// Overriden to return "null"
      /// </summary>
      /// <returns>null</returns>
            public override string ToString()
            {
                //return base.ToString ();
                return "null";
            }
        }

        ///<summary>The hash map where the JSONObject's properties are kept.</summary>
        private Dictionary<string, object> myHashMap;

        ///<summary>A shadow list of keys to enable access by sequence of insertion</summary>
        private List<string> myKeyIndexList;

        /// <summary>
        /// It is sometimes more convenient and less ambiguous to have a NULL
        /// object than to use C#'s null value.
        /// JSONObject.NULL.toString() returns "null".
        /// </summary>
        public static readonly JSONNull NULL = new JSONNull();


        /// <summary>
        ///  Construct an empty JSONObject.
        /// </summary>
        public JSONObject()
        {
            myHashMap      = new Dictionary<string,object>();
            myKeyIndexList = new List<string>();
        }

        /// <summary>
        /// Construct a JSONObject from a JSONTokener.
        /// </summary>
        /// <param name="x">A JSONTokener object containing the source string.</param>
        public JSONObject(JSONTokener x) : this()
        {
            char c;
            string key;
            if (x.next() == '%')
            {
                x.unescape();
            }
            x.back();
            if (x.nextClean() != '{')
            {
                throw new Exception("A JSONObject must begin with '{'");
            }
            while (true)
            {
                c = x.nextClean();
                switch (c)
                {
                    case (char)0:
                        throw new Exception("A JSONObject must end with '}'");
                    case '}':
                        return;
                    default:
                        x.back();
                        key = x.nextObject().ToString();
                        break;
                }
                if (x.nextClean() != ':')
                {
                    throw new Exception("Expected a ':' after a key");
                }
                object obj = x.nextObject();
                myHashMap.Add(key, obj);
                myKeyIndexList.Add(key);
                switch (x.nextClean())
                {
                    case ',':
                        if (x.nextClean() == '}')
                        {
                            return;
                        }
                        x.back();
                        break;
                    case '}':
                        return;
                    default:
                        throw new Exception("Expected a ',' or '}'");
                }
            }
        }


        /// <summary>
        /// Construct a JSONObject from a string.
        /// </summary>
        /// <param name="sJSON">A string beginning with '{' and ending with '}'.</param>
        public JSONObject(string sJSON) : this(new JSONTokener(sJSON))
        {

        }

        /// <summary>
        /// C# convenience method
        /// </summary>
        /// <returns>The Dictionary</returns>
        public Dictionary<string, object> getDictionary()
        {
            return myHashMap;
        }

        // OMITTED - all put methods can be replaced by a indexer in C#
        //         - ===================================================
        // public JSONObject put(String key, boolean value)
        // public JSONObject put(String key, double value)
        // public JSONObject put(String key, int value)

        /// <summary>
        /// Put a key/value pair in the JSONObject. If the value is null,
        /// then the key will be removed from the JSONObject if it is present.
        /// </summary>
        /// <param name="key"> A key string.</param>
        /// <param name="val">
        /// An object which is the value. It should be of one of these
        /// types: Boolean, Double, Integer, JSONArray, JSONObject, String, or the
        /// JSONObject.NULL object.
        /// </param>
        /// <returns>JSONObject</returns>
        public JSONObject put(string key, object val)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "key cannot be null");
            }
            if (val != null)
            {
                if (!myHashMap.ContainsKey(key))
                {
                    myHashMap.Add(key,val);
                    myKeyIndexList.Add(key);
                }
                else
                {
                    myHashMap[key]=val;
                }
            }
            else
            {
                remove(key);
            }
            return this;
        }

    /// <summary>
    /// Remove a object assosiateted with the given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
        public object remove(string key)
        {
            if (myHashMap.ContainsKey(key))
            {
                // TODO - does it really work ???
                object obj = myHashMap[key];
                myHashMap.Remove(key);
                myKeyIndexList.Remove(key);
                return obj;
            }
            return null;
        }
    }
}
