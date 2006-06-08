using System;
using System.Text;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace libsecondlife
{
	public delegate void PacketCallback(Packet packet);

	internal class AcceptAllCertificatePolicy : ICertificatePolicy
	{
		public AcceptAllCertificatePolicy()
		{
		}

		public bool CheckValidationResult(ServicePoint sPoint, 
			System.Security.Cryptography.X509Certificates.X509Certificate cert, 
			WebRequest wRequest,int certProb)
		{
			// Always accept
			return true;
		}
	}

	public class Circuit
	{
		public uint CircuitCode;

		private ProtocolManager Protocol;
		private NetworkManager Network;
		private byte[] Buffer;
		private Socket Connection;
		private IPEndPoint ipEndPoint;
		private EndPoint endPoint;

		public Circuit(ProtocolManager protocol, NetworkManager network, uint circuitCode)
		{
			Protocol = protocol;
			Network = network;
			CircuitCode = circuitCode;
			Buffer = new byte[4096];
			Connection = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		}

		public bool Open(string ip, int port)
		{
			// Setup the callback
			AsyncCallback onReceivedData = new AsyncCallback(this.OnRecievedData);

			// Create an endpoint that we will be communicating with (need it in two types due to
			// .NET weirdness)
			ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
			endPoint = (EndPoint)ipEndPoint;

			// Associate this circuit's socket with the given ip and port and start listening
			Connection.Connect(endPoint);
			Connection.BeginReceiveFrom(Buffer, 0, Buffer.Length, SocketFlags.None, ref endPoint, onReceivedData, null);

			// Send the UseCircuitCode packet to initiate the connection
			Packet packet = PacketBuilder.UseCircuitCode(Protocol, Network.LoginValues.AgentID, 
				Network.LoginValues.SessionID, CircuitCode);
			Connection.Send((byte[])packet.Data.ToArray(typeof(Byte)));

			return false;
		}

		public void Close()
		{
			//FIXME: CloseCircuit
			Connection.Close();
		}

		private void OnRecievedData(IAsyncResult result)
		{
			int numBytes = Connection.EndReceiveFrom(result, ref endPoint);
			Packet packet = new Packet(Buffer, numBytes, Protocol);
			PacketCallback callback = (PacketCallback)Network.Callbacks[packet.Layout.Name];
			callback(packet);
		}
	}

	public struct LoginReply
	{
		public LLUUID SessionID;
		public LLUUID SecureSessionID;
		public string StartLocation;
		public string FirstName;
		public string LastName;
		public int RegionX;
		public int RegionY;
		public string Home;
		public string Message;
		public uint CircuitCode;
		public int Port;
		public string IP;
		public string LookAt;
		public LLUUID AgentID;
		public uint SecondsSinceEpoch;
	}

	public class NetworkManager
	{
		public LoginReply LoginValues;
		public string LoginError;
		public Hashtable Callbacks;

		private ProtocolManager Protocol;
		private string LoginBuffer;
		private ArrayList Circuits;
		private Hashtable InternalCallbacks;

		public NetworkManager(ProtocolManager protocol)
		{
			Protocol = protocol;
			Circuits = new ArrayList();
			Callbacks = new Hashtable();
		}

		public bool Login(string firstName, string lastName, string password, string mac,
			int major, int minor, int patch, int build, string platform, string viewerDigest, 
			string userAgent, string author)
		{
			return Login(firstName, lastName, password, mac, major, minor, patch, build, platform,
				viewerDigest, userAgent, author, "https://login.agni.lindenlab.com/cgi-bin/login.cgi");
		}

		public bool Login(string firstName, string lastName, string password, string mac,
			int major, int minor, int patch, int build, string platform, string viewerDigest, 
			string userAgent, string author, string url)
		{
			WebRequest login;
			WebResponse response;
			
			// Generate an MD5 hash of the password
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(password));
			StringBuilder passwordDigest = new StringBuilder();
			// Convert the hash to a hex string
			foreach(byte b in hash)
			{
				passwordDigest.AppendFormat("{0:x2}", b);
			}

			string loginRequest = 
				"<?xml version=\"1.0\"?><methodCall><methodName>login_to_simulator</methodName>" +
				"<params><param><value><struct>" +
				"<member><name>first</name><value><string>" + firstName + "</string></value></member>" +
				"<member><name>last</name><value><string>" + lastName + "</string></value></member>" +
				"<member><name>passwd</name><value><string>$1$" + passwordDigest + "</string></value></member>" +
				"<member><name>start</name><value><string>last</string></value></member>" +
				"<member><name>major</name><value><string>" + major + "</string></value></member>" +
				"<member><name>minor</name><value><string>" + minor + "</string></value></member>" +
				"<member><name>patch</name><value><string>" + patch + "</string></value></member>" +
				"<member><name>build</name><value><string>" + build + "</string></value></member>" +
				"<member><name>platform</name><value><string>" + platform + "</string></value></member>" +
				"<member><name>mac</name><value><string>" + mac + "</string></value></member>" +
				"<member><name>viewer_digest</name><value><string>" + viewerDigest + "</string></value></member>" +
				"<member><name>user-agent</name><value><string>" + userAgent + 
				" (" + Helpers.VERSION + ")</string></value></member>" +
				"<member><name>author</name><value><string>" + author + "</string></value></member>" +
				"</struct></value></param></params></methodCall>"
				;

			try
			{
				// Override SSL authentication mechanisms
				ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();

				login = WebRequest.Create(url);
				login.ContentType = "text/xml";
				login.Method = "POST";
				login.Timeout = 12000;
                byte[] request = System.Text.Encoding.ASCII.GetBytes(loginRequest);
				login.ContentLength = request.Length;
				System.IO.Stream stream = login.GetRequestStream();
				stream.Write(request, 0, request.Length);
				stream.Close();
				response = login.GetResponse();

				if (response == null)
				{
					LoginError = "Error logging in: (Unknown)";
					Helpers.Log(LoginError, Helpers.LogLevel.Warning);
					return false;
				}

				//TODO: To support UTF8 avatar names the encoding should be handled better
				System.IO.StreamReader streamReader = new System.IO.StreamReader(response.GetResponseStream(), 
					System.Text.Encoding.ASCII);
				LoginBuffer = streamReader.ReadToEnd();
				streamReader.Close();
				response.Close();
			}
			catch (Exception e)
			{
				LoginError = "Error logging in: " + e.Message;
				Helpers.Log(LoginError, Helpers.LogLevel.Warning);
				return false;
			}

			if (!ParseLoginReply())
			{
				return false;
			}

			// Connect to the sim given in the login reply
			Circuit circuit = new Circuit(Protocol, this, LoginValues.CircuitCode);
			circuit.Open(LoginValues.IP, LoginValues.Port);

			return true;
		}

		private bool ParseLoginReply()
		{
			string msg;

			msg = RpcGetString(LoginBuffer, "<name>reason</name>");
			if (msg.Length != 0) 
			{
				LoginError = RpcGetString(LoginBuffer, "<name>message</name>");
				return false;
			}

			msg = RpcGetString(LoginBuffer, "login</name><value><string>true");
			if (msg.Length == 0)
			{
				LoginError = "Unknown login error";
				return false;
			}

			// Grab the login parameters
			LoginValues.SessionID = RpcGetString(LoginBuffer.ToString(), "<name>session_id</name>");
			LoginValues.SecureSessionID = RpcGetString(LoginBuffer.ToString(), "<name>secure_session_id</name>");
			LoginValues.StartLocation = RpcGetString(LoginBuffer.ToString(), "<name>start_location</name>");
			LoginValues.FirstName = RpcGetString(LoginBuffer.ToString(), "<name>first_name</name>");
			LoginValues.LastName = RpcGetString(LoginBuffer.ToString(), "<name>last_name</name>");
			LoginValues.RegionX = RpcGetInt(LoginBuffer.ToString(), "<name>region_x</name>");
			LoginValues.RegionY = RpcGetInt(LoginBuffer.ToString(), "<name>region_y</name>");
			LoginValues.Home = RpcGetString(LoginBuffer.ToString(), "<name>home</name>");
			LoginValues.Message = RpcGetString(LoginBuffer.ToString(), "<name>message</name>").Replace("\r\n", "");
			LoginValues.CircuitCode = (uint)RpcGetInt(LoginBuffer.ToString(), "<name>circuit_code</name>");
			LoginValues.Port = RpcGetInt(LoginBuffer.ToString(), "<name>sim_port</name>");
			LoginValues.IP = RpcGetString(LoginBuffer.ToString(), "<name>sim_ip</name>");
			LoginValues.LookAt = RpcGetString(LoginBuffer.ToString(), "<name>look_at</name>");
			LoginValues.AgentID = RpcGetString(LoginBuffer.ToString(), "<name>agent_id</name>");
			LoginValues.SecondsSinceEpoch = (uint)RpcGetInt(LoginBuffer.ToString(), "<name>seconds_since_epoch</name>");

			return true;
		}

		string RpcGetString(string rpc, string name)
		{
			int pos = rpc.IndexOf(name);
			int pos2;

			if (pos == -1)
			{
				return "";
			}

			rpc = rpc.Substring(pos, rpc.Length - pos);
			pos = rpc.IndexOf("<string>");

			if (pos == -1)
			{
				return "";
			}

			rpc = rpc.Substring(pos + 8, rpc.Length - (pos + 8));

			pos2 = rpc.IndexOf("</string>");

			if (pos2 == -1)
			{
				return "";
			}

			return rpc.Substring(0, pos2);
		}

		int RpcGetInt(string rpc, string name)
		{
			int pos = rpc.IndexOf(name);
			int pos2;

			if (pos == -1)
			{
				return -1;
			}

			rpc = rpc.Substring(pos, rpc.Length - pos);
			pos = rpc.IndexOf("<i4>");

			if (pos == -1)
			{
				return -1;
			}

			rpc = rpc.Substring(pos + 4, rpc.Length - (pos + 4));

			pos2 = rpc.IndexOf("</i4>");
			
			if (pos2 == -1)
			{
				return -1;
			}

			return Int32.Parse(rpc.Substring(0, pos2));
		}
	}
}
