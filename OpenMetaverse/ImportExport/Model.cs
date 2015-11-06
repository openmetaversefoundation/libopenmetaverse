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
using System.Collections.Generic;
using OpenMetaverse.Rendering;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.ImportExport
{

    public class ModelMaterial
    {
        public string ID;
        public Color4 DiffuseColor = Color4.White;
        public string Texture;
        public byte[] TextureData;
    }

    public class ModelFace
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public List<uint> Indices = new List<uint>();
        public string MaterialID = string.Empty;
        public ModelMaterial Material = new ModelMaterial();

        Dictionary<Vertex, int> LookUp = new Dictionary<Vertex, int>();

        public void AddVertex(Vertex v)
        {
            int index;

            if (LookUp.ContainsKey(v))
            {
                index = LookUp[v];
            }
            else
            {
                index = Vertices.Count;
                Vertices.Add(v);
                LookUp[v] = index;
            }

            Indices.Add((uint)index);
        }

    }

    public class ModelPrim
    {
        public List<Vector3> Positions;
        public Vector3 BoundMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public Vector3 BoundMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation = Quaternion.Identity;
        public List<ModelFace> Faces = new List<ModelFace>();
        public string ID;
        public byte[] Asset;

        public void CreateAsset(UUID creator)
        {
            OSDMap header = new OSDMap();
            header["version"] = 1;
            header["creator"] = creator;
            header["date"] = DateTime.Now;

            OSDArray faces = new OSDArray();
            foreach (var face in Faces)
            {
                OSDMap faceMap = new OSDMap();

                // Find UV min/max
                Vector2 uvMin = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 uvMax = new Vector2(float.MinValue, float.MinValue);
                foreach (var v in face.Vertices)
                {
                    if (v.TexCoord.X < uvMin.X) uvMin.X = v.TexCoord.X;
                    if (v.TexCoord.Y < uvMin.Y) uvMin.Y = v.TexCoord.Y;

                    if (v.TexCoord.X > uvMax.X) uvMax.X = v.TexCoord.X;
                    if (v.TexCoord.Y > uvMax.Y) uvMax.Y = v.TexCoord.Y;
                }
                OSDMap uvDomain = new OSDMap();
                uvDomain["Min"] = uvMin;
                uvDomain["Max"] = uvMax;
                faceMap["TexCoord0Domain"] = uvDomain;


                OSDMap positionDomain = new OSDMap();
                positionDomain["Min"] = new Vector3(-0.5f, -0.5f, -0.5f);
                positionDomain["Max"] = new Vector3(0.5f, 0.5f, 0.5f);
                faceMap["PositionDomain"] = positionDomain;

                List<byte> posBytes = new List<byte>(face.Vertices.Count * sizeof(UInt16) * 3);
                List<byte> norBytes = new List<byte>(face.Vertices.Count * sizeof(UInt16) * 3);
                List<byte> uvBytes = new List<byte>(face.Vertices.Count * sizeof(UInt16) * 2);

                foreach (var v in face.Vertices)
                {
                    posBytes.AddRange(Utils.UInt16ToBytes(Utils.FloatToUInt16(v.Position.X, -0.5f, 0.5f)));
                    posBytes.AddRange(Utils.UInt16ToBytes(Utils.FloatToUInt16(v.Position.Y, -0.5f, 0.5f)));
                    posBytes.AddRange(Utils.UInt16ToBytes(Utils.FloatToUInt16(v.Position.Z, -0.5f, 0.5f)));

                    norBytes.AddRange(Utils.UInt16ToBytes(Utils.FloatToUInt16(v.Normal.X, -1f, 1f)));
                    norBytes.AddRange(Utils.UInt16ToBytes(Utils.FloatToUInt16(v.Normal.Y, -1f, 1f)));
                    norBytes.AddRange(Utils.UInt16ToBytes(Utils.FloatToUInt16(v.Normal.Z, -1f, 1f)));

                    uvBytes.AddRange(Utils.UInt16ToBytes(Utils.FloatToUInt16(v.TexCoord.X, uvMin.X, uvMax.X)));
                    uvBytes.AddRange(Utils.UInt16ToBytes(Utils.FloatToUInt16(v.TexCoord.Y, uvMin.Y, uvMax.Y)));
                }

                faceMap["Position"] = posBytes.ToArray();
                faceMap["Normal"] = norBytes.ToArray();
                faceMap["TexCoord0"] = uvBytes.ToArray();

                List<byte> indexBytes = new List<byte>(face.Indices.Count * sizeof(UInt16));
                foreach (var t in face.Indices)
                {
                    indexBytes.AddRange(Utils.UInt16ToBytes((ushort)t));
                }
                faceMap["TriangleList"] = indexBytes.ToArray();

                faces.Add(faceMap);
            }

            byte[] physicStubBytes = Helpers.ZCompressOSD(PhysicsStub());

            byte[] meshBytes = Helpers.ZCompressOSD(faces);
            int n = 0;

            OSDMap lodParms = new OSDMap();
            lodParms["offset"] = n;
            lodParms["size"] = meshBytes.Length;
            header["high_lod"] = lodParms;
            n += meshBytes.Length;

            lodParms = new OSDMap();
            lodParms["offset"] = n;
            lodParms["size"] = physicStubBytes.Length;
            header["physics_convex"] = lodParms;
            n += physicStubBytes.Length;

            byte[] headerBytes = OSDParser.SerializeLLSDBinary(header, false);
            n += headerBytes.Length;

            Asset = new byte[n];

            int offset = 0;
            Buffer.BlockCopy(headerBytes, 0, Asset, offset, headerBytes.Length);
            offset += headerBytes.Length;

            Buffer.BlockCopy(meshBytes, 0, Asset, offset, meshBytes.Length);
            offset += meshBytes.Length;

            Buffer.BlockCopy(physicStubBytes, 0, Asset, offset, physicStubBytes.Length);
            offset += physicStubBytes.Length;

        }

        public static OSD PhysicsStub()
        {
            OSDMap ret = new OSDMap();
            ret["Max"] = new Vector3(0.5f, 0.5f, 0.5f);
            ret["Min"] = new Vector3(-0.5f, -0.5f, -0.5f);
            ret["BoundingVerts"] = new byte[] { 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 255, 255, 255, 127, 0, 0, 255, 255, 255, 127, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 0, 0, 255, 255, 0, 0, 255, 127, 255, 255, 255, 255, 255, 255, 0, 0, 255, 255, 255, 255, 255, 255, 0, 0, 0, 0, 255, 255, 0, 0, 255, 255 };
            return ret;
        }

    }
}