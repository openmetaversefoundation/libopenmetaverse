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
using Mono.Simd.Math;

namespace OpenMetaverse.Rendering
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RendererNameAttribute : System.Attribute
    {
        private string _name;

        public RendererNameAttribute(string name)
            : base()
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }

    /// <summary>
    /// Abstract base for rendering plugins
    /// </summary>
    public interface IRendering
    {
        /// <summary>
        /// Generates a basic mesh structure from a primitive
        /// </summary>
        /// <param name="prim">Primitive to generate the mesh from</param>
        /// <param name="lod">Level of detail to generate the mesh at</param>
        /// <returns>The generated mesh</returns>
        SimpleMesh GenerateSimpleMesh(Primitive prim, DetailLevel lod);

        /// <summary>
        /// Generates a a series of faces, each face containing a mesh and
        /// metadata
        /// </summary>
        /// <param name="prim">Primitive to generate the mesh from</param>
        /// <param name="lod">Level of detail to generate the mesh at</param>
        /// <returns>The generated mesh</returns>
        FacetedMesh GenerateFacetedMesh(Primitive prim, DetailLevel lod);

        /// <summary>
        /// Apply texture coordinate modifications from a
        /// <seealso cref="TextureEntryFace"/> to a list of vertices
        /// </summary>
        /// <param name="vertices">Vertex list to modify texture coordinates for</param>
        /// <param name="center">Center-point of the face</param>
        /// <param name="teFace">Face texture parameters</param>
        void TransformTexCoords(List<Vertex> vertices, Vector3f center, Primitive.TextureEntryFace teFace);
    }
}
