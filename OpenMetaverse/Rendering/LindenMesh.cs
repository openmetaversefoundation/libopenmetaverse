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
using System.IO;

namespace OpenMetaverse.Rendering
{
    /// <summary>
    /// Load and handle Linden Lab binary meshes.
    /// </summary>
    /// <remarks>
    /// The exact definition of this file is a bit sketchy, especially concerning skin weights.
    /// A good starting point is on the 
    /// <a href="http://wiki.secondlife.com/wiki/Avatar_Appearance#Linden_binary_mesh_file">second life wiki</a>
    /// </remarks>
    public class LindenMesh
    {
        const string MeshHeader = "Linden Binary Mesh 1.0";
        const string MorphFooter = "End Morphs";
        public LindenSkeleton Skeleton { get; private set; }    //!< The skeleton used to animate this mesh

        #region Mesh Structs

        /// <summary>
        /// Defines a polygon
        /// </summary>
        public struct Face
        {
            public short[] Indices;                     //!< Indices into the vertex array
        }

        /// <summary>
        /// Structure of a vertex, No surprises there, except for the Detail tex coord
        /// </summary>
        /// <remarks>
        /// The skinweights are a tad unconventional. The best explanation found is:
        /// >Each weight actually contains two pieces of information. The number to the 
        /// >left of the decimal point is the index of the joint and also implicitly 
        /// >indexes to the following joint. The actual weight is to the right of the
        /// >decimal point and interpolates between these two joints. The index is into
        /// >an "expanded" list of joints, not just a linear array of the joints as
        /// >defined in the skeleton file. In particular, any joint that has more than
        /// >one child will be repeated in the list for each of its children.
        ///
        /// Maybe I'm dense, but that description seems to be a bit hard to build an
        /// algorithm on. 
        /// 
        /// Esentially the weights are compressed into one floating point value.
        /// 1. The whole number part is an index into an array of joints
        /// 2. The fractional part is the weight that joint has
        /// 3. If the fractional part is 0 (x.0000) then the vertex is 100% influenced by the specified joint
        /// </remarks>
        public struct Vertex
        {
            public Vector3 Coord;                       //!< 3d co-ordinate of the vertex
            public Vector3 Normal;                      //!< Normal of the vertex
            public Vector3 BiNormal;                    //!< Bi normal of the vertex
            public Vector2 TexCoord;                    //!< UV maping of the vertex
            public Vector2 DetailTexCoord;              //!< Detailed? UV mapping
            public float Weight;                        //!< Used to calculate the skin weights

            /// <summary>
            /// Provide a nice format for debugging
            /// </summary>
            /// <returns>Vertex definition as a string</returns>
            public override string ToString()
            {
                return String.Format("Coord: {0} Norm: {1} BiNorm: {2} TexCoord: {3} DetailTexCoord: {4}", Coord, Normal, BiNormal, TexCoord, DetailTexCoord);
            }
        }

        /// <summary>
        /// Describes deltas to apply to a vertex in order to morph a vertex
        /// </summary>
        public struct MorphVertex
        {
            public uint VertexIndex;            //!< Index into the vertex list of the vertex to change
            public Vector3 Coord;               //!< Delta position
            public Vector3 Normal;              //!< Delta normal
            public Vector3 BiNormal;            //!< Delta BiNormal
            public Vector2 TexCoord;            //!< Delta UV mapping

            /// <summary>
            /// Provide a nice format for debugging
            /// </summary>
            /// <returns>MorphVertex definition as a string</returns>
            public override string ToString()
            {
                return String.Format("Index: {0} Coord: {1} Norm: {2} BiNorm: {3} TexCoord: {4}", VertexIndex, Coord, Normal, BiNormal, TexCoord);
            }
        }

        /// <summary>
        /// Describes a named mesh morph, essentially a named list of MorphVertices
        /// </summary>
        public struct Morph
        {
            public string Name;                     //!< Name of the morph
            public int NumVertices;                 //!< Number of vertices to distort
            public MorphVertex[] Vertices;          //!< The actual list of morph vertices

            /// <summary>
            /// Provide a nice format for debugging
            /// </summary>
            /// <returns>The name of the morph</returns>
            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>
        /// Don't really know what this does
        /// </summary>
        public struct VertexRemap
        {
            public int RemapSource;             //!< Source index
            public int RemapDestination;        //!< Destination index

            /// <summary>
            /// Provide a nice format for debugging
            /// </summary>
            /// <returns>Human friendly format</returns>
            public override string ToString()
            {
                return String.Format("{0} -> {1}", RemapSource, RemapDestination);
            }
        }
        #endregion Mesh Structs

        #region reference mesh
        /// <summary>
        /// A reference mesh is one way to implement level of detail
        /// </summary>
        /// <remarks>
        /// Reference meshes are supplemental meshes to full meshes. For all practical
        /// purposes almost all lod meshes are implemented as reference meshes, except for 
        /// 'avatar_eye_1.llm' which for some reason is implemented as a full mesh.
        /// </remarks>
        public class ReferenceMesh
        {
            public float MinPixelWidth;                 //!< Pixel width on screen before switching to coarser lod

            public string Header;                       //!< Header - marking the file as a Linden Lab Mesh (llm)
            public bool HasWeights;                     //!< Do the vertices carry any defintions about skin weights
            public bool HasDetailTexCoords;             //!< Do the vertices carry any defintions about detailed UV mappings
            public Vector3 Position;                    //!< Origin of this mesh
            public Vector3 RotationAngles;              //!< Used to reconstruct a normalized quarternion (These are *NOT* Euler rotations)
            public byte RotationOrder;                  //!< Not used
            public Vector3 Scale;                       //!< Scaling information
            public ushort NumFaces;                     //!< # of polygons in the mesh
            public Face[] Faces;                        //!< Polygons making up the mesh, the indices are into the full mesh


            /// <summary>
            /// Load a mesh from a stream
            /// </summary>
            /// <param name="filename">Filename and path of the file containing the reference mesh</param>
            public virtual void LoadMesh(string filename)
            {
                using(FileStream meshStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                using (EndianAwareBinaryReader reader = new EndianAwareBinaryReader(meshStream))
                {
                    Header = TrimAt0(reader.ReadString(24));
                    if (!String.Equals(Header, MeshHeader))
                        throw new FileLoadException("Unrecognized mesh format");

                    // Populate base mesh parameters
                    HasWeights = (reader.ReadByte() != 0);
                    HasDetailTexCoords = (reader.ReadByte() != 0);
                    Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    RotationAngles = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    RotationOrder = reader.ReadByte();
                    Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    NumFaces = reader.ReadUInt16();

                    Faces = new Face[NumFaces];

                    for (int i = 0; i < NumFaces; i++)
                        Faces[i].Indices = new[] { reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16() };
                }
            }
        }

        /// <summary>
        /// Level of Detail mesh
        /// </summary>
        [Obsolete("Renamed to: ReferenceMesh")]
        public class LODMesh
        {
            public float MinPixelWidth;

            protected string _header;
            protected bool _hasWeights;
            protected bool _hasDetailTexCoords;
            protected Vector3 _position;
            protected Vector3 _rotationAngles;
            protected byte _rotationOrder;
            protected Vector3 _scale;
            protected ushort _numFaces;
            protected Face[] _faces;

            public virtual void LoadMesh(string filename)
            {
                byte[] buffer = File.ReadAllBytes(filename);
                BitPack input = new BitPack(buffer, 0);

                _header = TrimAt0(input.UnpackString(24));
                if (!String.Equals(_header, MeshHeader))
                    return;

                // Populate base mesh variables
                _hasWeights = (input.UnpackByte() != 0);
                _hasDetailTexCoords = (input.UnpackByte() != 0);
                _position = new Vector3(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
                _rotationAngles = new Vector3(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
                _rotationOrder = input.UnpackByte();
                _scale = new Vector3(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
                _numFaces = input.UnpackUShort();

                _faces = new Face[_numFaces];

                for (int i = 0; i < _numFaces; i++)
                    _faces[i].Indices = new short[] { input.UnpackShort(), input.UnpackShort(), input.UnpackShort() };
            }
        }
        #endregion lod mesh

        public float MinPixelWidth;                                             //!< Width of redered avatar, before moving to a coarser LOD

        public string Name { get; protected set; }                              //!< The name of this mesh
        public string Header { get; protected set; }                            //!< The header marker contained in the .llm file
        public bool HasWeights { get; protected set; }                          //!< Does the file contain skin weights?
        public bool HasDetailTexCoords { get; protected set; }                  //!< Does the file contain detailed UV mapings
        public Vector3 Position { get; protected set; }                         //!< Origin of this mesh
        public Vector3 RotationAngles { get; protected set; }                   //!< Rotation - This is a compressed quaternion
        //public byte RotationOrder
        public Vector3 Scale { get; protected set; }                            //!< Scale of this mesh
        public ushort NumVertices { get; protected set; }                       //!< # of vertices in the file
        public Vertex[] Vertices { get; protected set; }                        //!< The actual vertices defining the 3d shape
        public ushort NumFaces { get; protected set; }                          //!< # of polygons in the file
        public Face[] Faces { get; protected set; }                             //!< The polgon defintion
        public ushort NumSkinJoints { get; protected set; }                     //!< # of joints influencing the mesh
        public string[] SkinJoints { get; protected set; }                      //!< Named list of joints
        public int NumRemaps { get; protected set; }                            //!< # of vertex remaps
        public VertexRemap[] VertexRemaps { get; protected set; }               //!< The actual vertex remapping list

        // lods can either be Reference meshes or full LindenMeshes
        // so we cannot use a collection of specialized classes
        public SortedList<int, object> LodMeshes { get; protected set; }        //!< The LOD meshes, available for this mesh

        public Morph[] Morphs;                                                  //!< The morphs this file contains

        /// <summary>
        /// Construct a linden mesh with the given name
        /// </summary>
        /// <param name="name">the name of the mesh</param>
        public LindenMesh(string name)
            : this(name, null) { }

        /// <summary>
        /// Construct a linden mesh with the given name
        /// </summary>
        /// <param name="name">the name of the mesh</param>
        /// <param name="skeleton">The skeleton governing mesh deformation</param>
        public LindenMesh(string name, LindenSkeleton skeleton)
        {
            Name = name;
            Skeleton = skeleton;
            LodMeshes = new SortedList<int, object>();

            if (Skeleton == null)
                Skeleton = LindenSkeleton.Load();
        }

        /// <summary>
        /// Load the mesh from a stream
        /// </summary>
        /// <param name="filename">The filename and path of the file containing the mesh data</param>
        public virtual void LoadMesh(string filename)
        {
            using(FileStream meshData = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (EndianAwareBinaryReader reader = new EndianAwareBinaryReader(meshData))
            {
                Header = TrimAt0(reader.ReadString(24));
                if (!String.Equals(Header, MeshHeader))
                    throw new FileLoadException("Unrecognized mesh format");

                // Populate base mesh parameters
                HasWeights = (reader.ReadByte() != 0);
                HasDetailTexCoords = (reader.ReadByte() != 0);
                Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                RotationAngles = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                /* RotationOrder = */ reader.ReadByte();
                Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                // Populate the vertex array
                NumVertices = reader.ReadUInt16();
                Vertices = new Vertex[NumVertices];
                for (int i = 0; i < NumVertices; i++)
                    Vertices[i].Coord = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                for (int i = 0; i < NumVertices; i++)
                    Vertices[i].Normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                for (int i = 0; i < NumVertices; i++)
                    Vertices[i].BiNormal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                for (int i = 0; i < NumVertices; i++)
                    Vertices[i].TexCoord = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                if (HasDetailTexCoords)
                {
                    for (int i = 0; i < NumVertices; i++)
                        Vertices[i].DetailTexCoord = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                }

                if (HasWeights)
                {
                    for (int i = 0; i < NumVertices; i++)
                        Vertices[i].Weight = reader.ReadSingle();
                }

                NumFaces = reader.ReadUInt16();
                Faces = new Face[NumFaces];

                for (int i = 0; i < NumFaces; i++)
                    Faces[i].Indices = new[] { reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16() };

                if (HasWeights)
                {
                    NumSkinJoints = reader.ReadUInt16();
                    SkinJoints = new string[NumSkinJoints];

                    for (int i = 0; i < NumSkinJoints; i++)
                    {
                        SkinJoints[i] = TrimAt0(reader.ReadString(64));
                    }
                }
                else
                {
                    NumSkinJoints = 0;
                    SkinJoints = new string[0];
                }

                // Grab morphs
                List<Morph> morphs = new List<Morph>();
                string morphName = TrimAt0(reader.ReadString(64));

                while (morphName != MorphFooter)
                {
                    if (reader.BaseStream.Position + 48 >= reader.BaseStream.Length)
                        throw new FileLoadException("Encountered end of file while parsing morphs");

                    Morph morph = new Morph();
                    morph.Name = morphName;
                    morph.NumVertices = reader.ReadInt32();
                    morph.Vertices = new MorphVertex[morph.NumVertices];

                    for (int i = 0; i < morph.NumVertices; i++)
                    {
                        morph.Vertices[i].VertexIndex = reader.ReadUInt32();
                        morph.Vertices[i].Coord = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        morph.Vertices[i].Normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        morph.Vertices[i].BiNormal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        morph.Vertices[i].TexCoord = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }

                    morphs.Add(morph);

                    // Grab the next name
                    morphName = TrimAt0(reader.ReadString(64));
                }

                Morphs = morphs.ToArray();

                // Check if there are remaps or if we're at the end of the file
                if (reader.BaseStream.Position < reader.BaseStream.Length - 1)
                {
                    NumRemaps = reader.ReadInt32();
                    VertexRemaps = new VertexRemap[NumRemaps];

                    for (int i = 0; i < NumRemaps; i++)
                    {
                        VertexRemaps[i].RemapSource = reader.ReadInt32();
                        VertexRemaps[i].RemapDestination = reader.ReadInt32();
                    }
                }
                else
                {
                    NumRemaps = 0;
                    VertexRemaps = new VertexRemap[0];
                }
            }

            // uncompress the skin weights
            if (Skeleton != null)
            {
                // some meshes aren't weighted, which doesn't make much sense.
                // we check for left and right eyeballs, and assign them a 100%
                // to their respective bone
                List<string> expandedJointList = Skeleton.BuildExpandedJointList(SkinJoints);
                if (expandedJointList.Count == 0)
                {
                    if (Name == "eyeBallLeftMesh")
                    {
                        expandedJointList.AddRange(new[] { "mEyeLeft", "mSkull" });
                    }
                    else if (Name == "eyeBallRightMesh")
                    {
                        expandedJointList.AddRange(new[] { "mEyeRight", "mSkull" });
                    }
                }

                if (expandedJointList.Count > 0)
                    ExpandCompressedSkinWeights(expandedJointList);
            }
        }

        #region Skin weight

        /// <summary>
        /// Layout of one skinweight element
        /// </summary>
        public struct SkinWeightElement
        {
            public string Bone1;        // Name of the first bone that influences the vertex
            public string Bone2;        // Name of the second bone that influences the vertex
            public float Weight1;       // Weight with whitch the first bone influences the vertex
            public float Weight2;       // Weight with whitch the second bone influences the vertex
        }

        /// <summary>List of skinweights, in the same order as the mesh vertices</summary>
        public List<SkinWeightElement> SkinWeights = new List<SkinWeightElement>();

        /// <summary>
        /// Decompress the skinweights
        /// </summary>
        /// <param name="expandedJointList">the expanded joint list, used to index which bones should influece the vertex</param>
        void ExpandCompressedSkinWeights(List<string> expandedJointList)
        {
            for (int i = 0; i < NumVertices; i++)
            {
                int boneIndex = (int)Math.Floor(Vertices[i].Weight); // Whole number part is the index
                float boneWeight = (Vertices[i].Weight - boneIndex); // fractional part it the weight

                if (boneIndex == 0)         // Special case for dealing with eye meshes, which doesn't have any weights
                {
                    SkinWeights.Add(new SkinWeightElement { Bone1 = expandedJointList[0], Weight1 = 1, Bone2 = expandedJointList[1], Weight2 = 0 });
                }
                else if (boneIndex < expandedJointList.Count)
                {
                    string bone1 = expandedJointList[boneIndex - 1];
                    string bone2 = expandedJointList[boneIndex];
                    SkinWeights.Add(new SkinWeightElement { Bone1 = bone1, Weight1 = 1 - boneWeight, Bone2 = bone2, Weight2 = boneWeight });
                }
                else
                {   // this should add a weight where the "invalid" Joint has a weight of zero
                    SkinWeights.Add(new SkinWeightElement
                    {
                        Bone1 = expandedJointList[boneIndex - 1],
                        Weight1 = 1 - boneWeight,
                        Bone2 = "mPelvis",
                        Weight2 = boneWeight
                    });
                }
            }
        }
        #endregion Skin weight

        [Obsolete("Use LoadLodMesh")]
        public virtual void LoadLODMesh(int level, string filename)
        {
            if (filename == "avatar_eye_1.llm")
                throw new ArgumentException("Eyballs are not LOD Meshes", "filename");

            LODMesh lod = new LODMesh();
            lod.LoadMesh(filename);
            LodMeshes[level] = lod;
        }

        public virtual object LoadLodMesh(int level, string filename)
        {
            if (filename != "avatar_eye_1.llm")
            {
                ReferenceMesh refMesh = new ReferenceMesh();
                refMesh.LoadMesh(filename);
                LodMeshes[level] = refMesh;
                return refMesh;
            }

            LindenMesh fullMesh = new LindenMesh("");
            fullMesh.LoadMesh(filename);
            LodMeshes[level] = fullMesh;
            return fullMesh;
        }

        /// <summary>
        /// Load a reference mesh from a given stream
        /// </summary>
        /// <param name="lodLevel">The lod level of this reference mesh</param>
        /// <param name="filename">the name and path of the file containing the mesh data</param>
        /// <returns>the loaded reference mesh</returns>
        public virtual ReferenceMesh LoadReferenceMesh(int lodLevel, string filename)
        {
            if (filename == "avatar_eye_1.llm")
                throw new ArgumentException("Eyballs are not LOD Meshes", "filename");

            ReferenceMesh reference = new ReferenceMesh();
            reference.LoadMesh(filename);
            LodMeshes[lodLevel] = lodLevel;
            return reference;
        }

        /// <summary>
        /// Trim a string at the first occurence of NUL
        /// </summary>
        /// <remarks>
        /// The llm file uses null terminated strings (C/C++ style), this is where
        /// the conversion is made.
        /// </remarks>
        /// <param name="s">The string to trim</param>
        /// <returns>A standard .Net string</returns>
        public static string TrimAt0(string s)
        {
            int pos = s.IndexOf("\0");

            if (pos >= 0)
                return s.Substring(0, pos);

            return s;
        }
    }
}
