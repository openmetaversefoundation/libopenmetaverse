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
        public GroundTextureSettings GroundTextures;

        /// <summary>
        /// Triggered on LandStatReply when the report type is for "top colliders"
        /// </summary>
        /// <param name="objectCount"></param>
        /// <param name="Tasks"></param>
        public delegate void GetTopCollidersReply(int objectCount, List<EstateTask> Tasks);

        /// <summary>
        /// Triggered on LandStatReply when the report type is for "top scripts"
        /// </summary>
        /// <param name="objectCount"></param>
        /// <param name="Tasks"></param>
        public delegate void GetTopScriptsReply(int objectCount, List<EstateTask> Tasks);

        /// <summary>
        /// Triggered when the list of estate managers is received for the current estate
        /// </summary>
        /// <param name="managers"></param>
        /// <param name="count"></param>
        /// <param name="estateID"></param>
        public delegate void EstateManagersReply(uint estateID, int count, List<LLUUID> managers);

        /// <summary>
        /// FIXME - Enumerate all params from EstateOwnerMessage packet
        /// </summary>
        /// <param name="denyNoPaymentInfo"></param>
        /// <param name="estateID"></param>
        /// <param name="estateName"></param>
        /// <param name="estateOwner"></param>
        public delegate void EstateUpdateInfoReply(string estateName, LLUUID estateOwner, uint estateID, bool denyNoPaymentInfo);

        public delegate void EstateManagersListReply(uint estateID, List<LLUUID> managers);

        public delegate void EstateBansReply(uint estateID, int count, List<LLUUID> banned);

        public delegate void EstateUsersReply(uint estateID, int count, List<LLUUID> allowedUsers);

        public delegate void EstateGroupsReply(uint estateID, int count, List<LLUUID> allowedGroups);

        public delegate void EstateCovenantReply(LLUUID covenantID, long timestamp, string estateName, LLUUID estateOwnerID);


        /// <summary>Callback for LandStatReply packets</summary>
        //public event LandStatReply OnLandStatReply;
        /// <summary>Triggered upon a successful .GetTopColliders()</summary>
        public event GetTopCollidersReply OnGetTopColliders;
        /// <summary>Triggered upon a successful .GetTopScripts()</summary>
        public event GetTopScriptsReply OnGetTopScripts;
        /// <summary>Returned, along with other info, upon a successful .GetInfo()</summary>
        public event EstateUpdateInfoReply OnGetEstateUpdateInfo;
        /// <summary>Returned, along with other info, upon a successful .GetInfo()</summary>
        public event EstateManagersReply OnGetEstateManagers;
        /// <summary>Returned, along with other info, upon a successful .GetInfo()</summary>
        public event EstateBansReply OnGetEstateBans;
        /// <summary>Returned, along with other info, upon a successful .GetInfo()</summary>
        public event EstateGroupsReply OnGetAllowedGroups;
        /// <summary>Returned, along with other info, upon a successful .GetInfo()</summary>
        public event EstateUsersReply OnGetAllowedUsers;
        /// <summary>Triggered upon a successful .RequestCovenant()</summary>
        public event EstateCovenantReply OnGetCovenant;

        /// <summary>
        /// Constructor for EstateTools class
        /// </summary>
        /// <param name="client"></param>
		public EstateTools(SecondLife client)
		{
			Client = client;
            Client.Network.RegisterCallback(PacketType.LandStatReply, new NetworkManager.PacketCallback(LandStatReplyHandler));
            Client.Network.RegisterCallback(PacketType.EstateOwnerMessage, new NetworkManager.PacketCallback(EstateOwnerMessageHandler));
            Client.Network.RegisterCallback(PacketType.EstateCovenantReply, new NetworkManager.PacketCallback(EstateCovenantReplyHandler));
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

        /// <summary>Used by EstateOwnerMessage packets</summary>
        public enum EstateAccessDelta
        {
            BanUser = 64,
            UnbanUser = 128
        }

        public enum EstateAccessReplyDelta : uint
        {
            AllowedUsers = 17,
            AllowedGroups = 18,
            EstateBans = 20,
            EstateManagers = 24
        }

        /// <summary>Used by GroundTextureSettings</summary>
        public class GroundTextureRegion
        {
            public LLUUID TextureID;
            public float Low;
            public float High;
        }

        /// <summary>Ground texture settings for each corner of the region</summary>
        public class GroundTextureSettings
        {
            public GroundTextureRegion Southwest;
            public GroundTextureRegion Northwest;
            public GroundTextureRegion Southeast;
            public GroundTextureRegion Northeast;
        }

        /// <summary>
        /// Requests estate information such as top scripts and colliders
        /// </summary>
        /// <param name="parcelLocalID"></param>
        /// <param name="reportType"></param>
        /// <param name="requestFlags"></param>
        /// <param name="filter"></param>
        public void LandStatRequest(int parcelLocalID, LandStatReportType reportType, uint requestFlags, string filter)
        {
            LandStatRequestPacket p = new LandStatRequestPacket();
            p.AgentData.AgentID = Client.Self.AgentID;
            p.AgentData.SessionID = Client.Self.SessionID;
            p.RequestData.Filter = Helpers.StringToField(filter);
            p.RequestData.ParcelLocalID = parcelLocalID;
            p.RequestData.ReportType = (uint)reportType;
            p.RequestData.RequestFlags = requestFlags;            
            Client.Network.SendPacket(p);
        }

        /// <summary>Requests estate settings, including estate manager and access/ban lists</summary>
        public void GetInfo()
        {
            EstateOwnerMessage("getinfo", new List<string>());
        }

        /// <summary>Requests the "Top Scripts" list for the current region</summary>
        public void GetTopScripts()
        {
            //EstateOwnerMessage("scripts", "");
            LandStatRequest(0, LandStatReportType.TopScripts, 0, "");
        }

        /// <summary>Requests the "Top Colliders" list for the current region</summary>
        public void GetTopColliders()
        {
            //EstateOwnerMessage("colliders", "");
            LandStatRequest(0, LandStatReportType.TopColliders, 0, "");
        }

        private void EstateCovenantReplyHandler(Packet packet, Simulator simulator)
        {
            EstateCovenantReplyPacket reply = (EstateCovenantReplyPacket)packet;
            if (OnGetCovenant != null)
            {
                try
                {
                    OnGetCovenant(
                       reply.Data.CovenantID,
                       reply.Data.CovenantTimestamp,
                       Helpers.FieldToUTF8String(reply.Data.EstateName),
                       reply.Data.EstateOwnerID);
                }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary></summary>
        /// <param name="packet"></param>
        /// <param name="simulator"></param>
        private void EstateOwnerMessageHandler(Packet packet, Simulator simulator)
        {
            EstateOwnerMessagePacket message = (EstateOwnerMessagePacket)packet;
            uint estateID;
            string method = Helpers.FieldToUTF8String(message.MethodData.Method);
            List<string> parameters = new List<string>();

            if (method == "estateupdateinfo")
            {
                string estateName = Helpers.FieldToUTF8String(message.ParamList[0].Parameter);
                LLUUID estateOwner = new LLUUID(Helpers.FieldToUTF8String(message.ParamList[1].Parameter));
                estateID = Helpers.BytesToUInt(message.ParamList[2].Parameter);
                /*
                foreach (EstateOwnerMessagePacket.ParamListBlock param in message.ParamList)
                {
                    parameters.Add(Helpers.FieldToUTF8String(param.Parameter));
                }
                */
                bool denyNoPaymentInfo;
                if (Helpers.BytesToUInt(message.ParamList[8].Parameter) == 0) denyNoPaymentInfo = true;
                else denyNoPaymentInfo = false;

                if (OnGetEstateUpdateInfo != null)
                {
                    try
                    {
                        OnGetEstateUpdateInfo(estateName, estateOwner, estateID, denyNoPaymentInfo);
                    }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }

            else if (method == "setaccess")
            {
                int count;
                estateID = Helpers.BytesToUInt(message.ParamList[0].Parameter);
                if (message.ParamList.Length > 1)
                {
                    EstateAccessReplyDelta accessType = (EstateAccessReplyDelta)Helpers.BytesToUInt(message.ParamList[1].Parameter);
                    switch (accessType)
                    {
                        case EstateAccessReplyDelta.EstateManagers:
                            if (OnGetEstateManagers != null)
                            {
                                count = (int)Helpers.BytesToUInt(message.ParamList[3].Parameter);
                                List<LLUUID> managers = new List<LLUUID>();
                                if (message.ParamList.Length > 5)
                                {
                                    for (int i = 5; i < message.ParamList.Length; i++)
                                    {
                                        LLUUID managerID;
                                        if (LLUUID.TryParse(Helpers.FieldToUTF8String(message.ParamList[i].Parameter), out managerID))
                                        {
                                            managers.Add(managerID);
                                        }
                                    }
                                    try { OnGetEstateManagers(estateID, count, managers); }
                                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                                }
                            }
                            break;

                        case EstateAccessReplyDelta.EstateBans:
                            if (OnGetEstateBans != null)
                            {
                                count = (int)Helpers.BytesToUInt(message.ParamList[4].Parameter);
                                List<LLUUID> bannedUsers = new List<LLUUID>();
                                if (message.ParamList.Length > 5)
                                {
                                    for (int i = 5; i < message.ParamList.Length; i++)
                                    {
                                        LLUUID bannedID;
                                        if (LLUUID.TryParse(Helpers.FieldToUTF8String(message.ParamList[i].Parameter), out bannedID))
                                        {
                                            bannedUsers.Add(bannedID);
                                        }
                                    }
                                    try { OnGetEstateBans(estateID, count, bannedUsers); }
                                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                                }
                            }
                            break;                            

                        case EstateAccessReplyDelta.AllowedUsers:
                            if (OnGetAllowedUsers != null)
                            {
                                count = (int)Helpers.BytesToUInt(message.ParamList[2].Parameter);
                                List<LLUUID> allowedUsers = new List<LLUUID>();
                                if (message.ParamList.Length > 5)
                                {
                                    for (int i = 5; i < message.ParamList.Length; i++)
                                    {
                                        LLUUID userID;
                                        if (LLUUID.TryParse(Helpers.FieldToUTF8String(message.ParamList[i].Parameter), out userID))
                                        {
                                            allowedUsers.Add(userID);
                                        }
                                    }
                                    try { OnGetAllowedUsers(estateID, count, allowedUsers); }
                                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                                }
                            }
                            break;

                        case EstateAccessReplyDelta.AllowedGroups:
                            if (OnGetAllowedGroups != null)
                            {
                                count = (int)Helpers.BytesToUInt(message.ParamList[3].Parameter);
                                List<LLUUID> allowedGroups = new List<LLUUID>();
                                if (message.ParamList.Length > 5)
                                {
                                    for (int i = 5; i < message.ParamList.Length; i++)
                                    {
                                        LLUUID groupID;
                                        if (LLUUID.TryParse(Helpers.FieldToUTF8String(message.ParamList[i].Parameter), out groupID))
                                        {
                                            allowedGroups.Add(groupID);
                                        }
                                    }
                                    try { OnGetAllowedGroups(estateID, count, allowedGroups); }
                                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                                }
                            }
                            break;
                    }

                    if (accessType == EstateAccessReplyDelta.EstateManagers)
                    {
                        if (OnGetEstateManagers != null)
                        {
                            count = (int)Helpers.BytesToUInt(message.ParamList[5].Parameter);
                            List<LLUUID> managers = new List<LLUUID>();

                            for (int i = 5; i < message.ParamList.Length; i++)
                            {
                                LLUUID managerID;
                                if (LLUUID.TryParse(Helpers.FieldToUTF8String(message.ParamList[i].Parameter), out managerID))
                                {
                                    managers.Add(managerID);
                                }
                            }
                            try { OnGetEstateManagers(estateID, count, managers); }
                            catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                        }
                    }

                }
            }

            /*
            Console.WriteLine("--- " + method + " ---");
            foreach (EstateOwnerMessagePacket.ParamListBlock block in message.ParamList)
            {
                Console.WriteLine(Helpers.FieldToUTF8String(block.Parameter));
            }
            Console.WriteLine("------");
            */
        }

        /// <summary></summary>
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
                    try { OnGetTopScripts((int)p.RequestData.TotalObjectCount, Tasks); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
                else if (OnGetTopColliders != null && type == LandStatReportType.TopColliders)
                {
                    try { OnGetTopColliders((int)p.RequestData.TotalObjectCount, Tasks); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
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

        public void EstateOwnerMessage(string method, string param)
        {
            List<string> listParams = new List<string>();
            listParams.Add(param);
            EstateOwnerMessage(method, listParams);
        }

        /// <summary>
        /// Used for setting and retrieving various estate panel settings
        /// </summary>
        /// <param name="method">EstateOwnerMessage Method field</param>
        /// <param name="listParams">List of parameters to include</param>
        public void EstateOwnerMessage(string method, List<string>listParams)
        {
            EstateOwnerMessagePacket estate = new EstateOwnerMessagePacket();
            estate.AgentData.AgentID = Client.Self.AgentID;
            estate.AgentData.SessionID = Client.Self.SessionID;
            estate.AgentData.TransactionID = LLUUID.Zero;
            estate.MethodData.Invoice = LLUUID.Random();
            estate.MethodData.Method = Helpers.StringToField(method);
            estate.ParamList = new EstateOwnerMessagePacket.ParamListBlock[listParams.Count];
            for (int i = 0; i < listParams.Count; i++)
            {
                estate.ParamList[i] = new EstateOwnerMessagePacket.ParamListBlock();
                estate.ParamList[i].Parameter = Helpers.StringToField(listParams[i]);
            }
            Client.Network.SendPacket((Packet)estate);
        }

        /// <summary>
        /// Kick an avatar from an estate
        /// </summary>
        /// <param name="userID">Key of Agent to remove</param>
		public void KickUser(LLUUID userID) 
		{
            EstateOwnerMessage("kickestate", userID.ToString());
		}

        /// <summary>Ban an avatar from an estate</summary>
        /// <param name="userID">Key of Agent to remove</param>
        public void BanUser(LLUUID userID)
        {
            List<string> listParams = new List<string>();
            uint flag = (uint)EstateAccessDelta.BanUser;
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(flag.ToString());
            listParams.Add(userID.ToString());
            EstateOwnerMessage("estateaccessdelta", listParams);
        }

        /// <summary>Unban an avatar from an estate</summary>
        /// <param name="userID">Key of Agent to remove</param>
        public void UnbanUser(LLUUID userID)
        {
            List<string> listParams = new List<string>();
            uint flag = (uint)EstateAccessDelta.UnbanUser;
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(flag.ToString());
            listParams.Add(userID.ToString());
            EstateOwnerMessage("estateaccessdelta", listParams);
        }

        /// <summary>
        /// Send a message dialog to everyone in an entire estate
        /// </summary>
        /// <param name="message">Message to send all users in the estate</param>
        public void EstateMessage(string message)
        {
            List<string> listParams = new List<string>();
            listParams.Add(Client.Self.FirstName + " " + Client.Self.LastName);
            listParams.Add(message);
            EstateOwnerMessage("instantmessage", listParams);
        }

        /// <summary>
        /// Send a message dialog to everyone in a simulator
        /// </summary>
        /// <param name="message">Message to send all users in the simulator</param>
        public void SimulatorMessage(string message)
        {
            List<string> listParams = new List<string>();
            listParams.Add("-1");
            listParams.Add("-1");
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(Client.Self.FirstName + " " + Client.Self.LastName);
            listParams.Add(message);
            EstateOwnerMessage("simulatormessage", listParams);
        }

        /// <summary>
        /// Send an avatar back to their home location
        /// </summary>
        /// <param name="pest">Key of avatar to send home</param>
        public void TeleportHomeUser(LLUUID pest)
        {
            List<string> listParams = new List<string>();
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(pest.ToString());
            EstateOwnerMessage("teleporthomeuser", listParams);
        }

        /// <summary>
        /// Begin the region restart process
        /// </summary>
        public void RestartRegion()
        {
            EstateOwnerMessage("restart", "120");
        }

        /// <summary>
        /// Cancels a region restart
        /// </summary>
        public void CancelRestart()
        {
            EstateOwnerMessage("restart", "-1");
        }

        /// <summary>Estate panel "Region" tab settings</summary>
        public void SetRegionInfo(bool blockTerraform, bool blockFly, bool allowDamage, bool allowLandResell, bool restrictPushing, bool allowParcelJoinDivide, float agentLimit, float objectBonus, bool mature)
        {
            List<string> listParams = new List<string>();
            if (blockTerraform) listParams.Add("Y"); else listParams.Add("N");
            if (blockFly) listParams.Add("Y"); else listParams.Add("N");
            if (allowDamage) listParams.Add("Y"); else listParams.Add("N");
            if (allowLandResell) listParams.Add("Y"); else listParams.Add("N");
            listParams.Add(agentLimit.ToString());
            listParams.Add(objectBonus.ToString());
            if (mature) listParams.Add("21"); else listParams.Add("13"); //FIXME - enumerate these settings
            if (restrictPushing) listParams.Add("Y"); else listParams.Add("N");
            if (allowParcelJoinDivide) listParams.Add("Y"); else listParams.Add("N");
            EstateOwnerMessage("setregioninfo", listParams);
        }

        /// <summary>Estate panel "Debug" tab settings</summary>
        public void SetRegionDebug(bool disableScripts, bool disableCollisions, bool disablePhysics)
        {
            List<string> listParams = new List<string>();
            if (disableScripts) listParams.Add("Y"); else listParams.Add("N");
            if (disableCollisions) listParams.Add("Y"); else listParams.Add("N");
            if (disablePhysics) listParams.Add("Y"); else listParams.Add("N");
            EstateOwnerMessage("setregiondebug", listParams);
        }

        public void RequestCovenant()
        {
            EstateCovenantRequestPacket req = new EstateCovenantRequestPacket();
            req.AgentData.AgentID = Client.Self.AgentID;
            req.AgentData.SessionID = Client.Self.SessionID;
            Client.Network.SendPacket(req);
        }

	}

}
