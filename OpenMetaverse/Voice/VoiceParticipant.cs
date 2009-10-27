using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenMetaverse.Voice
{
    public class VoiceParticipant
    {
        public string Sip{ get; set; }
        public string AvatarName { get; set; }
        public UUID id { get; set; }

        public VoiceParticipant( string puri )
        {
            id = IDFromName(puri);
        }

        /// <summary>
        /// Extract the avatar UUID encoded in a SIP URI
        /// </summary>
        /// <param name="inName"></param>
        /// <returns></returns>
        public static UUID IDFromName( string inName )
        {
        	// The "name" may actually be a SIP URI such as: "sip:xFnPP04IpREWNkuw1cOXlhw==@bhr.vivox.com"
	        // If it is, convert to a bare name before doing the transform.
	        string name = nameFromsipURI(inName);
	
        	// Doesn't look like a SIP URI, assume it's an actual name.
        	if(name==null)
		        name = inName;

        	// This will only work if the name is of the proper form.
	        // As an example, the account name for Monroe Linden (UUID 1673cfd3-8229-4445-8d92-ec3570e5e587) is:
	        // "xFnPP04IpREWNkuw1cOXlhw=="
	
	        if((name.Length == 25) && (name[0] == 'x') && (name[23] == '=') && (name[24] == '='))
	        {
		        // The name appears to have the right form.

		        // Reverse the transforms done by nameFromID
		        string temp = name.Replace( '-', '+' );
                temp = temp.Replace( '_', '/' );

                byte[] binary = Convert.FromBase64String( temp.Substring( 1) );
                UUID u = UUID.Zero;
                u.FromBytes(binary, 0);
                return u;
            }

            return UUID.Zero;
	    }

        private static string Encode64(string str)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }
        private static byte[] Decode64(string str)
        {
            return Convert.FromBase64String(str);
//            return System.Text.Encoding.UTF8.GetString(decbuff);
        }

        private static string nameFromsipURI( string uri)
    {
        Regex sip = new Regex("^sip:([^@]*)@.*$");
        Match m = sip.Match( uri );
        if (m.Success)
        {
            GroupCollection g = m.Groups;
            return g[1].Value;
        }

        return null;
    }

    }
}
