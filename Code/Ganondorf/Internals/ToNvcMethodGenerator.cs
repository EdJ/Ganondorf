namespace Ganondorf.Internals
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Collections.Generic;

    internal class ToNvcMethodGenerator<T> : BaseMethodGenerator<T, T, NameValueCollection>
    {
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

            var stringType = typeof(string);

            var nvcConstructor = NvcType.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { }, new ParameterModifier[] { });

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

            GenerateLevel(generator, fromType, string.Empty, 5, new HashSet<Type>());

            generator.Emit(OpCodes.Ldloc, 0);
            generator.Emit(OpCodes.Ret);

            return dm;
        }

        protected override void GenerateLevel(ILGenerator generator, Type toLevel, string prefix, int toLoadLocation, HashSet<Type> parentTypeTrail)
        {
            var stringType = typeof(string);
            
            var addMethod = NvcType.GetMethod("Add", new[] { stringType, stringType });

            var properties = toLevel.GetProperties();

            foreach (var prop in properties.Where(x => !TypeNeedsRecursing(x.PropertyType)))
            {
                string newPrefix = prefix + prop.Name;
                generator.Emit(OpCodes.Ldloc, 0);

                generator.Emit(OpCodes.Ldstr, newPrefix);

                if (toLevel.IsClass)
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

                var newTrail = new HashSet<Type>(parentTypeTrail);

                newTrail.Add(prop.PropertyType);

                string newPrefix = prefix + prop.Name;
                var type = prop.PropertyType;

                generator.Emit(OpCodes.Ldloc, toLoadLocation);

                toLoadLocation++;
                generator.DeclareLocal(type);

                generator.Emit(OpCodes.Callvirt, prop.GetGetMethod());
                generator.Emit(OpCodes.Stloc, toLoadLocation);

                GenerateLevel(generator, type, newPrefix + "_", toLoadLocation, newTrail);
            }
        }
    }
}
