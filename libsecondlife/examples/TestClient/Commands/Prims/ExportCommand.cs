using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using libsecondlife;
using libsecondlife.StructuredData;

namespace libsecondlife.TestClient
{
    public class ExportCommand : Command
    {
        List<LLUUID> Textures = new List<LLUUID>();
        AutoResetEvent GotPermissionsEvent = new AutoResetEvent(false);
        LLObject.ObjectPropertiesFamily Properties;
        bool GotPermissions = false;
        LLUUID SelectedObject = LLUUID.Zero;

        Dictionary<LLUUID, Primitive> PrimsWaiting = new Dictionary<LLUUID, Primitive>();
        AutoResetEvent AllPropertiesReceived = new AutoResetEvent(false);

        public ExportCommand(TestClient testClient)
        {
            testClient.Objects.OnObjectPropertiesFamily += new ObjectManager.ObjectPropertiesFamilyCallback(Objects_OnObjectPropertiesFamily);
            testClient.Objects.OnObjectProperties += new ObjectManager.ObjectPropertiesCallback(Objects_OnObjectProperties);
            testClient.Assets.OnImageReceived += new AssetManager.ImageReceivedCallback(Assets_OnImageReceived);
            testClient.Avatars.OnPointAt += new AvatarManager.PointAtCallback(Avatars_OnPointAt);

            Name = "export";
            Description = "Exports an object to an xml file. Usage: export uuid outputfile.xml";
        }

        public override string Execute(string[] args, LLUUID fromAgentID)
        {
            if (args.Length != 2 && !(args.Length == 1 && SelectedObject != LLUUID.Zero))
                return "Usage: export uuid outputfile.xml";

            LLUUID id;
            uint localid;
            string file;

            if (args.Length == 2)
            {
                file = args[1];
                if (!LLUUID.TryParse(args[0], out id))
                    return "Usage: export uuid outputfile.xml";
            }
            else
            {
                file = args[0];
                id = SelectedObject;
            }

            Primitive exportPrim;

            exportPrim = Client.Network.CurrentSim.ObjectsPrimitives.Find(
                delegate(Primitive prim) { return prim.ID == id; }
            );

            if (exportPrim != null)
            {
                if (exportPrim.ParentID != 0)
                    localid = exportPrim.ParentID;
                else
                    localid = exportPrim.LocalID;

                // Check for export permission first
                Client.Objects.RequestObjectPropertiesFamily(Client.Network.CurrentSim, id);
                GotPermissionsEvent.WaitOne(1000 * 10, false);

                if (!GotPermissions)
                {
                    return "Couldn't fetch permissions for the requested object, try again";
                }
                else
                {
                    GotPermissions = false;
                    if (Properties.OwnerID != Client.Self.AgentID && 
                        Properties.OwnerID != Client.MasterKey && 
                        Client.Self.AgentID != Client.Self.AgentID)
                    {
                        return "That object is owned by " + Properties.OwnerID + ", we don't have permission " +
                            "to export it";
                    }
                }

                List<Primitive> prims = Client.Network.CurrentSim.ObjectsPrimitives.FindAll(
                    delegate(Primitive prim)
                    {
                        return (prim.LocalID == localid || prim.ParentID == localid);
                    }
                );

                bool complete = RequestObjectProperties(prims, 250);

                if (!complete)
                {
                    Client.Log("Warning: Unable to retrieve full properties for:", Helpers.LogLevel.Warning);
                    foreach (LLUUID uuid in PrimsWaiting.Keys)
                        Client.Log(uuid.ToString(), Helpers.LogLevel.Warning);
                }

                string output = LLSDParser.SerializeXmlString(Helpers.PrimListToLLSD(prims));
                try { File.WriteAllText(file, output); }
                catch (Exception e) { return e.Message; }

                Client.Log("Exported " + prims.Count + " prims to " + file, Helpers.LogLevel.Info);

                // Create a list of all of the textures to download
                List<ImageRequest> textureRequests = new List<ImageRequest>();

                lock (Textures)
                {
                    for (int i = 0; i < prims.Count; i++)
                    {
                        Primitive prim = prims[i];

                        if (!Textures.Contains(prim.Textures.DefaultTexture.TextureID))
                            Textures.Add(prim.Textures.DefaultTexture.TextureID);

                        for (int j = 0; j < prim.Textures.FaceTextures.Length; j++)
                        {
                            if (prim.Textures.FaceTextures[j] != null && !Textures.Contains(prim.Textures.FaceTextures[j].TextureID))
                                Textures.Add(prim.Textures.FaceTextures[j].TextureID);
                        }
                    }

                    // Create a request list from all of the images
                    for (int i = 0; i < Textures.Count; i++)
                        textureRequests.Add(new ImageRequest(Textures[i], ImageType.Normal, 1013000.0f, 0));
                }

                // Download all of the textures in the export list
                Client.Assets.RequestImages(textureRequests);

                return "XML exported, began downloading " + Textures.Count + " textures";
            }
            else
            {
                return "Couldn't find UUID " + id.ToString() + " in the " + 
                    Client.Network.CurrentSim.ObjectsPrimitives.Count + 
                    "objects currently indexed in the current simulator";
            }
        }

        private bool RequestObjectProperties(List<Primitive> objects, int msPerRequest)
        {
            // Create an array of the local IDs of all the prims we are requesting properties for
            uint[] localids = new uint[objects.Count];
            
            lock (PrimsWaiting)
            {
                PrimsWaiting.Clear();

                for (int i = 0; i < objects.Count; ++i)
                {
                    localids[i] = objects[i].LocalID;
                    PrimsWaiting.Add(objects[i].ID, objects[i]);
                }
            }

            Client.Objects.SelectObjects(Client.Network.CurrentSim, localids);

            return AllPropertiesReceived.WaitOne(2000 + msPerRequest * objects.Count, false);
        }

        private void Assets_OnImageReceived(ImageDownload image, AssetTexture asset)
        {
            if (Textures.Contains(image.ID))
            {
                lock (Textures)
                    Textures.Remove(image.ID);

                if (image.Success)
                {
                    try { File.WriteAllBytes(image.ID.ToString() + ".jp2", asset.AssetData); }
                    catch (Exception ex) { Client.Log(ex.Message, Helpers.LogLevel.Error); }

                    if (asset.Decode())
                    {
                        try { File.WriteAllBytes(image.ID.ToString() + ".tga", asset.Image.ExportTGA()); }
                        catch (Exception ex) { Client.Log(ex.Message, Helpers.LogLevel.Error); }
                    }
                    else
                    {
                        Client.Log("Failed to decode image " + image.ID.ToString(), Helpers.LogLevel.Error);
                    }

                    Client.Log("Finished downloading image " + image.ID.ToString(), Helpers.LogLevel.Info);
                }
                else
                {
                    Client.Log("Failed to download image " + image.ID.ToString(), Helpers.LogLevel.Warning);
                }
            }
        }

        void Avatars_OnPointAt(LLUUID sourceID, LLUUID targetID, LLVector3d targetPos, 
            PointAtType pointType, float duration, LLUUID id)
        {
            if (sourceID == Client.MasterKey)
            {
                //Client.DebugLog("Master is now selecting " + targetID.ToString());
                SelectedObject = targetID;
            }
        }

        void Objects_OnObjectPropertiesFamily(Simulator simulator, LLObject.ObjectPropertiesFamily properties)
        {
            Properties = properties;
            GotPermissions = true;
            GotPermissionsEvent.Set();
        }

        void Objects_OnObjectProperties(Simulator simulator, LLObject.ObjectProperties properties)
        {
            lock (PrimsWaiting)
            {
                PrimsWaiting.Remove(properties.ObjectID);

                if (PrimsWaiting.Count == 0)
                    AllPropertiesReceived.Set();
            }
        }
    }
}
