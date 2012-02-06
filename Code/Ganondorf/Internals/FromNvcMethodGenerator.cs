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
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Used to generate a method to load an object from a NameValueCollection representing the query string.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the object to be loaded.
    /// </typeparam>
    internal class FromNvcMethodGenerator<T> : BaseMethodGenerator<NameValueCollection, T>
    {
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

            DynamicMethod dm = new DynamicMethod(
                "FromNvc_" + toType.FullName, 
                MethodAttributes.Static | MethodAttributes.Public, 
                CallingConventions.Standard, 
                toType, 
                new[] { NvcType }, 
                toType, 
                true);

            ILGenerator generator = dm.GetILGenerator();

            generator.DeclareLocal(typeof(object));
            generator.DeclareLocal(typeof(decimal));
            generator.DeclareLocal(typeof(float));
            generator.DeclareLocal(typeof(double));
            generator.DeclareLocal(toType);

            const int ObjectLocation = 4;

            if (toType.IsClass)
            {
                var toTypeConstructor =
                    toType.GetConstructor(
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, 
                        null, 
                        new Type[] { }, 
                        new ParameterModifier[] { });
                generator.Emit(OpCodes.Newobj, toTypeConstructor);

                generator.Emit(OpCodes.Stloc, ObjectLocation);
            }
            else
            {
                generator.Emit(OpCodes.Ldloca_S, 4);
                generator.Emit(OpCodes.Initobj, toType);
            }

            this.GenerateLevel(generator, toType, string.Empty, ObjectLocation, new HashSet<Type>());

            generator.Emit(OpCodes.Ldloc, ObjectLocation);
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
        protected override void GenerateLevel(ILGenerator generator, Type levelContainingType, string prefix, int toLoadLocation, HashSet<Type> parentTypeTrail)
        {
            var parentLocation = toLoadLocation;

            var getMethod = NvcType.GetMethod("Get", new[] { StringType });

            var properties = levelContainingType.GetProperties();

            foreach (var prop in properties.Where(x => !TypeNeedsRecursing(x.PropertyType)))
            {
                string newPrefix = prefix + prop.Name;

                if (levelContainingType.IsClass)
                {
                    generator.Emit(OpCodes.Ldloc, toLoadLocation);
                }
                else
                {
                    generator.Emit(OpCodes.Ldloca_S, toLoadLocation);
                }

                generator.Emit(OpCodes.Ldarg, 0);

                generator.Emit(OpCodes.Ldstr, newPrefix);

                generator.Emit(OpCodes.Callvirt, getMethod);

                var type = prop.PropertyType;
                if (type != StringType)
                {
                    var location = 0;
                    if (type == typeof(decimal))
                    {
                        location = 1;
                    }
                    else if (type == typeof(float))
                    {
                        location = 2;
                    }
                    else if (type == typeof(double))
                    {
                        location = 3;
                    }

                    if (type.IsEnum)
                    {
                        type = typeof(int);
                    }

                    generator.Emit(OpCodes.Ldloca, location);
                    var parseMethod = type.GetMethod("TryParse", new[] { StringType, type.MakeByRefType() });

                    generator.Emit(OpCodes.Call, parseMethod);
                    generator.Emit(OpCodes.Pop);

                    generator.Emit(OpCodes.Ldloc, location);
                }

                if (levelContainingType.IsClass)
                {
                    generator.Emit(OpCodes.Callvirt, prop.GetSetMethod());
                }
                else
                {
                    generator.Emit(OpCodes.Call, prop.GetSetMethod());
                }
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
                generator.DeclareLocal(type);

                var currentLocation = toLoadLocation;

                if (type.IsClass)
                {
                    var toTypeConstructor =
                        type.GetConstructor(
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, 
                            null, 
                            new Type[] { }, 
                            new ParameterModifier[] { });
                    generator.Emit(OpCodes.Newobj, toTypeConstructor);

                    generator.Emit(OpCodes.Stloc, currentLocation);
                }
                else
                {
                    generator.Emit(OpCodes.Ldloca_S, currentLocation);
                    generator.Emit(OpCodes.Initobj, type);
                }

                string newPrefix = prefix + prop.Name;
                this.GenerateLevel(generator, type, newPrefix + "_", toLoadLocation, newTrail);
                
                if (levelContainingType.IsClass)
                {
                    generator.Emit(OpCodes.Ldloc_S, parentLocation);
                    generator.Emit(OpCodes.Ldloc_S, currentLocation);
                    generator.Emit(OpCodes.Callvirt, prop.GetSetMethod());
                }
                else
                {
                    generator.Emit(OpCodes.Ldloca_S, parentLocation);
                    generator.Emit(OpCodes.Ldloc_S, currentLocation);
                    generator.Emit(OpCodes.Call, prop.GetSetMethod());
                }
            }
        }

        #endregion
    }
}