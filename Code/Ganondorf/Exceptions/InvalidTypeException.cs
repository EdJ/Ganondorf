// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvalidTypeException.cs" company="SigmoidFx">
//  Copyright Ed 2012. 
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ganondorf.Exceptions
{
    using System;

    /// <summary>
    /// Used to indicate the serialiser cannot cope with a type. This applies to value types, reference types with loops are currently detected at runtime.
    /// </summary>
    public class InvalidTypeException : Exception
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTypeException"/> class.
        /// </summary>
        public InvalidTypeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTypeException"/> class.
        /// </summary>
        /// <param name="typeThrownFor">
        /// The type the exception was thrown for.
        /// </param>
        public InvalidTypeException(Type typeThrownFor)
            : base(
                "Could not initialise the Ganondorf.QueryStringSerialiser for the type " + typeThrownFor.FullName
                + ", type must be a reference type or there is no way to deduce key names.")
        {
        }

        #endregion
    }
}