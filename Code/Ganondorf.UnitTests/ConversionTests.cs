// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConversionTests.cs" company="SigmoidFx">
//   Copyright Ed 2012.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ganondorf.UnitTests
{
    using Ganondorf.UnitTests.Classes;

    using NUnit.Framework;

    /// <summary>
    /// Testing converting a type to a NameValueCollection and back.
    /// </summary>
    [TestFixture]
    public class ConversionTests
    {
        /// <summary>
        /// Simple value type serialisation test, uses default values for the properties.
        /// </summary>
        [Test]
        public void ValueTypeTest()
        {
            QueryStringSerialiser<ValueTypeTestClass> s = new QueryStringSerialiser<ValueTypeTestClass>();
            var input = new ValueTypeTestClass();

            var qs = s.Map(input);
            var output = s.Load(qs);

            Assert.AreEqual(input, output);
        }

        /// <summary>
        /// Checks that there are no odd bounds errors with max values.
        /// (Unfortunately due to the way floating point numbers work in .net this is actually not possible).
        /// </summary>
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

        /// <summary>
        /// Checks that there are no odd bounds errors with min values.
        /// (Unfortunately due to the way floating point numbers work in .net this is actually not possible).
        /// </summary>
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

        /// <summary>
        /// Checks whether a null property value throws an exception.
        /// Currently the method generator does not take into account null objects, and will throw a NullReferenceException, which is not ideal.
        /// </summary>
        [Test]
        public void ReferenceTypeNullTest()
        {
            // Currently null properties cause a problem as the generated method tries to read their property values.
            QueryStringSerialiser<ReferenceTypeTestClass> s = new QueryStringSerialiser<ReferenceTypeTestClass>();
            var input = new ReferenceTypeTestClass();

            var qs = s.Map(input);
            var output = s.Load(qs);

            Assert.AreEqual(input, output);
        }

        /// <summary>
        /// Checks that serialising an object with a sub-object works correctly.
        /// </summary>
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

                if ((c.TestReference == null && this.TestReference != null)
                    || (c.TestReference != null && this.TestReference == null))
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
