using Fidget.Extensions.Reflection.Internal;

namespace Fidget.Extensions.Reflection
{
    /// <summary>
    /// Extension methods for working with type reflection.
    /// </summary>

    public static class TypeReflectorExtensions
    {
        /// <summary>
        /// Returns a fast reflection provider for the type.
        /// </summary>
        /// <typeparam name="T">Type to reflect.</typeparam>
        /// <param name="instance">Object instance whose type to reflect (optional).</param>
        
        public static ITypeReflector<T> Reflect<T>( T instance = default(T) ) => TypeReflector<T>.Instance;
    }
}