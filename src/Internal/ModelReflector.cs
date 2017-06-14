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
            Insertable = sort( mapped.Where( _=> !_.IsReadOnly ).Except( Concurrency ).Except( Generated ) );
            Updatable = sort( mapped.Where( _ => !_.IsReadOnly ).Except( Keys ).Except( Concurrency ).Except( Generated ) );
            Selectable = sort( mapped.Where( _=> !_.IsReadOnly ) );
        }
    }
}