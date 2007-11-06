using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using libsecondlife.LLSD;

namespace libsecondlife
{
    public class RegistrationApi
    {
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
        /// see https://secure-web6.secondlife.com/developers/third_party_reg/#service_create_user or
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
            public Nullable<LLVector3> StartLocation;
            public Nullable<LLVector3> StartLookAt;
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
            CapsRequest request = new CapsRequest(RegistrationApiCaps.AbsoluteUri, String.Empty, null);
            request.OnCapsResponse += new CapsRequest.CapsResponseCallback(GatherCapsResponse);

            // build post data
            byte[] postData = Encoding.ASCII.GetBytes(
                String.Format("first_name={0}&last_name={1}&password={2}", _userInfo.FirstName, _userInfo.LastName, 
                _userInfo.Password));

            // send
            request.MakeRequest(postData, "application/xml", 0, null);
        }

        private void GatherCapsResponse(object response, HttpRequestState state)
        {
            if (response is Dictionary<string, object>)
            {
                Dictionary<string, object> respTable = (Dictionary<string, object>)response;

                // parse
                _caps = new RegistrationCaps();

                _caps.CreateUser = new Uri((string)respTable["create_user"]);
                _caps.CheckName = new Uri((string)respTable["check_name"]);
                _caps.GetLastNames = new Uri((string)respTable["get_last_names"]);
                _caps.GetErrorCodes = new Uri((string)respTable["get_error_codes"]);

                // finalize
                _initializing++;

                GatherErrorMessages();
            }
        }

        private void GatherErrorMessages()
        {
            if (_caps.GetErrorCodes == null)
                throw new InvalidOperationException("access denied");	// this should work even for not-approved users

            CapsRequest request = new CapsRequest(_caps.GetErrorCodes.AbsoluteUri, String.Empty, null);
            request.OnCapsResponse += new CapsRequest.CapsResponseCallback(GatherErrorMessagesResponse);
            request.MakeRequest();
        }

        private void GatherErrorMessagesResponse(object response, HttpRequestState state)
        {
            if (response is Dictionary<string, object>)
            {
                // parse

                //FIXME: wtf?
                foreach (KeyValuePair<string, object> error in (Dictionary<string, object>)response)
                {
                    //StringBuilder sb = new StringBuilder();

                    //sb.Append(error[1]);
                    //sb.Append(" (");
                    //sb.Append(error[0]);
                    //sb.Append("): ");
                    //sb.Append(error[2]);

                    //_errors.Add((int)error[0], sb.ToString());
                }

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

            CapsRequest request = new CapsRequest(_caps.GetLastNames.AbsoluteUri, String.Empty, null);
            request.OnCapsResponse += new CapsRequest.CapsResponseCallback(GatherLastNamesResponse);
            request.MakeRequest();

            // FIXME: Block
        }

        private void GatherLastNamesResponse(object response, HttpRequestState state)
        {
            if (response is Dictionary<string, object>)
            {
                Dictionary<string, object> respTable = (Dictionary<string, object>)response;

                _lastNames = new List<LastName>(respTable.Count);

                for (Dictionary<string, object>.Enumerator it = respTable.GetEnumerator(); it.MoveNext(); )
                {
                    LastName ln = new LastName();

                    ln.ID = int.Parse(it.Current.Key.ToString());
                    ln.Name = it.Current.Value.ToString();

                    _lastNames.Add(ln);
                }

                _lastNames.Sort(new Comparison<LastName>(delegate(LastName a, LastName b) { return a.Name.CompareTo(b.Name); }));
            }
        }

        public bool CheckName(string firstName, LastName lastName)
        {
            if (Initializing)
                throw new InvalidOperationException("still initializing");

            if (_caps.CheckName == null)
                throw new InvalidOperationException("access denied; only approved developers have access to the registration api");

            // Create the POST data
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("username", firstName);
            query.Add("last_name_id", lastName.ID);
            byte[] postData = LLSDParser.SerializeXmlBytes(query);

            CapsRequest request = new CapsRequest(_caps.CheckName.AbsoluteUri, String.Empty, null);
            request.OnCapsResponse += new CapsRequest.CapsResponseCallback(CheckNameResponse);
            request.MakeRequest(postData, "application/xml", 0, null);

            // FIXME:
            return false;
        }

        private void CheckNameResponse(object response, HttpRequestState state)
        {
            if (response is bool)
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
        public LLUUID CreateUser(CreateUserParam user)
        {
            if (Initializing)
                throw new InvalidOperationException("still initializing");

            if (_caps.CreateUser == null)
                throw new InvalidOperationException("access denied; only approved developers have access to the registration api");

            // Create the POST data
            Dictionary<string, object> query = new Dictionary<string, object>();
            query.Add("username", user.FirstName);
            query.Add("last_name_id", user.LastName.ID);
            query.Add("email", user.Email);
            query.Add("password", user.Password);
            query.Add("dob", user.Birthdate.ToString("yyyy-MM-dd"));

            if (user.LimitedToEstate != null)
                query.Add("limited_to_estate", user.LimitedToEstate.Value);

            if (!string.IsNullOrEmpty(user.StartRegionName))
                query.Add("start_region_name", user.LimitedToEstate.Value);

            if (user.StartLocation != null)
            {
                query.Add("start_local_x", user.StartLocation.Value.X);
                query.Add("start_local_y", user.StartLocation.Value.Y);
                query.Add("start_local_z", user.StartLocation.Value.Z);
            }

            if (user.StartLookAt != null)
            {
                query.Add("start_look_at_x", user.StartLookAt.Value.X);
                query.Add("start_look_at_y", user.StartLookAt.Value.Y);
                query.Add("start_look_at_z", user.StartLookAt.Value.Z);
            }

            byte[] postData = LLSDParser.SerializeXmlBytes(query);

            // Make the request
            CapsRequest request = new CapsRequest(_caps.CreateUser.AbsoluteUri, String.Empty, null);
            request.OnCapsResponse += new CapsRequest.CapsResponseCallback(CreateUserResponse);
            request.MakeRequest(postData, "application/xml", 0, null);

            // FIXME: Block
            return LLUUID.Zero;
        }

        private void CreateUserResponse(object response, HttpRequestState state)
        {
            if (response is Dictionary<string, object>)
            {
                // everything is okay
                // FIXME:
                //return new LLUUID(((Dictionary<string, object>)response)["agent_id"].ToString());
            }
            else
            {
                // an error happened
                List<object> al = (List<object>)response;

                StringBuilder sb = new StringBuilder();

                foreach (int ec in al)
                {
                    if (sb.Length > 0)
                        sb.Append("; ");

                    sb.Append(_errors[ec]);
                }

                // FIXME:
                //throw new Exception("failed to create user: " + sb.ToString());
            }
        }
    }
}
