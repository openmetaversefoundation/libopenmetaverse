using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.StructuredData;

namespace Simian
{
    class HyperGridLink
    {
        public string RegionName;
        public ulong RegionHandle;
        public UUID RegionID;
        public UUID RegionImage;
        public int UDPPort;
        public int RemotingPort;
    }

    public class LLMap : IExtension<ISceneProvider>
    {
        static readonly UUID WATER_TEXTURE = new UUID("af588c7c-52b0-4d9e-a888-1fe9d6c35f45");
        static readonly UUID HYPERGRID_MAP_TEXTURE = new UUID("3f1f56ad-7811-42e6-b3c1-98b79fc5c360");

        ISceneProvider scene;

        public LLMap()
        {
        }

        public bool Start(ISceneProvider scene)
        {
            this.scene = scene;

            scene.UDP.RegisterPacketCallback(PacketType.MapLayerRequest, MapLayerRequestHandler);
            scene.UDP.RegisterPacketCallback(PacketType.MapBlockRequest, MapBlockRequestHandler);
            scene.UDP.RegisterPacketCallback(PacketType.MapItemRequest, MapItemRequestHandler);
            scene.UDP.RegisterPacketCallback(PacketType.TeleportRequest, TeleportRequestHandler);
            scene.UDP.RegisterPacketCallback(PacketType.TeleportLocationRequest, TeleportLocationRequestHandler);
            return true;
        }

        public void Stop()
        {
        }

        void MapLayerRequestHandler(Packet packet, Agent agent)
        {
            MapLayerRequestPacket request = (MapLayerRequestPacket)packet;
            GridLayerType type = (GridLayerType)request.AgentData.Flags;

            // FIXME: Do this properly. Use the grid service to get the aggregated map layers
            // (lots of map tiles in a single texture == layer)

            MapLayerReplyPacket reply = new MapLayerReplyPacket();
            reply.AgentData.AgentID = agent.ID;
            reply.AgentData.Flags = (uint)type;
            reply.LayerData = new MapLayerReplyPacket.LayerDataBlock[1];
            reply.LayerData[0] = new MapLayerReplyPacket.LayerDataBlock();
            reply.LayerData[0].Bottom = 0;
            reply.LayerData[0].Left = 0;
            reply.LayerData[0].Top = UInt16.MaxValue;
            reply.LayerData[0].Right = UInt16.MaxValue;
            reply.LayerData[0].ImageID = WATER_TEXTURE;

            scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
        }

        void MapBlockRequestHandler(Packet packet, Agent agent)
        {
            MapBlockRequestPacket request = (MapBlockRequestPacket)packet;
            bool returnNonexistent = (request.AgentData.Flags == 0x10000);
            GridLayerType type = (GridLayerType)(request.AgentData.Flags &~0x10000);

            IList<RegionInfo> regions = scene.Server.Grid.GetRegionsInArea(request.PositionData.MinX, request.PositionData.MinY,
                request.PositionData.MaxX, request.PositionData.MaxY);

            MapBlockReplyPacket reply = new MapBlockReplyPacket();
            reply.AgentData.AgentID = agent.ID;
            reply.AgentData.Flags = (uint)type;

            MapBlockReplyPacket.DataBlock[] blocks;

            if (returnNonexistent)
            {
                int blockCountX = request.PositionData.MaxX + 1 - request.PositionData.MinX;
                int blockCountY = request.PositionData.MaxY + 1 - request.PositionData.MinY;
                blocks = new MapBlockReplyPacket.DataBlock[blockCountX * blockCountY];
                int i = 0;

                for (int y = request.PositionData.MinY; y <= request.PositionData.MaxY; y++)
                {
                    for (int x = request.PositionData.MinX; x <= request.PositionData.MaxX; x++)
                    {
                        blocks[i] = new MapBlockReplyPacket.DataBlock();
                        blocks[i].X = (ushort)x;
                        blocks[i].Y = (ushort)y;

                        // See if we have data for this region
                        RegionInfo? region = null;
                        for (int j = 0; j < regions.Count; j++)
                        {
                            if (regions[j].X == x && regions[j].Y == y)
                            {
                                region = regions[j];
                                break;
                            }
                        }

                        if (region.HasValue)
                        {
                            blocks[i].Access = (byte)SimAccess.Min;
                            blocks[i].Agents = (byte)region.Value.AgentCount;
                            blocks[i].MapImageID = region.Value.MapTextureID;
                            blocks[i].Name = Utils.StringToBytes(region.Value.Name);
                            blocks[i].RegionFlags = (uint)region.Value.Flags;
                            blocks[i].WaterHeight = (byte)region.Value.WaterHeight;
                        }
                        else
                        {
                            blocks[i].Name = Utils.EmptyBytes;
                            blocks[i].MapImageID = WATER_TEXTURE;
                        }

                        ++i;
                    }
                }
            }
            else
            {
                blocks = new MapBlockReplyPacket.DataBlock[regions.Count];

                for (int i = 0; i < regions.Count; i++)
                {
                    RegionInfo region = regions[i];

                    blocks[i] = new MapBlockReplyPacket.DataBlock();
                    blocks[i].X = (ushort)region.X;
                    blocks[i].Y = (ushort)region.Y;
                    blocks[i].Access = (byte)SimAccess.Min;
                    blocks[i].Agents = (byte)region.AgentCount;
                    blocks[i].MapImageID = region.MapTextureID;
                    blocks[i].Name = Utils.StringToBytes(region.Name);
                    blocks[i].RegionFlags = (uint)region.Flags;
                    blocks[i].WaterHeight = (byte)region.WaterHeight;
                }
            }

            // FIXME: Handle large numbers of blocks by splitting things up
            reply.Data = blocks;

            scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
        }

        void MapItemRequestHandler(Packet packet, Agent agent)
        {
            MapItemRequestPacket request = (MapItemRequestPacket)packet;

            GridLayerType layerType = (GridLayerType)request.AgentData.Flags;
            GridItemType itemType = (GridItemType)request.RequestData.ItemType;

            uint regionX, regionY;
            Utils.LongToUInts(request.RequestData.RegionHandle, out regionX, out regionY);

            RegionInfo regionInfo;
            if (scene.Server.Grid.TryGetRegion(regionX, regionY, scene.RegionCertificate, out regionInfo))
            {
                Logger.Log("MapItemRequest for " + itemType + " from layer " + layerType + " in " + regionInfo.Name, Helpers.LogLevel.Info);

                MapItemReplyPacket reply = new MapItemReplyPacket();
                reply.AgentData.AgentID = agent.ID;
                reply.AgentData.Flags = request.AgentData.Flags;
                reply.RequestData.ItemType = (uint)itemType;
                reply.Data = new MapItemReplyPacket.DataBlock[0];

                scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
            }
            else
            {
                Logger.Log("MapItemRequest for " + itemType + " from layer " + layerType + " in unknown region at " + regionX + "," + regionY,
                    Helpers.LogLevel.Warning);
            }
        }

        void TeleportRequestHandler(Packet packet, Agent agent)
        {
            TeleportRequestPacket request = (TeleportRequestPacket)packet;

            // TODO: Stand the avatar up first

            if (request.Info.RegionID == scene.RegionID)
            {
                // Local teleport
                agent.Avatar.Prim.Position = request.Info.Position;
                agent.CurrentLookAt = request.Info.LookAt;

                // TODO: Actually adjust the agent's LookAt

                TeleportLocalPacket reply = new TeleportLocalPacket();
                reply.Info.AgentID = agent.ID;
                reply.Info.LocationID = 0; // Unused by the client
                reply.Info.LookAt = agent.CurrentLookAt;
                reply.Info.Position = agent.Avatar.Prim.Position;
                // TODO: Need a "Flying" boolean for Agent
                reply.Info.TeleportFlags = (uint)TeleportFlags.ViaRegionID;

                scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
            }
            else
            {
                TeleportFailedPacket reply = new TeleportFailedPacket();
                reply.Info.AgentID = agent.ID;
                reply.Info.Reason = Utils.StringToBytes("Unknown region");

                scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
            }
        }

        void TeleportLocationRequestHandler(Packet packet, Agent agent)
        {
            TeleportLocationRequestPacket request = (TeleportLocationRequestPacket)packet;

            // TODO: Stand the avatar up first

            if (request.Info.RegionHandle == scene.RegionHandle)
            {
                // Local teleport
                agent.Avatar.Prim.Position = request.Info.Position;
                agent.CurrentLookAt = request.Info.LookAt;

                TeleportLocalPacket reply = new TeleportLocalPacket();
                reply.Info.AgentID = agent.ID;
                reply.Info.LocationID = 0; // Unused by the client
                reply.Info.LookAt = agent.CurrentLookAt;
                reply.Info.Position = agent.Avatar.Prim.Position;
                // TODO: Need a "Flying" boolean for Agent
                reply.Info.TeleportFlags = (uint)TeleportFlags.ViaLocation;

                scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
            }
            // FIXME: Add .ini config support for HyperGrid destinations
            /*else if (request.Info.RegionHandle == Utils.UIntsToLong((scene.RegionX + 1) * 256, scene.RegionY * 256))
            {
                // Special case: adjacent simulator is the HyperGrid portal
                HyperGridTeleport(agent, new Uri("http://osl2.nac.uci.edu:9006/"), request.Info.Position);
            }*/
            else
            {
                TeleportFailedPacket reply = new TeleportFailedPacket();
                reply.Info.AgentID = agent.ID;
                reply.Info.Reason = Utils.StringToBytes("Unknown region");

                scene.UDP.SendPacket(agent.ID, reply, PacketCategory.Transaction);
            }
        }

        bool HyperGridTeleport(Agent agent, Uri destination, Vector3 destPos)
        {
            HyperGridLink link;

            TeleportProgress(agent, "Resolving destination IP address", TeleportFlags.ViaLocation);

            IPHostEntry entry = Dns.GetHostEntry(destination.DnsSafeHost);
            if (entry.AddressList != null && entry.AddressList.Length >= 1)
            {
                TeleportProgress(agent, "Retrieving destination details", TeleportFlags.ViaLocation);

                if (LinkRegion(destination, out link))
                {
                    TeleportProgress(agent, "Creating foreign agent", TeleportFlags.ViaLocation);

                    // This is a crufty part of the HyperGrid protocol. We need to generate a fragment of a UUID
                    // (missing the last four digits) and send that as the caps_path variable. Locally, we expand
                    // that out to http://foreignsim:httpport/CAPS/fragment0000/ and use it as the seed caps path
                    // that is sent to the client
                    UUID seedID = UUID.Random();
                    string seedCapFragment = seedID.ToString().Substring(0, 32);
                    Uri seedCap = new Uri(destination, "/CAPS/" + seedCapFragment + "0000/");

                    if (ExpectHyperGridUser(agent, destination, destPos, link, seedCap))
                    {
                        TeleportProgress(agent, "Establishing foreign agent presence", TeleportFlags.ViaLocation);

                        if (CreateChildAgent(agent, destination, destPos, link, seedCapFragment))
                        {
                            // Send the final teleport message to the client
                            if (scene.HasRunningEventQueue(agent))
                            {
                                uint x, y;
                                Utils.LongToUInts(link.RegionHandle, out x, out y);
                                x /= 256;
                                y /= 256;
                                Logger.Log(String.Format("HyperGrid teleporting to {0} ({1}, {2}) @ {3}",
                                    link.RegionName, x, y, destination), Helpers.LogLevel.Info);

                                OSDMap teleport = CapsMessages.TeleportFinish(agent.ID, 4, link.RegionHandle, seedCap, SimAccess.Min,
                                    entry.AddressList[0], link.UDPPort, TeleportFlags.ViaLocation);

                                scene.SendEvent(agent, "TeleportFinish", teleport);
                            }
                            else
                            {
                                Logger.Log("No running EventQueue for " + agent.FullName + ", sending TeleportFinish over UDP",
                                    Helpers.LogLevel.Warning);

                                TeleportFinishPacket teleport = new TeleportFinishPacket();
                                teleport.Info.AgentID = agent.ID;
                                teleport.Info.LocationID = 0; // Unused by the client
                                teleport.Info.RegionHandle = link.RegionHandle;
                                teleport.Info.SeedCapability = Utils.StringToBytes(seedCap.ToString());
                                teleport.Info.SimAccess = (byte)SimAccess.Min;
                                teleport.Info.SimIP = Utils.BytesToUInt(entry.AddressList[0].GetAddressBytes());
                                teleport.Info.SimPort = (ushort)link.UDPPort;
                                teleport.Info.TeleportFlags = (uint)TeleportFlags.ViaLocation;

                                scene.UDP.SendPacket(agent.ID, teleport, PacketCategory.Transaction);
                            }

                            // Remove the agent from the local scene (will also tear down the UDP connection)
                            //scene.ObjectRemove(this, agent.ID);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        bool LinkRegion(Uri destination, out HyperGridLink link)
        {
            try
            {
                #region Build Request

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(destination);
                request.Method = "POST";
                request.ContentType = "text/xml";

                MemoryStream memoryStream = new MemoryStream();
                using (XmlWriter writer = XmlWriter.Create(memoryStream))
                {
                    writer.WriteStartElement("methodCall");
                    {
                        writer.WriteElementString("methodName", "link_region");

                        writer.WriteStartElement("params");
                        writer.WriteStartElement("param");
                        writer.WriteStartElement("value");
                        writer.WriteStartElement("struct");
                        {
                            WriteStringMember(writer, "region_name", String.Empty);
                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.Flush();
                }

                request.ContentLength = memoryStream.Length;

                using (Stream writeStream = request.GetRequestStream())
                {
                    writeStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                }

                #endregion Build Request

                #region Parse Response

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                using (XmlReader reader = XmlReader.Create(response.GetResponseStream(), settings))
                {
                    link = new HyperGridLink();

                    reader.ReadStartElement("methodResponse");
                    {
                        reader.ReadStartElement("params");
                        reader.ReadStartElement("param");
                        reader.ReadStartElement("value");
                        reader.ReadStartElement("struct");
                        {
                            while (reader.Name == "member")
                            {
                                reader.ReadStartElement("member");
                                {
                                    string name = reader.ReadElementContentAsString("name", String.Empty);

                                    reader.ReadStartElement("value");
                                    {
                                        switch (name)
                                        {
                                            case "region_name":
                                                link.RegionName = reader.ReadElementContentAsString("string", String.Empty);
                                                break;
                                            case "handle":
                                                string handle = reader.ReadElementContentAsString("string", String.Empty);
                                                link.RegionHandle = UInt64.Parse(handle);
                                                break;
                                            case "uuid":
                                                string uuid = reader.ReadElementContentAsString("string", String.Empty);
                                                link.RegionID = UUID.Parse(uuid);
                                                break;
                                            case "internal_port":
                                                link.UDPPort = reader.ReadElementContentAsInt("string", String.Empty);
                                                break;
                                            case "region_image":
                                                string imageuuid = reader.ReadElementContentAsString("string", String.Empty);
                                                link.RegionImage = UUID.Parse(imageuuid);
                                                break;
                                            case "remoting_port":
                                                link.RemotingPort = reader.ReadElementContentAsInt("string", String.Empty);
                                                break;
                                            default:
                                                Logger.Log("[HyperGrid] Unrecognized response XML chunk: " + reader.ReadInnerXml(),
                                                    Helpers.LogLevel.Warning);
                                                break;
                                        }
                                    }
                                    reader.ReadEndElement();
                                }
                                reader.ReadEndElement();
                            }
                        }
                        reader.ReadEndElement();
                        reader.ReadEndElement();
                        reader.ReadEndElement();
                        reader.ReadEndElement();
                    }
                    reader.ReadEndElement();

                    return true;
                }

                #endregion Parse Response
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Error, ex);
            }

            link = null;
            return false;
        }

        bool ExpectHyperGridUser(Agent agent, Uri destination, Vector3 destPos, HyperGridLink link, Uri seedCap)
        {
            try
            {
                #region Build Request

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(destination);
                request.Method = "POST";
                request.ContentType = "text/xml";

                MemoryStream memoryStream = new MemoryStream();
                using (XmlWriter writer = XmlWriter.Create(memoryStream))
                {
                    writer.WriteStartElement("methodCall");
                    {
                        writer.WriteElementString("methodName", "expect_hg_user");

                        writer.WriteStartElement("params");
                        writer.WriteStartElement("param");
                        writer.WriteStartElement("value");
                        writer.WriteStartElement("struct");
                        {
                            WriteStringMember(writer, "session_id", agent.SessionID.ToString());
                            WriteStringMember(writer, "secure_session_id", agent.SecureSessionID.ToString());
                            WriteStringMember(writer, "firstname", agent.Info.FirstName);
                            WriteStringMember(writer, "lastname", agent.Info.LastName);
                            WriteStringMember(writer, "agent_id", agent.ID.ToString());
                            WriteStringMember(writer, "circuit_code", agent.CircuitCode.ToString());
                            WriteStringMember(writer, "startpos_x", destPos.X.ToString(Utils.EnUsCulture));
                            WriteStringMember(writer, "startpos_y", destPos.Y.ToString(Utils.EnUsCulture));
                            WriteStringMember(writer, "startpos_z", destPos.Z.ToString(Utils.EnUsCulture));
                            WriteStringMember(writer, "caps_path", seedCap.ToString());

                            WriteStringMember(writer, "region_uuid", link.RegionID.ToString());
                            //WriteStringMember(writer, "userserver_id", "");
                            //WriteStringMember(writer, "assetserver_id", "");
                            //WriteStringMember(writer, "inventoryserver_id", "");
                            WriteStringMember(writer, "root_folder_id", agent.Info.InventoryRoot.ToString());

                            string port = scene.Server.HttpUri.Port.ToString();

                            WriteStringMember(writer, "internal_port", port);
                            WriteStringMember(writer, "regionhandle", scene.RegionHandle.ToString());
                            WriteStringMember(writer, "home_address", IPAddress.Loopback.ToString());
                            WriteStringMember(writer, "home_port", port);
                            WriteStringMember(writer, "home_remoting", port);
                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.Flush();
                }

                request.ContentLength = memoryStream.Length;

                using (Stream writeStream = request.GetRequestStream())
                {
                    writeStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                }

                #endregion Build Request

                #region Parse Response

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                using (XmlReader reader = XmlReader.Create(response.GetResponseStream(), settings))
                {
                    bool success = false;
                    string reason = String.Empty;

                    reader.ReadStartElement("methodResponse");
                    {
                        reader.ReadStartElement("params");
                        reader.ReadStartElement("param");
                        reader.ReadStartElement("value");
                        reader.ReadStartElement("struct");
                        {
                            while (reader.Name == "member")
                            {
                                reader.ReadStartElement("member");
                                {
                                    string name = reader.ReadElementContentAsString("name", String.Empty);

                                    reader.ReadStartElement("value");
                                    {
                                        switch (name)
                                        {
                                            case "success":
                                                success = (reader.ReadElementContentAsString("string", String.Empty).ToUpper() == "TRUE");
                                                break;
                                            case "reason":
                                                reason = reader.ReadElementContentAsString("string", String.Empty);
                                                break;
                                            default:
                                                Logger.Log("[HyperGrid] Unrecognized response XML chunk: " + reader.ReadInnerXml(),
                                                    Helpers.LogLevel.Warning);
                                                break;
                                        }
                                    }
                                    reader.ReadEndElement();
                                }
                                reader.ReadEndElement();
                            }
                        }
                        reader.ReadEndElement();
                        reader.ReadEndElement();
                        reader.ReadEndElement();
                        reader.ReadEndElement();
                    }
                    reader.ReadEndElement();

                    if (!success)
                        Logger.Log("[HyperGrid] Teleport failed, reason: " + reason, Helpers.LogLevel.Warning);

                    return success;
                }

                #endregion Parse Response
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Error, ex);
            }

            return false;
        }

        bool CreateChildAgent(Agent agent, Uri destination, Vector3 destPos, HyperGridLink link, string seedCapFragment)
        {
            try
            {
                destination = new Uri(destination, "/agent/" + agent.ID.ToString() + "/");

                OSDMap args = new OSDMap();
                args["agent_id"] = OSD.FromUUID(agent.ID);
                args["base_folder"] = OSD.FromUUID(UUID.Zero);
                args["caps_path"] = OSD.FromString(seedCapFragment);
                args["children_seeds"] = OSD.FromBoolean(false);
                args["child"] = OSD.FromBoolean(false);
                args["circuit_code"] = OSD.FromString(agent.CircuitCode.ToString());
                args["first_name"] = OSD.FromString(agent.Info.FirstName);
                args["last_name"] = OSD.FromString(agent.Info.LastName);
                args["inventory_folder"] = OSD.FromUUID(agent.Info.InventoryRoot);
                args["secure_session_id"] = OSD.FromUUID(agent.SecureSessionID);
                args["session_id"] = OSD.FromUUID(agent.SessionID);
                args["start_pos"] = OSD.FromString(destPos.ToString());
                args["destination_handle"] = OSD.FromString(link.RegionHandle.ToString());

                LitJson.JsonData jsonData = OSDParser.SerializeJson(args);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonData.ToJson());

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(destination);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = data.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Flush();
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                bool success = false;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    Boolean.TryParse(reader.ReadToEnd(), out success);
                }

                return success;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, Helpers.LogLevel.Error, ex);
            }

            return false;
        }

        void TeleportProgress(Agent agent, string message, TeleportFlags flags)
        {
            TeleportProgressPacket progress = new TeleportProgressPacket();
            progress.AgentData.AgentID = agent.ID;
            progress.Info.Message = Utils.StringToBytes(message);
            progress.Info.TeleportFlags = (uint)flags;

            scene.UDP.SendPacket(agent.ID, progress, PacketCategory.Transaction);
        }

        static void WriteStringMember(XmlWriter writer, string name, string value)
        {
            writer.WriteStartElement("member");
            {
                writer.WriteElementString("name", name);

                writer.WriteStartElement("value");
                {
                    writer.WriteElementString("string", value);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
