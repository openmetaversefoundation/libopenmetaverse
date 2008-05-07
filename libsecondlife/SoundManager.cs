/*
 * Copyright (c) 2006-2008, Second Life Reverse Engineering Team
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
using libsecondlife.Packets;

namespace libsecondlife
{
    public class SoundManager
    {
        public readonly SecondLife Client;

        public delegate void AttachSoundCallback(LLUUID soundID, LLUUID ownerID, LLUUID objectID, float gain, byte flags);
        public delegate void AttachedSoundGainChangeCallback(LLUUID objectID, float gain);
        public delegate void SoundTriggerCallback(LLUUID soundID, LLUUID ownerID, LLUUID objectID, LLUUID parentID, float gain, ulong regionHandle, LLVector3 position);
        public delegate void PreloadSoundCallback(LLUUID soundID, LLUUID ownerID, LLUUID objectID);

        public event AttachSoundCallback OnAttachSound;
        public event AttachedSoundGainChangeCallback OnAttachSoundGainChange;
        public event SoundTriggerCallback OnSoundTrigger;
        public event PreloadSoundCallback OnPreloadSound;

        public SoundManager(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.AttachedSound, new NetworkManager.PacketCallback(AttachedSoundHandler));
            Client.Network.RegisterCallback(PacketType.AttachedSoundGainChange, new NetworkManager.PacketCallback(AttachedSoundGainChangeHandler));
            Client.Network.RegisterCallback(PacketType.PreloadSound, new NetworkManager.PacketCallback(PreloadSoundHandler));
            Client.Network.RegisterCallback(PacketType.SoundTrigger, new NetworkManager.PacketCallback(SoundTriggerHandler));
        }

        protected void AttachedSoundHandler(Packet packet, Simulator simulator)
        {
            AttachedSoundPacket sound = (AttachedSoundPacket)packet;
            if (OnAttachSound != null)
            {
                try { OnAttachSound(sound.DataBlock.SoundID, sound.DataBlock.OwnerID, sound.DataBlock.ObjectID, sound.DataBlock.Gain, sound.DataBlock.Flags); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        protected void AttachedSoundGainChangeHandler(Packet packet, Simulator simulator)
        {
            AttachedSoundGainChangePacket change = (AttachedSoundGainChangePacket)packet;
            if (OnAttachSoundGainChange != null)
            {
                try { OnAttachSoundGainChange(change.DataBlock.ObjectID, change.DataBlock.Gain); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }

        protected void PreloadSoundHandler(Packet packet, Simulator simulator)
        {
            PreloadSoundPacket preload = (PreloadSoundPacket)packet;
            if (OnPreloadSound != null)
            {
                foreach (PreloadSoundPacket.DataBlockBlock data in preload.DataBlock)
                {
                    try
                    {
                        OnPreloadSound(data.SoundID, data.OwnerID, data.ObjectID);
                    }
                    catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                }
            }
        }

        protected void SoundTriggerHandler(Packet packet, Simulator simulator)
        {
            SoundTriggerPacket trigger = (SoundTriggerPacket)packet;
            if (OnSoundTrigger != null)
            {
                try
                {
                    OnSoundTrigger(
                        trigger.SoundData.SoundID,
                        trigger.SoundData.OwnerID,
                        trigger.SoundData.ObjectID,
                        trigger.SoundData.ParentID,
                        trigger.SoundData.Gain,
                        trigger.SoundData.Handle,
                        trigger.SoundData.Position
                     );
                }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }
        }
    }
}
