/*
 * Copyright (c) 2007-2009, openmetaverse.org
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
using System.Net;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Messages.Linden;
using NUnit.Framework;

namespace OpenMetaverse.Tests
{
    /// <summary>
    /// These unit tests specifically test the Message class can serialize and deserialize its own data properly
    /// a passed test does not necessarily indicate the formatting is correct in the resulting OSD to be handled
    /// by the simulator.
    /// </summary>
    [TestFixture]
    public class MessageTests : Assert
    {
        private Uri testURI = new Uri("https://sim3187.agni.lindenlab.com:12043/cap/6028fc44-c1e5-80a1-f902-19bde114458b");
        private IPAddress testIP = IPAddress.Parse("127.0.0.1");
        private ulong testHandle = 1106108697797888;

        [Test]
        public void AgentGroupDataUpdateMessage()
        {
            AgentGroupDataUpdateMessage s = new AgentGroupDataUpdateMessage();
            s.AgentID = UUID.Random();



            AgentGroupDataUpdateMessage.GroupData[] blocks = new AgentGroupDataUpdateMessage.GroupData[2];
            AgentGroupDataUpdateMessage.GroupData g1 = new AgentGroupDataUpdateMessage.GroupData();

            g1.AcceptNotices = false;
            g1.Contribution = 1024;
            g1.GroupID = UUID.Random();
            g1.GroupInsigniaID = UUID.Random();
            g1.GroupName = "Group Name Test 1";
            g1.GroupPowers = GroupPowers.Accountable | GroupPowers.AllowLandmark | GroupPowers.AllowSetHome;
            g1.ListInProfile = false;
            blocks[0] = g1;

            AgentGroupDataUpdateMessage.GroupData g2 = new AgentGroupDataUpdateMessage.GroupData();
            g2.AcceptNotices = false;
            g2.Contribution = 16;
            g2.GroupID = UUID.Random();
            g2.GroupInsigniaID = UUID.Random();
            g2.GroupName = "Group Name Test 2";
            g2.GroupPowers = GroupPowers.ChangeActions | GroupPowers.DeedObject;
            g2.ListInProfile = true;
            blocks[1] = g2;

            s.GroupDataBlock = blocks;

            OSDMap map = s.Serialize();

            AgentGroupDataUpdateMessage t = new AgentGroupDataUpdateMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.AgentID, t.AgentID);

            for (int i = 0; i < t.GroupDataBlock.Length; i++)
            {
                Assert.AreEqual(s.GroupDataBlock[i].AcceptNotices, t.GroupDataBlock[i].AcceptNotices);
                Assert.AreEqual(s.GroupDataBlock[i].Contribution, t.GroupDataBlock[i].Contribution);
                Assert.AreEqual(s.GroupDataBlock[i].GroupID, t.GroupDataBlock[i].GroupID);
                Assert.AreEqual(s.GroupDataBlock[i].GroupInsigniaID, t.GroupDataBlock[i].GroupInsigniaID);
                Assert.AreEqual(s.GroupDataBlock[i].GroupName, t.GroupDataBlock[i].GroupName);
                Assert.AreEqual(s.GroupDataBlock[i].GroupPowers, t.GroupDataBlock[i].GroupPowers);
                Assert.AreEqual(s.GroupDataBlock[i].ListInProfile, t.GroupDataBlock[i].ListInProfile);
}
        }

        [Test]
        public void TeleportFinishMessage()
        {
            TeleportFinishMessage s = new TeleportFinishMessage();
            s.AgentID = UUID.Random();
            s.Flags = TeleportFlags.ViaLocation | TeleportFlags.IsFlying;
            s.IP = testIP;
            s.LocationID = 32767;
            s.Port = 3000;
            s.RegionHandle = testHandle;
            s.SeedCapability = testURI;
            s.SimAccess = SimAccess.Mature;

            OSDMap map = s.Serialize();

            TeleportFinishMessage t = new TeleportFinishMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.AgentID, t.AgentID);
            Assert.AreEqual(s.Flags, t.Flags);
            Assert.AreEqual(s.IP, t.IP);
            Assert.AreEqual(s.LocationID, t.LocationID);
            Assert.AreEqual(s.Port, t.Port);
            Assert.AreEqual(s.RegionHandle, t.RegionHandle);
            Assert.AreEqual(s.SeedCapability, t.SeedCapability);
            Assert.AreEqual(s.SimAccess, t.SimAccess);
        }

        [Test]
        public void EstablishAgentCommunicationMessage()
        {
            EstablishAgentCommunicationMessage s = new EstablishAgentCommunicationMessage();
            s.Address = testIP;
            s.AgentID = UUID.Random();
            s.Port = 3000;
            s.SeedCapability = testURI;

            OSDMap map = s.Serialize();

            EstablishAgentCommunicationMessage t = new EstablishAgentCommunicationMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.Address, t.Address);
            Assert.AreEqual(s.AgentID, t.AgentID);
            Assert.AreEqual(s.Port, t.Port);
            Assert.AreEqual(s.SeedCapability, t.SeedCapability);
        }

        [Test]
        public void ParcelObjectOwnersMessage()
        {
            ParcelObjectOwnersReplyMessage s = new ParcelObjectOwnersReplyMessage();
            s.PrimOwnersBlock = new ParcelObjectOwnersReplyMessage.PrimOwner[2];

            ParcelObjectOwnersReplyMessage.PrimOwner obj = new ParcelObjectOwnersReplyMessage.PrimOwner();
            obj.OwnerID = UUID.Random();
            obj.Count = 10;
            obj.IsGroupOwned = true;
            obj.OnlineStatus = false;
            obj.TimeStamp = new DateTime(2010, 4, 13, 7, 19, 43);
            s.PrimOwnersBlock[0] = obj;

            ParcelObjectOwnersReplyMessage.PrimOwner obj1 = new ParcelObjectOwnersReplyMessage.PrimOwner();
            obj1.OwnerID = UUID.Random();
            obj1.Count = 0;
            obj1.IsGroupOwned = false;
            obj1.OnlineStatus = false;
            obj1.TimeStamp = new DateTime(1991, 1, 31, 3, 13, 31);
            s.PrimOwnersBlock[1] = obj1;

            OSDMap map = s.Serialize();

            ParcelObjectOwnersReplyMessage t = new ParcelObjectOwnersReplyMessage();
            t.Deserialize(map);

            for (int i = 0; i < t.PrimOwnersBlock.Length; i++)
            {
                Assert.AreEqual(s.PrimOwnersBlock[i].Count, t.PrimOwnersBlock[i].Count);
                Assert.AreEqual(s.PrimOwnersBlock[i].IsGroupOwned, t.PrimOwnersBlock[i].IsGroupOwned);
                Assert.AreEqual(s.PrimOwnersBlock[i].OnlineStatus, t.PrimOwnersBlock[i].OnlineStatus);
                Assert.AreEqual(s.PrimOwnersBlock[i].OwnerID, t.PrimOwnersBlock[i].OwnerID);
                Assert.AreEqual(s.PrimOwnersBlock[i].TimeStamp, t.PrimOwnersBlock[i].TimeStamp);
            }
        }

        [Test]
        public void ChatterBoxInvitationMessage()
        {
            ChatterBoxInvitationMessage s = new ChatterBoxInvitationMessage();
            s.BinaryBucket = Utils.EmptyBytes;
            s.Dialog = InstantMessageDialog.InventoryOffered;
            s.FromAgentID = UUID.Random();
            s.FromAgentName = "Prokofy Neva";
            s.GroupIM = false;
            s.IMSessionID = s.FromAgentID ^ UUID.Random();
            s.Message = "Test Test Test";
            s.Offline = InstantMessageOnline.Online;
            s.ParentEstateID = 1;
            s.Position = Vector3.One;
            s.RegionID = UUID.Random();
            s.Timestamp = DateTime.UtcNow;
            s.ToAgentID = UUID.Random();

            OSDMap map = s.Serialize();

            ChatterBoxInvitationMessage t = new ChatterBoxInvitationMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.BinaryBucket, t.BinaryBucket);
            Assert.AreEqual(s.Dialog, t.Dialog);
            Assert.AreEqual(s.FromAgentID, t.FromAgentID);
            Assert.AreEqual(s.FromAgentName, t.FromAgentName);
            Assert.AreEqual(s.GroupIM, t.GroupIM);
            Assert.AreEqual(s.IMSessionID, t.IMSessionID);
            Assert.AreEqual(s.Message, t.Message);
            Assert.AreEqual(s.Offline, t.Offline);
            Assert.AreEqual(s.ParentEstateID, t.ParentEstateID);
            Assert.AreEqual(s.Position, t.Position);
            Assert.AreEqual(s.RegionID, t.RegionID);
            Assert.AreEqual(s.Timestamp, t.Timestamp);
            Assert.AreEqual(s.ToAgentID, t.ToAgentID);
        }

        [Test]
        public void ChatterboxSessionEventReplyMessage()
        {
            ChatterboxSessionEventReplyMessage s = new ChatterboxSessionEventReplyMessage();
            s.SessionID = UUID.Random();
            s.Success = true;

            OSDMap map = s.Serialize();

            ChatterboxSessionEventReplyMessage t = new ChatterboxSessionEventReplyMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.SessionID, t.SessionID);
            Assert.AreEqual(s.Success, t.Success);
        }

        [Test]
        public void ChatterBoxSessionStartReplyMessage()
        {
            ChatterBoxSessionStartReplyMessage s = new ChatterBoxSessionStartReplyMessage();
            s.ModeratedVoice = true;
            s.SessionID = UUID.Random();
            s.SessionName = "Test Session";
            s.Success = true;
            s.TempSessionID = UUID.Random();
            s.Type = 1;
            s.VoiceEnabled = true;

            OSDMap map = s.Serialize();

            ChatterBoxSessionStartReplyMessage t = new ChatterBoxSessionStartReplyMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.ModeratedVoice, t.ModeratedVoice);
            Assert.AreEqual(s.SessionID, t.SessionID);
            Assert.AreEqual(s.SessionName, t.SessionName);
            Assert.AreEqual(s.Success, t.Success);
            Assert.AreEqual(s.TempSessionID, t.TempSessionID);
            Assert.AreEqual(s.Type, t.Type);
            Assert.AreEqual(s.VoiceEnabled, t.VoiceEnabled);
        }

        [Test]
        public void ChatterBoxSessionAgentListUpdatesMessage()
        {
            ChatterBoxSessionAgentListUpdatesMessage s = new ChatterBoxSessionAgentListUpdatesMessage();
            s.SessionID = UUID.Random();
            s.Updates = new ChatterBoxSessionAgentListUpdatesMessage.AgentUpdatesBlock[1];

            ChatterBoxSessionAgentListUpdatesMessage.AgentUpdatesBlock block1 = new ChatterBoxSessionAgentListUpdatesMessage.AgentUpdatesBlock();
            block1.AgentID = UUID.Random();
            block1.CanVoiceChat = true;
            block1.IsModerator = true;
            block1.MuteText = true;
            block1.MuteVoice = true;
            block1.Transition = "ENTER";

            ChatterBoxSessionAgentListUpdatesMessage.AgentUpdatesBlock block2 = new ChatterBoxSessionAgentListUpdatesMessage.AgentUpdatesBlock();
            block2.AgentID = UUID.Random();
            block2.CanVoiceChat = true;
            block2.IsModerator = true;
            block2.MuteText = true;
            block2.MuteVoice = true;
            block2.Transition = "LEAVE";

            s.Updates[0] = block1;
           // s.Updates[1] = block2;

            OSDMap map = s.Serialize();

            ChatterBoxSessionAgentListUpdatesMessage t = new ChatterBoxSessionAgentListUpdatesMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.Message, t.Message);
            Assert.AreEqual(s.SessionID, t.SessionID);
            for (int i = 0; i < t.Updates.Length; i++)
            {
                Assert.AreEqual(s.Updates[i].AgentID, t.Updates[i].AgentID);
                Assert.AreEqual(s.Updates[i].CanVoiceChat, t.Updates[i].CanVoiceChat);
                Assert.AreEqual(s.Updates[i].IsModerator, t.Updates[i].IsModerator);
                Assert.AreEqual(s.Updates[i].MuteText, t.Updates[i].MuteText);
                Assert.AreEqual(s.Updates[i].MuteVoice, t.Updates[i].MuteVoice);
                Assert.AreEqual(s.Updates[i].Transition, t.Updates[i].Transition);
            }
        }

        [Test]
        public void ViewerStatsMessage()
        {
            ViewerStatsMessage s = new ViewerStatsMessage();

            s.AgentFPS = 45.5f;
            s.AgentsInView = 1;
            s.SystemCPU = "Intel 80286";
            s.StatsDropped = 2;
            s.StatsFailedResends = 3;
            s.SystemGPU = "Vesa VGA+";
            s.SystemGPUClass = 4;
            s.SystemGPUVendor = "China";
            s.SystemGPUVersion = String.Empty;
            s.InCompressedPackets = 5000;
            s.InKbytes = 6000;
            s.InPackets = 22000;
            s.InSavings = 19;
            s.MiscInt1 = 5;
            s.MiscInt2 = 6;
            s.FailuresInvalid = 20;
            s.AgentLanguage = "en";
            s.AgentMemoryUsed = 12878728;
            s.MetersTraveled = 9999123;
            s.object_kbytes = 70001;
            s.FailuresOffCircuit = 201;
            s.SystemOS = "Palm OS 3.1";
            s.OutCompressedPackets = 8000;
            s.OutKbytes = 9000999;
            s.OutPackets = 21000210;
            s.OutSavings = 181;
            s.AgentPing = 135579;
            s.SystemInstalledRam = 4000000;
            s.RegionsVisited = 4579;
            s.FailuresResent = 9;
            s.AgentRuntime = 360023;
            s.FailuresSendPacket = 565;
            s.SessionID = UUID.Random();
            s.SimulatorFPS = 454;
            s.AgentStartTime = new DateTime(1973, 1, 16, 5, 23, 33);
            s.MiscString1 = "Unused String";
            s.texture_kbytes = 9367498382;
            s.AgentVersion = "1";
            s.MiscVersion = 1;
            s.VertexBuffersEnabled = true;
            s.world_kbytes = 232344439;
            
            OSDMap map = s.Serialize();
            ViewerStatsMessage t = new ViewerStatsMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.AgentFPS, t.AgentFPS);
            Assert.AreEqual(s.AgentsInView, t.AgentsInView);
            Assert.AreEqual(s.SystemCPU, t.SystemCPU);
            Assert.AreEqual(s.StatsDropped, t.StatsDropped);
            Assert.AreEqual(s.StatsFailedResends, t.StatsFailedResends);
            Assert.AreEqual(s.SystemGPU, t.SystemGPU);
            Assert.AreEqual(s.SystemGPUClass, t.SystemGPUClass);
            Assert.AreEqual(s.SystemGPUVendor, t.SystemGPUVendor);
            Assert.AreEqual(s.SystemGPUVersion, t.SystemGPUVersion);
            Assert.AreEqual(s.InCompressedPackets, t.InCompressedPackets);
            Assert.AreEqual(s.InKbytes, t.InKbytes);
            Assert.AreEqual(s.InPackets, t.InPackets);
            Assert.AreEqual(s.InSavings, t.InSavings);
            Assert.AreEqual(s.MiscInt1, t.MiscInt1);
            Assert.AreEqual(s.MiscInt2, t.MiscInt2);
            Assert.AreEqual(s.FailuresInvalid, t.FailuresInvalid);
            Assert.AreEqual(s.AgentLanguage, t.AgentLanguage);
            Assert.AreEqual(s.AgentMemoryUsed, t.AgentMemoryUsed);
            Assert.AreEqual(s.MetersTraveled, t.MetersTraveled);
            Assert.AreEqual(s.object_kbytes, t.object_kbytes);
            Assert.AreEqual(s.FailuresOffCircuit, t.FailuresOffCircuit);
            Assert.AreEqual(s.SystemOS, t.SystemOS);
            Assert.AreEqual(s.OutCompressedPackets, t.OutCompressedPackets);
            Assert.AreEqual(s.OutKbytes, t.OutKbytes);
            Assert.AreEqual(s.OutPackets, t.OutPackets);
            Assert.AreEqual(s.OutSavings, t.OutSavings);
            Assert.AreEqual(s.AgentPing, t.AgentPing);
            Assert.AreEqual(s.SystemInstalledRam, t.SystemInstalledRam);
            Assert.AreEqual(s.RegionsVisited, t.RegionsVisited);
            Assert.AreEqual(s.FailuresResent, t.FailuresResent);
            Assert.AreEqual(s.AgentRuntime, t.AgentRuntime);
            Assert.AreEqual(s.FailuresSendPacket, t.FailuresSendPacket);
            Assert.AreEqual(s.SessionID, t.SessionID);
            Assert.AreEqual(s.SimulatorFPS, t.SimulatorFPS);
            Assert.AreEqual(s.AgentStartTime, t.AgentStartTime);
            Assert.AreEqual(s.MiscString1, t.MiscString1);
            Assert.AreEqual(s.texture_kbytes, t.texture_kbytes);
            Assert.AreEqual(s.AgentVersion, t.AgentVersion);
            Assert.AreEqual(s.MiscVersion, t.MiscVersion);
            Assert.AreEqual(s.VertexBuffersEnabled, t.VertexBuffersEnabled);
            Assert.AreEqual(s.world_kbytes, t.world_kbytes);
                
            
        }

        [Test]
        public void ParcelVoiceInfoRequestMessage()
        {
            ParcelVoiceInfoRequestMessage s = new ParcelVoiceInfoRequestMessage();
            s.SipChannelUri = testURI;
            s.ParcelID = 1;
            s.RegionName = "Hooper";

            OSDMap map = s.Serialize();

            ParcelVoiceInfoRequestMessage t = new ParcelVoiceInfoRequestMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.SipChannelUri, t.SipChannelUri);
            Assert.AreEqual(s.ParcelID, t.ParcelID);
            Assert.AreEqual(s.RegionName, t.RegionName);
        }

        [Test]
        public void ScriptRunningReplyMessage()
        {
            ScriptRunningReplyMessage s = new ScriptRunningReplyMessage();
            s.ItemID = UUID.Random();
            s.Mono = true;
            s.Running = true;
            s.ObjectID = UUID.Random();

            OSDMap map = s.Serialize();

            ScriptRunningReplyMessage t = new ScriptRunningReplyMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.ItemID, t.ItemID);
            Assert.AreEqual(s.message, t.message);
            Assert.AreEqual(s.Mono, t.Mono);
            Assert.AreEqual(s.ObjectID, t.ObjectID);
            Assert.AreEqual(s.Running, t.Running);

        }

        [Test]
        public void MapLayerMessage()
        {

            MapLayerMessage s = new MapLayerMessage();
            s.Flags = 1;


            MapLayerMessage.LayerData[] blocks = new MapLayerMessage.LayerData[2];

            MapLayerMessage.LayerData block = new MapLayerMessage.LayerData();
            block.ImageID = UUID.Random();
            block.Bottom = 1;
            block.Top = 2;
            block.Left = 3;
            block.Right = 4;



            blocks[0] = block;

            block.ImageID = UUID.Random();
            block.Bottom = 5;
            block.Top = 6;
            block.Left = 7;
            block.Right = 9;

            blocks[1] = block;

            s.LayerDataBlocks = blocks;

            OSDMap map = s.Serialize();

            MapLayerMessage t = new MapLayerMessage();

            t.Deserialize(map);

            Assert.AreEqual(s.Flags, t.Flags);


            for (int i = 0; i < s.LayerDataBlocks.Length; i++)
            {
                Assert.AreEqual(s.LayerDataBlocks[i].ImageID, t.LayerDataBlocks[i].ImageID);
                Assert.AreEqual(s.LayerDataBlocks[i].Top, t.LayerDataBlocks[i].Top);
                Assert.AreEqual(s.LayerDataBlocks[i].Left, t.LayerDataBlocks[i].Left);
                Assert.AreEqual(s.LayerDataBlocks[i].Right, t.LayerDataBlocks[i].Right);
                Assert.AreEqual(s.LayerDataBlocks[i].Bottom, t.LayerDataBlocks[i].Bottom);
            }
        }

        [Test] // VARIANT A
        public void ChatSessionRequestStartConference()
        {
            ChatSessionRequestStartConference s = new ChatSessionRequestStartConference();
            s.SessionID = UUID.Random();
            s.AgentsBlock = new UUID[2];
            s.AgentsBlock[0] = UUID.Random();
            s.AgentsBlock[0] = UUID.Random();

            OSDMap map = s.Serialize();

            ChatSessionRequestStartConference t = new ChatSessionRequestStartConference();
            t.Deserialize(map);

            Assert.AreEqual(s.SessionID, t.SessionID);
            Assert.AreEqual(s.Method, t.Method);
            for (int i = 0; i < t.AgentsBlock.Length; i++)
            {
                Assert.AreEqual(s.AgentsBlock[i], t.AgentsBlock[i]);
            }
        }

        [Test]
        public void ChatSessionRequestMuteUpdate()
        {
            ChatSessionRequestMuteUpdate s = new ChatSessionRequestMuteUpdate();
            s.AgentID = UUID.Random();
            s.RequestKey = "text";
            s.RequestValue = true;
            s.SessionID = UUID.Random();

            OSDMap map = s.Serialize();

            ChatSessionRequestMuteUpdate t = new ChatSessionRequestMuteUpdate();
            t.Deserialize(map);

            Assert.AreEqual(s.AgentID, t.AgentID);
            Assert.AreEqual(s.Method, t.Method);
            Assert.AreEqual(s.RequestKey, t.RequestKey);
            Assert.AreEqual(s.RequestValue, t.RequestValue);
            Assert.AreEqual(s.SessionID, t.SessionID);
        }

        [Test]
        public void ChatSessionAcceptInvitation()
        {
            ChatSessionAcceptInvitation s = new ChatSessionAcceptInvitation();
            s.SessionID = UUID.Random();

            OSDMap map = s.Serialize();

            ChatSessionAcceptInvitation t = new ChatSessionAcceptInvitation();
            t.Deserialize(map);

            Assert.AreEqual(s.Method, t.Method);
            Assert.AreEqual(s.SessionID, t.SessionID);
        }

        [Test]
        public void RequiredVoiceVersionMessage()
        {
            RequiredVoiceVersionMessage s = new RequiredVoiceVersionMessage();
            s.MajorVersion = 1;
            s.MinorVersion = 0;
            s.RegionName = "Hooper";

            OSDMap map = s.Serialize();

            RequiredVoiceVersionMessage t = new RequiredVoiceVersionMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.MajorVersion, t.MajorVersion);
            Assert.AreEqual(s.Message, t.Message);
            Assert.AreEqual(s.MinorVersion, t.MinorVersion);
            Assert.AreEqual(s.RegionName, t.RegionName);
        }

        [Test]
        public void CopyInventoryFromNotecardMessage()
        {
            CopyInventoryFromNotecardMessage s = new CopyInventoryFromNotecardMessage();
            s.CallbackID = 1;
            s.FolderID = UUID.Random();
            s.ItemID = UUID.Random();
            s.NotecardID = UUID.Random();
            s.ObjectID = UUID.Random();

            OSDMap map = s.Serialize();

            CopyInventoryFromNotecardMessage t = new CopyInventoryFromNotecardMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.CallbackID, t.CallbackID);
            Assert.AreEqual(s.FolderID, t.FolderID);
            Assert.AreEqual(s.ItemID, t.ItemID);
            Assert.AreEqual(s.NotecardID, t.NotecardID);
            Assert.AreEqual(s.ObjectID, t.ObjectID);
        }

        [Test]
        public void ProvisionVoiceAccountRequestMessage()
        {
            ProvisionVoiceAccountRequestMessage s = new ProvisionVoiceAccountRequestMessage();
            s.Username = "username";
            s.Password = "password";

            OSDMap map = s.Serialize();

            ProvisionVoiceAccountRequestMessage t = new ProvisionVoiceAccountRequestMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.Password, t.Password);
            Assert.AreEqual(s.Username, t.Username);
        }

        [Test]
        public void UpdateAgentLanguageMessage()
        {
            UpdateAgentLanguageMessage s = new UpdateAgentLanguageMessage();
            s.Language = "en";
            s.LanguagePublic = false;

            OSDMap map = s.Serialize();

            UpdateAgentLanguageMessage t = new UpdateAgentLanguageMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.Language, t.Language);
            Assert.AreEqual(s.LanguagePublic, t.LanguagePublic);

        }

        [Test]
        public void ParcelPropertiesMessage()
        {
            ParcelPropertiesMessage s = new ParcelPropertiesMessage();
            s.AABBMax = Vector3.Parse("<1,2,3>");
            s.AABBMin = Vector3.Parse("<2,3,1>");
            s.Area = 1024;
            s.AuctionID = uint.MaxValue;
            s.AuthBuyerID = UUID.Random();
            s.Bitmap = Utils.EmptyBytes;
            s.Category = ParcelCategory.Educational;
            s.ClaimDate = new DateTime(2008, 12, 25, 3, 15, 22);
            s.ClaimPrice = 1000;
            s.Desc = "Test Description";
            s.GroupID = UUID.Random();
            s.GroupPrims = 50;
            s.IsGroupOwned = false;
            s.LandingType = LandingType.None;
            s.LocalID = 1;
            s.MaxPrims = 234;
            s.MediaAutoScale = false;
            s.MediaDesc = "Example Media Description";
            s.MediaHeight = 480;
            s.MediaID = UUID.Random();
            s.MediaLoop = false;
            s.MediaType = "text/html";
            s.MediaURL = "http://www.openmetaverse.org"; 
            s.MediaWidth = 640;
            s.MusicURL = "http://scfire-ntc-aa04.stream.aol.com:80/stream/1075"; // Yee Haw
            s.Name = "Test Name";
            s.ObscureMedia = false;
            s.ObscureMusic = false;
            s.OtherCleanTime = 5;
            s.OtherCount = 200;
            s.OtherPrims = 300;
            s.OwnerID = UUID.Random();
            s.OwnerPrims = 0;
            s.ParcelFlags = ParcelFlags.AllowDamage | ParcelFlags.AllowGroupScripts | ParcelFlags.AllowVoiceChat;
            s.ParcelPrimBonus = 0f;
            s.PassHours = 1.5f;
            s.PassPrice = 10;
            s.PublicCount = 20;
            s.RegionDenyAgeUnverified = false;
            s.RegionDenyAnonymous = false;
            s.RegionPushOverride = true;
            s.RentPrice = 0;
            s.RequestResult = ParcelResult.Single;
            s.SalePrice = 9999;
            s.SelectedPrims = 1;
            s.SelfCount = 2;
            s.SequenceID = -4000;
            s.SimWideMaxPrims = 937;
            s.SimWideTotalPrims = 117;
            s.SnapSelection = false;
            s.SnapshotID = UUID.Random();
            s.Status = ParcelStatus.Leased;
            s.TotalPrims = 219;
            s.UserLocation = Vector3.Parse("<3,4,5>");
            s.UserLookAt = Vector3.Parse("<5,4,3>");

            OSDMap map = s.Serialize();
            ParcelPropertiesMessage t = new ParcelPropertiesMessage();

            t.Deserialize(map);

            Assert.AreEqual(s.AABBMax, t.AABBMax);
            Assert.AreEqual(s.AABBMin, t.AABBMin);
            Assert.AreEqual(s.Area, t.Area);
            Assert.AreEqual(s.AuctionID, t.AuctionID);
            Assert.AreEqual(s.AuthBuyerID, t.AuthBuyerID);
            Assert.AreEqual(s.Bitmap, t.Bitmap);
            Assert.AreEqual(s.Category, t.Category);
            Assert.AreEqual(s.ClaimDate, t.ClaimDate);
            Assert.AreEqual(s.ClaimPrice, t.ClaimPrice);
            Assert.AreEqual(s.Desc, t.Desc);
            Assert.AreEqual(s.GroupID, t.GroupID);
            Assert.AreEqual(s.GroupPrims, t.GroupPrims);
            Assert.AreEqual(s.IsGroupOwned, t.IsGroupOwned);
            Assert.AreEqual(s.LandingType, t.LandingType);
            Assert.AreEqual(s.LocalID, t.LocalID);
            Assert.AreEqual(s.MaxPrims, t.MaxPrims);
            Assert.AreEqual(s.MediaAutoScale, t.MediaAutoScale);
            Assert.AreEqual(s.MediaDesc, t.MediaDesc);
            Assert.AreEqual(s.MediaHeight, t.MediaHeight);
            Assert.AreEqual(s.MediaID, t.MediaID);
            Assert.AreEqual(s.MediaLoop, t.MediaLoop);
            Assert.AreEqual(s.MediaType, t.MediaType);
            Assert.AreEqual(s.MediaURL, t.MediaURL);
            Assert.AreEqual(s.MediaWidth, t.MediaWidth);
            Assert.AreEqual(s.MusicURL, t.MusicURL);
            Assert.AreEqual(s.Name, t.Name);
            Assert.AreEqual(s.ObscureMedia, t.ObscureMedia);
            Assert.AreEqual(s.ObscureMusic, t.ObscureMusic);
            Assert.AreEqual(s.OtherCleanTime, t.OtherCleanTime);
            Assert.AreEqual(s.OtherCount, t.OtherCount);
            Assert.AreEqual(s.OtherPrims, t.OtherPrims);
            Assert.AreEqual(s.OwnerID, t.OwnerID);
            Assert.AreEqual(s.OwnerPrims, t.OwnerPrims);
            Assert.AreEqual(s.ParcelFlags, t.ParcelFlags);
            Assert.AreEqual(s.ParcelPrimBonus, t.ParcelPrimBonus);
            Assert.AreEqual(s.PassHours, t.PassHours);
            Assert.AreEqual(s.PassPrice, t.PassPrice);
            Assert.AreEqual(s.PublicCount, t.PublicCount);
            Assert.AreEqual(s.RegionDenyAgeUnverified, t.RegionDenyAgeUnverified);
            Assert.AreEqual(s.RegionDenyAnonymous, t.RegionDenyAnonymous);
            Assert.AreEqual(s.RegionPushOverride, t.RegionPushOverride);
            Assert.AreEqual(s.RentPrice, t.RentPrice);
            Assert.AreEqual(s.RequestResult, t.RequestResult);
            Assert.AreEqual(s.SalePrice, t.SalePrice);
            Assert.AreEqual(s.SelectedPrims, t.SelectedPrims);
            Assert.AreEqual(s.SelfCount, t.SelfCount);
            Assert.AreEqual(s.SequenceID, t.SequenceID);
            Assert.AreEqual(s.SimWideMaxPrims, t.SimWideMaxPrims);
            Assert.AreEqual(s.SimWideTotalPrims, t.SimWideTotalPrims);
            Assert.AreEqual(s.SnapSelection, t.SnapSelection);
            Assert.AreEqual(s.SnapshotID, t.SnapshotID);
            Assert.AreEqual(s.Status, t.Status);
            Assert.AreEqual(s.TotalPrims, t.TotalPrims);
            Assert.AreEqual(s.UserLocation, t.UserLocation);
            Assert.AreEqual(s.UserLookAt, t.UserLookAt);
        }

        [Test]
        public void ParcelPropertiesUpdateMessage()
        {
            ParcelPropertiesUpdateMessage s = new ParcelPropertiesUpdateMessage();
            s.AuthBuyerID = UUID.Random();
            s.Category = ParcelCategory.Gaming;
            s.Desc = "Example Description";
            s.GroupID = UUID.Random();
            s.Landing = LandingType.LandingPoint;
            s.LocalID = 160;
            s.MediaAutoScale = true;
            s.MediaDesc = "Example Media Description";
            s.MediaHeight = 600;
            s.MediaID = UUID.Random();
            s.MediaLoop = false;
            s.MediaType = "image/jpeg";
            s.MediaURL = "http://www.openmetaverse.org/test.jpeg";
            s.MediaWidth = 800;
            s.MusicURL = "http://scfire-ntc-aa04.stream.aol.com:80/stream/1075";
            s.Name = "Example Parcel Description";
            s.ObscureMedia = true;
            s.ObscureMusic = true;
            s.ParcelFlags = ParcelFlags.AllowVoiceChat | ParcelFlags.ContributeWithDeed;
            s.PassHours = 5.5f;
            s.PassPrice = 100;
            s.SalePrice = 99;
            s.SnapshotID = UUID.Random();
            s.UserLocation = Vector3.Parse("<128,128,128>");
            s.UserLookAt = Vector3.Parse("<256,256,256>");

            OSDMap map = s.Serialize();

            ParcelPropertiesUpdateMessage t = new ParcelPropertiesUpdateMessage();

            t.Deserialize(map);

            Assert.AreEqual(s.AuthBuyerID, t.AuthBuyerID);
            Assert.AreEqual(s.Category, t.Category);
            Assert.AreEqual(s.Desc, t.Desc);
            Assert.AreEqual(s.GroupID, t.GroupID);
            Assert.AreEqual(s.Landing, t.Landing);
            Assert.AreEqual(s.LocalID, t.LocalID);
            Assert.AreEqual(s.MediaAutoScale, t.MediaAutoScale);
            Assert.AreEqual(s.MediaDesc, t.MediaDesc);
            Assert.AreEqual(s.MediaHeight, t.MediaHeight);
            Assert.AreEqual(s.MediaID, t.MediaID);
            Assert.AreEqual(s.MediaLoop, t.MediaLoop);
            Assert.AreEqual(s.MediaType, t.MediaType);
            Assert.AreEqual(s.MediaURL, t.MediaURL);
            Assert.AreEqual(s.MediaWidth, t.MediaWidth);
            Assert.AreEqual(s.MusicURL, t.MusicURL);
            Assert.AreEqual(s.Name, t.Name);
            Assert.AreEqual(s.ObscureMedia, t.ObscureMedia);
            Assert.AreEqual(s.ObscureMusic, t.ObscureMusic);
            Assert.AreEqual(s.ParcelFlags, t.ParcelFlags);
            Assert.AreEqual(s.PassHours, t.PassHours);
            Assert.AreEqual(s.PassPrice, t.PassPrice);
            Assert.AreEqual(s.SalePrice, t.SalePrice);
            Assert.AreEqual(s.SnapshotID, t.SnapshotID);
            Assert.AreEqual(s.UserLocation, t.UserLocation);
            Assert.AreEqual(s.UserLookAt, t.UserLookAt);
        }
        [Test]
        public void EnableSimulatorMessage()
        {
            EnableSimulatorMessage s = new EnableSimulatorMessage();
            s.Simulators = new EnableSimulatorMessage.SimulatorInfoBlock[2];

            EnableSimulatorMessage.SimulatorInfoBlock block1 = new EnableSimulatorMessage.SimulatorInfoBlock();
            block1.IP = testIP;
            block1.Port = 3000;
            block1.RegionHandle = testHandle;
            s.Simulators[0] = block1;

            EnableSimulatorMessage.SimulatorInfoBlock block2 = new EnableSimulatorMessage.SimulatorInfoBlock();
            block2.IP = testIP;
            block2.Port = 3001;
            block2.RegionHandle = testHandle;
            s.Simulators[1] = block2;

            OSDMap map = s.Serialize();

            EnableSimulatorMessage t = new EnableSimulatorMessage();
            t.Deserialize(map);

            for (int i = 0; i < t.Simulators.Length; i++)
            {
                Assert.AreEqual(s.Simulators[i].IP, t.Simulators[i].IP);
                Assert.AreEqual(s.Simulators[i].Port, t.Simulators[i].Port);
                Assert.AreEqual(s.Simulators[i].RegionHandle, t.Simulators[i].RegionHandle);
            }
        }

        [Test]
        public void RemoteParcelRequestMessage()
        {
            RemoteParcelRequestMessage s = new RemoteParcelRequestMessage();
            s.ParcelID = UUID.Random();
            OSDMap map = s.Serialize();

            RemoteParcelRequestMessage t = new RemoteParcelRequestMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.ParcelID, t.ParcelID);
        }

        [Test]
        public void UpdateScriptTaskMessage()
        {
            UpdateScriptTaskMessage s = new UpdateScriptTaskMessage();
            s.TaskID = UUID.Random();
            s.Target = "mono";
            s.ScriptRunning = true;
            s.ItemID = UUID.Random();

            OSDMap map = s.Serialize();
            UpdateScriptTaskMessage t = new UpdateScriptTaskMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.ItemID, t.ItemID);
            Assert.AreEqual(s.ScriptRunning, t.ScriptRunning);
            Assert.AreEqual(s.Target, t.Target);
            Assert.AreEqual(s.TaskID, t.TaskID);
        }

        [Test]
        public void UpdateScriptAgentMessage()
        {
            UpdateScriptAgentMessage s = new UpdateScriptAgentMessage();
            s.ItemID = UUID.Random();
            s.Target = "lsl2";

            OSDMap map = s.Serialize();

            UpdateScriptAgentMessage t = new UpdateScriptAgentMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.ItemID, t.ItemID);
            Assert.AreEqual(s.Target, t.Target);
        }

        [Test]
        public void SendPostcardMessage()
        {
            SendPostcardMessage s = new SendPostcardMessage();
            s.FromEmail = "contact@openmetaverse.org";
            s.FromName = "Jim Radford";
            s.GlobalPosition = Vector3.One;
            s.Message = "Hello, How are you today?";
            s.Subject = "Postcard from the edge";
            s.ToEmail = "test1@example.com";

            OSDMap map = s.Serialize();

            SendPostcardMessage t = new SendPostcardMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.FromEmail, t.FromEmail);
            Assert.AreEqual(s.FromName, t.FromName);
            Assert.AreEqual(s.GlobalPosition, t.GlobalPosition);
            Assert.AreEqual(s.Message, t.Message);
            Assert.AreEqual(s.Subject, t.Subject);
            Assert.AreEqual(s.ToEmail, t.ToEmail);
        }

        [Test]
        public void UpdateNotecardAgentInventoryMessage()
        {
            UpdateNotecardAgentInventoryMessage s = new UpdateNotecardAgentInventoryMessage();
            s.ItemID = UUID.Random();

            OSDMap map = s.Serialize();

            UpdateNotecardAgentInventoryMessage t = new UpdateNotecardAgentInventoryMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.ItemID, t.ItemID);
        }

        [Test]
        public void LandStatReplyMessage()
        {
            LandStatReplyMessage s = new LandStatReplyMessage();
            s.ReporType = 22;
            s.RequestFlags = 44;
            s.TotalObjectCount = 2;
            s.ReportDataBlocks = new LandStatReplyMessage.ReportDataBlock[2];

            LandStatReplyMessage.ReportDataBlock block1 = new LandStatReplyMessage.ReportDataBlock();
            block1.Location = Vector3.One;
            block1.MonoScore = 99;
            block1.OwnerName = "Profoky Neva";
            block1.Score = 10;
            block1.TaskID = UUID.Random();
            block1.TaskLocalID = 987341;
            block1.TaskName = "Verbal Flogging";
            block1.TimeStamp = new DateTime(2009, 5, 23, 4, 30, 0);
            s.ReportDataBlocks[0] = block1;

            LandStatReplyMessage.ReportDataBlock block2 = new LandStatReplyMessage.ReportDataBlock();
            block2.Location = Vector3.One;
            block2.MonoScore = 1;
            block2.OwnerName = "Philip Linden";
            block2.Score = 5;
            block2.TaskID = UUID.Random();
            block2.TaskLocalID = 987342;
            block2.TaskName = "Happy Ant";
            block2.TimeStamp = new DateTime(2008, 4, 22, 3, 29, 55);
            s.ReportDataBlocks[1] = block2;

            OSDMap map = s.Serialize();

            LandStatReplyMessage t = new LandStatReplyMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.ReporType, t.ReporType);
            Assert.AreEqual(s.RequestFlags, t.RequestFlags);
            Assert.AreEqual(s.TotalObjectCount, t.TotalObjectCount);

            for (int i = 0; i < t.ReportDataBlocks.Length; i++)
            {
                Assert.AreEqual(s.ReportDataBlocks[i].Location, t.ReportDataBlocks[i].Location);
                Assert.AreEqual(s.ReportDataBlocks[i].MonoScore, t.ReportDataBlocks[i].MonoScore);
                Assert.AreEqual(s.ReportDataBlocks[i].OwnerName, t.ReportDataBlocks[i].OwnerName);
                Assert.AreEqual(s.ReportDataBlocks[i].Score, t.ReportDataBlocks[i].Score);
                Assert.AreEqual(s.ReportDataBlocks[i].TaskID, t.ReportDataBlocks[i].TaskID);
                Assert.AreEqual(s.ReportDataBlocks[i].TaskLocalID, t.ReportDataBlocks[i].TaskLocalID);
                Assert.AreEqual(s.ReportDataBlocks[i].TaskName, t.ReportDataBlocks[i].TaskName);
                Assert.AreEqual(s.ReportDataBlocks[i].TimeStamp, t.ReportDataBlocks[i].TimeStamp);
            }
        }

        [Test]
        public void TelportFailedMessage()
        {
            TeleportFailedMessage s = new TeleportFailedMessage();
            s.AgentID = UUID.Random();
            s.MessageKey = "Key";
            s.Reason = "Unable To Teleport for some unspecified reason";
            s.ExtraParams = String.Empty;

            OSDMap map = s.Serialize();

            TeleportFailedMessage t = new TeleportFailedMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.AgentID, t.AgentID);
            Assert.AreEqual(s.ExtraParams, t.ExtraParams);
            Assert.AreEqual(s.MessageKey, t.MessageKey);
            Assert.AreEqual(s.Reason, t.Reason);

        }
    }
}

