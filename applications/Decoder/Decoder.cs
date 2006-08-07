/*
 * Decoder.cs: decodes pasted packet dumps
 *   See the README for usage instructions.
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

using System;
using System.Globalization;
using System.Text.RegularExpressions;

using libsecondlife;

class Decoder {
	private static int BUFSIZE = 8096;

	private static ProtocolManager protocol = new ProtocolManager("keywords.txt", "protocol.txt");
	private static byte[] data = new byte[BUFSIZE];
	private static byte[] temp = new byte[BUFSIZE];
	private static bool boring;
	private static int direction;
	private static string sim;
	private static int pos;
	private static Mode mode = Mode.Generic;
	private static Regex regex = RegexForMode(mode);

	private enum Mode
		{Generic
		,SLConsole
		,TCPDump
		};

	private static Regex RegexForMode(Mode mode) {
		switch (mode) {
			case Mode.Generic:
				return new Regex(@"(?:\s|PD:|^)(?:([0-9a-fA-F][0-9a-fA-F]){1,2}(?:\s|$))+");
			case Mode.SLConsole:
				return new Regex(@"PD:(?:([0-9a-f]{2})(?: |$))+");
			case Mode.TCPDump:
				return new Regex(@"^\t0x[0-9a-f]{3}0:  (?:([0-9a-f][0-9a-f]){1,2}(?: |$))+");
			default:
				throw new Exception("RegexForMode broken");
		}
	}

	private static Regex _modeRegex_SLConsole	= new Regex(@"^\S+ INFO: Second Life version ");
	private static Regex _modeRegex_TCPDump		= new Regex(@"^\d\d:\d\d:\d\d\.\d+ IP \S+ > \S+: ");
	private static void SetMode(string line) {
		if (_modeRegex_SLConsole.Match(line).Success)
			regex = RegexForMode(mode = Mode.SLConsole);
		else if (_modeRegex_TCPDump.Match(line).Success)
			regex = RegexForMode(mode = Mode.TCPDump);
	}

	public static void Main() {
		for (Reset();;) {
			string line = Console.ReadLine();
			if (line == null) {
				if (pos != 0)
					Done();

				return;
			}
			
			if (mode == Mode.Generic)
				SetMode(line);

			Match m = regex.Match(line);
			if (m.Success) {
				while (pos + m.Groups[1].Captures.Count >= BUFSIZE) {
					byte[] newData = new byte[data.Length + BUFSIZE];
					Array.Copy(data, 0, newData, 0, pos);
					data = newData;
				}

				foreach (Capture capture in m.Groups[1].Captures)
					data[pos++] = Byte.Parse(capture.ToString(), NumberStyles.AllowHexSpecifier);
			} else {
				if (pos != 0)
					Done();

				Prepare(line);
			}
		}
	}

	private static void Reset() {
		byte[] clear = {0,0,0,0,0,0,0,0};
		Array.Copy(clear, 0, data, 0, 8);
		boring = false;
		direction = 0;
		sim = "unknown";
		pos = 0;
	}

	private static Regex _prepareRegex_TCPDump_0	= new Regex(@"^\d\d:\d\d:\d\d\.\d+ (.+)");
	private static Regex _prepareRegex_TCPDump_1	= new Regex(@"^IP (\S+) > (\S+): UDP, ");
	private static Regex _prepareRegex_TCPDump_2	= new Regex(@"\.lindenlab\.com\.\d+$");
	private static void Prepare(string line) {
		Match m;
		if (mode == Mode.TCPDump && (m = _prepareRegex_TCPDump_0.Match(line)).Success)
			// packet header
			if ((m = _prepareRegex_TCPDump_1.Match(m.Groups[1].Captures[0].ToString())).Success)
				// UDP header
				if (_prepareRegex_TCPDump_2.Match(m.Groups[1].Captures[0].ToString()).Success) {
					// incoming SL
					direction = 1;
					sim = m.Groups[1].Captures[0].ToString();
				} else if (_prepareRegex_TCPDump_2.Match(m.Groups[2].Captures[0].ToString()).Success) {
					// outgoing SL
					direction = 2;
					sim = m.Groups[2].Captures[0].ToString();
				} else
					boring = true;
			else
				boring = true;
	}

	private static void Done() {
		if (!boring) try {
			byte[] buf;
			if (mode == Mode.TCPDump) {
				if ((data[0] & 0x0F) != 0x05) {
					Console.WriteLine("*** skipping packet with unusual IP header ***");
					Console.WriteLine();
					Reset();
					return;
				}
				if ((data[6] & 0x1F) != 0x00 || data[7] != 0x00) {
					// nonzero fragment offset; we already told them we're truncating the packet
					Reset();
					return;
				}
				if ((data[6] & 0x02) != 0x00) {
					Console.WriteLine("*** truncating fragmented packet ***");
				}

				if (data.Length - 28 > temp.Length)
					temp = new byte[data.Length];

				Array.Copy(data, 28, temp, 0, pos -= 28);

				if ((temp[0] & Helpers.MSG_ZEROCODED) != 0) {
					pos = Helpers.ZeroDecode(temp, pos, data);
					buf = data;
				} else
					buf = temp;
			} else
				if ((data[0] & Helpers.MSG_ZEROCODED) != 0) {
					pos = Helpers.ZeroDecode(data, pos, temp);
					buf = temp;
				} else
					buf = data;

			Packet packet = new Packet(buf, pos, protocol);

			if (direction == 0)
				Console.Write("    ");
			else if (direction == 1)
				Console.Write("<-- ");
			else if (direction == 2)
				Console.Write("--> ");
			Console.WriteLine("{0,21} {1,5} {2} "
					 ,direction == 0 ? "" : sim
					 ,packet.Sequence
					 ,InterpretOptions(packet.Data[0])
					);
			Console.WriteLine(packet);
		} catch (Exception e) {
			Console.WriteLine(e.Message);
		}

		Reset();
	}

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
