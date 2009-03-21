/*
 * Copyright (c) 2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
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
using System.Runtime.InteropServices;

namespace OpenMetaverse
{
    /// <summary>
    /// An 8-bit color structure including an alpha channel
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Color4 : IComparable<Color4>, IEquatable<Color4>
    {
        /// <summary>Red</summary>
        public float R;
        /// <summary>Green</summary>
        public float G;
        /// <summary>Blue</summary>
        public float B;
        /// <summary>Alpha</summary>
        public float A;

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public Color4(byte r, byte g, byte b, byte a)
        {
            const float quanta = 1.0f / 255.0f;

            R = (float)r * quanta;
            G = (float)g * quanta;
            B = (float)b * quanta;
            A = (float)a * quanta;
        }

        public Color4(float r, float g, float b, float a)
        {
            // Quick check to see if someone is doing something obviously wrong
            // like using float values from 0.0 - 255.0
            if (r > 1f || g > 1f || b > 1f || a > 1f)
                throw new ArgumentException(
                    String.Format("Attempting to initialize Color4 with out of range values <{0},{1},{2},{3}>",
                    r, g, b, a));

            // Valid range is from 0.0 to 1.0
            R = Utils.Clamp(r, 0f, 1f);
            G = Utils.Clamp(g, 0f, 1f);
            B = Utils.Clamp(b, 0f, 1f);
            A = Utils.Clamp(a, 0f, 1f);
        }

        /// <summary>
        /// Builds a color from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing a 16 byte color</param>
        /// <param name="pos">Beginning position in the byte array</param>
        /// <param name="inverted">True if the byte array stores inverted values,
        /// otherwise false. For example the color black (fully opaque) inverted
        /// would be 0xFF 0xFF 0xFF 0x00</param>
        public Color4(byte[] byteArray, int pos, bool inverted)
        {
            R = G = B = A = 0f;
            FromBytes(byteArray, pos, inverted);
        }

        /// <summary>
        /// Returns the raw bytes for this vector
        /// </summary>
        /// <param name="byteArray">Byte array containing a 16 byte color</param>
        /// <param name="pos">Beginning position in the byte array</param>
        /// <param name="inverted">True if the byte array stores inverted values,
        /// otherwise false. For example the color black (fully opaque) inverted
        /// would be 0xFF 0xFF 0xFF 0x00</param>
        /// <param name="alphaInverted">True if the alpha value is inverted in
        /// addition to whatever the inverted parameter is. Setting inverted true
        /// and alphaInverted true will flip the alpha value back to non-inverted,
        /// but keep the other color bytes inverted</param>
        /// <returns>A 16 byte array containing R, G, B, and A</returns>
        public Color4(byte[] byteArray, int pos, bool inverted, bool alphaInverted)
        {
            R = G = B = A = 0f;
            FromBytes(byteArray, pos, inverted, alphaInverted);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="color">Color to copy</param>
        public Color4(Color4 color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// IComparable.CompareTo implementation
        /// </summary>
        /// <remarks>Sorting ends up like this: |--Grayscale--||--Color--|.
        /// Alpha is only used when the colors are otherwise equivalent</remarks>
        public int CompareTo(Color4 color)
        {
            float thisHue = GetHue();
            float thatHue = color.GetHue();

            if (thisHue < 0f && thatHue < 0f)
            {
                // Both monochromatic
                if (R == color.R)
                {
                    // Monochromatic and equal, compare alpha
                    return A.CompareTo(color.A);
                }
                else
                {
                    // Compare lightness
                    return R.CompareTo(R);
                }
            }
            else
            {
                if (thisHue == thatHue)
                {
                    // RGB is equal, compare alpha
                    return A.CompareTo(color.A);
                }
                else
                {
                    // Compare hues
                    return thisHue.CompareTo(thatHue);
                }
            }
        }

        public void FromBytes(byte[] byteArray, int pos, bool inverted)
        {
            const float quanta = 1.0f / 255.0f;

            if (inverted)
            {
                R = (float)(255 - byteArray[pos]) * quanta;
                G = (float)(255 - byteArray[pos + 1]) * quanta;
                B = (float)(255 - byteArray[pos + 2]) * quanta;
                A = (float)(255 - byteArray[pos + 3]) * quanta;
            }
            else
            {
                R = (float)byteArray[pos] * quanta;
                G = (float)byteArray[pos + 1] * quanta;
                B = (float)byteArray[pos + 2] * quanta;
                A = (float)byteArray[pos + 3] * quanta;
            }
        }

        /// <summary>
        /// Builds a color from a byte array
        /// </summary>
        /// <param name="byteArray">Byte array containing a 16 byte color</param>
        /// <param name="pos">Beginning position in the byte array</param>
        /// <param name="inverted">True if the byte array stores inverted values,
        /// otherwise false. For example the color black (fully opaque) inverted
        /// would be 0xFF 0xFF 0xFF 0x00</param>
        /// <param name="alphaInverted">True if the alpha value is inverted in
        /// addition to whatever the inverted parameter is. Setting inverted true
        /// and alphaInverted true will flip the alpha value back to non-inverted,
        /// but keep the other color bytes inverted</param>
        public void FromBytes(byte[] byteArray, int pos, bool inverted, bool alphaInverted)
        {
            FromBytes(byteArray, pos, inverted);

            if (alphaInverted)
                A = 1.0f - A;
        }

        public byte[] GetBytes()
        {
            return GetBytes(false);
        }

        public byte[] GetBytes(bool inverted)
        {
            byte[] byteArray = new byte[4];
            ToBytes(byteArray, 0, inverted);
            return byteArray;
        }

        public byte[] GetFloatBytes()
        {
            byte[] bytes = new byte[16];
            ToFloatBytes(bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Writes the raw bytes for this color to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 16 bytes before the end of the array</param>
        public void ToBytes(byte[] dest, int pos)
        {
            ToBytes(dest, pos, false);
        }

        /// <summary>
        /// Serializes this color into four bytes in a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 4 bytes before the end of the array</param>
        /// <param name="inverted">True to invert the output (1.0 becomes 0
        /// instead of 255)</param>
        public void ToBytes(byte[] dest, int pos, bool inverted)
        {
            dest[pos + 0] = Utils.FloatToByte(R, 0f, 1f);
            dest[pos + 1] = Utils.FloatToByte(G, 0f, 1f);
            dest[pos + 2] = Utils.FloatToByte(B, 0f, 1f);
            dest[pos + 3] = Utils.FloatToByte(A, 0f, 1f);

            if (inverted)
            {
                dest[pos + 0] = (byte)(255 - dest[pos + 0]);
                dest[pos + 1] = (byte)(255 - dest[pos + 1]);
                dest[pos + 2] = (byte)(255 - dest[pos + 2]);
                dest[pos + 3] = (byte)(255 - dest[pos + 3]);
            }
        }

        /// <summary>
        /// Writes the raw bytes for this color to a byte array
        /// </summary>
        /// <param name="dest">Destination byte array</param>
        /// <param name="pos">Position in the destination array to start
        /// writing. Must be at least 16 bytes before the end of the array</param>
        public void ToFloatBytes(byte[] dest, int pos)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(R), 0, dest, pos + 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(G), 0, dest, pos + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(B), 0, dest, pos + 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(A), 0, dest, pos + 12, 4);
        }

        public float GetHue()
        {
            const float HUE_MAX = 360f;

            float max = Math.Max(Math.Max(R, G), B);
            float min = Math.Min(Math.Min(R, B), B);

            if (max == min)
            {
                // Achromatic, hue is undefined
                return -1f;
            }
            else if (R == max)
            {
                float bDelta = (((max - B) * (HUE_MAX / 6f)) + ((max - min) / 2f)) / (max - min);
                float gDelta = (((max - G) * (HUE_MAX / 6f)) + ((max - min) / 2f)) / (max - min);
                return bDelta - gDelta;
            }
            else if (G == max)
            {
                float rDelta = (((max - R) * (HUE_MAX / 6f)) + ((max - min) / 2f)) / (max - min);
                float bDelta = (((max - B) * (HUE_MAX / 6f)) + ((max - min) / 2f)) / (max - min);
                return (HUE_MAX / 3f) + rDelta - bDelta;
            }
            else // B == max
            {
                float gDelta = (((max - G) * (HUE_MAX / 6f)) + ((max - min) / 2f)) / (max - min);
                float rDelta = (((max - R) * (HUE_MAX / 6f)) + ((max - min) / 2f)) / (max - min);
                return ((2f * HUE_MAX) / 3f) + gDelta - rDelta;
            }
        }

        #endregion Public Methods

        #region Static Methods

        /// <summary>
        /// Create an RGB color from a hue, saturation, value combination
        /// </summary>
        /// <param name="hue">Hue</param>
        /// <param name="saturation">Saturation</param>
        /// <param name="value">Value</param>
        /// <returns>An fully opaque RGB color (alpha is 1.0)</returns>
        public static Color4 FromHSV(double hue, double saturation, double value)
        {
            double r = 0d;
            double g = 0d;
            double b = 0d;

            if (saturation == 0d)
            {
                // If s is 0, all colors are the same.
                // This is some flavor of gray.
                r = value;
                g = value;
                b = value;
            }
            else
            {
                double p;
                double q;
                double t;

                double fractionalSector;
                int sectorNumber;
                double sectorPos;

                // The color wheel consists of 6 sectors.
                // Figure out which sector you//re in.
                sectorPos = hue / 60d;
                sectorNumber = (int)(Math.Floor(sectorPos));

                // get the fractional part of the sector.
                // That is, how many degrees into the sector
                // are you?
                fractionalSector = sectorPos - sectorNumber;

                // Calculate values for the three axes
                // of the color. 
                p = value * (1d - saturation);
                q = value * (1d - (saturation * fractionalSector));
                t = value * (1d - (saturation * (1d - fractionalSector)));

                // Assign the fractional colors to r, g, and b
                // based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        r = value;
                        g = t;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = value;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = value;
                        b = t;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = value;
                        break;
                    case 4:
                        r = t;
                        g = p;
                        b = value;
                        break;
                    case 5:
                        r = value;
                        g = p;
                        b = q;
                        break;
                }
            }

            return new Color4((float)r, (float)g, (float)b, 1f);
        }

        #endregion Static Methods

        #region Overrides

        public override string ToString()
        {
            return String.Format(Utils.EnUsCulture, "<{0}, {1}, {2}, {3}>", R, G, B, A);
        }

        public string ToRGBString()
        {
            return String.Format(Utils.EnUsCulture, "<{0}, {1}, {2}>", R, G, B);
        }

        public override bool Equals(object obj)
        {
            return (obj is Color4) ? this == (Color4)obj : false;
        }

        public bool Equals(Color4 other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode() ^ A.GetHashCode();
        }

        #endregion Overrides

        #region Operators

        public static bool operator ==(Color4 lhs, Color4 rhs)
        {
            return (lhs.R == rhs.R) && (lhs.G == rhs.G) && (lhs.B == rhs.B) && (lhs.A == rhs.A);
        }

        public static bool operator !=(Color4 lhs, Color4 rhs)
        {
            return !(lhs == rhs);
        }

        #endregion Operators

        /// <summary>A Color4 with zero RGB values and fully opaque (alpha 1.0)</summary>
        public readonly static Color4 Black = new Color4(0f, 0f, 0f, 1f);

        /// <summary>A Color4 with full RGB values (1.0) and fully opaque (alpha 1.0)</summary>
        public readonly static Color4 White = new Color4(1f, 1f, 1f, 1f);
    }
}
