/*
 * Copyright (c) 2006-2014, openmetaverse.org
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
using System.Text;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Http;

namespace OpenMetaverse
{
    public class RegistrationApi
    {
        const int REQUEST_TIMEOUT = 1000 * 100;

        private struct UserInfo
        {
            public string FirstName;
            public string LastName;
            public string Password;
        }

        private struct RegistrationCaps
        {
            public Uri CreateUser;
            public Uri CheckName;
            public Uri GetLastNames;
            public Uri GetErrorCodes;
        }

        public struct LastName
        {
            public int ID;
            public string Name;
        }

        /// <summary>
        /// See https://secure-web6.secondlife.com/developers/third_party_reg/#service_create_user or
        /// https://wiki.secondlife.com/wiki/RegAPIDoc for description
        /// </summary>
        public class CreateUserParam
        {
            public string FirstName;
            public LastName LastName;
            public string Email;
            public string Password;
            public DateTime Birthdate;

            // optional:
            public Nullable<int> LimitedToEstate;
            public string StartRegionName;
            public Nullable<Vector3> StartLocation;
            public Nullable<Vector3> StartLookAt;
        }

        private UserInfo _userInfo;
        private RegistrationCaps _caps;
        private int _initializing;
        private List<LastName> _lastNames = new List<LastName>();
        private Dictionary<int, string> _errors = new Dictionary<int, string>();

        public bool Initializing
        {
            get
            {
                System.Diagnostics.Debug.Assert(_initializing <= 0);
                return (_initializing < 0);
            }
        }

        public List<LastName> LastNames
        {
            get
            {
                lock (_lastNames)
                {
                    if (_lastNames.Count <= 0)
                        GatherLastNames();
                }

                return _lastNames;
            }
        }

        public RegistrationApi(string firstName, string lastName, string password)
        {
            _initializing = -2;

            _userInfo = new UserInfo();

            _userInfo.FirstName = firstName;
            _userInfo.LastName = lastName;
            _userInfo.Password = password;

            GatherCaps();
        }

        public void WaitForInitialization()
        {
            while (Initializing)
                System.Threading.Thread.Sleep(10);
        }

        public Uri RegistrationApiCaps
        {
            get { return new Uri("https://cap.secondlife.com/get_reg_capabilities"); }
        }

        private void GatherCaps()
        {
            // build post data
            byte[] postData = Encoding.ASCII.GetBytes(
                String.Format("first_name={0}&last_name={1}&password={2}", _userInfo.FirstName, _userInfo.LastName, 
                _userInfo.Password));

            CapsClient request = new CapsClient(RegistrationApiCaps);
            request.OnComplete += new CapsClient.CompleteCallback(GatherCapsResponse);
            request.BeginGetResponse(postData, "application/x-www-form-urlencoded", REQUEST_TIMEOUT);
        }

        private void GatherCapsResponse(CapsClient client, OSD response, Exception error)
        {
            if (response is OSDMap)
            {
                OSDMap respTable = (OSDMap)response;

                // parse
                _caps = new RegistrationCaps();

                _caps.CreateUser = respTable["create_user"].AsUri();
                _caps.CheckName = respTable["check_name"].AsUri();
                _caps.GetLastNames = respTable["get_last_names"].AsUri();
                _caps.GetErrorCodes = respTable["get_error_codes"].AsUri();

                // finalize
                _initializing++;

                GatherErrorMessages();
            }
        }

        private void GatherErrorMessages()
        {
            if (_caps.GetErrorCodes == null)
                throw new InvalidOperationException("access denied");	// this should work even for not-approved users

            CapsClient request = new CapsClient(_caps.GetErrorCodes);
            request.OnComplete += new CapsClient.CompleteCallback(GatherErrorMessagesResponse);
            request.BeginGetResponse(REQUEST_TIMEOUT);
        }

        private void GatherErrorMessagesResponse(CapsClient client, OSD response, Exception error)
        {
            if (response is OSDMap)
            {
                // parse

                //FIXME: wtf?
                //foreach (KeyValuePair<string, object> error in (Dictionary<string, object>)response)
                //{
                    //StringBuilder sb = new StringBuilder();

                    //sb.Append(error[1]);
                    //sb.Append(" (");
                    //sb.Append(error[0]);
                    //sb.Append("): ");
                    //sb.Append(error[2]);

                    //_errors.Add((int)error[0], sb.ToString());
                //}

                // finalize
                _initializing++;
            }
        }

        public void GatherLastNames()
        {
            if (Initializing)
                throw new InvalidOperationException("still initializing");

            if (_caps.GetLastNames == null)
                throw new InvalidOperationException("access denied: only approved developers have access to the registration api");

            CapsClient request = new CapsClient(_caps.GetLastNames);
            request.OnComplete += new CapsClient.CompleteCallback(GatherLastNamesResponse);
            request.BeginGetResponse(REQUEST_TIMEOUT);

            // FIXME: Block
        }

        private void GatherLastNamesResponse(CapsClient client, OSD response, Exception error)
        {
            if (response is OSDMap)
            {
                //LLSDMap respTable = (LLSDMap)response;

                //FIXME:
                //_lastNames = new List<LastName>(respTable.Count);

                //for (Dictionary<string, object>.Enumerator it = respTable.GetEnumerator(); it.MoveNext(); )
                //{
                //    LastName ln = new LastName();

                //    ln.ID = int.Parse(it.Current.Key.ToString());
                //    ln.Name = it.Current.Value.ToString();

                //    _lastNames.Add(ln);
                //}

                //_lastNames.Sort(new Comparison<LastName>(delegate(LastName a, LastName b) { return a.Name.CompareTo(b.Name); }));
            }
        }

        public bool CheckName(string firstName, LastName lastName)
        {
            if (Initializing)
                throw new InvalidOperationException("still initializing");

            if (_caps.CheckName == null)
                throw new InvalidOperationException("access denied; only approved developers have access to the registration api");

            // Create the POST data
            OSDMap query = new OSDMap();
            query.Add("username", OSD.FromString(firstName));
            query.Add("last_name_id", OSD.FromInteger(lastName.ID));
            //byte[] postData = OSDParser.SerializeXmlBytes(query);

            CapsClient request = new CapsClient(_caps.CheckName);
            request.OnComplete += new CapsClient.CompleteCallback(CheckNameResponse);
            request.BeginGetResponse(REQUEST_TIMEOUT);

            // FIXME:
            return false;
        }

        private void CheckNameResponse(CapsClient client, OSD response, Exception error)
        {
            if (response.Type == OSDType.Boolean)
            {
                // FIXME:
                //(bool)response;
            }
            else
            {
                // FIXME:
            }
        }

        /// <summary>
        /// Returns the new user ID or throws an exception containing the error code
        /// The error codes can be found here: https://wiki.secondlife.com/wiki/RegAPIError
        /// </summary>
        /// <param name="user">New user account to create</param>
        /// <returns>The UUID of the new user account</returns>
        public UUID CreateUser(CreateUserParam user)
        {
            if (Initializing)
                throw new InvalidOperationException("still initializing");

            if (_caps.CreateUser == null)
                throw new InvalidOperationException("access denied; only approved developers have access to the registration api");

            // Create the POST data
            OSDMap query = new OSDMap();
            query.Add("username", OSD.FromString(user.FirstName));
            query.Add("last_name_id", OSD.FromInteger(user.LastName.ID));
            query.Add("email", OSD.FromString(user.Email));
            query.Add("password", OSD.FromString(user.Password));
            query.Add("dob", OSD.FromString(user.Birthdate.ToString("yyyy-MM-dd")));

            if (user.LimitedToEstate != null)
                query.Add("limited_to_estate", OSD.FromInteger(user.LimitedToEstate.Value));

            if (!string.IsNullOrEmpty(user.StartRegionName))
                query.Add("start_region_name", OSD.FromInteger(user.LimitedToEstate.Value));

            if (user.StartLocation != null)
            {
                query.Add("start_local_x", OSD.FromReal(user.StartLocation.Value.X));
                query.Add("start_local_y", OSD.FromReal(user.StartLocation.Value.Y));
                query.Add("start_local_z", OSD.FromReal(user.StartLocation.Value.Z));
            }

            if (user.StartLookAt != null)
            {
                query.Add("start_look_at_x", OSD.FromReal(user.StartLookAt.Value.X));
                query.Add("start_look_at_y", OSD.FromReal(user.StartLookAt.Value.Y));
                query.Add("start_look_at_z", OSD.FromReal(user.StartLookAt.Value.Z));
            }

            //byte[] postData = OSDParser.SerializeXmlBytes(query);

            // Make the request
            CapsClient request = new CapsClient(_caps.CreateUser);
            request.OnComplete += new CapsClient.CompleteCallback(CreateUserResponse);
            request.BeginGetResponse(REQUEST_TIMEOUT);

            // FIXME: Block
            return UUID.Zero;
        }

        private void CreateUserResponse(CapsClient client, OSD response, Exception error)
        {
            if (response is OSDMap)
            {
                // everything is okay
                // FIXME:
                //return new UUID(((Dictionary<string, object>)response)["agent_id"].ToString());
            }
            else
            {
                // an error happened
                OSDArray al = (OSDArray)response;

                StringBuilder sb = new StringBuilder();

                foreach (OSD ec in al)
                {
                    if (sb.Length > 0)
                        sb.Append("; ");

                    sb.Append(_errors[ec.AsInteger()]);
                }

                // FIXME:
                //throw new Exception("failed to create user: " + sb.ToString());
            }
        }
    }
}
