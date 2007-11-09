using System;
using System.Collections.Generic;
using libsecondlife;
using libsecondlife.Packets;
using NUnit.Framework;

namespace libsecondlife.Tests
{
    [TestFixture]
    public class PacketTests : Assert
    {
        [Test]
        public void HeaderFlags()
        {
            TestMessagePacket packet = new TestMessagePacket();

            packet.Header.AppendedAcks = false;
            packet.Header.Reliable = false;
            packet.Header.Resent = false;
            packet.Header.Zerocoded = false;

            Assert.IsFalse(packet.Header.AppendedAcks, "AppendedAcks: Failed to initially set the flag to false");
            Assert.IsFalse(packet.Header.Reliable, "Reliable: Failed to initially set the flag to false");
            Assert.IsFalse(packet.Header.Resent, "Resent: Failed to initially set the flag to false");
            Assert.IsFalse(packet.Header.Zerocoded, "Zerocoded: Failed to initially set the flag to false");

            packet.Header.AppendedAcks = false;
            packet.Header.Reliable = false;
            packet.Header.Resent = false;
            packet.Header.Zerocoded = false;

            Assert.IsFalse(packet.Header.AppendedAcks, "AppendedAcks: Failed to set the flag to false a second time");
            Assert.IsFalse(packet.Header.Reliable, "Reliable: Failed to set the flag to false a second time");
            Assert.IsFalse(packet.Header.Resent, "Resent: Failed to set the flag to false a second time");
            Assert.IsFalse(packet.Header.Zerocoded, "Zerocoded: Failed to set the flag to false a second time");

            packet.Header.AppendedAcks = true;
            packet.Header.Reliable = true;
            packet.Header.Resent = true;
            packet.Header.Zerocoded = true;

            Assert.IsTrue(packet.Header.AppendedAcks, "AppendedAcks: Failed to set the flag to true");
            Assert.IsTrue(packet.Header.Reliable, "Reliable: Failed to set the flag to true");
            Assert.IsTrue(packet.Header.Resent, "Resent: Failed to set the flag to true");
            Assert.IsTrue(packet.Header.Zerocoded, "Zerocoded: Failed to set the flag to true");

            packet.Header.AppendedAcks = true;
            packet.Header.Reliable = true;
            packet.Header.Resent = true;
            packet.Header.Zerocoded = true;

            Assert.IsTrue(packet.Header.AppendedAcks, "AppendedAcks: Failed to set the flag to true a second time");
            Assert.IsTrue(packet.Header.Reliable, "Reliable: Failed to set the flag to true a second time");
            Assert.IsTrue(packet.Header.Resent, "Resent: Failed to set the flag to true a second time");
            Assert.IsTrue(packet.Header.Zerocoded, "Zerocoded: Failed to set the flag to true a second time");

            packet.Header.AppendedAcks = false;
            packet.Header.Reliable = false;
            packet.Header.Resent = false;
            packet.Header.Zerocoded = false;

            Assert.IsFalse(packet.Header.AppendedAcks, "AppendedAcks: Failed to set the flag back to false");
            Assert.IsFalse(packet.Header.Reliable, "Reliable: Failed to set the flag back to false");
            Assert.IsFalse(packet.Header.Resent, "Resent: Failed to set the flag back to false");
            Assert.IsFalse(packet.Header.Zerocoded, "Zerocoded: Failed to set the flag back to false");
        }
    }
}
