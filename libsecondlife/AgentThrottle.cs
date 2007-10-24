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
using libsecondlife.Packets;

namespace libsecondlife
{
    /// <summary>
    /// Throttles the network traffic for various different traffic types.
    /// Access this class through SecondLife.Throttle
    /// </summary>
    public class AgentThrottle
    {
        /// <summary>Maximum bytes per second for resending unacknowledged packets</summary>
        public float Resend
        {
            get { return resend; }
            set
            {
                if (value > 150000.0f) resend = 150000.0f;
                else if (value < 10000.0f) resend = 10000.0f;
                else resend = value;
            }
        }
        /// <summary>Maximum bytes per second for LayerData terrain</summary>
        public float Land
        {
            get { return land; }
            set
            {
                if (value > 170000.0f) land = 170000.0f;
                else if (value < 0.0f) land = 0.0f; // We don't have control of these so allow throttling to 0
                else land = value;
            }
        }
        /// <summary>Maximum bytes per second for LayerData wind data</summary>
        public float Wind
        {
            get { return wind; }
            set
            {
                if (value > 34000.0f) wind = 34000.0f;
                else if (value < 0.0f) wind = 0.0f; // We don't have control of these so allow throttling to 0
                else wind = value;
            }
        }
        /// <summary>Maximum bytes per second for LayerData clouds</summary>
        public float Cloud
        {
            get { return cloud; }
            set
            {
                if (value > 34000.0f) cloud = 34000.0f;
                else if (value < 0.0f) cloud = 0.0f; // We don't have control of these so allow throttling to 0
                else cloud = value;
            }
        }
        /// <summary>Unknown, includes object data</summary>
        public float Task
        {
            get { return task; }
            set
            {
                if (value > 446000.0f) task = 446000.0f;
                else if (value < 4000.0f) task = 4000.0f;
                else task = value;
            }
        }
        /// <summary>Maximum bytes per second for textures</summary>
        public float Texture
        {
            get { return texture; }
            set
            {
                if (value > 446000.0f) texture = 446000.0f;
                else if (value < 4000.0f) texture = 4000.0f;
                else texture = value;
            }
        }
        /// <summary>Maximum bytes per second for downloaded assets</summary>
        public float Asset
        {
            get { return asset; }
            set
            {
                if (value > 220000.0f) asset = 220000.0f;
                else if (value < 10000.0f) asset = 10000.0f;
                else asset = value;
            }
        }

        /// <summary>Maximum bytes per second the entire connection, divided up
        /// between invidiual streams using default multipliers</summary>
        public float Total
        {
            get { return Resend + Land + Wind + Cloud + Task + Texture + Asset; }
            set
            {
                // These sane initial values were pulled from the Second Life client
                Resend = (value * 0.1f);
                Land = (float)(value * 0.52f / 3f);
                Wind = (float)(value * 0.05f);
                Cloud = (float)(value * 0.05f);
                Task = (float)(value * 0.704f / 3f);
                Texture = (float)(value * 0.704f / 3f);
                Asset = (float)(value * 0.484f / 3f);
            }
        }

        private SecondLife Client;
        private float resend;
        private float land;
        private float wind;
        private float cloud;
        private float task;
        private float texture;
        private float asset;

        /// <summary>
        /// Default constructor, uses a default high total of 1500 KBps (1536000)
        /// </summary>
        public AgentThrottle(SecondLife client)
        {
            Client = client;
            Total = 1536000.0f;
        }

        /// <summary>
        /// Constructor that decodes an existing AgentThrottle packet in to
        /// individual values
        /// </summary>
        /// <param name="data">Reference to the throttle data in an AgentThrottle
        /// packet</param>
        /// <param name="pos">Offset position to start reading at in the 
        /// throttle data</param>
        /// <remarks>This is generally not needed in libsecondlife clients as 
        /// the server will never send a throttle packet to the client</remarks>
        public AgentThrottle(byte[] data, int pos)
        {
            int i;
            if (!BitConverter.IsLittleEndian)
                for (i = 0; i < 7; i++)
                    Array.Reverse(data, pos + i * 4, 4);

            Resend = BitConverter.ToSingle(data, pos); pos += 4;
            Land = BitConverter.ToSingle(data, pos); pos += 4;
            Wind = BitConverter.ToSingle(data, pos); pos += 4;
            Cloud = BitConverter.ToSingle(data, pos); pos += 4;
            Task = BitConverter.ToSingle(data, pos); pos += 4;
            Texture = BitConverter.ToSingle(data, pos); pos += 4;
            Asset = BitConverter.ToSingle(data, pos);
        }

        /// <summary>
        /// Send an AgentThrottle packet to the current server using the 
        /// current values
        /// </summary>
        public void Set()
        {
            Set(Client.Network.CurrentSim);
        }

        /// <summary>
        /// Send an AgentThrottle packet to the specified server using the 
        /// current values
        /// </summary>
        public void Set(Simulator simulator)
        {
            AgentThrottlePacket throttle = new AgentThrottlePacket();
            throttle.AgentData.AgentID = Client.Self.AgentID;
            throttle.AgentData.SessionID = Client.Self.SessionID;
            throttle.AgentData.CircuitCode = Client.Network.CircuitCode;
            throttle.Throttle.GenCounter = 0;
            throttle.Throttle.Throttles = this.ToBytes();

            Client.Network.SendPacket(throttle, simulator);
        }

        /// <summary>
        /// Convert the current throttle values to a byte array that can be put
        /// in an AgentThrottle packet
        /// </summary>
        /// <returns>Byte array containing all the throttle values</returns>
        public byte[] ToBytes()
        {
            byte[] data = new byte[7 * 4];
            int i = 0;

            BitConverter.GetBytes(Resend).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Land).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Wind).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Cloud).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Task).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Texture).CopyTo(data, i); i += 4;
            BitConverter.GetBytes(Asset).CopyTo(data, i); i += 4;

            if (!BitConverter.IsLittleEndian)
                for (i = 0; i < 7; i++)
                    Array.Reverse(data, i * 4, 4);

            return data;
        }
    }
}
