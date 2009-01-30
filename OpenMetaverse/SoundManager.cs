/*
 * Copyright (c) 2006-2008, openmetaverse.org
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
using Mono.Simd.Math;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    public class SoundManager
    {
        public readonly GridClient Client;

        public delegate void AttachSoundCallback(Guid soundID, Guid ownerID, Guid objectID, float gain, byte flags);
        public delegate void AttachedSoundGainChangeCallback(Guid objectID, float gain);
        public delegate void SoundTriggerCallback(Guid soundID, Guid ownerID, Guid objectID, Guid parentID, float gain, ulong regionHandle, Vector3f position);
        public delegate void PreloadSoundCallback(Guid soundID, Guid ownerID, Guid objectID);

        public event AttachSoundCallback OnAttachSound;
        public event AttachedSoundGainChangeCallback OnAttachSoundGainChange;
        public event SoundTriggerCallback OnSoundTrigger;
        public event PreloadSoundCallback OnPreloadSound;

        public SoundManager(GridClient client)
        {
            Client = client;

            Client.Network.RegisterCallback(PacketType.AttachedSound, new NetworkManager.PacketCallback(AttachedSoundHandler));
            Client.Network.RegisterCallback(PacketType.AttachedSoundGainChange, new NetworkManager.PacketCallback(AttachedSoundGainChangeHandler));
            Client.Network.RegisterCallback(PacketType.PreloadSound, new NetworkManager.PacketCallback(PreloadSoundHandler));
            Client.Network.RegisterCallback(PacketType.SoundTrigger, new NetworkManager.PacketCallback(SoundTriggerHandler));
        }

        #region public methods

        /// <summary>
        /// Plays a sound in the current region at full volume from avatar position
        /// </summary>
        /// <param name="soundID">Guid of the sound to be played</param>
        public void SoundTrigger(Guid soundID)
        {
            SoundTrigger(soundID, Client.Self.SimPosition, 1.0f);
        }

        /// <summary>
        /// Plays a sound in the current region at full volume
        /// </summary>
        /// <param name="soundID">Guid of the sound to be played.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        public void SoundTrigger(Guid soundID, Vector3f position)
        {
            SoundTrigger(soundID, Client.Self.SimPosition, 1.0f);
        }

        /// <summary>
        /// Plays a sound in the current region
        /// </summary>
        /// <param name="soundID">Guid of the sound to be played.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        /// <param name="gain">volume of the sound, from 0.0 to 1.0</param>
        public void SoundTrigger(Guid soundID, Vector3f position, float gain)
        {
            SoundTrigger(soundID, Client.Network.CurrentSim.Handle, position, 1.0f);
        }
        /// <summary>
        /// Plays a sound in the specified sim
        /// </summary>
        /// <param name="soundID">Guid of the sound to be played.</param>
        /// <param name="sim">Guid of the sound to be played.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        /// <param name="gain">volume of the sound, from 0.0 to 1.0</param>
        public void SoundTrigger(Guid soundID, Simulator sim, Vector3f position, float gain)
        {
            SoundTrigger(soundID, sim.Handle, position, 1.0f);
        }

        /// <summary>
        /// Plays a sound
        /// </summary>
        /// <param name="soundID">Guid of the sound to be played.</param>
        /// <param name="handle">handle id for the sim to be played in.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        /// <param name="gain">volume of the sound, from 0.0 to 1.0</param>
        public void SoundTrigger(Guid soundID, ulong handle, Vector3f position, float gain)
        {
            SoundTriggerPacket soundtrigger = new SoundTriggerPacket();
            soundtrigger.SoundData = new SoundTriggerPacket.SoundDataBlock();
            soundtrigger.SoundData.SoundID = soundID;
            soundtrigger.SoundData.ObjectID = Guid.Empty;
            soundtrigger.SoundData.OwnerID = Guid.Empty;
            soundtrigger.SoundData.ParentID = Guid.Empty;
            soundtrigger.SoundData.Handle = handle;
            soundtrigger.SoundData.Position = position;
            soundtrigger.SoundData.Gain = gain;
            Client.Network.SendPacket(soundtrigger);
        }

        #endregion
        #region Packet Handlers
        protected void AttachedSoundHandler(Packet packet, Simulator simulator)
        {
            AttachedSoundPacket sound = (AttachedSoundPacket)packet;
            if (OnAttachSound != null)
            {
                try { OnAttachSound(sound.DataBlock.SoundID, sound.DataBlock.OwnerID, sound.DataBlock.ObjectID, sound.DataBlock.Gain, sound.DataBlock.Flags); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        protected void AttachedSoundGainChangeHandler(Packet packet, Simulator simulator)
        {
            AttachedSoundGainChangePacket change = (AttachedSoundGainChangePacket)packet;
            if (OnAttachSoundGainChange != null)
            {
                try { OnAttachSoundGainChange(change.DataBlock.ObjectID, change.DataBlock.Gain); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
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
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
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
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }
        #endregion
    }
}
