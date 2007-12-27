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
                float floatValue = LLObject.UnpackBeginCut(i);
                ushort result = LLObject.PackBeginCut(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                    ", and ended up with " + result);
            }
        }

        [Test]
        public void PathEnd()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.UnpackEndCut(i);
                ushort result = LLObject.PackEndCut(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                    ", and ended up with " + result);
            }
        }

        [Test]
        public void PathRevolutions()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.UnpackPathRevolutions(i);
                byte result = LLObject.PackPathRevolutions(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                    ", and ended up with " + result);
            }
        }

        [Test]
        public void PathScale()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.UnpackPathScale(i);
                byte result = LLObject.PackPathScale(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                    ", and ended up with " + result);
            }
        }

        [Test]
        public void PathShear()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.UnpackPathShear(i);
                byte result = LLObject.PackPathShear(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathTaper()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = LLObject.UnpackPathTaper(i);
                sbyte result = LLObject.PackPathTaper(floatValue);

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
        public void TextureEntry()
        {
            LLObject.TextureEntry te = new LLObject.TextureEntry(LLUUID.Random());
            LLObject.TextureEntryFace face = te.CreateFace(0);
            face.Bump = Bumpiness.Concrete;
            face.Fullbright = true;
            face.MediaFlags = true;
            face.OffsetU = 0.5f;
            face.OffsetV = -0.5f;
            face.RepeatU = 3.0f;
            face.RepeatV = 4.0f;
            face.RGBA = new LLColor(0f, 0.25f, 0.75f, 1f);
            face.Rotation = 1.5f;
            face.Shiny = Shininess.Medium;
            face.TexMapType = MappingType.Planar;
            face.TextureID = LLUUID.Random();

            byte[] teBytes = te.ToBytes();

            LLObject.TextureEntry te2 = new LLObject.TextureEntry(teBytes, 0, teBytes.Length);

            byte[] teBytes2 = te2.ToBytes();

            Assert.IsTrue(teBytes.Length == teBytes2.Length);

            for (int i = 0; i < teBytes.Length; i++)
            {
                Assert.IsTrue(teBytes[i] == teBytes2[i], "Byte " + i + " is not equal");
            }
        }
    }
}
