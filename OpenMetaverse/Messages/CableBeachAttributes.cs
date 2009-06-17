/*
 * Copyright (c) 2009, openmetaverse.org
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

namespace OpenMetaverse.Messages.CableBeach
{
    public static class CableBeachServices
    {
        public const string ASSETS = "http://openmetaverse.org/services/assets";
        public const string ASSET_CREATE_ASSET = "http://openmetaverse.org/services/assets/create_asset";
        public const string ASSET_GET_ASSET = "http://openmetaverse.org/services/assets/get_asset";
        public const string ASSET_GET_ASSET_METADATA = "http://openmetaverse.org/services/assets/get_asset_metadata";

        public const string FILESYSTEM = "http://openmetaverse.org/services/filesystem";
        public const string FILESYSTEM_CREATE_FILESYSTEM = "http://openmetaverse.org/services/filesystem/create_filesystem";
        public const string FILESYSTEM_CREATE_OBJECT = "http://openmetaverse.org/services/filesystem/create_object";
        public const string FILESYSTEM_FETCH_OBJECT = "http://openmetaverse.org/services/filesystem/fetch_object";
        public const string FILESYSTEM_GET_FILESYSTEM_SKELETON = "http://openmetaverse.org/services/filesystem/get_filesystem_skeleton";
        public const string FILESYSTEM_PURGE_FOLDER = "http://openmetaverse.org/services/filesystem/purge_folder";

        public const string MAP = "http://openmetaverse.org/services/map";
        public const string MAP_CREATE_REGION = "http://openmetaverse.org/services/map/create_region";
        public const string MAP_REGION_INFO = "http://openmetaverse.org/services/map/region_info";
        public const string MAP_DELETE_REGION = "http://openmetaverse.org/services/map/delete_region";
        public const string MAP_FETCH_REGION = "http://openmetaverse.org/services/map/fetch_region";
        public const string MAP_FETCH_REGION_DEFAULT = "http://openmetaverse.org/services/map/fetch_region_default";
        public const string MAP_REGION_SEARCH = "http://openmetaverse.org/services/map/region_search";
        public const string MAP_GET_REGION_COUNT = "http://openmetaverse.org/services/map/get_region_count";
        public const string MAP_REGION_UPDATE = "http://openmetaverse.org/services/map/region_update";

        public const string SIMULATOR = "http://openmetaverse.org/services/simulator";
        public const string SIMULATOR_ENABLE_CLIENT = "http://openmetaverse.org/services/simulator/enable_client";
        public const string SIMULATOR_CLOSE_AGENT_CONNECTION = "http://openmetaverse.org/services/simulator/close_agent_connection";
        public const string SIMULATOR_CHILD_AGENT_UPDATE = "http://openmetaverse.org/services/simulator/child_agent_update";
        public const string SIMULATOR_NEIGHBOR_UPDATE = "http://openmetaverse.org/services/simulator/neighbor_update";
    }

    public static class AvatarAttributes
    {
        // axschema.org attributes
        public static readonly Uri EMAIL = new Uri("http://axschema.org/contact/email");
        public static readonly Uri BIRTH_DATE = new Uri("http://axschema.org/birthDate");
        public static readonly Uri LANGUAGE = new Uri("http://axschema.org/pref/language");
        public static readonly Uri TIMEZONE = new Uri("http://axschema.org/pref/timezone");
        public static readonly Uri FIRST_NAME = new Uri("http://axschema.org/namePerson/first");
        public static readonly Uri LAST_NAME = new Uri("http://axschema.org/namePerson/last");
        public static readonly Uri COMPANY = new Uri("http://axschema.org/company/name");
        public static readonly Uri WEBSITE = new Uri("http://axschema.org/contact/web/default");
        public static readonly Uri BIOGRAPHY = new Uri("http://axschema.org/media/biography");

        // OpenSim attributes
        public static readonly Uri AVATAR_ID = new Uri("http://opensimulator.org/attributes/avatarID");
        public static readonly Uri DEFAULT_INVENTORY = new Uri("http://opensimulator.org/attributes/defaultInventory");
        public static readonly Uri LIBRARY_INVENTORY = new Uri("http://opensimulator.org/attributes/libraryInventory");
        public static readonly Uri IMAGE_ID = new Uri("http://opensimulator.org/attributes/imageID");
        public static readonly Uri FIRST_LIFE_IMAGE_ID = new Uri("http://opensimulator.org/attributes/firstLifeImageID");
        public static readonly Uri FIRST_LIFE_BIOGRAPHY = new Uri("http://opensimulator.org/attributes/firstLifeBiography");
        public static readonly Uri CAN_DO = new Uri("http://opensimulator.org/attributes/canDo");
        public static readonly Uri WANT_DO = new Uri("http://opensimulator.org/attributes/wantDo");

        public static readonly Uri HOME_REGION_X = new Uri("http://opensimulator.org/attributes/homeRegionX");
        public static readonly Uri HOME_REGION_Y = new Uri("http://opensimulator.org/attributes/homeRegionY");
        public static readonly Uri HOME_REGION_ID = new Uri("http://opensimulator.org/attributes/homeRegionId");
        public static readonly Uri HOME_POSITION = new Uri("http://opensimulator.org/attributes/homePosition");
        public static readonly Uri HOME_LOOKAT = new Uri("http://opensimulator.org/attributes/homeLookat");

        public static readonly Uri LAST_REGION_X = new Uri("http://opensimulator.org/attributes/lastRegionX");
        public static readonly Uri LAST_REGION_Y = new Uri("http://opensimulator.org/attributes/lastRegionY");
        public static readonly Uri LAST_REGION_ID = new Uri("http://opensimulator.org/attributes/lastRegionID");
        public static readonly Uri LAST_POSITION = new Uri("http://opensimulator.org/attributes/lastPosition");
        public static readonly Uri LAST_LOOKAT = new Uri("http://opensimulator.org/attributes/lastLookAt");

        public static readonly Uri LAST_LOGIN_DATE = new Uri("http://opensimulator.org/attributes/lastLoginDate");
        public static readonly Uri GOD_LEVEL = new Uri("http://opensimulator.org/attributes/godLevel");
        public static readonly Uri PARTNER_ID = new Uri("http://opensimulator.org/attributes/partnerId");
        public static readonly Uri USER_FLAGS = new Uri("http://opensimulator.org/attributes/userFlags");
        public static readonly Uri CUSTOM_TYPE = new Uri("http://opensimulator.org/attributes/customType");

        public static readonly Uri SHAPE_ITEM = new Uri("http://opensimulator.org/attributes/shapeItem");
        public static readonly Uri SKIN_ITEM = new Uri("http://opensimulator.org/attributes/skinItem");
        public static readonly Uri HAIR_ITEM = new Uri("http://opensimulator.org/attributes/hairItem");
        public static readonly Uri EYES_ITEM = new Uri("http://opensimulator.org/attributes/eyesItem");
        public static readonly Uri SHIRT_ITEM = new Uri("http://opensimulator.org/attributes/shirtItem");
        public static readonly Uri PANTS_ITEM = new Uri("http://opensimulator.org/attributes/pantsItem");
        public static readonly Uri SHOES_ITEM = new Uri("http://opensimulator.org/attributes/shoesItem");
        public static readonly Uri SOCKS_ITEM = new Uri("http://opensimulator.org/attributes/socksItem");
        public static readonly Uri JACKET_ITEM = new Uri("http://opensimulator.org/attributes/jacketItem");
        public static readonly Uri GLOVES_ITEM = new Uri("http://opensimulator.org/attributes/glovesItem");
        public static readonly Uri UNDERSHIRT_ITEM = new Uri("http://opensimulator.org/attributes/undershirtItem");
        public static readonly Uri UNDERPANTS_ITEM = new Uri("http://opensimulator.org/attributes/underpantsItem");
        public static readonly Uri SKIRT_ITEM = new Uri("http://opensimulator.org/attributes/skirtItem");
        public static readonly Uri AVATAR_HEIGHT = new Uri("http://opensimulator.org/attributes/avatarHeight");
        public static readonly Uri VISUAL_PARAMS = new Uri("http://opensimulator.org/attributes/visualParams");
        public static readonly Uri TEXTURE_ENTRY = new Uri("http://opensimulator.org/attributes/textureEntry");
    }
}
