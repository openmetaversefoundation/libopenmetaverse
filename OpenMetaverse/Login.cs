/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using Nwc.XmlRpc;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Http;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{
    #region Enums

    /// <summary>
    /// 
    /// </summary>
    public enum LoginStatus
    {
        /// <summary></summary>
        Failed = -1,
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        ConnectingToLogin,
        /// <summary></summary>
        ReadingResponse,
        /// <summary></summary>
        ConnectingToSim,
        /// <summary></summary>
        Redirecting,
        /// <summary></summary>
        Success
    }

    #endregion Enums

    #region Structs

    /// <summary>
    /// Login Request Parameters
    /// </summary>
    public struct LoginParams
    {
        /// <summary>The URL of the Login Server</summary>
        public string URI;
        /// <summary>The number of milliseconds to wait before a login is considered
        /// failed due to timeout</summary>
        public int Timeout;
        /// <summary>The request method</summary>
        /// <remarks>login_to_server is currently the only supported method</remarks>
        public string MethodName;
        /// <summary>The Agents First name</summary>
        public string FirstName;
        /// <summary>The Agents Last name</summary>
        public string LastName;
        /// <summary>A md5 hashed password</summary>
        /// <remarks>plaintext password will be automatically hashed</remarks>
        public string Password;
        /// <summary>The agents starting location once logged in</summary>
        /// <remarks>Either "last", "home", or a string encoded URI 
        /// containing the simulator name and x/y/z coordinates e.g: uri:hooper&128&152&17</remarks>
        /// <seealso cref="M:StartLocation"/>
        public string Start;
        /// <summary>A string containing the client software channel information</summary>
        /// <example>Second Life Release</example>
        public string Channel;
        /// <summary>The client software version information</summary>
        /// <remarks>The official viewer uses: Second Life Release n.n.n.n 
        /// where n is replaced with the current version of the viewer</remarks>
        public string Version;
        /// <summary>A string containing the platform information the agent is running on</summary>
        public string Platform;
        /// <summary>A string hash of the network cards Mac Address</summary>
        public string MAC;
        /// <summary>Unknown or deprecated</summary>
        public string ViewerDigest;
        /// <summary>A string hash of the first disk drives ID used to identify this clients uniqueness</summary>
        public string ID0;
        /// <summary>A string containing the viewers Software, this is not directly sent to the login server but 
        /// instead is used to generate the Version string</summary>
        public string UserAgent;
        /// <summary>A string representing the software creator. This is not directly sent to the login server but
        /// is used by the library to generate the Version information</summary>
        public string Author;
        /// <summary>If true, this agent agrees to the Terms of Service of the grid its connecting to</summary>
        public string AgreeToTos;
        /// <summary>Unknown</summary>
        public string ReadCritical;
        /// <summary>An array of string sent to the login server to enable various options</summary>
        public string[] Options;
    }

    public struct BuddyListEntry
    {
        public int buddy_rights_given;
        public string buddy_id;
        public int buddy_rights_has;
    }

    /// <summary>
    /// The decoded data returned from the login server after a successful login
    /// </summary>
    public struct LoginResponseData
    {
        /// <summary>true, false, indeterminate</summary>
        //[XmlRpcMember("login")]
        public string Login;
        public bool Success;
        public string Reason;
        /// <summary>Login message of the day</summary>
        public string Message;
        public UUID AgentID;
        public UUID SessionID;
        public UUID SecureSessionID;
        public string FirstName;
        public string LastName;
        public string StartLocation;
        /// <summary>M or PG, also agent_region_access and agent_access_max</summary>
        public string AgentAccess;
        public Vector3 LookAt;
        public ulong HomeRegion;
        public Vector3 HomePosition;
        public Vector3 HomeLookAt;
        public int CircuitCode;
        public int RegionX;
        public int RegionY;
        public int SimPort;
        public IPAddress SimIP;
        public string SeedCapability;
        public BuddyListEntry[] BuddyList;
        public int SecondsSinceEpoch;
        public string UDPBlacklist;

        #region Inventory
        
        public UUID InventoryRoot;
        public UUID LibraryRoot;
        public InventoryFolder[] InventorySkeleton;
        public InventoryFolder[] LibrarySkeleton;
        public UUID LibraryOwner;

        #endregion

        #region Redirection

        public string NextMethod;
        public string NextUrl;
        public string[] NextOptions;
        public int NextDuration;

        #endregion

        // These aren't currently being utilized by the library
        public string AgentAccessMax;
        public string AgentRegionAccess;
        public int AOTransition;
        public string InventoryHost;

        /// <summary>
        /// Parse LLSD Login Reply Data
        /// </summary>
        /// <param name="reply">An <seealso cref="OSDMap"/> 
        /// contaning the login response data</param>
        /// <remarks>XML-RPC logins do not require this as XML-RPC.NET 
        /// automatically populates the struct properly using attributes</remarks>
        public void Parse(OSDMap reply)
        {
            try
            {
                AgentID = ParseUUID("agent_id", reply);
                SessionID = ParseUUID("session_id", reply);
                SecureSessionID = ParseUUID("secure_session_id", reply);
                FirstName = ParseString("first_name", reply).Trim('"');
                LastName = ParseString("last_name", reply).Trim('"');
                StartLocation = ParseString("start_location", reply);
                AgentAccess = ParseString("agent_access", reply);
                LookAt = ParseVector3("look_at", reply);
                Reason = ParseString("reason", reply);
                Message = ParseString("message", reply);

                Login = reply["login"].AsString();
                Success = reply["login"].AsBoolean();
            }
            catch (OSDException e)
            {
                Logger.Log("Login server returned (some) invalid data: " + e.Message, Helpers.LogLevel.Warning);
            }

            // Home
            OSDMap home = null;
            OSD osdHome = OSDParser.DeserializeLLSDNotation(reply["home"].AsString());

            if (osdHome.Type == OSDType.Map)
            {
                home = (OSDMap)osdHome;

                OSD homeRegion;
                if (home.TryGetValue("region_handle", out homeRegion) && homeRegion.Type == OSDType.Array)
                {
                    OSDArray homeArray = (OSDArray)homeRegion;
                    if (homeArray.Count == 2)
                        HomeRegion = Utils.UIntsToLong((uint)homeArray[0].AsInteger(), (uint)homeArray[1].AsInteger());
                    else
                        HomeRegion = 0;
                }

                HomePosition = ParseVector3("position", home);
                HomeLookAt = ParseVector3("look_at", home);
            }
            else
            {
                HomeRegion = 0;
                HomePosition = Vector3.Zero;
                HomeLookAt = Vector3.Zero;
            }

            CircuitCode = (int)ParseUInt("circuit_code", reply);
            RegionX = (int)ParseUInt("region_x", reply);
            RegionY = (int)ParseUInt("region_y", reply);
            SimPort = (short)ParseUInt("sim_port", reply);
            string simIP = ParseString("sim_ip", reply);
            IPAddress.TryParse(simIP, out SimIP);
            SeedCapability = ParseString("seed_capability", reply);

            // Buddy list
            OSD buddyLLSD;
            if (reply.TryGetValue("buddy-list", out buddyLLSD) && buddyLLSD.Type == OSDType.Array)
            {
                List<BuddyListEntry> buddys = new List<BuddyListEntry>();
                OSDArray buddyArray = (OSDArray)buddyLLSD;
                for (int i = 0; i < buddyArray.Count; i++)
                {
                    if (buddyArray[i].Type == OSDType.Map)
                    {
                        BuddyListEntry bud = new BuddyListEntry();
                        OSDMap buddy = (OSDMap)buddyArray[i];

                        bud.buddy_id = buddy["buddy_id"].AsString();
                        bud.buddy_rights_given = (int)ParseUInt("buddy_rights_given", buddy);
                        bud.buddy_rights_has = (int)ParseUInt("buddy_rights_has", buddy);

                        buddys.Add(bud);
                    }
                    BuddyList = buddys.ToArray();
                }
            }

            SecondsSinceEpoch = (int)ParseUInt("seconds_since_epoch", reply);
            
            InventoryRoot = ParseMappedUUID("inventory-root", "folder_id", reply);
            InventorySkeleton = ParseInventorySkeleton("inventory-skeleton", reply);
            
            LibraryOwner = ParseMappedUUID("inventory-lib-owner", "agent_id", reply);
            LibraryRoot = ParseMappedUUID("inventory-lib-root", "folder_id", reply);
            LibrarySkeleton = ParseInventorySkeleton("inventory-skel-lib", reply);
        }

        public void Parse(Hashtable reply)
        {
            try
            {
                AgentID = ParseUUID("agent_id", reply);
                SessionID = ParseUUID("session_id", reply);
                SecureSessionID = ParseUUID("secure_session_id", reply);
                FirstName = ParseString("first_name", reply).Trim('"');
                LastName = ParseString("last_name", reply).Trim('"');
                StartLocation = ParseString("start_location", reply);
                AgentAccess = ParseString("agent_access", reply);
                LookAt = ParseVector3("look_at", reply);
                Reason = ParseString("reason", reply);
                Message = ParseString("message", reply);

                if (reply.ContainsKey("login"))
                {
                    Login = (string)reply["login"];
                    Success = Login == "true";
                }
            }
            catch (Exception e)
            {
                Logger.Log("Login server returned (some) invalid data: " + e.Message, Helpers.LogLevel.Warning);
            }
            if (!Success)
                return;

            // Home
            OSDMap home = null;
            OSD osdHome = OSDParser.DeserializeLLSDNotation(reply["home"].ToString());

            if (osdHome.Type == OSDType.Map)
            {
                home = (OSDMap)osdHome;

                OSD homeRegion;
                if (home.TryGetValue("region_handle", out homeRegion) && homeRegion.Type == OSDType.Array)
                {
                    OSDArray homeArray = (OSDArray)homeRegion;
                    if (homeArray.Count == 2)
                        HomeRegion = Utils.UIntsToLong((uint)homeArray[0].AsInteger(), (uint)homeArray[1].AsInteger());
                    else
                        HomeRegion = 0;
                }

                HomePosition = ParseVector3("position", home);
                HomeLookAt = ParseVector3("look_at", home);
            }
            else
            {
                HomeRegion = 0;
                HomePosition = Vector3.Zero;
                HomeLookAt = Vector3.Zero;
            }

            CircuitCode = (int)ParseUInt("circuit_code", reply);
            RegionX = (int)ParseUInt("region_x", reply);
            RegionY = (int)ParseUInt("region_y", reply);
            SimPort = (short)ParseUInt("sim_port", reply);
            string simIP = ParseString("sim_ip", reply);
            IPAddress.TryParse(simIP, out SimIP);
            SeedCapability = ParseString("seed_capability", reply);

            // Buddy list
            if (reply.ContainsKey("buddy-list") && reply["buddy-list"] is ArrayList)
            {
                List<BuddyListEntry> buddys = new List<BuddyListEntry>();

                ArrayList buddyArray = (ArrayList)reply["buddy-list"];
                for (int i = 0; i < buddyArray.Count; i++)
                {
                    if (buddyArray[i] is Hashtable)
                    {
                        BuddyListEntry bud = new BuddyListEntry();
                        Hashtable buddy = (Hashtable)buddyArray[i];

                        bud.buddy_id = ParseString("buddy_id", buddy);
                        bud.buddy_rights_given = (int)ParseUInt("buddy_rights_given", buddy);
                        bud.buddy_rights_has = (int)ParseUInt("buddy_rights_has", buddy);

                        buddys.Add(bud);
                    }
                }

                BuddyList = buddys.ToArray();
            }

            SecondsSinceEpoch = (int)ParseUInt("seconds_since_epoch", reply);

            InventoryRoot = ParseMappedUUID("inventory-root", "folder_id", reply);
            InventorySkeleton = ParseInventorySkeleton("inventory-skeleton", reply);

            LibraryOwner = ParseMappedUUID("inventory-lib-owner", "agent_id", reply);
            LibraryRoot = ParseMappedUUID("inventory-lib-root", "folder_id", reply);
            LibrarySkeleton = ParseInventorySkeleton("inventory-skel-lib", reply);

            // UDP Blacklist
            if (reply.ContainsKey("udp_blacklist"))
            {
                UDPBlacklist = ParseString("udp_blacklist", reply);
            }
        }

        public void ToXmlRpc(XmlWriter writer)
        {
            writer.WriteStartElement("methodResponse");
            {
                writer.WriteStartElement("params");
                writer.WriteStartElement("param");
                writer.WriteStartElement("value");
                writer.WriteStartElement("struct");
                {
                    if (Success)
                    {
                        // session_id
                        WriteXmlRpcStringMember(writer, false, "session_id", SessionID.ToString());

                        // ui-config
                        WriteXmlRpcArrayStart(writer, "ui-config");
                        WriteXmlRpcStringMember(writer, true, "allow_first_life", "Y");
                        WriteXmlRpcArrayEnd(writer);

                        // inventory-lib-owner
                        WriteXmlRpcArrayStart(writer, "inventory-lib-owner");
                        WriteXmlRpcStringMember(writer, true, "agent_id", LibraryOwner.ToString());
                        WriteXmlRpcArrayEnd(writer);

                        // start_location
                        WriteXmlRpcStringMember(writer, false, "start_location", StartLocation);

                        // seconds_since_epoch
                        WriteXmlRpcIntMember(writer, false, "seconds_since_epoch", (uint)SecondsSinceEpoch);

                        // event_categories (TODO)
                        WriteXmlRpcArrayStart(writer, "event_categories");
                        WriteXmlRpcCategory(writer, "Default Event Category", 20);
                        WriteXmlRpcArrayEnd(writer);

                        // tutorial_setting (TODO)
                        WriteXmlRpcArrayStart(writer, "tutorial_setting");
                        WriteXmlRpcTutorialSetting(writer, "http://127.0.0.1/tutorial/");
                        WriteXmlRpcArrayEnd(writer);

                        // classified_categories (TODO)
                        WriteXmlRpcArrayStart(writer, "classified_categories");
                        WriteXmlRpcCategory(writer, "Default Classified Category", 1);
                        WriteXmlRpcArrayEnd(writer);

                        // inventory-root
                        WriteXmlRpcArrayStart(writer, "inventory-root");
                        WriteXmlRpcStringMember(writer, true, "folder_id", InventoryRoot.ToString());
                        WriteXmlRpcArrayEnd(writer);

                        // sim_port
                        WriteXmlRpcIntMember(writer, false, "sim_port", (uint)SimPort);

                        // agent_id
                        WriteXmlRpcStringMember(writer, false, "agent_id", AgentID.ToString());

                        // agent_access
                        WriteXmlRpcStringMember(writer, false, "agent_access", AgentAccess);

                        // inventory-skeleton
                        WriteXmlRpcArrayStart(writer, "inventory-skeleton");
                        if (InventorySkeleton != null)
                        {
                            for (int i = 0; i < InventorySkeleton.Length; i++)
                            {
                                WriteXmlRpcInventoryItem(writer, InventorySkeleton[i].Name, InventorySkeleton[i].ParentUUID,
                                    (uint)InventorySkeleton[i].Version, (uint)InventorySkeleton[i].PreferredType,InventorySkeleton[i].UUID);
                            }
                        }
                        else
                        {
                            WriteXmlRpcInventoryItem(writer, "Inventory", UUID.Zero, 1, (uint)AssetType.Folder, InventoryRoot);
                        }
                        WriteXmlRpcArrayEnd(writer);

                        // buddy-list
                        WriteXmlRpcArrayStart(writer, "buddy-list");
                        if (BuddyList != null)
                        {
                            for (int i = 0; i < BuddyList.Length; i++)
                            {
                                WriteXmlRpcBuddy(writer, (uint)BuddyList[i].buddy_rights_given, 
                                    (uint)BuddyList[i].buddy_rights_has, UUID.Parse(BuddyList[i].buddy_id));
                            }
                        }
                        else
                        {
                            //WriteXmlRpcBuddy(writer, 0, 0, UUID.Random());
                        }
                        WriteXmlRpcArrayEnd(writer);

                        // first_name
                        WriteXmlRpcStringMember(writer, false, "first_name", FirstName);

                        // global-textures
                        WriteXmlRpcArrayStart(writer, "global-textures");
                        writer.WriteStartElement("value");
                        writer.WriteStartElement("struct");
                        {
                            WriteXmlRpcStringMember(writer, false, "sun_texture_id", "cce0f112-878f-4586-a2e2-a8f104bba271");
                            WriteXmlRpcStringMember(writer, false, "cloud_texture_id", "fc4b9f0b-d008-45c6-96a4-01dd947ac621");
                            WriteXmlRpcStringMember(writer, false, "moon_texture_id", "d07f6eed-b96a-47cd-b51d-400ad4a1c428");
                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        WriteXmlRpcArrayEnd(writer);

                        // inventory-skel-lib
                        WriteXmlRpcArrayStart(writer, "inventory-skel-lib");
                        if (LibrarySkeleton != null)
                        {
                            for (int i = 0; i < LibrarySkeleton.Length; i++)
                            {
                                WriteXmlRpcInventoryItem(writer, LibrarySkeleton[i].Name, LibrarySkeleton[i].ParentUUID,
                                    (uint)LibrarySkeleton[i].Version, (uint)LibrarySkeleton[i].PreferredType, LibrarySkeleton[i].UUID);
                            }
                        }
                        else
                        {
                            WriteXmlRpcInventoryItem(writer, "Library", UUID.Zero, 1, (uint)AssetType.Folder, LibraryRoot);
                        }
                        WriteXmlRpcArrayEnd(writer);

                        // seed_capability
                        WriteXmlRpcStringMember(writer, false, "seed_capability", SeedCapability);

                        // gestures
                        WriteXmlRpcArrayStart(writer, "gestures");
                        WriteXmlRpcGesture(writer, UUID.Random(), UUID.Random());
                        WriteXmlRpcArrayEnd(writer);

                        // sim_ip
                        WriteXmlRpcStringMember(writer, false, "sim_ip", SimIP.ToString());

                        // inventory-lib-root
                        WriteXmlRpcArrayStart(writer, "inventory-lib-root");
                        WriteXmlRpcStringMember(writer, true, "folder_id", LibraryRoot.ToString());
                        WriteXmlRpcArrayEnd(writer);

                        // login-flags
                        WriteXmlRpcArrayStart(writer, "login-flags");
                        writer.WriteStartElement("value");
                        writer.WriteStartElement("struct");
                        {
                            WriteXmlRpcStringMember(writer, false, "gendered", "Y");
                            WriteXmlRpcStringMember(writer, false, "stipend_since_login", "N");
                            WriteXmlRpcStringMember(writer, false, "ever_logged_in", "Y");
                            if (DateTime.Now.IsDaylightSavingTime())
                                WriteXmlRpcStringMember(writer, false, "daylight_savings", "Y");
                            else
                                WriteXmlRpcStringMember(writer, false, "daylight_savings", "N");
                        }
                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        WriteXmlRpcArrayEnd(writer);

                        // inventory_host
                        WriteXmlRpcStringMember(writer, false, "inventory_host", IPAddress.Loopback.ToString());

                        // home
                        OSDArray homeRegionHandle = new OSDArray(2);
                        uint homeRegionX, homeRegionY;
                        Utils.LongToUInts(HomeRegion, out homeRegionX, out homeRegionY);
                        homeRegionHandle.Add(OSD.FromReal((double)homeRegionX));
                        homeRegionHandle.Add(OSD.FromReal((double)homeRegionY));

                        OSDMap home = new OSDMap(3);
                        home["region_handle"] = homeRegionHandle;
                        home["position"] = OSD.FromVector3(HomePosition);
                        home["look_at"] = OSD.FromVector3(HomeLookAt);

                        WriteXmlRpcStringMember(writer, false, "home", OSDParser.SerializeLLSDNotation(home));

                        // message
                        WriteXmlRpcStringMember(writer, false, "message", Message);

                        // look_at
                        string lookAt = OSDParser.SerializeLLSDNotation(OSD.FromVector3(LookAt));
                        WriteXmlRpcStringMember(writer, false, "look_at", lookAt);

                        // login
                        WriteXmlRpcStringMember(writer, false, "login", "true");

                        // event_notifications
                        WriteXmlRpcArrayStart(writer, "event_notifications");
                        WriteXmlRpcArrayEnd(writer);

                        // secure_session_id
                        WriteXmlRpcStringMember(writer, false, "secure_session_id", SecureSessionID.ToString());

                        // region_x
                        WriteXmlRpcIntMember(writer, false, "region_x", (uint)RegionX);

                        // last_name
                        WriteXmlRpcStringMember(writer, false, "last_name", LastName);

                        // region_y
                        WriteXmlRpcIntMember(writer, false, "region_y", (uint)RegionY);

                        // circuit_code
                        WriteXmlRpcIntMember(writer, false, "circuit_code", (uint)CircuitCode);

                        // initial-outfit
                        WriteXmlRpcArrayStart(writer, "initial-outfit");
                        WriteXmlRpcArrayEnd(writer);
                    }
                    else
                    {
                        // Login failure
                    }
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Close();
        }

        #region Parsing Helpers

        public static uint ParseUInt(string key, OSDMap reply)
        {
            OSD osd;
            if (reply.TryGetValue(key, out osd))
                return (uint)osd.AsInteger();
            else
                return 0;
        }

        public static uint ParseUInt(string key, Hashtable reply)
        {
            if (reply.ContainsKey(key))
            {
                object value = reply[key];
                if (value is int)
                    return (uint)(int)value;
            }

            return 0;
        }

        public static UUID ParseUUID(string key, OSDMap reply)
        {
            OSD osd;
            if (reply.TryGetValue(key, out osd))
                return osd.AsUUID();
            else
                return UUID.Zero;
        }

        public static UUID ParseUUID(string key, Hashtable reply)
        {
            if (reply.ContainsKey(key))
            {
                UUID value;
                if (UUID.TryParse((string)reply[key], out value))
                    return value;
            }

            return UUID.Zero;
        }

        public static string ParseString(string key, OSDMap reply)
        {
            OSD osd;
            if (reply.TryGetValue(key, out osd))
                return osd.AsString();
            else
                return String.Empty;
        }

        public static string ParseString(string key, Hashtable reply)
        {
            if (reply.ContainsKey(key))
                return (string)reply[key];

            return String.Empty;
        }

        public static Vector3 ParseVector3(string key, OSDMap reply)
        {
            OSD osd;
            if (reply.TryGetValue(key, out osd))
            {
                if (osd.Type == OSDType.Array)
                {
                    return ((OSDArray)osd).AsVector3();
                }
                else if (osd.Type == OSDType.String)
                {
                    OSDArray array = (OSDArray)OSDParser.DeserializeLLSDNotation(osd.AsString());
                    return array.AsVector3();
                }
            }

            return Vector3.Zero;
        }

        public static Vector3 ParseVector3(string key, Hashtable reply)
        {
            if (reply.ContainsKey(key))
            {
                object value = reply[key];

                if (value is IList)
                {
                    IList list = (IList)value;
                    if (list.Count == 3)
                    {
                        float x, y, z;
                        Single.TryParse((string)list[0], out x);
                        Single.TryParse((string)list[1], out y);
                        Single.TryParse((string)list[2], out z);

                        return new Vector3(x, y, z);
                    }
                }
                else if (value is string)
                {
                    OSDArray array = (OSDArray)OSDParser.DeserializeLLSDNotation((string)value);
                    return array.AsVector3();
                }
            }

            return Vector3.Zero;
        }

        public static UUID ParseMappedUUID(string key, string key2, OSDMap reply)
        {
            OSD folderOSD;
            if (reply.TryGetValue(key, out folderOSD) && folderOSD.Type == OSDType.Array)
            {
                OSDArray array = (OSDArray)folderOSD;
                if (array.Count == 1 && array[0].Type == OSDType.Map)
                {
                    OSDMap map = (OSDMap)array[0];
                    OSD folder;
                    if (map.TryGetValue(key2, out folder))
                        return folder.AsUUID();
                }
            }

            return UUID.Zero;
        }

        public static UUID ParseMappedUUID(string key, string key2, Hashtable reply)
        {
            if (reply.ContainsKey(key) && reply[key] is ArrayList)
            {
                ArrayList array = (ArrayList)reply[key];
                if (array.Count == 1 && array[0] is Hashtable)
                {
                    Hashtable map = (Hashtable)array[0];
                    return ParseUUID(key2, map);
                }
            }

            return UUID.Zero;
        }

        public static InventoryFolder[] ParseInventoryFolders(string key, UUID owner, OSDMap reply)
        {
            List<InventoryFolder> folders = new List<InventoryFolder>();

            OSD skeleton;
            if (reply.TryGetValue(key, out skeleton) && skeleton.Type == OSDType.Array)
            {
                OSDArray array = (OSDArray)skeleton;

                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].Type == OSDType.Map)
                    {
                        OSDMap map = (OSDMap)array[i];
                        InventoryFolder folder = new InventoryFolder(map["folder_id"].AsUUID());
                        folder.PreferredType = (AssetType)map["type_default"].AsInteger();
                        folder.Version = map["version"].AsInteger();
                        folder.OwnerID = owner;
                        folder.ParentUUID = map["parent_id"].AsUUID();
                        folder.Name = map["name"].AsString();

                        folders.Add(folder);
                    }
                }
            }

            return folders.ToArray();
        }

        public InventoryFolder[] ParseInventorySkeleton(string key, OSDMap reply)
        {
            List<InventoryFolder> folders = new List<InventoryFolder>();

            OSD skeleton;
            if (reply.TryGetValue(key, out skeleton) && skeleton.Type == OSDType.Array)
            {
                OSDArray array = (OSDArray)skeleton;
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].Type == OSDType.Map)
                    {
                        OSDMap map = (OSDMap)array[i];
                        InventoryFolder folder = new InventoryFolder(map["folder_id"].AsUUID());
                        folder.Name = map["name"].AsString();
                        folder.ParentUUID = map["parent_id"].AsUUID();
                        folder.PreferredType = (AssetType)map["type_default"].AsInteger();
                        folder.Version = map["version"].AsInteger();
                        folders.Add(folder);
                    }
                }
            }
            return folders.ToArray();
        }

        public InventoryFolder[] ParseInventorySkeleton(string key, Hashtable reply)
        {
            List<InventoryFolder> folders = new List<InventoryFolder>();

            if (reply.ContainsKey(key) && reply[key] is ArrayList)
            {
                ArrayList array = (ArrayList)reply[key];
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i] is Hashtable)
                    {
                        Hashtable map = (Hashtable)array[i];
                        InventoryFolder folder = new InventoryFolder(ParseUUID("folder_id", map));
                        folder.Name = ParseString("name", map);
                        folder.ParentUUID = ParseUUID("parent_id", map);
                        folder.PreferredType = (AssetType)ParseUInt("type_default", map);
                        folder.Version = (int)ParseUInt("version", map);
                        folders.Add(folder);
                    }
                }
            }

            return folders.ToArray();
        }

        #endregion Parsing Helpers

        #region XmlRpc Serializing Helpers

        public static void WriteXmlRpcStringMember(XmlWriter writer, bool wrapWithValueStruct, string name, string value)
        {
            if (wrapWithValueStruct)
            {
                writer.WriteStartElement("value");
                writer.WriteStartElement("struct");
            }
            writer.WriteStartElement("member");
            {
                writer.WriteElementString("name", name);
                writer.WriteStartElement("value");
                {
                    writer.WriteElementString("string", value);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            if (wrapWithValueStruct)
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        public static void WriteXmlRpcIntMember(XmlWriter writer, bool wrapWithValueStruct, string name, uint value)
        {
            if (wrapWithValueStruct)
            {
                writer.WriteStartElement("value");
                writer.WriteStartElement("struct");
            }
            writer.WriteStartElement("member");
            {
                writer.WriteElementString("name", name);
                writer.WriteStartElement("value");
                {
                    writer.WriteElementString("i4", value.ToString());
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            if (wrapWithValueStruct)
            {
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        public static void WriteXmlRpcArrayStart(XmlWriter writer, string name)
        {
            writer.WriteStartElement("member");
            writer.WriteElementString("name", name);
            writer.WriteStartElement("value");
            writer.WriteStartElement("array");
            writer.WriteStartElement("data");
        }

        public static void WriteXmlRpcArrayEnd(XmlWriter writer)
        {
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public static void WriteXmlRpcEmptyValueStruct(XmlWriter writer)
        {
            writer.WriteStartElement("value");
            writer.WriteStartElement("struct");
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public static void WriteXmlRpcCategory(XmlWriter writer, string name, uint id)
        {
            writer.WriteStartElement("value");
            writer.WriteStartElement("struct");
            {
                WriteXmlRpcStringMember(writer, false, "category_name", name);
                WriteXmlRpcIntMember(writer, false, "category_id", id);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public static void WriteXmlRpcInventoryItem(XmlWriter writer, string name, UUID parentID,
            uint version, uint typeDefault, UUID folderID)
        {
            writer.WriteStartElement("value");
            writer.WriteStartElement("struct");
            {
                WriteXmlRpcStringMember(writer, false, "name", name);
                WriteXmlRpcStringMember(writer, false, "parent_id", parentID.ToString());
                WriteXmlRpcIntMember(writer, false, "version", version);
                WriteXmlRpcIntMember(writer, false, "type_default", typeDefault);
                WriteXmlRpcStringMember(writer, false, "folder_id", folderID.ToString());
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public static void WriteXmlRpcBuddy(XmlWriter writer, uint rightsHas, uint rightsGiven, UUID buddyID)
        {
            writer.WriteStartElement("value");
            writer.WriteStartElement("struct");
            {
                WriteXmlRpcIntMember(writer, false, "buddy_rights_has", rightsHas);
                WriteXmlRpcIntMember(writer, false, "buddy_rights_given", rightsGiven);
                WriteXmlRpcStringMember(writer, false, "buddy_id", buddyID.ToString());
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public static void WriteXmlRpcGesture(XmlWriter writer, UUID assetID, UUID itemID)
        {
            writer.WriteStartElement("value");
            writer.WriteStartElement("struct");
            {
                WriteXmlRpcStringMember(writer, false, "asset_id", assetID.ToString());
                WriteXmlRpcStringMember(writer, false, "item_id", itemID.ToString());
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public static void WriteXmlRpcTutorialSetting(XmlWriter writer, string url)
        {
            writer.WriteStartElement("value");
            writer.WriteStartElement("struct");
            {
                WriteXmlRpcStringMember(writer, false, "tutorial_url", url);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteStartElement("value");
            writer.WriteStartElement("struct");
            {
                writer.WriteStartElement("member");
                {
                    writer.WriteElementString("name", "use_tutorial");
                    writer.WriteStartElement("value");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        #endregion XmlRpc Serializing Helpers
    }

    #endregion Structs

    /// <summary>
    /// Oveerides SSL certificate validation check for Mono
    /// </summary>
    /// <remarks>Remove me when MONO can handle ServerCertificateValidationCallback</remarks>
    public class AcceptAllCertificatePolicy : ICertificatePolicy
    {
        public AcceptAllCertificatePolicy()
        {
        }

        public bool CheckValidationResult(ServicePoint sPoint,
            System.Security.Cryptography.X509Certificates.X509Certificate cert,
            WebRequest wRequest, int certProb)
        {
            // Always accept
            return true;
        }
    }

    /// <summary>
    /// Login Routines
    /// </summary>
    public partial class NetworkManager : INetworkManager
    {
        #region Delegates

        /// <summary>
        /// Fired when a login request is successful or not
        /// </summary>
        /// <param name="login"></param>
        /// <param name="message"></param>
        public delegate void LoginCallback(LoginStatus login, string message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginSuccess"></param>
        /// <param name="redirect"></param>
        /// <param name="replyData"></param>
        /// <param name="message"></param>
        /// <param name="reason"></param>
        public delegate void LoginResponseCallback(bool loginSuccess, bool redirect, string message, string reason, LoginResponseData replyData);

        #endregion Delegates

        #region Events

        /// <summary>Called any time the login status changes, will eventually
        /// return LoginStatus.Success or LoginStatus.Failure</summary>
        public event LoginCallback OnLogin;

        /// <summary>Called when a reply is received from the login server, the
        /// login sequence will block until this event returns</summary>
        private event LoginResponseCallback OnLoginResponse;

        #endregion Events

        #region Public Members
        /// <summary>Seed CAPS URL returned from the login server</summary>
        public string LoginSeedCapability = String.Empty;
        /// <summary>Current state of logging in</summary>
        public LoginStatus LoginStatusCode { get { return InternalStatusCode; } }
        /// <summary>Upon login failure, contains a short string key for the
        /// type of login error that occurred</summary>
        public string LoginErrorKey { get { return InternalErrorKey; } }
        /// <summary>The raw XML-RPC reply from the login server, exactly as it
        /// was received (minus the HTTP header)</summary>
        public string RawLoginReply { get { return InternalRawLoginReply; } }
        /// <summary>During login this contains a descriptive version of 
        /// LoginStatusCode. After a successful login this will contain the 
        /// message of the day, and after a failed login a descriptive error 
        /// message will be returned</summary>
        public string LoginMessage { get { return InternalLoginMessage; } }

        #endregion

        #region Private Members
        private LoginParams? CurrentContext = null;
        private AutoResetEvent LoginEvent = new AutoResetEvent(false);
        private LoginStatus InternalStatusCode = LoginStatus.None;
        private string InternalErrorKey = String.Empty;
        private string InternalLoginMessage = String.Empty;
        private string InternalRawLoginReply = String.Empty;
        private Dictionary<LoginResponseCallback, string[]> CallbackOptions = new Dictionary<LoginResponseCallback, string[]>();
        /// <summary>A list of packets obtained during the login process which networkmanager will log but not process</summary>
        private readonly List<string> UDPBlacklist = new List<string>();
        #endregion

        #region Public Methods

        /// <summary>
        /// Generate sane default values for a login request
        /// </summary>
        /// <param name="firstName">Account first name</param>
        /// <param name="lastName">Account last name</param>
        /// <param name="password">Account password</param>
        /// <param name="userAgent">Client application name</param>
        /// <param name="userVersion">Client application version</param>
        /// <returns>A populated <seealso cref="LoginParams"/> struct containing
        /// sane defaults</returns>
        public LoginParams DefaultLoginParams(string firstName, string lastName, string password,
            string userAgent, string userVersion)
        {
            List<string> options = new List<string>(15);
            options.Add("inventory-root");
            options.Add("inventory-skeleton");
            options.Add("inventory-lib-root");
            options.Add("inventory-lib-owner");
            options.Add("inventory-skel-lib");
            options.Add("initial-outfit");
            options.Add("gestures");
            options.Add("event_categories");
            options.Add("event_notifications");
            options.Add("classified_categories");
            options.Add("buddy-list");
            options.Add("ui-config");
            options.Add("tutorial_settings");
            options.Add("login-flags");
            options.Add("global-textures");

            LoginParams loginParams = new LoginParams();
            if (Client == null)
                throw new NullReferenceException("GridClient must be instantiated before calling DefaultLoginParams()");

            loginParams.URI = Client.Settings.LOGIN_SERVER;
            loginParams.Timeout = Client.Settings.LOGIN_TIMEOUT;
            loginParams.MethodName = "login_to_simulator";
            loginParams.FirstName = firstName;
            loginParams.LastName = lastName;
            loginParams.Password = password;
            loginParams.Start = "last";
            loginParams.Channel = userAgent + " (OpenMetaverse)";
            loginParams.Version = userVersion;
            loginParams.Platform = GetPlatform();
            loginParams.MAC = GetMAC();
            loginParams.ViewerDigest = String.Empty;
            loginParams.Options = options.ToArray();
            loginParams.ID0 = GetMAC();

            return loginParams;
        }

        /// <summary>
        /// Simplified login that takes the most common and required fields
        /// </summary>
        /// <param name="firstName">Account first name</param>
        /// <param name="lastName">Account last name</param>
        /// <param name="password">Account password</param>
        /// <param name="userAgent">Client application name</param>
        /// <param name="userVersion">Client application version</param>
        /// <returns>Whether the login was successful or not. On failure the
        /// LoginErrorKey string will contain the error code and LoginMessage
        /// will contain a description of the error</returns>
        public bool Login(string firstName, string lastName, string password, string userAgent, string userVersion)
        {
            return Login(firstName, lastName, password, userAgent, "last", userVersion);
        }

        /// <summary>
        /// Simplified login that takes the most common fields along with a
        /// starting location URI, and can accept an MD5 string instead of a
        /// plaintext password
        /// </summary>
        /// <param name="firstName">Account first name</param>
        /// <param name="lastName">Account last name</param>
        /// <param name="password">Account password or MD5 hash of the password
        /// such as $1$1682a1e45e9f957dcdf0bb56eb43319c</param>
        /// <param name="userAgent">Client application name</param>
        /// <param name="start">Starting location URI that can be built with
        /// StartLocation()</param>
        /// <param name="userVersion">Client application version</param>
        /// <returns>Whether the login was successful or not. On failure the
        /// LoginErrorKey string will contain the error code and LoginMessage
        /// will contain a description of the error</returns>
        public bool Login(string firstName, string lastName, string password, string userAgent, string start,
            string userVersion)
        {
            LoginParams loginParams = DefaultLoginParams(firstName, lastName, password, userAgent, userVersion);
            loginParams.Start = start;

            return Login(loginParams);
        }

        /// <summary>
        /// Login that takes a struct of all the values that will be passed to
        /// the login server
        /// </summary>
        /// <param name="loginParams">The values that will be passed to the login
        /// server, all fields must be set even if they are String.Empty</param>
        /// <returns>Whether the login was successful or not. On failure the
        /// LoginErrorKey string will contain the error code and LoginMessage
        /// will contain a description of the error</returns>
        public bool Login(LoginParams loginParams)
        {
            BeginLogin(loginParams);

            LoginEvent.WaitOne(loginParams.Timeout, false);

            if (CurrentContext != null)
            {
                CurrentContext = null; // Will force any pending callbacks to bail out early
                InternalStatusCode = LoginStatus.Failed;
                InternalLoginMessage = "Timed out";
                return false;
            }

            return (InternalStatusCode == LoginStatus.Success);
        }

        public void BeginLogin(LoginParams loginParams)
        {
            // FIXME: Now that we're using CAPS we could cancel the current login and start a new one
            if (CurrentContext != null)
                throw new Exception("Login already in progress");

            LoginEvent.Reset();
            CurrentContext = loginParams;

            BeginLogin();
        }

        public void RegisterLoginResponseCallback(LoginResponseCallback callback)
        {
            RegisterLoginResponseCallback(callback, null);
        }


        public void RegisterLoginResponseCallback(LoginResponseCallback callback, string[] options)
        {
            CallbackOptions.Add(callback, options);
            OnLoginResponse += callback;
        }

        public void UnregisterLoginResponseCallback(LoginResponseCallback callback)
        {
            CallbackOptions.Remove(callback);
            OnLoginResponse -= callback;
        }

        /// <summary>
        /// Build a start location URI for passing to the Login function
        /// </summary>
        /// <param name="sim">Name of the simulator to start in</param>
        /// <param name="x">X coordinate to start at</param>
        /// <param name="y">Y coordinate to start at</param>
        /// <param name="z">Z coordinate to start at</param>
        /// <returns>String with a URI that can be used to login to a specified
        /// location</returns>
        public static string StartLocation(string sim, int x, int y, int z)
        {
            return String.Format("uri:{0}&{1}&{2}&{3}", sim.ToLower(), x, y, z);
        }

        #endregion

        #region Private Methods

        private void BeginLogin()
        {
            LoginParams loginParams = CurrentContext.Value;

            #region Sanity Check loginParams

            if (loginParams.Options == null)
                loginParams.Options = new List<string>().ToArray();

            // Convert the password to MD5 if it isn't already
            if (loginParams.Password.Length != 35 && !loginParams.Password.StartsWith("$1$"))
                loginParams.Password = Utils.MD5(loginParams.Password);

            if (loginParams.ViewerDigest== null)
                loginParams.ViewerDigest = String.Empty;

            if (loginParams.Version == null)
                loginParams.Version = String.Empty;

            if (loginParams.UserAgent == null)
                loginParams.UserAgent = String.Empty;

            if (loginParams.Platform == null)
                loginParams.Platform = String.Empty;

            if (loginParams.MAC == null)
                loginParams.MAC = String.Empty;

            if (loginParams.Channel == null)
                loginParams.Channel = String.Empty;

            if (loginParams.AgreeToTos == null)
                loginParams.AgreeToTos = "true";

            if (loginParams.ReadCritical == null)
                loginParams.ReadCritical = "true";

            if (loginParams.Author == null)
                loginParams.Author = String.Empty;

            #endregion

            // Override SSL authentication mechanisms. DO NOT convert this to the 
            // .NET 2.0 preferred method, the equivalent function in Mono has a 
            // different name and it will break compatibility!
            #pragma warning disable 0618
            ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
            // TODO: At some point, maybe we should check the cert?

            if (Client.Settings.USE_LLSD_LOGIN)
            {
                #region LLSD Based Login
                
                // Create the CAPS login structure
                OSDMap loginLLSD = new OSDMap();
                loginLLSD["first"] = OSD.FromString(loginParams.FirstName);
                loginLLSD["last"] = OSD.FromString(loginParams.LastName);
                loginLLSD["passwd"] = OSD.FromString(loginParams.Password);
                loginLLSD["start"] = OSD.FromString(loginParams.Start);
                loginLLSD["channel"] = OSD.FromString(loginParams.Channel);
                loginLLSD["version"] = OSD.FromString(loginParams.Version);
                loginLLSD["platform"] = OSD.FromString(loginParams.Platform);
                loginLLSD["mac"] = OSD.FromString(loginParams.MAC);
                loginLLSD["agree_to_tos"] = OSD.FromBoolean(true);
                loginLLSD["read_critical"] = OSD.FromBoolean(true);
                loginLLSD["viewer_digest"] = OSD.FromString(loginParams.ViewerDigest);
                loginLLSD["id0"] = OSD.FromString(loginParams.ID0);
                
                // Create the options LLSD array
                OSDArray optionsOSD = new OSDArray();
                for (int i = 0; i < loginParams.Options.Length; i++)
                    optionsOSD.Add(OSD.FromString(loginParams.Options[i]));

                foreach (string[] callbackOpts in CallbackOptions.Values)
                {
                    if (callbackOpts != null)
                    {
                        for (int i = 0; i < callbackOpts.Length; i++)
                        {
                            if (!optionsOSD.Contains(callbackOpts[i]))
                                optionsOSD.Add(callbackOpts[i]);
                        }
                    }
                }
                loginLLSD["options"] = optionsOSD;

                // Make the CAPS POST for login
                Uri loginUri;
                try
                {
                    loginUri = new Uri(loginParams.URI);
                }
                catch (Exception ex)
                {
                    Logger.Log(String.Format("Failed to parse login URI {0}, {1}", loginParams.URI, ex.Message),
                        Helpers.LogLevel.Error, Client);
                    return;
                }

                CapsClient loginRequest = new CapsClient(loginUri);
                loginRequest.OnComplete += new CapsClient.CompleteCallback(LoginReplyLLSDHandler);
                loginRequest.UserData = CurrentContext;
                UpdateLoginStatus(LoginStatus.ConnectingToLogin, String.Format("Logging in as {0} {1}...", loginParams.FirstName, loginParams.LastName));
                loginRequest.BeginGetResponse(loginLLSD, OSDFormat.Xml, Client.Settings.CAPS_TIMEOUT);

                #endregion
            }
            else
            {
                #region XML-RPC Based Login Code
                
                // Create the Hashtable for XmlRpcCs
                Hashtable loginXmlRpc = new Hashtable();
                loginXmlRpc["first"] = loginParams.FirstName;
                loginXmlRpc["last"] = loginParams.LastName;
                loginXmlRpc["passwd"] = loginParams.Password;
                loginXmlRpc["start"] = loginParams.Start;
                loginXmlRpc["channel"] = loginParams.Channel;
                loginXmlRpc["version"] = loginParams.Version;
                loginXmlRpc["platform"] = loginParams.Platform;
                loginXmlRpc["mac"] = loginParams.MAC;
                loginXmlRpc["agree_to_tos"] = true;
                loginXmlRpc["read_critical"] = true;
                loginXmlRpc["viewer_digest"] = loginParams.ViewerDigest;
                loginXmlRpc["id0"] = loginParams.ID0;
                loginXmlRpc["last_exec_event"] = 0;

                // Create the options array
                ArrayList options = new ArrayList();
                for (int i = 0; i < loginParams.Options.Length; i++)
                    options.Add(loginParams.Options[i]);

                foreach (string[] callbackOpts in CallbackOptions.Values)
                {
                    if (callbackOpts != null)
                    {
                        for (int i = 0; i < callbackOpts.Length; i++)
                        {
                            if (!options.Contains(callbackOpts[i]))
                                options.Add(callbackOpts[i]);
                        }
                    }
                }
                loginXmlRpc["options"] = options;

                try
                {
                    ArrayList loginArray = new ArrayList(1);
                    loginArray.Add(loginXmlRpc);
                    XmlRpcRequest request = new XmlRpcRequest(CurrentContext.Value.MethodName, loginArray);

                    // Start the request
                    Thread requestThread = new Thread(new ThreadStart(
                        delegate()
                        {
                            LoginReplyXmlRpcHandler(
                                request.Send(CurrentContext.Value.URI, CurrentContext.Value.Timeout),
                                loginParams);
                        }));
                    requestThread.Name = "XML-RPC Login";
                    requestThread.Start();
                }
                catch (Exception e)
                {
                    UpdateLoginStatus(LoginStatus.Failed, "Error opening the login server connection: " + e);
                }

                #endregion
            }
        }

        private void UpdateLoginStatus(LoginStatus status, string message)
        {
            InternalStatusCode = status;
            InternalLoginMessage = message;

            Logger.DebugLog("Login status: " + status.ToString() + ": " + message, Client);

            // If we reached a login resolution trigger the event
            if (status == LoginStatus.Success || status == LoginStatus.Failed)
            {
                CurrentContext = null;
                LoginEvent.Set();
            }

            // Fire the login status callback
            if (OnLogin != null)
            {
                try { OnLogin(status, message); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// Handles response from XML-RPC login replies
        /// </summary>
        private void LoginReplyXmlRpcHandler(XmlRpcResponse response, LoginParams context)
        {
            LoginResponseData reply = new LoginResponseData();
            ushort simPort = 0;
            uint regionX = 0;
            uint regionY = 0;

            // Fetch the login response
            try
            {
                reply.Parse(response.Value as Hashtable);
                if (context.GetHashCode() != CurrentContext.Value.GetHashCode())
                {
                    Logger.Log("Login Response does not match login request", Helpers.LogLevel.Warning);
                    // TODO: Although the hash codes to not match the login appears to work correctly if
                    // we don't exit the process here. Need to look into why the hashcodes do not match with Mono.
                    // See LIBOMV-485 for additional information
                    //
                    // Temporarily disabling this to allow mono based clients to login with XML-RPC
                    //return;
                }
            }
            catch (Exception e)
            {
                UpdateLoginStatus(LoginStatus.Failed, "Error retrieving the login response from the server " + e.Message );
                Logger.Log("Login response failure: " + e.Message + " " + e.StackTrace, Helpers.LogLevel.Debug);
                return;
            }

            string reason = reply.Reason;
            string message = reply.Message;
            
            if (reply.Login == "true")
            {
                // Remove the quotes around our first name.
                if (reply.FirstName[0] == '"')
                    reply.FirstName = reply.FirstName.Remove(0, 1);
                if (reply.FirstName[reply.FirstName.Length - 1] == '"')
                    reply.FirstName = reply.FirstName.Remove(reply.FirstName.Length - 1);

                #region Critical Information

                try
                {
                    // Networking
                    Client.Network.CircuitCode = (uint)reply.CircuitCode;
                    regionX = (uint)reply.RegionX;
                    regionY = (uint)reply.RegionY;
                    simPort = (ushort)reply.SimPort;
                    LoginSeedCapability = reply.SeedCapability;
                }
                catch (Exception)
                {
                    UpdateLoginStatus(LoginStatus.Failed, "Login server failed to return critical information");
                    return;
                }

                #endregion Critical Information
                
                /* Add any blacklisted UDP packets to the blacklist
                 * for exclusion from packet processing */
                if(reply.UDPBlacklist != null)
                    UDPBlacklist.AddRange(reply.UDPBlacklist.Split(','));
                
                // Misc:
                //uint timestamp = (uint)reply.seconds_since_epoch;
                //DateTime time = Helpers.UnixTimeToDateTime(timestamp); // TODO: Do something with this?

                // Unhandled:
                // reply.gestures
                // reply.event_categories
                // reply.classified_categories
                // reply.event_notifications
                // reply.ui_config
                // reply.login_flags
                // reply.global_textures
                // reply.inventory_lib_root
                // reply.inventory_lib_owner
                // reply.inventory_skeleton
                // reply.inventory_skel_lib
                // reply.initial_outfit
            }
            
            bool redirect = (reply.Login == "indeterminate");
            
            try
            {
                if (OnLoginResponse != null)
                {
                    try { OnLoginResponse(reply.Success, redirect, message, reason, reply); }
                    catch (Exception ex) { Logger.Log(ex.ToString(), Helpers.LogLevel.Error); }
                }
            }
            catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }

            // Make the next network jump, if needed
            if (redirect)
            {
                UpdateLoginStatus(LoginStatus.Redirecting, "Redirecting login...");
                LoginParams loginParams = CurrentContext.Value;
                loginParams.URI = reply.NextUrl;
                //CurrentContext.Value.MethodName = reply.next_method;

                // Sleep for some amount of time while the servers work
                int seconds = reply.NextDuration;
                Logger.Log("Sleeping for " + seconds + " seconds during a login redirect",
                    Helpers.LogLevel.Info);
                Thread.Sleep(seconds * 1000);

                // Ignore next_options for now
                CurrentContext = loginParams;
                
                // Ignore next_options and next_duration for now
                BeginLogin();
            }
            else if (reply.Success)
            {
                UpdateLoginStatus(LoginStatus.ConnectingToSim, "Connecting to simulator...");

                ulong handle = Utils.UIntsToLong(regionX, regionY);

                // Connect to the sim given in the login reply
                if (Connect(reply.SimIP, simPort, handle, true, LoginSeedCapability) != null)
                {
                    // Request the economy data right after login
                    SendPacket(new EconomyDataRequestPacket());

                    // Update the login message with the MOTD returned from the server
                    UpdateLoginStatus(LoginStatus.Success, message);

                    // Fire an event for connecting to the grid
                    if (OnConnected != null)
                    {
                        try { OnConnected(this.Client); }
                        catch (Exception e) { Logger.Log(e.ToString(), Helpers.LogLevel.Error); }
                    }
                }
                else
                {
                    UpdateLoginStatus(LoginStatus.Failed, "Unable to connect to simulator");
                }
            }
            else
            {
                // Make sure a usable error key is set

                if (!String.IsNullOrEmpty(reason))
                    InternalErrorKey = reason;
                else
                    InternalErrorKey = "unknown";

                UpdateLoginStatus(LoginStatus.Failed, message);
            }
        }

        /// <summary>
        /// Handle response from LLSD login replies
        /// </summary>
        /// <param name="client"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        private void LoginReplyLLSDHandler(CapsClient client, OSD result, Exception error)
        {
            if (error == null)
            {
                if (result != null && result.Type == OSDType.Map)
                {
                    OSDMap map = (OSDMap)result;
                    OSD osd;

                    LoginResponseData data = new LoginResponseData();
                    data.Parse(map);

                    if (map.TryGetValue("login", out osd))
                    {
                        bool loginSuccess = osd.AsBoolean();
                        bool redirect = (osd.AsString() == "indeterminate");

                        if (redirect)
                        {
                            // Login redirected

                            // Make the next login URL jump
                            UpdateLoginStatus(LoginStatus.Redirecting, data.Message);

                            LoginParams loginParams = CurrentContext.Value;
                            loginParams.URI = LoginResponseData.ParseString("next_url", map);
                            //CurrentContext.Params.MethodName = LoginResponseData.ParseString("next_method", map);

                            // Sleep for some amount of time while the servers work
                            int seconds = (int)LoginResponseData.ParseUInt("next_duration", map);
                            Logger.Log("Sleeping for " + seconds + " seconds during a login redirect",
                                Helpers.LogLevel.Info);
                            Thread.Sleep(seconds * 1000);

                            // Ignore next_options for now
                            CurrentContext = loginParams;

                            BeginLogin();
                        }
                        else if (loginSuccess)
                        {
                            // Login succeeded

                            // Fire the login callback
                            if (OnLoginResponse != null)
                            {
                                try { OnLoginResponse(loginSuccess, redirect, data.Message, data.Reason, data); }
                                catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                            }

                            // These parameters are stored in NetworkManager, so instead of registering
                            // another callback for them we just set the values here
                            CircuitCode = (uint)data.CircuitCode;
                            LoginSeedCapability = data.SeedCapability;

                            UpdateLoginStatus(LoginStatus.ConnectingToSim, "Connecting to simulator...");

                            ulong handle = Utils.UIntsToLong((uint)data.RegionX, (uint)data.RegionY);

                            if (data.SimIP != null && data.SimPort != 0)
                            {
                                // Connect to the sim given in the login reply
                                if (Connect(data.SimIP, (ushort)data.SimPort, handle, true, LoginSeedCapability) != null)
                                {
                                    // Request the economy data right after login
                                    SendPacket(new EconomyDataRequestPacket());

                                    // Update the login message with the MOTD returned from the server
                                    UpdateLoginStatus(LoginStatus.Success, data.Message);

                                    // Fire an event for connecting to the grid
                                    if (OnConnected != null)
                                    {
                                        try { OnConnected(this.Client); }
                                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                                    }
                                }
                                else
                                {
                                    UpdateLoginStatus(LoginStatus.Failed,
                                        "Unable to establish a UDP connection to the simulator");
                                }
                            }
                            else
                            {
                                UpdateLoginStatus(LoginStatus.Failed,
                                    "Login server did not return a simulator address");
                            }
                        }
                        else
                        {
                            // Login failed

                            // Make sure a usable error key is set
                            if (data.Reason != String.Empty)
                                InternalErrorKey = data.Reason;
                            else
                                InternalErrorKey = "unknown";

                            UpdateLoginStatus(LoginStatus.Failed, data.Message);
                        }
                    }
                    else
                    {
                        // Got an LLSD map but no login value
                        UpdateLoginStatus(LoginStatus.Failed, "login parameter missing in the response");
                    }
                }
                else
                {
                    // No LLSD response
                    InternalErrorKey = "bad response";
                    UpdateLoginStatus(LoginStatus.Failed, "Empty or unparseable login response");
                }
            }
            else
            {
                // Connection error
                InternalErrorKey = "no connection";
                UpdateLoginStatus(LoginStatus.Failed, error.Message);
            }
        }
        
        /// <summary>
        /// Get current OS
        /// </summary>
        /// <returns>Either "Win" or "Linux"</returns>
        private static string GetPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    return "Linux";
                default:
                    return "Win";
            }
        }

        /// <summary>
        /// Get clients default Mac Address
        /// </summary>
        /// <returns>A string containing the first found Mac Address</returns>
        private static string GetMAC()
        {
            string mac = String.Empty;
            System.Net.NetworkInformation.NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            if (nics.Length > 0)
                mac = nics[0].GetPhysicalAddress().ToString().ToUpper();

            if (mac.Length < 12)
                mac = mac.PadRight(12, '0');

            return String.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                mac.Substring(0, 2),
                mac.Substring(2, 2),
                mac.Substring(4, 2),
                mac.Substring(6, 2),
                mac.Substring(8, 2),
                mac.Substring(10, 2));
        }

        #endregion
    }
}
