using System;
using System.Collections.Generic;
using System.Net;
using libsecondlife;
using libsecondlife.Packets;
using NUnit.Framework;

namespace libsecondlife.Tests
{
    [TestFixture]
    public class PrimObjectTests : Assert
    {
        [Test]
        public void PathBegin()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.PathBeginFloat(i);
                ushort result = LLObject.PathBeginUInt16(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathEnd()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.PathEndFloat(i);
                ushort result = LLObject.PathEndUInt16(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathRadiusOffset()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = LLObject.PathRadiusOffsetFloat(i);
                sbyte result = LLObject.PathRadiusOffsetByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathRevolutions()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.PathRevolutionsFloat(i);
                byte result = LLObject.PathRevolutionsByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathScale()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.PathScaleFloat(i);
                byte result = LLObject.PathScaleByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathShear()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.PathShearFloat(i);
                byte result = LLObject.PathShearByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathSkew()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = LLObject.PathSkewFloat(i);
                sbyte result = LLObject.PathSkewByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathTaper()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = LLObject.PathTaperFloat(i);
                sbyte result = LLObject.PathTaperByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void ProfileBegin()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.ProfileBeginFloat(i);
                ushort result = LLObject.ProfileBeginUInt16(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void ProfileEnd()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.ProfileEndFloat(i);
                ushort result = LLObject.ProfileEndUInt16(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void TextureEntryOffsets()
        {
            for (float i = -1.0f; i <= 1.0f; i += 0.001f)
            {
                i = (float)Math.Round(i, 3);

                short offset = Helpers.TEOffsetShort(i);
                float foffset = Helpers.TEOffsetFloat(BitConverter.GetBytes(offset), 0);
                foffset = (float)Math.Round(foffset, 3);

                Assert.IsTrue(foffset - i < Single.Epsilon, foffset + " is not equal to " + i);
            }
        }

        [Test]
        public void TextureEntryRotations()
        {
            ;
        }

        [Test]
        public void TextureEntry()
        {
            LLObject.TextureEntry2 te = new LLObject.TextureEntry2(LLUUID.Random());
            LLObject.TextureEntryFace face = te.CreateFace(0);
            face.Bump = LLObject.Bumpiness.Concrete;
            face.Fullbright = true;
            face.MediaFlags = true;
            face.OffsetU = 1.0f;
            face.OffsetV = 2.0f;
            face.RepeatU = 3.0f;
            face.RepeatV = 4.0f;
            face.RGBA = 1234;
            face.Rotation = 5.0f;
            face.Shiny = LLObject.Shininess.Medium;
            face.TexMapType = LLObject.Mapping.Planar;
            face.TextureID = LLUUID.Random();

            byte[] teBytes = te.ToBytes();

            LLObject.TextureEntry2 te2 = new LLObject.TextureEntry2(teBytes, 0, teBytes.Length);

            byte[] teBytes2 = te2.ToBytes();

            Assert.IsTrue(teBytes.Length == teBytes2.Length);

            for (int i = 0; i < teBytes.Length; i++)
            {
                Assert.IsTrue(teBytes[i] == teBytes2[i], "Byte " + i + " is not equal");
            }
        }
    }
}
