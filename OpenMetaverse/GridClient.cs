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

namespace OpenMetaverse
{
    /// <summary>
    /// Main class to expose grid functionality to clients. All of the
    /// classes needed for sending and receiving data are accessible through 
    /// this class.
    /// </summary>
    /// <example>
    /// <code>
    /// // Example minimum code required to instantiate class and 
    /// // connect to a simulator.
    /// using System;
    /// using System.Collections.Generic;
    /// using System.Text;
    /// using OpenMetaverse;
    /// 
    /// namespace FirstBot
    /// {
    ///     class Bot
    ///     {
    ///         public static GridClient Client;
    ///         static void Main(string[] args)
    ///         {
    ///             Client = new GridClient(); // instantiates the GridClient class
    ///                                        // to the global Client object
    ///             // Login to Simulator
    ///             Client.Network.Login("FirstName", "LastName", "Password", "FirstBot", "1.0");
    ///             // Wait for a Keypress
    ///             Console.ReadLine();
    ///             // Logout of simulator
    ///             Client.Network.Logout();
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class GridClient
    {
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
        /// <summary>Estate subsystem</summary>
        public EstateTools Estate;
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
        /// <summary>
        /// Default constructor
        /// </summary>
        public GridClient()
        {
            // These are order-dependant
            LoggerInstance Log = new LoggerInstance();
            Network = new NetworkManager(Log, ;
            Settings = new Settings(this);
            Parcels = new ParcelManager(this);
            Self = new AgentManager(this);
            Avatars = new AvatarManager(this);
            Estate = new EstateTools(this);
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

            //if (Settings.ENABLE_INVENTORY_STORE)
            //    InventoryStore = new Inventory(Inventory);
            //if (Settings.ENABLE_LIBRARY_STORE)
            //    LibraryStore = new Inventory(Inventory);

            //Inventory.OnSkeletonsReceived +=
            //    delegate(InventoryManager manager)
            //    {
            //        if (Settings.ENABLE_INVENTORY_STORE)
            //            InventoryStore.InitializeFromSkeleton(Inventory.InventorySkeleton);
            //        if (Settings.ENABLE_LIBRARY_STORE)
            //            LibraryStore.InitializeFromSkeleton(Inventory.LibrarySkeleton);
            //    };
        }

        /// <summary>
        /// Return the full name of this instance
        /// </summary>
        /// <returns>Client avatars full name</returns>
        public override string ToString()
        {
            return Self.Name;
        }
    }
}
