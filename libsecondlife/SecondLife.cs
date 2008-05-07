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
        /// <param name="message">Text sent to log</param>
        /// <param name="level">The severity of the log entry from <seealso cref="Helpers.LogLevel"/></param>
        public delegate void LogCallback(string message, Helpers.LogLevel level);


        /// <summary>Networking subsystem</summary>
        public NetworkManager Network;
        /// <summary>Settings class including constant values and changeable
        /// parameters for everything</summary>
        public Settings Settings;
        /// <summary>Parcel (subdivided simulator lots) subsystem</summary>
        public ParcelManager Parcels;
        /// <summary>Our own avatars subsystem</summary>
        public AgentManager Self;
        /// <summary>Other avatars subsystem</summary>
        public AvatarManager Avatars;
        /// <summary>Friends list subsystem</summary>
        public FriendsManager Friends;
        /// <summary>Grid (aka simulator group) subsystem</summary>
        public GridManager Grid;
        /// <summary>Object subsystem</summary>
        public ObjectManager Objects;
        /// <summary>Group subsystem</summary>
        public GroupManager Groups;
        /// <summary>Asset subsystem</summary>
        public AssetManager Assets;
        /// <summary>Appearance subsystem</summary>
        public AppearanceManager Appearance;
        /// <summary>Inventory subsystem</summary>
        public InventoryManager Inventory;
        /// <summary>Directory searches including classifieds, people, land 
        /// sales, etc</summary>
        public DirectoryManager Directory;
        /// <summary>Handles land, wind, and cloud heightmaps</summary>
        public TerrainManager Terrain;
        /// <summary>Handles sound-related networking</summary>
        public SoundManager Sound;
        /// <summary>Throttling total bandwidth usage, or allocating bandwidth
        /// for specific data stream types</summary>
        public AgentThrottle Throttle;
        

        /// <summary>Triggered whenever a message is logged. If this is left
        /// null, log messages will go to the console</summary>
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
            Self = new AgentManager(this);
            Avatars = new AvatarManager(this);
            Friends = new FriendsManager(this);
            Grid = new GridManager(this);
            Objects = new ObjectManager(this);
            Groups = new GroupManager(this);
            Assets = new AssetManager(this);
            Appearance = new AppearanceManager(this, Assets);
            Inventory = new InventoryManager(this);
            Directory = new DirectoryManager(this);
            Terrain = new TerrainManager(this);
            Sound = new SoundManager(this);
            Throttle = new AgentThrottle(this);
        }

        /// <summary>
        /// Return the full name of this instance
        /// </summary>
        /// <returns>Client avatars full name</returns>
        public override string ToString()
        {
            return Self.Name;
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

        /// <summary>
        /// Static log function for when Client data is not available
        /// </summary>
        /// <param name="message">Text sent to log</param>
        /// <param name="level">The severity of the log entry from <seealso cref="Helpers.LogLevel"/></param>
        public static void LogStatic(string message, Helpers.LogLevel level)
        {
            Console.WriteLine("{0}: {1}", level.ToString().ToUpper(), message);
        }

        /// <summary>
        /// Static logging function for <seealso cref="T:System.Exception"/> handling
        /// </summary>
        /// <param name="message">Text sent to log</param>
        /// <param name="level">The severity of the log entry from <seealso cref="Helpers.LogLevel"/></param>
        /// <param name="exception">The <seealso cref="T:System.Exception"/> thrown</param>
        public static void LogStatic(string message, Helpers.LogLevel level, Exception exception)
        {
            Console.WriteLine("{0} [libsecondlife]: {1} ({2})", level.ToString().ToUpper(), message, exception);
        }

        /// <summary>
        /// If the library is compiled with DEBUG defined, and SecondLife.Debug
        /// is true, either an event will be fired for the debug message or 
        /// it will be written to the console
        /// </summary>
        /// <param name="message">The debug message</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DebugLogStatic(string message)
        {
            Console.WriteLine("DEBUG [libsecondlife]: {0}", message);
        }
    }
}
