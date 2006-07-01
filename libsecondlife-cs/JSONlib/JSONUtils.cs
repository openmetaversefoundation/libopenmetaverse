using System;
using System.Text;

namespace Nii.JSON
{  
  /// <summary>
  ///  Public Domain 2002 JSON.org
  ///  @author JSON.org
  ///  @version 0.1
  ///  Ported to C# by Are Bjolseth, teleplan.no
  /// </summary>
	public sealed class JSONUtils
	{
		/// <summary>
		/// Produce a string in double quotes with backslash sequences in all the right places.
		/// </summary>
		/// <param name="s">A String</param>
		/// <returns>A String correctly formatted for insertion in a JSON message.</returns>
		public static string Enquote(string s) 
		{
			if (s == null || s.Length == 0) 
			{
				return "\"\"";
			}
			char         c;
			int          i;
			int          len = s.Length;
			StringBuilder sb = new StringBuilder(len + 4);
			string       t;

			sb.Append('"');
			for (i = 0; i < len; i += 1) 
			{
				c = s[i];
				if ((c == '\\') || (c == '"') || (c == '>'))
				{
					sb.Append('\\');
					sb.Append(c);
				}
				else if (c == '\b')
					sb.Append("\\b");
				else if (c == '\t')
					sb.Append("\\t");
				else if (c == '\n')
					sb.Append("\\n");
				else if (c == '\f')
					sb.Append("\\f");
				else if (c == '\r')
					sb.Append("\\r");
				else
				{
					if (c < ' ') 
					{
						//t = "000" + Integer.toHexString(c);
						string tmp = new string(c,1);
						t = "000" + int.Parse(tmp,System.Globalization.NumberStyles.HexNumber);
						sb.Append("\\u" + t.Substring(t.Length - 4));
					} 
					else 
					{
						sb.Append(c);
					}
				}
			}
			sb.Append('"');
			return sb.ToString();
		}
	}
}
