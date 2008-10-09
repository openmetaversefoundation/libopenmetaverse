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

namespace OpenMetaverse.Rendering
{
    class Extruder
    {
        public float taperTopFactorX = 1f;
        public float taperTopFactorY = 1f;
        public float taperBotFactorX = 1f;
        public float taperBotFactorY = 1f;

        public float pushX = 0f;
        public float pushY = 0f;

        // twist amount in radians.  NOT DEGREES.
        public float twistTop = 0;
        public float twistBot = 0;
        public float twistMid = 0;
        public float pathScaleX = 1.0f;
        public float pathScaleY = 0.5f;
        public float skew = 0.0f;
        public float radius = 0.0f;
        public float revolutions = 1.0f;

        public float pathCutBegin = 0.0f;
        public float pathCutEnd = 1.0f;

        public ushort pathBegin = 0;
        public ushort pathEnd = 0;

        public float pathTaperX = 0.0f;
        public float pathTaperY = 0.0f;

        /// <summary>
        /// Creates an extrusion of a profile along a linear path. Used to create prim types box, cylinder, and prism.
        /// </summary>
        /// <param name="m"></param>
        /// <returns>A mesh of the extruded shape</returns>
        public MeshmerizerMesh ExtrudeLinearPath(MeshmerizerMesh m)
        {
            MeshmerizerMesh result = new MeshmerizerMesh();

            MeshmerizerMesh newLayer;
            MeshmerizerMesh lastLayer = null;

            int step = 0;
            int steps = 1;

            float twistTotal = twistTop - twistBot;
            // if the profile has a lot of twist, add more layers otherwise the layers may overlap
            // and the resulting mesh may be quite inaccurate. This method is arbitrary and may not
            // accurately match the viewer
            float twistTotalAbs = System.Math.Abs(twistTotal);
            if (twistTotalAbs > 0.01)
                steps += (int)(twistTotalAbs * 3.66f); // dahlia's magic number ;)

            double percentOfPathMultiplier = 1.0 / steps;

            float start = -0.5f;

            float stepSize = 1.0f / (float)steps;

            float xProfileScale = 1.0f;
            float yProfileScale = 1.0f;

            float xOffset = 0.0f;
            float yOffset = 0.0f;
            float zOffset = start;

            float xOffsetStepIncrement = pushX / steps;
            float yOffsetStepIncrement = pushY / steps;

            //float percentOfPath = 0.0f;
            float percentOfPath = (float)pathBegin * 2.0e-5f;
            zOffset += percentOfPath;
            bool done = false;
            do // loop through the length of the path and add the layers
            {
                newLayer = m.Clone();

                if (taperBotFactorX < 1.0f)
                    xProfileScale = 1.0f - (1.0f - percentOfPath) * (1.0f - taperBotFactorX);
                else if (taperTopFactorX < 1.0f)
                    xProfileScale = 1.0f - percentOfPath * (1.0f - taperTopFactorX);
                else xProfileScale = 1.0f;

                if (taperBotFactorY < 1.0f)
                    yProfileScale = 1.0f - (1.0f - percentOfPath) * (1.0f - taperBotFactorY);
                else if (taperTopFactorY < 1.0f)
                    yProfileScale = 1.0f - percentOfPath * (1.0f - taperTopFactorY);
                else yProfileScale = 1.0f;

                MeshmerizerVertex vTemp = new MeshmerizerVertex(0.0f, 0.0f, 0.0f);

                // apply the taper to the profile before any rotations
                if (xProfileScale != 1.0f || yProfileScale != 1.0f)
                {
                    foreach (MeshmerizerVertex v in newLayer.vertices)
                    {
                        if (v != null)
                        {
                            v.X *= xProfileScale;
                            v.Y *= yProfileScale;
                        }
                    }
                }


                float twist = twistBot + (twistTotal * (float)percentOfPath);
                // apply twist rotation to the profile layer and position the layer in the prim

                Quaternion profileRot = Quaternion.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), twist);
                foreach (MeshmerizerVertex v in newLayer.vertices)
                {
                    if (v != null)
                    {
                        vTemp = v * profileRot;
                        v.X = vTemp.X + xOffset;
                        v.Y = vTemp.Y + yOffset;
                        v.Z = vTemp.Z + zOffset;
                    }
                }

                if (step == 0) // the first layer, invert normals
                {
                    foreach (Triangle t in newLayer.triangles)
                    {
                        t.invertNormal();
                    }
                }

                result.Append(newLayer);

                int iLastNull = 0;

                if (lastLayer != null)
                {
                    int i, count = newLayer.vertices.Count;

                    for (i = 0; i < count; i++)
                    {
                        int iNext = (i + 1);

                        if (lastLayer.vertices[i] == null) // cant make a simplex here
                        {
                            iLastNull = i + 1;
                        }
                        else
                        {
                            if (i == count - 1) // End of list
                                iNext = iLastNull;

                            if (lastLayer.vertices[iNext] == null) // Null means wrap to begin of last segment
                                iNext = iLastNull;

                            result.Add(new Triangle(newLayer.vertices[i], lastLayer.vertices[i], newLayer.vertices[iNext]));
                            result.Add(new Triangle(newLayer.vertices[iNext], lastLayer.vertices[i], lastLayer.vertices[iNext]));
                        }
                    }
                }
                lastLayer = newLayer;

                // calc the step for the next interation of the loop

                if (step < steps)
                {
                    step++;
                    percentOfPath += (float)percentOfPathMultiplier;

                    xOffset += xOffsetStepIncrement;
                    yOffset += yOffsetStepIncrement;
                    zOffset += stepSize;

                    if (percentOfPath > 1.0f - (float)pathEnd * 2.0e-5f)
                        done = true;
                }
                else done = true;

            } while (!done); // loop until all the layers in the path are completed

            return result;
        }

        /// <summary>
        /// Extrudes a shape around a circular path. Used to create prim types torus, ring, and tube.
        /// </summary>
        /// <param name="m"></param>
        /// <returns>a mesh of the extruded shape</returns>
        public MeshmerizerMesh ExtrudeCircularPath(MeshmerizerMesh m)
        {
            MeshmerizerMesh result = new MeshmerizerMesh();

            MeshmerizerMesh newLayer;
            MeshmerizerMesh lastLayer = null;

            int step;
            int steps = 24;

            float twistTotal = twistTop - twistBot;
            // if the profile has a lot of twist, add more layers otherwise the layers may overlap
            // and the resulting mesh may be quite inaccurate. This method is arbitrary and doesn't
            // accurately match the viewer
            if (System.Math.Abs(twistTotal) > (float)System.Math.PI * 1.5f) steps *= 2;
            if (System.Math.Abs(twistTotal) > (float)System.Math.PI * 3.0f) steps *= 2;

            // double percentOfPathMultiplier = 1.0 / steps;
            // double angleStepMultiplier = System.Math.PI * 2.0 / steps;

            float yPathScale = pathScaleY * 0.5f;
            float pathLength = pathCutEnd - pathCutBegin;
            float totalSkew = skew * 2.0f * pathLength;
            float skewStart = (-skew) + pathCutBegin * 2.0f * skew;

            // It's not quite clear what pushY (Y top shear) does, but subtracting it from the start and end
            // angles appears to approximate it's effects on path cut. Likewise, adding it to the angle used
            // to calculate the sine for generating the path radius appears to approximate it's effects there
            // too, but there are some subtle differences in the radius which are noticeable as the prim size
            // increases and it may affect megaprims quite a bit. The effect of the Y top shear parameter on
            // the meshes generated with this technique appear nearly identical in shape to the same prims when
            // displayed by the viewer.


            float startAngle = (float)(System.Math.PI * 2.0 * pathCutBegin * revolutions) - pushY * 0.9f;
            float endAngle = (float)(System.Math.PI * 2.0 * pathCutEnd * revolutions) - pushY * 0.9f;
            float stepSize = (float)0.2617993878; // 2*PI / 24 segments per revolution

            step = (int)(startAngle / stepSize);
            float angle = startAngle;

            float xProfileScale = 1.0f;
            float yProfileScale = 1.0f;

            bool done = false;
            do // loop through the length of the path and add the layers
            {
                newLayer = m.Clone();

                float percentOfPath = (angle - startAngle) / (endAngle - startAngle); // endAngle should always be larger than startAngle

                if (pathTaperX > 0.001f) // can't really compare to 0.0f as the value passed is never exactly zero
                    xProfileScale = 1.0f - percentOfPath * pathTaperX;
                else if (pathTaperX < -0.001f)
                    xProfileScale = 1.0f + (1.0f - percentOfPath) * pathTaperX;
                else xProfileScale = 1.0f;

                if (pathTaperY > 0.001f)
                    yProfileScale = 1.0f - percentOfPath * pathTaperY;
                else if (pathTaperY < -0.001f)
                    yProfileScale = 1.0f + (1.0f - percentOfPath) * pathTaperY;
                else yProfileScale = 1.0f;

                MeshmerizerVertex vTemp = new MeshmerizerVertex(0.0f, 0.0f, 0.0f);

                // apply the taper to the profile before any rotations
                if (xProfileScale != 1.0f || yProfileScale != 1.0f)
                {
                    foreach (MeshmerizerVertex v in newLayer.vertices)
                    {
                        if (v != null)
                        {
                            v.X *= xProfileScale;
                            v.Y *= yProfileScale;
                        }
                    }
                }

                float radiusScale;

                if (radius > 0.001f)
                    radiusScale = 1.0f - radius * percentOfPath;
                else if (radius < 0.001f)
                    radiusScale = 1.0f + radius * (1.0f - percentOfPath);
                else
                    radiusScale = 1.0f;

                float twist = twistBot + (twistTotal * (float)percentOfPath);

                float xOffset;
                float yOffset;
                float zOffset;

                xOffset = 0.5f * (skewStart + totalSkew * (float)percentOfPath);
                xOffset += (float)System.Math.Sin(angle) * pushX * 0.45f;
                yOffset = (float)(System.Math.Cos(angle) * (0.5f - yPathScale)) * radiusScale;
                zOffset = (float)(System.Math.Sin(angle + pushY * 0.9f) * (0.5f - yPathScale)) * radiusScale;

                // next apply twist rotation to the profile layer
                if (twistTotal != 0.0f || twistBot != 0.0f)
                {
                    Quaternion profileRot = new Quaternion(new Vector3(0.0f, 0.0f, 1.0f), twist);
                    foreach (MeshmerizerVertex v in newLayer.vertices)
                    {
                        if (v != null)
                        {
                            vTemp = v * profileRot;
                            v.X = vTemp.X;
                            v.Y = vTemp.Y;
                            v.Z = vTemp.Z;
                        }
                    }
                }

                // now orient the rotation of the profile layer relative to it's position on the path
                // adding pushY to the angle used to generate the quat appears to approximate the viewer
                Quaternion layerRot = Quaternion.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), (float)angle + pushY * 0.9f);
                foreach (MeshmerizerVertex v in newLayer.vertices)
                {
                    if (v != null)
                    {
                        vTemp = v * layerRot;
                        v.X = vTemp.X + xOffset;
                        v.Y = vTemp.Y + yOffset;
                        v.Z = vTemp.Z + zOffset;
                    }
                }

                if (angle == startAngle) // the first layer, invert normals
                {
                    foreach (Triangle t in newLayer.triangles)
                    {
                        t.invertNormal();
                    }
                }

                result.Append(newLayer);

                int iLastNull = 0;

                if (lastLayer != null)
                {
                    int i, count = newLayer.vertices.Count;

                    for (i = 0; i < count; i++)
                    {
                        int iNext = (i + 1);

                        if (lastLayer.vertices[i] == null) // cant make a simplex here
                        {
                            iLastNull = i + 1;
                        }
                        else
                        {
                            if (i == count - 1) // End of list
                                iNext = iLastNull;

                            if (lastLayer.vertices[iNext] == null) // Null means wrap to begin of last segment
                                iNext = iLastNull;

                            result.Add(new Triangle(newLayer.vertices[i], lastLayer.vertices[i], newLayer.vertices[iNext]));
                            result.Add(new Triangle(newLayer.vertices[iNext], lastLayer.vertices[i], lastLayer.vertices[iNext]));
                        }
                    }
                }
                lastLayer = newLayer;

                // calc the angle for the next interation of the loop
                if (angle >= endAngle)
                {
                    done = true;
                }
                else
                {
                    angle = stepSize * ++step;
                    if (angle > endAngle)
                        angle = endAngle;
                }
            } while (!done); // loop until all the layers in the path are completed

            return result;
        }
    }
}
