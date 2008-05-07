using System;
using libsecondlife;
using libsecondlife.Packets;

namespace libsecondlife.TestClient
{
    public class DetectBotCommand : Command
    {
        public DetectBotCommand(TestClient testClient)
        {
            Name = "detectbot";
            Description = "Runs in the background, reporting any potential bots";

            testClient.Network.RegisterCallback(PacketType.AvatarAppearance, new NetworkManager.PacketCallback(AvatarAppearanceHandler));
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            return "This command is always running";
        }

        private void AvatarAppearanceHandler(Packet packet, Simulator simulator)
        {
            AvatarAppearancePacket appearance = (AvatarAppearancePacket)packet;

            LLObject.TextureEntry te = new LLObject.TextureEntry(appearance.ObjectData.TextureEntry, 0, 
                appearance.ObjectData.TextureEntry.Length);

            if (IsNullOrZero(te.FaceTextures[(int)AppearanceManager.TextureIndex.EyesBaked] ) &&
                IsNullOrZero(te.FaceTextures[(int)AppearanceManager.TextureIndex.HeadBaked]) &&
                IsNullOrZero(te.FaceTextures[(int)AppearanceManager.TextureIndex.LowerBaked]) &&
                IsNullOrZero(te.FaceTextures[(int)AppearanceManager.TextureIndex.SkirtBaked]) &&
                IsNullOrZero(te.FaceTextures[(int)AppearanceManager.TextureIndex.UpperBaked]))
            {
                Console.WriteLine("Avatar " + appearance.Sender.ID.ToString() + " may be a bot");
            }
        }

        private bool IsNullOrZero(LLObject.TextureEntryFace face)
        {
            return (face == null || face.TextureID == LLUUID.Zero);
        }
    }
}
