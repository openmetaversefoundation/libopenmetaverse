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
			if (val.Length == 36)
			{
				val = val.Replace("-", "");
			}
			
			if (val.Length != 32)
			{
				Data = new byte[16];
				return;
			}

			Data = new byte[val.Length / 2];

			for(int i = 0; i < 16; ++i)
			{
				Data[i] = Convert.ToByte(val.Substring(i * 2, 2), 16);
			}
		}

		public LLUUID(byte[] byteArray, int pos)
		{
			for (int i = 0; i < 16; ++i)
			{
				Data[i] = byteArray[pos + i];
			}
		}

		public static implicit operator LLUUID(string val)
		{
			return new LLUUID(val);
		}
	}

	public class LLVector3
	{
		public float x;
		public float y;
		public float z;

		public LLVector3()
		{
			x = y = z = 0.0F;
		}

		public LLVector3(byte[] byteArray, int pos)
		{
			x = BitConverter.ToSingle(byteArray, pos);
			y = BitConverter.ToSingle(byteArray, pos + 4);
			z = BitConverter.ToSingle(byteArray, pos + 8);
		}
	}

	public class LLVector3d
	{
		public double x;
		public double y;
		public double z;

		public LLVector3d()
		{
			x = y = z = 0.0D;
		}

		public LLVector3d(byte[] byteArray, int pos)
		{
			x = BitConverter.ToDouble(byteArray, pos);
			y = BitConverter.ToDouble(byteArray, pos + 8);
			z = BitConverter.ToDouble(byteArray, pos + 16);
		}
	}

	public class LLVector4
	{
		public float x;
		public float y;
		public float z;
		public float s;

		public LLVector4()
		{
			x = y = z = s = 0.0F;
		}

		public LLVector4(byte[] byteArray, int pos)
		{
			x = BitConverter.ToSingle(byteArray, pos);
			y = BitConverter.ToSingle(byteArray, pos + 4);
			z = BitConverter.ToSingle(byteArray, pos + 8);
			s = BitConverter.ToSingle(byteArray, pos + 12);
		}
	}

	public class LLQuaternion
	{
		public float x;
		public float y;
		public float z;
		public float s;

		public LLQuaternion()
		{
			x = y = z = s = 0.0F;
		}

		public LLQuaternion(byte[] byteArray, int pos)
		{
			x = BitConverter.ToSingle(byteArray, pos);
			y = BitConverter.ToSingle(byteArray, pos + 4);
			z = BitConverter.ToSingle(byteArray, pos + 8);
			s = BitConverter.ToSingle(byteArray, pos + 12);
		}
	}
}
