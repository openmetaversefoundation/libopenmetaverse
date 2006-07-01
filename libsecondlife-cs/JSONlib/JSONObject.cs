using System;
using System.Collections;
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
        #region Local struct JSONNull
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
        #endregion

        ///<summary>The hash map where the JSONObject's properties are kept.</summary>
        private Hashtable myHashMap;

        ///<summary>A shadow list of keys to enable access by sequence of insertion</summary>
        private ArrayList myKeyIndexList;

        /// <summary>
        /// It is sometimes more convenient and less ambiguous to have a NULL
        /// object than to use C#'s null value.
        /// JSONObject.NULL.toString() returns "null".
        /// </summary>
        public static readonly JSONNull NULL = new JSONNull();

        #region Constructors for JSONObject
        /// <summary>
        ///  Construct an empty JSONObject.
        /// </summary>
        public JSONObject()
        {
            myHashMap      = new Hashtable();
            myKeyIndexList = new ArrayList();
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

        // public JSONObject(Hashtable map)
        // By changing to arg to interface, all classes that implements IDictionary can be used
        // public interface IDictionary : ICollection, IEnumerable
        // Classes that implements IDictionary
        // 1. BaseChannelObjectWithProperties - Provides a base implementation of a channel object that wants to provide a dictionary interface to its properties.
        // 2. DictionaryBase - Provides the abstract (MustInherit in Visual Basic) base class for a strongly typed collection of key-and-value pairs.
        // 3. Hashtable - Represents a collection of key-and-value pairs that are organized based on the hash code of the key.
        // 4. HybridDictionary - Implements IDictionary by using a ListDictionary while the collection is small, and then switching to a Hashtable when the collection gets large.
        // 5. ListDictionary - Implements IDictionary using a singly linked list. Recommended for collections that typically contain 10 items or less.
        // 6. PropertyCollection - Contains the properties of a DirectoryEntry.
        // 7. PropertyDescriptorCollection - Represents a collection of PropertyDescriptor objects.
        // 8. SortedList - Represents a collection of key-and-value pairs that are sorted by the keys and are accessible by key and by index.
        // 9. StateBag - Manages the view state of ASP.NET server controls, including pages. This class cannot be inherited.
        // See ms-help://MS.VSCC.2003/MS.MSDNQTR.2003FEB.1033/cpref/html/frlrfsystemcollectionsidictionaryclasstopic.htm


        /// <summary>
        /// Construct a JSONObject from a IDictionary
        /// </summary>
        /// <param name="map"></param>
        public JSONObject(IDictionary map)
        {
            myHashMap      = new Hashtable(map);
            myKeyIndexList = new ArrayList(map);
        }

        #endregion

        /// <summary>
        /// Accumulate values under a key. It is similar to the put method except
        /// that if there is already an object stored under the key then a
        /// JSONArray is stored under the key to hold all of the accumulated values.
        /// If there is already a JSONArray, then the new value is appended to it.
        /// In contrast, the put method replaces the previous value.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <param name="val">An object to be accumulated under the key.</param>
        /// <returns>this</returns>
        public JSONObject accumulate(string key, object val)
        {
            JSONArray a;
            object obj = opt(key);
            if (obj == null)
            {
                put(key, val);
            }
            else if (obj is JSONArray)
            {
                a = (JSONArray)obj;
                a.put(val);
            }
            else
            {
                a = new JSONArray();
                a.put(obj);
                a.put(val);
                put(key,a);
            }
            return this;
        }


        #region C# specific extensions
        /// <summary>
        /// Return the key for the associated index
        /// </summary>
        public string this[int i]
        {
            get
            {
                DictionaryEntry de = (DictionaryEntry)myKeyIndexList[i];
                return de.Key.ToString();
            }
        }

        /// <summary>
        /// Get/Add an object with the associated key
        /// </summary>
        public object this[string key]
        {
            get
            {
                return getValue(key);
            }
            set
            {
                put(key,value);
            }
        }

        /// <summary>
        /// Return the number of JSON items in hashtable
        /// </summary>
        public int Count
        {
            get
            {
                return myHashMap.Count;
            }
        }
        /// <summary>
        /// C# convenience method
        /// </summary>
        /// <returns>The Hashtable</returns>
        public IDictionary getDictionary()
        {
            return myHashMap;
        }
        #endregion


        #region Gettes for a value associated with a key - use indexer instead
        /// <summary>
        /// Alias to Java get method
        /// Get the value object associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>The object associated with the key.</returns>
        public object getValue(string key)
        {
            //return myHashMap[key];
            object obj = opt(key);
            if (obj == null)
            {
                throw new Exception("No such element");
            }
            return obj;
        }

        /// <summary>
        /// Get the boolean value associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>The truth.</returns>
        public bool getBool(string key)
        {
            object o = getValue(key);
            if (o is bool)
            {
                bool b = (bool)o;
                return b;
            }
            string msg = string.Format("JSONObject[{0}] is not a Boolean",JSONUtils.Enquote(key));
            throw new Exception(msg);
        }

        /// <summary>
        /// Get the double value associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>The double value</returns>
        public double getDouble(string key)
        {
            object o = getValue(key);
            if (o is double)
                return (double)o;
            if (o is float)
                return (double)o;

            if (o is string)
            {
                return Convert.ToDouble(o);
            }
            string msg = string.Format("JSONObject[{0}] is not a double",JSONUtils.Enquote(key));
            throw new Exception(msg);
        }

        /// <summary>
        /// Get the int value associated with a key.
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns> The integer value.</returns>
        public int getInt(string key)
        {
            object o = getValue(key);
            if (o is int)
            {
                return (int)o;
            }

            if (o is string)
            {
                return Convert.ToInt32(o);
            }
            string msg = string.Format("JSONObject[{0}] is not a int",JSONUtils.Enquote(key));
            throw new Exception(msg);
        }

        /// <summary>
        /// Get the JSONArray value associated with a key.
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns>A JSONArray which is the value</returns>
        public JSONArray getJSONArray(string key)
        {
            object o = getValue(key);
            if (o is JSONArray)
            {
                return (JSONArray)o;
            }
            string msg = string.Format("JSONObject[{0}]={1} is not a JSONArray",JSONUtils.Enquote(key),o);
            throw new Exception(msg);
        }

        /// <summary>
        /// Get the JSONObject value associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>A JSONObject which is the value.</returns>
        public JSONObject getJSONObject(string key)
        {
            object o = getValue(key);
            if (o is JSONObject)
            {
                return (JSONObject)o;
            }
            string msg = string.Format("JSONObject[{0}]={1} is not a JSONArray",JSONUtils.Enquote(key),o);
            throw new Exception(msg);
        }

        /// <summary>
        /// Get the string associated with a key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>A string which is the value.</returns>
        public string getString(string key)
        {
            return getValue(key).ToString();
        }
        #endregion


        /// <summary>
        /// Determine if the JSONObject contains a specific key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>true if the key exists in the JSONObject.</returns>
        public bool has(string key)
        {
            return myHashMap.ContainsKey(key);
        }


        /// <summary>
        /// Get an enumeration of the keys of the JSONObject.
        /// Added to be true to orginal Java implementation
        /// Indexers are easier to use
        /// </summary>
        /// <returns></returns>
        public IEnumerator keys()
        {
            return myHashMap.Keys.GetEnumerator();
        }

        /// <summary>
        /// Determine if the value associated with the key is null or if there is no value.
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns>true if there is no value associated with the key or if the valus is the JSONObject.NULL object</returns>
        public bool isNull(string key)
        {
            return JSONObject.NULL.Equals(opt(key));
        }

        /// <summary>
        /// Get the number of keys stored in the JSONObject.
        /// </summary>
        /// <returns>The number of keys in the JSONObject.</returns>
        public int Length()
        {
            return myHashMap.Count;
        }

        /// <summary>
        /// Produce a JSONArray containing the names of the elements of this JSONObject
        /// </summary>
        /// <returns>A JSONArray containing the key strings, or null if the JSONObject</returns>
        public JSONArray names()
        {
            JSONArray ja = new JSONArray();

            //NOTE!! I choose to use keys stored in the ArrayList, to maintain sequence order
            foreach (string key in myKeyIndexList)
            {
                ja.put(key);
            }
            if (ja.Length() == 0)
            {
                return null;
            }
            return ja;
        }

        /// <summary>
        /// Produce a string from a number.
        /// </summary>
        /// <param name="number">Number value type object</param>
        /// <returns>String representation of the number</returns>
        public string numberToString(object number)
        {
            if (number is float && ((float)number) == float.NaN)
            {
                string msg = string.Format("");
                throw new ArgumentException("object must be a valid number", "number");
            }
            if (number is double && ((double)number) == double.NaN)
            {
                string msg = string.Format("");
                throw new ArgumentException("object must be a valid number", "number");
            }

            // Shave off trailing zeros and decimal point, if possible

            string s = ((double) number).ToString(NumberFormatInfo.InvariantInfo).ToLower();

            if (s.IndexOf('e') < 0 && s.IndexOf('.') > 0)
            {
                while(s.EndsWith("0"))
                {
                    s.Substring(0,s.Length-1);
                }
                if (s.EndsWith("."))
                {
                    s.Substring(0,s.Length-1);
                }
            }
            return s;
        }

        #region Get an optional value associated with a key.
        /// <summary>
        /// Get an optional value associated with a key.
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns>An object which is the value, or null if there is no value.</returns>
        public object opt(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "Null key");
            }
            return myHashMap[key];
        }

        /// <summary>
        /// Get an optional value associated with a key.
        /// It returns false if there is no such key, or if the value is not
        /// Boolean.TRUE or the String "true".
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>bool value object</returns>
        public bool optBoolean(string key)
        {
            return optBoolean(key, false);
        }

        /// <summary>
        /// Get an optional value associated with a key.
        /// It returns false if there is no such key, or if the value is not
        /// Boolean.TRUE or the String "true".
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <param name="defaultValue">The preferred return value if conversion fails</param>
        /// <returns>bool value object</returns>
        public bool optBoolean(string key, bool defaultValue)
        {
            object obj = opt(key);
            if (obj != null)
            {
                if (obj is bool)
                    return (bool)obj;
                if (obj is string)
                {
                    return Convert.ToBoolean(obj);
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Get an optional double associated with a key,
        /// or NaN if there is no such key or if its value is not a number.
        /// If the value is a string, an attempt will be made to evaluate it as
        /// a number.
        /// </summary>
        /// <param name="key">A string which is the key.</param>
        /// <returns>A double value object</returns>
        public double optDouble(string key)
        {
            return optDouble(key, double.NaN);
        }

        /// <summary>
        /// Get an optional double associated with a key,
        /// or NaN if there is no such key or if its value is not a number.
        /// If the value is a string, an attempt will be made to evaluate it as
        /// a number.
        /// </summary>
        /// <param name="key">A string which is the key.</param>
        /// <param name="defaultValue">The default</param>
        /// <returns>A double value object</returns>
        public double optDouble(string key, double defaultValue)
        {
            object obj = opt(key);
            if (obj != null)
            {
                if (obj is double)
                    return (double)obj;
                if (obj is float)
                    return (double)obj;
                if (obj is string)
                {
                    return Convert.ToDouble(obj);
                }
            }
            return defaultValue;
        }

        /// <summary>
      ///  Get an optional double associated with a key, or the
      ///  defaultValue if there is no such key or if its value is not a number.
      ///  If the value is a string, an attempt will be made to evaluate it as
      ///  number.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>An int object value</returns>
        public int optInt(string key)
        {
            return optInt(key, 0);
        }

        /// <summary>
        ///  Get an optional double associated with a key, or the
        ///  defaultValue if there is no such key or if its value is not a number.
        ///  If the value is a string, an attempt will be made to evaluate it as
        ///  number.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <param name="defaultValue">The default value</param>
        /// <returns>An int object value</returns>
        public int optInt(string key, int defaultValue)
        {
            object obj = opt(key);
            if (obj != null)
            {
                if (obj is int)
                    return (int)obj;
                if (obj is string)
                    return Convert.ToInt32(obj);
            }
            return defaultValue;
        }

        /// <summary>
        /// Get an optional JSONArray associated with a key.
        /// It returns null if there is no such key, or if its value is not a JSONArray
        /// </summary>
        /// <param name="key">A key string</param>
        /// <returns>A JSONArray which is the value</returns>
        public JSONArray optJSONArray(string key)
        {
            object obj = opt(key);
            if (obj is JSONArray)
            {
                return (JSONArray)obj;
            }
            return null;
        }

        /// <summary>
        /// Get an optional JSONObject associated with a key.
        /// It returns null if there is no such key, or if its value is not a JSONObject.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>A JSONObject which is the value</returns>
        public JSONObject optJSONObject(string key)
        {
            object obj = opt(key);
            if (obj is JSONObject)
            {
                return (JSONObject)obj;
            }
            return null;
        }

        /// <summary>
        /// Get an optional string associated with a key.
        /// It returns an empty string if there is no such key. If the value is not
        /// a string and is not null, then it is coverted to a string.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <returns>A string which is the value.</returns>
        public string optString(string key)
        {
            object obj = opt(key);

            return optString(key, "");
        }

        /// <summary>
        /// Get an optional string associated with a key.
        /// It returns the defaultValue if there is no such key.
        /// </summary>
        /// <param name="key">A key string.</param>
        /// <param name="defaultValue">The default</param>
        /// <returns>A string which is the value.</returns>
        public string optString(string key, string defaultValue)
        {
            object obj = opt(key);
            if (obj != null)
            {
                return obj.ToString();
            }
            return defaultValue;
        }
        #endregion

        #region Put methods for adding key/value pairs
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
                    string test = key;
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
    /// Add a key value pair
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <returns></returns>
        public JSONObject putOpt(string key, object val)
        {
            if (val != null)
            {
                put(key,val);
            }
            return this;
        }
        #endregion

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

    /// <summary>
    /// Append an array of JSONObjects to current object
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
        public JSONArray toJSONArray(JSONArray names)
        {
            if (names == null | names.Length() == 0)
                return null;

            JSONArray ja = new JSONArray();
            for (int i=0; i<names.Length(); i++)
            {
                ja.put(this.opt(names.getString(i)));
            }
            return ja;
        }

    /// <summary>
    /// Overridden to return a JSON formattet object as a string
    /// </summary>
    /// <returns>JSON object as formatted string</returns>
        public override string ToString()
        {
            object obj = null;
            //string s;
            StringBuilder sb = new StringBuilder();

            sb.Append('{');
            foreach (string key in myHashMap.Keys)  //NOTE! Could also use myKeyIndexList !!!
            {
                if (obj != null)
                    sb.Append(',');
                obj = myHashMap[key];
                if (obj != null)
                {
                    sb.Append(JSONUtils.Enquote(key));
                    sb.Append(':');

                    if (obj is string)
                    {
                        sb.Append(JSONUtils.Enquote((string)obj));
                    }
                    else if (obj is float || obj is double)
                    {
                        sb.Append(numberToString(obj));
                    }
                    else if (obj is bool)
                    {
                        sb.Append(obj.ToString().ToLower());
                    }
                    else
                    {
                        sb.Append(obj.ToString());
                    }
                }
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
