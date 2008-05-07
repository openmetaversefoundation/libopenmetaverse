/*
 * Copyright (c) 2007-2008, Second Life Reverse Engineering Team
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
using System.Drawing;
using System.Drawing.Imaging;

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
        /// <param name="bakeType"></param>
        /// <param name="textureCount">Total number of layers this layer set is
        /// composed of</param>
        /// <param name="paramValues">Appearance parameters the drive the 
        /// baking process</param>
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
            {
                Bake();
            }
        }

        /// <summary>
        /// Adds an image to this baking texture and potentially processes it, or
        /// stores it for processing later
        /// </summary>
        /// <param name="index">The baking texture index of the image to be added</param>
        /// <param name="texture">JPEG2000 compressed image to be
        /// added to the baking texture</param>
        /// <returns>True if this texture is completely baked and JPEG2000 data 
        /// is available, otherwise false</returns>
        public bool AddTexture(AppearanceManager.TextureIndex index, AssetTexture texture)
        {
            lock (Textures)
            {
                try 
                {

                    texture.Decode();
                    Textures.Add(index, texture);
                    Client.DebugLog("Adding texture " + index.ToString() + " ID: " + texture.AssetID.ToString() + " to bake " + BakeType.ToString());
                }
                catch ( Exception e )
                {
                    Client.DebugLog( "caught exception while trying add texture: " + e.Message.ToString());
                }

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
            BakedTexture = new AssetTexture(new Image(BakeWidth, BakeHeight, ImageChannels.Color | ImageChannels.Alpha | ImageChannels.Bump));

            if (BakeType == AppearanceManager.BakeType.Eyes)
            {
                initBakedLayerColor(255, 255, 255);
                if (!DrawLayer(AppearanceManager.TextureIndex.EyesIris))
                {
                    Client.Log("Missing texture for EYES - unable to bake layer", Helpers.LogLevel.Warning);
                }
            }
            else if (BakeType == AppearanceManager.BakeType.Head)
            {
                // need to use the visual parameters to determine the base skin color in RGB but
                // it's not apparent how to define RGB levels from the skin color parameters, so
                // for now use a grey foundation for the skin and skirt layers
                initBakedLayerColor(128, 128, 128);
                DrawLayer(AppearanceManager.TextureIndex.HeadBodypaint);
            }
            else if (BakeType == AppearanceManager.BakeType.Skirt)
            {
                float skirtRed = 1.0f, skirtGreen = 1.0f, skirtBlue = 1.0f;
                try
                {
                    ParamValues.TryGetValue(VisualParams.Find("skirt_red", "skirt").ParamID, out skirtRed);
                    ParamValues.TryGetValue(VisualParams.Find("skirt_green", "skirt").ParamID, out skirtGreen);
                    ParamValues.TryGetValue(VisualParams.Find("skirt_blue", "skirt").ParamID, out skirtBlue);
                }
                catch
                {
                    Client.Log("Unable to determine skirt color from visual params", Helpers.LogLevel.Warning);
                }
                initBakedLayerColor((int)(skirtRed * 255.0f), (int)(skirtGreen * 255.0f), (int)(skirtBlue * 255.0f));
                DrawLayer(AppearanceManager.TextureIndex.Skirt);
            }
            else if (BakeType == AppearanceManager.BakeType.UpperBody)
            {
                initBakedLayerColor(128, 128, 128);
                DrawLayer(AppearanceManager.TextureIndex.UpperBodypaint);
                DrawLayer(AppearanceManager.TextureIndex.UpperUndershirt);
                DrawLayer(AppearanceManager.TextureIndex.UpperGloves);
                DrawLayer(AppearanceManager.TextureIndex.UpperShirt);
                DrawLayer(AppearanceManager.TextureIndex.UpperJacket);
            }
            else if (BakeType == AppearanceManager.BakeType.LowerBody)
            {
                initBakedLayerColor(128, 128, 128);
                DrawLayer(AppearanceManager.TextureIndex.LowerBodypaint);
                DrawLayer(AppearanceManager.TextureIndex.LowerUnderpants);
                DrawLayer(AppearanceManager.TextureIndex.LowerSocks);
                DrawLayer(AppearanceManager.TextureIndex.LowerShoes);
                DrawLayer(AppearanceManager.TextureIndex.LowerPants);
                DrawLayer(AppearanceManager.TextureIndex.LowerJacket);
            }

            BakedTexture.Encode();

        }

        private bool DrawLayer(AppearanceManager.TextureIndex textureIndex)
        {
            int i = 0;

            AssetTexture texture = new AssetTexture();

            if (!Textures.TryGetValue(textureIndex, out texture))
                return false;

            Image source = texture.Image;
            bool sourceHasAlpha = ((source.Channels & ImageChannels.Alpha) != 0 && source.Alpha != null);
            bool sourceHasBump = ((source.Channels & ImageChannels.Bump) != 0 && source.Bump != null);

            bool copySourceAlphaToBakedLayer = sourceHasAlpha && (
                textureIndex == AppearanceManager.TextureIndex.HeadBodypaint ||
                textureIndex == AppearanceManager.TextureIndex.Skirt
            );

            if (source.Width != BakeWidth || source.Height != BakeHeight)
                source.ResizeNearestNeighbor(BakeWidth, BakeHeight);


            Int32 alpha = 255;
            //Int32 alphaInv = 255 - alpha;
            Int32 alphaInv = 256 - alpha;

            byte[] bakedRed = BakedTexture.Image.Red;
            byte[] bakedGreen = BakedTexture.Image.Green;
            byte[] bakedBlue = BakedTexture.Image.Blue;
            byte[] bakedAlpha = BakedTexture.Image.Alpha;
            byte[] bakedBump = BakedTexture.Image.Bump;

            byte[] sourceRed = source.Red;
            byte[] sourceGreen = source.Green;
            byte[] sourceBlue = source.Blue;
            byte[] sourceAlpha = null;
            byte[] sourceBump = null;

            if (sourceHasAlpha)
                sourceAlpha = source.Alpha;

            if (sourceHasBump)
                sourceBump = source.Bump;

            for (int y = 0; y < BakeHeight; y++)
            {
                for (int x = 0; x < BakeWidth; x++)
                {
                    if (sourceHasAlpha)
                    {
                        alpha = sourceAlpha[i];
                        //alphaInv = 255 - alpha;
                        alphaInv = 256 - alpha;
                    }

                    bakedRed[i] = (byte)((bakedRed[i] * alphaInv + sourceRed[i] * alpha) >> 8);
                    bakedGreen[i] = (byte)((bakedGreen[i] * alphaInv + sourceGreen[i] * alpha) >> 8);
                    bakedBlue[i] = (byte)((bakedBlue[i] * alphaInv + sourceBlue[i] * alpha) >> 8);

                    if (copySourceAlphaToBakedLayer)
                        bakedAlpha[i] = sourceAlpha[i];

                    if (sourceHasBump)
                        bakedBump[i] = sourceBump[i];

                    i++;

                }
            }
            return true;
        }


        /// <summary>
        /// initBakedLayerColor()
        /// fills a baked layer as a solid *appearing* color
        /// the colors are subtly dithered on a 16x16 grid to prevent the jpeg2000 stage from compressing it
        /// too far since it seems to cause upload failures if the image is a pure solid color
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        private void initBakedLayerColor(int r, int g, int b)
        {
            byte rByte = (byte)r;
            byte gByte = (byte)g;
            byte bByte = (byte)b;

            byte rAlt, gAlt, bAlt;

            rAlt = rByte;
            gAlt = gByte;
            bAlt = bByte;

            if (rByte < byte.MaxValue)
                rAlt++;
            else rAlt--;

            if (gByte < byte.MaxValue)
                gAlt++;
            else gAlt--;

            if (bByte < byte.MaxValue)
                bAlt++;
            else bAlt--;

            int i = 0;

            byte[] red = BakedTexture.Image.Red;
            byte[] green = BakedTexture.Image.Green;
            byte[] blue = BakedTexture.Image.Blue;
            byte[] alpha = BakedTexture.Image.Alpha;
            byte[] bump = BakedTexture.Image.Bump;

            for (int y = 0; y < BakeHeight; y++)
            {
                for (int x = 0; x < BakeWidth; x++)
                {
                    if (((x ^ y) & 0x10) == 0)
                    {
                        red[i] = rAlt;
                        green[i] = gByte;
                        blue[i] = bByte;
                        alpha[i] = 255;
                        bump[i] = 0;
                    }
                    else
                    {
                        red[i] = rByte;
                        green[i] = gAlt;
                        blue[i] = bAlt;
                        alpha[i] = 255;
                        bump[i] = 0;
                    }

                    ++i;
                }
            }

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
