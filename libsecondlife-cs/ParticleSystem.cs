using System;
using System.Collections.Generic;
using System.Text;

namespace libsecondlife
{
    /// <summary>
    /// 
    /// </summary>
    public class ParticleSystem
    {
        public uint PartStartRGBA;
        public uint PartEndRGBA;

        public LLVector3 PartStartScale = LLVector3.Zero;
        public LLVector3 PartEndScale = LLVector3.Zero;

        public float PartMaxAge;
        public float SrcMaxAge;

        public LLVector3 SrcAccel = LLVector3.Zero;

        public float SrcAngleBegin;
        public float SrcAngleEnd;

        public int SrcBurstPartCount;
        public float SrcBurstRadius;
        public float SrcBurstRate;
        public float SrcBurstSpeedMin;
        public float SrcBurstSpeedMax;

        public LLVector3 SrcOmega = LLVector3.Zero;

        public LLUUID SrcTargetKey = LLUUID.Zero;
        public LLUUID SrcTexture = LLUUID.Zero;

        public SourcePattern SrcPattern;
        public ParticleFlags PartFlags;

        public uint Version; //???
        public uint StartTick; //???

        public enum SourcePattern : byte
        {
            None = 0,
            Drop = 0x01,
            Explode = 0x02,
            Angle = 0x04,
            AngleCone = 0x08,
            AngleConeEmpty = 0x10
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum ParticleFlags : ushort
        {
            None = 0,
            InterpColor = 0x001,
            InterpScale = 0x002,
            Bounce = 0x004,
            Wind = 0x008,
            FollowSrc = 0x010,
            FollowVelocity = 0x20,
            TargetPos = 0x40,
            TargetLinear = 0x080,
            Emissive = 0x100
        }

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
        /// <param name="data"></param>
        /// <param name="pos"></param>
        private void FromBytes(byte[] data, int pos)
        {
            int i = pos;

            if (data.Length == 0)
                return;

            Version = (uint)(data[i++] + (data[i++] << 8) +
                    (data[i++] << 16) + (data[i++] << 24));

            StartTick = (uint)(data[i++] + (data[i++] << 8) +
                    (data[i++] << 16) + (data[i++] << 24));

            SrcPattern = (SourcePattern)data[i++];

            SrcMaxAge = (data[i++] + (data[i++] << 8)) / 256.0f;

            //Unknown
            i += 2;

            SrcAngleBegin = (data[i++] / 100.0f) * (float)Math.PI;
            SrcAngleEnd = (data[i++] / 100.0f) * (float)Math.PI;

            SrcBurstRate = (data[i++] + (data[i++] << 8)) / 256.0f;
            SrcBurstRadius = (data[i++] + (data[i++] << 8)) / 256.0f;
            SrcBurstSpeedMin = (data[i++] + (data[i++] << 8)) / 256.0f;
            SrcBurstSpeedMax = (data[i++] + (data[i++] << 8)) / 256.0f;
            SrcBurstPartCount = data[i++];

            SrcOmega = new LLVector3();
            SrcOmega.X = (data[i++] + (data[i++] << 8)) / 128.0f - 256.0f;
            SrcOmega.Y = (data[i++] + (data[i++] << 8)) / 128.0f - 256.0f;
            SrcOmega.Z = (data[i++] + (data[i++] << 8)) / 128.0f - 256.0f;

            SrcAccel = new LLVector3();
            SrcAccel.X = (data[i++] + (data[i++] << 8)) / 128.0f - 256.0f;
            SrcAccel.Y = (data[i++] + (data[i++] << 8)) / 128.0f - 256.0f;
            SrcAccel.Z = (data[i++] + (data[i++] << 8)) / 128.0f - 256.0f;

            SrcTexture = new LLUUID(data, i);
            i += 16;
            SrcTargetKey = new LLUUID(data, i);
            i += 16;

            PartFlags = (ParticleFlags)(data[i++] + (data[i++] << 8));

            PartMaxAge = (data[i++] + (data[i++] << 8)) / 256.0f;

            //Unknown
            i += 2;

            PartStartRGBA = (uint)(data[i++] + (data[i++] << 8) +
                    (data[i++] << 16) + (data[i++] << 24));

            PartEndRGBA = (uint)(data[i++] + (data[i++] << 8) +
                    (data[i++] << 16) + (data[i++] << 24));

            PartStartScale = new LLVector3();
            PartStartScale.X = data[i++] / 32.0f;
            PartStartScale.Y = data[i++] / 32.0f;

            PartEndScale = new LLVector3();
            PartEndScale.X = data[i++] / 32.0f;
            PartEndScale.Y = data[i++] / 32.0f;
        }
    }
}
