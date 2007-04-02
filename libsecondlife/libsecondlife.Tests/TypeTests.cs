using System;
using System.Collections;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;
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
            one = a.ToStringHyphenated();
            two = b.ToStringHyphenated();
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

        [Test]
        public void VectorQuaternionMath()
        {
            ;
        }

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

            object obj = LLSD.ParseTerseLLSD(testOne);
            Assert.IsInstanceOfType(typeof(ArrayList), obj, "Expected ArrayList, got " + obj.GetType().ToString());
            ArrayList array = (ArrayList)obj;
            Assert.IsTrue(array.Count == 3, "Expected three contained objects, got " + array.Count);
            Assert.IsTrue((double)array[0] > 0.9d && (double)array[0] < 1.0d, "Unexpected value for first real " + (double)array[0]);
            Assert.IsTrue((double)array[1] < 0.0d && (double)array[1] > -0.03d, "Unexpected value for second real " + (double)array[1]);
            Assert.IsTrue((double)array[2] == 0.0d, "Unexpected value for third real " + (double)array[2]);

            obj = LLSD.ParseTerseLLSD(testTwo);
            Assert.IsInstanceOfType(typeof(ArrayList), obj, "Expected ArrayList, got " + obj.GetType().ToString());
            array = (ArrayList)obj;
            Assert.IsTrue(array.Count == 2, "Expected two contained objects, got " + array.Count);
            Assert.IsTrue((double)array[1] == 0.0d, "Unexpected value for real " + (double)array[1]);
            obj = array[0];
            Assert.IsInstanceOfType(typeof(ArrayList), obj, "Expected ArrayList, got " + obj.GetType().ToString());
            array = (ArrayList)obj;
            Assert.IsTrue((double)array[0] == 1.0d && (double)array[1] == 1.0d && (double)array[2] == 1.0d,
                "Unexpected value(s) for nested array: " + (double)array[0] + ", " + (double)array[1] + ", " + 
                (double)array[2]);

            obj = LLSD.ParseTerseLLSD(testThree);
            Assert.IsInstanceOfType(typeof(Hashtable), obj, "Expected Hashtable, got " + obj.GetType().ToString());
            Hashtable hashtable = (Hashtable)obj;
            Assert.IsTrue(hashtable.Count == 3, "Expected three contained objects, got " + hashtable.Count);
            Assert.IsInstanceOfType(typeof(ArrayList), hashtable["region_handle"]);
            Assert.IsTrue(((ArrayList)hashtable["region_handle"]).Count == 2);
            Assert.IsInstanceOfType(typeof(ArrayList), hashtable["position"]);
            Assert.IsTrue(((ArrayList)hashtable["position"]).Count == 3);
            Assert.IsInstanceOfType(typeof(ArrayList), hashtable["look_at"]);
            Assert.IsTrue(((ArrayList)hashtable["look_at"]).Count == 3);
        }
    }
}
