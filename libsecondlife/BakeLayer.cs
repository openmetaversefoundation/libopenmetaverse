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
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace libsecondlife.Baking
{
    /// <summary>
    /// A set of textures that are layered on texture of each other and "baked"
    /// in to a single texture, for avatar appearances
    /// </summary>
    public class Baker
    {
        /// <summary>Reference to the SecondLife client</summary>
        protected SecondLife Client;

        /// <summary>Appearance parameters the drive the baking process</summary>
        protected Dictionary<int, float> ParamValues;
        
        /// <summary>Wearable textures</summary>
        protected Dictionary<AppearanceManager.TextureIndex, AssetTexture> Textures = new Dictionary<AppearanceManager.TextureIndex, AssetTexture>();
        protected int TextureCount;

        public AssetTexture BakedTexture;

        /// <summary>Width of the final baked image and scratchpad</summary>
        protected int BakeWidth;
        /// <summary>Height of the final baked image and scratchpad</summary>
        protected int BakeHeight;
        /// <summary>Bake type</summary>
        public AppearanceManager.BakeType BakeType;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the SecondLife client</param>
        /// <param name="totalLayers">Total number of layers this layer set is
        /// composed of</param>
        /// <param name="paramValues">Appearance parameters the drive the 
        /// baking process</param>
        /// <param name="width">Width of the final baked image</param>
        /// <param name="height">Height of the final baked image</param>
        public Baker(SecondLife client, AppearanceManager.BakeType bakeType, int textureCount, Dictionary<int, float> paramValues)
        {
            Client = client;
            BakeType = bakeType;
            TextureCount = textureCount;

            if (bakeType == AppearanceManager.BakeType.Eyes)
            {
                BakeWidth = 128;
                BakeHeight = 128;
            }
            else
            {
                BakeWidth = 512;
                BakeHeight = 512;
            }

            ParamValues = paramValues;

            if (textureCount == 0)
                Bake();
        }
        
        /// <summary>
        /// Adds an image to this baking texture and potentially processes it, or
        /// stores it for processing later
        /// </summary>
        /// <param name="index">The baking texture index of the image to be added</param>
        /// <param name="jp2data">JPEG2000 compressed image to be added to the 
        /// baking texture</param>
        /// <returns>True if this texture is completely baked and JPEG2000 data 
        /// is available, otherwise false</returns>
        public bool AddTexture(AppearanceManager.TextureIndex index, AssetTexture texture)
        {
            lock (Textures)
            {
                texture.Decode();
                Textures.Add(index, texture);
                Client.DebugLog("Adding texture " + index.ToString() + " to bake " + BakeType.ToString());
            }

            if (Textures.Count == TextureCount)
            {
                Bake();
                return true;
            }
            else
                return false;
        }

        public bool MissingTexture(AppearanceManager.TextureIndex index)
        {
            Client.DebugLog("Missing texture " + index.ToString() + " in bake " + BakeType.ToString());
            TextureCount--;

            if (Textures.Count == TextureCount)
            {
                Bake();
                return true;
            }
            else
                return false;
        }

        protected void Bake()
        {
            Client.DebugLog("Baking " + BakeType.ToString());
            BakedTexture = new AssetTexture(new Image(BakeWidth, BakeHeight, ImageChannels.Color | ImageChannels.Alpha | ImageChannels.Bump));
            int i = 0;

            for (int y = 0; y < BakeHeight; y++)
            {
                for (int x = 0; x < BakeWidth; x++)
                {
                    if (((x ^ y) & 0x10) == 0)
                    {
                        BakedTexture.Image.Red[i] = 255;
                        BakedTexture.Image.Green[i] = 0;
                        BakedTexture.Image.Blue[i] = 0;
                        BakedTexture.Image.Alpha[i] = 255;
                        BakedTexture.Image.Bump[i] = 0;
                    }
                    else
                    {
                        BakedTexture.Image.Red[i] = 0;
                        BakedTexture.Image.Green[i] = 0;
                        BakedTexture.Image.Blue[i] = 255;
                        BakedTexture.Image.Alpha[i] = 255;
                        BakedTexture.Image.Bump[i] = 0;
                    }

                    ++i;
                }
            }

            BakedTexture.Encode();
        }

        public static AppearanceManager.BakeType BakeTypeFor(AppearanceManager.TextureIndex index)
        {
            switch (index)
            {
                case AppearanceManager.TextureIndex.HeadBodypaint:
                    return AppearanceManager.BakeType.Head;

                case AppearanceManager.TextureIndex.UpperBodypaint:
                case AppearanceManager.TextureIndex.UpperGloves:
                case AppearanceManager.TextureIndex.UpperUndershirt:
                case AppearanceManager.TextureIndex.UpperShirt:
                case AppearanceManager.TextureIndex.UpperJacket:
                    return AppearanceManager.BakeType.UpperBody;

                case AppearanceManager.TextureIndex.LowerBodypaint:
                case AppearanceManager.TextureIndex.LowerUnderpants:
                case AppearanceManager.TextureIndex.LowerSocks:
                case AppearanceManager.TextureIndex.LowerShoes:
                case AppearanceManager.TextureIndex.LowerPants:
                case AppearanceManager.TextureIndex.LowerJacket:
                    return AppearanceManager.BakeType.LowerBody;

                case AppearanceManager.TextureIndex.EyesIris:
                    return AppearanceManager.BakeType.Eyes;

                case AppearanceManager.TextureIndex.Skirt:
                    return AppearanceManager.BakeType.Skirt;

                default:
                    return AppearanceManager.BakeType.Unknown;
            }
        }
    }
}
