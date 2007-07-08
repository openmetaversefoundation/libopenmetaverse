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
using System.Threading;
using libsecondlife.Packets;
using libsecondlife.AssetSystem;
using libsecondlife.InventorySystem;

namespace libsecondlife
{
    /// <summary>
    /// Main class to expose Second Life functionality to clients. All of the
    /// classes needed for sending and receiving data are accessible through 
    /// this class.
    /// </summary>
    public class SecondLife
    {
        /// <summary>
        /// Callback used for client apps to receive log messages from
        /// libsecondlife
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public delegate void LogCallback(string message, Helpers.LogLevel level);


        /// <summary>Networking Subsystem</summary>
        public NetworkManager Network;
        /// <summary>Parcel (subdivided simulator lots) Subsystem</summary>
        public ParcelManager Parcels;
        /// <summary>'Client's Avatar' Subsystem</summary>
        public MainAvatar Self;
        /// <summary>Other Avatars Subsystem</summary>
        public AvatarManager Avatars;
        /// <summary>Friends List Subsystem</summary>
        public FriendManager Friends;
        /// <summary>Grid (aka simulator group) Subsystem</summary>
        public GridManager Grid;
        /// <summary>Object Subsystem</summary>
        public ObjectManager Objects;
        /// <summary>Group Subsystem</summary>
        public GroupManager Groups;
        /// <summary>Asset Subsystem</summary>
        public libsecondlife.AssetSystem.AssetManager Assets;
        /// <summary>Appearance Subsystem</summary>
        public libsecondlife.AssetSystem.AppearanceManager Appearance;
        /// <summary>Inventory Subsystem</summary>
        public libsecondlife.InventorySystem.InventoryManager Inventory;
        /// <summary>Image Subsystem</summary>
        public ImageManager Images;
        /// <summary>Directory searches including classifieds, people, land 
        /// sales, etc</summary>
        public DirectoryManager Directory;
        /// <summary>Handles land, wind, and cloud heightmaps</summary>
        public TerrainManager Terrain;
        /// <summary>Throttling total bandwidth usage, or allocating bandwidth
        /// for specific data stream types</summary>
        public AgentThrottle Throttle;
        /// <summary>Settings class including constant values and changeable
        /// parameters for everything</summary>
        public Settings Settings;

        /// <summary>Triggered whenever a message is logged.
        /// If this is left null, log messages will go to 
        /// the console</summary>
        public event LogCallback OnLogMessage;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SecondLife()
        {
            // These are order-dependant
            Network = new NetworkManager(this);
            Settings = new Settings(this);
            Parcels = new ParcelManager(this);
            Self = new MainAvatar(this);
            Avatars = new AvatarManager(this);
            Friends = new FriendManager(this);
            Grid = new GridManager(this);
            Objects = new ObjectManager(this);
            Groups = new GroupManager(this);
            Assets = new libsecondlife.AssetSystem.AssetManager(this);
            Appearance = new libsecondlife.AssetSystem.AppearanceManager(this);
            Images = new ImageManager(this);
            Inventory = new InventoryManager(this);
            Directory = new DirectoryManager(this);
            Terrain = new TerrainManager(this);
            Throttle = new AgentThrottle(this);
        }

        /// <summary>
        /// Return the full name of this instance
        /// </summary>
        /// <returns>Client avatars full name</returns>
        public override string ToString()
        {
            return Self.FirstName + " " + Self.LastName;
        }

        /// <summary>
        /// Send a log message to the debugging output system
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">The severity of the log entry</param>
        public void Log(string message, Helpers.LogLevel level)
        {
            if (level == Helpers.LogLevel.Debug && !Settings.DEBUG) return;

            if (OnLogMessage != null)
            {
                try { OnLogMessage(message, level); }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
            else
            {
                if (Settings.LOG_NAMES)
                    Console.WriteLine("{0} [{1} {2}]: {3}", level.ToString().ToUpper(), Self.FirstName, Self.LastName, message);
                else
                    Console.WriteLine("{0}: {1}", level.ToString().ToUpper(), message);
            }
        }

        /// <summary>
        /// If the library is compiled with DEBUG defined, and SecondLife.Debug
        /// is true, either an event will be fired for the debug message or 
        /// it will be written to the console
        /// </summary>
        /// <param name="message">The debug message</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public void DebugLog(string message)
        {
            if (Settings.DEBUG)
            {
                if (OnLogMessage != null)
                {
                    try { OnLogMessage(message, Helpers.LogLevel.Debug); }
                    catch (Exception e) { Console.WriteLine(e.ToString()); }
                }
                else
                {
                    if (Settings.LOG_NAMES)
                        Console.WriteLine("DEBUG [{0} {1}]: {2}", Self.FirstName, Self.LastName, message);
                    else
                        Console.WriteLine("DEBUG: {0}", message);
                }
            }
        }
    }
}
