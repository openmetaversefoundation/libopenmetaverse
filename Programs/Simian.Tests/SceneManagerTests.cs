using System;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;
using ExtensionLoader;
using NUnit.Framework;

namespace Simian.Tests
{
    [TestFixture(Description = "Simian.Extensions.SceneManager")]
    public class SceneManagerTest
    {
        Simian simian;
        Random rand = new Random();
        Agent agent;
        Agent observer;

        [SetUp]
        public void Start()
        {
            simian = new Simian();
            simian.Start();

            agent = CreateDummyAgent();
            simian.Scenes[0].AgentAdd(this, agent, PrimFlags.None);

            observer = CreateDummyAgent();
            simian.Scenes[0].AgentAdd(this, observer, PrimFlags.None);
        }

        [TearDown]
        public void Stop()
        {
            simian.Stop();
        }

        [Test]
        public void AvatarAppearanceTest()
        {
            Primitive.TextureEntry textures = new Primitive.TextureEntry(AppearanceManager.DEFAULT_AVATAR_TEXTURE);
            byte[] visualParams = new byte[218];
            rand.NextBytes(visualParams);

            UUID receivedAgentID = UUID.Zero;
            Primitive.TextureEntry receivedTextureEntry = null;
            byte[] receivedVisualParams = null;

            AutoResetEvent callbackEvent = new AutoResetEvent(false);
            OutgoingPacketCallback callback = new OutgoingPacketCallback(
                delegate(Packet packet, UUID agentID, PacketCategory category)
                {
                    if (packet is AvatarAppearancePacket)
                    {
                        AvatarAppearancePacket appearance = (AvatarAppearancePacket)packet;

                        receivedAgentID = appearance.Sender.ID;
                        receivedTextureEntry = new Primitive.TextureEntry(appearance.ObjectData.TextureEntry, 0, appearance.ObjectData.TextureEntry.Length);
                        receivedVisualParams = new byte[appearance.VisualParam.Length];
                        for (int i = 0; i < receivedVisualParams.Length; i++)
                            receivedVisualParams[i] = appearance.VisualParam[i].ParamValue;

                        callbackEvent.Set();
                    }

                    return false;
                }
            );

            simian.Scenes[0].UDP.OnOutgoingPacket += callback;
            simian.Scenes[0].AgentAppearance(this, agent, textures, visualParams);

            Assert.IsTrue(callbackEvent.WaitOne(1000, false), "Timed out waiting for callback");
            simian.Scenes[0].UDP.OnOutgoingPacket -= callback;
            
            Assert.That(receivedAgentID == agent.ID, "Agent ID mismatch");
            
            Assert.That(receivedVisualParams.Length == 218, "VisualParams has an incorrect length");
            for (int i = 0; i < 218; i++)
                Assert.That(receivedVisualParams[i] == visualParams[i], "VisualParam mismatch at position " + i);

            Assert.That(receivedTextureEntry.DefaultTexture.TextureID == textures.DefaultTexture.TextureID, "Default texture mismatch");
            for (int i = 0; i < receivedTextureEntry.FaceTextures.Length; i++)
            {
                Assert.That(
                    (receivedTextureEntry.FaceTextures[i] == null && textures.FaceTextures[i] == null) ||
                    (receivedTextureEntry.FaceTextures[i].TextureID == textures.FaceTextures[i].TextureID), "TextureEntry mismatch at position " + i);
            }
        }

        Agent CreateDummyAgent()
        {
            AgentInfo info = new AgentInfo();
            info.FirstName = "Dummy";
            info.LastName = "Agent";

            Agent agent = new Agent(new SimulationObject(new Avatar(), simian.Scenes[0]), info);
            agent.Avatar.Prim.ID = UUID.Random();
            agent.SessionID = UUID.Random();
            agent.Avatar.Prim.Position = new Vector3(128f, 128f, 40f);
            agent.Avatar.Prim.Rotation = Quaternion.Identity;
            return agent;
        }
    }
}
