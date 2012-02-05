// -----------------------------------------------------------------------
// <copyright file="EnumTests.cs" company="SigmoidFx">
//  Copyright Ed 2012.
// </copyright>
// -----------------------------------------------------------------------

namespace Ganondorf.UnitTests
{
    using NUnit.Framework;

    /// <summary>
    /// Tests that check enum conversion.
    /// </summary>
    [TestFixture]
    public class EnumTests
    {
        /// <summary>
        /// A simple enum test.
        /// </summary>
        [Test]
        public void EnumTest()
        {
            var serialiser = new QueryStringSerialiser<SimpleEnumTestClass>();

            var test = new SimpleEnumTestClass();

            var mapped = serialiser.Map(test);
            var output = serialiser.Load(mapped);

            Assert.AreEqual(test, output);
        }

        /// <summary>
        /// Checks whether enums assigned an invalid value can be serialised.
        /// </summary>
        [Test]
        public void InvalidMaxValueEnumTest()
        {
            var serialiser = new QueryStringSerialiser<SimpleEnumTestClass>();

            var test = new SimpleEnumTestClass { Test = (TestEnum)int.MaxValue };

            var mapped = serialiser.Map(test);
            var output = serialiser.Load(mapped);

            Assert.AreEqual(test, output);
        }

        /// <summary>
        /// Checks whether enums assigned an invalid value can be serialised.
        /// </summary>
        [Test]
        public void InvalidMinValueEnumTest()
        {
            var serialiser = new QueryStringSerialiser<SimpleEnumTestClass>();

            var test = new SimpleEnumTestClass { Test = (TestEnum)int.MinValue };

            var mapped = serialiser.Map(test);
            var output = serialiser.Load(mapped);

            Assert.AreEqual(test, output);
        }

        private class SimpleEnumTestClass
        {
            public TestEnum Test { get; set; }

            public override bool Equals(object toCompare)
            {
                var c = toCompare as SimpleEnumTestClass;
                if (c == null)
                {
                    return false;
                }

                return c.Test == this.Test;
            }

            public override int GetHashCode()
            {
                return (int)this.Test;
            }
        }

        private enum TestEnum
        {
            True = 0,
            False = 1
        }

        /// <summary>
        /// Checks whether an enum with two options assigned the same value can be serialised.
        /// </summary>
        [Test]
        public void BadValueEnumTest()
        {
            var serialiser = new QueryStringSerialiser<BadValueEnumTestClass>();

            var test = new BadValueEnumTestClass();

            var mapped = serialiser.Map(test);
            var output = serialiser.Load(mapped);

            Assert.AreEqual(test, output);
        }

        /// <summary>
        /// Checks whether an enum with two options assigned the same value can be serialised.
        /// This test is actually bizarre, as the two values are technically equal but also not.
        /// </summary>
        [Test]
        public void BadValueBadParseEnumTest()
        {
            var serialiser = new QueryStringSerialiser<BadValueEnumTestClass>();

            var test = new BadValueEnumTestClass { Test = BadValueEnum.False };

            var mapped = serialiser.Map(test);
            var output = serialiser.Load(mapped);

            Assert.AreEqual(test, output);

            // And there's your problem, this will pass.
            Assert.AreEqual(BadValueEnum.True, BadValueEnum.False);
        }

        private class BadValueEnumTestClass
        {
            public BadValueEnum Test { get; set; }

            public override bool Equals(object toCompare)
            {
                var c = toCompare as BadValueEnumTestClass;
                if (c == null)
                {
                    return false;
                }

                return c.Test == this.Test;
            }

            public override int GetHashCode()
            {
                return (int)this.Test;
            }
        }

        private enum BadValueEnum
        {
            True = 0,
            False = 0
        }
    }
}
