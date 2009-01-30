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
using System.Collections.Generic;
using System.IO;
using Mono.Simd.Math;

namespace OpenMetaverse.Rendering
{
    public class LindenMesh
    {
        const string MESH_HEADER = "Linden Binary Mesh 1.0";
        const string MORPH_FOOTER = "End Morphs";

        #region Mesh Structs

        public struct Face
        {
            public short[] Indices;
        }

        public struct Vertex
        {
            public Vector3f Coord;
            public Vector3f Normal;
            public Vector3f BiNormal;
            public Vector2 TexCoord;
            public Vector2 DetailTexCoord;
            public float Weight;

            public override string ToString()
            {
                return String.Format("Coord: {0} Norm: {1} BiNorm: {2} TexCoord: {3} DetailTexCoord: {4}", Coord, Normal, BiNormal, TexCoord, DetailTexCoord, Weight);
            }
        }

        public struct MorphVertex
        {
            public uint VertexIndex;
            public Vector3f Coord;
            public Vector3f Normal;
            public Vector3f BiNormal;
            public Vector2 TexCoord;

            public override string ToString()
            {
                return String.Format("Index: {0} Coord: {1} Norm: {2} BiNorm: {3} TexCoord: {4}", VertexIndex, Coord, Normal, BiNormal, TexCoord);
            }
        }

        public struct Morph
        {
            public string Name;
            public int NumVertices;
            public MorphVertex[] Vertices;

            public override string ToString()
            {
                return Name;
            }
        }

        public struct VertexRemap
        {
            public int RemapSource;
            public int RemapDestination;

            public override string ToString()
            {
                return String.Format("{0} -> {1}", RemapSource, RemapDestination);
            }
        }

        #endregion Mesh Structs

        /// <summary>
        /// Level of Detail mesh
        /// </summary>
        public class LODMesh
        {
            public float MinPixelWidth;

            protected string _header;
            protected bool _hasWeights;
            protected bool _hasDetailTexCoords;
            protected Vector3f _position;
            protected Vector3f _rotationAngles;
            protected byte _rotationOrder;
            protected Vector3f _scale;
            protected ushort _numFaces;
            protected Face[] _faces;

            public virtual void LoadMesh(string filename)
            {
                byte[] buffer = File.ReadAllBytes(filename);
                BitPack input = new BitPack(buffer, 0);

                _header = input.UnpackString(24).TrimEnd(new char[] { '\0' });
                if (!String.Equals(_header, MESH_HEADER))
                    throw new FileLoadException("Unrecognized mesh format");

                // Populate base mesh variables
                _hasWeights = (input.UnpackByte() != 0);
                _hasDetailTexCoords = (input.UnpackByte() != 0);
                _position = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
                _rotationAngles = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
                _rotationOrder = input.UnpackByte();
                _scale = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
                _numFaces = (ushort)input.UnpackUShort();

                _faces = new Face[_numFaces];

                for (int i = 0; i < _numFaces; i++)
                    _faces[i].Indices = new short[] { input.UnpackShort(), input.UnpackShort(), input.UnpackShort() };
            }
        }

        public float MinPixelWidth;

        public string Name { get { return _name; } }
        public string Header { get { return _header; } }
        public bool HasWeights { get { return _hasWeights; } }
        public bool HasDetailTexCoords { get { return _hasDetailTexCoords; } }
        public Vector3f Position { get { return _position; } }
        public Vector3f RotationAngles { get { return _rotationAngles; } }
        //public byte RotationOrder
        public Vector3f Scale { get { return _scale; } }
        public ushort NumVertices { get { return _numVertices; } }
        public Vertex[] Vertices { get { return _vertices; } }
        public ushort NumFaces { get { return _numFaces; } }
        public Face[] Faces { get { return _faces; } }
        public ushort NumSkinJoints { get { return _numSkinJoints; } }
        public string[] SkinJoints { get { return _skinJoints; } }
        public Morph[] Morphs { get { return _morphs; } }
        public int NumRemaps { get { return _numRemaps; } }
        public VertexRemap[] VertexRemaps { get { return _vertexRemaps; } }
        public SortedList<int, LODMesh> LODMeshes { get { return _lodMeshes; } }

        protected string _name;
        protected string _header;
        protected bool _hasWeights;
        protected bool _hasDetailTexCoords;
        protected Vector3f _position;
        protected Vector3f _rotationAngles;
        protected byte _rotationOrder;
        protected Vector3f _scale;
        protected ushort _numVertices;
        protected Vertex[] _vertices;
        protected ushort _numFaces;
        protected Face[] _faces;
        protected ushort _numSkinJoints;
        protected string[] _skinJoints;
        protected Morph[] _morphs;
        protected int _numRemaps;
        protected VertexRemap[] _vertexRemaps;
        protected SortedList<int, LODMesh> _lodMeshes;

        public LindenMesh(string name)
        {
            _name = name;
            _lodMeshes = new SortedList<int, LODMesh>();
        }

        public virtual void LoadMesh(string filename)
        {
            byte[] buffer = File.ReadAllBytes(filename);
            BitPack input = new BitPack(buffer, 0);

            _header = input.UnpackString(24).TrimEnd(new char[] { '\0' });
            if (!String.Equals(_header, MESH_HEADER))
                throw new FileLoadException("Unrecognized mesh format");

            // Populate base mesh variables
            _hasWeights = (input.UnpackByte() != 0);
            _hasDetailTexCoords = (input.UnpackByte() != 0);
            _position = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
            _rotationAngles = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
            _rotationOrder = input.UnpackByte();
            _scale = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
            _numVertices = (ushort)input.UnpackUShort();

            // Populate the vertex array
            _vertices = new Vertex[_numVertices];

            for (int i = 0; i < _numVertices; i++)
                _vertices[i].Coord = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());

            for (int i = 0; i < _numVertices; i++)
                _vertices[i].Normal = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());

            for (int i = 0; i < _numVertices; i++)
                _vertices[i].BiNormal = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());

            for (int i = 0; i < _numVertices; i++)
                _vertices[i].TexCoord = new Vector2(input.UnpackFloat(), input.UnpackFloat());

            if (_hasDetailTexCoords)
            {
                for (int i = 0; i < _numVertices; i++)
                    _vertices[i].DetailTexCoord = new Vector2(input.UnpackFloat(), input.UnpackFloat());
            }

            if (_hasWeights)
            {
                for (int i = 0; i < _numVertices; i++)
                    _vertices[i].Weight = input.UnpackFloat();
            }

            _numFaces = input.UnpackUShort();

            _faces = new Face[_numFaces];

            for (int i = 0; i < _numFaces; i++)
                _faces[i].Indices = new short[] { input.UnpackShort(), input.UnpackShort(), input.UnpackShort() };

            if (_hasWeights)
            {
                _numSkinJoints = input.UnpackUShort();
                _skinJoints = new string[_numSkinJoints];

                for (int i = 0; i < _numSkinJoints; i++)
                    _skinJoints[i] = input.UnpackString(64).TrimEnd(new char[] { '\0' });
            }
            else
            {
                _numSkinJoints = 0;
                _skinJoints = new string[0];
            }

            // Grab morphs
            List<Morph> morphs = new List<Morph>();
            string morphName = input.UnpackString(64).TrimEnd(new char[] { '\0' });

            while (morphName != MORPH_FOOTER)
            {
                if (input.BytePos + 48 >= input.Data.Length) throw new FileLoadException("Encountered end of file while parsing morphs");

                Morph morph = new Morph();
                morph.Name = morphName;
                morph.NumVertices = input.UnpackInt();
                morph.Vertices = new MorphVertex[morph.NumVertices];

                for (int i = 0; i < morph.NumVertices; i++)
                {
                    MorphVertex vertex;
                    vertex.VertexIndex = input.UnpackUInt();
                    vertex.Coord = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
                    vertex.Normal = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
                    vertex.BiNormal = new Vector3f(input.UnpackFloat(), input.UnpackFloat(), input.UnpackFloat());
                    vertex.TexCoord = new Vector2(input.UnpackFloat(), input.UnpackFloat());
                }

                morphs.Add(morph);

                // Grab the next name
                morphName = input.UnpackString(64).TrimEnd(new char[] { '\0' });
            }

            _morphs = morphs.ToArray();

            // Check if there are remaps or if we're at the end of the file
            if (input.BytePos < input.Data.Length - 1)
            {
                _numRemaps = input.UnpackInt();
                _vertexRemaps = new VertexRemap[_numRemaps];

                for (int i = 0; i < _numRemaps; i++)
                {
                    _vertexRemaps[i].RemapSource = input.UnpackInt();
                    _vertexRemaps[i].RemapDestination = input.UnpackInt();
                }
            }
            else
            {
                _numRemaps = 0;
                _vertexRemaps = new VertexRemap[0];
            }
        }

        public virtual void LoadLODMesh(int level, string filename)
        {
            LODMesh lod = new LODMesh();
            lod.LoadMesh(filename);
            _lodMeshes[level] = lod;
        }
    }
}
