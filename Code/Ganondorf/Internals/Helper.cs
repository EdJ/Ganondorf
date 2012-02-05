namespace Ganondorf.Internals
{
    using System;
    using System.Collections.Generic;

    internal static class Helper
    {
        private static readonly HashSet<Type> AllowedTypes = new HashSet<Type>
            {
                typeof(string),
                typeof(bool),
                typeof(int),
                typeof(decimal),
                typeof(float),
                typeof(double),
                typeof(long),
                typeof(short),
                typeof(char),
                typeof(uint),
                typeof(ulong),
                typeof(ushort),
                typeof(byte),
                typeof(sbyte),
            };

        internal static bool TypeNeedsRecursing(Type type)
        {
            return !type.IsEnum && !AllowedTypes.Contains(type);
        }
    }
}
