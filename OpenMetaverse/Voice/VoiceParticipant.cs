/*
 * Copyright (c) 2007-2009, openmetaverse.org
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenMetaverse.Voice
{
    public class VoiceParticipant
    {
        private string Sip;
        private string AvatarName { get; set; }
        private UUID id;

        private bool muted;
        private int volume;
        private VoiceSession session;
        private float energy;

        public float Energy { get { return energy; } }
        private bool speaking;
        public bool IsSpeaking { get { return speaking; } }
        public string URI { get { return Sip; } }
        public UUID ID { get { return id; } }

        public VoiceParticipant(string puri, VoiceSession s)
        {
            id = IDFromName(puri);
            Sip = puri;
            session = s;
        }

        /// <summary>
        /// Extract the avatar UUID encoded in a SIP URI
        /// </summary>
        /// <param name="inName"></param>
        /// <returns></returns>
        public static UUID IDFromName(string inName)
        {
            // The "name" may actually be a SIP URI such as: "sip:xFnPP04IpREWNkuw1cOXlhw==@bhr.vivox.com"
            // If it is, convert to a bare name before doing the transform.
            string name = nameFromsipURI(inName);

            // Doesn't look like a SIP URI, assume it's an actual name.
            if (name == null)
                name = inName;

            // This will only work if the name is of the proper form.
            // As an example, the account name for Monroe Linden (UUID 1673cfd3-8229-4445-8d92-ec3570e5e587) is:
            // "xFnPP04IpREWNkuw1cOXlhw=="

            if ((name.Length == 25) && (name[0] == 'x') && (name[23] == '=') && (name[24] == '='))
            {
                // The name appears to have the right form.

                // Reverse the transforms done by nameFromID
                string temp = name.Replace('-', '+');
                temp = temp.Replace('_', '/');

                byte[] binary = Convert.FromBase64String(temp.Substring(1));
                UUID u = UUID.Zero;
                u.FromBytes(binary, 0);
                return u;
            }

            return UUID.Zero;
        }

        private static string Encode64(string str)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }
        private static byte[] Decode64(string str)
        {
            return Convert.FromBase64String(str);
            //            return System.Text.Encoding.UTF8.GetString(decbuff);
        }

        private static string nameFromsipURI(string uri)
        {
            Regex sip = new Regex("^sip:([^@]*)@.*$");
            Match m = sip.Match(uri);
            if (m.Success)
            {
                GroupCollection g = m.Groups;
                return g[1].Value;
            }

            return null;
        }

        public string Name
        {
            get { return AvatarName; }
            set { AvatarName = value; }
        }

        public bool IsMuted
        {
            get { return muted; }
            set
            {
                muted = value;
                StringBuilder sb = new StringBuilder();
                sb.Append(OpenMetaverse.Voice.VoiceGateway.MakeXML("SessionHandle", session.Handle));
                sb.Append(OpenMetaverse.Voice.VoiceGateway.MakeXML("ParticipantURI", Sip));
                sb.Append(OpenMetaverse.Voice.VoiceGateway.MakeXML("Mute", muted ? "1" : "0"));
                session.Connector.Request("Session.SetParticipantMuteForMe.1", sb.ToString());
            }
        }

        public int Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                StringBuilder sb = new StringBuilder();
                sb.Append(OpenMetaverse.Voice.VoiceGateway.MakeXML("SessionHandle", session.Handle));
                sb.Append(OpenMetaverse.Voice.VoiceGateway.MakeXML("ParticipantURI", Sip));
                sb.Append(OpenMetaverse.Voice.VoiceGateway.MakeXML("Volume", volume.ToString()));
                session.Connector.Request("Session.SetParticipantVolumeForMe.1", sb.ToString());
            }
        }

        internal void SetProperties(bool speak, bool mute, float en)
        {
            speaking = speak;
            muted = mute;
            energy = en;
        }
    }
}

