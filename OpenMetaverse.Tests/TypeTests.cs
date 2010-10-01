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
using System.Collections;
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using NUnit.Framework;

namespace OpenMetaverse.Tests
{
    [TestFixture]
    public class TypeTests : Assert
    {
        [Test]
        public void UUIDs()
        {
            // Creation
            UUID a = new UUID();
            byte[] bytes = a.GetBytes();
            for (int i = 0; i < 16; i++)
                Assert.IsTrue(bytes[i] == 0x00);

            // Comparison
            a = new UUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0xFF, 0xFF }, 0);
            UUID b = new UUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0);

            Assert.IsTrue(a == b, "UUID comparison operator failed, " + a.ToString() + " should equal " + 
                b.ToString());

            // From string
            a = new UUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0);
            string zeroonetwo = "00010203-0405-0607-0809-0a0b0c0d0e0f";
            b = new UUID(zeroonetwo);

            Assert.IsTrue(a == b, "UUID hyphenated string constructor failed, should have " + a.ToString() + 
                " but we got " + b.ToString());

            // ToString()            
            Assert.IsTrue(a == b);                        
            Assert.IsTrue(a == (UUID)zeroonetwo);

            // TODO: CRC test
        }

        [Test]
        public void Vector3ApproxEquals()
        {
            Vector3 a = new Vector3(1f, 0f, 0f);
            Vector3 b = new Vector3(0f, 0f, 0f);

            Assert.IsFalse(a.ApproxEquals(b, 0.9f), "ApproxEquals failed (1)");
            Assert.IsTrue(a.ApproxEquals(b, 1.0f), "ApproxEquals failed (2)");

            a = new Vector3(-1f, 0f, 0f);
            b = new Vector3(1f, 0f, 0f);

            Assert.IsFalse(a.ApproxEquals(b, 1.9f), "ApproxEquals failed (3)");
            Assert.IsTrue(a.ApproxEquals(b, 2.0f), "ApproxEquals failed (4)");

            a = new Vector3(0f, -1f, 0f);
            b = new Vector3(0f, -1.1f, 0f);

            Assert.IsFalse(a.ApproxEquals(b, 0.09f), "ApproxEquals failed (5)");
            Assert.IsTrue(a.ApproxEquals(b, 0.11f), "ApproxEquals failed (6)");

            a = new Vector3(0f, 0f, 0.00001f);
            b = new Vector3(0f, 0f, 0f);

            Assert.IsFalse(b.ApproxEquals(a, Single.Epsilon), "ApproxEquals failed (6)");
            Assert.IsTrue(b.ApproxEquals(a, 0.0001f), "ApproxEquals failed (7)");
        }

        [Test]
        public void Quaternions()
        {
            Quaternion a = new Quaternion(1, 0, 0, 0);
            Quaternion b = new Quaternion(1, 0, 0, 0);

            Assert.IsTrue(a == b, "Quaternion comparison operator failed");

            Quaternion expected = new Quaternion(0, 0, 0, -1);
            Quaternion result = a * b;

            Assert.IsTrue(result == expected, a.ToString() + " * " + b.ToString() + " produced " + result.ToString() +
                " instead of " + expected.ToString());

            a = new Quaternion(1, 0, 0, 0);
            b = new Quaternion(0, 1, 0, 0);
            expected = new Quaternion(0, 0, -1, 0);
            result = a * b;

            Assert.IsTrue(result == expected, a.ToString() + " * " + b.ToString() + " produced " + result.ToString() +
                " instead of " + expected.ToString());

            a = new Quaternion(0, 0, 1, 0);
            b = new Quaternion(0, 1, 0, 0);
            expected = new Quaternion(1, 0, 0, 0);
            result = a * b;

            Assert.IsTrue(result == expected, a.ToString() + " * " + b.ToString() + " produced " + result.ToString() +
                " instead of " + expected.ToString());
        }

        //[Test]
        //public void VectorQuaternionMath()
        //{
        //    // Convert a vector to a quaternion and back
        //    Vector3 a = new Vector3(1f, 0.5f, 0.75f);
        //    Quaternion b = a.ToQuaternion();
        //    Vector3 c;
        //    b.GetEulerAngles(out c.X, out c.Y, out c.Z);

        //    Assert.IsTrue(a == c, c.ToString() + " does not equal " + a.ToString());
        //}

        [Test]
        public void FloatsToTerseStrings()
        {
            float f = 1.20f;
            string a = String.Empty;
            string b = "1.2";
            
            a = Helpers.FloatToTerseString(f);
            Assert.IsTrue(a == b, f.ToString() + " converted to " + a + ", expecting " + b);

            f = 24.00f;
            b = "24";

            a = Helpers.FloatToTerseString(f);
            Assert.IsTrue(a == b, f.ToString() + " converted to " + a + ", expecting " + b);

            f = -0.59f;
            b = "-.59";

            a = Helpers.FloatToTerseString(f);
            Assert.IsTrue(a == b, f.ToString() + " converted to " + a + ", expecting " + b);

            f = 0.59f;
            b = ".59";

            a = Helpers.FloatToTerseString(f);
            Assert.IsTrue(a == b, f.ToString() + " converted to " + a + ", expecting " + b);
        }

        [Test]
        public void BitUnpacking()
        {
            byte[] data = new byte[] { 0x80, 0x00, 0x0F, 0x50, 0x83, 0x7D };
            BitPack bitpacker = new BitPack(data, 0);

            int b = bitpacker.UnpackBits(1);
            Assert.IsTrue(b == 1, "Unpacked " + b + " instead of 1");

            b = bitpacker.UnpackBits(1);
            Assert.IsTrue(b == 0, "Unpacked " + b + " instead of 0");

            bitpacker = new BitPack(data, 2);

            b = bitpacker.UnpackBits(4);
            Assert.IsTrue(b == 0, "Unpacked " + b + " instead of 0");

            b = bitpacker.UnpackBits(8);
            Assert.IsTrue(b == 0xF5, "Unpacked " + b + " instead of 0xF5");

            b = bitpacker.UnpackBits(4);
            Assert.IsTrue(b == 0, "Unpacked " + b + " instead of 0");

            b = bitpacker.UnpackBits(10);
            Assert.IsTrue(b == 0x0183, "Unpacked " + b + " instead of 0x0183");
        }

        [Test]
        public void BitPacking()
        {
            byte[] packedBytes = new byte[12];
            BitPack bitpacker = new BitPack(packedBytes, 0);

            bitpacker.PackBits(0x0ABBCCDD, 32);
            bitpacker.PackBits(25, 5);
            bitpacker.PackFloat(123.321f);
            bitpacker.PackBits(1000, 16);

            bitpacker = new BitPack(packedBytes, 0);

            int b = bitpacker.UnpackBits(32);
            Assert.IsTrue(b == 0x0ABBCCDD, "Unpacked " + b + " instead of 2864434397");

            b = bitpacker.UnpackBits(5);
            Assert.IsTrue(b == 25, "Unpacked " + b + " instead of 25");

            float f = bitpacker.UnpackFloat();
            Assert.IsTrue(f == 123.321f, "Unpacked " + f + " instead of 123.321");

            b = bitpacker.UnpackBits(16);
            Assert.IsTrue(b == 1000, "Unpacked " + b + " instead of 1000");

            packedBytes = new byte[1];
            bitpacker = new BitPack(packedBytes, 0);
            bitpacker.PackBit(true);

            bitpacker = new BitPack(packedBytes, 0);
            b = bitpacker.UnpackBits(1);
            Assert.IsTrue(b == 1, "Unpacked " + b + " instead of 1");

            packedBytes = new byte[1] { Byte.MaxValue };
            bitpacker = new BitPack(packedBytes, 0);
            bitpacker.PackBit(false);

            bitpacker = new BitPack(packedBytes, 0);
            b = bitpacker.UnpackBits(1);
            Assert.IsTrue(b == 0, "Unpacked " + b + " instead of 0");
        }

        [Test]
        public void LLSDTerseParsing()
        {
            string testOne = "[r0.99967899999999998428,r-0.025334599999999998787,r0]";
            string testTwo = "[[r1,r1,r1],r0]";
            string testThree = "{'region_handle':[r255232, r256512], 'position':[r33.6, r33.71, r43.13], 'look_at':[r34.6, r33.71, r43.13]}";

            OSD obj = OSDParser.DeserializeLLSDNotation(testOne);
            Assert.IsInstanceOfType(typeof(OSDArray), obj, "Expected SDArray, got " + obj.GetType().ToString());
            OSDArray array = (OSDArray)obj;
            Assert.IsTrue(array.Count == 3, "Expected three contained objects, got " + array.Count);
            Assert.IsTrue(array[0].AsReal() > 0.9d && array[0].AsReal() < 1.0d, "Unexpected value for first real " + array[0].AsReal());
            Assert.IsTrue(array[1].AsReal() < 0.0d && array[1].AsReal() > -0.03d, "Unexpected value for second real " + array[1].AsReal());
            Assert.IsTrue(array[2].AsReal() == 0.0d, "Unexpected value for third real " + array[2].AsReal());

            obj = OSDParser.DeserializeLLSDNotation(testTwo);
            Assert.IsInstanceOfType(typeof(OSDArray), obj, "Expected SDArray, got " + obj.GetType().ToString());
            array = (OSDArray)obj;
            Assert.IsTrue(array.Count == 2, "Expected two contained objects, got " + array.Count);
            Assert.IsTrue(array[1].AsReal() == 0.0d, "Unexpected value for real " + array[1].AsReal());
            obj = array[0];
            Assert.IsInstanceOfType(typeof(OSDArray), obj, "Expected ArrayList, got " + obj.GetType().ToString());
            array = (OSDArray)obj;
            Assert.IsTrue(array[0].AsReal() == 1.0d && array[1].AsReal() == 1.0d && array[2].AsReal() == 1.0d,
                "Unexpected value(s) for nested array: " + array[0].AsReal() + ", " + array[1].AsReal() + ", " +
                array[2].AsReal());

            obj = OSDParser.DeserializeLLSDNotation(testThree);
            Assert.IsInstanceOfType(typeof(OSDMap), obj, "Expected LLSDMap, got " + obj.GetType().ToString());
            OSDMap hashtable = (OSDMap)obj;
            Assert.IsTrue(hashtable.Count == 3, "Expected three contained objects, got " + hashtable.Count);
            Assert.IsInstanceOfType(typeof(OSDArray), hashtable["region_handle"]);
            Assert.IsTrue(((OSDArray)hashtable["region_handle"]).Count == 2);
            Assert.IsInstanceOfType(typeof(OSDArray), hashtable["position"]);
            Assert.IsTrue(((OSDArray)hashtable["position"]).Count == 3);
            Assert.IsInstanceOfType(typeof(OSDArray), hashtable["look_at"]);
            Assert.IsTrue(((OSDArray)hashtable["look_at"]).Count == 3);
        }
    }
}
