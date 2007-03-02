using System;
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
            // Comparison
            LLUUID a = new LLUUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0);
            LLUUID b = new LLUUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0);

            Assert.IsTrue(a == b, "LLUUID comparison operator failed, " + a.ToString() + " should equal " + 
                b.ToString());

            // From string
            a = new LLUUID(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
                0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0);
            b = new LLUUID("00010203-0405-0607-0809-0a0b0c0d0e0f");

            Assert.IsTrue(a == b, "LLUUID hyphenated string constructor failed, should have " + a.ToString() + 
                " but we got " + b.ToString());

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
    }
}
