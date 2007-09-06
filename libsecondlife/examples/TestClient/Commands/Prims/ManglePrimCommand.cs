using System;
using System.Threading;
using libsecondlife;

namespace libsecondlife.TestClient
{
    public class ManglePrimCommand : Command
    {
        //private LLUUID CurrentRequest;
        //private AutoResetEvent PropertiesEvent = new AutoResetEvent(false);

        public ManglePrimCommand(TestClient testClient)
        {
            Name = "mangleprim";
            Description = "Modifies the TextureEntry of a prim to allow extended fields to be inserted. " +
                "Usage: mangleprim [prim-uuid] [face-index] [texture-uuid]";

            //testClient.Objects.OnObjectPropertiesFamily += new ObjectManager.ObjectPropertiesFamilyCallback(Objects_OnObjectPropertiesFamily);
        }

        // FIXME: Check permissions first when the permission system is robust enough to support proper checking
        //private void Objects_OnObjectPropertiesFamily(Simulator simulator, LLObject.ObjectPropertiesFamily properties)
        //{
        //    if (properties.ObjectID == CurrentRequest)
        //    {
        //        if (properties.Permissions.
        //    }
        //}

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            LLUUID primID;
            int faceIndex;
            LLUUID faceID;

            if (args.Length != 3)
                return "Usage: mangleprim [prim-uuid] [face-index] [texture-uuid]";

            if (LLUUID.TryParse(args[0], out primID) &&
                Int32.TryParse(args[1], out faceIndex) &&
                LLUUID.TryParse(args[2], out faceID))
            {
                // Search for this prim in the local objects
                Primitive target = Client.Network.CurrentSim.Objects.Find(
                    delegate(Primitive prim)
                    {
                        return prim.ID == primID;
                    }
                );

                if (target != null)
                {
                    LLObject.TextureEntry textureEntry = target.Textures.Clone();

                    LLObject.TextureEntryFace face = textureEntry.CreateFace((uint)faceIndex);
                    face.TextureID = faceID;
                    Client.Objects.SetTextures(Client.Network.CurrentSim, target.LocalID, textureEntry);

                    return String.Format("Blindly setting prim {0} texture index {1} to texture value {2}",
                        primID.ToStringHyphenated(), faceIndex, faceID.ToStringHyphenated());

                    // Request permissions for the target
                    //Client.Objects.RequestObjectPropertiesFamily(Client.Network.CurrentSim, primID);
                    //if (PropertiesEvent.WaitOne(10 * 1000, false))
                    //{
                    //}
                    //else
                    //{
                    //    return "Timed out while requesting properties for " + primID.ToStringHyphenated();
                    //}
                }
                else
                {
                    return "Cannot find prim " + primID.ToStringHyphenated();
                }
            }
            else
            {
                return "Usage: mangleprim [prim-uuid] [face-index] [texture-uuid]";
            }
        }
    }
}
