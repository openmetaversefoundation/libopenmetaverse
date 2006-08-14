using System;

namespace libsecondlife
{
	/// <summary>
	/// Summary description for Utils.
	/// </summary>
	public class Utils
	{
		private Utils()
		{
		}

		public static string ByteArrayToString( byte[] byteArray )
		{
			if ( byteArray == null )
			{
				return "";
			}

			string output = "";

			bool printable = true;

			for (int i = 0; i < byteArray.Length; ++i)
			{
				// Check if there are any unprintable characters in the array
				if ((byteArray[i] < 0x20 || byteArray[i] > 0x7E) && byteArray[i] != 0x09
					&& byteArray[i] != 0x0D)
				{
					printable = false;
				}
			}

			if (printable)
			{
				output = Helpers.FieldToString(byteArray);
			}
			else
			{
				for (int i = 0; i < byteArray.Length; i += 16)
				{
					for (int j = 0; j < 16; j++)
					{
						if ((i + j) < byteArray.Length)
						{
							string s = String.Format("{0:X} ", byteArray[i + j]); 
							if( s.Length == 2 )
							{
								s = "0" + s;
							}

							output += s;
						}
						else
						{
							output += "   ";
						}
					}

					for (int j = 0; j < 16 && (i + j) < byteArray.Length; j++)
					{
						if (byteArray[i + j] >= 0x20 && byteArray[i + j] < 0x7E)
						{
							output += (char)byteArray[i + j];
						}
						else
						{
							output += ".";
						}
					}

					output += "\n";
				}
			}

			return output;
		}
	}
}
