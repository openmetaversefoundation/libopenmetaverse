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
    [TestFixture]
    public class MessageTests : Assert
    {
        private Uri testURI = new Uri("https://sim3187.agni.lindenlab.com:12043/cap/6028fc44-c1e5-80a1-f902-19bde114458b");
        private IPAddress testIP = IPAddress.Parse("127.0.0.1");
        private ulong testHandle = 1106108697797888;

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
            ParcelObjectOwnersMessage s = new ParcelObjectOwnersMessage();
            s.DataBlocks = new ParcelObjectOwnersMessage.PrimOwners[2];

            ParcelObjectOwnersMessage.PrimOwners obj = new ParcelObjectOwnersMessage.PrimOwners();
            obj.OwnerID = UUID.Random();
            obj.Count = 10;
            obj.IsGroupOwned = true;
            obj.OnlineStatus = false;
            obj.TimeStamp = DateTime.UtcNow;
            s.DataBlocks[0] = obj;

            ParcelObjectOwnersMessage.PrimOwners obj1 = new ParcelObjectOwnersMessage.PrimOwners();
            obj1.OwnerID = UUID.Random();
            obj1.Count = 0;
            obj1.IsGroupOwned = false;
            obj1.OnlineStatus = false;
            obj1.TimeStamp = DateTime.UtcNow;
            s.DataBlocks[1] = obj1;

            OSDMap map = s.Serialize();

            ParcelObjectOwnersMessage t = new ParcelObjectOwnersMessage();
            t.Deserialize(map);

            for (int i = 0; i < t.DataBlocks.Length; i++)
            {
                Assert.AreEqual(s.DataBlocks[i].Count, t.DataBlocks[i].Count);
                Assert.AreEqual(s.DataBlocks[i].IsGroupOwned, t.DataBlocks[i].IsGroupOwned);
                Assert.AreEqual(s.DataBlocks[i].OnlineStatus, t.DataBlocks[i].OnlineStatus);
                Assert.AreEqual(s.DataBlocks[i].OwnerID, t.DataBlocks[i].OwnerID);
                Assert.AreEqual(s.DataBlocks[i].TimeStamp, t.DataBlocks[i].TimeStamp);
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
        public void ParcelVoiceInfoRequestMessage()
        {
            ParcelVoiceInfoRequestMessage s = new ParcelVoiceInfoRequestMessage();
            s.channel_uri = testURI;
            s.parcel_local_id = 1;
            s.region_name = "Hooper";

            OSDMap map = s.Serialize();

            ParcelVoiceInfoRequestMessage t = new ParcelVoiceInfoRequestMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.channel_uri, t.channel_uri);
            Assert.AreEqual(s.parcel_local_id, t.parcel_local_id);
            Assert.AreEqual(s.region_name, t.region_name);
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

        [Test]
        public void ChatSessionRequestMessage()
        {
            ChatSessionRequestMessage s = new ChatSessionRequestMessage();
            s.Method = "mute update";
            s.RequestKey = "voice";
            s.RequestValue = false;
            s.SessionID = UUID.Random();
            s.AgentID = UUID.Random();

            OSDMap map = s.Serialize();

            ChatSessionRequestMessage t = new ChatSessionRequestMessage();
            t.Deserialize(map);

            Assert.AreEqual(s.AgentID, t.AgentID);
            Assert.AreEqual(s.Method, t.Method);
            Assert.AreEqual(s.RequestKey, t.RequestKey);
            Assert.AreEqual(s.RequestValue, t.RequestValue);
            Assert.AreEqual(s.SessionID, t.SessionID);

            s.RequestKey = "text";
            s.RequestValue = true;
            s.SessionID = UUID.Random();
            s.AgentID = UUID.Random();

            map = s.Serialize();
            t.Deserialize(map);

            Assert.AreEqual(s.AgentID, t.AgentID);
            Assert.AreEqual(s.Method, t.Method);
            Assert.AreEqual(s.RequestKey, t.RequestKey);
            Assert.AreEqual(s.RequestValue, t.RequestValue);
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
    }
}

