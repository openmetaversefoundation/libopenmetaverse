using System;
using System.Collections.Generic;
using System.Net;
using libsecondlife;
using libsecondlife.Packets;
using NUnit.Framework;

namespace libsecondlife.Tests
{
    [TestFixture]
    public class NetworkTests : Assert
    {
        SecondLife Client;
        ulong CurrentRegionHandle = 0;
        ulong AhernRegionHandle = 1096213093149184;
        bool DetectedObject = false;

        [SetUp]
        public void Init()
        {
            Client = new SecondLife();

            //string startLoc = NetworkManager.StartLocation("ahern", 128, 128, 32);

            // Register callbacks
            Client.Network.RegisterCallback(PacketType.ObjectUpdate, new NetworkManager.PacketCallback(ObjectUpdateHandler));

            bool result = Client.Network.Login("Testing", "Anvil", "testinganvil", "Unit Test Framework", //startLoc,
                "contact@libsecondlife.org"); //, false);

            Assert.IsTrue(result, "Login failed for Testing Anvil: " + Client.Network.LoginError);

            int start = Environment.TickCount;
            while (Client.Network.CurrentSim.Region.Name == "")
            {
                if (Environment.TickCount - start > 5000)
                {
                    Assert.Fail("Timeout waiting for a RegionHandshake packet");
                }
            }

            Assert.AreEqual(Client.Network.CurrentSim.Region.Name.ToLower(), "ahern", "Logged in to sim " + 
                Client.Network.CurrentSim.Region.Name + " instead of Ahern");
        }

        [Test]
        public void DetectObjects()
        {
            int start = Environment.TickCount;
            while (!DetectedObject)
            {
                if (Environment.TickCount - start > 5000)
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
                if (Environment.TickCount - start > 5000)
                {
                    Assert.Fail("Timeout waiting for an ObjectUpdate packet");
                }
            }

            Assert.IsTrue(CurrentRegionHandle == AhernRegionHandle, "Current region is " +
                CurrentRegionHandle + " when we were expecting " + AhernRegionHandle + ", possible endian issue");
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
            Client.Network.Logout();
            Client = null;
        }
    }
}
