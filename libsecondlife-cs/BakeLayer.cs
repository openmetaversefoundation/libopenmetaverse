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
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace libsecondlife
{
    /// <summary>
    /// A set of textures that are layered on top of each other and "baked"
    /// in to a single texture, for avatar appearances
    /// </summary>
    public class BakeLayer
    {
        public enum BakeOrder
        {
            Unknown = -1,
            HeadBodypaint = 0,
            Hair,
            EyesIris,
            UpperBodypaint,
            UpperUndershirt,
            UpperShirt,
            UpperJacket,
            LowerBodypaint,
            LowerUnderpants,
            LowerSocks,
            LowerShoes,
            LowerPants,
            LowerJacket,
            Skirt
        }

        /// <summary>Maximum number of wearables for any baked layer</summary>
        public const int WEARABLES_PER_LAYER = 7;

        /// <summary>Final compressed JPEG2000 data</summary>
        public byte[] FinalData = new byte[0];
        /// <summary>Whether this bake is complete or not</summary>
        public bool Finished = false;

        /// <summary>Reference to the SecondLife client</summary>
        protected SecondLife Client;
        /// <summary>Total number of textures in this bake</summary>
        protected int TotalLayers;
        /// <summary>Appearance parameters the drive the baking process</summary>
        protected Dictionary<int, float> ParamValues;
        /// <summary>GDI+ image that textures are composited to</summary>
        protected Bitmap Scratchpad;
        /// <summary>List of textures sorted by their baking order</summary>
        protected SortedList<BakeOrder, byte[]> Textures = new SortedList<BakeOrder, byte[]>(WEARABLES_PER_LAYER);
        /// <summary>Width of the final baked image and scratchpad</summary>
        protected int BakeWidth = 512;
        /// <summary>Height of the final baked image and scratchpad</summary>
        protected int BakeHeight = 512;


        private Assembly assembly = null;
        

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the SecondLife client</param>
        /// <param name="totalLayers">Total number of layers this bake set is
        /// composed of</param>
        /// <param name="paramValues">Appearance parameters the drive the 
        /// baking process</param>
        public BakeLayer(SecondLife client, int totalLayers, Dictionary<int, float> paramValues)
        {
            Client = client;
            TotalLayers = totalLayers;
            ParamValues = paramValues;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the SecondLife client</param>
        /// <param name="totalLayers">Total number of layers this bake set is
        /// composed of</param>
        /// <param name="paramValues">Appearance parameters the drive the 
        /// baking process</param>
        /// <param name="width">Width of the final baked image</param>
        /// <param name="height">Height of the final baked image</param>
        public BakeLayer(SecondLife client, int totalLayers, Dictionary<int, float> paramValues, int width, int height)
        {
            Client = client;
            TotalLayers = totalLayers;
            ParamValues = paramValues;
            BakeWidth = width;
            BakeHeight = height;
        }

        /// <summary>
        /// Adds an image to this baking layer and potentially processes it, or
        /// stores it for processing later
        /// </summary>
        /// <param name="index">The baking layer index of the image to be added</param>
        /// <param name="jp2data">JPEG2000 compressed image to be added to the 
        /// baking layer</param>
        /// <returns>True if this layer is completely baked and JPEG2000 data 
        /// is available, otherwise false</returns>
        public bool AddImage(BakeOrder index, byte[] jp2data)
        {
            lock (Textures)
            {
                Textures.Add(index, jp2data);

                if (Textures.Count == TotalLayers)
                {
                    // All of the layers are in place, we can bake
                    Bake();
                    Finished = true;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Create the various dynamic alpha masks, apply them to the affected
        /// textures, and composite all of the textures in to the final scratch
        /// pad
        /// </summary>
        protected void Bake()
        {
            if (TotalLayers == 1)
            {
                // FIXME: Create a properly formatted JP2 bake (5 comps)
            }
            else
            {
                Client.Log("Too many layers for the null baking code!", Helpers.LogLevel.Error);
            }
        }

        private StreamReader GetResource(string resourceName)
        {
            if (assembly == null) assembly = Assembly.GetExecutingAssembly();

            return new StreamReader(assembly.GetManifestResourceStream(String.Format("libsecondlife.Resources.{0}",
                resourceName)));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UpperBakeLayer : BakeLayer
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the SecondLife client</param>
        /// <param name="totalLayers">Total number of layers this bake set is
        /// composed of</param>
        /// <param name="paramValues">Appearance parameters the drive the 
        /// baking process</param>
        public UpperBakeLayer(SecondLife client, int totalLayers, Dictionary<int, float> paramValues)
            : base(client, totalLayers, paramValues)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected new void Bake()
        {
            // FIXME: Iterate through each texture, generate the alpha masks and apply them,
            // and combine the masked texture in to the final scratch pad
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LowerBakeLayer : BakeLayer
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the SecondLife client</param>
        /// <param name="totalLayers">Total number of layers this bake set is
        /// composed of</param>
        /// <param name="paramValues">Appearance parameters the drive the 
        /// baking process</param>
        public LowerBakeLayer(SecondLife client, int totalLayers, Dictionary<int, float> paramValues)
            : base(client, totalLayers, paramValues)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected new void Bake()
        {
            // FIXME: Iterate through each texture, generate the alpha masks and apply them,
            // and combine the masked texture in to the final scratch pad
        }
    }
}
