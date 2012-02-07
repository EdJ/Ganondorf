// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FromNvcMethodGenerator.cs" company="SigmoidFx">
//   Copyright Ed 2012.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ganondorf.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection.Emit;

    using Ganondorf.Interfaces;

    /// <summary>
    /// Used to generate a method to load an object from a NameValueCollection representing the query string.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object to be loaded.
    /// </typeparam>
    internal class FromNvcMethodGenerator<T> : BaseMethodGenerator<NameValueCollection, T>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FromNvcMethodGenerator{T}"/> class.
        /// </summary>
        /// <param name="snippetGenerator">
        /// The snippet generator used to generate the DynamicMethod.
        /// </param>
        internal FromNvcMethodGenerator(ISnippetGenerator snippetGenerator)
        {
            this.SnippetGenerator = snippetGenerator;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets or sets the snippet generator used to generate the DynamicMethod.
        /// </summary>
        private ISnippetGenerator SnippetGenerator { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Generates a dynamic method using an ILGenerator that maps an object from a NameValueCollection.
        /// </summary>
        /// <returns>
        /// A DynamicMethod that can be used to map an object of type T from a NameValueCollection.
        /// </returns>
        protected override DynamicMethod GenerateDelegate()
        {
            var toType = typeof(T);

            this.SnippetGenerator.DeclareMethod(toType, typeof(NameValueCollection), "FromNvc_");

            this.SnippetGenerator.DeclarePrimitiveContainers();
            this.SnippetGenerator.DeclareLocal(toType);

            const int ObjectLocation = 4;

            this.SnippetGenerator.InstantiateType(toType, ObjectLocation);

            this.GenerateLevel(toType, string.Empty, ObjectLocation, new HashSet<Type>());

            this.SnippetGenerator.LoadReturnValue(ObjectLocation);

            DynamicMethod dm = this.SnippetGenerator.GetGeneratedMethod();

            return dm;
        }

        /// <summary>
        /// Generates IL for loading a single reference type from a NameValueCollection.
        /// </summary>
        /// <param name="levelContainingType">
        /// The type of the class that's being recursed.
        /// </param>
        /// <param name="prefix">
        /// The prefix to use on the property names when adding them as keys to the NameValueCollection.
        /// </param>
        /// <param name="toLoadLocation">
        /// The current local variable location of the parent instance.
        /// </param>
        /// <param name="parentTypeTrail">
        /// A list of types that have been seen before when serialising the class. If a type is seen twice trying to recurse it results in a possible infinite loop.
        /// </param>
        protected void GenerateLevel(Type levelContainingType, string prefix, int toLoadLocation, HashSet<Type> parentTypeTrail)
        {
            var parentLocation = toLoadLocation;

            var getMethod = NvcType.GetMethod("Get", new[] { StringType });

            var properties = levelContainingType.GetProperties();

            foreach (var prop in properties.Where(x => !TypeNeedsRecursing(x.PropertyType)))
            {
                string newPrefix = prefix + prop.Name;

                this.SnippetGenerator.LoadInstanceReference(toLoadLocation, levelContainingType);

                this.SnippetGenerator.LoadArgument(0);

                this.SnippetGenerator.LoadString(newPrefix);

                this.SnippetGenerator.CallMethod(getMethod, true);

                this.SnippetGenerator.TryParsePrimitiveValue(prop.PropertyType);

                this.SnippetGenerator.CallMethod(levelContainingType, prop.GetSetMethod());
            }

            foreach (var prop in properties.Where(x => TypeNeedsRecursing(x.PropertyType)))
            {
                if (parentTypeTrail.Contains(prop.PropertyType))
                {
                    continue;
                }

                var newTrail = new HashSet<Type>(parentTypeTrail) { prop.PropertyType };

                var type = prop.PropertyType;

                toLoadLocation++;
                this.SnippetGenerator.DeclareLocal(type);

                var currentLocation = toLoadLocation;

                this.SnippetGenerator.InstantiateType(type, currentLocation);
                
                string newPrefix = prefix + prop.Name;
                this.GenerateLevel(type, newPrefix + "_", toLoadLocation, newTrail);

                this.SnippetGenerator.LoadInstanceReference(parentLocation, levelContainingType);
                this.SnippetGenerator.LoadInstanceReference(currentLocation, NvcType);

                this.SnippetGenerator.CallMethod(levelContainingType, prop.GetSetMethod());
            }
        }

        #endregion
    }
}