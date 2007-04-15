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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System.IO;
using System.Text;
using System.Threading;
using libsecondlife.Packets;

namespace libsecondlife
{
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
        public delegate void LoginCallback(LoginStatus login);

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
            public string Major;
            /// <summary></summary>
            public string Minor;
            /// <summary></summary>
            public string Patch;
            /// <summary></summary>
            public string Build;
            /// <summary></summary>
            public string Platform;
            /// <summary></summary>
            public string MAC;
            /// <summary></summary>
            public string ViewerDigest;
            /// <summary></summary>
            public List<string> Options;
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

        /// <summary>Called any time the login status changes, will eventually
        /// return Success or Failure</summary>
        public event LoginCallback OnLogin;

        /// <summary>String holding the login message. Can be either a tip of
        /// the day style message or a description of the login error depending
        /// on whether the login was successful or not</summary>
        public string LoginMessage = String.Empty;
        /// <summary></summary>
        public string LoginErrorKey = String.Empty;
        /// <summary>Maintained for backwards compatibility</summary>
        [Obsolete("This has been broken up in to LoginErrorKey and LoginMessage", false)]
        public string LoginError { get { return String.Format("{0}: {1}", LoginErrorKey, LoginMessage); } }
        /// <summary>Seed CAPS URL returned from the login server</summary>
        public string LoginSeedCapability = String.Empty;

        private HttpWebRequest LoginRequest;
        private ManualResetEvent LoginEvent = new ManualResetEvent(false);
        private LoginParams CurrentLoginParams;
        private bool LoginSuccess = false;


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

            LoginParams loginParams = new LoginParams();

            loginParams.URI = Client.Settings.LOGIN_SERVER;
            loginParams.Timeout = Client.Settings.LOGIN_TIMEOUT;
            loginParams.MethodName = "login_to_simulator";
            loginParams.FirstName = firstName;
            loginParams.LastName = lastName;
            loginParams.Password = password;
            loginParams.Start = "last";
            loginParams.Major = "1";
            loginParams.Minor = "50";
            loginParams.Patch = "50";
            loginParams.Build = "50";
            loginParams.Platform = "Win";
            loginParams.MAC = String.Empty;
            loginParams.ViewerDigest = String.Empty;
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
            LoginEvent.Reset();

            BeginLogin(loginParams);

            LoginEvent.WaitOne(loginParams.Timeout + 1000, false);

            if (LoginErrorKey.Length == 0) LoginErrorKey = "libsl";
            if (!LoginSuccess && LoginMessage.Length == 0) LoginMessage = "Connection timed out";

            return LoginSuccess;
        }

        public void BeginLogin(LoginParams loginParams)
        {
            // Reset the login values
            LoginSuccess = false;
            CurrentLoginParams = loginParams;

            // Convert the password to MD5 if it isn't already
            if (loginParams.Password.Length != 35 && !loginParams.Password.StartsWith("$1$"))
                loginParams.Password = Helpers.MD5(loginParams.Password);

            // Set the sim disconnect timer interval
            DisconnectTimer.Interval = Client.Settings.SIMULATOR_TIMEOUT;

            // Override SSL authentication mechanisms
            ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();

            // Build the request data
            StringBuilder output = new StringBuilder(2048);

            XmlWriter login = XmlWriter.Create(output);

            login.WriteProcessingInstruction("xml", "version='1.0'");
            login.WriteStartElement("methodCall");
            login.WriteElementString("methodName", loginParams.MethodName);
            login.WriteStartElement("params");
            login.WriteStartElement("param");
            login.WriteStartElement("value");
            login.WriteStartElement("struct");

            WriteStringMember(login, "first", loginParams.FirstName);
            WriteStringMember(login, "last", loginParams.LastName);
            WriteStringMember(login, "passwd", loginParams.Password);
            WriteStringMember(login, "start", loginParams.Start);
            WriteStringMember(login, "major", loginParams.Major);
            WriteStringMember(login, "minor", loginParams.Minor);
            WriteStringMember(login, "patch", loginParams.Patch);
            WriteStringMember(login, "build", loginParams.Build);
            WriteStringMember(login, "platform", loginParams.Platform);
            WriteStringMember(login, "mac", loginParams.MAC);
            WriteStringMember(login, "agree_to_tos", "true");
            WriteStringMember(login, "read_critical", "true");
            WriteStringMember(login, "viewer_digest", loginParams.ViewerDigest);

            WriteOptionsMember(login, loginParams.Options);

            login.WriteEndElement();
            login.WriteEndElement();
            login.WriteEndElement();
            login.WriteEndElement();
            login.WriteEndElement();

            login.Close();

            byte[] bytes = Encoding.UTF8.GetBytes(output.ToString());

            LoginRequest = (HttpWebRequest)HttpWebRequest.Create(loginParams.URI);
            LoginRequest.Timeout = loginParams.Timeout;
            LoginRequest.Method = "POST";
            LoginRequest.ContentType = "text/xml";
            LoginRequest.ContentLength = bytes.Length;

            UpdateLoginStatus(LoginStatus.ConnectingToLogin);

            try
            {
                // Start the request
                LoginRequest.BeginGetRequestStream(new AsyncCallback(LoginRequestCallback), bytes);
            }
            catch (WebException e)
            {
                LoginErrorKey = "libsl";
                LoginMessage = "Error opening the login server connection: " + e.Message;
                LoginEvent.Set();

                UpdateLoginStatus(LoginStatus.Failed);
            }
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

        private void UpdateLoginStatus(LoginStatus status)
        {
            ThreadUtil.FireAndForget(OnLogin, new object[] { status });
        }

        private void LoginRequestCallback(IAsyncResult result)
        {
            try
            {
                byte[] bytes = (byte[])result.AsyncState;

                Stream output = LoginRequest.EndGetRequestStream(result);

                // Build the request
                output.Write(bytes, 0, bytes.Length);
                output.Close();

                UpdateLoginStatus(LoginStatus.ReadingResponse);

                LoginRequest.BeginGetResponse(new AsyncCallback(LoginResponseCallback), null);
            }
            catch (WebException e)
            {
                LoginErrorKey = "libsl";
                LoginMessage = "Error connecting to the login server: " + e.Message;
                LoginEvent.Set();

                UpdateLoginStatus(LoginStatus.Failed);
            }
        }

        private void LoginResponseCallback(IAsyncResult result)
        {
            LoginStatus status = LoginStatus.None;

            try
            {
                HttpWebResponse response = (HttpWebResponse)LoginRequest.EndGetResponse(result);
                XmlReader reader = XmlReader.Create(response.GetResponseStream());

                // DEBUG
                //StringBuilder output = new StringBuilder((int)response.ContentLength);
                //XmlWriterSettings settings = new XmlWriterSettings();
                //settings.Indent = true;
                //XmlWriter writer = XmlTextWriter.Create(output, settings);
                //writer.WriteNode(reader, true);
                //writer.Close();
                //Console.WriteLine(output.ToString());
                //File.WriteAllText("login.xml", output.ToString());

                // Parse the incoming xml
                bool redirect = false;
                string nextURL = String.Empty;
                string nextMethod = String.Empty;
                string name, value;
                IPAddress simIP = IPAddress.Loopback;
                ushort simPort = 0;

                reader.ReadStartElement("methodResponse");

                if (!reader.IsStartElement("fault"))
                {
                    reader.ReadStartElement("params");
                    reader.ReadStartElement("param");
                    reader.ReadStartElement("value");
                    reader.ReadStartElement("struct");

                    while (reader.IsStartElement("member"))
                    {
                        reader.ReadStartElement("member");
                        name = reader.ReadElementString("name");

                        switch (name)
                        {
                            case "login":
                                value = ReadStringValue(reader);

                                if (value == "indeterminate")
                                    redirect = true;
                                else if (value == "true")
                                    LoginSuccess = true;

                                break;
                            case "next_url":
                                nextURL = ReadStringValue(reader);
                                break;
                            case "next_method":
                                nextMethod = ReadStringValue(reader);
                                break;
                            case "reason":
                                LoginErrorKey = ReadStringValue(reader);
                                break;
                            case "agent_id":
                                LLUUID.TryParse(ReadStringValue(reader), out Client.Network.AgentID);
                                Client.Self.ID = Client.Network.AgentID;
                                break;
                            case "session_id":
                                LLUUID.TryParse(ReadStringValue(reader), out Client.Network.SessionID);
                                break;
                            case "secure_session_id":
                                LLUUID.TryParse(ReadStringValue(reader), out Client.Network.SecureSessionID);
                                break;
                            case "circuit_code":
                                Client.Network.CircuitCode = (uint)ReadIntegerValue(reader);
                                break;
                            case "first_name":
                                Client.Self.FirstName = ReadStringValue(reader).Trim(new char[] { '"' });
                                break;
                            case "last_name":
                                Client.Self.LastName = ReadStringValue(reader).Trim(new char[] { '"' });
                                break;
                            case "start_location":
                                Client.Self.StartLocation = ReadStringValue(reader);
                                break;
                            case "look_at":
                                ArrayList look_at = (ArrayList)LLSD.ParseTerseLLSD(ReadStringValue(reader));
                                Client.Self.LookAt = new LLVector3(
                                    (float)(double)look_at[0],
                                    (float)(double)look_at[1],
                                    (float)(double)look_at[2]);
                                break;
                            case "home":
                                Hashtable home = (Hashtable)LLSD.ParseTerseLLSD(ReadStringValue(reader));

                                ArrayList array = (ArrayList)home["position"];
                                Client.Self.HomePosition = new LLVector3(
                                    (float)(double)array[0],
                                    (float)(double)array[1],
                                    (float)(double)array[2]);

                                array = (ArrayList)home["look_at"];
                                Client.Self.HomeLookAt = new LLVector3(
                                    (float)(double)array[0],
                                    (float)(double)array[1],
                                    (float)(double)array[2]);
                                break;
                            case "agent_access":
                                Client.Self.AgentAccess = ReadStringValue(reader);
                                break;
                            case "message":
                                LoginMessage = ReadStringValue(reader);
                                break;
                            case "region_x":
                                //FIXME:
                                int regionX = ReadIntegerValue(reader);
                                break;
                            case "region_y":
                                // FIXME:
                                int regionY = ReadIntegerValue(reader);
                                break;
                            case "sim_port":
                                simPort = (ushort)ReadIntegerValue(reader);
                                break;
                            case "sim_ip":
                                IPAddress.TryParse(ReadStringValue(reader), out simIP);
                                break;
                            case "seconds_since_epoch":
                                uint timestamp = (uint)ReadIntegerValue(reader);
                                DateTime time = Helpers.UnixTimeToDateTime(timestamp);
                                // FIXME: ???
                                break;
                            case "seed_capability":
                                LoginSeedCapability = ReadStringValue(reader);
                                break;
                            case "inventory-root":
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("array");
                                reader.ReadStartElement("data");
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("struct");

                                ReadStringMember(reader, out name, out value);
                                LLUUID.TryParse(value, out Client.Self.InventoryRootFolderUUID);

                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                break;
                            case "inventory-lib-root":
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("array");
                                reader.ReadStartElement("data");
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("struct");

                                ReadStringMember(reader, out name, out value);
                                // FIXME:
                                //LLUUID.TryParse(value, out Client.Self.InventoryLibRootFolderUUID);

                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                break;
                            case "inventory-lib-owner":
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("array");
                                reader.ReadStartElement("data");
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("struct");

                                ReadStringMember(reader, out name, out value);
                                // FIXME:
                                //LLUUID.TryParse(value, out Client.Self.InventoryLibOwnerUUID);

                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                break;
                            case "inventory-skeleton":
                                {
                                    reader.ReadStartElement("value");
                                    reader.ReadStartElement("array");
                                    reader.ReadStartElement("data");

                                    int typeDefault, version;
                                    string invName;
                                    LLUUID folderID, parentID;

                                    while (ReadInventoryMember(reader, out typeDefault, out version, out invName,
                                        out folderID, out parentID))
                                    {
                                        // FIXME:
                                    }

                                    reader.ReadEndElement();
                                    reader.ReadEndElement();
                                    reader.ReadEndElement();
                                    break;
                                }
                            case "inventory-skel-lib":
                                {
                                    reader.ReadStartElement("value");
                                    reader.ReadStartElement("array");
                                    reader.ReadStartElement("data");

                                    int typeDefault, version;
                                    string invName;
                                    LLUUID folderID, parentID;

                                    while (ReadInventoryMember(reader, out typeDefault, out version, out invName,
                                        out folderID, out parentID))
                                    {
                                        // FIXME:
                                    }

                                    reader.ReadEndElement();
                                    reader.ReadEndElement();
                                    reader.ReadEndElement();
                                    break;
                                }
                            case "gestures":
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("array");
                                reader.ReadStartElement("data");

                                while (reader.IsStartElement("value"))
                                {
                                    reader.ReadStartElement("value");
                                    reader.ReadStartElement("struct");

                                    while (ReadStringMember(reader, out name, out value))
                                    {
                                        switch (name)
                                        {
                                            case "asset_id":
                                                // FIXME:
                                                break;
                                            case "item_id":
                                                // FIXME:
                                                break;
                                            default:
                                                Client.Log("Unhandled element in login reply (gestures)", 
                                                    Helpers.LogLevel.Error);
                                                reader.Skip();
                                                break;
                                        }

                                        // FIXME:
                                    }

                                    reader.ReadEndElement();
                                    reader.ReadEndElement();
                                }

                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                break;
                            case "event_categories":
                                {
                                    reader.ReadStartElement("value");
                                    reader.ReadStartElement("array");
                                    reader.ReadStartElement("data");

                                    int categoryID;
                                    string categoryName;

                                    while (ReadCategoryMember(Client, reader, out categoryID, out categoryName))
                                    {
                                        // FIXME:
                                    }

                                    reader.ReadEndElement();
                                    reader.ReadEndElement();
                                    reader.ReadEndElement();
                                    break;
                                }
                            case "classified_categories":
                                {
                                    reader.ReadStartElement("value");
                                    reader.ReadStartElement("array");
                                    reader.ReadStartElement("data");

                                    int categoryID;
                                    string categoryName;

                                    while (ReadCategoryMember(Client, reader, out categoryID, out categoryName))
                                    {
                                        // FIXME:
                                    }

                                    reader.ReadEndElement();
                                    reader.ReadEndElement();
                                    reader.ReadEndElement();
                                    break;
                                }
                            case "event_notifications":
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("array");
                                reader.ReadStartElement("data");

                                // FIXME:

                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                break;
                            case "buddy-list":
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("array");
                                reader.ReadStartElement("data");

                                int buddyRightsGiven, buddyRightsHas;
                                LLUUID buddyID;

                                while (ReadBuddyMember(Client, reader, out buddyRightsGiven, out buddyRightsHas, 
                                    out buddyID))
                                {
                                    // FIXME:
                                }

                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                break;
                            case "ui-config":
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("array");
                                reader.ReadStartElement("data");
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("struct");

                                while (ReadStringMember(reader, out name, out value))
                                {
                                    switch (name)
                                    {
                                        case "allow_first_life":
                                            // FIXME:
                                            break;
                                        default:
                                            Client.Log("Unhandled element in login reply (ui-config)", 
                                                Helpers.LogLevel.Error);
                                            reader.Skip();
                                            break;
                                    }
                                }

                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                break;
                            case "login-flags":
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("array");
                                reader.ReadStartElement("data");
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("struct");

                                while (ReadStringMember(reader, out name, out value))
                                {
                                    switch (name)
                                    {
                                        case "ever_logged_in":
                                            // FIXME:
                                            break;
                                        case "daylight_savings":
                                            // FIXME:
                                            break;
                                        case "stipend_since_login":
                                            // FIXME:
                                            break;
                                        case "gendered":
                                            // FIXME:
                                            break;
                                        default:
                                            Client.Log("Unhandled element in login reply (login-flags)", 
                                                Helpers.LogLevel.Error);
                                            reader.Skip();
                                            break;
                                    }
                                }

                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                break;
                            case "global-textures":
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("array");
                                reader.ReadStartElement("data");
                                reader.ReadStartElement("value");
                                reader.ReadStartElement("struct");

                                while (ReadStringMember(reader, out name, out value))
                                {
                                    switch (name)
                                    {
                                        case "cloud_texture_id":
                                            // FIXME:
                                            break;
                                        case "sun_texture_id":
                                            // FIXME:
                                            break;
                                        case "moon_texture_id":
                                            // FIXME:
                                            break;
                                        default:
                                            Client.Log("Unhandled element in login reply (global-textures)", 
                                                Helpers.LogLevel.Error);
                                            reader.Skip();
                                            break;
                                    }
                                }

                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                reader.ReadEndElement();
                                break;
                            default:
                                Client.Log("Unhandled element in login reply", Helpers.LogLevel.Error);
                                reader.Skip();
                                break;
                        }

                        reader.ReadEndElement();
                    }

                    reader.ReadEndElement();
                    reader.ReadEndElement();
                    reader.ReadEndElement();
                    reader.ReadEndElement();


                    if (redirect)
                    {
                        UpdateLoginStatus(LoginStatus.Redirecting);

                        // Handle indeterminate logins
                        CurrentLoginParams.URI = nextURL;
                        CurrentLoginParams.MethodName = nextMethod;
                        BeginLogin(CurrentLoginParams);
                        return;
                    }
                    else if (LoginSuccess)
                    {
                        UpdateLoginStatus(LoginStatus.ConnectingToSim);

                        // Connect to the sim given in the login reply
                        if (Connect(simIP, simPort, true, LoginSeedCapability) != null)
                        {
                            // Request the economy data right after login
                            SendPacket(new EconomyDataRequestPacket());

                            status = LoginStatus.Success;

                            // Fire an event for connecting to the grid
                            if (OnConnected != null)
                            {
                                try { OnConnected(this.Client); }
                                catch (Exception e) { Client.Log(e.ToString(), Helpers.LogLevel.Error); }
                            }
                        }
                        else
                        {
                            LoginSuccess = false;
                            LoginErrorKey = "libsl";
                            LoginMessage = "Unable to connect to the simulator";
                            status = LoginStatus.Failed;
                        }
                    }
                    else
                    {
                        LoginSuccess = false;
                        status = LoginStatus.Failed;
                    }
                }
                else
                {
                    reader.ReadStartElement("fault");
                    reader.ReadStartElement("value");
                    reader.ReadStartElement("struct");

                    ReadStringMember(reader, out name, out value);

                    Client.Network.LoginMessage = value;
                }

                reader.Close();
                response.Close();
            }
            catch (WebException e)
            {
                LoginErrorKey = "libsl";
                LoginMessage = "Error reading response: " + e.Message;
                status = LoginStatus.Failed;
            }
            catch (XmlException e)
            {
                LoginErrorKey = "libsl";
                LoginMessage = "Error parsing reply XML: " + e.Message + Environment.NewLine + e.StackTrace;
                status = LoginStatus.Failed;
            }

            LoginEvent.Set();
            UpdateLoginStatus(status);
        }

        private static string ReadStringValue(XmlReader reader)
        {
            reader.ReadStartElement("value");
            string value = reader.ReadElementString("string");
            reader.ReadEndElement();
            return value;
        }

        private static int ReadIntegerValue(XmlReader reader)
        {
            reader.ReadStartElement("value");
            int value;
            Int32.TryParse(reader.ReadElementString("i4"), out value);
            reader.ReadEndElement();
            return value;
        }

        private static bool ReadStringMember(XmlReader reader, out string name, out string value)
        {
            if (reader.IsStartElement("member"))
            {
                reader.ReadStartElement("member");
                name = reader.ReadElementString("name");
                reader.ReadStartElement("value");
                value = reader.ReadElementString("string");
                reader.ReadEndElement();
                reader.ReadEndElement();

                return true;
            }
            else
            {
                name = String.Empty;
                value = String.Empty;
                return false;
            }
        }

        private static bool ReadInventoryMember(XmlReader reader, out int typeDefault, out int version,
            out string invName, out LLUUID folderID, out LLUUID parentID)
        {
            typeDefault = 0;
            version = 0;
            invName = String.Empty;
            folderID = LLUUID.Zero;
            parentID = LLUUID.Zero;
            bool ret = false;

            if (reader.IsStartElement("value"))
            {
                reader.ReadStartElement("value");

                if (reader.IsStartElement("struct"))
                {
                    reader.ReadStartElement("struct");

                    string name;

                    while (reader.IsStartElement("member"))
                    {
                        reader.ReadStartElement("member");
                        name = reader.ReadElementString("name");

                        switch (name)
                        {
                            case "type_default":
                                typeDefault = ReadIntegerValue(reader);
                                break;
                            case "version":
                                version = ReadIntegerValue(reader);
                                break;
                            case "name":
                                invName = ReadStringValue(reader);
                                break;
                            case "folder_id":
                                string folder = ReadStringValue(reader);
                                LLUUID.TryParse(folder, out folderID);
                                break;
                            case "parent_id":
                                string parent = ReadStringValue(reader);
                                LLUUID.TryParse(parent, out parentID);
                                break;
                            default:
                                ;
                                break;
                        }

                        reader.ReadEndElement();
                    }

                    reader.ReadEndElement();
                    ret = true;
                }

                reader.ReadEndElement();
            }

            return ret;
        }

        private static bool ReadCategoryMember(SecondLife client, XmlReader reader, out int categoryID, 
            out string categoryName)
        {
            categoryID = 0;
            categoryName = String.Empty;
            bool ret = false;

            if (reader.IsStartElement("value"))
            {
                reader.ReadStartElement("value");

                if (reader.IsStartElement("struct"))
                {
                    reader.ReadStartElement("struct");

                    string name;

                    while (reader.IsStartElement("member"))
                    {
                        reader.ReadStartElement("member");
                        name = reader.ReadElementString("name");

                        switch (name)
                        {
                            case "category_id":
                                categoryID = ReadIntegerValue(reader);
                                break;
                            case "category_name":
                                categoryName = ReadStringValue(reader);
                                break;
                            default:
                                client.Log("Unhandled element in login reply (CategoryMember)", 
                                    Helpers.LogLevel.Error);
                                reader.Skip();
                                break;
                        }

                        reader.ReadEndElement();
                    }

                    reader.ReadEndElement();
                    ret = true;
                }

                reader.ReadEndElement();
            }

            return ret;
        }

        private static bool ReadBuddyMember(SecondLife client, XmlReader reader, out int buddyRightsGiven, 
            out int buddyRightsHas, out LLUUID buddyID)
        {
            buddyRightsGiven = 0;
            buddyRightsHas = 0;
            buddyID = LLUUID.Zero;
            bool ret = false;

            if (reader.IsStartElement("value"))
            {
                reader.ReadStartElement("value");

                if (reader.IsStartElement("struct"))
                {
                    reader.ReadStartElement("struct");

                    string name;

                    while (reader.IsStartElement("member"))
                    {
                        reader.ReadStartElement("member");
                        name = reader.ReadElementString("name");

                        switch (name)
                        {
                            case "buddy_rights_given":
                                buddyRightsGiven = ReadIntegerValue(reader);
                                break;
                            case "buddy_id":
                                string buddy = ReadStringValue(reader);
                                LLUUID.TryParse(buddy, out buddyID);
                                break;
                            case "buddy_rights_has":
                                buddyRightsHas = ReadIntegerValue(reader);
                                break;
                            default:
                                client.Log("Unhandled element in login reply (BuddyMember)", 
                                    Helpers.LogLevel.Error);
                                reader.Skip();
                                break;
                        }

                        reader.ReadEndElement();
                    }

                    reader.ReadEndElement();
                    ret = true;
                }

                reader.ReadEndElement();
            }

            return ret;
        }

        private static void WriteStringMember(XmlWriter writer, string name, string value)
        {
            writer.WriteStartElement("member");
            writer.WriteElementString("name", name);
            writer.WriteStartElement("value");
            writer.WriteElementString("string", value);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private static void WriteOptionsMember(XmlWriter writer, List<string> options)
        {
            writer.WriteStartElement("member");
            writer.WriteElementString("name", "options");
            writer.WriteStartElement("value");
            writer.WriteStartElement("array");
            writer.WriteStartElement("data");

            for (int i = 0; i < options.Count; i++)
            {
                writer.WriteStartElement("value");
                writer.WriteElementString("string", options[i]);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}
