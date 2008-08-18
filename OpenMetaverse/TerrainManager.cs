/*
 * Copyright (c) 2007-2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    public class TerrainManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="data"></param>
        public delegate void LandPatchCallback(Simulator simulator, int x, int y, int width, float[] data);

        /// <summary></summary>
        public event LandPatchCallback OnLandPatch;

        public InternalDictionary<ulong, TerrainPatch[]> SimPatches = new InternalDictionary<ulong, TerrainPatch[]>();

        private GridClient Client;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client"></param>
        public TerrainManager(GridClient client)
        {
            Client = client;
            Client.Network.RegisterCallback(PacketType.LayerData, new NetworkManager.PacketCallback(LayerDataHandler));
        }

        /// <summary>
        /// Retrieve the terrain height at a given coordinate
        /// </summary>
        /// <param name="regionHandle">The region that the point of interest is in</param>
        /// <param name="x">Sim X coordinate, valid range is from 0 to 255</param>
        /// <param name="y">Sim Y coordinate, valid range is from 0 to 255</param>
        /// <param name="height">The terrain height at the given point if the
        /// lookup was successful, otherwise 0.0f</param>
        /// <returns>True if the lookup was successful, otherwise false</returns>
        public bool TerrainHeightAtPoint(ulong regionHandle, int x, int y, out float height)
        {
            if (x >= 0 && x < 256 && y >= 0 && y < 256)
            {
                lock (SimPatches)
                {
                    if (SimPatches.ContainsKey(regionHandle))
                    {
                        int patchX = x / 16;
                        int patchY = y / 16;
                        x = x % 16;
                        y = y % 16;

                        if (SimPatches[regionHandle][patchY * 16 + patchX] != null)
                        {
                            height = SimPatches[regionHandle][patchY * 16 + patchX].Heightmap[y * 16 + x];
                            return true;
                        }
                    }
                }
            }

            height = 0.0f;
            return false;
        }

        private void DecompressLand(Simulator simulator, BitPack bitpack, TerrainPatch.GroupHeader group)
        {
            int x;
            int y;
            int[] patches = new int[32 * 32];
            int count = 0;

            while (true)
            {
                TerrainPatch.Header header = TerrainCompressor.DecodePatchHeader(bitpack);

                if (header.QuantWBits == TerrainCompressor.END_OF_PATCHES)
                    break;

                x = header.PatchIDs >> 5;
                y = header.PatchIDs & 0x1F;

                if (x >= TerrainCompressor.PATCHES_PER_EDGE || y >= TerrainCompressor.PATCHES_PER_EDGE)
                {
                    Logger.Log(String.Format(
                        "Invalid LayerData land packet, x={0}, y={1}, dc_offset={2}, range={3}, quant_wbits={4}, patchids={5}, count={6}",
                        x, y, header.DCOffset, header.Range, header.QuantWBits, header.PatchIDs, count),
                        Helpers.LogLevel.Warning, Client);
                    return;
                }

                // Decode this patch
                TerrainCompressor.DecodePatch(patches, bitpack, header, group.PatchSize);

                // Decompress this patch
                float[] heightmap = TerrainCompressor.DecompressPatch(patches, header, group);

                count++;

                if (OnLandPatch != null)
                {
                    try { OnLandPatch(simulator, x, y, group.PatchSize, heightmap); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }

                if (Client.Settings.STORE_LAND_PATCHES)
                {
                    lock (SimPatches)
                    {
                        if (!SimPatches.ContainsKey(simulator.Handle))
                            SimPatches.Add(simulator.Handle, new TerrainPatch[16 * 16]);

                        SimPatches[simulator.Handle][y * 16 + x] = new TerrainPatch();
                        SimPatches[simulator.Handle][y * 16 + x].Heightmap = heightmap;
                    }
                }
            }
        }

        private void DecompressWind(Simulator simulator, BitPack bitpack, TerrainPatch.GroupHeader group)
        {
            ;
        }

        private void DecompressCloud(Simulator simulator, BitPack bitpack, TerrainPatch.GroupHeader group)
        {
            ;
        }

        private void LayerDataHandler(Packet packet, Simulator simulator)
        {
            LayerDataPacket layer = (LayerDataPacket)packet;
            BitPack bitpack = new BitPack(layer.LayerData.Data, 0);
            TerrainPatch.GroupHeader header = new TerrainPatch.GroupHeader();
            TerrainPatch.LayerType type = (TerrainPatch.LayerType)layer.LayerID.Type;

            // Stride
            header.Stride = bitpack.UnpackBits(16);
            // Patch size
            header.PatchSize = bitpack.UnpackBits(8);
            // Layer type
            header.Type = (TerrainPatch.LayerType)bitpack.UnpackBits(8);

            switch (type)
            {
                case TerrainPatch.LayerType.Land:
                    if (OnLandPatch != null || Client.Settings.STORE_LAND_PATCHES)
                        DecompressLand(simulator, bitpack, header);
                    break;
                case TerrainPatch.LayerType.Water:
                    Logger.Log("Got a Water LayerData packet, implement me!", Helpers.LogLevel.Error, Client);
                    break;
                case TerrainPatch.LayerType.Wind:
                    DecompressWind(simulator, bitpack, header);
                    break;
                case TerrainPatch.LayerType.Cloud:
                    DecompressCloud(simulator, bitpack, header);
                    break;
                default:
                    Logger.Log("Unrecognized LayerData type " + type.ToString(), Helpers.LogLevel.Warning, Client);
                    break;
            }
        }
    }
}
