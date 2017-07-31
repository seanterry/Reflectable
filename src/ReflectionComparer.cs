using System;
using System.Collections.Generic;
using System.Linq;

namespace Fidget.Extensions.Reflection
{
    /// <summary>
    /// Compares the property values of instances of the type to determine equality.
    /// </summary>
    /// <typeparam name="T">Type of the objects being compared.</typeparam>

    public class ReflectionComparer<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// Reflector for instances of the type.
        /// </summary>
        
        static readonly ITypeReflector<T> Reflector = TypeReflectorExtensions.Reflect<T>();

        /// <summary>
        /// Private constructor.
        /// </summary>
        
        ReflectionComparer() {}

        /// <summary>
        /// Gets an instance to compare the property values of instances of the type to determine equality.
        /// </summary>
        
        public static IEqualityComparer<T> CompareProperties { get; } = new ReflectionComparer<T>();

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>

        public bool Equals( T x, T y ) =>
            x == null && y == null ? true :
            x == null || y == null ? false :
            Reflector.All( _=> _.Equal( x, y ) );

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The object for which a hash code is to be returned.</param>
        
        public int GetHashCode( T obj )
        {
            if ( obj == null ) throw new ArgumentNullException( nameof( obj ) );

            return obj.GetHashCode();
        }
    }
}