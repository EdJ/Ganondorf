namespace Ganondorf
{
    using System.Collections.Specialized;
    using Ganondorf.Internals;
    using Ganondorf.Exceptions;

    public class QueryStringSerialiser<T>
    {
        public QueryStringSerialiser()
        {
            var type = typeof(T);
            if (!Helper.TypeNeedsRecursing(type))
            {
                throw new InvalidTypeException(type);
            }
        }

        public NameValueCollection Map(T toMap)
        {
            var methodGenerator = new ToNvcMethodGenerator<T>();
            var method = methodGenerator.GenerateMethod();

            return method(toMap);
        }

        public T Load(NameValueCollection loadFrom)
        {
            var methodGenerator = new FromNvcMethodGenerator<T>();
            var method = methodGenerator.GenerateMethod();

            return method(loadFrom);
        }
    }
}
