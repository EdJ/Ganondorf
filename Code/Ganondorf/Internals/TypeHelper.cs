// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeHelper.cs" company="SigmoidFx">
//   Copyright Ed 2012.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ganondorf.Internals
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A helper class used to hold a couple of methods that don't really fit elsewhere.
    /// </summary>
    internal static class TypeHelper
    {
        #region Constants and Fields

        /// <summary>
        /// A list of the value types.
        /// </summary>
        private static readonly HashSet<Type> ValueTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(bool),
                typeof(int),
                typeof(decimal),
                typeof(float),
                typeof(double),
                typeof(long),
                typeof(short),
                typeof(char),
                typeof(uint),
                typeof(ulong),
                typeof(ushort),
                typeof(byte),
                typeof(sbyte)
            };

        #endregion

        #region Methods

        /// <summary>
        /// This is used to indicate if a type needs to be recursed by the serialiser.
        /// Reference types are recursed, while value types are ToString()ed and added to the NameValueCollection.
        /// </summary>
        /// <param name="type">
        /// The type to check.
        /// </param>
        /// <returns>
        /// Whether to recurse the type in question.
        /// </returns>
        internal static bool TypeNeedsRecursing(Type type)
        {
            return !type.IsEnum && !ValueTypes.Contains(type);
        }

        #endregion
    }
}