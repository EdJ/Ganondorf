// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvalidObjectTests.cs" company="SigmoidFx">
//   Copyright Ed 2012.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ganondorf.UnitTests
{
    using System;
    using System.Reflection;

    using Ganondorf.Exceptions;

    using NUnit.Framework;

    /// <summary>
    /// Used to check that the QueryStringSerialiser won't attempt to serialise an object it cannot deal with.
    /// </summary>
    [TestFixture]
    public class InvalidObjectTests
    {
        /// <summary>
        /// Checks that all the reference types throw InvalidTypeExceptions when added as the type parameter of a QueryStringSerialiser.
        /// </summary>
        /// <param name="paramType">
        /// The type to check.
        /// </param>
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
            // This is not going to work in the current version of the QSS. Perhaps a generic "SerialisedPrimitive" key would be acceptable.
            // I think throwing an "invalid object" exception would make sense, however, as the library isn't really designed to deal with raw primitives, and you can just wrap them in a class.
            Type runnerType = typeof(QueryStringSerialiser<>);
            try
            {
                Activator.CreateInstance(runnerType.MakeGenericType(paramType));
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }
}
