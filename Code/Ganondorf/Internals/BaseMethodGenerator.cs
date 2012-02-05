namespace Ganondorf.Internals
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Collections.Generic;

    internal abstract class BaseMethodGenerator<T, TFirst, TSecond>
    {
        protected static readonly Type NvcType = typeof(NameValueCollection);
        protected static readonly Type StringType = typeof(string);
        protected static readonly Type DecimalType = typeof(decimal);

        protected abstract DynamicMethod GenerateDelegate();

        protected static bool TypeNeedsRecursing(Type type)
        {
            return Helper.TypeNeedsRecursing(type);
        }

        protected abstract void GenerateLevel(ILGenerator generator, Type levelContainingType, string prefix, int toLoadLocation, HashSet<Type> parentTypeTrail);
        
        public Func<TFirst, TSecond> GenerateMethod()
        {
            DynamicMethod del = this.GenerateDelegate();

            this.Map = (MappingFunction)del.CreateDelegate(typeof(MappingFunction));

            // Wrap it in a type-safe wrapper.
            Func<TFirst, TSecond> output = x => this.Map(x);

            return output;
        }

        public MappingFunction Map { get; set; }

        public delegate TSecond MappingFunction(TFirst from);
    }
}
