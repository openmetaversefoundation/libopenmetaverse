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
