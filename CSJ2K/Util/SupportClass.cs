//
// In order to convert some functionality to Visual C#, the Java Language Conversion Assistant
// creates "support classes" that duplicate the original functionality.  
//
// Support classes replicate the functionality of the original code, but in some cases they are 
// substantially different architecturally. Although every effort is made to preserve the 
// original architecture of the application in the converted project, the user should be aware that 
// the primary goal of these support classes is to replicate functionality, and that at times 
// the architecture of the resulting solution may differ somewhat.
//

using System;

	/// <summary>
	/// This interface should be implemented by any class whose instances are intended 
	/// to be executed by a thread.
	/// </summary>
	public interface IThreadRunnable
	{
		/// <summary>
		/// This method has to be implemented in order that starting of the thread causes the object's 
		/// run method to be called in that separately executing thread.
		/// </summary>
		void Run();
	}

/// <summary>
/// Contains conversion support elements such as classes, interfaces and static methods.
/// </summary>
internal class SupportClass
{
	/// <summary>
	/// Converts an array of sbytes to an array of bytes
	/// </summary>
	/// <param name="sbyteArray">The array of sbytes to be converted</param>
	/// <returns>The new array of bytes</returns>
	public static byte[] ToByteArray(sbyte[] sbyteArray)
	{
		byte[] byteArray = null;

		if (sbyteArray != null)
		{
			byteArray = new byte[sbyteArray.Length];
			for(int index=0; index < sbyteArray.Length; index++)
				byteArray[index] = (byte) sbyteArray[index];
		}
		return byteArray;
	}

	/// <summary>
	/// Converts a string to an array of bytes
	/// </summary>
	/// <param name="sourceString">The string to be converted</param>
	/// <returns>The new array of bytes</returns>
	public static byte[] ToByteArray(System.String sourceString)
	{
		return System.Text.UTF8Encoding.UTF8.GetBytes(sourceString);
	}

	/// <summary>
	/// Converts a array of object-type instances to a byte-type array.
	/// </summary>
	/// <param name="tempObjectArray">Array to convert.</param>
	/// <returns>An array of byte type elements.</returns>
	public static byte[] ToByteArray(System.Object[] tempObjectArray)
	{
		byte[] byteArray = null;
		if (tempObjectArray != null)
		{
			byteArray = new byte[tempObjectArray.Length];
			for (int index = 0; index < tempObjectArray.Length; index++)
				byteArray[index] = (byte)tempObjectArray[index];
		}
		return byteArray;
	}

	/*******************************/
	/// <summary>
	/// Writes the exception stack trace to the received stream
	/// </summary>
	/// <param name="throwable">Exception to obtain information from</param>
	/// <param name="stream">Output sream used to write to</param>
	public static void WriteStackTrace(System.Exception throwable, System.IO.TextWriter stream)
	{
		stream.Write(throwable.StackTrace);
		stream.Flush();
	}

	/*******************************/
	/// <summary>
	/// Receives a byte array and returns it transformed in an sbyte array
	/// </summary>
	/// <param name="byteArray">Byte array to process</param>
	/// <returns>The transformed array</returns>
	public static sbyte[] ToSByteArray(byte[] byteArray)
	{
		sbyte[] sbyteArray = null;
		if (byteArray != null)
		{
			sbyteArray = new sbyte[byteArray.Length];
			for(int index=0; index < byteArray.Length; index++)
				sbyteArray[index] = (sbyte) byteArray[index];
		}
		return sbyteArray;
	}

	/*******************************/
	/// <summary>
	/// Converts an array of sbytes to an array of chars
	/// </summary>
	/// <param name="sByteArray">The array of sbytes to convert</param>
	/// <returns>The new array of chars</returns>
	public static char[] ToCharArray(sbyte[] sByteArray) 
	{
		return System.Text.UTF8Encoding.UTF8.GetChars(ToByteArray(sByteArray));
	}

	/// <summary>
	/// Converts an array of bytes to an array of chars
	/// </summary>
	/// <param name="byteArray">The array of bytes to convert</param>
	/// <returns>The new array of chars</returns>
	public static char[] ToCharArray(byte[] byteArray) 
	{
		return System.Text.UTF8Encoding.UTF8.GetChars(byteArray);
	}

	/*******************************/
	/// <summary>
	/// This method returns the literal value received
	/// </summary>
	/// <param name="literal">The literal to return</param>
	/// <returns>The received value</returns>
	public static long Identity(long literal)
	{
		return literal;
	}

	/// <summary>
	/// This method returns the literal value received
	/// </summary>
	/// <param name="literal">The literal to return</param>
	/// <returns>The received value</returns>
	public static ulong Identity(ulong literal)
	{
		return literal;
	}

	/// <summary>
	/// This method returns the literal value received
	/// </summary>
	/// <param name="literal">The literal to return</param>
	/// <returns>The received value</returns>
	public static float Identity(float literal)
	{
		return literal;
	}

	/// <summary>
	/// This method returns the literal value received
	/// </summary>
	/// <param name="literal">The literal to return</param>
	/// <returns>The received value</returns>
	public static double Identity(double literal)
	{
		return literal;
	}

	/*******************************/
	/// <summary>
	/// Provides support functions to create read-write random acces files and write functions
	/// </summary>
	public class RandomAccessFileSupport
	{
		/// <summary>
		/// Creates a new random acces stream with read-write or read rights
		/// </summary>
		/// <param name="fileName">A relative or absolute path for the file to open</param>
		/// <param name="mode">Mode to open the file in</param>
		/// <returns>The new System.IO.FileStream</returns>
		public static System.IO.FileStream CreateRandomAccessFile(System.String fileName, System.String mode) 
		{
			System.IO.FileStream newFile = null;

            if (mode.CompareTo("rw") == 0)
            //    newFile = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
            //else if (mode.CompareTo("rw+") == 0)
                newFile = new System.IO.FileStream(fileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
            else if (mode.CompareTo("r") == 0)
                newFile = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            else
                throw new System.ArgumentException();

			return newFile;
		}

		/// <summary>
		/// Creates a new random acces stream with read-write or read rights
		/// </summary>
		/// <param name="fileName">File infomation for the file to open</param>
		/// <param name="mode">Mode to open the file in</param>
		/// <returns>The new System.IO.FileStream</returns>
		public static System.IO.FileStream CreateRandomAccessFile(System.IO.FileInfo fileName, System.String mode)
		{
			return CreateRandomAccessFile(fileName.FullName, mode);
		}

		/// <summary>
		/// Writes the data to the specified file stream
		/// </summary>
		/// <param name="data">Data to write</param>
		/// <param name="fileStream">File to write to</param>
		public static void WriteBytes(System.String data,System.IO.FileStream fileStream)
		{
			int index = 0;
			int length = data.Length;

			while(index < length)
				fileStream.WriteByte((byte)data[index++]);	
		}

		/// <summary>
		/// Writes the received string to the file stream
		/// </summary>
		/// <param name="data">String of information to write</param>
		/// <param name="fileStream">File to write to</param>
		public static void WriteChars(System.String data,System.IO.FileStream fileStream)
		{
			WriteBytes(data, fileStream);	
		}

		/// <summary>
		/// Writes the received data to the file stream
		/// </summary>
		/// <param name="sByteArray">Data to write</param>
		/// <param name="fileStream">File to write to</param>
		public static void WriteRandomFile(sbyte[] sByteArray,System.IO.FileStream fileStream)
		{
			byte[] byteArray = ToByteArray(sByteArray);
			fileStream.Write(byteArray, 0, byteArray.Length);
		}
	}

	/*******************************/
	/// <summary>
	/// Performs an unsigned bitwise right shift with the specified number
	/// </summary>
	/// <param name="number">Number to operate on</param>
	/// <param name="bits">Ammount of bits to shift</param>
	/// <returns>The resulting number from the shift operation</returns>
	public static int URShift(int number, int bits)
	{
        if ( number >= 0)
			return number >> bits;
		else
			return (number >> bits) + (2 << ~bits);
	}

	/// <summary>
	/// Performs an unsigned bitwise right shift with the specified number
	/// </summary>
	/// <param name="number">Number to operate on</param>
	/// <param name="bits">Ammount of bits to shift</param>
	/// <returns>The resulting number from the shift operation</returns>
	public static int URShift(int number, long bits)
	{
		return URShift(number, (int)bits);
	}

	/// <summary>
	/// Performs an unsigned bitwise right shift with the specified number
	/// </summary>
	/// <param name="number">Number to operate on</param>
	/// <param name="bits">Ammount of bits to shift</param>
	/// <returns>The resulting number from the shift operation</returns>
	public static long URShift(long number, int bits)
	{
		if (number >= 0)
			return number >> bits;
		else
			return (number >> bits) + (2L << ~bits);
	}

	/// <summary>
	/// Performs an unsigned bitwise right shift with the specified number
	/// </summary>
	/// <param name="number">Number to operate on</param>
	/// <param name="bits">Ammount of bits to shift</param>
	/// <returns>The resulting number from the shift operation</returns>
	public static long URShift(long number, long bits)
	{
		return URShift(number, (int)bits);
	}

	/*******************************/
	/// <summary>Reads a number of characters from the current source Stream and writes the data to the target array at the specified index.</summary>
	/// <param name="sourceStream">The source Stream to read from.</param>
	/// <param name="target">Contains the array of characteres read from the source Stream.</param>
	/// <param name="start">The starting index of the target array.</param>
	/// <param name="count">The maximum number of characters to read from the source Stream.</param>
	/// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source Stream. Returns -1 if the end of the stream is reached.</returns>
	public static System.Int32 ReadInput(System.IO.Stream sourceStream, sbyte[] target, int start, int count)
	{
		// Returns 0 bytes if not enough space in target
		if (target.Length == 0)
			return 0;

		byte[] receiver = new byte[target.Length];
		int bytesRead   = sourceStream.Read(receiver, start, count);

		// Returns -1 if EOF
		if (bytesRead == 0)	
			return -1;
                
		for(int i = start; i < start + bytesRead; i++)
			target[i] = (sbyte)receiver[i];
                
		return bytesRead;
	}

	/// <summary>Reads a number of characters from the current source TextReader and writes the data to the target array at the specified index.</summary>
	/// <param name="sourceTextReader">The source TextReader to read from</param>
	/// <param name="target">Contains the array of characteres read from the source TextReader.</param>
	/// <param name="start">The starting index of the target array.</param>
	/// <param name="count">The maximum number of characters to read from the source TextReader.</param>
	/// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source TextReader. Returns -1 if the end of the stream is reached.</returns>
	public static System.Int32 ReadInput(System.IO.TextReader sourceTextReader, sbyte[] target, int start, int count)
	{
		// Returns 0 bytes if not enough space in target
		if (target.Length == 0) return 0;

		char[] charArray = new char[target.Length];
		int bytesRead = sourceTextReader.Read(charArray, start, count);

		// Returns -1 if EOF
		if (bytesRead == 0) return -1;

		for(int index=start; index<start+bytesRead; index++)
			target[index] = (sbyte)charArray[index];

		return bytesRead;
	}

	/*******************************/
	/// <summary>
	/// The class performs token processing in strings
	/// </summary>
	public class Tokenizer: System.Collections.IEnumerator
	{
		/// Position over the string
		private long currentPos = 0;

		/// Include demiliters in the results.
		private bool includeDelims = false;

		/// Char representation of the String to tokenize.
		private char[] chars = null;
			
		//The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character and the form-feed character
		private string delimiters = " \t\n\r\f";		

		/// <summary>
		/// Initializes a new class instance with a specified string to process
		/// </summary>
		/// <param name="source">String to tokenize</param>
		public Tokenizer(System.String source)
		{			
			this.chars = source.ToCharArray();
		}

		/// <summary>
		/// Initializes a new class instance with a specified string to process
		/// and the specified token delimiters to use
		/// </summary>
		/// <param name="source">String to tokenize</param>
		/// <param name="delimiters">String containing the delimiters</param>
		public Tokenizer(System.String source, System.String delimiters):this(source)
		{			
			this.delimiters = delimiters;
		}


		/// <summary>
		/// Initializes a new class instance with a specified string to process, the specified token 
		/// delimiters to use, and whether the delimiters must be included in the results.
		/// </summary>
		/// <param name="source">String to tokenize</param>
		/// <param name="delimiters">String containing the delimiters</param>
		/// <param name="includeDelims">Determines if delimiters are included in the results.</param>
		public Tokenizer(System.String source, System.String delimiters, bool includeDelims):this(source,delimiters)
		{
			this.includeDelims = includeDelims;
		}	


		/// <summary>
		/// Returns the next token from the token list
		/// </summary>
		/// <returns>The string value of the token</returns>
		public System.String NextToken()
		{				
			return NextToken(this.delimiters);
		}

		/// <summary>
		/// Returns the next token from the source string, using the provided
		/// token delimiters
		/// </summary>
		/// <param name="delimiters">String containing the delimiters to use</param>
		/// <returns>The string value of the token</returns>
		public System.String NextToken(System.String delimiters)
		{
			//According to documentation, the usage of the received delimiters should be temporary (only for this call).
			//However, it seems it is not true, so the following line is necessary.
			this.delimiters = delimiters;

			//at the end 
			if (this.currentPos == this.chars.Length)
				throw new System.ArgumentOutOfRangeException();
			//if over a delimiter and delimiters must be returned
			else if (   (System.Array.IndexOf(delimiters.ToCharArray(),chars[this.currentPos]) != -1)
				     && this.includeDelims )                	
				return "" + this.chars[this.currentPos++];
			//need to get the token wo delimiters.
			else
				return nextToken(delimiters.ToCharArray());
		}

		//Returns the nextToken wo delimiters
		private System.String nextToken(char[] delimiters)
		{
			string token="";
			long pos = this.currentPos;

			//skip possible delimiters
			while (System.Array.IndexOf(delimiters,this.chars[currentPos]) != -1)
				//The last one is a delimiter (i.e there is no more tokens)
				if (++this.currentPos == this.chars.Length)
				{
					this.currentPos = pos;
					throw new System.ArgumentOutOfRangeException();
				}
			
			//getting the token
			while (System.Array.IndexOf(delimiters,this.chars[this.currentPos]) == -1)
			{
				token+=this.chars[this.currentPos];
				//the last one is not a delimiter
				if (++this.currentPos == this.chars.Length)
					break;
			}
			return token;
		}

				
		/// <summary>
		/// Determines if there are more tokens to return from the source string
		/// </summary>
		/// <returns>True or false, depending if there are more tokens</returns>
		public bool HasMoreTokens()
		{
			//keeping the current pos
			long pos = this.currentPos;
			
			try
			{
				this.NextToken();
			}
			catch (System.ArgumentOutOfRangeException)
			{				
				return false;
			}
			finally
			{
				this.currentPos = pos;
			}
			return true;
		}

		/// <summary>
		/// Remaining tokens count
		/// </summary>
		public int Count
		{
			get
			{
				//keeping the current pos
				long pos = this.currentPos;
				int i = 0;
			
				try
				{
					while (true)
					{
						this.NextToken();
						i++;
					}
				}
				catch (System.ArgumentOutOfRangeException)
				{				
					this.currentPos = pos;
					return i;
				}
			}
		}

		/// <summary>
		///  Performs the same action as NextToken.
		/// </summary>
		public System.Object Current
		{
			get
			{
				return (Object) this.NextToken();
			}		
		}		
		
		/// <summary>
		//  Performs the same action as HasMoreTokens.
		/// </summary>
		/// <returns>True or false, depending if there are more tokens</returns>
		public bool MoveNext()
		{
			return this.HasMoreTokens();
		}
		
		/// <summary>
		/// Does nothing.
		/// </summary>
		public void  Reset()
		{
			;
		}			
	}
	/*******************************/
	/// <summary>
	/// This class provides auxiliar functionality to read and unread characters from a string into a buffer.
	/// </summary>
	private class BackStringReader : System.IO.StringReader
	{
		private char[] buffer;
		private int position = 1;

		/// <summary>
		/// Constructor. Calls the base constructor.
		/// </summary>
		/// <param name="stringReader">The buffer from which chars will be read.</param>
		/// <param name="size">The size of the Back buffer.</param>
		public BackStringReader(String s) : base (s)
		{
			this.buffer = new char[position];
		}


		/// <summary>
		/// Reads a character.
		/// </summary>
		/// <returns>The character read.</returns>
		public override int Read()
		{
			if (this.position >= 0 && this.position < this.buffer.Length)
				return (int) this.buffer[this.position++];
			return base.Read();
		}

		/// <summary>
		/// Reads an amount of characters from the buffer and copies the values to the array passed.
		/// </summary>
		/// <param name="array">Array where the characters will be stored.</param>
		/// <param name="index">The beginning index to read.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <returns>The number of characters read.</returns>
		public override int Read(char[] array, int index, int count)
		{
			int readLimit = this.buffer.Length - this.position;

			if (count <= 0)
				return 0;

			if (readLimit > 0)
			{
				if (count < readLimit)
					readLimit = count;
                Buffer.BlockCopy(this.buffer, this.position, array, index, readLimit);
				count -= readLimit;
				index += readLimit;
				this.position += readLimit;
			}

			if (count > 0)
			{
				count = base.Read(array, index, count);
				if (count == -1)
				{
					if (readLimit == 0)
						return -1;
					return readLimit;
				}
				return readLimit + count;
			}
			return readLimit;
		}

		/// <summary>
		/// Unreads a character.
		/// </summary>
		/// <param name="unReadChar">The character to be unread.</param>
		public void UnRead(int unReadChar)
		{
			this.position--;
			this.buffer[this.position] = (char) unReadChar;
		}

		/// <summary>
		/// Unreads an amount of characters by moving these to the buffer.
		/// </summary>
		/// <param name="array">The character array to be unread.</param>
		/// <param name="index">The beginning index to unread.</param>
		/// <param name="count">The number of characters to unread.</param>
		public void UnRead(char[] array, int index, int count)
		{
			this.Move(array, index, count);
		}

		/// <summary>
		/// Unreads an amount of characters by moving these to the buffer.
		/// </summary>
		/// <param name="array">The character array to be unread.</param>
		public void UnRead(char[] array)
		{
			this.Move(array, 0, array.Length - 1);
		}

		/// <summary>
		/// Moves the array of characters to the buffer.
		/// </summary>
		/// <param name="array">Array of characters to move.</param>
		/// <param name="index">Offset of the beginning.</param>
		/// <param name="count">Amount of characters to move.</param>
		private void Move(char[] array, int index, int count)
		{
			for (int arrayPosition = index + count; arrayPosition >= index; arrayPosition--)
				this.UnRead(array[arrayPosition]);
		}
	}

	/*******************************/

	/// <summary>
	/// The StreamTokenizerSupport class takes an input stream and parses it into "tokens".
	/// The stream tokenizer can recognize identifiers, numbers, quoted strings, and various comment styles. 
	/// </summary>
	public class StreamTokenizerSupport
	{

		/// <summary>
		/// Internal constants and fields
		/// </summary>

		private const System.String TOKEN	= "Token[";
		private const System.String NOTHING	= "NOTHING";
		private const System.String NUMBER	= "number=";
		private const System.String EOF		= "EOF";
		private const System.String EOL		= "EOL";
		private const System.String QUOTED	= "quoted string=";
		private const System.String LINE	= "], Line ";
		private const System.String DASH	= "-.";
		private const System.String DOT		= ".";
		
		private const int TT_NOTHING		= - 4;
		
		private const sbyte ORDINARYCHAR	= 0x00;
		private const sbyte WORDCHAR		= 0x01;
		private const sbyte WHITESPACECHAR	= 0x02;
		private const sbyte COMMENTCHAR		= 0x04;
		private const sbyte QUOTECHAR		= 0x08;
		private const sbyte NUMBERCHAR		= 0x10;
		
		private const int STATE_NEUTRAL		= 0;
		private const int STATE_WORD		= 1;
		private const int STATE_NUMBER1		= 2;
		private const int STATE_NUMBER2		= 3;
		private const int STATE_NUMBER3		= 4;
		private const int STATE_NUMBER4		= 5;
		private const int STATE_STRING		= 6;
		private const int STATE_LINECOMMENT	= 7;
		private const int STATE_DONE_ON_EOL = 8;

		private const int STATE_PROCEED_ON_EOL			= 9;
		private const int STATE_POSSIBLEC_COMMENT		= 10;
		private const int STATE_POSSIBLEC_COMMENT_END	= 11;
		private const int STATE_C_COMMENT				= 12;
		private const int STATE_STRING_ESCAPE_SEQ		= 13;
		private const int STATE_STRING_ESCAPE_SEQ_OCTAL	= 14;
		
		private const int STATE_DONE		= 100;
		
		private sbyte[] attribute			= new sbyte[256];
		private bool eolIsSignificant		= false;
		private bool slashStarComments	= false;
		private bool slashSlashComments	= false;
		private bool lowerCaseMode		= false;
		private bool pushedback			= false;
		private int lineno				= 1;

		private BackReader inReader;
		private BackStringReader inStringReader;
		private BackInputStream inStream;
		private System.Text.StringBuilder buf;


		/// <summary>
		/// Indicates that the end of the stream has been read.
		/// </summary>
		public const int TT_EOF		= - 1;

		/// <summary>
		/// Indicates that the end of the line has been read.
		/// </summary>
		public const int TT_EOL		= '\n';

		/// <summary>
		/// Indicates that a number token has been read.
		/// </summary>
		public const int TT_NUMBER	= - 2;

		/// <summary>
		/// Indicates that a word token has been read.
		/// </summary>
		public const int TT_WORD	= - 3;

		/// <summary>
		/// If the current token is a number, this field contains the value of that number.
		/// </summary>
		public double nval;

		/// <summary>
		/// If the current token is a word token, this field contains a string giving the characters of the word 
		/// token.
		/// </summary>
		public System.String sval;

		/// <summary>
		/// After a call to the nextToken method, this field contains the type of the token just read.
		/// </summary>
		public int ttype;


		/// <summary>
		/// Internal methods
		/// </summary>

		private int read()
		{
			if (this.inReader != null)
				return this.inReader.Read();
			else if (this.inStream != null)
				return this.inStream.Read();
			else
				return this.inStringReader.Read();
		}
		
		private void unread(int ch)
		{
			if (this.inReader != null) 
				this.inReader.UnRead(ch);
			else if (this.inStream != null)
				this.inStream.UnRead(ch);
			else
				this.inStringReader.UnRead(ch);
		}

		private void init()
		{
			this.buf = new System.Text.StringBuilder();
			this.ttype = StreamTokenizerSupport.TT_NOTHING;
			
			this.WordChars('A', 'Z');
			this.WordChars('a', 'z');
			this.WordChars(160, 255);
			this.WhitespaceChars(0x00, 0x20);
			this.CommentChar('/');
			this.QuoteChar('\'');
			this.QuoteChar('\"');
			this.ParseNumbers();
		}

		private void setAttributes(int low, int hi, sbyte attrib)
		{
			int l = System.Math.Max(0, low);
			int h = System.Math.Min(255, hi);
			for (int i = l; i <= h; i++)
				this.attribute[i] = attrib;
		}
		
		private bool isWordChar(int data)
		{
			char ch = (char) data;
			return (data != - 1 && (ch > 255 || this.attribute[ch] == StreamTokenizerSupport.WORDCHAR || this.attribute[ch] == StreamTokenizerSupport.NUMBERCHAR));
		}
		
		/// <summary>
		/// Creates a StreamToknizerSupport that parses the given string.
		/// </summary>
		/// <param name="reader">The System.IO.StringReader that contains the String to be parsed.</param>
		public StreamTokenizerSupport(System.IO.StringReader reader)
		{
			string s = "";
			for (int i = reader.Read(); i != -1 ; i = reader.Read())
			{
				s += (char) i;
			}
			reader.Close();
            		this.inStringReader = new BackStringReader(s);
			this.init();
		}		

		/// <summary>
		/// Creates a StreamTokenizerSupport that parses the given stream.
		/// </summary>
		/// <param name="reader">Reader to be parsed.</param>
		public StreamTokenizerSupport(System.IO.StreamReader reader)
		{
			this.inReader = new BackReader(new System.IO.StreamReader(reader.BaseStream, reader.CurrentEncoding).BaseStream, 2, reader.CurrentEncoding);
			this.init();
		}
		
		/// <summary>
		/// Creates a StreamTokenizerSupport that parses the given stream.
		/// </summary>
		/// <param name="stream">Stream to be parsed.</param>
		public StreamTokenizerSupport(System.IO.Stream stream)
		{
			this.inStream = new BackInputStream(new System.IO.BufferedStream(stream), 2);
			this.init();
		}
		
		/// <summary>
		/// Specified that the character argument starts a single-line comment.
		/// </summary>
		/// <param name="ch">The character.</param>
		public virtual void CommentChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
				this.attribute[ch] = StreamTokenizerSupport.COMMENTCHAR;
		}
		
		/// <summary>
		/// Determines whether or not ends of line are treated as tokens.
		/// </summary>
		/// <param name="flag">True indicates that end-of-line characters are separate tokens; False indicates 
		/// that end-of-line characters are white space.</param>
		public virtual void  EOLIsSignificant(bool flag)
		{
			this.eolIsSignificant = flag;
		}

		/// <summary>
		/// Return the current line number.
		/// </summary>
		/// <returns>Current line number</returns>
		public virtual int Lineno()
		{
			return this.lineno;
		}
		
		/// <summary>
		/// Determines whether or not word token are automatically lowercased.
		/// </summary>
		/// <param name="flag">True indicates that all word tokens should be lowercased.</param>
		public virtual void LowerCaseMode(bool flag)
		{
			this.lowerCaseMode = flag;
		}
		
		/// <summary>
		/// Parses the next token from the input stream of this tokenizer.
		/// </summary>
		/// <returns>The value of the ttype field.</returns>
		public virtual int NextToken()
		{
			char prevChar = (char) (0);
			char ch = (char) (0);
			char qChar = (char) (0);
			int octalNumber = 0;
			int state;

			if (this.pushedback)
			{
				this.pushedback = false;
				return this.ttype;
			}
			
			this.ttype = StreamTokenizerSupport.TT_NOTHING;
			state = StreamTokenizerSupport.STATE_NEUTRAL;
			this.nval = 0.0;
			this.sval = null;
			this.buf.Length = 0;

			do 
			{
				int data = this.read();
				prevChar = ch;
				ch = (char) data;
				
				switch (state)
				{
					case StreamTokenizerSupport.STATE_NEUTRAL:
					{
						if (data == - 1)
						{
							this.ttype = TT_EOF;
							state = StreamTokenizerSupport.STATE_DONE;
						}
						else if (ch > 255)
						{
							this.buf.Append(ch);
							this.ttype = StreamTokenizerSupport.TT_WORD;
							state = StreamTokenizerSupport.STATE_WORD;
						}
						else if (this.attribute[ch] == StreamTokenizerSupport.COMMENTCHAR)
						{
							state = StreamTokenizerSupport.STATE_LINECOMMENT;
						}
						else if (this.attribute[ch] == StreamTokenizerSupport.WORDCHAR)
						{
							this.buf.Append(ch);
							this.ttype = StreamTokenizerSupport.TT_WORD;
							state = StreamTokenizerSupport.STATE_WORD;
						}
						else if (this.attribute[ch] == StreamTokenizerSupport.NUMBERCHAR)
						{
							this.ttype = StreamTokenizerSupport.TT_NUMBER;
							this.buf.Append(ch);
							if (ch == '-')
								state = StreamTokenizerSupport.STATE_NUMBER1;
							else if (ch == '.')
								state = StreamTokenizerSupport.STATE_NUMBER3;
							else
								state = StreamTokenizerSupport.STATE_NUMBER2;
						}
						else if (this.attribute[ch] == StreamTokenizerSupport.QUOTECHAR)
						{
							qChar = ch;
							this.ttype = ch;
							state = StreamTokenizerSupport.STATE_STRING;
						}
						else if ((this.slashSlashComments || this.slashStarComments) && ch == '/')
							state = StreamTokenizerSupport.STATE_POSSIBLEC_COMMENT;
						else if (this.attribute[ch] == StreamTokenizerSupport.ORDINARYCHAR)
						{
							this.ttype = ch;
							state = StreamTokenizerSupport.STATE_DONE;
						}
						else if (ch == '\n' || ch == '\r')
						{
							this.lineno++;
							if (this.eolIsSignificant)
							{
								this.ttype = StreamTokenizerSupport.TT_EOL;
								if (ch == '\n')
									state = StreamTokenizerSupport.STATE_DONE;
								else if (ch == '\r')
									state = StreamTokenizerSupport.STATE_DONE_ON_EOL;
							}
							else if (ch == '\r')
								state = StreamTokenizerSupport.STATE_PROCEED_ON_EOL;
						}
						break;
					}
					case StreamTokenizerSupport.STATE_WORD: 
					{
						if (this.isWordChar(data))
							this.buf.Append(ch);
						else
						{
							if (data != - 1)
								this.unread(ch);
							this.sval = this.buf.ToString();
							state = StreamTokenizerSupport.STATE_DONE;
						}
						break;
					}
					case StreamTokenizerSupport.STATE_NUMBER1: 
					{
						if (data == - 1 || this.attribute[ch] != StreamTokenizerSupport.NUMBERCHAR || ch == '-')
						{
							if ( this.attribute[ch] == StreamTokenizerSupport.COMMENTCHAR && System.Char.IsNumber(ch) )
							{
								this.buf.Append(ch);
								state = StreamTokenizerSupport.STATE_NUMBER2;
							}
							else
							{
								if (data != - 1)
									this.unread(ch);
								this.ttype = '-';
								state = StreamTokenizerSupport.STATE_DONE;
							}
						}
						else
						{
							this.buf.Append(ch);
							if (ch == '.')
								state = StreamTokenizerSupport.STATE_NUMBER3;
							else
								state = StreamTokenizerSupport.STATE_NUMBER2;
						}
						break;
					}
					case StreamTokenizerSupport.STATE_NUMBER2: 
					{
						if (data == - 1 || this.attribute[ch] != StreamTokenizerSupport.NUMBERCHAR || ch == '-')
						{
							if (System.Char.IsNumber(ch) && this.attribute[ch] == StreamTokenizerSupport.WORDCHAR)
							{
								this.buf.Append(ch);
							}
							else if (ch == '.' && this.attribute[ch] == StreamTokenizerSupport.WHITESPACECHAR)
							{
								this.buf.Append(ch);
							}

							else if ( (data != -1) && (this.attribute[ch] == StreamTokenizerSupport.COMMENTCHAR && System.Char.IsNumber(ch) ))
							{
								this.buf.Append(ch);
							}
							else
							{
								if (data != - 1)
									this.unread(ch);
								try
								{
									this.nval = System.Double.Parse(this.buf.ToString());
								}
								catch (System.FormatException) {}
								state = StreamTokenizerSupport.STATE_DONE;
							}
						}
						else
						{
							this.buf.Append(ch);
							if (ch == '.')
								state = StreamTokenizerSupport.STATE_NUMBER3;
						}
						break;
					}
					case StreamTokenizerSupport.STATE_NUMBER3: 
					{
						if (data == - 1 || this.attribute[ch] != StreamTokenizerSupport.NUMBERCHAR || ch == '-' || ch == '.')
						{
							if ( this.attribute[ch] == StreamTokenizerSupport.COMMENTCHAR && System.Char.IsNumber(ch))
							{
								this.buf.Append(ch);
							}
							else 
							{
								if (data != - 1)
									this.unread(ch);
								System.String str = this.buf.ToString();
								if (str.Equals(StreamTokenizerSupport.DASH))
								{
									this.unread('.');
									this.ttype = '-';
								}
								else if (str.Equals(StreamTokenizerSupport.DOT) && !(StreamTokenizerSupport.WORDCHAR != this.attribute[prevChar]))
									this.ttype = '.';
								else
								{
									try
									{
										this.nval = System.Double.Parse(str);
									}
									catch (System.FormatException){}
								}
								state = StreamTokenizerSupport.STATE_DONE;
							}
						}
						else
						{
							this.buf.Append(ch);
							state = StreamTokenizerSupport.STATE_NUMBER4;
						}
						break;
					}
					case StreamTokenizerSupport.STATE_NUMBER4: 
					{
						if (data == - 1 || this.attribute[ch] != StreamTokenizerSupport.NUMBERCHAR || ch == '-' || ch == '.')
						{
							if (data != - 1)
								this.unread(ch);
							try
							{
								this.nval = System.Double.Parse(this.buf.ToString());
							}
							catch (System.FormatException) {}
							state = StreamTokenizerSupport.STATE_DONE;
						}
						else
							this.buf.Append(ch);
						break;
					}
					case StreamTokenizerSupport.STATE_LINECOMMENT: 
					{
						if (data == - 1)
						{
							this.ttype = StreamTokenizerSupport.TT_EOF;
							state = StreamTokenizerSupport.STATE_DONE;
						}
						else if (ch == '\n' || ch == '\r')
						{
							this.unread(ch);
							state = StreamTokenizerSupport.STATE_NEUTRAL;
						}
						break;
					}
					case StreamTokenizerSupport.STATE_DONE_ON_EOL: 
					{
						if (ch != '\n' && data != - 1)
							this.unread(ch);
						state = StreamTokenizerSupport.STATE_DONE;
						break;
					}
					case StreamTokenizerSupport.STATE_PROCEED_ON_EOL: 
					{
						if (ch != '\n' && data != - 1)
							this.unread(ch);
						state = StreamTokenizerSupport.STATE_NEUTRAL;
						break;
					}
					case StreamTokenizerSupport.STATE_STRING: 
					{
						if (data == - 1 || ch == qChar || ch == '\r' || ch == '\n')
						{
							this.sval = this.buf.ToString();
							if (ch == '\r' || ch == '\n')
								this.unread(ch);
							state = StreamTokenizerSupport.STATE_DONE;
						}
						else if (ch == '\\')
							state = StreamTokenizerSupport.STATE_STRING_ESCAPE_SEQ;
						else
							this.buf.Append(ch);
						break;
					}
					case StreamTokenizerSupport.STATE_STRING_ESCAPE_SEQ: 
					{
						if (data == - 1)
						{
							this.sval = this.buf.ToString();
							state = StreamTokenizerSupport.STATE_DONE;
							break;
						}
						
						state = StreamTokenizerSupport.STATE_STRING;
						if (ch == 'a')
							this.buf.Append(0x7);
						else if (ch == 'b')
							this.buf.Append('\b');
						else if (ch == 'f')
							this.buf.Append(0xC);
						else if (ch == 'n')
							this.buf.Append('\n');
						else if (ch == 'r')
							this.buf.Append('\r');
						else if (ch == 't')
							this.buf.Append('\t');
						else if (ch == 'v')
							this.buf.Append(0xB);
						else if (ch >= '0' && ch <= '7')
						{
							octalNumber = ch - '0';
							state = StreamTokenizerSupport.STATE_STRING_ESCAPE_SEQ_OCTAL;
						}
						else
							this.buf.Append(ch);
						break;
					}
					case StreamTokenizerSupport.STATE_STRING_ESCAPE_SEQ_OCTAL: 
					{
						if (data == - 1 || ch < '0' || ch > '7')
						{
							this.buf.Append((char) octalNumber);
							if (data == - 1)
							{
								this.sval = buf.ToString();
								state = StreamTokenizerSupport.STATE_DONE;
							}
							else
							{
								this.unread(ch);
								state = StreamTokenizerSupport.STATE_STRING;
							}
						}
						else
						{
							int temp = octalNumber * 8 + (ch - '0');
							if (temp < 256)
								octalNumber = temp;
							else
							{
								buf.Append((char) octalNumber);
								buf.Append(ch);
								state = StreamTokenizerSupport.STATE_STRING;
							}
						}
						break;
					}
					case StreamTokenizerSupport.STATE_POSSIBLEC_COMMENT: 
					{
						if (ch == '*')
							state = StreamTokenizerSupport.STATE_C_COMMENT;
						else if (ch == '/')
							state = StreamTokenizerSupport.STATE_LINECOMMENT;
						else
						{
							if (data != - 1)
								this.unread(ch);
							this.ttype = '/';
							state = StreamTokenizerSupport.STATE_DONE;
						}
						break;
					}
					case StreamTokenizerSupport.STATE_C_COMMENT: 
					{
						if (ch == '*')
							state = StreamTokenizerSupport.STATE_POSSIBLEC_COMMENT_END;
						if (ch == '\n')
							this.lineno++;
						else if (data == - 1)
						{
							this.ttype = StreamTokenizerSupport.TT_EOF;
							state = StreamTokenizerSupport.STATE_DONE;
						}
						break;
					}
					case StreamTokenizerSupport.STATE_POSSIBLEC_COMMENT_END: 
					{
						if (data == - 1)
						{
							this.ttype = StreamTokenizerSupport.TT_EOF;
							state = StreamTokenizerSupport.STATE_DONE;
						}
						else if (ch == '/')
							state = StreamTokenizerSupport.STATE_NEUTRAL;
						else if (ch != '*')
							state = StreamTokenizerSupport.STATE_C_COMMENT;
						break;
					}
				}
			}
			while (state != StreamTokenizerSupport.STATE_DONE);
			
			if (this.ttype == StreamTokenizerSupport.TT_WORD && this.lowerCaseMode)
				this.sval = this.sval.ToLower();

			return this.ttype;
		}

		/// <summary>
		/// Specifies that the character argument is "ordinary" in this tokenizer.
		/// </summary>
		/// <param name="ch">The character.</param>
		public virtual void OrdinaryChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
				this.attribute[ch] = StreamTokenizerSupport.ORDINARYCHAR;
		}
		
		/// <summary>
		/// Specifies that all characters c in the range low less-equal c less-equal high are "ordinary" in this 
		/// tokenizer.
		/// </summary>
		/// <param name="low">Low end of the range.</param>
		/// <param name="hi">High end of the range.</param>
		public virtual void OrdinaryChars(int low, int hi)
		{
			this.setAttributes(low, hi, StreamTokenizerSupport.ORDINARYCHAR);
		}
		
		/// <summary>
		/// Specifies that numbers should be parsed by this tokenizer.
		/// </summary>
		public virtual void ParseNumbers()
		{
			for (int i = '0'; i <= '9'; i++)
				this.attribute[i] = StreamTokenizerSupport.NUMBERCHAR;
			this.attribute['.'] = StreamTokenizerSupport.NUMBERCHAR;
			this.attribute['-'] = StreamTokenizerSupport.NUMBERCHAR;
		}
		
		/// <summary>
		/// Causes the next call to the nextToken method of this tokenizer to return the current value in the 
		/// ttype field, and not to modify the value in the nval or sval field.
		/// </summary>
		public virtual void PushBack()
		{
			if (this.ttype != StreamTokenizerSupport.TT_NOTHING)
				this.pushedback = true;
		}

		/// <summary>
		/// Specifies that matching pairs of this character delimit string constants in this tokenizer.
		/// </summary>
		/// <param name="ch">The character.</param>
		public virtual void QuoteChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
				this.attribute[ch] = QUOTECHAR;
		}
		
		/// <summary>
		/// Resets this tokenizer's syntax table so that all characters are "ordinary." See the ordinaryChar 
		/// method for more information on a character being ordinary.
		/// </summary>
		public virtual void ResetSyntax()
		{
			this.OrdinaryChars(0x00, 0xff);
		}
		
		/// <summary>
		/// Determines whether or not the tokenizer recognizes C++-style comments.
		/// </summary>
		/// <param name="flag">True indicates to recognize and ignore C++-style comments.</param>
		public virtual void SlashSlashComments(bool flag)
		{
			this.slashSlashComments = flag;
		}
		
		/// <summary>
		/// Determines whether or not the tokenizer recognizes C-style comments.
		/// </summary>
		/// <param name="flag">True indicates to recognize and ignore C-style comments.</param>
		public virtual void SlashStarComments(bool flag)
		{
			this.slashStarComments = flag;
		}
		
		/// <summary>
		/// Returns the string representation of the current stream token.
		/// </summary>
		/// <returns>A String representation of the current stream token.</returns>
		public override System.String ToString()
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder(StreamTokenizerSupport.TOKEN);
			
			switch (this.ttype)
			{
				case StreamTokenizerSupport.TT_NOTHING:
				{
					buffer.Append(StreamTokenizerSupport.NOTHING);
					break;
				}
				case StreamTokenizerSupport.TT_WORD: 
				{
					buffer.Append(this.sval);
					break;
				}
				case StreamTokenizerSupport.TT_NUMBER: 
				{
					buffer.Append(StreamTokenizerSupport.NUMBER);
					buffer.Append(this.nval);
					break;
				}
				case StreamTokenizerSupport.TT_EOF: 
				{
					buffer.Append(StreamTokenizerSupport.EOF);
					break;
				}
				case StreamTokenizerSupport.TT_EOL: 
				{
					buffer.Append(StreamTokenizerSupport.EOL);
					break;
				}
			}

			if (this.ttype > 0)
			{
				if (this.attribute[this.ttype] == StreamTokenizerSupport.QUOTECHAR)
				{
					buffer.Append(StreamTokenizerSupport.QUOTED);
					buffer.Append(this.sval);
				}
				else
				{
					buffer.Append('\'');
					buffer.Append((char) this.ttype);
					buffer.Append('\'');
				}
			}

			buffer.Append(StreamTokenizerSupport.LINE);
			buffer.Append(this.lineno);
			return buffer.ToString();
		}

		/// <summary>
		/// Specifies that all characters c in the range low less-equal c less-equal high are white space 
		/// characters.
		/// </summary>
		/// <param name="low">The low end of the range.</param>
		/// <param name="hi">The high end of the range.</param>
		public virtual void WhitespaceChars(int low, int hi)
		{
			this.setAttributes(low, hi, StreamTokenizerSupport.WHITESPACECHAR);
		}
		
		/// <summary>
		/// Specifies that all characters c in the range low less-equal c less-equal high are word constituents.
		/// </summary>
		/// <param name="low">The low end of the range.</param>
		/// <param name="hi">The high end of the range.</param>
		public virtual void WordChars(int low, int hi)
		{
			this.setAttributes(low, hi, StreamTokenizerSupport.WORDCHAR);
		}
	}


	/*******************************/
	/// <summary>
	/// This class provides functionality to reads and unread characters into a buffer.
	/// </summary>
	public class BackReader : System.IO.StreamReader
	{
		private char[] buffer;
		private int position = 1;
		//private int markedPosition;

		/// <summary>
		/// Constructor. Calls the base constructor.
		/// </summary>
		/// <param name="streamReader">The buffer from which chars will be read.</param>
		/// <param name="size">The size of the Back buffer.</param>
		public BackReader(System.IO.Stream streamReader, int size, System.Text.Encoding encoding) : base(streamReader,encoding)
		{
			this.buffer = new char[size];
			this.position = size;
		}

		/// <summary>
		/// Constructor. Calls the base constructor.
		/// </summary>
		/// <param name="streamReader">The buffer from which chars will be read.</param>
		public BackReader(System.IO.Stream streamReader, System.Text.Encoding encoding) : base(streamReader, encoding)
		{
			this.buffer = new char[this.position];
		}

		/// <summary>
		/// Checks if this stream support mark and reset methods.
		/// </summary>
		/// <remarks>
		/// This method isn't supported.
		/// </remarks>
		/// <returns>Always false.</returns>
		public bool MarkSupported()
		{
			return false;
		}

		/// <summary>
		/// Marks the element at the corresponding position.
		/// </summary>
		/// <remarks>
		/// This method isn't supported.
		/// </remarks>
		public void Mark(int position)
		{
			throw new System.IO.IOException("Mark operations are not allowed");			
		}

		/// <summary>
		/// Resets the current stream.
		/// </summary>
		/// <remarks>
		/// This method isn't supported.
		/// </remarks>
		public void Reset()
		{
			throw new System.IO.IOException("Mark operations are not allowed");
		}

		/// <summary>
		/// Reads a character.
		/// </summary>
		/// <returns>The character read.</returns>
		public override int Read()
		{
			if (this.position >= 0 && this.position < this.buffer.Length)
				return (int) this.buffer[this.position++];
			return base.Read();
		}

		/// <summary>
		/// Reads an amount of characters from the buffer and copies the values to the array passed.
		/// </summary>
		/// <param name="array">Array where the characters will be stored.</param>
		/// <param name="index">The beginning index to read.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <returns>The number of characters read.</returns>
		public override int Read(char[] array, int index, int count)
		{
			int readLimit = this.buffer.Length - this.position;

			if (count <= 0)
				return 0;

			if (readLimit > 0)
			{
				if (count < readLimit)
					readLimit = count;
                Buffer.BlockCopy(this.buffer, this.position, array, index, readLimit);
				count -= readLimit;
				index += readLimit;
				this.position += readLimit;
			}

			if (count > 0)
			{
				count = base.Read(array, index, count);
				if (count == -1)
				{
					if (readLimit == 0)
						return -1;
					return readLimit;
				}
				return readLimit + count;
			}
			return readLimit;
		}

		/// <summary>
		/// Checks if this buffer is ready to be read.
		/// </summary>
		/// <returns>True if the position is less than the length, otherwise false.</returns>
		public bool IsReady()
		{
			return (this.position >= this.buffer.Length || this.BaseStream.Position >= this.BaseStream.Length);
		}

		/// <summary>
		/// Unreads a character.
		/// </summary>
		/// <param name="unReadChar">The character to be unread.</param>
		public void UnRead(int unReadChar)
		{
			this.position--;
			this.buffer[this.position] = (char) unReadChar;
		}

		/// <summary>
		/// Unreads an amount of characters by moving these to the buffer.
		/// </summary>
		/// <param name="array">The character array to be unread.</param>
		/// <param name="index">The beginning index to unread.</param>
		/// <param name="count">The number of characters to unread.</param>
		public void UnRead(char[] array, int index, int count)
		{
			this.Move(array, index, count);
		}

		/// <summary>
		/// Unreads an amount of characters by moving these to the buffer.
		/// </summary>
		/// <param name="array">The character array to be unread.</param>
		public void UnRead(char[] array)
		{
			this.Move(array, 0, array.Length - 1);
		}

		/// <summary>
		/// Moves the array of characters to the buffer.
		/// </summary>
		/// <param name="array">Array of characters to move.</param>
		/// <param name="index">Offset of the beginning.</param>
		/// <param name="count">Amount of characters to move.</param>
		private void Move(char[] array, int index, int count)
		{
			for (int arrayPosition = index + count; arrayPosition >= index; arrayPosition--)
				this.UnRead(array[arrayPosition]);
		}
	}


	/*******************************/
	/// <summary>
	/// Provides functionality to read and unread from a Stream.
	/// </summary>
	public class BackInputStream : System.IO.BinaryReader
	{
		private byte[] buffer;
		private int position = 1;

		/// <summary>
		/// Creates a BackInputStream with the specified stream and size for the buffer.
		/// </summary>
		/// <param name="streamReader">The stream to use.</param>
		/// <param name="size">The specific size of the buffer.</param>
		public BackInputStream(System.IO.Stream streamReader, System.Int32 size) : base(streamReader)
		{
			this.buffer = new byte[size];
			this.position = size;
		}

		/// <summary>
		/// Creates a BackInputStream with the specified stream.
		/// </summary>
		/// <param name="streamReader">The stream to use.</param>
		public BackInputStream(System.IO.Stream streamReader) : base(streamReader)
		{
			this.buffer = new byte[this.position];
		}

		/// <summary>
		/// Checks if this stream support mark and reset methods.
		/// </summary>
		/// <returns>Always false, these methods aren't supported.</returns>
		public bool MarkSupported()
		{	
			return false;
		}

		/// <summary>
		/// Reads the next bytes in the stream.
		/// </summary>
		/// <returns>The next byte readed</returns>
		public override int Read()
		{
			if (position >= 0 && position < buffer.Length)
				return (int) this.buffer[position++];
			return base.Read();
		}

		/// <summary>
		/// Reads the amount of bytes specified from the stream.
		/// </summary>
		/// <param name="array">The buffer to read data into.</param>
		/// <param name="index">The beginning point to read.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <returns>The number of characters read into buffer.</returns>
		public virtual int Read(sbyte[] array, int index, int count)
		{
			int byteCount = 0;
			int readLimit = count + index;
			byte[] aux = ToByteArray(array);

			for (byteCount = 0; position < buffer.Length && index < readLimit; byteCount++)
				aux[index++] = buffer[position++];

			if (index < readLimit)
				byteCount += base.Read(aux, index, readLimit - index);

			for(int i = 0; i < aux.Length;i++)
				array[i] = (sbyte)aux[i];

			return byteCount;
		}

		/// <summary>
		/// Unreads a byte from the stream.
		/// </summary>
		/// <param name="element">The value to be unread.</param>
		public void UnRead(int element)
		{
			this.position--;
			if (position >= 0)
				this.buffer[this.position] = (byte) element;
		}

		/// <summary>
		/// Unreads an amount of bytes from the stream.
		/// </summary>
		/// <param name="array">The byte array to be unread.</param>
		/// <param name="index">The beginning index to unread.</param>
		/// <param name="count">The number of bytes to be unread.</param>
		public void UnRead(byte[] array, int index, int count)
		{
			this.Move(array, index, count);
		}

		/// <summary>
		/// Unreads an array of bytes from the stream.
		/// </summary>
		/// <param name="array">The byte array to be unread.</param>
		public void UnRead(byte[] array)
		{
			this.Move(array, 0, array.Length - 1);
		}

		/// <summary>
		/// Skips the specified number of bytes from the underlying stream.
		/// </summary>
		/// <param name="numberOfBytes">The number of bytes to be skipped.</param>
		/// <returns>The number of bytes actually skipped</returns>
		public long Skip(long numberOfBytes)
		{
			return this.BaseStream.Seek(numberOfBytes, System.IO.SeekOrigin.Current) - this.BaseStream.Position;
		}

		/// <summary>
		/// Moves data from the array to the buffer field.
		/// </summary>
		/// <param name="array">The array of bytes to be unread.</param>
		/// <param name="index">The beginning index to unread.</param>
		/// <param name="count">The amount of bytes to be unread.</param>
		private void Move(byte[] array, int index, int count)
		{
			for (int arrayPosition = index + count;  arrayPosition >= index; arrayPosition--)
				this.UnRead(array[arrayPosition]);
		}
	}


	/*******************************/
	/// <summary>
	/// Support class used to handle threads
	/// </summary>
	public class ThreadClass : IThreadRunnable
	{
		/// <summary>
		/// The instance of System.Threading.Thread
		/// </summary>
		private System.Threading.Thread threadField;
	      
		/// <summary>
		/// Initializes a new instance of the ThreadClass class
		/// </summary>
		public ThreadClass()
		{
			threadField = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
		}
	 
		/// <summary>
		/// Initializes a new instance of the Thread class.
		/// </summary>
		/// <param name="Name">The name of the thread</param>
		public ThreadClass(System.String Name)
		{
			threadField = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
			this.Name = Name;
		}
	      
		/// <summary>
		/// Initializes a new instance of the Thread class.
		/// </summary>
		/// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
		public ThreadClass(System.Threading.ThreadStart Start)
		{
			threadField = new System.Threading.Thread(Start);
		}
	 
		/// <summary>
		/// Initializes a new instance of the Thread class.
		/// </summary>
		/// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
		/// <param name="Name">The name of the thread</param>
		public ThreadClass(System.Threading.ThreadStart Start, System.String Name)
		{
			threadField = new System.Threading.Thread(Start);
			this.Name = Name;
		}
	      
		/// <summary>
		/// This method has no functionality unless the method is overridden
		/// </summary>
		public virtual void Run()
		{
		}
	      
		/// <summary>
		/// Causes the operating system to change the state of the current thread instance to ThreadState.Running
		/// </summary>
		public virtual void Start()
		{
			threadField.Start();
		}
	      
		/// <summary>
		/// Interrupts a thread that is in the WaitSleepJoin thread state
		/// </summary>
		public virtual void Interrupt()
		{
			threadField.Interrupt();
		}
	      
		/// <summary>
		/// Gets the current thread instance
		/// </summary>
		public System.Threading.Thread Instance
		{
			get
			{
				return threadField;
			}
			set
			{
				threadField = value;
			}
		}
	      
		/// <summary>
		/// Gets or sets the name of the thread
		/// </summary>
		public System.String Name
		{
			get
			{
				return threadField.Name;
			}
			set
			{
				if (threadField.Name == null)
					threadField.Name = value; 
			}
		}
	      
		/// <summary>
		/// Gets or sets a value indicating the scheduling priority of a thread
		/// </summary>
		public System.Threading.ThreadPriority Priority
		{
			get
			{
				return threadField.Priority;
			}
			set
			{
				threadField.Priority = value;
			}
		}
	      
		/// <summary>
		/// Gets a value indicating the execution status of the current thread
		/// </summary>
		public bool IsAlive
		{
			get
			{
				return threadField.IsAlive;
			}
		}
	      
		/// <summary>
		/// Gets or sets a value indicating whether or not a thread is a background thread.
		/// </summary>
		public bool IsBackground
		{
			get
			{
				return threadField.IsBackground;
			} 
			set
			{
				threadField.IsBackground = value;
			}
		}
	      
		/// <summary>
		/// Blocks the calling thread until a thread terminates
		/// </summary>
		public void Join()
		{
			threadField.Join();
		}
	      
		/// <summary>
		/// Blocks the calling thread until a thread terminates or the specified time elapses
		/// </summary>
		/// <param name="MiliSeconds">Time of wait in milliseconds</param>
		public void Join(long MiliSeconds)
		{
			lock(this)
			{
				threadField.Join(new System.TimeSpan(MiliSeconds * 10000));
			}
		}
	      
		/// <summary>
		/// Blocks the calling thread until a thread terminates or the specified time elapses
		/// </summary>
		/// <param name="MiliSeconds">Time of wait in milliseconds</param>
		/// <param name="NanoSeconds">Time of wait in nanoseconds</param>
		public void Join(long MiliSeconds, int NanoSeconds)
		{
			lock(this)
			{
				threadField.Join(new System.TimeSpan(MiliSeconds * 10000 + NanoSeconds * 100));
			}
		}
	    /* 
		/// <summary>
		/// Resumes a thread that has been suspended
		/// </summary>
		public void Resume()
		{
			threadField.Resume();
		}

		/// <summary>
		/// Raises a ThreadAbortException in the thread on which it is invoked, 
		/// to begin the process of terminating the thread. Calling this method 
		/// usually terminates the thread
		/// </summary>
		public void Abort()
		{
			threadField.Abort();
		}
	    
 
		/// <summary>
		/// Raises a ThreadAbortException in the thread on which it is invoked, 
		/// to begin the process of terminating the thread while also providing
		/// exception information about the thread termination. 
		/// Calling this method usually terminates the thread.
		/// </summary>
		/// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted</param>
		public void Abort(System.Object stateInfo)
		{
			lock(this)
			{
				threadField.Abort(stateInfo);
			}
		}
	      
		/// <summary>
		/// Suspends the thread, if the thread is already suspended it has no effect
		/// </summary>
		public void Suspend()
		{
			threadField.Suspend();
		}
	    */
  
		/// <summary>
		/// Obtain a String that represents the current Object
		/// </summary>
		/// <returns>A String that represents the current Object</returns>
		public override System.String ToString()
		{
			return "Thread[" + Name + "," + Priority.ToString() + "," + "" + "]";
		}
	     
		/// <summary>
		/// Gets the currently running thread
		/// </summary>
		/// <returns>The currently running thread</returns>
		public static ThreadClass Current()
		{
			ThreadClass CurrentThread = new ThreadClass();
			CurrentThread.Instance = System.Threading.Thread.CurrentThread;
			return CurrentThread;
		}
	}


	/*******************************/
	/// <summary>
	/// SupportClass for the Stack class.
	/// </summary>
	public class StackSupport
	{
		/// <summary>
		/// Removes the element at the top of the stack and returns it.
		/// </summary>
		/// <param name="stack">The stack where the element at the top will be returned and removed.</param>
		/// <returns>The element at the top of the stack.</returns>
		public static System.Object Pop(System.Collections.ArrayList stack)
		{
			System.Object obj = stack[stack.Count - 1];
			stack.RemoveAt(stack.Count - 1);

			return obj;
		}
	}


}
