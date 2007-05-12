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
            string startLoc = NetworkManager.StartLocation("Ahern", 128, 128, 32);
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

            Assert.AreEqual("ahern", Client.Network.CurrentSim.Name.ToLower(), "Logged in to sim " + 
                Client.Network.CurrentSim.Name + " instead of Ahern");
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

            Assert.IsTrue(CurrentRegionHandle == AhernRegionHandle, "Current region is " +
                CurrentRegionHandle + " when we were expecting " + AhernRegionHandle + ", possible endian issue");
        }

        [Test]
        public void Teleport()
        {
            Assert.IsTrue(Client.Self.Teleport(MorrisRegionHandle, new LLVector3(128, 128, 32)),
                "Teleport to Morris failed");

            // Assert that we really did make it to our scheduled destination
            Assert.AreEqual("morris", Client.Network.CurrentSim.Name.ToLower(),
                "Expected to teleport to Morris, ended up in " + Client.Network.CurrentSim.Name +
                ". Possibly region full or offline?");

            ///////////////////////////////////////////////////////////////////
            // TODO: Add a local region teleport
            ///////////////////////////////////////////////////////////////////

            Assert.IsTrue(Client.Self.Teleport(AhernRegionHandle, new LLVector3(128, 128, 32)), 
                "Teleport to Ahern failed");

            // Assert that we really did make it to our scheduled destination
            Assert.AreEqual("ahern", Client.Network.CurrentSim.Name.ToLower(),
                "Expected to teleport to Ahern, ended up in " + Client.Network.CurrentSim.Name +
                ". Possibly region full or offline?");
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
