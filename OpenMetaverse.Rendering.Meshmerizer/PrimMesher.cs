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

namespace OpenMetaverse.Rendering
{
    public class PrimMesh
    {
        private const float twoPi = 2.0f * (float)Math.PI;

        public List<Coord> coords;
        public List<MeshmerizerFace> faces;

        public int sides = 4;
        public int hollowSides = 4;
        public float profileStart = 0.0f;
        public float profileEnd = 1.0f;
        public float hollow = 0.0f;
        public int twistBegin = 0;
        public int twistEnd = 0;
        public float topShearX = 0.0f;
        public float topShearY = 0.0f;
        public float pathCutBegin = 0.0f;
        public float pathCutEnd = 1.0f;
        public float dimpleBegin = 0.0f;
        public float dimpleEnd = 1.0f;
        public float skew = 0.0f;
        public float holeSizeX = 1.0f; // called pathScaleX in pbs
        public float holeSizeY = 0.25f;
        public float taperX = 0.0f;
        public float taperY = 0.0f;
        public float radius = 0.0f;
        public float revolutions = 1.0f;
        public int stepsPerRevolution = 24;

        public string ParamsToDisplayString()
        {
            string s = "";
            s += "sides..................: " + this.sides.ToString();
            s += "\nhollowSides..........: " + this.hollowSides.ToString();
            s += "\nprofileStart.........: " + this.profileStart.ToString();
            s += "\nprofileEnd...........: " + this.profileEnd.ToString();
            s += "\nhollow...............: " + this.hollow.ToString();
            s += "\ntwistBegin...........: " + this.twistBegin.ToString();
            s += "\ntwistEnd.............: " + this.twistEnd.ToString();
            s += "\ntopShearX............: " + this.topShearX.ToString();
            s += "\ntopShearY............: " + this.topShearY.ToString();
            s += "\npathCutBegin.........: " + this.pathCutBegin.ToString();
            s += "\npathCutEnd...........: " + this.pathCutEnd.ToString();
            s += "\ndimpleBegin..........: " + this.dimpleBegin.ToString();
            s += "\ndimpleEnd............: " + this.dimpleEnd.ToString();
            s += "\nskew.................: " + this.skew.ToString();
            s += "\nholeSizeX............: " + this.holeSizeX.ToString();
            s += "\nholeSizeY............: " + this.holeSizeY.ToString();
            s += "\ntaperX...............: " + this.taperX.ToString();
            s += "\ntaperY...............: " + this.taperY.ToString();
            s += "\nradius...............: " + this.radius.ToString();
            s += "\nrevolutions..........: " + this.revolutions.ToString();
            s += "\nstepsPerRevolution...: " + this.stepsPerRevolution.ToString();

            return s;
        }


        public PrimMesh(int sides, float profileStart, float profileEnd, float hollow, int hollowSides)
        {
            this.coords = new List<Coord>();
            this.faces = new List<MeshmerizerFace>();

            this.sides = sides;
            this.profileStart = profileStart;
            this.profileEnd = profileEnd;
            this.hollow = hollow;
            this.hollowSides = hollowSides;

            if (sides < 3)
                this.sides = 3;
            if (hollowSides < 3)
                this.hollowSides = 3;
            if (profileStart < 0.0f)
                this.profileStart = 0.0f;
            if (profileEnd > 1.0f)
                this.profileEnd = 1.0f;
            if (profileEnd < 0.02f)
                this.profileEnd = 0.02f;
            if (profileStart >= profileEnd)
                this.profileStart = profileEnd - 0.02f;
            if (hollow > 1.0f)
                this.hollow = 1.0f;
            if (hollow < 0.0f)
                this.hollow = 0.0f;
        }

        public void ExtrudeLinear()
        {
            this.coords = new List<Coord>();
            this.faces = new List<MeshmerizerFace>();

            int step = 0;
            int steps = 1;

            float length = this.pathCutEnd - this.pathCutBegin;
            float twistBegin = this.twistBegin / 360.0f * twoPi;
            float twistEnd = this.twistEnd / 360.0f * twoPi;
            float twistTotal = twistEnd - twistBegin;
            float twistTotalAbs = Math.Abs(twistTotal);
            if (twistTotalAbs > 0.01f)
                steps += (int)(twistTotalAbs * 3.66); //  dahlia's magic number

            float start = -0.5f;
            float stepSize = length / (float)steps;
            float percentOfPathMultiplier = stepSize;
            float xProfileScale = 1.0f;
            float yProfileScale = 1.0f;
            float xOffset = 0.0f;
            float yOffset = 0.0f;
            float zOffset = start;
            float xOffsetStepIncrement = this.topShearX / steps;
            float yOffsetStepIncrement = this.topShearY / steps;

            float percentOfPath = this.pathCutBegin;
            zOffset += percentOfPath;

            float hollow = this.hollow;

            // sanity checks
            float initialProfileRot = 0.0f;
            if (this.sides == 3)
            {
                if (this.hollowSides == 4)
                {
                    if (hollow > 0.7f)
                        hollow = 0.7f;
                    hollow *= 0.707f;
                }
                else hollow *= 0.5f;
            }
            else if (this.sides == 4)
            {
                initialProfileRot = 1.25f * (float)Math.PI;
                if (this.hollowSides != 4)
                    hollow *= 0.707f;
            }
            else if (this.sides == 24 && this.hollowSides == 4)
                hollow *= 1.414f;

            MeshmerizerProfile profile = new MeshmerizerProfile(this.sides, this.profileStart, this.profileEnd, hollow, this.hollowSides, true);

            if (initialProfileRot != 0.0f)
                profile.AddRot(Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), initialProfileRot));

            bool done = false;
            while (!done)
            {
                MeshmerizerProfile newLayer = profile.Clone();

                if (this.taperX == 0.0f)
                    xProfileScale = 1.0f;
                else if (this.taperX > 0.0f)
                    xProfileScale = 1.0f - percentOfPath * this.taperX;
                else xProfileScale = 1.0f + (1.0f - percentOfPath) * this.taperX;

                if (this.taperY == 0.0f)
                    yProfileScale = 1.0f;
                else if (this.taperY > 0.0f)
                    yProfileScale = 1.0f - percentOfPath * this.taperY;
                else yProfileScale = 1.0f + (1.0f - percentOfPath) * this.taperY;

                if (xProfileScale != 1.0f || yProfileScale != 1.0f)
                    newLayer.Scale(xProfileScale, yProfileScale);

                float twist = twistBegin + twistTotal * percentOfPath;
                if (twist != 0.0f)
                    newLayer.AddRot(Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), twist));

                newLayer.AddPos(xOffset, yOffset, zOffset);

                if (step == 0)
                    newLayer.FlipNormals();

                // append this layer

                int coordsLen = this.coords.Count;
                newLayer.AddValue2Faces(coordsLen);

                this.coords.AddRange(newLayer.coords);

                if (percentOfPath <= this.pathCutBegin || percentOfPath >= this.pathCutEnd)
                    this.faces.AddRange(newLayer.faces);

                // fill faces between layers

                int numVerts = newLayer.coords.Count;
                MeshmerizerFace newFace = new MeshmerizerFace();
                if (step > 0)
                {
                    for (int i = coordsLen; i < this.coords.Count - 1; i++)
                    {
                        newFace.v1 = i;
                        newFace.v2 = i - numVerts;
                        newFace.v3 = i - numVerts + 1;
                        this.faces.Add(newFace);

                        newFace.v2 = i - numVerts + 1;
                        newFace.v3 = i + 1;
                        this.faces.Add(newFace);
                    }

                    newFace.v1 = coordsLen - 1;
                    newFace.v2 = coordsLen - numVerts;
                    newFace.v3 = coordsLen;
                    this.faces.Add(newFace);

                    newFace.v1 = coordsLen + numVerts - 1;
                    newFace.v2 = coordsLen - 1;
                    newFace.v3 = coordsLen;
                    this.faces.Add(newFace);
                }

                // calc the step for the next iteration of the loop

                if (step < steps)
                {
                    step += 1;
                    percentOfPath += percentOfPathMultiplier;
                    xOffset += xOffsetStepIncrement;
                    yOffset += yOffsetStepIncrement;
                    zOffset += stepSize;
                    if (percentOfPath > this.pathCutEnd)
                        done = true;
                }
                else done = true;
            }
        }

        public void ExtrudeCircular()
        {
            this.coords = new List<Coord>();
            this.faces = new List<MeshmerizerFace>();

            int step = 0;
            int steps = 24;

            float twistBegin = this.twistBegin / 360.0f * twoPi;
            float twistEnd = this.twistEnd / 360.0f * twoPi;
            float twistTotal = twistEnd - twistBegin;

            // if the profile has a lot of twist, add more layers otherwise the layers may overlap
            // and the resulting mesh may be quite inaccurate. This method is arbitrary and doesn't
            // accurately match the viewer
            float twistTotalAbs = Math.Abs(twistTotal);
            if (twistTotalAbs > 0.01f)
            {
                if (twistTotalAbs > Math.PI * 1.5f)
                    steps *= 2;
                if (twistTotalAbs > Math.PI * 3.0f)
                    steps *= 2;
            }

            float yPathScale = this.holeSizeY * 0.5f;
            float pathLength = this.pathCutEnd - this.pathCutBegin;
            float totalSkew = this.skew * 2.0f * pathLength;
            float skewStart = this.pathCutBegin * 2.0f * this.skew - this.skew;
            float xOffsetTopShearXFactor = this.topShearX * (0.25f + 0.5f * (0.5f - this.holeSizeY));
            float yShearCompensation = 1.0f + Math.Abs(this.topShearY) * 0.25f;

            // It's not quite clear what pushY (Y top shear) does, but subtracting it from the start and end
            // angles appears to approximate it's effects on path cut. Likewise, adding it to the angle used
            // to calculate the sine for generating the path radius appears to approximate it's effects there
            // too, but there are some subtle differences in the radius which are noticeable as the prim size
            // increases and it may affect megaprims quite a bit. The effect of the Y top shear parameter on
            // the meshes generated with this technique appear nearly identical in shape to the same prims when
            // displayed by the viewer.

            float startAngle = (twoPi * this.pathCutBegin * this.revolutions) - this.topShearY * 0.9f;
            float endAngle = (twoPi * this.pathCutEnd * this.revolutions) - this.topShearY * 0.9f;
            float stepSize = twoPi / this.stepsPerRevolution;

            step = (int)(startAngle / stepSize);
            int firstStep = step;
            float angle = startAngle;
            float hollow = this.hollow;

            // sanity checks
            float initialProfileRot = 0.0f;
            if (this.sides == 3)
            {
                initialProfileRot = (float)Math.PI;
                if (this.hollowSides == 4)
                {
                    if (hollow > 0.7f)
                        hollow = 0.7f;
                    hollow *= 0.707f;
                }
                else hollow *= 0.5f;
            }
            else if (this.sides == 4)
            {
                initialProfileRot = 0.25f * (float)Math.PI;
                if (this.hollowSides != 4)
                    hollow *= 0.707f;
            }
            else if (this.sides > 4)
            {
                initialProfileRot = (float)Math.PI;
                if (this.hollowSides == 4)
                {
                    if (hollow > 0.7f)
                        hollow = 0.7f;
                    hollow /= 0.7f;
                }
            }

            bool needEndFaces = false;
            if (this.pathCutBegin != 0.0 || this.pathCutEnd != 1.0)
                needEndFaces = true;
            else if (this.taperX != 0.0 || this.taperY != 0.0)
                needEndFaces = true;
            else if (this.skew != 0.0)
                needEndFaces = true;
            else if (twistTotal != 0.0)
                needEndFaces = true;

            MeshmerizerProfile profile = new MeshmerizerProfile(this.sides, this.profileStart, this.profileEnd, hollow, this.hollowSides, needEndFaces);

            if (initialProfileRot != 0.0f)
                profile.AddRot(Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), initialProfileRot));

            bool done = false;
            while (!done) // loop through the length of the path and add the layers
            {
                bool isEndLayer = false;
                if (angle == startAngle || angle >= endAngle)
                    isEndLayer = true;

                MeshmerizerProfile newLayer = profile.Clone(isEndLayer && needEndFaces);

                float xProfileScale = (1.0f - Math.Abs(this.skew)) * this.holeSizeX;
                float yProfileScale = this.holeSizeY;

                float percentOfPath = angle / (twoPi * this.revolutions);
                float percentOfAngles = (angle - startAngle) / (endAngle - startAngle);

                if (this.taperX > 0.01f)
                    xProfileScale *= 1.0f - percentOfPath * this.taperX;
                else if (this.taperX < -0.01f)
                    xProfileScale *= 1.0f + (1.0f - percentOfPath) * this.taperX;

                if (this.taperY > 0.01f)
                    yProfileScale *= 1.0f - percentOfPath * this.taperY;
                else if (this.taperY < -0.01f)
                    yProfileScale *= 1.0f + (1.0f - percentOfPath) * this.taperY;

                if (xProfileScale != 1.0f || yProfileScale != 1.0f)
                    newLayer.Scale(xProfileScale, yProfileScale);

                float radiusScale = 1.0f;
                if (this.radius > 0.001f)
                    radiusScale = 1.0f - this.radius * percentOfPath;
                else if (this.radius < 0.001f)
                    radiusScale = 1.0f + this.radius * (1.0f - percentOfPath);

                float twist = twistBegin + twistTotal * percentOfPath;

                float xOffset = 0.5f * (skewStart + totalSkew * percentOfAngles);
                xOffset += (float)Math.Sin(angle) * xOffsetTopShearXFactor;

                float yOffset = yShearCompensation * (float)Math.Cos(angle) * (0.5f - yPathScale) * radiusScale;

                float zOffset = (float)Math.Sin(angle + this.topShearY) * (0.5f - yPathScale) * radiusScale;

                // next apply twist rotation to the profile layer
                if (twistTotal != 0.0f || twistBegin != 0.0f)
                    newLayer.AddRot(Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), twist));

                // now orient the rotation of the profile layer relative to it's position on the path
                // adding taperY to the angle used to generate the quat appears to approximate the viewer
                //newLayer.AddRot(new Quaternion(new MeshmerizerVertex(1.0f, 0.0f, 0.0f), angle + this.topShearY * 0.9f));
                newLayer.AddRot(Quaternion.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), angle + this.topShearY));
                newLayer.AddPos(xOffset, yOffset, zOffset);

                if (angle == startAngle)
                    newLayer.FlipNormals();

                // append the layer and fill in the sides

                int coordsLen = this.coords.Count;
                newLayer.AddValue2Faces(coordsLen);

                this.coords.AddRange(newLayer.coords);

                if (isEndLayer)
                    this.faces.AddRange(newLayer.faces);

                // fill faces between layers

                int numVerts = newLayer.coords.Count;
                MeshmerizerFace newFace = new MeshmerizerFace();
                if (step > firstStep)
                {
                    for (int i = coordsLen; i < this.coords.Count - 1; i++)
                    {
                        newFace.v1 = i;
                        newFace.v2 = i - numVerts;
                        newFace.v3 = i - numVerts + 1;
                        this.faces.Add(newFace);

                        newFace.v2 = i - numVerts + 1;
                        newFace.v3 = i + 1;
                        this.faces.Add(newFace);
                    }

                    newFace.v1 = coordsLen - 1;
                    newFace.v2 = coordsLen - numVerts;
                    newFace.v3 = coordsLen;
                    this.faces.Add(newFace);

                    newFace.v1 = coordsLen + numVerts - 1;
                    newFace.v2 = coordsLen - 1;
                    newFace.v3 = coordsLen;
                    this.faces.Add(newFace);
                }

                // calculate terms for next iteration
                // calculate the angle for the next iteration of the loop

                if (angle >= endAngle)
                    done = true;
                else
                {
                    step += 1;
                    angle = stepSize * step;
                    if (angle > endAngle)
                        angle = endAngle;
                }
            }
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

        public void Scale(float x, float y, float z)
        {
            int i;
            int numVerts = this.coords.Count;
            Coord vert;

            for (i = 0; i < numVerts; i++)
            {
                vert = this.coords[i];
                vert.X *= x;
                vert.Y *= y;
                vert.Z *= z;
                this.coords[i] = vert;
            }
        }
    }
}
