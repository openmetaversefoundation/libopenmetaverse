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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenMetaverse.Assets;
using OpenMetaverse.StructuredData;

// The common elements shared between rendering plugins are defined here

namespace OpenMetaverse.Rendering
{
    #region Enums

    public enum FaceType : ushort
    {
        PathBegin = 0x1 << 0,
        PathEnd = 0x1 << 1,
        InnerSide = 0x1 << 2,
        ProfileBegin = 0x1 << 3,
        ProfileEnd = 0x1 << 4,
        OuterSide0 = 0x1 << 5,
        OuterSide1 = 0x1 << 6,
        OuterSide2 = 0x1 << 7,
        OuterSide3 = 0x1 << 8
    }

    [Flags]
    public enum FaceMask
    {
        Single = 0x0001,
        Cap = 0x0002,
        End = 0x0004,
        Side = 0x0008,
        Inner = 0x0010,
        Outer = 0x0020,
        Hollow = 0x0040,
        Open = 0x0080,
        Flat = 0x0100,
        Top = 0x0200,
        Bottom = 0x0400
    }

    public enum DetailLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Highest = 3
    }

    #endregion Enums

    #region Structs

    [StructLayout(LayoutKind.Explicit)]
    public struct Vertex : IEquatable<Vertex>
    {
        [FieldOffset(0)]
        public Vector3 Position;
        [FieldOffset(12)]
        public Vector3 Normal;
        [FieldOffset(24)]
        public Vector2 TexCoord;

        public override string ToString()
        {
            return String.Format("P: {0} N: {1} T: {2}", Position, Normal, TexCoord);
        }

        public override int GetHashCode()
        {
            int hash = Position.GetHashCode();
            hash = hash * 31 + Normal.GetHashCode();
            hash = hash * 31 + TexCoord.GetHashCode();
            return hash;
        }

        public static bool operator ==(Vertex value1, Vertex value2)
        {
            return value1.Position == value2.Position
                && value1.Normal == value2.Normal
                && value1.TexCoord == value2.TexCoord;
        }

        public static bool operator !=(Vertex value1, Vertex value2)
        {
            return !(value1 == value2);
        }

        public override bool Equals(object obj)
        {
            return (obj is Vertex) ? this == (Vertex)obj : false;
        }

        public bool Equals(Vertex other)
        {
            return this == other;
        }
    }

    public struct ProfileFace
    {
        public int Index;
        public int Count;
        public float ScaleU;
        public bool Cap;
        public bool Flat;
        public FaceType Type;

        public override string ToString()
        {
            return Type.ToString();
        }
    };

    public struct Profile
    {
        public float MinX;
        public float MaxX;
        public bool Open;
        public bool Concave;
        public int TotalOutsidePoints;
        public List<Vector3> Positions;
        public List<ProfileFace> Faces;
    }

    public struct PathPoint
    {
        public Vector3 Position;
        public Vector2 Scale;
        public Quaternion Rotation;
        public float TexT;
    }

    public struct Path
    {
        public List<PathPoint> Points;
        public bool Open;
    }

    public struct Face
    {
        // Only used for Inner/Outer faces
        public int BeginS;
        public int BeginT;
        public int NumS;
        public int NumT;

        public int ID;
        public Vector3 Center;
        public Vector3 MinExtent;
        public Vector3 MaxExtent;
        public List<Vertex> Vertices;
        public List<ushort> Indices;
        public List<int> Edge;
        public FaceMask Mask;
        public Primitive.TextureEntryFace TextureFace;
        public object UserData;

        public override string ToString()
        {
            return Mask.ToString();
        }
    }

    #endregion Structs

    #region Exceptions

    public class RenderingException : Exception
    {
        public RenderingException(string message)
            : base(message)
        {
        }

        public RenderingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    #endregion Exceptions

    #region Mesh Classes

    public class Mesh
    {
        public Primitive Prim;
        public Path Path;
        public Profile Profile;

        public override string ToString()
        {
            if (Prim.Properties != null && !String.IsNullOrEmpty(Prim.Properties.Name))
            {
                return Prim.Properties.Name;
            }
            else
            {
                return String.Format("{0} ({1})", Prim.LocalID, Prim.PrimData);
            }
        }
    }

    /// <summary>
    /// Contains all mesh faces that belong to a prim
    /// </summary>
    public class FacetedMesh : Mesh
    {
        /// <summary>List of primitive faces</summary>
        public List<Face> Faces;

        /// <summary>
        /// Decodes mesh asset into FacetedMesh
        /// </summary>
        /// <param name="prim">Mesh primitive</param>
        /// <param name="meshAsset">Asset retrieved from the asset server</param>
        /// <param name="LOD">Level of detail</param>
        /// <param name="mesh">Resulting decoded FacetedMesh</param>
        /// <returns>True if mesh asset decoding was successful</returns>
        public static bool TryDecodeFromAsset(Primitive prim, AssetMesh meshAsset, DetailLevel LOD, out FacetedMesh mesh)
        {
            mesh = null;

            try
            {
                if (!meshAsset.Decode())
                {
                    return false;
                }

                OSDMap MeshData = meshAsset.MeshData;

                mesh = new FacetedMesh();

                mesh.Faces = new List<Face>();
                mesh.Prim = prim;
                mesh.Profile.Faces = new List<ProfileFace>();
                mesh.Profile.Positions = new List<Vector3>();
                mesh.Path.Points = new List<PathPoint>();

                OSD facesOSD = null;

                switch (LOD)
                {
                    default:
                    case DetailLevel.Highest:
                        facesOSD = MeshData["high_lod"];
                        break;

                    case DetailLevel.High:
                        facesOSD = MeshData["medium_lod"];
                        break;

                    case DetailLevel.Medium:
                        facesOSD = MeshData["low_lod"];
                        break;

                    case DetailLevel.Low:
                        facesOSD = MeshData["lowest_lod"];
                        break;
                }

                if (facesOSD == null || !(facesOSD is OSDArray))
                {
                    return false;
                }

                OSDArray decodedMeshOsdArray = (OSDArray)facesOSD;

                for (int faceNr = 0; faceNr < decodedMeshOsdArray.Count; faceNr++)
                {
                    OSD subMeshOsd = decodedMeshOsdArray[faceNr];

                    // Decode each individual face
                    if (subMeshOsd is OSDMap)
                    {
                        Face oface = new Face();
                        oface.ID = faceNr;
                        oface.Vertices = new List<Vertex>();
                        oface.Indices = new List<ushort>();
                        oface.TextureFace = prim.Textures.GetFace((uint)faceNr);

                        OSDMap subMeshMap = (OSDMap)subMeshOsd;

                        Vector3 posMax;
                        Vector3 posMin;

                        // If PositionDomain is not specified, the default is from -0.5 to 0.5
                        if (subMeshMap.ContainsKey("PositionDomain"))
                        {
                            posMax = ((OSDMap)subMeshMap["PositionDomain"])["Max"];
                            posMin = ((OSDMap)subMeshMap["PositionDomain"])["Min"];
                        }
                        else
                        {
                            posMax = new Vector3(0.5f, 0.5f, 0.5f);
                            posMin = new Vector3(-0.5f, -0.5f, -0.5f);
                        }

                        // Vertex positions
                        byte[] posBytes = subMeshMap["Position"];

                        // Normals
                        byte[] norBytes = null;
                        if (subMeshMap.ContainsKey("Normal"))
                        {
                            norBytes = subMeshMap["Normal"];
                        }

                        // UV texture map
                        Vector2 texPosMax = Vector2.Zero;
                        Vector2 texPosMin = Vector2.Zero;
                        byte[] texBytes = null;
                        if (subMeshMap.ContainsKey("TexCoord0"))
                        {
                            texBytes = subMeshMap["TexCoord0"];
                            texPosMax = ((OSDMap)subMeshMap["TexCoord0Domain"])["Max"];
                            texPosMin = ((OSDMap)subMeshMap["TexCoord0Domain"])["Min"];
                        }

                        // Extract the vertex position data
                        // If present normals and texture coordinates too
                        for (int i = 0; i < posBytes.Length; i += 6)
                        {
                            ushort uX = Utils.BytesToUInt16(posBytes, i);
                            ushort uY = Utils.BytesToUInt16(posBytes, i + 2);
                            ushort uZ = Utils.BytesToUInt16(posBytes, i + 4);

                            Vertex vx = new Vertex();

                            vx.Position = new Vector3(
                                Utils.UInt16ToFloat(uX, posMin.X, posMax.X),
                                Utils.UInt16ToFloat(uY, posMin.Y, posMax.Y),
                                Utils.UInt16ToFloat(uZ, posMin.Z, posMax.Z));

                            if (norBytes != null && norBytes.Length >= i + 4)
                            {
                                ushort nX = Utils.BytesToUInt16(norBytes, i);
                                ushort nY = Utils.BytesToUInt16(norBytes, i + 2);
                                ushort nZ = Utils.BytesToUInt16(norBytes, i + 4);

                                vx.Normal = new Vector3(
                                    Utils.UInt16ToFloat(nX, posMin.X, posMax.X),
                                    Utils.UInt16ToFloat(nY, posMin.Y, posMax.Y),
                                    Utils.UInt16ToFloat(nZ, posMin.Z, posMax.Z));
                            }

                            var vertexIndexOffset = oface.Vertices.Count * 4;

                            if (texBytes != null && texBytes.Length >= vertexIndexOffset + 4)
                            {
                                ushort tX = Utils.BytesToUInt16(texBytes, vertexIndexOffset);
                                ushort tY = Utils.BytesToUInt16(texBytes, vertexIndexOffset + 2);

                                vx.TexCoord = new Vector2(
                                    Utils.UInt16ToFloat(tX, texPosMin.X, texPosMax.X),
                                    Utils.UInt16ToFloat(tY, texPosMin.Y, texPosMax.Y));
                            }

                            oface.Vertices.Add(vx);
                        }

                        byte[] triangleBytes = subMeshMap["TriangleList"];
                        for (int i = 0; i < triangleBytes.Length; i += 6)
                        {
                            ushort v1 = (ushort)(Utils.BytesToUInt16(triangleBytes, i));
                            oface.Indices.Add(v1);
                            ushort v2 = (ushort)(Utils.BytesToUInt16(triangleBytes, i + 2));
                            oface.Indices.Add(v2);
                            ushort v3 = (ushort)(Utils.BytesToUInt16(triangleBytes, i + 4));
                            oface.Indices.Add(v3);
                        }

                        mesh.Faces.Add(oface);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log("Failed to decode mesh asset: " + ex.Message, Helpers.LogLevel.Warning);
                return false;
            }

            return true;
        }
    }

    public class SimpleMesh : Mesh
    {
        public List<Vertex> Vertices;
        public List<ushort> Indices;

        public SimpleMesh()
        {
        }

        public SimpleMesh(SimpleMesh mesh)
        {
            this.Indices = new List<ushort>(mesh.Indices);
            this.Path.Open = mesh.Path.Open;
            this.Path.Points = new List<PathPoint>(mesh.Path.Points);
            this.Prim = mesh.Prim;
            this.Profile.Concave = mesh.Profile.Concave;
            this.Profile.Faces = new List<ProfileFace>(mesh.Profile.Faces);
            this.Profile.MaxX = mesh.Profile.MaxX;
            this.Profile.MinX = mesh.Profile.MinX;
            this.Profile.Open = mesh.Profile.Open;
            this.Profile.Positions = new List<Vector3>(mesh.Profile.Positions);
            this.Profile.TotalOutsidePoints = mesh.Profile.TotalOutsidePoints;
            this.Vertices = new List<Vertex>(mesh.Vertices);
        }
    }

    #endregion Mesh Classes

    #region Plugin Loading

    public static class RenderingLoader
    {
        public static List<string> ListRenderers(string path)
        {
            List<string> plugins = new List<string>();
            string[] files = Directory.GetFiles(path, "OpenMetaverse.Rendering.*.dll");

            foreach (string f in files)
            {
                try
                {
                    Assembly a = Assembly.LoadFrom(f);
                    System.Type[] types = a.GetTypes();
                    foreach (System.Type type in types)
                    {
                        if (type.GetInterface("IRendering") != null)
                        {
                            if (type.GetCustomAttributes(typeof(RendererNameAttribute), false).Length == 1)
                            {
                                plugins.Add(f);
                            }
                            else
                            {
                                Logger.Log("Rendering plugin does not support the [RendererName] attribute: " + f,
                                    Helpers.LogLevel.Warning);
                            }

                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(String.Format("Unrecognized rendering plugin {0}: {1}", f, e.Message),
                        Helpers.LogLevel.Warning, e);
                }
            }

            return plugins;
        }

        public static IRendering LoadRenderer(string filename)
        {
            try
            {
                Assembly a = Assembly.LoadFrom(filename);
                System.Type[] types = a.GetTypes();
                foreach (System.Type type in types)
                {
                    if (type.GetInterface("IRendering") != null)
                    {
                        if (type.GetCustomAttributes(typeof(RendererNameAttribute), false).Length == 1)
                        {
                            return (IRendering)Activator.CreateInstance(type);
                        }
                        else
                        {
                            throw new RenderingException(
                                "Rendering plugin does not support the [RendererName] attribute");
                        }
                    }
                }

                throw new RenderingException(
                    "Rendering plugin does not support the IRendering interface");
            }
            catch (Exception e)
            {
                throw new RenderingException("Failed loading rendering plugin: " + e.Message, e);
            }
        }
    }

    #endregion Plugin Loading
}
