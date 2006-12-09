using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace libsecondlife
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
            [XmlEnum("None")] None = 0,
            /// <summary></summary>
            [XmlEnum("Drop")] Drop = 0x01,
            /// <summary></summary>
            [XmlEnum("Explode")] Explode = 0x02,
            /// <summary></summary>
            [XmlEnum("Angle")] Angle = 0x04,
            /// <summary></summary>
            [XmlEnum("AngleCone")] AngleCone = 0x08,
            /// <summary></summary>
            [XmlEnum("AngleConeEmpty")] AngleConeEmpty = 0x10
        }

        /// <summary>
        /// Information on a particle system assocaited with a prim
        /// </summary>
        [Flags]
        public enum ParticleFlags : ushort
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
            Emissive = 0x100
        }

        /// <summary></summary>
        [XmlAttribute("startrgba"), DefaultValue(0)] public uint PartStartRGBA;
        /// <summary></summary>
        [XmlAttribute("endrgba"), DefaultValue(0)] public uint PartEndRGBA;
        /// <summary></summary>
        [XmlAttribute("maxage"), DefaultValue(0)] public float PartMaxAge;
        /// <summary></summary>
        [XmlAttribute("srcmaxage"), DefaultValue(0)] public float SrcMaxAge;
        /// <summary></summary>
        [XmlAttribute("srcanglebegin"), DefaultValue(0)] public float SrcAngleBegin;
        /// <summary></summary>
        [XmlAttribute("srcangleend"), DefaultValue(0)] public float SrcAngleEnd;
        /// <summary></summary>
        [XmlAttribute("srcburstpartcount"), DefaultValue(0)] public int SrcBurstPartCount;
        /// <summary></summary>
        [XmlAttribute("srcburstradius"), DefaultValue(0)] public float SrcBurstRadius;
        /// <summary></summary>
        [XmlAttribute("srcburstrate"), DefaultValue(0)] public float SrcBurstRate;
        /// <summary></summary>
        [XmlAttribute("srcburstspeedmin"), DefaultValue(0)] public float SrcBurstSpeedMin;
        /// <summary></summary>
        [XmlAttribute("srcburstspeedmax"), DefaultValue(0)] public float SrcBurstSpeedMax;
        /// <summary>Unknown</summary>
        [XmlAttribute("serial"), DefaultValue(0)] public uint Serial;
        /// <summary>Unknown</summary>
        [XmlAttribute("starttick"), DefaultValue(0)] public uint StartTick;
        /// <summary></summary>
        [XmlAttribute("srcpattern"), DefaultValue(SourcePattern.None)] public SourcePattern SrcPattern = SourcePattern.None;
        /// <summary>Various options that describe the behavior of this system</summary>
        [XmlAttribute("particleflags"), DefaultValue(ParticleFlags.None)] public ParticleFlags PartFlags = ParticleFlags.None;
        /// <summary></summary>
        [XmlElement("srctarget")] public LLUUID SrcTargetKey = LLUUID.Zero;
        /// <summary>Texture that will be applied to the particles</summary>
        [XmlElement("srctexture")] public LLUUID SrcTexture = LLUUID.Zero;
        /// <summary></summary>
        [XmlElement("partstartscale")] public LLVector3 PartStartScale = LLVector3.Zero;
        /// <summary></summary>
        [XmlElement("partendscale")] public LLVector3 PartEndScale = LLVector3.Zero;
        /// <summary></summary>
        [XmlElement("srcaccel")] public LLVector3 SrcAccel = LLVector3.Zero;
        /// <summary></summary>
        [XmlElement("srcomega")] public LLVector3 SrcOmega = LLVector3.Zero;
        
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
            int i = pos;

            if (data.Length == 0)
                return;

            Serial = (uint)(data[i++] + (data[i++] << 8) +
                    (data[i++] << 16) + (data[i++] << 24));

            StartTick = (uint)(data[i++] + (data[i++] << 8) +
                    (data[i++] << 16) + (data[i++] << 24));

            SrcPattern = (SourcePattern)data[i++];

            SrcMaxAge = (data[i++] + (data[i++] << 8)) / 256.0f;

            // TODO: Unknown
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

            // TODO: Unknown
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
