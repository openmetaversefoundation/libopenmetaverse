/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using ExtensionLoader;
using OpenMetaverse;

using LSL_Float = Simian.ScriptTypes.LSL_Float;
using LSL_Integer = Simian.ScriptTypes.LSL_Integer;
using LSL_Key = Simian.ScriptTypes.LSL_Key;
using LSL_List = Simian.ScriptTypes.LSL_List;
using LSL_Rotation = Simian.ScriptTypes.LSL_Rotation;
using LSL_String = Simian.ScriptTypes.LSL_String;
using LSL_Vector = Simian.ScriptTypes.LSL_Vector;

namespace Simian.Extensions
{
    /// <summary>
    /// Contains all LSL ll-functions. This class will be in Default AppDomain.
    /// </summary>
    public class ScriptApi : IScriptApi
    {
        const bool ERROR_ON_NOT_IMPLEMENTED = true;
        const float SCRIPT_DELAY_FACTOR = 1.0f;
        const float SCRIPT_DISTANCE_FACTOR = 1.0f;
        const float MIN_TIMER_INTERVAL = 0.5f;

        #region Base64 Data

        //  <remarks>
        //  <para>
        //  The .NET definition of base 64 is:
        //  <list>
        //  <item>
        //  Significant: A-Z a-z 0-9 + -
        //  </item>
        //  <item>
        //  Whitespace: \t \n \r ' '
        //  </item>
        //  <item>
        //  Valueless: =
        //  </item>
        //  <item>
        //  End-of-string: \0 or '=='
        //  </item>
        //  </list>
        //  </para>
        //  <para>
        //  Each point in a base-64 string represents
        //  a 6 bit value. A 32-bit integer can be
        //  represented using 6 characters (with some
        //  redundancy).
        //  </para>
        //  <para>
        //  LSL requires a base64 string to be 8
        //  characters in length. LSL also uses '/'
        //  rather than '-' (MIME compliant).
        //  </para>
        //  <para>
        //  RFC 1341 used as a reference (as specified
        //  by the SecondLife Wiki).
        //  </para>
        //  <para>
        //  SL do not record any kind of exception for
        //  these functions, so the string to integer
        //  conversion returns '0' if an invalid
        //  character is encountered during conversion.
        //  </para>
        //  <para>
        //  References
        //  <list>
        //  <item>
        //  http://lslwiki.net/lslwiki/wakka.php?wakka=Base64
        //  </item>
        //  <item>
        //  </item>
        //  </list>
        //  </para>
        //  </remarks>
        //  <summary>
        //  Table for converting 6-bit integers into
        //  base-64 characters
        //  </summary>
        private static readonly char[] i2ctable =
        {
            'A','B','C','D','E','F','G','H',
            'I','J','K','L','M','N','O','P',
            'Q','R','S','T','U','V','W','X',
            'Y','Z',
            'a','b','c','d','e','f','g','h',
            'i','j','k','l','m','n','o','p',
            'q','r','s','t','u','v','w','x',
            'y','z',
            '0','1','2','3','4','5','6','7',
            '8','9',
            '+','/'
        };

        //  <summary>
        //  Table for converting base-64 characters
        //  into 6-bit integers.
        //  </summary>
        private static readonly int[] c2itable =
        {
            -1,-1,-1,-1,-1,-1,-1,-1,    // 0x
            -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,    // 1x
            -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,    // 2x
            -1,-1,-1,63,-1,-1,-1,64,
            53,54,55,56,57,58,59,60,    // 3x
            61,62,-1,-1,-1,0,-1,-1,
            -1,1,2,3,4,5,6,7,           // 4x
            8,9,10,11,12,13,14,15,
            16,17,18,19,20,21,22,23,    // 5x
            24,25,26,-1,-1,-1,-1,-1,
            -1,27,28,29,30,31,32,33,    // 6x
            34,35,36,37,38,39,40,41,
            42,43,44,45,46,47,48,49,    // 7x
            50,51,52,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,    // 8x
            -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,    // 9x
            -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,    // Ax
            -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,    // Bx
            -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,    // Cx
            -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,    // Dx
            -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,    // Ex
            -1,-1,-1,-1,-1,-1,-1,-1,
            -1,-1,-1,-1,-1,-1,-1,-1,    // Fx
            -1,-1,-1,-1,-1,-1,-1,-1
        };

        #endregion Base64 Data

        Simian server;
        SimulationObject hostObject;
        UUID scriptID;
        bool isGodMode;
        bool automaticLinkPermission;
        DateTime startTime = DateTime.Now;

        public ScriptApi()
        {
        }

        public void Start(Simian server, SimulationObject hostObject, UUID scriptID, bool isGodMode, bool automaticLinkPermission)
        {
            this.server = server;
            this.hostObject = hostObject;
            this.scriptID = scriptID;
            this.isGodMode = isGodMode;
            this.automaticLinkPermission = automaticLinkPermission;
        }

        public void Stop()
        {
        }

        /*public void Initialize(IScriptEngine ScriptEngine, SimulationObject host, uint localID, UUID itemID)
        {
            m_ScriptEngine = ScriptEngine;
            m_host = host;
            m_localID = localID;
            m_itemID = itemID;

            scriptDelayFactor =
                m_ScriptEngine.Config.GetFloat("ScriptDelayFactor", 1.0f);
            m_ScriptDistanceFactor =
                m_ScriptEngine.Config.GetFloat("ScriptDistanceLimitFactor", 1.0f);
            m_MinTimerInterval =
                m_ScriptEngine.Config.GetFloat("MinTimerInterval", 0.5f);
            m_automaticLinkPermission =
                m_ScriptEngine.Config.GetBoolean("AutomaticLinkPermission", false);

            m_TransferModule =
                    m_ScriptEngine.World.RequestModuleInterface<IMessageTransferModule>();
            AsyncCommands = new AsyncCommandManager(ScriptEngine);
        }*/

        void ScriptSleep(int delay)
        {
            delay = (int)((float)delay * SCRIPT_DELAY_FACTOR);
            if (delay == 0)
                return;
            System.Threading.Thread.Sleep(delay);
        }

        #region ll* Functions

        public void state(string newState)
        {
            server.ScriptEngine.TriggerState(scriptID, newState);
        }

        public void llResetScript()
        {
            hostObject.AddScriptLPS(1);
            server.ScriptEngine.ApiResetScript(scriptID);
        }

        public void llResetOtherScript(string name)
        {
            UUID otherScriptID;
            hostObject.AddScriptLPS(1);

            if ((otherScriptID = ScriptByName(name)) != UUID.Zero)
                server.ScriptEngine.ResetScript(otherScriptID);
            else
                ShoutError("llResetOtherScript: script " + name + " not found");
        }

        public LSL_Integer llGetScriptState(string name)
        {
            UUID otherScriptID;
            hostObject.AddScriptLPS(1);

            if ((otherScriptID = ScriptByName(name)) != UUID.Zero)
            {
                return server.ScriptEngine.GetScriptState(otherScriptID) ? 1 : 0;
            }

            ShoutError("llGetScriptState: script " + name + " not found");

            // If we didn't find it, then it's safe to
            // assume it is not running.
            return 0;
        }

        public void llSetScriptState(string name, int run)
        {
            UUID otherScriptID;
            hostObject.AddScriptLPS(1);

            if ((otherScriptID = ScriptByName(name)) != UUID.Zero)
            {
                server.ScriptEngine.SetScriptState(otherScriptID, run == 0 ? false : true);
            }
            else
            {
                ShoutError("llSetScriptState: script " + name + " not found");
            }
        }

        public LSL_Float llSin(double f)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Sin(f);
        }

        public LSL_Float llCos(double f)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Cos(f);
        }

        public LSL_Float llTan(double f)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Tan(f);
        }

        public LSL_Float llAtan2(double x, double y)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Atan2(x, y);
        }

        public LSL_Float llSqrt(double f)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Sqrt(f);
        }

        public LSL_Float llPow(double fbase, double fexponent)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Pow(fbase, fexponent);
        }

        public LSL_Integer llAbs(int i)
        {
            // changed to replicate LSL behaviour whereby minimum int value is returned untouched.
            hostObject.AddScriptLPS(1);
            if (i == Int32.MinValue)
                return i;
            else
                return (int)Math.Abs(i);
        }

        public LSL_Float llFabs(double f)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Abs(f);
        }

        public LSL_Float llFrand(double mag)
        {
            hostObject.AddScriptLPS(1);
            return Utils.RandomDouble() * mag;
        }

        public LSL_Integer llFloor(double f)
        {
            hostObject.AddScriptLPS(1);
            return (int)Math.Floor(f);
        }

        public LSL_Integer llCeil(double f)
        {
            hostObject.AddScriptLPS(1);
            return (int)Math.Ceiling(f);
        }

        // Xantor 01/May/2008 fixed midpointrounding (2.5 becomes 3.0 instead of 2.0, default = ToEven)
        public LSL_Integer llRound(double f)
        {
            hostObject.AddScriptLPS(1);
            return (int)Math.Round(f, MidpointRounding.AwayFromZero);
        }

        //This next group are vector operations involving squaring and square root. ckrinke
        public LSL_Float llVecMag(LSL_Vector v)
        {
            hostObject.AddScriptLPS(1);
            return LSL_Vector.Mag(v);
        }

        public LSL_Vector llVecNorm(LSL_Vector v)
        {
            hostObject.AddScriptLPS(1);
            double mag = LSL_Vector.Mag(v);
            LSL_Vector nor = new LSL_Vector();
            nor.x = v.x / mag;
            nor.y = v.y / mag;
            nor.z = v.z / mag;
            return nor;
        }

        public LSL_Float llVecDist(LSL_Vector a, LSL_Vector b)
        {
            hostObject.AddScriptLPS(1);
            double dx = a.x - b.x;
            double dy = a.y - b.y;
            double dz = a.z - b.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public LSL_Vector llRot2Euler(LSL_Rotation r)
        {
            hostObject.AddScriptLPS(1);
            //This implementation is from http://lslwiki.net/lslwiki/wakka.php?wakka=LibraryRotationFunctions. ckrinke
            LSL_Rotation t = new LSL_Rotation(r.x * r.x, r.y * r.y, r.z * r.z, r.s * r.s);
            double m = (t.x + t.y + t.z + t.s);
            if (m == 0) return new LSL_Vector();
            double n = 2 * (r.y * r.s + r.x * r.z);
            double p = m * m - n * n;
            if (p > 0)
                return new LSL_Vector(NormalizeAngle(Math.Atan2(2.0 * (r.x * r.s - r.y * r.z), (-t.x - t.y + t.z + t.s))),
                                             NormalizeAngle(Math.Atan2(n, Math.Sqrt(p))),
                                             NormalizeAngle(Math.Atan2(2.0 * (r.z * r.s - r.x * r.y), (t.x - t.y - t.z + t.s))));
            else if (n > 0)
                return new LSL_Vector(0.0, Math.PI / 2, NormalizeAngle(Math.Atan2((r.z * r.s + r.x * r.y), 0.5 - t.x - t.z)));
            else
                return new LSL_Vector(0.0, -Math.PI / 2, NormalizeAngle(Math.Atan2((r.z * r.s + r.x * r.y), 0.5 - t.x - t.z)));
        }

        /* From wiki:
         * The Euler angle vector (in radians) is converted to a rotation by doing the rotations around the 3 axes
         * in Z, Y, X order. So llEuler2Rot(<1.0, 2.0, 3.0> * DEG_TO_RAD) generates a rotation by taking the zero rotation,
         * a vector pointing along the X axis, first rotating it 3 degrees around the global Z axis, then rotating the resulting
         * vector 2 degrees around the global Y axis, and finally rotating that 1 degree around the global X axis.
         */

        /* How we arrived at this llEuler2Rot
         *
         * Experiment in SL to determine conventions:
         *   llEuler2Rot(<PI,0,0>)=<1,0,0,0>
         *   llEuler2Rot(<0,PI,0>)=<0,1,0,0>
         *   llEuler2Rot(<0,0,PI>)=<0,0,1,0>
         *
         * Important facts about Quaternions
         *  - multiplication is non-commutative (a*b != b*a)
         *  - http://en.wikipedia.org/wiki/Quaternion#Basis_multiplication
         *
         * Above SL experiment gives (c1,c2,c3,s1,s2,s3 as defined in our llEuler2Rot):
         *   Qx = c1+i*s1
         *   Qy = c2+j*s2;
         *   Qz = c3+k*s3;
         *
         * Rotations applied in order (from above) Z, Y, X
         * Q = (Qz * Qy) * Qx
         * ((c1+i*s1)*(c2+j*s2))*(c3+k*s3)
         * (c1*c2+i*s1*c2+j*c1*s2+ij*s1*s2)*(c3+k*s3)
         * (c1*c2+i*s1*c2+j*c1*s2+k*s1*s2)*(c3+k*s3)
         * c1*c2*c3+i*s1*c2*c3+j*c1*s2*c3+k*s1*s2*c3+k*c1*c2*s3+ik*s1*c2*s3+jk*c1*s2*s3+kk*s1*s2*s3
         * c1*c2*c3+i*s1*c2*c3+j*c1*s2*c3+k*s1*s2*c3+k*c1*c2*s3 -j*s1*c2*s3 +i*c1*s2*s3   -s1*s2*s3
         * regroup: x=i*(s1*c2*c3+c1*s2*s3)
         *          y=j*(c1*s2*c3-s1*c2*s3)
         *          z=k*(s1*s2*c3+c1*c2*s3)
         *          s=   c1*c2*c3-s1*s2*s3
         *
         * This implementation agrees with the functions found here:
         * http://lslwiki.net/lslwiki/wakka.php?wakka=LibraryRotationFunctions
         * And with the results in SL.
         *
         * It's also possible to calculate llEuler2Rot by direct multiplication of
         * the Qz, Qy, and Qx vectors (as above - and done in the "accurate" function
         * from the wiki).
         * Apparently in some cases this is better from a numerical precision perspective?
         */

        public LSL_Rotation llEuler2Rot(LSL_Vector v)
        {
            hostObject.AddScriptLPS(1);

            double x, y, z, s;

            double c1 = Math.Cos(v.x / 2.0);
            double c2 = Math.Cos(v.y / 2.0);
            double c3 = Math.Cos(v.z / 2.0);
            double s1 = Math.Sin(v.x / 2.0);
            double s2 = Math.Sin(v.y / 2.0);
            double s3 = Math.Sin(v.z / 2.0);

            x = s1 * c2 * c3 + c1 * s2 * s3;
            y = c1 * s2 * c3 - s1 * c2 * s3;
            z = s1 * s2 * c3 + c1 * c2 * s3;
            s = c1 * c2 * c3 - s1 * s2 * s3;

            return new LSL_Rotation(x, y, z, s);
        }

        public LSL_Rotation llAxes2Rot(LSL_Vector fwd, LSL_Vector left, LSL_Vector up)
        {
            hostObject.AddScriptLPS(1);
            double s;
            double tr = fwd.x + left.y + up.z + 1.0;

            if (tr >= 1.0)
            {
                s = 0.5 / Math.Sqrt(tr);
                return new LSL_Rotation(
                        (left.z - up.y) * s,
                        (up.x - fwd.z) * s,
                        (fwd.y - left.x) * s,
                        0.25 / s);
            }
            else
            {
                double max = (left.y > up.z) ? left.y : up.z;

                if (max < fwd.x)
                {
                    s = Math.Sqrt(fwd.x - (left.y + up.z) + 1.0);
                    double x = s * 0.5;
                    s = 0.5 / s;
                    return new LSL_Rotation(
                            x,
                            (fwd.y + left.x) * s,
                            (up.x + fwd.z) * s,
                            (left.z - up.y) * s);
                }
                else if (max == left.y)
                {
                    s = Math.Sqrt(left.y - (up.z + fwd.x) + 1.0);
                    double y = s * 0.5;
                    s = 0.5 / s;
                    return new LSL_Rotation(
                            (fwd.y + left.x) * s,
                            y,
                            (left.z + up.y) * s,
                            (up.x - fwd.z) * s);
                }
                else
                {
                    s = Math.Sqrt(up.z - (fwd.x + left.y) + 1.0);
                    double z = s * 0.5;
                    s = 0.5 / s;
                    return new LSL_Rotation(
                            (up.x + fwd.z) * s,
                            (left.z + up.y) * s,
                            z,
                            (fwd.y - left.x) * s);
                }
            }
        }

        public LSL_Vector llRot2Fwd(LSL_Rotation r)
        {
            hostObject.AddScriptLPS(1);

            double x, y, z, m;

            m = r.x * r.x + r.y * r.y + r.z * r.z + r.s * r.s;
            // m is always greater than zero
            // if m is not equal to 1 then Rotation needs to be normalized
            if (Math.Abs(1.0 - m) > 0.000001) // allow a little slop here for calculation precision
            {
                m = 1.0 / Math.Sqrt(m);
                r.x *= m;
                r.y *= m;
                r.z *= m;
                r.s *= m;
            }

            // Fast Algebric Calculations instead of Vectors & Quaternions Product
            x = r.x * r.x - r.y * r.y - r.z * r.z + r.s * r.s;
            y = 2 * (r.x * r.y + r.z * r.s);
            z = 2 * (r.x * r.z - r.y * r.s);
            return (new LSL_Vector(x, y, z));
        }

        public LSL_Vector llRot2Left(LSL_Rotation r)
        {
            hostObject.AddScriptLPS(1);

            double x, y, z, m;

            m = r.x * r.x + r.y * r.y + r.z * r.z + r.s * r.s;
            // m is always greater than zero
            // if m is not equal to 1 then Rotation needs to be normalized
            if (Math.Abs(1.0 - m) > 0.000001) // allow a little slop here for calculation precision
            {
                m = 1.0 / Math.Sqrt(m);
                r.x *= m;
                r.y *= m;
                r.z *= m;
                r.s *= m;
            }

            // Fast Algebric Calculations instead of Vectors & Quaternions Product
            x = 2 * (r.x * r.y - r.z * r.s);
            y = -r.x * r.x + r.y * r.y - r.z * r.z + r.s * r.s;
            z = 2 * (r.x * r.s + r.y * r.z);
            return (new LSL_Vector(x, y, z));
        }

        public LSL_Vector llRot2Up(LSL_Rotation r)
        {
            hostObject.AddScriptLPS(1);
            double x, y, z, m;

            m = r.x * r.x + r.y * r.y + r.z * r.z + r.s * r.s;
            // m is always greater than zero
            // if m is not equal to 1 then Rotation needs to be normalized
            if (Math.Abs(1.0 - m) > 0.000001) // allow a little slop here for calculation precision
            {
                m = 1.0 / Math.Sqrt(m);
                r.x *= m;
                r.y *= m;
                r.z *= m;
                r.s *= m;
            }

            // Fast Algebric Calculations instead of Vectors & Quaternions Product
            x = 2 * (r.x * r.z + r.y * r.s);
            y = 2 * (-r.x * r.s + r.y * r.z);
            z = -r.x * r.x - r.y * r.y + r.z * r.z + r.s * r.s;
            return (new LSL_Vector(x, y, z));
        }

        public LSL_Rotation llRotBetween(LSL_Vector a, LSL_Vector b)
        {
            //A and B should both be normalized
            hostObject.AddScriptLPS(1);
            double dotProduct = LSL_Vector.Dot(a, b);
            LSL_Vector crossProduct = LSL_Vector.Cross(a, b);
            double magProduct = LSL_Vector.Mag(a) * LSL_Vector.Mag(b);
            double angle = Math.Acos(dotProduct / magProduct);
            LSL_Vector axis = LSL_Vector.Norm(crossProduct);
            double s = Math.Sin(angle / 2);

            double x = axis.x * s;
            double y = axis.y * s;
            double z = axis.z * s;
            double w = Math.Cos(angle / 2);

            if (Double.IsNaN(x) || Double.IsNaN(y) || Double.IsNaN(z) || Double.IsNaN(w))
                return new LSL_Rotation(0.0f, 0.0f, 0.0f, 1.0f);

            return new LSL_Rotation((float)x, (float)y, (float)z, (float)w);
        }

        public void llWhisper(int channelID, string text)
        {
            hostObject.AddScriptLPS(1);

            if (text.Length > 1023)
                text = text.Substring(0, 1023);

            server.Scene.ObjectChat(this, hostObject.Prim.Properties.OwnerID, hostObject.Prim.ID, ChatAudibleLevel.Fully, ChatType.Whisper,
                ChatSourceType.Object, hostObject.Prim.Properties.Name, hostObject.GetSimulatorPosition(), channelID, text);
        }

        public void llSay(int channelID, string text)
        {
            hostObject.AddScriptLPS(1);

            if (text.Length > 1023)
                text = text.Substring(0, 1023);

            server.Scene.ObjectChat(this, hostObject.Prim.Properties.OwnerID, hostObject.Prim.ID, ChatAudibleLevel.Fully, ChatType.Normal,
                ChatSourceType.Object, hostObject.Prim.Properties.Name, hostObject.GetSimulatorPosition(), channelID, text);
        }

        public void llShout(int channelID, string text)
        {
            hostObject.AddScriptLPS(1);

            if (text.Length > 1023)
                text = text.Substring(0, 1023);

            server.Scene.ObjectChat(this, hostObject.Prim.Properties.OwnerID, hostObject.Prim.ID, ChatAudibleLevel.Fully, ChatType.Shout,
                ChatSourceType.Object, hostObject.Prim.Properties.Name, hostObject.GetSimulatorPosition(), channelID, text);
        }

        public void llRegionSay(int channelID, string text)
        {
            hostObject.AddScriptLPS(1);

            if (channelID == 0)
            {
                LSLError("Cannot use llRegionSay() on channel 0");
                return;
            }

            if (text.Length > 1023)
                text = text.Substring(0, 1023);

            server.Scene.ObjectChat(this, hostObject.Prim.Properties.OwnerID, hostObject.Prim.ID, ChatAudibleLevel.Fully, ChatType.RegionSay,
                ChatSourceType.Object, hostObject.Prim.Properties.Name, hostObject.GetSimulatorPosition(), channelID, text);
        }

        public LSL_Integer llListen(int channelID, string name, string ID, string msg)
        {
            hostObject.AddScriptLPS(1);

            UUID keyID;
            UUID.TryParse(ID, out keyID);

            return server.ScriptEngine.AddListener(scriptID, hostObject.Prim.ID, channelID, name, keyID, msg);
        }

        public void llListenControl(int number, int active)
        {
            hostObject.AddScriptLPS(1);
            server.ScriptEngine.SetListenerState(scriptID, number, active == 0 ? false : true);
        }

        public void llListenRemove(int number)
        {
            hostObject.AddScriptLPS(1);
            server.ScriptEngine.RemoveListener(scriptID, number);
        }

        public void llSensor(string name, string id, int type, double range, double arc)
        {
            hostObject.AddScriptLPS(1);

            UUID keyID = UUID.Zero;
            UUID.TryParse(id, out keyID);

            server.ScriptEngine.SensorOnce(scriptID, hostObject.Prim.ID, name, keyID, type, range, arc);
        }

        public void llSensorRepeat(string name, string id, int type, double range, double arc, double rate)
        {
            hostObject.AddScriptLPS(1);

            UUID keyID = UUID.Zero;
            UUID.TryParse(id, out keyID);

            server.ScriptEngine.SensorRepeat(scriptID, hostObject.Prim.ID, name, keyID, type, range, arc, rate);
        }

        public void llSensorRemove()
        {
            hostObject.AddScriptLPS(1);
            server.ScriptEngine.SensorRemove(scriptID);
        }

        public LSL_String llDetectedName(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_String.Empty;
            return detectedParams.Name;
        }

        public LSL_Key llDetectedKey(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_Key.Zero;
            return detectedParams.Key;
        }

        public LSL_Key llDetectedOwner(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_Key.Zero;
            return detectedParams.Owner;
        }

        public LSL_Integer llDetectedType(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_Integer.Zero;
            return detectedParams.Type;
        }

        public LSL_Vector llDetectedPos(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_Vector.Zero;
            return detectedParams.Position;
        }

        public LSL_Vector llDetectedVel(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_Vector.Zero;
            return detectedParams.Velocity;
        }

        public LSL_Vector llDetectedGrab(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_Vector.Zero;
            return detectedParams.Offset;
        }

        public LSL_Rotation llDetectedRot(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_Rotation.Identity;
            return detectedParams.Rotation;
        }

        public LSL_Integer llDetectedGroup(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return ScriptTypes.FALSE;
            if (hostObject.Prim.GroupID.ToString() == detectedParams.Group.value)
                return ScriptTypes.TRUE;
            return ScriptTypes.FALSE;
        }

        public LSL_Integer llDetectedLinkNumber(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams parms = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (parms == null)
                return LSL_Integer.Zero;
            return parms.LinkNum;
        }

        /// <summary>
        /// See http://wiki.secondlife.com/wiki/LlDetectedTouchBinormal for details
        /// </summary>
        public LSL_Vector llDetectedTouchBinormal(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_Vector.Zero;
            return detectedParams.TouchBinormal;
        }

        /// <summary>
        /// See http://wiki.secondlife.com/wiki/LlDetectedTouchFace for details
        /// </summary>
        public LSL_Integer llDetectedTouchFace(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return ScriptTypes.TOUCH_INVALID_FACE;
            return detectedParams.TouchFace;
        }

        /// <summary>
        /// See http://wiki.secondlife.com/wiki/LlDetectedTouchNormal for details
        /// </summary>
        public LSL_Vector llDetectedTouchNormal(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_Vector.Zero;
            return detectedParams.TouchNormal;
        }

        /// <summary>
        /// See http://wiki.secondlife.com/wiki/LlDetectedTouchPos for details
        /// </summary>
        public LSL_Vector llDetectedTouchPos(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return LSL_Vector.Zero;
            return detectedParams.TouchPos;
        }

        /// <summary>
        /// See http://wiki.secondlife.com/wiki/LlDetectedTouchST for details
        /// </summary>
        public LSL_Vector llDetectedTouchST(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);
            if (detectedParams == null)
                return ScriptTypes.TOUCH_INVALID_TEXCOORD;
            return detectedParams.TouchST;
        }

        /// <summary>
        /// See http://wiki.secondlife.com/wiki/LlDetectedTouchUV for details
        /// </summary>
        public LSL_Vector llDetectedTouchUV(int number)
        {
            hostObject.AddScriptLPS(1);
            DetectParams detectedParams = server.ScriptEngine.GetDetectParams(scriptID, number);

            if (detectedParams == null)
                return ScriptTypes.TOUCH_INVALID_TEXCOORD;
            return detectedParams.TouchUV;
        }

        public void llDie()
        {
            hostObject.AddScriptLPS(1);
            throw new ScriptSelfDeleteException();
        }

        public LSL_Float llGround(LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            Vector3 pos = hostObject.GetSimulatorPosition();

            float x = pos.X + (float)offset.x;
            float y = pos.Y + (float)offset.y;

            // Clamp to valid position
            float simWidth = (float)(server.Scene.TerrainPatchCountWidth * server.Scene.TerrainPatchWidth);
            float simHeight = (float)(server.Scene.TerrainPatchCountHeight * server.Scene.TerrainPatchHeight);

            if (x < 0.0f)
                x = 0.0f;
            else if (x >= simWidth)
                x = simWidth - 1.0f;
            if (y < 0.0f)
                y = 0.0f;
            else if (y >= simHeight)
                y = simHeight - 1.0f;

            return server.Scene.GetTerrainHeightAt(x, y);
        }

        public LSL_Float llCloud(LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            //FIXME:
            return LSL_Float.Zero;
        }

        public LSL_Vector llWind(LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            Vector2 windSpeed = server.Scene.GetWindSpeedAt((float)offset.x, (float)offset.y);
            return new LSL_Vector(windSpeed.X, windSpeed.Y, 0.0);
        }

        public void llSetStatus(int status, int value)
        {
            hostObject.AddScriptLPS(1);

            int statusrotationaxis = 0;

            if ((status & ScriptTypes.STATUS_PHYSICS) == ScriptTypes.STATUS_PHYSICS)
            {
                if (value == 1)
                    server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags | PrimFlags.Physics);
                else
                    server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags & ~PrimFlags.Physics);
            }

            if ((status & ScriptTypes.STATUS_PHANTOM) == ScriptTypes.STATUS_PHANTOM)
            {
                if (value == 1)
                    server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags | PrimFlags.Phantom);
                else
                    server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags & ~PrimFlags.Phantom);
            }

            if ((status & ScriptTypes.STATUS_CAST_SHADOWS) == ScriptTypes.STATUS_CAST_SHADOWS)
            {
                server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags | PrimFlags.CastShadows);
            }

            if ((status & ScriptTypes.STATUS_ROTATE_X) == ScriptTypes.STATUS_ROTATE_X)
            {
                statusrotationaxis |= ScriptTypes.STATUS_ROTATE_X;
            }

            if ((status & ScriptTypes.STATUS_ROTATE_Y) == ScriptTypes.STATUS_ROTATE_Y)
            {
                statusrotationaxis |= ScriptTypes.STATUS_ROTATE_Y;
            }

            if ((status & ScriptTypes.STATUS_ROTATE_Z) == ScriptTypes.STATUS_ROTATE_Z)
            {
                statusrotationaxis |= ScriptTypes.STATUS_ROTATE_Z;
            }

            if ((status & ScriptTypes.STATUS_BLOCK_GRAB) == ScriptTypes.STATUS_BLOCK_GRAB)
            {
                NotImplemented("llSetStatus - STATUS_BLOCK_GRAB");
            }

            if ((status & ScriptTypes.STATUS_DIE_AT_EDGE) == ScriptTypes.STATUS_DIE_AT_EDGE)
            {
                if (value == 1)
                    server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags | PrimFlags.DieAtEdge);
                else
                    server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags & ~PrimFlags.DieAtEdge);
            }

            if ((status & ScriptTypes.STATUS_RETURN_AT_EDGE) == ScriptTypes.STATUS_RETURN_AT_EDGE)
            {
                if (value == 1)
                    server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags | PrimFlags.ReturnAtEdge);
                else
                    server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags & ~PrimFlags.ReturnAtEdge);
            }

            if ((status & ScriptTypes.STATUS_SANDBOX) == ScriptTypes.STATUS_SANDBOX)
            {
                if (value == 1)
                    server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags | PrimFlags.Sandbox);
                else
                    server.Scene.ObjectFlags(this, hostObject, hostObject.Prim.Flags & ~PrimFlags.Sandbox);
            }

            if (statusrotationaxis != 0)
            {
                const int X_AXIS = 2;
                const int Y_AXIS = 4;
                const int Z_AXIS = 8;

                SimulationObject parent = hostObject.GetLinksetParent();
                float setValue = (value > 0) ? 1f : 0f;

                Vector3 rotationAxis = parent.RotationAxis;

                if ((statusrotationaxis & X_AXIS) != 0) rotationAxis.X = setValue;
                if ((statusrotationaxis & Y_AXIS) != 0) rotationAxis.Y = setValue;
                if ((statusrotationaxis & Z_AXIS) != 0) rotationAxis.Z = setValue;

                server.Scene.ObjectSetRotationAxis(this, parent, rotationAxis);
            }
        }

        public LSL_Integer llGetStatus(int status)
        {
            hostObject.AddScriptLPS(1);

            switch (status)
            {
                case ScriptTypes.STATUS_PHYSICS:
                    if ((hostObject.Prim.Flags & PrimFlags.Physics) == PrimFlags.Physics)
                        return 1;
                    return 0;
                case ScriptTypes.STATUS_PHANTOM:
                    if ((hostObject.Prim.Flags & PrimFlags.Phantom) == PrimFlags.Phantom)
                        return 1;
                    return 0;
                case ScriptTypes.STATUS_CAST_SHADOWS:
                    if ((hostObject.Prim.Flags & PrimFlags.CastShadows) == PrimFlags.CastShadows)
                        return 1;
                    return 0;
                case ScriptTypes.STATUS_BLOCK_GRAB:
                    NotImplemented("llGetStatus - STATUS_BLOCK_GRAB");
                    return 0;
                case ScriptTypes.STATUS_DIE_AT_EDGE:
                    if ((hostObject.Prim.Flags & PrimFlags.DieAtEdge) == PrimFlags.DieAtEdge)
                        return 1;
                    return 0;
                case ScriptTypes.STATUS_RETURN_AT_EDGE:
                    if ((hostObject.Prim.Flags & PrimFlags.ReturnAtEdge) == PrimFlags.ReturnAtEdge)
                        return 1;
                    return 0;
                case ScriptTypes.STATUS_ROTATE_X:
                    NotImplemented("llGetStatus - STATUS_ROTATE_X");
                    return 0;
                case ScriptTypes.STATUS_ROTATE_Y:
                    NotImplemented("llGetStatus - STATUS_ROTATE_Y");
                    return 0;
                case ScriptTypes.STATUS_ROTATE_Z:
                    NotImplemented("llGetStatus - STATUS_ROTATE_Z");
                    return 0;
                case ScriptTypes.STATUS_SANDBOX:
                    if ((hostObject.Prim.Flags & PrimFlags.Sandbox) == PrimFlags.Sandbox)
                        return 1;
                    return 0;
            }

            return 0;
        }

        public void llSetScale(LSL_Vector scale)
        {
            hostObject.AddScriptLPS(1);

            // TODO: Apply constraints
            hostObject.Prim.Scale = new Vector3((float)scale.x, (float)scale.y, (float)scale.z);
            server.Scene.ObjectAdd(this, hostObject, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        public LSL_Vector llGetScale()
        {
            hostObject.AddScriptLPS(1);
            return new LSL_Vector(hostObject.Prim.Scale.X, hostObject.Prim.Scale.Y, hostObject.Prim.Scale.Z);
        }

        public void llSetClickAction(int action)
        {
            hostObject.AddScriptLPS(1);

            hostObject.Prim.ClickAction = (ClickAction)action;
            server.Scene.ObjectAdd(this, hostObject, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        public void llSetColor(LSL_Vector color, int face)
        {
            hostObject.AddScriptLPS(1);
            SetColor(hostObject, color, face);
        }

        public LSL_Float llGetAlpha(int face)
        {
            hostObject.AddScriptLPS(1);
            return GetAlpha(hostObject, face);
        }

        public void llSetAlpha(double alpha, int face)
        {
            hostObject.AddScriptLPS(1);
            SetAlpha(hostObject, alpha, face);
        }

        public void llSetLinkAlpha(int linknumber, double alpha, int face)
        {
            hostObject.AddScriptLPS(1);

            List<SimulationObject> parts = GetLinkParts(linknumber);

            foreach (SimulationObject part in parts)
                SetAlpha(part, alpha, face);
        }

        public LSL_Vector llGetColor(int face)
        {
            hostObject.AddScriptLPS(1);
            return GetColor(hostObject, face);
        }

        public void llSetTexture(string texture, int face)
        {
            hostObject.AddScriptLPS(1);
            SetTexture(hostObject, texture, face);
            // ScriptSleep(200);
        }

        public void llSetLinkTexture(int linknumber, string texture, int face)
        {
            hostObject.AddScriptLPS(1);

            List<SimulationObject> parts = GetLinkParts(linknumber);

            foreach (SimulationObject part in parts)
                SetTexture(part, texture, face);

            // ScriptSleep(200);
        }

        public void llScaleTexture(double u, double v, int face)
        {
            hostObject.AddScriptLPS(1);
            ScaleTexture(hostObject, u, v, face);
            // ScriptSleep(200);
        }

        public void llOffsetTexture(double u, double v, int face)
        {
            hostObject.AddScriptLPS(1);
            OffsetTexture(hostObject, u, v, face);
            // ScriptSleep(200);
        }

        public void llRotateTexture(double rotation, int face)
        {
            hostObject.AddScriptLPS(1);
            RotateTexture(hostObject, rotation, face);
            // ScriptSleep(200);
        }

        public LSL_String llGetTexture(int face)
        {
            hostObject.AddScriptLPS(1);
            return GetTexture(hostObject, face);
        }

        public void llSetPos(LSL_Vector pos)
        {
            hostObject.AddScriptLPS(1);

            SetPos(hostObject, pos);
            ScriptSleep(200);
        }

        public LSL_Vector llGetPos()
        {
            hostObject.AddScriptLPS(1);
            Vector3 pos = hostObject.GetSimulatorPosition();
            return new LSL_Vector(pos.X, pos.Y, pos.Z);
        }

        public LSL_Vector llGetLocalPos()
        {
            hostObject.AddScriptLPS(1);
            return new LSL_Vector(hostObject.Prim.Position.X, hostObject.Prim.Position.Y, hostObject.Prim.Position.Z);
        }

        public void llSetRot(LSL_Rotation rot)
        {
            hostObject.AddScriptLPS(1);

            // try to let this work as in SL...
            if (hostObject.Prim.ParentID == 0)
            {
                // special case: If we are root, rotate complete linkset to new rotation
                SetRot(hostObject, Rot2Quaternion(rot));
            }
            else
            {
                // we are a child. The rotation values will be set to the one of root modified by rot, as in SL. Don't ask.
                SimulationObject parent = hostObject.GetLinksetParent();
                SetRot(hostObject, parent.Prim.Rotation * Rot2Quaternion(rot));
            }

            ScriptSleep(200);
        }

        public void llSetLocalRot(LSL_Rotation rot)
        {
            hostObject.AddScriptLPS(1);
            SetRot(hostObject, Rot2Quaternion(rot));
            ScriptSleep(200);
        }

        /// <summary>
        /// See http://lslwiki.net/lslwiki/wakka.php?wakka=ChildRotation
        /// </summary>
        public LSL_Rotation llGetRot()
        {
            // unlinked or root prim then use llRootRotation
            // see llRootRotaion for references.
            if (hostObject.Prim.ParentID == 0)
                return llGetRootRotation();

            hostObject.AddScriptLPS(1);

            Quaternion q = hostObject.GetSimulatorRotation();
            return new LSL_Rotation(q.X, q.Y, q.Z, q.W);
        }

        public LSL_Rotation llGetLocalRot()
        {
            hostObject.AddScriptLPS(1);
            return new LSL_Rotation(hostObject.Prim.Rotation.X, hostObject.Prim.Rotation.Y, hostObject.Prim.Rotation.Z, hostObject.Prim.Rotation.W);
        }

        public void llSetForce(LSL_Vector force, int local)
        {
            hostObject.AddScriptLPS(1);

            // TODO: Apply constraints?
            if (local != 0)
                force *= llGetRot();

            Vector3 velocity = new Vector3((float)force.x, (float)force.y, (float)force.z);

            // Child prims do not have velocity, only parents
            SimulationObject parent = hostObject.GetLinksetParent();
            server.Scene.ObjectTransform(this, parent, parent.Prim.Position, parent.Prim.Rotation, velocity, parent.Prim.Acceleration,
                parent.Prim.AngularVelocity);
        }

        public LSL_Vector llGetForce()
        {
            hostObject.AddScriptLPS(1);

            // Child prims do not have velocity, only parents
            SimulationObject parent = hostObject.GetLinksetParent();
            return new LSL_Vector(parent.Prim.Velocity.X, parent.Prim.Velocity.Y, parent.Prim.Velocity.Z);
        }

        public LSL_Integer llTarget(LSL_Vector position, double range)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llTarget");
            return LSL_Integer.Zero;
        }

        public void llTargetRemove(int number)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llTargetRemove");
        }

        public LSL_Integer llRotTarget(LSL_Rotation rot, double error)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRotTarget");
            return LSL_Integer.Zero;
        }

        public void llRotTargetRemove(int number)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRotTargetRemove");
        }

        public void llMoveToTarget(LSL_Vector target, double tau)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llMoveToTarget");
        }

        public void llStopMoveToTarget()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llStopMoveToTarget");
        }

        public void llApplyImpulse(LSL_Vector force, int local)
        {
            hostObject.AddScriptLPS(1);
            
            //TODO: No energy force yet
            if (force.x > 20000.0) force.x = 20000.0;
            if (force.y > 20000.0) force.y = 20000.0;
            if (force.z > 20000.0) force.z = 20000.0;

            if (local != 0)
                force *= llGetRot();

            // Child prims do not move, only parents
            SimulationObject parent = hostObject.GetLinksetParent();

            server.Scene.ObjectApplyImpulse(this, parent, new Vector3((float)force.x, (float)force.y, (float)force.z));
        }

        public void llApplyRotationalImpulse(LSL_Vector force, int local)
        {
            hostObject.AddScriptLPS(1);

            // TODO: Constraints?
            if (local != 0)
                force *= llGetRot();

            // Apply rotation impulse to the parent
            SimulationObject parent = hostObject.GetLinksetParent();

            // Can't apply rotational impulse to avatars
            if (!(parent.Prim is Avatar))
                server.Scene.ObjectApplyRotationalImpulse(this, parent, new Vector3((float)force.x, (float)force.y, (float)force.z));
        }

        public void llSetTorque(LSL_Vector torque, int local)
        {
            hostObject.AddScriptLPS(1);

            if (local != 0)
                torque *= llGetRot();

            // Set torque on the parent
            SimulationObject parent = hostObject.GetLinksetParent();

            // Can't set torque on avatars
            if (!(parent.Prim is Avatar))
                server.Scene.ObjectSetTorque(this, parent, new Vector3((float)torque.x, (float)torque.y, (float)torque.z));
        }

        public LSL_Vector llGetTorque()
        {
            hostObject.AddScriptLPS(1);

            SimulationObject parent = hostObject.GetLinksetParent();
            return new LSL_Vector((float)parent.Torque.X, (float)parent.Torque.Y, (float)parent.Torque.Z);
        }

        public void llSetForceAndTorque(LSL_Vector force, LSL_Vector torque, int local)
        {
            llSetForce(force, local);
            llSetTorque(torque, local);
        }

        public LSL_Vector llGetVel()
        {
            hostObject.AddScriptLPS(1);

            SimulationObject parent = hostObject.GetLinksetParent();
            return new LSL_Vector(parent.Prim.Velocity.X, parent.Prim.Velocity.Y, parent.Prim.Velocity.Z);
        }

        public LSL_Vector llGetAccel()
        {
            hostObject.AddScriptLPS(1);

            SimulationObject parent = hostObject.GetLinksetParent();
            return new LSL_Vector(parent.Prim.Acceleration.X, parent.Prim.Acceleration.Y, parent.Prim.Acceleration.Z);
        }

        public LSL_Vector llGetOmega()
        {
            hostObject.AddScriptLPS(1);

            SimulationObject parent = hostObject.GetLinksetParent();
            return new LSL_Vector(parent.Prim.AngularVelocity.X, parent.Prim.AngularVelocity.Y, parent.Prim.AngularVelocity.Z);
        }

        public LSL_Float llGetTimeOfDay()
        {
            hostObject.AddScriptLPS(1);
            return (double)((DateTime.Now.TimeOfDay.TotalMilliseconds / 1000) % (3600 * 4));
        }

        public LSL_Float llGetWallclock()
        {
            hostObject.AddScriptLPS(1);
            return DateTime.Now.TimeOfDay.TotalSeconds;
        }

        public LSL_Float llGetTime()
        {
            hostObject.AddScriptLPS(1);
            TimeSpan ScriptTime = DateTime.Now - startTime;
            return (double)(ScriptTime.TotalMilliseconds / 1000);
        }

        public void llResetTime()
        {
            hostObject.AddScriptLPS(1);
            startTime = DateTime.Now;
        }

        public LSL_Float llGetAndResetTime()
        {
            hostObject.AddScriptLPS(1);
            TimeSpan ScriptTime = DateTime.Now - startTime;
            startTime = DateTime.Now;
            return (double)(ScriptTime.TotalMilliseconds / 1000);
        }

        public void llSound(string sound, double volume, int queue, int loop)
        {
            hostObject.AddScriptLPS(1);
            // This function has been deprecated
            // see http://www.lslwiki.net/lslwiki/wakka.php?wakka=llSound
            Deprecated("llSound");
        }

        // Xantor 20080528 PlaySound updated so it accepts an objectinventory name -or- a key to a sound
        // 20080530 Updated to remove code duplication
        public void llPlaySound(string sound, double volume)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llPlaySound");
        }

        // Xantor 20080528 we should do this differently.
        // 1) apply the sound to the object
        // 2) schedule full update
        // just sending the sound out once doesn't work so well when other avatars come in view later on
        // or when the prim gets moved, changed, sat on, whatever
        // see large number of mantises (mantes?)
        // 20080530 Updated to remove code duplication
        // 20080530 Stop sound if there is one, otherwise volume only changes don't work
        public void llLoopSound(string sound, double volume)
        {
            hostObject.AddScriptLPS(1);

            if (hostObject.Prim.Sound != UUID.Zero)
                llStopSound();

            hostObject.Prim.Sound = KeyOrName(sound);
            hostObject.Prim.SoundGain = (float)volume;
            hostObject.Prim.SoundFlags = 1; // TODO: ???
            hostObject.Prim.SoundRadius = 20; // TODO: Randomly selected

            server.Scene.ObjectAdd(this, hostObject, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        public void llLoopSoundMaster(string sound, double volume)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llLoopSoundMaster");
        }

        public void llLoopSoundSlave(string sound, double volume)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llLoopSoundSlave");
        }

        public void llPlaySoundSlave(string sound, double volume)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llPlaySoundSlave");
        }

        public void llTriggerSound(string sound, double volume)
        {
            hostObject.AddScriptLPS(1);

            server.Scene.TriggerSound(this, hostObject.Prim.ID, hostObject.GetLinksetParent().Prim.ID,
                hostObject.Prim.Properties.OwnerID, KeyOrName(sound), hostObject.GetSimulatorPosition(),
                (float)volume);
        }

        // Xantor 20080528: Clear prim data of sound instead
        public void llStopSound()
        {
            hostObject.AddScriptLPS(1);

            hostObject.Prim.Sound = UUID.Zero;
            hostObject.Prim.SoundGain = 0;
            hostObject.Prim.SoundFlags = 0;
            hostObject.Prim.SoundRadius = 0;

            server.Scene.ObjectAdd(this, hostObject, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        public void llPreloadSound(string sound)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llPreloadSound");
            // ScriptSleep(1000);
        }

        /// <summary>
        /// Return a portion of the designated string bounded by
        /// inclusive indices (start and end). As usual, the negative
        /// indices, and the tolerance for out-of-bound values, makes
        /// this more complicated than it might otherwise seem.
        /// </summary>

        public LSL_String llGetSubString(string src, int start, int end)
        {
            hostObject.AddScriptLPS(1);

            // Normalize indices (if negative).
            // After normlaization they may still be
            // negative, but that is now relative to
            // the start, rather than the end, of the
            // sequence.
            if (start < 0)
            {
                start = src.Length + start;
            }
            if (end < 0)
            {
                end = src.Length + end;
            }

            // Conventional substring
            if (start <= end)
            {
                // Implies both bounds are out-of-range.
                if (end < 0 || start >= src.Length)
                {
                    return String.Empty;
                }
                // If end is positive, then it directly
                // corresponds to the lengt of the substring
                // needed (plus one of course). BUT, it
                // must be within bounds.
                if (end >= src.Length)
                {
                    end = src.Length - 1;
                }

                if (start < 0)
                {
                    return src.Substring(0, end + 1);
                }
                // Both indices are positive
                return src.Substring(start, (end + 1) - start);
            }

            // Inverted substring (end < start)
            else
            {
                // Implies both indices are below the
                // lower bound. In the inverted case, that
                // means the entire string will be returned
                // unchanged.
                if (start < 0)
                {
                    return src;
                }
                // If both indices are greater than the upper
                // bound the result may seem initially counter
                // intuitive.
                if (end >= src.Length)
                {
                    return src;
                }

                if (end < 0)
                {
                    if (start < src.Length)
                    {
                        return src.Substring(start);
                    }
                    else
                    {
                        return String.Empty;
                    }
                }
                else
                {
                    if (start < src.Length)
                    {
                        return src.Substring(0, end + 1) + src.Substring(start);
                    }
                    else
                    {
                        return src.Substring(0, end + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Delete substring removes the specified substring bounded
        /// by the inclusive indices start and end. Indices may be
        /// negative (indicating end-relative) and may be inverted,
        /// i.e. end < start.
        /// </summary>
        public LSL_String llDeleteSubString(string src, int start, int end)
        {
            hostObject.AddScriptLPS(1);

            // Normalize indices (if negative).
            // After normlaization they may still be
            // negative, but that is now relative to
            // the start, rather than the end, of the
            // sequence.
            if (start < 0)
            {
                start = src.Length + start;
            }
            if (end < 0)
            {
                end = src.Length + end;
            }
            // Conventionally delimited substring
            if (start <= end)
            {
                // If both bounds are outside of the existing
                // string, then return unchanges.
                if (end < 0 || start >= src.Length)
                {
                    return src;
                }
                // At least one bound is in-range, so we
                // need to clip the out-of-bound argument.
                if (start < 0)
                {
                    start = 0;
                }

                if (end >= src.Length)
                {
                    end = src.Length - 1;
                }

                return src.Remove(start, end - start + 1);
            }
            // Inverted substring
            else
            {
                // In this case, out of bounds means that
                // the existing string is part of the cut.
                if (start < 0 || end >= src.Length)
                {
                    return String.Empty;
                }

                if (end > 0)
                {
                    if (start < src.Length)
                    {
                        return src.Remove(start).Remove(0, end + 1);
                    }
                    else
                    {
                        return src.Remove(0, end + 1);
                    }
                }
                else
                {
                    if (start < src.Length)
                    {
                        return src.Remove(start);
                    }
                    else
                    {
                        return src;
                    }
                }
            }
        }

        /// <summary>
        /// Insert string inserts the specified string identified by src
        /// at the index indicated by index. Index may be negative, in
        /// which case it is end-relative. The index may exceed either
        /// string bound, with the result being a concatenation.
        /// </summary>
        public LSL_String llInsertString(string dest, int index, string src)
        {
            hostObject.AddScriptLPS(1);

            // Normalize indices (if negative).
            // After normlaization they may still be
            // negative, but that is now relative to
            // the start, rather than the end, of the
            // sequence.
            if (index < 0)
            {
                index = dest.Length + index;

                // Negative now means it is less than the lower
                // bound of the string.

                if (index < 0)
                {
                    return src + dest;
                }

            }

            if (index >= dest.Length)
            {
                return dest + src;
            }

            // The index is in bounds.
            // In this case the index refers to the index that will
            // be assigned to the first character of the inserted string.
            // So unlike the other string operations, we do not add one
            // to get the correct string length.
            return dest.Substring(0, index) + src + dest.Substring(index);
        }

        public LSL_String llToUpper(string src)
        {
            hostObject.AddScriptLPS(1);
            return src.ToUpper();
        }

        public LSL_String llToLower(string src)
        {
            hostObject.AddScriptLPS(1);
            return src.ToLower();
        }

        public LSL_Integer llGiveMoney(string destination, int amount)
        {
            hostObject.AddScriptLPS(1);

            InventoryTaskItem item = InventorySelf();
            if (item != null)
            {
                NotImplemented("llGiveMoney");
                return LSL_Integer.Zero;
            }
            else
            {
                return LSL_Integer.Zero;
            }
        }

        public void llMakeExplosion(int particles, double scale, double vel, double lifetime, double arc, string texture, LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            Deprecated("llMakeExplosion");
        }

        public void llMakeFountain(int particles, double scale, double vel, double lifetime, double arc, int bounce, string texture, LSL_Vector offset, double bounce_offset)
        {
            hostObject.AddScriptLPS(1);
            Deprecated("llMakeFountain");
        }

        public void llMakeSmoke(int particles, double scale, double vel, double lifetime, double arc, string texture, LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            Deprecated("llMakeSmoke");
        }

        public void llMakeFire(int particles, double scale, double vel, double lifetime, double arc, string texture, LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            Deprecated("llMakeFire");
        }

        public void llRezAtRoot(string inventory, LSL_Vector pos, LSL_Vector vel, LSL_Rotation rot, int param)
        {
            hostObject.AddScriptLPS(1);

            if (Double.IsNaN(rot.x) || Double.IsNaN(rot.y) || Double.IsNaN(rot.z) || Double.IsNaN(rot.s))
                return;
            float dist = (float)llVecDist(llGetPos(), pos);

            if (dist > SCRIPT_DISTANCE_FACTOR * 10.0f)
                return;

            // TODO: Constraints?
            Vector3 llpos = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);
            Vector3 llvel = new Vector3((float)vel.x, (float)vel.y, (float)vel.z);
            Quaternion llrot = new Quaternion((float)rot.x, (float)rot.y, (float)rot.z, (float)rot.s);

            if (Vector3.Distance(llpos, hostObject.GetSimulatorPosition()) > 10.0f)
                return; // wiki says, if it's further than 10m away, silently fail.

            // need the magnitude later
            float velmag = llvel.Length();

            Asset asset;
            List<SimulationObject> newLinkset = null;
            SimulationObject newParent = null;
            InventoryTaskItem item = InventoryKey(inventory, AssetType.Object);

            if (inventory == "default")
            {
                // Special handler to allow llRez*() functions to work from the scripting console
                Primitive cube = new Primitive();
                cube.PrimData = OpenMetaverse.ObjectManager.BuildBasicShape(PrimType.Box);
                SimulationObject obj = new SimulationObject(cube, server);
                newLinkset = new List<SimulationObject>(1);
                newLinkset.Add(obj);
            }

            if ((item != null &&
                server.TaskInventory.TryGetAsset(hostObject.Prim.ID, item.AssetID, out asset) &&
                asset is AssetPrim &&
                server.Assets.TryDecodePrimAsset(((AssetPrim)asset).AssetData, out newLinkset)) ||
                newLinkset != null)
            {
                for (int i = 0; i < newLinkset.Count; i++)
                {
                    SimulationObject newObj = newLinkset[i];

                    // objects rezzed with this method are die_at_edge by default.
                    newObj.Prim.Flags |= PrimFlags.DieAtEdge;

                    if (newObj.Prim.ParentID == 0)
                    {
                        newObj.Prim.Position = llpos;
                        newObj.Prim.Velocity = llvel;
                        newObj.Prim.Rotation = llrot;
                    }

                    if (server.Scene.ObjectAdd(this, newObj, hostObject.Prim.Properties.OwnerID, param, PrimFlags.None) &&
                        newObj.Prim.ParentID == 0)
                    {
                        newParent = newObj;
                    }
                }

                if (newParent != null)
                {
                    server.ScriptEngine.PostObjectEvent(hostObject.Prim.ID, new EventParams("object_rez",
                        new Object[] { new LSL_String(newParent.ToString()) },
                        new DetectParams[0])
                    );

                    float groupMass = newParent.GetLinksetMass();

                    if ((newParent.Prim.Flags & PrimFlags.Physics) == PrimFlags.Physics)
                    {
                        // Apply recoil to the current object
                        llvel *= groupMass;
                        llApplyImpulse(new LSL_Vector(llvel.X, llvel.Y, llvel.Z), 0);
                    }

                    // Variable script delay? (see (http://wiki.secondlife.com/wiki/LSL_Delay)
                    ScriptSleep((int)((groupMass * velmag) / 10f));
                    return;
                }
            }

            llSay(0, "Could not find object " + inventory);
        }

        public void llRezObject(string inventory, LSL_Vector pos, LSL_Vector vel, LSL_Rotation rot, int param)
        {
            llRezAtRoot(inventory, pos, vel, rot, param);
        }

        //TODO: partial implementation, rotates objects correctly but does not apply strength or damping attributes
        public void llLookAt(LSL_Vector target, double strength, double damping)
        {
            hostObject.AddScriptLPS(1);

            // Determine where we are looking from 
            LSL_Vector from = llGetPos();

            // Work out the normalised vector from the source to the target 
            LSL_Vector delta = llVecNorm(target - from);
            LSL_Vector angle = LSL_Vector.Zero;

            // Calculate the yaw 
            // subtracting PI_OVER_TWO is required to compensate for the odd SL coordinate system 
            angle.x = llAtan2(delta.z, delta.y) - Utils.PI_OVER_TWO;

            // Calculate pitch 
            angle.y = llAtan2(delta.x, llSqrt((delta.y * delta.y) + (delta.z * delta.z)));

            // we need to convert from a vector describing
            // the angles of rotation in radians into rotation value
            LSL_Rotation rot = llEuler2Rot(angle);

            // Orient the object to the angle calculated 
            llSetRot(rot);
        }

        public void llStopLookAt()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llStopLookAt");
        }

        public void llSetTimerEvent(double sec)
        {
            hostObject.AddScriptLPS(1);

            if (sec != 0.0 && sec < MIN_TIMER_INTERVAL)
                sec = MIN_TIMER_INTERVAL;

            // Setting timer repeat
            server.ScriptEngine.SetTimerEvent(scriptID, sec);
        }

        public void llSleep(double sec)
        {
            hostObject.AddScriptLPS(1);
            System.Threading.Thread.Sleep((int)(sec * 1000.0));
        }

        public LSL_Float llGetMass()
        {
            hostObject.AddScriptLPS(1);

            SimulationObject parent = hostObject.GetLinksetParent();

            if (hostObject == parent)
                return hostObject.GetLinksetMass();
            else
                return hostObject.GetMass();
        }

        public void llCollisionFilter(string name, string id, int accept)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llCollisionFilter");
        }

        public void llTakeControls(int controls, int accept, int pass_on)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llTakeControls");
        }

        public void llReleaseControls()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llReleaseControls");
        }

        public void llAttachToAvatar(int attachment)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llAttachToAvatar");
        }

        public void llDetachFromAvatar()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llDetachFromAvatar");
        }

        public void llTakeCamera(string avatar)
        {
            hostObject.AddScriptLPS(1);
            Deprecated("llTakeCamera");
        }

        public void llReleaseCamera(string avatar)
        {
            hostObject.AddScriptLPS(1);
            Deprecated("llReleaseCamera");
        }

        public LSL_Key llGetOwner()
        {
            hostObject.AddScriptLPS(1);
            return hostObject.Prim.Properties.OwnerID.ToString();
        }

        public void llInstantMessage(string user, string message)
        {
            hostObject.AddScriptLPS(1);

            UUID toID;
            UUID.TryParse(user, out toID);

            server.Messages.SendInstantMessage(this, hostObject.Prim.ID, hostObject.Prim.Properties.Name, toID,
                InstantMessageDialog.MessageFromObject, false, hostObject.Prim.ID, false, hostObject.GetSimulatorPosition(),
                0, UUID.Zero, DateTime.Now, message, Utils.EmptyBytes);
            ScriptSleep(2000);
        }

        public void llEmail(string address, string subject, string message)
        {
            hostObject.AddScriptLPS(1);

            server.Messages.SendEmail(this, hostObject.Prim.ID, address, subject, message);
            ScriptSleep(20000);
        }

        public void llGetNextEmail(string address, string subject)
        {
            hostObject.AddScriptLPS(1);

            Email email;
            if (server.Messages.GetNextEmail(hostObject.Prim.ID, address, subject, out email))
            {
                server.ScriptEngine.PostObjectEvent(hostObject.Prim.ID, new EventParams("email", new Object[] {
                    new LSL_String(email.Time.ToString()),
                    new LSL_String(email.Sender),
                    new LSL_String(email.Subject),
                    new LSL_String(email.Message),
                    new LSL_Integer(email.NumLeft) },
                    new DetectParams[0])
                );
            }
        }

        public LSL_Key llGetKey()
        {
            hostObject.AddScriptLPS(1);
            return hostObject.Prim.ID.ToString();
        }

        public void llSetBuoyancy(double buoyancy)
        {
            hostObject.AddScriptLPS(1);
            //SimulationObject parent = hostObject.GetLinksetParent();
            NotImplemented("llSetBuoyancy");
            //server.Scene.ObjectSetBuoyancy(this, parent, (float)buoyancy);
        }

        public void llSetHoverHeight(double height, int water, double tau)
        {
            hostObject.AddScriptLPS(1);

            Vector3 pos = hostObject.GetSimulatorPosition();
            float targetHeight = 0f;

            if (water == 1)
            {
                float waterHeight = server.Scene.WaterHeight;
                if (waterHeight > targetHeight)
                    targetHeight = waterHeight + (float)height;
            }
            else
            {
                float landHeight = server.Scene.GetTerrainHeightAt(pos.X, pos.Y);
                targetHeight = landHeight + (float)height;
            }

            SimulationObject parent = hostObject.GetLinksetParent();
            parent.Prim.Flags |= PrimFlags.Flying;

            Vector3 newPosition = parent.Prim.Position;
            if (targetHeight > 0f)
                newPosition.Z = targetHeight;

            server.Scene.ObjectTransform(this, parent, newPosition, parent.Prim.Rotation, parent.Prim.Velocity,
                parent.Prim.Acceleration, parent.Prim.AngularVelocity);
        }

        public void llStopHover()
        {
            hostObject.AddScriptLPS(1);

            SimulationObject parent = hostObject.GetLinksetParent();
            parent.Prim.Flags &= ~PrimFlags.Flying;

            server.Scene.ObjectTransform(this, parent, parent.Prim.Position, parent.Prim.Rotation, parent.Prim.Velocity,
                parent.Prim.Acceleration, parent.Prim.AngularVelocity);
        }

        public void llMinEventDelay(double delay)
        {
            hostObject.AddScriptLPS(1);
            server.ScriptEngine.SetScriptMinEventDelay(hostObject.Prim.ID, delay);
        }

        /// <summary>
        /// llSoundPreload is deprecated. In SL this appears to do absolutely nothing
        /// and is documented to have no delay.
        /// </summary>
        public void llSoundPreload(string sound)
        {
            hostObject.AddScriptLPS(1);
        }

        public void llRotLookAt(LSL_Rotation target, double strength, double damping)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRotLookAt");
        }

        public LSL_Integer llStringLength(string str)
        {
            hostObject.AddScriptLPS(1);
            if (str.Length > 0)
                return str.Length;
            else
                return 0;
        }

        public void llStartAnimation(string anim)
        {
            hostObject.AddScriptLPS(1);

            Agent agent;
            InventoryTaskItem item = InventorySelf();

            if (item != null)
            {
                if (item.PermissionGranter != UUID.Zero)
                {
                    if ((item.GrantedPermissions & ScriptTypes.PERMISSION_TRIGGER_ANIMATION) != 0)
                    {
                        if (server.Scene.TryGetAgent(item.PermissionGranter, out agent))
                        {
                            UUID animID = InventoryKey(anim, AssetType.Animation).AssetID;

                            if (animID != UUID.Zero)
                            {
                                server.Avatars.AddAnimation(agent, animID);
                                server.Avatars.SendAnimations(agent);
                            }
                        }
                    }
                }
            }
        }

        public void llStopAnimation(string anim)
        {
            hostObject.AddScriptLPS(1);

            Agent agent;
            InventoryTaskItem item = InventorySelf();

            if (item != null)
            {
                if (item.PermissionGranter != UUID.Zero)
                {
                    if ((item.GrantedPermissions & ScriptTypes.PERMISSION_TRIGGER_ANIMATION) != 0)
                    {
                        if (server.Scene.TryGetAgent(item.PermissionGranter, out agent))
                        {
                            UUID animID = InventoryKey(anim, AssetType.Animation).AssetID;

                            if (animID != UUID.Zero)
                            {
                                server.Avatars.RemoveAnimation(agent, animID);
                                server.Avatars.SendAnimations(agent);
                            }
                        }
                    }
                }
            }
        }

        public void llPointAt(LSL_Vector pos)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llPointAt");
        }

        public void llStopPointAt()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llStopPointAt");
        }

        public void llTargetOmega(LSL_Vector axis, double spinrate, double gain)
        {
            hostObject.AddScriptLPS(1);

            Vector3 angVel = new Vector3((float)axis.x, (float)axis.y, (float)axis.z);
            angVel *= (float)spinrate;

            server.Scene.ObjectTransform(this, hostObject, hostObject.Prim.Position, hostObject.Prim.Rotation,
                hostObject.Prim.Velocity, hostObject.Prim.Acceleration, angVel);
        }

        public LSL_Integer llGetStartParameter()
        {
            hostObject.AddScriptLPS(1);
            return server.ScriptEngine.GetStartParameter(scriptID);
        }

        public void llGodLikeRezObject(string inventory, LSL_Vector pos)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGodLikeRezObject");
        }

        public void llRequestPermissions(string agent, int perm)
        {
            hostObject.AddScriptLPS(1);

            UUID agentID;
            if (UUID.TryParse(agent, out agentID))
            {
                InventoryTaskItem scriptItem = InventorySelf();
                if (scriptItem != null)
                {
                    if (agentID == UUID.Zero || perm == 0)
                    {
                        // Releasing permissions
                        llReleaseControls();

                        scriptItem.PermissionGranter = UUID.Zero;
                        scriptItem.GrantedPermissions = 0;

                        server.ScriptEngine.PostScriptEvent(scriptID, new EventParams(
                            "run_time_permissions",
                            new Object[] { LSL_Integer.Zero },
                            new DetectParams[0]));
                    }
                    else
                    {
                        if (scriptItem.PermissionGranter != agentID || (perm & ScriptTypes.PERMISSION_TAKE_CONTROLS) == 0)
                            llReleaseControls();

                        SimulationObject parent = hostObject.GetLinksetParent();
                        if (parent.Prim.ParentID != 0)
                        {
                            // FIXME: Finish this function
                        }
                    }
                }
            }

            /*if (m_host.ParentGroup.IsAttachment && (UUID)agent == m_host.ParentGroup.RootPart.AttachedAvatar)
            {
                // When attached, certain permissions are implicit if requested from owner
                int implicitPerms = ScriptTypes.PERMISSION_TAKE_CONTROLS |
                        ScriptTypes.PERMISSION_TRIGGER_ANIMATION |
                        ScriptTypes.PERMISSION_CONTROL_CAMERA |
                        ScriptTypes.PERMISSION_ATTACH;

                if ((perm & (~implicitPerms)) == 0) // Requested only implicit perms
                {
                    lock (m_host.TaskInventory)
                    {
                        m_host.TaskInventory[invItemID].PermsGranter = agentID;
                        m_host.TaskInventory[invItemID].PermsMask = perm;
                    }

                    m_ScriptEngine.PostScriptEvent(m_itemID, new EventParams(
                            "run_time_permissions", new Object[] {
                            new LSL_Integer(perm) },
                            new DetectParams[0]));

                    return;
                }
            }
            else if (m_host.SitTargetAvatar == agentID) // Sitting avatar
            {
                // When agent is sitting, certain permissions are implicit if requested from sitting agent
                int implicitPerms = ScriptTypes.PERMISSION_TRIGGER_ANIMATION |
                    ScriptTypes.PERMISSION_CONTROL_CAMERA |
                    ScriptTypes.PERMISSION_TRACK_CAMERA |
                    ScriptTypes.PERMISSION_TAKE_CONTROLS;

                if ((perm & (~implicitPerms)) == 0) // Requested only implicit perms
                {
                    lock (m_host.TaskInventory)
                    {
                        m_host.TaskInventory[invItemID].PermsGranter = agentID;
                        m_host.TaskInventory[invItemID].PermsMask = perm;
                    }

                    m_ScriptEngine.PostScriptEvent(m_itemID, new EventParams(
                            "run_time_permissions", new Object[] {
                            new LSL_Integer(perm) },
                            new DetectParams[0]));

                    return;
                }
            }

            ScenePresence presence = World.GetScenePresence(agentID);

            if (presence != null)
            {
                string ownerName = resolveName(m_host.ParentGroup.RootPart.OwnerID);
                if (ownerName == String.Empty)
                    ownerName = "(hippos)";

                if (!m_waitingForScriptAnswer)
                {
                    lock (m_host.TaskInventory)
                    {
                        m_host.TaskInventory[invItemID].PermsGranter = agentID;
                        m_host.TaskInventory[invItemID].PermsMask = 0;
                    }

                    presence.ControllingClient.OnScriptAnswer += handleScriptAnswer;
                    m_waitingForScriptAnswer = true;
                }

                presence.ControllingClient.SendScriptQuestion(
                    m_host.UUID, m_host.ParentGroup.RootPart.Name, ownerName, invItemID, perm);

                return;
            }

            // Requested agent is not in range, refuse perms
            m_ScriptEngine.PostScriptEvent(m_itemID, new EventParams(
                    "run_time_permissions", new Object[] {
                    new LSL_Integer(0) },
                    new DetectParams[0]));*/
        }

        public LSL_Key llGetPermissionsKey()
        {
            hostObject.AddScriptLPS(1);

            InventoryTaskItem scriptItem = server.TaskInventory.FindItem(hostObject.Prim.ID,
                delegate(InventoryTaskItem item) { return item.ID == scriptID; });

            if (scriptItem != null)
                return scriptItem.PermissionGranter.ToString();
            else
                return LSL_Key.Zero;
        }

        public LSL_Integer llGetPermissions()
        {
            hostObject.AddScriptLPS(1);

            InventoryTaskItem scriptItem = server.TaskInventory.FindItem(hostObject.Prim.ID,
                delegate(InventoryTaskItem item) { return item.ID == scriptID; });

            uint perms = 0;

            if (scriptItem != null)
            {
                perms = scriptItem.GrantedPermissions;
                if (automaticLinkPermission)
                    perms |= ScriptTypes.PERMISSION_CHANGE_LINKS;
            }

            return perms;
        }

        public LSL_Integer llGetLinkNumber()
        {
            hostObject.AddScriptLPS(1);
            return hostObject.LinkNumber;
        }

        public void llSetLinkColor(int linknumber, LSL_Vector color, int face)
        {
            List<SimulationObject> parts = GetLinkParts(linknumber);

            foreach (SimulationObject part in parts)
                SetColor(part, color, face);
        }

        public void llCreateLink(string target, int parent)
        {
        }

        public void llBreakLink(int linknum)
        {
        }

        public void llBreakAllLinks()
        {
        }

        public LSL_Key llGetLinkKey(int linknum)
        {
            hostObject.AddScriptLPS(1);
            SimulationObject parent = hostObject.GetLinksetParent();
            return parent.Prim.ID.ToString();
        }

        /// <summary>
        /// The rules governing the returned name are not simple. The only
        /// time a blank name is returned is if the target prim has a blank
        /// name. If no prim with the given link number can be found then
        /// usually NULL_KEY is returned but there are exceptions.
        /// 
        /// In a single unlinked prim, A call with 0 returns the name, all 
        /// other values for link number return NULL_KEY
        ///
        /// In link sets it is more complicated.
        /// 
        /// If the script is in the root prim:-
        ///     A zero link number returns NULL_KEY.
        ///     Positive link numbers return the name of the prim, or NULL_KEY 
        ///     if a prim does not exist at that position.
        ///     Negative link numbers return the name of the first child prim.
        /// 
        /// If the script is in a child prim:-
        ///     Link numbers 0 or 1 return the name of the root prim.
        ///     Positive link numbers return the name of the prim or NULL_KEY
        ///     if a prim does not exist at that position.
        ///     Negative numbers return the name of the root prim.
        /// 
        /// References
        /// http://lslwiki.net/lslwiki/wakka.php?wakka=llGetLinkName
        /// Mentions NULL_KEY being returned
        /// http://wiki.secondlife.com/wiki/LlGetLinkName
        /// Mentions using the LINK_* constants, some of which are negative 
        /// </summary>
        public LSL_String llGetLinkName(int linknum)
        {
            hostObject.AddScriptLPS(1);

            // simplest case, this prims link number
            if (hostObject.LinkNumber == linknum)
                return hostObject.Prim.Properties.Name;

            // Single prim
            if (hostObject.LinkNumber == 0)
            {
                if (linknum == 0)
                    return hostObject.Prim.Properties.Name;
                else
                    return LSL_Key.Zero;
            }

            // Linkset
            SimulationObject parent = hostObject.GetLinksetParent();
            SimulationObject target = null;

            if (hostObject == parent) // this is the parent prim
            {
                if (linknum < 0)
                    target = parent.GetLinksetPrim(2);
                else
                    target = parent.GetLinksetPrim(linknum);
            }
            else // this is a child prim
            {
                if (linknum < 2)
                    target = parent.GetLinksetPrim(1);
                else
                    target = parent.GetLinksetPrim(linknum);
            }

            if (target != null)
                return target.Prim.Properties.Name;
            else
                return LSL_Key.Zero;
        }

        public LSL_Integer llGetInventoryNumber(int type)
        {
            hostObject.AddScriptLPS(1);
            int count = 0;

            server.TaskInventory.ForEachItem(hostObject.Prim.ID,
                delegate(InventoryTaskItem item)
                {
                    if (type == -1 || (int)item.AssetType == type)
                        ++count;
                }
            );

            return count;
        }

        public LSL_String llGetInventoryName(int type, int number)
        {
            NotImplemented("llGetInventoryName");
            return LSL_String.Empty;
        }

        public LSL_Float llGetEnergy()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetEnergy");
            return new LSL_Float(1f);
        }

        public void llGiveInventory(string destination, string inventory)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGiveInventory");
        }

        public void llRemoveInventory(string name)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRemoveInventory");
        }

        public void llSetText(string text, LSL_Vector color, double alpha)
        {
            hostObject.AddScriptLPS(1);

            if (text.Length > 255)
                text = text.Substring(0, 255);

            hostObject.Prim.TextColor = new Color4(
                (float)Utils.Clamp(color.x, 0f, 1f),
                (float)Utils.Clamp(color.y, 0f, 1f),
                (float)Utils.Clamp(color.z, 0f, 1f),
                (float)Utils.Clamp(alpha, 0f, 1f));
            hostObject.Prim.Text = text;

            server.Scene.ObjectAdd(this, hostObject, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        public LSL_Float llWater(LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            return server.Scene.WaterHeight;
        }

        public void llPassTouches(int pass)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llPassTouches");
        }

        public LSL_Key llRequestAgentData(string id, int data)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRequestAgentData");
            return LSL_Key.Zero;
        }

        public LSL_Key llRequestInventoryData(string name)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRequestInventoryData");
            return LSL_Key.Zero;
        }

        public void llSetDamage(double damage)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetDamage");
        }

        public void llTeleportAgentHome(string agent)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llTeleportAgentHome");
        }

        public void llTextBox(string avatar, string message, int chat_channel)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llTextBox");
        }

        public void llModifyLand(int action, int brush)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llModifyLand");
        }

        public void llCollisionSound(string impact_sound, double impact_volume)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llCollisionSound");
        }

        public void llCollisionSprite(string impact_sprite)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llCollisionSprite");
        }

        public LSL_String llGetAnimation(string id)
        {
            // This should only return a value if the avatar is in the same region
            NotImplemented("llGetAnimation");
            return LSL_String.Empty;
        }

        public void llMessageLinked(int linknumber, int num, string msg, string id)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llMessageLinked");
        }

        public void llPushObject(string target, LSL_Vector impulse, LSL_Vector ang_impulse, int local)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llPushObject");
        }

        public void llPassCollisions(int pass)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llPassCollisions");
        }

        public LSL_String llGetScriptName()
        {
            string result = String.Empty;
            NotImplemented("llGetScriptName");
            return LSL_String.Empty;
        }

        public LSL_Integer llGetNumberOfSides()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetNumberOfSides");
            return LSL_Integer.Zero;
        }

        /* The new/changed functions were tested with the following LSL script:
         *
        default
        {
            state_entry()
            {
                rotation rot = llEuler2Rot(<0,70,0> * DEG_TO_RAD);

                llOwnerSay("to get here, we rotate over: "+ (string) llRot2Axis(rot));
                llOwnerSay("and we rotate for: "+ (llRot2Angle(rot) * RAD_TO_DEG));

                // convert back and forth between quaternion <-> vector and angle

                rotation newrot = llAxisAngle2Rot(llRot2Axis(rot),llRot2Angle(rot));

                llOwnerSay("Old rotation was: "+(string) rot);
                llOwnerSay("re-converted rotation is: "+(string) newrot);

                llSetRot(rot);  // to check the parameters in the prim
            }
        }
         *
         */

        // Xantor 29/apr/2008
        // Returns rotation described by rotating angle radians about axis.
        // q = cos(a/2) + i (x * sin(a/2)) + j (y * sin(a/2)) + k (z * sin(a/2))
        public LSL_Rotation llAxisAngle2Rot(LSL_Vector axis, double angle)
        {
            hostObject.AddScriptLPS(1);

            double x, y, z, s, t;

            s = Math.Cos(angle / 2);
            t = Math.Sin(angle / 2); // temp value to avoid 2 more sin() calcs
            x = axis.x * t;
            y = axis.y * t;
            z = axis.z * t;

            return new LSL_Rotation(x, y, z, s);
        }


        // Xantor 29/apr/2008
        // converts a Quaternion to X,Y,Z axis rotations
        public LSL_Vector llRot2Axis(LSL_Rotation rot)
        {
            hostObject.AddScriptLPS(1);
            double x, y, z;

            if (rot.s > 1) // normalization needed
            {
                double length = Math.Sqrt(rot.x * rot.x + rot.y * rot.y +
                        rot.z * rot.z + rot.s * rot.s);

                rot.x /= length;
                rot.y /= length;
                rot.z /= length;
                rot.s /= length;

            }

            // double angle = 2 * Math.Acos(rot.s);
            double s = Math.Sqrt(1 - rot.s * rot.s);
            if (s < 0.001)
            {
                x = 1;
                y = z = 0;
            }
            else
            {
                x = rot.x / s; // normalise axis
                y = rot.y / s;
                z = rot.z / s;
            }

            return new LSL_Vector(x, y, z);
        }


        // Returns the angle of a quaternion (see llRot2Axis for the axis)
        public LSL_Float llRot2Angle(LSL_Rotation rot)
        {
            hostObject.AddScriptLPS(1);

            if (rot.s > 1) // normalization needed
            {
                double length = Math.Sqrt(rot.x * rot.x + rot.y * rot.y +
                        rot.z * rot.z + rot.s * rot.s);

                rot.x /= length;
                rot.y /= length;
                rot.z /= length;
                rot.s /= length;
            }

            double angle = 2 * Math.Acos(rot.s);

            return angle;
        }

        public LSL_Float llAcos(double val)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Acos(val);
        }

        public LSL_Float llAsin(double val)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Asin(val);
        }

        // Xantor 30/apr/2008
        public LSL_Float llAngleBetween(LSL_Rotation a, LSL_Rotation b)
        {
            hostObject.AddScriptLPS(1);

            return (double)Math.Acos(a.x * b.x + a.y * b.y + a.z * b.z + a.s * b.s) * 2;
        }

        public LSL_Key llGetInventoryKey(string name)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetInventoryKey");
            return LSL_Key.Zero;
        }

        public void llAllowInventoryDrop(int add)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llAllowInventoryDrop");
        }

        public LSL_Vector llGetSunDirection()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetSunDirection");
            return LSL_Vector.Zero;
        }

        public LSL_Vector llGetTextureOffset(int face)
        {
            hostObject.AddScriptLPS(1);
            return GetTextureOffset(hostObject, face);
        }

        public LSL_Vector llGetTextureScale(int side)
        {
            hostObject.AddScriptLPS(1);
            Primitive.TextureEntry tex = hostObject.Prim.Textures;
            LSL_Vector scale;

            if (side == -1)
                side = 0;

            scale.x = tex.GetFace((uint)side).RepeatU;
            scale.y = tex.GetFace((uint)side).RepeatV;
            scale.z = 0.0;

            return scale;
        }

        public LSL_Float llGetTextureRot(int face)
        {
            hostObject.AddScriptLPS(1);
            return GetTextureRot(hostObject, face);
        }

        public LSL_Integer llSubStringIndex(string source, string pattern)
        {
            hostObject.AddScriptLPS(1);
            return source.IndexOf(pattern);
        }

        public LSL_Key llGetOwnerKey(string id)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetOwnerKey");
            return LSL_Key.Zero;
        }

        public LSL_Vector llGetCenterOfMass()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetCenterOfMass");
            return LSL_Vector.Zero;
        }

        public LSL_List llListSort(LSL_List src, int stride, int ascending)
        {
            hostObject.AddScriptLPS(1);

            if (stride <= 0)
                stride = 1;

            return src.Sort(stride, ascending);
        }

        public LSL_Integer llGetListLength(LSL_List src)
        {
            hostObject.AddScriptLPS(1);

            if (src == null)
                return 0;
            else
                return src.Length;
        }

        public LSL_Integer llList2Integer(LSL_List src, int index)
        {
            hostObject.AddScriptLPS(1);
            if (index < 0)
                index = src.Length + index;

            if (index >= src.Length)
                return 0;

            try
            {
                if (src.Data[index] is LSL_Integer)
                    return (LSL_Integer)src.Data[index];
                else if (src.Data[index] is LSL_Float)
                    return Convert.ToInt32(((LSL_Float)src.Data[index]).value);
                return new LSL_Integer(src.Data[index].ToString());
            }
            catch (FormatException)
            {
                return 0;
            }
        }

        public LSL_Float llList2Float(LSL_List src, int index)
        {
            hostObject.AddScriptLPS(1);
            if (index < 0)
                index = src.Length + index;

            if (index >= src.Length)
                return 0.0;

            try
            {
                if (src.Data[index] is LSL_Integer)
                    return Convert.ToDouble(((LSL_Integer)src.Data[index]).value);
                else if (src.Data[index] is LSL_Float)
                    return Convert.ToDouble(((LSL_Float)src.Data[index]).value);
                else if (src.Data[index] is LSL_String)
                    return Convert.ToDouble(((LSL_String)src.Data[index]).value);
                return Convert.ToDouble(src.Data[index]);
            }
            catch (FormatException)
            {
                return 0.0;
            }
        }

        public LSL_String llList2String(LSL_List src, int index)
        {
            hostObject.AddScriptLPS(1);
            if (index < 0)
                index = src.Length + index;

            if (index >= src.Length)
                return String.Empty;

            return src.Data[index].ToString();
        }

        public LSL_Key llList2Key(LSL_List src, int index)
        {
            hostObject.AddScriptLPS(1);
            if (index < 0)
                index = src.Length + index;

            if (index >= src.Length)
                return LSL_Key.Empty;

            return src.Data[index].ToString();
        }

        public LSL_Vector llList2Vector(LSL_List src, int index)
        {
            hostObject.AddScriptLPS(1);
            if (index < 0)
                index = src.Length + index;

            if (index >= src.Length)
                return LSL_Vector.Zero;

            if (src.Data[index].GetType() == typeof(LSL_Vector))
                return (LSL_Vector)src.Data[index];
            else
                return new LSL_Vector(src.Data[index].ToString());
        }

        public LSL_Rotation llList2Rot(LSL_List src, int index)
        {
            hostObject.AddScriptLPS(1);
            if (index < 0)
                index = src.Length + index;

            if (index >= src.Length)
                return LSL_Rotation.Identity;

            if (src.Data[index].GetType() == typeof(LSL_Rotation))
                return (LSL_Rotation)src.Data[index];
            else
                return new LSL_Rotation(src.Data[index].ToString());
        }

        public LSL_List llList2List(LSL_List src, int start, int end)
        {
            hostObject.AddScriptLPS(1);
            return src.GetSublist(start, end);
        }

        public LSL_List llDeleteSubList(LSL_List src, int start, int end)
        {
            return src.DeleteSublist(start, end);
        }

        public LSL_Integer llGetListEntryType(LSL_List src, int index)
        {
            hostObject.AddScriptLPS(1);
            if (index < 0)
                index = src.Length + index;

            if (index >= src.Length)
                return 0;

            if (src.Data[index] is LSL_Integer || src.Data[index] is Int32)
                return 1;
            if (src.Data[index] is LSL_Float || src.Data[index] is Single || src.Data[index] is Double)
                return 2;
            if (src.Data[index] is LSL_String || src.Data[index] is String)
            {
                UUID tuuid;
                if (UUID.TryParse(src.Data[index].ToString(), out tuuid))
                    return 4;
                else
                    return 3;
            }
            if (src.Data[index] is LSL_Vector)
                return 5;
            if (src.Data[index] is LSL_Rotation)
                return 6;
            if (src.Data[index] is LSL_List)
                return 7;
            return 0;

        }

        /// <summary>
        /// Process the supplied list and return the
        /// content of the list formatted as a comma
        /// separated list. There is a space after
        /// each comma.
        /// </summary>
        public LSL_String llList2CSV(LSL_List src)
        {
            string ret = String.Empty;
            int x = 0;

            hostObject.AddScriptLPS(1);

            if (src.Data.Length > 0)
            {
                ret = src.Data[x++].ToString();
                for (; x < src.Data.Length; x++)
                {
                    ret += ", " + src.Data[x].ToString();
                }
            }

            return ret;
        }

        /// <summary>
        /// The supplied string is scanned for commas
        /// and converted into a list. Commas are only
        /// effective if they are encountered outside
        /// of '<' '>' delimiters. Any whitespace
        /// before or after an element is trimmed.
        /// </summary>
        public LSL_List llCSV2List(string src)
        {
            LSL_List result = new LSL_List();
            int parens = 0;
            int start = 0;
            int length = 0;

            hostObject.AddScriptLPS(1);

            for (int i = 0; i < src.Length; i++)
            {
                switch (src[i])
                {
                    case '<':
                        parens++;
                        length++;
                        break;
                    case '>':
                        if (parens > 0)
                            parens--;
                        length++;
                        break;
                    case ',':
                        if (parens == 0)
                        {
                            result.Add(src.Substring(start, length).Trim());
                            start += length + 1;
                            length = 0;
                        }
                        else
                        {
                            length++;
                        }
                        break;
                    default:
                        length++;
                        break;
                }
            }

            result.Add(src.Substring(start, length).Trim());

            return result;
        }

        ///  <summary>
        ///  Randomizes the list, be arbitrarily reordering
        ///  sublists of stride elements. As the stride approaches
        ///  the size of the list, the options become very
        ///  limited.
        ///  </summary>
        ///  <remarks>
        ///  This could take a while for very large list
        ///  sizes.
        ///  </remarks>
        public LSL_List llListRandomize(LSL_List src, int stride)
        {
            LSL_List result;
            Random rand = new Random();

            int chunkk;
            int[] chunks;

            hostObject.AddScriptLPS(1);

            if (stride <= 0)
            {
                stride = 1;
            }

            // Stride MUST be a factor of the list length
            // If not, then return the src list. This also
            // traps those cases where stride > length.

            if (src.Length != stride && src.Length % stride == 0)
            {
                chunkk = src.Length / stride;

                chunks = new int[chunkk];

                for (int i = 0; i < chunkk; i++)
                    chunks[i] = i;

                // Knuth shuffle the chunkk index
                for (int i = chunkk - 1; i >= 1; i--)
                {
                    // Elect an unrandomized chunk to swap
                    int index = rand.Next(i + 1);
                    int tmp;

                    // and swap position with first unrandomized chunk
                    tmp = chunks[i];
                    chunks[i] = chunks[index];
                    chunks[index] = tmp;
                }

                // Construct the randomized list

                result = new LSL_List();

                for (int i = 0; i < chunkk; i++)
                {
                    for (int j = 0; j < stride; j++)
                    {
                        result.Add(src.Data[chunks[i] * stride + j]);
                    }
                }
            }
            else
            {
                object[] array = new object[src.Length];
                Array.Copy(src.Data, 0, array, 0, src.Length);
                result = new LSL_List(array);
            }

            return result;
        }

        /// <summary>
        /// Elements in the source list starting with 0 and then
        /// every i+stride. If the stride is negative then the scan
        /// is backwards producing an inverted result.
        /// Only those elements that are also in the specified
        /// range are included in the result.
        /// </summary>
        public LSL_List llList2ListStrided(LSL_List src, int start, int end, int stride)
        {
            LSL_List result = new LSL_List();
            int[] si = new int[2];
            int[] ei = new int[2];
            bool twopass = false;

            hostObject.AddScriptLPS(1);

            //  First step is always to deal with negative indices

            if (start < 0)
                start = src.Length + start;
            if (end < 0)
                end = src.Length + end;

            //  Out of bounds indices are OK, just trim them
            //  accordingly

            if (start > src.Length)
                start = src.Length;

            if (end > src.Length)
                end = src.Length;

            //  There may be one or two ranges to be considered

            if (start != end)
            {

                if (start <= end)
                {
                    si[0] = start;
                    ei[0] = end;
                }
                else
                {
                    si[1] = start;
                    ei[1] = src.Length;
                    si[0] = 0;
                    ei[0] = end;
                    twopass = true;
                }

                //  The scan always starts from the beginning of the
                //  source list, but members are only selected if they
                //  fall within the specified sub-range. The specified
                //  range values are inclusive.
                //  A negative stride reverses the direction of the
                //  scan producing an inverted list as a result.

                if (stride == 0)
                    stride = 1;

                if (stride > 0)
                {
                    for (int i = 0; i < src.Length; i += stride)
                    {
                        if (i <= ei[0] && i >= si[0])
                            result.Add(src.Data[i]);
                        if (twopass && i >= si[1] && i <= ei[1])
                            result.Add(src.Data[i]);
                    }
                }
                else if (stride < 0)
                {
                    for (int i = src.Length - 1; i >= 0; i += stride)
                    {
                        if (i <= ei[0] && i >= si[0])
                            result.Add(src.Data[i]);
                        if (twopass && i >= si[1] && i <= ei[1])
                            result.Add(src.Data[i]);
                    }
                }
            }

            return result;
        }

        public LSL_Integer llGetRegionAgentCount()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetRegionAgentCount");
            return LSL_Integer.Zero;
        }

        public LSL_Vector llGetRegionCorner()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("");
            return LSL_Vector.Zero;
        }

        /// <summary>
        /// Insert the list identified by <src> into the
        /// list designated by <dest> such that the first
        /// new element has the index specified by <index>
        /// </summary>
        public LSL_List llListInsertList(LSL_List dest, LSL_List src, int index)
        {
            LSL_List pref = null;
            LSL_List suff = null;

            hostObject.AddScriptLPS(1);

            if (index < 0)
            {
                index = index + dest.Length;
                if (index < 0)
                {
                    index = 0;
                }
            }

            if (index != 0)
            {
                pref = dest.GetSublist(0, index - 1);
                if (index < dest.Length)
                {
                    suff = dest.GetSublist(index, -1);
                    return pref + src + suff;
                }
                else
                {
                    return pref + src;
                }
            }
            else
            {
                if (index < dest.Length)
                {
                    suff = dest.GetSublist(index, -1);
                    return src + suff;
                }
                else
                {
                    return src;
                }
            }
        }

        /// <summary>
        /// Returns the index of the first occurrence of test
        /// in src.
        /// </summary>
        public LSL_Integer llListFindList(LSL_List src, LSL_List test)
        {
            int index = -1;
            int length = src.Length - test.Length + 1;

            hostObject.AddScriptLPS(1);

            // If either list is empty, do not match
            if (src.Length != 0 && test.Length != 0)
            {
                for (int i = 0; i < length; i++)
                {
                    if (src.Data[i].Equals(test.Data[0]))
                    {
                        int j;
                        for (j = 1; j < test.Length; j++)
                            if (!src.Data[i + j].Equals(test.Data[j]))
                                break;
                        if (j == test.Length)
                        {
                            index = i;
                            break;
                        }
                    }
                }
            }

            return index;
        }

        public LSL_String llGetObjectName()
        {
            hostObject.AddScriptLPS(1);
            return hostObject.Prim.Properties.Name;
        }

        public void llSetObjectName(string name)
        {
            hostObject.AddScriptLPS(1);
            // TODO: Constraints
            hostObject.Prim.Properties.Name = name;
        }

        public LSL_String llGetDate()
        {
            hostObject.AddScriptLPS(1);
            DateTime date = DateTime.Now.ToUniversalTime();
            string result = date.ToString("yyyy-MM-dd");
            return result;
        }

        public LSL_Integer llEdgeOfWorld(LSL_Vector pos, LSL_Vector dir)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llEdgeOfWorld");
            return LSL_Integer.Zero;
        }

        /// <summary>
        /// Not fully implemented yet. Still to do:-
        /// AGENT_BUSY
        /// Remove as they are done
        /// </summary>
        public LSL_Integer llGetAgentInfo(string id)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetAgentInfo");
            return LSL_Integer.Zero;
        }

        public LSL_String llGetAgentLanguage(string id)
        {
            // This should only return a value if the avatar is in the same region
            //ckrinke 1-30-09 : This needs to parse the XMLRPC language field supplied
            //by the client at login. Currently returning only en-us until our I18N
            //effort gains momentum
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetAgentLanguage");
            return "en-us";
        }

        public void llAdjustSoundVolume(double volume)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llAdjustSoundVolume");
            // ScriptSleep(100);
        }

        public void llSetSoundQueueing(int queue)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetSoundQueueing");
        }

        public void llSetSoundRadius(double radius)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetSoundRadius");
        }

        public LSL_String llKey2Name(string id)
        {
            hostObject.AddScriptLPS(1);

            UUID key;
            if (UUID.TryParse(id, out key))
            {
                Agent agent;
                SimulationObject obj;

                if (server.Scene.TryGetAgent(key, out agent))
                    return agent.FullName;
                else if (server.Scene.TryGetObject(key, out obj))
                    return obj.Prim.Properties.Name;
            }

            return LSL_String.Empty;
        }

        public void llSetTextureAnim(int mode, int face, int sizex, int sizey, double start, double length, double rate)
        {
            hostObject.AddScriptLPS(1);
            Primitive.TextureAnimation pTexAnim = new Primitive.TextureAnimation();
            pTexAnim.Flags = (Primitive.TextureAnimMode)mode;

            //ALL_SIDES
            if (face == ScriptTypes.ALL_SIDES)
                face = 255;

            pTexAnim.Face = (uint)face;
            pTexAnim.Length = (float)length;
            pTexAnim.Rate = (float)rate;
            pTexAnim.SizeX = (uint)sizex;
            pTexAnim.SizeY = (uint)sizey;
            pTexAnim.Start = (float)start;

            hostObject.Prim.TextureAnim = pTexAnim;
            server.Scene.ObjectAdd(this, hostObject, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        public void llTriggerSoundLimited(string sound, double volume, LSL_Vector top_north_east, LSL_Vector bottom_south_west)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llTriggerSoundLimited");
        }

        public void llEjectFromLand(string pest)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llEjectFromLand");
        }

        public LSL_List llParseString2List(string str, LSL_List separators, LSL_List in_spacers)
        {
            hostObject.AddScriptLPS(1);
            LSL_List ret = new LSL_List();
            LSL_List spacers = new LSL_List();
            if (in_spacers.Length > 0 && separators.Length > 0)
            {
                for (int i = 0; i < in_spacers.Length; i++)
                {
                    object s = in_spacers.Data[i];
                    for (int j = 0; j < separators.Length; j++)
                    {
                        if (separators.Data[j].ToString() == s.ToString())
                        {
                            s = null;
                            break;
                        }
                    }
                    if (s != null)
                    {
                        spacers.Add(s);
                    }
                }
            }

            object[] delimiters = new object[separators.Length + spacers.Length];
            separators.Data.CopyTo(delimiters, 0);
            spacers.Data.CopyTo(delimiters, separators.Length);
            bool dfound = false;

            do
            {
                dfound = false;
                int cindex = -1;
                string cdeli = "";
                for (int i = 0; i < delimiters.Length; i++)
                {
                    int index = str.IndexOf(delimiters[i].ToString());
                    bool found = index != -1;
                    if (found && String.Empty != delimiters[i].ToString())
                    {
                        if ((cindex > index) || (cindex == -1))
                        {
                            cindex = index;
                            cdeli = delimiters[i].ToString();
                        }
                        dfound = dfound || found;
                    }
                }
                if (cindex != -1)
                {
                    if (cindex > 0)
                    {
                        ret.Add(new LSL_String(str.Substring(0, cindex)));
                    }
                    // Cannot use spacers.Contains() because spacers may be either type String or LSLString
                    for (int j = 0; j < spacers.Length; j++)
                    {
                        if (spacers.Data[j].ToString() == cdeli)
                        {
                            ret.Add(new LSL_String(cdeli));
                            break;
                        }
                    }
                    str = str.Substring(cindex + cdeli.Length);
                }
            } while (dfound);

            if (str != "")
                ret.Add(new LSL_String(str));

            return ret;
        }

        public LSL_Integer llOverMyLand(string id)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llOverMyLand");
            return LSL_Integer.Zero;
        }

        public LSL_Key llGetLandOwnerAt(LSL_Vector pos)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetLandOwnerAt");
            return LSL_Key.Zero;
        }

        /// <summary>
        /// According to http://lslwiki.net/lslwiki/wakka.php?wakka=llGetAgentSize
        /// only the height of avatars vary and that says:- 
        /// Width (x) and depth (y) are constant. (0.45m and 0.6m respectively).
        /// </summary>
        public LSL_Vector llGetAgentSize(string id)
        {
            UUID key;
            Agent agent;

            if (UUID.TryParse(id, out key) && server.Scene.TryGetAgent(key, out agent))
                return new LSL_Vector(0.45, 0.6, agent.Height);
            else
                return LSL_Vector.Zero;
        }

        public LSL_Integer llSameGroup(string agent)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSameGroup");
            return LSL_Integer.Zero;
        }

        public void llUnSit(string id)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llUnSit");
        }

        public LSL_Vector llGroundSlope(LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);

            Vector3 pos = hostObject.GetSimulatorPosition() +
                new Vector3((float)offset.x, (float)offset.y, (float)offset.z);

            Vector3 p0 = new Vector3(pos.X, pos.Y, (float)llGround(new LSL_Vector(pos.X, pos.Y, pos.Z)));
            Vector3 p1 = new Vector3(pos.X + 1, pos.Y, (float)llGround(new LSL_Vector(pos.X + 1, pos.Y, pos.Z)));
            Vector3 p2 = new Vector3(pos.X, pos.Y + 1, (float)llGround(new LSL_Vector(pos.X, pos.Y + 1, pos.Z)));

            Vector3 v0 = new Vector3(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3 v1 = new Vector3(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);

            v0.Normalize();
            v1.Normalize();

            return new LSL_Vector(
                (v0.Y * v1.Z) - (v0.Z * v1.Y),
                (v0.Z * v1.X) - (v0.X * v1.Z),
                (v0.X * v1.Y) - (v0.Y * v1.X));
        }

        public LSL_Vector llGroundNormal(LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            LSL_Vector x = llGroundSlope(offset);
            return new LSL_Vector(x.x, x.y, 1.0);
        }

        public LSL_Vector llGroundContour(LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            LSL_Vector x = llGroundSlope(offset);
            return new LSL_Vector(-x.y, x.x, 0.0);
        }

        public LSL_Integer llGetAttached()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetAttached");
            return LSL_Integer.Zero;
        }

        public LSL_Integer llGetFreeMemory()
        {
            hostObject.AddScriptLPS(1);
            // Make scripts designed for LSO happy
            return 16384;
        }

        public LSL_String llGetRegionName()
        {
            hostObject.AddScriptLPS(1);
            return server.Scene.RegionName;
        }

        public LSL_Float llGetRegionTimeDilation()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetRegionTimeDilation");
            return 1.0f;
        }

        public LSL_Float llGetRegionFPS()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetRegionFPS");
            return LSL_Float.Zero;
        }

        public void llParticleSystem(LSL_List rules)
        {
            hostObject.AddScriptLPS(1);
            if (rules.Length == 0)
            {
                hostObject.Prim.ParticleSys = new Primitive.ParticleSystem();
            }
            else
            {
                Primitive.ParticleSystem prules = GetNewParticleSystemWithSLDefaultValues();
                LSL_Vector tempv = LSL_Vector.Zero;

                float tempf = 0;

                for (int i = 0; i < rules.Length; i += 2)
                {
                    switch ((int)rules.Data[i])
                    {
                        case (int)ScriptTypes.PSYS_PART_FLAGS:
                            prules.PartDataFlags = (Primitive.ParticleSystem.ParticleDataFlags)(uint)rules.GetLSLIntegerItem(i + 1);
                            break;

                        case (int)ScriptTypes.PSYS_PART_START_COLOR:
                            tempv = rules.GetVector3Item(i + 1);
                            prules.PartStartColor.R = (float)tempv.x;
                            prules.PartStartColor.G = (float)tempv.y;
                            prules.PartStartColor.B = (float)tempv.z;
                            break;

                        case (int)ScriptTypes.PSYS_PART_START_ALPHA:
                            tempf = (float)rules.GetLSLFloatItem(i + 1);
                            prules.PartStartColor.A = tempf;
                            break;

                        case (int)ScriptTypes.PSYS_PART_END_COLOR:
                            tempv = rules.GetVector3Item(i + 1);
                            prules.PartEndColor.R = (float)tempv.x;
                            prules.PartEndColor.G = (float)tempv.y;
                            prules.PartEndColor.B = (float)tempv.z;
                            break;

                        case (int)ScriptTypes.PSYS_PART_END_ALPHA:
                            tempf = (float)rules.GetLSLFloatItem(i + 1);
                            prules.PartEndColor.A = tempf;
                            break;

                        case (int)ScriptTypes.PSYS_PART_START_SCALE:
                            tempv = rules.GetVector3Item(i + 1);
                            prules.PartStartScaleX = (float)tempv.x;
                            prules.PartStartScaleY = (float)tempv.y;
                            break;

                        case (int)ScriptTypes.PSYS_PART_END_SCALE:
                            tempv = rules.GetVector3Item(i + 1);
                            prules.PartEndScaleX = (float)tempv.x;
                            prules.PartEndScaleY = (float)tempv.y;
                            break;

                        case (int)ScriptTypes.PSYS_PART_MAX_AGE:
                            tempf = (float)rules.GetLSLFloatItem(i + 1);
                            prules.PartMaxAge = tempf;
                            break;

                        case (int)ScriptTypes.PSYS_SRC_ACCEL:
                            tempv = rules.GetVector3Item(i + 1);
                            prules.PartAcceleration.X = (float)tempv.x;
                            prules.PartAcceleration.Y = (float)tempv.y;
                            prules.PartAcceleration.Z = (float)tempv.z;
                            break;

                        case (int)ScriptTypes.PSYS_SRC_PATTERN:
                            int tmpi = (int)rules.GetLSLIntegerItem(i + 1);
                            prules.Pattern = (Primitive.ParticleSystem.SourcePattern)tmpi;
                            break;

                        case (int)ScriptTypes.PSYS_SRC_TEXTURE:
                            prules.Texture = KeyOrName(rules.GetLSLStringItem(i + 1));
                            break;

                        case (int)ScriptTypes.PSYS_SRC_BURST_RATE:
                            tempf = (float)rules.GetLSLFloatItem(i + 1);
                            prules.BurstRate = (float)tempf;
                            break;

                        case (int)ScriptTypes.PSYS_SRC_BURST_PART_COUNT:
                            prules.BurstPartCount = (byte)(int)rules.GetLSLIntegerItem(i + 1);
                            break;

                        case (int)ScriptTypes.PSYS_SRC_BURST_RADIUS:
                            tempf = (float)rules.GetLSLFloatItem(i + 1);
                            prules.BurstRadius = (float)tempf;
                            break;

                        case (int)ScriptTypes.PSYS_SRC_BURST_SPEED_MIN:
                            tempf = (float)rules.GetLSLFloatItem(i + 1);
                            prules.BurstSpeedMin = (float)tempf;
                            break;

                        case (int)ScriptTypes.PSYS_SRC_BURST_SPEED_MAX:
                            tempf = (float)rules.GetLSLFloatItem(i + 1);
                            prules.BurstSpeedMax = (float)tempf;
                            break;

                        case (int)ScriptTypes.PSYS_SRC_MAX_AGE:
                            tempf = (float)rules.GetLSLFloatItem(i + 1);
                            prules.MaxAge = (float)tempf;
                            break;

                        case (int)ScriptTypes.PSYS_SRC_TARGET_KEY:
                            UUID key = UUID.Zero;
                            if (UUID.TryParse(rules.Data[i + 1].ToString(), out key))
                            {
                                prules.Target = key;
                            }
                            else
                            {
                                prules.Target = hostObject.Prim.ID;
                            }
                            break;

                        case (int)ScriptTypes.PSYS_SRC_OMEGA:
                            // AL: This is an assumption, since it is the only thing that would match.
                            tempv = rules.GetVector3Item(i + 1);
                            prules.AngularVelocity.X = (float)tempv.x;
                            prules.AngularVelocity.Y = (float)tempv.y;
                            prules.AngularVelocity.Z = (float)tempv.z;
                            break;

                        case (int)ScriptTypes.PSYS_SRC_ANGLE_BEGIN:
                            tempf = (float)rules.GetLSLFloatItem(i + 1);
                            prules.InnerAngle = (float)tempf;
                            break;

                        case (int)ScriptTypes.PSYS_SRC_ANGLE_END:
                            tempf = (float)rules.GetLSLFloatItem(i + 1);
                            prules.OuterAngle = (float)tempf;
                            break;
                    }

                }

                prules.CRC = 1;
                hostObject.Prim.ParticleSys = prules;
            }

            server.Scene.ObjectAdd(this, hostObject, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        public void llGroundRepel(double height, int water, double tau)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGroundRepel");
        }

        public void llGiveInventoryList(string destination, string category, LSL_List inventory)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGiveInventoryList");
        }

        public void llSetVehicleType(int type)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetVehicleType");
        }

        public void llSetVehicleFloatParam(int param, LSL_Float value)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetVehicleFloatParam");
        }

        public void llSetVehicleVectorParam(int param, LSL_Vector vec)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetVehicleVectorParam");
        }

        public void llSetVehicleRotationParam(int param, LSL_Rotation rot)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetVehicleRotationParam");
        }

        public void llSetVehicleFlags(int flags)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetVehicleFlags");
        }

        public void llRemoveVehicleFlags(int flags)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRemoveVehicleFlags");
        }

        public void llSitTarget(LSL_Vector offset, LSL_Rotation rot)
        {
            hostObject.AddScriptLPS(1);
            // LSL quaternions can normalize to 0, normal Quaternions can't.
            if (rot.s == 0 && rot.x == 0 && rot.y == 0 && rot.z == 0)
                rot.z = 1; // ZERO_ROTATION = 0,0,0,1

            NotImplemented("llSitTarget");
        }

        public LSL_Key llAvatarOnSitTarget()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llAvatarOnSitTarget");
            return LSL_Key.Zero;
        }

        public void llAddToLandPassList(string avatar, double hours)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llAddToLandPassList");
        }

        public void llSetTouchText(string text)
        {
            hostObject.AddScriptLPS(1);
            //TODO: Constraints?
            hostObject.Prim.Properties.TouchName = text;
        }

        public void llSetSitText(string text)
        {
            hostObject.AddScriptLPS(1);
            // TODO: Constraints?
            hostObject.Prim.Properties.SitName = text;
        }

        public void llSetCameraEyeOffset(LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetCameraEyeOffset");
        }

        public void llSetCameraAtOffset(LSL_Vector offset)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetCameraAtOffset");
        }

        public LSL_String llDumpList2String(LSL_List src, string seperator)
        {
            hostObject.AddScriptLPS(1);
            if (src.Length == 0)
            {
                return String.Empty;
            }
            string ret = String.Empty;
            foreach (object o in src.Data)
            {
                ret = ret + o.ToString() + seperator;
            }
            ret = ret.Substring(0, ret.Length - seperator.Length);
            return ret;
        }

        public LSL_Integer llScriptDanger(LSL_Vector pos)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llScriptDanger");
            return LSL_Integer.Zero;
        }

        public void llDialog(string avatar, string message, LSL_List buttons, int chat_channel)
        {
            NotImplemented("llDialog");
        }

        public void llVolumeDetect(int detect)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llVolumeDetect");
        }

        /// <summary>
        /// This is a depecated function so this just replicates the result of
        /// invoking it in SL
        /// </summary>
        public void llRemoteLoadScript(string target, string name, int running, int start_param)
        {
            hostObject.AddScriptLPS(1);
            // Report an error as it does in SL
            ShoutError("Deprecated. Please use llRemoteLoadScriptPin instead.");
            // ScriptSleep(3000);
        }

        public void llSetRemoteScriptAccessPin(int pin)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetRemoteScriptAccessPin");
        }

        public void llRemoteLoadScriptPin(string target, string name, int pin, int running, int start_param)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRemoteLoadScriptPin");
        }

        public void llOpenRemoteDataChannel()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llOpenRemoteDataChannel");
        }

        public LSL_Key llSendRemoteData(string channel, string dest, int idata, string sdata)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSendRemoteData");
            return LSL_Key.Zero;
        }

        public void llRemoteDataReply(string channel, string message_id, string sdata, int idata)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRemoteDataReply");
        }

        public void llCloseRemoteDataChannel(string channel)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llCloseRemoteDataChannel");
        }

        public LSL_String llMD5String(string src, int nonce)
        {
            hostObject.AddScriptLPS(1);
            return Utils.MD5String(src + ":" + nonce.ToString());
        }

        public LSL_String llSHA1String(string src)
        {
            hostObject.AddScriptLPS(1);
            return Utils.SHA1String(src).ToLower();
        }

        public void llSetPrimitiveParams(LSL_List rules)
        {
            hostObject.AddScriptLPS(1);
            SetPrimParams(hostObject, rules);
        }

        public void llSetLinkPrimitiveParams(int linknumber, LSL_List rules)
        {
            hostObject.AddScriptLPS(1);

            List<SimulationObject> parts = GetLinkParts(linknumber);

            foreach (SimulationObject part in parts)
                SetPrimParams(part, rules);
        }

        public LSL_String llStringToBase64(string str)
        {
            hostObject.AddScriptLPS(1);
            try
            {
                byte[] encData_byte = new byte[str.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(str);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Encode" + e.Message);
            }
        }

        public LSL_String llBase64ToString(string str)
        {
            hostObject.AddScriptLPS(1);
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            try
            {
                byte[] todecode_byte = Convert.FromBase64String(str);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Decode" + e.Message);
            }
        }

        public LSL_String llXorBase64Strings(string str1, string str2)
        {
            hostObject.AddScriptLPS(1);
            Deprecated("llXorBase64Strings");
            // ScriptSleep(300);
            return String.Empty;
        }

        public void llRemoteDataSetRegion()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRemoteDataSetRegion");
        }

        public LSL_Float llLog10(double val)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Log10(val);
        }

        public LSL_Float llLog(double val)
        {
            hostObject.AddScriptLPS(1);
            return (double)Math.Log(val);
        }

        public LSL_List llGetAnimationList(string id)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetAnimationList");
            return new LSL_List();
        }

        public void llSetParcelMusicURL(string url)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetParcelMusicURL");
            // ScriptSleep(2000);
        }

        public LSL_Vector llGetRootPosition()
        {
            hostObject.AddScriptLPS(1);
            SimulationObject parent = hostObject.GetLinksetParent();
            return new LSL_Vector(parent.Prim.Position.X, parent.Prim.Position.Y, parent.Prim.Position.Z);
        }

        /// <summary>
        /// http://lslwiki.net/lslwiki/wakka.php?wakka=llGetRot
        /// http://lslwiki.net/lslwiki/wakka.php?wakka=ChildRotation
        /// Also tested in sl in regards to the behaviour in attachments/mouselook
        /// In the root prim:-
        ///     Returns the object rotation if not attached
        ///     Returns the avatars rotation if attached
        ///     Returns the camera rotation if attached and the avatar is in mouselook
        /// </summary>
        public LSL_Rotation llGetRootRotation()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetRootRotation");
            return LSL_Rotation.Identity;
        }

        public LSL_String llGetObjectDesc()
        {
            hostObject.AddScriptLPS(1);
            return hostObject.Prim.Properties.Description;
        }

        public void llSetObjectDesc(string desc)
        {
            hostObject.AddScriptLPS(1);
            // TODO: Constraints?
            hostObject.Prim.Properties.Description = desc;
        }

        public LSL_String llGetCreator()
        {
            hostObject.AddScriptLPS(1);
            return hostObject.Prim.Properties.CreatorID.ToString();
        }

        public LSL_String llGetTimestamp()
        {
            hostObject.AddScriptLPS(1);
            return DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        }

        public LSL_Integer llGetNumberOfPrims()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetNumberOfPrims");
            return LSL_Integer.Zero;
        }

        /// <summary>
        /// A partial implementation.
        /// http://lslwiki.net/lslwiki/wakka.php?wakka=llGetBoundingBox
        /// So far only valid for standing/flying/ground sitting avatars and single prim objects.
        /// If the object has multiple prims and/or a sitting avatar then the bounding
        /// box is for the root prim only.
        /// </summary>
        public LSL_List llGetBoundingBox(string obj)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetBoundingBox");
            return new LSL_List();
        }

        public LSL_Vector llGetGeometricCenter()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetGeometricCenter");
            return LSL_Vector.Zero;
        }

        public LSL_List llGetPrimitiveParams(LSL_List rules)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetPrimitiveParams");
            return new LSL_List();
        }

        //  <summary>
        //  Converts a 32-bit integer into a Base64
        //  character string. Base64 character strings
        //  are always 8 characters long. All iinteger
        //  values are acceptable.
        //  </summary>
        //  <param name="number">
        //  32-bit integer to be converted.
        //  </param>
        //  <returns>
        //  8 character string. The 1st six characters
        //  contain the encoded number, the last two
        //  characters are padded with "=".
        //  </returns>
        public LSL_String llIntegerToBase64(int number)
        {
            // uninitialized string
            char[] imdt = new char[8];

            hostObject.AddScriptLPS(1);

            // Manually unroll the loop
            imdt[7] = '=';
            imdt[6] = '=';
            imdt[5] = i2ctable[number << 4 & 0x3F];
            imdt[4] = i2ctable[number >> 2 & 0x3F];
            imdt[3] = i2ctable[number >> 8 & 0x3F];
            imdt[2] = i2ctable[number >> 14 & 0x3F];
            imdt[1] = i2ctable[number >> 20 & 0x3F];
            imdt[0] = i2ctable[number >> 26 & 0x3F];

            return new string(imdt);
        }

        //  <summary>
        //  Converts an eight character base-64 string
        //  into a 32-bit integer.
        //  </summary>
        //  <param name="str">
        //  8 characters string to be converted. Other
        //  length strings return zero.
        //  </param>
        //  <returns>
        //  Returns an integer representing the
        //  encoded value providedint he 1st 6
        //  characters of the string.
        //  </returns>
        //  <remarks>
        //  This is coded to behave like LSL's
        //  implementation (I think), based upon the
        //  information available at the Wiki.
        //  If more than 8 characters are supplied,
        //  zero is returned.
        //  If a NULL string is supplied, zero will
        //  be returned.
        //  If fewer than 6 characters are supplied, then
        //  the answer will reflect a partial
        //  accumulation.
        //  <para>
        //  The 6-bit segments are
        //  extracted left-to-right in big-endian mode,
        //  which means that segment 6 only contains the
        //  two low-order bits of the 32 bit integer as
        //  its high order 2 bits. A short string therefore
        //  means loss of low-order information. E.g.
        //
        //  |<---------------------- 32-bit integer ----------------------->|<-Pad->|
        //  |<--Byte 0----->|<--Byte 1----->|<--Byte 2----->|<--Byte 3----->|<-Pad->|
        //  |3|3|2|2|2|2|2|2|2|2|2|2|1|1|1|1|1|1|1|1|1|1| | | | | | | | | | |P|P|P|P|
        //  |1|0|9|8|7|6|5|4|3|2|1|0|9|8|7|6|5|4|3|2|1|0|9|8|7|6|5|4|3|2|1|0|P|P|P|P|
        //  |  str[0]   |  str[1]   |  str[2]   |  str[3]   |  str[4]   |  str[6]   |
        //
        //  </para>
        //  </remarks>
        public LSL_Integer llBase64ToInteger(string str)
        {
            int number = 0;
            int digit;

            hostObject.AddScriptLPS(1);

            //    Require a well-fromed base64 string
            if (str.Length > 8)
                return 0;

            //    The loop is unrolled in the interests
            //    of performance and simple necessity.
            //
            //    MUST find 6 digits to be well formed
            //      -1 == invalid
            //       0 == padding
            if ((digit = c2itable[str[0]]) <= 0)
            {
                return digit < 0 ? (int)0 : number;
            }
            number += --digit << 26;

            if ((digit = c2itable[str[1]]) <= 0)
            {
                return digit < 0 ? (int)0 : number;
            }
            number += --digit << 20;

            if ((digit = c2itable[str[2]]) <= 0)
            {
                return digit < 0 ? (int)0 : number;
            }
            number += --digit << 14;

            if ((digit = c2itable[str[3]]) <= 0)
            {
                return digit < 0 ? (int)0 : number;
            }
            number += --digit << 8;

            if ((digit = c2itable[str[4]]) <= 0)
            {
                return digit < 0 ? (int)0 : number;
            }
            number += --digit << 2;

            if ((digit = c2itable[str[5]]) <= 0)
            {
                return digit < 0 ? (int)0 : number;
            }
            number += --digit >> 4;

            // ignore trailing padding
            return number;
        }

        public LSL_Float llGetGMTclock()
        {
            hostObject.AddScriptLPS(1);
            return DateTime.UtcNow.TimeOfDay.TotalSeconds;
        }

        public LSL_String llGetSimulatorHostname()
        {
            hostObject.AddScriptLPS(1);
            return System.Environment.MachineName;
        }

        //  <summary>
        //  Scan the string supplied in 'src' and
        //  tokenize it based upon two sets of
        //  tokenizers provided in two lists,
        //  separators and spacers.
        //  </summary>
        //
        //  <remarks>
        //  Separators demarcate tokens and are
        //  elided as they are encountered. Spacers
        //  also demarcate tokens, but are themselves
        //  retained as tokens.
        //
        //  Both separators and spacers may be arbitrarily
        //  long strings. i.e. ":::".
        //
        //  The function returns an ordered list
        //  representing the tokens found in the supplied
        //  sources string. If two successive tokenizers
        //  are encountered, then a NULL entry is added
        //  to the list.
        //
        //  It is a precondition that the source and
        //  toekizer lisst are non-null. If they are null,
        //  then a null pointer exception will be thrown
        //  while their lengths are being determined.
        //
        //  A small amount of working memoryis required
        //  of approximately 8*#tokenizers.
        //
        //  There are many ways in which this function
        //  can be implemented, this implementation is
        //  fairly naive and assumes that when the
        //  function is invooked with a short source
        //  string and/or short lists of tokenizers, then
        //  performance will not be an issue.
        //
        //  In order to minimize the perofrmance
        //  effects of long strings, or large numbers
        //  of tokeizers, the function skips as far as
        //  possible whenever a toekenizer is found,
        //  and eliminates redundant tokenizers as soon
        //  as is possible.
        //
        //  The implementation tries to avoid any copying
        //  of arrays or other objects.
        //  </remarks>

        public LSL_List llParseStringKeepNulls(string src, LSL_List separators, LSL_List spacers)
        {
            int beginning = 0;
            int srclen = src.Length;
            int seplen = separators.Length;
            object[] separray = separators.Data;
            int spclen = spacers.Length;
            object[] spcarray = spacers.Data;
            int mlen = seplen + spclen;

            int[] offset = new int[mlen + 1];
            bool[] active = new bool[mlen];

            int best;
            int j;

            //    Initial capacity reduces resize cost
            LSL_List tokens = new LSL_List();

            hostObject.AddScriptLPS(1);

            //    All entries are initially valid
            for (int i = 0; i < mlen; i++)
                active[i] = true;

            offset[mlen] = srclen;

            while (beginning < srclen)
            {

                best = mlen;    // as bad as it gets

                //    Scan for separators
                for (j = 0; j < seplen; j++)
                {
                    if (active[j])
                    {
                        // scan all of the markers
                        if ((offset[j] = src.IndexOf(separray[j].ToString(), beginning)) == -1)
                        {
                            // not present at all
                            active[j] = false;
                        }
                        else
                        {
                            // present and correct
                            if (offset[j] < offset[best])
                            {
                                // closest so far
                                best = j;
                                if (offset[best] == beginning)
                                    break;
                            }
                        }
                    }
                }

                //    Scan for spacers
                if (offset[best] != beginning)
                {
                    for (j = seplen; (j < mlen) && (offset[best] > beginning); j++)
                    {
                        if (active[j])
                        {
                            // scan all of the markers
                            if ((offset[j] = src.IndexOf(spcarray[j - seplen].ToString(), beginning)) == -1)
                            {
                                // not present at all
                                active[j] = false;
                            }
                            else
                            {
                                // present and correct
                                if (offset[j] < offset[best])
                                {
                                    // closest so far
                                    best = j;
                                }
                            }
                        }
                    }
                }

                //    This is the normal exit from the scanning loop

                if (best == mlen)
                {
                    // no markers were found on this pass
                    // so we're pretty much done
                    tokens.Add(new LSL_String(src.Substring(beginning, srclen - beginning)));
                    break;
                }

                //    Otherwise we just add the newly delimited token
                //    and recalculate where the search should continue.

                tokens.Add(new LSL_String(src.Substring(beginning, offset[best] - beginning)));

                if (best < seplen)
                {
                    beginning = offset[best] + (separray[best].ToString()).Length;
                }
                else
                {
                    beginning = offset[best] + (spcarray[best - seplen].ToString()).Length;
                    tokens.Add(new LSL_String(spcarray[best - seplen].ToString()));
                }
            }

            //    This an awkward an not very intuitive boundary case. If the
            //    last substring is a tokenizer, then there is an implied trailing
            //    null list entry. Hopefully the single comparison will not be too
            //    arduous. Alternatively the 'break' could be replaced with a return
            //    but that's shabby programming.
            if (beginning == srclen)
            {
                if (srclen != 0)
                    tokens.Add(new LSL_String(""));
            }

            return tokens;
        }

        public LSL_Integer llGetObjectPermMask(int mask)
        {
            hostObject.AddScriptLPS(1);

            int permmask = 0;

            if (mask == ScriptTypes.MASK_BASE)//0
                permmask = (int)hostObject.Prim.Properties.Permissions.BaseMask;
            else if (mask == ScriptTypes.MASK_OWNER)//1
                permmask = (int)hostObject.Prim.Properties.Permissions.OwnerMask;
            else if (mask == ScriptTypes.MASK_GROUP)//2
                permmask = (int)hostObject.Prim.Properties.Permissions.GroupMask;
            else if (mask == ScriptTypes.MASK_EVERYONE)//3
                permmask = (int)hostObject.Prim.Properties.Permissions.EveryoneMask;
            else if (mask == ScriptTypes.MASK_NEXT)//4
                permmask = (int)hostObject.Prim.Properties.Permissions.NextOwnerMask;

            return permmask;
        }

        public void llSetObjectPermMask(int mask, int value)
        {
            hostObject.AddScriptLPS(1);

            if (isGodMode)
            {
                if (mask == ScriptTypes.MASK_BASE)//0
                    hostObject.Prim.Properties.Permissions.BaseMask = (PermissionMask)value;
                else if (mask == ScriptTypes.MASK_OWNER)//1
                    hostObject.Prim.Properties.Permissions.OwnerMask = (PermissionMask)value;
                else if (mask == ScriptTypes.MASK_GROUP)//2
                    hostObject.Prim.Properties.Permissions.GroupMask = (PermissionMask)value;
                else if (mask == ScriptTypes.MASK_EVERYONE)//3
                    hostObject.Prim.Properties.Permissions.EveryoneMask = (PermissionMask)value;
                else if (mask == ScriptTypes.MASK_NEXT)//4
                    hostObject.Prim.Properties.Permissions.NextOwnerMask = (PermissionMask)value;
            }
        }

        public LSL_Integer llGetInventoryPermMask(string itemName, int mask)
        {
            hostObject.AddScriptLPS(1);

            InventoryTaskItem findItem = server.TaskInventory.FindItem(hostObject.Prim.ID,
                delegate(InventoryTaskItem item) { return item.Name == itemName; });

            if (findItem != null)
            {
                switch (mask)
                {
                    case 0: return (int)findItem.Permissions.BaseMask;
                    case 1: return (int)findItem.Permissions.OwnerMask;
                    case 2: return (int)findItem.Permissions.GroupMask;
                    case 3: return (int)findItem.Permissions.EveryoneMask;
                    case 4: return (int)findItem.Permissions.NextOwnerMask;
                }
            }

            return -1;
        }

        public void llSetInventoryPermMask(string item, int mask, int value)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetInventoryPermMask");
        }

        public LSL_Key llGetInventoryCreator(string item)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetInventoryCreator");
            return LSL_Key.Zero;
        }

        public void llOwnerSay(string msg)
        {
            hostObject.AddScriptLPS(1);

            if (msg.Length > 1023)
                msg = msg.Substring(0, 1023);

            server.Scene.ObjectChat(this, hostObject.Prim.Properties.OwnerID, hostObject.Prim.ID, ChatAudibleLevel.Fully, ChatType.OwnerSay,
                ChatSourceType.Object, hostObject.Prim.Properties.Name, hostObject.GetSimulatorPosition(), 0, msg);
        }

        public LSL_Key llRequestSimulatorData(string simulator, int data)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRequestSimulatorData");
            return LSL_Key.Zero;
        }

        public void llForceMouselook(int mouselook)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llForceMouselook");
        }

        public LSL_Float llGetObjectMass(string id)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetObjectMass");
            return LSL_Float.Zero;
        }

        /// <summary>
        /// illListReplaceList removes the sub-list defined by the inclusive indices
        /// start and end and inserts the src list in its place. The inclusive
        /// nature of the indices means that at least one element must be deleted
        /// if the indices are within the bounds of the existing list. I.e. 2,2
        /// will remove the element at index 2 and replace it with the source
        /// list. Both indices may be negative, with the usual interpretation. An
        /// interesting case is where end is lower than start. As these indices
        /// bound the list to be removed, then 0->end, and start->lim are removed
        /// and the source list is added as a suffix.
        /// </summary>
        public LSL_List llListReplaceList(LSL_List dest, LSL_List src, int start, int end)
        {
            LSL_List pref = null;

            hostObject.AddScriptLPS(1);

            // Note that although we have normalized, both
            // indices could still be negative.
            if (start < 0)
            {
                start = start + dest.Length;
            }

            if (end < 0)
            {
                end = end + dest.Length;
            }
            // The comventional case, remove a sequence starting with
            // start and ending with end. And then insert the source
            // list.
            if (start <= end)
            {
                // If greater than zero, then there is going to be a
                // surviving prefix. Otherwise the inclusive nature
                // of the indices mean that we're going to add the
                // source list as a prefix.
                if (start > 0)
                {
                    pref = dest.GetSublist(0, start - 1);
                    // Only add a suffix if there is something
                    // beyond the end index (it's inclusive too).
                    if (end + 1 < dest.Length)
                    {
                        return pref + src + dest.GetSublist(end + 1, -1);
                    }
                    else
                    {
                        return pref + src;
                    }
                }
                // If start is less than or equal to zero, then
                // the new list is simply a prefix. We still need to
                // figure out any necessary surgery to the destination
                // based upon end. Note that if end exceeds the upper
                // bound in this case, the entire destination list
                // is removed.
                else
                {
                    if (end + 1 < dest.Length)
                    {
                        return src + dest.GetSublist(end + 1, -1);
                    }
                    else
                    {
                        return src;
                    }
                }
            }
            // Finally, if start > end, we strip away a prefix and
            // a suffix, to leave the list that sits <between> ens
            // and start, and then tag on the src list. AT least
            // that's my interpretation. We can get sublist to do
            // this for us. Note that one, or both of the indices
            // might have been negative.
            else
            {
                return dest.GetSublist(end + 1, start - 1) + src;
            }
        }

        public void llLoadURL(string avatar_id, string message, string url)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llLoadURL");
            // ScriptSleep(10000);
        }

        public void llParcelMediaCommandList(LSL_List commandList)
        {
            // TODO: Not implemented yet (missing in libomv?):
            //  PARCEL_MEDIA_COMMAND_LOOP_SET    float loop      Use this to get or set the parcel's media loop duration. (1.19.1 RC0 or later)
            hostObject.AddScriptLPS(1);

            NotImplemented("llParcelMediaCommandList");
            // ScriptSleep(2000);
        }

        public LSL_List llParcelMediaQuery(LSL_List aList)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llParcelMediaQuery");
            return new LSL_List();
        }

        public LSL_Integer llModPow(int a, int b, int c)
        {
            hostObject.AddScriptLPS(1);
            long tmp = 0L;
            Math.DivRem(Convert.ToInt64(Math.Pow(a, b)), c, out tmp);
            // ScriptSleep(1000);
            return Convert.ToInt32(tmp);
        }

        public LSL_Integer llGetInventoryType(string name)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetInventoryType");
            return LSL_Integer.Zero;
        }

        public void llSetPayPrice(int price, LSL_List quick_pay_buttons)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetPayPrice");
        }

        public LSL_Vector llGetCameraPos()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetCameraPos");
            return LSL_Vector.Zero;
        }

        public LSL_Rotation llGetCameraRot()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetCameraRot");
            return LSL_Rotation.Identity;
        }

        /// <summary>
        /// The SL implementation does nothing, it is deprecated
        /// This duplicates SL
        /// </summary>
        public void llSetPrimURL(string url)
        {
            hostObject.AddScriptLPS(1);
            // ScriptSleep(2000);
        }

        /// <summary>
        /// The SL implementation shouts an error, it is deprecated
        /// This duplicates SL
        /// </summary>
        public void llRefreshPrimURL()
        {
            hostObject.AddScriptLPS(1);
            ShoutError("llRefreshPrimURL - not yet supported");
            // ScriptSleep(20000);
        }

        public LSL_String llEscapeURL(string url)
        {
            hostObject.AddScriptLPS(1);
            try
            {
                return Uri.EscapeUriString(url);
            }
            catch (Exception ex)
            {
                return "llEscapeURL: " + ex.ToString();
            }
        }

        public LSL_String llUnescapeURL(string url)
        {
            hostObject.AddScriptLPS(1);
            try
            {
                return Uri.UnescapeDataString(url);
            }
            catch (Exception ex)
            {
                return "llUnescapeURL: " + ex.ToString();
            }
        }

        public void llMapDestination(string simname, LSL_Vector pos, LSL_Vector lookAt)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llMapDestination");
            // ScriptSleep(1000);
        }

        public void llAddToLandBanList(string avatar, double hours)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llAddToLandBanList");
            // ScriptSleep(100);
        }

        public void llRemoveFromLandPassList(string avatar)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRemoveFromLandPassList");
            // ScriptSleep(100);
        }

        public void llRemoveFromLandBanList(string avatar)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llRemoveFromLandBanList");
            // ScriptSleep(100);
        }

        public void llSetCameraParams(LSL_List rules)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llSetCameraParams");
        }

        public void llClearCameraParams()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llClearCameraParams");
        }

        public LSL_Float llListStatistics(int operation, LSL_List src)
        {
            hostObject.AddScriptLPS(1);
            LSL_List nums = LSL_List.ToDoubleList(src);
            switch (operation)
            {
                case ScriptTypes.LIST_STAT_RANGE:
                    return nums.Range();
                case ScriptTypes.LIST_STAT_MIN:
                    return nums.Min();
                case ScriptTypes.LIST_STAT_MAX:
                    return nums.Max();
                case ScriptTypes.LIST_STAT_MEAN:
                    return nums.Mean();
                case ScriptTypes.LIST_STAT_MEDIAN:
                    return nums.Median();
                case ScriptTypes.LIST_STAT_NUM_COUNT:
                    return nums.NumericLength();
                case ScriptTypes.LIST_STAT_STD_DEV:
                    return nums.StdDev();
                case ScriptTypes.LIST_STAT_SUM:
                    return nums.Sum();
                case ScriptTypes.LIST_STAT_SUM_SQUARES:
                    return nums.SumSqrs();
                case ScriptTypes.LIST_STAT_GEOMETRIC_MEAN:
                    return nums.GeometricMean();
                case ScriptTypes.LIST_STAT_HARMONIC_MEAN:
                    return nums.HarmonicMean();
                default:
                    return 0.0;
            }
        }

        public LSL_Integer llGetUnixTime()
        {
            hostObject.AddScriptLPS(1);
            return Utils.DateTimeToUnixTime(DateTime.Now);
        }

        public LSL_Integer llGetParcelFlags(LSL_Vector pos)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetParcelFlags");
            return LSL_Integer.Zero;
        }

        public LSL_Integer llGetRegionFlags()
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetRegionFlags");
            return LSL_Integer.Zero;
        }

        public LSL_String llXorBase64StringsCorrect(string str1, string str2)
        {
            hostObject.AddScriptLPS(1);
            string ret = String.Empty;
            string src1 = llBase64ToString(str1);
            string src2 = llBase64ToString(str2);
            int c = 0;
            for (int i = 0; i < src1.Length; i++)
            {
                ret += (char)(src1[i] ^ src2[c]);

                c++;
                if (c >= src2.Length)
                    c = 0;
            }
            return llStringToBase64(ret);
        }

        public LSL_String llHTTPRequest(string url, LSL_List parameters, string body)
        {
            NotImplemented("llHTTPRequest");
            return LSL_String.Empty;
        }

        public void llResetLandBanList()
        {
            NotImplemented("llResetLandBanList");
        }

        public void llResetLandPassList()
        {
            NotImplemented("llResetLandBanList");
        }

        public LSL_Integer llGetParcelPrimCount(LSL_Vector pos, int category, int sim_wide)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetParcelPrimCount");
            return LSL_Integer.Zero;
        }

        public LSL_List llGetParcelPrimOwners(LSL_Vector pos)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetParcelPrimOwners");
            return new LSL_List();
        }

        public LSL_Integer llGetObjectPrimCount(string object_id)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetObjectPrimCount");
            return LSL_Integer.Zero;
        }

        public LSL_Integer llGetParcelMaxPrims(LSL_Vector pos, int sim_wide)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetParcelMaxPrims");
            return LSL_Integer.Zero;
        }

        public LSL_List llGetParcelDetails(LSL_Vector pos, LSL_List param)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetParcelDetails");
            return new LSL_List();
        }

        public LSL_String llStringTrim(string src, int type)
        {
            hostObject.AddScriptLPS(1);
            if (type == (int)ScriptTypes.STRING_TRIM_HEAD) { return src.TrimStart(); }
            if (type == (int)ScriptTypes.STRING_TRIM_TAIL) { return src.TrimEnd(); }
            if (type == (int)ScriptTypes.STRING_TRIM) { return src.Trim(); }
            return src;
        }

        public LSL_List llGetObjectDetails(string id, LSL_List args)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetObjectDetails");
            return new LSL_List();
        }

        public LSL_Key llGetNumberOfNotecardLines(string name)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetNumberOfNotecardLines");
            return LSL_Key.Zero;
        }

        public LSL_String llGetNotecardLine(string name, int line)
        {
            hostObject.AddScriptLPS(1);
            NotImplemented("llGetNotecardLine");
            return LSL_Key.Zero;
        }

        #endregion ll* Functions

        #region Helpers

        /*private void HandleScriptAnswer(Agent client, UUID taskID, UUID itemID, int answer)
        {
            if (taskID != m_host.UUID)
                return;

            UUID invItemID = InventorySelf();

            if (invItemID == UUID.Zero)
                return;

            client.OnScriptAnswer -= handleScriptAnswer;
            m_waitingForScriptAnswer = false;

            if ((answer & ScriptTypes.PERMISSION_TAKE_CONTROLS) == 0)
                llReleaseControls();

            lock (m_host.TaskInventory)
            {
                m_host.TaskInventory[invItemID].PermsMask = answer;
            }

            m_ScriptEngine.PostScriptEvent(m_itemID, new EventParams(
                    "run_time_permissions", new Object[] {
                    new LSL_Integer(answer) },
                    new DetectParams[0]));
        }*/

        private string ResolveName(UUID objectID)
        {
            // try agents
            Agent agent;
            if (server.Scene.TryGetAgent(objectID, out agent))
                return agent.FullName;

            // try scene objects
            SimulationObject obj;
            if (server.Scene.TryGetObject(objectID, out obj))
                return obj.Prim.Properties.Name;

            return String.Empty;
        }

        private List<SimulationObject> GetLinkParts(int linkType)
        {
            SimulationObject parent = null;
            List<SimulationObject> parts;

            if (hostObject.Prim.ParentID != 0)
                server.Scene.TryGetObject(hostObject.Prim.ParentID, out parent);

            switch (linkType)
            {
                case ScriptTypes.LINK_SET:
                    if (parent != null)
                    {
                        // This is a child prim
                        parts = parent.GetChildren();
                        parts.Insert(0, parent);
                    }
                    else
                    {
                        // This is a single/parent prim
                        parts = hostObject.GetChildren();
                        parts.Insert(0, hostObject);
                    }

                    return parts;
                case ScriptTypes.LINK_ROOT:
                    parts = new List<SimulationObject>(1);

                    if (parent != null)
                        // This is a child prim
                        parts.Add(parent);
                    else
                        // This is a single/parent prim
                        parts.Add(hostObject);

                    return parts;
                case ScriptTypes.LINK_ALL_OTHERS:
                    if (parent != null)
                    {
                        // This is a child prim
                        parts = parent.GetChildren();
                        parts.Remove(hostObject);
                        parts.Insert(0, parent);
                    }
                    else
                    {
                        // This is a single/parent prim
                        parts = hostObject.GetChildren();
                    }

                    return parts;
                case ScriptTypes.LINK_ALL_CHILDREN:
                    if (parent != null)
                        // This is a child prim
                        parts = parent.GetChildren();
                    else
                        // This is a single/parent prim
                        parts = hostObject.GetChildren();

                    return parts;
                case ScriptTypes.LINK_THIS:
                    parts = new List<SimulationObject>(1);
                    parts.Add(hostObject);

                    return parts;
                default:
                    // Sanity check
                    if (linkType < 0)
                        return new List<SimulationObject>(0);

                    // Look for a prim in the linkset with the given link number
                    SimulationObject prim = null;
                    if (parent != null)
                        prim = parent.GetLinksetPrim(linkType);
                    else
                        prim = hostObject.GetLinksetPrim(linkType);

                    // Return the prim if found, otherwise an empty set
                    if (prim != null)
                    {
                        parts = new List<SimulationObject>(1);
                        parts.Add(prim);
                    }
                    else
                    {
                        parts = new List<SimulationObject>(0);
                    }

                    return parts;
            }
        }

        private InventoryTaskItem InventorySelf()
        {
            hostObject.AddScriptLPS(1);
            return server.TaskInventory.FindItem(hostObject.Prim.ID,
                delegate(InventoryTaskItem item) { return item.AssetID == scriptID; });
        }

        private InventoryTaskItem InventoryKey(string name, AssetType type)
        {
            hostObject.AddScriptLPS(1);

            return server.TaskInventory.FindItem(hostObject.Prim.ID,
                delegate(InventoryTaskItem item) { return item.AssetType == type && item.Name == name; });
        }

        private InventoryTaskItem InventoryKey(string name)
        {
            hostObject.AddScriptLPS(1);

            return server.TaskInventory.FindItem(hostObject.Prim.ID,
                delegate(InventoryTaskItem item) { return item.Name == name; });
        }

        /// <summary>
        /// accepts a valid UUID, -or- a name of an inventory item.
        /// Returns a valid UUID or UUID.Zero if key invalid and item not found
        /// in prim inventory.
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        private UUID KeyOrName(string k)
        {
            UUID key;
            if (UUID.TryParse(k, out key))
                return key;
            else
                return InventoryKey(k).ID;
        }

        /// <summary>
        /// Typecasts an LSL_Rotation to a Quaternion
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private Quaternion Rot2Quaternion(LSL_Rotation r)
        {
            return new Quaternion((float)r.x, (float)r.y, (float)r.z, (float)r.s);
        }

        UUID ScriptByName(string name)
        {
            InventoryTaskItem scriptItem = server.TaskInventory.FindItem(hostObject.Prim.ID,
                delegate(InventoryTaskItem item) { return item.AssetType == AssetType.LSLText && item.Name == name; });

            return (scriptItem != null) ? scriptItem.ID : UUID.Zero;
        }

        void ShoutError(string msg)
        {
            llShout(ScriptTypes.DEBUG_CHANNEL, msg);
        }

        void NotImplemented(string command)
        {
            if (ERROR_ON_NOT_IMPLEMENTED)
                throw new NotImplementedException("Command not implemented: " + command);
        }

        void Deprecated(string command)
        {
            throw new Exception("Command deprecated: " + command);
        }

        void LSLError(string msg)
        {
            throw new Exception("LSL Runtime Error: " + msg);
        }

        // normalize an angle between 0 - 2*PI (0 and 360 degrees)
        private double NormalizeAngle(double angle)
        {
            angle = angle % (Math.PI * 2);
            if (angle < 0) angle = angle + Math.PI * 2;
            return angle;
        }

        private void SetAlpha(SimulationObject part, double alpha, int face)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;
            Color4 texcolor;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                texcolor = tex.CreateFace((uint)face).RGBA;
                texcolor.A = Utils.Clamp((float)alpha, 0.0f, 1.0f);
                tex.FaceTextures[face].RGBA = texcolor;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
            else if (face == ScriptTypes.ALL_SIDES)
            {
                for (int i = 0; i < GetNumberOfSides(part); i++)
                {
                    if (tex.FaceTextures[i] != null)
                    {
                        texcolor = tex.FaceTextures[i].RGBA;
                        texcolor.A = Utils.Clamp((float)alpha, 0.0f, 1.0f);
                        tex.FaceTextures[i].RGBA = texcolor;
                    }
                }

                texcolor = tex.DefaultTexture.RGBA;
                texcolor.A = Utils.Clamp((float)alpha, 0.0f, 1.0f);
                tex.DefaultTexture.RGBA = texcolor;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
        }

        private void SetFlexi(SimulationObject part, bool flexi, int softness, float gravity, float friction,
            float wind, float tension, LSL_Vector force)
        {
            if (flexi)
            {
                part.Prim.Flexible = new Primitive.FlexibleData();
                part.Prim.Flexible.Softness = softness;
                part.Prim.Flexible.Gravity = gravity;
                part.Prim.Flexible.Drag = friction;
                part.Prim.Flexible.Wind = wind;
                part.Prim.Flexible.Tension = tension;
                part.Prim.Flexible.Force.X = (float)force.x;
                part.Prim.Flexible.Force.Y = (float)force.y;
                part.Prim.Flexible.Force.Z = (float)force.z;
                part.Prim.PrimData.PathCurve = PathCurve.Flexible;
            }
            else
            {
                part.Prim.Flexible = null;
                part.Prim.PrimData.PathCurve = PathCurve.Line;
            }

            server.Scene.ObjectAdd(this, part, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        private void SetPointLight(SimulationObject part, bool light, LSL_Vector color, float intensity, float radius, float falloff)
        {
            if (part == null)
                return;

            if (light)
            {
                part.Prim.Light = new Primitive.LightData();
                part.Prim.Light.Color.R = Utils.Clamp((float)color.x, 0.0f, 1.0f);
                part.Prim.Light.Color.G = Utils.Clamp((float)color.y, 0.0f, 1.0f);
                part.Prim.Light.Color.B = Utils.Clamp((float)color.z, 0.0f, 1.0f);
                part.Prim.Light.Intensity = intensity;
                part.Prim.Light.Radius = radius;
                part.Prim.Light.Falloff = falloff;
            }
            else
            {
                part.Prim.Light = null;
            }

            server.Scene.ObjectAdd(this, part, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        private LSL_Vector GetColor(SimulationObject part, int face)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;
            Color4 texcolor;
            LSL_Vector rgb = LSL_Vector.Zero;

            if (face == ScriptTypes.ALL_SIDES)
            {
                int i;

                for (i = 0; i < GetNumberOfSides(part); i++)
                {
                    texcolor = tex.GetFace((uint)i).RGBA;
                    rgb.x += texcolor.R;
                    rgb.y += texcolor.G;
                    rgb.z += texcolor.B;
                }

                rgb.x /= (float)GetNumberOfSides(part);
                rgb.y /= (float)GetNumberOfSides(part);
                rgb.z /= (float)GetNumberOfSides(part);

                return rgb;
            }
            if (face >= 0 && face < GetNumberOfSides(part))
            {
                texcolor = tex.GetFace((uint)face).RGBA;
                rgb.x = texcolor.R;
                rgb.y = texcolor.G;
                rgb.z = texcolor.B;
                return rgb;
            }
            else
            {
                return new LSL_Vector();
            }
        }

        private LSL_Float GetAlpha(SimulationObject part, int face)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;

            if (face == ScriptTypes.ALL_SIDES)
            {
                int i;
                double sum = 0.0;

                for (i = 0; i < GetNumberOfSides(part); i++)
                    sum += (double)tex.GetFace((uint)i).RGBA.A;

                return sum;
            }
            else if (face >= 0 && face < GetNumberOfSides(part))
            {
                return (double)tex.GetFace((uint)face).RGBA.A;
            }
            else
            {
                return 0.0;
            }
        }

        private LSL_String GetTexture(SimulationObject part, int face)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;

            if (face == ScriptTypes.ALL_SIDES)
                face = 0;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                Primitive.TextureEntryFace texface;
                texface = tex.GetFace((uint)face);

                return texface.TextureID.ToString();
            }
            else
            {
                return LSL_String.Empty;
            }
        }

        // this function to understand which shape it is (taken from meshmerizer)
        // quite useful can be used by meshmerizer to have a centralized point of understanding the shape
        // except that it refers to scripting constants
        private int GetScriptPrimType(PrimType type)
        {
            switch (type)
            {
                case PrimType.Cylinder:
                    return ScriptTypes.PRIM_TYPE_CYLINDER;
                case PrimType.Prism:
                    return ScriptTypes.PRIM_TYPE_PRISM;
                case PrimType.Ring:
                    return ScriptTypes.PRIM_TYPE_RING;
                case PrimType.Sculpt:
                    return ScriptTypes.PRIM_TYPE_SCULPT;
                case PrimType.Sphere:
                    return ScriptTypes.PRIM_TYPE_SPHERE;
                case PrimType.Torus:
                    return ScriptTypes.PRIM_TYPE_TORUS;
                case PrimType.Tube:
                    return ScriptTypes.PRIM_TYPE_TUBE;
                case PrimType.Box:
                case PrimType.Unknown:
                default:
                    return ScriptTypes.PRIM_TYPE_BOX;
            }
        }

        // Helper functions to understand if object has cut, hollow, dimple, and other affecting number of faces
        private void HasCutHollowDimpleProfileCut(int primType, Primitive.ConstructionData shape, out bool hasCut, out bool hasHollow,
            out bool hasDimple, out bool hasProfileCut)
        {
            if (primType == ScriptTypes.PRIM_TYPE_BOX
                ||
                primType == ScriptTypes.PRIM_TYPE_CYLINDER
                ||
                primType == ScriptTypes.PRIM_TYPE_PRISM)

                hasCut = (Primitive.PackBeginCut(shape.ProfileBegin) > 0) || (Primitive.PackEndCut(shape.ProfileEnd) > 0);
            else
                hasCut = (Primitive.PackBeginCut(shape.PathBegin) > 0) || (Primitive.PackEndCut(shape.PathEnd) > 0);

            hasHollow = Primitive.PackProfileHollow(shape.ProfileHollow) > 0;
            hasDimple = (Primitive.PackBeginCut(shape.ProfileBegin) > 0) || (Primitive.PackEndCut(shape.ProfileEnd) > 0); // taken from llSetPrimitiveParams
            hasProfileCut = hasDimple; // A profile cut is required to create a dimple
        }

        private int GetNumberOfSides(SimulationObject part)
        {
            int ret = 0;
            bool hasCut;
            bool hasHollow;
            bool hasDimple;
            bool hasProfileCut;

            int primType = GetScriptPrimType(part.Prim.Type);
            HasCutHollowDimpleProfileCut(primType, part.Prim.PrimData, out hasCut, out hasHollow, out hasDimple, out hasProfileCut);

            switch (primType)
            {
                case ScriptTypes.PRIM_TYPE_BOX:
                    ret = 6;
                    if (hasCut) ret += 2;
                    if (hasHollow) ret += 1;
                    break;
                case ScriptTypes.PRIM_TYPE_CYLINDER:
                    ret = 3;
                    if (hasCut) ret += 2;
                    if (hasHollow) ret += 1;
                    break;
                case ScriptTypes.PRIM_TYPE_PRISM:
                    ret = 5;
                    if (hasCut) ret += 2;
                    if (hasHollow) ret += 1;
                    break;
                case ScriptTypes.PRIM_TYPE_SPHERE:
                    ret = 1;
                    if (hasCut) ret += 2;
                    if (hasDimple) ret += 2;
                    if (hasHollow) ret += 3; // Emulate lsl on secondlife (according to documentation it should have added only +1)
                    break;
                case ScriptTypes.PRIM_TYPE_TORUS:
                    ret = 1;
                    if (hasCut) ret += 2;
                    if (hasProfileCut) ret += 2;
                    if (hasHollow) ret += 1;
                    break;
                case ScriptTypes.PRIM_TYPE_TUBE:
                    ret = 4;
                    if (hasCut) ret += 2;
                    if (hasProfileCut) ret += 2;
                    if (hasHollow) ret += 1;
                    break;
                case ScriptTypes.PRIM_TYPE_RING:
                    ret = 3;
                    if (hasCut) ret += 2;
                    if (hasProfileCut) ret += 2;
                    if (hasHollow) ret += 1;
                    break;
                case ScriptTypes.PRIM_TYPE_SCULPT:
                    ret = 1;
                    break;
            }
            return ret;
        }

        private void SetPrimitiveBlockShapeParams(SimulationObject prim, int holeshape, LSL_Vector cut, float hollow, LSL_Vector twist)
        {
            switch (holeshape)
            {
                case ScriptTypes.PRIM_HOLE_CIRCLE:
                    prim.Prim.PrimData.ProfileHole = HoleType.Circle;
                    break;
                case ScriptTypes.PRIM_HOLE_SQUARE:
                    prim.Prim.PrimData.ProfileHole = HoleType.Square;
                    break;
                case ScriptTypes.PRIM_HOLE_TRIANGLE:
                    prim.Prim.PrimData.ProfileHole = HoleType.Triangle;
                    break;
                case ScriptTypes.PRIM_HOLE_DEFAULT:
                default:
                    prim.Prim.PrimData.ProfileHole = HoleType.Same;
                    break;
            }

            if (cut.x < 0f)
                cut.x = 0f;
            if (cut.x > 1f)
                cut.x = 1f;
            if (cut.y < 0f)
                cut.y = 0f;
            if (cut.y > 1f)
                cut.y = 1f;
            if (cut.y - cut.x < 0.05f)
                cut.x = cut.y - 0.05f;

            prim.Prim.PrimData.ProfileBegin = (float)cut.x;
            prim.Prim.PrimData.ProfileEnd = (float)cut.y;

            if (hollow < 0f)
                hollow = 0f;
            if (hollow > 0.95)
                hollow = 0.95f;

            prim.Prim.PrimData.ProfileHollow = hollow;

            if (twist.x < -1.0f)
                twist.x = -1.0f;
            if (twist.x > 1.0f)
                twist.x = 1.0f;
            if (twist.y < -1.0f)
                twist.y = -1.0f;
            if (twist.y > 1.0f)
                twist.y = 1.0f;

            prim.Prim.PrimData.PathTwistBegin = (float)twist.x;
            prim.Prim.PrimData.PathTwist = (float)twist.y;
        }

        private void SetPrimitiveShapeParams(SimulationObject prim, int holeshape, LSL_Vector cut, float hollow, LSL_Vector twist,
            LSL_Vector taper_b, LSL_Vector topshear)
        {
            SetPrimitiveBlockShapeParams(prim, holeshape, cut, hollow, twist);

            //shapeBlock.ProfileCurve += fudge;

            if (taper_b.x < 0f)
                taper_b.x = 0f;
            if (taper_b.x > 2f)
                taper_b.x = 2f;
            if (taper_b.y < 0f)
                taper_b.y = 0f;
            if (taper_b.y > 2f)
                taper_b.y = 2f;

            prim.Prim.PrimData.PathScaleX = (float)taper_b.x;
            prim.Prim.PrimData.PathScaleY = (float)taper_b.y;

            if (topshear.x < -0.5f)
                topshear.x = -0.5f;
            if (topshear.x > 0.5f)
                topshear.x = 0.5f;
            if (topshear.y < -0.5f)
                topshear.y = -0.5f;
            if (topshear.y > 0.5f)
                topshear.y = 0.5f;

            prim.Prim.PrimData.PathShearX = (float)topshear.x;
            prim.Prim.PrimData.PathShearY = (float)topshear.y;
        }

        private void SetPrimitiveShapeParams(SimulationObject prim, int holeshape, LSL_Vector cut, float hollow, LSL_Vector twist, LSL_Vector dimple)
        {
            SetPrimitiveBlockShapeParams(prim, holeshape, cut, hollow, twist);

            // profile/path swapped for a sphere
            prim.Prim.PrimData.PathBegin = prim.Prim.PrimData.ProfileBegin;
            prim.Prim.PrimData.PathEnd = prim.Prim.PrimData.ProfileEnd;

            //shapeBlock.ProfileCurve += fudge;

            prim.Prim.PrimData.PathScaleX = Primitive.UnpackPathScale(100);
            prim.Prim.PrimData.PathScaleY = Primitive.UnpackPathScale(100);

            if (dimple.x < 0f)
                dimple.x = 0f;
            if (dimple.x > 1f)
                dimple.x = 1f;
            if (dimple.y < 0f)
                dimple.y = 0f;
            if (dimple.y > 1f)
                dimple.y = 1f;
            if (dimple.y - cut.x < 0.05f)
                dimple.x = cut.y - 0.05f;

            prim.Prim.PrimData.ProfileBegin = Primitive.UnpackBeginCut((ushort)(50000.0 * dimple.x));
            prim.Prim.PrimData.ProfileEnd = Primitive.UnpackBeginCut((ushort)(50000.0 * (1.0 - dimple.y)));

            server.Scene.ObjectAdd(this, prim, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        private void SetPrimitiveShapeParams(SimulationObject prim, int holeshape, LSL_Vector cut, float hollow, LSL_Vector twist,
            LSL_Vector holesize, LSL_Vector topshear, LSL_Vector profilecut, LSL_Vector taper_a, float revolutions, float radiusoffset,
            float skew)
        {
            SetPrimitiveBlockShapeParams(prim, holeshape, cut, hollow, twist);

            //shapeBlock.ProfileCurve += fudge;

            // profile/path swapped for a torrus, tube, ring
            //shapeBlock.PathBegin = shapeBlock.ProfileBegin;
            //shapeBlock.PathEnd = shapeBlock.ProfileEnd;

            if (holesize.x < 0.05f)
                holesize.x = 0.05f;
            if (holesize.x > 1f)
                holesize.x = 1f;
            if (holesize.y < 0.05f)
                holesize.y = 0.05f;
            if (holesize.y > 0.5f)
                holesize.y = 0.5f;

            prim.Prim.PrimData.PathScaleX = (float)holesize.x;
            prim.Prim.PrimData.PathScaleY = (float)holesize.y;

            if (topshear.x < -0.5f)
                topshear.x = -0.5f;
            if (topshear.x > 0.5f)
                topshear.x = 0.5f;
            if (topshear.y < -0.5f)
                topshear.y = -0.5f;
            if (topshear.y > 0.5f)
                topshear.y = 0.5f;

            prim.Prim.PrimData.PathShearX = (float)topshear.x;
            prim.Prim.PrimData.PathShearY = (float)topshear.y;

            if (profilecut.x < 0f)
                profilecut.x = 0f;
            if (profilecut.x > 1f)
                profilecut.x = 1f;
            if (profilecut.y < 0f)
                profilecut.y = 0f;
            if (profilecut.y > 1f)
                profilecut.y = 1f;
            if (profilecut.y - cut.x < 0.05f)
                profilecut.x = cut.y - 0.05f;

            prim.Prim.PrimData.ProfileBegin = (float)profilecut.x;
            prim.Prim.PrimData.ProfileEnd = (float)profilecut.y;

            if (taper_a.x < -1f)
                taper_a.x = -1f;
            if (taper_a.x > 1f)
                taper_a.x = 1f;
            if (taper_a.y < -1f)
                taper_a.y = -1f;
            if (taper_a.y > 1f)
                taper_a.y = 1f;

            prim.Prim.PrimData.PathTaperX = (float)taper_a.x;
            prim.Prim.PrimData.PathTaperY = (float)taper_a.y;

            if (revolutions < 1f)
                revolutions = 1f;
            if (revolutions > 4f)
                revolutions = 4f;

            prim.Prim.PrimData.PathRevolutions = revolutions;

            // limits on radiusoffset depend on revolutions and hole size (how?) seems like the maximum range is 0 to 1
            if (radiusoffset < 0f)
                radiusoffset = 0f;
            if (radiusoffset > 1f)
                radiusoffset = 1f;

            prim.Prim.PrimData.PathRadiusOffset = radiusoffset;

            if (skew < -0.95f)
                skew = -0.95f;
            if (skew > 0.95f)
                skew = 0.95f;

            prim.Prim.PrimData.PathSkew = skew;
        }

        private void SetPrimitiveShapeParams(SimulationObject prim, string map, int type)
        {
            UUID sculptId;

            if (!UUID.TryParse(map, out sculptId))
            {
                llSay(0, "Could not parse key " + map);
                return;
            }

            prim.Prim.PrimData.PathScaleX = Primitive.UnpackPathScale(100);
            prim.Prim.PrimData.PathScaleY = Primitive.UnpackPathScale(150);

            if (type != ScriptTypes.PRIM_SCULPT_TYPE_CYLINDER &&
                type != ScriptTypes.PRIM_SCULPT_TYPE_PLANE &&
                type != ScriptTypes.PRIM_SCULPT_TYPE_SPHERE &&
                type != ScriptTypes.PRIM_SCULPT_TYPE_TORUS)
            {
                // default
                type = ScriptTypes.PRIM_SCULPT_TYPE_SPHERE;
            }

            if (prim.Prim.Sculpt == null)
                prim.Prim.Sculpt = new Primitive.SculptData();

            switch (type)
            {
                case ScriptTypes.PRIM_SCULPT_TYPE_CYLINDER:
                    prim.Prim.Sculpt.Type = SculptType.Cylinder;
                    break;
                case ScriptTypes.PRIM_SCULPT_TYPE_PLANE:
                    prim.Prim.Sculpt.Type = SculptType.Plane;
                    break;
                case ScriptTypes.PRIM_SCULPT_TYPE_TORUS:
                    prim.Prim.Sculpt.Type = SculptType.Torus;
                    break;
                case ScriptTypes.PRIM_SCULPT_TYPE_SPHERE:
                default:
                    prim.Prim.Sculpt.Type = SculptType.Sphere;
                    break;
            }

            prim.Prim.Sculpt.SculptTexture = sculptId;
        }

        private void SetPrimParams(SimulationObject prim, LSL_List rules)
        {
            SimulationObject parent = prim.GetLinksetParent();
            int idx = 0;

            while (idx < rules.Length)
            {
                int face;
                LSL_Vector v;
                int code = rules.GetLSLIntegerItem(idx++);
                int remain = rules.Length - idx;

                switch (code)
                {
                    case ScriptTypes.PRIM_POSITION:
                        if (remain < 1) return;

                        v = rules.GetVector3Item(idx++);
                        SetPos(prim, v);
                        break;
                    case ScriptTypes.PRIM_SIZE:
                        if (remain < 1) return;

                        v = rules.GetVector3Item(idx++);
                        SetScale(prim, v);
                        break;
                    case ScriptTypes.PRIM_ROTATION:
                        if (remain < 1) return;

                        LSL_Rotation q = rules.GetQuaternionItem(idx++);
                        // try to let this work as in SL...
                        if (parent == prim)
                        {
                            // special case: If we are root, rotate the parent to the new rotation
                            SetRot(parent, Rot2Quaternion(q));
                        }
                        else
                        {
                            // We are a child. The rotation values will be set to the one of root modified by rot, as in SL. Don't ask.
                            SetRot(hostObject, parent.Prim.Rotation * Rot2Quaternion(q));
                        }
                        break;
                    case ScriptTypes.PRIM_TYPE:
                        if (remain < 3) return;

                        code = (int)rules.GetLSLIntegerItem(idx++);

                        remain = rules.Length - idx;
                        float hollow;
                        LSL_Vector twist;
                        LSL_Vector taper_b;
                        LSL_Vector topshear;
                        float revolutions;
                        float radiusoffset;
                        float skew;
                        LSL_Vector holesize;
                        LSL_Vector profilecut;

                        switch (code)
                        {
                            case ScriptTypes.PRIM_TYPE_BOX:
                                if (remain < 6) return;

                                face = (int)rules.GetLSLIntegerItem(idx++);
                                v = rules.GetVector3Item(idx++); // cut
                                hollow = (float)rules.GetLSLFloatItem(idx++);
                                twist = rules.GetVector3Item(idx++);
                                taper_b = rules.GetVector3Item(idx++);
                                topshear = rules.GetVector3Item(idx++);

                                prim.Prim.PrimData.PathCurve = PathCurve.Line;
                                SetPrimitiveShapeParams(prim, face, v, hollow, twist, taper_b, topshear);
                                break;
                            case ScriptTypes.PRIM_TYPE_CYLINDER:
                                if (remain < 6) return;

                                face = (int)rules.GetLSLIntegerItem(idx++); // holeshape
                                v = rules.GetVector3Item(idx++); // cut
                                hollow = (float)rules.GetLSLFloatItem(idx++);
                                twist = rules.GetVector3Item(idx++);
                                taper_b = rules.GetVector3Item(idx++);
                                topshear = rules.GetVector3Item(idx++);
                                prim.Prim.PrimData.ProfileCurve = ProfileCurve.Circle;
                                prim.Prim.PrimData.PathCurve = PathCurve.Line;
                                SetPrimitiveShapeParams(prim, face, v, hollow, twist, taper_b, topshear);
                                break;
                            case ScriptTypes.PRIM_TYPE_PRISM:
                                if (remain < 6) return;

                                face = (int)rules.GetLSLIntegerItem(idx++); // holeshape
                                v = rules.GetVector3Item(idx++); //cut
                                hollow = (float)rules.GetLSLFloatItem(idx++);
                                twist = rules.GetVector3Item(idx++);
                                taper_b = rules.GetVector3Item(idx++);
                                topshear = rules.GetVector3Item(idx++);
                                prim.Prim.PrimData.PathCurve = PathCurve.Line;
                                SetPrimitiveShapeParams(prim, face, v, hollow, twist, taper_b, topshear);
                                break;
                            case ScriptTypes.PRIM_TYPE_SPHERE:
                                if (remain < 5) return;

                                face = (int)rules.GetLSLIntegerItem(idx++); // holeshape
                                v = rules.GetVector3Item(idx++); // cut
                                hollow = (float)rules.GetLSLFloatItem(idx++);
                                twist = rules.GetVector3Item(idx++);
                                taper_b = rules.GetVector3Item(idx++); // dimple
                                prim.Prim.PrimData.PathCurve = PathCurve.Circle;
                                SetPrimitiveShapeParams(prim, face, v, hollow, twist, taper_b);
                                break;
                            case ScriptTypes.PRIM_TYPE_TORUS:
                                if (remain < 11) return;

                                face = (int)rules.GetLSLIntegerItem(idx++); // holeshape
                                v = rules.GetVector3Item(idx++); //cut
                                hollow = (float)rules.GetLSLFloatItem(idx++);
                                twist = rules.GetVector3Item(idx++);
                                holesize = rules.GetVector3Item(idx++);
                                topshear = rules.GetVector3Item(idx++);
                                profilecut = rules.GetVector3Item(idx++);
                                taper_b = rules.GetVector3Item(idx++); // taper_a
                                revolutions = (float)rules.GetLSLFloatItem(idx++);
                                radiusoffset = (float)rules.GetLSLFloatItem(idx++);
                                skew = (float)rules.GetLSLFloatItem(idx++);
                                prim.Prim.PrimData.PathCurve = PathCurve.Circle;
                                SetPrimitiveShapeParams(prim, face, v, hollow, twist, holesize, topshear, profilecut, taper_b, revolutions, radiusoffset, skew);
                                break;
                            case ScriptTypes.PRIM_TYPE_TUBE:
                                if (remain < 11) return;

                                face = (int)rules.GetLSLIntegerItem(idx++); // holeshape
                                v = rules.GetVector3Item(idx++); //cut
                                hollow = (float)rules.GetLSLFloatItem(idx++);
                                twist = rules.GetVector3Item(idx++);
                                holesize = rules.GetVector3Item(idx++);
                                topshear = rules.GetVector3Item(idx++);
                                profilecut = rules.GetVector3Item(idx++);
                                taper_b = rules.GetVector3Item(idx++); // taper_a
                                revolutions = (float)rules.GetLSLFloatItem(idx++);
                                radiusoffset = (float)rules.GetLSLFloatItem(idx++);
                                skew = (float)rules.GetLSLFloatItem(idx++);
                                prim.Prim.PrimData.PathCurve = PathCurve.Circle;
                                SetPrimitiveShapeParams(prim, face, v, hollow, twist, holesize, topshear, profilecut, taper_b, revolutions, radiusoffset, skew);
                                break;
                            case ScriptTypes.PRIM_TYPE_RING:
                                if (remain < 11) return;

                                face = (int)rules.GetLSLIntegerItem(idx++); // holeshape
                                v = rules.GetVector3Item(idx++); //cut
                                hollow = (float)rules.GetLSLFloatItem(idx++);
                                twist = rules.GetVector3Item(idx++);
                                holesize = rules.GetVector3Item(idx++);
                                topshear = rules.GetVector3Item(idx++);
                                profilecut = rules.GetVector3Item(idx++);
                                taper_b = rules.GetVector3Item(idx++); // taper_a
                                revolutions = (float)rules.GetLSLFloatItem(idx++);
                                radiusoffset = (float)rules.GetLSLFloatItem(idx++);
                                skew = (float)rules.GetLSLFloatItem(idx++);
                                prim.Prim.PrimData.PathCurve = PathCurve.Circle;
                                SetPrimitiveShapeParams(prim, face, v, hollow, twist, holesize, topshear, profilecut, taper_b, revolutions, radiusoffset, skew);
                                break;
                            case ScriptTypes.PRIM_TYPE_SCULPT:
                                if (remain < 2) return;

                                string map = rules.Data[idx++].ToString();
                                face = (int)rules.GetLSLIntegerItem(idx++); // type
                                prim.Prim.PrimData.PathCurve = PathCurve.Circle;
                                SetPrimitiveShapeParams(prim, map, face);
                                break;
                        }

                        break;
                    case ScriptTypes.PRIM_TEXTURE:
                        if (remain < 5) return;

                        face = (int)rules.GetLSLIntegerItem(idx++);
                        string tex = rules.Data[idx++].ToString();
                        LSL_Vector repeats = rules.GetVector3Item(idx++);
                        LSL_Vector offsets = rules.GetVector3Item(idx++);
                        double rotation = (double)rules.GetLSLFloatItem(idx++);

                        SetTexture(prim, tex, face);
                        ScaleTexture(prim, repeats.x, repeats.y, face);
                        OffsetTexture(prim, offsets.x, offsets.y, face);
                        RotateTexture(prim, rotation, face);
                        break;
                    case ScriptTypes.PRIM_COLOR:
                        if (remain < 3) return;

                        face = (int)rules.GetLSLIntegerItem(idx++);
                        LSL_Vector color = rules.GetVector3Item(idx++);
                        double alpha = (double)rules.GetLSLFloatItem(idx++);

                        SetColor(prim, color, face);
                        SetAlpha(prim, alpha, face);
                        break;
                    case ScriptTypes.PRIM_FLEXIBLE:
                        if (remain < 7) return;

                        bool flexi = rules.GetLSLIntegerItem(idx++);
                        int softness = rules.GetLSLIntegerItem(idx++);
                        float gravity = (float)rules.GetLSLFloatItem(idx++);
                        float friction = (float)rules.GetLSLFloatItem(idx++);
                        float wind = (float)rules.GetLSLFloatItem(idx++);
                        float tension = (float)rules.GetLSLFloatItem(idx++);
                        LSL_Vector force = rules.GetVector3Item(idx++);

                        SetFlexi(prim, flexi, softness, gravity, friction, wind, tension, force);
                        break;
                    case ScriptTypes.PRIM_POINT_LIGHT:
                        if (remain < 5) return;

                        bool light = rules.GetLSLIntegerItem(idx++);
                        LSL_Vector lightcolor = rules.GetVector3Item(idx++);
                        float intensity = (float)rules.GetLSLFloatItem(idx++);
                        float radius = (float)rules.GetLSLFloatItem(idx++);
                        float falloff = (float)rules.GetLSLFloatItem(idx++);

                        SetPointLight(prim, light, lightcolor, intensity, radius, falloff);
                        break;
                    case ScriptTypes.PRIM_GLOW:
                        if (remain < 2)
                            return;
                        face = rules.GetLSLIntegerItem(idx++);
                        float glow = (float)rules.GetLSLFloatItem(idx++);

                        SetGlow(prim, face, glow);
                        break;
                    case ScriptTypes.PRIM_BUMP_SHINY:
                        if (remain < 3)
                            return;
                        face = (int)rules.GetLSLIntegerItem(idx++);
                        int shiny = (int)rules.GetLSLIntegerItem(idx++);
                        Bumpiness bump = (Bumpiness)Convert.ToByte((int)rules.GetLSLIntegerItem(idx++));

                        SetShiny(prim, face, shiny, bump);
                        break;
                    case ScriptTypes.PRIM_FULLBRIGHT:
                        if (remain < 2) return;

                        face = rules.GetLSLIntegerItem(idx++);
                        bool st = rules.GetLSLIntegerItem(idx++);

                        SetFullBright(prim, face, st);
                        break;
                    case ScriptTypes.PRIM_MATERIAL:
                        if (remain < 1) return;

                        int mat = rules.GetLSLIntegerItem(idx++);
                        if (mat < 0 || mat > 7) return;

                        prim.Prim.PrimData.Material = (Material)mat;
                        break;
                    case ScriptTypes.PRIM_PHANTOM:
                        if (remain < 1) return;

                        string ph = rules.Data[idx++].ToString();

                        if (ph.Equals("1"))
                            server.Scene.ObjectFlags(this, parent, parent.Prim.Flags | PrimFlags.Phantom);
                        else
                            server.Scene.ObjectFlags(this, parent, parent.Prim.Flags & ~PrimFlags.Phantom);

                        break;
                    case ScriptTypes.PRIM_PHYSICS:
                        if (remain < 1) return;

                        string phy = rules.Data[idx++].ToString();

                        if (phy.Equals("1"))
                            server.Scene.ObjectFlags(this, parent, parent.Prim.Flags & ~PrimFlags.Phantom);
                        else
                            server.Scene.ObjectFlags(this, parent, parent.Prim.Flags & ~PrimFlags.Physics);

                        break;
                    case ScriptTypes.PRIM_TEMP_ON_REZ:
                        if (remain < 1) return;

                        string temp = rules.Data[idx++].ToString();

                        if (temp.Equals("1"))
                            server.Scene.ObjectFlags(this, parent, parent.Prim.Flags & ~PrimFlags.TemporaryOnRez);
                        else
                            server.Scene.ObjectFlags(this, parent, parent.Prim.Flags & ~PrimFlags.TemporaryOnRez);

                        break;
                }
            }
        }

        private void SetScale(SimulationObject part, LSL_Vector scale)
        {
            if (part == null)
                return;

            if (scale.x < 0.01 || scale.y < 0.01 || scale.z < 0.01)
                return;

            part.Prim.Scale = new Vector3((float)scale.x, (float)scale.y, (float)scale.z);
            server.Scene.ObjectAdd(this, part, hostObject.Prim.Properties.OwnerID, 0, PrimFlags.None);
        }

        private void SetColor(SimulationObject part, LSL_Vector color, int face)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;
            Color4 texcolor;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                texcolor = tex.CreateFace((uint)face).RGBA;
                texcolor.R = Utils.Clamp((float)color.x, 0.0f, 1.0f);
                texcolor.G = Utils.Clamp((float)color.y, 0.0f, 1.0f);
                texcolor.B = Utils.Clamp((float)color.z, 0.0f, 1.0f);
                tex.FaceTextures[face].RGBA = texcolor;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
            else if (face == ScriptTypes.ALL_SIDES)
            {
                for (uint i = 0; i < GetNumberOfSides(part); i++)
                {
                    if (tex.FaceTextures[i] != null)
                    {
                        texcolor = tex.FaceTextures[i].RGBA;
                        texcolor.R = Utils.Clamp((float)color.x, 0.0f, 1.0f);
                        texcolor.G = Utils.Clamp((float)color.y, 0.0f, 1.0f);
                        texcolor.B = Utils.Clamp((float)color.z, 0.0f, 1.0f);
                        tex.FaceTextures[i].RGBA = texcolor;
                    }
                    texcolor = tex.DefaultTexture.RGBA;
                    texcolor.R = Utils.Clamp((float)color.x, 0.0f, 1.0f);
                    texcolor.G = Utils.Clamp((float)color.y, 0.0f, 1.0f);
                    texcolor.B = Utils.Clamp((float)color.z, 0.0f, 1.0f);
                    tex.DefaultTexture.RGBA = texcolor;
                }

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
        }

        public void SetGlow(SimulationObject part, int face, float glow)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                tex.CreateFace((uint)face);
                tex.FaceTextures[face].Glow = glow;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
            else if (face == ScriptTypes.ALL_SIDES)
            {
                for (uint i = 0; i < GetNumberOfSides(part); i++)
                {
                    if (tex.FaceTextures[i] != null)
                        tex.FaceTextures[i].Glow = glow;

                    tex.DefaultTexture.Glow = glow;
                }

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
        }

        public void SetShiny(SimulationObject part, int face, int shiny, Bumpiness bump)
        {

            Shininess sval = new Shininess();

            switch (shiny)
            {
                case 0:
                    sval = Shininess.None;
                    break;
                case 1:
                    sval = Shininess.Low;
                    break;
                case 2:
                    sval = Shininess.Medium;
                    break;
                case 3:
                    sval = Shininess.High;
                    break;
                default:
                    sval = Shininess.None;
                    break;
            }

            Primitive.TextureEntry tex = part.Prim.Textures;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                tex.CreateFace((uint)face);
                tex.FaceTextures[face].Shiny = sval;
                tex.FaceTextures[face].Bump = bump;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
            else if (face == ScriptTypes.ALL_SIDES)
            {
                for (uint i = 0; i < GetNumberOfSides(part); i++)
                {
                    if (tex.FaceTextures[i] != null)
                    {
                        tex.FaceTextures[i].Shiny = sval;
                        tex.FaceTextures[i].Bump = bump; ;
                    }
                    tex.DefaultTexture.Shiny = sval;
                    tex.DefaultTexture.Bump = bump;
                }

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
        }

        public void SetFullBright(SimulationObject part, int face, bool bright)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                tex.CreateFace((uint)face);
                tex.FaceTextures[face].Fullbright = bright;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
            else if (face == ScriptTypes.ALL_SIDES)
            {
                for (uint i = 0; i < GetNumberOfSides(part); i++)
                {
                    if (tex.FaceTextures[i] != null)
                    {
                        tex.FaceTextures[i].Fullbright = bright;
                    }
                }

                tex.DefaultTexture.Fullbright = bright;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
        }

        private void SetTexture(SimulationObject part, string texture, int face)
        {
            UUID textureID = UUID.Zero;

            if (!UUID.TryParse(texture, out textureID))
                textureID = InventoryKey(texture, AssetType.Texture).AssetID;

            if (textureID == UUID.Zero)
                return;

            Primitive.TextureEntry tex = part.Prim.Textures;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                Primitive.TextureEntryFace texface = tex.CreateFace((uint)face);
                texface.TextureID = textureID;
                tex.FaceTextures[face] = texface;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
            else if (face == ScriptTypes.ALL_SIDES)
            {
                for (uint i = 0; i < GetNumberOfSides(part); i++)
                {
                    if (tex.FaceTextures[i] != null)
                    {
                        tex.FaceTextures[i].TextureID = textureID;
                    }
                }

                tex.DefaultTexture.TextureID = textureID;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
        }

        private void ScaleTexture(SimulationObject part, double u, double v, int face)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                Primitive.TextureEntryFace texface = tex.CreateFace((uint)face);
                texface.RepeatU = (float)u;
                texface.RepeatV = (float)v;
                tex.FaceTextures[face] = texface;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
            if (face == ScriptTypes.ALL_SIDES)
            {
                for (int i = 0; i < GetNumberOfSides(part); i++)
                {
                    if (tex.FaceTextures[i] != null)
                    {
                        tex.FaceTextures[i].RepeatU = (float)u;
                        tex.FaceTextures[i].RepeatV = (float)v;
                    }
                }

                tex.DefaultTexture.RepeatU = (float)u;
                tex.DefaultTexture.RepeatV = (float)v;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
        }

        private void OffsetTexture(SimulationObject part, double u, double v, int face)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                Primitive.TextureEntryFace texface = tex.CreateFace((uint)face);
                texface.OffsetU = (float)u;
                texface.OffsetV = (float)v;
                tex.FaceTextures[face] = texface;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
            if (face == ScriptTypes.ALL_SIDES)
            {
                for (int i = 0; i < GetNumberOfSides(part); i++)
                {
                    if (tex.FaceTextures[i] != null)
                    {
                        tex.FaceTextures[i].OffsetU = (float)u;
                        tex.FaceTextures[i].OffsetV = (float)v;
                    }
                }

                tex.DefaultTexture.OffsetU = (float)u;
                tex.DefaultTexture.OffsetV = (float)v;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
        }

        private void RotateTexture(SimulationObject part, double rotation, int face)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                Primitive.TextureEntryFace texface = tex.CreateFace((uint)face);
                texface.Rotation = (float)rotation;
                tex.FaceTextures[face] = texface;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
            if (face == ScriptTypes.ALL_SIDES)
            {
                for (int i = 0; i < GetNumberOfSides(part); i++)
                {
                    if (tex.FaceTextures[i] != null)
                    {
                        tex.FaceTextures[i].Rotation = (float)rotation;
                    }
                }

                tex.DefaultTexture.Rotation = (float)rotation;

                server.Scene.ObjectModifyTextures(this, part, part.Prim.MediaURL, tex);
            }
        }

        private void SetPos(SimulationObject part, LSL_Vector targetPos)
        {
            // Capped movemment if distance > 10m (http://wiki.secondlife.com/wiki/LlSetPos)
            LSL_Vector currentPos = llGetLocalPos();
            if (llVecDist(currentPos, targetPos) > 10.0f * SCRIPT_DISTANCE_FACTOR)
                targetPos = currentPos + SCRIPT_DISTANCE_FACTOR * 10.0f * llVecNorm(targetPos - currentPos);

            Vector3 newPos = new Vector3((float)targetPos.x, (float)targetPos.y, (float)targetPos.z);

            server.Scene.ObjectTransform(this, part, newPos, part.Prim.Rotation, part.Prim.Velocity, part.Prim.Acceleration,
                part.Prim.AngularVelocity);
        }

        private void SetRot(SimulationObject part, Quaternion rot)
        {
            server.Scene.ObjectTransform(this, part, part.Prim.Position, rot, part.Prim.Velocity, part.Prim.Acceleration,
                part.Prim.AngularVelocity);
        }

        private LSL_Vector GetTextureOffset(SimulationObject part, int face)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;
            LSL_Vector offset = LSL_Vector.Zero;

            if (face == ScriptTypes.ALL_SIDES)
                face = 0;

            if (face >= 0 && face < GetNumberOfSides(part))
            {
                offset.x = tex.GetFace((uint)face).OffsetU;
                offset.y = tex.GetFace((uint)face).OffsetV;
            }

            return offset;
        }

        private LSL_Float GetTextureRot(SimulationObject part, int face)
        {
            Primitive.TextureEntry tex = part.Prim.Textures;
            if (face == -1)
                face = 0;

            if (face >= 0 && face < GetNumberOfSides(part))
                return tex.GetFace((uint)face).Rotation;
            else
                return 0.0;
        }

        private Primitive.ParticleSystem GetNewParticleSystemWithSLDefaultValues()
        {
            Primitive.ParticleSystem ps = new Primitive.ParticleSystem();

            // TODO find out about the other defaults and add them here
            ps.PartStartColor = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            ps.PartEndColor = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            ps.PartStartScaleX = 1.0f;
            ps.PartStartScaleY = 1.0f;
            ps.PartEndScaleX = 1.0f;
            ps.PartEndScaleY = 1.0f;
            ps.BurstSpeedMin = 1.0f;
            ps.BurstSpeedMax = 1.0f;
            ps.BurstRate = 0.1f;
            ps.PartMaxAge = 10.0f;
            return ps;
        }

        #endregion Helpers
    }
}
