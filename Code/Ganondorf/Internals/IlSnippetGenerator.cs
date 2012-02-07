// -----------------------------------------------------------------------
// <copyright file="IlSnippetGenerator.cs" company="SigmoidFx">
//  Copyright Ed 2012.
// </copyright>
// -----------------------------------------------------------------------

namespace Ganondorf.Internals
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    using Ganondorf.Interfaces;

    public class IlSnippetGenerator : ISnippetGenerator
    {
        private ILGenerator Generator { get; set; }

        private DynamicMethod CurrentMethod { get; set; }

        public void DeclareMethod(Type toType, Type fromType, string methodNamePrefix)
        {
            DynamicMethod dm = new DynamicMethod(
                methodNamePrefix + toType.FullName,
                MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard,
                toType,
                new[] { fromType },
                toType,
                true);

            this.Generator = dm.GetILGenerator();

            this.CurrentMethod = dm;
        }

        public void DeclareLocal(Type toType)
        {
            this.Generator.DeclareLocal(toType);
        }

        public int DeclarePrimitiveContainers()
        {
            this.Generator.DeclareLocal(typeof(object));
            this.Generator.DeclareLocal(typeof(decimal));
            this.Generator.DeclareLocal(typeof(float));
            this.Generator.DeclareLocal(typeof(double));

            return 4;
        }

        public void LoadString(string newPrefix)
        {
            this.Generator.Emit(OpCodes.Ldstr, newPrefix);
        }

        public void LoadArgument(int argumentNumber)
        {
            this.Generator.Emit(OpCodes.Ldarg, argumentNumber);
        }

        public void LoadLocation(int locationNumber)
        {
            this.Generator.Emit(OpCodes.Ldloc, locationNumber);
        }

        public void LoadContainingInstanceOntoStack(int toLoadLocation, Type levelContainingType)
        {
            if (levelContainingType.IsClass)
            {
                this.Generator.Emit(OpCodes.Ldloc, toLoadLocation);
            }
            else
            {
                this.Generator.Emit(OpCodes.Ldloca, toLoadLocation);
            }
        }

        public void ParsePrimitiveValue(PropertyInfo prop)
        {
            var type = prop.PropertyType;
            if (type != TypeHelper.StringType)
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

                this.Generator.Emit(OpCodes.Ldloca, location);
                var parseMethod = type.GetMethod("TryParse", new[] { TypeHelper.StringType, type.MakeByRefType() });

                this.Generator.Emit(OpCodes.Call, parseMethod);
                this.Generator.Emit(OpCodes.Pop);

                this.Generator.Emit(OpCodes.Ldloc, location);
            }
        }

        public void ToStringPrimitiveValue(PropertyInfo prop)
        {
            var type = prop.PropertyType;
            if (type != TypeHelper.StringType)
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

                this.Generator.Emit(OpCodes.Stloc, location);

                this.Generator.Emit(OpCodes.Ldloca, location);
                var toStringMethod = type.GetMethod("ToString", new Type[] { });

                this.Generator.Emit(OpCodes.Call, toStringMethod);
            }
        }

        public void CallMethod(MethodInfo methodInfo, bool virtualCall)
        {
            if (virtualCall)
            {
                this.Generator.Emit(OpCodes.Callvirt, methodInfo);
            }
            else
            {
                this.Generator.Emit(OpCodes.Call, methodInfo);
            }
        }

        public void CallMethod(Type levelContainingType, MethodInfo methodInfo)
        {
            if (levelContainingType.IsClass)
            {
                this.Generator.Emit(OpCodes.Callvirt, methodInfo);
            }
            else
            {
                this.Generator.Emit(OpCodes.Call, methodInfo);
            }
        }

        public void LoadNonPrimitiveOntoStack(Type levelContainingType, int currentLocation, int parentLocation)
        {
            if (levelContainingType.IsClass)
            {
                this.Generator.Emit(OpCodes.Ldloc_S, parentLocation);
                this.Generator.Emit(OpCodes.Ldloc_S, currentLocation);
            }
            else
            {
                this.Generator.Emit(OpCodes.Ldloca_S, parentLocation);
                this.Generator.Emit(OpCodes.Ldloc_S, currentLocation);
            }
        }

        public void InstantiateType(Type typeToInstantiate, int objectLocation)
        {
            if (typeToInstantiate.IsClass)
            {
                var typeConstructor =
                    typeToInstantiate.GetConstructor(
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new Type[] { },
                        new ParameterModifier[] { });
                this.Generator.Emit(OpCodes.Newobj, typeConstructor);

                this.Generator.Emit(OpCodes.Stloc, objectLocation);
            }
            else
            {
                this.Generator.Emit(OpCodes.Ldloca_S, objectLocation);
                this.Generator.Emit(OpCodes.Initobj, typeToInstantiate);
            }
        }

        public void StoreTopStackItemInLocation(int location)
        {
            this.Generator.Emit(OpCodes.Stloc, location);
        }

        public void LoadReturnValue(int outputValueLocation)
        {
            this.Generator.Emit(OpCodes.Ldloc, outputValueLocation);
            this.Generator.Emit(OpCodes.Ret);
        }

        public DynamicMethod GetGeneratedMethod()
        {
            return this.CurrentMethod;
        }
    }
}
