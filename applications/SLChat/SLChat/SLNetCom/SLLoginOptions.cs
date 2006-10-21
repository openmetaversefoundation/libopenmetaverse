using System;
using System.Collections.Generic;
using System.Text;

namespace SLNetworkComm
{
    public class SLLoginOptions
    {
        private string firstName;
        private string lastName;
        private string password;
        private string author = string.Empty;
        private string userAgent = string.Empty;
        private string startLocation = "Home";

        public SLLoginOptions()
        {

        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                    return string.Empty;
                else
                    return firstName + " " + lastName;
            }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public string StartLocation
        {
            get { return startLocation; }
            set { startLocation = value; }
        }

        public string UserAgent
        {
            get { return userAgent; }
            set { userAgent = value; }
        }

        public string Author
        {
            get { return author; }
            set { author = value; }
        }
    }
}
