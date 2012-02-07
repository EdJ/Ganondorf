// -----------------------------------------------------------------------
// <copyright file="ISnippetGenerator.cs" company="SigmoidFx">
//  Copyright Ed 2012.
// </copyright>
// -----------------------------------------------------------------------

namespace Ganondorf.Interfaces
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Abstracts the ILGenerator for testing purposes.
    /// </summary>
    public interface ISnippetGenerator
    {
        /// <summary>
        /// Declares a new method, should override a previous method if called (the method will not be valid until finalise).
        /// </summary>
        /// <param name="toType">The type the method returns.</param>
        /// <param name="fromType">The type of the method's parameter.</param>
        /// <param name="methodNamePrefix">The prefix for the method name (the suffix is the toType.Name)</param>
        void DeclareMethod(Type toType, Type fromType, string methodNamePrefix);

        void DeclareLocal(Type toType);

        int DeclarePrimitiveContainers();

        void LoadString(string newPrefix);

        void LoadArgument(int argumentNumber);

        void LoadLocation(int locationNumber);

        void LoadContainingInstanceOntoStack(int toLoadLocation, Type levelContainingType);

        void ParsePrimitiveValue(PropertyInfo prop);

        void ToStringPrimitiveValue(PropertyInfo prop);

        void CallMethod(Type levelContainingType, MethodInfo methodInfo);

        void LoadNonPrimitiveOntoStack(Type levelContainingType, int currentLocation, int parentLocation);

        void InstantiateType(Type typeToInstantiate, int objectLocation);

        void LoadReturnValue(int outputValueLocation);

        void StoreTopStackItemInLocation(int location);

        DynamicMethod GetGeneratedMethod();

        void CallMethod(MethodInfo methodInfo, bool virtualCall);
    }
}
