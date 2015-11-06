/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
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
using OpenMetaverse.StructuredData;

namespace OpenMetaverse
{
    /// <summary>
    /// Particle system specific enumerators, flags and methods.
    /// </summary>
    public partial class Primitive
    {
        #region Subclasses

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
                Beam = 0x200,
                /// <summary>continuous ribbon particle</summary>
                Ribbon = 0x400,
                /// <summary>particle data contains glow</summary>
                DataGlow = 0x10000,
                /// <summary>particle data contains blend functions</summary>
                DataBlend = 0x20000,
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

            public enum BlendFunc : byte
            {
                One = 0,
                Zero = 1,
                DestColor = 2,
                SourceColor = 3,
                OneMinusDestColor = 4,
                OneMinusSourceColor = 5,
                DestAlpha = 6,
                SourceAlpha = 7,
                OneMinusDestAlpha = 8,
                OneMinusSourceAlpha = 9,
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
            /// <summary>A <see cref="T:Vector3"/> which represents the velocity (speed) from the source which particles are emitted</summary>
            public Vector3 AngularVelocity;
            /// <summary>A <see cref="T:Vector3"/> which represents the Acceleration from the source which particles are emitted</summary>
            public Vector3 PartAcceleration;
            /// <summary>The <see cref="T:UUID"/> Key of the texture displayed on the particle</summary>
            public UUID Texture;
            /// <summary>The <see cref="T:UUID"/> Key of the specified target object or avatar particles will follow</summary>
            public UUID Target;
            /// <summary>Flags of particle from <seealso cref="T:ParticleDataFlags"/></summary>
            public ParticleDataFlags PartDataFlags;
            /// <summary>Max Age particle system will emit particles for</summary>
            public float PartMaxAge;
            /// <summary>The <see cref="T:Color4"/> the particle has at the beginning of its lifecycle</summary>
            public Color4 PartStartColor;
            /// <summary>The <see cref="T:Color4"/> the particle has at the ending of its lifecycle</summary>
            public Color4 PartEndColor;
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

            /// <summary>A <see langword="float"/> that represents the start glow value</summary>
            /// <remarks>Minimum value is 0, maximum value is 1</remarks>
            public float PartStartGlow;
            /// <summary>A <see langword="float"/> that represents the end glow value</summary>
            /// <remarks>Minimum value is 0, maximum value is 1</remarks>
            public float PartEndGlow;

            /// <summary>OpenGL blend function to use at particle source</summary>
            public byte BlendFuncSource;
            /// <summary>OpenGL blend function to use at particle destination</summary>
            public byte BlendFuncDest;

            public const byte MaxDataBlockSize = 98;
            public const byte LegacyDataBlockSize = 86;
            public const byte SysDataSize = 68;
            public const byte PartDataSize = 18;

            /// <summary>
            /// Can this particle system be packed in a legacy compatible way
            /// </summary>
            /// <returns>True if the particle system doesn't use new particle system features</returns>
            public bool IsLegacyCompatible()
            {
                return !HasGlow() && !HasBlendFunc();
            }

            public bool HasGlow()
            {
                return PartStartGlow > 0f || PartEndGlow > 0f;
            }

            public bool HasBlendFunc()
            {
                return BlendFuncSource != (byte)BlendFunc.SourceAlpha || BlendFuncDest != (byte)BlendFunc.OneMinusSourceAlpha;
            }

            /// <summary>
            /// Decodes a byte[] array into a ParticleSystem Object
            /// </summary>
            /// <param name="data">ParticleSystem object</param>
            /// <param name="pos">Start position for BitPacker</param>
            public ParticleSystem(byte[] data, int pos)
            {
                PartStartGlow = 0f;
                PartEndGlow = 0f;
                BlendFuncSource = (byte)BlendFunc.SourceAlpha;
                BlendFuncDest = (byte)BlendFunc.OneMinusSourceAlpha;

                CRC = PartFlags = 0;
                Pattern = SourcePattern.None;
                MaxAge = StartAge = InnerAngle = OuterAngle = BurstRate = BurstRadius = BurstSpeedMin =
                    BurstSpeedMax = 0.0f;
                BurstPartCount = 0;
                AngularVelocity = PartAcceleration = Vector3.Zero;
                Texture = Target = UUID.Zero;
                PartDataFlags = ParticleDataFlags.None;
                PartMaxAge = 0.0f;
                PartStartColor = PartEndColor = Color4.Black;
                PartStartScaleX = PartStartScaleY = PartEndScaleX = PartEndScaleY = 0.0f;

                int size = data.Length - pos;
                BitPack pack = new BitPack(data, pos);

                if (size == LegacyDataBlockSize)
                {
                    UnpackSystem(ref pack);
                    UnpackLegacyData(ref pack);
                }
                else if (size > LegacyDataBlockSize && size <= MaxDataBlockSize)
                {
                    int sysSize = pack.UnpackBits(32);
                    if (sysSize != SysDataSize) return; // unkown particle system data size
                    UnpackSystem(ref pack);
                    int dataSize = pack.UnpackBits(32);
                    UnpackLegacyData(ref pack);


                    if ((PartDataFlags & ParticleDataFlags.DataGlow) == ParticleDataFlags.DataGlow)
                    {
                        if (pack.Data.Length - pack.BytePos < 2) return;
                        uint glow = pack.UnpackUBits(8);
                        PartStartGlow = glow / 255f;
                        glow = pack.UnpackUBits(8);
                        PartEndGlow = glow / 255f;
                    }

                    if ((PartDataFlags & ParticleDataFlags.DataBlend) == ParticleDataFlags.DataBlend)
                    {
                        if (pack.Data.Length - pack.BytePos < 2) return;
                        BlendFuncSource = (byte)pack.UnpackUBits(8);
                        BlendFuncDest = (byte)pack.UnpackUBits(8);
                    }

                }
            }

            void UnpackSystem(ref BitPack pack)
            {
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
                AngularVelocity = new Vector3(x, y, z);
                x = pack.UnpackFixed(true, 8, 7);
                y = pack.UnpackFixed(true, 8, 7);
                z = pack.UnpackFixed(true, 8, 7);
                PartAcceleration = new Vector3(x, y, z);
                Texture = pack.UnpackUUID();
                Target = pack.UnpackUUID();
            }

            void UnpackLegacyData(ref BitPack pack)
            {
                PartDataFlags = (ParticleDataFlags)pack.UnpackUBits(32);
                PartMaxAge = pack.UnpackFixed(false, 8, 8);
                byte r = pack.UnpackByte();
                byte g = pack.UnpackByte();
                byte b = pack.UnpackByte();
                byte a = pack.UnpackByte();
                PartStartColor = new Color4(r, g, b, a);
                r = pack.UnpackByte();
                g = pack.UnpackByte();
                b = pack.UnpackByte();
                a = pack.UnpackByte();
                PartEndColor = new Color4(r, g, b, a);
                PartStartScaleX = pack.UnpackFixed(false, 3, 5);
                PartStartScaleY = pack.UnpackFixed(false, 3, 5);
                PartEndScaleX = pack.UnpackFixed(false, 3, 5);
                PartEndScaleY = pack.UnpackFixed(false, 3, 5);
            }

            /// <summary>
            /// Generate byte[] array from particle data
            /// </summary>
            /// <returns>Byte array</returns>
            public byte[] GetBytes()
            {
                int size = LegacyDataBlockSize;
                if (!IsLegacyCompatible()) size += 8; // two new ints for size
                if (HasGlow()) size += 2; // two bytes for start and end glow
                if (HasBlendFunc()) size += 2; // two bytes for start end end blend function

                byte[] bytes = new byte[size];
                BitPack pack = new BitPack(bytes, 0);

                if (IsLegacyCompatible())
                {
                    PackSystemBytes(ref pack);
                    PackLegacyData(ref pack);
                }
                else
                {
                    if (HasGlow()) PartDataFlags |= ParticleDataFlags.DataGlow;
                    if (HasBlendFunc()) PartDataFlags |= ParticleDataFlags.DataBlend;

                    pack.PackBits(SysDataSize, 32);
                    PackSystemBytes(ref pack);
                    int partSize = PartDataSize;
                    if (HasGlow()) partSize += 2; // two bytes for start and end glow
                    if (HasBlendFunc()) partSize += 2; // two bytes for start end end blend function
                    pack.PackBits(partSize, 32);
                    PackLegacyData(ref pack);

                    if (HasGlow())
                    {
                        pack.PackBits((byte)(PartStartGlow * 255f), 8);
                        pack.PackBits((byte)(PartEndGlow * 255f), 8);
                    }

                    if (HasBlendFunc())
                    {
                        pack.PackBits(BlendFuncSource, 8);
                        pack.PackBits(BlendFuncDest, 8);
                    }
                }

                return bytes;
            }

            void PackSystemBytes(ref BitPack pack)
            {
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
            }

            void PackLegacyData(ref BitPack pack)
            {
                pack.PackBits((uint)PartDataFlags, 32);
                pack.PackFixed(PartMaxAge, false, 8, 8);
                pack.PackColor(PartStartColor);
                pack.PackColor(PartEndColor);
                pack.PackFixed(PartStartScaleX, false, 3, 5);
                pack.PackFixed(PartStartScaleY, false, 3, 5);
                pack.PackFixed(PartEndScaleX, false, 3, 5);
                pack.PackFixed(PartEndScaleY, false, 3, 5);
            }

            public OSD GetOSD()
            {
                OSDMap map = new OSDMap();

                map["crc"] = OSD.FromInteger(CRC);
                map["part_flags"] = OSD.FromInteger(PartFlags);
                map["pattern"] = OSD.FromInteger((int)Pattern);
                map["max_age"] = OSD.FromReal(MaxAge);
                map["start_age"] = OSD.FromReal(StartAge);
                map["inner_angle"] = OSD.FromReal(InnerAngle);
                map["outer_angle"] = OSD.FromReal(OuterAngle);
                map["burst_rate"] = OSD.FromReal(BurstRate);
                map["burst_radius"] = OSD.FromReal(BurstRadius);
                map["burst_speed_min"] = OSD.FromReal(BurstSpeedMin);
                map["burst_speed_max"] = OSD.FromReal(BurstSpeedMax);
                map["burst_part_count"] = OSD.FromInteger(BurstPartCount);
                map["ang_velocity"] = OSD.FromVector3(AngularVelocity);
                map["part_acceleration"] = OSD.FromVector3(PartAcceleration);
                map["texture"] = OSD.FromUUID(Texture);
                map["target"] = OSD.FromUUID(Target);

                map["part_data_flags"] = (uint)PartDataFlags;
                map["part_max_age"] = PartMaxAge;
                map["part_start_color"] = PartStartColor;
                map["part_end_color"] = PartEndColor;
                map["part_start_scale"] = new Vector3(PartStartScaleX, PartStartScaleY, 0f);
                map["part_end_scale"] = new Vector3(PartEndScaleX, PartEndScaleY, 0f);

                if (HasGlow())
                {
                    map["part_start_glow"] = PartStartGlow;
                    map["part_end_glow"] = PartEndGlow;
                }

                if (HasBlendFunc())
                {
                    map["blendfunc_source"] = BlendFuncSource;
                    map["blendfunc_dest"] = BlendFuncDest;
                }

                return map;
            }

            public static ParticleSystem FromOSD(OSD osd)
            {
                ParticleSystem partSys = new ParticleSystem();
                OSDMap map = osd as OSDMap;

                if (map != null)
                {
                    partSys.CRC = map["crc"].AsUInteger();
                    partSys.PartFlags = map["part_flags"].AsUInteger();
                    partSys.Pattern = (SourcePattern)map["pattern"].AsInteger();
                    partSys.MaxAge = (float)map["max_age"].AsReal();
                    partSys.StartAge = (float)map["start_age"].AsReal();
                    partSys.InnerAngle = (float)map["inner_angle"].AsReal();
                    partSys.OuterAngle = (float)map["outer_angle"].AsReal();
                    partSys.BurstRate = (float)map["burst_rate"].AsReal();
                    partSys.BurstRadius = (float)map["burst_radius"].AsReal();
                    partSys.BurstSpeedMin = (float)map["burst_speed_min"].AsReal();
                    partSys.BurstSpeedMax = (float)map["burst_speed_max"].AsReal();
                    partSys.BurstPartCount = (byte)map["burst_part_count"].AsInteger();
                    partSys.AngularVelocity = map["ang_velocity"].AsVector3();
                    partSys.PartAcceleration = map["part_acceleration"].AsVector3();
                    partSys.Texture = map["texture"].AsUUID();
                    partSys.Target = map["target"].AsUUID();

                    partSys.PartDataFlags = (ParticleDataFlags)map["part_data_flags"].AsUInteger();
                    partSys.PartMaxAge = map["part_max_age"];
                    partSys.PartStartColor = map["part_start_color"];
                    partSys.PartEndColor = map["part_end_color"];
                    Vector3 ss = map["part_start_scale"];
                    partSys.PartStartScaleX = ss.X;
                    partSys.PartStartScaleY = ss.Y;
                    Vector3 es = map["part_end_scale"];
                    partSys.PartEndScaleX = es.X;
                    partSys.PartEndScaleY = es.Y;

                    if (map.ContainsKey("part_start_glow"))
                    {
                        partSys.PartStartGlow = map["part_start_glow"];
                        partSys.PartEndGlow = map["part_end_glow"];
                    }

                    if (map.ContainsKey("blendfunc_source"))
                    {
                        partSys.BlendFuncSource = (byte)map["blendfunc_source"].AsUInteger();
                        partSys.BlendFuncDest = (byte)map["blendfunc_dest"].AsUInteger();
                    }
                }

                return partSys;
            }
        }

        #endregion Subclasses

        #region Public Members

        /// <summary></summary>
        public ParticleSystem ParticleSys;

        #endregion Public Members
    }
}
