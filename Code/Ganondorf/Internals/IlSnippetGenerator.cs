// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IlSnippetGenerator.cs" company="SigmoidFx">
//   Copyright Ed 2012.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ganondorf.Internals
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    using Ganondorf.Interfaces;

    /// <summary>
    /// Allows generation of methods with snippets via an ILGenerator and DynamicMethod.
    /// Must be initialised by calling DeclareMethod.
    /// </summary>
    public class IlSnippetGenerator : ISnippetGenerator
    {
        #region Properties

        /// <summary>
        /// Gets or sets the current method that the ILGenerator is creating.
        /// This will be overwritten if DeclareMethod is re-called, and will not exist until DeclareMethod is called.
        /// </summary>
        private DynamicMethod CurrentMethod { get; set; }

        /// <summary>
        /// Gets or sets the ILGenerator that is generating the DynamicMethod.
        /// </summary>
        private ILGenerator Generator { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calls a method, as either a virtual or standard call.
        /// Virtual calls tend to be used on classes, and non-virtual on value types.
        /// </summary>
        /// <param name="methodInfo">
        /// The method to call.
        /// </param>
        /// <param name="virtualCall">
        /// Whether to call as a virtual.
        /// </param>
        public void CallMethod(MethodInfo methodInfo, bool virtualCall)
        {
            OpCode callType = virtualCall ? OpCodes.Callvirt : OpCodes.Call;
            this.Generator.Emit(callType, methodInfo);
        }

        /// <summary>
        /// Call a method attached to a containing type.
        /// Will assume that reference types should have their methods called as virtuals.
        /// </summary>
        /// <param name="containingType">
        /// The type that the method is attached to.
        /// </param>
        /// <param name="methodInfo">
        /// The method to call.
        /// </param>
        public void CallMethod(Type containingType, MethodInfo methodInfo)
        {
            bool isClass = containingType.IsClass;

            this.CallMethod(methodInfo, isClass);
        }

        /// <summary>
        /// Declares a local variable of a specific type.
        /// </summary>
        /// <param name="toType">
        /// The type of the variable to declare.
        /// </param>
        public void DeclareLocal(Type toType)
        {
            this.Generator.DeclareLocal(toType);
        }

        /// <summary>
        /// Initialises method generation, should be called before using any other methods on this class.
        /// </summary>
        /// <param name="toType">
        /// The return type of the method.
        /// </param>
        /// <param name="fromType">
        /// The type of the method's only parameter.
        /// </param>
        /// <param name="methodNamePrefix">
        /// The prefix to apply to the method name (the suffix is the FullName property of the toType parameter).
        /// </param>
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

        /// <summary>
        /// Declares local variables that can hold an instance of any (possibly boxed) primitive.
        /// </summary>
        /// <returns>
        /// The number of local variables declared.
        /// </returns>
        public int DeclarePrimitiveContainers()
        {
            this.Generator.DeclareLocal(typeof(object));
            this.Generator.DeclareLocal(typeof(decimal));
            this.Generator.DeclareLocal(typeof(float));
            this.Generator.DeclareLocal(typeof(double));

            return 4;
        }

        /// <summary>
        /// Gets the method that has been generated.
        /// </summary>
        /// <returns>
        /// The DynamicMethod that has been generated.
        /// </returns>
        public DynamicMethod GetGeneratedMethod()
        {
            return this.CurrentMethod;
        }

        /// <summary>
        /// Instantiates a new instance of a type into a specified local variable.
        /// </summary>
        /// <param name="typeToInstantiate">
        /// The type to instantiate.
        /// </param>
        /// <param name="objectLocation">
        /// The location of the variable to store the new instance in.
        /// </param>
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

        /// <summary>
        /// Loads a specific method parameter onto the stack.
        /// </summary>
        /// <param name="argumentNumber">
        /// The argument number of the parameter to load.
        /// </param>
        public void LoadArgument(int argumentNumber)
        {
            this.Generator.Emit(OpCodes.Ldarg, argumentNumber);
        }

        /// <summary>
        /// Loads a reference to an instance from a specified local varaiable.
        /// Reference types are stored as pointers, but value types must be specifically requested as pointers.
        /// </summary>
        /// <param name="toLoadLocation">
        /// The location of the instance to load a reference to.
        /// </param>
        /// <param name="instanceType">
        /// The instance type.
        /// </param>
        public void LoadInstanceReference(int toLoadLocation, Type instanceType)
        {
            OpCode loadType = instanceType.IsClass ? OpCodes.Ldloc : OpCodes.Ldloca;
            this.Generator.Emit(loadType, toLoadLocation);
        }

        /// <summary>
        /// Loads a variable from a specific local location onto the stack and returns from the method.
        /// </summary>
        /// <param name="returnVariableLocation">
        /// The return variable location.
        /// </param>
        public void LoadReturnValue(int returnVariableLocation)
        {
            this.Generator.Emit(OpCodes.Ldloc, returnVariableLocation);
            this.Generator.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Loads a string onto the stack.
        /// </summary>
        /// <param name="toLoad">
        /// The string to load onto the stack.
        /// </param>
        public void LoadString(string toLoad)
        {
            this.Generator.Emit(OpCodes.Ldstr, toLoad);
        }

        /// <summary>
        /// Stores the item from the top of the stack into a specific local variable location.
        /// </summary>
        /// <param name="location">
        /// The location to store the item to.
        /// </param>
        public void StoreItemFromStack(int location)
        {
            this.Generator.Emit(OpCodes.Stloc, location);
        }

        /// <summary>
        /// Calls ToString on a primitive of a specific type currently residing on top of the stack.
        /// </summary>
        /// <param name="type">
        /// The type of the primitive.
        /// </param>
        public void ToStringPrimitiveValue(Type type)
        {
            if (type == TypeHelper.StringType)
            {
                return;
            }

            int location = GetTypeLocation(type);

            if (type.IsEnum)
            {
                type = typeof(int);
            }

            this.Generator.Emit(OpCodes.Stloc, location);

            this.Generator.Emit(OpCodes.Ldloca, location);
            MethodInfo toStringMethod = type.GetMethod("ToString", new Type[] { });

            this.Generator.Emit(OpCodes.Call, toStringMethod);
        }

        /// <summary>
        /// Calls a primitive type's TryParse on a string residing on top of the stack.
        /// </summary>
        /// <param name="type">
        /// The type of the primitive.
        /// </param>
        public void TryParsePrimitiveValue(Type type)
        {
            if (type == TypeHelper.StringType)
            {
                return;
            }

            int location = GetTypeLocation(type);

            if (type.IsEnum)
            {
                type = typeof(int);
            }

            this.Generator.Emit(OpCodes.Ldloca, location);
            MethodInfo parseMethod = type.GetMethod("TryParse", new[] { TypeHelper.StringType, type.MakeByRefType() });

            this.Generator.Emit(OpCodes.Call, parseMethod);
            this.Generator.Emit(OpCodes.Pop);

            this.Generator.Emit(OpCodes.Ldloc, location);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the location of a variable of a specific type, based on the intial call to DeclarePrimitiveContainers.
        /// </summary>
        /// <param name="type">
        /// The type of the variable to find a location for.
        /// </param>
        /// <returns>
        /// The location the variable can be stored in.
        /// </returns>
        private static int GetTypeLocation(Type type)
        {
            int location = 0;
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

            return location;
        }

        #endregion
    }
}