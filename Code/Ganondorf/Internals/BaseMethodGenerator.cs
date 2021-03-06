﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseMethodGenerator.cs" company="SigmoidFx">
//   Copyright Ed 2012.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ganondorf.Internals
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection.Emit;

    /// <summary>
    /// A class used as a base for the two method generators.
    /// Has a couple of shared methods as well as a basic outline of the algorithm.
    /// </summary>
    /// <typeparam name="TFirst">
    /// The type being mapping from.
    /// </typeparam>
    /// <typeparam name="TSecond">
    /// The type being mapped to.
    /// </typeparam>
    internal abstract class BaseMethodGenerator<TFirst, TSecond>
    {
        #region Constants and Fields

        /// <summary>
        /// Single instance of the typeof(decimal) used for type checking.
        /// </summary>
        protected static readonly Type DecimalType = typeof(decimal);

        /// <summary>
        /// Single instance of the typeof(NameValueCollection) used for type checking.
        /// </summary>
        protected static readonly Type NvcType = typeof(NameValueCollection);

        /// <summary>
        /// Single instance of the typeof(string) used for type checking.
        /// </summary>
        protected static readonly Type StringType = typeof(string);

        #endregion

        #region Delegates

        /// <summary>
        /// This is used to define the delegate that will eventually become the dynamic method to map from one type to the other.
        /// </summary>
        /// <param name="from">
        /// The an instance of the type to map from.
        /// </param>
        /// <returns>
        /// The an instance of the type being mapped to.
        /// </returns>
        public delegate TSecond MappingFunction(TFirst from);

        #endregion

        #region Public Methods

        /// <summary>
        /// Entry point for any external usages of the MethodGenerator classes.
        /// </summary>
        /// <returns>
        /// A function that maps an object (of type T) to a NameValueCollection or vice-versa.
        /// </returns>
        public Func<TFirst, TSecond> GenerateMethod()
        {
            DynamicMethod del = this.GenerateDelegate();

            var map = (MappingFunction)del.CreateDelegate(typeof(MappingFunction));

            // Wrap it in a type-safe wrapper.
            Func<TFirst, TSecond> output = x => map(x);

            return output;
        }

        #endregion

        #region Methods

        /// <summary>
        /// This is just an access point for the TypeHelper.TypeNeedsRecursing method.
        /// </summary>
        /// <param name="type">
        /// The type to check.
        /// </param>
        /// <returns>
        /// Whether to attempt to serialise the properties of the type.
        /// </returns>
        protected static bool TypeNeedsRecursing(Type type)
        {
            return TypeHelper.TypeNeedsRecursing(type);
        }

        /// <summary>
        /// Simple access point for the method generation method.
        /// </summary>
        /// <returns>
        /// A DynamicMethod that maps the types, that is then turned into a delegate for speed purposes.
        /// </returns>
        protected abstract DynamicMethod GenerateDelegate();

        #endregion
    }
}