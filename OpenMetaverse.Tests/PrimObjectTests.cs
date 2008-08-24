/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using System.Collections.Generic;
using System.Net;
using OpenMetaverse;
using OpenMetaverse.Packets;
using NUnit.Framework;

namespace OpenMetaverse.Tests
{
    [TestFixture]
    public class PrimObjectTests : Assert
    {
        [Test]
        public void PathBegin()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = Primitive.UnpackBeginCut(i);
                ushort result = Primitive.PackBeginCut(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                    ", and ended up with " + result);
            }
        }

        [Test]
        public void PathEnd()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = Primitive.UnpackEndCut(i);
                ushort result = Primitive.PackEndCut(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                    ", and ended up with " + result);
            }
        }

        [Test]
        public void PathRevolutions()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = Primitive.UnpackPathRevolutions(i);
                byte result = Primitive.PackPathRevolutions(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                    ", and ended up with " + result);
            }
        }

        [Test]
        public void PathScale()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = Primitive.UnpackPathScale(i);
                byte result = Primitive.PackPathScale(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                    ", and ended up with " + result);
            }
        }

        //[Test]
        //public void PathShear()
        //{
        //    for (byte i = 0; i < byte.MaxValue; i++)
        //    {
        //        float floatValue = Primitive.UnpackPathShear(i);
        //        byte result = Primitive.PackPathShear(floatValue);

        //        Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
        //        ", and ended up with " + result);
        //    }
        //}

        [Test]
        public void PathTaper()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = Primitive.UnpackPathTaper(i);
                sbyte result = Primitive.PackPathTaper(floatValue);

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
            Primitive.TextureEntry te = new Primitive.TextureEntry(UUID.Random());
            Primitive.TextureEntryFace face = te.CreateFace(0);
            face.Bump = Bumpiness.Concrete;
            face.Fullbright = true;
            face.MediaFlags = true;
            face.OffsetU = 0.5f;
            face.OffsetV = -0.5f;
            face.RepeatU = 3.0f;
            face.RepeatV = 4.0f;
            face.RGBA = new Color4(0f, 0.25f, 0.75f, 1f);
            face.Rotation = 1.5f;
            face.Shiny = Shininess.Medium;
            face.TexMapType = MappingType.Planar;
            face.TextureID = UUID.Random();

            byte[] teBytes = te.ToBytes();

            Primitive.TextureEntry te2 = new Primitive.TextureEntry(teBytes, 0, teBytes.Length);

            byte[] teBytes2 = te2.ToBytes();

            Assert.IsTrue(teBytes.Length == teBytes2.Length);

            for (int i = 0; i < teBytes.Length; i++)
            {
                Assert.IsTrue(teBytes[i] == teBytes2[i], "Byte " + i + " is not equal");
            }
        }
    }
}
