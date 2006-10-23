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
using libsecondlife.Packets;

namespace libsecondlife
{
	/// <summary>
	/// 
	/// </summary>
	public class EstateTools
	{
		private SecondLife Client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
		public EstateTools(SecondLife client)
		{
			Client = client;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prey"></param>
		public void KickUser(LLUUID prey) 
		{
            EstateOwnerMessagePacket estate = new EstateOwnerMessagePacket();
            estate.AgentData.AgentID = Client.Network.AgentID;
            estate.AgentData.SessionID = Client.Network.SessionID;
            estate.MethodData.Invoice = LLUUID.GenerateUUID();
            estate.MethodData.Method = Helpers.StringToField("kick");
            estate.ParamList = new EstateOwnerMessagePacket.ParamListBlock[2];
            estate.ParamList[0].Parameter = Helpers.StringToField(Client.Network.AgentID.ToStringHyphenated());
            estate.ParamList[1].Parameter = Helpers.StringToField(prey.ToStringHyphenated());

            Client.Network.SendPacket((Packet)estate);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prey"></param>
		public void BanUser(LLUUID prey) 
		{
            // FIXME:
			//Client.Network.SendPacket(Packets.Estate.EstateBan(Client.Protocol,Client.Avatar.ID,Client.Network.SessionID,prey));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prey"></param>
		public void UnBanUser(LLUUID prey) 
		{
            // FIXME:
			//Client.Network.SendPacket(Packets.Estate.EstateUnBan(Client.Protocol,Client.Avatar.ID,Client.Network.SessionID,prey));
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prey"></param>
		public void TeleportHomeUser(LLUUID prey) 
		{
            // FIXME:
			//Client.Network.SendPacket(Packets.Estate.EstateTeleportUser(Client.Protocol,Client.Avatar.ID,Client.Network.SessionID,prey));
		}
	}
}
