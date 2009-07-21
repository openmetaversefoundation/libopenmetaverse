/*
 * Copyright (c) 2008, openmetaverse.org
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
using System.IO;
using System.Collections.Generic;
using System.Reflection;

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

    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Binormal;
        public Vector2 TexCoord;

        public override string ToString()
        {
            return String.Format("P: {0} N: {1} B: {2} T: {3}", Position, Normal, Binormal, TexCoord);
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

    public class FacetedMesh : Mesh
    {
        public List<Face> Faces;
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
