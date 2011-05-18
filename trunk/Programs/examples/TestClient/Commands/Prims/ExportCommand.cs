using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Assets;

namespace OpenMetaverse.TestClient
{
    public class ExportCommand : Command
    {
        List<UUID> Textures = new List<UUID>();
        AutoResetEvent GotPermissionsEvent = new AutoResetEvent(false);
        Primitive.ObjectProperties Properties;
        bool GotPermissions = false;
        UUID SelectedObject = UUID.Zero;

        Dictionary<UUID, Primitive> PrimsWaiting = new Dictionary<UUID, Primitive>();
        AutoResetEvent AllPropertiesReceived = new AutoResetEvent(false);

        public ExportCommand(TestClient testClient)
        {
            testClient.Objects.ObjectPropertiesFamily += new EventHandler<ObjectPropertiesFamilyEventArgs>(Objects_OnObjectPropertiesFamily);

            testClient.Objects.ObjectProperties += new EventHandler<ObjectPropertiesEventArgs>(Objects_OnObjectProperties);
            testClient.Avatars.ViewerEffectPointAt += new EventHandler<ViewerEffectPointAtEventArgs>(Avatars_ViewerEffectPointAt);

            Name = "export";
            Description = "Exports an object to an xml file. Usage: export uuid outputfile.xml";
            Category = CommandCategory.Objects;
        }        

        void Avatars_ViewerEffectPointAt(object sender, ViewerEffectPointAtEventArgs e)
        {
            if (e.SourceID == Client.MasterKey)
            {
                //Client.DebugLog("Master is now selecting " + targetID.ToString());
                SelectedObject = e.TargetID;
            }
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length != 2 && !(args.Length == 1 && SelectedObject != UUID.Zero))
                return "Usage: export uuid outputfile.xml";

            UUID id;
            uint localid;
            string file;

            if (args.Length == 2)
            {
                file = args[1];
                if (!UUID.TryParse(args[0], out id))
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
                    Logger.Log("Warning: Unable to retrieve full properties for:", Helpers.LogLevel.Warning, Client);
                    foreach (UUID uuid in PrimsWaiting.Keys)
                        Logger.Log(uuid.ToString(), Helpers.LogLevel.Warning, Client);
                }

                string output = OSDParser.SerializeLLSDXmlString(Helpers.PrimListToOSD(prims));
                try { File.WriteAllText(file, output); }
                catch (Exception e) { return e.Message; }

                Logger.Log("Exported " + prims.Count + " prims to " + file, Helpers.LogLevel.Info, Client);

                // Create a list of all of the textures to download
                List<ImageRequest> textureRequests = new List<ImageRequest>();

                lock (Textures)
                {
                    for (int i = 0; i < prims.Count; i++)
                    {
                        Primitive prim = prims[i];

                        if (prim.Textures.DefaultTexture.TextureID != Primitive.TextureEntry.WHITE_TEXTURE &&
                            !Textures.Contains(prim.Textures.DefaultTexture.TextureID))
                        {
                            Textures.Add(prim.Textures.DefaultTexture.TextureID);
                        }

                        for (int j = 0; j < prim.Textures.FaceTextures.Length; j++)
                        {
                            if (prim.Textures.FaceTextures[j] != null &&
                                prim.Textures.FaceTextures[j].TextureID != Primitive.TextureEntry.WHITE_TEXTURE &&
                                !Textures.Contains(prim.Textures.FaceTextures[j].TextureID))
                            {
                                Textures.Add(prim.Textures.FaceTextures[j].TextureID);
                            }
                        }

                        if (prim.Sculpt != null && prim.Sculpt.SculptTexture != UUID.Zero && !Textures.Contains(prim.Sculpt.SculptTexture))
                        {
                            Textures.Add(prim.Sculpt.SculptTexture);
                        }
                    }

                    // Create a request list from all of the images
                    for (int i = 0; i < Textures.Count; i++)
                        textureRequests.Add(new ImageRequest(Textures[i], ImageType.Normal, 1013000.0f, 0));
                }

                // Download all of the textures in the export list
                foreach (ImageRequest request in textureRequests)
                {
                    Client.Assets.RequestImage(request.ImageID, request.Type, Assets_OnImageReceived);
                }

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

        private void Assets_OnImageReceived(TextureRequestState state, AssetTexture asset)
        {

            if (state == TextureRequestState.Finished && Textures.Contains(asset.AssetID))
            {
                lock (Textures)
                    Textures.Remove(asset.AssetID);

                if (state == TextureRequestState.Finished)
                {
                    try { File.WriteAllBytes(asset.AssetID + ".jp2", asset.AssetData); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client); }

                    if (asset.Decode())
                    {
                        try { File.WriteAllBytes(asset.AssetID + ".tga", asset.Image.ExportTGA()); }
                        catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client); }
                    }
                    else
                    {
                        Logger.Log("Failed to decode image " + asset.AssetID, Helpers.LogLevel.Error, Client);
                    }

                    Logger.Log("Finished downloading image " + asset.AssetID, Helpers.LogLevel.Info, Client);
                }
                else
                {
                    Logger.Log("Failed to download image " + asset.AssetID + ":" + state, Helpers.LogLevel.Warning, Client);
                }
            }
        }

        void Objects_OnObjectPropertiesFamily(object sender, ObjectPropertiesFamilyEventArgs e)
        {
            Properties = new Primitive.ObjectProperties();
            Properties.SetFamilyProperties(e.Properties);
            GotPermissions = true;
            GotPermissionsEvent.Set();
        }

        void Objects_OnObjectProperties(object sender, ObjectPropertiesEventArgs e)
        {
            lock (PrimsWaiting)
            {
                PrimsWaiting.Remove(e.Properties.ObjectID);

                if (PrimsWaiting.Count == 0)
                    AllPropertiesReceived.Set();
            }
        }
    }
}
