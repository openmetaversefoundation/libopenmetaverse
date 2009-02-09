using System;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Packets;
using ExtensionLoader;
using Simian;
using Simian.Extensions;
using NUnit.Framework;

namespace SimianTests
{
    [TestFixture(Description = "Simian.Extensions.SceneManager")]
    public class SceneManagerTest
    {
        Simian.Simian simian;
        SceneManager sceneManager;
        Random rand = new Random();
        Agent agent;
        Agent observer;

        [SetUp]
        public void Start()
        {
            simian = new Simian.Simian();
            simian.UDP = new UDPManager();
            (simian.UDP as IExtension<Simian.Simian>).Start(simian);
            sceneManager = new SceneManager();
            simian.Scene = sceneManager;
            sceneManager.Start(simian);

            agent = CreateDummyAgent();
            simian.Scene.AgentAdd(this, agent, PrimFlags.None);

            observer = CreateDummyAgent();
            simian.Scene.AgentAdd(this, observer, PrimFlags.None);
        }

        [TearDown]
        public void Stop()
        {
            sceneManager.Stop();
            (simian.UDP as IExtension<Simian.Simian>).Stop();
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

            simian.UDP.OnOutgoingPacket += callback;
            sceneManager.AgentAppearance(this, agent, textures, visualParams);

            Assert.IsTrue(callbackEvent.WaitOne(1000, false), "Timed out waiting for callback");
            simian.UDP.OnOutgoingPacket -= callback;
            
            Assert.That(receivedAgentID == agent.Avatar.ID, "Agent ID mismatch");
            
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

        static Agent CreateDummyAgent()
        {
            Agent agent = new Agent();
            agent.Avatar.ID = UUID.Random();
            agent.SessionID = UUID.Random();
            agent.Avatar.Position = new Vector3(128f, 128f, 40f);
            agent.Avatar.Rotation = Quaternion.Identity;
            return agent;
        }
    }
}
