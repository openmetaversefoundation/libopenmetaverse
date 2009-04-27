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
using System.Collections.Generic;
using System.Net;
using OpenMetaverse.StructuredData;

namespace OpenMetaverse.Messages
{
    public static partial class MessageUtils
    {
        public static IPAddress ToIP(OSD osd)
        {
            byte[] binary = osd.AsBinary();
            if (binary != null && binary.Length == 4)
                return new IPAddress(binary);
            else
                return IPAddress.Any;
        }

        public static OSD FromIP(IPAddress address)
        {
            if (address != null && address != IPAddress.Any)
                return OSD.FromBinary(address.GetAddressBytes());
            else
                return new OSD();
        }

        public static Dictionary<string, string> ToDictionaryString(OSD osd)
        {
            if (osd.Type == OSDType.Map)
            {
                OSDMap map = (OSDMap)osd;
                Dictionary<string, string> dict = new Dictionary<string, string>(map.Count);
                foreach (KeyValuePair<string, OSD> entry in map)
                    dict.Add(entry.Key, entry.Value.AsString());
                return dict;
            }

            return new Dictionary<string, string>(0);
        }

        public static Dictionary<string, Uri> ToDictionaryUri(OSD osd)
        {
            if (osd.Type == OSDType.Map)
            {
                OSDMap map = (OSDMap)osd;
                Dictionary<string, Uri> dict = new Dictionary<string, Uri>(map.Count);
                foreach (KeyValuePair<string, OSD> entry in map)
                    dict.Add(entry.Key, entry.Value.AsUri());
                return dict;
            }

            return new Dictionary<string, Uri>(0);
        }

        public static OSDMap FromDictionaryString(Dictionary<string, string> dict)
        {
            if (dict != null)
            {
                OSDMap map = new OSDMap(dict.Count);
                foreach (KeyValuePair<string, string> entry in dict)
                    map.Add(entry.Key, OSD.FromString(entry.Value));
                return map;
            }

            return new OSDMap(0);
        }

        public static OSDMap FromDictionaryUri(Dictionary<string, Uri> dict)
        {
            if (dict != null)
            {
                OSDMap map = new OSDMap(dict.Count);
                foreach (KeyValuePair<string, Uri> entry in dict)
                    map.Add(entry.Key, OSD.FromUri(entry.Value));
                return map;
            }

            return new OSDMap(0);
        }
    }
}
