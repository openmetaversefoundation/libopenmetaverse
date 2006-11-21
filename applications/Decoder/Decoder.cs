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
using System.IO;
using System.Text.RegularExpressions;

using libsecondlife;
using libsecondlife.Packets;

class Decoder {
	private static int BUFSIZE = 8096;

	private static SecondLife client = new SecondLife();
	private static string grep = null;
	private static byte[] data = new byte[BUFSIZE];
	private static byte[] temp = new byte[BUFSIZE];
	private static bool boring;
	private static string endpoints;
	private static int pos;
	private static Mode mode = Mode.Generic;
	private static Regex regex = RegexForMode(mode);

	private enum Mode
		{Generic
		,TCPDump
		};

	private static Regex RegexForMode(Mode mode) {
		switch (mode) {
			case Mode.Generic:
				return new Regex(@"(?:\s|PD:|^)(?:([0-9a-fA-F][0-9a-fA-F]){1,2}(?:\s|$))+");
			case Mode.TCPDump:
				return new Regex(@"^\t0x....:  (?:([0-9a-f][0-9a-f]){1,2}(?: |$))+");
			default:
				throw new Exception("RegexForMode broken");
		}
	}

	private static Regex _modeRegex_TCPDump		= new Regex(@"^\d\d:\d\d:\d\d\.\d+ IP \S+ > \S+: ");
	private static void SetMode(string line) {
		if (_modeRegex_TCPDump.Match(line).Success)
			regex = RegexForMode(mode = Mode.TCPDump);
	}

	public static void Main(string[] args) {
		if (args.Length > 0) {
			// FIXME
			Console.WriteLine("sorry, filtering is currently broken :(");
			return;
			// grep = String.Join(" ", args);
		}

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
				if (pos == 0 && m.Groups[1].Captures.Count < 4) {
					boring = true;
					continue;
				}

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
		endpoints = "";
		pos = 0;
	}

	private static Regex _prepareRegex_TCPDump_0	= new Regex(@"^\d\d:\d\d:\d\d\.\d+ (.+)");
	private static Regex _prepareRegex_TCPDump_1	= new Regex(@"^IP (\S+ > \S+): UDP, ");
	private static Regex _prepareRegex_TCPDump_2	= new Regex(@"\.lindenlab\.com\.\d+");
	private static void Prepare(string line) {
		Match m;
		if (mode == Mode.TCPDump && (m = _prepareRegex_TCPDump_0.Match(line)).Success)
			// packet header
			if ((m = _prepareRegex_TCPDump_1.Match(m.Groups[1].Captures[0].ToString())).Success)
				// UDP header
				if (_prepareRegex_TCPDump_2.Match(m.Groups[1].Captures[0].ToString()).Success)
					// SL header
					endpoints = m.Groups[1].Captures[0].ToString();
				else
					boring = true;
			else
				boring = true;
	}

	private static void Done()
    {
        byte[] zeroBuffer = new byte[4096];

		if (!boring) try {
			byte[] buf;
			if ((data[0] & 0xF0) == 0x40) {
				// strip IP and UDP headers
				int headerlen = (data[0] & 0x0F) * 4 + 8;

				if ((data[6] & 0x1F) != 0x00 || data[7] != 0x00) {
					// nonzero fragment offset; we already told them we're truncating the packet
					Reset();
					return;
				}
				if ((data[6] & 0x02) != 0x00) {
					Console.WriteLine("*** truncating fragmented packet ***");
				}

				if (data.Length - headerlen > temp.Length)
					temp = new byte[data.Length];

				Array.Copy(data, headerlen, temp, 0, pos -= headerlen);

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

			Packet packet = Packet.BuildPacket(buf, ref pos, zeroBuffer);

			if (grep != null) {
				bool match = false;

                //FIXME: This needs to be updated for the new libsecondlife API
                //foreach (Block block in packet.Blocks())
                //{
                //    foreach (Field field in block.Fields)
                //    {
                //        string value;
                //        if (field.Layout.Type == FieldType.Variable)
                //            value = DataConvert.toChoppedString(field.Data);
                //        else
                //            value = field.Data.ToString();
                //        if (Regex.Match(packet.Layout.Name + "." + block.Layout.Name + "." + field.Layout.Name + " = " + value, grep, RegexOptions.IgnoreCase).Success)
                //        {
                //            match = true;
                //            break;
                //        }

                //        // try matching variable fields in 0x notation
                //        if (field.Layout.Type == FieldType.Variable)
                //        {
                //            StringWriter sw = new StringWriter();
                //            sw.Write("0x");
                //            foreach (byte b in (byte[])field.Data)
                //                sw.Write("{0:x2}", b);
                //            if (Regex.Match(packet.Layout.Name + "." + block.Layout.Name + "." + field.Layout.Name + " = " + sw, grep, RegexOptions.IgnoreCase).Success)
                //            {
                //                match = true;
                //                break;
                //            }
                //        }
                //    }
                //}

				if (!match) {
					Reset();
					return;
				}
			}

			Console.WriteLine("{0,5} {1} {2}"
					 ,packet.Header.Sequence
					 ,InterpretOptions(packet.Header)
					 ,endpoints
					);
			Console.WriteLine(packet);
		} catch (Exception e) {
			Console.WriteLine(e.Message);
		}

		Reset();
	}

	private static string InterpretOptions(Header header) {
		return "["
		     + (header.AppendedAcks	? "Ack" : "   ")
		     + " "
		     + (header.Resent		? "Res" : "   ")
		     + " "
		     + (header.Reliable		? "Rel" : "   ")
		     + " "
		     + (header.Zerocoded	? "Zer" : "   ")
		     + "]"
		     ;
	}

}
