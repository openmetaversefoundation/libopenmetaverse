/*
 * Copyright (c) 2007-2008, the libsecondlife development team
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
using System.Collections.Generic;
using System.ComponentModel;

namespace libsecondlife
{
    /// <summary>
    /// Particle system specific enumerators, flags and methods.
    /// </summary>
    public partial class Primitive
    {
        /// <summary>
        /// Complete structure for the particle system
        /// </summary>
        public struct ParticleSystem
        {
            /// <summary>
            /// Particle source pattern
            /// </summary>
            public enum SourcePattern : byte
            {
                /// <summary>None</summary>
                None = 0,
                /// <summary>Drop particles from source position with no force</summary>
                Drop = 0x01,
                /// <summary>"Explode" particles in all directions</summary>
                Explode = 0x02,
                /// <summary>Particles shoot across a 2D area</summary>
                Angle = 0x04,
                /// <summary>Particles shoot across a 3D Cone</summary>
                AngleCone = 0x08,
                /// <summary>Inverse of AngleCone (shoot particles everywhere except the 3D cone defined</summary>
                AngleConeEmpty = 0x10
            }

            /// <summary>
            /// Particle Data Flags
            /// </summary>
            [Flags]
            public enum ParticleDataFlags : uint
            {
                /// <summary>None</summary>
                None = 0,
                /// <summary>Interpolate color and alpha from start to end</summary>
                InterpColor = 0x001,
                /// <summary>Interpolate scale from start to end</summary>
                InterpScale = 0x002,
                /// <summary>Bounce particles off particle sources Z height</summary>
                Bounce = 0x004,
                /// <summary>velocity of particles is dampened toward the simulators wind</summary>
                Wind = 0x008,
                /// <summary>Particles follow the source</summary>
                FollowSrc = 0x010,
                /// <summary>Particles point towards the direction of source's velocity</summary>
                FollowVelocity = 0x020,
                /// <summary>Target of the particles</summary>
                TargetPos = 0x040,
                /// <summary>Particles are sent in a straight line</summary>
                TargetLinear = 0x080,
                /// <summary>Particles emit a glow</summary>
                Emissive = 0x100,
                /// <summary>used for point/grab/touch</summary>
                Beam = 0x200
            }

            /// <summary>
            /// Particle Flags Enum
            /// </summary>
            [Flags]
            public enum ParticleFlags : uint
            {
                /// <summary>None</summary>
                None = 0,
                /// <summary>Acceleration and velocity for particles are
                /// relative to the object rotation</summary>
                ObjectRelative = 0x01,
                /// <summary>Particles use new 'correct' angle parameters</summary>
                UseNewAngle = 0x02
            }


            public uint CRC;
            /// <summary>Particle Flags</summary>
            /// <remarks>There appears to be more data packed in to this area
            /// for many particle systems. It doesn't appear to be flag values
            /// and serialization breaks unless there is a flag for every
            /// possible bit so it is left as an unsigned integer</remarks>
            public uint PartFlags;
            /// <summary><seealso cref="T:SourcePattern"/> pattern of particles</summary>
            public SourcePattern Pattern;
            /// <summary>A <see langword="float"/> representing the maximimum age (in seconds) particle will be displayed</summary>
            /// <remarks>Maximum value is 30 seconds</remarks>
            public float MaxAge;
            /// <summary>A <see langword="float"/> representing the number of seconds, 
            /// from when the particle source comes into view, 
            /// or the particle system's creation, that the object will emits particles; 
            /// after this time period no more particles are emitted</summary>
            public float StartAge;
            /// <summary>A <see langword="float"/> in radians that specifies where particles will not be created</summary>
            public float InnerAngle;
            /// <summary>A <see langword="float"/> in radians that specifies where particles will be created</summary>
            public float OuterAngle;
            /// <summary>A <see langword="float"/> representing the number of seconds between burts.</summary>
            public float BurstRate;
            /// <summary>A <see langword="float"/> representing the number of meters
            /// around the center of the source where particles will be created.</summary>
            public float BurstRadius;
            /// <summary>A <see langword="float"/> representing in seconds, the minimum speed between bursts of new particles 
            /// being emitted</summary>
            public float BurstSpeedMin;
            /// <summary>A <see langword="float"/> representing in seconds the maximum speed of new particles being emitted.</summary>
            public float BurstSpeedMax;
            /// <summary>A <see langword="byte"/> representing the maximum number of particles emitted per burst</summary>
            public byte BurstPartCount;
            /// <summary>A <see cref="T:LLVector3"/> which represents the velocity (speed) from the source which particles are emitted</summary>
            public LLVector3 AngularVelocity;
            /// <summary>A <see cref="T:LLVector3"/> which represents the Acceleration from the source which particles are emitted</summary>
            public LLVector3 PartAcceleration;
            /// <summary>The <see cref="T:LLUUID"/> Key of the texture displayed on the particle</summary>
            public LLUUID Texture;
            /// <summary>The <see cref="T:LLUUID"/> Key of the specified target object or avatar particles will follow</summary>
            public LLUUID Target;
            /// <summary>Flags of particle from <seealso cref="T:ParticleDataFlags"/></summary>
            public ParticleDataFlags PartDataFlags;
            /// <summary>Max Age particle system will emit particles for</summary>
            public float PartMaxAge;
            /// <summary>The <see cref="T:LLColor"/> the particle has at the beginning of its lifecycle</summary>
            public LLColor PartStartColor;
            /// <summary>The <see cref="T:LLColor"/> the particle has at the ending of its lifecycle</summary>
            public LLColor PartEndColor;
            /// <summary>A <see langword="float"/> that represents the starting X size of the particle</summary>
            /// <remarks>Minimum value is 0, maximum value is 4</remarks>
            public float PartStartScaleX;
            /// <summary>A <see langword="float"/> that represents the starting Y size of the particle</summary>
            /// <remarks>Minimum value is 0, maximum value is 4</remarks>
            public float PartStartScaleY;
            /// <summary>A <see langword="float"/> that represents the ending X size of the particle</summary>
            /// <remarks>Minimum value is 0, maximum value is 4</remarks>
            public float PartEndScaleX;
            /// <summary>A <see langword="float"/> that represents the ending Y size of the particle</summary>
            /// <remarks>Minimum value is 0, maximum value is 4</remarks>
            public float PartEndScaleY;

            /// <summary>
            /// Decodes a byte[] array into a ParticleSystem Object
            /// </summary>
            /// <param name="data">ParticleSystem object</param>
            /// <param name="pos">Start position for BitPacker</param>
            public ParticleSystem(byte[] data, int pos)
            {
                // TODO: Not sure exactly how many bytes we need here, so partial 
                // (truncated) data will cause an exception to be thrown
                if (data.Length > 0)
                {
                    BitPack pack = new BitPack(data, pos);

                    CRC = pack.UnpackUBits(32);
                    PartFlags = pack.UnpackUBits(32);
                    Pattern = (SourcePattern)pack.UnpackByte();
                    MaxAge = pack.UnpackFixed(false, 8, 8);
                    StartAge = pack.UnpackFixed(false, 8, 8);
                    InnerAngle = pack.UnpackFixed(false, 3, 5);
                    OuterAngle = pack.UnpackFixed(false, 3, 5);
                    BurstRate = pack.UnpackFixed(false, 8, 8);
                    BurstRadius = pack.UnpackFixed(false, 8, 8);
                    BurstSpeedMin = pack.UnpackFixed(false, 8, 8);
                    BurstSpeedMax = pack.UnpackFixed(false, 8, 8);
                    BurstPartCount = pack.UnpackByte();
                    float x = pack.UnpackFixed(true, 8, 7);
                    float y = pack.UnpackFixed(true, 8, 7);
                    float z = pack.UnpackFixed(true, 8, 7);
                    AngularVelocity = new LLVector3(x, y, z);
                    x = pack.UnpackFixed(true, 8, 7);
                    y = pack.UnpackFixed(true, 8, 7);
                    z = pack.UnpackFixed(true, 8, 7);
                    PartAcceleration = new LLVector3(x, y, z);
                    Texture = pack.UnpackUUID();
                    Target = pack.UnpackUUID();

                    PartDataFlags = (ParticleDataFlags)pack.UnpackUBits(32);
                    PartMaxAge = pack.UnpackFixed(false, 8, 8);
                    byte r = pack.UnpackByte();
                    byte g = pack.UnpackByte();
                    byte b = pack.UnpackByte();
                    byte a = pack.UnpackByte();
                    PartStartColor = new LLColor(r, g, b, a);
                    r = pack.UnpackByte();
                    g = pack.UnpackByte();
                    b = pack.UnpackByte();
                    a = pack.UnpackByte();
                    PartEndColor = new LLColor(r, g, b, a);
                    PartStartScaleX = pack.UnpackFixed(false, 3, 5);
                    PartStartScaleY = pack.UnpackFixed(false, 3, 5);
                    PartEndScaleX = pack.UnpackFixed(false, 3, 5);
                    PartEndScaleY = pack.UnpackFixed(false, 3, 5);
                }
                else
                {
                    CRC = PartFlags = 0;
                    Pattern = SourcePattern.None;
                    MaxAge = StartAge = InnerAngle = OuterAngle = BurstRate = BurstRadius = BurstSpeedMin =
                        BurstSpeedMax = 0.0f;
                    BurstPartCount = 0;
                    AngularVelocity = PartAcceleration = LLVector3.Zero;
                    Texture = Target = LLUUID.Zero;
                    PartDataFlags = ParticleDataFlags.None;
                    PartMaxAge = 0.0f;
                    PartStartColor = PartEndColor = LLColor.Black;
                    PartStartScaleX = PartStartScaleY = PartEndScaleX = PartEndScaleY = 0.0f;
                }
            }

            /// <summary>
            /// Generate byte[] array from particle data
            /// </summary>
            /// <returns>Byte array</returns>
            public byte[] GetBytes()
            {
                byte[] bytes = new byte[86];
                BitPack pack = new BitPack(bytes, 0);

                pack.PackBits(CRC, 32);
                pack.PackBits((uint)PartFlags, 32);
                pack.PackBits((uint)Pattern, 8);
                pack.PackFixed(MaxAge, false, 8, 8);
                pack.PackFixed(StartAge, false, 8, 8);
                pack.PackFixed(InnerAngle, false, 3, 5);
                pack.PackFixed(OuterAngle, false, 3, 5);
                pack.PackFixed(BurstRate, false, 8, 8);
                pack.PackFixed(BurstRadius, false, 8, 8);
                pack.PackFixed(BurstSpeedMin, false, 8, 8);
                pack.PackFixed(BurstSpeedMax, false, 8, 8);
                pack.PackBits(BurstPartCount, 8);
                pack.PackFixed(AngularVelocity.X, true, 8, 7);
                pack.PackFixed(AngularVelocity.Y, true, 8, 7);
                pack.PackFixed(AngularVelocity.Z, true, 8, 7);
                pack.PackFixed(PartAcceleration.X, true, 8, 7);
                pack.PackFixed(PartAcceleration.Y, true, 8, 7);
                pack.PackFixed(PartAcceleration.Z, true, 8, 7);
                pack.PackUUID(Texture);
                pack.PackUUID(Target);

                pack.PackBits((uint)PartDataFlags, 32);
                pack.PackFixed(PartMaxAge, false, 8, 8);
                pack.PackColor(PartStartColor);
                pack.PackColor(PartEndColor);
                pack.PackFixed(PartStartScaleX, false, 3, 5);
                pack.PackFixed(PartStartScaleY, false, 3, 5);
                pack.PackFixed(PartEndScaleX, false, 3, 5);
                pack.PackFixed(PartEndScaleY, false, 3, 5);

                return bytes;
            }
        }
    }
}
