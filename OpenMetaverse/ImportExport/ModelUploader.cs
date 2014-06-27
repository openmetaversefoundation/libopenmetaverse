/*
 * Copyright (c) 2006-2014, openmetaverse.org
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
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Http;

namespace OpenMetaverse.ImportExport
{
    /// <summary>
    /// Implements mesh upload communications with the simulator
    /// </summary>
    public class ModelUploader
    {
        /// <summary>
        /// Inlcude stub convex hull physics, required for uploading to Second Life
        /// </summary>
        public bool IncludePhysicsStub;

        /// <summary>
        /// Use the same mesh used for geometry as the physical mesh upload
        /// </summary>
        public bool UseModelAsPhysics;

        GridClient Client;
        List<ModelPrim> Prims;

        public delegate void ModelUploadCallback(OSD result);

        /// <summary>
        /// Creates instance of the mesh uploader
        /// </summary>
        /// <param name="client">GridClient instance to communicate with the simulator</param>
        /// <param name="prims">List of ModelPrimitive objects to upload as a linkset</param>
        public ModelUploader(GridClient client, List<ModelPrim> prims)
        {
            this.Client = client;
            this.Prims = prims;
        }

        OSD AssetResources(bool includeImages)
        {
            OSDArray instanceList = new OSDArray();
            List<byte[]> meshes = new List<byte[]>();
            List<byte[]> textures = new List<byte[]>();

            foreach (var prim in Prims)
            {
                OSDMap primMap = new OSDMap();

                OSDArray faceList = new OSDArray();

                foreach (var face in prim.Faces)
                {
                    OSDMap faceMap = new OSDMap();

                    faceMap["diffuse_color"] = face.Material.DiffuseColor;
                    faceMap["fullbright"] = false;

                    faceList.Add(faceMap);
                }

                primMap["face_list"] = faceList;

                primMap["position"] = prim.Position;
                primMap["rotation"] = prim.Rotation;
                primMap["scale"] = prim.Scale;

                primMap["material"] = 3; // always sent as "wood" material
                primMap["physics_shape_type"] = 2; // always sent as "convex hull";
                primMap["mesh"] = meshes.Count;
                meshes.Add(prim.Asset);


                instanceList.Add(primMap);
            }

            OSDMap resources = new OSDMap();
            resources["instance_list"] = instanceList;

            OSDArray meshList = new OSDArray();
            foreach (var mesh in meshes)
            {
                meshList.Add(OSD.FromBinary(mesh));
            }
            resources["mesh_list"] = meshList;

            OSDArray textureList = new OSDArray();
            resources["texture_list"] = textureList;

            resources["metric"] = "MUT_Unspecified";


            return resources;
        }

        /// <summary>
        /// Performs model upload in one go, without first checking for the price
        /// </summary>
        public void Upload()
        {
            Upload(null);
        }

        /// <summary>
        /// Performs model upload in one go, without first checking for the price
        /// </summary>
        /// <param name="callback">Callback that will be invoke upon completion of the upload. Null is sent on request failure</param>
        public void Upload(ModelUploadCallback callback)
        {
            PrepareUpload((result =>
            {
                if (result is OSDMap)
                {
                    var res = (OSDMap)result;
                    Uri uploader = new Uri(res["uploader"]);
                    PerformUpload(uploader, callback);
                }
            }));

        }

        /// <summary>
        /// Ask server for details of cost and impact of the mesh upload
        /// </summary>
        /// <param name="callback">Callback that will be invoke upon completion of the upload. Null is sent on request failure</param>
        public void PrepareUpload(ModelUploadCallback callback)
        {
            Uri url = null;
            if (Client.Network.CurrentSim == null ||
                Client.Network.CurrentSim.Caps == null ||
                null == (url = Client.Network.CurrentSim.Caps.CapabilityURI("NewFileAgentInventory")))
            {
                Logger.Log("Cannot upload mesh, no connection or NewFileAgentInventory not available", Helpers.LogLevel.Warning);
                return;
            }

            OSDMap req = new OSDMap();
            req["name"] = "Test upload object";
            req["description"] = "Radegast mesh upload " + DateTime.Now.ToString();

            req["asset_resources"] = AssetResources(false);
            req["asset_type"] = "mesh";
            req["inventory_type"] = "object";

            req["folder_id"] = Client.Inventory.FindFolderForType(AssetType.Object);
            req["texture_folder_id"] = Client.Inventory.FindFolderForType(AssetType.Texture);

            req["everyone_mask"] = (int)PermissionMask.All;
            req["group_mask"] = (int)PermissionMask.All; ;
            req["next_owner_mask"] = (int)PermissionMask.All;

            CapsClient request = new CapsClient(url);
            request.OnComplete += (client, result, error) =>
            {
                if (error != null || result == null || result.Type != OSDType.Map)
                {
                    Logger.Log("Mesh upload request failure", Helpers.LogLevel.Error);
                    if (callback != null) callback(null);
                    return;
                }
                OSDMap res = (OSDMap)result;

                if (res["state"] != "upload")
                {
                    Logger.Log("Mesh upload failure: " + res["message"], Helpers.LogLevel.Error);
                    if (callback != null) callback(null);
                    return;
                }

                Logger.Log("Response from mesh upload prepare:\n" + OSDParser.SerializeLLSDNotationFormatted(result), Helpers.LogLevel.Debug);
                if (callback != null) callback(result);
            };

            request.BeginGetResponse(req, OSDFormat.Xml, 60 * 1000);

        }

        /// <summary>
        /// Performas actual mesh and image upload
        /// </summary>
        /// <param name="uploader">Uri recieved in the upload prepare stage</param>
        /// <param name="callback">Callback that will be invoke upon completion of the upload. Null is sent on request failure</param>
        public void PerformUpload(Uri uploader, ModelUploadCallback callback)
        {
            CapsClient request = new CapsClient(uploader);
            request.OnComplete += (client, result, error) =>
            {
                if (error != null || result == null || result.Type != OSDType.Map)
                {
                    Logger.Log("Mesh upload request failure", Helpers.LogLevel.Error);
                    if (callback != null) callback(null);
                    return;
                }
                OSDMap res = (OSDMap)result;
                Logger.Log("Response from mesh upload perform:\n" + OSDParser.SerializeLLSDNotationFormatted(result), Helpers.LogLevel.Debug);
                if (callback != null) callback(res);
            };

            request.BeginGetResponse(AssetResources(true), OSDFormat.Xml, 60 * 1000);
        }


    }
}