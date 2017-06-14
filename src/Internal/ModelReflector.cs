using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Fidget.Extensions.Reflection.Internal
{
    /// <summary>
    /// Reflection provider for a database model type.
    /// </summary>
    /// <typeparam name="T">Type of the model to reflect.</typeparam>

    class ModelReflector<T> : IModelReflector<T> where T : class
    {
        /// <summary>
        /// Gets the singleton instance of the type.
        /// </summary>
        
        public static IModelReflector<T> Instance { get; } = new ModelReflector<T>();

        /// <summary>
        /// Reflector for the model type.
        /// </summary>
        
        readonly ITypeReflector<T> TypeReflector = TypeReflectorExtensions.Reflect<T>();

        /// <summary>
        /// Gets the table attribute on the model if one exists, otherwise null.
        /// </summary>
        
        public TableAttribute Table { get; }

        /// <summary>
        /// Gets the collection of key properties defined on the model.
        /// </summary>
        
        public IEnumerable<IPropertyReflector<T>> Keys { get; }

        /// <summary>
        /// Gets the collection of database-generated properties.
        /// </summary>
        
        public IEnumerable<IPropertyReflector<T>> Generated { get; }

        /// <summary>
        /// Gets the collection of fields that participate in concurrency checks.
        /// </summary>
        
        public IEnumerable<IPropertyReflector<T>> Concurrency { get; }

        /// <summary>
        /// Gets the collection of insertable properties.
        /// </summary>
        
        public IEnumerable<IPropertyReflector<T>> Insertable { get; }

        /// <summary>
        /// Gets the collection of updatable properties.
        /// </summary>
        
        public IEnumerable<IPropertyReflector<T>> Updatable { get; }

        /// <summary>
        /// Gets the collection of selectable properties.
        /// </summary>
        
        public IEnumerable<IPropertyReflector<T>> Selectable { get; }

        /// <summary>
        /// Constructs a reflection provider for a database model type.
        /// </summary>
        
        ModelReflector() 
        {
            var type = typeof(T);
            Table = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();

            // order mapped fields by column ordinal then name
            var mapped = TypeReflector
                .Where( _=> _.PropertyInfo.GetCustomAttribute<NotMappedAttribute>() == null )
                .OrderBy( _ => _.PropertyInfo.GetCustomAttribute<ColumnAttribute>()?.Order ?? int.MaxValue )
                .ThenBy( _ => _.PropertyInfo.Name )
                .ToArray();

            // returns the collection sorted in the defined order
            IEnumerable<IPropertyReflector<T>> sort( IEnumerable<IPropertyReflector<T>> properties ) => properties
                .OrderBy( _ => Array.IndexOf( mapped, _ ) )
                .ToArray();

            Keys = sort( mapped.Where( _=> _.PropertyInfo.GetCustomAttribute<KeyAttribute>() != null ) );
            Generated = sort( mapped.Where( _=> ( _.PropertyInfo.GetCustomAttribute<DatabaseGeneratedAttribute>()?.DatabaseGeneratedOption ?? DatabaseGeneratedOption.None ) != DatabaseGeneratedOption.None ) );
            Concurrency = sort( mapped.Where( _=> _.PropertyInfo.GetCustomAttribute<ConcurrencyCheckAttribute>() != null ) );
            Selectable = sort( mapped.Where( _ => !_.IsReadOnly ) );
            Insertable = sort( Selectable.Except( Concurrency ).Except( Generated ) );
            Updatable = sort( Insertable.Except( Keys ) );
        }

        /// <summary>
        /// Tries to gets and changed updatable property values between two model instances.
        /// </summary>
        /// <param name="current">Current model values.</param>
        /// <param name="comparer">Comparer containing original values.</param>
        /// <param name="changes">Collection of detection value changes indexed by property name.</param>
        /// <returns>True if changes were detected, otherwise false.</returns>
        
        public bool TryGetChanges( T current, T comparer, out IDictionary<string,object> changes )
        {
            if ( current == null ) throw new ArgumentNullException( nameof( current ) );
            if ( comparer == null ) throw new ArgumentNullException( nameof( comparer ) );
            
            changes = new Dictionary<string,object>();

            // validate key
            foreach ( var property in Keys )
            {
                if ( !property.Equal( current, comparer ) ) throw new InvalidOperationException( "Changed detected to key values" );
            }

            foreach ( var property in Updatable )
            {
                if ( !property.Equal( current, comparer ) )
                {
                    changes[property.PropertyInfo.Name] = property.GetValue( current );
                }
            }

            return changes.Any();
        }
    }
}