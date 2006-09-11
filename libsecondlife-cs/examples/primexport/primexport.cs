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
		//
        static void PrimSeen(Simulator simulator, PrimObject prim, U64 regionHandle, ushort timeDilation)
        {
            uint type = 0;
            string output = "";

            output += "<primitive name=\"Object\" description=\"\" key=\"Num_000" + prim.LocalID + "\" version=\"2\">\n";
            output += "<states>\n" +
                "<physics params=\"\">FALSE</physics>\n" +
                "<temporary params=\"\">FALSE</temporary>\n" +
                "<phantom params=\"\">FALSE</phantom>\n" +
                "</states>\n";
            output += "<properties>\n" +
                "<levelofdetail val=\"9\" />\n";

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
            else if (prim.ProfileCurve == 3 && prim.PathCurve == 32)
            {
                type = 6;
            }
            else
            {
                Console.WriteLine("Unhandled prim type, ProfileCurve=" +
                    prim.ProfileCurve + ", PathCurve=" + prim.PathCurve);
                type = 0;
            }

            output += "<type val=\"" + type + "\" />\n";
            output += "<position x=\"" + prim.Position.X +
                "\" y=\"" + prim.Position.Y +
                "\" z=\"" + prim.Position.Z + "\" />\n";
            output += "<rotation x=\"" + prim.Rotation.X +
                "\" y=\"" + prim.Rotation.Y +
                "\" z=\"" + prim.Rotation.Z +
                "\" s=\"" + /* HACK: libsl doesn't set prim.Rotation.S +*/ "1.0\" />\n";
            output += "<size x=\"" + prim.Scale.X +
                "\" y=\"" + prim.Scale.Y +
                "\" z=\"" + prim.Scale.Z + "\" />\n";
            output += "<cut x=\"" + prim.ProfileBegin + "\" y=\"" + prim.ProfileEnd + "\" />\n";
            output += "<dimple x=\"" + prim.PathBegin + "\" y=\"" + prim.PathEnd + "\" />\n";
            output += "<advancedcut x=\"" + prim.ProfileBegin + "\" y=\"" + prim.ProfileEnd + "\" />\n";
            output += "<hollow val=\"" + prim.ProfileHollow + "\" />\n";
            output += "<twist x=\"" + prim.PathTwistBegin + "\" y=\"" + prim.PathTwist + "\" />\n";
            output += "<topsize x=\"" + Math.Abs(prim.PathScaleX - 1.0F) + "\" y=\"" +
                Math.Abs(prim.PathScaleY - 1.0F) + "\" />\n";
            output += "<holesize x=\"" + (1.0F - prim.PathScaleX) + "\" y=\"" + (1.0F - prim.PathScaleY) + "\" />\n";
            output += "<topshear x=\"" + prim.PathShearX + "\" y=\"" + prim.PathShearY + "\" />\n";
            // prim.blender stores taper values a bit different than the SL network layer
            output += "<taper x=\"" + Math.Abs(prim.PathScaleX - 1.0F) + "\" y=\"" +
                Math.Abs(prim.PathScaleY - 1.0F) + "\" />\n";
            output += "<revolutions val=\"" + prim.PathRevolutions + "\" />\n";
            output += "<radiusoffset val=\"" + prim.PathRadiusOffset + "\" />\n";
            output += "<skew val=\"" + prim.PathSkew + "\" />\n";
            output += "<material val=\"" + prim.Material + "\" />\n";
            // TODO: Hollowshape. 16-21 = circle, 32-37 = square, 48-53 = triangle
            output += "<hollowshape val=\"0\" />\n";

            output += "<textures params=\"\">\n" +
                "</textures>\n" +
                "<scripts params=\"\">\n" +
                "</scripts>\n" +
                "</properties>\n" +
                "</primitive>\n";

            Console.WriteLine(output);
        }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            frmPrimExport exportForm = new frmPrimExport();
            exportForm.ShowDialog();
		}
	}
}
