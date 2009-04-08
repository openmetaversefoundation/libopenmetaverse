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
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Http;
using OpenMetaverse.Packets;
using CookComputing.XmlRpc;
using OpenMetaverse.Interfaces;

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
        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string URI;
        /// <summary></summary>
        public int Timeout;
        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string MethodName;
        /// <summary></summary>
        [XmlRpcMember("first")]
        public string FirstName;
        /// <summary></summary>
        [XmlRpcMember("last")]
        public string LastName;
        /// <summary></summary>
        [XmlRpcMember("passwd")]
        public string Password;
        /// <summary></summary>
        [XmlRpcMember("start")]
        public string Start;
        /// <summary></summary>
        [XmlRpcMember("channel")]
        public string Channel;
        /// <summary></summary>
        [XmlRpcMember("version")]
        public string Version;
        /// <summary></summary>
        [XmlRpcMember("platform")]
        public string Platform;
        /// <summary></summary>
        [XmlRpcMember("mac")]
        public string MAC;
        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("viewer_digest")]
        public string ViewerDigest;
        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string id0;
        /* Login via XML-RPC Specific Members */
        [XmlRpcMember("user-agent")]
        public string user_agent;
        public string author;
        public string agree_to_tos;
        public string read_critical;
        public string viewer_digest;
        public string[] options;
    }

    #region XML-RPC login respons structs
    /// <summary>
    /// Represents a folder entry returned during login
    /// </summary>
    public struct InventorySkeletonEntry
    {
        /// <summary>
        /// The default <seealso cref="AssetType"/> stored in this folder
        /// </summary>
        public int type_default;
        /// <summary>The version number of this folder asset</summary>
        public int version;
        /// <summary>The name of this foler</summary>
        public string name;
        /// <summary>This folders <seealso cref="UUID"/></summary>
        public string folder_id;
        /// <summary>The parent folders <seealso cref="UUID"/></summary>
        public string parent_id;
    }

    /// <summary>
    /// Represents the root folder of the inventory tree
    /// </summary>
    public struct InventoryRootEntry
    {
        /// <summary>The root folders <seealso cref="UUID"/> represented as a string</summary>
        public string folder_id;
    }

    public struct CategoryEntry
    {
        public int category_id;
        public string category_name;
    }

    public struct EventNotificationEntry
    {
        // ???
    }

    public struct GlobalTextureEntry
    {
        public string cloud_texture_id;
        public string sun_texture_id;
        public string moon_texture_id;
    }

    public struct InventoryLibraryOwnerEntry
    {
        public string agent_id;
    }

    public struct LoginFlagsEntry
    {
        public string ever_logged_in;
        public string daylight_savings;
        public string stipend_since_login;
        public string gendered;
    }

    public struct BuddyListEntry
    {
        public int buddy_rights_given;
        public string buddy_id;
        public int buddy_rights_has;
    }

    public struct GestureEntry
    {
        public string asset_id;
        public string item_id;
    }

    public struct UIConfigEntry
    {
        public string allow_first_life;
    }

    public struct OutfitEntry
    {
        // ???
    }
    #endregion
    /*
    public struct XMLLoginMethodResponse
    {
        public string login;
        public string message;

        #region Login Failure
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string reason;
        #endregion

        #region Login Success
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("inventory-skeleton")]
        public InventorySkeletonEntry[] inventory_skeleton;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string session_id;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("inventory-root")]
        public InventoryRootEntry[] inventory_root;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public EventNotificationEntry[] event_notifications;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public CategoryEntry[] event_categories;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string secure_session_id;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string start_location;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string first_name;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string last_name;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public int region_x;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public int region_y;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("global-textures")]
        public GlobalTextureEntry[] global_textures;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string home;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("inventory-lib-owner")]
        public InventoryLibraryOwnerEntry[] inventory_lib_owner;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("inventory-lib-root")]
        public InventoryRootEntry[] inventory_lib_root;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("inventory-skel-lib")]
        public InventorySkeletonEntry[] inventory_skel_lib;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public CategoryEntry[] classified_categories;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("login-flags")]
        public LoginFlagsEntry[] login_flags;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string agent_access;

        [XmlRpcMember("buddy-list")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public BuddyListEntry[] buddy_list;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public int circuit_code;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public int sim_port;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public GestureEntry[] gestures;
        [XmlRpcMember("ui-config")]
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public UIConfigEntry[] ui_config;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string sim_ip;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string look_at;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string agent_id;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public int seconds_since_epoch;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string seed_capability;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("initial-outfit")]
        public OutfitEntry[] initial_outfit;
        #endregion

        #region Redirection
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string next_method;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string next_url;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string[] next_options;

        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string next_duration;
        #endregion
    }
    */
    /// <summary>
    /// 
    /// </summary>
    public struct LoginResponseData
    {
        /// <summary>true, false, indeterminate</summary>
        public string login;

        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public bool Success;

        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string Reason;

        /// <summary>Login Motd</summary>
        [XmlRpcMember("message")]
        public string Message;
        
        /// <summary></summary>
        [XmlRpcMember("agent_id")]
        public string AgentID;

        /// <summary></summary>
        [XmlRpcMember("session_id")]
        public string SessionID;

        /// <summary></summary>
        
        [XmlRpcMember("secure_session_id")]
        public string SecureSessionID;

        /// <summary></summary>
        
        [XmlRpcMember("first_name")]
        public string FirstName;

        /// <summary></summary>
        
        [XmlRpcMember("last_name")]
        public string LastName;

        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("start_location")]
        public string StartLocation;

        /// <summary>M or PG // also agent_region_access and agent_access_max</summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        [XmlRpcMember("agent_access")]
        public string AgentAccess;

        /// <summary>
        /// {'region_handle':[r257280, r259584], 'position':[r157.684, r148.283, r650], 'look_at':[r1, r0, r0]}
        /// </summary>
        public string home;

        /// <summary></summary>
        /// <remarks>[r0.99967899999999998428,r0.025349300000000001692,r0]</remarks>
        [XmlRpcMember("look_at")]
        public string LookAt;

        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public ulong HomeRegion;

        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public Vector3 HomePosition;

        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public Vector3 HomeLookAt;

        /// <summary></summary>
        [XmlRpcMember("circuit_code")]
        public int CircuitCode;

        /// <summary></summary>
        [XmlRpcMember("region_x")]
        public int RegionX;

        /// <summary></summary>
        [XmlRpcMember("region_y")]
        public int RegionY;

        /// <summary></summary>
        [XmlRpcMember("sim_port")]
        public int SimPort;

        /// <summary></summary>
        [XmlRpcMember("sim_ip")]
        public string SimIP;

        /// <summary></summary>
        [XmlRpcMember("seed_capability")]
        public string SeedCapability;

        /// <summary></summary>
        [XmlRpcMember("buddy-list")]
        public BuddyListEntry[] BuddyList;
        
        /// <summary></summary>
        [XmlRpcMember("seconds_since_epoch")]
        public int SecondsSinceEpoch;

        #region Inventory
        /// <summary></summary>
        [XmlRpcMember("inventory-root")]
        public InventoryRootEntry[] InventoryRoot;
        
        /// <summary></summary>
        [XmlRpcMember("inventory-lib-root")]
        public InventoryRootEntry[] LibraryRoot;
        
        /// <summary></summary>
        [XmlRpcMember("inventory-skeleton")]
        public InventorySkeletonEntry[] InventorySkeleton;
        
        /// <summary></summary>
        [XmlRpcMember("inventory-skel-lib")]
        public InventorySkeletonEntry[] LibrarySkeleton;

        /// <summary></summary>
        [XmlRpcMember("inventory-lib-owner")]
        public InventoryLibraryOwnerEntry[] LibraryOwner;
        #endregion

        #region Redirection
        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string next_method;

        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string next_url;

        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string[] next_options;

        /// <summary></summary>
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public int next_duration;
        #endregion

        /* These aren't currently being utilized by the library */
        /// <summary></summary>
        public string agent_access_max;
        /// <summary></summary>
        public string agent_region_access;
        /// <summary></summary>
        public int ao_transition;
        /// <summary></summary>
        public string inventory_host;
        /// <summary></summary>
        public string udp_blacklist;

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
                AgentID = ParseString("agent_id", reply);
                SessionID = ParseString("session_id", reply);
                SecureSessionID = ParseString("secure_session_id", reply);
                FirstName = ParseString("first_name", reply).Trim('"');
                LastName = ParseString("last_name", reply).Trim('"');
                StartLocation = ParseString("start_location", reply);
                AgentAccess = ParseString("agent_access", reply);
                LookAt = ParseString("look_at", reply); 
            }
            catch (OSDException e)
            {
                Logger.DebugLog("Login server returned (some) invalid data: " + e.Message);
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
            //string simIP = ParseString("sim_ip", reply);
            SimIP = ParseString("sim_ip", reply);
            //IPAddress.TryParse(simIP, out SimIP);
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
            
            InventoryRoot = new InventoryRootEntry[1];
            InventoryRoot[0].folder_id = ParseMappedUUID("inventory-root", "folder_id", reply).ToString();
            InventorySkeleton = ParseInventorySkeleton("inventory-skeleton", reply);

            LibraryRoot = new InventoryRootEntry[1];
            
            LibraryOwner = new InventoryLibraryOwnerEntry[1];
            LibraryOwner[0].agent_id = ParseMappedUUID("inventory-lib-owner", "agent_id", reply).ToString();

            LibraryRoot[0].folder_id = ParseMappedUUID("inventory-lib-root", "folder_id", reply).ToString();
            LibrarySkeleton = ParseInventorySkeleton("inventory-skel-lib", reply);


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

        public static UUID ParseUUID(string key, OSDMap reply)
        {
            OSD osd;
            if (reply.TryGetValue(key, out osd))
                return osd.AsUUID();
            else
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

        private InventorySkeletonEntry[] ParseInventorySkeleton(string key, OSDMap reply)
        {
            List<InventorySkeletonEntry> folders = new List<InventorySkeletonEntry>();

            OSD skeleton;
            if (reply.TryGetValue(key, out skeleton) && skeleton.Type == OSDType.Array)
            {
                OSDArray array = (OSDArray)skeleton;
                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].Type == OSDType.Map)
                    {
                        OSDMap map = (OSDMap)array[i];
                        InventorySkeletonEntry folder = new InventorySkeletonEntry();
                        folder.folder_id = map["folder_id"].AsString();
                        folder.name = map["name"].AsString();
                        folder.parent_id = map["parent_id"].AsString();
                        folder.type_default = map["type_default"].AsInteger();
                        folder.version = map["version"].AsInteger();
                        folders.Add(folder);
                    }
                }
            }
            return folders.ToArray();
        }


        #endregion Parsing Helpers
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
            List<string> options = new List<string>();

            LoginParams loginParams = new LoginParams();

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
            loginParams.options = options.ToArray();
            loginParams.id0 = GetMAC();

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

            // Sanity check login Params
            #region Sanity Check loginParams
            if (loginParams.options == null)
                loginParams.options = new List<string>().ToArray();

            // Convert the password to MD5 if it isn't already
            if (loginParams.Password.Length != 35 && !loginParams.Password.StartsWith("$1$"))
                loginParams.Password = Utils.MD5(loginParams.Password);

            if (loginParams.viewer_digest == null)
                loginParams.viewer_digest = String.Empty;

            if (loginParams.Version == null)
                loginParams.Version = String.Empty;

            if (loginParams.user_agent == null)
                loginParams.user_agent = String.Empty;

            if (loginParams.Platform == null)
                loginParams.Platform = String.Empty;

            if (loginParams.MAC == null)
                loginParams.MAC = String.Empty;

            if (loginParams.Channel == null)
                loginParams.Channel = String.Empty;

            if (loginParams.agree_to_tos == null)
                loginParams.agree_to_tos = "true";

            if (loginParams.read_critical == null)
                loginParams.read_critical = "true";

            if (loginParams.author == null)
                loginParams.author = String.Empty;
            #endregion

            // Override SSL authentication mechanisms. DO NOT convert this to the 
            // .NET 2.0 preferred method, the equivalent function in Mono has a 
            // different name and it will break compatibility!
            #pragma warning disable 0618
            ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();

            if (Client.Settings.USE_LLSD_LOGIN)
            {
                #region LLSD Based Login
                
                // TODO: At some point, maybe we should check the cert?

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
                loginLLSD["id0"] = OSD.FromString(loginParams.id0);
                
                // Create the options LLSD array
                OSDArray optionsOSD = new OSDArray();
                for (int i = 0; i < loginParams.options.Length; i++)
                    optionsOSD.Add(OSD.FromString(loginParams.options[i]));

                
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
                loginRequest.OnComplete += new CapsClient.CompleteCallback(LoginReplyHandler);
                loginRequest.UserData = CurrentContext;
                UpdateLoginStatus(LoginStatus.ConnectingToLogin, String.Format("Logging in as {0} {1}...", loginParams.FirstName, loginParams.LastName));
                loginRequest.StartRequest(OSDParser.SerializeLLSDXmlBytes(loginLLSD), "application/xml+llsd");

                #endregion
            }
            else
            {
                #region XML-RPC Based Login Code
                
                loginParams.FirstName = CurrentContext.Value.FirstName;
                loginParams.LastName = CurrentContext.Value.LastName;
                loginParams.Password = CurrentContext.Value.Password;
                loginParams.Start = CurrentContext.Value.Start;
                loginParams.Channel = CurrentContext.Value.Channel;
                loginParams.Version = CurrentContext.Value.Version;
                loginParams.Platform = CurrentContext.Value.Platform;
                loginParams.MAC = CurrentContext.Value.MAC;
                
                List<string> options = new List<string>(CurrentContext.Value.options.Length + CallbackOptions.Values.Count);
                options.AddRange(CurrentContext.Value.options);
                foreach (string[] callbackOpts in CallbackOptions.Values)
                {
                    if (callbackOpts != null)
                        foreach (string option in callbackOpts)
                            if (!options.Contains(option)) // TODO: Replace with some kind of Dictionary/Set?
                                options.Add(option);
                }

                loginParams.options = options.ToArray();

                try
                {
                    ILoginProxy proxy = XmlRpcProxyGen.Create<ILoginProxy>();
                    proxy.KeepAlive = false;
                    proxy.Expect100Continue = false;
                    proxy.ResponseEvent += new XmlRpcResponseEventHandler(proxy_ResponseEvent);
                    proxy.Url = CurrentContext.Value.URI;
                    proxy.XmlRpcMethod = CurrentContext.Value.MethodName;

                    // Start the request
                    proxy.BeginLoginToSimulator(loginParams, new AsyncCallback(LoginMethodCallback), new object[] { proxy, CurrentContext });
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
        #region XML-RPC Callbacks

        private void proxy_ResponseEvent(object sender, XmlRpcResponseEventArgs args)
        {
            TextReader reader = new StreamReader(args.ResponseStream);
            InternalRawLoginReply = reader.ReadToEnd();
        }

        /// <summary>
        /// Handles response from XML-RPC.NET login replies
        /// </summary>
        /// <param name="result"></param>
        private void LoginMethodCallback(IAsyncResult result)
        {
            object[] asyncState = result.AsyncState as object[];
            ILoginProxy proxy = asyncState[0] as ILoginProxy;
            LoginParams context = (LoginParams)asyncState[1];
            XmlRpcAsyncResult clientResult = result as XmlRpcAsyncResult;
            LoginResponseData reply;
            IPAddress simIP = IPAddress.Any; // Temporary
            ushort simPort = 0;
            uint regionX = 0;
            uint regionY = 0;
            bool loginSuccess = false;

            // Fetch the login response
            try
            {
                reply = proxy.EndLoginToSimulator(clientResult);
                if (context.GetHashCode() != CurrentContext.Value.GetHashCode())
                {
                    Logger.Log("Login Response does not match login request", Helpers.LogLevel.Warning);
                    return;
                }
            }
            catch (Exception e)
            {
                UpdateLoginStatus(LoginStatus.Failed, "Error retrieving the login response from the server " + e.Message );
                return;
            }

            string reason = reply.Reason;
            string message = reply.Message;
            
            if (reply.login == "true")
            {
                loginSuccess = true;

                // FIXME: No information should be set here, everything can take care of itself
                // through login reply handlers

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
                    //simIP = reply.SimIP;
                    IPAddress.TryParse(reply.SimIP, out simIP);
                    LoginSeedCapability = reply.SeedCapability;
                }
                catch (Exception)
                {
                    UpdateLoginStatus(LoginStatus.Failed, "Login server failed to return critical information");
                    return;
                }

                #endregion Critical Information

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
            
            bool redirect = (reply.login == "indeterminate");
            
            try
            {
                if (OnLoginResponse != null)
                {
                    try { OnLoginResponse(loginSuccess, redirect, message, reason, reply); }
                    catch (Exception ex) { Logger.Log(ex.ToString(), Helpers.LogLevel.Error); }
                }
            }
            catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, ex); }

            // Make the next network jump, if needed
            if (redirect)
            {
                UpdateLoginStatus(LoginStatus.Redirecting, "Redirecting login...");
                LoginParams loginParams = CurrentContext.Value;
                loginParams.URI = reply.next_url;
                //CurrentContext.Value.MethodName = reply.next_method;

                // Sleep for some amount of time while the servers work
                int seconds = reply.next_duration;
                Logger.Log("Sleeping for " + seconds + " seconds during a login redirect",
                    Helpers.LogLevel.Info);
                Thread.Sleep(seconds * 1000);

                // Ignore next_options for now
                CurrentContext = loginParams;
                
                // Ignore next_options and next_duration for now
                BeginLogin();
            }
            else if (loginSuccess)
            {
                UpdateLoginStatus(LoginStatus.ConnectingToSim, "Connecting to simulator...");

                ulong handle = Utils.UIntsToLong(regionX, regionY);

                // Connect to the sim given in the login reply
                if (Connect(simIP, simPort, handle, true, LoginSeedCapability) != null)
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


        #endregion

        /// <summary>
        /// Handle response from LLSD login replies
        /// </summary>
        /// <param name="client"></param>
        /// <param name="result"></param>
        /// <param name="error"></param>
        private void LoginReplyHandler(CapsClient client, OSD result, Exception error)
        {
            if (error == null)
            {
                if (result != null && result.Type == OSDType.Map)
                {
                    OSDMap map = (OSDMap)result;

                    OSD osd;
                    string reason, message;

                    if (map.TryGetValue("reason", out osd))
                        reason = osd.AsString();
                    else
                        reason = String.Empty;

                    if (map.TryGetValue("message", out osd))
                        message = osd.AsString();
                    else
                        message = String.Empty;

                    if (map.TryGetValue("login", out osd))
                    {
                        bool loginSuccess = osd.AsBoolean();
                        bool redirect = (osd.AsString() == "indeterminate");
                        LoginResponseData data = new LoginResponseData();
                        data.Reason = reason;
                        data.Message = message;               

                        if (redirect)
                        {
                            // Login redirected

                            // Make the next login URL jump
                            UpdateLoginStatus(LoginStatus.Redirecting, message);

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

                            // Parse successful login replies into LoginResponseData structs
                            data.Parse(map);

                            // Fire the login callback
                            if (OnLoginResponse != null)
                            {
                                try { OnLoginResponse(loginSuccess, redirect, message, reason, data); }
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
                                if (Connect(IPAddress.Parse(data.SimIP), (ushort)data.SimPort, handle, true, LoginSeedCapability) != null)
                                {
                                    // Request the economy data right after login
                                    SendPacket(new EconomyDataRequestPacket());

                                    // Update the login message with the MOTD returned from the server
                                    UpdateLoginStatus(LoginStatus.Success, message);

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
                            if (reason != String.Empty)
                                InternalErrorKey = reason;
                            else
                                InternalErrorKey = "unknown";

                            UpdateLoginStatus(LoginStatus.Failed, message);
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
