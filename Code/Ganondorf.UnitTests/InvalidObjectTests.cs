namespace Ganondorf.UnitTests
{
    using NUnit.Framework;
    using Ganondorf.Exceptions;
    using System;
    using System.Reflection;

    [TestFixture]
    public class InvalidObjectTests
    {
        [ExpectedException(ExpectedException = typeof(InvalidTypeException))]
        [TestCase(typeof(string))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(int))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(float))]
        [TestCase(typeof(double))]
        [TestCase(typeof(long))]
        [TestCase(typeof(short))]
        [TestCase(typeof(char))]
        [TestCase(typeof(uint))]
        [TestCase(typeof(ulong))]
        [TestCase(typeof(ushort))]
        [TestCase(typeof(byte))]
        [TestCase(typeof(sbyte))]
        public void VanillaValueTypeTest(Type paramType)
        {
            // This is not going to work.
            // I think throwing an "invalid object" exception would make sense.
            Type runnerType = typeof(QueryStringSerialiser<>);
            try
            {
                var runner = Activator.CreateInstance(runnerType.MakeGenericType(paramType));
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }
}
