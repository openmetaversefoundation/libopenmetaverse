using System;
using System.Collections;
using System.Net;
using libsecondlife;
using NUnit.Framework;

namespace libsecondlife.Tests
{
    [TestFixture]
    public class EndianTests : Assert
    {
        SecondLife Client;
        DebugServer Server;

        [SetUp]
        public void Init()
        {
            // Initialize the client
            try
            {
                Client = new SecondLife("keywords.txt", "message_template.msg");
                Client.Network.AgentID = LLUUID.GenerateUUID();
                Client.Network.SessionID = LLUUID.GenerateUUID();
            }
            catch (Exception)
            {
                Assert.IsTrue(false, "Failed to initialize the client, " + 
                    "keywords.txt or message_template.msg missing?");
            }

            Console.WriteLine("Initializing the server");
            Server = new DebugServer("keywords.txt", "message_template.msg", 8338);

            Assert.IsTrue(Server.Initialized, "Failed to initialize the server, couldn't bind to port 8338?");

            // Login with the client to the server
            Simulator sim = Client.Network.Connect(IPAddress.Loopback, 8338, 1, true);

            Assert.IsNotNull(sim, "Failed to connect to the debugging simulator");
        }

        private void StartServer()
        {
            Console.WriteLine("Initializing the server");
            Server = new DebugServer("keywords.txt", "message_template.msg", 8338);
        }

        [Test]
        public void SimpleTest()
        {
            Assert.IsTrue(true);
        }

        [TearDown]
        public void Shutdown()
        {
            // Log the client out
            try
            {
                // Shutdown the server
                Client.Network.SendPacket(System.Text.Encoding.UTF8.GetBytes("stopserver"));

                Client.Network.Logout();
            }
            catch (NotConnectedException)
            {
                Assert.IsTrue(false, "Logout failed, not connected");
            }

            Client = null;
        }
    }
}
