using System;
using System.Collections;
using System.Net;
using libsecondlife;
using libsecondlife.Packets;
using NUnit.Framework;

namespace libsecondlife.Tests
{
    [TestFixture]
    public class EndianTests : Assert
    {
        SecondLife Client = null;
        DebugServer Server = null;
        Packet CurrentPacket = null;
        bool NetworkFinished = false;

        [SetUp]
        public void Init()
        {
            Client = new SecondLife();
            Client.Network.AgentID = LLUUID.GenerateUUID();
            Client.Network.SessionID = LLUUID.GenerateUUID();

            Server = new DebugServer("keywords.txt", "message_template.msg", 8338);
            Assert.IsTrue(Server.Initialized, "Failed to initialize the server, couldn't bind to port 8338?");

            Simulator debugSim = Client.Network.Connect(IPAddress.Loopback, 8338, 1, true);
            Assert.IsNotNull(debugSim, "Failed to connect to the debugging simulator");

            Client.Network.RegisterCallback(PacketType.SimulatorAssign, new PacketCallback(SimulatorAssignHandler));
        }

        [Test]
        public void U8Receive()
        {
            CurrentPacket = null;
            NetworkFinished = false;

            // 2. Instruct the server to send a SimulatorAssign to the client with some fixed values

            int start = Environment.TickCount;

            while (!NetworkFinished && Environment.TickCount - start < 5000)
            {
                System.Threading.Thread.Sleep(0);
            }

            // 5. Parse the Packet and run our assertion(s)
            Assert.IsNotNull(CurrentPacket, "Never received the packet");
            Assert.IsTrue(true);
        }

        [Test]
        public void S8Receive()
        {
            ;
        }

        [Test]
        public void U16Receive()
        {
            ;
        }

        [Test]
        public void S16Receive()
        {
            ;
        }

        private void SimulatorAssignHandler(Packet packet, Simulator sim)
        {
            CurrentPacket = packet;
            NetworkFinished = true;
        }

        [TearDown]
        public void Shutdown()
        {
            try
            {
                Client.Network.SendPacket(System.Text.Encoding.UTF8.GetBytes("stopserver"));
                Client.Network.Logout();
            }
            catch (NotConnectedException)
            {
                Assert.IsTrue(false, "Logout failed, not connected");
            }

            Client = null;
            Server = null;
        }
    }
}
