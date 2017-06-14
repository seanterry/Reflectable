using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fidget.Extensions.Reflection
{
    /// <summary>
    /// Defines a reflection provider for database model types.
    /// </summary>
    /// <typeparam name="T">Type of the model to reflect.</typeparam>

    public interface IModelReflector<T> where T : class
    {
        /// <summary>
        /// Gets the table attribute on the model if one exists, otherwise null.
        /// </summary>

        TableAttribute Table { get; }

        /// <summary>
        /// Gets the collection of properties on the model type decorated by KeyAttribute.
        /// </summary>
        /// <remarks>Properties decorated by NotMappedAttribute will be ignored.</remarks>
        
        IEnumerable<IPropertyReflector<T>> Keys { get; }

        /// <summary>
        /// Gets the collection of properties on the model type decorated by DatabaseGeneratedAttribute
        /// with any option other than None.
        /// </summary>
        /// <remarks>Properties decorated by NotMappedAttribute will be ignored.</remarks>

        IEnumerable<IPropertyReflector<T>> Generated { get; }

        /// <summary>
        /// Gets the collection of properties on the model type decorated by ConcurrencyCheckAttribute.
        /// </summary>
        /// <remarks>Properties decorated by NotMappedAttribute will be ignored.</remarks>

        IEnumerable<IPropertyReflector<T>> Concurrency { get; }

        /// <summary>
        /// Gets the collection of properties on the model type that can participate in a database insert statement.
        /// </summary>
        /// <remarks>
        /// Includes any property that meets the following criteria:
        /// - Not read-only.
        /// - Not decorated by NotMappedAttribute.
        /// - Not in the Generated collection.
        /// - Not in the Concurrency collection.
        /// </remarks>

        IEnumerable<IPropertyReflector<T>> Insertable { get; }

        /// <summary>
        /// Gets the collection of properties on the model type that can be updated in a database update statement.
        /// </summary>
        /// <remarks>
        /// Includes any property that meets the following criteria:
        /// - Not read-only.
        /// - Not decorated by NotMappedAttribute.
        /// - Not in the Generated collection.
        /// - Not in the Concurrency collection.
        /// - Not in the Keys collection.
        /// </remarks>

        IEnumerable<IPropertyReflector<T>> Updatable { get; }

        /// <summary>
        /// Gets the collection of properties that can participate in a database select statement.
        /// </summary>
        /// <remarks>
        /// Includes any property that meets the following criteria:
        /// - Not read-only.
        /// - Not decorated by NotMappedAttribute.
        /// </remarks>
        
        IEnumerable<IPropertyReflector<T>> Selectable { get; }
    }
}