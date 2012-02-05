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
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Used to generate a method to add value from an object into a NameValueCollection representing the query string.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object to be loaded.
    /// </typeparam>
    internal class ToNvcMethodGenerator<T> : BaseMethodGenerator<T, NameValueCollection>
    {
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

            DynamicMethod dm = new DynamicMethod(
                "ToNvc_" + fromType.FullName, 
                MethodAttributes.Static | MethodAttributes.Public, 
                CallingConventions.Standard, 
                NvcType, 
                new[] { fromType }, 
                fromType, 
                true);

            var nvcConstructor =
                NvcType.GetConstructor(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, 
                    null, 
                    new Type[] { }, 
                    new ParameterModifier[] { });

            ILGenerator generator = dm.GetILGenerator();

            generator.DeclareLocal(NvcType);
            generator.DeclareLocal(typeof(object));
            generator.DeclareLocal(typeof(decimal));
            generator.DeclareLocal(typeof(float));
            generator.DeclareLocal(typeof(double));
            generator.DeclareLocal(typeof(T));

            generator.Emit(OpCodes.Newobj, nvcConstructor);
            generator.Emit(OpCodes.Stloc, 0);

            generator.Emit(OpCodes.Ldarg, 0);
            generator.Emit(OpCodes.Stloc, 5);

            this.GenerateLevel(generator, fromType, string.Empty, 5, new HashSet<Type>());

            generator.Emit(OpCodes.Ldloc, 0);
            generator.Emit(OpCodes.Ret);

            return dm;
        }

        /// <summary>
        /// Generates IL for loading a single reference type from a NameValueCollection.
        /// </summary>
        /// <param name="generator">
        /// The ILGenerator that is being used to generate the DynamicMethod.
        /// </param>
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
        protected override void GenerateLevel(
            ILGenerator generator, Type levelContainingType, string prefix, int toLoadLocation, HashSet<Type> parentTypeTrail)
        {
            var stringType = typeof(string);

            var addMethod = NvcType.GetMethod("Add", new[] { stringType, stringType });

            var properties = levelContainingType.GetProperties();

            foreach (var prop in properties.Where(x => !TypeNeedsRecursing(x.PropertyType)))
            {
                string newPrefix = prefix + prop.Name;
                generator.Emit(OpCodes.Ldloc, 0);

                generator.Emit(OpCodes.Ldstr, newPrefix);

                if (levelContainingType.IsClass)
                {
                    generator.Emit(OpCodes.Ldloc, toLoadLocation);
                }
                else
                {
                    generator.Emit(OpCodes.Ldloca, toLoadLocation);
                }

                generator.Emit(OpCodes.Callvirt, prop.GetGetMethod());
                var type = prop.PropertyType;
                if (type != stringType)
                {
                    var location = 1;
                    if (type == typeof(decimal))
                    {
                        location = 2;
                    }
                    else if (type == typeof(float))
                    {
                        location = 3;
                    }
                    else if (type == typeof(double))
                    {
                        location = 4;
                    }

                    if (type.IsEnum)
                    {
                        type = typeof(int);
                    }

                    generator.Emit(OpCodes.Stloc, location);

                    generator.Emit(OpCodes.Ldloca, location);
                    var toStringMethod = type.GetMethod("ToString", new Type[] { });

                    generator.Emit(OpCodes.Call, toStringMethod);
                }

                generator.Emit(OpCodes.Callvirt, addMethod);
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

                generator.Emit(OpCodes.Ldloc, toLoadLocation);

                toLoadLocation++;
                generator.DeclareLocal(type);

                generator.Emit(OpCodes.Callvirt, prop.GetGetMethod());
                generator.Emit(OpCodes.Stloc, toLoadLocation);

                this.GenerateLevel(generator, type, newPrefix + "_", toLoadLocation, newTrail);
            }
        }

        #endregion
    }
}