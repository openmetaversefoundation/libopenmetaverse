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
    public static class AvatarAttributes
    {
        public static readonly Uri EMAIL = new Uri("http://axschema.org/contact/email");
        public static readonly Uri BIRTH_DATE = new Uri("http://axschema.org/birthDate");
        public static readonly Uri LANGUAGE = new Uri("http://axschema.org/pref/language");
        public static readonly Uri TIMEZONE = new Uri("http://axschema.org/pref/timezone");
        public static readonly Uri FIRST_NAME = new Uri("http://axschema.org/namePerson/first");
        public static readonly Uri LAST_NAME = new Uri("http://axschema.org/namePerson/last");
        public static readonly Uri COMPANY = new Uri("http://axschema.org/company/name");
        public static readonly Uri WEBSITE = new Uri("http://axschema.org/contact/web/default");
        public static readonly Uri IMAGE = new Uri("http://axschema.org/media/image/default");
        public static readonly Uri BIOGRAPHY = new Uri("http://axschema.org/media/biography");

        // Service attributes
        public static readonly Uri INVENTORY_SERVER = new Uri("http://openmetaverse.org/attributes/inventoryServer");

        // OpenSim attributes
        public static readonly Uri DEFAULT_INVENTORY = new Uri("http://opensimulator.org/attributes/defaultInventory");
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
        public static readonly Uri LAST_POSITION = new Uri("http://opensimulator.org/attributes/lastPosition");
        public static readonly Uri LAST_LOOKAT = new Uri("http://opensimulator.org/attributes/lastLookat");
        public static readonly Uri LAST_LOGIN_DATE = new Uri("http://opensimulator.org/attributes/lastLoginDate");
        public static readonly Uri PROFILE_FLAGS = new Uri("http://opensimulator.org/attributes/profileFlags");
        public static readonly Uri GOD_LEVEL = new Uri("http://opensimulator.org/attributes/godLevel");
        public static readonly Uri PARTNER_ID = new Uri("http://opensimulator.org/attributes/partnerId");
    }
}
