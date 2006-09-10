/*
 * Copyright (c) 2006, Second Life Reverse Engineering Team
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
using System.Collections;
using libsecondlife;

namespace primexport
{
	class primexport
	{
        static bool waiting = true;

		//
        static void PrimSeen(Simulator simulator, PrimObject prim, U64 regionHandle, ushort timeDilation)
        {
            uint type = 0;

            Console.WriteLine("<primitive name=\"Object\" description=\"\" key=\"Num_000" + prim.LocalID + "\" version=\"2\">");
            Console.WriteLine("<states>\n" +
                "<physics params=\"\">FALSE</physics>\n" +
                "<temporary params=\"\">FALSE</temporary>\n" +
                "<phantom params=\"\">FALSE</phantom>\n" +
                "</states>");
            Console.WriteLine("<properties>\n" +
                "<levelofdetail val=\"9\" />");

            if (prim.ProfileCurve == 1 && prim.PathCurve == 16)
            {
                type = 0;
            }
            else if (prim.ProfileCurve == 0 && prim.PathCurve == 16)
            {
                type = 1;
            }
            else if (prim.ProfileCurve == 3 && prim.PathCurve == 16)
            {
                type = 2;
            }
            else if (prim.ProfileCurve == 5 && prim.PathCurve == 32)
            {
                type = 3;
            }
            else if (prim.ProfileCurve == 0 && prim.PathCurve == 32)
            {
                type = 4;
            }
            else if (prim.ProfileCurve == 1 && prim.PathCurve == 32)
            {
                type = 5;
            }
            else if (prim.ProfileCurve == 3 && prim.PathCurve == 16)
            {
                type = 6;
            }
            else
            {
                Console.WriteLine("Unhandled prim type, ProfileCurve=" + 
                    prim.ProfileCurve + ", PathCurve=" + prim.PathCurve);
                type = 0;
            }

            Console.WriteLine("<type val=\"" + type +"\" />");
            Console.WriteLine("<position x=\"" + prim.Position.X + 
                "\" y=\"" + prim.Position.Y +
                "\" z=\"" + prim.Position.Z + "\" />");
            Console.WriteLine("<rotation x=\"" + prim.Rotation.X + 
                "\" y=\"" + prim.Rotation.Y + 
                "\" z=\"" + prim.Rotation.Z + 
                "\" s=\"" + prim.Rotation.S + "\" />");
            Console.WriteLine("<size x=\"" + prim.Scale.X + 
                "\" y=\"" + prim.Scale.Y + 
                "\" z=\"" + prim.Scale.Z + "\" />");
            // cut and dimple may be reversed
            Console.WriteLine("<cut x=\"" + prim.PathBegin + "\" y=\"" + prim.PathEnd + "\" />");
            Console.WriteLine("<dimple x=\"" + prim.ProfileBegin + "\" y=\"" + prim.ProfileEnd + "\" />");
            Console.WriteLine("<advancedcut x=\"" + prim.ProfileBegin + "\" y=\"" + prim.ProfileEnd + "\" />");
            Console.WriteLine("<hollow val=\"" + prim.ProfileHollow + "\" />");
            Console.WriteLine("<twist x=\"" + prim.PathTwistBegin + "\" y=\"" + prim.PathTwist + "\" />");
            Console.WriteLine("<topsize x=\"" + prim.PathScaleX + "\" y=\"" + prim.PathScaleY + "\" />");
            Console.WriteLine("<holesize x=\"" + prim.PathScaleX + "\" y=\"" + prim.PathScaleY + "\" />");
            Console.WriteLine("<topshear x=\"" + prim.PathShearX + "\" y=\"" + prim.PathShearY + "\" />");
            Console.WriteLine("<taper x=\"" + prim.PathTaperX + "\" y=\"" + prim.PathTaperY + "\" />");
            Console.WriteLine("<revolutions val=\"" + prim.PathRevolutions + "\" />");
            Console.WriteLine("<radiusoffset val=\"" + prim.PathRadiusOffset + "\" />");
            Console.WriteLine("<skew val=\"" + prim.PathSkew + "\" />");
            Console.WriteLine("<material val=\"" + prim.Material + "\" />");
            // TODO: Hollowshape. 16-21 = circle, 32-37 = square, 48-53 = triangle
            Console.WriteLine("<hollowshape val=\"0\" />");

            Console.WriteLine("<textures params=\"\">\n" +
                "</textures>\n" +
                "<scripts params=\"\">\n" +
                "</scripts>\n" +
                "</properties>\n" +
                "</primitive>\n\n");
        }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			SecondLife client;

			if (args.Length < 3)
			{
				Console.WriteLine("Usage: primexport [firstname] [lastname] [password]");
				return;
			}

			try
			{
				client = new SecondLife("keywords.txt", "protocol.txt");
			}
			catch (Exception e)
			{
				// Error initializing the client, probably missing file(s)
				Console.WriteLine(e.ToString());
				return;
			}

			// Register an event handler for new objects
            client.Objects.OnNewPrim += new NewPrimCallback(PrimSeen);

			// Setup the login values
			Hashtable loginParams = NetworkManager.DefaultLoginValues(args[0], args[1], args[2], "00:00:00:00:00:00",
				"last", 1, 50, 50, 50, "Win", "0", "primexport", "jhurliman@wsu.edu");

			if (!client.Network.Login(loginParams))
			{
				// Login failed
				Console.WriteLine("ERROR: " + client.Network.LoginError);
				return;
			}

			while (waiting)
			{
				client.Tick();
			}

			client.Network.Logout();
		}
	}
}
