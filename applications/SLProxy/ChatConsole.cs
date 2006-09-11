/*
 * ChatConsole.cs: sample SLProxy appliation that writes chat to the console.
 *   Typing on the console will send chat to Second Life.
 *
 * Copyright (c) 2006 Austin Jennings
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

using SLProxy;
using libsecondlife;
using Nwc.XmlRpc;

using System;
using System.Collections;
using System.Net;
using System.Threading;

public class ChatConsole {
	private static SecondLife client;
	private static ProtocolManager protocolManager;
	private static Proxy proxy;
	private static LLUUID agentID;
	private static LLUUID sessionID;

	public static void Main(string[] args) {
		// configure the proxy
		client = new SecondLife("keywords.txt", "protocol.txt");
		protocolManager = client.Protocol;
		ProxyConfig proxyConfig = new ProxyConfig("ChatConsole", "austin.jennings@gmail.com", protocolManager, args);
		proxy = new Proxy(proxyConfig);

		// set a delegate for when the client logs in
		proxy.SetLoginResponseDelegate(new XmlRpcResponseDelegate(Login));

		// add a delegate for incoming chat
		proxy.AddDelegate("ChatFromSimulator", Direction.Incoming, new PacketDelegate(ChatFromSimulator));

		// start the proxy
		proxy.Start();
	}

	private static void Login(XmlRpcResponse response) {
		Hashtable values = (Hashtable)response.Value;
		if (values.Contains("agent_id") && values.Contains("session_id")) {
			// remember our agentID and sessionID
			agentID = new LLUUID((string)values["agent_id"]);
			sessionID = new LLUUID((string)values["session_id"]);

			// start a new thread that reads lines from the console
			(new Thread(new ThreadStart(ReadFromConsole))).Start();
		}
	}

	private static void ReadFromConsole() {
		// send text from the console in an infinite loop
		for (;;) {
			// read a line from the console
			string message = Console.ReadLine();

			// construct a ChatFromViewer packet
			Hashtable blocks = new Hashtable();
			Hashtable fields;
			fields = new Hashtable();
			fields["Channel"] = (int)0;
			fields["Message"] = message;
			fields["Type"] = (byte)1;
			blocks[fields] = "ChatData";
			fields = new Hashtable();
			fields["AgentID"] = agentID;
			fields["SessionID"] = sessionID;
			blocks[fields] = "AgentData";
			Packet chatPacket = PacketBuilder.BuildPacket("ChatFromViewer", protocolManager, blocks, Helpers.MSG_RELIABLE);

			// inject the packet
			proxy.InjectPacket(chatPacket, Direction.Outgoing);
		}
	}

	private static Packet ChatFromSimulator(Packet packet, IPEndPoint sim) {
		// deconstruct the packet
		Hashtable blocks = PacketUtility.Unbuild(packet);
		string message = DataConvert.toChoppedString(PacketUtility.GetField(blocks, "ChatData", "Message"));
		string name = DataConvert.toChoppedString(PacketUtility.GetField(blocks, "ChatData", "FromName"));
		byte audible = (byte)PacketUtility.GetField(blocks, "ChatData", "Audible");
		byte type = (byte)PacketUtility.GetField(blocks, "ChatData", "ChatType");

		// if this was a normal, audible message, write it to the console
		if (audible != 0 && (type == 0 || type == 1 || type == 2))
			Console.WriteLine(name + ": " + message);

		// return the packet unmodified
		return packet;
	}
}
