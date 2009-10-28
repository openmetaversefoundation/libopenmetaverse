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
using OpenMetaverse.Packets;
using System.Collections.Generic;

namespace OpenMetaverse
{
    /// <summary>Describes tasks returned in LandStatReply</summary>
    public class EstateTask
    {
        public Vector3 Position;
        public float Score;
        public UUID TaskID;
        public uint TaskLocalID;
        public string TaskName;
        public string OwnerName;
    }

    /// <summary>
    /// Estate level administration and utilities
    /// </summary>
    public class EstateTools
    {
        private GridClient Client;

        /// <summary>Textures for each of the four terrain height levels</summary>
        public GroundTextureSettings GroundTextures;

        /// <summary>Upper/lower texture boundaries for each corner of the sim</summary>
        public GroundTextureHeightSettings GroundTextureLimits;

        #region Delegates

        /// <summary>
        /// Triggered on LandStatReply when the report type is for "top colliders"
        /// </summary>
        /// <param name="objectCount"></param>
        /// <param name="Tasks"></param>
        public delegate void TopCollidersReplyCallback(int objectCount, Dictionary<UUID, EstateTask> Tasks);

        /// <summary>
        /// Triggered on LandStatReply when the report type is for "top scripts"
        /// </summary>
        /// <param name="objectCount"></param>
        /// <param name="Tasks"></param>
        public delegate void TopScriptsReplyCallback(int objectCount, Dictionary<UUID, EstateTask> Tasks);

        /// <summary>
        /// Triggered when the list of estate managers is received for the current estate
        /// </summary>
        /// <param name="managers"></param>
        /// <param name="count"></param>
        /// <param name="estateID"></param>
        public delegate void EstateManagersReply(uint estateID, int count, List<UUID> managers);

        /// <summary>
        /// FIXME - Enumerate all params from EstateOwnerMessage packet
        /// </summary>
        /// <param name="denyNoPaymentInfo"></param>
        /// <param name="estateID"></param>
        /// <param name="estateName"></param>
        /// <param name="estateOwner"></param>
        public delegate void EstateUpdateInfoReply(string estateName, UUID estateOwner, uint estateID, bool denyNoPaymentInfo);

        public delegate void EstateManagersListReply(uint estateID, List<UUID> managers);

        public delegate void EstateBansReply(uint estateID, int count, List<UUID> banned);

        public delegate void EstateUsersReply(uint estateID, int count, List<UUID> allowedUsers);

        public delegate void EstateGroupsReply(uint estateID, int count, List<UUID> allowedGroups);

        public delegate void EstateCovenantReply(UUID covenantID, long timestamp, string estateName, UUID estateOwnerID);
        #endregion

        #region Events
        // <summary>Callback for LandStatReply packets</summary>
        //public event LandStatReply OnLandStatReply;
        /// <summary>Triggered upon a successful .GetTopColliders()</summary>
        public event TopCollidersReplyCallback OnGetTopColliders;
        /// <summary>Triggered upon a successful .GetTopScripts()</summary>
        public event TopScriptsReplyCallback OnGetTopScripts;
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
        #endregion

        /// <summary>
        /// Constructor for EstateTools class
        /// </summary>
        /// <param name="client"></param>
        public EstateTools(GridClient client)
        {
            GroundTextures = new GroundTextureSettings();
            GroundTextureLimits = new GroundTextureHeightSettings();

            Client = client;
            Client.Network.RegisterCallback(PacketType.LandStatReply, LandStatReplyHandler);
            Client.Network.RegisterCallback(PacketType.EstateOwnerMessage, EstateOwnerMessageHandler);
            Client.Network.RegisterCallback(PacketType.EstateCovenantReply, EstateCovenantReplyHandler);
        }

        #region Enums
        /// <summary>Used in the ReportType field of a LandStatRequest</summary>
        public enum LandStatReportType
        {
            TopScripts = 0,
            TopColliders = 1
        }

        /// <summary>Used by EstateOwnerMessage packets</summary>
        public enum EstateAccessDelta : uint 
        {
            BanUser = 64,
            BanUserAllEstates = 66,
            UnbanUser = 128,
            UnbanUserAllEstates = 130,
            AddManager = 256,
            AddManagerAllEstates = 257,
            RemoveManager = 512,
            RemoveManagerAllEstates = 513,
            AddUserAsAllowed = 4,
            AddAllowedAllEstates = 6,
            RemoveUserAsAllowed = 8,
            RemoveUserAllowedAllEstates = 10,
            AddGroupAsAllowed = 16,
            AddGroupAllowedAllEstates = 18,
            RemoveGroupAsAllowed = 32,
            RemoveGroupAllowedAllEstates = 34
        }

        /// <summary>Used by EstateOwnerMessage packets</summary>
        public enum EstateAccessReplyDelta : uint
        {
            AllowedUsers = 17,
            AllowedGroups = 18,
            EstateBans = 20,
            EstateManagers = 24
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum EstateReturnFlags : uint
        {
            /// <summary>No flags set</summary>
            None = 2,
            /// <summary>Only return targets scripted objects</summary>
            ReturnScripted = 6,
            /// <summary>Only return targets objects if on others land</summary>
            ReturnOnOthersLand = 3,
            /// <summary>Returns target's scripted objects and objects on other parcels</summary>
            ReturnScriptedAndOnOthers = 7
        }
        #endregion
        #region Structs
        /// <summary>Ground texture settings for each corner of the region</summary>
        // TODO: maybe move this class to the Simulator object and implement it there too        
        public struct GroundTextureSettings
        {
            public UUID Low;
            public UUID MidLow;
            public UUID MidHigh;
            public UUID High;
        }

        /// <summary>Used by GroundTextureHeightSettings</summary>
        public struct GroundTextureHeight
        {
            public float Low;
            public float High;
        }

        /// <summary>The high and low texture thresholds for each corner of the sim</summary>
        public struct GroundTextureHeightSettings
        {
            public GroundTextureHeight SW;
            public GroundTextureHeight NW;
            public GroundTextureHeight SE;
            public GroundTextureHeight NE;
        }
        #endregion
        #region Public Methods
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
            p.RequestData.Filter = Utils.StringToBytes(filter);
            p.RequestData.ParcelLocalID = parcelLocalID;
            p.RequestData.ReportType = (uint)reportType;
            p.RequestData.RequestFlags = requestFlags;
            Client.Network.SendPacket(p);
        }

        /// <summary>Requests estate settings, including estate manager and access/ban lists</summary>
        public void RequestInfo()
        {
            EstateOwnerMessage("getinfo", "");
        }

        /// <summary>Requests the "Top Scripts" list for the current region</summary>
        public void RequestTopScripts()
        {
            //EstateOwnerMessage("scripts", "");
            LandStatRequest(0, LandStatReportType.TopScripts, 0, "");
        }

        /// <summary>Requests the "Top Colliders" list for the current region</summary>
        public void RequestTopColliders()
        {
            //EstateOwnerMessage("colliders", "");
            LandStatRequest(0, LandStatReportType.TopColliders, 0, "");
        }

        /// <summary>
        /// Set several estate specific configuration variables
        /// </summary>
        /// <param name="WaterHeight">The Height of the waterlevel over the entire estate. Defaults to 20</param>
        /// <param name="TerrainRaiseLimit">The maximum height change allowed above the baked terrain. Defaults to 4</param>
        /// <param name="TerrainLowerLimit">The minimum height change allowed below the baked terrain. Defaults to -4</param>
        /// <param name="UseEstateSun">true to use</param>
        /// <param name="FixedSun">if True forces the sun position to the position in SunPosition</param>
        /// <param name="SunPosition">The current position of the sun on the estate, or when FixedSun is true the static position
        /// the sun will remain. <remarks>6.0 = Sunrise, 30.0 = Sunset</remarks></param>
        public void SetTerrainVariables(float WaterHeight, float TerrainRaiseLimit,
            float TerrainLowerLimit, bool UseEstateSun, bool FixedSun, float SunPosition)
        {
            List<string> simVariables = new List<string>();
            simVariables.Add(WaterHeight.ToString(Utils.EnUsCulture));
            simVariables.Add(TerrainRaiseLimit.ToString(Utils.EnUsCulture));
            simVariables.Add(TerrainLowerLimit.ToString(Utils.EnUsCulture));
            simVariables.Add(UseEstateSun ? "Y" : "N");
            simVariables.Add(FixedSun ? "Y" : "N");
            simVariables.Add(SunPosition.ToString(Utils.EnUsCulture));
            simVariables.Add("Y"); //Not used?
            simVariables.Add("N"); //Not used?
            simVariables.Add("0.00"); //Also not used?
            EstateOwnerMessage("setregionterrain", simVariables);
        }

        /// <summary>
        /// Request return of objects owned by specified avatar 
        /// </summary>
        /// <param name="Target">The Agents <see cref="UUID"/> owning the primitives to return</param>
        /// <param name="flag">specify the coverage and type of objects to be included in the return</param>
        /// <param name="EstateWide">true to perform return on entire estate</param>
        public void SimWideReturn(UUID Target, EstateReturnFlags flag, bool EstateWide)
        {
            if (EstateWide)
            {
                List<string> param = new List<string>();
                param.Add(flag.ToString());
                param.Add(Target.ToString());
                EstateOwnerMessage("estateobjectreturn", param);
            }
            else
            {
                SimWideDeletesPacket simDelete = new SimWideDeletesPacket();
                simDelete.AgentData.AgentID = Client.Self.AgentID;
                simDelete.AgentData.SessionID = Client.Self.SessionID;
                simDelete.DataBlock.TargetID = Target;
                simDelete.DataBlock.Flags = (uint)flag;
                Client.Network.SendPacket(simDelete);
            }
        }

        /// <summary></summary>
        /// <param name="method"></param>
        /// <param name="param"></param>
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
        public void EstateOwnerMessage(string method, List<string> listParams)
        {
            EstateOwnerMessagePacket estate = new EstateOwnerMessagePacket();
            estate.AgentData.AgentID = Client.Self.AgentID;
            estate.AgentData.SessionID = Client.Self.SessionID;
            estate.AgentData.TransactionID = UUID.Zero;
            estate.MethodData.Invoice = UUID.Random();
            estate.MethodData.Method = Utils.StringToBytes(method);
            estate.ParamList = new EstateOwnerMessagePacket.ParamListBlock[listParams.Count];
            for (int i = 0; i < listParams.Count; i++)
            {
                estate.ParamList[i] = new EstateOwnerMessagePacket.ParamListBlock();
                estate.ParamList[i].Parameter = Utils.StringToBytes(listParams[i]);
            }
            Client.Network.SendPacket((Packet)estate);
        }

        /// <summary>
        /// Kick an avatar from an estate
        /// </summary>
        /// <param name="userID">Key of Agent to remove</param>
        public void KickUser(UUID userID)
        {
            EstateOwnerMessage("kickestate", userID.ToString());
        }

        /// <summary>
        /// Ban an avatar from an estate</summary>
        /// <param name="userID">Key of Agent to remove</param>
        /// <param name="allEstates">Ban user from this estate and all others owned by the estate owner</param>
        public void BanUser(UUID userID, bool allEstates)
        {
            List<string> listParams = new List<string>();
            uint flag = allEstates ? (uint)EstateAccessDelta.BanUserAllEstates : (uint)EstateAccessDelta.BanUser;
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(flag.ToString());
            listParams.Add(userID.ToString());
            EstateOwnerMessage("estateaccessdelta", listParams);
        }

        /// <summary>Unban an avatar from an estate</summary>
        /// <param name="userID">Key of Agent to remove</param>
        ///  /// <param name="allEstates">Unban user from this estate and all others owned by the estate owner</param>
        public void UnbanUser(UUID userID, bool allEstates)
        {
            List<string> listParams = new List<string>();
            uint flag = allEstates ? (uint)EstateAccessDelta.UnbanUserAllEstates : (uint)EstateAccessDelta.UnbanUser;
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
        public void TeleportHomeUser(UUID pest)
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

        /// <summary>Used for setting the region's terrain textures for its four height levels</summary>
        /// <param name="low"></param>
        /// <param name="midLow"></param>
        /// <param name="midHigh"></param>
        /// <param name="high"></param>
        public void SetRegionTerrain(UUID low, UUID midLow, UUID midHigh, UUID high)
        {
            List<string> listParams = new List<string>();
            listParams.Add("0 " + low.ToString());
            listParams.Add("1 " + midLow.ToString());
            listParams.Add("2 " + midHigh.ToString());
            listParams.Add("3 " + high.ToString());
            EstateOwnerMessage("texturedetail", listParams);
            EstateOwnerMessage("texturecommit", "");
        }

        /// <summary>Used for setting sim terrain texture heights</summary> 
        public void SetRegionTerrainHeights(float lowSW, float highSW, float lowNW, float highNW, float lowSE, float highSE, float lowNE, float highNE)
        {
            List<string> listParams = new List<string>();
            listParams.Add("0 " + lowSW.ToString(Utils.EnUsCulture) + " " + highSW.ToString(Utils.EnUsCulture)); //SW low-high 
            listParams.Add("1 " + lowNW.ToString(Utils.EnUsCulture) + " " + highNW.ToString(Utils.EnUsCulture)); //NW low-high 
            listParams.Add("2 " + lowSE.ToString(Utils.EnUsCulture) + " " + highSE.ToString(Utils.EnUsCulture)); //SE low-high 
            listParams.Add("3 " + lowNE.ToString(Utils.EnUsCulture) + " " + highNE.ToString(Utils.EnUsCulture)); //NE low-high 
            EstateOwnerMessage("textureheights", listParams);
            EstateOwnerMessage("texturecommit", "");
        }

        /// <summary>Requests the estate covenant</summary>
        public void RequestCovenant()
        {
            EstateCovenantRequestPacket req = new EstateCovenantRequestPacket();
            req.AgentData.AgentID = Client.Self.AgentID;
            req.AgentData.SessionID = Client.Self.SessionID;
            Client.Network.SendPacket(req);
        }

        /// <summary>
        /// Upload a terrain RAW file
        /// </summary>
        /// <param name="fileData">A byte array containing the encoded terrain data</param>
        /// <param name="fileName">The name of the file being uploaded</param>
        /// <returns>The Id of the transfer request</returns>
        public UUID UploadTerrain(byte[] fileData, string fileName)
        {
            AssetUpload upload = new AssetUpload();
            upload.AssetData = fileData;
            upload.AssetType = AssetType.Unknown;
            upload.Size = fileData.Length;
            upload.ID = UUID.Random();

            // Tell the library we have a pending file to upload
            Client.Assets.SetPendingAssetUploadData(upload);

            // Create and populate a list with commands specific to uploading a raw terrain file
            List<String> paramList = new List<string>();
            paramList.Add("upload filename");
            paramList.Add(fileName);

            // Tell the simulator we have a new raw file to upload
            Client.Estate.EstateOwnerMessage("terrain", paramList);

            return upload.ID;
        }

        /// <summary>
        /// Teleports all users home in current Estate
        /// </summary>
        public void TeleportHomeAllUsers()
        {
            List<string> Params = new List<string>();
            Params.Add(Client.Self.AgentID.ToString());
            EstateOwnerMessage("teleporthomeallusers", Params);
        }

        /// <summary>
        /// Remove estate manager</summary>
        /// <param name="userID">Key of Agent to Remove</param>
        /// <param name="allEstates">removes manager to this estate and all others owned by the estate owner</param>
        public void RemoveEstateManager(UUID userID, bool allEstates)
        {
            List<string> listParams = new List<string>();
            uint flag = allEstates ? (uint)EstateAccessDelta.RemoveManagerAllEstates : (uint)EstateAccessDelta.RemoveManager;
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(flag.ToString());
            listParams.Add(userID.ToString());
            EstateOwnerMessage("estateaccessdelta", listParams);
        }

        /// <summary>
        /// Add estate manager</summary>
        /// <param name="userID">Key of Agent to Add</param>
        /// <param name="allEstates">Add agent as manager to this estate and all others owned by the estate owner</param>
        public void AddEstateManager(UUID userID, bool allEstates)
        {
            List<string> listParams = new List<string>();
            uint flag = allEstates ? (uint)EstateAccessDelta.AddManagerAllEstates : (uint)EstateAccessDelta.AddManager;
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(flag.ToString());
            listParams.Add(userID.ToString());
            EstateOwnerMessage("estateaccessdelta", listParams);
        }

        /// <summary>
        /// Add's an agent to the estate Allowed list</summary>
        /// <param name="userID">Key of Agent to Add</param>
        /// <param name="allEstates">Add agent as an allowed reisdent to All estates if true</param>
        public void AddAllowedUser(UUID userID, bool allEstates)
        {
            List<string> listParams = new List<string>();
            uint flag = allEstates ? (uint)EstateAccessDelta.AddAllowedAllEstates : (uint)EstateAccessDelta.AddUserAsAllowed;
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(flag.ToString());
            listParams.Add(userID.ToString());
            EstateOwnerMessage("estateaccessdelta", listParams);
        }

        /// <summary>
        /// Removes an agent from the estate Allowed list</summary>
        /// <param name="userID">Key of Agent to Remove</param>
        /// <param name="allEstates">Removes agent as an allowed reisdent from All estates if true</param>
        public void RemoveAllowedUser(UUID userID, bool allEstates)
        {
            List<string> listParams = new List<string>();
            uint flag = allEstates ? (uint)EstateAccessDelta.RemoveUserAllowedAllEstates : (uint)EstateAccessDelta.RemoveUserAsAllowed;
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(flag.ToString());
            listParams.Add(userID.ToString());
            EstateOwnerMessage("estateaccessdelta", listParams);
        }
        ///
        /// <summary>
        /// Add's a group to the estate Allowed list</summary>
        /// <param name="groupID">Key of Group to Add</param>
        /// <param name="allEstates">Add Group as an allowed group to All estates if true</param>
        public void AddAllowedGroup(UUID groupID, bool allEstates)
        {
            List<string> listParams = new List<string>();
            uint flag = allEstates ? (uint)EstateAccessDelta.AddGroupAllowedAllEstates : (uint)EstateAccessDelta.AddAllowedAllEstates;
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(flag.ToString());
            listParams.Add(groupID.ToString());
            EstateOwnerMessage("estateaccessdelta", listParams);
        }
        ///
        /// <summary>
        /// Removes a group from the estate Allowed list</summary>
        /// <param name="groupID">Key of Group to Remove</param>
        /// <param name="allEstates">Removes Group as an allowed Group from All estates if true</param>
        public void RemoveAllowedGroup(UUID groupID, bool allEstates)
        {
            List<string> listParams = new List<string>();
            uint flag = allEstates ? (uint)EstateAccessDelta.RemoveGroupAllowedAllEstates : (uint)EstateAccessDelta.RemoveGroupAsAllowed;
            listParams.Add(Client.Self.AgentID.ToString());
            listParams.Add(flag.ToString());
            listParams.Add(groupID.ToString());
            EstateOwnerMessage("estateaccessdelta", listParams);
        }
        #endregion


        #region Packet Handlers

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void EstateCovenantReplyHandler(object sender, PacketReceivedEventArgs e)
        {
            EstateCovenantReplyPacket reply = (EstateCovenantReplyPacket)e.Packet;
            if (OnGetCovenant != null)
            {
                try
                {
                    OnGetCovenant(
                       reply.Data.CovenantID,
                       reply.Data.CovenantTimestamp,
                       Utils.BytesToString(reply.Data.EstateName),
                       reply.Data.EstateOwnerID);
                }
                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void EstateOwnerMessageHandler(object sender, PacketReceivedEventArgs e)
        {
            EstateOwnerMessagePacket message = (EstateOwnerMessagePacket)e.Packet;
            uint estateID;
            string method = Utils.BytesToString(message.MethodData.Method);
            //List<string> parameters = new List<string>();

            if (method == "estateupdateinfo")
            {
                string estateName = Utils.BytesToString(message.ParamList[0].Parameter);
                UUID estateOwner = new UUID(Utils.BytesToString(message.ParamList[1].Parameter));
                estateID = Utils.BytesToUInt(message.ParamList[2].Parameter);
                /*
                foreach (EstateOwnerMessagePacket.ParamListBlock param in message.ParamList)
                {
                    parameters.Add(Utils.BytesToString(param.Parameter));
                }
                */
                bool denyNoPaymentInfo;
                if (Utils.BytesToUInt(message.ParamList[8].Parameter) == 0) denyNoPaymentInfo = true;
                else denyNoPaymentInfo = false;

                if (OnGetEstateUpdateInfo != null)
                {
                    try
                    {
                        OnGetEstateUpdateInfo(estateName, estateOwner, estateID, denyNoPaymentInfo);
                    }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                }
            }

            else if (method == "setaccess")
            {
                int count;
                estateID = Utils.BytesToUInt(message.ParamList[0].Parameter);
                if (message.ParamList.Length > 1)
                {
                    //param comes in as a string for some reason
                    uint param;
                    if (!uint.TryParse(Utils.BytesToString(message.ParamList[1].Parameter), out param)) return;

                    EstateAccessReplyDelta accessType = (EstateAccessReplyDelta)param;

                    switch (accessType)
                    {
                        case EstateAccessReplyDelta.EstateManagers:
                            if (OnGetEstateManagers != null)
                            {
                                if (message.ParamList.Length > 5)
                                {
                                    if (!int.TryParse(Utils.BytesToString(message.ParamList[5].Parameter), out count)) return;
                                    List<UUID> managers = new List<UUID>();
                                    for (int i = 6; i < message.ParamList.Length; i++)
                                    {
                                        try
                                        {
                                            UUID managerID = new UUID(message.ParamList[i].Parameter, 0);
                                            managers.Add(managerID);
                                        }
                                        catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                                    }
                                    try { OnGetEstateManagers(estateID, count, managers); }
                                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                                }
                            }
                            break;

                        case EstateAccessReplyDelta.EstateBans:
                            if (OnGetEstateBans != null)
                            {
                                if (message.ParamList.Length > 6)
                                {
                                    if (!int.TryParse(Utils.BytesToString(message.ParamList[4].Parameter), out count)) return;
                                    List<UUID> bannedUsers = new List<UUID>();
                                    for (int i = 7; i < message.ParamList.Length; i++)
                                    {
                                        try
                                        {
                                            UUID bannedID = new UUID(message.ParamList[i].Parameter, 0);
                                            bannedUsers.Add(bannedID);
                                        }
                                        catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                                    }
                                    try { OnGetEstateBans(estateID, count, bannedUsers); }
                                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                                }
                            }
                            break;

                        case EstateAccessReplyDelta.AllowedUsers:
                            if (OnGetAllowedUsers != null)
                            {
                                if (message.ParamList.Length > 5)
                                {
                                    if (!int.TryParse(Utils.BytesToString(message.ParamList[2].Parameter), out count)) return;
                                    List<UUID> allowedUsers = new List<UUID>();
                                    for (int i = 6; i < message.ParamList.Length; i++)
                                    {
                                        try
                                        {
                                            UUID allowedID = new UUID(message.ParamList[i].Parameter, 0);
                                            allowedUsers.Add(allowedID);
                                        }
                                        catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                                    }
                                    try { OnGetAllowedUsers(estateID, count, allowedUsers); }
                                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                                }
                            }
                            break;

                        case EstateAccessReplyDelta.AllowedGroups:
                            if (OnGetAllowedGroups != null)
                            {
                                if (message.ParamList.Length > 5)
                                {
                                    if (!int.TryParse(Utils.BytesToString(message.ParamList[3].Parameter), out count)) return;
                                    List<UUID> allowedGroups = new List<UUID>();
                                    for (int i = 5; i < message.ParamList.Length; i++)
                                    {
                                        try
                                        {
                                            UUID groupID = new UUID(message.ParamList[i].Parameter, 0);
                                            allowedGroups.Add(groupID);
                                        }
                                        catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                                    }
                                    try { OnGetAllowedGroups(estateID, count, allowedGroups); }
                                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                                }
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>Process an incoming packet and raise the appropriate events</summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The EventArgs object containing the packet data</param>
        protected void LandStatReplyHandler(object sender, PacketReceivedEventArgs e)
        {
            //if (OnLandStatReply != null || OnGetTopScripts != null || OnGetTopColliders != null)
            if (OnGetTopScripts != null || OnGetTopColliders != null)
            {
                LandStatReplyPacket p = (LandStatReplyPacket)e.Packet;
                Dictionary<UUID, EstateTask> Tasks = new Dictionary<UUID, EstateTask>();

                foreach (LandStatReplyPacket.ReportDataBlock rep in p.ReportData)
                {
                    EstateTask task = new EstateTask();
                    task.Position = new Vector3(rep.LocationX, rep.LocationY, rep.LocationZ);
                    task.Score = rep.Score;
                    task.TaskID = rep.TaskID;
                    task.TaskLocalID = rep.TaskLocalID;
                    task.TaskName = Utils.BytesToString(rep.TaskName);
                    task.OwnerName = Utils.BytesToString(rep.OwnerName);
                    Tasks.Add(task.TaskID, task);
                }

                LandStatReportType type = (LandStatReportType)p.RequestData.ReportType;

                if (OnGetTopScripts != null && type == LandStatReportType.TopScripts)
                {
                    try { OnGetTopScripts((int)p.RequestData.TotalObjectCount, Tasks); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                }
                else if (OnGetTopColliders != null && type == LandStatReportType.TopColliders)
                {
                    try { OnGetTopColliders((int)p.RequestData.TotalObjectCount, Tasks); }
                    catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
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
        #endregion
    }
}
