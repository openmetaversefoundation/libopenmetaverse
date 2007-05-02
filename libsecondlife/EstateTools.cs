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
using System.Collections.Generic;

namespace libsecondlife
{
	/// <summary>
	/// Estate level administration and utilities
	/// </summary>
	public class EstateTools
	{
		private SecondLife Client;

        /// <summary>
        /// Triggered on incoming LandStatReply
        /// </summary>
        /// <param name="reportType"></param>
        /// <param name="requestFlags"></param>
        /// <param name="objectCount"></param>
        /// <param name="task"></param>
        //public delegate void LandStatReply(LandStatReportType reportType, uint requestFlags, int objectCount, List<EstateTask> Tasks);
        /// <summary>
        /// Triggered on incoming LandStatReply when the report type is for "top colliders"
        /// </summary>
        /// <param name="objectCount"></param>
        /// <param name="Tasks"></param>
        public delegate void GetTopCollidersReply(int objectCount, List<EstateTask> Tasks);
        /// <summary>
        /// Triggered on incoming LandStatReply when the report type is for "top scripts"
        /// </summary>
        /// <param name="objectCount"></param>
        /// <param name="Tasks"></param>
        public delegate void GetTopScriptsReply(int objectCount, List<EstateTask> Tasks);

        /// <summary>Callback for incoming LandStatReply packets</summary>
        //public event LandStatReply OnLandStatReply;
        /// <summary>Triggered upon a successful .GetTopColliders()</summary>
        public event GetTopCollidersReply OnGetTopColliders;
        /// <summary>Triggered upon a successful .GetTopScripts()</summary>
        public event GetTopScriptsReply OnGetTopScripts;

        /// <summary>
        /// Constructor for EstateTools class
        /// </summary>
        /// <param name="client"></param>
		public EstateTools(SecondLife client)
		{
			Client = client;
            Client.Network.RegisterCallback(PacketType.LandStatReply, new NetworkManager.PacketCallback(LandStatReplyHandler));
		}

        /// <summary>Describes tasks returned in LandStatReply</summary>
        public class EstateTask
        {
            public LLVector3 Position;
            public float Score;
            public LLUUID TaskID;
            public uint TaskLocalID;
            public string TaskName;
            public string OwnerName;
        }

        /// <summary>Used in the ReportType field of a LandStatRequest</summary>
        public enum LandStatReportType
        {
            TopScripts = 0,
            TopColliders = 1
        }

        /// <summary>
        /// Requests estate information such as top scripts and colliders
        /// </summary>
        /// <param name="parcelLocalID"></param>
        /// <param name="ReportType"></param>
        /// <param name="RequestFlags"></param>
        /// <param name="Filter"></param>
        public void LandStatRequest(int parcelLocalID, LandStatReportType reportType, uint requestFlags, string filter)
        {
            LandStatRequestPacket p = new LandStatRequestPacket();
            p.AgentData.AgentID = Client.Network.AgentID;
            p.AgentData.SessionID = Client.Network.SessionID;
            p.RequestData.Filter = Helpers.StringToField(filter);
            p.RequestData.ParcelLocalID = parcelLocalID;
            p.RequestData.ReportType = (uint)reportType;
            p.RequestData.RequestFlags = requestFlags;            
            Client.Network.SendPacket(p);
        }

        /// <summary>Requests the "Top Scripts" list for the current region</summary>
        public void GetTopScripts()
        {
            LandStatRequest(0, LandStatReportType.TopScripts, 0, "");
        }

        /// <summary>Requests the "Top Colliders" list for the current region</summary>
        public void GetTopColliders()
        {
            LandStatRequest(0, LandStatReportType.TopColliders, 0, "");
        }

        /// <summary>
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void LandStatReplyHandler(Packet packet, Simulator simulator)
        {
            //if (OnLandStatReply != null || OnGetTopScripts != null || OnGetTopColliders != null)
            if (OnGetTopScripts != null || OnGetTopColliders != null)
            {
                LandStatReplyPacket p = (LandStatReplyPacket)packet;
                List<EstateTask> Tasks = new List<EstateTask>();

                foreach (LandStatReplyPacket.ReportDataBlock rep in p.ReportData)
                {
                    EstateTask task = new EstateTask();
                    task.Position = new LLVector3(rep.LocationX, rep.LocationY, rep.LocationZ);
                    task.Score = rep.Score;
                    task.TaskID = rep.TaskID;
                    task.TaskLocalID = rep.TaskLocalID;
                    task.TaskName = Helpers.FieldToUTF8String(rep.TaskName);
                    task.OwnerName = Helpers.FieldToUTF8String(rep.OwnerName);
                    Tasks.Add(task);
                }

                LandStatReportType type = (LandStatReportType)p.RequestData.ReportType;

                if (OnGetTopScripts != null && type == LandStatReportType.TopScripts)
                {
                    OnGetTopScripts((int)p.RequestData.TotalObjectCount, Tasks);
                }
                else if (OnGetTopColliders != null && type == LandStatReportType.TopColliders)
                {
                    OnGetTopColliders((int)p.RequestData.TotalObjectCount, Tasks);
                }

                /*
                if (OnGetTopColliders != null)
                {
                    //FIXME - System.UnhandledExceptionEventArgs
                    OnLandStatReply(
                        type,
                        p.RequestData.RequestFlags,
                        (int)p.RequestData.TotalObjectCount,
                        Tasks
                    );
                }
                */

            }
        }
                

        /// <summary>
        /// Kick an Avatar from an estate
        /// </summary>
        /// <param name="prey">Key of Avatar to kick</param>
		public void KickUser(LLUUID prey) 
		{
            EstateOwnerMessagePacket estate = new EstateOwnerMessagePacket();
            estate.AgentData.AgentID = Client.Network.AgentID;
            estate.AgentData.SessionID = Client.Network.SessionID;
            estate.MethodData.Invoice = LLUUID.Random();
            estate.MethodData.Method = Helpers.StringToField("kick");
            estate.ParamList = new EstateOwnerMessagePacket.ParamListBlock[2];
            estate.ParamList[0].Parameter = Helpers.StringToField(Client.Network.AgentID.ToStringHyphenated());
            estate.ParamList[1].Parameter = Helpers.StringToField(prey.ToStringHyphenated());

            Client.Network.SendPacket((Packet)estate);
		}

        /// <summary>
        /// Ban an Avatar from an estate
        /// </summary>
        /// <param name="prey">Key of Avatar to ban</param>
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
