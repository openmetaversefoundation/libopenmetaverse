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
using System.Net;
using libsecondlife;
using libsecondlife.Packets;
using libsecondlife.Utilities;
using NUnit.Framework;

namespace libsecondlife.Tests
{
    [TestFixture]
    public class NetworkTests : Assert
    {
        SecondLife Client;

        ulong CurrentRegionHandle = 0;
        ulong AhernRegionHandle = 1096213093149184;
        ulong MorrisRegionHandle = 1096213093149183;
        ulong DoreRegionHandle = 1095113581521408;
        ulong HooperRegionHandle = 1106108697797888;
        bool DetectedObject = false;

        LLUUID LookupKey1 = new LLUUID("25472683cb324516904a6cd0ecabf128");
        //string LookupName1 = "Bot Ringo";

        public NetworkTests()
        {
            Client = new SecondLife();

            // Register callbacks
            Client.Network.RegisterCallback(PacketType.ObjectUpdate, new NetworkManager.PacketCallback(ObjectUpdateHandler));
            //Client.Self.OnTeleport += new MainAvatar.TeleportCallback(OnTeleportHandler);

            // Connect to the grid
            string startLoc = NetworkManager.StartLocation("Hooper", 179, 18, 32);
            Client.Network.Login("Testing", "Anvil", "testinganvil", "Unit Test Framework", startLoc,
                "contact@libsecondlife.org");
        }

        ~NetworkTests()
        {
            Client.Network.Logout();
        }

        [SetUp]
        public void Init()
        {
            Assert.IsTrue(Client.Network.Connected, "Client is not connected to the grid");

            int start = Environment.TickCount;

            Assert.AreEqual("hooper", Client.Network.CurrentSim.Name.ToLower(), "Logged in to sim " + 
                Client.Network.CurrentSim.Name + " instead of hooper");
        }

        [Test]
        public void DetectObjects()
        {
            int start = Environment.TickCount;
            while (!DetectedObject)
            {
                if (Environment.TickCount - start > 10000)
                {
                    Assert.Fail("Timeout waiting for an ObjectUpdate packet");
                }
            }
        }

        [Test]
        public void U64Receive()
        {
            int start = Environment.TickCount;
            while (CurrentRegionHandle == 0)
            {
                if (Environment.TickCount - start > 10000)
                {
                    Assert.Fail("Timeout waiting for an ObjectUpdate packet");
                }
            }

            Assert.IsTrue(CurrentRegionHandle == HooperRegionHandle, "Current region is " +
                CurrentRegionHandle + " when we were expecting " + HooperRegionHandle + ", possible endian issue");
        }

 

        [Test]
        public void Teleport()
        {
            // test in-sim teleports
            Assert.IsTrue(CapsQueueRunning(), "CAPS Event queue is not running in " + Client.Network.CurrentSim.Name);
            string localSimName = Client.Network.CurrentSim.Name;
            Assert.IsTrue(Client.Self.Teleport(Client.Network.CurrentSim.Handle, new LLVector3(121, 13, 41)),
                "Teleport In-Sim Failed " + Client.Network.CurrentSim.Name);

            //// Assert that we really did make it to our scheduled destination
            Assert.AreEqual(localSimName, Client.Network.CurrentSim.Name,
                "Expected to teleport to " + localSimName + ", ended up in " + Client.Network.CurrentSim.Name +
                ". Possibly region full or offline?");

            Assert.IsTrue(CapsQueueRunning(), "CAPS Event queue is not running in " + Client.Network.CurrentSim.Name);
            Assert.IsTrue(Client.Self.Teleport(DoreRegionHandle, new LLVector3(128, 128, 32)),
                "Teleport to Dore failed");

            // Assert that we really did make it to our scheduled destination
            Assert.AreEqual("dore", Client.Network.CurrentSim.Name.ToLower(),
                "Expected to teleport to Dore, ended up in " + Client.Network.CurrentSim.Name +
                ". Possibly region full or offline?");

            Assert.IsTrue(CapsQueueRunning(), "CAPS Event queue is not running in " + Client.Network.CurrentSim.Name);
            Assert.IsTrue(Client.Self.Teleport(HooperRegionHandle, new LLVector3(179, 18, 32)),
                "Teleport to Hooper failed");

            // Assert that we really did make it to our scheduled destination
            Assert.AreEqual("hooper", Client.Network.CurrentSim.Name.ToLower(),
                "Expected to teleport to Hooper, ended up in " + Client.Network.CurrentSim.Name +
                ". Possibly region full or offline?");

        }

        [Test]
        public void CapsQueue()
        {
            Assert.IsTrue(CapsQueueRunning(), "CAPS Event Queue is not running and failed to start");
        }

        public bool CapsQueueRunning()
        {
            if (Client.Network.CurrentSim.Caps.IsEventQueueRunning)
                return true;

            bool Success = false;
            // make sure caps event queue is running
            System.Threading.AutoResetEvent waitforCAPS = new System.Threading.AutoResetEvent(false);
            NetworkManager.EventQueueRunningCallback capsRunning = delegate(Simulator sim)
            {
                waitforCAPS.Set();
            };

            Client.Network.OnEventQueueRunning += capsRunning;
            if (waitforCAPS.WaitOne(10000, false))
            {
                Success = true;
            }
            else
            {
                Success = false;
                Assert.Fail("Timeout waiting for event Queue to startup");
            }
            Client.Network.OnEventQueueRunning -= capsRunning;
            return Success;
        }

        private void ObjectUpdateHandler(Packet packet, Simulator sim)
        {
            ObjectUpdatePacket update = (ObjectUpdatePacket)packet;

            DetectedObject = true;
            CurrentRegionHandle = update.RegionData.RegionHandle;
        }

        [TearDown]
        public void Shutdown()
        {
        }
    }
}
