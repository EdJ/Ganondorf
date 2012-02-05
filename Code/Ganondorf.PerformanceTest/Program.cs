// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="SigmoidFx">
//   Copyright Ed 2012.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ganondorf.PerformanceTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// A simple performance test of the mapping methods.
    /// </summary>
    public class Program
    {
        #region Methods

        /// <summary>
        /// Entry point for the performance test.
        /// </summary>
        /// <param name="args">
        /// The command-line arguments. These are not used.
        /// </param>
        public static void Main(string[] args)
        {
            SpeedTest();
            ConcurrencyTest();
        }

        /// <summary>
        /// Checks to make sure the lazy loading of the method generation is working correctly.
        /// </summary>
        private static void ConcurrencyTest()
        {
            QueryStringSerialiser<ValueTypeTestClass> serialiser = new QueryStringSerialiser<ValueTypeTestClass>();

            const int TestIterations = 10000;

            List<long> timings = new List<long>(TestIterations);

            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();

            Parallel.ForEach(
                Enumerable.Range(1, TestIterations),
                x =>
                    {
                        Stopwatch s = new Stopwatch();
                        s.Start();
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

                        var qs = serialiser.Map(input);
                        serialiser.Load(qs);
                        s.Stop();

                        timings.Add(s.ElapsedMilliseconds);
                    });

            totalTime.Stop();

            Console.WriteLine("Test iterations: " + TestIterations);
            Console.WriteLine("Total test time: " + totalTime.ElapsedMilliseconds + "ms");

            // This is important as the map method is lazily loaded, and so the first iteration will swallow the method generation overhead.
            Console.WriteLine("First serialisation time: " + timings[0] + "ms");
            Console.WriteLine("Average serialisation time: " + timings.Average() + "ms");
            Console.WriteLine("Slowest serialisation time: " + timings.Max() + "ms");
            Console.WriteLine("Fastest serialisation time: " + timings.Min() + "ms");

            Console.ReadLine();
        }

        /// <summary>
        /// Tests method generation and mapping speed.
        /// </summary>
        private static void SpeedTest()
        {
            QueryStringSerialiser<ValueTypeTestClass> serialiser = new QueryStringSerialiser<ValueTypeTestClass>();

            const int TestIterations = 10000;

            List<long> timings = new List<long>(TestIterations);

            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();

            Stopwatch s = new Stopwatch();
            s.Start();

            foreach (var counter in Enumerable.Range(1, TestIterations))
            {
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

                var qs = serialiser.Map(input);
                serialiser.Load(qs);
                s.Stop();

                timings.Add(s.ElapsedMilliseconds);

                s.Restart();
            }

            totalTime.Stop();

            Console.WriteLine("Test iterations: " + TestIterations);
            Console.WriteLine("Total test time: " + totalTime.ElapsedMilliseconds + "ms");

            // This is important as the map method is lazily loaded, and so the first iteration will swallow the method generation overhead.
            Console.WriteLine("First serialisation time: " + timings[0] + "ms");
            Console.WriteLine("Average serialisation time: " + timings.Average() + "ms");
            Console.WriteLine("Slowest serialisation time: " + timings.Max() + "ms");
            Console.WriteLine("Fastest serialisation time: " + timings.Min() + "ms");

            Console.ReadLine();
        }

        #endregion

        /// <summary>
        /// A simple value type-only class used in the performance test.
        /// Please excuse the lack of documentation, it's fairly obvious what each property is for.
        /// </summary>
        private struct ValueTypeTestClass
        {
            #region Public Properties

            public bool TestBool { get; set; }

            public byte TestByte { get; set; }

            public char TestChar { get; set; }

            public decimal TestDecimal { get; set; }

            public double TestDouble { get; set; }

            public float TestFloat { get; set; }

            public int TestInt { get; set; }

            public long TestLong { get; set; }

            public short TestShort { get; set; }

            public sbyte TestSignedByte { get; set; }

            public string TestString { get; set; }

            public uint TestUnsignedInt { get; set; }

            public ulong TestUnsignedLong { get; set; }

            public ushort TestUnsignedShort { get; set; }

            #endregion

            #region Public Methods

            public override bool Equals(object toCompare)
            {
                if (toCompare == null || !(toCompare is ValueTypeTestClass))
                {
                    return false;
                } 

                var c = (ValueTypeTestClass)toCompare;

                return c.TestString == this.TestString && c.TestInt == this.TestInt && c.TestDecimal == this.TestDecimal
                       && c.TestFloat == this.TestFloat && c.TestDouble == this.TestDouble
                       && c.TestLong == this.TestLong && c.TestBool == this.TestBool && c.TestShort == this.TestShort
                       && c.TestChar == this.TestChar && c.TestUnsignedInt == this.TestUnsignedInt
                       && c.TestUnsignedLong == this.TestUnsignedLong && c.TestUnsignedShort == this.TestUnsignedShort
                       && c.TestByte == this.TestByte && c.TestSignedByte == this.TestSignedByte;
            }

            public override int GetHashCode()
            {
                return this.TestInt;
            }

            #endregion
        }
    }
}