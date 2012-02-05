namespace Ganondorf.UnitTests
{
    using Ganondorf.Exceptions;
    using Ganondorf.UnitTests.Classes;

    using NUnit.Framework;

    [TestFixture]
    public class ConversionTests
    {
        [Test]
        public void ValueTypeTest()
        {
            QueryStringSerialiser<ValueTypeTestClass> s = new QueryStringSerialiser<ValueTypeTestClass>();
            var input = new ValueTypeTestClass();

            var qs = s.Map(input);
            var output = s.Load(qs);

            Assert.AreEqual(input, output);
        }

        [Test]
        public void ValueTypeTestMaxValue()
        {
            QueryStringSerialiser<ValueTypeTestClass> s = new QueryStringSerialiser<ValueTypeTestClass>();
            var input = new ValueTypeTestClass
            {
                TestString = "Test String",
                TestInt = int.MaxValue,
                TestDecimal = decimal.MaxValue,
                TestFloat = float.MaxValue,
                // Interesting fact: double.Parse(double.MaxValue.ToString()) will throw due to double.MaxValue.ToString() being bigger than double.MaxValue.
                TestDouble = double.MaxValue / 10d,
                TestLong = long.MaxValue,
                TestBool = true,
                TestShort = short.MaxValue,
                TestChar = char.MaxValue,
                TestUnsignedInt = uint.MaxValue,
                TestUnsignedLong = ulong.MaxValue,
                TestUnsignedShort = ushort.MaxValue,
                TestByte = byte.MaxValue,
                TestSignedByte = sbyte.MaxValue,
            };

            var qs = s.Map(input);
            var output = s.Load(qs);

            Assert.AreEqual("Test String", output.TestString);
            Assert.AreEqual(int.MaxValue, output.TestInt);
            Assert.AreEqual(decimal.MaxValue, output.TestDecimal);

            // Doubles and Floats of .MaxValue size get rounded when they're ToStringed.
            Assert.AreEqual(float.Parse(float.MaxValue.ToString()), output.TestFloat);
            Assert.AreEqual(double.Parse((double.MaxValue / 10d).ToString()), output.TestDouble);

            Assert.AreEqual(long.MaxValue, output.TestLong);
            Assert.AreEqual(true, output.TestBool);
            Assert.AreEqual(short.MaxValue, output.TestShort);
            Assert.AreEqual(char.MaxValue, output.TestChar);
            Assert.AreEqual(uint.MaxValue, output.TestUnsignedInt);
            Assert.AreEqual(ulong.MaxValue, output.TestUnsignedLong);
            Assert.AreEqual(ushort.MaxValue, output.TestUnsignedShort);
            Assert.AreEqual(byte.MaxValue, output.TestByte);
            Assert.AreEqual(sbyte.MaxValue, output.TestSignedByte);

            // There's no point doing an object comparison, the floating-point types will not be equal.
        }

        [Test]
        public void ValueTypeTestMinValue()
        {
            QueryStringSerialiser<ValueTypeTestClass> s = new QueryStringSerialiser<ValueTypeTestClass>();
            var input = new ValueTypeTestClass
            {
                TestString = "Test String",
                TestInt = int.MinValue,
                TestDecimal = decimal.MinValue,
                TestFloat = float.MinValue,
                // Interesting fact: double.Parse(double.MinValue.ToString()) will throw due to double.MinValue.ToString() being bigger than double.MinValue.
                TestDouble = double.MinValue / 10d,
                TestLong = long.MinValue,
                TestBool = true,
                TestShort = short.MinValue,
                TestChar = char.MinValue,
                TestUnsignedInt = uint.MinValue,
                TestUnsignedLong = ulong.MinValue,
                TestUnsignedShort = ushort.MinValue,
                TestByte = byte.MinValue,
                TestSignedByte = sbyte.MinValue,
            };

            var qs = s.Map(input);
            var output = s.Load(qs);

            Assert.AreEqual("Test String", output.TestString);
            Assert.AreEqual(int.MinValue, output.TestInt);
            Assert.AreEqual(decimal.MinValue, output.TestDecimal);

            // Doubles and Floats of .MinValue size get rounded when they're ToStringed.
            Assert.AreEqual(float.Parse(float.MinValue.ToString()), output.TestFloat);
            Assert.AreEqual(double.Parse((double.MinValue / 10d).ToString()), output.TestDouble);

            Assert.AreEqual(long.MinValue, output.TestLong);
            Assert.AreEqual(true, output.TestBool);
            Assert.AreEqual(short.MinValue, output.TestShort);
            Assert.AreEqual(char.MinValue, output.TestChar);
            Assert.AreEqual(uint.MinValue, output.TestUnsignedInt);
            Assert.AreEqual(ulong.MinValue, output.TestUnsignedLong);
            Assert.AreEqual(ushort.MinValue, output.TestUnsignedShort);
            Assert.AreEqual(byte.MinValue, output.TestByte);
            Assert.AreEqual(sbyte.MinValue, output.TestSignedByte);

            // There's no point doing an object comparison, the floating-point types will not be equal.
        }

        [Test]
        public void ReferenceTypeNullTest()
        {
            // Currently null properties cause a problem as the method generator tries to read their property values.
            QueryStringSerialiser<ReferenceTypeTestClass> s = new QueryStringSerialiser<ReferenceTypeTestClass>();
            var input = new ReferenceTypeTestClass();

            var qs = s.Map(input);
            var output = s.Load(qs);

            Assert.AreEqual(input, output);
        }

        [Test]
        public void ReferenceTypeTest()
        {
            QueryStringSerialiser<ReferenceTypeTestClass> s = new QueryStringSerialiser<ReferenceTypeTestClass>();
            var input = new ReferenceTypeTestClass
            {
                TestReference = new ValueTypeTestClass()
            };

            var qs = s.Map(input);
            var output = s.Load(qs);

            Assert.AreEqual(input, output);
        }

        private class ReferenceTypeTestClass
        {
            public ValueTypeTestClass TestReference { get; set; }

            public override bool Equals(object toCheck)
            {
                var c = toCheck as ReferenceTypeTestClass;
                if (c == null)
                {
                    return false;
                }

                if (c.TestReference == null && this.TestReference != null
                    || c.TestReference != null && this.TestReference == null)
                {
                    return false;
                }

                if (c.TestReference == null & this.TestReference == null)
                {
                    return true;
                }

                return c.TestReference.Equals(this.TestReference);
            }

            public override int GetHashCode()
            {
                if (this.TestReference == null)
                {
                    return -1;
                }

                return this.TestReference.GetHashCode();
            }
        }
    }
}
