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
using OpenMetaverse.StructuredData;

namespace OpenMetaverse
{
    #region enums
    /// <summary>
    /// Permissions for control of object media
    /// </summary>
    [Flags]
    public enum MediaPermission : byte
    {
        None = 0,
        Owner = 1,
        Group = 2,
        Anyone = 4,
        All = None | Owner | Group | Anyone
    }

    /// <summary>
    /// Style of cotrols that shold be displayed to the user
    /// </summary>
    public enum MediaControls
    {
        Standard = 0,
        Mini
    }
    #endregion enums

    /// <summary>
    /// Class representing media data for a single face
    /// </summary>
    public class MediaEntry
    {
        /// <summary>Is display of the alternative image enabled</summary>
        public bool EnableAlterntiveImage;

        /// <summary>Should media auto loop</summary>
        public bool AutoLoop;

        /// <summary>Shoule media be auto played</summary>
        public bool AutoPlay;

        /// <summary>Auto scale media to prim face</summary>
        public bool AutoScale;

        /// <summary>Should viewer automatically zoom in on the face when clicked</summary>
        public bool AutoZoom;

        /// <summary>Should viewer interpret first click as interaction with the media
        /// or when false should the first click be treated as zoom in commadn</summary>
        public bool InteractOnFirstClick;

        /// <summary>Style of controls viewer should display when
        /// viewer media on this face</summary>
        public MediaControls Controls;

        /// <summary>Starting URL for the media</summary>
        public string HomeURL;

        /// <summary>Currently navigated URL</summary>
        public string CurrentURL;

        /// <summary>Media height in pixes</summary>
        public int Height;

        /// <summary>Media width in pixels</summary>
        public int Width;

        /// <summary>Who can controls the media</summary>
        public MediaPermission ControlPermissions;

        /// <summary>Who can interact with the media</summary>
        public MediaPermission InteractPermissions;

        /// <summary>Is URL whitelist enabled</summary>
        public bool EnableWhiteList;

        /// <summary>Array of URLs that are whitelisted</summary>
        public string[] WhiteList;

        /// <summary>
        /// Serialize to OSD
        /// </summary>
        /// <returns>OSDMap with the serialized data</returns>
        public OSDMap GetOSD()
        {
            OSDMap map = new OSDMap();

            map["alt_image_enable"] = OSD.FromBoolean(EnableAlterntiveImage);
            map["auto_loop"] = OSD.FromBoolean(AutoLoop);
            map["auto_play"] = OSD.FromBoolean(AutoPlay);
            map["auto_scale"] = OSD.FromBoolean(AutoScale);
            map["auto_zoom"] = OSD.FromBoolean(AutoZoom);
            map["controls"] = OSD.FromInteger((int)Controls);
            map["current_url"] = OSD.FromString(CurrentURL);
            map["first_click_interact"] = OSD.FromBoolean(InteractOnFirstClick);
            map["height_pixels"] = OSD.FromInteger(Height);
            map["home_url"] = OSD.FromString(HomeURL);
            map["perms_control"] = OSD.FromInteger((int)ControlPermissions);
            map["perms_interact"] = OSD.FromInteger((int)InteractPermissions);

            List<OSD> wl = new List<OSD>();
            if (WhiteList != null && WhiteList.Length > 0)
            {
                for (int i = 0; i < WhiteList.Length; i++)
                    wl.Add(OSD.FromString(WhiteList[i]));
            }

            map["whitelist"] = new OSDArray(wl);
            map["whitelist_enable"] = OSD.FromBoolean(EnableWhiteList);
            map["width_pixels"] = OSD.FromInteger(Width);

            return map;
        }

        /// <summary>
        /// Deserialize from OSD data
        /// </summary>
        /// <param name="osd">Serialized OSD data</param>
        /// <returns>Deserialized object</returns>
        public static MediaEntry FromOSD(OSD osd)
        {
            MediaEntry m = new MediaEntry();
            OSDMap map = (OSDMap)osd;

            m.EnableAlterntiveImage = map["alt_image_enable"].AsBoolean();
            m.AutoLoop = map["auto_loop"].AsBoolean();
            m.AutoPlay = map["auto_play"].AsBoolean();
            m.AutoScale = map["auto_scale"].AsBoolean();
            m.AutoZoom = map["auto_zoom"].AsBoolean();
            m.Controls = (MediaControls)map["controls"].AsInteger();
            m.CurrentURL = map["current_url"].AsString();
            m.InteractOnFirstClick = map["first_click_interact"].AsBoolean();
            m.Height = map["height_pixels"].AsInteger();
            m.HomeURL = map["home_url"].AsString();
            m.ControlPermissions = (MediaPermission)map["perms_control"].AsInteger();
            m.InteractPermissions = (MediaPermission)map["perms_interact"].AsInteger();

            if (map["whitelist"].Type == OSDType.Array)
            {
                OSDArray wl = (OSDArray)map["whitelist"];
                if (wl.Count > 0)
                {
                    m.WhiteList = new string[wl.Count];
                    for (int i = 0; i < wl.Count; i++)
                    {
                        m.WhiteList[i] = wl[i].AsString();
                    }
                }
            }

            m.EnableWhiteList = map["whitelist_enable"].AsBoolean();
            m.Width = map["width_pixels"].AsInteger();

            return m;
        }
    }

    public partial class Primitive
    {
        /// <summary>
        /// Current version of the media data for the prim
        /// </summary>
        public string MediaVersion = string.Empty;

        /// <summary>
        /// Array of media entries indexed by face number
        /// </summary>
        public MediaEntry[] FaceMedia;
    }
}
