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
using System.Threading;
using libsecondlife;
using libsecondlife.Utilities.Assets;
using libsecondlife.Packets;

namespace libsecondlife.Utilities.Appearance
{
    public enum TextureIndex
    {
        HeadBodypaint = 0,
        UpperShirt,
        LowerPants,
        EyesIris,
        Hair,
        UpperBodypaint,
        LowerBodypaint,
        LowerShoes,
        HeadBaked,
        UpperBaked,
        LowerBaked,
        EyesBaked,
        LowerSocks,
        UpperJacket,
        LowerJacket,
        UpperUndershirt,
        LowerUnderpants,
        Skirt,
        SkirtBaked
    }

    public enum WearableType : byte
    {
        Shape = 0,
        Skin,
        Hair,
        Eyes,
        Shirt,
        Pants,
        Shoes,
        Socks,
        Jacket,
        Gloves,
        Undershirt,
        Underpants,
        Skirt,
        Invalid = 255
    };


    /// <summary>
    /// A single visual characteristic of an avatar mesh, such as eyebrow height
    /// </summary>
    public struct VisualParam
    {
        /// <summary>Index of this visual param</summary>
        public int ParamID;
        /// <summary>Internal name</summary>
        public string Name;
        /// <summary>Group ID this parameter belongs to</summary>
        public int Group;
        /// <summary>Name of the wearable this parameter belongs to</summary>
        public string Wearable;
        /// <summary>Displayable label of this characteristic</summary>
        public string Label;
        /// <summary>Displayable label for the minimum value of this characteristic</summary>
        public string LabelMin;
        /// <summary>Displayable label for the maximum value of this characteristic</summary>
        public string LabelMax;
        /// <summary>Default value</summary>
        public float Default;
        /// <summary>Minimum value</summary>
        public float Min;
        /// <summary>Maximum value</summary>
        public float Max;

        /// <summary>
        /// Set all the values through the constructor
        /// </summary>
        /// <param name="paramID">Index of this visual param</param>
        /// <param name="name">Internal name</param>
        /// <param name="group"></param>
        /// <param name="wearable"></param>
        /// <param name="label">Displayable label of this characteristic</param>
        /// <param name="labelMin">Displayable label for the minimum value of this characteristic</param>
        /// <param name="labelMax">Displayable label for the maximum value of this characteristic</param>
        /// <param name="def">Default value</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        public VisualParam(int paramID, string name, int group, string wearable, string label, string labelMin, string labelMax, float def, float min, float max)
        {
            ParamID = paramID;
            Name = name;
            Group = group;
            Wearable = wearable;
            Label = label;
            LabelMin = labelMin;
            LabelMax = labelMax;
            Default = def;
            Max = max;
            Min = min;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class AppearanceManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wearables">A mapping of WearableTypes to KeyValuePairs
        /// with Asset ID of the wearable as key and Item ID as value</param>
        public delegate void AgentWearablesCallback(Dictionary<WearableType, KeyValuePair<LLUUID, LLUUID>> wearables);


        /// <summary>
        /// 
        /// </summary>
        public event AgentWearablesCallback OnAgentWearables;

        /// <summary>Total number of wearables for each avatar</summary>
        public const int WEARABLE_COUNT = 13;

        /// <summary>Map of what wearables are included in each bake</summary>
        public static readonly WearableType[][] WEARABLE_BAKE_MAP = new WearableType[][]
        {
            // Head
            new WearableType[] { WearableType.Shape, WearableType.Skin,    WearableType.Hair,    WearableType.Invalid, WearableType.Invalid, WearableType.Invalid,    WearableType.Invalid },
            // Upper body
            new WearableType[] { WearableType.Shape, WearableType.Skin,    WearableType.Shirt,   WearableType.Jacket,  WearableType.Gloves,  WearableType.Undershirt, WearableType.Invalid },
            // Lower body
            new WearableType[] { WearableType.Shape, WearableType.Skin,    WearableType.Pants,   WearableType.Shoes,   WearableType.Socks,   WearableType.Jacket,     WearableType.Underpants },
            // Eyes
            new WearableType[] { WearableType.Eyes,  WearableType.Invalid, WearableType.Invalid, WearableType.Invalid, WearableType.Invalid, WearableType.Invalid,    WearableType.Invalid },
            // Skirt
            new WearableType[] { WearableType.Skin,  WearableType.Invalid, WearableType.Invalid, WearableType.Invalid, WearableType.Invalid, WearableType.Invalid,    WearableType.Invalid }
        };

        /// <summary></summary>
        public static readonly LLUUID[] BAKED_TEXTURE_HASH = new LLUUID[]
        {
            new LLUUID("18ded8d6-bcfc-e415-8539-944c0f5ea7a6"),
	        new LLUUID("338c29e3-3024-4dbb-998d-7c04cf4fa88f"),
	        new LLUUID("91b4a2c7-1b1a-ba16-9a16-1f8f8dcc1c3f"),
	        new LLUUID("b2cf28af-b840-1071-3c6a-78085d8128b5"),
	        new LLUUID("ea800387-ea1a-14e0-56cb-24f2022f969a")
        };

        /// <summary>Default avatar texture, used to detect when a custom
        /// texture is not set for a face</summary>
        public static readonly LLUUID DEFAULT_AVATAR = new LLUUID("c228d1cf-4b5d-4ba8-84f4-899a0796aa97");


        private SecondLife Client;
        private AssetManager Assets;
        private int WearablesSerialNum = 0;


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="assets"></param>
        public AppearanceManager(SecondLife client, libsecondlife.Utilities.Assets.AssetManager assets)
        {
            Client = client;
            Assets = assets;

            Client.Network.RegisterCallback(PacketType.AgentWearablesUpdate, new NetworkManager.PacketCallback(AgentWearablesHandler));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool SetCurrentAppearance()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RequestAgentWearables()
        {
            AgentWearablesRequestPacket request = new AgentWearablesRequestPacket();
            request.AgentData.AgentID = Client.Network.AgentID;
            request.AgentData.SessionID = Client.Network.SessionID;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wearables">A mapping of WearableType to ItemIDs, must 
        /// have exactly WEARABLE_COUNT entries</param>
        public void SendAgentWearables(Dictionary<WearableType, LLUUID> wearables)
        {
            if (wearables.Count != WEARABLE_COUNT)
            {
                Client.Log("SendAgentWearables(): wearables must contain " + WEARABLE_COUNT + " IDs", 
                    Helpers.LogLevel.Warning);
                return;
            }

            AgentIsNowWearingPacket wearing = new AgentIsNowWearingPacket();

            wearing.AgentData.AgentID = Client.Network.AgentID;
            wearing.AgentData.SessionID = Client.Network.SessionID;
            wearing.WearableData = new AgentIsNowWearingPacket.WearableDataBlock[WEARABLE_COUNT];

            int i = 0;
            foreach (KeyValuePair<WearableType, LLUUID> pair in wearables)
            {
                wearing.WearableData[i] = new AgentIsNowWearingPacket.WearableDataBlock();
                wearing.WearableData[i].WearableType = (byte)pair.Key;
                wearing.WearableData[i].ItemID = pair.Value;

                i++;
            }

            Client.Network.SendPacket(wearing);
        }

        private void AgentWearablesHandler(Packet packet, Simulator simulator)
        {
            lock (OnAgentWearables)
            {
                if (OnAgentWearables != null)
                {
                    Dictionary<WearableType, KeyValuePair<LLUUID, LLUUID>> wearables = new Dictionary<WearableType, KeyValuePair<LLUUID, LLUUID>>();
                    AgentWearablesUpdatePacket update = (AgentWearablesUpdatePacket)packet;

                    foreach (AgentWearablesUpdatePacket.WearableDataBlock block in update.WearableData)
                    {
                        KeyValuePair<LLUUID, LLUUID> ids = new KeyValuePair<LLUUID, LLUUID>(block.AssetID, block.ItemID);
                        WearableType type = (WearableType)block.WearableType;
                        wearables[type] = ids;
                    }

                    try { OnAgentWearables(wearables); }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }
    }
}
