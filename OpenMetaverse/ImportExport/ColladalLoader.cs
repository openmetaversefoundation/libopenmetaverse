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
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Xml.Serialization;
using OpenMetaverse.ImportExport.Collada14;
using OpenMetaverse.Rendering;
using OpenMetaverse.Imaging;

namespace OpenMetaverse.ImportExport
{
    /// <summary>
    /// Parsing Collada model files into data structures
    /// </summary>
    public class ColladaLoader
    {
        COLLADA Model;
        static XmlSerializer Serializer = null;
        List<Node> Nodes;
        List<ModelMaterial> Materials;
        Dictionary<string, string> MatSymTarget;
        string FileName;

        class Node
        {
            public Matrix4 Transform = Matrix4.Identity;
            public string Name;
            public string ID;
            public string MeshID;
        }

        /// <summary>
        /// Parses Collada document
        /// </summary>
        /// <param name="filename">Load .dae model from this file</param>
        /// <param name="loadImages">Load and decode images for uploading with model</param>
        /// <returns>A list of mesh prims that were parsed from the collada file</returns>
        public List<ModelPrim> Load(string filename, bool loadImages)
        {
            try
            {
                // Create an instance of the XmlSerializer specifying type and namespace.
                if (Serializer == null)
                {
                    Serializer = new XmlSerializer(typeof(COLLADA));
                }

                this.FileName = filename;

                // A FileStream is needed to read the XML document.
                FileStream fs = new FileStream(filename, FileMode.Open);
                XmlReader reader = XmlReader.Create(fs);
                Model = (COLLADA)Serializer.Deserialize(reader);
                fs.Close();
                var prims = Parse();
                if (loadImages)
                {
                    LoadImages(prims);
                }
                return prims;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed parsing collada file: " + ex.Message, Helpers.LogLevel.Error, ex);
                return new List<ModelPrim>();
            }
        }

        void LoadImages(List<ModelPrim> prims)
        {
            foreach (var prim in prims)
            {
                foreach (var face in prim.Faces)
                {
                    if (!string.IsNullOrEmpty(face.Material.Texture))
                    {
                        LoadImage(face.Material);
                    }
                }
            }
        }

        void LoadImage(ModelMaterial material)
        {
            var fname = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FileName), material.Texture);

            try
            {
                string ext = System.IO.Path.GetExtension(material.Texture).ToLower();

                Bitmap bitmap = null;

                if (ext == ".jp2" || ext == ".j2c")
                {
                    material.TextureData = File.ReadAllBytes(fname);
                    return;
                }

                if (ext == ".tga")
                {
                    bitmap = LoadTGAClass.LoadTGA(fname);
                }
                else
                {
                    bitmap = (Bitmap)Image.FromFile(fname);
                }

                int width = bitmap.Width;
                int height = bitmap.Height;

                // Handle resizing to prevent excessively large images and irregular dimensions
                if (!IsPowerOfTwo((uint)width) || !IsPowerOfTwo((uint)height) || width > 1024 || height > 1024)
                {
                    var origWidth = width;
                    var origHieght = height;

                    width = ClosestPowerOwTwo(width);
                    height = ClosestPowerOwTwo(height);

                    width = width > 1024 ? 1024 : width;
                    height = height > 1024 ? 1024 : height;

                    Logger.Log("Image has irregular dimensions " + origWidth + "x" + origHieght + ". Resizing to " + width + "x" + height, Helpers.LogLevel.Info);

                    Bitmap resized = new Bitmap(width, height, bitmap.PixelFormat);
                    Graphics graphics = Graphics.FromImage(resized);

                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.InterpolationMode =
                       System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(bitmap, 0, 0, width, height);

                    bitmap.Dispose();
                    bitmap = resized;
                }

                material.TextureData = OpenJPEG.EncodeFromImage(bitmap, false);

                Logger.Log("Successfully encoded " + fname, Helpers.LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed loading " + fname + ": " + ex.Message, Helpers.LogLevel.Warning);
            }

        }

        bool IsPowerOfTwo(uint n)
        {
            return (n & (n - 1)) == 0 && n != 0;
        }

        int ClosestPowerOwTwo(int n)
        {
            int res = 1;

            while (res < n)
            {
                res <<= 1;
            }

            return res > 1 ? res / 2 : 1;
        }

        ModelMaterial ExtractMaterial(object diffuse)
        {
            ModelMaterial ret = new ModelMaterial();
            if (diffuse is common_color_or_texture_typeColor)
            {
                var col = (common_color_or_texture_typeColor)diffuse;
                ret.DiffuseColor = new Color4((float)col.Values[0], (float)col.Values[1], (float)col.Values[2], (float)col.Values[3]);
            }
            else if (diffuse is common_color_or_texture_typeTexture)
            {
                var tex = (common_color_or_texture_typeTexture)diffuse;
                ret.Texture = tex.texcoord;
            }
            return ret;

        }

        void ParseMaterials()
        {

            if (Model == null) return;

            Materials = new List<ModelMaterial>();

            // Material -> effect mapping
            Dictionary<string, string> matEffect = new Dictionary<string, string>();
            List<ModelMaterial> tmpEffects = new List<ModelMaterial>();

            // Image ID -> filename mapping
            Dictionary<string, string> imgMap = new Dictionary<string, string>();

            foreach (var item in Model.Items)
            {
                if (item is library_images)
                {
                    var images = (library_images)item;
                    if (images.image != null)
                    {
                        foreach (var image in images.image)
                        {
                            var img = (image)image;
                            string ID = img.id;
                            if (img.Item is string)
                            {
                                imgMap[ID] = (string)img.Item;
                            }
                        }
                    }
                }
            }

            foreach (var item in Model.Items)
            {
                if (item is library_materials)
                {
                    var materials = (library_materials)item;
                    if (materials.material != null)
                    {
                        foreach (var material in materials.material)
                        {
                            var ID = material.id;
                            if (material.instance_effect != null)
                            {
                                if (!string.IsNullOrEmpty(material.instance_effect.url))
                                {
                                    matEffect[material.instance_effect.url.Substring(1)] = ID;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var item in Model.Items)
            {
                if (item is library_effects)
                {
                    var effects = (library_effects)item;
                    if (effects.effect != null)
                    {
                        foreach (var effect in effects.effect)
                        {
                            string ID = effect.id;
                            foreach (var effItem in effect.Items)
                            {
                                if (effItem is effectFx_profile_abstractProfile_COMMON)
                                {
                                    var teq = ((effectFx_profile_abstractProfile_COMMON)effItem).technique;
                                    if (teq != null)
                                    {
                                        if (teq.Item is effectFx_profile_abstractProfile_COMMONTechniquePhong)
                                        {
                                            var shader = (effectFx_profile_abstractProfile_COMMONTechniquePhong)teq.Item;
                                            if (shader.diffuse != null)
                                            {
                                                var material = ExtractMaterial(shader.diffuse.Item);
                                                material.ID = ID;
                                                tmpEffects.Add(material);
                                            }
                                        }
                                        else if (teq.Item is effectFx_profile_abstractProfile_COMMONTechniqueLambert)
                                        {
                                            var shader = (effectFx_profile_abstractProfile_COMMONTechniqueLambert)teq.Item;
                                            if (shader.diffuse != null)
                                            {
                                                var material = ExtractMaterial(shader.diffuse.Item);
                                                material.ID = ID;
                                                tmpEffects.Add(material);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var effect in tmpEffects)
            {
                if (matEffect.ContainsKey(effect.ID))
                {
                    effect.ID = matEffect[effect.ID];
                    if (!string.IsNullOrEmpty(effect.Texture))
                    {
                        if (imgMap.ContainsKey(effect.Texture))
                        {
                            effect.Texture = imgMap[effect.Texture];
                        }
                    }
                    Materials.Add(effect);
                }
            }
        }

        void ProcessNode(node node)
        {
            Node n = new Node();
            n.ID = node.id;

            if (node.Items != null)
                // Try finding matrix
                foreach (var i in node.Items)
                {
                    if (i is matrix)
                    {
                        var m = (matrix)i;
                        for (int a = 0; a < 4; a++)
                            for (int b = 0; b < 4; b++)
                            {
                                n.Transform[b, a] = (float)m.Values[a * 4 + b];
                            }
                    }
                }

            // Find geometry and material
            if (node.instance_geometry != null && node.instance_geometry.Length > 0)
            {
                var instGeom = node.instance_geometry[0];
                if (!string.IsNullOrEmpty(instGeom.url))
                {
                    n.MeshID = instGeom.url.Substring(1);
                }
                if (instGeom.bind_material != null && instGeom.bind_material.technique_common != null)
                {
                    foreach (var teq in instGeom.bind_material.technique_common)
                    {
                        var target = teq.target;
                        if (!string.IsNullOrEmpty(target))
                        {
                            target = target.Substring(1);
                            MatSymTarget[teq.symbol] = target;
                        }
                    }
                }
            }

            if (node.Items != null && node.instance_geometry != null && node.instance_geometry.Length > 0)
                Nodes.Add(n);

            // Recurse if the scene is hierarchical
            if (node.node1 != null)
                foreach (node nd in node.node1)
                    ProcessNode(nd);
        }

        void ParseVisualScene()
        {
            Nodes = new List<Node>();
            if (Model == null) return;

            MatSymTarget = new Dictionary<string, string>();

            foreach (var item in Model.Items)
            {
                if (item is library_visual_scenes)
                {
                    var scene = ((library_visual_scenes)item).visual_scene[0];
                    foreach (var node in scene.node)
                    {
                        ProcessNode(node);
                    }
                }
            }
        }

        List<ModelPrim> Parse()
        {
            var Prims = new List<ModelPrim>();

            float DEG_TO_RAD = 0.017453292519943295769236907684886f;

            if (Model == null) return Prims;

            Matrix4 transform = Matrix4.Identity;

            UpAxisType upAxis = UpAxisType.Y_UP;

            var asset = Model.asset;
            if (asset != null)
            {
                upAxis = asset.up_axis;
                if (asset.unit != null)
                {
                    float meter = (float)asset.unit.meter;
                    transform[0, 0] = meter;
                    transform[1, 1] = meter;
                    transform[2, 2] = meter;
                }
            }

            Matrix4 rotation = Matrix4.Identity;

            if (upAxis == UpAxisType.X_UP)
            {
                rotation = Matrix4.CreateFromEulers(0.0f, 90.0f * DEG_TO_RAD, 0.0f);
            }
            else if (upAxis == UpAxisType.Y_UP)
            {
                rotation = Matrix4.CreateFromEulers(90.0f * DEG_TO_RAD, 0.0f, 0.0f);
            }

            rotation = rotation * transform;
            transform = rotation;

            ParseVisualScene();
            ParseMaterials();

            foreach (var item in Model.Items) {
                if (item is library_geometries) {
                    var geometries = (library_geometries)item;
                    foreach (var geo in geometries.geometry) {
                        var mesh = geo.Item as mesh;
                        if (mesh == null) 
                            continue;

                        var nodes = Nodes.FindAll(n => n.MeshID == geo.id);     // Find all instances of this geometry
                        if (nodes != null) {
                            ModelPrim firstPrim = null;         // The first prim is actually calculated, the others are just copied from it.

                            Vector3 asset_scale = new Vector3(1,1,1);
                            Vector3 asset_offset = new Vector3(0, 0, 0);            // Scale and offset between Collada and OS asset (Which is always in a unit cube)

                            foreach (var node in nodes) {
                                var prim = new ModelPrim();
                                prim.ID = node.ID;
                                Prims.Add(prim);

                                // First node is used to create the asset. This is as the code to crate the byte array is somewhat
                                // erroneously placed in the ModelPrim class.
                                if (firstPrim == null) {
                                    firstPrim = prim;
                                    AddPositions(out asset_scale, out asset_offset, mesh, prim, transform);     // transform is used only for inch -> meter and up axis transform. 

                                    foreach (var mitem in mesh.Items) {
                                        if (mitem is triangles)
                                            AddFacesFromPolyList(Triangles2Polylist((triangles)mitem), mesh, prim, transform);  // Transform is used to turn normals according to up axis
                                        if (mitem is polylist)
                                            AddFacesFromPolyList((polylist)mitem, mesh, prim, transform);
                                    }

                                    prim.CreateAsset(UUID.Zero);
                                }
                                else {
                                     // Copy the values set by Addpositions and AddFacesFromPolyList as these are the same as long as the mesh is the same
                                     prim.Asset = firstPrim.Asset;
                                     prim.BoundMin = firstPrim.BoundMin;
                                     prim.BoundMax = firstPrim.BoundMax;
                                     prim.Positions = firstPrim.Positions;
                                     prim.Faces = firstPrim.Faces;
                                }

                                // Note: This ignores any shear or similar non-linear effects. This can cause some problems but it
                                // is unlikely that authoring software can generate such matrices.
                                node.Transform.Decompose(out prim.Scale, out prim.Rotation, out prim.Position);
                                float roll, pitch, yaw;
                                node.Transform.GetEulerAngles(out roll, out pitch, out yaw);

                                // The offset created when normalizing the mesh vertices into the OS unit cube must be rotated
                                // before being added to the position part of the Collada transform. 
                                Matrix4 rot = Matrix4.CreateFromQuaternion(prim.Rotation);              // Convert rotation to matrix for for Transform
                                Vector3 offset = Vector3.Transform(asset_offset * prim.Scale, rot);     // The offset must be rotated and mutiplied by the Collada file's scale as the offset is added during rendering with the unit cube mesh already multiplied by the compound scale.
                                prim.Position += offset;
                                prim.Scale *= asset_scale;                                              // Modify scale from Collada instance by the rescaling done in AddPositions()
                            }
                        }
                    }
                }
            }

            return Prims;
        }

        source FindSource(source[] sources, string id)
        {
            id = id.Substring(1);

            foreach (var src in sources)
            {
                if (src.id == id)
                    return src;
            }
            return null;
        }

        void AddPositions(out Vector3 scale, out Vector3 offset, mesh mesh, ModelPrim prim, Matrix4 transform)
        {
            prim.Positions = new List<Vector3>();
            source posSrc = FindSource(mesh.source, mesh.vertices.input[0].source);
            double[] posVals = ((float_array)posSrc.Item).Values;

            for (int i = 0; i < posVals.Length / 3; i++)
            {
                Vector3 pos = new Vector3((float)posVals[i * 3], (float)posVals[i * 3 + 1], (float)posVals[i * 3 + 2]);
                pos = Vector3.Transform(pos, transform);
                prim.Positions.Add(pos);
            }

            prim.BoundMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            prim.BoundMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var pos in prim.Positions)
            {
                if (pos.X > prim.BoundMax.X) prim.BoundMax.X = pos.X;
                if (pos.Y > prim.BoundMax.Y) prim.BoundMax.Y = pos.Y;
                if (pos.Z > prim.BoundMax.Z) prim.BoundMax.Z = pos.Z;

                if (pos.X < prim.BoundMin.X) prim.BoundMin.X = pos.X;
                if (pos.Y < prim.BoundMin.Y) prim.BoundMin.Y = pos.Y;
                if (pos.Z < prim.BoundMin.Z) prim.BoundMin.Z = pos.Z;
            }

            scale = prim.BoundMax - prim.BoundMin;
            offset = prim.BoundMin + (scale / 2);

            // Fit vertex positions into identity cube -0.5 .. 0.5
            for (int i = 0; i < prim.Positions.Count; i++)
            {
                Vector3 pos = prim.Positions[i];
                pos = new Vector3(
                    scale.X == 0 ? 0 : ((pos.X - prim.BoundMin.X) / scale.X) - 0.5f,
                    scale.Y == 0 ? 0 : ((pos.Y - prim.BoundMin.Y) / scale.Y) - 0.5f,
                    scale.Z == 0 ? 0 : ((pos.Z - prim.BoundMin.Z) / scale.Z) - 0.5f
                    );
                prim.Positions[i] = pos;
            }
        }

        int[] StrToArray(string s)
        {
            string[] vals = Regex.Split(s.Trim(), @"\s+");
            int[] ret = new int[vals.Length];

            for (int i = 0; i < ret.Length; i++)
            {
                int.TryParse(vals[i], out ret[i]);
            }

            return ret;
        }

        void AddFacesFromPolyList(polylist list, mesh mesh, ModelPrim prim, Matrix4 transform)
        {
            string material = list.material;
            source posSrc = null;
            source normalSrc = null;
            source uvSrc = null;

            ulong stride = 0;
            int posOffset = -1;
            int norOffset = -1;
            int uvOffset = -1;

            foreach (var inp in list.input)
            {
                stride = Math.Max(stride, inp.offset);

                if (inp.semantic == "VERTEX")
                {
                    posSrc = FindSource(mesh.source, mesh.vertices.input[0].source);
                    posOffset = (int)inp.offset;
                }
                else if (inp.semantic == "NORMAL")
                {
                    normalSrc = FindSource(mesh.source, inp.source);
                    norOffset = (int)inp.offset;
                }
                else if (inp.semantic == "TEXCOORD")
                {
                    uvSrc = FindSource(mesh.source, inp.source);
                    uvOffset = (int)inp.offset;
                }
            }

            stride += 1;

            if (posSrc == null) return;

            var vcount = StrToArray(list.vcount);
            var idx = StrToArray(list.p);

            Vector3[] normals = null;
            if (normalSrc != null)
            {
                var norVal = ((float_array)normalSrc.Item).Values;
                normals = new Vector3[norVal.Length / 3];

                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i] = new Vector3((float)norVal[i * 3 + 0], (float)norVal[i * 3 + 1], (float)norVal[i * 3 + 2]);
                    normals[i] = Vector3.TransformNormal(normals[i], transform);
                    normals[i].Normalize();
                }
            }

            Vector2[] uvs = null;
            if (uvSrc != null)
            {
                var uvVal = ((float_array)uvSrc.Item).Values;
                uvs = new Vector2[uvVal.Length / 2];

                for (int i = 0; i < uvs.Length; i++)
                {
                    uvs[i] = new Vector2((float)uvVal[i * 2 + 0], (float)uvVal[i * 2 + 1]);
                }

            }

            ModelFace face = new ModelFace();
            face.MaterialID = list.material;
            if (face.MaterialID != null)
            {
                if (MatSymTarget.ContainsKey(list.material))
                {
                    ModelMaterial mat = Materials.Find(m => m.ID == MatSymTarget[list.material]);
                    if (mat != null)
                    {
                        face.Material = mat;
                    }
                }
            }

            int curIdx = 0;

            for (int i = 0; i < vcount.Length; i++)
            {
                var nvert = vcount[i];
                if (nvert < 3 || nvert > 4)
                {
                    throw new InvalidDataException("Only triangles and quads supported");
                }

                Vertex[] verts = new Vertex[nvert];
                for (int j = 0; j < nvert; j++)
                {
                    verts[j].Position = prim.Positions[idx[curIdx + posOffset + (int)stride * j]];

                    if (normals != null)
                    {
                        verts[j].Normal = normals[idx[curIdx + norOffset + (int)stride * j]];
                    }

                    if (uvs != null)
                    {
                        verts[j].TexCoord = uvs[idx[curIdx + uvOffset + (int)stride * j]];
                    }
                }


                if (nvert == 3) // add the triangle
                {
                    face.AddVertex(verts[0]);
                    face.AddVertex(verts[1]);
                    face.AddVertex(verts[2]);
                }
                else if (nvert == 4) // quad, add two triangles
                {
                    face.AddVertex(verts[0]);
                    face.AddVertex(verts[1]);
                    face.AddVertex(verts[2]);

                    face.AddVertex(verts[0]);
                    face.AddVertex(verts[2]);
                    face.AddVertex(verts[3]);
                }

                curIdx += (int)stride * nvert;
            }

            prim.Faces.Add(face);


        }

        polylist Triangles2Polylist(triangles triangles)
        {
            polylist poly = new polylist();
            poly.count = triangles.count;
            poly.input = triangles.input;
            poly.material = triangles.material;
            poly.name = triangles.name;
            poly.p = triangles.p;

            string str = "3 ";
            System.Text.StringBuilder builder = new System.Text.StringBuilder(str.Length * (int)poly.count);
            for (int i = 0; i < (int)poly.count; i++) builder.Append(str);
            poly.vcount = builder.ToString();

            return poly;
        }

    }
}
