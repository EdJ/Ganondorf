namespace Ganondorf.UnitTests
{
    using Ganondorf.UnitTests.Classes;

    using NUnit.Framework;

    [TestFixture]
    public class NestedTypesTests
    {
        [Test]
        public void RecursivePrimitiveTest()
        {
            // Ahhh, Mr. Halting problem, pleased to meet you.
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

        [Test]
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
