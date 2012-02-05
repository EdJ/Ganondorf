namespace Ganondorf.UnitTests
{
    using NUnit.Framework;

    [TestFixture]
    public class StructTests
    {
        [Test]
        public void ValueTypeStructTest()
        {
            QueryStringSerialiser<ValueTypeTestStruct> s = new QueryStringSerialiser<ValueTypeTestStruct>();
            var input = new ValueTypeTestStruct();

            var qs = s.Map(input);
            var output = s.Load(qs);

            Assert.AreEqual(input, output);
        }

        private struct ValueTypeTestStruct
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
                if (toCompare == null || !(toCompare is ValueTypeTestStruct))
                {
                    return false;
                }

                var c = (ValueTypeTestStruct)toCompare;

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
    }
}
