namespace Ganondorf.Internals
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Collections.Generic;

    internal class FromNvcMethodGenerator<T> : BaseMethodGenerator<T, NameValueCollection, T>
    {
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

            var stringType = typeof(string);

            ILGenerator generator = dm.GetILGenerator();

            generator.DeclareLocal(typeof(object));
            generator.DeclareLocal(typeof(decimal));
            generator.DeclareLocal(typeof(float));
            generator.DeclareLocal(typeof(double));
            generator.DeclareLocal(toType);

            var objectLocation = 4;

            if (toType.IsClass)
            {
                var toTypeConstructor = toType.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { }, new ParameterModifier[] { });
                generator.Emit(OpCodes.Newobj, toTypeConstructor);

                generator.Emit(OpCodes.Stloc, objectLocation);
            }
            else
            {
                generator.Emit(OpCodes.Ldloca_S, 4);
                generator.Emit(OpCodes.Initobj, toType);
            }

            GenerateLevel(generator, toType, string.Empty, objectLocation, new HashSet<Type>());

            generator.Emit(OpCodes.Ldloc, objectLocation);
            generator.Emit(OpCodes.Ret);

            return dm;
        }

        protected override void GenerateLevel(ILGenerator generator, Type toLevel, string prefix, int toLoadLocation, HashSet<Type> parentTypeTrail)
        {
            var parentLocation = toLoadLocation;

            var getMethod = NvcType.GetMethod("Get", new[] { StringType });

            var properties = toLevel.GetProperties();

            foreach (var prop in properties.Where(x => !TypeNeedsRecursing(x.PropertyType)))
            {
                string newPrefix = prefix + prop.Name;

                if (toLevel.IsClass)
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

                if (toLevel.IsClass)
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

                var newTrail = new HashSet<Type>(parentTypeTrail);

                newTrail.Add(prop.PropertyType);

                var type = prop.PropertyType;

                toLoadLocation++;
                generator.DeclareLocal(type);

                var currentLocation = toLoadLocation;

                if (type.IsClass)
                {
                    var toTypeConstructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { }, new ParameterModifier[] { });
                    generator.Emit(OpCodes.Newobj, toTypeConstructor);

                    generator.Emit(OpCodes.Stloc, currentLocation);
                }
                else
                {
                    generator.Emit(OpCodes.Ldloca_S, currentLocation);
                    generator.Emit(OpCodes.Initobj, type);
                }

                string newPrefix = prefix + prop.Name;
                GenerateLevel(generator, type, newPrefix + "_", toLoadLocation, newTrail);

                generator.Emit(OpCodes.Ldloc_S, parentLocation);
                generator.Emit(OpCodes.Ldloc_S, currentLocation);
                generator.Emit(OpCodes.Callvirt, prop.GetSetMethod());
            }
        }
    }
}
