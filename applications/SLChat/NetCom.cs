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
		
		public NetCom(string fname, string lname, string pwrd, ChatScreen wndChat)
		{
			//Our NetCom main thing, we go through and set
			//our settings up.
			winChat = wndChat;
			client = new SecondLife("keywords.txt", "protocol.txt");
			firstname = fname;
			lastname = lname;
			password = pwrd;
		}
		
		private void ChatIncoming(Packet packet, Simulator simulator)
		{
			//Incoming chat handler, basicly chat from simulator
			//Callback for "ChatFromSimulator"
			//client = new SecondLife("keywords.txt", "protocol.txt");
			if (packet.Layout.Name == "ChatFromSimulator")
			{
				string output		= "";
				string message		= "";
				byte audible		= 0;
				byte type			= 0;
				byte sourcetype		= 0;
				string name			= "";
				LLUUID id			= new LLUUID();
				byte command		= 0;
				LLUUID commandID	= new LLUUID();

				ArrayList blocks;

				blocks = packet.Blocks();

				foreach (Block block in blocks)
				{
					foreach (Field field in block.Fields)
					{
						if (field.Layout.Name == "ID")
						{
							id = (LLUUID)field.Data;
						} 
						else if(field.Layout.Name == "Name")
						{
							name = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
						}
						else if(field.Layout.Name == "SourceType")
						{
							sourcetype = (byte)field.Data;
						}
						else if(field.Layout.Name == "Type")
						{
							type = (byte)field.Data;
						}
						else if(field.Layout.Name == "Audible")
						{
							audible = (byte)field.Data;
						}
						else if(field.Layout.Name == "Message")
						{
							message = System.Text.Encoding.UTF8.GetString((byte[])field.Data).Replace("\0", "");
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
					if(name != firstname + " " + lastname)
					{
						//If the name and first name is not ours
						//so we don't get our own talkback with our name.
						output = newline + name + ": " + message;
						winChat.ReturnData(output,3,name,id.ToString());
					}else{
						//Now if it IS our text, we want to replace
						//the name with "You", this makes it easier
						//to distinguish our own text.
						name = "You";
						output = newline + name + ": " + message;
						winChat.ReturnData(output,3,name,id.ToString());
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
				client.Network.RegisterCallback("ChatFromSimulator", new PacketCallback(ChatIncoming));
				
				Hashtable loginParams = NetworkManager.DefaultLoginValues(firstname, lastname, password,
					"00:00:00:00:00:00", "last", 1, 11, 11, 11, "Win", "0", "SLChat", "ozspade@slinked.net");


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
