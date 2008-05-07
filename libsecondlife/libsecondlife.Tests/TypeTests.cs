/*
 * Copyright (c) 2007-2008, Second Life Reverse Engineering Team
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
using System.Collections;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;
using libsecondlife.StructuredData;
using NUnit.Framework;

namespace libsecondlife.Tests
{
    [TestFixture]
    public class TypeTests : Assert
    {
        [Test]
        public void LLUUIDs()
        {
            // Creation
            LLUUID a = new LLUUID();
            byte[] bytes = a.GetBytes();
            for (int i = 0; i < 16; i++)
                Assert.IsTrue(bytes[i] == 0x00);

            // Comparison
            a = new LLUUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0xFF, 0xFF }, 0);
            LLUUID b = new LLUUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0);

            Assert.IsTrue(a == b, "LLUUID comparison operator failed, " + a.ToString() + " should equal " + 
                b.ToString());

            // From string
            a = new LLUUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0);
            string zeroonetwo = "00010203-0405-0607-0809-0a0b0c0d0e0f";
            b = new LLUUID(zeroonetwo);

            Assert.IsTrue(a == b, "LLUUID hyphenated string constructor failed, should have " + a.ToString() + 
                " but we got " + b.ToString());

            // ToString()
            string one = a.ToString();
            string two = b.ToString();
            Assert.IsTrue(a == b);
            one = a.ToString();
            two = b.ToString();
            Assert.IsTrue(a == b);
            Assert.IsTrue(a == zeroonetwo);

            // TODO: CRC test
        }

        [Test]
        public void Quaternions()
        {
            LLQuaternion a = new LLQuaternion(1, 0, 0, 0);
            LLQuaternion b = new LLQuaternion(1, 0, 0, 0);

            Assert.IsTrue(a == b, "LLQuaternion comparison operator failed");

            LLQuaternion expected = new LLQuaternion(0, 0, 0, -1);
            LLQuaternion result = a * b;

            Assert.IsTrue(result == expected, a.ToString() + " * " + b.ToString() + " produced " + result.ToString() +
                " instead of " + expected.ToString());

            a = new LLQuaternion(1, 0, 0, 0);
            b = new LLQuaternion(0, 1, 0, 0);
            expected = new LLQuaternion(0, 0, 1, 0);
            result = a * b;

            Assert.IsTrue(result == expected, a.ToString() + " * " + b.ToString() + " produced " + result.ToString() +
                " instead of " + expected.ToString());

            a = new LLQuaternion(0, 0, 1, 0);
            b = new LLQuaternion(0, 1, 0, 0);
            expected = new LLQuaternion(-1, 0, 0, 0);
            result = a * b;

            Assert.IsTrue(result == expected, a.ToString() + " * " + b.ToString() + " produced " + result.ToString() +
                " instead of " + expected.ToString());
        }

        //[Test]
        //public void VectorQuaternionMath()
        //{
        //    // Convert a vector to a quaternion and back
        //    LLVector3 a = new LLVector3(1f, 0.5f, 0.75f);
        //    LLQuaternion b = a.ToQuaternion();
        //    LLVector3 c;
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
        }

        [Test]
        public void LLSDTerseParsing()
        {
            string testOne = "[r0.99967899999999998428,r-0.025334599999999998787,r0]";
            string testTwo = "[[r1,r1,r1],r0]";
            string testThree = "{'region_handle':[r255232, r256512], 'position':[r33.6, r33.71, r43.13], 'look_at':[r34.6, r33.71, r43.13]}";

            LLSD obj = LLSDParser.DeserializeNotation(testOne);
            Assert.IsInstanceOfType(typeof(LLSDArray), obj, "Expected LLSDArray, got " + obj.GetType().ToString());
            LLSDArray array = (LLSDArray)obj;
            Assert.IsTrue(array.Count == 3, "Expected three contained objects, got " + array.Count);
            Assert.IsTrue(array[0].AsReal() > 0.9d && array[0].AsReal() < 1.0d, "Unexpected value for first real " + array[0].AsReal());
            Assert.IsTrue(array[1].AsReal() < 0.0d && array[1].AsReal() > -0.03d, "Unexpected value for second real " + array[1].AsReal());
            Assert.IsTrue(array[2].AsReal() == 0.0d, "Unexpected value for third real " + array[2].AsReal());

            obj = LLSDParser.DeserializeNotation(testTwo);
            Assert.IsInstanceOfType(typeof(LLSDArray), obj, "Expected LLSDArray, got " + obj.GetType().ToString());
            array = (LLSDArray)obj;
            Assert.IsTrue(array.Count == 2, "Expected two contained objects, got " + array.Count);
            Assert.IsTrue(array[1].AsReal() == 0.0d, "Unexpected value for real " + array[1].AsReal());
            obj = array[0];
            Assert.IsInstanceOfType(typeof(LLSDArray), obj, "Expected ArrayList, got " + obj.GetType().ToString());
            array = (LLSDArray)obj;
            Assert.IsTrue(array[0].AsReal() == 1.0d && array[1].AsReal() == 1.0d && array[2].AsReal() == 1.0d,
                "Unexpected value(s) for nested array: " + array[0].AsReal() + ", " + array[1].AsReal() + ", " +
                array[2].AsReal());

            obj = LLSDParser.DeserializeNotation(testThree);
            Assert.IsInstanceOfType(typeof(LLSDMap), obj, "Expected LLSDMap, got " + obj.GetType().ToString());
            LLSDMap hashtable = (LLSDMap)obj;
            Assert.IsTrue(hashtable.Count == 3, "Expected three contained objects, got " + hashtable.Count);
            Assert.IsInstanceOfType(typeof(LLSDArray), hashtable["region_handle"]);
            Assert.IsTrue(((LLSDArray)hashtable["region_handle"]).Count == 2);
            Assert.IsInstanceOfType(typeof(LLSDArray), hashtable["position"]);
            Assert.IsTrue(((LLSDArray)hashtable["position"]).Count == 3);
            Assert.IsInstanceOfType(typeof(LLSDArray), hashtable["look_at"]);
            Assert.IsTrue(((LLSDArray)hashtable["look_at"]).Count == 3);
        }
    }
}
