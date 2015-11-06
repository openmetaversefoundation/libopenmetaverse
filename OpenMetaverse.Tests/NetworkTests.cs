/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
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
using OpenMetaverse;
using OpenMetaverse.Packets;
using NUnit.Framework;

namespace OpenMetaverse.Tests
{
    [TestFixture]
    [Category("Network")]
    public class NetworkTests : Assert
    {
        GridClient Client;

        //ulong CurrentRegionHandle = 0;
        //ulong AhernRegionHandle = 1096213093149184;
        //ulong MorrisRegionHandle = 1096213093149183;
        //ulong DoreRegionHandle = 1095113581521408;
        //ulong HooperRegionHandle = 1106108697797888;
        bool DetectedObject = false;

        public NetworkTests()
        {
            Client = new GridClient();
            Client.Self.Movement.Fly = true;
            // Register callbacks
            Client.Network.RegisterCallback(PacketType.ObjectUpdate, ObjectUpdateHandler);
            //Client.Self.OnTeleport += new MainAvatar.TeleportCallback(OnTeleportHandler)
        }

        [TestFixtureSetUp]
        public void Init()
        {
            Console.Write("Logging in Testing Anvil...");
            // Connect to the grid
            string startLoc = NetworkManager.StartLocation("Hooper", 179, 18, 32);
            Assert.IsTrue(Client.Network.Login("Testing", "Anvil", "testinganvil", "Unit Test Framework", startLoc,
                "contact@openmetaverse.co"), "Client failed to login, reason: " + Client.Network.LoginMessage);
            Console.WriteLine("Done");

            Assert.IsTrue(Client.Network.Connected, "Client is not connected to the grid");

            //int start = Environment.TickCount;

            Assert.AreEqual("hooper", Client.Network.CurrentSim.Name.ToLower(), "Logged in to sim " + 
                Client.Network.CurrentSim.Name + " instead of hooper");
        }

        [Test]
        public void DetectObjects()
        {
            int start = Environment.TickCount;
            while (!DetectedObject)
            {
                if (Environment.TickCount - start > 20000)
                {
                    Assert.Fail("Timeout waiting for an ObjectUpdate packet");
                }
            }
        }

        /*
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
                CurrentRegionHandle + " (" + Client.Network.CurrentSim.Name + ")" + " when we were expecting " + HooperRegionHandle + " (Dore), possible endian issue");
        }
        */
        /*[Test]
        public void Teleport()
        {
            // test in-sim teleports
            Assert.IsTrue(CapsQueueRunning(), "CAPS Event queue is not running in " + Client.Network.CurrentSim.Name);
            string localSimName = Client.Network.CurrentSim.Name;
            Assert.IsTrue(Client.Self.Teleport(Client.Network.CurrentSim.Handle, new Vector3(121, 13, 41)),
                "Teleport In-Sim Failed " + Client.Network.CurrentSim.Name);

            // Assert that we really did make it to our scheduled destination
            Assert.AreEqual(localSimName, Client.Network.CurrentSim.Name,
                "Expected to teleport to " + localSimName + ", ended up in " + Client.Network.CurrentSim.Name +
                ". Possibly region full or offline?");

            Assert.IsTrue(CapsQueueRunning(), "CAPS Event queue is not running in " + Client.Network.CurrentSim.Name);
            Assert.IsTrue(Client.Self.Teleport(DoreRegionHandle, new Vector3(128, 128, 32)),
                "Teleport to Dore failed");

            // Assert that we really did make it to our scheduled destination
            Assert.AreEqual("dore", Client.Network.CurrentSim.Name.ToLower(),
                "Expected to teleport to Dore, ended up in " + Client.Network.CurrentSim.Name +
                ". Possibly region full or offline?");

            Assert.IsTrue(CapsQueueRunning(), "CAPS Event queue is not running in " + Client.Network.CurrentSim.Name);
            Assert.IsTrue(Client.Self.Teleport(HooperRegionHandle, new Vector3(179, 18, 32)),
                "Teleport to Hooper failed");

            // Assert that we really did make it to our scheduled destination
            Assert.AreEqual("hooper", Client.Network.CurrentSim.Name.ToLower(),
                "Expected to teleport to Hooper, ended up in " + Client.Network.CurrentSim.Name +
                ". Possibly region full or offline?");
        }*/

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
            EventHandler<EventQueueRunningEventArgs> capsRunning = delegate(object sender, EventQueueRunningEventArgs e)
            {
                waitforCAPS.Set();
            };            

            Client.Network.EventQueueRunning += capsRunning;
            if (waitforCAPS.WaitOne(10000, false))
            {
                Success = true;
            }
            else
            {
                Success = false;
                Assert.Fail("Timeout waiting for event Queue to startup");
            }
            Client.Network.EventQueueRunning -= capsRunning;
            return Success;
        }

        private void ObjectUpdateHandler(object sender, PacketReceivedEventArgs e)
        {
            //ObjectUpdatePacket update = (ObjectUpdatePacket)packet;

            DetectedObject = true;
            //CurrentRegionHandle = update.RegionData.RegionHandle;
        }

        [TestFixtureTearDown]
        public void Shutdown()
        {
            Console.Write("Logging out...");
            Client.Network.Logout();
            Console.WriteLine("Done");
        }
    }
}
