using System;
using System.Text;
using System.Globalization;

namespace Nii.JSON
{
    /// <summary>
    /// <para>
    ///  A JSONTokener takes a source string and extracts characters and tokens from
    ///  it. It is used by the JSONObject and JSONArray constructors to parse
    ///  JSON source strings.
    ///  </para>
    ///  <para>
    ///  Public Domain 2002 JSON.org
    ///  @author JSON.org
    ///  @version 0.1
    ///  </para>
    ///  <para>Ported to C# by Are Bjolseth, teleplan.no</para>
    ///  <para>
    ///  <list type="bullet">
    ///  <item><description>Implement Custom exceptions</description></item>
    ///  <item><description>Add unit testing</description></item>
    ///  <item><description>Add log4net</description></item>
    ///  </list>
    ///  </para>
    /// </summary>
    public class JSONTokener
    {
        /// <summary>The index of the next character.</summary>
        private int myIndex;
        /// <summary>The source string being tokenized.</summary>
        private string mySource;

        /// <summary>
        /// Construct a JSONTokener from a string.
        /// </summary>
        /// <param name="s">A source string.</param>
        public JSONTokener(string s)
        {
            myIndex = 0;
            mySource = s;
        }

        /// <summary>
        /// Back up one character. This provides a sort of lookahead capability,
        /// so that you can test for a digit or letter before attempting to parse
        /// the next number or identifier.
        /// </summary>
        public void back()
        {
            if (myIndex > 0)
                myIndex -= 1;
        }

        /// <summary>
        /// Get the hex value of a character (base16).
        /// </summary>
        /// <param name="c">
        /// A character between '0' and '9' or between 'A' and 'F' or
        /// between 'a' and 'f'.
        /// </param>
        /// <returns>An int between 0 and 15, or -1 if c was not a hex digit.</returns>
        public static int dehexchar(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }
            if (c >= 'A' && c <= 'F')
            {
                return c + 10 - 'A';
            }
            if (c >= 'a' && c <= 'f')
            {
                return c + 10 - 'a';
            }
            return -1;
        }

        /// <summary>
        /// Determine if the source string still contains characters that next() can consume.
        /// </summary>
        /// <returns>true if not yet at the end of the source.</returns>
        public bool more()
        {
            return myIndex < mySource.Length;
        }

        /// <summary>
        /// Get the next character in the source string.
        /// </summary>
        /// <returns>The next character, or 0 if past the end of the source string.</returns>
        public char next()
        {
            char c = more() ? mySource[myIndex] : (char)0;
            myIndex +=1;
            return c;
        }

        /// <summary>
        /// Get the next n characters.
        /// </summary>
        /// <param name="n">The number of characters to take.</param>
        /// <returns>A string of n characters.</returns>
        public string next(int n)
        {
            int i = myIndex;
            int j = i + n;
            if (j >= mySource.Length)
            {
                string msg = "Substring bounds error";
                throw (new Exception(msg));
            }
            myIndex += n;
            return mySource.Substring(i,j);
        }

        /// <summary>
        /// Get the next char in the string, skipping whitespace
        /// and comments (slashslash and slashstar).
        /// </summary>
        /// <returns>A character, or 0 if there are no more characters.</returns>
        public char nextClean()
        {
            while (true)
            {
                char c = next();
                if (c == '/')
                {
                    switch (next())
                    {
                        case '/':
                            do
                            {
                                c = next();
                            } while (c != '\n' && c != '\r' && c != 0);
                            break;
                        case '*':
                            while (true)
                            {
                                c = next();
                                if (c == 0)
                                {
                                    throw (new Exception("Unclosed comment."));
                                }
                                if (c == '*')
                                {
                                    if (next() == '/')
                                    {
                                        break;
                                    }
                                    back();
                                }
                            }
                            break;
                        default:
                            back();
                            return '/';
                    }
                }
                else if (c == 0 || c > ' ')
                {
                    return c;
                }
            }
        }

        /// <summary>
        /// Return the characters up to the next close quote character.
        /// Backslash processing is done. The formal JSON format does not
        /// allow strings in single quotes, but an implementation is allowed to
        /// accept them.
        /// </summary>
        /// <param name="quote">The quoting character, either " or '</param>
        /// <returns>A String.</returns>
        public string nextString(char quote)
        {
            char c;
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                c = next();
                if ((c == 0x00) || (c == 0x0A) || (c == 0x0D))
                {
                    throw (new Exception("Unterminated string"));
                }
                // CTRL chars
                if (c == '\\')
                {
                    c = next();
                    switch (c)
                    {
                        case 'b': //Backspace
                            sb.Append('\b');
                            break;
                        case 't': //Horizontal tab
                            sb.Append('\t');
                            break;
                        case 'n':  //newline
                            sb.Append('\n');
                            break;
                        case 'f':  //Form feed
                            sb.Append('\f');
                            break;
                        case 'r':  // Carriage return
                            sb.Append('\r');
                            break;
                        case 'u':
                            //sb.append((char)Integer.parseInt(next(4), 16)); // 16 == radix, ie. hex
                            int iascii = int.Parse(next(4),System.Globalization.NumberStyles.HexNumber);
                            sb.Append((char)iascii);
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
                else
                {
                    if (c == quote)
                    {
                        return sb.ToString();
                    }
                    sb.Append(c);
                }
            }//END-while
        }

        /// <summary>
        /// Get the next value as object. The value can be a Boolean, Double, Integer,
        /// JSONArray, JSONObject, or String, or the JSONObject.NULL object.
        /// </summary>
        /// <returns>An object.</returns>
        public object nextObject()
        {
            char c = nextClean();
            string s;

            if (c == '"' || c == '\'')
            {
                return nextString(c);
            }
            // Object
            if (c == '{')
            {
                back();
                return new JSONObject(this);
            }

            // JSON Array
            if (c == '[')
            {
                back();
                return new JSONArray(this);
            }

            StringBuilder sb = new StringBuilder();

            char b = c;
            while (c >= ' ' && c != ':' && c != ',' && c != ']' && c != '}' && c != '/')
            {
                sb.Append(c);
                c = next();
            }
            back();

            s = sb.ToString().Trim();
            if (s == "true")
                return bool.Parse("true");
            if (s == "false")
                return bool.Parse("false");
            if (s == "null")
                return JSONObject.NULL;

            if ((b >= '0' && b <= '9') || b == '.' || b == '-' || b == '+')
            {
                int intResult;
                if (Int32.TryParse(s, out intResult))
                    return intResult;
                double doubleResult;
                if (Double.TryParse(s, out doubleResult))
                    return doubleResult;
            }
            if (s == "")
            {
                throw (new Exception("Missing value"));
            }
            return s;
        }

        /// <summary>
        /// Unescape the source text. Convert %hh sequences to single characters,
        /// and convert plus to space. There are Web transport systems that insist on
        /// doing unnecessary URL encoding. This provides a way to undo it.
        /// </summary>
        public void unescape()
        {
            mySource = unescape(mySource);
        }

        /// <summary>
        /// Convert %hh sequences to single characters, and convert plus to space.
        /// </summary>
        /// <param name="s">A string that may contain plus and %hh sequences.</param>
        /// <returns>The unescaped string.</returns>
        public static string unescape(string s)
        {
            int len = s.Length;
            StringBuilder sb = new StringBuilder();
            for (int i=0; i < len; i++)
            {
                char c = s[i];
                if (c == '+')
                {
                    c = ' ';
                }
                else if (c == '%' && (i + 2 < len))
                {
                    int d = dehexchar(s[i+1]);
                    int e = dehexchar(s[i+2]);
                    if (d >= 0 && e >= 0)
                    {
                        c = (char)(d*16 + e);
                        i += 2;
                    }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
