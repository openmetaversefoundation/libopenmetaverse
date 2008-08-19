using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;

namespace Simian
{
    public class SceneManager : ISimianExtension
    {
        Simian server;
        int currentLocalID = 0;
        int currentWearablesSerialNum = 0;

        public SceneManager(Simian server)
        {
            this.server = server;
        }

        public void Start()
        {
            server.UDPServer.RegisterPacketCallback(PacketType.CompleteAgentMovement, new UDPServer.PacketCallback(CompleteAgentMovementHandler));
            server.UDPServer.RegisterPacketCallback(PacketType.AgentWearablesRequest, new UDPServer.PacketCallback(AgentWearablesRequestHandler));

            LoadTerrain("Maps/default.tga");
        }

        public void Stop()
        {
        }

        void CompleteAgentMovementHandler(Packet packet, Agent agent)
        {
            CompleteAgentMovementPacket request = (CompleteAgentMovementPacket)packet;

            // Create a representation for this agent
            Avatar avatar = new Avatar();
            avatar.ID = agent.AgentID;
            avatar.LocalID = (uint)Interlocked.Increment(ref currentLocalID);
            avatar.Position = new Vector3(128f, 128f, 25f);
            avatar.Rotation = Quaternion.Identity;
            avatar.Scale = new Vector3(1f, 1f, 3f);

            // Set the avatar name
            NameValue[] name = new NameValue[2];
            name[0] = new NameValue("FirstName", NameValue.ValueType.String, NameValue.ClassType.ReadWrite,
                NameValue.SendtoType.SimViewer, agent.FirstName);
            name[1] = new NameValue("LastName", NameValue.ValueType.String, NameValue.ClassType.ReadWrite,
                NameValue.SendtoType.SimViewer, agent.LastName);
            avatar.NameValues = name;

            // Link this avatar up with the corresponding agent
            agent.Avatar = avatar;

            // Give testers a provisionary balance of 1000L
            agent.Balance = 1000;

            AgentMovementCompletePacket complete = new AgentMovementCompletePacket();
            complete.AgentData.AgentID = agent.AgentID;
            complete.AgentData.SessionID = agent.SessionID;
            complete.Data.LookAt = Vector3.UnitX;
            complete.Data.Position = avatar.Position;
            complete.Data.RegionHandle = server.RegionHandle;
            complete.Data.Timestamp = Utils.DateTimeToUnixTime(DateTime.Now);
            complete.SimData.ChannelVersion = Utils.StringToBytes("Simian");

            agent.SendPacket(complete);

            SendLayerData(agent);
        }

        void AgentWearablesRequestHandler(Packet packet, Agent agent)
        {
            AgentWearablesUpdatePacket update = new AgentWearablesUpdatePacket();
            update.AgentData.AgentID = agent.AgentID;
            update.AgentData.SessionID = agent.SessionID;
            // Technically this should be per-agent, but if the only requirement is that it
            // increments this is easier
            update.AgentData.SerialNum = (uint)Interlocked.Increment(ref currentWearablesSerialNum);
            update.WearableData = new AgentWearablesUpdatePacket.WearableDataBlock[4];
            for (int i = 0; i < 4; i++)
            {
                update.WearableData[i] = new AgentWearablesUpdatePacket.WearableDataBlock();
                update.WearableData[i].AssetID = UUID.Zero;
                update.WearableData[i].ItemID = UUID.Zero;
                update.WearableData[i].WearableType = 42; // HACK
            }

            agent.SendPacket(update);
        }

        void LoadTerrain(string mapFile)
        {
            if (File.Exists(mapFile))
            {
                lock (server.Heightmap)
                {
                    Bitmap bmp = LoadTGAClass.LoadTGA(mapFile);

                    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    IntPtr ptr = bmpData.Scan0;
                    int bytes = bmpData.Stride * bmp.Height;
                    byte[] rgbValues = new byte[bytes];
                    Marshal.Copy(ptr, rgbValues, 0, bytes);
                    bmp.UnlockBits(bmpData);

                    server.Heightmap = new float[65536];
                    for (int i = 1, pos = 0; i < server.Heightmap.Length; i++, pos += 3)
                        server.Heightmap[i] = (float)rgbValues[pos];
                }
            }
            else
            {
                Logger.Log("Map file " + mapFile + " not found, defaulting to 25m", Helpers.LogLevel.Info);

                server.Heightmap = new float[65536];
                for (int i = 0; i < server.Heightmap.Length; i++)
                    server.Heightmap[i] = 25f;
            }
        }

        void SendLayerData(Agent agent)
        {
            lock (server.Heightmap)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        int[] patches = new int[1];
                        patches[0] = (y * 16) + x;
                        LayerDataPacket layer = TerrainCompressor.CreateLandPacket(server.Heightmap, patches);
                        agent.SendPacket(layer);
                    }
                }
            }
        }

    }
}
