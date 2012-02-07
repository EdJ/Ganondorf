// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryStringSerialiser.cs" company="SigmoidFx">
//   Copyright Ed 2012.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ganondorf
{
    using System;
    using System.Collections.Specialized;
    using System.Threading;

    using Ganondorf.Exceptions;
    using Ganondorf.Internals;

    /// <summary>
    /// A class that deals with serialising objects to and from the QS via a NameValueCollection.
    /// The internal method cache is only used for this instance, if high performance is desired a singleton pattern or cache may be advisable.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object to serialise.
    /// </typeparam>
    public class QueryStringSerialiser<T>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringSerialiser{T}"/> class.
        /// </summary>
        /// <exception cref="InvalidTypeException">
        /// If the type that is passed into the serialiser is a value type this exception will be thrown, as value types cannot currently be serialised.
        /// </exception>
        public QueryStringSerialiser()
        {
            var type = typeof(T);
            if (!TypeHelper.TypeNeedsRecursing(type))
            {
                throw new InvalidTypeException(type);
            }

            this.FromNvc = new Lazy<Func<NameValueCollection, T>>(GenerateLoadMethod, LazyThreadSafetyMode.ExecutionAndPublication);
            this.ToNvc = new Lazy<Func<T, NameValueCollection>>(GenerateMapMethod, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a method used to map a T to a NameValueCollection.
        /// </summary>
        private Lazy<Func<NameValueCollection, T>> FromNvc { get; set; }

        /// <summary>
        /// Gets or sets a method used to map a T from a NameValueCollection.
        /// </summary>
        private Lazy<Func<T, NameValueCollection>> ToNvc { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Instantiates a new T from the NameValueCollection representing the query string.
        /// </summary>
        /// <param name="loadFrom">
        /// The NameValueCollection to load from.
        /// </param>
        /// <returns>
        /// An instantiated T with data parsed from the NameValueCollection.
        /// </returns>
        public T Load(NameValueCollection loadFrom)
        {
            return this.FromNvc.Value(loadFrom);
        }

        /// <summary>
        /// Maps an object into a NameValueCollection so it can be used as a set of query string parameters.
        /// </summary>
        /// <param name="toMap">
        /// The object to map.
        /// </param>
        /// <returns>
        /// A NameValueCollection populated with the properties of the T.
        /// </returns>
        public NameValueCollection Map(T toMap)
        {
            return this.ToNvc.Value(toMap);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Uses a FromNvcMethodGenerator to generate a new T from a NameValueCollection.
        /// </summary>
        /// <returns>
        /// A new instance of T with parameters loaded from the NameValueCollection.
        /// </returns>
        private static Func<NameValueCollection, T> GenerateLoadMethod()
        {
            var methodGenerator = new FromNvcMethodGenerator<T>(new IlSnippetGenerator());
            return methodGenerator.GenerateMethod();
        }

        /// <summary>
        /// Uses a ToNvcMethodGenerator to generate a new NameValueCollection from a T.
        /// </summary>
        /// <returns>
        /// A new NameValueCollection instance with data loaded from the T.
        /// </returns>
        private static Func<T, NameValueCollection> GenerateMapMethod()
        {
            var methodGenerator = new ToNvcMethodGenerator<T>(new IlSnippetGenerator());
            return methodGenerator.GenerateMethod();
        }

        #endregion
    }
}