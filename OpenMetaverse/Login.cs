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
using OpenMetaverse.Capabilities;
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
    /// 
    /// </summary>
    public struct LoginParams
    {
        /// <summary></summary>
        public string URI;
        /// <summary></summary>
        public int Timeout;
        /// <summary></summary>
        public string MethodName;
        /// <summary></summary>
        public string FirstName;
        /// <summary></summary>
        public string LastName;
        /// <summary></summary>
        public string Password;
        /// <summary></summary>
        public string Start;
        /// <summary></summary>
        public string Channel;
        /// <summary></summary>
        public string Version;
        /// <summary></summary>
        public string Platform;
        /// <summary></summary>
        public string MAC;
        /// <summary></summary>
        public string ViewerDigest;
        /// <summary></summary>
        public List<string> Options;
        /// <summary></summary>
        public string id0;
    }

    public struct LoginResponseData
    {
        public bool Success;
        public string Reason;
        public string Message;
        public UUID AgentID;
        public UUID SessionID;
        public UUID SecureSessionID;
        public string FirstName;
        public string LastName;
        public string StartLocation;
        public string AgentAccess;
        public Vector3 LookAt;
        public ulong HomeRegion;
        public Vector3 HomePosition;
        public Vector3 HomeLookAt;
        public uint CircuitCode;
        public uint RegionX;
        public uint RegionY;
        public ushort SimPort;
        public IPAddress SimIP;
        public string SeedCapability;
        public FriendInfo[] BuddyList;
        public DateTime SecondsSinceEpoch;
        public UUID InventoryRoot;
        public UUID LibraryRoot;
        public InventoryFolder[] InventorySkeleton;
        public InventoryFolder[] LibrarySkeleton;
        public UUID LibraryOwner;

        public void Parse(LLSDMap reply)
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
            }
            catch (LLSDException e)
            {
                Logger.DebugLog("Login server returned (some) invalid data: " + e.Message);
            }

            // Home
            LLSDMap home = null;
            LLSD llsdHome = LLSDParser.DeserializeNotation(reply["home"].AsString());

            if (llsdHome.Type == LLSDType.Map)
            {
                home = (LLSDMap)llsdHome;

                LLSD homeRegion;
                if (home.TryGetValue("region_handle", out homeRegion) && homeRegion.Type == LLSDType.Array)
                {
                    LLSDArray homeArray = (LLSDArray)homeRegion;
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

            CircuitCode = ParseUInt("circuit_code", reply);
            RegionX = ParseUInt("region_x", reply);
            RegionY = ParseUInt("region_y", reply);
            SimPort = (ushort)ParseUInt("sim_port", reply);
            string simIP = ParseString("sim_ip", reply);
            IPAddress.TryParse(simIP, out SimIP);
            SeedCapability = ParseString("seed_capability", reply);

            // Buddy list
            LLSD buddyLLSD;
            if (reply.TryGetValue("buddy-list", out buddyLLSD) && buddyLLSD.Type == LLSDType.Array)
            {
                LLSDArray buddyArray = (LLSDArray)buddyLLSD;
                BuddyList = new FriendInfo[buddyArray.Count];

                for (int i = 0; i < buddyArray.Count; i++)
                {
                    if (buddyArray[i].Type == LLSDType.Map)
                    {
                        LLSDMap buddy = (LLSDMap)buddyArray[i];
                        BuddyList[i] = new FriendInfo(
                            ParseUUID("buddy_id", buddy),
                            (FriendRights)ParseUInt("buddy_rights_given", buddy),
                            (FriendRights)ParseUInt("buddy_rights_has", buddy));
                    }
                }
            }

            SecondsSinceEpoch = Utils.UnixTimeToDateTime(ParseUInt("seconds_since_epoch", reply));
            InventoryRoot = ParseMappedUUID("inventory-root", "folder_id", reply);
            InventorySkeleton = ParseInventoryFolders("inventory-skeleton", AgentID, reply);
            LibraryRoot = ParseMappedUUID("inventory-lib-root", "folder_id", reply);
            LibraryOwner = ParseMappedUUID("inventory-lib-owner", "agent_id", reply);
            LibrarySkeleton = ParseInventoryFolders("inventory-skel-lib", LibraryOwner, reply);
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
                        WriteXmlRpcIntMember(writer, false, "seconds_since_epoch", Utils.DateTimeToUnixTime(SecondsSinceEpoch));

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
                        WriteXmlRpcIntMember(writer, false, "sim_port", SimPort);

                        // agent_id
                        WriteXmlRpcStringMember(writer, false, "agent_id", AgentID.ToString());

                        // agent_access
                        WriteXmlRpcStringMember(writer, false, "agent_access", AgentAccess);

                        // inventory-skeleton
                        WriteXmlRpcArrayStart(writer, "inventory-skeleton");
                        if (InventorySkeleton != null)
                        {
                            foreach (InventoryFolder folder in InventorySkeleton)
                                WriteXmlRpcInventoryItem(writer, folder.Name, folder.ParentUUID, (uint)folder.Version, (uint)folder.PreferredType, folder.UUID);
                        }
                        else
                        {
                            WriteXmlRpcInventoryItem(writer, "Inventory", UUID.Zero, 1, (uint)InventoryType.Category, InventoryRoot);
                        }
                        WriteXmlRpcArrayEnd(writer);

                        // buddy-list
                        WriteXmlRpcArrayStart(writer, "buddy-list");
                        if (BuddyList != null)
                        {
                            foreach (FriendInfo friend in BuddyList)
                                WriteXmlRpcBuddy(writer, (uint)friend.MyFriendRights, (uint)friend.TheirFriendRights, friend.UUID);
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
                            foreach (InventoryFolder folder in LibrarySkeleton)
                                WriteXmlRpcInventoryItem(writer, folder.Name, folder.ParentUUID, (uint)folder.Version, (uint)folder.PreferredType, folder.UUID);
                        }
                        else
                        {
                            WriteXmlRpcInventoryItem(writer, "Library", UUID.Zero, 1, (uint)InventoryType.Category, LibraryRoot);
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
                        LLSDArray homeRegionHandle = new LLSDArray(2);
                        uint homeRegionX, homeRegionY;
                        Utils.LongToUInts(HomeRegion, out homeRegionX, out homeRegionY);
                        homeRegionHandle.Add(LLSD.FromReal((double)homeRegionX));
                        homeRegionHandle.Add(LLSD.FromReal((double)homeRegionY));

                        LLSDMap home = new LLSDMap(3);
                        home["region_handle"] = homeRegionHandle;
                        home["position"] = LLSD.FromVector3(HomePosition);
                        home["look_at"] = LLSD.FromVector3(HomeLookAt);

                        WriteXmlRpcStringMember(writer, false, "home", LLSDParser.SerializeNotation(home));

                        // message
                        WriteXmlRpcStringMember(writer, false, "message", Message);

                        // look_at
                        WriteXmlRpcStringMember(writer, false, "look_at", LLSDParser.SerializeNotation(LLSD.FromVector3(LookAt)));

                        // login
                        WriteXmlRpcStringMember(writer, false, "login", "true");

                        // event_notifications
                        WriteXmlRpcArrayStart(writer, "event_notifications");
                        WriteXmlRpcArrayEnd(writer);

                        // secure_session_id
                        WriteXmlRpcStringMember(writer, false, "secure_session_id", SecureSessionID.ToString());

                        // region_x
                        WriteXmlRpcIntMember(writer, false, "region_x", RegionX);

                        // last_name
                        WriteXmlRpcStringMember(writer, false, "last_name", LastName);

                        // region_y
                        WriteXmlRpcIntMember(writer, false, "region_y", RegionY);

                        // circuit_code
                        WriteXmlRpcIntMember(writer, false, "circuit_code", CircuitCode);

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

        public static uint ParseUInt(string key, LLSDMap reply)
        {
            LLSD llsd;
            if (reply.TryGetValue(key, out llsd))
                return (uint)llsd.AsInteger();
            else
                return 0;
        }

        public static UUID ParseUUID(string key, LLSDMap reply)
        {
            LLSD llsd;
            if (reply.TryGetValue(key, out llsd))
                return llsd.AsUUID();
            else
                return UUID.Zero;
        }

        public static string ParseString(string key, LLSDMap reply)
        {
            LLSD llsd;
            if (reply.TryGetValue(key, out llsd))
                return llsd.AsString();
            else
                return String.Empty;
        }

        public static Vector3 ParseVector3(string key, LLSDMap reply)
        {
            LLSD llsd;
            if (reply.TryGetValue(key, out llsd))
            {
                if (llsd.Type == LLSDType.Array)
                {
                    return ((LLSDArray)llsd).AsVector3();
                }
                else if (llsd.Type == LLSDType.String)
                {
                    LLSDArray array = (LLSDArray)LLSDParser.DeserializeNotation(llsd.AsString());
                    return array.AsVector3();
                }
            }

            return Vector3.Zero;
        }

        public static UUID ParseMappedUUID(string key, string key2, LLSDMap reply)
        {
            LLSD folderLLSD;
            if (reply.TryGetValue(key, out folderLLSD) && folderLLSD.Type == LLSDType.Array)
            {
                LLSDArray array = (LLSDArray)folderLLSD;
                if (array.Count == 1 && array[0].Type == LLSDType.Map)
                {
                    LLSDMap map = (LLSDMap)array[0];
                    LLSD folder;
                    if (map.TryGetValue(key2, out folder))
                        return folder.AsUUID();
                }
            }

            return UUID.Zero;
        }

        public static InventoryFolder[] ParseInventoryFolders(string key, UUID owner, LLSDMap reply)
        {
            List<InventoryFolder> folders = new List<InventoryFolder>();

            LLSD skeleton;
            if (reply.TryGetValue(key, out skeleton) && skeleton.Type == LLSDType.Array)
            {
                LLSDArray array = (LLSDArray)skeleton;

                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i].Type == LLSDType.Map)
                    {
                        LLSDMap map = (LLSDMap)array[i];
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

    // TODO: Remove me when MONO can handle ServerCertificateValidationCallback
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

    public partial class NetworkManager : INetworkManager
    {
        #region Delegates

        /// <summary>
        /// 
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

        private LoginParams? CurrentContext = null;
        private AutoResetEvent LoginEvent = new AutoResetEvent(false);
        private LoginStatus InternalStatusCode = LoginStatus.None;
        private string InternalErrorKey = String.Empty;
        private string InternalLoginMessage = String.Empty;
        private string InternalRawLoginReply = String.Empty;
        private Dictionary<LoginResponseCallback, string[]> CallbackOptions = new Dictionary<LoginResponseCallback, string[]>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstName">Account first name</param>
        /// <param name="lastName">Account last name</param>
        /// <param name="password">Account password</param>
        /// <param name="userAgent">Client application name</param>
        /// <param name="userVersion">Client application version</param>
        /// <returns></returns>
        public LoginParams DefaultLoginParams(string firstName, string lastName, string password,
            string userAgent, string userVersion)
        {
            List<string> options = new List<string>();
            //options.Add("gestures");
            //options.Add("event_categories");
            //options.Add("event_notifications");
            //options.Add("classified_categories");
            //options.Add("ui-config");
            //options.Add("login-flags");
            //options.Add("global-textures");
            //options.Add("initial-outfit");

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
            loginParams.Options = options;
			// workaround for bots being caught up in a global ban
			// This *should* be the hash of the first hard drive, 
			// but any unique identifier works.
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

        private void BeginLogin()
        {
            LoginParams loginParams = CurrentContext.Value;

            // Sanity check
            if (loginParams.Options == null)
                loginParams.Options = new List<string>();

            // Convert the password to MD5 if it isn't already
            if (loginParams.Password.Length != 35 && !loginParams.Password.StartsWith("$1$"))
                loginParams.Password = Utils.MD5(loginParams.Password);

            // Override SSL authentication mechanisms. DO NOT convert this to the 
            // .NET 2.0 preferred method, the equivalent function in Mono has a 
            // different name and it will break compatibility!
            #pragma warning disable 0618
            ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
            // TODO: At some point, maybe we should check the cert?

            // Create the CAPS login structure
            LLSDMap loginLLSD = new LLSDMap();
            loginLLSD["first"] = LLSD.FromString(loginParams.FirstName);
            loginLLSD["last"] = LLSD.FromString(loginParams.LastName);
            loginLLSD["passwd"] = LLSD.FromString(loginParams.Password);
            loginLLSD["start"] = LLSD.FromString(loginParams.Start);
            loginLLSD["channel"] = LLSD.FromString(loginParams.Channel);
            loginLLSD["version"] = LLSD.FromString(loginParams.Version);
            loginLLSD["platform"] = LLSD.FromString(loginParams.Platform);
            loginLLSD["mac"] = LLSD.FromString(loginParams.MAC);
            loginLLSD["agree_to_tos"] = LLSD.FromBoolean(true);
            loginLLSD["read_critical"] = LLSD.FromBoolean(true);
            loginLLSD["viewer_digest"] = LLSD.FromString(loginParams.ViewerDigest);
            loginLLSD["id0"] = LLSD.FromString(loginParams.id0);

            // Create the options LLSD array
            LLSDArray optionsLLSD = new LLSDArray();
            for (int i = 0; i < loginParams.Options.Count; i++)
                optionsLLSD.Add(LLSD.FromString(loginParams.Options[i]));
            foreach (string[] callbackOpts in CallbackOptions.Values)
            {
                if (callbackOpts != null)
                {
                    for (int i = 0; i < callbackOpts.Length; i++)
                    {
                        if (!optionsLLSD.Contains(callbackOpts[i]))
                            optionsLLSD.Add(callbackOpts[i]);
                    }
                }
            }
            loginLLSD["options"] = optionsLLSD;

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
            loginRequest.StartRequest(LLSDParser.SerializeXmlBytes(loginLLSD), "application/xml+llsd");
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

        private void LoginReplyHandler(CapsClient client, LLSD result, Exception error)
        {
            if (error == null)
            {
                if (result != null && result.Type == LLSDType.Map)
                {
                    LLSDMap map = (LLSDMap)result;

                    LLSD llsd;
                    string reason, message;

                    if (map.TryGetValue("reason", out llsd))
                        reason = llsd.AsString();
                    else
                        reason = String.Empty;

                    if (map.TryGetValue("message", out llsd))
                        message = llsd.AsString();
                    else
                        message = String.Empty;

                    if (map.TryGetValue("login", out llsd))
                    {
                        bool loginSuccess = llsd.AsBoolean();
                        bool redirect = (llsd.AsString() == "indeterminate");
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
                            CircuitCode = data.CircuitCode;
                            LoginSeedCapability = data.SeedCapability;

                            UpdateLoginStatus(LoginStatus.ConnectingToSim, "Connecting to simulator...");

                            ulong handle = Utils.UIntsToLong(data.RegionX, data.RegionY);

                            if (data.SimIP != null && data.SimPort != 0)
                            {
                                // Connect to the sim given in the login reply
                                if (Connect(data.SimIP, data.SimPort, handle, true, LoginSeedCapability) != null)
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
            string mac = "";
            System.Net.NetworkInformation.NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            if (nics.Length > 0)
            {
                mac = nics[0].GetPhysicalAddress().ToString().ToUpper();
            }

            if (mac.Length < 12)
            {
                mac = mac.PadRight(12, '0');
            }

            return String.Format("{0}:{1}:{2}:{3}:{4}:{5}",
                                 mac.Substring(0, 2),
                                 mac.Substring(2, 2),
                                 mac.Substring(4, 2),
                                 mac.Substring(6, 2),
                                 mac.Substring(8, 2),
                                 mac.Substring(10, 2));
        }
    }
}
