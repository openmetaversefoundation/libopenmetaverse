/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
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
using System.IO;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Assets
{
    /// <summary>
    /// Represents Mesh asset
    /// </summary>
    public class AssetMesh : Asset
    {
        /// <summary>Override the base classes AssetType</summary>
        public override AssetType AssetType { get { return AssetType.Mesh; } }

        /// <summary>
        /// Decoded mesh data
        /// </summary>
        public OSDMap MeshData;

        /// <summary>Initializes a new instance of an AssetMesh object</summary>
        public AssetMesh() { }

        /// <summary>Initializes a new instance of an AssetMesh object with parameters</summary>
        /// <param name="assetID">A unique <see cref="UUID"/> specific to this asset</param>
        /// <param name="assetData">A byte array containing the raw asset data</param>
        public AssetMesh(UUID assetID, byte[] assetData)
            : base(assetID, assetData)
        {
        }

        /// <summary>
        /// TODO: Encodes Collada file into LLMesh format
        /// </summary>
        public override void Encode() { }

        /// <summary>
        /// Decodes mesh asset. See <see cref="OpenMetaverse.Rendering.FacetedMesh.TryDecodeFromAsset"/>
        /// to furter decode it for rendering</summary>
        /// <returns>true</returns>
        public override bool Decode()
        {
            try
            {
                MeshData = new OSDMap();

                using (MemoryStream data = new MemoryStream(AssetData))
                {
                    OSDMap header = (OSDMap)OSDParser.DeserializeLLSDBinary(data);
                    MeshData["asset_header"] = header;
                    long start = data.Position;

                    foreach(string partName in header.Keys)
                    {
                        if (header[partName].Type != OSDType.Map)
                        {
                            MeshData[partName] = header[partName];
                            continue;
                        }

                        OSDMap partInfo = (OSDMap)header[partName];
                        if (partInfo["offset"] < 0 || partInfo["size"] == 0)
                        {
                            MeshData[partName] = partInfo;
                            continue;
                        }

                        byte[] part = new byte[partInfo["size"]];
                        Buffer.BlockCopy(AssetData, partInfo["offset"] + (int)start, part, 0, part.Length);
                        MeshData[partName] = Helpers.ZDecompressOSD(part);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to decode mesh asset", Helpers.LogLevel.Error, ex);
                return false;
            }
        }
    }
}
