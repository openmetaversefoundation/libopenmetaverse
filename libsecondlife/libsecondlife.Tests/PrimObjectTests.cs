using System;
using System.Collections.Generic;
using System.Net;
using libsecondlife;
using libsecondlife.Packets;
using NUnit.Framework;

namespace libsecondlife.Tests
{
    [TestFixture]
    public class PrimObjectTests : Assert
    {
        [Test]
        public void PathBegin()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.PathBeginFloat(i);
                byte result = LLObject.PathBeginByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathEnd()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.PathEndFloat(i);
                byte result = LLObject.PathEndByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathRadiusOffset()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = LLObject.PathRadiusOffsetFloat(i);
                sbyte result = LLObject.PathRadiusOffsetByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathRevolutions()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.PathRevolutionsFloat(i);
                byte result = LLObject.PathRevolutionsByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathScale()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.PathScaleFloat(i);
                byte result = LLObject.PathScaleByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathShear()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.PathShearFloat(i);
                byte result = LLObject.PathShearByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathSkew()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = LLObject.PathSkewFloat(i);
                sbyte result = LLObject.PathSkewByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathTaper()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = LLObject.PathTaperFloat(i);
                sbyte result = LLObject.PathTaperByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void ProfileBegin()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.ProfileBeginFloat(i);
                byte result = LLObject.ProfileBeginByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void ProfileEnd()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = LLObject.ProfileEndFloat(i);
                byte result = LLObject.ProfileEndByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }
    }
}
