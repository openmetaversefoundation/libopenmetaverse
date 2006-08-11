using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using libsecondlife;

namespace SLNetworkComm
{
	/// <summary>
	/// Description of SLNetCom:
	/// NetCom stands for Network Communication
	/// Basically all functions that have to do with communicating
	/// too and from SL should happen in here. This keeps things
	/// organized and interface seperate from network code.
	/// </summary>
	public partial class SLNetCom
	{
		private SecondLife client;
		private bool loggedIn = false;
        private SLLoginOptions loginOptions;
        
        // NetcomSync is used for raising IM/Chat events on the
        // GUI/main thread. Useful if you're modifying GUI controls
        // in the client app when responding to IM/Chat events.
        private ISynchronizeInvoke netcomSync;

        public SLNetCom()
        {
            this.InitializeClient();
            loginOptions = new SLLoginOptions();
        }

		public SLNetCom(string firstName, string lastName, string password, string loginLocation)
		{
            this.InitializeClient();

            loginOptions = new SLLoginOptions();
			loginOptions.FirstName = firstName;
			loginOptions.LastName = lastName;
			loginOptions.Password = password;
            loginOptions.StartLocation = loginLocation;
		}

        public SLNetCom(SLLoginOptions logOptions)
        {
            this.InitializeClient();
            loginOptions = logOptions;
        }

        private void InitializeClient()
        {
            client = new SecondLife("keywords.txt", "protocol.txt");
            client.Network.RegisterCallback("ChatFromSimulator", new PacketCallback(ChatIncoming));
            client.Network.RegisterCallback("ImprovedInstantMessage", new PacketCallback(InstantMessageIncoming));
        }
		
		private void InstantMessageIncoming(Packet packet, Simulator simulator)
		{
            if (packet.Layout.Name != "ImprovedInstantMessage") return;

            LLUUID FromAgentID   = new LLUUID();
            LLUUID ToAgentID     = new LLUUID();
            uint ParentEstateID  = 0;
            LLUUID RegionID      = new LLUUID();
            LLVector3 Position   = new LLVector3();
            bool Offline         = false;
            byte Dialog          = 0;
            LLUUID ID            = new LLUUID();
            uint Timestamp       = 0;
            DateTime dt          = new DateTime(1970, 1, 1, 0, 0, 0, 0); //The Unix epoch!
            string FromAgentName = string.Empty;
            string Message       = string.Empty;
            string BinaryBucket  = string.Empty;

            ArrayList blocks = packet.Blocks();

            foreach (Block block in blocks)
            {
                foreach (Field field in block.Fields)
                {
                    switch (field.Layout.Name)
                    {
                        case "FromAgentID":
                            FromAgentID = (LLUUID)field.Data;
                            break;

                        case "ToAgentID":
                            ToAgentID = (LLUUID)field.Data;
                            break;

                        case "ParentEstateID":
                            ParentEstateID = (uint)field.Data;
                            break;

                        case "RegionID":
                            RegionID = (LLUUID)field.Data;
                            break;

                        case "Position":
                            Position = (LLVector3)field.Data;
                            break;

                        case "Offline":
                            Offline = ((byte)field.Data == 1 ? true : false);
                            break;

                        case "Dialog":
                            Dialog = (byte)field.Data;
                            break;

                        case "ID":
                            ID = (LLUUID)field.Data;
                            break;

                        case "Timestamp":
                            Timestamp = (uint)field.Data;

                            if (Timestamp == 0) //User is online
                                dt = DateTime.Now;
                            else //User is offline
                                dt = dt.AddSeconds(Timestamp);

                            break;

                        case "FromAgentName":
                            FromAgentName = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", string.Empty);
                            break;

                        case "Message":
                            Message = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", string.Empty);
                            break;

                        case "BinaryBucket":
                            BinaryBucket = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", string.Empty);
                            break;
                    }
                }

                InstantMessageEventArgs eventArgs = new InstantMessageEventArgs(
                    FromAgentID, ToAgentID, ParentEstateID, RegionID,
                    Position, Offline, Dialog, ID,
                    dt, FromAgentName, Message, BinaryBucket);

                if (netcomSync != null)
                {
                    object[] ea = new object[1];
                    ea[0] = eventArgs;
                    netcomSync.Invoke(new OnInstantMessageRaise(OnInstantMessageReceived), ea);
                }
                else
                {
                    OnInstantMessageReceived(eventArgs);
                }
            }
		}
		
		private void ChatIncoming(Packet packet, Simulator simulator)
		{
            if (packet.Layout.Name != "ChatFromSimulator") return;

            string fromname         = string.Empty; //Name of source.
            LLUUID sourceid         = new LLUUID(); //UUID of source, object/avatar
            LLUUID ownerid          = new LLUUID(); //UUID of owner, if object UUID = owner of object, if avatar UUID = same as source
            SLSourceType sourcetype = SLSourceType.None;
            SLChatType chattype     = SLChatType.Whisper;
            bool audible            = false; //Audible: 1 if audible, 0 if beyond 20m (message is null)
            LLVector3 position      = new LLVector3(); //Region local position of source.
            string message          = string.Empty; //Message from source
            byte command            = 0; //Unused?
            LLUUID commandID        = new LLUUID(); //Unused?
            
            ArrayList blocks = packet.Blocks();

            foreach (Block block in blocks)
            {
                foreach (Field field in block.Fields)
                {
                    switch (field.Layout.Name)
                    {
                        case "SourceID":
                            sourceid = (LLUUID)field.Data;
                            break;

                        case "OwnerID":
                            ownerid = (LLUUID)field.Data;
                            break;

                        case "FromName":
                            fromname = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", string.Empty);
                            break;

                        case "SourceType":
                            sourcetype = (SLSourceType)(byte)field.Data;
                            break;

                        case "ChatType":
                            chattype = (SLChatType)(byte)field.Data;
                            break;

                        case "Audible":
                            audible = ((byte)field.Data == 1 ? true : false);
                            break;

                        case "Message":
                            message = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", string.Empty);
                            break;

                        case "Position":
                            position = (LLVector3)field.Data;
                            break;

                        case "Command":
                            command = (byte)field.Data;
                            break;

                        case "CommandID":
                            commandID = (LLUUID)field.Data;
                            break;
                    }
				}

                ChatEventArgs eventArgs = new ChatEventArgs(
                    message, chattype,
                    position, sourcetype, sourceid, ownerid,
                    fromname, audible, command, commandID);

                if (netcomSync != null)
                {
                    object[] ea = new object[1];
                    ea[0] = eventArgs;
                    netcomSync.Invoke(new OnChatRaise(OnChatReceived), ea);
                }
                else
                {
                    OnChatReceived(eventArgs);
                }
			}
		}
		
		public void Login()
		{
            //LoginReply will be used to contain the output text.
            string loginReply;

            //Checking for empty/null login fields. Leave the Login() method if true.
            if (string.IsNullOrEmpty(loginOptions.FirstName) ||
                string.IsNullOrEmpty(loginOptions.LastName) ||
                string.IsNullOrEmpty(loginOptions.Password) ||
                string.IsNullOrEmpty(loginOptions.StartLocation))
            {
                loginReply = "A login field is blank!";

                OnClientLoginError(new ClientLoginEventArgs(loginReply));
                return;
            }

            Hashtable loginParams = NetworkManager.DefaultLoginValues(
                loginOptions.FirstName, loginOptions.LastName, loginOptions.Password,
                "00:00:00:00:00:00",
                loginOptions.StartLocation.ToLower(),
                1, 50, 50, 50, "Win",
                "0", loginOptions.UserAgent, loginOptions.Author);

            //uri:Ahern&amp;195&amp;233&amp;30

            // An example of how to pass additional options to the login server
            // Request information on the Root Inventory Folder, and Inventory Skeleton
            //			alAdditionalInfo.Add("inventory-skeleton");

            //ArrayList alAdditionalInfo = new ArrayList();
            //alAdditionalInfo.Add("inventory-root");
            //loginParams.Add("options",alAdditionalInfo);

            //Hashtable loginReply = new Hashtable();

            if (client.Network.Login(loginParams))
            {
                // Login was successful
                loginReply = "Message of the day: " + client.Network.LoginValues["message"];
                loggedIn = true;

                OnClientLoggedIn(new ClientLoginEventArgs(loginReply));
            }
            else
            {
                // Login failed
                loginReply = "Error logging in: " + client.Network.LoginError;

                OnClientLoginError(new ClientLoginEventArgs(loginReply));
            }
		}
		
		public void Logout()
		{
            if (!loggedIn) return;

			client.Network.Logout();
            loggedIn = false;

            string logoutReply = "Successfully logged out!";
            OnClientLoggedOut(new ClientLoginEventArgs(logoutReply));
		}
		
		public void ChatOut(string chat, SLChatType type, int channel)
		{
            if (!loggedIn) return;
            if (string.IsNullOrEmpty(chat)) return;

            switch (type)
            {
                case SLChatType.Say:
                    client.Avatar.Say(chat, channel);
                    break;

                case SLChatType.Shout:
                    client.Avatar.Shout(chat, channel);
                    break;

                case SLChatType.Whisper:
                    client.Avatar.Whisper(chat, channel);
                    break;
            }

            OnChatSent(new ChatSentEventArgs(chat, type, channel));
		}

        public void SendInstantMessage(string message, LLUUID target, LLUUID session)
        {
            if (!loggedIn) return;

            client.Avatar.InstantMessage(loginOptions.FullName, session, target, message, null);
            OnInstantMessageSent(new InstantMessageSentEventArgs(message, target, session, DateTime.Now));
        }

        public SecondLife Client
        {
            get { return client; }
        }

        public bool LoggedIn
        {
            get { return loggedIn; }
        }

        public SLLoginOptions LoginOptions
        {
            get { return loginOptions; }
        }

        public ISynchronizeInvoke NetcomSync
        {
            get { return netcomSync; }
            set { netcomSync = value; }
        }
	}
}
