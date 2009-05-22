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
using OpenMetaverse;
using OpenMetaverse.Packets;
using NUnit.Framework;

namespace OpenMetaverse.Tests
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

        [Test]
        public void ToBytesMultiple()
        {
            UUID testID = UUID.Random();

            DirPlacesReplyPacket bigPacket = new DirPlacesReplyPacket();
            bigPacket.Header.Zerocoded = false;
            bigPacket.Header.Sequence = 42;
            bigPacket.Header.AppendedAcks = true;
            bigPacket.Header.AckList = new uint[50];
            for (int i = 0; i < bigPacket.Header.AckList.Length; i++) { bigPacket.Header.AckList[i] = (uint)i; }
            bigPacket.AgentData.AgentID = testID;
            bigPacket.QueryData = new DirPlacesReplyPacket.QueryDataBlock[100];
            for (int i = 0; i < bigPacket.QueryData.Length; i++)
            {
                bigPacket.QueryData[i] = new DirPlacesReplyPacket.QueryDataBlock();
                bigPacket.QueryData[i].QueryID = testID;
            }
            bigPacket.QueryReplies = new DirPlacesReplyPacket.QueryRepliesBlock[100];
            for (int i = 0; i < bigPacket.QueryReplies.Length; i++)
            {
                bigPacket.QueryReplies[i] = new DirPlacesReplyPacket.QueryRepliesBlock();
                bigPacket.QueryReplies[i].Auction = (i & 1) == 0;
                bigPacket.QueryReplies[i].Dwell = (float)i;
                bigPacket.QueryReplies[i].ForSale = (i & 1) == 0;
                bigPacket.QueryReplies[i].Name = Utils.StringToBytes("DirPlacesReply Test String");
                bigPacket.QueryReplies[i].ParcelID = testID;
            }
            bigPacket.StatusData = new DirPlacesReplyPacket.StatusDataBlock[100];
            for (int i = 0; i < bigPacket.StatusData.Length; i++)
            {
                bigPacket.StatusData[i] = new DirPlacesReplyPacket.StatusDataBlock();
                bigPacket.StatusData[i].Status = (uint)i;
            }

            byte[][] splitPackets = bigPacket.ToBytesMultiple();

            int queryDataCount = 0;
            int queryRepliesCount = 0;
            int statusDataCount = 0;
            for (int i = 0; i < splitPackets.Length; i++)
            {
                byte[] packetData = splitPackets[i];
                int len = packetData.Length - 1;
                DirPlacesReplyPacket packet = (DirPlacesReplyPacket)Packet.BuildPacket(packetData, ref len, packetData);

                Assert.IsTrue(packet.AgentData.AgentID == bigPacket.AgentData.AgentID);

                for (int j = 0; j < packet.QueryReplies.Length; j++)
                {
                    Assert.IsTrue(packet.QueryReplies[j].Dwell == (float)(queryRepliesCount + j),
                        "Expected Dwell of " + (float)(queryRepliesCount + j) + " but got " + packet.QueryReplies[j].Dwell);
                    Assert.IsTrue(packet.QueryReplies[j].ParcelID == testID);
                }

                queryDataCount += packet.QueryData.Length;
                queryRepliesCount += packet.QueryReplies.Length;
                statusDataCount += packet.StatusData.Length;
            }

            Assert.IsTrue(queryDataCount == bigPacket.QueryData.Length);
            Assert.IsTrue(queryRepliesCount == bigPacket.QueryData.Length);
            Assert.IsTrue(statusDataCount == bigPacket.StatusData.Length);
        }
    }
}
