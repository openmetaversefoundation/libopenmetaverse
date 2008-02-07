/*
 * Copyright (c) 2006-2007, Second Life Reverse Engineering Team
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
        public SecondLife Client;

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
            //FIXME
        }

        protected void AttachedSoundGainChangeHandler(Packet packet, Simulator simulator)
        {
            //FIXME
        }

        protected void PreloadSoundHandler(Packet packet, Simulator simulator)
        {
            //FIXME
        }

        protected void SoundTriggerHandler(Packet packet, Simulator simulator)
        {
            //FIXME
        }

        #region Methods

        /// <summary>
        /// Plays a sound in the current region at full volume from avatar position
        /// </summary>
        /// <param name="soundID">UUID of the sound to be played</param>
        public void SoundTrigger(LLUUID soundID)
        {
            SoundTrigger(soundID, Client.Self.SimPosition, 1.0f);
        }

        /// <summary>
        /// Plays a sound in the current region at full volume
        /// </summary>
        /// <param name="soundID">UUID of the sound to be played.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        public void SoundTrigger(LLUUID soundID, LLVector3 position)
        {
            SoundTrigger(soundID, Client.Self.SimPosition, 1.0f);
        }
        /// <summary>
        /// Plays a sound in the current region
        /// </summary>
        /// <param name="soundID">UUID of the sound to be played.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        /// <param name="gain">volume of the sound, from 0.0 to 1.0</param>
        public void SoundTrigger(LLUUID soundID, LLVector3 position, float gain)
        {
            SoundTrigger(soundID, Client.Network.CurrentSim.Handle, position, 1.0f);
        }
        /// <summary>
        /// Plays a sound in the specified sim
        /// </summary>
        /// <param name="soundID">UUID of the sound to be played.</param>
        /// <param name="sim">UUID of the sound to be played.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        /// <param name="gain">volume of the sound, from 0.0 to 1.0</param>
        public void SoundTrigger(LLUUID soundID, Simulator sim, LLVector3 position, float gain)
        {
            SoundTrigger(soundID, sim.Handle, position, 1.0f);
        }
        /// <summary>
        /// Plays a sound
        /// </summary>
        /// <param name="soundID">UUID of the sound to be played.</param>
        /// <param name="handle">handle id for the sim to be played in.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        /// <param name="gain">volume of the sound, from 0.0 to 1.0</param>
        public void SoundTrigger(LLUUID soundID, ulong handle , LLVector3 position, float gain)
        {
            SoundTriggerPacket soundtrigger = new SoundTriggerPacket();
            soundtrigger.SoundData = new SoundTriggerPacket.SoundDataBlock();
            soundtrigger.SoundData.SoundID = soundID;
            soundtrigger.SoundData.ObjectID = LLUUID.Zero;
            soundtrigger.SoundData.OwnerID = LLUUID.Zero;
            soundtrigger.SoundData.ParentID = LLUUID.Zero;
            soundtrigger.SoundData.Handle = handle;
            soundtrigger.SoundData.Position = position;
            soundtrigger.SoundData.Gain = gain;
            Client.Network.SendPacket(soundtrigger);
        }

        #endregion
    }
}
