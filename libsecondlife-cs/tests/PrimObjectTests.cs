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
                float floatValue = PrimObject.PathBeginFloat(i);
                byte result = PrimObject.PathBeginByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathEnd()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = PrimObject.PathEndFloat(i);
                byte result = PrimObject.PathEndByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathRadiusOffset()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = PrimObject.PathRadiusOffsetFloat(i);
                sbyte result = PrimObject.PathRadiusOffsetByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathRevolutions()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = PrimObject.PathRevolutionsFloat(i);
                byte result = PrimObject.PathRevolutionsByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathScale()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = PrimObject.PathScaleFloat(i);
                byte result = PrimObject.PathScaleByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathShear()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = PrimObject.PathShearFloat(i);
                byte result = PrimObject.PathShearByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathSkew()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = PrimObject.PathSkewFloat(i);
                sbyte result = PrimObject.PathSkewByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void PathTaper()
        {
            for (sbyte i = sbyte.MinValue; i < sbyte.MaxValue; i++)
            {
                float floatValue = PrimObject.PathTaperFloat(i);
                sbyte result = PrimObject.PathTaperByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void ProfileBegin()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = PrimObject.ProfileBeginFloat(i);
                byte result = PrimObject.ProfileBeginByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }

        [Test]
        public void ProfileEnd()
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                float floatValue = PrimObject.ProfileEndFloat(i);
                byte result = PrimObject.ProfileEndByte(floatValue);

                Assert.IsTrue(result == i, "Started with " + i + ", float value was " + floatValue +
                ", and ended up with " + result);
            }
        }
    }
}
