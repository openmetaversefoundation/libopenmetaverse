/*
 * Copyright (c) 2007, Second Life Reverse Engineering Team
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the Second Life Reverse Engineering Team nor the names 
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
using libsecondlife.Packets;

namespace libsecondlife
{
    public class TerrainManager
    {
        public enum LayerType : byte
        {
            Land = 0x4C,
            Water = 0x57,
            Wind = 0x37,
            Cloud = 0x38
        }

        public struct GroupHeader
        {
            public int Stride;
            public int PatchSize;
            public LayerType Type;
        }

        public struct PatchHeader
        {
            public float DCOffset;
            public int Range;
            public int QuantWBits;
            public int PatchIDs;
            public uint WordBits;
        }

        public class Patch
        {
            public float[] Heightmap;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="simulator"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="data"></param>
        public delegate void LandPatchCallback(Simulator simulator, int x, int y, int width, float[] data);


        /// <summary>
        /// 
        /// </summary>
        public event LandPatchCallback OnLandPatch;


        private const byte END_OF_PATCHES = 97;
        private const int PATCHES_PER_EDGE = 16;
        private const float OO_SQRT2 = 0.7071067811865475244008443621049f;

        private SecondLife Client;
        private Dictionary<ulong, Patch[]> SimPatches = new Dictionary<ulong, Patch[]>();
        private float[] DequantizeTable16 = new float[16 * 16];
        private float[] DequantizeTable32 = new float[32 * 32];
        private float[] ICosineTable16 = new float[16 * 16];
        private float[] ICosineTable32 = new float[32 * 32];
        private int[] DeCopyMatrix16 = new int[16 * 16];
        private int[] DeCopyMatrix32 = new int[32 * 32];


        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public TerrainManager(SecondLife client)
        {
            Client = client;

            // Initialize the decompression tables
            BuildDequantizeTable16();
            BuildDequantizeTable32();
            SetupICosines16();
            SetupICosines32();
            BuildDecopyMatrix16();
            BuildDecopyMatrix32();

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
            if (x > 0 && x < 256 && y > 0 && y < 256)
            {
                lock (SimPatches)
                {
                    if (SimPatches.ContainsKey(regionHandle))
                    {
                        int patchX = (int)Math.DivRem(x, 16, out x);
                        int patchY = (int)Math.DivRem(y, 16, out y);

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

        private void BuildDequantizeTable16()
        {
            for (int j = 0; j < 16; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    DequantizeTable16[j * 16 + i] = 1.0f + 2.0f * (float)(i + j);
                }
            }
        }

        private void BuildDequantizeTable32()
        {
            for (int j = 0; j < 32; j++)
            {
                for (int i = 0; i < 32; i++)
                {
                    DequantizeTable32[j * 32 + i] = 1.0f + 2.0f * (float)(i + j);
                }
            }
        }

        private void SetupICosines16()
        {
            const float hposz = (float)Math.PI * 0.5f / 16.0f;

            for (int u = 0; u < 16; u++)
            {
                for (int n = 0; n < 16; n++)
                {
                    ICosineTable16[u * 16 + n] = (float)Math.Cos((2.0f * (float)n + 1.0f) * (float)u * hposz);
                }
            }
        }

        private void SetupICosines32()
        {
            const float hposz = (float)Math.PI * 0.5f / 32.0f;

            for (int u = 0; u < 32; u++)
            {
                for (int n = 0; n < 32; n++)
                {
                    ICosineTable32[u * 32 + n] = (float)Math.Cos((2.0f * (float)n + 1.0f) * (float)u * hposz);
                }
            }
        }

        private void BuildDecopyMatrix16()
        {
            bool diag = false;
            bool right = true;
            int i = 0;
            int j = 0;
            int count = 0;

            while (i < 16 && j < 16)
            {
                DeCopyMatrix16[j * 16 + i] = count++;

                if (!diag)
                {
                    if (right)
                    {
                        if (i < 16 - 1) i++;
                        else j++;

                        right = false;
                        diag = true;
                    }
                    else
                    {
                        if (j < 16 - 1) j++;
                        else i++;

                        right = true;
                        diag = true;
                    }
                }
                else
                {
                    if (right)
                    {
                        i++;
                        j--;
                        if (i == 16 - 1 || j == 0) diag = false;
                    }
                    else
                    {
                        i--;
                        j++;
                        if (j == 16 - 1 || i == 0) diag = false;
                    }
                }
            }
        }

        private void BuildDecopyMatrix32()
        {
            bool diag = false;
            bool right = true;
            int i = 0;
            int j = 0;
            int count = 0;

            while (i < 32 && j < 32)
            {
                DeCopyMatrix32[j * 32 + i] = count++;

                if (!diag)
                {
                    if (right)
                    {
                        if (i < 32 - 1) i++;
                        else j++;

                        right = false;
                        diag = true;
                    }
                    else
                    {
                        if (j < 32 - 1) j++;
                        else i++;

                        right = true;
                        diag = true;
                    }
                }
                else
                {
                    if (right)
                    {
                        i++;
                        j--;
                        if (i == 32 - 1 || j == 0) diag = false;
                    }
                    else
                    {
                        i--;
                        j++;
                        if (j == 32 - 1 || i == 0) diag = false;
                    }
                }
            }
        }

        private PatchHeader DecodePatchHeader(BitPack bitpack)
        {
            PatchHeader header = new PatchHeader();

            // Quantized word bits
            header.QuantWBits = bitpack.UnpackBits(8);
            if (header.QuantWBits == END_OF_PATCHES)
                return header;

            // DC offset
            header.DCOffset = bitpack.UnpackFloat();

            // Range
            header.Range = bitpack.UnpackBits(16);

            // Patch IDs (10 bits)
            header.PatchIDs = bitpack.UnpackBits(10);

            // Word bits
            header.WordBits = (uint)((header.QuantWBits & 0xf) + 2);

            return header;
        }

        private void IDCTColumn16(float[] linein, float[] lineout, int column)
        {
            float total;
            int usize;

            for (int n = 0; n < 16; n++)
            {
                total = OO_SQRT2 * linein[column];

                for (int u = 1; u < 16; u++)
                {
                    usize = u * 16;
                    total += linein[usize + column] * ICosineTable16[usize + n];
                }

                lineout[16 * n + column] = total;
            }
        }

        private void IDCTColumn32(float[] linein, float[] lineout, int column)
        {
            float total;
            int usize;

            for (int n = 0; n < 32; n++)
            {
                total = OO_SQRT2 * linein[column];

                for (int u = 1; u < 32; u++)
                {
                    usize = u * 32;
                    total += linein[usize + column] * ICosineTable32[usize + n];
                }

                lineout[32 * n + column] = total;
            }
        }

        private void IDCTLine16(float[] linein, float[] lineout, int line)
        {
            const float oosob = 2.0f / 16.0f;
            int lineSize = line * 16;
            float total;

            for (int n = 0; n < 16; n++)
            {
                total = OO_SQRT2 * linein[lineSize];

                for (int u = 1; u < 16; u++)
                {
                    total += linein[lineSize + u] * ICosineTable16[u * 16 + n];
                }

                lineout[lineSize + n] = total * oosob;
            }
        }

        private void IDCTLine32(float[] linein, float[] lineout, int line)
        {
            const float oosob = 2.0f / 32.0f;
            int lineSize = line * 32;
            float total;

            for (int n = 0; n < 32; n++)
            {
                total = OO_SQRT2 * linein[lineSize];

                for (int u = 1; u < 32; u++)
                {
                    total += linein[lineSize + u] * ICosineTable32[u * 32 + n];
                }

                lineout[lineSize + n] = total * oosob;
            }
        }

        private void DecodePatch(int[] patches, BitPack bitpack, PatchHeader header, int size)
        {
            int temp;
            for (int n = 0; n < size * size; n++)
            {
                // ?
                temp = bitpack.UnpackBits(1);
                if (temp != 0)
                {
                    // Value or EOB
                    temp = bitpack.UnpackBits(1);
                    if (temp != 0)
                    {
                        // Value
                        temp = bitpack.UnpackBits(1);
                        if (temp != 0)
                        {
                            // Negative
                            temp = bitpack.UnpackBits((int)header.WordBits);
                            patches[n] = temp * -1;
                        }
                        else
                        {
                            // Positive
                            temp = bitpack.UnpackBits((int)header.WordBits);
                            patches[n] = temp;
                        }
                    }
                    else
                    {
                        // Set the rest to zero
                        // TODO: This might not be necessary
                        for (int o = n; o < size * size; o++)
                        {
                            patches[o] = 0;
                        }
                        break;
                    }
                }
                else
                {
                    patches[n] = 0;
                }
            }
        }

        private float[] DecompressPatch(int[] patches, PatchHeader header, GroupHeader group)
        {
            float[] block = new float[group.PatchSize * group.PatchSize];
            float[] output = new float[group.PatchSize * group.PatchSize];
            int prequant = (header.QuantWBits >> 4) + 2;
            int quantize = 1 << prequant;
            float ooq = 1.0f / (float)quantize;
            float mult = ooq * (float)header.Range;
            float addval = mult * (float)(1 << (prequant - 1)) + header.DCOffset;

            if (group.PatchSize == 16)
            {
                for (int n = 0; n < 16 * 16; n++)
                {
                    block[n] = patches[DeCopyMatrix16[n]] * DequantizeTable16[n];
                }

                float[] ftemp = new float[32 * 32];

                for (int o = 0; o < 16; o++)
                    IDCTColumn16(block, ftemp, o);
                for (int o = 0; o < 16; o++)
                    IDCTLine16(ftemp, block, o);
            }
            else
            {
                for (int n = 0; n < 32 * 32; n++)
                {
                    block[n] = patches[DeCopyMatrix32[n]] * DequantizeTable32[n];
                }

                //IDCTPatchLarge(block);
                Client.Log("Implement IDCTPatchLarge", Helpers.LogLevel.Warning);
            }

            for (int j = 0; j < block.Length; j++)
            {
                output[j] = block[j] * mult + addval;
            }

            return output;
        }

        private void DecompressLand(Simulator simulator, BitPack bitpack, GroupHeader group)
        {
            int x;
            int y;
            int[] patches = new int[32 * 32];
            int count = 0;
            //group.Stride = 256;

            while (true)
            {
                PatchHeader header = DecodePatchHeader(bitpack);

                if (header.QuantWBits == END_OF_PATCHES)
                    break;

                x = header.PatchIDs >> 5;
                y = header.PatchIDs & 0x1F;

                if (x >= PATCHES_PER_EDGE || y >= PATCHES_PER_EDGE)
                {
                    Client.Log("Invalid LayerData land packet, x = " + x + ", y = " + y + ", dc_offset = " +
                        header.DCOffset + ", range = " + header.Range + ", quant_wbits = " + header.QuantWBits +
                        ", patchids = " + header.PatchIDs + ", count = " + count, Helpers.LogLevel.Warning);
                    return;
                }

                // Decode this patch
                DecodePatch(patches, bitpack, header, group.PatchSize);

                // Decompress this patch
                float[] heightmap = DecompressPatch(patches, header, group);

                count++;

                if (OnLandPatch != null)
                {
                    try { OnLandPatch(simulator, x, y, group.PatchSize, heightmap); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }

                    if (Client.Settings.STORE_LAND_PATCHES)
                    {
                        lock (SimPatches)
                        {
                            if (!SimPatches.ContainsKey(simulator.Handle))
                                SimPatches.Add(simulator.Handle, new Patch[16 * 16]);

                            SimPatches[simulator.Handle][y * 16 + x] = new Patch();
                            SimPatches[simulator.Handle][y * 16 + x].Heightmap = heightmap;
                        }
                    }
                }
            }
        }

        private void DecompressWind(Simulator simulator, BitPack bitpack, GroupHeader group)
        {
            ;
        }

        private void DecompressCloud(Simulator simulator, BitPack bitpack, GroupHeader group)
        {
            ;
        }

        private void LayerDataHandler(Packet packet, Simulator simulator)
        {
            LayerDataPacket layer = (LayerDataPacket)packet;
            BitPack bitpack = new BitPack(layer.LayerData.Data, 0);
            GroupHeader header = new GroupHeader();
            LayerType type = (LayerType)layer.LayerID.Type;

            // Stride
            header.Stride = bitpack.UnpackBits(16);
            // Patch size
            header.PatchSize = bitpack.UnpackBits(8);
            // Layer type
            header.Type = (LayerType)bitpack.UnpackBits(8);

            if (type != header.Type)
                Client.DebugLog("LayerData: LayerID.Type " + type.ToString() + " does not match decoded type " +
                    header.Type.ToString());

            switch (type)
            {
                case LayerType.Land:
                    if (OnLandPatch != null) DecompressLand(simulator, bitpack, header);
                    break;
                case LayerType.Water:
                    Client.Log("Got a Water LayerData packet, implement me!", Helpers.LogLevel.Info);
                    break;
                case LayerType.Wind:
                    DecompressWind(simulator, bitpack, header);
                    break;
                case LayerType.Cloud:
                    DecompressCloud(simulator, bitpack, header);
                    break;
                default:
                    Client.Log("Unrecognized LayerData type " + type.ToString(), Helpers.LogLevel.Warning);
                    break;
            }
        }
    }
}
