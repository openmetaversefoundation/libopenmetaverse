/*
 * Copyright (c) 2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
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
 * 
 * 
 * This code comes from the OpenSim project. Meshmerizer is written by dahlia
 * <dahliatrimble@gmail.com>
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Globalization;

namespace OpenMetaverse.Rendering
{
    public enum ProfileShape : byte
    {
        Circle = 0,
        Square = 1,
        IsometricTriangle = 2,
        EquilateralTriangle = 3,
        RightTriangle = 4,
        HalfCircle = 5
    }

    public enum HollowShape : byte
    {
        Same = 0,
        Circle = 16,
        Square = 32,
        Triangle = 48
    }

    public enum PCodeEnum : byte
    {
        Primitive = 9,
        Avatar = 47,
        Grass = 95,
        NewTree = 111,
        ParticleSystem = 143,
        Tree = 255
    }

    public enum Extrusion : byte
    {
        Straight = 16,
        Curve1 = 32,
        Curve2 = 48,
        Flexible = 128
    }

    public class PhysicsVector
    {
        public float X;
        public float Y;
        public float Z;

        public PhysicsVector()
        {
        }

        public PhysicsVector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void setValues(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public static readonly PhysicsVector Zero = new PhysicsVector(0f, 0f, 0f);

        public override string ToString()
        {
            return "<" + X + "," + Y + "," + Z + ">";
        }

        /// <summary>
        /// These routines are the easiest way to store XYZ values in an Vector3 without requiring 3 calls.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] byteArray = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(X), 0, byteArray, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Y), 0, byteArray, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(Z), 0, byteArray, 8, 4);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteArray, 0, 4);
                Array.Reverse(byteArray, 4, 4);
                Array.Reverse(byteArray, 8, 4);
            }

            return byteArray;
        }

        public void FromBytes(byte[] byteArray, int pos)
        {
            byte[] conversionBuffer = null;
            if (!BitConverter.IsLittleEndian)
            {
                // Big endian architecture
                if (conversionBuffer == null)
                    conversionBuffer = new byte[12];

                Buffer.BlockCopy(byteArray, pos, conversionBuffer, 0, 12);

                Array.Reverse(conversionBuffer, 0, 4);
                Array.Reverse(conversionBuffer, 4, 4);
                Array.Reverse(conversionBuffer, 8, 4);

                X = BitConverter.ToSingle(conversionBuffer, 0);
                Y = BitConverter.ToSingle(conversionBuffer, 4);
                Z = BitConverter.ToSingle(conversionBuffer, 8);
            }
            else
            {
                // Little endian architecture
                X = BitConverter.ToSingle(byteArray, pos);
                Y = BitConverter.ToSingle(byteArray, pos + 4);
                Z = BitConverter.ToSingle(byteArray, pos + 8);
            }
        }

        // Operations
        public static PhysicsVector operator +(PhysicsVector a, PhysicsVector b)
        {
            return new PhysicsVector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static PhysicsVector operator -(PhysicsVector a, PhysicsVector b)
        {
            return new PhysicsVector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static PhysicsVector cross(PhysicsVector a, PhysicsVector b)
        {
            return new PhysicsVector(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        public float length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public static float GetDistanceTo(PhysicsVector a, PhysicsVector b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static PhysicsVector operator /(PhysicsVector v, float f)
        {
            return new PhysicsVector(v.X / f, v.Y / f, v.Z / f);
        }

        public static PhysicsVector operator *(PhysicsVector v, float f)
        {
            return new PhysicsVector(v.X * f, v.Y * f, v.Z * f);
        }

        public static PhysicsVector operator *(float f, PhysicsVector v)
        {
            return v * f;
        }

        public virtual bool IsIdentical(PhysicsVector v, float tolerance)
        {
            PhysicsVector diff = this - v;
            float d = diff.length();
            if (d <= tolerance)
                return true;

            return false;
        }
    }

    public class MeshmerizerVertex : PhysicsVector, IComparable<MeshmerizerVertex>
    {
        public MeshmerizerVertex(float x, float y, float z)
            : base(x, y, z)
        {
        }

        public MeshmerizerVertex normalize()
        {
            float tlength = length();
            if (tlength != 0)
            {
                float mul = 1.0f / tlength;
                return new MeshmerizerVertex(X * mul, Y * mul, Z * mul);
            }
            else
            {
                return new MeshmerizerVertex(0, 0, 0);
            }
        }

        public MeshmerizerVertex cross(MeshmerizerVertex v)
        {
            return new MeshmerizerVertex(Y * v.Z - Z * v.Y, Z * v.X - X * v.Z, X * v.Y - Y * v.X);
        }

        // disable warning: mono compiler moans about overloading
        // operators hiding base operator but should not according to C#
        // language spec
#pragma warning disable 0108
        public static MeshmerizerVertex operator *(MeshmerizerVertex v, Quaternion q)
        {
            // From http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/transforms/

            MeshmerizerVertex v2 = new MeshmerizerVertex(0f, 0f, 0f);

            v2.X = q.W * q.W * v.X +
                2f * q.Y * q.W * v.Z -
                2f * q.Z * q.W * v.Y +
                     q.X * q.X * v.X +
                2f * q.Y * q.X * v.Y +
                2f * q.Z * q.X * v.Z -
                     q.Z * q.Z * v.X -
                     q.Y * q.Y * v.X;

            v2.Y =
                2f * q.X * q.Y * v.X +
                     q.Y * q.Y * v.Y +
                2f * q.Z * q.Y * v.Z +
                2f * q.W * q.Z * v.X -
                     q.Z * q.Z * v.Y +
                     q.W * q.W * v.Y -
                2f * q.X * q.W * v.Z -
                     q.X * q.X * v.Y;

            v2.Z =
                2f * q.X * q.Z * v.X +
                2f * q.Y * q.Z * v.Y +
                     q.Z * q.Z * v.Z -
                2f * q.W * q.Y * v.X -
                     q.Y * q.Y * v.Z +
                2f * q.W * q.X * v.Y -
                     q.X * q.X * v.Z +
                     q.W * q.W * v.Z;

            return v2;
        }

        public static MeshmerizerVertex operator +(MeshmerizerVertex v1, MeshmerizerVertex v2)
        {
            return new MeshmerizerVertex(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static MeshmerizerVertex operator -(MeshmerizerVertex v1, MeshmerizerVertex v2)
        {
            return new MeshmerizerVertex(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static MeshmerizerVertex operator *(MeshmerizerVertex v1, MeshmerizerVertex v2)
        {
            return new MeshmerizerVertex(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }

        public static MeshmerizerVertex operator +(MeshmerizerVertex v1, float am)
        {
            v1.X += am;
            v1.Y += am;
            v1.Z += am;
            return v1;
        }

        public static MeshmerizerVertex operator -(MeshmerizerVertex v1, float am)
        {
            v1.X -= am;
            v1.Y -= am;
            v1.Z -= am;
            return v1;
        }

        public static MeshmerizerVertex operator *(MeshmerizerVertex v1, float am)
        {
            v1.X *= am;
            v1.Y *= am;
            v1.Z *= am;
            return v1;
        }

        public static MeshmerizerVertex operator /(MeshmerizerVertex v1, float am)
        {
            if (am == 0f)
            {
                return new MeshmerizerVertex(0f, 0f, 0f);
            }
            float mul = 1.0f / am;
            v1.X *= mul;
            v1.Y *= mul;
            v1.Z *= mul;
            return v1;
        }
#pragma warning restore 0108


        public float dot(MeshmerizerVertex v)
        {
            return X * v.X + Y * v.Y + Z * v.Z;
        }

        public MeshmerizerVertex(PhysicsVector v)
            : base(v.X, v.Y, v.Z)
        {
        }

        public MeshmerizerVertex Clone()
        {
            return new MeshmerizerVertex(X, Y, Z);
        }

        public static MeshmerizerVertex FromAngle(double angle)
        {
            return new MeshmerizerVertex((float)Math.Cos(angle), (float)Math.Sin(angle), 0.0f);
        }


        public virtual bool Equals(MeshmerizerVertex v, float tolerance)
        {
            PhysicsVector diff = this - v;
            float d = diff.length();
            if (d < tolerance)
                return true;

            return false;
        }


        public int CompareTo(MeshmerizerVertex other)
        {
            if (X < other.X)
                return -1;

            if (X > other.X)
                return 1;

            if (Y < other.Y)
                return -1;

            if (Y > other.Y)
                return 1;

            if (Z < other.Z)
                return -1;

            if (Z > other.Z)
                return 1;

            return 0;
        }

        public static bool operator >(MeshmerizerVertex me, MeshmerizerVertex other)
        {
            return me.CompareTo(other) > 0;
        }

        public static bool operator <(MeshmerizerVertex me, MeshmerizerVertex other)
        {
            return me.CompareTo(other) < 0;
        }

        public String ToRaw()
        {
            // Why this stuff with the number formatter?
            // Well, the raw format uses the english/US notation of numbers
            // where the "," separates groups of 1000 while the "." marks the border between 1 and 10E-1.
            // The german notation uses these characters exactly vice versa!
            // The Float.ToString() routine is a localized one, giving different results depending on the country
            // settings your machine works with. Unusable for a machine readable file format :-(
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberDecimalDigits = 3;

            String s1 = X.ToString("N2", nfi) + " " + Y.ToString("N2", nfi) + " " + Z.ToString("N2", nfi);

            return s1;
        }
    }

    public class Triangle
    {
        public MeshmerizerVertex v1;
        public MeshmerizerVertex v2;
        public MeshmerizerVertex v3;

        private float radius_square;
        private float cx;
        private float cy;

        public Triangle(MeshmerizerVertex _v1, MeshmerizerVertex _v2, MeshmerizerVertex _v3)
        {
            v1 = _v1;
            v2 = _v2;
            v3 = _v3;

            CalcCircle();
        }

        public bool isInCircle(float x, float y)
        {
            float dx, dy;
            float dd;

            dx = x - cx;
            dy = y - cy;

            dd = dx * dx + dy * dy;
            if (dd < radius_square)
                return true;
            else
                return false;
        }

        public bool isDegraded()
        {
            // This means, the vertices of this triangle are somewhat strange.
            // They either line up or at least two of them are identical
            return (radius_square == 0.0);
        }

        private void CalcCircle()
        {
            // Calculate the center and the radius of a circle given by three points p1, p2, p3
            // It is assumed, that the triangles vertices are already set correctly
            double p1x, p2x, p1y, p2y, p3x, p3y;

            // Deviation of this routine:
            // A circle has the general equation (M-p)^2=r^2, where M and p are vectors
            // this gives us three equations f(p)=r^2, each for one point p1, p2, p3
            // putting respectively two equations together gives two equations
            // f(p1)=f(p2) and f(p1)=f(p3)
            // bringing all constant terms to one side brings them to the form
            // M*v1=c1 resp.M*v2=c2 where v1=(p1-p2) and v2=(p1-p3) (still vectors)
            // and c1, c2 are scalars (Naming conventions like the variables below)
            // Now using the equations that are formed by the components of the vectors
            // and isolate Mx lets you make one equation that only holds My
            // The rest is straight forward and eaasy :-)
            //

            /* helping variables for temporary results */
            double c1, c2;
            double v1x, v1y, v2x, v2y;

            double z, n;

            double rx, ry;

            // Readout the three points, the triangle consists of
            p1x = v1.X;
            p1y = v1.Y;

            p2x = v2.X;
            p2y = v2.Y;

            p3x = v3.X;
            p3y = v3.Y;

            /* calc helping values first */
            c1 = (p1x * p1x + p1y * p1y - p2x * p2x - p2y * p2y) / 2;
            c2 = (p1x * p1x + p1y * p1y - p3x * p3x - p3y * p3y) / 2;

            v1x = p1x - p2x;
            v1y = p1y - p2y;

            v2x = p1x - p3x;
            v2y = p1y - p3y;

            z = (c1 * v2x - c2 * v1x);
            n = (v1y * v2x - v2y * v1x);

            if (n == 0.0) // This is no triangle, i.e there are (at least) two points at the same location
            {
                radius_square = 0.0f;
                return;
            }

            cy = (float)(z / n);

            if (v2x != 0.0)
            {
                cx = (float)((c2 - v2y * cy) / v2x);
            }
            else if (v1x != 0.0)
            {
                cx = (float)((c1 - v1y * cy) / v1x);
            }
            else
            {
                throw new Exception("Malformed triangle");
            }

            rx = (p1x - cx);
            ry = (p1y - cy);

            radius_square = (float)(rx * rx + ry * ry);
        }

        public List<Simplex> GetSimplices()
        {
            List<Simplex> result = new List<Simplex>();
            Simplex s1 = new Simplex(v1, v2);
            Simplex s2 = new Simplex(v2, v3);
            Simplex s3 = new Simplex(v3, v1);

            result.Add(s1);
            result.Add(s2);
            result.Add(s3);

            return result;
        }

        public override String ToString()
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.CurrencyDecimalDigits = 2;
            nfi.CurrencyDecimalSeparator = ".";

            String s1 = "<" + v1.X.ToString(nfi) + "," + v1.Y.ToString(nfi) + "," + v1.Z.ToString(nfi) + ">";
            String s2 = "<" + v2.X.ToString(nfi) + "," + v2.Y.ToString(nfi) + "," + v2.Z.ToString(nfi) + ">";
            String s3 = "<" + v3.X.ToString(nfi) + "," + v3.Y.ToString(nfi) + "," + v3.Z.ToString(nfi) + ">";

            return s1 + ";" + s2 + ";" + s3;
        }

        public PhysicsVector getNormal()
        {
            // Vertices

            // Vectors for edges
            PhysicsVector e1;
            PhysicsVector e2;

            e1 = new PhysicsVector(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
            e2 = new PhysicsVector(v1.X - v3.X, v1.Y - v3.Y, v1.Z - v3.Z);

            // Cross product for normal
            PhysicsVector n = PhysicsVector.cross(e1, e2);

            // Length
            float l = n.length();

            // Normalized "normal"
            n = n / l;

            return n;
        }

        public void invertNormal()
        {
            MeshmerizerVertex vt;
            vt = v1;
            v1 = v2;
            v2 = vt;
        }

        // Dumps a triangle in the "raw faces" format, blender can import. This is for visualisation and
        // debugging purposes
        public String ToStringRaw()
        {
            String output = v1.ToRaw() + " " + v2.ToRaw() + " " + v3.ToRaw();
            return output;
        }
    }

    public struct Coord
    {
        public float X;
        public float Y;
        public float Z;

        public Coord(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return this.X.ToString() + " " + this.Y.ToString() + " " + this.Z.ToString();
        }
    }

    public struct MeshmerizerFace
    {
        public int v1;
        public int v2;
        public int v3;

        public MeshmerizerFace(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    internal struct Angle
    {
        internal float angle;
        internal float X;
        internal float Y;

        internal Angle(float angle, float x, float y)
        {
            this.angle = angle;
            this.X = x;
            this.Y = y;
        }
    }

    class AngleList
    {
        private float iX, iY; // intersection point

        private Angle[] angles3 =
        {
            new Angle(0.0f, 1.0f, 0.0f),
            new Angle(0.33333333333333333f, -0.5f, 0.86602540378443871f),
            new Angle(0.66666666666666667f, -0.5f, -0.86602540378443837f),
            new Angle(1.0f, 1.0f, 0.0f)
        };

        private Angle[] angles4 =
        {
            new Angle(0.0f, 1.0f, 0.0f),
            new Angle(0.25f, 0.0f, 1.0f),
            new Angle(0.5f, -1.0f, 0.0f),
            new Angle(0.75f, 0.0f, -1.0f),
            new Angle(1.0f, 1.0f, 0.0f)
        };

        private Angle[] angles24 =
        {
            new Angle(0.0f, 1.0f, 0.0f),
            new Angle(0.041666666666666664f, 0.96592582628906831f, 0.25881904510252074f),
            new Angle(0.083333333333333329f, 0.86602540378443871f, 0.5f),
            new Angle(0.125f, 0.70710678118654757f, 0.70710678118654746f),
            new Angle(0.16666666666666667f, 0.5f, 0.8660254037844386f),
            new Angle(0.20833333333333331f, 0.25881904510252096f, 0.9659258262890682f),
            new Angle(0.25f, 0.0f, 1.0f),
            new Angle(0.29166666666666663f, -0.25881904510252063f, 0.96592582628906831f),
            new Angle(0.33333333333333333f, -0.5f, 0.86602540378443871f),
            new Angle(0.375f, -0.70710678118654746f, 0.70710678118654757f),
            new Angle(0.41666666666666663f, -0.86602540378443849f, 0.5f),
            new Angle(0.45833333333333331f, -0.9659258262890682f, 0.25881904510252102f),
            new Angle(0.5f, -1.0f, 0.0f),
            new Angle(0.54166666666666663f, -0.96592582628906842f, -0.25881904510252035f),
            new Angle(0.58333333333333326f, -0.86602540378443882f, -0.5f),
            new Angle(0.62499999999999989f, -0.70710678118654791f, -0.70710678118654713f),
            new Angle(0.66666666666666667f, -0.5f, -0.86602540378443837f),
            new Angle(0.70833333333333326f, -0.25881904510252152f, -0.96592582628906809f),
            new Angle(0.75f, 0.0f, -1.0f),
            new Angle(0.79166666666666663f, 0.2588190451025203f, -0.96592582628906842f),
            new Angle(0.83333333333333326f, 0.5f, -0.86602540378443904f),
            new Angle(0.875f, 0.70710678118654735f, -0.70710678118654768f),
            new Angle(0.91666666666666663f, 0.86602540378443837f, -0.5f),
            new Angle(0.95833333333333326f, 0.96592582628906809f, -0.25881904510252157f),
            new Angle(1.0f, 1.0f, 0.0f)
        };

        private Angle interpolatePoints(float newPoint, Angle p1, Angle p2)
        {
            float m = (newPoint - p1.angle) / (p2.angle - p1.angle);
            return new Angle(newPoint, p1.X + m * (p2.X - p1.X), p1.Y + m * (p2.Y - p1.Y));
        }

        private void intersection(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        { // ref: http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/
            double denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            double uaNumerator = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);

            if (denom != 0.0)
            {
                double ua = uaNumerator / denom;
                iX = (float)(x1 + ua * (x2 - x1));
                iY = (float)(y1 + ua * (y2 - y1));
            }
        }

        internal List<Angle> angles;

        internal void makeAngles(int sides, float startAngle, float stopAngle)
        {
            angles = new List<Angle>();
            double twoPi = System.Math.PI * 2.0;
            float twoPiInv = 1.0f / (float)twoPi;

            if (sides < 1)
                throw new Exception("number of sides not greater than zero");
            if (stopAngle <= startAngle)
                throw new Exception("stopAngle not greater than startAngle");

            if ((sides == 3 || sides == 4 || sides == 24))
            {
                startAngle *= twoPiInv;
                stopAngle *= twoPiInv;

                Angle[] sourceAngles;
                if (sides == 3)
                    sourceAngles = angles3;
                else if (sides == 4)
                    sourceAngles = angles4;
                else sourceAngles = angles24;

                int startAngleIndex = (int)(startAngle * sides);
                int endAngleIndex = sourceAngles.Length - 1;
                if (stopAngle < 1.0f)
                    endAngleIndex = (int)(stopAngle * sides) + 1;
                if (endAngleIndex == startAngleIndex)
                    endAngleIndex++;

                for (int angleIndex = startAngleIndex; angleIndex < endAngleIndex + 1; angleIndex++)
                    angles.Add(sourceAngles[angleIndex]);

                if (startAngle > 0.0f)
                    angles[0] = interpolatePoints(startAngle, angles[0], angles[1]);

                if (stopAngle < 1.0f)
                {
                    int lastAngleIndex = angles.Count - 1;
                    angles[lastAngleIndex] = interpolatePoints(stopAngle, angles[lastAngleIndex - 1], angles[lastAngleIndex]);
                }
            }
            else
            {
                double stepSize = twoPi / sides;

                int startStep = (int)(startAngle / stepSize);
                double angle = stepSize * startStep;
                int step = startStep;
                double stopAngleTest = stopAngle;
                if (stopAngle < twoPi)
                {
                    stopAngleTest = stepSize * ((int)(stopAngle / stepSize) + 1);
                    if (stopAngleTest < stopAngle)
                        stopAngleTest += stepSize;
                    if (stopAngleTest > twoPi)
                        stopAngleTest = twoPi;
                }

                while (angle <= stopAngleTest)
                {
                    Angle newAngle;
                    newAngle.angle = (float)angle;
                    newAngle.X = (float)System.Math.Cos(angle);
                    newAngle.Y = (float)System.Math.Sin(angle);
                    angles.Add(newAngle);
                    step += 1;
                    angle = stepSize * step;
                }

                if (startAngle > angles[0].angle)
                {
                    Angle newAngle;
                    intersection(angles[0].X, angles[0].Y, angles[1].X, angles[1].Y, 0.0f, 0.0f, (float)Math.Cos(startAngle), (float)Math.Sin(startAngle));
                    newAngle.angle = startAngle;
                    newAngle.X = iX;
                    newAngle.Y = iY;
                    angles[0] = newAngle;
                }

                int index = angles.Count - 1;
                if (stopAngle < angles[index].angle)
                {
                    Angle newAngle;
                    intersection(angles[index - 1].X, angles[index - 1].Y, angles[index].X, angles[index].Y, 0.0f, 0.0f, (float)Math.Cos(stopAngle), (float)Math.Sin(stopAngle));
                    newAngle.angle = stopAngle;
                    newAngle.X = iX;
                    newAngle.Y = iY;
                    angles[index] = newAngle;
                }
            }
        }
    }

    // A simplex is a section of a straight line.
    // It is defined by its endpoints, i.e. by two vertices
    // Operation on vertices are
    public class Simplex : IComparable<Simplex>
    {
        public MeshmerizerVertex v1;
        public MeshmerizerVertex v2;

        public Simplex(MeshmerizerVertex _v1, MeshmerizerVertex _v2)
        {
            v1 = _v1;
            v2 = _v2;
        }

        public int CompareTo(Simplex other)
        {
            MeshmerizerVertex lv1, lv2, ov1, ov2, temp;

            lv1 = v1;
            lv2 = v2;
            ov1 = other.v1;
            ov2 = other.v2;

            if (lv1 > lv2)
            {
                temp = lv1;
                lv1 = lv2;
                lv2 = temp;
            }

            if (ov1 > ov2)
            {
                temp = ov1;
                ov1 = ov2;
                ov2 = temp;
            }

            if (lv1 > ov1)
            {
                return 1;
            }
            if (lv1 < ov1)
            {
                return -1;
            }

            if (lv2 > ov2)
            {
                return 1;
            }
            if (lv2 < ov2)
            {
                return -1;
            }

            return 0;
        }

        private static void intersectParameter(PhysicsVector p1, PhysicsVector r1, PhysicsVector p2, PhysicsVector r2,
                                               ref float lambda, ref float mu)
        {
            // Intersects two straights
            // p1, p2, points on the straight
            // r1, r2, directional vectors of the straight. Not necessarily of length 1!
            // note, that l, m can be scaled such, that the range 0..1 is mapped to the area between two points,
            // thus allowing to decide whether an intersection is between two points

            float r1x = r1.X;
            float r1y = r1.Y;
            float r2x = r2.X;
            float r2y = r2.Y;

            float denom = r1y * r2x - r1x * r2y;

            float p1x = p1.X;
            float p1y = p1.Y;
            float p2x = p2.X;
            float p2y = p2.Y;

            float z1 = -p2x * r2y + p1x * r2y + (p2y - p1y) * r2x;
            float z2 = -p2x * r1y + p1x * r1y + (p2y - p1y) * r1x;

            if (denom == 0.0f) // Means the straights are parallel. Either no intersection or an infinite number of them
            {
                if (z1 == 0.0f)
                {
                    // Means they are identical -> many, many intersections
                    lambda = Single.NaN;
                    mu = Single.NaN;
                }
                else
                {
                    lambda = Single.PositiveInfinity;
                    mu = Single.PositiveInfinity;
                }
                return;
            }


            lambda = z1 / denom;
            mu = z2 / denom;
        }


        // Intersects the simplex with another one.
        // the borders are used to deal with float inaccuracies
        // As a rule of thumb, the borders are
        // lowerBorder1 : 0.0
        // lowerBorder2 : 0.0
        // upperBorder1 : 1.0
        // upperBorder2 : 1.0
        // Set these to values near the given parameters (e.g. 0.001 instead of 1 to exclude simplex starts safely, or to -0.001 to include them safely)
        public static PhysicsVector Intersect(
            Simplex s1,
            Simplex s2,
            float lowerBorder1,
            float lowerBorder2,
            float upperBorder1,
            float upperBorder2)
        {
            PhysicsVector firstSimplexDirection = s1.v2 - s1.v1;
            PhysicsVector secondSimplexDirection = s2.v2 - s2.v1;

            float lambda = 0.0f;
            float mu = 0.0f;

            // Give us the parameters of an intersection. This subroutine does *not* take the constraints
            // (intersection must be between v1 and v2 and it must be in the positive direction of the ray)
            // into account. We do that afterwards.
            intersectParameter(s1.v1, firstSimplexDirection, s2.v1, secondSimplexDirection, ref lambda, ref mu);

            if (Single.IsInfinity(lambda)) // Special case. No intersection at all. directions parallel.
                return null;

            if (Single.IsNaN(lambda)) // Special case. many, many intersections.
                return null;

            if (lambda > upperBorder1) // We're behind v2
                return null;

            if (lambda < lowerBorder1)
                return null;

            if (mu < lowerBorder2) // outside simplex 2
                return null;

            if (mu > upperBorder2) // outside simplex 2
                return null;

            return s1.v1 + lambda * firstSimplexDirection;
        }

        // Intersects the simplex with a ray. The ray is defined as all p=origin + lambda*direction
        // where lambda >= 0
        public PhysicsVector RayIntersect(MeshmerizerVertex origin, PhysicsVector direction, bool bEndsIncluded)
        {
            PhysicsVector simplexDirection = v2 - v1;

            float lambda = 0.0f;
            float mu = 0.0f;

            // Give us the parameters of an intersection. This subroutine does *not* take the constraints
            // (intersection must be between v1 and v2 and it must be in the positive direction of the ray)
            // into account. We do that afterwards.
            intersectParameter(v1, simplexDirection, origin, direction, ref lambda, ref mu);

            if (Single.IsInfinity(lambda)) // Special case. No intersection at all. directions parallel.
                return null;

            if (Single.IsNaN(lambda)) // Special case. many, many intersections.
                return null;

            if (mu < 0.0) // We're on the wrong side of the ray
                return null;

            if (lambda > 1.0) // We're behind v2
                return null;

            if (lambda == 1.0 && !bEndsIncluded)
                return null; // The end of the simplices are not included

            if (lambda < 0.0f) // we're before v1;
                return null;

            return v1 + lambda * simplexDirection;
        }
    }

    public class PrimitiveBaseShape
    {
        private byte[] m_textureEntry;

        private ushort _pathBegin;
        private byte _pathCurve;
        private ushort _pathEnd;
        private sbyte _pathRadiusOffset;
        private byte _pathRevolutions;
        private byte _pathScaleX;
        private byte _pathScaleY;
        private byte _pathShearX;
        private byte _pathShearY;
        private sbyte _pathSkew;
        private sbyte _pathTaperX;
        private sbyte _pathTaperY;
        private sbyte _pathTwist;
        private sbyte _pathTwistBegin;
        private byte _pCode;
        private ushort _profileBegin;
        private ushort _profileEnd;
        private ushort _profileHollow;
        private Vector3 _scale;
        private byte _state;
        private ProfileShape _profileShape;
        private HollowShape _hollowShape;

        // Sculpted
        private UUID _sculptTexture = UUID.Zero;
        private byte _sculptType = (byte)0;
        private byte[] _sculptData = new byte[0];

        // Flexi
        private int _flexiSoftness = 0;
        private float _flexiTension = 0f;
        private float _flexiDrag = 0f;
        private float _flexiGravity = 0f;
        private float _flexiWind = 0f;
        private float _flexiForceX = 0f;
        private float _flexiForceY = 0f;
        private float _flexiForceZ = 0f;

        //Bright n sparkly
        private float _lightColorR = 0f;
        private float _lightColorG = 0f;
        private float _lightColorB = 0f;
        private float _lightColorA = 1f;
        private float _lightRadius = 0f;
        private float _lightCutoff = 0f;
        private float _lightFalloff = 0f;
        private float _lightIntensity = 1f;
        private bool _flexiEntry = false;
        private bool _lightEntry = false;
        private bool _sculptEntry = false;

        public byte ProfileCurve
        {
            get { return (byte)((byte)HollowShape | (byte)ProfileShape); }

            set
            {
                // Handle hollow shape component
                byte hollowShapeByte = (byte)(value & 0xf0);

                if (!Enum.IsDefined(typeof(HollowShape), hollowShapeByte))
                {
                    this._hollowShape = HollowShape.Same;
                }
                else
                {
                    this._hollowShape = (HollowShape)hollowShapeByte;
                }

                // Handle profile shape component
                byte profileShapeByte = (byte)(value & 0xf);

                if (!Enum.IsDefined(typeof(ProfileShape), profileShapeByte))
                {
                    this._profileShape = ProfileShape.Square;
                }
                else
                {
                    this._profileShape = (ProfileShape)profileShapeByte;
                }
            }
        }

        public PrimitiveBaseShape(Primitive prim)
        {
            ExtraParams = new byte[1];

            _pathBegin = Primitive.PackBeginCut(prim.PrimData.PathBegin);
            _pathCurve = (byte)prim.PrimData.PathCurve;
            _pathEnd = Primitive.PackEndCut(prim.PrimData.PathEnd);
            _pathRadiusOffset = Primitive.PackPathTwist(prim.PrimData.PathRadiusOffset);
            _pathRevolutions = Primitive.PackPathRevolutions(prim.PrimData.PathRevolutions);
            _pathScaleX = Primitive.PackPathScale(prim.PrimData.PathScaleX);
            _pathScaleY = Primitive.PackPathScale(prim.PrimData.PathScaleY);
            _pathShearX = (byte)Primitive.PackPathShear(prim.PrimData.PathShearX);
            _pathShearY = (byte)Primitive.PackPathShear(prim.PrimData.PathShearY);
            _pathSkew = Primitive.PackPathTwist(prim.PrimData.PathSkew);
            _pathTaperX = Primitive.PackPathTaper(prim.PrimData.PathTaperX);
            _pathTaperY = Primitive.PackPathTaper(prim.PrimData.PathTaperY);
            _pathTwist = Primitive.PackPathTwist(prim.PrimData.PathTwist);
            _pathTwistBegin = Primitive.PackPathTwist(prim.PrimData.PathTwistBegin);
            _pCode = (byte)prim.PrimData.PCode;
            _profileBegin = Primitive.PackBeginCut(prim.PrimData.ProfileBegin);
            _profileEnd = Primitive.PackEndCut(prim.PrimData.ProfileEnd);
            _profileHollow = Primitive.PackProfileHollow(prim.PrimData.ProfileHollow);
            _scale = prim.Scale;
            _state = prim.PrimData.State;
            _profileShape = (ProfileShape)(byte)prim.PrimData.ProfileCurve;
            _hollowShape = (HollowShape)(byte)prim.PrimData.ProfileHole;
            //Textures = prim.Textures;
        }

        public Primitive.TextureEntry Textures
        {
            get
            {
                return new Primitive.TextureEntry(m_textureEntry, 0, m_textureEntry.Length);
            }

            set { m_textureEntry = value.ToBytes(); }
        }

        public byte[] TextureEntry
        {
            get { return m_textureEntry; }

            set
            {
                if (value == null)
                    m_textureEntry = new byte[1];
                else
                    m_textureEntry = value;
            }
        }

        /*
        public static PrimitiveBaseShape Default
        {
            get
            {
                PrimitiveBaseShape boxShape = CreateBox();

                boxShape.SetScale(0.5f);

                return boxShape;
            }
        }

        
        public static PrimitiveBaseShape Create()
        {
            PrimitiveBaseShape shape = new PrimitiveBaseShape();
            return shape;
        }

        public static PrimitiveBaseShape CreateBox()
        {
            PrimitiveBaseShape shape = Create();

            shape._pathCurve = (byte)Extrusion.Straight;
            shape._profileShape = ProfileShape.Square;
            shape._pathScaleX = 100;
            shape._pathScaleY = 100;

            return shape;
        }

        public static PrimitiveBaseShape CreateCylinder()
        {
            PrimitiveBaseShape shape = Create();

            shape._pathCurve = (byte)Extrusion.Curve1;
            shape._profileShape = ProfileShape.Square;

            shape._pathScaleX = 100;
            shape._pathScaleY = 100;

            return shape;
        }
        
        public static PrimitiveBaseShape CreateCylinder(float radius, float heigth)
        {
            PrimitiveBaseShape shape = CreateCylinder();

            shape.SetHeigth(heigth);
            shape.SetRadius(radius);

            return shape;
        }
        */

        public void SetScale(float side)
        {
            _scale = new Vector3(side, side, side);
        }

        public void SetHeigth(float heigth)
        {
            _scale.Z = heigth;
        }

        public void SetRadius(float radius)
        {
            _scale.X = _scale.Y = radius * 2f;
        }

        // TODO: void returns need to change of course
        public virtual void GetMesh()
        {
        }

        public PrimitiveBaseShape Copy()
        {
            return (PrimitiveBaseShape)MemberwiseClone();
        }

        public void SetPathRange(Vector3 pathRange)
        {
            _pathBegin = Primitive.PackBeginCut(pathRange.X);
            _pathEnd = Primitive.PackEndCut(pathRange.Y);
        }

        public void SetSculptData(byte sculptType, UUID SculptTextureUUID)
        {
            _sculptType = sculptType;
            _sculptTexture = SculptTextureUUID;
        }

        public void SetProfileRange(Vector3 profileRange)
        {
            _profileBegin = Primitive.PackBeginCut(profileRange.X);
            _profileEnd = Primitive.PackEndCut(profileRange.Y);
        }

        public byte[] ExtraParams
        {
            get
            {
                return ExtraParamsToBytes();
            }
            set
            {
                ReadInExtraParamsBytes(value);
            }
        }

        public ushort PathBegin
        {
            get
            {
                return _pathBegin;
            }
            set
            {
                _pathBegin = value;
            }
        }

        public byte PathCurve
        {
            get
            {
                return _pathCurve;
            }
            set
            {
                _pathCurve = value;
            }
        }

        public ushort PathEnd
        {
            get
            {
                return _pathEnd;
            }
            set
            {
                _pathEnd = value;
            }
        }

        public sbyte PathRadiusOffset
        {
            get
            {
                return _pathRadiusOffset;
            }
            set
            {
                _pathRadiusOffset = value;
            }
        }

        public byte PathRevolutions
        {
            get
            {
                return _pathRevolutions;
            }
            set
            {
                _pathRevolutions = value;
            }
        }

        public byte PathScaleX
        {
            get
            {
                return _pathScaleX;
            }
            set
            {
                _pathScaleX = value;
            }
        }

        public byte PathScaleY
        {
            get
            {
                return _pathScaleY;
            }
            set
            {
                _pathScaleY = value;
            }
        }

        public byte PathShearX
        {
            get
            {
                return _pathShearX;
            }
            set
            {
                _pathShearX = value;
            }
        }

        public byte PathShearY
        {
            get
            {
                return _pathShearY;
            }
            set
            {
                _pathShearY = value;
            }
        }

        public sbyte PathSkew
        {
            get
            {
                return _pathSkew;
            }
            set
            {
                _pathSkew = value;
            }
        }

        public sbyte PathTaperX
        {
            get
            {
                return _pathTaperX;
            }
            set
            {
                _pathTaperX = value;
            }
        }

        public sbyte PathTaperY
        {
            get
            {
                return _pathTaperY;
            }
            set
            {
                _pathTaperY = value;
            }
        }

        public sbyte PathTwist
        {
            get
            {
                return _pathTwist;
            }
            set
            {
                _pathTwist = value;
            }
        }

        public sbyte PathTwistBegin
        {
            get
            {
                return _pathTwistBegin;
            }
            set
            {
                _pathTwistBegin = value;
            }
        }

        public byte PCode
        {
            get
            {
                return _pCode;
            }
            set
            {
                _pCode = value;
            }
        }

        public ushort ProfileBegin
        {
            get
            {
                return _profileBegin;
            }
            set
            {
                _profileBegin = value;
            }
        }

        public ushort ProfileEnd
        {
            get
            {
                return _profileEnd;
            }
            set
            {
                _profileEnd = value;
            }
        }

        public ushort ProfileHollow
        {
            get
            {
                return _profileHollow;
            }
            set
            {
                _profileHollow = value;
            }
        }

        public Vector3 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
            }
        }

        public byte State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        public ProfileShape ProfileShape
        {
            get
            {
                return _profileShape;
            }
            set
            {
                _profileShape = value;
            }
        }

        public HollowShape HollowShape
        {
            get
            {
                return _hollowShape;
            }
            set
            {
                _hollowShape = value;
            }
        }

        public UUID SculptTexture
        {
            get
            {
                return _sculptTexture;
            }
            set
            {
                _sculptTexture = value;
            }
        }

        public byte SculptType
        {
            get
            {
                return _sculptType;
            }
            set
            {
                _sculptType = value;
            }
        }

        public byte[] SculptData
        {
            get
            {
                return _sculptData;
            }
            set
            {
                _sculptData = value;
            }
        }

        public int FlexiSoftness
        {
            get
            {
                return _flexiSoftness;
            }
            set
            {
                _flexiSoftness = value;
            }
        }

        public float FlexiTension
        {
            get
            {
                return _flexiTension;
            }
            set
            {
                _flexiTension = value;
            }
        }

        public float FlexiDrag
        {
            get
            {
                return _flexiDrag;
            }
            set
            {
                _flexiDrag = value;
            }
        }

        public float FlexiGravity
        {
            get
            {
                return _flexiGravity;
            }
            set
            {
                _flexiGravity = value;
            }
        }

        public float FlexiWind
        {
            get
            {
                return _flexiWind;
            }
            set
            {
                _flexiWind = value;
            }
        }

        public float FlexiForceX
        {
            get
            {
                return _flexiForceX;
            }
            set
            {
                _flexiForceX = value;
            }
        }

        public float FlexiForceY
        {
            get
            {
                return _flexiForceY;
            }
            set
            {
                _flexiForceY = value;
            }
        }

        public float FlexiForceZ
        {
            get
            {
                return _flexiForceZ;
            }
            set
            {
                _flexiForceZ = value;
            }
        }

        public float LightColorR
        {
            get
            {
                return _lightColorR;
            }
            set
            {
                _lightColorR = value;
            }
        }

        public float LightColorG
        {
            get
            {
                return _lightColorG;
            }
            set
            {
                _lightColorG = value;
            }
        }

        public float LightColorB
        {
            get
            {
                return _lightColorB;
            }
            set
            {
                _lightColorB = value;
            }
        }

        public float LightColorA
        {
            get
            {
                return _lightColorA;
            }
            set
            {
                _lightColorA = value;
            }
        }

        public float LightRadius
        {
            get
            {
                return _lightRadius;
            }
            set
            {
                _lightRadius = value;
            }
        }

        public float LightCutoff
        {
            get
            {
                return _lightCutoff;
            }
            set
            {
                _lightCutoff = value;
            }
        }

        public float LightFalloff
        {
            get
            {
                return _lightFalloff;
            }
            set
            {
                _lightFalloff = value;
            }
        }

        public float LightIntensity
        {
            get
            {
                return _lightIntensity;
            }
            set
            {
                _lightIntensity = value;
            }
        }

        public bool FlexiEntry
        {
            get
            {
                return _flexiEntry;
            }
            set
            {
                _flexiEntry = value;
            }
        }

        public bool LightEntry
        {
            get
            {
                return _lightEntry;
            }
            set
            {
                _lightEntry = value;
            }
        }

        public bool SculptEntry
        {
            get
            {
                return _sculptEntry;
            }
            set
            {
                _sculptEntry = value;
            }
        }

        public byte[] ExtraParamsToBytes()
        {
            ushort FlexiEP = 0x10;
            ushort LightEP = 0x20;
            ushort SculptEP = 0x30;

            int i = 0;
            uint TotalBytesLength = 1; // ExtraParamsNum

            uint ExtraParamsNum = 0;
            if (_flexiEntry)
            {
                ExtraParamsNum++;
                TotalBytesLength += 16;// data
                TotalBytesLength += 2 + 4; // type
            }
            if (_lightEntry)
            {
                ExtraParamsNum++;
                TotalBytesLength += 16;// data
                TotalBytesLength += 2 + 4; // type
            }
            if (_sculptEntry)
            {
                ExtraParamsNum++;
                TotalBytesLength += 17;// data
                TotalBytesLength += 2 + 4; // type
            }

            byte[] returnbytes = new byte[TotalBytesLength];


            // uint paramlength = ExtraParamsNum;

            // Stick in the number of parameters
            returnbytes[i++] = (byte)ExtraParamsNum;

            if (_flexiEntry)
            {
                byte[] FlexiData = GetFlexiBytes();

                returnbytes[i++] = (byte)(FlexiEP % 256);
                returnbytes[i++] = (byte)((FlexiEP >> 8) % 256);

                returnbytes[i++] = (byte)(FlexiData.Length % 256);
                returnbytes[i++] = (byte)((FlexiData.Length >> 8) % 256);
                returnbytes[i++] = (byte)((FlexiData.Length >> 16) % 256);
                returnbytes[i++] = (byte)((FlexiData.Length >> 24) % 256);
                Array.Copy(FlexiData, 0, returnbytes, i, FlexiData.Length);
                i += FlexiData.Length;
            }
            if (_lightEntry)
            {
                byte[] LightData = GetLightBytes();

                returnbytes[i++] = (byte)(LightEP % 256);
                returnbytes[i++] = (byte)((LightEP >> 8) % 256);

                returnbytes[i++] = (byte)(LightData.Length % 256);
                returnbytes[i++] = (byte)((LightData.Length >> 8) % 256);
                returnbytes[i++] = (byte)((LightData.Length >> 16) % 256);
                returnbytes[i++] = (byte)((LightData.Length >> 24) % 256);
                Array.Copy(LightData, 0, returnbytes, i, LightData.Length);
                i += LightData.Length;
            }
            if (_sculptEntry)
            {
                byte[] SculptData = GetSculptBytes();

                returnbytes[i++] = (byte)(SculptEP % 256);
                returnbytes[i++] = (byte)((SculptEP >> 8) % 256);

                returnbytes[i++] = (byte)(SculptData.Length % 256);
                returnbytes[i++] = (byte)((SculptData.Length >> 8) % 256);
                returnbytes[i++] = (byte)((SculptData.Length >> 16) % 256);
                returnbytes[i++] = (byte)((SculptData.Length >> 24) % 256);
                Array.Copy(SculptData, 0, returnbytes, i, SculptData.Length);
                i += SculptData.Length;
            }

            if (!_flexiEntry && !_lightEntry && !_sculptEntry)
            {
                byte[] returnbyte = new byte[1];
                returnbyte[0] = 0;
                return returnbyte;
            }


            return returnbytes;
        }

        public void ReadInUpdateExtraParam(ushort type, bool inUse, byte[] data)
        {
            const ushort FlexiEP = 0x10;
            const ushort LightEP = 0x20;
            const ushort SculptEP = 0x30;

            switch (type)
            {
                case FlexiEP:
                    if (!inUse)
                    {
                        _flexiEntry = false;
                        return;
                    }
                    ReadFlexiData(data, 0);
                    break;

                case LightEP:
                    if (!inUse)
                    {
                        _lightEntry = false;
                        return;
                    }
                    ReadLightData(data, 0);
                    break;

                case SculptEP:
                    if (!inUse)
                    {
                        _sculptEntry = false;
                        return;
                    }
                    ReadSculptData(data, 0);
                    break;
            }
        }

        public void ReadInExtraParamsBytes(byte[] data)
        {
            if (data == null)
                return;

            const ushort FlexiEP = 0x10;
            const ushort LightEP = 0x20;
            const ushort SculptEP = 0x30;

            bool lGotFlexi = false;
            bool lGotLight = false;
            bool lGotSculpt = false;

            int i = 0;
            byte extraParamCount = 0;
            if (data.Length > 0)
            {
                extraParamCount = data[i++];
            }


            for (int k = 0; k < extraParamCount; k++)
            {
                ushort epType = Utils.BytesToUInt16(data, i);

                i += 2;
                // uint paramLength = Helpers.BytesToUIntBig(data, i);

                i += 4;
                switch (epType)
                {
                    case FlexiEP:
                        ReadFlexiData(data, i);
                        i += 16;
                        lGotFlexi = true;
                        break;

                    case LightEP:
                        ReadLightData(data, i);
                        i += 16;
                        lGotLight = true;
                        break;

                    case SculptEP:
                        ReadSculptData(data, i);
                        i += 17;
                        lGotSculpt = true;
                        break;
                }
            }

            if (!lGotFlexi)
                _flexiEntry = false;
            if (!lGotLight)
                _lightEntry = false;
            if (!lGotSculpt)
                _sculptEntry = false;

        }

        public void ReadSculptData(byte[] data, int pos)
        {
            byte[] SculptTextureUUID = new byte[16];
            UUID SculptUUID = UUID.Zero;
            byte SculptTypel = data[16 + pos];

            if (data.Length + pos >= 17)
            {
                _sculptEntry = true;
                SculptTextureUUID = new byte[16];
                SculptTypel = data[16 + pos];
                Array.Copy(data, pos, SculptTextureUUID, 0, 16);
                SculptUUID = new UUID(SculptTextureUUID, 0);
            }
            else
            {
                _sculptEntry = false;
                SculptUUID = UUID.Zero;
                SculptTypel = 0x00;
            }

            if (_sculptEntry)
            {
                if (_sculptType != (byte)1 && _sculptType != (byte)2 && _sculptType != (byte)3 && _sculptType != (byte)4)
                    _sculptType = 4;
            }
            _sculptTexture = SculptUUID;
            _sculptType = SculptTypel;
            //m_log.Info("[SCULPT]:" + SculptUUID.ToString());
        }

        public byte[] GetSculptBytes()
        {
            byte[] data = new byte[17];

            _sculptTexture.GetBytes().CopyTo(data, 0);
            data[16] = (byte)_sculptType;

            return data;
        }

        public void ReadFlexiData(byte[] data, int pos)
        {
            if (data.Length - pos >= 16)
            {
                _flexiEntry = true;
                _flexiSoftness = ((data[pos] & 0x80) >> 6) | ((data[pos + 1] & 0x80) >> 7);

                _flexiTension = (float)(data[pos++] & 0x7F) / 10.0f;
                _flexiDrag = (float)(data[pos++] & 0x7F) / 10.0f;
                _flexiGravity = (float)(data[pos++] / 10.0f) - 10.0f;
                _flexiWind = (float)data[pos++] / 10.0f;
                Vector3 lForce = new Vector3(data, pos);
                _flexiForceX = lForce.X;
                _flexiForceY = lForce.Y;
                _flexiForceZ = lForce.Z;
            }
            else
            {
                _flexiEntry = false;
                _flexiSoftness = 0;

                _flexiTension = 0.0f;
                _flexiDrag = 0.0f;
                _flexiGravity = 0.0f;
                _flexiWind = 0.0f;
                _flexiForceX = 0f;
                _flexiForceY = 0f;
                _flexiForceZ = 0f;
            }
        }

        public byte[] GetFlexiBytes()
        {
            byte[] data = new byte[16];
            int i = 0;

            // Softness is packed in the upper bits of tension and drag
            data[i] = (byte)((_flexiSoftness & 2) << 6);
            data[i + 1] = (byte)((_flexiSoftness & 1) << 7);

            data[i++] |= (byte)((byte)(_flexiTension * 10.01f) & 0x7F);
            data[i++] |= (byte)((byte)(_flexiDrag * 10.01f) & 0x7F);
            data[i++] = (byte)((_flexiGravity + 10.0f) * 10.01f);
            data[i++] = (byte)(_flexiWind * 10.01f);
            Vector3 lForce = new Vector3(_flexiForceX, _flexiForceY, _flexiForceZ);
            lForce.GetBytes().CopyTo(data, i);

            return data;
        }

        public void ReadLightData(byte[] data, int pos)
        {
            if (data.Length - pos >= 16)
            {
                _lightEntry = true;
                Color4 lColor = new Color4(data, pos, false);
                _lightIntensity = lColor.A;
                _lightColorA = 1f;
                _lightColorR = lColor.R;
                _lightColorG = lColor.G;
                _lightColorB = lColor.B;

                _lightRadius = Utils.BytesToFloat(data, pos + 4);
                _lightCutoff = Utils.BytesToFloat(data, pos + 8);
                _lightFalloff = Utils.BytesToFloat(data, pos + 12);
            }
            else
            {
                _lightEntry = false;
                _lightColorA = 1f;
                _lightColorR = 0f;
                _lightColorG = 0f;
                _lightColorB = 0f;
                _lightRadius = 0f;
                _lightCutoff = 0f;
                _lightFalloff = 0f;
                _lightIntensity = 0f;
            }
        }

        public byte[] GetLightBytes()
        {
            byte[] data = new byte[16];

            // Alpha channel in color is intensity
            Color4 tmpColor = new Color4(_lightColorR, _lightColorG, _lightColorB, _lightIntensity);

            tmpColor.GetBytes().CopyTo(data, 0);
            Utils.FloatToBytes(_lightRadius).CopyTo(data, 4);
            Utils.FloatToBytes(_lightCutoff).CopyTo(data, 8);
            Utils.FloatToBytes(_lightFalloff).CopyTo(data, 12);

            return data;
        }
    }

    // A simple hull is a set of vertices building up to simplices that border a region
    // The word simple referes to the fact, that this class assumes, that all simplices
    // do not intersect
    // Simple hulls can be added and subtracted.
    // Vertices can be checked to lie inside a hull
    // Also note, that the sequence of the vertices is important and defines if the region that
    // is defined by the hull lies inside or outside the simplex chain
    public class SimpleHull
    {
        //private static readonly log4net.ILog m_log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private List<MeshmerizerVertex> vertices = new List<MeshmerizerVertex>();
        private List<MeshmerizerVertex> holeVertices = new List<MeshmerizerVertex>(); // Only used, when the hull is hollow

        // Adds a MeshmerizerVertex to the end of the list
        public void AddVertex(MeshmerizerVertex v)
        {
            vertices.Add(v);
        }

        public override String ToString()
        {
            String result = String.Empty;
            foreach (MeshmerizerVertex v in vertices)
            {
                result += "b:" + v.ToString() + "\n";
            }

            return result;
        }


        public List<MeshmerizerVertex> getVertices()
        {
            List<MeshmerizerVertex> newVertices = new List<MeshmerizerVertex>();

            newVertices.AddRange(vertices);
            newVertices.Add(null);
            newVertices.AddRange(holeVertices);

            return newVertices;
        }

        public SimpleHull Clone()
        {
            SimpleHull result = new SimpleHull();
            foreach (MeshmerizerVertex v in vertices)
            {
                result.AddVertex(v.Clone());
            }

            foreach (MeshmerizerVertex v in holeVertices)
            {
                result.holeVertices.Add(v.Clone());
            }

            return result;
        }

        public bool IsPointIn(MeshmerizerVertex v1)
        {
            int iCounter = 0;
            List<Simplex> simplices = buildSimplexList();
            foreach (Simplex s in simplices)
            {
                // Send a ray along the positive X-Direction
                // Note, that this direction must correlate with the "below" interpretation
                // of handling for the special cases below
                PhysicsVector intersection = s.RayIntersect(v1, new PhysicsVector(1.0f, 0.0f, 0.0f), true);

                if (intersection == null)
                    continue; // No intersection. Done. More tests to follow otherwise

                // Did we hit the end of a simplex?
                // Then this can be one of two special cases:
                // 1. we go through a border exactly at a joint
                // 2. we have just marginally touched a corner
                // 3. we can slide along a border
                // Solution: If the other vertex is "below" the ray, we don't count it
                // Thus corners pointing down are counted twice, corners pointing up are not counted
                // borders are counted once
                if (intersection.IsIdentical(s.v1, 0.001f))
                {
                    if (s.v2.Y < v1.Y)
                        continue;
                }
                // Do this for the other vertex two
                if (intersection.IsIdentical(s.v2, 0.001f))
                {
                    if (s.v1.Y < v1.Y)
                        continue;
                }
                iCounter++;
            }

            return iCounter % 2 == 1; // Point is inside if the number of intersections is odd
        }

        public bool containsPointsFrom(SimpleHull otherHull)
        {
            foreach (MeshmerizerVertex v in otherHull.vertices)
            {
                if (IsPointIn(v))
                    return true;
            }

            return false;
        }


        private List<Simplex> buildSimplexList()
        {
            List<Simplex> result = new List<Simplex>();

            // Not asserted but assumed: at least three vertices
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                Simplex s = new Simplex(vertices[i], vertices[i + 1]);
                result.Add(s);
            }
            Simplex s1 = new Simplex(vertices[vertices.Count - 1], vertices[0]);
            result.Add(s1);

            if (holeVertices.Count == 0)
                return result;

            // Same here. At least three vertices in hole assumed
            for (int i = 0; i < holeVertices.Count - 1; i++)
            {
                Simplex s = new Simplex(holeVertices[i], holeVertices[i + 1]);
                result.Add(s);
            }

            s1 = new Simplex(holeVertices[holeVertices.Count - 1], holeVertices[0]);
            result.Add(s1);
            return result;
        }

        private MeshmerizerVertex getNextVertex(MeshmerizerVertex currentVertex)
        {
            int iCurrentIndex;
            iCurrentIndex = vertices.IndexOf(currentVertex);

            // Error handling for iCurrentIndex==-1 should go here (and probably never will)

            iCurrentIndex++;
            if (iCurrentIndex == vertices.Count)
                iCurrentIndex = 0;

            return vertices[iCurrentIndex];
        }

        public MeshmerizerVertex FindVertex(MeshmerizerVertex vBase, float tolerance)
        {
            foreach (MeshmerizerVertex v in vertices)
            {
                if (v.IsIdentical(vBase, tolerance))
                    return v;
            }

            return null;
        }

        public void FindIntersection(Simplex s, ref MeshmerizerVertex Intersection, ref MeshmerizerVertex nextVertex)
        {
            MeshmerizerVertex bestIntersection = null;
            float distToV1 = Single.PositiveInfinity;
            Simplex bestIntersectingSimplex = null;

            List<Simplex> simple = buildSimplexList();
            foreach (Simplex sTest in simple)
            {
                PhysicsVector vvTemp = Simplex.Intersect(sTest, s, -.001f, -.001f, 0.999f, .999f);

                MeshmerizerVertex vTemp = null;
                if (vvTemp != null)
                    vTemp = new MeshmerizerVertex(vvTemp);

                if (vTemp != null)
                {
                    PhysicsVector diff = (s.v1 - vTemp);
                    float distTemp = diff.length();

                    if (bestIntersection == null || distTemp < distToV1)
                    {
                        bestIntersection = vTemp;
                        distToV1 = distTemp;
                        bestIntersectingSimplex = sTest;
                    }
                }
            }

            Intersection = bestIntersection;
            if (bestIntersectingSimplex != null)
                nextVertex = bestIntersectingSimplex.v2;
            else
                nextVertex = null;
        }


        public static SimpleHull SubtractHull(SimpleHull baseHull, SimpleHull otherHull)
        {
            SimpleHull baseHullClone = baseHull.Clone();
            SimpleHull otherHullClone = otherHull.Clone();
            bool intersects = false;

            //m_log.Debug("State before intersection detection");
            //m_log.DebugFormat("The baseHull is:\n{1}", 0, baseHullClone.ToString());
            //m_log.DebugFormat("The otherHull is:\n{1}", 0, otherHullClone.ToString());

            {
                int iBase, iOther;

                // Insert into baseHull
                for (iBase = 0; iBase < baseHullClone.vertices.Count; iBase++)
                {
                    int iBaseNext = (iBase + 1) % baseHullClone.vertices.Count;
                    Simplex sBase = new Simplex(baseHullClone.vertices[iBase], baseHullClone.vertices[iBaseNext]);

                    for (iOther = 0; iOther < otherHullClone.vertices.Count; iOther++)
                    {
                        int iOtherNext = (iOther + 1) % otherHullClone.vertices.Count;
                        Simplex sOther =
                            new Simplex(otherHullClone.vertices[iOther], otherHullClone.vertices[iOtherNext]);

                        PhysicsVector intersect = Simplex.Intersect(sBase, sOther, 0.001f, -.001f, 0.999f, 1.001f);
                        if (intersect != null)
                        {
                            MeshmerizerVertex vIntersect = new MeshmerizerVertex(intersect);
                            baseHullClone.vertices.Insert(iBase + 1, vIntersect);
                            sBase.v2 = vIntersect;
                            intersects = true;
                        }
                    }
                }
            }

            //m_log.Debug("State after intersection detection for the base hull");
            //m_log.DebugFormat("The baseHull is:\n{1}", 0, baseHullClone.ToString());

            {
                int iOther, iBase;

                // Insert into otherHull
                for (iOther = 0; iOther < otherHullClone.vertices.Count; iOther++)
                {
                    int iOtherNext = (iOther + 1) % otherHullClone.vertices.Count;
                    Simplex sOther = new Simplex(otherHullClone.vertices[iOther], otherHullClone.vertices[iOtherNext]);

                    for (iBase = 0; iBase < baseHullClone.vertices.Count; iBase++)
                    {
                        int iBaseNext = (iBase + 1) % baseHullClone.vertices.Count;
                        Simplex sBase = new Simplex(baseHullClone.vertices[iBase], baseHullClone.vertices[iBaseNext]);

                        PhysicsVector intersect = Simplex.Intersect(sBase, sOther, -.001f, 0.001f, 1.001f, 0.999f);
                        if (intersect != null)
                        {
                            MeshmerizerVertex vIntersect = new MeshmerizerVertex(intersect);
                            otherHullClone.vertices.Insert(iOther + 1, vIntersect);
                            sOther.v2 = vIntersect;
                            intersects = true;
                        }
                    }
                }
            }

            //m_log.Debug("State after intersection detection for the base hull");
            //m_log.DebugFormat("The otherHull is:\n{1}", 0, otherHullClone.ToString());

            bool otherIsInBase = baseHullClone.containsPointsFrom(otherHullClone);
            if (!intersects && otherIsInBase)
            {
                // We have a hole here
                baseHullClone.holeVertices = otherHullClone.vertices;
                return baseHullClone;
            }

            SimpleHull result = new SimpleHull();

            // Find a good starting Simplex from baseHull
            // A good starting simplex is one that is outside otherHull
            // Such a simplex must exist, otherwise the result will be empty
            MeshmerizerVertex baseStartVertex = null;
            {
                int iBase;
                for (iBase = 0; iBase < baseHullClone.vertices.Count; iBase++)
                {
                    int iBaseNext = (iBase + 1) % baseHullClone.vertices.Count;
                    MeshmerizerVertex center = new MeshmerizerVertex((baseHullClone.vertices[iBase] + baseHullClone.vertices[iBaseNext]) / 2.0f);
                    bool isOutside = !otherHullClone.IsPointIn(center);
                    if (isOutside)
                    {
                        baseStartVertex = baseHullClone.vertices[iBaseNext];
                        break;
                    }
                }
            }


            if (baseStartVertex == null) // i.e. no simplex fulfilled the "outside" condition.
            // In otherwords, subtractHull completely embraces baseHull
            {
                return result;
            }

            // The simplex that *starts* with baseStartVertex is outside the cutting hull,
            // so we can start our walk with the next vertex without loosing a branch
            MeshmerizerVertex V1 = baseStartVertex;
            bool onBase = true;

            // And here is how we do the magic :-)
            // Start on the base hull.
            // Walk the vertices in the positive direction
            // For each vertex check, whether it is a vertex shared with the other hull
            // if this is the case, switch over to walking the other vertex list.
            // Note: The other hull *must* go backwards to our starting point (via several orther vertices)
            // Thus it is important that the cutting hull has the inverse directional sense than the
            // base hull!!!!!!!!! (means if base goes CW around it's center cutting hull must go CCW)

            bool done = false;
            while (!done)
            {
                result.AddVertex(V1);
                MeshmerizerVertex nextVertex = null;
                if (onBase)
                {
                    nextVertex = otherHullClone.FindVertex(V1, 0.001f);
                }
                else
                {
                    nextVertex = baseHullClone.FindVertex(V1, 0.001f);
                }

                if (nextVertex != null) // A node that represents an intersection
                {
                    V1 = nextVertex; // Needed to find the next vertex on the other hull
                    onBase = !onBase;
                }

                if (onBase)
                    V1 = baseHullClone.getNextVertex(V1);
                else
                    V1 = otherHullClone.getNextVertex(V1);

                if (V1 == baseStartVertex)
                    done = true;
            }

            //m_log.DebugFormat("The resulting Hull is:\n{1}", 0, result.ToString());

            return result;
        }
    }

    public class MeshmerizerMesh
    {
        public List<MeshmerizerVertex> vertices;
        public List<Triangle> triangles;
        GCHandle pinnedVirtexes;
        GCHandle pinnedIndex;
        public PrimMesh primMesh = null;

        public MeshmerizerMesh()
        {
            vertices = new List<MeshmerizerVertex>();
            triangles = new List<Triangle>();
        }

        public MeshmerizerMesh Clone()
        {
            MeshmerizerMesh result = new MeshmerizerMesh();

            foreach (MeshmerizerVertex v in vertices)
            {
                if (v == null)
                    result.vertices.Add(null);
                else
                    result.vertices.Add(v.Clone());
            }

            foreach (Triangle t in triangles)
            {
                int iV1, iV2, iV3;
                iV1 = vertices.IndexOf(t.v1);
                iV2 = vertices.IndexOf(t.v2);
                iV3 = vertices.IndexOf(t.v3);

                Triangle newT = new Triangle(result.vertices[iV1], result.vertices[iV2], result.vertices[iV3]);
                result.Add(newT);
            }

            return result;
        }

        public void Add(Triangle triangle)
        {
            int i;
            i = vertices.IndexOf(triangle.v1);
            if (i < 0)
                throw new ArgumentException("Vertex v1 not known to mesh");
            i = vertices.IndexOf(triangle.v2);
            if (i < 0)
                throw new ArgumentException("Vertex v2 not known to mesh");
            i = vertices.IndexOf(triangle.v3);
            if (i < 0)
                throw new ArgumentException("Vertex v3 not known to mesh");

            triangles.Add(triangle);
        }

        public void Add(MeshmerizerVertex v)
        {
            vertices.Add(v);
        }

        public void Remove(MeshmerizerVertex v)
        {
            int i;

            // First, remove all triangles that are build on v
            for (i = 0; i < triangles.Count; i++)
            {
                Triangle t = triangles[i];
                if (t.v1 == v || t.v2 == v || t.v3 == v)
                {
                    triangles.RemoveAt(i);
                    i--;
                }
            }

            // Second remove v itself
            vertices.Remove(v);
        }

        public void RemoveTrianglesOutside(SimpleHull hull)
        {
            int i;

            for (i = 0; i < triangles.Count; i++)
            {
                Triangle t = triangles[i];
                MeshmerizerVertex v1 = t.v1;
                MeshmerizerVertex v2 = t.v2;
                MeshmerizerVertex v3 = t.v3;
                PhysicsVector m = v1 + v2 + v3;
                m /= 3.0f;
                if (!hull.IsPointIn(new MeshmerizerVertex(m)))
                {
                    triangles.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Add(List<MeshmerizerVertex> lv)
        {
            foreach (MeshmerizerVertex v in lv)
            {
                vertices.Add(v);
            }
        }

        public List<PhysicsVector> getVertexList()
        {
            List<PhysicsVector> result = new List<PhysicsVector>();
            foreach (MeshmerizerVertex v in vertices)
            {
                result.Add(v);
            }
            return result;
        }

        public float[] getVertexListAsFloatLocked()
        {
            float[] result;

            if (primMesh == null)
            {
                result = new float[vertices.Count * 3];
                for (int i = 0; i < vertices.Count; i++)
                {
                    MeshmerizerVertex v = vertices[i];
                    if (v == null)
                        continue;
                    result[3 * i + 0] = v.X;
                    result[3 * i + 1] = v.Y;
                    result[3 * i + 2] = v.Z;
                }
                pinnedVirtexes = GCHandle.Alloc(result, GCHandleType.Pinned);
            }
            else
            {
                int count = primMesh.coords.Count;
                result = new float[count * 3];
                for (int i = 0; i < count; i++)
                {
                    Coord c = primMesh.coords[i];
                    {
                        int resultIndex = 3 * i;
                        result[resultIndex] = c.X;
                        result[resultIndex + 1] = c.Y;
                        result[resultIndex + 2] = c.Z;
                    }

                }
                pinnedVirtexes = GCHandle.Alloc(result, GCHandleType.Pinned);
            }
            return result;
        }

        public int[] getIndexListAsInt()
        {
            int[] result;

            if (primMesh == null)
            {
                result = new int[triangles.Count * 3];
                for (int i = 0; i < triangles.Count; i++)
                {
                    Triangle t = triangles[i];
                    result[3 * i + 0] = vertices.IndexOf(t.v1);
                    result[3 * i + 1] = vertices.IndexOf(t.v2);
                    result[3 * i + 2] = vertices.IndexOf(t.v3);
                }
            }
            else
            {
                int numFaces = primMesh.faces.Count;
                result = new int[numFaces * 3];
                for (int i = 0; i < numFaces; i++)
                {
                    MeshmerizerFace f = primMesh.faces[i];

                    int resultIndex = i * 3;
                    result[resultIndex] = f.v1;
                    result[resultIndex + 1] = f.v2;
                    result[resultIndex + 2] = f.v3;
                }
            }
            return result;
        }

        /// <summary>
        /// creates a list of index values that defines triangle faces. THIS METHOD FREES ALL NON-PINNED MESH DATA
        /// </summary>
        /// <returns></returns>
        public int[] getIndexListAsIntLocked()
        {
            int[] result = getIndexListAsInt();
            pinnedIndex = GCHandle.Alloc(result, GCHandleType.Pinned);

            return result;
        }

        public void releasePinned()
        {
            pinnedVirtexes.Free();
            pinnedIndex.Free();
        }

        /// <summary>
        /// frees up the source mesh data to minimize memory - call this method after calling get*Locked() functions
        /// </summary>
        public void releaseSourceMeshData()
        {
            triangles = null;
            vertices = null;
            primMesh = null;
        }

        public void Append(MeshmerizerMesh newMesh)
        {
            foreach (MeshmerizerVertex v in newMesh.vertices)
                vertices.Add(v);

            foreach (Triangle t in newMesh.triangles)
                Add(t);
        }

        // Do a linear transformation of  mesh.
        public void TransformLinear(float[,] matrix, float[] offset)
        {
            foreach (MeshmerizerVertex v in vertices)
            {
                if (v == null)
                    continue;
                float x, y, z;
                x = v.X * matrix[0, 0] + v.Y * matrix[1, 0] + v.Z * matrix[2, 0];
                y = v.X * matrix[0, 1] + v.Y * matrix[1, 1] + v.Z * matrix[2, 1];
                z = v.X * matrix[0, 2] + v.Y * matrix[1, 2] + v.Z * matrix[2, 2];
                v.X = x + offset[0];
                v.Y = y + offset[1];
                v.Z = z + offset[2];
            }
        }
    }

    /// <summary>
    /// generates a profile for extrusion
    /// </summary>
    public class MeshmerizerProfile
    {
        private const float twoPi = 2.0f * (float)Math.PI;

        internal List<Coord> coords;
        internal List<MeshmerizerFace> faces;

        internal MeshmerizerProfile()
        {
            this.coords = new List<Coord>();
            this.faces = new List<MeshmerizerFace>();
        }

        public MeshmerizerProfile(int sides, float profileStart, float profileEnd, float hollow, int hollowSides, bool createFaces)
        {
            this.coords = new List<Coord>();
            this.faces = new List<MeshmerizerFace>();
            Coord center = new Coord(0.0f, 0.0f, 0.0f);
            List<Coord> hollowCoords = new List<Coord>();

            AngleList angles = new AngleList();
            AngleList hollowAngles = new AngleList();

            float xScale = 0.5f;
            float yScale = 0.5f;
            if (sides == 4)  // corners of a square are sqrt(2) from center
            {
                xScale = 0.707f;
                yScale = 0.707f;
            }

            float startAngle = profileStart * twoPi;
            float stopAngle = profileEnd * twoPi;
            // float stepSize = twoPi / sides;

            try { angles.makeAngles(sides, startAngle, stopAngle); }
            catch (Exception ex)
            {
                Console.WriteLine("makeAngles failed: Exception: " + ex.ToString());
                Console.WriteLine("sides: " + sides.ToString() + " startAngle: " + startAngle.ToString() + " stopAngle: " + stopAngle.ToString());
                return;
            }

            if (hollow > 0.001f)
            {
                if (sides == hollowSides)
                    hollowAngles = angles;
                else
                {
                    try { hollowAngles.makeAngles(hollowSides, startAngle, stopAngle); }
                    catch (Exception ex)
                    {
                        Console.WriteLine("makeAngles failed: Exception: " + ex.ToString());
                        Console.WriteLine("sides: " + sides.ToString() + " startAngle: " + startAngle.ToString() + " stopAngle: " + stopAngle.ToString());
                        return;
                    }
                }
            }
            else
                this.coords.Add(center);

            float z = 0.0f;

            Angle angle;
            Coord newVert = new Coord();
            if (hollow > 0.001f && hollowSides != sides)
            {
                int numHollowAngles = hollowAngles.angles.Count;
                for (int i = 0; i < numHollowAngles; i++)
                {
                    angle = hollowAngles.angles[i];
                    newVert.X = hollow * xScale * angle.X;
                    newVert.Y = hollow * yScale * angle.Y;
                    newVert.Z = z;

                    hollowCoords.Add(newVert);
                }
            }

            int index = 0;
            int numAngles = angles.angles.Count;
            for (int i = 0; i < numAngles; i++)
            {
                angle = angles.angles[i];
                newVert.X = angle.X * xScale;
                newVert.Y = angle.Y * yScale;
                newVert.Z = z;
                this.coords.Add(newVert);

                if (hollow > 0.0f)
                {
                    if (hollowSides == sides)
                    {
                        newVert.X *= hollow;
                        newVert.Y *= hollow;
                        newVert.Z = z;
                        hollowCoords.Add(newVert);
                    }
                }
                else if (createFaces && angle.angle > 0.0001f)
                {
                    MeshmerizerFace newFace = new MeshmerizerFace();
                    newFace.v1 = 0;
                    newFace.v2 = index;
                    newFace.v3 = index + 1;
                    this.faces.Add(newFace);
                }
                index += 1;
            }

            if (hollow > 0.0f)
            {
                hollowCoords.Reverse();

                if (createFaces)
                {
                    int numOuterVerts = this.coords.Count;
                    int numHollowVerts = hollowCoords.Count;
                    int numTotalVerts = numOuterVerts + numHollowVerts;

                    if (numOuterVerts == numHollowVerts)
                    {
                        MeshmerizerFace newFace = new MeshmerizerFace();

                        for (int coordIndex = 0; coordIndex < numOuterVerts - 1; coordIndex++)
                        {
                            newFace.v1 = coordIndex;
                            newFace.v2 = coordIndex + 1;
                            newFace.v3 = numTotalVerts - coordIndex - 1;
                            this.faces.Add(newFace);

                            newFace.v1 = coordIndex + 1;
                            newFace.v2 = numTotalVerts - coordIndex - 2;
                            newFace.v3 = numTotalVerts - coordIndex - 1;
                            this.faces.Add(newFace);
                        }
                    }
                    else
                    {
                        if (numOuterVerts < numHollowVerts)
                        {
                            MeshmerizerFace newFace = new MeshmerizerFace();
                            int j = 0; // j is the index for outer vertices
                            int maxJ = numOuterVerts - 1;
                            for (int i = 0; i < numHollowVerts; i++) // i is the index for inner vertices
                            {
                                if (j < maxJ)
                                    if (angles.angles[j + 1].angle - hollowAngles.angles[i].angle <= hollowAngles.angles[i].angle - angles.angles[j].angle)
                                    {
                                        newFace.v1 = numTotalVerts - i - 1;
                                        newFace.v2 = j;
                                        newFace.v3 = j + 1;

                                        this.faces.Add(newFace);
                                        j += 1;
                                    }

                                newFace.v1 = j;
                                newFace.v2 = numTotalVerts - i - 2;
                                newFace.v3 = numTotalVerts - i - 1;

                                this.faces.Add(newFace);
                            }
                        }
                        else // numHollowVerts < numOuterVerts
                        {
                            MeshmerizerFace newFace = new MeshmerizerFace();
                            int j = 0; // j is the index for inner vertices
                            int maxJ = numHollowVerts - 1;
                            for (int i = 0; i < numOuterVerts; i++)
                            {
                                if (j < maxJ)
                                    if (hollowAngles.angles[j + 1].angle - angles.angles[i].angle <= angles.angles[i].angle - hollowAngles.angles[j].angle)
                                    {
                                        newFace.v1 = i;
                                        newFace.v2 = numTotalVerts - j - 2;
                                        newFace.v3 = numTotalVerts - j - 1;

                                        this.faces.Add(newFace);
                                        j += 1;
                                    }

                                newFace.v1 = numTotalVerts - j - 1;
                                newFace.v2 = i;
                                newFace.v3 = i + 1;

                                this.faces.Add(newFace);
                            }
                        }
                    }
                }

                this.coords.AddRange(hollowCoords);
            }
        }

        public MeshmerizerProfile Clone()
        {
            return this.Clone(true);
        }

        public MeshmerizerProfile Clone(bool needFaces)
        {
            MeshmerizerProfile clone = new MeshmerizerProfile();

            clone.coords.AddRange(this.coords);
            if (needFaces)
                clone.faces.AddRange(this.faces);

            return clone;
        }

        public void AddPos(Coord v)
        {
            this.AddPos(v.X, v.Y, v.Z);
        }

        public void AddPos(float x, float y, float z)
        {
            int i;
            int numVerts = this.coords.Count;
            Coord vert;

            for (i = 0; i < numVerts; i++)
            {
                vert = this.coords[i];
                vert.X += x;
                vert.Y += y;
                vert.Z += z;
                this.coords[i] = vert;
            }
        }

        public void AddRot(Quaternion q)
        {
            int i;
            int numVerts = this.coords.Count;
            Coord vert;

            for (i = 0; i < numVerts; i++)
            {
                vert = this.coords[i];
                MeshmerizerVertex v = new MeshmerizerVertex(vert.X, vert.Y, vert.Z) * q;

                vert.X = v.X;
                vert.Y = v.Y;
                vert.Z = v.Z;
                this.coords[i] = vert;
            }
        }

        public void Scale(float x, float y)
        {
            int i;
            int numVerts = this.coords.Count;
            Coord vert;

            for (i = 0; i < numVerts; i++)
            {
                vert = this.coords[i];
                vert.X *= x;
                vert.Y *= y;
                this.coords[i] = vert;
            }
        }

        public void FlipNormals()
        {
            int i;
            int numFaces = this.faces.Count;
            MeshmerizerFace tmpFace;
            int tmp;

            for (i = 0; i < numFaces; i++)
            {
                tmpFace = this.faces[i];
                tmp = tmpFace.v3;
                tmpFace.v3 = tmpFace.v1;
                tmpFace.v1 = tmp;
                this.faces[i] = tmpFace;
            }
        }

        public void AddValue2Faces(int num)
        {
            int numFaces = this.faces.Count;
            MeshmerizerFace tmpFace;
            for (int i = 0; i < numFaces; i++)
            {
                tmpFace = this.faces[i];
                tmpFace.v1 += num;
                tmpFace.v2 += num;
                tmpFace.v3 += num;
                this.faces[i] = tmpFace;
            }
        }
    }
}
