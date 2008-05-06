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

        public InternalDictionary<ulong, Patch[]> SimPatches = new InternalDictionary<ulong, Patch[]>();

        private const int PATCHES_PER_EDGE = 16;
        private const float OO_SQRT2 = 0.7071067811865475244008443621049f;
        private const int STRIDE = 264;

        // Bit packing codes
        private const int ZERO_CODE = 0x0;
        private const int ZERO_EOB = 0x2;
        private const int POSITIVE_VALUE = 0x6;
        private const int NEGATIVE_VALUE = 0x7;
        private const int END_OF_PATCHES = 97;

        private SecondLife Client;
        private float[] DequantizeTable16 = new float[16 * 16];
        private float[] DequantizeTable32 = new float[32 * 32];
        private float[] CosineTable16 = new float[16 * 16];
        private float[] CosineTable32 = new float[32 * 32];
        private int[] CopyMatrix16 = new int[16 * 16];
        private int[] CopyMatrix32 = new int[32 * 32];

        // Not used by clients
        private float[] QuantizeTable16 = new float[16 * 16];


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
            SetupCosines16();
            SetupCosines32();
            BuildCopyMatrix16();
            BuildCopyMatrix32();

            // Not used by clients
            BuildQuantizeTable16();

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

        /// <summary>
        /// Creates a LayerData packet for compressed land data given a full
        /// simulator heightmap and an array of indices of patches to compress
        /// </summary>
        /// <param name="heightmap">A 256 * 256 array of floating point values
        /// specifying the height at each meter in the simulator</param>
        /// <param name="patches">Array of indexes in the 16x16 grid of patches
        /// for this simulator. For example if 1 and 17 are specified, patches
        /// x=1,y=0 and x=1,y=1 are sent</param>
        /// <returns></returns>
        public LayerDataPacket CreateLandPacket(float[] heightmap, int[] patches)
        {
            LayerDataPacket layer = new LayerDataPacket();
            layer.LayerID.Type = (byte)LayerType.Land;

            GroupHeader header = new GroupHeader();
            header.Stride = STRIDE;
            header.PatchSize = 16;
            header.Type = LayerType.Land;

            byte[] data = new byte[1536];
            BitPack bitpack = new BitPack(data, 0);
            bitpack.PackBits(header.Stride, 16);
            bitpack.PackBits(header.PatchSize, 8);
            bitpack.PackBits((int)header.Type, 8);

            for (int i = 0; i < patches.Length; i++)
            {
                CreatePatch(bitpack, heightmap, patches[i] % 16, (patches[i] - (patches[i] % 16)) / 16);
            }

            bitpack.PackBits(END_OF_PATCHES, 8);

            layer.LayerData.Data = new byte[bitpack.BytePos + 1];
            Buffer.BlockCopy(bitpack.Data, 0, layer.LayerData.Data, 0, bitpack.BytePos + 1);

            return layer;
        }

        /// <summary>
        /// Add a patch of terrain to a BitPacker
        /// </summary>
        /// <param name="output">BitPacker to write the patch to</param>
        /// <param name="heightmap">Heightmap of the simulator, must be a 256 *
        /// 256 float array</param>
        /// <param name="x">X offset of the patch to create, valid values are
        /// from 0 to 15</param>
        /// <param name="y">Y offset of the patch to create, valid values are
        /// from 0 to 15</param>
        public void CreatePatch(BitPack output, float[] heightmap, int x, int y)
        {
            if (heightmap.Length != 256 * 256)
            {
                Logger.Log("Invalid heightmap value of " + heightmap.Length + " passed to CreatePatch()",
                    Helpers.LogLevel.Error, Client);
                return;
            }

            if (x < 0 || x > 15 || y < 0 || y > 15)
            {
                Logger.Log("Invalid x or y patch offset passed to CreatePatch(), x=" + x + ", y=" + y,
                    Helpers.LogLevel.Error, Client);
                return;
            }

            PatchHeader header = PrescanPatch(heightmap, x, y);
            header.QuantWBits = 136;
            header.PatchIDs = (y & 0x1F);
            header.PatchIDs += (x << 5);

            // TODO: What is prequant?
            int[] patch = CompressPatch(heightmap, x, y, header, 10);
            int wbits = EncodePatchHeader(output, header, patch);
            // TODO: What is postquant?
            EncodePatch(output, patch, 0, wbits);
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

        private void BuildQuantizeTable16()
        {
            for (int j = 0; j < 16; j++)
            {
                for (int i = 0; i < 16; i++)
                {
                    QuantizeTable16[j * 16 + i] = 1.0f / (1.0f + 2.0f * ((float)i + (float)j));
                }
            }
        }

        private void SetupCosines16()
        {
            const float hposz = (float)Math.PI * 0.5f / 16.0f;

            for (int u = 0; u < 16; u++)
            {
                for (int n = 0; n < 16; n++)
                {
                    CosineTable16[u * 16 + n] = (float)Math.Cos((2.0f * (float)n + 1.0f) * (float)u * hposz);
                }
            }
        }

        private void SetupCosines32()
        {
            const float hposz = (float)Math.PI * 0.5f / 32.0f;

            for (int u = 0; u < 32; u++)
            {
                for (int n = 0; n < 32; n++)
                {
                    CosineTable32[u * 32 + n] = (float)Math.Cos((2.0f * (float)n + 1.0f) * (float)u * hposz);
                }
            }
        }

        private void BuildCopyMatrix16()
        {
            bool diag = false;
            bool right = true;
            int i = 0;
            int j = 0;
            int count = 0;

            while (i < 16 && j < 16)
            {
                CopyMatrix16[j * 16 + i] = count++;

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

        private void BuildCopyMatrix32()
        {
            bool diag = false;
            bool right = true;
            int i = 0;
            int j = 0;
            int count = 0;

            while (i < 32 && j < 32)
            {
                CopyMatrix32[j * 32 + i] = count++;

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

        private PatchHeader PrescanPatch(float[] heightmap, int patchX, int patchY)
        {
            PatchHeader header = new PatchHeader();
            float zmax = -99999999.0f;
            float zmin = 99999999.0f;

            for (int j = patchY * 16; j < (patchY + 1) * 16; j++)
            {
                for (int i = patchX * 16; i < (patchX + 1) * 16; i++)
                {
                    if (heightmap[j * 256 + i] > zmax) zmax = heightmap[j * 256 + i];
                    if (heightmap[j * 256 + i] < zmin) zmin = heightmap[j * 256 + i];
                }
            }

            header.DCOffset = zmin;
            header.Range = (int)((zmax - zmin) + 1.0f);

            return header;
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
            header.WordBits = (uint)((header.QuantWBits & 0x0f) + 2);

            return header;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="header"></param>
        /// <param name="patch"></param>
        /// <returns>wbits</returns>
        private int EncodePatchHeader(BitPack output, PatchHeader header, int[] patch)
        {
            int temp;
            int wbits = (header.QuantWBits & 0x0f) + 2;
            uint maxWbits = (uint)wbits + 5;
            uint minWbits = ((uint)wbits >> 1);

            wbits = (int)minWbits;

            for (int i = 0; i < patch.Length; i++)
            {
                temp = patch[i];

                if (temp != 0)
                {
                    // Get the absolute value
                    if (temp < 0) temp *= -1;

                    for (int j = (int)maxWbits; j > (int)minWbits; j--)
                    {
                        if ((temp & (1 << j)) != 0)
                        {
                            if (j > wbits) wbits = j;
                            break;
                        }
                    }
                }
            }

            wbits += 1;

            header.QuantWBits &= 0xf0;

            if (wbits > 17 || wbits < 2)
            {
                Logger.Log("Bits needed per word in EncodePatchHeader() are outside the allowed range", 
                    Helpers.LogLevel.Error, Client);
            }

            header.QuantWBits |= (wbits - 2);

            output.PackBits(header.QuantWBits, 8);
            output.PackFloat(header.DCOffset);
            output.PackBits(header.Range, 16);
            output.PackBits(header.PatchIDs, 10);

            return wbits;
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
                    total += linein[usize + column] * CosineTable16[usize + n];
                }

                lineout[16 * n + column] = total;
            }
        }

/*        private void IDCTColumn32(float[] linein, float[] lineout, int column)
        {
            float total;
            int usize;

            for (int n = 0; n < 32; n++)
            {
                total = OO_SQRT2 * linein[column];

                for (int u = 1; u < 32; u++)
                {
                    usize = u * 32;
                    total += linein[usize + column] * CosineTable32[usize + n];
                }

                lineout[32 * n + column] = total;
            }
        }
*/
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
                    total += linein[lineSize + u] * CosineTable16[u * 16 + n];
                }

                lineout[lineSize + n] = total * oosob;
            }
        }

 /*       private void IDCTLine32(float[] linein, float[] lineout, int line)
        {
            const float oosob = 2.0f / 32.0f;
            int lineSize = line * 32;
            float total;

            for (int n = 0; n < 32; n++)
            {
                total = OO_SQRT2 * linein[lineSize];

                for (int u = 1; u < 32; u++)
                {
                    total += linein[lineSize + u] * CosineTable32[u * 32 + n];
                }

                lineout[lineSize + n] = total * oosob;
            }
        } */

        private void DCTLine16(float[] linein, float[] lineout, int line)
        {
            float total = 0.0f;
            int lineSize = line * 16;

            for (int n = 0; n < 16; n++)
            {
                total += linein[lineSize + n];
            }

            lineout[lineSize] = OO_SQRT2 * total;

            for (int u = 1; u < 16; u++)
            {
                total = 0.0f;

                for (int n = 0; n < 16; n++)
                {
                    total += linein[lineSize + n] * CosineTable16[u * 16 + n];
                }

                lineout[lineSize + u] = total;
            }
        }

        private void DCTColumn16(float[] linein, int[] lineout, int column)
        {
            float total = 0.0f;
            const float oosob = 2.0f / 16.0f;

            for (int n = 0; n < 16; n++)
            {
                total += linein[16 * n + column];
            }

            lineout[CopyMatrix16[column]] = (int)(OO_SQRT2 * total * oosob * QuantizeTable16[column]);

            for (int u = 1; u < 16; u++)
            {
                total = 0.0f;

                for (int n = 0; n < 16; n++)
                {
                    total += linein[16 * n + column] * CosineTable16[u * 16 + n];
                }

                lineout[CopyMatrix16[16 * u + column]] = (int)(total * oosob * QuantizeTable16[16 * u + column]);
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

        private void EncodePatch(BitPack output, int[] patch, int postquant, int wbits)
        {
            int temp;
            bool eob;

            if (postquant > 16 * 16 || postquant < 0)
            {
                Logger.Log("Postquant is outside the range of allowed values in EncodePatch()", Helpers.LogLevel.Error, Client);
                return;
            }

            if (postquant != 0) patch[16 * 16 - postquant] = 0;

            for (int i = 0; i < 16 * 16; i++)
            {
                eob = false;
                temp = patch[i];

                if (temp == 0)
                {
                    eob = true;

                    for (int j = i; j < 16 * 16 - postquant; j++)
                    {
                        if (patch[j] != 0)
                        {
                            eob = false;
                            break;
                        }
                    }

                    if (eob)
                    {
                        output.PackBits(ZERO_EOB, 2);
                        return;
                    }
                    else
                    {
                        output.PackBits(ZERO_CODE, 1);
                    }
                }
                else
                {
                    if (temp < 0)
                    {
                        temp *= -1;

                        if (temp > (1 << wbits)) temp = (1 << wbits);

                        output.PackBits(NEGATIVE_VALUE, 3);
                        output.PackBits(temp, wbits);
                    }
                    else
                    {
                        if (temp > (1 << wbits)) temp = (1 << wbits);

                        output.PackBits(POSITIVE_VALUE, 3);
                        output.PackBits(temp, wbits);
                    }
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
                    block[n] = patches[CopyMatrix16[n]] * DequantizeTable16[n];
                }

                float[] ftemp = new float[16 * 16];

                for (int o = 0; o < 16; o++)
                    IDCTColumn16(block, ftemp, o);
                for (int o = 0; o < 16; o++)
                    IDCTLine16(ftemp, block, o);
            }
            else
            {
                for (int n = 0; n < 32 * 32; n++)
                {
                    block[n] = patches[CopyMatrix32[n]] * DequantizeTable32[n];
                }

                Logger.Log("Implement IDCTPatchLarge", Helpers.LogLevel.Error, Client);
            }

            for (int j = 0; j < block.Length; j++)
            {
                output[j] = block[j] * mult + addval;
            }

            return output;
        }

        private int[] CompressPatch(float[] heightmap, int patchX, int patchY, PatchHeader header, int prequant)
        {
            float[] block = new float[16 * 16];
            int wordsize = prequant;
            float oozrange = 1.0f / (float)header.Range;
            float range = (float)(1 << prequant);
            float premult = oozrange * range;
            float sub = (float)(1 << (prequant - 1)) + header.DCOffset * premult;

            header.QuantWBits = wordsize - 2;
            header.QuantWBits |= (prequant - 2) << 4;

            int k = 0;
            for (int j = patchY * 16; j < (patchY + 1) * 16; j++)
            {
                for (int i = patchX * 16; i < (patchX + 1) * 16; i++)
                {
                    block[k++] = heightmap[j * 256 + i] * premult - sub;
                }
            }

            float[] ftemp = new float[16 * 16];
            int[] itemp = new int[16 * 16];

            for (int o = 0; o < 16; o++)
                DCTLine16(block, ftemp, o);
            for (int o = 0; o < 16; o++)
                DCTColumn16(ftemp, itemp, o);

            return itemp;
        }

        private void DecompressLand(Simulator simulator, BitPack bitpack, GroupHeader group)
        {
            int x;
            int y;
            int[] patches = new int[32 * 32];
            int count = 0;

            while (true)
            {
                PatchHeader header = DecodePatchHeader(bitpack);

                if (header.QuantWBits == END_OF_PATCHES)
                    break;

                x = header.PatchIDs >> 5;
                y = header.PatchIDs & 0x1F;

                if (x >= PATCHES_PER_EDGE || y >= PATCHES_PER_EDGE)
                {
                    Logger.Log("Invalid LayerData land packet, x = " + x + ", y = " + y + ", dc_offset = " +
                        header.DCOffset + ", range = " + header.Range + ", quant_wbits = " + header.QuantWBits +
                        ", patchids = " + header.PatchIDs + ", count = " + count, Helpers.LogLevel.Warning, Client);
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
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }

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

            switch (type)
            {
                case LayerType.Land:
                    if (OnLandPatch != null || Client.Settings.STORE_LAND_PATCHES)
                        DecompressLand(simulator, bitpack, header);
                    break;
                case LayerType.Water:
                    Logger.Log("Got a Water LayerData packet, implement me!", Helpers.LogLevel.Error, Client);
                    break;
                case LayerType.Wind:
                    DecompressWind(simulator, bitpack, header);
                    break;
                case LayerType.Cloud:
                    DecompressCloud(simulator, bitpack, header);
                    break;
                default:
                    Logger.Log("Unrecognized LayerData type " + type.ToString(), Helpers.LogLevel.Warning, Client);
                    break;
            }
        }
    }
}
