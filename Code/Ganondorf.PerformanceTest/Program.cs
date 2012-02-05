using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Ganondorf.PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            QueryStringSerialiser<ValueTypeTestClass> s = new QueryStringSerialiser<ValueTypeTestClass>();

            var input = new ValueTypeTestClass
            {
                TestString = "Test String",
                TestInt = int.MaxValue,
                TestDecimal = decimal.MaxValue,
                TestFloat = float.MaxValue,
                TestDouble = double.MaxValue / 10d,
                TestLong = long.MaxValue,
                TestBool = true,
                TestShort = short.MaxValue,
                TestChar = 'Y',
                TestUnsignedInt = uint.MaxValue,
                TestUnsignedLong = ulong.MaxValue,
                TestUnsignedShort = ushort.MaxValue,
                TestByte = byte.MaxValue,
                TestSignedByte = sbyte.MaxValue,
            };

            Map(input);

            var qs = s.Map(input);
            var output = s.Load(qs);

            Console.ReadLine();
        }

        private static void Map(ValueTypeTestClass t)
        {
            NameValueCollection n = new NameValueCollection();

            n.Add("TestInt", t.TestInt.ToString());
        }

        private static void DeMap(NameValueCollection n)
        {
            var s = new ValueTypeTestClass();
            int test;
            int.TryParse(n.Get("TestIne"), out test);
            s.TestInt = test;
        }

        private struct ValueTypeTestClass
        {
            public string TestString { get; set; }
            public int TestInt { get; set; }
            public decimal TestDecimal { get; set; }
            public float TestFloat { get; set; }
            public double TestDouble { get; set; }
            public long TestLong { get; set; }
            public bool TestBool { get; set; }
            public short TestShort { get; set; }
            public char TestChar { get; set; }
            public uint TestUnsignedInt { get; set; }
            public ulong TestUnsignedLong { get; set; }
            public ushort TestUnsignedShort { get; set; }
            public byte TestByte { get; set; }
            public sbyte TestSignedByte { get; set; }

            public override bool Equals(object toCompare)
            {
                if (toCompare == null || !(toCompare is ValueTypeTestClass))
                {
                    return false;
                }

                var c = (ValueTypeTestClass)toCompare;

                return c.TestString == this.TestString
                    && c.TestInt == this.TestInt
                    && c.TestDecimal == this.TestDecimal
                    && c.TestFloat == this.TestFloat
                    && c.TestDouble == this.TestDouble
                    && c.TestLong == this.TestLong
                    && c.TestBool == this.TestBool
                    && c.TestShort == this.TestShort
                    && c.TestChar == this.TestChar
                    && c.TestUnsignedInt == this.TestUnsignedInt
                    && c.TestUnsignedLong == this.TestUnsignedLong
                    && c.TestUnsignedShort == this.TestUnsignedShort
                    && c.TestByte == this.TestByte
                    && c.TestSignedByte == this.TestSignedByte;
            }

            public override int GetHashCode()
            {
                return this.TestInt;
            }
        }

        private class RecursivePrimitiveTestClass
        {
            public RecursivePrimitiveTestClass TestRecursive { get; set; }
            public ValueTypeTestClass TestReferences { get; set; }
        }

        private static void MapSimpleTest()
        {
            var toTest = new TestClass
            {
                TestOne = 7m,
                TestTwo = "Test!",
                Nested = new InnerClass
                {
                    TestOne = "Hello!",
                    TestTwo = new SuperNested
                    {
                        TestOne = "Check out the recursion!"
                    }
                },
                TestEnum = NestedEnum.FileNotFound,
                TestThree = true
            };

            QueryStringSerialiser<TestClass> s = new QueryStringSerialiser<TestClass>();
            var otherCollection = s.Map(toTest);

            Console.WriteLine(otherCollection["TestOne"]);
            Console.WriteLine(otherCollection["TestTwo"]);
            Console.WriteLine(otherCollection["TestThree"]);
            Console.WriteLine(otherCollection["TestEnum"]);
            Console.WriteLine(otherCollection["Nested_TestOne"]);
            Console.WriteLine(otherCollection["Nested_TestTwo_TestOne"]);

            var obj = s.Load(otherCollection);
        }

        private static NameValueCollection Map(TestClass toMap)
        {
            NameValueCollection output = new NameValueCollection();

            output.Add("TestOne", toMap.TestOne.ToString());
            output.Add("TestTwo", toMap.TestTwo);
            output.Add("TestEnum", ((int)toMap.TestEnum).ToString());

            output.Add("Test", (toMap.Nested ?? new InnerClass()).TestOne);

            return output;
        }

        private static TestClass Parse(NameValueCollection toParse)
        {
            TestClass output = new TestClass();

            int testOne;
            int.TryParse(toParse.Get("TestOne"), out testOne);

            output.TestOne = testOne;

            output.TestTwo = toParse.Get("TestTwo");

            bool testThree;
            bool.TryParse(toParse.Get("TestThree"), out testThree);

            output.TestThree = testThree;

            int testEnum;
            int.TryParse(toParse.Get("TestEnum"), out testEnum);
            output.TestEnum = (NestedEnum)testEnum;

            var nested = new InnerClass();
            nested.TestOne = toParse.Get("Nested_TestOne");

            var superNested = new SuperNested();
            superNested.TestOne = toParse.Get("Nested_TestTwo_TestOne");

            nested.TestTwo = superNested;
            return output;
        }

        private struct TestClass
        {
            public decimal TestOne { get; set; }
            public string TestTwo { get; set; }
            public InnerClass Nested { get; set; }
            public bool TestThree { get; set; }
            public NestedEnum TestEnum { get; set; }
        }

        private enum NestedEnum
        {
            True = 0,
            False = 1,
            FileNotFound = 2
        }

        private class InnerClass
        {
            public string TestOne { get; set; }
            public SuperNested TestTwo { get; set; }
        }

        private class SuperNested
        {
            public string TestOne { get; set; }
        }
    }
}
