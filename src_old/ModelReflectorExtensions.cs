using Fidget.Extensions.Reflection.Internal;

namespace Fidget.Extensions.Reflection
{
    /// <summary>
    /// Extension methods related to the model reflector.
    /// </summary>

    public static class ModelReflectorExtensions
    {
        /// <summary>
        /// Returns the reflection provider for the model type.
        /// </summary>
        /// <typeparam name="T">Model type to reflect.</typeparam>
        
        public static IModelReflector<T> Model<T>() where T: class => ModelReflector<T>.Instance;

        /// <summary>
        /// Returns the reflection provider for the model type.
        /// </summary>
        /// <typeparam name="T">Model type to reflect.</typeparam>
        /// <param name="instance">Instance whose model type to reflect (optional).</param>
        
        public static IModelReflector<T> Model<T>( this T instance ) where T: class => Model<T>();
    }
}