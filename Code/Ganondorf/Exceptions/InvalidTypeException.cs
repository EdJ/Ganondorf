namespace Ganondorf.Exceptions
{
    using System;

    public class InvalidTypeException : Exception
    {
        public InvalidTypeException()
        {
        }

        public InvalidTypeException(Type typeThrownFor)
            : base("Could not initialise the Ganondorf.QueryStringSerialiser for the type " + typeThrownFor.FullName + ", type must be a reference type or there is no way to deduce key names")
        {
        }
    }
}
