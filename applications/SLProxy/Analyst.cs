/*
 * Analyst.cs: proxy that dumps all packets to and from the server
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

using System;
using System.Net;

public class Analyst {
	public static void Main(string[] args) {
		ProtocolManager protocolManager = new ProtocolManager("keywords.txt", "protocol.txt");
		ProxyConfig proxyConfig = new ProxyConfig("Analyst", "austin.jennings@gmail.com", protocolManager, args);
		Proxy proxy = new Proxy(proxyConfig);

		// register delegates for all packets
		RegisterDelegates(proxy, protocolManager.LowMaps);
		RegisterDelegates(proxy, protocolManager.MediumMaps);
		RegisterDelegates(proxy, protocolManager.HighMaps);

		proxy.Start();
	}

	// register delegates for each packet in an array of packet maps
	private static void RegisterDelegates(Proxy proxy, MapPacket[] maps) {
		PacketDelegate incomingLogger = new PacketDelegate(LogIncomingPacket);
		PacketDelegate outgoingLogger = new PacketDelegate(LogOutgoingPacket);
		foreach (MapPacket map in maps)
			if (map != null) {
				proxy.AddDelegate(map.Name, Direction.Incoming, incomingLogger);
				proxy.AddDelegate(map.Name, Direction.Outgoing, outgoingLogger);
			}
	}

	// delegate for incoming packets: log the packet and return it unharmed
	private static Packet LogIncomingPacket(Packet packet, IPEndPoint endPoint) {
		LogPacket(packet, endPoint, Direction.Incoming);
		return packet;
	}

	// delegate for outgoing packets: log the packet and return it unharmed
	private static Packet LogOutgoingPacket(Packet packet, IPEndPoint endPoint) {
		LogPacket(packet, endPoint, Direction.Outgoing);
		return packet;
	}

	// helper method: perform the logging of a packet
	private static void LogPacket(Packet packet, IPEndPoint endPoint, Direction direction) {
		Console.WriteLine("{0} {1,21} {2,5} {3}{4}{5}"
				 ,direction == Direction.Incoming ? "<--" : "-->"
				 ,endPoint
				 ,packet.Sequence
				 ,InterpretOptions(packet.Data[0])
				 ,Environment.NewLine
				 ,packet
				 );
	}

	// produce a string representing a packet's header options
	private static string InterpretOptions(byte options) {
		return "["
		     + ((options & Helpers.MSG_APPENDED_ACKS) != 0 ? "Ack" : "   ")
		     + " "
		     + ((options & Helpers.MSG_RESENT)        != 0 ? "Res" : "   ")
		     + " "
		     + ((options & Helpers.MSG_RELIABLE)      != 0 ? "Rel" : "   ")
		     + " "
		     + ((options & Helpers.MSG_ZEROCODED)     != 0 ? "Zer" : "   ")
		     + "]"
		     ;
	}
}
