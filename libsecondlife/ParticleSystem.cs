using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace libsecondlife
{
    public partial class Primitive
    {
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public struct ParticleSystem
        {
            /// <summary>
            /// 
            /// </summary>
            public enum SourcePattern : byte
            {
                /// <summary></summary>
                None = 0,
                /// <summary></summary>
                Drop = 0x01,
                /// <summary></summary>
                Explode = 0x02,
                /// <summary></summary>
                Angle = 0x04,
                /// <summary></summary>
                AngleCone = 0x08,
                /// <summary></summary>
                AngleConeEmpty = 0x10
            }

            /// <summary>
            /// 
            /// </summary>
            [Flags]
            public enum ParticleDataFlags : uint
            {
                /// <summary></summary>
                None = 0,
                /// <summary></summary>
                InterpColor = 0x001,
                /// <summary></summary>
                InterpScale = 0x002,
                /// <summary></summary>
                Bounce = 0x004,
                /// <summary></summary>
                Wind = 0x008,
                /// <summary></summary>
                FollowSrc = 0x010,
                /// <summary></summary>
                FollowVelocity = 0x020,
                /// <summary></summary>
                TargetPos = 0x040,
                /// <summary></summary>
                TargetLinear = 0x080,
                /// <summary></summary>
                Emissive = 0x100,
                /// <summary></summary>
                Beam = 0x200
            }

            /// <summary>
            /// 
            /// </summary>
            [Flags]
            public enum ParticleFlags : uint
            {
                /// <summary></summary>
                None = 0,
                /// <summary>Acceleration and velocity for particles are
                /// relative to the object rotation</summary>
                ObjectRelative = 0x01,
                /// <summary>Particles use new 'correct' angle parameters</summary>
                UseNewAngle = 0x02
            }


            public uint CRC;
            /// <summary></summary>
            /// <remarks>There appears to be more data packed in to this area
            /// for many particle systems. It doesn't appear to be flag values
            /// and serialization breaks unless there is a flag for every
            /// possible bit so it is left as an unsigned integer</remarks>
            public uint PartFlags;
            public SourcePattern Pattern;
            public float MaxAge;
            public float StartAge;
            public float InnerAngle;
            public float OuterAngle;
            public float BurstRate;
            public float BurstRadius;
            public float BurstSpeedMin;
            public float BurstSpeedMax;
            public byte BurstPartCount;
            public LLVector3 AngularVelocity;
            public LLVector3 PartAcceleration;
            public LLUUID Texture;
            public LLUUID Target;
            public ParticleDataFlags PartDataFlags;
            public float PartMaxAge;
            public LLColor PartStartColor;
            public LLColor PartEndColor;
            public float PartStartScaleX;
            public float PartStartScaleY;
            public float PartEndScaleX;
            public float PartEndScaleY;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public ParticleSystem(byte[] data, int pos)
            {
                // TODO: Not sure exactly how many bytes we need here, so partial 
                // (but truncated) data will cause an exception to be thrown
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
            /// 
            /// </summary>
            /// <returns></returns>
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
                pack.PackFixed(MaxAge, false, 8, 8);
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
