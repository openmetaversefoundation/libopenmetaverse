/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
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
using libsecondlife.AssetSystem;

namespace libsecondlife
{
    /// <summary>
    /// Callback used for client apps to receive log messages from
    /// libsecondlife
    /// </summary>
    /// <param name="message"></param>
    /// <param name="level"></param>
    public delegate void LogCallback(string message, Helpers.LogLevel level);

    /// <summary>
    /// Main class to expose Second Life functionality to clients. All of the
    /// classes needed for sending and receiving data are accessible through 
    /// this class.
    /// </summary>
    public class SecondLife
    {
        public const string LOGIN_SERVER = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";

        /// <summary>Networking Subsystem</summary>
        public NetworkManager Network;
        /// <summary>Parcel (subdivided simulator lots) Subsystem</summary>
        public ParcelManager Parcels;
        /// <summary>'Client's Avatar' Subsystem</summary>
        public MainAvatar Self;
        /// <summary>Other Avatars Subsystem</summary>
        public AvatarManager Avatars;
        /// <summary>Grid (aka simulator group) Subsystem</summary>
        public GridManager Grid;
        /// <summary>Object Subsystem</summary>
        public ObjectManager Objects;
        /// <summary>Group Subsystem</summary>
        public GroupManager Groups;

        /// <summary>Image Subsystem</summary>
        private ImageManager _ImageManager;
        public ImageManager Images
        {
            get
            {
                if (_ImageManager == null)
                {
                    _ImageManager = new ImageManager(this);
                    return _ImageManager;
                }
                else
                {
                    return _ImageManager;
                }
            }

            set
            {
                _ImageManager = value;
            }
        }

        /// <summary></summary>
        public event LogCallback OnLogMessage;
        /// <summary>Debug flag</summary>
        public bool Debug;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SecondLife()
        {
            Network = new NetworkManager(this);
            Parcels = new ParcelManager(this);
            Self = new MainAvatar(this);
            Avatars = new AvatarManager(this);
            Grid = new GridManager(this);
            Objects = new ObjectManager(this);
            Groups = new GroupManager(this);
            Debug = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Client Avatar's Full Name</returns>
        public override string ToString()
        {
            return Self.FirstName + " " + Self.LastName;
        }

        /// <summary>
        /// A simple sleep function that will allow pending threads to run
        /// </summary>
        public void Tick()
        {
            System.Threading.Thread.Sleep(0);
        }

        /// <summary>
        /// Send a log message to the debugging output system
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">From the LogLevel enum, either Info, Warning, or Error</param>
        public void Log(string message, Helpers.LogLevel level)
        {
            if (OnLogMessage != null)
            {
                OnLogMessage(message, level);
            }
            else
            {
                Console.WriteLine(level.ToString().ToUpper() + ": " + message);
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
            if (Debug)
            {
                if (OnLogMessage != null)
                {
                    OnLogMessage(message, Helpers.LogLevel.Debug);
                }
                else
                {
                    Console.WriteLine("DEBUG: " + message);
                }
            }
        }
    }
}
