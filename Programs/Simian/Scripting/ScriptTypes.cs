using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace Simian
{
    [Flags]
    public enum Changed : uint
    {
        INVENTORY = 1,
        COLOR = 2,
        SHAPE = 4,
        SCALE = 8,
        TEXTURE = 16,
        LINK = 32,
        ALLOWED_DROP = 64,
        OWNER = 128,
        REGION_RESTART = 256,
        REGION = 512,
        TELEPORT = 1024
    }

    public static class ScriptTypes
    {
        #region LSL Types

        public struct LSL_Vector
        {
            public double x;
            public double y;
            public double z;

            #region Constructors

            public LSL_Vector(LSL_Vector vector)
            {
                x = (float)vector.x;
                y = (float)vector.y;
                z = (float)vector.z;
            }

            public LSL_Vector(double X, double Y, double Z)
            {
                x = X;
                y = Y;
                z = Z;
            }

            public LSL_Vector(string str)
            {
                str = str.Replace('<', ' ');
                str = str.Replace('>', ' ');
                string[] tmps = str.Split(new Char[] { ',', '<', '>' });
                if (tmps.Length < 3)
                {
                    x = y = z = 0;
                    return;
                }
                bool res;
                res = Double.TryParse(tmps[0], out x);
                res = res & Double.TryParse(tmps[1], out y);
                res = res & Double.TryParse(tmps[2], out z);
            }

            #endregion

            #region Overriders

            public override string ToString()
            {
                string s = String.Format("<{0:0.000000},{1:0.000000},{2:0.000000}>", x, y, z);
                return s;
            }

            public static explicit operator LSL_String(LSL_Vector vec)
            {
                string s = String.Format("<{0:0.000000},{1:0.000000},{2:0.000000}>", vec.x, vec.y, vec.z);
                return new LSL_String(s);
            }

            public static explicit operator string(LSL_Vector vec)
            {
                string s = String.Format("<{0:0.000000},{1:0.000000},{2:0.000000}>", vec.x, vec.y, vec.z);
                return s;
            }

            public static explicit operator LSL_Vector(string s)
            {
                return new LSL_Vector(s);
            }

            public static implicit operator LSL_List(LSL_Vector vec)
            {
                return new LSL_List(new object[] { vec });
            }

            public static bool operator ==(LSL_Vector lhs, LSL_Vector rhs)
            {
                return (lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z);
            }

            public static bool operator !=(LSL_Vector lhs, LSL_Vector rhs)
            {
                return !(lhs == rhs);
            }

            public override int GetHashCode()
            {
                return (x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode());
            }

            public override bool Equals(object o)
            {
                if (!(o is LSL_Vector)) return false;

                LSL_Vector vector = (LSL_Vector)o;

                return (x == vector.x && y == vector.y && z == vector.z);
            }

            public static LSL_Vector operator -(LSL_Vector vector)
            {
                return new LSL_Vector(-vector.x, -vector.y, -vector.z);
            }

            #endregion

            #region Vector & Vector Math

            // Vector-Vector Math
            public static LSL_Vector operator +(LSL_Vector lhs, LSL_Vector rhs)
            {
                return new LSL_Vector(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
            }

            public static LSL_Vector operator -(LSL_Vector lhs, LSL_Vector rhs)
            {
                return new LSL_Vector(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
            }

            public static LSL_Float operator *(LSL_Vector lhs, LSL_Vector rhs)
            {
                return Dot(lhs, rhs);
            }

            public static LSL_Vector operator %(LSL_Vector v1, LSL_Vector v2)
            {
                //Cross product
                LSL_Vector tv;
                tv.x = (v1.y * v2.z) - (v1.z * v2.y);
                tv.y = (v1.z * v2.x) - (v1.x * v2.z);
                tv.z = (v1.x * v2.y) - (v1.y * v2.x);
                return tv;
            }

            #endregion

            #region Vector & Float Math

            // Vector-Float and Float-Vector Math
            public static LSL_Vector operator *(LSL_Vector vec, float val)
            {
                return new LSL_Vector(vec.x * val, vec.y * val, vec.z * val);
            }

            public static LSL_Vector operator *(float val, LSL_Vector vec)
            {
                return new LSL_Vector(vec.x * val, vec.y * val, vec.z * val);
            }

            public static LSL_Vector operator /(LSL_Vector v, float f)
            {
                v.x = v.x / f;
                v.y = v.y / f;
                v.z = v.z / f;
                return v;
            }

            #endregion

            #region Vector & Double Math

            public static LSL_Vector operator *(LSL_Vector vec, double val)
            {
                return new LSL_Vector(vec.x * val, vec.y * val, vec.z * val);
            }

            public static LSL_Vector operator *(double val, LSL_Vector vec)
            {
                return new LSL_Vector(vec.x * val, vec.y * val, vec.z * val);
            }

            public static LSL_Vector operator /(LSL_Vector v, double f)
            {
                v.x = v.x / f;
                v.y = v.y / f;
                v.z = v.z / f;
                return v;
            }

            #endregion

            #region Vector & Rotation Math

            // Vector-Rotation Math
            public static LSL_Vector operator *(LSL_Vector v, LSL_Rotation r)
            {
                LSL_Rotation vq = new LSL_Rotation(v.x, v.y, v.z, 0);
                LSL_Rotation nq = new LSL_Rotation(-r.x, -r.y, -r.z, r.s);

                // adapted for operator * computing "b * a"
                LSL_Rotation result = nq * (vq * r);

                return new LSL_Vector(result.x, result.y, result.z);
            }

            public static LSL_Vector operator /(LSL_Vector v, LSL_Rotation r)
            {
                r.s = -r.s;
                return v * r;
            }

            #endregion

            #region Static Helper Functions

            public static double Dot(LSL_Vector v1, LSL_Vector v2)
            {
                return (v1.x * v2.x) + (v1.y * v2.y) + (v1.z * v2.z);
            }

            public static LSL_Vector Cross(LSL_Vector v1, LSL_Vector v2)
            {
                return new LSL_Vector
                    (
                    v1.y * v2.z - v1.z * v2.y,
                    v1.z * v2.x - v1.x * v2.z,
                    v1.x * v2.y - v1.y * v2.x
                    );
            }

            public static double Mag(LSL_Vector v)
            {
                return Math.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
            }

            public static LSL_Vector Norm(LSL_Vector vector)
            {
                double mag = Mag(vector);
                return new LSL_Vector(vector.x / mag, vector.y / mag, vector.z / mag);
            }

            #endregion

            public static readonly LSL_Vector Zero = new LSL_Vector();
        }

        public struct LSL_Rotation
        {
            public double x;
            public double y;
            public double z;
            public double s;

            #region Constructors

            public LSL_Rotation(LSL_Rotation Quat)
            {
                x = (float)Quat.x;
                y = (float)Quat.y;
                z = (float)Quat.z;
                s = (float)Quat.s;
                if (x == 0 && y == 0 && z == 0 && s == 0)
                    s = 1;
            }

            public LSL_Rotation(double X, double Y, double Z, double S)
            {
                x = X;
                y = Y;
                z = Z;
                s = S;
                if (x == 0 && y == 0 && z == 0 && s == 0)
                    s = 1;
            }

            public LSL_Rotation(string str)
            {
                str = str.Replace('<', ' ');
                str = str.Replace('>', ' ');
                string[] tmps = str.Split(new Char[] { ',', '<', '>' });
                if (tmps.Length < 4)
                {
                    x = y = z = s = 0;
                    return;
                }
                bool res;
                res = Double.TryParse(tmps[0], out x);
                res = res & Double.TryParse(tmps[1], out y);
                res = res & Double.TryParse(tmps[2], out z);
                res = res & Double.TryParse(tmps[3], out s);
                if (x == 0 && y == 0 && z == 0 && s == 0)
                    s = 1;
            }

            #endregion

            #region Overriders

            public override int GetHashCode()
            {
                return (x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ s.GetHashCode());
            }

            public override bool Equals(object o)
            {
                if (!(o is LSL_Rotation)) return false;

                LSL_Rotation quaternion = (LSL_Rotation)o;

                return x == quaternion.x && y == quaternion.y && z == quaternion.z && s == quaternion.s;
            }

            public override string ToString()
            {
                string st = String.Format("<{0:0.000000},{1:0.000000},{2:0.000000},{3:0.000000}>", x, y, z, s);
                return st;
            }

            public static explicit operator string(LSL_Rotation r)
            {
                string s = String.Format("<{0:0.000000},{1:0.000000},{2:0.000000},{3:0.000000}>", r.x, r.y, r.z, r.s);
                return s;
            }

            public static explicit operator LSL_String(LSL_Rotation r)
            {
                string s = String.Format("<{0:0.000000},{1:0.000000},{2:0.000000},{3:0.000000}>", r.x, r.y, r.z, r.s);
                return new LSL_String(s);
            }

            public static explicit operator LSL_Rotation(string s)
            {
                return new LSL_Rotation(s);
            }

            public static implicit operator LSL_List(LSL_Rotation r)
            {
                return new LSL_List(new object[] { r });
            }

            public static bool operator ==(LSL_Rotation lhs, LSL_Rotation rhs)
            {
                // Return true if the fields match:
                return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.s == rhs.s;
            }

            public static bool operator !=(LSL_Rotation lhs, LSL_Rotation rhs)
            {
                return !(lhs == rhs);
            }

            public static double Mag(LSL_Rotation q)
            {
                return Math.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.s * q.s);
            }

            #endregion

            #region Operators

            public static LSL_Rotation operator +(LSL_Rotation a, LSL_Rotation b)
            {
                return new LSL_Rotation(a.x + b.x, a.y + b.y, a.z + b.z, a.s + b.s);
            }

            public static LSL_Rotation operator /(LSL_Rotation a, LSL_Rotation b)
            {
                b.s = -b.s;
                return a * b;
            }

            public static LSL_Rotation operator -(LSL_Rotation a, LSL_Rotation b)
            {
                return new LSL_Rotation(a.x - b.x, a.y - b.y, a.z - b.z, a.s - b.s);
            }

            // using the equations below, we need to do "b * a" to be compatible with LSL
            public static LSL_Rotation operator *(LSL_Rotation b, LSL_Rotation a)
            {
                LSL_Rotation c;
                c.x = a.s * b.x + a.x * b.s + a.y * b.z - a.z * b.y;
                c.y = a.s * b.y + a.y * b.s + a.z * b.x - a.x * b.z;
                c.z = a.s * b.z + a.z * b.s + a.x * b.y - a.y * b.x;
                c.s = a.s * b.s - a.x * b.x - a.y * b.y - a.z * b.z;
                return c;
            }

            #endregion

            public static readonly LSL_Rotation Identity = new LSL_Rotation(0.0, 0.0, 0.0, 1.0);
        }

        public class LSL_List
        {
            private object[] value;

            #region Properties

            public int Length
            {
                get
                {
                    if (value == null)
                        value = new Object[0];
                    return value.Length;
                }
            }

            public object[] Data
            {
                get
                {
                    if (value == null)
                        value = new Object[0];
                    return value;
                }

                set { this.value = value; }
            }

            #endregion

            #region Constructors

            public LSL_List(params object[] args)
            {
                value = new object[args.Length];
                value = args;
            }

            #endregion

            // Member functions to obtain item as specific types.
            // For cases where implicit conversions would apply if items
            // were not in a list (e.g. integer to float, but not float
            // to integer) functions check for alternate types so as to
            // down-cast from Object to the correct type.
            // Note: no checks for item index being valid are performed

            #region Accessor Functions

            public ScriptTypes.LSL_Float GetLSLFloatItem(int itemIndex)
            {
                if (value[itemIndex] is ScriptTypes.LSL_Integer)
                {
                    return (ScriptTypes.LSL_Integer)value[itemIndex];
                }
                else if (value[itemIndex] is Int32)
                {
                    return new ScriptTypes.LSL_Float((int)value[itemIndex]);
                }
                else if (value[itemIndex] is float)
                {
                    return new ScriptTypes.LSL_Float((float)value[itemIndex]);
                }
                else if (value[itemIndex] is Double)
                {
                    return new ScriptTypes.LSL_Float((Double)value[itemIndex]);
                }
                else
                {
                    return (ScriptTypes.LSL_Float)value[itemIndex];
                }
            }

            public ScriptTypes.LSL_String GetLSLStringItem(int itemIndex)
            {
                if (value[itemIndex] is ScriptTypes.LSL_Key)
                {
                    return (ScriptTypes.LSL_Key)value[itemIndex];
                }
                else if (value[itemIndex] is String)
                {
                    return new ScriptTypes.LSL_String((string)value[itemIndex]);
                }
                else
                {
                    return (ScriptTypes.LSL_String)value[itemIndex];
                }
            }

            public ScriptTypes.LSL_Integer GetLSLIntegerItem(int itemIndex)
            {
                if (value[itemIndex] is ScriptTypes.LSL_Integer)
                    return (ScriptTypes.LSL_Integer)value[itemIndex];
                else if (value[itemIndex] is Int32)
                    return new LSL_Integer((int)value[itemIndex]);
                else
                    throw new InvalidCastException();
            }

            public ScriptTypes.LSL_Vector GetVector3Item(int itemIndex)
            {
                return (ScriptTypes.LSL_Vector)value[itemIndex];
            }

            public ScriptTypes.LSL_Rotation GetQuaternionItem(int itemIndex)
            {
                return (ScriptTypes.LSL_Rotation)value[itemIndex];
            }

            public ScriptTypes.LSL_Key GetKeyItem(int itemIndex)
            {
                return (ScriptTypes.LSL_Key)value[itemIndex];
            }

            #endregion

            #region Operators

            public static LSL_List operator +(LSL_List a, LSL_List b)
            {
                object[] tmp;
                tmp = new object[a.Length + b.Length];
                a.Data.CopyTo(tmp, 0);
                b.Data.CopyTo(tmp, a.Length);
                return new LSL_List(tmp);
            }

            private void ExtendAndAdd(object o)
            {
                Array.Resize(ref value, Length + 1);
                value.SetValue(o, Length - 1);
            }

            public static LSL_List operator +(LSL_List a, LSL_String s)
            {
                a.ExtendAndAdd(s);
                return a;
            }

            public static LSL_List operator +(LSL_List a, LSL_Integer i)
            {
                a.ExtendAndAdd(i);
                return a;
            }

            public static LSL_List operator +(LSL_List a, LSL_Float d)
            {
                a.ExtendAndAdd(d);
                return a;
            }

            public static bool operator ==(LSL_List a, LSL_List b)
            {
                int la = -1;
                int lb = -1;
                try { la = a.Length; }
                catch (NullReferenceException) { }
                try { lb = b.Length; }
                catch (NullReferenceException) { }

                return la == lb;
            }

            public static bool operator !=(LSL_List a, LSL_List b)
            {
                int la = -1;
                int lb = -1;
                try { la = a.Length; }
                catch (NullReferenceException) { }
                try { lb = b.Length; }
                catch (NullReferenceException) { }

                return la != lb;
            }

            #endregion

            public void Add(object o)
            {
                object[] tmp;
                tmp = new object[value.Length + 1];
                value.CopyTo(tmp, 0);
                tmp[value.Length] = o;
                value = tmp;
            }

            public bool Contains(object o)
            {
                bool ret = false;
                foreach (object i in Data)
                {
                    if (i == o)
                    {
                        ret = true;
                        break;
                    }
                }
                return ret;
            }

            public LSL_List DeleteSublist(int start, int end)
            {
                // Not an easy one
                // If start <= end, remove that part
                // if either is negative, count from the end of the array
                // if the resulting start > end, remove all BUT that part

                Object[] ret;

                if (start < 0)
                    start = value.Length - start;

                if (start < 0)
                    start = 0;

                if (end < 0)
                    end = value.Length - end;
                if (end < 0)
                    end = 0;

                if (start > end)
                {
                    if (end >= value.Length)
                        return new LSL_List(new Object[0]);

                    if (start >= value.Length)
                        start = value.Length - 1;

                    return GetSublist(end, start);
                }

                // start >= 0 && end >= 0 here
                if (start >= value.Length)
                {
                    ret = new Object[value.Length];
                    Array.Copy(value, 0, ret, 0, value.Length);

                    return new LSL_List(ret);
                }

                if (end >= value.Length)
                    end = value.Length - 1;

                // now, this makes the math easier
                int remove = end + 1 - start;

                ret = new Object[value.Length - remove];
                if (ret.Length == 0)
                    return new LSL_List(ret);

                int src;
                int dest = 0;

                for (src = 0; src < value.Length; src++)
                {
                    if (src < start || src > end)
                        ret[dest++] = value[src];
                }

                return new LSL_List(ret);
            }

            public LSL_List GetSublist(int start, int end)
            {

                object[] ret;

                // Take care of neg start or end's
                // NOTE that either index may still be negative after
                // adding the length, so we must take additional
                // measures to protect against this. Note also that
                // after normalisation the negative indices are no
                // longer relative to the end of the list.

                if (start < 0)
                {
                    start = value.Length + start;
                }

                if (end < 0)
                {
                    end = value.Length + end;
                }

                // The conventional case is start <= end
                // NOTE that the case of an empty list is
                // dealt with by the initial test. Start
                // less than end is taken to be the most
                // common case.

                if (start <= end)
                {

                    // Start sublist beyond length
                    // Also deals with start AND end still negative
                    if (start >= value.Length || end < 0)
                    {
                        return new LSL_List();
                    }

                    // Sublist extends beyond the end of the supplied list
                    if (end >= value.Length)
                    {
                        end = value.Length - 1;
                    }

                    // Sublist still starts before the beginning of the list
                    if (start < 0)
                    {
                        start = 0;
                    }

                    ret = new object[end - start + 1];

                    Array.Copy(value, start, ret, 0, end - start + 1);

                    return new LSL_List(ret);

                }

                // Deal with the segmented case: 0->end + start->EOL

                else
                {

                    LSL_List result = null;

                    // If end is negative, then prefix list is empty
                    if (end < 0)
                    {
                        result = new LSL_List();
                        // If start is still negative, then the whole of
                        // the existing list is returned. This case is
                        // only admitted if end is also still negative.
                        if (start < 0)
                        {
                            return this;
                        }

                    }
                    else
                    {
                        result = GetSublist(0, end);
                    }

                    // If start is outside of list, then just return
                    // the prefix, whatever it is.
                    if (start >= value.Length)
                    {
                        return result;
                    }

                    return result + GetSublist(start, Data.Length);

                }
            }

            private static int compare(object left, object right, int ascending)
            {
                if (!left.GetType().Equals(right.GetType()))
                {
                    // unequal types are always "equal" for comparison purposes.
                    // this way, the bubble sort will never swap them, and we'll
                    // get that feathered effect we're looking for
                    return 0;
                }

                int ret = 0;

                if (left is LSL_Key)
                {
                    LSL_Key l = (LSL_Key)left;
                    LSL_Key r = (LSL_Key)right;
                    ret = String.CompareOrdinal(l.value, r.value);
                }
                else if (left is LSL_String)
                {
                    LSL_String l = (LSL_String)left;
                    LSL_String r = (LSL_String)right;
                    ret = String.CompareOrdinal(l.value, r.value);
                }
                else if (left is LSL_Integer)
                {
                    LSL_Integer l = (LSL_Integer)left;
                    LSL_Integer r = (LSL_Integer)right;
                    ret = Math.Sign(l.value - r.value);
                }
                else if (left is LSL_Float)
                {
                    LSL_Float l = (LSL_Float)left;
                    LSL_Float r = (LSL_Float)right;
                    ret = Math.Sign(l.value - r.value);
                }
                else if (left is LSL_Vector)
                {
                    LSL_Vector l = (LSL_Vector)left;
                    LSL_Vector r = (LSL_Vector)right;
                    ret = Math.Sign(LSL_Vector.Mag(l) - LSL_Vector.Mag(r));
                }
                else if (left is LSL_Rotation)
                {
                    LSL_Rotation l = (LSL_Rotation)left;
                    LSL_Rotation r = (LSL_Rotation)right;
                    ret = Math.Sign(LSL_Rotation.Mag(l) - LSL_Rotation.Mag(r));
                }

                if (ascending == 0)
                {
                    ret = 0 - ret;
                }

                return ret;
            }

            public LSL_List Sort(int stride, int ascending)
            {
                if (Data.Length == 0)
                    return new LSL_List(); // Don't even bother

                object[] ret = new object[Data.Length];
                Array.Copy(Data, 0, ret, 0, Data.Length);

                if (stride <= 0)
                {
                    stride = 1;
                }

                // we can optimize here in the case where stride == 1 and the list
                // consists of homogeneous types

                if (stride == 1)
                {
                    bool homogeneous = true;
                    int index;
                    for (index = 1; index < Data.Length; index++)
                    {
                        if (!Data[0].GetType().Equals(Data[index].GetType()))
                        {
                            homogeneous = false;
                            break;
                        }
                    }

                    if (homogeneous)
                    {
                        Array.Sort(ret, new HomogeneousComparer());
                        if (ascending == 0)
                        {
                            Array.Reverse(ret);
                        }
                        return new LSL_List(ret);
                    }
                }

                // Because of the desired type specific feathered sorting behavior
                // requried by the spec, we MUST use a non-optimized bubble sort here.
                // Anything else will give you the incorrect behavior.

                // begin bubble sort...
                int i;
                int j;
                int k;
                int n = Data.Length;

                for (i = 0; i < (n - stride); i += stride)
                {
                    for (j = i + stride; j < n; j += stride)
                    {
                        if (compare(ret[i], ret[j], ascending) > 0)
                        {
                            for (k = 0; k < stride; k++)
                            {
                                object tmp = ret[i + k];
                                ret[i + k] = ret[j + k];
                                ret[j + k] = tmp;
                            }
                        }
                    }
                }

                // end bubble sort

                return new LSL_List(ret);
            }

            #region CSV Methods

            public static LSL_List FromCSV(string csv)
            {
                return new LSL_List(csv.Split(','));
            }

            public string ToCSV()
            {
                string ret = "";
                foreach (object o in this.Data)
                {
                    if (ret == "")
                    {
                        ret = o.ToString();
                    }
                    else
                    {
                        ret = ret + ", " + o.ToString();
                    }
                }
                return ret;
            }

            private string ToSoup()
            {
                string output;
                output = String.Empty;
                if (value.Length == 0)
                {
                    return String.Empty;
                }
                foreach (object o in value)
                {
                    output = output + o.ToString();
                }
                return output;
            }

            public static explicit operator String(LSL_List l)
            {
                return l.ToSoup();
            }

            public static explicit operator LSL_String(LSL_List l)
            {
                return new LSL_String(l.ToSoup());
            }

            public override string ToString()
            {
                return ToSoup();
            }

            #endregion

            #region Statistic Methods

            public double Min()
            {
                double minimum = double.PositiveInfinity;
                double entry;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (double.TryParse(Data[i].ToString(), out entry))
                    {
                        if (entry < minimum) minimum = entry;
                    }
                }
                return minimum;
            }

            public double Max()
            {
                double maximum = double.NegativeInfinity;
                double entry;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (double.TryParse(Data[i].ToString(), out entry))
                    {
                        if (entry > maximum) maximum = entry;
                    }
                }
                return maximum;
            }

            public double Range()
            {
                return (this.Max() / this.Min());
            }

            public int NumericLength()
            {
                int count = 0;
                double entry;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (double.TryParse(Data[i].ToString(), out entry))
                    {
                        count++;
                    }
                }
                return count;
            }

            public static LSL_List ToDoubleList(LSL_List src)
            {
                LSL_List ret = new LSL_List();
                double entry;
                for (int i = 0; i < src.Data.Length - 1; i++)
                {
                    if (double.TryParse(src.Data[i].ToString(), out entry))
                    {
                        ret.Add(entry);
                    }
                }
                return ret;
            }

            public double Sum()
            {
                double sum = 0;
                double entry;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (double.TryParse(Data[i].ToString(), out entry))
                    {
                        sum = sum + entry;
                    }
                }
                return sum;
            }

            public double SumSqrs()
            {
                double sum = 0;
                double entry;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (double.TryParse(Data[i].ToString(), out entry))
                    {
                        sum = sum + Math.Pow(entry, 2);
                    }
                }
                return sum;
            }

            public double Mean()
            {
                return (this.Sum() / this.NumericLength());
            }

            public void NumericSort()
            {
                IComparer Numeric = new NumericComparer();
                Array.Sort(Data, Numeric);
            }

            public void AlphaSort()
            {
                IComparer Alpha = new AlphaComparer();
                Array.Sort(Data, Alpha);
            }

            public double Median()
            {
                return Qi(0.5);
            }

            public double GeometricMean()
            {
                double ret = 1.0;
                LSL_List nums = ToDoubleList(this);
                for (int i = 0; i < nums.Data.Length; i++)
                {
                    ret *= (double)nums.Data[i];
                }
                return Math.Exp(Math.Log(ret) / (double)nums.Data.Length);
            }

            public double HarmonicMean()
            {
                double ret = 0.0;
                LSL_List nums = ToDoubleList(this);
                for (int i = 0; i < nums.Data.Length; i++)
                {
                    ret += 1.0 / (double)nums.Data[i];
                }
                return ((double)nums.Data.Length / ret);
            }

            public double Variance()
            {
                double s = 0;
                LSL_List num = ToDoubleList(this);
                for (int i = 0; i < num.Data.Length; i++)
                {
                    s += Math.Pow((double)num.Data[i], 2);
                }
                return (s - num.Data.Length * Math.Pow(num.Mean(), 2)) / (num.Data.Length - 1);
            }

            public double StdDev()
            {
                return Math.Sqrt(this.Variance());
            }

            public double Qi(double i)
            {
                LSL_List j = this;
                j.NumericSort();

                if (Math.Ceiling(this.Length * i) == this.Length * i)
                {
                    return (double)((double)j.Data[(int)(this.Length * i - 1)] + (double)j.Data[(int)(this.Length * i)]) / 2;
                }
                else
                {
                    return (double)j.Data[((int)(Math.Ceiling(this.Length * i))) - 1];
                }
            }

            #endregion

            public string ToPrettyString()
            {
                string output;
                if (value.Length == 0)
                {
                    return "[]";
                }
                output = "[";
                foreach (object o in value)
                {
                    if (o is String)
                    {
                        output = output + "\"" + o + "\", ";
                    }
                    else
                    {
                        output = output + o.ToString() + ", ";
                    }
                }
                output = output.Substring(0, output.Length - 2);
                output = output + "]";
                return output;
            }

            #region Comparers

            class HomogeneousComparer : IComparer
            {
                public HomogeneousComparer()
                {
                }

                public int Compare(object lhs, object rhs)
                {
                    return compare(lhs, rhs, 1);
                }
            }

            public class AlphaComparer : IComparer
            {
                int IComparer.Compare(object x, object y)
                {
                    return string.Compare(x.ToString(), y.ToString());
                }
            }

            public class NumericComparer : IComparer
            {
                int IComparer.Compare(object x, object y)
                {
                    double a;
                    double b;
                    if (!double.TryParse(x.ToString(), out a))
                    {
                        a = 0.0;
                    }
                    if (!double.TryParse(y.ToString(), out b))
                    {
                        b = 0.0;
                    }
                    if (a < b)
                    {
                        return -1;
                    }
                    else if (a == b)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }

            #endregion

            #region Overriders

            public override bool Equals(object o)
            {
                if (!(o is LSL_List))
                    return false;

                return Data.Length == ((LSL_List)o).Data.Length;
            }

            public override int GetHashCode()
            {
                return Data.GetHashCode();
            }

            #endregion
        }

        public struct LSL_Key
        {
            public string value;

            #region Constructors

            public LSL_Key(string s)
            {
                value = s;
            }

            #endregion

            #region Methods

            static public bool Parse2Key(string s)
            {
                Regex isuuid = new Regex(@"^[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}$", RegexOptions.Compiled);
                if (isuuid.IsMatch(s))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            #endregion

            #region Operators

            static public implicit operator Boolean(LSL_Key k)
            {
                if (k.value.Length == 0 || k.value == "00000000-0000-0000-0000-000000000000")
                    return false;

                Regex isuuid = new Regex(@"^[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}$", RegexOptions.Compiled);
                return isuuid.IsMatch(k.value);
            }

            static public implicit operator LSL_Key(string s)
            {
                return new LSL_Key(s);
            }

            static public implicit operator String(LSL_Key k)
            {
                return k.value;
            }

            static public implicit operator LSL_String(LSL_Key k)
            {
                return k.value;
            }

            public static bool operator ==(LSL_Key k1, LSL_Key k2)
            {
                return k1.value == k2.value;
            }
            public static bool operator !=(LSL_Key k1, LSL_Key k2)
            {
                return k1.value != k2.value;
            }

            #endregion

            #region Overriders

            public override bool Equals(object o)
            {
                return o.ToString() == value;
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }

            public override string ToString()
            {
                return value;
            }

            #endregion

            public static readonly LSL_Key Zero = new LSL_Key("00000000-0000-0000-0000-000000000000");
            public static readonly LSL_Key Empty = new LSL_Key(String.Empty);
        }

        public struct LSL_String
        {
            public string value;

            #region Constructors

            public LSL_String(string s)
            {
                value = s;
            }

            public LSL_String(double d)
            {
                string s = String.Format("{0:0.000000}", d);
                value = s;
            }

            public LSL_String(LSL_Float f)
            {
                string s = String.Format("{0:0.000000}", f.value);
                value = s;
            }

            #endregion

            #region Operators

            static public implicit operator Boolean(LSL_String s)
            {
                if (s.value.Length == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            static public implicit operator String(LSL_String s)
            {
                return s.value;
            }

            static public implicit operator LSL_String(string s)
            {
                return new LSL_String(s);
            }

            public static string ToString(LSL_String s)
            {
                return s.value;
            }

            public override string ToString()
            {
                return value;
            }

            public static bool operator ==(LSL_String s1, string s2)
            {
                return s1.value == s2;
            }

            public static bool operator !=(LSL_String s1, string s2)
            {
                return s1.value != s2;
            }

            public static LSL_String operator +(LSL_String s1, LSL_String s2)
            {
                return new LSL_String(s1.value + s2.value);
            }

            public static explicit operator double(LSL_String s)
            {
                return new LSL_Float(s).value;
            }

            public static explicit operator LSL_Integer(LSL_String s)
            {
                return new LSL_Integer(s.value);
            }

            public static explicit operator LSL_String(double d)
            {
                return new LSL_String(d);
            }

            public static explicit operator LSL_String(LSL_Float f)
            {
                return new LSL_String(f);
            }

            static public explicit operator LSL_String(bool b)
            {
                if (b)
                    return new LSL_String("1");
                else
                    return new LSL_String("0");
            }

            public static implicit operator LSL_Vector(LSL_String s)
            {
                return new LSL_Vector(s.value);
            }

            public static implicit operator LSL_Rotation(LSL_String s)
            {
                return new LSL_Rotation(s.value);
            }

            public static implicit operator LSL_Float(LSL_String s)
            {
                return new LSL_Float(s);
            }

            public static implicit operator LSL_List(LSL_String s)
            {
                return new LSL_List(new object[] { s });
            }

            #endregion

            #region Overriders

            public override bool Equals(object o)
            {
                return value == o.ToString();
            }

            public override int GetHashCode()
            {
                return value.GetHashCode();
            }

            #endregion

            #region " Standard string functions "

            //Clone,CompareTo,Contains
            //CopyTo,EndsWith,Equals,GetEnumerator,GetHashCode,GetType,GetTypeCode
            //IndexOf,IndexOfAny,Insert,IsNormalized,LastIndexOf,LastIndexOfAny
            //Length,Normalize,PadLeft,PadRight,Remove,Replace,Split,StartsWith,Substring,ToCharArray,ToLowerInvariant
            //ToString,ToUpper,ToUpperInvariant,Trim,TrimEnd,TrimStart
            public bool Contains(string value) { return value.Contains(value); }
            public int IndexOf(string value) { return value.IndexOf(value); }
            public int Length { get { return value.Length; } }

            #endregion

            public static readonly LSL_String Empty = new LSL_String(String.Empty);
        }

        public struct LSL_Integer
        {
            public int value;

            #region Constructors

            public LSL_Integer(int i)
            {
                value = i;
            }

            public LSL_Integer(double d)
            {
                value = (int)d;
            }

            public LSL_Integer(string s)
            {
                Regex r = new Regex("(^[ ]*0[xX][0-9A-Fa-f][0-9A-Fa-f]*)|(^[ ]*-?[0-9][0-9]*)");
                Match m = r.Match(s);
                string v = m.Groups[0].Value;

                if (v == String.Empty)
                {
                    value = 0;
                }
                else
                {
                    try
                    {
                        if (v.Contains("x") || v.Contains("X"))
                        {
                            value = int.Parse(v.Substring(2), System.Globalization.NumberStyles.HexNumber);
                        }
                        else
                        {
                            value = int.Parse(v, System.Globalization.NumberStyles.Integer);
                        }
                    }
                    catch (OverflowException)
                    {
                        value = -1;
                    }
                }
            }

            #endregion

            #region Operators

            static public implicit operator int(LSL_Integer i)
            {
                return i.value;
            }

            static public explicit operator uint(LSL_Integer i)
            {
                return (uint)i.value;
            }

            static public explicit operator LSL_String(LSL_Integer i)
            {
                return new LSL_String(i.ToString());
            }

            public static implicit operator LSL_List(LSL_Integer i)
            {
                return new LSL_List(new object[] { i });
            }

            static public implicit operator Boolean(LSL_Integer i)
            {
                if (i.value == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            static public implicit operator LSL_Integer(int i)
            {
                return new LSL_Integer(i);
            }

            static public explicit operator LSL_Integer(string s)
            {
                return new LSL_Integer(s);
            }

            static public implicit operator LSL_Integer(uint u)
            {
                return new LSL_Integer(u);
            }

            static public explicit operator LSL_Integer(double d)
            {
                return new LSL_Integer(d);
            }

            static public explicit operator LSL_Integer(LSL_Float f)
            {
                return new LSL_Integer(f.value);
            }

            static public implicit operator LSL_Integer(bool b)
            {
                if (b)
                    return new LSL_Integer(1);
                else
                    return new LSL_Integer(0);
            }

            static public LSL_Integer operator ==(LSL_Integer i1, LSL_Integer i2)
            {
                bool ret = i1.value == i2.value;
                return new LSL_Integer((ret ? 1 : 0));
            }

            static public LSL_Integer operator !=(LSL_Integer i1, LSL_Integer i2)
            {
                bool ret = i1.value != i2.value;
                return new LSL_Integer((ret ? 1 : 0));
            }

            static public LSL_Integer operator <(LSL_Integer i1, LSL_Integer i2)
            {
                bool ret = i1.value < i2.value;
                return new LSL_Integer((ret ? 1 : 0));
            }
            static public LSL_Integer operator <=(LSL_Integer i1, LSL_Integer i2)
            {
                bool ret = i1.value <= i2.value;
                return new LSL_Integer((ret ? 1 : 0));
            }

            static public LSL_Integer operator >(LSL_Integer i1, LSL_Integer i2)
            {
                bool ret = i1.value > i2.value;
                return new LSL_Integer((ret ? 1 : 0));
            }

            static public LSL_Integer operator >=(LSL_Integer i1, LSL_Integer i2)
            {
                bool ret = i1.value >= i2.value;
                return new LSL_Integer((ret ? 1 : 0));
            }

            static public LSL_Integer operator +(LSL_Integer i1, int i2)
            {
                return new LSL_Integer(i1.value + i2);
            }

            static public LSL_Integer operator -(LSL_Integer i1, int i2)
            {
                return new LSL_Integer(i1.value - i2);
            }

            static public LSL_Integer operator *(LSL_Integer i1, int i2)
            {
                return new LSL_Integer(i1.value * i2);
            }

            static public LSL_Integer operator /(LSL_Integer i1, int i2)
            {
                return new LSL_Integer(i1.value / i2);
            }

            static public LSL_Integer operator -(LSL_Integer i)
            {
                return new LSL_Integer(-i.value);
            }

            static public LSL_Integer operator ~(LSL_Integer i)
            {
                return new LSL_Integer(~i.value);
            }

            public override bool Equals(Object o)
            {
                if (!(o is LSL_Integer))
                    return false;
                return value == ((LSL_Integer)o).value;
            }

            public override int GetHashCode()
            {
                return value;
            }

            static public LSL_Integer operator &(LSL_Integer i1, LSL_Integer i2)
            {
                int ret = i1.value & i2.value;
                return ret;
            }

            static public LSL_Integer operator %(LSL_Integer i1, LSL_Integer i2)
            {
                int ret = i1.value % i2.value;
                return ret;
            }

            static public LSL_Integer operator |(LSL_Integer i1, LSL_Integer i2)
            {
                int ret = i1.value | i2.value;
                return ret;
            }

            static public LSL_Integer operator ^(LSL_Integer i1, LSL_Integer i2)
            {
                int ret = i1.value ^ i2.value;
                return ret;
            }

            static public LSL_Integer operator !(LSL_Integer i1)
            {
                return i1.value == 0 ? 1 : 0;
            }

            public static LSL_Integer operator ++(LSL_Integer i)
            {
                i.value++;
                return i;
            }


            public static LSL_Integer operator --(LSL_Integer i)
            {
                i.value--;
                return i;
            }

            public static LSL_Integer operator <<(LSL_Integer i, int s)
            {
                return i.value << s;
            }

            public static LSL_Integer operator >>(LSL_Integer i, int s)
            {
                return i.value >> s;
            }

            static public implicit operator System.Double(LSL_Integer i)
            {
                return (double)i.value;
            }

            public static bool operator true(LSL_Integer i)
            {
                return i.value != 0;
            }

            public static bool operator false(LSL_Integer i)
            {
                return i.value == 0;
            }

            #endregion

            #region Overriders

            public override string ToString()
            {
                return this.value.ToString();
            }

            #endregion

            public static readonly LSL_Integer Zero = new LSL_Integer();
        }

        public struct LSL_Float
        {
            public double value;

            #region Constructors

            public LSL_Float(int i)
            {
                this.value = (double)i;
            }

            public LSL_Float(double d)
            {
                this.value = d;
            }

            public LSL_Float(string s)
            {
                Regex r = new Regex("^ *(\\+|-)?([0-9]+\\.?[0-9]*|\\.[0-9]+)([eE](\\+|-)?[0-9]+)?");
                Match m = r.Match(s);
                string v = m.Groups[0].Value;

                v = v.Trim();

                if (v == String.Empty || v == null)
                    v = "0.0";
                else
                    if (!v.Contains(".") && !v.ToLower().Contains("e"))
                        v = v + ".0";
                    else
                        if (v.EndsWith("."))
                            v = v + "0";
                this.value = double.Parse(v, System.Globalization.NumberStyles.Float);
            }

            #endregion

            #region Operators

            static public explicit operator float(LSL_Float f)
            {
                return (float)f.value;
            }

            static public explicit operator int(LSL_Float f)
            {
                return (int)f.value;
            }

            static public explicit operator uint(LSL_Float f)
            {
                return (uint)Math.Abs(f.value);
            }

            static public implicit operator Boolean(LSL_Float f)
            {
                if (f.value == 0.0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            static public implicit operator LSL_Float(int i)
            {
                return new LSL_Float(i);
            }

            static public implicit operator LSL_Float(LSL_Integer i)
            {
                return new LSL_Float(i.value);
            }

            static public explicit operator LSL_Float(string s)
            {
                return new LSL_Float(s);
            }

            public static implicit operator LSL_List(LSL_Float f)
            {
                return new LSL_List(new object[] { f });
            }

            static public implicit operator LSL_Float(double d)
            {
                return new LSL_Float(d);
            }

            static public implicit operator LSL_Float(bool b)
            {
                if (b)
                    return new LSL_Float(1.0);
                else
                    return new LSL_Float(0.0);
            }

            static public bool operator ==(LSL_Float f1, LSL_Float f2)
            {
                return f1.value == f2.value;
            }

            static public bool operator !=(LSL_Float f1, LSL_Float f2)
            {
                return f1.value != f2.value;
            }

            static public LSL_Float operator ++(LSL_Float f)
            {
                f.value++;
                return f;
            }

            static public LSL_Float operator --(LSL_Float f)
            {
                f.value--;
                return f;
            }

            static public LSL_Float operator +(LSL_Float f, int i)
            {
                return new LSL_Float(f.value + (double)i);
            }

            static public LSL_Float operator -(LSL_Float f, int i)
            {
                return new LSL_Float(f.value - (double)i);
            }

            static public LSL_Float operator *(LSL_Float f, int i)
            {
                return new LSL_Float(f.value * (double)i);
            }

            static public LSL_Float operator /(LSL_Float f, int i)
            {
                return new LSL_Float(f.value / (double)i);
            }

            static public LSL_Float operator +(LSL_Float lhs, LSL_Float rhs)
            {
                return new LSL_Float(lhs.value + rhs.value);
            }

            static public LSL_Float operator -(LSL_Float lhs, LSL_Float rhs)
            {
                return new LSL_Float(lhs.value - rhs.value);
            }

            static public LSL_Float operator *(LSL_Float lhs, LSL_Float rhs)
            {
                return new LSL_Float(lhs.value * rhs.value);
            }

            static public LSL_Float operator /(LSL_Float lhs, LSL_Float rhs)
            {
                return new LSL_Float(lhs.value / rhs.value);
            }

            static public LSL_Float operator -(LSL_Float f)
            {
                return new LSL_Float(-f.value);
            }

            static public implicit operator System.Double(LSL_Float f)
            {
                return f.value;
            }

            #endregion

            #region Overriders

            public override string ToString()
            {
                return String.Format("{0:0.000000}", this.value);
            }

            public override bool Equals(Object o)
            {
                if (!(o is LSL_Float))
                    return false;
                return value == ((LSL_Float)o).value;
            }

            public override int GetHashCode()
            {
                return Convert.ToInt32(value);
            }

            #endregion

            public static readonly LSL_Float Zero = new LSL_Float();
        }

        #endregion LSL Types

        #region Constants

        public enum PrimitiveRule : int
        {
            PSYS_PART_FLAGS = 0,
            PSYS_PART_START_COLOR = 1,
            PSYS_PART_START_ALPHA = 2,
            PSYS_PART_END_COLOR = 3,
            PSYS_PART_END_ALPHA = 4,
            PSYS_PART_START_SCALE = 5,
            PSYS_PART_END_SCALE = 6,
            PSYS_PART_MAX_AGE = 7,
            PSYS_SRC_ACCEL = 8,
            PSYS_SRC_PATTERN = 9,
            PSYS_SRC_TEXTURE = 12,
            PSYS_SRC_BURST_RATE = 13,
            PSYS_SRC_BURST_PART_COUNT = 15,
            PSYS_SRC_BURST_RADIUS = 16,
            PSYS_SRC_BURST_SPEED_MIN = 17,
            PSYS_SRC_BURST_SPEED_MAX = 18,
            PSYS_SRC_MAX_AGE = 19,
            PSYS_SRC_TARGET_KEY = 20,
            PSYS_SRC_OMEGA = 21,
            PSYS_SRC_ANGLE_BEGIN = 22,
            PSYS_SRC_ANGLE_END = 23
        }

        public static readonly LSL_Integer TRUE = new LSL_Integer(1);
        public static readonly LSL_Integer FALSE = new LSL_Integer(0);

        public const int STATUS_PHYSICS = 1;
        public const int STATUS_ROTATE_X = 2;
        public const int STATUS_ROTATE_Y = 4;
        public const int STATUS_ROTATE_Z = 8;
        public const int STATUS_PHANTOM = 16;
        public const int STATUS_SANDBOX = 32;
        public const int STATUS_BLOCK_GRAB = 64;
        public const int STATUS_DIE_AT_EDGE = 128;
        public const int STATUS_RETURN_AT_EDGE = 256;
        public const int STATUS_CAST_SHADOWS = 512;

        public const int AGENT = 1;
        public const int ACTIVE = 2;
        public const int PASSIVE = 4;
        public const int SCRIPTED = 8;

        public static readonly LSL_Integer PAY_HIDE = new LSL_Integer(-1);
        public static readonly LSL_Integer PAY_DEFAULT = new LSL_Integer(-2);

        public const int CONTROL_FWD = 1;
        public const int CONTROL_BACK = 2;
        public const int CONTROL_LEFT = 4;
        public const int CONTROL_RIGHT = 8;
        public const int CONTROL_UP = 16;
        public const int CONTROL_DOWN = 32;
        public const int CONTROL_ROT_LEFT = 256;
        public const int CONTROL_ROT_RIGHT = 512;
        public const int CONTROL_LBUTTON = 268435456;
        public const int CONTROL_ML_LBUTTON = 1073741824;

        //Permissions
        public const int PERMISSION_DEBIT = 2;
        public const int PERMISSION_TAKE_CONTROLS = 4;
        public const int PERMISSION_REMAP_CONTROLS = 8;
        public const int PERMISSION_TRIGGER_ANIMATION = 16;
        public const int PERMISSION_ATTACH = 32;
        public const int PERMISSION_RELEASE_OWNERSHIP = 64;
        public const int PERMISSION_CHANGE_LINKS = 128;
        public const int PERMISSION_CHANGE_JOINTS = 256;
        public const int PERMISSION_CHANGE_PERMISSIONS = 512;
        public const int PERMISSION_TRACK_CAMERA = 1024;
        public const int PERMISSION_CONTROL_CAMERA = 2048;

        public const int AGENT_FLYING = 1;
        public const int AGENT_ATTACHMENTS = 2;
        public const int AGENT_SCRIPTED = 4;
        public const int AGENT_MOUSELOOK = 8;
        public const int AGENT_SITTING = 16;
        public const int AGENT_ON_OBJECT = 32;
        public const int AGENT_AWAY = 64;
        public const int AGENT_WALKING = 128;
        public const int AGENT_IN_AIR = 256;
        public const int AGENT_TYPING = 512;
        public const int AGENT_CROUCHING = 1024;
        public const int AGENT_BUSY = 2048;
        public const int AGENT_ALWAYS_RUN = 4096;

        //Particle Systems
        public const int PSYS_PART_INTERP_COLOR_MASK = 1;
        public const int PSYS_PART_INTERP_SCALE_MASK = 2;
        public const int PSYS_PART_BOUNCE_MASK = 4;
        public const int PSYS_PART_WIND_MASK = 8;
        public const int PSYS_PART_FOLLOW_SRC_MASK = 16;
        public const int PSYS_PART_FOLLOW_VELOCITY_MASK = 32;
        public const int PSYS_PART_TARGET_POS_MASK = 64;
        public const int PSYS_PART_TARGET_LINEAR_MASK = 128;
        public const int PSYS_PART_EMISSIVE_MASK = 256;
        public const int PSYS_PART_FLAGS = 0;
        public const int PSYS_PART_START_COLOR = 1;
        public const int PSYS_PART_START_ALPHA = 2;
        public const int PSYS_PART_END_COLOR = 3;
        public const int PSYS_PART_END_ALPHA = 4;
        public const int PSYS_PART_START_SCALE = 5;
        public const int PSYS_PART_END_SCALE = 6;
        public const int PSYS_PART_MAX_AGE = 7;
        public const int PSYS_SRC_ACCEL = 8;
        public const int PSYS_SRC_PATTERN = 9;
        public const int PSYS_SRC_INNERANGLE = 10;
        public const int PSYS_SRC_OUTERANGLE = 11;
        public const int PSYS_SRC_TEXTURE = 12;
        public const int PSYS_SRC_BURST_RATE = 13;
        public const int PSYS_SRC_BURST_PART_COUNT = 15;
        public const int PSYS_SRC_BURST_RADIUS = 16;
        public const int PSYS_SRC_BURST_SPEED_MIN = 17;
        public const int PSYS_SRC_BURST_SPEED_MAX = 18;
        public const int PSYS_SRC_MAX_AGE = 19;
        public const int PSYS_SRC_TARGET_KEY = 20;
        public const int PSYS_SRC_OMEGA = 21;
        public const int PSYS_SRC_ANGLE_BEGIN = 22;
        public const int PSYS_SRC_ANGLE_END = 23;
        public const int PSYS_SRC_PATTERN_DROP = 1;
        public const int PSYS_SRC_PATTERN_EXPLODE = 2;
        public const int PSYS_SRC_PATTERN_ANGLE = 4;
        public const int PSYS_SRC_PATTERN_ANGLE_CONE = 8;
        public const int PSYS_SRC_PATTERN_ANGLE_CONE_EMPTY = 16;

        public const int VEHICLE_TYPE_NONE = 0;
        public const int VEHICLE_TYPE_SLED = 1;
        public const int VEHICLE_TYPE_CAR = 2;
        public const int VEHICLE_TYPE_BOAT = 3;
        public const int VEHICLE_TYPE_AIRPLANE = 4;
        public const int VEHICLE_TYPE_BALLOON = 5;
        public const int VEHICLE_LINEAR_FRICTION_TIMESCALE = 16;
        public const int VEHICLE_ANGULAR_FRICTION_TIMESCALE = 17;
        public const int VEHICLE_LINEAR_MOTOR_DIRECTION = 18;
        public const int VEHICLE_LINEAR_MOTOR_OFFSET = 20;
        public const int VEHICLE_ANGULAR_MOTOR_DIRECTION = 19;
        public const int VEHICLE_HOVER_HEIGHT = 24;
        public const int VEHICLE_HOVER_EFFICIENCY = 25;
        public const int VEHICLE_HOVER_TIMESCALE = 26;
        public const int VEHICLE_BUOYANCY = 27;
        public const int VEHICLE_LINEAR_DEFLECTION_EFFICIENCY = 28;
        public const int VEHICLE_LINEAR_DEFLECTION_TIMESCALE = 29;
        public const int VEHICLE_LINEAR_MOTOR_TIMESCALE = 30;
        public const int VEHICLE_LINEAR_MOTOR_DECAY_TIMESCALE = 31;
        public const int VEHICLE_ANGULAR_DEFLECTION_EFFICIENCY = 32;
        public const int VEHICLE_ANGULAR_DEFLECTION_TIMESCALE = 33;
        public const int VEHICLE_ANGULAR_MOTOR_TIMESCALE = 34;
        public const int VEHICLE_ANGULAR_MOTOR_DECAY_TIMESCALE = 35;
        public const int VEHICLE_VERTICAL_ATTRACTION_EFFICIENCY = 36;
        public const int VEHICLE_VERTICAL_ATTRACTION_TIMESCALE = 37;
        public const int VEHICLE_BANKING_EFFICIENCY = 38;
        public const int VEHICLE_BANKING_MIX = 39;
        public const int VEHICLE_BANKING_TIMESCALE = 40;
        public const int VEHICLE_REFERENCE_FRAME = 44;
        public const int VEHICLE_FLAG_NO_DEFLECTION_UP = 1;
        public const int VEHICLE_FLAG_LIMIT_ROLL_ONLY = 2;
        public const int VEHICLE_FLAG_HOVER_WATER_ONLY = 4;
        public const int VEHICLE_FLAG_HOVER_TERRAIN_ONLY = 8;
        public const int VEHICLE_FLAG_HOVER_GLOBAL_HEIGHT = 16;
        public const int VEHICLE_FLAG_HOVER_UP_ONLY = 32;
        public const int VEHICLE_FLAG_LIMIT_MOTOR_UP = 64;
        public const int VEHICLE_FLAG_MOUSELOOK_STEER = 128;
        public const int VEHICLE_FLAG_MOUSELOOK_BANK = 256;
        public const int VEHICLE_FLAG_CAMERA_DECOUPLED = 512;

        public const int INVENTORY_ALL = -1;
        public const int INVENTORY_NONE = -1;
        public const int INVENTORY_TEXTURE = 0;
        public const int INVENTORY_SOUND = 1;
        public const int INVENTORY_LANDMARK = 3;
        public const int INVENTORY_CLOTHING = 5;
        public const int INVENTORY_OBJECT = 6;
        public const int INVENTORY_NOTECARD = 7;
        public const int INVENTORY_SCRIPT = 10;
        public const int INVENTORY_BODYPART = 13;
        public const int INVENTORY_ANIMATION = 20;
        public const int INVENTORY_GESTURE = 21;

        public const int ATTACH_CHEST = 1;
        public const int ATTACH_HEAD = 2;
        public const int ATTACH_LSHOULDER = 3;
        public const int ATTACH_RSHOULDER = 4;
        public const int ATTACH_LHAND = 5;
        public const int ATTACH_RHAND = 6;
        public const int ATTACH_LFOOT = 7;
        public const int ATTACH_RFOOT = 8;
        public const int ATTACH_BACK = 9;
        public const int ATTACH_PELVIS = 10;
        public const int ATTACH_MOUTH = 11;
        public const int ATTACH_CHIN = 12;
        public const int ATTACH_LEAR = 13;
        public const int ATTACH_REAR = 14;
        public const int ATTACH_LEYE = 15;
        public const int ATTACH_REYE = 16;
        public const int ATTACH_NOSE = 17;
        public const int ATTACH_RUARM = 18;
        public const int ATTACH_RLARM = 19;
        public const int ATTACH_LUARM = 20;
        public const int ATTACH_LLARM = 21;
        public const int ATTACH_RHIP = 22;
        public const int ATTACH_RULEG = 23;
        public const int ATTACH_RLLEG = 24;
        public const int ATTACH_LHIP = 25;
        public const int ATTACH_LULEG = 26;
        public const int ATTACH_LLLEG = 27;
        public const int ATTACH_BELLY = 28;
        public const int ATTACH_RPEC = 29;
        public const int ATTACH_LPEC = 30;
        public const int ATTACH_HUD_CENTER_2 = 31;
        public const int ATTACH_HUD_TOP_RIGHT = 32;
        public const int ATTACH_HUD_TOP_CENTER = 33;
        public const int ATTACH_HUD_TOP_LEFT = 34;
        public const int ATTACH_HUD_CENTER_1 = 35;
        public const int ATTACH_HUD_BOTTOM_LEFT = 36;
        public const int ATTACH_HUD_BOTTOM = 37;
        public const int ATTACH_HUD_BOTTOM_RIGHT = 38;

        public const int LAND_LEVEL = 0;
        public const int LAND_RAISE = 1;
        public const int LAND_LOWER = 2;
        public const int LAND_SMOOTH = 3;
        public const int LAND_NOISE = 4;
        public const int LAND_REVERT = 5;
        public const int LAND_SMALL_BRUSH = 1;
        public const int LAND_MEDIUM_BRUSH = 2;
        public const int LAND_LARGE_BRUSH = 3;

        //Agent Dataserver
        public const int DATA_ONLINE = 1;
        public const int DATA_NAME = 2;
        public const int DATA_BORN = 3;
        public const int DATA_RATING = 4;
        public const int DATA_SIM_POS = 5;
        public const int DATA_SIM_STATUS = 6;
        public const int DATA_SIM_RATING = 7;
        public const int DATA_PAYINFO = 8;
        public const int DATA_SIM_RELEASE = 128;

        public const int ANIM_ON = 1;
        public const int LOOP = 2;
        public const int REVERSE = 4;
        public const int PING_PONG = 8;
        public const int SMOOTH = 16;
        public const int ROTATE = 32;
        public const int SCALE = 64;
        public const int ALL_SIDES = -1;
        public const int LINK_SET = -1;
        public const int LINK_ROOT = 1;
        public const int LINK_ALL_OTHERS = -2;
        public const int LINK_ALL_CHILDREN = -3;
        public const int LINK_THIS = -4;
        public const int CHANGED_INVENTORY = 1;
        public const int CHANGED_COLOR = 2;
        public const int CHANGED_SHAPE = 4;
        public const int CHANGED_SCALE = 8;
        public const int CHANGED_TEXTURE = 16;
        public const int CHANGED_LINK = 32;
        public const int CHANGED_ALLOWED_DROP = 64;
        public const int CHANGED_OWNER = 128;
        public const int CHANGED_REGION_RESTART = 256;
        public const int CHANGED_REGION = 512;
        public const int CHANGED_TELEPORT = 1024;
        public const int TYPE_INVALID = 0;
        public const int TYPE_INTEGER = 1;
        public const int TYPE_FLOAT = 2;
        public const int TYPE_STRING = 3;
        public const int TYPE_KEY = 4;
        public const int TYPE_VECTOR = 5;
        public const int TYPE_ROTATION = 6;

        //XML RPC Remote Data Channel
        public const int REMOTE_DATA_CHANNEL = 1;
        public const int REMOTE_DATA_REQUEST = 2;
        public const int REMOTE_DATA_REPLY = 3;

        //llHTTPRequest
        public const int HTTP_METHOD = 0;
        public const int HTTP_MIMETYPE = 1;
        public const int HTTP_BODY_MAXLENGTH = 2;
        public const int HTTP_VERIFY_CERT = 3;

        public const int PRIM_MATERIAL = 2;
        public const int PRIM_PHYSICS = 3;
        public const int PRIM_TEMP_ON_REZ = 4;
        public const int PRIM_PHANTOM = 5;
        public const int PRIM_POSITION = 6;
        public const int PRIM_SIZE = 7;
        public const int PRIM_ROTATION = 8;
        public const int PRIM_TYPE = 9;
        public const int PRIM_TEXTURE = 17;
        public const int PRIM_COLOR = 18;
        public const int PRIM_BUMP_SHINY = 19;
        public const int PRIM_FULLBRIGHT = 20;
        public const int PRIM_FLEXIBLE = 21;
        public const int PRIM_TEXGEN = 22;
        public const int PRIM_CAST_SHADOWS = 24; // Not implemented, here for completeness sake
        public const int PRIM_POINT_LIGHT = 23; // Huh?
        public const int PRIM_GLOW = 25;
        public const int PRIM_TEXGEN_DEFAULT = 0;
        public const int PRIM_TEXGEN_PLANAR = 1;

        public const int PRIM_TYPE_BOX = 0;
        public const int PRIM_TYPE_CYLINDER = 1;
        public const int PRIM_TYPE_PRISM = 2;
        public const int PRIM_TYPE_SPHERE = 3;
        public const int PRIM_TYPE_TORUS = 4;
        public const int PRIM_TYPE_TUBE = 5;
        public const int PRIM_TYPE_RING = 6;
        public const int PRIM_TYPE_SCULPT = 7;

        public const int PRIM_HOLE_DEFAULT = 0;
        public const int PRIM_HOLE_CIRCLE = 16;
        public const int PRIM_HOLE_SQUARE = 32;
        public const int PRIM_HOLE_TRIANGLE = 48;

        public const int PRIM_MATERIAL_STONE = 0;
        public const int PRIM_MATERIAL_METAL = 1;
        public const int PRIM_MATERIAL_GLASS = 2;
        public const int PRIM_MATERIAL_WOOD = 3;
        public const int PRIM_MATERIAL_FLESH = 4;
        public const int PRIM_MATERIAL_PLASTIC = 5;
        public const int PRIM_MATERIAL_RUBBER = 6;
        public const int PRIM_MATERIAL_LIGHT = 7;

        public const int PRIM_SHINY_NONE = 0;
        public const int PRIM_SHINY_LOW = 1;
        public const int PRIM_SHINY_MEDIUM = 2;
        public const int PRIM_SHINY_HIGH = 3;
        public const int PRIM_BUMP_NONE = 0;
        public const int PRIM_BUMP_BRIGHT = 1;
        public const int PRIM_BUMP_DARK = 2;
        public const int PRIM_BUMP_WOOD = 3;
        public const int PRIM_BUMP_BARK = 4;
        public const int PRIM_BUMP_BRICKS = 5;
        public const int PRIM_BUMP_CHECKER = 6;
        public const int PRIM_BUMP_CONCRETE = 7;
        public const int PRIM_BUMP_TILE = 8;
        public const int PRIM_BUMP_STONE = 9;
        public const int PRIM_BUMP_DISKS = 10;
        public const int PRIM_BUMP_GRAVEL = 11;
        public const int PRIM_BUMP_BLOBS = 12;
        public const int PRIM_BUMP_SIDING = 13;
        public const int PRIM_BUMP_LARGETILE = 14;
        public const int PRIM_BUMP_STUCCO = 15;
        public const int PRIM_BUMP_SUCTION = 16;
        public const int PRIM_BUMP_WEAVE = 17;

        public const int PRIM_SCULPT_TYPE_SPHERE = 1;
        public const int PRIM_SCULPT_TYPE_TORUS = 2;
        public const int PRIM_SCULPT_TYPE_PLANE = 3;
        public const int PRIM_SCULPT_TYPE_CYLINDER = 4;

        public const int MASK_BASE = 0;
        public const int MASK_OWNER = 1;
        public const int MASK_GROUP = 2;
        public const int MASK_EVERYONE = 3;
        public const int MASK_NEXT = 4;

        public const int PERM_TRANSFER = 8192;
        public const int PERM_MODIFY = 16384;
        public const int PERM_COPY = 32768;
        public const int PERM_MOVE = 524288;
        public const int PERM_ALL = 2147483647;

        public const int PARCEL_MEDIA_COMMAND_STOP = 0;
        public const int PARCEL_MEDIA_COMMAND_PAUSE = 1;
        public const int PARCEL_MEDIA_COMMAND_PLAY = 2;
        public const int PARCEL_MEDIA_COMMAND_LOOP = 3;
        public const int PARCEL_MEDIA_COMMAND_TEXTURE = 4;
        public const int PARCEL_MEDIA_COMMAND_URL = 5;
        public const int PARCEL_MEDIA_COMMAND_TIME = 6;
        public const int PARCEL_MEDIA_COMMAND_AGENT = 7;
        public const int PARCEL_MEDIA_COMMAND_UNLOAD = 8;
        public const int PARCEL_MEDIA_COMMAND_AUTO_ALIGN = 9;
        public const int PARCEL_MEDIA_COMMAND_TYPE = 10;
        public const int PARCEL_MEDIA_COMMAND_SIZE = 11;
        public const int PARCEL_MEDIA_COMMAND_DESC = 12;

        public const int PARCEL_FLAG_ALLOW_FLY = 0x1;                           // parcel allows flying
        public const int PARCEL_FLAG_ALLOW_SCRIPTS = 0x2;                       // parcel allows outside scripts
        public const int PARCEL_FLAG_ALLOW_LANDMARK = 0x8;                      // parcel allows landmarks to be created
        public const int PARCEL_FLAG_ALLOW_TERRAFORM = 0x10;                    // parcel allows anyone to terraform the land
        public const int PARCEL_FLAG_ALLOW_DAMAGE = 0x20;                       // parcel allows damage
        public const int PARCEL_FLAG_ALLOW_CREATE_OBJECTS = 0x40;               // parcel allows anyone to create objects
        public const int PARCEL_FLAG_USE_ACCESS_GROUP = 0x100;                  // parcel limits access to a group
        public const int PARCEL_FLAG_USE_ACCESS_LIST = 0x200;                   // parcel limits access to a list of residents
        public const int PARCEL_FLAG_USE_BAN_LIST = 0x400;                      // parcel uses a ban list, including restricting access based on payment info
        public const int PARCEL_FLAG_USE_LAND_PASS_LIST = 0x800;                // parcel allows passes to be purchased
        public const int PARCEL_FLAG_LOCAL_SOUND_ONLY = 0x8000;                 // parcel restricts spatialized sound to the parcel
        public const int PARCEL_FLAG_RESTRICT_PUSHOBJECT = 0x200000;            // parcel restricts llPushObject
        public const int PARCEL_FLAG_ALLOW_GROUP_SCRIPTS = 0x2000000;           // parcel allows scripts owned by group
        public const int PARCEL_FLAG_ALLOW_CREATE_GROUP_OBJECTS = 0x4000000;    // parcel allows group object creation
        public const int PARCEL_FLAG_ALLOW_ALL_OBJECT_ENTRY = 0x8000000;        // parcel allows objects owned by any user to enter
        public const int PARCEL_FLAG_ALLOW_GROUP_OBJECT_ENTRY = 0x10000000;     // parcel allows with the same group to enter

        public const int REGION_FLAG_ALLOW_DAMAGE = 0x1;                        // region is entirely damage enabled
        public const int REGION_FLAG_FIXED_SUN = 0x10;                          // region has a fixed sun position
        public const int REGION_FLAG_BLOCK_TERRAFORM = 0x40;                    // region terraforming disabled
        public const int REGION_FLAG_SANDBOX = 0x100;                           // region is a sandbox
        public const int REGION_FLAG_DISABLE_COLLISIONS = 0x1000;               // region has disabled collisions
        public const int REGION_FLAG_DISABLE_PHYSICS = 0x4000;                  // region has disabled physics
        public const int REGION_FLAG_BLOCK_FLY = 0x80000;                       // region blocks flying
        public const int REGION_FLAG_ALLOW_DIRECT_TELEPORT = 0x100000;          // region allows direct teleports
        public const int REGION_FLAG_RESTRICT_PUSHOBJECT = 0x400000;            // region restricts llPushObject

        public const int STRING_TRIM_HEAD = 1;
        public const int STRING_TRIM_TAIL = 2;
        public const int STRING_TRIM = 3;
        public const int LIST_STAT_RANGE = 0;
        public const int LIST_STAT_MIN = 1;
        public const int LIST_STAT_MAX = 2;
        public const int LIST_STAT_MEAN = 3;
        public const int LIST_STAT_MEDIAN = 4;
        public const int LIST_STAT_STD_DEV = 5;
        public const int LIST_STAT_SUM = 6;
        public const int LIST_STAT_SUM_SQUARES = 7;
        public const int LIST_STAT_NUM_COUNT = 8;
        public const int LIST_STAT_GEOMETRIC_MEAN = 9;
        public const int LIST_STAT_HARMONIC_MEAN = 100;

        //ParcelPrim Categories
        public const int PARCEL_COUNT_TOTAL = 0;
        public const int PARCEL_COUNT_OWNER = 1;
        public const int PARCEL_COUNT_GROUP = 2;
        public const int PARCEL_COUNT_OTHER = 3;
        public const int PARCEL_COUNT_SELECTED = 4;
        public const int PARCEL_COUNT_TEMP = 5;

        public const int DEBUG_CHANNEL = 0x7FFFFFFF;
        public const int PUBLIC_CHANNEL = 0x00000000;

        public const int OBJECT_NAME = 1;
        public const int OBJECT_DESC = 2;
        public const int OBJECT_POS = 3;
        public const int OBJECT_ROT = 4;
        public const int OBJECT_VELOCITY = 5;
        public const int OBJECT_OWNER = 6;
        public const int OBJECT_GROUP = 7;
        public const int OBJECT_CREATOR = 8;

        // constants for llSetCameraParams
        public const int CAMERA_PITCH = 0;
        public const int CAMERA_FOCUS_OFFSET = 1;
        public const int CAMERA_FOCUS_OFFSET_X = 2;
        public const int CAMERA_FOCUS_OFFSET_Y = 3;
        public const int CAMERA_FOCUS_OFFSET_Z = 4;
        public const int CAMERA_POSITION_LAG = 5;
        public const int CAMERA_FOCUS_LAG = 6;
        public const int CAMERA_DISTANCE = 7;
        public const int CAMERA_BEHINDNESS_ANGLE = 8;
        public const int CAMERA_BEHINDNESS_LAG = 9;
        public const int CAMERA_POSITION_THRESHOLD = 10;
        public const int CAMERA_FOCUS_THRESHOLD = 11;
        public const int CAMERA_ACTIVE = 12;
        public const int CAMERA_POSITION = 13;
        public const int CAMERA_POSITION_X = 14;
        public const int CAMERA_POSITION_Y = 15;
        public const int CAMERA_POSITION_Z = 16;
        public const int CAMERA_FOCUS = 17;
        public const int CAMERA_FOCUS_X = 18;
        public const int CAMERA_FOCUS_Y = 19;
        public const int CAMERA_FOCUS_Z = 20;
        public const int CAMERA_POSITION_LOCKED = 21;
        public const int CAMERA_FOCUS_LOCKED = 22;

        // constants for llGetParcelDetails
        public const int PARCEL_DETAILS_NAME = 0;
        public const int PARCEL_DETAILS_DESC = 1;
        public const int PARCEL_DETAILS_OWNER = 2;
        public const int PARCEL_DETAILS_GROUP = 3;
        public const int PARCEL_DETAILS_AREA = 4;

        // constants for llSetClickAction
        public const int CLICK_ACTION_NONE = 0;
        public const int CLICK_ACTION_TOUCH = 0;
        public const int CLICK_ACTION_SIT = 1;
        public const int CLICK_ACTION_BUY = 2;
        public const int CLICK_ACTION_PAY = 3;
        public const int CLICK_ACTION_OPEN = 4;
        public const int CLICK_ACTION_PLAY = 5;
        public const int CLICK_ACTION_OPEN_MEDIA = 6;

        // constants for the llDetectedTouch* functions
        public const int TOUCH_INVALID_FACE = -1;
        public static readonly LSL_Vector TOUCH_INVALID_TEXCOORD = new LSL_Vector(-1.0, -1.0, 0.0);

        #endregion Constants
    }
}
