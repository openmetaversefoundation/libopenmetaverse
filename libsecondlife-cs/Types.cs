/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;

namespace libsecondlife
{
	public class LLUUID
	{
		public byte[] Data;

		public LLUUID()
		{
			Data = new byte[16];
		}

		public LLUUID(string val)
		{
			Data = new byte[16];

			if (val.Length == 36)
			{
				val = val.Replace("-", "");
			}
			
			if (val.Length != 32)
			{
				return;
			}

			for(int i = 0; i < 16; ++i)
			{
				Data[i] = Convert.ToByte(val.Substring(i * 2, 2), 16);
			}
		}

		public LLUUID(byte[] byteArray, int pos)
		{
			Data = new byte[16];

			Array.Copy(byteArray, pos, Data, 0, 16);
		}

		public override int GetHashCode()
		{
			return BitConverter.ToInt32(Data, 0);
		}

		public override bool Equals(object o)
		{
			if (!(o is LLUUID))
			{
				return false;
			}

			return this == (LLUUID)o;
		}

		public static bool operator==(LLUUID lhs, LLUUID rhs)
		{
			for (int i = 0; i < 16; ++i)
			{
				if (lhs.Data[i] != rhs.Data[i])
				{
					return false;
				}
			}

			return true;
		}

		public static bool operator !=(LLUUID lhs, LLUUID rhs)
		{
			return !(lhs == rhs);
		}

		public static implicit operator LLUUID(string val)
		{
			return new LLUUID(val);
		}

		public override string ToString()
		{
			string uuid = "";

			for (int i = 0; i < 16; ++i)
			{
				uuid += Data[i].ToString("x2");
			}

			return uuid;
		}
	}

	public class LLVector3
	{
		public float X;
		public float Y;
		public float Z;

		public LLVector3()
		{
			X = Y = Z = 0.0F;
		}

		public LLVector3(byte[] byteArray, int pos)
		{
			X = BitConverter.ToSingle(byteArray, pos);
			Y = BitConverter.ToSingle(byteArray, pos + 4);
			Z = BitConverter.ToSingle(byteArray, pos + 8);
		}

		public LLVector3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[12];

			Array.Copy(BitConverter.GetBytes(X), 0, byteArray, 0, 4);
			Array.Copy(BitConverter.GetBytes(Y), 0, byteArray, 4, 4);
			Array.Copy(BitConverter.GetBytes(Z), 0, byteArray, 8, 4);

			return byteArray;
		}

		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString();
		}
	}

	public class LLVector3d
	{
		public double X;
		public double Y;
		public double Z;

		public LLVector3d()
		{
			X = Y = Z = 0.0D;
		}

		public LLVector3d(byte[] byteArray, int pos)
		{
			X = BitConverter.ToDouble(byteArray, pos);
			Y = BitConverter.ToDouble(byteArray, pos + 8);
			Z = BitConverter.ToDouble(byteArray, pos + 16);
		}

		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString();
		}
	}

	public class LLVector4
	{
		public float X;
		public float Y;
		public float Z;
		public float S;

		public LLVector4()
		{
			X = Y = Z = S = 0.0F;
		}

		public LLVector4(byte[] byteArray, int pos)
		{
			X = BitConverter.ToSingle(byteArray, pos);
			Y = BitConverter.ToSingle(byteArray, pos + 4);
			Z = BitConverter.ToSingle(byteArray, pos + 8);
			S = BitConverter.ToSingle(byteArray, pos + 12);
		}

		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString() + " " + S.ToString();
		}
	}

	public class LLQuaternion
	{
		public float X;
		public float Y;
		public float Z;
		public float S;

		public LLQuaternion()
		{
			X = Y = Z = S = 0.0F;
		}

		public LLQuaternion(byte[] byteArray, int pos)
		{
			X = BitConverter.ToSingle(byteArray, pos);
			Y = BitConverter.ToSingle(byteArray, pos + 4);
			Z = BitConverter.ToSingle(byteArray, pos + 8);
			S = BitConverter.ToSingle(byteArray, pos + 12);
		}

		public LLQuaternion(float x, float y, float z, float s)
		{
			X = x;
			Y = y;
			Z = z;
			S = s;
		}

		public byte[] GetBytes()
		{
			byte[] byteArray = new byte[16];

			Array.Copy(BitConverter.GetBytes(X), 0, byteArray, 0, 4);
			Array.Copy(BitConverter.GetBytes(Y), 0, byteArray, 4, 4);
			Array.Copy(BitConverter.GetBytes(Z), 0, byteArray, 8, 4);
			Array.Copy(BitConverter.GetBytes(S), 0, byteArray, 12, 4);

			return byteArray;
		}

		public override string ToString()
		{
			return X.ToString() + " " + Y.ToString() + " " + Z.ToString() + " " + S.ToString();
		}
	}

	public class PrimObject
	{
		public sbyte PathTwistBegin;
		public byte PathEnd;
		public byte ProfileBegin;
		public sbyte PathRadiusOffset;
		public sbyte PathSkew;
		public byte ProfileCurve;
		public byte PathScaleX;
		public byte PathScaleY;
		public byte Material;
		public string NameValue;
		public byte PathShearX;
		public byte PathShearY;
		public sbyte PathTaperX;
		public sbyte PathTaperY;
		public byte ProfileEnd;
		public byte PathBegin;
		public byte PathCurve;
		public LLVector3 Scale;
		public byte State;
		public sbyte PathTwist;
		public byte ProfileHollow;
		public byte PathRevolutions;
		public LLQuaternion Rotation;
	}
}
