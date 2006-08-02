/*
 * Created by SharpDevelop.
 * User: Oz
 * Date: 7/11/2006
 * Time: 8:32 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using libsecondlife;

namespace SLChat
{
	/// <summary>
	/// Description of NetCom:
	/// NetCom stands for Network Communication
	/// Basicly all functions that have to do with communicating
	/// too and from SL should happen in here. This keeps things
	/// organized and interface seperate from network code.
	/// </summary>
	public class NetCom
	{
		public  SecondLife client;
		string newline = System.Environment.NewLine;
		public static ChatScreen winChat;
		public string firstname;
		public string lastname;
		public string password;
		public bool loggedin;
		public string loginLocation;
		
		public NetCom(string fname, string lname, string pwrd, string logLocation, ChatScreen wndChat)
		{
			//Our NetCom main thing, we go through and set
			//our settings up.
			winChat = wndChat;
			client = new SecondLife("keywords.txt", "protocol.txt");
			client.Network.RegisterCallback("ChatFromSimulator", new PacketCallback(ChatIncoming));
			client.Network.RegisterCallback("ImprovedInstantMessage", new PacketCallback(InstantMessageIncoming));
			
			firstname = fname;
			lastname = lname;
			password = pwrd;
			loginLocation = logLocation;
		}
		
		private void InstantMessageIncoming(Packet packet, Simulator simulator)
		{
			if (packet.Layout.Name == "ImprovedInstantMessage")
			{
				string output = "";
				LLUUID FromAgentID	= new LLUUID();
				LLUUID ToAgentID	= new LLUUID();
				uint ParentEstateID	= 0;
				LLUUID RegionID		= new LLUUID();
				LLVector3 Position	= new LLVector3();
				byte Offline		= 0;
				byte Dialog			= 0;
				LLUUID ID			= new LLUUID();
				uint Timestamp		= 0;
				string FromAgentName	= "";
				string Message		= "";
				string BinaryBucket		= "";

				ArrayList blocks;

				blocks = packet.Blocks();

				foreach (Block block in blocks)
				{
					foreach (Field field in block.Fields)
					{
						if(field.Layout.Name == "FromAgentID")
						{
							FromAgentID = (LLUUID)field.Data;
						}
						else if(field.Layout.Name == "ToAgentID")
						{
							ToAgentID = (LLUUID)field.Data;
						}
						else if(field.Layout.Name == "ParentEstateID")
						{
							ParentEstateID = (uint)field.Data;
						}
						else if(field.Layout.Name == "RegionID")
						{
							RegionID = (LLUUID)field.Data;
						}
						else if(field.Layout.Name == "Position")
						{
							Position = (LLVector3)field.Data;
						}
						else if(field.Layout.Name == "Offline")
						{
							Offline = (byte)field.Data;
						}
						else if(field.Layout.Name == "Dialog")
						{
							Dialog = (byte)field.Data;
						}
						else if(field.Layout.Name == "ID")
						{
							ID = (LLUUID)field.Data;
						}
						else if(field.Layout.Name == "Timestamp")
						{
							Timestamp = (uint)field.Data;
						}
						else if(field.Layout.Name == "FromAgentName")
						{
							FromAgentName = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if(field.Layout.Name == "Message")
						{
							Message = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if(field.Layout.Name == "BinaryBucket")
						{
							BinaryBucket = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
					}
				}

				output = newline + FromAgentName + ": " + Message;
				winChat.ReturnData(output,4,FromAgentName,FromAgentID.ToString());
			} 
		}
		
		private void ChatIncoming(Packet packet, Simulator simulator)
		{
			//Incoming chat handler, basicly chat from simulator
			//Callback for "ChatFromSimulator"
			//client = new SecondLife("keywords.txt", "protocol.txt");
			if (packet.Layout.Name == "ChatFromSimulator")
			{
				string output		= "";
				string fromname			= ""; //Name of source.
				LLUUID sourceid			= new LLUUID(); //UUID of source, object/avatar
				LLUUID ownerid			= new LLUUID(); //UUID of owner, if object UUID = owner of object, if avatar UUID = same as source
				byte sourcetype		= 0; //1 = avatar, 2 = object
				byte chattype			= 0; //0 = Whisper, 1 = Say, 2 = Shout, 3 = unknown, 4 = typing notification, 5 = chatbar open/close
				byte audible		= 0; //Audible: 1 if audible, 0 if beyond 20m (message is null)
				LLVector3 position		= new LLVector3(); //Region local position of source.
				string message		= ""; //Message from source
				byte command		= 0; //Unused?
				LLUUID commandID	= new LLUUID(); //Unused?

				ArrayList blocks;

				blocks = packet.Blocks();

				foreach (Block block in blocks)
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "SourceID")
						{
							sourceid = (LLUUID)field.Data;
						} 
						else if(field.Layout.Name == "OwnerID")
						{
							ownerid = (LLUUID)field.Data;
						}
						else if(field.Layout.Name == "FromName")
						{
							fromname = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if(field.Layout.Name == "SourceType")
						{
							sourcetype = (byte)field.Data;
						}
						else if(field.Layout.Name == "ChatType")
						{
							chattype = (byte)field.Data;
						}
						else if(field.Layout.Name == "Audible")
						{
							audible = (byte)field.Data;
						}
						else if(field.Layout.Name == "Message")
						{
							message = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if(field.Layout.Name == "Position")
						{
							position = (LLVector3)field.Data;
						}
						else if(field.Layout.Name == "Command")
						{
							command = (byte)field.Data;
						}
						else if(field.Layout.Name == "CommandID")
						{
							commandID = (LLUUID)field.Data;
						}
						
					}
				}
				if(message!="")
				{
					//If we haven't recieved a blank message
					if(fromname != firstname + " " + lastname)
					{
						//If the name and first name is not ours
						//so we don't get our own talkback with our name.
						output = newline + fromname + ": " + message;
						winChat.ReturnData(output,3,fromname,sourceid.ToString());
					}else{
						//Now if it IS our text, we want to replace
						//the name with "You", this makes it easier
						//to distinguish our own text.
						fromname = "You";
						output = newline + fromname + ": " + message;
						winChat.ReturnData(output,3,fromname,sourceid.ToString());
					}
				}
			}
		}
		
		public void Login()
		{
			//Our login function.
			//LoginReply will be used to capture the output text.
			string loginReply;
			//Double checking on name.
			if(firstname != "" & lastname != "" & password != "")
			{
				
				Hashtable loginParams = NetworkManager.DefaultLoginValues(firstname, lastname, password,
					"00:00:00:00:00:00", loginLocation, 1, 11, 11, 11, "Win", "0", "SLChat", "ozspade@slinked.net");
				//uri:Ahern&amp;195&amp;233&amp;30

				// An example of how to pass additional options to the login server
				// Request information on the Root Inventory Folder, and Inventory Skeleton
				//			alAdditionalInfo.Add("inventory-skeleton");

				//ArrayList alAdditionalInfo = new ArrayList();
				//alAdditionalInfo.Add("inventory-root");
				//loginParams.Add("options",alAdditionalInfo);

				//Hashtable loginReply = new Hashtable();

						
				if (!client.Network.Login(loginParams))
				{
					// Login failed
					loginReply = "Error logging in: " + newline + client.Network.LoginError + newline;

			
					//return error;
				}
				// Login was successful
				loginReply = newline + "Message of the day: " + client.Network.LoginValues["message"] + newline;
				winChat.ReturnData(loginReply,1,"","");
				//return success;
			}else{
					loginReply = newline + "A login field is blank!";
					winChat.ReturnData(loginReply,1,"error","");
					//return error;
			}
		}
		
		public void Logout()
		{
			//Our logout function.
			string logoutReply;
			client.Network.Logout();
			logoutReply = newline + "Successfully logged out!" + newline;
			winChat.ReturnData(logoutReply,2,"","");
		}
		
		public void ChatOut(string chat, int type, int channel)
		{
			//Any outgoing chat will be handled here.
			
			//Chat == text to output, Type == Say, Chat or Whisper
			//Channel == public or script (0 is public).
			if(type==0)
			{
				//Type: Say
				client.Avatar.Say(chat,channel);
			}else if(type==1){
				//Type: Shout
				client.Avatar.Shout(chat,channel);
			}else if(type==2){
				//Type: Whisper
			}
		}
	}
}
