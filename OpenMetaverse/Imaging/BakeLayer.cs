/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using OpenMetaverse.Assets;

namespace OpenMetaverse.Imaging
{
    /// <summary>
    /// A set of textures that are layered on texture of each other and "baked"
    /// in to a single texture, for avatar appearances
    /// </summary>
    public class Baker
    {
        public AssetTexture BakedTexture { get { return _bakedTexture; } }
        public Dictionary<int, float> ParamValues { get { return _paramValues; } }
        public Dictionary<AppearanceManager.TextureIndex, AssetTexture> Textures { get { return _textures; } }
        public int TextureCount { get { return _textureCount; } }
        public int BakeWidth { get { return _bakeWidth; } }
        public int BakeHeight { get { return _bakeHeight; } }
        public AppearanceManager.BakeType BakeType { get { return _bakeType; } }

        /// <summary>Reference to the GridClient object</summary>
        protected GridClient _client;
        /// <summary>Finald baked texture</summary>
        protected AssetTexture _bakedTexture;
        /// <summary>Appearance parameters the drive the baking process</summary>
        protected Dictionary<int, float> _paramValues;
        /// <summary>Wearable textures</summary>
        protected Dictionary<AppearanceManager.TextureIndex, AssetTexture> _textures = new Dictionary<AppearanceManager.TextureIndex, AssetTexture>();
        /// <summary>Total number of textures in the bake</summary>
        protected int _textureCount;
        /// <summary>Width of the final baked image and scratchpad</summary>
        protected int _bakeWidth;
        /// <summary>Height of the final baked image and scratchpad</summary>
        protected int _bakeHeight;
        /// <summary>Bake type</summary>
        protected AppearanceManager.BakeType _bakeType;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client">Reference to the GridClient object</param>
        /// <param name="bakeType"></param>
        /// <param name="textureCount">Total number of layers this layer set is
        /// composed of</param>
        /// <param name="paramValues">Appearance parameters the drive the 
        /// baking process</param>
        public Baker(GridClient client, AppearanceManager.BakeType bakeType, int textureCount, Dictionary<int, float> paramValues)
        {
            _client = client;
            _bakeType = bakeType;
            _textureCount = textureCount;

            if (bakeType == AppearanceManager.BakeType.Eyes)
            {
                _bakeWidth = 128;
                _bakeHeight = 128;
            }
            else
            {
                _bakeWidth = 512;
                _bakeHeight = 512;
            }

            _paramValues = paramValues;

            if (textureCount == 0)
                Bake();
        }

        /// <summary>
        /// Adds an image to this baking texture and potentially processes it, or
        /// stores it for processing later
        /// </summary>
        /// <param name="index">The baking texture index of the image to be added</param>
        /// <param name="texture">JPEG2000 compressed image to be
        /// added to the baking texture</param>
        /// <param name="needsDecode">True if <code>Decode()</code> needs to be
        /// called for the texture, otherwise false</param>
        /// <returns>True if this texture is completely baked and JPEG2000 data 
        /// is available, otherwise false</returns>
        public bool AddTexture(AppearanceManager.TextureIndex index, AssetTexture texture, bool needsDecode)
        {
            lock (_textures)
            {
                if (needsDecode)
                {
                    try
                    {
                        texture.Decode();
                    }
                    catch (Exception e)
                    {
                        Logger.Log(String.Format("AddTexture({0}, {1})", index, texture.AssetID), Helpers.LogLevel.Error, e);
                        return false;
                    }
                }

                _textures.Add(index, texture);
                Logger.DebugLog(String.Format("Added texture {0} (ID: {1}) to bake {2}", index, texture.AssetID, _bakeType), _client);
            }

            if (_textures.Count >= _textureCount)
            {
                Bake();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool MissingTexture(AppearanceManager.TextureIndex index)
        {
            Logger.DebugLog(String.Format("Missing texture {0} in bake {1}", index, _bakeType), _client);
            _textureCount--;

            if (_textures.Count >= _textureCount)
            {
                Bake();
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void Bake()
        {
            _bakedTexture = new AssetTexture(new ManagedImage(_bakeWidth, _bakeHeight,
                ManagedImage.ImageChannels.Color | ManagedImage.ImageChannels.Alpha | ManagedImage.ImageChannels.Bump));

            if (_bakeType == AppearanceManager.BakeType.Eyes)
            {
                InitBakedLayerColor(255, 255, 255);
                if (!DrawLayer(AppearanceManager.TextureIndex.EyesIris))
                {
                    Logger.Log("Missing texture for EYES - unable to bake layer", Helpers.LogLevel.Warning, _client);
                }
            }
            else if (_bakeType == AppearanceManager.BakeType.Head)
            {
                // FIXME: Need to use the visual parameters to determine the base skin color in RGB but
                // it's not apparent how to define RGB levels from the skin color parameters, so
                // for now use a grey foundation for the skin
                InitBakedLayerColor(128, 128, 128);
                DrawLayer(AppearanceManager.TextureIndex.HeadBodypaint);

                // HACK: Bake the eyelashes in if we have them
                ManagedImage eyelashes = LoadAlphaLayer("head_alpha.tga");

                if (eyelashes != null)
                {
                    Logger.DebugLog("Loaded head_alpha.tga, baking in eyelashes");
                    DrawLayer(eyelashes, true);
                }
                else
                {
                    Logger.Log("head_alpha.tga resource not found, skipping eyelashes", Helpers.LogLevel.Info);
                }
            }
            else if (_bakeType == AppearanceManager.BakeType.Skirt)
            {
                float skirtRed = 1.0f, skirtGreen = 1.0f, skirtBlue = 1.0f;

                try
                {
                    _paramValues.TryGetValue(VisualParams.Find("skirt_red", "skirt").ParamID, out skirtRed);
                    _paramValues.TryGetValue(VisualParams.Find("skirt_green", "skirt").ParamID, out skirtGreen);
                    _paramValues.TryGetValue(VisualParams.Find("skirt_blue", "skirt").ParamID, out skirtBlue);
                }
                catch
                {
                    Logger.Log("Unable to determine skirt color from visual params", Helpers.LogLevel.Warning, _client);
                }

                InitBakedLayerColor((byte)(skirtRed * 255.0f), (byte)(skirtGreen * 255.0f), (byte)(skirtBlue * 255.0f));
                DrawLayer(AppearanceManager.TextureIndex.Skirt);
            }
            else if (_bakeType == AppearanceManager.BakeType.UpperBody)
            {
                InitBakedLayerColor(128, 128, 128);
                DrawLayer(AppearanceManager.TextureIndex.UpperBodypaint);
                DrawLayer(AppearanceManager.TextureIndex.UpperUndershirt);
                DrawLayer(AppearanceManager.TextureIndex.UpperGloves);
                DrawLayer(AppearanceManager.TextureIndex.UpperShirt);
                DrawLayer(AppearanceManager.TextureIndex.UpperJacket);
            }
            else if (_bakeType == AppearanceManager.BakeType.LowerBody)
            {
                InitBakedLayerColor(128, 128, 128);
                DrawLayer(AppearanceManager.TextureIndex.LowerBodypaint);
                DrawLayer(AppearanceManager.TextureIndex.LowerUnderpants);
                DrawLayer(AppearanceManager.TextureIndex.LowerSocks);
                DrawLayer(AppearanceManager.TextureIndex.LowerShoes);
                DrawLayer(AppearanceManager.TextureIndex.LowerPants);
                DrawLayer(AppearanceManager.TextureIndex.LowerJacket);
            }

            _bakedTexture.Encode();
        }

        private bool DrawLayer(AppearanceManager.TextureIndex textureIndex)
        {
            AssetTexture texture;
            bool useSourceAlpha = 
                (textureIndex == AppearanceManager.TextureIndex.HeadBodypaint ||
                textureIndex == AppearanceManager.TextureIndex.Skirt);

            if (_textures.TryGetValue(textureIndex, out texture))
                return DrawLayer(texture.Image, useSourceAlpha);
            else
                return false;
        }

        private bool DrawLayer(ManagedImage source, bool useSourceAlpha)
        {
            bool sourceHasColor;
            bool sourceHasAlpha;
            bool sourceHasBump;
            int i = 0;

            sourceHasColor = ((source.Channels & ManagedImage.ImageChannels.Color) != 0 &&
                    source.Red != null && source.Green != null && source.Blue != null);
            sourceHasAlpha = ((source.Channels & ManagedImage.ImageChannels.Alpha) != 0 && source.Alpha != null);
            sourceHasBump = ((source.Channels & ManagedImage.ImageChannels.Bump) != 0 && source.Bump != null);

            useSourceAlpha = (useSourceAlpha && sourceHasAlpha);

            if (source.Width != _bakeWidth || source.Height != _bakeHeight)
            {
                try { source.ResizeNearestNeighbor(_bakeWidth, _bakeHeight); }
                catch { return false; }
            }

            int alpha = 255;
            //int alphaInv = 255 - alpha;
            int alphaInv = 256 - alpha;

            byte[] bakedRed = _bakedTexture.Image.Red;
            byte[] bakedGreen = _bakedTexture.Image.Green;
            byte[] bakedBlue = _bakedTexture.Image.Blue;
            byte[] bakedAlpha = _bakedTexture.Image.Alpha;
            byte[] bakedBump = _bakedTexture.Image.Bump;

            byte[] sourceRed = source.Red;
            byte[] sourceGreen = source.Green;
            byte[] sourceBlue = source.Blue;
            byte[] sourceAlpha = null;
            byte[] sourceBump = null;

            if (sourceHasAlpha)
                sourceAlpha = source.Alpha;

            if (sourceHasBump)
                sourceBump = source.Bump;

            for (int y = 0; y < _bakeHeight; y++)
            {
                for (int x = 0; x < _bakeWidth; x++)
                {
                    if (sourceHasAlpha)
                    {
                        alpha = sourceAlpha[i];
                        //alphaInv = 255 - alpha;
                        alphaInv = 256 - alpha;
                    }

                    if (sourceHasColor)
                    {
                        bakedRed[i] = (byte)((bakedRed[i] * alphaInv + sourceRed[i] * alpha) >> 8);
                        bakedGreen[i] = (byte)((bakedGreen[i] * alphaInv + sourceGreen[i] * alpha) >> 8);
                        bakedBlue[i] = (byte)((bakedBlue[i] * alphaInv + sourceBlue[i] * alpha) >> 8);
                    }

                    if (useSourceAlpha)
                        bakedAlpha[i] = sourceAlpha[i];

                    if (sourceHasBump)
                        bakedBump[i] = sourceBump[i];

                    ++i;
                }
            }

            return true;
        }

        /// <summary>
        /// Fills a baked layer as a solid *appearing* color. The colors are 
        /// subtly dithered on a 16x16 grid to prevent the JPEG2000 stage from 
        /// compressing it too far since it seems to cause upload failures if 
        /// the image is a pure solid color
        /// </summary>
        /// <param name="r">Red value</param>
        /// <param name="g">Green value</param>
        /// <param name="b">Blue value</param>
        private void InitBakedLayerColor(byte r, byte g, byte b)
        {
            byte rByte = r;
            byte gByte = g;
            byte bByte = b;

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

            byte[] red = _bakedTexture.Image.Red;
            byte[] green = _bakedTexture.Image.Green;
            byte[] blue = _bakedTexture.Image.Blue;
            byte[] alpha = _bakedTexture.Image.Alpha;
            byte[] bump = _bakedTexture.Image.Bump;

            for (int y = 0; y < _bakeHeight; y++)
            {
                for (int x = 0; x < _bakeWidth; x++)
                {
                    if (((x ^ y) & 0x10) == 0)
                    {
                        red[i] = rAlt;
                        green[i] = gByte;
                        blue[i] = bByte;
                        alpha[i] = 128;
                        bump[i] = 0;
                    }
                    else
                    {
                        red[i] = rByte;
                        green[i] = gAlt;
                        blue[i] = bAlt;
                        alpha[i] = 128;
                        bump[i] = 0;
                    }

                    ++i;
                }
            }

        }

        public static ManagedImage LoadAlphaLayer(string fileName)
        {
            Stream stream = Helpers.GetResourceStream(fileName);

            if (stream != null)
            {
                try
                {
                    // FIXME: It would save cycles and memory if we wrote a direct
                    // loader to ManagedImage for these files instead of using the
                    // TGA loader
                    Bitmap bitmap = LoadTGAClass.LoadTGA(stream);
                    stream.Close();

                    ManagedImage alphaLayer = new ManagedImage(bitmap);

                    // Disable all layers except the alpha layer
                    alphaLayer.Red = null;
                    alphaLayer.Green = null;
                    alphaLayer.Blue = null;
                    alphaLayer.Channels = ManagedImage.ImageChannels.Alpha;

                    return alphaLayer;
                }
                catch (Exception e)
                {
                    Logger.Log(String.Format("LoadAlphaLayer() failed on file: {0} ({1}", fileName, e.Message),
                        Helpers.LogLevel.Error, e);
                    return null;
                }
            }
            else
            {
                return null;
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
