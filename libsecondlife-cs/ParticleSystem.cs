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
        public class ParticleSystem
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
            /// Flags for the particle system behavior
            /// </summary>
            [Flags]
            public enum ParticleSystemFlags : uint
            {
                /// <summary></summary>
                None = 0,
                /// <summary>Acceleration and velocity for particles are
                /// relative to the object rotation</summary>
                ObjectRelative = 0x01,
                /// <summary>Particles use new 'correct' angle parameters</summary>
                UseNewAngle = 0x02
            }

            /// <summary>
            /// Flags for the particles in this particle system
            /// </summary>
            [Flags]
            public enum ParticleFlags : uint
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
                FollowVelocity = 0x20,
                /// <summary></summary>
                TargetPos = 0x40,
                /// <summary></summary>
                TargetLinear = 0x080,
                /// <summary></summary>
                Emissive = 0x100,
                /// <summary></summary>
                Beam = 0x200
            }


            public uint CRC;
            public SourcePattern Pattern = SourcePattern.None;
            public ParticleSystemFlags Flags = ParticleSystemFlags.None;
            public float MaxAge;
            public float StartAge;
            public float InnerAngle;
            public float OuterAngle;
            public float BurstRate;
            public float BurstRadius;
            public float BurstSpeedMin;
            public float BurstSpeedMax;
            public float BurstPartCount;
            public LLVector3 AngularVelocity;
            public LLVector3 PartAcceleration;
            public LLUUID Texture;
            public LLUUID Target;
            public LLColor PartStartColor;
            public LLColor PartEndColor;
            public float PartStartScaleX;
            public float PartStartScaleY;
            public float PartEndScaleX;
            public float PartEndScaleY;
            public float PartMaxAge;
            public ParticleFlags PartFlags = ParticleFlags.None;
            

            /// <summary>
            /// 
            /// </summary>
            public ParticleSystem()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            public ParticleSystem(byte[] data, int pos)
            {
                FromBytes(data, pos);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public byte[] GetBytes()
            {
                byte[] bytes = new byte[0];
                // FIXME: Finish ParticleSystem.GetBytes()
                return bytes;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="pos"></param>
            private void FromBytes(byte[] data, int pos)
            {
                if (data.Length == 0)
                    return;

                BitPack pack = new BitPack(data, pos);

                CRC = pack.UnpackUBits(32);
                Flags = (ParticleSystemFlags)pack.UnpackUBits(32);
                Pattern = (SourcePattern)pack.UnpackByte();
                MaxAge = pack.UnpackFixed(false, 8, 8);
                StartAge = pack.UnpackFixed(false, 8, 8);
                InnerAngle = pack.UnpackFixed(false, 3, 5);
                OuterAngle = pack.UnpackFixed(false, 3, 5);
                BurstRate = pack.UnpackFixed(false, 8, 8);
                BurstRadius = pack.UnpackFixed(false, 8, 8);
                BurstSpeedMin = pack.UnpackFixed(false, 8, 8);
                BurstSpeedMax = pack.UnpackFixed(false, 8, 8);
                BurstPartCount = (uint)pack.UnpackByte();
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
                PartFlags = (ParticleFlags)pack.UnpackUBits(32);
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
        }
    }
}
