using System.Collections.Generic;

namespace Fidget.Extensions.Reflection
{
    /// <summary>
    /// Defines a fast reflection provider for a type.
    /// </summary>
    /// <typeparam name="T">Type to reflect.</typeparam>

    public interface ITypeReflector<T> : IEnumerable<IPropertyReflector<T>>
    {
        /// <summary>
        /// Gets the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>

        IPropertyReflector<T> this[string propertyName] { get; }

        /// <summary>
        /// Creates and returns a shallow copy of the source instance.
        /// </summary>
        /// <param name="source">Source instance to clone.</param>

        T Clone( T source );

        /// <summary>
        /// Copies the property values from the source instance to the target.
        /// </summary>
        /// <param name="source">Source instance whose property values to copy.</param>
        /// <param name="target">Target instance into which to copy the values.</param>
        /// <returns>The target instance.</returns>

        T CopyTo( T source, T target );
    }
}