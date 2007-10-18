using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife.LLSD
{
    public static partial class LLSDParser
    {
        public static object DeserializeNotation(string notationData)
        {
            int unused;
            return ParseNotationElement(notationData, out unused);
        }

        private static object ParseNotationElement(string notationData, out int endPos)
        {
            if (notationData.Length == 0)
            {
                endPos = 0;
                return null;
            }

            // Identify what type of object this is
            switch (notationData[0])
            {
                case '!':
                    endPos = 1;
                    return null;
                case '1':
                    endPos = 1;
                    return true;
                case '0':
                    endPos = 1;
                    return false;
                case 'i':
                {
                    if (notationData.Length < 2)
                    {
                        endPos = notationData.Length;
                        return 0;
                    }

                    int value;
                    endPos = FindEnd(notationData, 1);

                    if (Int32.TryParse(notationData.Substring(1, endPos - 1), out value))
                        return value;
                    else
                        return 0;
                }
                case 'r':
                {
                    if (notationData.Length < 2)
                    {
                        endPos = notationData.Length;
                        return 0d;
                    }

                    double value;
                    endPos = FindEnd(notationData, 1);

                    if (Double.TryParse(notationData.Substring(1, endPos - 1), System.Globalization.NumberStyles.Float,
                        Helpers.EnUsCulture.NumberFormat, out value))
                        return value;
                    else
                        return 0d;
                }
                case 'u':
                {
                    if (notationData.Length < 17)
                    {
                        endPos = notationData.Length;
                        return LLUUID.Zero;
                    }

                    LLUUID value;
                    endPos = FindEnd(notationData, 1);

                    if (LLUUID.TryParse(notationData.Substring(1, endPos - 1), out value))
                        return value;
                    else
                        return LLUUID.Zero;
                }
                case 'b':
                    throw new NotImplementedException("Notation binary type is unimplemented");
                case 's':
                case '"':
                case '\'':
                    if (notationData.Length < 2)
                    {
                        endPos = notationData.Length;
                        return String.Empty;
                    }

                    endPos = FindEnd(notationData, 1);
                    return notationData.Substring(1, endPos - 1).Trim(new char[] { '"', '\'' });
                case 'l':
                    throw new NotImplementedException("Notation URI type is unimplemented");
                case 'd':
                    throw new NotImplementedException("Notation date type is unimplemented");
                case '[':
                {
                    if (notationData.IndexOf(']') == -1)
                        throw new LLSDException("Invalid notation array");

                    int pos = 0;
                    List<object> array = new List<object>();

                    while (notationData[pos] != ']')
                    {
                        ++pos;

                        // Advance past comma if need be
                        if (notationData[pos] == ',') ++pos;

                        // Allow a single whitespace character
                        if (pos < notationData.Length && notationData[pos] == ' ') ++pos;

                        int end;
                        array.Add(ParseNotationElement(notationData.Substring(pos), out end));
                        pos += end;
                    }

                    endPos = pos + 1;
                    return array;
                }
                case '{':
                {
                    if (notationData.IndexOf('}') == -1)
                        throw new LLSDException("Invalid notation map");

                    int pos = 0;
                    Dictionary<string, object> hashtable = new Dictionary<string, object>();

                    while (notationData[pos] != '}')
                    {
                        ++pos;

                        // Advance past comma if need be
                        if (notationData[pos] == ',') ++pos;

                        // Allow a single whitespace character
                        if (pos < notationData.Length && notationData[pos] == ' ') ++pos;

                        if (notationData[pos] != '\'')
                            throw new LLSDException("Expected a map key");

                        int endquote = notationData.IndexOf('\'', pos + 1);
                        if (endquote == -1 || (endquote + 1) >= notationData.Length || notationData[endquote + 1] != ':')
                            throw new LLSDException("Invalid map format");

                        string key = notationData.Substring(pos, endquote - pos);
                        key = key.Trim(new char[] { '"', '\'' }); //key.Replace("'", String.Empty);
                        pos += (endquote - pos) + 2;

                        int end;
                        hashtable[key] = ParseNotationElement(notationData.Substring(pos), out end);
                        pos += end;
                    }

                    endPos = pos + 1;
                    return hashtable;
                }
                default:
                    throw new LLSDException("Unknown notation value type");
            }
        }

        private static int FindEnd(string llsd, int start)
        {
            int end = llsd.IndexOfAny(new char[] { ',', ']', '}' });
            if (end == -1) end = llsd.Length - 1;
            return end;
        }

        #region Deprecated
        //private static object DeserializeNotation(byte[] notationData, int startPos, out int endPos)
        //{
        //    if (startPos >= notationData.Length)
        //    {
        //        endPos = notationData.Length;
        //        return null;
        //    }

        //    switch ((char)notationData[startPos])
        //    {
        //        case '!':
        //            endPos = 1;
        //            return null;
        //        case '1':
        //            endPos = 1;
        //            return true;
        //        case '0':
        //            endPos = 1;
        //            return false;
        //        case 'i':
        //        {
        //            if (notationData.Length - startPos < 2)
        //            {
        //                endPos = notationData.Length;
        //                return 0;
        //            }

        //            int value;
        //            endPos = FindEnd(notationData, startPos + 1);

        //            if (Int32.TryParse(UTF8Encoding.UTF8.GetString(notationData, startPos + 1, endPos - (startPos + 1)), out value))
        //                return value;
        //            else
        //                return 0;
        //        }
        //        case 'r':
        //        {
        //            if (notationData.Length - startPos < 2)
        //            {
        //                endPos = notationData.Length;
        //                return 0d;
        //            }

        //            double value;
        //            endPos = FindEnd(notationData, startPos + 1);

        //            if (Double.TryParse(UTF8Encoding.UTF8.GetString(notationData, startPos + 1, endPos - (startPos + 1)),
        //                System.Globalization.NumberStyles.Float, Helpers.EnUsCulture.NumberFormat, out value))
        //                return value;
        //            else
        //                return 0d;
        //        }
        //        case 'u':
        //        {
        //            if (notationData.Length - startPos < 17)
        //            {
        //                endPos = notationData.Length;
        //                return LLUUID.Zero;
        //            }

        //            LLUUID value;
        //            endPos = FindEnd(notationData, startPos + 1);

        //            if (LLUUID.TryParse(UTF8Encoding.UTF8.GetString(notationData, startPos + 1, endPos - (startPos + 1)), out value))
        //                return value;
        //            else
        //                return LLUUID.Zero;
        //        }
        //        case 'b':
        //        {
        //            if (notationData.Length - startPos < 2)
        //            {
        //                endPos = notationData.Length;
        //                return new byte[0];
        //            }

        //            byte[] value;
        //            endPos = FindEnd(notationData, startPos + 1);

        //            if ((char)notationData[startPos + 1] == '(')
        //            {
        //                // Format: b(8)........

        //                int endParens = Array.FindIndex<byte>(notationData, startPos + 1,
        //                    delegate(byte b) { return (char)b == ')'; });

        //                if (endParens == -1)
        //                    return new byte[0];

        //                int len;
        //                if (!Int32.TryParse(UTF8Encoding.UTF8.GetString(notationData, startPos + 2, endParens - (startPos + 2)), out len))
        //                    return new byte[0];

        //                // Make sure the size of the binary data doesn't run past the end of the total amount of data we have
        //                if (endParens + 2 + len >= notationData.Length)
        //                    return new byte[0];

        //                if ((char)notationData[endParens + 1] == '"')
        //                {
        //                    // Quoted binary data (I know, this makes no sense)
        //                    value = new byte[len - 2];
        //                    Buffer.BlockCopy(notationData, endParens + 2, value, 0, len - 2);
        //                }
        //                else
        //                {
        //                    // Unquoted binary data (technically breaks the protocol spec but we parse it anyways)
        //                    value = new byte[len];
        //                    Buffer.BlockCopy(notationData, endParens + 1, value, 0, len);
        //                }

        //                return value;
        //            }
        //            else
        //            {
        //                // Format: b64...........==

        //                if (notationData.Length - startPos < 4)
        //                    return new byte[0];

        //                int baseVal;
        //                if (!Int32.TryParse(UTF8Encoding.UTF8.GetString(notationData, startPos + 1, 2), out baseVal))
        //                    return new byte[0];

        //                string str = UTF8Encoding.UTF8.GetString(notationData, startPos + 3, endPos - (startPos + 3));

        //                switch (baseVal)
        //                {
        //                    case 64:
        //                        return Convert.FromBase64String(str);
        //                    default:
        //                        throw new LLSDException("Unsupported encoding: base" + baseVal);
        //                }
        //            }
        //        }
        //        case '\'':
        //            // Find the 
        //            break;
        //        case '"':
        //            break;
        //        case 's':
        //            break;
        //        case 'l':
        //            break;
        //        case 'd':
        //            break;
        //        case '[':
        //            break;
        //        case '{':
        //            break;
        //        default:
        //            throw new LLSDException("Unhandled notation type: " + (char)notationData[startPos]);
        //    }
        //}

        //private static int FindEnd(byte[] llsd, int start)
        //{
        //    int end = Array.FindIndex<byte>(llsd, start,
        //        delegate(byte b)
        //        {
        //            char c = (char)b;
        //            return (c == ',' || c == ']' || c == '}');
        //        }
        //    );

        //    if (end == -1) end = llsd.Length - 1;
        //    return end;
        //}
        #endregion Deprecated
    }
}
