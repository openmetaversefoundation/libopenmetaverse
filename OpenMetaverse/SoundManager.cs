/*
 * Copyright (c) 2006-2009, openmetaverse.org
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
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    /// <summary>
    /// 
    /// </summary>
    public class SoundManager
    {
        #region Private Members
        private readonly GridClient Client;
        #endregion

        #region Event Handling
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AttachedSoundEventArgs> m_AttachedSound;

        ///<summary>Raises the AttachedSound Event</summary>
        /// <param name="e">A AttachedSoundEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAttachedSound(AttachedSoundEventArgs e)
        {
            EventHandler<AttachedSoundEventArgs> handler = m_AttachedSound;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AttachedSoundLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// sound</summary>
        public event EventHandler<AttachedSoundEventArgs> AttachedSound
        {
            add { lock (m_AttachedSoundLock) { m_AttachedSound += value; } }
            remove { lock (m_AttachedSoundLock) { m_AttachedSound -= value; } }
        }
                
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<AttachedSoundGainChangeEventArgs> m_AttachedSoundGainChange;

        ///<summary>Raises the AttachedSoundGainChange Event</summary>
        /// <param name="e">A AttachedSoundGainChangeEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnAttachedSoundGainChange(AttachedSoundGainChangeEventArgs e)
        {
            EventHandler<AttachedSoundGainChangeEventArgs> handler = m_AttachedSoundGainChange;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_AttachedSoundGainChangeLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary></summary>
        public event EventHandler<AttachedSoundGainChangeEventArgs> AttachedSoundGainChange
        {
            add { lock (m_AttachedSoundGainChangeLock) { m_AttachedSoundGainChange += value; } }
            remove { lock (m_AttachedSoundGainChangeLock) { m_AttachedSoundGainChange -= value; } }
        }
        
        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<SoundTriggerEventArgs> m_SoundTrigger;

        ///<summary>Raises the SoundTrigger Event
        /// <param name="e">A SoundTriggerEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnSoundTrigger(SoundTriggerEventArgs e)
        {
            EventHandler<SoundTriggerEventArgs> handler = m_SoundTrigger;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_SoundTriggerLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<SoundTriggerEventArgs> SoundTrigger
        {
            add { lock (m_SoundTriggerLock) { m_SoundTrigger += value; } }
            remove { lock (m_SoundTriggerLock) { m_SoundTrigger -= value; } }
        }

        /// <summary>The event subscribers, null of no subscribers</summary>
        private EventHandler<PreloadSoundEventArgs> m_PreloadSound;

        ///<summary>Raises the PreloadSound Event</summary>
        /// <param name="e">A PreloadSoundEventArgs object containing
        /// the data sent from the simulator</param>
        protected virtual void OnPreloadSound(PreloadSoundEventArgs e)
        {
            EventHandler<PreloadSoundEventArgs> handler = m_PreloadSound;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_PreloadSoundLock = new object();

        /// <summary>Raised when the simulator sends us data containing
        /// ...</summary>
        public event EventHandler<PreloadSoundEventArgs> PreloadSound
        {
            add { lock (m_PreloadSoundLock) { m_PreloadSound += value; } }
            remove { lock (m_PreloadSoundLock) { m_PreloadSound -= value; } }
        }

        #endregion

        /// <summary>
        /// Construct a new instance of the SoundManager class, used for playing and receiving
        /// sound assets
        /// </summary>
        /// <param name="client">A reference to the current GridClient instance</param>
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
        /// <param name="soundID">UUID of the sound to be played</param>
        public void PlaySound(UUID soundID)
        {
            SendSoundTrigger(soundID, Client.Self.SimPosition, 1.0f);
        }

        /// <summary>
        /// Plays a sound in the current region at full volume
        /// </summary>
        /// <param name="soundID">UUID of the sound to be played.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        public void SendSoundTrigger(UUID soundID, Vector3 position)
        {
            SendSoundTrigger(soundID, Client.Self.SimPosition, 1.0f);
        }

        /// <summary>
        /// Plays a sound in the current region
        /// </summary>
        /// <param name="soundID">UUID of the sound to be played.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        /// <param name="gain">volume of the sound, from 0.0 to 1.0</param>
        public void SendSoundTrigger(UUID soundID, Vector3 position, float gain)
        {
            SendSoundTrigger(soundID, Client.Network.CurrentSim.Handle, position, gain);
        }
        /// <summary>
        /// Plays a sound in the specified sim
        /// </summary>
        /// <param name="soundID">UUID of the sound to be played.</param>
        /// <param name="sim">UUID of the sound to be played.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        /// <param name="gain">volume of the sound, from 0.0 to 1.0</param>
        public void SendSoundTrigger(UUID soundID, Simulator sim, Vector3 position, float gain)
        {
            SendSoundTrigger(soundID, sim.Handle, position, gain);
        }

        /// <summary>
        /// Play a sound asset
        /// </summary>
        /// <param name="soundID">UUID of the sound to be played.</param>
        /// <param name="handle">handle id for the sim to be played in.</param>
        /// <param name="position">position for the sound to be played at. Normally the avatar.</param>
        /// <param name="gain">volume of the sound, from 0.0 to 1.0</param>
        public void SendSoundTrigger(UUID soundID, ulong handle, Vector3 position, float gain)
        {
            SoundTriggerPacket soundtrigger = new SoundTriggerPacket();
            soundtrigger.SoundData = new SoundTriggerPacket.SoundDataBlock();
            soundtrigger.SoundData.SoundID = soundID;
            soundtrigger.SoundData.ObjectID = UUID.Zero;
            soundtrigger.SoundData.OwnerID = UUID.Zero;
            soundtrigger.SoundData.ParentID = UUID.Zero;
            soundtrigger.SoundData.Handle = handle;
            soundtrigger.SoundData.Position = position;
            soundtrigger.SoundData.Gain = gain;
            Client.Network.SendPacket(soundtrigger);
        }

        #endregion
        #region Packet Handlers

        /// <summary>Process an incoming <see cref="AttachedSoundPacket"/> packet</summary>
        /// <param name="packet">The <see cref="AttachedSoundPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void AttachedSoundHandler(Packet packet, Simulator simulator)
        {            
            if (m_AttachedSound != null)
            {
                AttachedSoundPacket sound = (AttachedSoundPacket)packet;

                OnAttachedSound(new AttachedSoundEventArgs(sound.DataBlock.SoundID, sound.DataBlock.OwnerID, sound.DataBlock.ObjectID, 
                    sound.DataBlock.Gain, (SoundFlags)sound.DataBlock.Flags));                
            }
        }

        /// <summary>Process an incoming <see cref="AttachedSoundGainChangePacket"/> packet</summary>
        /// <param name="packet">The <see cref="AttachedSoundGainChangePacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void AttachedSoundGainChangeHandler(Packet packet, Simulator simulator)
        {            
            if (m_AttachedSoundGainChange != null)
            {
                AttachedSoundGainChangePacket change = (AttachedSoundGainChangePacket)packet;
                OnAttachedSoundGainChange(new AttachedSoundGainChangeEventArgs(change.DataBlock.ObjectID, change.DataBlock.Gain));                
            }
        }

        /// <summary>Process an incoming <see cref="PreloadSoundPacket"/> packet</summary>
        /// <param name="packet">The <see cref="PreloadSoundPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void PreloadSoundHandler(Packet packet, Simulator simulator)
        {
            
            if (m_PreloadSound != null)
            {
                PreloadSoundPacket preload = (PreloadSoundPacket)packet;

                foreach (PreloadSoundPacket.DataBlockBlock data in preload.DataBlock)
                {
                    OnPreloadSound(new PreloadSoundEventArgs(data.SoundID, data.OwnerID, data.ObjectID));                    
                }
            }
        }

        /// <summary>Process an incoming <see cref="SoundTriggerPacket"/> packet</summary>
        /// <param name="packet">The <see cref="SoundTriggerPacket"/> packet containing the data</param>
        /// <param name="simulator">The simulator the packet originated from</param>
        protected void SoundTriggerHandler(Packet packet, Simulator simulator)
        {            
            if (m_SoundTrigger != null)
            {
                SoundTriggerPacket trigger = (SoundTriggerPacket)packet;
                OnSoundTrigger(new SoundTriggerEventArgs(trigger.SoundData.SoundID,
                        trigger.SoundData.OwnerID,
                        trigger.SoundData.ObjectID,
                        trigger.SoundData.ParentID,
                        trigger.SoundData.Gain,
                        trigger.SoundData.Handle,
                        trigger.SoundData.Position));                
            }            
        }
        
        #endregion
    }
    #region EventArgs

    /// <summary>Provides data for the <see cref="SoundManager.AttachedSound"/> event</summary>
    /// <remarks>The <see cref="SoundManager.AttachedSound"/> event occurs when the simulator sends
    /// the sound data which emits from an agents attachment</remarks>
    /// <example>
    /// The following code example shows the process to subscribe to the <see cref="SoundManager.AttachedSound"/> event
    /// and a stub to handle the data passed from the simulator
    /// <code>
    ///     // Subscribe to the AttachedSound event
    ///     Client.Sound.AttachedSound += Sound_AttachedSound;
    ///     
    ///     // process the data raised in the event here
    ///     private void Sound_AttachedSound(object sender, AttachedSoundEventArgs e)
    ///     {
    ///         // ... Process AttachedSoundEventArgs here ...
    ///     }
    /// </code>
    /// </example>
    public class AttachedSoundEventArgs : EventArgs
    {
        private readonly UUID m_SoundID;
        private readonly UUID m_OwnerID;
        private readonly UUID m_ObjectID;
        private readonly float m_Gain;
        private readonly SoundFlags m_Flags;

        /// <summary>Get the sound asset id</summary>
        public UUID SoundID { get { return m_SoundID; } }
        /// <summary>Get the ID of the owner</summary>
        public UUID OwnerID { get { return m_OwnerID; } }
        /// <summary>Get the ID of the Object</summary>
        public UUID ObjectID { get { return m_ObjectID; } }
        /// <summary>Get the volume level</summary>
        public float Gain { get { return m_Gain; } }
        /// <summary>Get the <see cref="SoundFlags"/></summary>
        public SoundFlags Flags { get { return m_Flags; } }

        /// <summary>
        /// Construct a new instance of the SoundTriggerEventArgs class
        /// </summary>
        /// <param name="soundID">The sound asset id</param>
        /// <param name="ownerID">The ID of the owner</param>
        /// <param name="objectID">The ID of the object</param>
        /// <param name="gain">The volume level</param>
        /// <param name="flags">The <see cref="SoundFlags"/></param>
        public AttachedSoundEventArgs(UUID soundID, UUID ownerID, UUID objectID, float gain, SoundFlags flags)
        {
            this.m_SoundID = soundID;
            this.m_OwnerID = ownerID;
            this.m_ObjectID = objectID;
            this.m_Gain = gain;
            this.m_Flags = flags;
        }
    }

    /// <summary>Provides data for the <see cref="SoundManager.AttachedSoundGainChange"/> event</summary>
    /// <remarks>The <see cref="SoundManager.AttachedSoundGainChange"/> event occurs when an attached sound
    /// changes its volume level</remarks>
    public class AttachedSoundGainChangeEventArgs : EventArgs
    {
        private readonly UUID m_ObjectID;
        private readonly float m_Gain;

        /// <summary>Get the ID of the Object</summary>
        public UUID ObjectID { get { return m_ObjectID; } }
        /// <summary>Get the volume level</summary>
        public float Gain { get { return m_Gain; } }

        /// <summary>
        /// Construct a new instance of the AttachedSoundGainChangedEventArgs class
        /// </summary>
        /// <param name="objectID">The ID of the Object</param>
        /// <param name="gain">The new volume level</param>
        public AttachedSoundGainChangeEventArgs(UUID objectID, float gain)
        {
            this.m_ObjectID = objectID;
            this.m_Gain = gain;
        }
    }

    /// <summary>Provides data for the <see cref="SoundManager.SoundTrigger"/> event</summary>
    /// <remarks><para>The <see cref="SoundManager.SoundTrigger"/> event occurs when the simulator forwards
    /// a request made by yourself or another agent to play either an asset sound or a built in sound</para>
    /// 
    /// <para>Requests to play sounds where the <see cref="SoundTriggerEventArgs.SoundID"/> is not one of the built-in
    /// <see cref="Sounds"/> will require sending a request to download the sound asset before it can be played</para>
    /// </remarks>
    /// <example>
    /// The following code example uses the <see cref="SoundTriggerEventArgs.OwnerID"/>, <see cref="SoundTriggerEventArgs.SoundID"/> 
    /// and <see cref="SoundTriggerEventArgs.Gain"/>
    /// properties to display some information on a sound request on the <see cref="Console"/> window.
    /// <code>
    ///     // subscribe to the event
    ///     Client.Sound.SoundTrigger += Sound_SoundTrigger;
    ///
    ///     // play the pre-defined BELL_TING sound
    ///     Client.Sound.SendSoundTrigger(Sounds.BELL_TING);
    ///     
    ///     // handle the response data
    ///     private void Sound_SoundTrigger(object sender, SoundTriggerEventArgs e)
    ///     {
    ///         Console.WriteLine("{0} played the sound {1} at volume {2}",
    ///             e.OwnerID, e.SoundID, e.Gain);
    ///     }    
    /// </code>
    /// </example>
    public class SoundTriggerEventArgs : EventArgs
    {
        private readonly UUID m_SoundID;
        private readonly UUID m_OwnerID;
        private readonly UUID m_ObjectID;
        private readonly UUID m_ParentID;
        private readonly float m_Gain;
        private readonly ulong m_RegionHandle;
        private readonly Vector3 m_Position;

        /// <summary>Get the sound asset id</summary>
        public UUID SoundID { get { return m_SoundID; } }
        /// <summary>Get the ID of the owner</summary>
        public UUID OwnerID { get { return m_OwnerID; } }
        /// <summary>Get the ID of the Object</summary>
        public UUID ObjectID { get { return m_ObjectID; } }
        /// <summary>Get the ID of the objects parent</summary>
        public UUID ParentID { get { return m_ParentID; } }
        /// <summary>Get the volume level</summary>
        public float Gain { get { return m_Gain; } }
        /// <summary>Get the regionhandle</summary>
        public ulong RegionHandle { get { return m_RegionHandle; } }
        /// <summary>Get the source position</summary>
        public Vector3 Position { get { return m_Position; } }

        /// <summary>
        /// Construct a new instance of the SoundTriggerEventArgs class
        /// </summary>
        /// <param name="soundID">The sound asset id</param>
        /// <param name="ownerID">The ID of the owner</param>
        /// <param name="objectID">The ID of the object</param>
        /// <param name="parentID">The ID of the objects parent</param>
        /// <param name="gain">The volume level</param>
        /// <param name="regionHandle">The regionhandle</param>
        /// <param name="position">The source position</param>
        public SoundTriggerEventArgs(UUID soundID, UUID ownerID, UUID objectID, UUID parentID, float gain, ulong regionHandle, Vector3 position)
        {
            this.m_SoundID = soundID;
            this.m_OwnerID = ownerID;
            this.m_ObjectID = objectID;
            this.m_ParentID = parentID;
            this.m_Gain = gain;
            this.m_RegionHandle = regionHandle;
            this.m_Position = position;
        }
    }

    /// <summary>Provides data for the <see cref="AvatarManager.AvatarAppearance"/> event</summary>
    /// <remarks>The <see cref="AvatarManager.AvatarAppearance"/> event occurs when the simulator sends
    /// the appearance data for an avatar</remarks>
    /// <example>
    /// The following code example uses the <see cref="AvatarAppearanceEventArgs.AvatarID"/> and <see cref="AvatarAppearanceEventArgs.VisualParams"/>
    /// properties to display the selected shape of an avatar on the <see cref="Console"/> window.
    /// <code>
    ///     // subscribe to the event
    ///     Client.Avatars.AvatarAppearance += Avatars_AvatarAppearance;
    /// 
    ///     // handle the data when the event is raised
    ///     void Avatars_AvatarAppearance(object sender, AvatarAppearanceEventArgs e)
    ///     {
    ///         Console.WriteLine("The Agent {0} is using a {1} shape.", e.AvatarID, (e.VisualParams[31] &gt; 0) : "male" ? "female")
    ///     }
    /// </code>
    /// </example>
    public class PreloadSoundEventArgs : EventArgs
    {
        private readonly UUID m_SoundID;
        private readonly UUID m_OwnerID;
        private readonly UUID m_ObjectID;

        /// <summary>Get the sound asset id</summary>
        public UUID SoundID { get { return m_SoundID; } }
        /// <summary>Get the ID of the owner</summary>
        public UUID OwnerID { get { return m_OwnerID; } }
        /// <summary>Get the ID of the Object</summary>
        public UUID ObjectID { get { return m_ObjectID; } }

        /// <summary>
        /// Construct a new instance of the PreloadSoundEventArgs class
        /// </summary>
        /// <param name="soundID">The sound asset id</param>
        /// <param name="ownerID">The ID of the owner</param>
        /// <param name="objectID">The ID of the object</param>
        public PreloadSoundEventArgs(UUID soundID, UUID ownerID, UUID objectID)
        {
            this.m_SoundID = soundID;
            this.m_OwnerID = ownerID;
            this.m_ObjectID = objectID;
        }
    }
    #endregion
}