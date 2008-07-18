/*
 * Copyright (c) 2007-2008, Second Life Reverse Engineering Team
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
using System.Threading;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using libsecondlife.StructuredData;
using libsecondlife.Capabilities;
using libsecondlife.Packets;

namespace libsecondlife
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
        public LLUUID AgentID;
        public LLUUID SessionID;
        public LLUUID SecureSessionID;
        public string FirstName;
        public string LastName;
        public string StartLocation;
        public string AgentAccess;
        public LLVector3 LookAt;
        public ulong HomeRegion;
        public LLVector3 HomePosition;
        public LLVector3 HomeLookAt;
        public uint CircuitCode;
        public uint RegionX;
        public uint RegionY;
        public ushort SimPort;
        public IPAddress SimIP;
        public string SeedCapability;
        public FriendInfo[] BuddyList;
        public DateTime SecondsSinceEpoch;
        public LLUUID InventoryRoot;
        public LLUUID LibraryRoot;
        public InventoryFolder[] InventorySkeleton;
        public InventoryFolder[] LibrarySkeleton;
        public LLUUID LibraryOwner;

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
                LookAt = ParseLLVector3("look_at", reply); 
            }
            catch (LLSDException e)
            {
                // FIXME: sometimes look_at comes back with invalid values e.g: 'look_at':'[r1,r2.0193899999999998204e-06,r0]'
                // need to handle that somehow
                Logger.DebugLog("login server returned (some) invalid data: " + e.Message);
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
                        HomeRegion = Helpers.UIntsToLong((uint)homeArray[0].AsInteger(), (uint)homeArray[1].AsInteger());
                    else
                        HomeRegion = 0;
                }

                HomePosition = ParseLLVector3("position", home);
                HomeLookAt = ParseLLVector3("look_at", home);
            }
            else
            {
                HomeRegion = 0;
                HomePosition = LLVector3.Zero;
                HomeLookAt = LLVector3.Zero;
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

            SecondsSinceEpoch = Helpers.UnixTimeToDateTime(ParseUInt("seconds_since_epoch", reply));
            InventoryRoot = ParseMappedUUID("inventory-root", "folder_id", reply);
            InventorySkeleton = ParseInventoryFolders("inventory-skeleton", AgentID, reply);
            LibraryRoot = ParseMappedUUID("inventory-lib-root", "folder_id", reply);
            LibraryOwner = ParseMappedUUID("inventory-lib-owner", "agent_id", reply);
            LibrarySkeleton = ParseInventoryFolders("inventory-skel-lib", LibraryOwner, reply);
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

        public static LLUUID ParseUUID(string key, LLSDMap reply)
        {
            LLSD llsd;
            if (reply.TryGetValue(key, out llsd))
                return llsd.AsUUID();
            else
                return LLUUID.Zero;
        }

        public static string ParseString(string key, LLSDMap reply)
        {
            LLSD llsd;
            if (reply.TryGetValue(key, out llsd))
                return llsd.AsString();
            else
                return String.Empty;
        }

        public static LLVector3 ParseLLVector3(string key, LLSDMap reply)
        {
            LLSD llsd;
            if (reply.TryGetValue(key, out llsd))
            {
                if (llsd.Type == LLSDType.Array)
                {
                    LLVector3 vec = new LLVector3();
                    vec.FromLLSD(llsd);
                    return vec;
                }
                else if (llsd.Type == LLSDType.String)
                {
                    LLSDArray array = (LLSDArray)LLSDParser.DeserializeNotation(llsd.AsString());
                    LLVector3 vec = new LLVector3();
                    vec.FromLLSD(array);
                    return vec;
                }
            }

            return LLVector3.Zero;
        }

        public static LLUUID ParseMappedUUID(string key, string key2, LLSDMap reply)
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

            return LLUUID.Zero;
        }

        public static InventoryFolder[] ParseInventoryFolders(string key, LLUUID owner, LLSDMap reply)
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

                return folders.ToArray();
            }

            return new InventoryFolder[0];
        }

        #endregion Parsing Helpers
    }

    #endregion Structs

    // TODO: Remove me when MONO can handle ServerCertificateValidationCallback
    internal class AcceptAllCertificatePolicy : ICertificatePolicy
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

    public partial class NetworkManager
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
            loginParams.Channel = userAgent + " (libsecondlife)";
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
                loginParams.Password = Helpers.MD5(loginParams.Password);

            // Override SSL authentication mechanisms. DO NOT convert this to the 
            // .NET 2.0 preferred method, the equivalent function in Mono has a 
            // different name and it will break compatibility!
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

            CapsClient loginRequest = new CapsClient(new Uri(loginParams.URI));
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

                        // Parse successful login replies in to LoginResponseData structs
                        if (loginSuccess)
                            data.Parse(map);

                        if (OnLoginResponse != null)
                        {
                            try { OnLoginResponse(loginSuccess, redirect, message, reason, data); }
                            catch (Exception ex) { Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex); }
                        }

                        if (loginSuccess && !redirect)
                        {
                            // Login succeeded

                            // These parameters are stored in NetworkManager, so instead of registering
                            // another callback for them we just set the values here
                            CircuitCode = data.CircuitCode;
                            LoginSeedCapability = data.SeedCapability;

                            UpdateLoginStatus(LoginStatus.ConnectingToSim, "Connecting to simulator...");

                            ulong handle = Helpers.UIntsToLong(data.RegionX, data.RegionY);

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
                        else if (redirect)
                        {
                            // Login redirected

                            // Make the next login URL jump
                            UpdateLoginStatus(LoginStatus.Redirecting, "Redirecting login...");

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
