namespace Ganondorf.UnitTests
{
    using System;

    using Ganondorf.UnitTests.Classes;

    using NUnit.Framework;

    [TestFixture]
    public class NestedTypesTests
    {
        /// <summary>
        /// Checks that a recursive class definition does not throw a StackOverflow-, or other long-running, Exception.
        /// The current way of handling this situation is not ideal.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void RecursivePrimitiveTest()
        {
            // Ahhh, Mr. Halting problem; Pleased to meet you.
            QueryStringSerialiser<RecursivePrimitiveTestClass> s = new QueryStringSerialiser<RecursivePrimitiveTestClass>();
            var input = new RecursivePrimitiveTestClass();

            var qs = s.Map(input);
            var output = s.Load(qs);

            Assert.AreEqual(input, output);
        }

        private class RecursivePrimitiveTestClass
        {
            public RecursivePrimitiveTestClass TestRecursive { get; set; }
            public ValueTypeTestClass TestReferences { get; set; }
        }

        /// <summary>
        /// Checks that multi-level recursion is recognised.
        /// The current way of handling this situation is not ideal.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NullReferenceException))]
        public void MultiLevelRecursiveTypeTest()
        {
            // This one is a bit harder, the method generators will need to store references to the classes they have walked.
            QueryStringSerialiser<DualLevelRecursivePrimitiveTestLevelOne> s = new QueryStringSerialiser<DualLevelRecursivePrimitiveTestLevelOne>();
            var input = new DualLevelRecursivePrimitiveTestLevelOne
            {
                RecursiveRefernce = new DualLevelRecursivePrimitiveTestLevelTwo
                {
                    TestReferences = new ValueTypeTestClass()
                },
                TestReferences = new ValueTypeTestClass()
            };

            var qs = s.Map(input);
            var output = s.Load(qs);

            Assert.AreEqual(input, output);
        }

        private class DualLevelRecursivePrimitiveTestLevelOne
        {
            public DualLevelRecursivePrimitiveTestLevelTwo RecursiveRefernce { get; set; }

            public ValueTypeTestClass TestReferences { get; set; }
        }

        private class DualLevelRecursivePrimitiveTestLevelTwo
        {
            public DualLevelRecursivePrimitiveTestLevelOne RecursiveRefernce { get; set; }

            public ValueTypeTestClass TestReferences { get; set; }
        }
    }
}
