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

namespace libsecondlife
{
    /// <summary>
    /// Class for controlling various system settings.
    /// </summary>
    public class Settings
    {
        /// <summary>The version of libsecondlife (not the SL protocol itself)</summary>
        public string VERSION = "libsecondlife 0.0.9";
        /// <summary>XML-RPC login server to connect to</summary>
        public string LOGIN_SERVER = "https://login.agni.lindenlab.com/cgi-bin/login.cgi";

        /// <summary>Millisecond interval between ticks, where all ACKs are 
        /// sent out and the age of unACKed packets is checked</summary>
        public readonly int NETWORK_TICK_LENGTH = 500;
        /// <summary>The maximum value of a packet sequence number. After that 
        /// we assume the sequence number just rolls over? Or maybe the 
        /// protocol isn't able to sustain a connection past that</summary>
        public readonly int MAX_SEQUENCE = 0xFFFFFF;
        /// <summary>Number of milliseconds before a teleport attempt will time
        /// out</summary>
        public readonly int TELEPORT_TIMEOUT = 18 * 1000;

        /// <summary>The maximum size of the sequence number inbox, used to
        /// check for resent and/or duplicate packets</summary>
        public int INBOX_SIZE = 100;
        /// <summary>Milliseconds before a packet is assumed lost and resent</summary>
        public int RESEND_TIMEOUT = 4000;
        /// <summary>Milliseconds before the connection to a simulator is 
        /// assumed lost</summary>
        public int SIMULATOR_TIMEOUT = 15000;
        /// <summary>Maximum number of queued ACKs to be sent before SendAcks()
        /// is forced</summary>
        public int MAX_PENDING_ACKS = 10;
        /// <summary>Maximum number of ACKs to append to a packet</summary>
        public int MAX_APPENDED_ACKS = 10;
        /// <summary>Cost of uploading an asset</summary>
        public int UPLOAD_COST { get { return priceUpload; } }

        private SecondLife Client;
        private int priceUpload = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client">Client connection Object to use</param>
        public Settings(SecondLife client)
        {
            Client = client;

            Client.Network.RegisterCallback(Packets.PacketType.EconomyData, new NetworkManager.PacketCallback(EconomyDataHandler));
        }

        /// <summary>
        /// Presumably for outputting asset upload costs.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void EconomyDataHandler(Packet packet, Simulator simulator)
        {
            EconomyDataPacket econ = (EconomyDataPacket)packet;

            priceUpload = econ.Info.PriceUpload;
        }
    }
}
