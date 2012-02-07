// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToNvcMethodGenerator.cs" company="SigmoidFx">
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
    /// Used to generate a method to add value from an object into a NameValueCollection representing the query string.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object to be loaded.
    /// </typeparam>
    internal class ToNvcMethodGenerator<T> : BaseMethodGenerator<T, NameValueCollection>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ToNvcMethodGenerator{T}"/> class.
        /// </summary>
        /// <param name="snippetGenerator">
        /// The snippet generator used to generate the DynamicMethod.
        /// </param>
        internal ToNvcMethodGenerator(ISnippetGenerator snippetGenerator)
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
        /// Generates a dynamic method using an ILGenerator that maps an object to a NameValueCollection.
        /// </summary>
        /// <returns>
        /// A DynamicMethod that can be used to map an object of type T from a NameValueCollection.
        /// </returns>
        protected override DynamicMethod GenerateDelegate()
        {
            var fromType = typeof(T);

            this.SnippetGenerator.DeclareMethod(NvcType, fromType, "ToNvc_");

            var localOffset = this.SnippetGenerator.DeclarePrimitiveContainers();
            this.SnippetGenerator.DeclareLocal(NvcType);
            this.SnippetGenerator.DeclareLocal(typeof(T));

            this.SnippetGenerator.InstantiateType(NvcType, localOffset);

            var inputLocation = localOffset + 1;

            this.SnippetGenerator.LoadArgument(0);
            this.SnippetGenerator.StoreItemFromStack(inputLocation);

            this.GenerateLevel(fromType, string.Empty, localOffset, inputLocation, new HashSet<Type>());

            this.SnippetGenerator.LoadReturnValue(localOffset);

            var dm = this.SnippetGenerator.GetGeneratedMethod();

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
        /// <param name="outputLocation">
        /// The current local variable location of the output instance.
        /// </param>
        /// <param name="toLoadLocation">
        /// The current local variable location of the parent instance.
        /// </param>
        /// <param name="parentTypeTrail">
        /// A list of types that have been seen before when serialising the class. If a type is seen twice trying to recurse it results in a possible infinite loop.
        /// </param>
        protected void GenerateLevel(Type levelContainingType, string prefix, int outputLocation, int toLoadLocation, HashSet<Type> parentTypeTrail)
        {
            var stringType = typeof(string);

            var addMethod = NvcType.GetMethod("Add", new[] { stringType, stringType });

            var properties = levelContainingType.GetProperties();

            foreach (var prop in properties.Where(x => !TypeNeedsRecursing(x.PropertyType)))
            {
                string newPrefix = prefix + prop.Name;
                this.SnippetGenerator.LoadInstanceReference(outputLocation, NvcType);

                this.SnippetGenerator.LoadString(newPrefix);

                this.SnippetGenerator.LoadInstanceReference(toLoadLocation, levelContainingType);

                this.SnippetGenerator.CallMethod(prop.GetGetMethod(), true);
                
                this.SnippetGenerator.ToStringPrimitiveValue(prop.PropertyType);

                this.SnippetGenerator.CallMethod(addMethod, true);
            }

            foreach (var prop in properties.Where(x => TypeNeedsRecursing(x.PropertyType)))
            {
                if (parentTypeTrail.Contains(prop.PropertyType))
                {
                    continue;
                }

                var newTrail = new HashSet<Type>(parentTypeTrail) { prop.PropertyType };

                string newPrefix = prefix + prop.Name;
                var type = prop.PropertyType;

                this.SnippetGenerator.LoadInstanceReference(toLoadLocation, levelContainingType);

                toLoadLocation++;

                this.SnippetGenerator.DeclareLocal(type);

                this.SnippetGenerator.CallMethod(prop.GetGetMethod(), prop.PropertyType.IsByRef);

                this.SnippetGenerator.StoreItemFromStack(toLoadLocation);

                this.GenerateLevel(type, newPrefix + "_", outputLocation, toLoadLocation, newTrail);
            }
        }

        #endregion
    }
}