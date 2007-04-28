using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Baker
{
	public static class Oven
	{
		public static Bitmap ModifyAlphaMask(Bitmap alpha, byte weight, float ramp)
		{
			// Create the new modifiable image (our canvas)
			int width = alpha.Width;
			int height = alpha.Height;
			int pixelFormatSize = Image.GetPixelFormatSize(alpha.PixelFormat) / 8;
			int stride = width * pixelFormatSize;
			byte[] data = new byte[stride * height];
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
			Bitmap modified = new Bitmap(width, height, stride, alpha.PixelFormat, pointer);
			
			// Copy the existing alpha mask to the canvas
			Graphics g = Graphics.FromImage(modified);
			g.DrawImageUnscaledAndClipped(alpha, new Rectangle(0, 0, width, height));
			g.Dispose();
			
			// Modify the canvas based on the input weight and ramp values
			// TODO: use the ramp
			// TODO: only bother with the alpha values
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] < weight) data[i] = 0;
			}
			
			return modified;
		}
		
		public static Bitmap ApplyAlphaMask(Bitmap source, Bitmap alpha)
		{
			// Create the new modifiable image (our canvas)
			int width = source.Width;
			int height = source.Height;
			
			if (alpha.Width != width || alpha.Height != height ||
			    alpha.PixelFormat != source.PixelFormat)
			{
				throw new Exception("Source image and alpha mask formats do not match");
			}
			
			int pixelFormatSize = Image.GetPixelFormatSize(source.PixelFormat) / 8;
			int stride = width * pixelFormatSize;
			byte[] data = new byte[stride * height];
			GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
			Bitmap modified = new Bitmap(width, height, stride, source.PixelFormat, pointer);
			
			// Copy the source image to the canvas
			Graphics g = Graphics.FromImage(modified);
			g.DrawImageUnscaledAndClipped(source, new Rectangle(0, 0, width, height));
			g.Dispose();
			
			// Get access to the pixel data for the alpha mask (probably using lockbits)
			
			// Combine the alpha mask alpha bytes in to the canvas
			
			return modified;
		}
	}
}
