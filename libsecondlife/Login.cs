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
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using libsecondlife.LLSD;
using libsecondlife.Packets;
using CookComputing.XmlRpc;

namespace libsecondlife
{
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginSuccess"></param>
        /// <param name="redirect"></param>
        /// <param name="simIP"></param>
        /// <param name="simPort"></param>
        /// <param name="regionX"></param>
        /// <param name="regionY"></param>
        /// <param name="reason"></param>
        /// <param name="message"></param>
        [Obsolete("Use LoginResponseCallback instead.")]
        public delegate void LoginReplyCallback(bool loginSuccess, bool redirect, IPAddress simIP, int simPort,
            uint regionX, uint regionY, string reason, string message);
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
            public string UserAgent;
            /// <summary></summary>
            public string Author;
            /// <summary></summary>
            public List<string> Options;
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
            public LLVector3 HomePosition;
            public LLVector3 HomeLookAt;
            public uint CircuitCode;
            public uint RegionX;
            public uint RegionY;
            public ushort SimPort;
            public IPAddress SimIP;
            public string SeedCapability;
            public FriendsManager.FriendInfo[] BuddyList;
            public DateTime SecondsSinceEpoch;
            public LLUUID InventoryRoot;
            public LLUUID LibraryRoot;
            public InventoryFolder[] InventorySkeleton;
            public InventoryFolder[] LibrarySkeleton;
            public LLUUID LibraryOwner;

            public void Parse(LoginMethodResponse reply)
            {
                AgentID = LLUUID.Parse(reply.agent_id);
                SessionID = LLUUID.Parse(reply.session_id);
                SecureSessionID = LLUUID.Parse(reply.secure_session_id);
                FirstName = reply.first_name;
                LastName = reply.last_name;
                StartLocation = reply.start_location;
                AgentAccess = reply.agent_access;

                List<object> look_at = (List<object>)LLSDParser.DeserializeNotation(reply.look_at);
                LookAt = new LLVector3(
                    (float)(double)look_at[0],
                    (float)(double)look_at[1],
                    (float)(double)look_at[2]);

                if (reply.home != null)
                {
                    Dictionary<string, object> home = (Dictionary<string, object>)LLSDParser.DeserializeNotation(reply.home);
                    List<object> array = (List<object>)home["position"];
                    HomePosition = new LLVector3(
                        (float)(double)array[0],
                        (float)(double)array[1],
                        (float)(double)array[2]);

                    array = (List<object>)home["look_at"];
                    HomeLookAt = new LLVector3(
                        (float)(double)array[0],
                        (float)(double)array[1],
                        (float)(double)array[2]);
                }

                CircuitCode = (uint)reply.circuit_code;
                RegionX = (uint)reply.region_x;
                RegionY = (uint)reply.region_y;
                SimPort = (ushort)reply.sim_port;
                SimIP = IPAddress.Parse(reply.sim_ip);
                SeedCapability = reply.seed_capability;

                if (reply.buddy_list != null)
                {
                    BuddyList = new FriendsManager.FriendInfo[reply.buddy_list.Length];
                    for (int i = 0; i < BuddyList.Length; ++i)
                    {
                        BuddyListEntry buddy = reply.buddy_list[i];
                        BuddyList[i] = new FriendsManager.FriendInfo(buddy.buddy_id, (FriendsManager.RightsFlags)buddy.buddy_rights_given,
                                (FriendsManager.RightsFlags)buddy.buddy_rights_has);
                    }
                }
                else
                {
                    BuddyList = new FriendsManager.FriendInfo[0];
                }

                InventoryRoot = LLUUID.Parse(reply.inventory_root[0].folder_id);
                LibraryRoot = LLUUID.Parse(reply.inventory_lib_root[0].folder_id);
                LibraryOwner = LLUUID.Parse(reply.inventory_lib_owner[0].agent_id);
                InventorySkeleton = ParseSkeleton(reply.inventory_skeleton, AgentID);
                LibrarySkeleton = ParseSkeleton(reply.inventory_skel_lib, LibraryOwner);
            }

            public InventoryFolder[] ParseSkeleton(InventorySkeletonEntry[] skeleton, LLUUID owner)
            {
                Dictionary<LLUUID, InventoryFolder> Folders = new Dictionary<LLUUID, InventoryFolder>();
                Dictionary<LLUUID, List<InventoryFolder>> FoldersChildren = new Dictionary<LLUUID, List<InventoryFolder>>(skeleton.Length);

                foreach (InventorySkeletonEntry entry in skeleton)
                {
                    InventoryFolder folder = new InventoryFolder(entry.folder_id);
                    if (entry.type_default != -1)
                        folder.PreferredType = (AssetType)entry.type_default;
                    folder.Version = entry.version;
                    folder.OwnerID = owner;
                    folder.ParentUUID = LLUUID.Parse(entry.parent_id);
                    folder.Name = entry.name;
                    Folders.Add(entry.folder_id, folder);

                    if (entry.parent_id != LLUUID.Zero)
                    {
                        List<InventoryFolder> parentChildren;
                        if (!FoldersChildren.TryGetValue(entry.parent_id, out parentChildren))
                        {
                            parentChildren = new List<InventoryFolder>();
                            FoldersChildren.Add(entry.parent_id, parentChildren);
                        }
                        parentChildren.Add(folder);
                    }
                }

                foreach (KeyValuePair<LLUUID, List<InventoryFolder>> pair in FoldersChildren)
                {
                    if (Folders.ContainsKey(pair.Key))
                    {
                        InventoryFolder parentFolder = Folders[pair.Key];
                        parentFolder.DescendentCount = pair.Value.Count; // Should we set this here? it's just the folders, not the items!
                    }
                }

                // Should we do this or just return an IEnumerable?
                InventoryFolder[] ret = new InventoryFolder[Folders.Count];
                int index = 0;
                foreach (InventoryFolder folder in Folders.Values)
                {
                    ret[index] = folder;
                    ++index;
                }
                return ret;
            }
        }

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

        /// <summary>Called when a reply is received from the login server, the
        /// login sequence will block until this event returns</summary>
        [Obsolete("Use RegisterLoginResponse instead.")]
        public event LoginReplyCallback OnLoginReply;

        /// <summary>Called any time the login status changes, will eventually
        /// return LoginStatus.Success or LoginStatus.Failure</summary>
        public event LoginCallback OnLogin;

        /// <summary>Called when a reply is received from the login server, the
        /// login sequence will block until this event returns</summary>
        private event LoginResponseCallback OnLoginResponse;

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

        private class LoginContext
        {
            public LoginParams Params;
        }

        private LoginContext CurrentContext = null;
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
        /// <param name="userAgent">Client application name and version</param>
        /// <param name="author">Client application author</param>
        /// <returns></returns>
        public LoginParams DefaultLoginParams(string firstName, string lastName, string password,
            string userAgent, string author)
        {
            List<string> options = new List<string>();
            options.Add("inventory-root");
            options.Add("inventory-skeleton");
            options.Add("inventory-lib-root");
            options.Add("inventory-lib-owner");
            options.Add("inventory-skel-lib");
            options.Add("gestures");
            options.Add("event_categories");
            options.Add("event_notifications");
            options.Add("classified_categories");
            options.Add("buddy-list");
            options.Add("ui-config");
            options.Add("login-flags");
            options.Add("global-textures");
            // initial-outfit?

            LoginParams loginParams = new LoginParams();

            loginParams.URI = Client.Settings.LOGIN_SERVER;
            loginParams.Timeout = Client.Settings.LOGIN_TIMEOUT;
            loginParams.MethodName = "login_to_simulator";
            loginParams.FirstName = firstName;
            loginParams.LastName = lastName;
            loginParams.Password = password;
            loginParams.Start = "last";
            loginParams.Channel = "libsecondlife";
            loginParams.Version = Client.Settings.VERSION;
            loginParams.Platform = "Win";
            loginParams.MAC = String.Empty;
            loginParams.ViewerDigest = String.Empty;
            loginParams.UserAgent = userAgent;
            loginParams.Author = author;
            loginParams.Options = options;

            return loginParams;
        }

        /// <summary>
        /// Simplified login that takes the most common and required fields
        /// </summary>
        /// <param name="firstName">Account first name</param>
        /// <param name="lastName">Account last name</param>
        /// <param name="password">Account password</param>
        /// <param name="userAgent">Client application name and version</param>
        /// <param name="author">Client application author</param>
        /// <returns>Whether the login was successful or not. On failure the
        /// LoginErrorKey string will contain the error code and LoginMessage
        /// will contain a description of the error</returns>
        public bool Login(string firstName, string lastName, string password, string userAgent, string author)
        {
            return Login(firstName, lastName, password, userAgent, "last", author);
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
        /// <param name="userAgent">Client application name and version</param>
        /// <param name="start">Starting location URI that can be built with
        /// StartLocation()</param>
        /// <param name="author">Client application author</param>
        /// <returns>Whether the login was successful or not. On failure the
        /// LoginErrorKey string will contain the error code and LoginMessage
        /// will contain a description of the error</returns>
        public bool Login(string firstName, string lastName, string password, string userAgent, string start,
            string author)
        {
            LoginParams loginParams = DefaultLoginParams(firstName, lastName, password, userAgent, author);
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

        private void BeginLogin()
        {
            // Sanity check some of the parameters
            if (CurrentContext.Params.ViewerDigest == null)
                CurrentContext.Params.ViewerDigest = String.Empty;
            if (CurrentContext.Params.Version == null)
                CurrentContext.Params.Version = String.Empty;
            if (CurrentContext.Params.UserAgent == null)
                CurrentContext.Params.UserAgent = String.Empty;
            if (CurrentContext.Params.Platform == null)
                CurrentContext.Params.Platform = String.Empty;
            if (CurrentContext.Params.Options == null)
                CurrentContext.Params.Options = new List<string>();
            if (CurrentContext.Params.MAC == null)
                CurrentContext.Params.MAC = String.Empty;
            if (CurrentContext.Params.Channel == null)
                CurrentContext.Params.Channel = String.Empty;

            // Convert the password to MD5 if it isn't already
            if (CurrentContext.Params.Password.Length != 35 && !CurrentContext.Params.Password.StartsWith("$1$"))
                CurrentContext.Params.Password = Helpers.MD5(CurrentContext.Params.Password);

            // Set the sim disconnect timer interval
            DisconnectTimer.Interval = Client.Settings.SIMULATOR_TIMEOUT;

            // Override SSL authentication mechanisms. DO NOT convert this to the 
            // .NET 2.0 preferred method, the equivalent function in Mono has a 
            // different name and it will break compatibility!
            ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
            // TODO: At some point, maybe we should check the cert?

            LoginMethodParams loginParams;
            loginParams.first = CurrentContext.Params.FirstName;
            loginParams.last = CurrentContext.Params.LastName;
            loginParams.passwd = CurrentContext.Params.Password;
            loginParams.start = CurrentContext.Params.Start;
            loginParams.channel = CurrentContext.Params.Channel;
            loginParams.version = CurrentContext.Params.Version;
            loginParams.platform = CurrentContext.Params.Platform;
            loginParams.mac = CurrentContext.Params.MAC;
            loginParams.user_agent = CurrentContext.Params.UserAgent;
            loginParams.author = CurrentContext.Params.Author;
            loginParams.agree_to_tos = "true";
            loginParams.read_critical = "true";
            loginParams.viewer_digest = CurrentContext.Params.ViewerDigest;

            List<string> options = new List<string>(CurrentContext.Params.Options.Count + CallbackOptions.Values.Count);
            options.AddRange(CurrentContext.Params.Options);
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
                proxy.Url = CurrentContext.Params.URI;
                proxy.XmlRpcMethod = CurrentContext.Params.MethodName;

                // Start the request
                proxy.BeginLoginToSimulator(loginParams, new AsyncCallback(LoginMethodCallback), new object[] { proxy, CurrentContext });
            }
            catch (Exception e)
            {
                UpdateLoginStatus(LoginStatus.Failed, "Error opening the login server connection: " + e);
            }
        }

        public void BeginLogin(LoginParams loginParams)
        {
            if (CurrentContext != null)
                throw new Exception("Login already in progress");

            LoginEvent.Reset();

            CurrentContext = new LoginContext();
            CurrentContext.Params = loginParams;

            BeginLogin();
        }

        public void RegisterLoginResponseCallback(LoginResponseCallback callback)
        {
            RegisterLoginResponseCallback(callback, null);
        }

        public void RegisterLoginResponseCallback(LoginResponseCallback callback, string[] options) {
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

        private void UpdateLoginStatus(LoginStatus status, string message)
        {
            InternalStatusCode = status;
            InternalLoginMessage = message;

            Client.DebugLog("Login status: " + status.ToString() + ": " + message);

            if (OnLogin != null)
            {
                try { OnLogin(status, message); }
                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
            }

            if (status == LoginStatus.Success || status == LoginStatus.Failed)
            {
                CurrentContext = null;
                LoginEvent.Set();
            }
        }

        private void proxy_ResponseEvent(object sender, XmlRpcResponseEventArgs args)
        {
            TextReader reader = new StreamReader(args.ResponseStream);
            InternalRawLoginReply = reader.ReadToEnd();
        }

        private void LoginMethodCallback(IAsyncResult result)
        {
            object[] asyncState = result.AsyncState as object[];
            ILoginProxy proxy = asyncState[0] as ILoginProxy;
            LoginContext context = asyncState[1] as LoginContext;
            XmlRpcAsyncResult clientResult = result as XmlRpcAsyncResult;
            LoginMethodResponse reply;
            IPAddress simIP = IPAddress.Any; // Temporary
            ushort simPort = 0;
            uint regionX = 0;
            uint regionY = 0;
            bool loginSuccess = false;

            // Fetch the login response
            try
            {
                reply = proxy.EndLoginToSimulator(clientResult);
                if (context != CurrentContext)
                    return;
            }
            catch (Exception)
            {
                UpdateLoginStatus(LoginStatus.Failed, "Error retrieving the login response from the server");
                return;
            }

            string reason = reply.reason;
            string message = reply.message;

            if (reply.login == "true")
            {
                loginSuccess = true;

                // FIXME: No information should be set here, everything can take care of itself
                // through login reply handlers

                // Remove the quotes around our first name.
                if (reply.first_name[0] == '"')
                    reply.first_name = reply.first_name.Remove(0, 1);
                if (reply.first_name[reply.first_name.Length - 1] == '"')
                    reply.first_name = reply.first_name.Remove(reply.first_name.Length - 1);

                #region Critical Information

                try
                {
                    // Networking
                    Client.Network.CircuitCode = (uint)reply.circuit_code;
                    regionX = (uint)reply.region_x;
                    regionY = (uint)reply.region_y;
                    simPort = (ushort)reply.sim_port;
                    IPAddress.TryParse(reply.sim_ip, out simIP);
                    LoginSeedCapability = reply.seed_capability;
                }
                catch (Exception)
                {
                    UpdateLoginStatus(LoginStatus.Failed, "Login server failed to return critical information");
                    return;
                }

                #endregion Critical Information

                // Buddies:
                if (reply.buddy_list != null)
                {
                    foreach (BuddyListEntry buddy in reply.buddy_list)
                    {
                        Client.Friends.AddFriend(buddy.buddy_id, (FriendsManager.RightsFlags)buddy.buddy_rights_given,
                            (FriendsManager.RightsFlags)buddy.buddy_rights_has);
                    }
                }

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

            // Fire the client handler
            if (OnLoginReply != null)
            {
                try { OnLoginReply(loginSuccess, redirect, simIP, simPort, regionX, regionY, reason, message); }
                catch (Exception ex) { Client.Log(ex.ToString(), Helpers.LogLevel.Error); }
            }

            try
            {
                if (OnLoginResponse != null)
                {
                    LoginResponseData data = new LoginResponseData();
                    if (loginSuccess)
                    {
                        data.Parse(reply);
                    }
                    try { OnLoginResponse(loginSuccess, redirect, message, reason, data); }
                    catch (Exception ex) { Client.Log(ex.ToString(), Helpers.LogLevel.Error); }
                }
            }
            catch (Exception ex) { Client.Log(ex.ToString(), Helpers.LogLevel.Error); }

            // Make the next network jump, if needed
            if (redirect)
            {
                UpdateLoginStatus(LoginStatus.Redirecting, "Redirecting login...");

                // Handle indeterminate logins
                CurrentContext.Params.URI = reply.next_url;
                CurrentContext.Params.MethodName = reply.next_method;

                // Ignore next_options and next_duration for now
                BeginLogin();
            }
            else if (loginSuccess)
            {
                UpdateLoginStatus(LoginStatus.ConnectingToSim, "Connecting to simulator...");

                ulong handle = Helpers.UIntsToLong(regionX, regionY);

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
                        catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
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

        public interface ILoginProxy : IXmlRpcProxy
        {
            [XmlRpcMethod("login_to_simulator")]
            LoginMethodResponse LoginToSimulator(LoginMethodParams loginParams);

            [XmlRpcBegin("login_to_simulator")]
            IAsyncResult BeginLoginToSimulator(LoginMethodParams loginParams);

            [XmlRpcBegin("login_to_simulator")]
            IAsyncResult BeginLoginToSimulator(LoginMethodParams loginParams, AsyncCallback callback);

            [XmlRpcBegin("login_to_simulator")]
            IAsyncResult BeginLoginToSimulator(LoginMethodParams loginParams, AsyncCallback callback, object asyncState);

            [XmlRpcEnd("login_to_simulator")]
            LoginMethodResponse EndLoginToSimulator(IAsyncResult result);
        }

        #region XML-RPC structs

        public struct LoginMethodParams
        {
            public string first;
            public string last;
            public string passwd;
            public string start;
            public string channel;
            public string version;
            public string platform;
            public string mac;
            [XmlRpcMember("user-agent")]
            public string user_agent;
            public string author;
            public string agree_to_tos;
            public string read_critical;
            public string viewer_digest;
            public string[] options;
        }

        public struct LoginMethodResponse
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

        public struct InventoryRootEntry
        {
            public string folder_id;
        }

        public struct InventorySkeletonEntry
        {
            public int type_default;
            public int version;
            public string name;
            public string folder_id;
            public string parent_id;
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

        public struct OutfitEntry
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

        #endregion
    }
}
