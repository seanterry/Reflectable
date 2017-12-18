/*  Copyright 2017 Sean Terry

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Reflectable.Internal
{
    /// <summary>
    /// Fast reflection provider for a type.
    /// </summary>
    /// <typeparam name="T">Type to reflect.</typeparam>

    public class TypeReflector<T> : ITypeReflector<T>
    {
        /// <summary>
        /// Type being reflected.
        /// </summary>
        
        readonly Type type = typeof(T);

        /// <summary>
        /// Collection of properties on the type.
        /// </summary>
        
        readonly IReadOnlyDictionary<string,IPropertyReflector<T>> properties;

        /// <summary>
        /// Returns an enumerator for iterating the property collection.
        /// </summary>
        
        public IEnumerator<IPropertyReflector<T>> GetEnumerator() => properties.Values.GetEnumerator();

        /// <summary>
        /// Returns an enumerator for iterating the property collection.
        /// </summary>
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets the specified property.
        /// </summary>
        /// <param name="name">Name of the property.</param>

        public IPropertyReflector<T> this[string name] { get => properties[name]; }

        /// <summary>
        /// Collection of properties on the type that are arrays.
        /// </summary>

        readonly IEnumerable<IPropertyReflector<T>> arrays;

        /// <summary>
        /// Constructs a fast reflection provider for a type.
        /// </summary>

        TypeReflector()
        {
            // creates and returns a reflector for the given property
            IPropertyReflector<T> createReflector( PropertyInfo propertyInfo )
            {
                var type = typeof( PropertyReflector<,> )
                    .MakeGenericType( typeof(T), propertyInfo.PropertyType );

                return (IPropertyReflector<T>)Activator
                    .CreateInstance( type, propertyInfo.Name );
            }
            
            var properties = new Dictionary<string,IPropertyReflector<T>>();
            var arrays = new List<IPropertyReflector<T>>();

            foreach ( var property in type.GetProperties() )
            {
                var reflector = createReflector( property );
                properties.Add( reflector.Property.Name, reflector );

                if ( reflector.IsArray )
                {
                    arrays.Add( reflector );
                }
            }

            this.properties = properties;
            this.arrays = arrays;
        }

        /// <summary>
        /// Gets the singleton instance of the type.
        /// </summary>

        public static ITypeReflector<T> Instance { get; } = new TypeReflector<T>();

        /// <summary>
        /// Creates and returns a shallow copy of the source instance.
        /// </summary>
        /// <param name="source">Source instance to clone.</param>

        public T Clone( T source )
        {
            if ( source == null ) throw new ArgumentNullException( nameof( source ) );

            var clone = (T)CloneMethod.Instance.Invoke( source );

            // perform a structural copy of any array properties
            foreach ( var property in arrays )
            {
                if ( !property.IsReadOnly )
                {
                    property.Copy( source, clone );
                }
            }

            return clone;
        }

        /// <summary>
        /// Copies the property values from the source instance to the target.
        /// </summary>
        /// <param name="source">Source instance whose property values to copy.</param>
        /// <param name="target">Target instance into which to copy the values.</param>
        /// <returns>The target instance.</returns>

        public T CopyTo( T source, T target )
        {
            if ( source == null ) throw new ArgumentNullException( nameof( source ) );
            if ( target == null ) throw new ArgumentNullException( nameof( target ) );

            foreach ( var property in this )
            {
                if ( !property.IsReadOnly )
                {
                    property.Copy( source, target );
                }
            }

            return target;
        }
    }
}